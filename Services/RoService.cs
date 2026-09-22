using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models;

namespace MiniService.Services;

public record SvcDash(int OpenRO, int InGarage, int DoneToday, decimal RevenueMonth, int Cars, int Parts, int LowStockParts,
    int PendingWarranty, decimal ApprovedWarrantyAmount,
    int TodayAppointments, int PendingAppointments,
    int PendingStockIns, decimal MonthStockInValue,
    int PendingStockOuts, decimal MonthStockOutValue,
    List<(ROStatus Status, int Count)> ByStatus);

public interface IRoService
{
    // master
    Task<List<Customer>> CustomersAsync(string? q);
    Task<int> CreateCustomerAsync(Customer c);
    Task<List<Car>> CarsAsync(string? q);
    Task<int> CreateCarAsync(Car car);
    // parts & inventory
    Task<List<Part>> PartsAsync(string? q, bool? lowStockOnly);
    Task<Part?> GetPartAsync(int id);
    Task<int> CreatePartAsync(Part part);
    Task<(bool ok, string msg)> AdjustStockAsync(int partId, decimal qty, string mode, string? note);
    Task<List<Part>> PartsForSelectAsync();
    // RO
    Task<List<RepairOrder>> ROsAsync(ROStatus? status, string? q);
    Task<RepairOrder?> GetROAsync(int id);
    Task<int> CreateROAsync(RepairOrder ro);
    Task AddLineAsync(int roId, LineType type, string name, decimal qty, decimal price, int? partId = null, ExpenseType expenseType = ExpenseType.Customer);
    Task RemoveLineAsync(int lineId);
    Task<(bool ok, string msg)> TransitionAsync(int roId, ROStatus to);
    Task<(bool ok, string msg)> DeleteROAsync(int roId);
    Task<SvcDash> DashboardAsync();
    // warranty
    Task<List<WarrantyReport>> WarrantyReportsAsync(WarrantyStatus? status, string? q);
    Task<WarrantyReport?> GetWarrantyReportAsync(int id);
    Task<int> CreateWarrantyReportFromROAsync(int roId, string issueDesc, string diagResult, string? errCodeCD, string? errCodePN, int? partIdError, string createdBy);
    Task<(bool ok, string msg)> TransitionWarrantyAsync(int reportId, WarrantyStatus to, decimal? approvedAmount, string? note);
    Task<(bool ok, string msg)> DeleteWarrantyReportAsync(int reportId);
    Task<List<RepairOrder>> ROsEligibleForWarrantyAsync();
    // appointments (Ser_App)
    Task<List<Appointment>> AppointmentsAsync(AppointmentStatus? status, string? q, DateTime? date);
    Task<Appointment?> GetAppointmentAsync(int id);
    Task<int> CreateAppointmentAsync(Appointment app);
    Task<(bool ok, string msg)> TransitionAppointmentStatusAsync(int id, AppointmentStatus to, string? cancelReason = null);
    Task<(bool ok, string msg, int? roId)> CheckInAppointmentAsync(int id, int odometer, string? technician);
    Task<(bool ok, string msg)> DeleteAppointmentAsync(int id);
    // stock-in (Ser_Inv_StockIn)
    Task<List<StockIn>> StockInsAsync(StockInStatus? status, string? q, DateTime? fromDate, DateTime? toDate);
    Task<StockIn?> GetStockInAsync(int id);
    Task<int> CreateStockInAsync(StockIn stockIn, List<StockInDetail> items);
    Task<(bool ok, string msg)> TransitionStockInStatusAsync(int id, StockInStatus to, string? approvedBy = null, string? note = null);
    Task<(bool ok, string msg)> DeleteStockInAsync(int id);
    // stock-out (Ser_Inv_StockOut)
    Task<List<StockOut>> StockOutsAsync(StockOutStatus? status, string? q, DateTime? fromDate, DateTime? toDate, int? roId = null);
    Task<StockOut?> GetStockOutAsync(int id);
    Task<int> CreateStockOutAsync(StockOut stockOut, List<StockOutDetail> items);
    Task<(bool ok, string msg)> TransitionStockOutStatusAsync(int id, StockOutStatus to, string? approvedBy = null, string? note = null);
    Task<(bool ok, string msg)> DeleteStockOutAsync(int id);
    Task<List<RepairOrder>> ROsForStockOutAsync();
    // dropdown data
    Task<List<Car>> CarsForSelectAsync();
}

public class RoService(AppDbContext db) : IRoService
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

    /// <summary>Chuyển trạng thái Báo cáo bảo hành theo Ser_WarrantyReport_Status.</summary>
    public static WarrantyStatus[] AllowedNextWarranty(WarrantyStatus s) => s switch
    {
        WarrantyStatus.Pending => [WarrantyStatus.Sent],
        WarrantyStatus.Sent => [WarrantyStatus.Confirmed, WarrantyStatus.Reverted],
        WarrantyStatus.Confirmed => [WarrantyStatus.Accepted, WarrantyStatus.Rejected, WarrantyStatus.Reverted],
        WarrantyStatus.Reverted => [WarrantyStatus.Sent],
        _ => []
    };

    /// <summary>Chuyển trạng thái cuộc hẹn dịch vụ theo SerAppStatus.</summary>
    public static AppointmentStatus[] AllowedNextAppointment(AppointmentStatus s) => s switch
    {
        AppointmentStatus.Pending => [AppointmentStatus.Contacted, AppointmentStatus.Confirmed, AppointmentStatus.Cancelled],
        AppointmentStatus.Contacted => [AppointmentStatus.Confirmed, AppointmentStatus.Cancelled],
        AppointmentStatus.Confirmed => [AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled],
        _ => []
    };

    /// <summary>Chuyển trạng thái Phiếu Nhập kho theo Ser_Inv_StockIn idn.CarService.</summary>
    public static StockInStatus[] AllowedNextStockIn(StockInStatus s) => s switch
    {
        StockInStatus.Pending => [StockInStatus.Executing, StockInStatus.Finished, StockInStatus.Rejected],
        StockInStatus.Executing => [StockInStatus.Finished, StockInStatus.Rejected],
        _ => []
    };

    /// <summary>Chuyển trạng thái Phiếu Xuất kho theo Ser_Inv_StockOut idn.CarService.</summary>
    public static StockOutStatus[] AllowedNextStockOut(StockOutStatus s) => s switch
    {
        StockOutStatus.Pending => [StockOutStatus.Executing, StockOutStatus.Finished, StockOutStatus.Rejected],
        StockOutStatus.Executing => [StockOutStatus.Finished, StockOutStatus.Rejected],
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

    public Task<List<Part>> PartsAsync(string? q, bool? lowStockOnly)
    {
        var query = db.Parts.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Code.Contains(q) || p.Name.Contains(q) || (p.Model != null && p.Model.Contains(q)) || (p.Location != null && p.Location.Contains(q)));
        if (lowStockOnly == true)
            query = query.Where(p => p.InStock <= p.MinStock);
        return query.OrderBy(p => p.Code).ToListAsync();
    }
    public Task<Part?> GetPartAsync(int id) => db.Parts.FirstOrDefaultAsync(p => p.Id == id);
    public async Task<int> CreatePartAsync(Part part)
    {
        if (string.IsNullOrWhiteSpace(part.Code))
            throw new InvalidOperationException("Mã phụ tùng không được để trống.");
        part.Code = part.Code.Trim().ToUpperInvariant();
        part.Name = part.Name.Trim();
        var exists = await db.Parts.AnyAsync(p => p.Code == part.Code);
        if (exists)
            throw new InvalidOperationException($"Mã phụ tùng '{part.Code}' đã tồn tại.");
        db.Parts.Add(part);
        await db.SaveChangesAsync();
        return part.Id;
    }
    public async Task<(bool ok, string msg)> AdjustStockAsync(int partId, decimal qty, string mode, string? note)
    {
        var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == partId);
        if (part == null) return (false, "Không tìm thấy phụ tùng.");
        if (mode == "set")
        {
            if (qty < 0) return (false, "Số lượng tồn không thể âm.");
            part.InStock = qty;
        }
        else
        {
            if (part.InStock + qty < 0) return (false, "Số lượng xuất vượt quá tồn kho hiện có.");
            part.InStock += qty;
        }
        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật tồn kho '{part.Name}': {part.InStock} {part.Unit}.");
    }
    public Task<List<Part>> PartsForSelectAsync() =>
        db.Parts.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();

    public async Task<List<RepairOrder>> ROsAsync(ROStatus? status, string? q)
    {
        var query = db.ROs.Include(r => r.Car).Include(r => r.Customer).Include(r => r.Lines).Include(r => r.Appointment).AsQueryable();
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(r => r.Code.Contains(q) || r.Car.Plate.Contains(q));
        var list = await query.ToListAsync();
        return list.OrderByDescending(r => r.CreatedAt).ToList();
    }

    public Task<RepairOrder?> GetROAsync(int id) =>
        db.ROs.Include(r => r.Car).ThenInclude(c => c.Customer).Include(r => r.Customer).Include(r => r.Lines).Include(r => r.Appointment)
          .Include(r => r.WarrantyReports).Include(r => r.StockOuts)
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

    public async Task AddLineAsync(int roId, LineType type, string name, decimal qty, decimal price, int? partId = null, ExpenseType expenseType = ExpenseType.Customer)
    {
        var ro = await db.ROs.FirstOrDefaultAsync(r => r.Id == roId) ?? throw new KeyNotFoundException();
        if (ro.Status is ROStatus.Finished or ROStatus.Paid or ROStatus.Rejected or ROStatus.NotResponding)
            throw new InvalidOperationException("RO đã kết thúc — không thêm dòng.");

        if (type == LineType.Part && partId.HasValue && partId.Value > 0)
        {
            var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == partId.Value);
            if (part != null)
            {
                if (string.IsNullOrWhiteSpace(name)) name = part.Name;
                if (price <= 0) price = part.SalePrice;
                if (part.InStock >= qty) part.InStock -= qty;
            }
        }

        db.Lines.Add(new RepairLine
        {
            ROId = roId,
            Type = type,
            ExpenseType = expenseType,
            PartId = (type == LineType.Part && partId > 0) ? partId : null,
            Name = name.Trim(),
            Quantity = qty <= 0 ? 1 : qty,
            UnitPrice = price
        });
        await db.SaveChangesAsync();
    }

    public async Task RemoveLineAsync(int lineId)
    {
        var l = await db.Lines.Include(x => x.Part).FirstOrDefaultAsync(x => x.Id == lineId);
        if (l != null)
        {
            if (l.Type == LineType.Part && l.PartId.HasValue && l.Part != null)
            {
                l.Part.InStock += l.Quantity;
            }
            db.Lines.Remove(l);
            await db.SaveChangesAsync();
        }
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
        return (true, $"Đã chuyển sang: {Ui.Status(to).text}.");
    }

    public async Task<(bool ok, string msg)> DeleteROAsync(int roId)
    {
        var ro = await db.ROs.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy RO.");
        if (ro.Status is not (ROStatus.Created or ROStatus.Printed or ROStatus.Rejected or ROStatus.NotResponding))
            return (false, "Chỉ xóa được RO ở trạng thái Lập báo giá / In / Hủy / Không liên lạc.");

        // Luật 2023.H.CarServices: Không được xóa nếu đã có Báo cáo bảo hành đính kèm
        var hasWarranty = await db.WarrantyReports.AnyAsync(w => w.ROId == roId);
        if (hasWarranty)
            return (false, "Không thể xóa RO đã lập Báo cáo bảo hành.");

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
        var totalParts = await db.Parts.CountAsync();
        var lowStockParts = await db.Parts.CountAsync(p => p.InStock <= p.MinStock);

        var pendingWarranty = await db.WarrantyReports.CountAsync(w =>
            w.Status == WarrantyStatus.Pending || w.Status == WarrantyStatus.Sent || w.Status == WarrantyStatus.Confirmed);
        var approvedWarrantyItems = await db.WarrantyReports.Where(w => w.Status == WarrantyStatus.Accepted)
            .Select(w => (decimal?)(w.ApprovedAmount ?? w.ClaimAmount)).ToListAsync();
        var approvedWarranty = approvedWarrantyItems.Sum() ?? 0;

        var todayAppointments = await db.Appointments.CountAsync(a => a.AppointmentDate.Date == today);
        var pendingAppointments = await db.Appointments.CountAsync(a =>
            a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Contacted || a.Status == AppointmentStatus.Confirmed);

        var pendingStockIns = await db.StockIns.CountAsync(s => s.Status == StockInStatus.Pending || s.Status == StockInStatus.Executing);
        var monthStockInItems = await db.StockIns.Include(s => s.Items)
            .Where(s => s.Status == StockInStatus.Finished && s.StockInDate >= monthStart)
            .ToListAsync();
        var monthStockInValue = monthStockInItems.Sum(s => s.Total);

        var pendingStockOuts = await db.StockOuts.CountAsync(s => s.Status == StockOutStatus.Pending || s.Status == StockOutStatus.Executing);
        var monthStockOutItems = await db.StockOuts.Include(s => s.Items)
            .Where(s => s.Status == StockOutStatus.Finished && s.StockOutDate >= monthStart)
            .ToListAsync();
        var monthStockOutValue = monthStockOutItems.Sum(s => s.Total);

        return new SvcDash(
            ros.Count(r => openStatuses.Contains(r.Status)),
            ros.Count(r => r.Status == ROStatus.InGarage),
            ros.Count(r => r.FinishedAt?.Date == today),
            ros.Where(r => r.Status is ROStatus.Paid or ROStatus.Finished && r.CreatedAt >= monthStart).Sum(r => r.Total),
            await db.Cars.CountAsync(),
            totalParts,
            lowStockParts,
            pendingWarranty,
            approvedWarranty,
            todayAppointments,
            pendingAppointments,
            pendingStockIns,
            monthStockInValue,
            pendingStockOuts,
            monthStockOutValue,
            byStatus);
    }

    // --- Warranty Management (Ser_ROWarrantyReport) ---
    public Task<List<WarrantyReport>> WarrantyReportsAsync(WarrantyStatus? status, string? q)
    {
        var query = db.WarrantyReports
            .Include(w => w.Car)
            .Include(w => w.Customer)
            .Include(w => w.RO)
            .Include(w => w.Items)
            .Include(w => w.PartError)
            .AsQueryable();

        if (status.HasValue) query = query.Where(w => w.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(w => w.ReportNo.Contains(term) || w.RO.Code.Contains(term) || w.Car.Plate.Contains(term) || w.Customer.Name.Contains(term));
        }
        return query.OrderByDescending(w => w.CreatedAt).ToListAsync();
    }

    public Task<WarrantyReport?> GetWarrantyReportAsync(int id) =>
        db.WarrantyReports
            .Include(w => w.Car).ThenInclude(c => c.Customer)
            .Include(w => w.Customer)
            .Include(w => w.RO).ThenInclude(r => r.Lines)
            .Include(w => w.Items).ThenInclude(i => i.Part)
            .Include(w => w.PartError)
            .FirstOrDefaultAsync(w => w.Id == id);

    public Task<List<RepairOrder>> ROsEligibleForWarrantyAsync() =>
        db.ROs.Include(r => r.Car).Include(r => r.Customer).Include(r => r.Lines)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task<int> CreateWarrantyReportFromROAsync(int roId, string issueDesc, string diagResult, string? errCodeCD, string? errCodePN, int? partIdError, string createdBy)
    {
        var ro = await db.ROs.Include(r => r.Car).Include(r => r.Customer).Include(r => r.Lines).ThenInclude(l => l.Part)
            .FirstOrDefaultAsync(r => r.Id == roId) ?? throw new InvalidOperationException("RO không tồn tại.");

        if (string.IsNullOrWhiteSpace(issueDesc))
            throw new InvalidOperationException("Triệu chứng / yêu cầu bảo hành không được để trống (CusRequest).");
        if (string.IsNullOrWhiteSpace(diagResult))
            throw new InvalidOperationException("Kết quả chẩn đoán kỹ thuật không được để trống (CarStatus).");

        var reportNo = $"WAR{DateTime.Now:yyMMdd}-{await db.WarrantyReports.CountAsync() + 1:D3}";
        var report = new WarrantyReport
        {
            ReportNo = reportNo,
            ROId = ro.Id,
            CarId = ro.CarId,
            CustomerId = ro.CustomerId,
            Odometer = ro.Odometer,
            Status = WarrantyStatus.Pending,
            IssueDescription = issueDesc.Trim(),
            DiagnosticResult = diagResult.Trim(),
            ErrorCodeCD = string.IsNullOrWhiteSpace(errCodeCD) ? "DTC-001" : errCodeCD.Trim().ToUpperInvariant(),
            ErrorCodePN = string.IsNullOrWhiteSpace(errCodePN) ? "PN-01" : errCodePN.Trim().ToUpperInvariant(),
            PartIDError = (partIdError.HasValue && partIdError.Value > 0) ? partIdError.Value : null,
            CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "web" : createdBy,
            CreatedAt = DateTime.Now
        };

        // Lấy các dòng từ RO: ưu tiên các dòng được gắn ExpenseType = Warranty; nếu chưa đánh dấu thì lấy tất cả dòng
        var warrantyLines = ro.Lines.Where(l => l.ExpenseType == ExpenseType.Warranty).ToList();
        if (warrantyLines.Count == 0) warrantyLines = ro.Lines.ToList();

        foreach (var l in warrantyLines)
        {
            report.Items.Add(new WarrantyReportItem
            {
                Type = l.Type,
                PartId = l.PartId,
                Code = l.Part?.Code ?? (l.Type == LineType.Labor ? "LAB" : "PRT"),
                Name = l.Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                IsAccepted = true
            });
        }
        report.ClaimAmount = report.Items.Sum(i => i.Amount);

        db.WarrantyReports.Add(report);
        await db.SaveChangesAsync();
        return report.Id;
    }

    public async Task<(bool ok, string msg)> TransitionWarrantyAsync(int reportId, WarrantyStatus to, decimal? approvedAmount, string? note)
    {
        var report = await db.WarrantyReports.FirstOrDefaultAsync(w => w.Id == reportId);
        if (report == null) return (false, "Không tìm thấy Báo cáo bảo hành.");

        if (!AllowedNextWarranty(report.Status).Contains(to))
            return (false, $"Không thể chuyển từ '{Ui.WarrantyStatus(report.Status).text}' sang '{Ui.WarrantyStatus(to).text}'.");

        report.Status = to;
        if (to == WarrantyStatus.Sent)
        {
            report.SubmittedAt = DateTime.Now;
        }
        else if (to == WarrantyStatus.Accepted)
        {
            report.DecidedAt = DateTime.Now;
            report.ApprovedAmount = approvedAmount ?? report.ClaimAmount;
            if (!string.IsNullOrWhiteSpace(note)) report.DecisionNote = note.Trim();
        }
        else if (to == WarrantyStatus.Rejected)
        {
            report.DecidedAt = DateTime.Now;
            report.RejectionReason = string.IsNullOrWhiteSpace(note) ? "Không đủ điều kiện bảo hành." : note.Trim();
        }
        else if (to == WarrantyStatus.Reverted)
        {
            report.RejectionReason = string.IsNullOrWhiteSpace(note) ? "Cần bổ sung hồ sơ / hình ảnh hư hỏng." : note.Trim();
        }

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật Báo cáo bảo hành sang: {Ui.WarrantyStatus(to).text}.");
    }

    public async Task<(bool ok, string msg)> DeleteWarrantyReportAsync(int reportId)
    {
        var report = await db.WarrantyReports.Include(w => w.Items).FirstOrDefaultAsync(w => w.Id == reportId);
        if (report == null) return (false, "Không tìm thấy Báo cáo bảo hành.");

        // Guard theo Ser_ROWarrantyReport_DeleteX: Chỉ xóa được khi PEND hoặc REVERT
        if (report.Status is not (WarrantyStatus.Pending or WarrantyStatus.Reverted))
            return (false, "Không thể xóa BCBH đã gửi lên HTC hoặc đã được duyệt.");

        db.WarrantyReportItems.RemoveRange(report.Items);
        db.WarrantyReports.Remove(report);
        await db.SaveChangesAsync();
        return (true, "Đã xóa Báo cáo bảo hành.");
    }

    // --- Service Appointment Management (Ser_App) ---
    public async Task<List<Appointment>> AppointmentsAsync(AppointmentStatus? status, string? q, DateTime? date)
    {
        var query = db.Appointments
            .Include(a => a.Car)
            .Include(a => a.Customer)
            .Include(a => a.RO)
            .AsQueryable();

        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        if (date.HasValue)
        {
            var targetDate = date.Value.Date;
            query = query.Where(a => a.AppointmentDate.Date == targetDate);
        }
        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(a => a.AppNo.ToLower().Contains(kw)
                || a.Car.Plate.ToLower().Contains(kw)
                || a.Car.Model.ToLower().Contains(kw)
                || a.Customer.Name.ToLower().Contains(kw)
                || (a.Customer.Phone != null && a.Customer.Phone.Contains(kw))
                || (a.Advisor != null && a.Advisor.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderBy(a => a.AppointmentDate).ToList();
    }

    public Task<Appointment?> GetAppointmentAsync(int id) =>
        db.Appointments
            .Include(a => a.Car)
            .Include(a => a.Customer)
            .Include(a => a.RO)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<int> CreateAppointmentAsync(Appointment app)
    {
        var car = await db.Cars.FirstOrDefaultAsync(c => c.Id == app.CarId)
            ?? throw new InvalidOperationException("Không tìm thấy thông tin xe.");

        app.CustomerId = car.CustomerId;
        if (string.IsNullOrWhiteSpace(app.AppNo))
        {
            var countToday = await db.Appointments.CountAsync();
            app.AppNo = $"APP{DateTime.Today:yyMMdd}-{countToday + 1:D3}";
        }
        app.Status = AppointmentStatus.Pending;
        app.CreatedAt = DateTime.Now;

        db.Appointments.Add(app);
        await db.SaveChangesAsync();
        return app.Id;
    }

    public async Task<(bool ok, string msg)> TransitionAppointmentStatusAsync(int id, AppointmentStatus to, string? cancelReason = null)
    {
        var app = await db.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return (false, "Không tìm thấy lịch hẹn.");

        if (!AllowedNextAppointment(app.Status).Contains(to))
            return (false, $"Không thể chuyển từ '{Ui.AppointmentStatus(app.Status).text}' sang '{Ui.AppointmentStatus(to).text}'.");

        app.Status = to;
        if (to == AppointmentStatus.Confirmed)
        {
            app.ConfirmedAt = DateTime.Now;
        }
        else if (to == AppointmentStatus.Cancelled)
        {
            app.CancelReason = string.IsNullOrWhiteSpace(cancelReason) ? "Khách báo hủy / bận." : cancelReason.Trim();
        }

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật trạng thái lịch hẹn sang: {Ui.AppointmentStatus(to).text}.");
    }

    public async Task<(bool ok, string msg, int? roId)> CheckInAppointmentAsync(int id, int odometer, string? technician)
    {
        var app = await db.Appointments.Include(a => a.Car).Include(a => a.Customer).FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return (false, "Không tìm thấy lịch hẹn.", null);

        if (app.Status == AppointmentStatus.CheckedIn && app.ROId.HasValue)
            return (false, "Lịch hẹn này đã được tiếp nhận tạo Lệnh sửa chữa.", app.ROId);

        if (app.Status == AppointmentStatus.Cancelled)
            return (false, "Lịch hẹn đã bị hủy, không thể tiếp nhận vào xưởng.", null);

        // Sinh mã RO tự động theo quy chuẩn idn.CarService
        var roCount = await db.ROs.CountAsync();
        var roCode = $"RO{DateTime.Today:yyMMdd}-{roCount + 1:D3}";

        var intakeNote = $"[Đặt hẹn {app.AppNo} - {Ui.AppServiceType(app.ServiceType)}]";
        if (!string.IsNullOrWhiteSpace(app.Cavity)) intakeNote += $" [Khoang: {app.Cavity}]";
        if (!string.IsNullOrWhiteSpace(app.CustomerRequest)) intakeNote += $" {app.CustomerRequest}";

        var ro = new RepairOrder
        {
            Code = roCode,
            CarId = app.CarId,
            CustomerId = app.CustomerId,
            Status = ROStatus.InGarage, // Xe vào xưởng
            Odometer = odometer > 0 ? odometer : 0,
            IntakeNote = intakeNote,
            Technician = !string.IsNullOrWhiteSpace(technician) ? technician.Trim() : app.Advisor,
            AppointmentId = app.Id,
            CreatedBy = "checkin",
            CreatedAt = DateTime.Now,
            IntakeAt = DateTime.Now
        };

        db.ROs.Add(ro);
        await db.SaveChangesAsync();

        app.ROId = ro.Id;
        app.Status = AppointmentStatus.CheckedIn;
        app.CheckedInAt = DateTime.Now;
        await db.SaveChangesAsync();

        return (true, $"Tiếp nhận thành công! Đã tạo Lệnh sửa chữa {ro.Code}.", ro.Id);
    }

    public async Task<(bool ok, string msg)> DeleteAppointmentAsync(int id)
    {
        var app = await db.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return (false, "Không tìm thấy lịch hẹn.");

        if (app.Status is not (AppointmentStatus.Pending or AppointmentStatus.Cancelled))
            return (false, "Chỉ xóa được lịch hẹn ở trạng thái Mới tạo hoặc Đã hủy.");

        if (app.ROId.HasValue)
            return (false, "Không thể xóa lịch hẹn đã sinh Lệnh sửa chữa.");

        db.Appointments.Remove(app);
        await db.SaveChangesAsync();
        return (true, "Đã xóa lịch hẹn thành công.");
    }

    // --- Stock-In Management (Ser_Inv_StockIn & Ser_Inv_StockInDetail) ---
    public async Task<List<StockIn>> StockInsAsync(StockInStatus? status, string? q, DateTime? fromDate, DateTime? toDate)
    {
        var query = db.StockIns.Include(s => s.Items).ThenInclude(i => i.Part).AsQueryable();

        if (status.HasValue) query = query.Where(s => s.Status == status.Value);
        if (fromDate.HasValue) query = query.Where(s => s.StockInDate.Date >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(s => s.StockInDate.Date <= toDate.Value.Date);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(s => s.StockInNo.ToLower().Contains(kw)
                || s.SupplierName.ToLower().Contains(kw)
                || (s.BillNo != null && s.BillNo.ToLower().Contains(kw))
                || (s.Description != null && s.Description.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(s => s.StockInDate).ThenByDescending(s => s.CreatedAt).ToList();
    }

    public Task<StockIn?> GetStockInAsync(int id) =>
        db.StockIns.Include(s => s.Items).ThenInclude(i => i.Part).FirstOrDefaultAsync(s => s.Id == id);

    public async Task<int> CreateStockInAsync(StockIn stockIn, List<StockInDetail> items)
    {
        if (string.IsNullOrWhiteSpace(stockIn.SupplierName))
            throw new InvalidOperationException("Vui lòng nhập tên nhà cung cấp (SupplierName).");

        if (items.Count == 0)
            throw new InvalidOperationException("Vui lòng thêm ít nhất một phụ tùng vào phiếu nhập kho.");

        if (string.IsNullOrWhiteSpace(stockIn.StockInNo))
        {
            var countToday = await db.StockIns.CountAsync();
            stockIn.StockInNo = $"NK{DateTime.Today:yyMMdd}-{countToday + 1:D3}";
        }
        stockIn.Status = StockInStatus.Pending;
        stockIn.CreatedAt = DateTime.Now;

        foreach (var item in items)
        {
            var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId)
                ?? throw new InvalidOperationException($"Phụ tùng ID={item.PartId} không tồn tại.");

            item.PartCode = part.Code;
            item.PartName = part.Name;
            item.Unit = string.IsNullOrWhiteSpace(item.Unit) ? part.Unit : item.Unit;
            if (item.UnitPrice <= 0) item.UnitPrice = part.CostPrice > 0 ? part.CostPrice : part.SalePrice;
            if (string.IsNullOrWhiteSpace(item.Location)) item.Location = part.Location;
            stockIn.Items.Add(item);
        }

        db.StockIns.Add(stockIn);
        await db.SaveChangesAsync();
        return stockIn.Id;
    }

    public async Task<(bool ok, string msg)> TransitionStockInStatusAsync(int id, StockInStatus to, string? approvedBy = null, string? note = null)
    {
        var stockIn = await db.StockIns.Include(s => s.Items).ThenInclude(i => i.Part).FirstOrDefaultAsync(s => s.Id == id);
        if (stockIn == null) return (false, "Không tìm thấy phiếu nhập kho.");

        if (!AllowedNextStockIn(stockIn.Status).Contains(to))
            return (false, $"Không thể chuyển từ '{Ui.StockInStatus(stockIn.Status).text}' sang '{Ui.StockInStatus(to).text}'.");

        if (to == StockInStatus.Finished)
        {
            if (stockIn.Items.Count == 0)
                return (false, "Phiếu nhập kho chưa có phụ tùng nào, không thể duyệt nhập kho.");

            // Tự động tăng tồn kho và cập nhật giá vốn theo Ser_Inv_StockIn_StatusUpdateToFinished
            foreach (var item in stockIn.Items)
            {
                var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId);
                if (part != null)
                {
                    part.InStock += item.Quantity;
                    if (item.UnitPrice > 0)
                    {
                        part.CostPrice = item.UnitPrice; // Cập nhật giá vốn nhập mới nhất
                    }
                }
            }
            stockIn.Status = StockInStatus.Finished;
            stockIn.FinishedAt = DateTime.Now;
            stockIn.ApprovedBy = string.IsNullOrWhiteSpace(approvedBy) ? "Kế toán kho" : approvedBy.Trim();
            if (!string.IsNullOrWhiteSpace(note))
                stockIn.Description = string.IsNullOrWhiteSpace(stockIn.Description) ? note.Trim() : $"{stockIn.Description} | {note.Trim()}";

            await db.SaveChangesAsync();
            return (true, $"Đã duyệt nhập kho {stockIn.StockInNo}! Tồn kho và giá vốn phụ tùng đã được cập nhật thành công.");
        }
        else if (to == StockInStatus.Executing)
        {
            stockIn.Status = StockInStatus.Executing;
            if (!string.IsNullOrWhiteSpace(note))
                stockIn.Description = string.IsNullOrWhiteSpace(stockIn.Description) ? note.Trim() : $"{stockIn.Description} | {note.Trim()}";

            await db.SaveChangesAsync();
            return (true, $"Đã chuyển phiếu {stockIn.StockInNo} sang trạng thái: {Ui.StockInStatus(to).text}.");
        }
        else if (to == StockInStatus.Rejected)
        {
            stockIn.Status = StockInStatus.Rejected;
            if (!string.IsNullOrWhiteSpace(note))
                stockIn.Description = string.IsNullOrWhiteSpace(stockIn.Description) ? $"[Hủy: {note.Trim()}]" : $"{stockIn.Description} [Hủy: {note.Trim()}]";

            await db.SaveChangesAsync();
            return (true, $"Đã hủy phiếu nhập kho {stockIn.StockInNo}.");
        }

        return (false, "Trạng thái không hợp lệ.");
    }

    public async Task<(bool ok, string msg)> DeleteStockInAsync(int id)
    {
        var stockIn = await db.StockIns.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == id);
        if (stockIn == null) return (false, "Không tìm thấy phiếu nhập kho.");

        // Guard theo Ser_Inv_StockIn idn.CarService: Chỉ được xóa khi Pending hoặc Rejected, KHÔNG được xóa khi Finished
        if (stockIn.Status == StockInStatus.Finished)
            return (false, "Không thể xóa phiếu nhập kho đã hoàn tất (Finished).");

        db.StockInDetails.RemoveRange(stockIn.Items);
        db.StockIns.Remove(stockIn);
        await db.SaveChangesAsync();
        return (true, "Đã xóa phiếu nhập kho.");
    }

    // --- Stock-Out Management (Ser_Inv_StockOut & Ser_Inv_StockOutDetail) ---
    public async Task<List<StockOut>> StockOutsAsync(StockOutStatus? status, string? q, DateTime? fromDate, DateTime? toDate, int? roId = null)
    {
        var query = db.StockOuts
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .Include(s => s.RO)
            .Include(s => s.Car)
            .Include(s => s.Customer)
            .AsQueryable();

        if (status.HasValue) query = query.Where(s => s.Status == status.Value);
        if (roId.HasValue && roId.Value > 0) query = query.Where(s => s.ROId == roId.Value);
        if (fromDate.HasValue) query = query.Where(s => s.StockOutDate.Date >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(s => s.StockOutDate.Date <= toDate.Value.Date);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(s => s.StockOutNo.ToLower().Contains(kw)
                || (s.RecipientName != null && s.RecipientName.ToLower().Contains(kw))
                || (s.RO != null && s.RO.Code.ToLower().Contains(kw))
                || (s.Car != null && s.Car.Plate.ToLower().Contains(kw))
                || (s.Customer != null && s.Customer.Name.ToLower().Contains(kw))
                || (s.Description != null && s.Description.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(s => s.StockOutDate).ThenByDescending(s => s.CreatedAt).ToList();
    }

    public Task<StockOut?> GetStockOutAsync(int id) =>
        db.StockOuts
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .Include(s => s.RO).ThenInclude(r => r!.Lines)
            .Include(s => s.Car)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == id);

    public Task<List<RepairOrder>> ROsForStockOutAsync() =>
        db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Lines).ThenInclude(l => l.Part)
            .Where(r => r.Status != ROStatus.Rejected && r.Status != ROStatus.NotResponding && r.Status != ROStatus.Finished)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<int> CreateStockOutAsync(StockOut stockOut, List<StockOutDetail> items)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Vui lòng thêm ít nhất một phụ tùng vào phiếu xuất kho.");

        if (stockOut.ROId.HasValue && stockOut.ROId.Value > 0)
        {
            var ro = await db.ROs.Include(r => r.Car).Include(r => r.Customer).FirstOrDefaultAsync(r => r.Id == stockOut.ROId.Value);
            if (ro != null)
            {
                stockOut.CarId ??= ro.CarId;
                stockOut.CustomerId ??= ro.CustomerId;
                if (string.IsNullOrWhiteSpace(stockOut.RecipientName))
                    stockOut.RecipientName = ro.Technician ?? ro.Customer.Name;
            }
        }

        if (string.IsNullOrWhiteSpace(stockOut.StockOutNo))
        {
            var countToday = await db.StockOuts.CountAsync();
            stockOut.StockOutNo = $"XK{DateTime.Today:yyMMdd}-{countToday + 1:D3}";
        }
        stockOut.Status = StockOutStatus.Pending;
        stockOut.CreatedAt = DateTime.Now;

        foreach (var item in items)
        {
            var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId)
                ?? throw new InvalidOperationException($"Phụ tùng ID={item.PartId} không tồn tại.");

            item.PartCode = part.Code;
            item.PartName = part.Name;
            item.Unit = string.IsNullOrWhiteSpace(item.Unit) ? part.Unit : item.Unit;
            if (item.UnitPrice <= 0) item.UnitPrice = part.SalePrice > 0 ? part.SalePrice : part.CostPrice;
            if (string.IsNullOrWhiteSpace(item.Location)) item.Location = part.Location;
            stockOut.Items.Add(item);
        }

        db.StockOuts.Add(stockOut);
        await db.SaveChangesAsync();
        return stockOut.Id;
    }

    public async Task<(bool ok, string msg)> TransitionStockOutStatusAsync(int id, StockOutStatus to, string? approvedBy = null, string? note = null)
    {
        var stockOut = await db.StockOuts.Include(s => s.Items).ThenInclude(i => i.Part).Include(s => s.RO).FirstOrDefaultAsync(s => s.Id == id);
        if (stockOut == null) return (false, "Không tìm thấy phiếu xuất kho.");

        if (!AllowedNextStockOut(stockOut.Status).Contains(to))
            return (false, $"Không thể chuyển từ '{Ui.StockOutStatus(stockOut.Status).text}' sang '{Ui.StockOutStatus(to).text}'.");

        if (to == StockOutStatus.Finished)
        {
            if (stockOut.Items.Count == 0)
                return (false, "Phiếu xuất kho chưa có mặt hàng phụ tùng nào.");

            // Kiểm tra tồn kho thực tế trước khi duyệt xuất (CheckStockBalance theo Ser_Inv_StockOut)
            foreach (var item in stockOut.Items)
            {
                var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId);
                if (part == null)
                    return (false, $"Phụ tùng '{item.PartCode}' không còn tồn tại trong hệ thống.");

                if (part.InStock < item.Quantity)
                    return (false, $"Không đủ tồn kho cho '{part.Name}' [{part.Code}]: Hiện còn {part.InStock:0.##} {part.Unit}, yêu cầu xuất {item.Quantity:0.##} {part.Unit}.");
            }

            // Trừ tồn kho thực tế
            foreach (var item in stockOut.Items)
            {
                var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId);
                if (part != null)
                {
                    part.InStock -= item.Quantity;
                }
            }

            stockOut.Status = StockOutStatus.Finished;
            stockOut.FinishedAt = DateTime.Now;
            stockOut.ApprovedBy = string.IsNullOrWhiteSpace(approvedBy) ? "Thủ kho" : approvedBy.Trim();
            if (!string.IsNullOrWhiteSpace(note))
                stockOut.Description = string.IsNullOrWhiteSpace(stockOut.Description) ? note.Trim() : $"{stockOut.Description} | {note.Trim()}";

            await db.SaveChangesAsync();
            return (true, $"Đã hoàn tất xuất kho {stockOut.StockOutNo}! Đã trừ tồn kho {stockOut.Items.Count} mặt hàng phụ tùng.");
        }
        else if (to == StockOutStatus.Executing)
        {
            stockOut.Status = StockOutStatus.Executing;
            if (!string.IsNullOrWhiteSpace(note))
                stockOut.Description = string.IsNullOrWhiteSpace(stockOut.Description) ? note.Trim() : $"{stockOut.Description} | {note.Trim()}";

            await db.SaveChangesAsync();
            return (true, $"Đã chuyển phiếu xuất {stockOut.StockOutNo} sang trạng thái: {Ui.StockOutStatus(to).text}.");
        }
        else if (to == StockOutStatus.Rejected)
        {
            stockOut.Status = StockOutStatus.Rejected;
            if (!string.IsNullOrWhiteSpace(note))
                stockOut.Description = string.IsNullOrWhiteSpace(stockOut.Description) ? $"[Hủy: {note.Trim()}]" : $"{stockOut.Description} [Hủy: {note.Trim()}]";

            await db.SaveChangesAsync();
            return (true, $"Đã hủy phiếu xuất kho {stockOut.StockOutNo}.");
        }

        return (false, "Trạng thái không hợp lệ.");
    }

    public async Task<(bool ok, string msg)> DeleteStockOutAsync(int id)
    {
        var stockOut = await db.StockOuts.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == id);
        if (stockOut == null) return (false, "Không tìm thấy phiếu xuất kho.");

        // Guard: Chỉ được xóa khi Pending hoặc Rejected, KHÔNG được xóa khi Finished
        if (stockOut.Status == StockOutStatus.Finished)
            return (false, "Không thể xóa phiếu xuất kho đã hoàn tất (Finished).");

        db.StockOutDetails.RemoveRange(stockOut.Items);
        db.StockOuts.Remove(stockOut);
        await db.SaveChangesAsync();
        return (true, "Đã xóa phiếu xuất kho.");
    }
}
