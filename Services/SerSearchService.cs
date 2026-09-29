using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models;

namespace MiniService.Services;

// =============================================================================================
// ĐỢT 2 — ba service port 1:1 từ SQL ĐANG CHẠY của DMSCarSv (nhánh V20.2023.Release.V2).
//  (A) SerRoSearchService      <- BizCarSv.Service.RO.cs:498  (Ser_RO_GetStatusList02_New20230220)
//  (B) RoPartNeedService       <- BizCarSv.PartOrder.cs:3985  (Ser_Inv_OrderInshipment_GetAll_01)
//  (C) SerPaymentReportService <- BizCarSv.Debit.cs:3628       (SerPaymentGet) + :2800 (SerPaymentCreate)
//
// ⚠ SQLite: KHÔNG dùng string.Contains (EF dịch thành instr() ⇒ PHÂN BIỆT hoa/thường, bẫy #18).
//   Mọi ô tìm dùng EF.Functions.Like + escape ký tự đại diện. Vẫn còn gap: LIKE của SQLite chỉ
//   bỏ hoa/thường với ASCII nên '%Văn%' ≠ '%VĂN%' (bẫy #24) — khai trong báo cáo, không nói đã khớp.
// =============================================================================================

/// <summary>Bọc chung phần escape cho LIKE (SQLite/Postgres đều dùng ESCAPE '\').</summary>
internal static class LikeUtil
{
    /// <summary>Escape '%', '_' và '\' của người dùng để chúng thành ký tự thường.</summary>
    public static string Esc(string kw) => kw.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    /// <summary>
    /// Chuẩn hoá biển số y như nguồn: bỏ ' ', '.', '-', '_' ở CẢ ô nhập và cột DB
    /// (BizCarSv.Service.RO.cs:760-762 — PlateNoProxy + strPlateNoParttern.Replace).
    /// </summary>
    public static string NormPlate(string s) =>
        s.Replace(" ", "").Replace(".", "").Replace("-", "").Replace("_", "");
}

// ------------------------------------------------------------------ (A) Tra cứu lệnh sửa chữa

public record SerRoSearchArgs(
    DateTime? CheckInFrom, DateTime? CheckInTo,
    DateTime? ActualDeliveryFrom, DateTime? ActualDeliveryTo,
    string[]? StatusCodes, string? PlateNo, string? FrameNo,
    string? QuotationNo, string? CusName, string? DealerCode,
    // Phân trang chỉ có ở BẢN WEB của cùng màn (2023.H.CarServices ROSearchInfoController.SearchDL,
    // tham số Ft_PageIndex/Ft_PageSize, mặc định 0/10). Bỏ trống ⇒ dùng đúng bản WinForm: TOP 500.
    int? PageIndex = null, int? PageSize = null);

public interface ISerRoSearchService
{
    Task<(List<SerRoSearchRow> rows, int itemCount, int pageCount)> SearchAsync(SerRoSearchArgs a);
}

public class SerRoSearchService(AppDbContext db) : ISerRoSearchService
{
    /// <summary>TOP 500 — BizCarSv.Service.RO.cs:572. Nguồn chốt cứng, không cấu hình được.</summary>
    private const int TOP_500 = 500;

    public async Task<(List<SerRoSearchRow> rows, int itemCount, int pageCount)> SearchAsync(SerRoSearchArgs a)
    {
        var q = db.ROs.Include(r => r.Car).Include(r => r.Customer).AsQueryable();

        // --- Lọc đại lý: nguồn luôn truyền đại lý của người đăng nhập (FrmROSearchInfo.cs:149).
        //     Đích cô lập bằng OrgId nên DealerCode chỉ là bộ lọc tuỳ chọn (khai GAP trong báo cáo).
        if (!string.IsNullOrWhiteSpace(a.DealerCode))
            q = q.Where(r => r.DealerCode == a.DealerCode);

        // --- Khoảng ngày vào xưởng: nguồn so CHUỖI 'yyyy-MM-dd' của convert(nvarchar, ro.CheckinDate, 23)
        //     (Service.RO.cs:766) ⇒ so ở mức NGÀY, bao gồm cả hai đầu. Đích so theo .Date cho tương đương.
        if (a.CheckInFrom.HasValue)
        {
            var f = a.CheckInFrom.Value.Date;
            q = q.Where(r => r.IntakeAt != null && r.IntakeAt!.Value.Date >= f);
        }
        if (a.CheckInTo.HasValue)
        {
            var t = a.CheckInTo.Value.Date;
            q = q.Where(r => r.IntakeAt != null && r.IntakeAt!.Value.Date <= t);
        }
        // --- Khoảng ngày giao xe thực tế (Service.RO.cs:767). Màn FrmROSearchInfo truyền "" cho cả hai
        //     (FrmROSearchInfo.cs:152-153) ⇒ trên WinForm bộ lọc này CHẾT; giữ ở tầng biz cho đúng nguồn.
        if (a.ActualDeliveryFrom.HasValue)
        {
            var f = a.ActualDeliveryFrom.Value.Date;
            q = q.Where(r => r.FinishedAt != null && r.FinishedAt!.Value.Date >= f);
        }
        if (a.ActualDeliveryTo.HasValue)
        {
            var t = a.ActualDeliveryTo.Value.Date;
            q = q.Where(r => r.FinishedAt != null && r.FinishedAt!.Value.Date <= t);
        }

        // --- Danh sách trạng thái: rỗng ⇒ KHÔNG lọc (FrmROSearchInfo.cs:138 + SqlUtils.BuildClauseConditionList).
        if (a.StatusCodes is { Length: > 0 })
        {
            var stages = a.StatusCodes.Select(SerRoStage.Parse).Where(x => x != null)
                                      .Select(x => x!.Value).Distinct().ToList();
            // Mã lạ bị bỏ; nếu SẠCH mã lạ thì nguồn sinh `and ro.Status in ()` — SQL Server lỗi cú pháp,
            // nên ở đích trả rỗng (không đoán) thay vì âm thầm bỏ bộ lọc.
            if (stages.Count == 0) return ([], 0, 0);
            q = q.Where(r => stages.Contains(r.Status));
        }

        // --- Ô tìm biển số: bọc %...% rồi so trên cột đã bỏ ' . - _ ' (Service.RO.cs:761-762).
        if (!string.IsNullOrWhiteSpace(a.PlateNo))
        {
            var kw = "%" + LikeUtil.Esc(LikeUtil.NormPlate(a.PlateNo.Trim())) + "%";
            q = q.Where(r => EF.Functions.Like(
                r.Car.Plate.Replace(" ", "").Replace(".", "").Replace("-", "").Replace("_", ""), kw, "\\"));
        }
        // --- Ô tìm số khung (Service.RO.cs:760: like '%...%' trên ro.FrameNo).
        if (!string.IsNullOrWhiteSpace(a.FrameNo))
        {
            var kw = "%" + LikeUtil.Esc(a.FrameNo.Trim()) + "%";
            q = q.Where(r => r.Car.Vin != null && EF.Functions.Like(r.Car.Vin!, kw, "\\"));
        }
        // --- Ô tìm tên khách: like '%...%' trên cus.CusName (Service.RO.cs:765).
        if (!string.IsNullOrWhiteSpace(a.CusName))
        {
            var kw = "%" + LikeUtil.Esc(a.CusName.Trim()) + "%";
            q = q.Where(r => EF.Functions.Like(r.Customer.Name, kw, "\\"));
        }
        // --- Số báo giá: nguồn CẮT tiền tố 'BG-' rồi so BẰNG với ro.RONo (Service.RO.cs:754 + :764).
        //     Màn này truyền "" (FrmROSearchInfo.cs:156) nên bộ lọc chết trên WinForm; giữ ở tầng biz.
        if (!string.IsNullOrWhiteSpace(a.QuotationNo))
        {
            var no = a.QuotationNo.Trim().Replace("BG-", "");
            q = q.Where(r => r.Code == no);
        }

        // --- Hai chế độ lấy dòng, đúng hai bản nguồn của CÙNG màn:
        //  • WinForm (mặc định): TOP 500 bản MỚI NHẤT theo CheckInDate DESC (Service.RO.cs:572 + :741),
        //    rồi client xếp lại TĂNG dần (SerROService.cs:637 `DefaultView.Sort = CheckInDate ASC`).
        //  • WEB (khi truyền page/pageSize): phân trang theo
        //    `Row_Number() over(order by ro.CheckInDate asc, ro.ROID asc)` + `MyRowIdx` trong
        //    [start, end] và trả MyCount/pageCount (BizCarSv.Master.cs:81102, :75349-81358;
        //    controller ROSearchInfoController.cs:263-268, :374).
        List<RepairOrder> rows;
        int itemCount, pageCount = 0, offset = 0;
        if (a.PageSize is > 0)
        {
            var pageIndex = a.PageIndex ?? 0;          // Ft_PageIndex mặc định 0 (0-based)
            var pageSize = a.PageSize!.Value;          // Ft_PageSize mặc định 10
            itemCount = await q.CountAsync();
            offset = pageIndex * pageSize;
            rows = await q.OrderBy(r => r.IntakeAt).ThenBy(r => r.Id)
                          .Skip(offset).Take(pageSize).ToListAsync();
            pageCount = itemCount % pageSize == 0 ? itemCount / pageSize : itemCount / pageSize + 1;
        }
        else
        {
            rows = await q.OrderByDescending(r => r.IntakeAt).ThenByDescending(r => r.Id)
                          .Take(TOP_500).ToListAsync();
            rows = rows.OrderBy(r => r.IntakeAt).ThenBy(r => r.Id).ToList();
            itemCount = rows.Count;
            pageCount = 1;
        }

        var caller = a.DealerCode;   // '@strCallerDealerCode' của nguồn (Service.RO.cs:730)
        var outRows = rows
            .Select((r, i) =>
            {
                var code = SerRoStage.Of(r.Status);
                var name = SerRoStage.StatusName(r.Status);
                bool sameDealer = caller != null && r.DealerCode == caller;
                return new SerRoSearchRow
                {
                    RoId = r.Id,
                    RoNo = r.Code,
                    RoRoNo = "BG-" + r.Code,                                  // Service.RO.cs:728
                    NormalizedRoNo = sameDealer ? "LS-" + r.Code : "******",   // :729-732
                    DealerCode = r.DealerCode,
                    PlateNo = r.Car.Plate,
                    FrameNo = r.Car.Vin,
                    // tm.TradeMarkName + ' - ' + mdl.ModelName (Service.RO.cs:697). Nguồn KHÔNG dùng
                    // bản ghi đè của ro (tm1/mdl1) cho cột này — giữ đúng, chỉ ghép hãng + model của XE.
                    TradeMarkNameModel = string.IsNullOrWhiteSpace(r.Car.TradeMarkName)
                        ? r.Car.Model : r.Car.TradeMarkName + " - " + r.Car.Model,
                    ColorCode = r.Car.Color,
                    CusName = r.Customer.Name,
                    OwnerName = r.Customer.Name,
                    CusRequest = r.CustomerRequest,
                    CheckInDate = r.IntakeAt,
                    PlanedDeliveryDate = r.ExpectedDelivery,
                    PaidCreatedDate = r.PaidCreatedDate,
                    Creator = r.CreatedBy,                                     // cột nhãn "CVDV" bind ro.Creator (:66)
                    // CreatorName = isnull(su.UserName, ro.Creator) — đích không có bảng người dùng
                    // nên luôn là nhánh fallback (BizCarSv.Master.cs:75384).
                    CreatorName = r.CreatedBy,
                    NormalizedCreator = sameDealer ? (r.CreatedBy ?? "") : "******",  // :733-736
                    StatusCode = code,
                    StatusName = name,
                    RowColor = RowColorOf(name),
                    MyRowIdx = offset + i + 1
                };
            }).ToList();
        return (outRows, itemCount, pageCount);
    }

    /// <summary>
    /// Màu nền ô biển số / số báo giá theo StatusName — FrmROSearchInfo.cs:389-427.
    /// Nguồn KHÔNG có nhánh cho "Hủy, Hẹn lại" và "Không xác định" ⇒ trả rỗng (không tự thêm màu).
    /// </summary>
    private static string RowColorOf(string statusName) => statusName switch
    {
        "Chờ sửa" => "#FFC0FF",              // Color.FromArgb(255,192,255) — :394
        "Lệnh hủy" => "#FFFF80",             // (255,255,128) — :399
        "Sửa xong" => "#C0FFFF",             // (192,255,255) — :404
        "Đã giao xe" => "#C0FFC0",           // (192,255,192) — :409
        "Đang sửa" => "#C0C0FF",             // (192,192,255) — :414
        "Kiểm tra cuối cùng" => "#E6E6FA",   // Color.Lavender — :421
        "Thanh toán xong" => "#FFC0C0",      // (255,192,192) — :426
        _ => ""
    };
}

// ------------------------------------------- (B) Phụ tùng của RO đang đợi hàng (để lập đơn đặt)

public interface IRoPartNeedService
{
    Task<(List<RoPartNeedRow> rows, int total, int pageCount)> SearchAsync(int pageSize, int currentPage, string? dealerCode);
    Task<(bool ok, string msg, List<RoPartNeedRow> chosen)> SelectAsync(int[] partIds, int pageSize, int currentPage, string? dealerCode);
}

public class RoPartNeedService(AppDbContext db) : IRoPartNeedService
{
    /// <summary>
    /// Port 1:1 `Ser_Inv_OrderInshipment_GetAll_01` — BizCarSv.PartOrder.cs:4041-4080.
    /// Điều kiện ĐANG CHẠY: ro.status = 'w4p' (:4068) · ro.DealerCode = @DealerCode (:4069)
    ///                      · part.IsActive = @IsActive, client truyền Flag.Active = '1' (:4070, PartOrderService.cs:46).
    /// Thứ tự: Row_Number() over (order by part.PartId desc) (:4060).
    /// Phân trang: MyRowIdx ∈ [start+1, start+count] (:4046-4048, :4076-4078).
    /// ⚠ Nguồn viết `left join Ser_MST_Part` nhưng lại đặt `part.IsActive` trong WHERE ⇒ thực chất là
    ///   INNER JOIN: dòng phụ tùng chưa gắn PartID bị loại. Đích giữ đúng hành vi đó (inner join).
    /// </summary>
    private IQueryable<RoPartNeedRow> BuildQuery(string? dealerCode)
    {
        var q = from ro in db.ROs
                join line in db.Lines on ro.Id equals line.ROId
                join part in db.Parts on line.PartId equals (int?)part.Id   // inner join — xem chú thích trên
                where ro.Status == ROStatus.Wait4Part
                      && line.Type == LineType.Part
                      && part.IsActive == "1"
                select new { ro, line, part };
        if (!string.IsNullOrWhiteSpace(dealerCode))
            q = q.Where(x => x.ro.DealerCode == dealerCode);
        return q.OrderByDescending(x => x.part.Id).ThenBy(x => x.line.Id)
                .Select(x => new RoPartNeedRow
                {
                    DealerCode = x.ro.DealerCode,
                    RoId = x.ro.Id,
                    RoNo = "BG-" + x.ro.Code,          // PartOrder.cs:4051
                    PartId = x.part.Id,
                    PartCode = x.part.Code,
                    EngName = x.part.EngName,
                    VieName = x.part.Name,
                    Unit = x.part.Unit,
                    Quantity = x.line.Quantity,        // rop.QUANTITY — grid nhãn "SL Cần"
                    Vat = x.line.Vat,
                    Note = x.line.Note
                });
    }

    public async Task<(List<RoPartNeedRow> rows, int total, int pageCount)> SearchAsync(int pageSize, int currentPage, string? dealerCode)
    {
        // Nguồn: page_size ∈ {20,30,50}, mặc định 30 (FrmPartROSearch.cs:27, :146-160).
        if (pageSize <= 0) pageSize = 30;
        if (currentPage <= 0) currentPage = 1;
        var baseQ = BuildQuery(dealerCode);
        var total = await baseQ.CountAsync();          // select Count(0) MyCount — PartOrder.cs:4072-4075
        var start = (currentPage - 1) * pageSize;      // PartOrderService.cs:33
        var rows = await baseQ.Skip(start).Take(pageSize).ToListAsync();
        for (int i = 0; i < rows.Count; i++) rows[i].MyRowIdx = start + i + 1;   // MyRowIdx 1-based (:4059 chú thích)
        // page_count: dư thì +1 (FrmPartROSearch.cs:172-179).
        var pageCount = total % pageSize > 0 ? total / pageSize + 1 : total / pageSize;
        return (rows, total, pageCount);
    }

    /// <summary>
    /// Nút "Chọn" của nguồn (btnSelect_Click, FrmPartROSearch.cs:381-410) gom các dòng đã tick
    /// rồi đẩy sang FrmOrderPartCreate.InsertPartList.
    /// ⚠ NGUỒN HỎNG: dòng 385 `dtPartNEW.Rows.Clear()` xoá chính DataSource của grid TRƯỚC khi
    ///   vòng lặp dòng 386 đọc `gviewSearch.RowCount` ⇒ RowCount = 0 ⇒ luôn rơi vào nhánh
    ///   "Chưa chọn phụ tùng" (:403). Đích làm ĐÚNG ý định (đọc rồi mới trả) và KHAI lệch này,
    ///   không port cái hỏng, cũng không im lặng.
    /// Ba cột nguồn copy sang đơn đặt: PartID, PartCode, VieName, Unit (:392-396).
    /// </summary>
    public async Task<(bool ok, string msg, List<RoPartNeedRow> chosen)> SelectAsync(
        int[] partIds, int pageSize, int currentPage, string? dealerCode)
    {
        var (rows, _, _) = await SearchAsync(pageSize, currentPage, dealerCode);
        var set = (partIds ?? []).ToHashSet();
        var chosen = rows.Where(r => r.PartId != null && set.Contains(r.PartId!.Value)).ToList();
        if (chosen.Count == 0) return (false, "Chưa chọn phụ tùng", []);   // nguyên văn :403
        return (true, $"Đã chọn {chosen.Count} dòng phụ tùng.", chosen);
    }
}

// --------------------------------------------------- (C) Bảng kê phiếu thu (quyết toán dịch vụ)

public interface ISerPaymentReportService
{
    Task<(List<SerPaymentRow> rows, decimal sumAmount)> SearchAsync(DateTime? from, DateTime? to, string[]? types, string? dealerCode);
    Task<(bool ok, string msg, SerPaymentRow? row)> CreateAsync(
        string? dealerCode, int? cusId, string? paymentAmount, string? payPersonName,
        string? payPersonIDCardNo, DateTime? payDate, string? note, string? insNo,
        string? insName, string? insPhone, int? supplierId, string? paymentType);
}

public class SerPaymentReportService(AppDbContext db) : ISerPaymentReportService
{
    /// <summary>
    /// Port 1:1 `SerPaymentGet` — BizCarSv.Debit.cs:3679-3708.
    /// Điều kiện ĐANG CHẠY: p.DealerCode = đại lý gọi (client thêm "=" ở DebitService.cs:659)
    ///   · datediff(day, @FromDate, p.paydate) >= 0 và datediff(day, p.paydate, @ToDate) >= 0
    ///     (:3705-3706) ⇒ so ở mức NGÀY, BAO GỒM cả hai đầu — khác màn phiếu YCXK đợt 1 vốn cắt mất
    ///     phần cuối ngày.
    ///   · p.PaymentType in ('1','2') (:3707) — LUÔN áp, kể cả khi không tick ô nào.
    ///   · thêm danh sách loại từ 2 ô tick (FrmReportSerPaymentGet.cs:143-160).
    /// Chân grid tổng PAYMENTAMOUNT (FrmReportSerPaymentGet.cs:109-111).
    /// </summary>
    public async Task<(List<SerPaymentRow> rows, decimal sumAmount)> SearchAsync(
        DateTime? from, DateTime? to, string[]? types, string? dealerCode)
    {
        var q = db.Payments.AsQueryable();
        if (!string.IsNullOrWhiteSpace(dealerCode)) q = q.Where(p => p.DealerCode == dealerCode);
        if (from.HasValue) { var f = from.Value.Date; q = q.Where(p => p.PaidAt.Date >= f); }
        if (to.HasValue) { var t = to.Value.Date; q = q.Where(p => p.PaidAt.Date <= t); }
        // p.PaymentType in ('1','2') — cố định trong SQL nguồn (:3707).
        q = q.Where(p => p.PaymentType == SerPaymentType.Customer || p.PaymentType == SerPaymentType.Insurance);
        if (types is { Length: > 0 })
        {
            var t2 = types.Where(x => x == SerPaymentType.Customer || x == SerPaymentType.Insurance).Distinct().ToArray();
            if (t2.Length == 0) return ([], 0);       // tick mã lạ ⇒ không đoán, trả rỗng
            q = q.Where(p => t2.Contains(p.PaymentType));
        }

        // left join ser_customer lấy tên + ĐT (Debit.cs:3681-3682). Đích chỉ có MỘT cột Phone
        // (nguồn isnull(cus.tel, cus.mobile)) — lệch đã khai từ đợt 1.
        var list = await (from p in q
                          join c in db.Customers on p.CustomerId equals (int?)c.Id into cs
                          from c in cs.DefaultIfEmpty()
                          orderby p.PaidAt, p.Id
                          select new SerPaymentRow
                          {
                              PaymentId = p.Id,
                              PaymentNo = p.PaymentNo,
                              PayDate = p.PaidAt,
                              CusId = p.CustomerId,
                              CusName = c != null ? c.Name : null,
                              CusPhone = c != null ? c.Phone : null,
                              PayPersonName = p.PayPersonName,
                              PayPersonIDCardNo = p.PayPersonIDCardNo,
                              InsNo = p.InsNo,
                              InsName = p.InsName,
                              InsPhone = p.InsPhone,
                              SupplierId = p.SupplierId,
                              Note = p.Note,
                              PaymentAmount = p.Amount,
                              PaymentType = p.PaymentType,
                              RoId = p.ROId,
                              RoCode = p.ROCode
                          }).ToListAsync();
        foreach (var r in list) r.PaymentTypeText = SerPaymentType.Text(r.PaymentType);
        return (list, list.Sum(r => r.PaymentAmount));
    }

    /// <summary>
    /// Port 1:1 `SerPaymentCreate` — BizCarSv.Debit.cs:2800-2890, guard `checkPaymentFieldEmpty` :69-134.
    /// ⚠ CHỈ BỐN guard ĐANG CHẠY: DealerCode rỗng (:78) · PaymentAmount rỗng (:87) ·
    ///   PaymentAmount không phải số (:96-104) · PayDate rỗng (:126).
    ///   Hai guard "thiếu người thanh toán" và "thiếu số CMND" bị COMMENT ở :106-124 ⇒ KHÔNG port.
    /// </summary>
    public async Task<(bool ok, string msg, SerPaymentRow? row)> CreateAsync(
        string? dealerCode, int? cusId, string? paymentAmount, string? payPersonName,
        string? payPersonIDCardNo, DateTime? payDate, string? note, string? insNo,
        string? insName, string? insPhone, int? supplierId, string? paymentType)
    {
        if (string.IsNullOrWhiteSpace(dealerCode)) return (false, "Thiếu mã đại lý.", null);
        if (string.IsNullOrWhiteSpace(paymentAmount)) return (false, "Thiếu số tiền thu.", null);
        if (!decimal.TryParse(paymentAmount, out var amount)) return (false, "Số tiền thu không phải số.", null);
        if (!payDate.HasValue) return (false, "Thiếu ngày thu.", null);
        // PaymentType: nguồn ghi thẳng giá trị truyền vào, nhưng bảng kê chỉ nhận '1'/'2' (:3707)
        // ⇒ chặn ở đích để không sinh dòng vô hình. Khai là CHẶT HƠN nguồn.
        var type = string.IsNullOrWhiteSpace(paymentType) ? SerPaymentType.Customer : paymentType.Trim();
        if (type != SerPaymentType.Customer && type != SerPaymentType.Insurance)
            return (false, "Loại phiếu thu không hợp lệ (chỉ '1' khách hàng hoặc '2' bảo hiểm).", null);
        if (cusId.HasValue && !await db.Customers.AnyAsync(c => c.Id == cusId.Value))
            return (false, "Không tìm thấy khách hàng.", null);

        var p = new Payment
        {
            ROId = 0, ROCode = "",            // phiếu thu lập tay không gắn RO
            DealerCode = dealerCode.Trim(),
            CustomerId = cusId,
            Amount = amount,
            PayPersonName = payPersonName,
            PayPersonIDCardNo = payPersonIDCardNo,
            PaidAt = payDate.Value,
            Note = note,
            InsNo = insNo, InsName = insName, InsPhone = insPhone,
            SupplierId = supplierId,
            PaymentType = type,
            PaymentNo = await GenPaymentNoAsync(dealerCode.Trim(), payDate.Value)
        };
        db.Payments.Add(p);
        await db.SaveChangesAsync();
        var (rows, _) = await SearchAsync(payDate.Value.Date, payDate.Value.Date, null, dealerCode.Trim());
        return (true, $"Đã lập phiếu thu {p.PaymentNo}.", rows.FirstOrDefault(r => r.PaymentId == p.Id));
    }

    /// <summary>
    /// Sinh số phiếu thu — port 1:1 `SerDebitGeneratePaymentNo` (Debit.cs:3414-3462)
    /// + `GeneratePaymentNo` (:357-392) + `CreateNewDayPaymentNo` (:396-404) + `CreateSameDayPaymentNo` (:407-422).
    /// Dạng: "PT-" + DealerCode + "-" + yyMMdd + "-" + số thứ tự 3 chữ số.
    /// Nguồn chỉ xét các số phiếu có LEN = DealerCode.Length + 14 (:3436-3438) và MAX theo chuỗi;
    /// nếu 6 ký tự ngày trong số MAX khớp ngày hiện tại thì tăng 3 số cuối, không thì về 001.
    /// ⚠ Nguồn dùng getdate() của DB; đích dùng NGÀY THU (PayDate) để số phiếu không lệch ngày chứng từ
    ///   — khai là lệch có chủ đích.
    /// </summary>
    private async Task<string> GenPaymentNoAsync(string dealerCode, DateTime payDate)
    {
        var yymmdd = payDate.ToString("yyMMdd");            // convert(varchar, getdate(), 12) — Debit.cs:3429
        var len = dealerCode.Length + 14;                    // :3436
        var prefix = "PT-" + dealerCode + "-";
        var candidates = await db.Payments
            .Where(p => p.DealerCode == dealerCode && p.PaymentNo != null)
            .Select(p => p.PaymentNo!).ToListAsync();
        var max = candidates.Where(s => s.Length == len).OrderByDescending(s => s, StringComparer.Ordinal).FirstOrDefault();
        if (string.IsNullOrEmpty(max)) return prefix + yymmdd + "-001";           // CreateNewDayPaymentNo
        // 6 ký tự ngày nằm ở vị trí Length-6-3-1 (Debit.cs:373-377).
        var datePart = max.Substring(max.Length - 6 - 3 - 1, 6);
        if (datePart != yymmdd) return prefix + yymmdd + "-001";
        var idx = int.Parse(max.Substring(max.Length - 3, 3)) + 1;                // CreateSameDayPaymentNo
        return max[..^3] + idx.ToString("D3");
    }
}
