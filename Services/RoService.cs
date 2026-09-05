using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models;

namespace MiniService.Services;

public record SvcDash(int OpenRO, int InGarage, int DoneToday, decimal RevenueMonth, int Cars,
    List<(ROStatus Status, int Count)> ByStatus);

public interface IRoService
{
    // master
    Task<List<Customer>> CustomersAsync(string? q);
    Task<int> CreateCustomerAsync(Customer c);
    Task<List<Car>> CarsAsync(string? q);
    Task<int> CreateCarAsync(Car car);
    // RO
    Task<List<RepairOrder>> ROsAsync(ROStatus? status, string? q);
    Task<RepairOrder?> GetROAsync(int id);
    Task<int> CreateROAsync(RepairOrder ro);
    Task AddLineAsync(int roId, LineType type, string name, decimal qty, decimal price);
    Task RemoveLineAsync(int lineId);
    Task<(bool ok, string msg)> TransitionAsync(int roId, ROStatus to);
    Task<(bool ok, string msg)> IssueEInvoiceAsync(int roId);
    Task<(bool ok, string msg)> FileInsuranceClaimAsync(int roId);
    Task<(bool ok, string msg)> DeleteROAsync(int roId);
    Task<SvcDash> DashboardAsync();
    // dropdown data
    Task<List<Car>> CarsForSelectAsync();
}

public class RoService(AppDbContext db, IIntegrationService integ) : IRoService
{
    /// <summary>Chuyển trạng thái hợp lệ theo state machine idn.CarService.</summary>
    public static ROStatus[] AllowedNext(ROStatus s) => s switch
    {
        ROStatus.Created => [ROStatus.Printed, ROStatus.Rejected],
        ROStatus.Printed => [ROStatus.HasRO, ROStatus.Wait4Part, ROStatus.Rejected],
        ROStatus.Wait4Part => [ROStatus.HasPart, ROStatus.NotResponding],
        ROStatus.HasPart => [ROStatus.HasRO],
        ROStatus.HasRO => [ROStatus.InGarage, ROStatus.Rejected],
        ROStatus.InGarage => [ROStatus.Repaired],
        ROStatus.Repaired => [ROStatus.CheckEnd],
        ROStatus.CheckEnd => [ROStatus.Paid],
        ROStatus.Paid => [ROStatus.Finished],
        _ => []
    };

    public Task<List<Customer>> CustomersAsync(string? q)
    {
        var query = db.Customers.Include(c => c.Cars).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(c => c.Name.Contains(q) || c.Code.Contains(q) || (c.Phone ?? "").Contains(q));
        return query.OrderBy(c => c.Name).ToListAsync();
    }
    public async Task<int> CreateCustomerAsync(Customer c)
    {
        if (string.IsNullOrWhiteSpace(c.Code)) c.Code = $"KH{await db.Customers.CountAsync() + 1:D4}";
        db.Customers.Add(c); await db.SaveChangesAsync(); return c.Id;
    }

    public Task<List<Car>> CarsAsync(string? q)
    {
        var query = db.Cars.Include(c => c.Customer).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(c => c.Plate.Contains(q) || c.Model.Contains(q) || (c.Vin ?? "").Contains(q));
        return query.OrderBy(c => c.Plate).ToListAsync();
    }
    public Task<List<Car>> CarsForSelectAsync() => db.Cars.Include(c => c.Customer).OrderBy(c => c.Plate).ToListAsync();
    public async Task<int> CreateCarAsync(Car car) { db.Cars.Add(car); await db.SaveChangesAsync(); return car.Id; }

    public async Task<List<RepairOrder>> ROsAsync(ROStatus? status, string? q)
    {
        var query = db.ROs.Include(r => r.Car).Include(r => r.Customer).Include(r => r.Lines).AsQueryable();
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(r => r.Code.Contains(q) || r.Car.Plate.Contains(q));
        var list = await query.ToListAsync();
        return list.OrderByDescending(r => r.CreatedAt).ToList();
    }

    public Task<RepairOrder?> GetROAsync(int id) =>
        db.ROs.Include(r => r.Car).ThenInclude(c => c.Customer).Include(r => r.Customer).Include(r => r.Lines)
          .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<int> CreateROAsync(RepairOrder ro)
    {
        var car = await db.Cars.FirstOrDefaultAsync(c => c.Id == ro.CarId) ?? throw new InvalidOperationException("Xe không tồn tại.");
        ro.CustomerId = car.CustomerId;
        ro.Code = $"RO{DateTime.Now:yyMMdd}-{await db.ROs.CountAsync() + 1:D3}";
        ro.Status = ROStatus.Created;
        db.ROs.Add(ro);
        await db.SaveChangesAsync();
        return ro.Id;
    }

    public async Task AddLineAsync(int roId, LineType type, string name, decimal qty, decimal price)
    {
        var ro = await db.ROs.FirstOrDefaultAsync(r => r.Id == roId) ?? throw new KeyNotFoundException();
        if (ro.Status is ROStatus.Finished or ROStatus.Paid or ROStatus.Rejected or ROStatus.NotResponding)
            throw new InvalidOperationException("RO đã kết thúc — không thêm dòng.");
        db.Lines.Add(new RepairLine { ROId = roId, Type = type, Name = name.Trim(), Quantity = qty <= 0 ? 1 : qty, UnitPrice = price });
        await db.SaveChangesAsync();
    }

    public async Task RemoveLineAsync(int lineId)
    {
        var l = await db.Lines.Include(x => x.RO).FirstOrDefaultAsync(x => x.Id == lineId);
        if (l == null) return;
        if (l.RO.Status is ROStatus.Finished or ROStatus.Paid or ROStatus.Rejected or ROStatus.NotResponding)
            throw new InvalidOperationException("RO đã kết thúc — không xóa dòng.");
        db.Lines.Remove(l);
        await db.SaveChangesAsync();
    }

    public async Task<(bool ok, string msg)> TransitionAsync(int roId, ROStatus to)
    {
        var ro = await db.ROs.FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy RO.");
        if (!AllowedNext(ro.Status).Contains(to)) return (false, $"Không thể chuyển {Ui.Status(ro.Status).text} → {Ui.Status(to).text}.");
        ro.Status = to;
        if (to == ROStatus.InGarage) ro.IntakeAt ??= DateTime.Now;
        if (to == ROStatus.Finished) ro.FinishedAt ??= DateTime.Now;
        await db.SaveChangesAsync();

        // Quyết toán (PAID) → tự đẩy HĐĐT sang MiniTVAN + tích điểm thân thiết (MiniLoyalty).
        if (to == ROStatus.Paid)
        {
            if (string.IsNullOrEmpty(ro.LoyaltyInfo))
            {
                var full = await db.ROs.Include(r => r.Customer).Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == roId);
                var info = await integ.EarnLoyaltyAsync(full?.Customer?.Phone, full?.Customer?.Name, full?.Total ?? 0, ro.Code);
                if (info != null) { ro.LoyaltyInfo = info; await db.SaveChangesAsync(); }
            }
            if (string.IsNullOrEmpty(ro.EInvoiceCode))
            {
                var (iok, imsg) = await IssueEInvoiceAsync(roId);
                return (true, $"Đã quyết toán. {imsg}");
            }
        }
        return (true, $"Đã chuyển sang: {Ui.Status(to).text}.");
    }

    public async Task<(bool ok, string msg)> IssueEInvoiceAsync(int roId)
    {
        var ro = await db.ROs.Include(r => r.Customer).Include(r => r.Car).Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy RO.");
        if (!string.IsNullOrEmpty(ro.EInvoiceCode)) return (false, "RO đã có hóa đơn điện tử: " + ro.EInvoiceCode);
        if (ro.Total <= 0) return (false, "Chưa có dòng chi phí — không thể xuất hóa đơn.");

        var r = await integ.PushEInvoiceAsync(ro);
        ro.EInvoiceStatus = r.status; ro.EInvoiceAt = DateTime.Now;
        if (r.ok) { ro.EInvoiceCode = r.tctCode; ro.EInvoiceError = null; }
        else { ro.EInvoiceError = r.error; }
        await db.SaveChangesAsync();
        if (r.ok) { await integ.NotifyCustomerAsync(ro); return (true, $"Đã xuất HĐĐT, mã tra cứu {r.tctCode}."); }
        return (false, "Xuất HĐĐT thất bại: " + r.error);
    }

    public async Task<(bool ok, string msg)> FileInsuranceClaimAsync(int roId)
    {
        var ro = await db.ROs.Include(r => r.Car).Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy RO.");
        if (!string.IsNullOrEmpty(ro.InsuranceClaimCode)) return (false, "RO đã có yêu cầu bồi thường: " + ro.InsuranceClaimCode);
        if (ro.Total <= 0) return (false, "Chưa có chi phí sửa chữa — không thể yêu cầu bồi thường.");

        var r = await integ.FileInsuranceClaimAsync(ro.Car?.Plate, ro.Total, $"Sửa chữa {ro.Code}");
        if (r.ok)
        {
            ro.InsuranceClaimCode = r.code; ro.InsuranceClaimStatus = r.status;
            await db.SaveChangesAsync();
            return (true, $"Đã gửi yêu cầu bồi thường {r.code} ({r.status}).");
        }
        return (false, "Không lập được yêu cầu bồi thường: " + r.error);
    }

    public async Task<(bool ok, string msg)> DeleteROAsync(int roId)
    {
        var ro = await db.ROs.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy RO.");
        if (ro.Status is not (ROStatus.Created or ROStatus.Printed or ROStatus.Rejected or ROStatus.NotResponding))
            return (false, "Chỉ xóa được RO ở trạng thái Lập báo giá / In / Hủy / Không liên lạc.");
        db.Lines.RemoveRange(ro.Lines);
        db.ROs.Remove(ro);
        await db.SaveChangesAsync();
        return (true, "Đã xóa RO.");
    }

    public async Task<SvcDash> DashboardAsync()
    {
        var ros = await db.ROs.Include(r => r.Lines).ToListAsync();
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var byStatus = ros.GroupBy(r => r.Status).Select(g => (g.Key, g.Count())).OrderBy(x => (int)x.Key).ToList();
        var openStatuses = new[] { ROStatus.Created, ROStatus.Printed, ROStatus.Wait4Part, ROStatus.HasPart, ROStatus.HasRO, ROStatus.InGarage, ROStatus.Repaired, ROStatus.CheckEnd };
        return new SvcDash(
            ros.Count(r => openStatuses.Contains(r.Status)),
            ros.Count(r => r.Status == ROStatus.InGarage),
            ros.Count(r => r.FinishedAt?.Date == today),
            ros.Where(r => r.Status is ROStatus.Paid or ROStatus.Finished && r.CreatedAt >= monthStart).Sum(r => r.Total),
            await db.Cars.CountAsync(),
            byStatus);
    }
}
