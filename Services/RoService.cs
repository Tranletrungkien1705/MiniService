using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models;

namespace MiniService.Services;

public record SvcDash(int OpenRO, int InGarage, int DoneToday, decimal RevenueMonth, int Cars, int Parts, int LowStockParts,
    int PendingWarranty, decimal ApprovedWarrantyAmount,
    int TodayAppointments, int PendingAppointments,
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
}
