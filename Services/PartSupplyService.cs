using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models;

namespace MiniService.Services;

// =============================================================================================
// Tầng nghiệp vụ cho 3 màn port 1:1 từ 2021.1.TCMotor/Dev/DMSCarSv:
//   (1) FrmROStockRequisitionList + FrmROStockRequisition  -> Ser_ROStockRequisition
//   (2) FrmPartQuotationSearch                             -> Ser_Inv_Quote
//   (3) FrmOrderPartSearch                                 -> Ser_Part_Order
// Bộ lọc và công thức giữ ĐÚNG như nguồn (kể cả chỗ nguồn tự lệch — có ghi chú tại chỗ).
// =============================================================================================

// ---------------------------------------------------------------------------------------------
// (1) Phiếu yêu cầu xuất kho phụ tùng theo RO
// ---------------------------------------------------------------------------------------------
public interface IStockReqService
{
    /// <summary>
    /// Tìm phiếu — port 1:1 FrmROStockRequisitionList.cs:92-141 (hàm GetCustomer).
    /// </summary>
    Task<List<RoStockRequisition>> SearchAsync(DateTime? from, DateTime? to, string? roNo, string? status, string? assistant);
    Task<RoStockRequisition?> GetAsync(int id);
    /// <summary>Dòng phụ tùng của RO gắn với phiếu — nguồn lấy từ Ser_ROPartItems (FrmROStockRequisition.cs:298).</summary>
    Task<List<RepairLine>> PartLinesAsync(int id);
    Task<(bool ok, string msg, int id)> CreateAsync(int roId, string? note, string? assistant);
    /// <summary>Xuất kho (nút "Tạo phiếu xuất") — port 1:1 FrmROStockRequisition.cs:340-348.</summary>
    Task<(bool ok, string msg)> FinishAsync(int id);
}

public class StockReqService(AppDbContext db, IInventoryService inv, ILogger<StockReqService> log) : IStockReqService
{
    public async Task<List<RoStockRequisition>> SearchAsync(DateTime? from, DateTime? to, string? roNo, string? status, string? assistant)
    {
        var q = db.StockReqs.AsQueryable();
        // Nguồn chỉ lọc ngày khi CẢ HAI ô có giá trị (FrmROStockRequisitionList.cs:99-109), và so bằng
        // .Date ở cả hai đầu — nghĩa là "đến ngày" lấy mốc 00:00:00, KHÔNG gồm hết ngày đó.
        // Giữ nguyên 1:1; chỗ này đã ghi vào mục "cố ý bỏ / lệch nguồn" của báo cáo.
        if (from.HasValue && to.HasValue)
        {
            var f = from.Value.Date; var t = to.Value.Date;
            q = q.Where(x => x.CreatedDate >= f && x.CreatedDate <= t);
        }
        // "ROReStockRequisition like '%x%'" (FrmROStockRequisitionList.cs:115) — cột dẫn xuất "PX-"+RONo,
        // nên bên đích so trên RONo sau khi bỏ tiền tố "PX-" người dùng có thể gõ vào.
        if (!string.IsNullOrWhiteSpace(roNo))
        {
            var key = roNo.Trim().Replace("PX-", "").Replace("LS-", "").Replace("BG-", "");
            // BẮT BUỘC dùng EF.Functions.Like, KHÔNG dùng string.Contains: trên SQLite EF dịch Contains
            // thành instr() — PHÂN BIỆT hoa/thường — nên truy vấn trả 0 dòng dù build 0 Errors.
            // Nguồn dùng "like %x%" của SQL Server (không phân biệt hoa/thường).
            q = q.Where(x => EF.Functions.Like(x.RONo, "%" + key + "%"));
        }
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status);          // Status = '...' — :119
        if (!string.IsNullOrWhiteSpace(assistant))
            q = q.Where(x => x.Assistant != null && EF.Functions.Like(x.Assistant, "%" + assistant + "%"));  // like — :123
        return await q.OrderByDescending(x => x.CreatedDate).ThenBy(x => x.RONo).ToListAsync();
    }

    public Task<RoStockRequisition?> GetAsync(int id) => db.StockReqs.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<RepairLine>> PartLinesAsync(int id)
    {
        var r = await GetAsync(id);
        if (r == null || r.ROId == null) return [];
        return await db.Lines.Where(l => l.ROId == r.ROId.Value && l.Type == LineType.Part).OrderBy(l => l.Id).ToListAsync();
    }

    public async Task<(bool ok, string msg, int id)> CreateAsync(int roId, string? note, string? assistant)
    {
        var ro = await db.ROs.Include(x => x.Car).FirstOrDefaultAsync(x => x.Id == roId);
        if (ro == null) return (false, "Không tìm thấy lệnh sửa chữa.", 0);
        // Nguồn dò phiếu theo RONo (FrmROStockRequisition.cs:271) nên 1 RO chỉ có 1 phiếu đang dùng.
        if (await db.StockReqs.AnyAsync(x => x.RONo == ro.Code)) return (false, "RO này đã có phiếu yêu cầu xuất kho.", 0);
        var r = new RoStockRequisition
        {
            StkReqId = "STK" + DateTime.Now.ToString("yyMMddHHmmss"),
            ROId = ro.Id, RONo = ro.Code, PlateNo = ro.Car.Plate, FrameNo = ro.Car.Vin,
            Assistant = assistant ?? ro.ServiceAdvisor, Note = note, Status = StockReqStatus.Create
        };
        db.StockReqs.Add(r); await db.SaveChangesAsync();
        return (true, "Đã tạo phiếu yêu cầu xuất kho " + r.ROReStockRequisition + ".", r.Id);
    }

    public async Task<(bool ok, string msg)> FinishAsync(int id)
    {
        var r = await GetAsync(id);
        if (r == null) return (false, "Không tìm thấy phiếu.");
        // Guard của nguồn: chỉ xuất khi IsXuatKho và StkReqID khác rỗng (FrmROStockRequisition.cs:342).
        if (string.IsNullOrWhiteSpace(r.StkReqId)) return (false, "Phiếu chưa có số — không xuất kho được.");
        if (r.Status == StockReqStatus.Finish) return (false, "Phiếu đã xuất kho.");
        // Bên đích có tồn kho thật nên xuất luôn từng dòng phụ tùng (nguồn gọi SerROStockRequisitionFinish("1")).
        int issued = 0;
        foreach (var l in await PartLinesAsync(id))
        {
            if (l.PartId == null) continue;
            var (iok, _) = await inv.IssueStockAsync(l.PartId.Value, (int)Math.Ceiling(l.Quantity), r.ROId, "Phiếu yêu cầu xuất kho " + r.ROReStockRequisition);
            if (iok) issued++;
        }
        r.Status = StockReqStatus.Finish;
        await db.SaveChangesAsync();
        log.LogInformation("Phiếu YCXK {No} đã xuất kho, {Issued} dòng", r.ROReStockRequisition, issued);
        return (true, $"Đã xuất kho ({issued} dòng phụ tùng).");   // nguồn báo "Đã xuất kho" — :346
    }
}

// ---------------------------------------------------------------------------------------------
// (2) Báo giá phụ tùng
// ---------------------------------------------------------------------------------------------
public interface IPartQuotationService
{
    /// <summary>Tìm báo giá — port 1:1 FrmPartQuotationSearch.cs:81-145 + BizCarSv.Inventory.Quote.cs:1747-1850.</summary>
    Task<List<PartQuotation>> SearchAsync(string? quoteNo, string? customerName, DateTime? from, DateTime? to, string[]? statusValues);
    Task<PartQuotation?> GetAsync(int id);
    Task<(bool ok, string msg, int id)> CreateAsync(PartQuotation q);
    Task<(bool ok, string msg)> AddItemAsync(int quoteId, PartQuotationItem item);
    /// <summary>Đổi trạng thái phiếu xuất (SOStatus) — StatusText/StatusValue tự suy theo nguồn.</summary>
    Task<(bool ok, string msg)> SetStockOutStatusAsync(int id, string? soStatus);
}

public class PartQuotationService(AppDbContext db) : IPartQuotationService
{
    public async Task<List<PartQuotation>> SearchAsync(string? quoteNo, string? customerName, DateTime? from, DateTime? to, string[]? statusValues)
    {
        var q = db.PartQuotations.Include(x => x.Items).Include(x => x.Customer).AsQueryable();
        // Nguồn: FromDate + " 00:00:00", ToDate + " 23:59:59" (FrmPartQuotationSearch.cs:89,93).
        if (from.HasValue) { var f = from.Value.Date; q = q.Where(x => x.CreatedDate >= f); }
        if (to.HasValue) { var t = to.Value.Date.AddDays(1).AddSeconds(-1); q = q.Where(x => x.CreatedDate <= t); }
        // EF.Functions.Like thay cho string.Contains — xem ghi chú ở StockReqService.SearchAsync
        // (SQLite dịch Contains thành instr() phân biệt hoa/thường, khác "like" của nguồn).
        if (!string.IsNullOrWhiteSpace(quoteNo))
            q = q.Where(x => EF.Functions.Like(x.QuoteNo, "%" + quoteNo + "%"));                                // like % % — SerPartQuotationService.cs:170
        if (!string.IsNullOrWhiteSpace(customerName))
            q = q.Where(x => x.Customer != null && EF.Functions.Like(x.Customer.Name, "%" + customerName + "%")); // like % % — :164
        // Nguồn sắp xếp: order by QuoteNo, CreatedDate — BizCarSv.Inventory.Quote.cs:1843.
        var list = await q.OrderBy(x => x.QuoteNo).ThenBy(x => x.CreatedDate).ToListAsync();
        // StatusValue là cột DẪN XUẤT (tính sau khi join phiếu xuất) nên nguồn lọc ở #tbl_quote,
        // tức lọc SAU khi đã tính — vì vậy bên đích cũng lọc trong bộ nhớ, không đẩy xuống SQL.
        if (statusValues is { Length: > 0 }) list = list.Where(x => statusValues.Contains(x.StatusValue)).ToList();
        return list;
    }

    public Task<PartQuotation?> GetAsync(int id) =>
        db.PartQuotations.Include(x => x.Items).Include(x => x.Customer).FirstOrDefaultAsync(x => x.Id == id);

    public async Task<(bool ok, string msg, int id)> CreateAsync(PartQuotation q)
    {
        if (string.IsNullOrWhiteSpace(q.QuoteNo))
            q.QuoteNo = "BG" + DateTime.Now.ToString("yyMMddHHmmss");
        if (await db.PartQuotations.AnyAsync(x => x.QuoteNo == q.QuoteNo)) return (false, "Số báo giá đã tồn tại.", 0);
        db.PartQuotations.Add(q); await db.SaveChangesAsync();
        return (true, "Đã tạo báo giá " + q.QuoteNo + ".", q.Id);
    }

    public async Task<(bool ok, string msg)> AddItemAsync(int quoteId, PartQuotationItem item)
    {
        var q = await db.PartQuotations.FirstOrDefaultAsync(x => x.Id == quoteId);
        if (q == null) return (false, "Không tìm thấy báo giá.");
        // Nguồn không cho sửa báo giá đã tạo phiếu xuất (FrmPartQuotationSearch.cs:176 truyền SOStatus sang
        // form chi tiết để chặn sửa) — bên đích chặn ngay tại đây.
        if (!string.IsNullOrWhiteSpace(q.SOStatus)) return (false, "Báo giá đã có phiếu xuất — không sửa được dòng.");
        if (item.Quantity <= 0) return (false, "Số lượng phải lớn hơn 0.");
        if (item.Factor <= 0) item.Factor = 1;             // isnull(Factor,1) — Quote.cs:1883
        item.QuoteId = quoteId;
        db.PartQuotationItems.Add(item); await db.SaveChangesAsync();
        return (true, "Đã thêm dòng phụ tùng.");
    }

    public async Task<(bool ok, string msg)> SetStockOutStatusAsync(int id, string? soStatus)
    {
        var q = await db.PartQuotations.FirstOrDefaultAsync(x => x.Id == id);
        if (q == null) return (false, "Không tìm thấy báo giá.");
        // Chỉ nhận đúng tập giá trị của Ser_Inv_StockOut.Status mà nguồn map (Quote.cs:1807-1822).
        if (!(soStatus is null or "" or "1" or "3" or "4" or "5")) return (false, "Trạng thái phiếu xuất không hợp lệ.");
        q.SOStatus = string.IsNullOrWhiteSpace(soStatus) ? null : soStatus;
        await db.SaveChangesAsync();
        return (true, "Trạng thái: " + q.StatusText);
    }
}

// ---------------------------------------------------------------------------------------------
// (3) Đơn đặt hàng phụ tùng
// ---------------------------------------------------------------------------------------------
public interface IPartOrderService
{
    /// <summary>Tìm đơn — port 1:1 FrmOrderPartSearch.cs:479-580 + BizCarSv.PartOrder.cs:2540-2646.</summary>
    Task<List<PartOrder>> SearchAsync(string? orderNo, string? orderNoUser, string? confirmNo,
        int? supplierId, string[]? statuses, DateTime? fromSendDate, DateTime? toSendDate, string? partCode);
    Task<PartOrder?> GetAsync(int id);
    Task<(bool ok, string msg, int id)> CreateAsync(PartOrder o);
    Task<(bool ok, string msg)> AddDetailAsync(int orderId, PartOrderDetail d);
    /// <summary>Ghi nhận hàng về cho 1 dòng — PendingDeliveryQty tính lại theo công thức nguồn.</summary>
    Task<(bool ok, string msg)> DeliverAsync(int detailId, decimal qty);
    Task<(bool ok, string msg)> SetStatusAsync(int id, string status);
}

public class PartOrderService(AppDbContext db, ILogger<PartOrderService> log) : IPartOrderService
{
    public async Task<List<PartOrder>> SearchAsync(string? orderNo, string? orderNoUser, string? confirmNo,
        int? supplierId, string[]? statuses, DateTime? fromSendDate, DateTime? toSendDate, string? partCode)
    {
        // Nguồn LUÔN lọc si.IsActive = '1' ở cả 2 bước (PartOrder.cs:2549 và :2640).
        var q = db.PartOrders.Include(x => x.Details).Where(x => x.IsActive == "1");
        // EF.Functions.Like thay cho string.Contains — xem ghi chú ở StockReqService.SearchAsync.
        if (!string.IsNullOrWhiteSpace(orderNo))
            q = q.Where(x => EF.Functions.Like(x.OrderNo, "%" + orderNo + "%"));                                        // like % % — PartOrderService.cs:305
        if (!string.IsNullOrWhiteSpace(orderNoUser))
            q = q.Where(x => x.OrderNoUser != null && EF.Functions.Like(x.OrderNoUser, "%" + orderNoUser + "%"));        // :311
        if (!string.IsNullOrWhiteSpace(confirmNo))
            q = q.Where(x => x.ConfirmNo != null && EF.Functions.Like(x.ConfirmNo, "%" + confirmNo + "%"));              // :343
        if (supplierId.HasValue && supplierId.Value > 0) q = q.Where(x => x.SupplierId == supplierId.Value);            // "=" — :301
        if (statuses is { Length: > 0 }) q = q.Where(x => statuses.Contains(x.Status));                                  // IN — :327
        // Khoảng ngày của nguồn áp vào si.SendDate (KHÔNG phải CreateDate) — PartOrder.cs:2641-2642.
        // Nhãn trên form là "ngày nhập", nhưng cột thật là SendDate; giữ nguyên nguồn.
        if (fromSendDate.HasValue) { var f = fromSendDate.Value.Date; q = q.Where(x => x.SendDate != null && x.SendDate >= f); }
        if (toSendDate.HasValue) { var t = toSendDate.Value.Date.AddDays(1).AddSeconds(-1); q = q.Where(x => x.SendDate != null && x.SendDate <= t); }
        // Lọc theo mã phụ tùng = sub-query trên bảng chi tiết (zzzzClauseWhere_strPartCodeSubQuery, PartOrder.cs:2562).
        if (!string.IsNullOrWhiteSpace(partCode))
            q = q.Where(x => x.Details.Any(d => EF.Functions.Like(d.PartCode, "%" + partCode + "%")));
        return await q.OrderByDescending(x => x.CreateDate).ToListAsync();
    }

    public Task<PartOrder?> GetAsync(int id) => db.PartOrders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id);

    public async Task<(bool ok, string msg, int id)> CreateAsync(PartOrder o)
    {
        if (string.IsNullOrWhiteSpace(o.OrderNo)) o.OrderNo = "DH" + DateTime.Now.ToString("yyMMddHHmmss");
        if (await db.PartOrders.AnyAsync(x => x.OrderNo == o.OrderNo)) return (false, "Số đơn hàng đã tồn tại.", 0);
        o.IsActive = "1"; o.Status = PartOrderStatus.Create;
        db.PartOrders.Add(o); await db.SaveChangesAsync();
        return (true, "Đã tạo đơn đặt phụ tùng " + o.OrderNo + ".", o.Id);
    }

    public async Task<(bool ok, string msg)> AddDetailAsync(int orderId, PartOrderDetail d)
    {
        var o = await db.PartOrders.FirstOrDefaultAsync(x => x.Id == orderId);
        if (o == null) return (false, "Không tìm thấy đơn hàng.");
        if (o.Status == PartOrderStatus.Finish) return (false, "Đơn đã hoàn thành — không thêm dòng.");
        if (d.Quantity <= 0) return (false, "Số lượng phải lớn hơn 0.");
        if (d.Factor <= 0) d.Factor = 1;
        d.OrderPartId = orderId;
        db.PartOrderDetails.Add(d); await db.SaveChangesAsync();
        return (true, "Đã thêm dòng phụ tùng.");
    }

    public async Task<(bool ok, string msg)> DeliverAsync(int detailId, decimal qty)
    {
        var d = await db.PartOrderDetails.FirstOrDefaultAsync(x => x.Id == detailId);
        if (d == null) return (false, "Không tìm thấy dòng đơn hàng.");
        if (qty <= 0) return (false, "Số lượng về phải lớn hơn 0.");
        var newQty = (d.DeliveryQuantity ?? 0) + qty;
        if (newQty > d.Quantity) return (false, $"Về vượt số đặt (đặt {d.Quantity}, đã về {d.DeliveryQuantity ?? 0}).");
        d.DeliveryQuantity = newQty;
        d.LastDateDelivery = DateTime.Now;                      // LASTDATEDELIVERY — FrmOrderPartSearch.cs:171
        await db.SaveChangesAsync();
        // Nguồn có 2 trạng thái theo tiến độ về hàng: '2' Hàng đang về, '3' Hoàn thành (Const.Main.cs:422-423).
        var o = await GetAsync(d.OrderPartId);
        if (o != null)
        {
            var all = o.Details.All(x => x.PendingDeliveryQty == 0);
            o.Status = all ? PartOrderStatus.Finish : PartOrderStatus.SIExecuting;
            await db.SaveChangesAsync();
            log.LogInformation("Đơn {No}: về thêm {Qty}, trạng thái {St}", o.OrderNo, qty, o.StatusText);
        }
        return (true, $"Đã ghi nhận về {qty}. Còn chờ {d.PendingDeliveryQty}.");
    }

    public async Task<(bool ok, string msg)> SetStatusAsync(int id, string status)
    {
        var o = await db.PartOrders.FirstOrDefaultAsync(x => x.Id == id);
        if (o == null) return (false, "Không tìm thấy đơn hàng.");
        if (status is not (PartOrderStatus.Create or PartOrderStatus.Confirm or PartOrderStatus.SIExecuting or PartOrderStatus.Finish))
            return (false, "Trạng thái không hợp lệ.");
        o.Status = status; await db.SaveChangesAsync();
        return (true, "Trạng thái: " + o.StatusText);
    }
}
