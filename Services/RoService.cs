using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models;

namespace MiniService.Services;

public record SvcDash(int OpenRO, int InGarage, int DoneToday, decimal RevenueMonth, int Cars, int Parts, int LowStockParts,
    int PendingWarranty, decimal ApprovedWarrantyAmount,
    int TodayAppointments, int PendingAppointments,
    int PendingStockIns, decimal MonthStockInValue,
    int PendingStockOuts, decimal MonthStockOutValue,
    int PendingCustomerCares, int FeedbackCustomerCares,
    int PendingPayments, decimal MonthPaymentRevenue,
    int PendingQuotes, decimal MonthQuoteValue,
    int ServicePackages,
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
    // customer care 24h (Ser_CustomerCare24h)
    Task<List<CustomerCare>> CustomerCaresAsync(CustomerCareStatus? status, string? q);
    Task<CustomerCare?> GetCustomerCareAsync(int id);
    Task<int> CreateCustomerCareAsync(CustomerCare care);
    Task<(bool ok, string msg)> UpdateCustomerCareSurveyAsync(int id, CustomerCareStatus status, bool hasCarProblem, int? qualityRating, int? staffRating, bool? willingToReturn, int? facilityRating, string? feedback, string? internalNote, string? contactedBy);
    Task<(bool ok, string msg)> DeleteCustomerCareAsync(int id);
    Task<List<RepairOrder>> ROsEligibleForCustomerCareAsync();
    // payment (Ser_Payment)
    Task<List<Payment>> PaymentsAsync(PaymentStatus? status, string? q, DateTime? fromDate, DateTime? toDate, int? roId = null);
    Task<Payment?> GetPaymentAsync(int id);
    Task<int> CreatePaymentAsync(Payment payment);
    Task<(bool ok, string msg)> TransitionPaymentStatusAsync(int id, PaymentStatus to, string? cashier = null, string? note = null);
    Task<(bool ok, string msg)> DeletePaymentAsync(int id);
    Task<List<RepairOrder>> ROsForPaymentAsync();
    // quotation (Ser_Inv_Quote)
    Task<List<Quote>> QuotesAsync(QuoteStatus? status, string? q, DateTime? fromDate, DateTime? toDate);
    Task<Quote?> GetQuoteAsync(int id);
    Task<int> CreateQuoteAsync(Quote quote, List<QuoteItem> items);
    Task<(bool ok, string msg)> TransitionQuoteStatusAsync(int id, QuoteStatus to);
    Task<(bool ok, string msg, int? stockOutId)> ConvertQuoteToStockOutAsync(int id, string? approvedBy = null);
    Task<(bool ok, string msg, int? roId)> ConvertQuoteToROAsync(int id, string? technician = null);
    Task<(bool ok, string msg)> DeleteQuoteAsync(int id);
    Task<List<Customer>> CustomersForSelectAsync();
    // service packages (Ser_ServicePackage)
    Task<List<ServicePackage>> ServicePackagesAsync(string? q, bool? isPublic, bool? isActive);
    Task<ServicePackage?> GetServicePackageAsync(int id);
    Task<int> CreateServicePackageAsync(ServicePackage package, List<ServicePackageItem> items);
    Task<(bool ok, string msg)> DeleteServicePackageAsync(int id);
    Task<List<ServicePackage>> ServicePackagesForSelectAsync();
    Task<(bool ok, string msg, int itemsAdded)> ApplyServicePackageToROAsync(int packageId, int roId);
    Task<(bool ok, string msg, int itemsAdded)> ApplyServicePackageToQuoteAsync(int packageId, int quoteId);
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

    /// <summary>Chuyển trạng thái Phiếu thu theo Ser_Payment idn.CarService.</summary>
    public static PaymentStatus[] AllowedNextPayment(PaymentStatus s) => s switch
    {
        PaymentStatus.Draft => [PaymentStatus.Completed, PaymentStatus.Cancelled],
        PaymentStatus.Completed => [PaymentStatus.Cancelled],
        _ => []
    };

    /// <summary>Chuyển trạng thái Báo giá theo Ser_Inv_Quote idn.CarService.</summary>
    public static QuoteStatus[] AllowedNextQuote(QuoteStatus s) => s switch
    {
        QuoteStatus.Draft => [QuoteStatus.Sent, QuoteStatus.Confirmed, QuoteStatus.Rejected],
        QuoteStatus.Sent => [QuoteStatus.Confirmed, QuoteStatus.Rejected],
        QuoteStatus.Confirmed => [QuoteStatus.Converted, QuoteStatus.Rejected],
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
          .Include(r => r.WarrantyReports).Include(r => r.StockOuts).Include(r => r.CustomerCares).Include(r => r.Payments)
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
        if (to == ROStatus.Finished)
        {
            ro.FinishedAt ??= DateTime.Now;
            // Tự động kích hoạt quy trình CSKH 24h theo Ser_CustomerCare24h idn.CarService
            var hasCare = await db.CustomerCares.AnyAsync(c => c.ROId == roId);
            if (!hasCare)
            {
                var care = new CustomerCare
                {
                    CareNo = $"CC{DateTime.Now:yyMMdd}-{await db.CustomerCares.CountAsync() + 1:D3}",
                    ROId = ro.Id,
                    CarId = ro.CarId,
                    CustomerId = ro.CustomerId,
                    Status = CustomerCareStatus.Pending,
                    CreatedBy = "system"
                };
                db.CustomerCares.Add(care);
            }
        }
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

        var pendingCustomerCares = await db.CustomerCares.CountAsync(c => c.Status == CustomerCareStatus.Pending);
        var feedbackCustomerCares = await db.CustomerCares.CountAsync(c => c.Status == CustomerCareStatus.NeedFeedback);

        var pendingPayments = await db.Payments.CountAsync(p => p.Status == PaymentStatus.Draft);
        var monthPayments = await db.Payments
            .Where(p => p.Status == PaymentStatus.Completed && p.PaymentDate >= monthStart)
            .ToListAsync();
        var monthPaymentRevenue = monthPayments.Sum(p => p.PaymentAmount);

        var pendingQuotes = await db.Quotes.CountAsync(q => q.Status == QuoteStatus.Draft || q.Status == QuoteStatus.Sent);
        var monthQuotes = await db.Quotes.Include(q => q.Items)
            .Where(q => (q.Status == QuoteStatus.Confirmed || q.Status == QuoteStatus.Converted) && q.QuoteDate >= monthStart)
            .ToListAsync();
        var monthQuoteValue = monthQuotes.Sum(q => q.Total);
        var totalServicePackages = await db.ServicePackages.CountAsync();

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
            pendingCustomerCares,
            feedbackCustomerCares,
            pendingPayments,
            monthPaymentRevenue,
            pendingQuotes,
            monthQuoteValue,
            totalServicePackages,
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

    // --- Customer Care 24h Management (Ser_CustomerCare24h) ---
    public async Task<List<CustomerCare>> CustomerCaresAsync(CustomerCareStatus? status, string? q)
    {
        var query = db.CustomerCares
            .Include(c => c.RO)
            .Include(c => c.Car)
            .Include(c => c.Customer)
            .AsQueryable();

        if (status.HasValue) query = query.Where(c => c.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(c => c.CareNo.ToLower().Contains(kw)
                || c.Car.Plate.ToLower().Contains(kw)
                || c.Car.Model.ToLower().Contains(kw)
                || c.Customer.Name.ToLower().Contains(kw)
                || (c.Customer.Phone != null && c.Customer.Phone.ToLower().Contains(kw))
                || (c.RO != null && c.RO.Code.ToLower().Contains(kw))
                || (c.CustomerFeedback != null && c.CustomerFeedback.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(c => c.CreatedAt).ToList();
    }

    public Task<CustomerCare?> GetCustomerCareAsync(int id) =>
        db.CustomerCares
            .Include(c => c.RO).ThenInclude(r => r.Lines).ThenInclude(l => l.Part)
            .Include(c => c.Car)
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<int> CreateCustomerCareAsync(CustomerCare care)
    {
        var ro = await db.ROs.Include(r => r.Car).Include(r => r.Customer).FirstOrDefaultAsync(r => r.Id == care.ROId)
            ?? throw new InvalidOperationException("Không tìm thấy Lệnh sửa chữa.");

        care.CarId = ro.CarId;
        care.CustomerId = ro.CustomerId;
        care.CareNo = $"CC{DateTime.Now:yyMMdd}-{await db.CustomerCares.CountAsync() + 1:D3}";
        care.CreatedAt = DateTime.Now;

        db.CustomerCares.Add(care);
        await db.SaveChangesAsync();
        return care.Id;
    }

    public async Task<(bool ok, string msg)> UpdateCustomerCareSurveyAsync(int id, CustomerCareStatus status,
        bool hasCarProblem, int? qualityRating, int? staffRating, bool? willingToReturn, int? facilityRating,
        string? feedback, string? internalNote, string? contactedBy)
    {
        var care = await db.CustomerCares.FirstOrDefaultAsync(c => c.Id == id);
        if (care == null) return (false, "Không tìm thấy phiếu CSKH.");

        care.Status = status;
        care.HasCarProblem = hasCarProblem;
        care.QualityRating = qualityRating;
        care.StaffRating = staffRating;
        care.WillingToReturn = willingToReturn;
        care.FacilityRating = facilityRating;
        care.CustomerFeedback = feedback?.Trim();
        care.InternalNote = internalNote?.Trim();
        care.ContactedBy = string.IsNullOrWhiteSpace(contactedBy) ? "CSKH" : contactedBy.Trim();
        care.ContactedDate = DateTime.Now;

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật kết quả khảo sát CSKH ({Ui.CustomerCareStatus(status).text}).");
    }

    public async Task<(bool ok, string msg)> DeleteCustomerCareAsync(int id)
    {
        var care = await db.CustomerCares.FirstOrDefaultAsync(c => c.Id == id);
        if (care == null) return (false, "Không tìm thấy phiếu CSKH.");

        db.CustomerCares.Remove(care);
        await db.SaveChangesAsync();
        return (true, "Đã xóa phiếu CSKH.");
    }

    public Task<List<RepairOrder>> ROsEligibleForCustomerCareAsync() =>
        db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.CustomerCares)
            .Where(r => (r.Status == ROStatus.Finished || r.Status == ROStatus.Paid) && !r.CustomerCares.Any())
            .OrderByDescending(r => r.FinishedAt ?? r.CreatedAt)
            .ToListAsync();

    // --- Payment & Cashier Management (Ser_Payment & Ser_PaymentDetail) ---
    public async Task<List<Payment>> PaymentsAsync(PaymentStatus? status, string? q, DateTime? fromDate, DateTime? toDate, int? roId = null)
    {
        var query = db.Payments
            .Include(p => p.RO)
            .Include(p => p.Customer)
            .Include(p => p.Car)
            .AsQueryable();

        if (status.HasValue) query = query.Where(p => p.Status == status.Value);
        if (roId.HasValue && roId.Value > 0) query = query.Where(p => p.ROId == roId.Value);
        if (fromDate.HasValue) query = query.Where(p => p.PaymentDate >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(p => p.PaymentDate <= toDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(p => p.PaymentNo.ToLower().Contains(kw)
                || p.PayPersonName.ToLower().Contains(kw)
                || (p.PayPersonPhone != null && p.PayPersonPhone.ToLower().Contains(kw))
                || (p.TransactionRef != null && p.TransactionRef.ToLower().Contains(kw))
                || p.RO.Code.ToLower().Contains(kw)
                || p.Car.Plate.ToLower().Contains(kw)
                || p.Customer.Name.ToLower().Contains(kw));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.CreatedAt).ToList();
    }

    public Task<Payment?> GetPaymentAsync(int id) =>
        db.Payments
            .Include(p => p.RO).ThenInclude(r => r.Lines)
            .Include(p => p.Customer)
            .Include(p => p.Car)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<int> CreatePaymentAsync(Payment payment)
    {
        var ro = await db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Lines)
            .Include(r => r.Payments)
            .FirstOrDefaultAsync(r => r.Id == payment.ROId)
            ?? throw new InvalidOperationException("Không tìm thấy Lệnh sửa chữa (RO).");

        payment.CustomerId = ro.CustomerId;
        payment.CarId = ro.CarId;
        payment.RoTotalAmount = ro.Total;
        payment.ThirdPartyAmount = ro.WarrantyTotal;
        payment.PayableAmount = Math.Max(0, payment.RoTotalAmount - payment.DiscountAmount - payment.ThirdPartyAmount);

        if (payment.PaymentAmount <= 0)
        {
            payment.PaymentAmount = payment.PayableAmount;
        }

        if (string.IsNullOrWhiteSpace(payment.PayPersonName))
        {
            payment.PayPersonName = ro.Customer.Name;
        }
        if (string.IsNullOrWhiteSpace(payment.PayPersonPhone))
        {
            payment.PayPersonPhone = ro.Customer.Phone;
        }

        payment.PaymentNo = $"PT{DateTime.Now:yyMMdd}-{await db.Payments.CountAsync() + 1:D3}";
        payment.CreatedAt = DateTime.Now;

        if (payment.Status == PaymentStatus.Completed)
        {
            payment.CompletedAt = DateTime.Now;
            // Nếu thu tiền đủ hoặc hoàn tất thanh toán, tự động chuyển trạng thái RO sang PAID nếu đang ở CheckEnd / Repaired / HasRO
            if (ro.Status is ROStatus.CheckEnd or ROStatus.Repaired or ROStatus.HasRO)
            {
                ro.Status = ROStatus.Paid;
            }
        }

        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        return payment.Id;
    }

    public async Task<(bool ok, string msg)> TransitionPaymentStatusAsync(int id, PaymentStatus to, string? cashier = null, string? note = null)
    {
        var payment = await db.Payments
            .Include(p => p.RO).ThenInclude(r => r.Payments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null) return (false, "Không tìm thấy phiếu thu.");

        if (!AllowedNextPayment(payment.Status).Contains(to))
            return (false, $"Không thể chuyển từ '{Ui.PaymentStatus(payment.Status).text}' sang '{Ui.PaymentStatus(to).text}'.");

        if (to == PaymentStatus.Completed)
        {
            payment.Status = PaymentStatus.Completed;
            payment.CompletedAt = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(cashier)) payment.Cashier = cashier.Trim();
            if (!string.IsNullOrWhiteSpace(note)) payment.Note = string.IsNullOrWhiteSpace(payment.Note) ? note.Trim() : $"{payment.Note} | {note.Trim()}";

            if (payment.RO.Status is ROStatus.CheckEnd or ROStatus.Repaired or ROStatus.HasRO)
            {
                payment.RO.Status = ROStatus.Paid;
            }

            await db.SaveChangesAsync();
            return (true, $"Đã hoàn tất thu tiền phiếu {payment.PaymentNo} ({payment.PaymentAmount:N0}đ)! Trạng thái RO đã chuyển sang 'Đã thanh toán'.");
        }
        else if (to == PaymentStatus.Cancelled)
        {
            payment.Status = PaymentStatus.Cancelled;
            if (!string.IsNullOrWhiteSpace(note)) payment.Note = string.IsNullOrWhiteSpace(payment.Note) ? $"[Hủy: {note.Trim()}]" : $"{payment.Note} [Hủy: {note.Trim()}]";

            // Nếu RO đang là Paid và không còn phiếu thu Completed nào khác, có thể đưa RO về CheckEnd
            var otherCompleted = payment.RO.Payments.Any(p => p.Id != id && p.Status == PaymentStatus.Completed);
            if (!otherCompleted && payment.RO.Status == ROStatus.Paid)
            {
                payment.RO.Status = ROStatus.CheckEnd;
            }

            await db.SaveChangesAsync();
            return (true, $"Đã hủy phiếu thu {payment.PaymentNo}.");
        }

        return (false, "Trạng thái không hợp lệ.");
    }

    public async Task<(bool ok, string msg)> DeletePaymentAsync(int id)
    {
        var payment = await db.Payments.FirstOrDefaultAsync(p => p.Id == id);
        if (payment == null) return (false, "Không tìm thấy phiếu thu.");

        if (payment.Status == PaymentStatus.Completed)
            return (false, "Không thể xóa phiếu thu đã hoàn tất (Completed). Vui lòng thực hiện Hủy phiếu nếu cần thu hồi.");

        db.Payments.Remove(payment);
        await db.SaveChangesAsync();
        return (true, "Đã xóa phiếu thu.");
    }

    public Task<List<RepairOrder>> ROsForPaymentAsync() =>
        db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Lines)
            .Include(r => r.Payments)
            .Where(r => r.Status != ROStatus.Created && r.Status != ROStatus.Rejected && r.Status != ROStatus.NotResponding)
            .OrderByDescending(r => r.Status == ROStatus.CheckEnd || r.Status == ROStatus.Repaired)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync();

    // --- Quotation Management (Ser_Inv_Quote & Ser_Inv_QuotePartItems) ---
    public Task<List<Customer>> CustomersForSelectAsync() =>
        db.Customers.Include(c => c.Cars).OrderBy(c => c.Name).ToListAsync();

    public async Task<List<Quote>> QuotesAsync(QuoteStatus? status, string? q, DateTime? fromDate, DateTime? toDate)
    {
        var query = db.Quotes
            .Include(x => x.Customer)
            .Include(x => x.Car)
            .Include(x => x.Items)
            .Include(x => x.StockOut)
            .Include(x => x.RO)
            .AsQueryable();

        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (fromDate.HasValue) query = query.Where(x => x.QuoteDate.Date >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(x => x.QuoteDate.Date <= toDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(x => x.QuoteNo.ToLower().Contains(kw)
                || x.CustomerName.ToLower().Contains(kw)
                || (x.CustomerPhone != null && x.CustomerPhone.Contains(kw))
                || (x.RecipientName != null && x.RecipientName.ToLower().Contains(kw))
                || (x.Car != null && x.Car.Plate.ToLower().Contains(kw))
                || (x.Note != null && x.Note.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(x => x.QuoteDate).ThenByDescending(x => x.CreatedAt).ToList();
    }

    public Task<Quote?> GetQuoteAsync(int id) =>
        db.Quotes
            .Include(q => q.Customer).ThenInclude(c => c!.Cars)
            .Include(q => q.Car)
            .Include(q => q.Items).ThenInclude(i => i.Part)
            .Include(q => q.StockOut)
            .Include(q => q.RO)
            .FirstOrDefaultAsync(q => q.Id == id);

    public async Task<int> CreateQuoteAsync(Quote quote, List<QuoteItem> items)
    {
        if (string.IsNullOrWhiteSpace(quote.CustomerName))
            throw new InvalidOperationException("Vui lòng nhập tên khách hàng.");

        if (items.Count == 0)
            throw new InvalidOperationException("Vui lòng thêm ít nhất một phụ tùng/hạng mục vào báo giá.");

        if (string.IsNullOrWhiteSpace(quote.QuoteNo))
        {
            var countToday = await db.Quotes.CountAsync();
            quote.QuoteNo = $"BG{DateTime.Today:yyMMdd}-{countToday + 1:D3}";
        }
        quote.Status = QuoteStatus.Draft;
        quote.CreatedAt = DateTime.Now;

        foreach (var item in items)
        {
            if (item.PartId.HasValue && item.PartId.Value > 0)
            {
                var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId.Value);
                if (part != null)
                {
                    item.PartCode = part.Code;
                    item.PartName = part.Name;
                    item.Unit = string.IsNullOrWhiteSpace(item.Unit) ? part.Unit : item.Unit;
                    if (item.UnitPrice <= 0) item.UnitPrice = part.SalePrice;
                }
            }
            quote.Items.Add(item);
        }

        db.Quotes.Add(quote);
        await db.SaveChangesAsync();
        return quote.Id;
    }

    public async Task<(bool ok, string msg)> TransitionQuoteStatusAsync(int id, QuoteStatus to)
    {
        var quote = await db.Quotes.FirstOrDefaultAsync(q => q.Id == id);
        if (quote == null) return (false, "Không tìm thấy báo giá.");

        if (!AllowedNextQuote(quote.Status).Contains(to))
            return (false, $"Không thể chuyển từ '{Ui.QuoteStatus(quote.Status).text}' sang '{Ui.QuoteStatus(to).text}'.");

        quote.Status = to;
        if (to == QuoteStatus.Confirmed)
        {
            quote.ConfirmedAt = DateTime.Now;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật trạng thái báo giá {quote.QuoteNo} sang: {Ui.QuoteStatus(to).text}.");
    }

    public async Task<(bool ok, string msg, int? stockOutId)> ConvertQuoteToStockOutAsync(int id, string? approvedBy = null)
    {
        var quote = await db.Quotes.Include(q => q.Items).ThenInclude(i => i.Part).FirstOrDefaultAsync(q => q.Id == id);
        if (quote == null) return (false, "Không tìm thấy báo giá.", null);

        if (quote.Status == QuoteStatus.Converted)
            return (false, "Báo giá này đã được chuyển đổi trước đó.", quote.StockOutId);

        if (quote.Items.Count == 0)
            return (false, "Báo giá không có mặt hàng phụ tùng nào.", null);

        // Tạo phiếu xuất kho loại Normal (Bán lẻ theo báo giá)
        var stockOut = new StockOut
        {
            Type = StockOutType.Normal,
            Status = StockOutStatus.Pending,
            StockOutDate = DateTime.Today,
            StockOutNo = $"XK{DateTime.Today:yyMMdd}-{await db.StockOuts.CountAsync() + 1:D3}",
            CustomerId = quote.CustomerId,
            CarId = quote.CarId,
            QuoteId = quote.Id,
            RecipientName = !string.IsNullOrWhiteSpace(quote.RecipientName) ? quote.RecipientName : quote.CustomerName,
            Description = $"Xuất kho bán lẻ phụ tùng theo Báo giá số {quote.QuoteNo}.",
            CreatedBy = quote.CreatedBy,
            CreatedAt = DateTime.Now
        };

        foreach (var item in quote.Items)
        {
            var partId = item.PartId ?? 0;
            if (partId == 0)
            {
                var p = await db.Parts.FirstOrDefaultAsync(p => p.Code == item.PartCode);
                if (p != null) partId = p.Id;
            }

            if (partId > 0)
            {
                var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == partId);
                stockOut.Items.Add(new StockOutDetail
                {
                    PartId = partId,
                    PartCode = item.PartCode,
                    PartName = item.PartName,
                    Unit = item.Unit,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    VatPercent = item.VatPercent,
                    Location = part?.Location,
                    Note = $"Xuất từ Báo giá {quote.QuoteNo}"
                });
            }
        }

        if (stockOut.Items.Count == 0)
            return (false, "Các phụ tùng trong báo giá chưa được định danh trong danh mục kho.", null);

        db.StockOuts.Add(stockOut);
        await db.SaveChangesAsync();

        quote.Status = QuoteStatus.Converted;
        quote.StockOutId = stockOut.Id;
        await db.SaveChangesAsync();

        return (true, $"Đã chuyển Báo giá {quote.QuoteNo} thành Phiếu xuất kho {stockOut.StockOutNo}.", stockOut.Id);
    }

    public async Task<(bool ok, string msg, int? roId)> ConvertQuoteToROAsync(int id, string? technician = null)
    {
        var quote = await db.Quotes.Include(q => q.Items).Include(q => q.Car).FirstOrDefaultAsync(q => q.Id == id);
        if (quote == null) return (false, "Không tìm thấy báo giá.", null);

        if (quote.Status == QuoteStatus.Converted)
            return (false, "Báo giá này đã được chuyển đổi trước đó.", quote.ROId);

        if (!quote.CarId.HasValue || quote.CarId.Value <= 0)
        {
            // Kiểm tra xem khách hàng có xe nào không
            if (quote.CustomerId.HasValue)
            {
                var firstCar = await db.Cars.FirstOrDefaultAsync(c => c.CustomerId == quote.CustomerId.Value);
                if (firstCar != null) quote.CarId = firstCar.Id;
            }
        }

        if (!quote.CarId.HasValue || quote.CarId.Value <= 0)
            return (false, "Báo giá chưa liên kết thông tin xe để lập Lệnh sửa chữa (RO). Vui lòng cập nhật thông tin xe.", null);

        var car = await db.Cars.FirstOrDefaultAsync(c => c.Id == quote.CarId.Value);
        if (car == null) return (false, "Xe không tồn tại trong hệ thống.", null);

        var ro = new RepairOrder
        {
            CarId = car.Id,
            CustomerId = car.CustomerId,
            Code = $"RO{DateTime.Now:yyMMdd}-{await db.ROs.CountAsync() + 1:D3}",
            Status = ROStatus.Created,
            IntakeNote = $"Lập Lệnh sửa chữa từ Báo giá {quote.QuoteNo}. {(string.IsNullOrWhiteSpace(quote.Note) ? "" : "Ghi chú: " + quote.Note)}",
            Technician = string.IsNullOrWhiteSpace(technician) ? "Thợ tiếp nhận" : technician.Trim(),
            CreatedBy = quote.CreatedBy,
            CreatedAt = DateTime.Now
        };

        foreach (var item in quote.Items)
        {
            ro.Lines.Add(new RepairLine
            {
                Type = LineType.Part,
                ExpenseType = ExpenseType.Customer,
                PartId = item.PartId,
                Name = item.PartName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
        }

        db.ROs.Add(ro);
        await db.SaveChangesAsync();

        quote.Status = QuoteStatus.Converted;
        quote.ROId = ro.Id;
        await db.SaveChangesAsync();

        return (true, $"Đã chuyển Báo giá {quote.QuoteNo} thành Lệnh sửa chữa {ro.Code}.", ro.Id);
    }

    public async Task<(bool ok, string msg)> DeleteQuoteAsync(int id)
    {
        var quote = await db.Quotes.Include(q => q.Items).FirstOrDefaultAsync(q => q.Id == id);
        if (quote == null) return (false, "Không tìm thấy báo giá.");

        if (quote.Status == QuoteStatus.Converted)
            return (false, "Không thể xóa báo giá đã chuyển đổi thành Phiếu xuất kho hoặc Lệnh sửa chữa.");

        db.QuoteItems.RemoveRange(quote.Items);
        db.Quotes.Remove(quote);
        await db.SaveChangesAsync();
        return (true, "Đã xóa báo giá thành công.");
    }

    // --- Service Package Management (Ser_ServicePackage) ---
    public async Task<List<ServicePackage>> ServicePackagesAsync(string? q, bool? isPublic, bool? isActive)
    {
        var query = db.ServicePackages
            .Include(p => p.Items).ThenInclude(i => i.Part)
            .AsQueryable();

        if (isPublic.HasValue) query = query.Where(p => p.IsPublic == isPublic.Value);
        if (isActive.HasValue) query = query.Where(p => p.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(p => p.PackageNo.ToLower().Contains(kw)
                || p.Name.ToLower().Contains(kw)
                || (p.Description != null && p.Description.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderBy(p => p.PackageNo).ToList();
    }

    public Task<ServicePackage?> GetServicePackageAsync(int id) =>
        db.ServicePackages
            .Include(p => p.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<int> CreateServicePackageAsync(ServicePackage package, List<ServicePackageItem> items)
    {
        if (string.IsNullOrWhiteSpace(package.PackageNo))
        {
            var count = await db.ServicePackages.CountAsync();
            package.PackageNo = $"PKG-BD-{count + 1:D2}";
        }
        else
        {
            package.PackageNo = package.PackageNo.Trim().ToUpperInvariant();
        }

        var exists = await db.ServicePackages.AnyAsync(p => p.PackageNo == package.PackageNo);
        if (exists)
            throw new InvalidOperationException($"Mã gói dịch vụ '{package.PackageNo}' đã tồn tại trong hệ thống.");

        package.CreatedAt = DateTime.Now;

        foreach (var item in items)
        {
            if (item.Type == LineType.Part && item.PartId.HasValue && item.PartId.Value > 0)
            {
                var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId.Value);
                if (part != null)
                {
                    item.Code = string.IsNullOrWhiteSpace(item.Code) ? part.Code : item.Code;
                    item.Name = string.IsNullOrWhiteSpace(item.Name) ? part.Name : item.Name;
                    item.Unit = string.IsNullOrWhiteSpace(item.Unit) ? part.Unit : item.Unit;
                    if (item.UnitPrice <= 0) item.UnitPrice = part.SalePrice > 0 ? part.SalePrice : part.CostPrice;
                }
            }
            package.Items.Add(item);
        }

        db.ServicePackages.Add(package);
        await db.SaveChangesAsync();
        return package.Id;
    }

    public async Task<(bool ok, string msg)> DeleteServicePackageAsync(int id)
    {
        var package = await db.ServicePackages.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id);
        if (package == null) return (false, "Không tìm thấy gói dịch vụ.");

        db.ServicePackageItems.RemoveRange(package.Items);
        db.ServicePackages.Remove(package);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa gói dịch vụ '{package.PackageNo} - {package.Name}'.");
    }

    public Task<List<ServicePackage>> ServicePackagesForSelectAsync() =>
        db.ServicePackages
            .Include(p => p.Items)
            .Where(p => p.IsActive)
            .OrderBy(p => p.PackageNo)
            .ToListAsync();

    public async Task<(bool ok, string msg, int itemsAdded)> ApplyServicePackageToROAsync(int packageId, int roId)
    {
        var package = await db.ServicePackages
            .Include(p => p.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(p => p.Id == packageId);
        if (package == null) return (false, "Không tìm thấy gói dịch vụ.", 0);

        var ro = await db.ROs.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy Lệnh sửa chữa (RO).", 0);

        if (ro.Status is ROStatus.Paid or ROStatus.Finished or ROStatus.Rejected or ROStatus.NotResponding)
            return (false, $"Không thể thêm hạng mục vào RO đang ở trạng thái '{Ui.Status(ro.Status).text}'.", 0);

        if (package.Items.Count == 0)
            return (false, "Gói dịch vụ này chưa có hạng mục nào để áp dụng.", 0);

        int count = 0;
        foreach (var item in package.Items)
        {
            var lineName = !string.IsNullOrWhiteSpace(item.Name) ? item.Name : (item.Part != null ? item.Part.Name : "Hạng mục bảo dưỡng");
            ro.Lines.Add(new RepairLine
            {
                ROId = roId,
                Type = item.Type,
                ExpenseType = item.ExpenseType,
                PartId = item.PartId,
                Name = $"[{package.PackageNo}] {lineName}",
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
            count++;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã áp dụng gói '{package.PackageNo} - {package.Name}' ({count} hạng mục) vào Lệnh sửa chữa {ro.Code}.", count);
    }

    public async Task<(bool ok, string msg, int itemsAdded)> ApplyServicePackageToQuoteAsync(int packageId, int quoteId)
    {
        var package = await db.ServicePackages
            .Include(p => p.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(p => p.Id == packageId);
        if (package == null) return (false, "Không tìm thấy gói dịch vụ.", 0);

        var quote = await db.Quotes.Include(q => q.Items).FirstOrDefaultAsync(q => q.Id == quoteId);
        if (quote == null) return (false, "Không tìm thấy Báo giá.", 0);

        if (quote.Status is QuoteStatus.Converted or QuoteStatus.Rejected)
            return (false, $"Không thể thêm hạng mục vào Báo giá đang ở trạng thái '{Ui.QuoteStatus(quote.Status).text}'.", 0);

        if (package.Items.Count == 0)
            return (false, "Gói dịch vụ này chưa có hạng mục nào để nạp.", 0);

        int count = 0;
        foreach (var item in package.Items)
        {
            var partCode = !string.IsNullOrWhiteSpace(item.Code) ? item.Code : (item.Part != null ? item.Part.Code : (item.Type == LineType.Labor ? "CONG-BD" : "PT-BD"));
            var partName = !string.IsNullOrWhiteSpace(item.Name) ? item.Name : (item.Part != null ? item.Part.Name : "Hạng mục bảo dưỡng");
            var unit = !string.IsNullOrWhiteSpace(item.Unit) ? item.Unit : (item.Part != null ? item.Part.Unit : (item.Type == LineType.Labor ? "Lần" : "Cái"));

            quote.Items.Add(new QuoteItem
            {
                QuoteId = quoteId,
                PartId = item.PartId,
                PartCode = partCode,
                PartName = $"[{package.PackageNo}] {partName}",
                Unit = unit,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountPercent = 0,
                VatPercent = item.VatPercent,
                Note = string.IsNullOrWhiteSpace(item.Note) ? $"Combo {package.PackageNo}" : $"[{package.PackageNo}] {item.Note}"
            });
            count++;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã nạp gói '{package.PackageNo} - {package.Name}' ({count} hạng mục) vào Báo giá {quote.QuoteNo}.", count);
    }
}
