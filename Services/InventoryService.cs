using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models;

namespace MiniService.Services;

public record InvDash(int Parts, int LowStock, decimal StockValue, int StockOutsToday);

public interface IInventoryService
{
    Task<List<Part>> PartsAsync(string? q);
    Task<(bool ok, string msg, int id)> CreatePartAsync(Part p);
    Task<(bool ok, string msg)> ReceiveAsync(int partId, int qty);           // nhập kho
    Task<(bool ok, string msg)> IssueStockAsync(int partId, int qty, int? roId, string? reason);  // xuất kho
    Task<List<StockOut>> StockOutsAsync(int? roId);
    Task<(bool ok, string msg, decimal total, int issued)> SettleAsync(int roId, PayMethod method, string? note); // quyết toán
    Task<List<Payment>> PaymentsAsync(int roId);
    Task<InvDash> DashboardAsync();
}

public class InventoryService(AppDbContext db, ILogger<InventoryService> log) : IInventoryService
{
    public Task<List<Part>> PartsAsync(string? q)
    {
        var query = db.Parts.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => p.Name.Contains(q) || p.Code.Contains(q));
        return query.OrderBy(p => p.Code).ToListAsync();
    }

    public async Task<(bool ok, string msg, int id)> CreatePartAsync(Part p)
    {
        if (string.IsNullOrWhiteSpace(p.Name)) return (false, "Cần tên phụ tùng.", 0);
        if (string.IsNullOrWhiteSpace(p.Code)) p.Code = "PT" + (await db.Parts.CountAsync() + 1).ToString("D3");
        if (await db.Parts.AnyAsync(x => x.Code == p.Code)) return (false, "Mã phụ tùng đã tồn tại.", 0);
        db.Parts.Add(p); await db.SaveChangesAsync();
        return (true, "Đã thêm phụ tùng.", p.Id);
    }

    public async Task<(bool ok, string msg)> ReceiveAsync(int partId, int qty)
    {
        if (qty <= 0) return (false, "Số lượng phải > 0.");
        var p = await db.Parts.FirstOrDefaultAsync(x => x.Id == partId);
        if (p == null) return (false, "Không tìm thấy phụ tùng.");
        p.OnHand += qty; await db.SaveChangesAsync();
        return (true, $"Đã nhập {qty} {p.Unit}. Tồn: {p.OnHand}.");
    }

    // Xuất kho: KHÔNG cho âm tồn.
    public async Task<(bool ok, string msg)> IssueStockAsync(int partId, int qty, int? roId, string? reason)
    {
        if (qty <= 0) return (false, "Số lượng phải > 0.");
        var p = await db.Parts.FirstOrDefaultAsync(x => x.Id == partId);
        if (p == null) return (false, "Không tìm thấy phụ tùng.");
        if (p.OnHand < qty) return (false, $"Tồn kho không đủ (còn {p.OnHand} {p.Unit}, cần {qty}).");
        string? roCode = null;
        if (roId.HasValue) roCode = (await db.ROs.FirstOrDefaultAsync(r => r.Id == roId))?.Code;
        p.OnHand -= qty;
        db.StockOuts.Add(new StockOut
        {
            Code = "XK" + DateTime.Now.ToString("yyMMddHHmmss"), PartId = p.Id, PartName = p.Name,
            Quantity = qty, UnitPrice = p.Price, ROId = roId, ROCode = roCode, Reason = reason
        });
        await db.SaveChangesAsync();
        log.LogInformation("Xuất kho {Part} x{Qty} (RO={RO}), tồn còn {OnHand}", p.Code, qty, roCode, p.OnHand);
        return (true, $"Đã xuất {qty} {p.Unit} {p.Name}. Tồn còn {p.OnHand}.");
    }

    public Task<List<StockOut>> StockOutsAsync(int? roId)
    {
        var q = db.StockOuts.AsQueryable();
        if (roId.HasValue) q = q.Where(s => s.ROId == roId.Value);
        return q.OrderByDescending(s => s.Id).Take(300).ToListAsync();
    }

    // Quyết toán RO: ghi nhận thanh toán + tự xuất kho các dòng phụ tùng có gắn mã kho (PartId).
    public async Task<(bool ok, string msg, decimal total, int issued)> SettleAsync(int roId, PayMethod method, string? note)
    {
        var ro = await db.ROs.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy RO.", 0, 0);
        if (ro.Status is ROStatus.Rejected or ROStatus.NotResponding) return (false, "RO đã hủy — không quyết toán.", 0, 0);
        if (await db.Payments.AnyAsync(p => p.ROId == roId)) return (false, "RO đã được quyết toán.", 0, 0);
        var total = ro.Lines.Sum(l => l.Quantity * l.UnitPrice);
        if (total <= 0) return (false, "RO chưa có chi phí.", 0, 0);

        // Tự xuất kho cho dòng phụ tùng gắn mã kho (nếu đủ tồn).
        int issued = 0;
        foreach (var l in ro.Lines.Where(x => x.Type == LineType.Part && x.PartId.HasValue))
        {
            var (iok, _) = await IssueStockAsync(l.PartId!.Value, (int)Math.Ceiling(l.Quantity), roId, "Quyết toán RO " + ro.Code);
            if (iok) issued++;
        }
        db.Payments.Add(new Payment { ROId = roId, ROCode = ro.Code, Amount = total, Method = method, Note = note });
        await db.SaveChangesAsync();
        log.LogInformation("Quyết toán RO {RO}: {Total}, xuất kho {Issued} dòng", ro.Code, total, issued);
        return (true, $"Đã quyết toán {total:N0}đ ({method}). Xuất kho {issued} dòng phụ tùng.", total, issued);
    }

    public Task<List<Payment>> PaymentsAsync(int roId) =>
        db.Payments.Where(p => p.ROId == roId).OrderByDescending(p => p.Id).ToListAsync();

    public async Task<InvDash> DashboardAsync()
    {
        var parts = await db.Parts.ToListAsync();
        var today = DateTime.Today;
        return new InvDash(
            parts.Count,
            parts.Count(p => p.OnHand <= p.MinStock),
            parts.Sum(p => p.OnHand * p.Price),
            await db.StockOuts.CountAsync(s => s.CreatedAt >= today));
    }
}
