using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models;
using MiniService.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://0.0.0.0:{Environment.GetEnvironmentVariable("PORT") ?? "8080"}");

var conn = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=miniservice.db";
builder.Services.AddDbContext<AppDbContext>(o =>
{
    if (DbUtil.IsPostgres(conn)) o.UseNpgsql(DbUtil.ToNpgsql(conn));
    else o.UseSqlite(conn);
});
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IRoService, RoService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
    await Seeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());

app.Use(async (ctx, next) =>
{
    var key = ctx.Request.Headers["X-Api-Key"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(key)) ctx.Request.Cookies.TryGetValue(TenantContext.CookieName, out key);
    if (!string.IsNullOrWhiteSpace(key))
    {
        using var lookup = app.Services.CreateScope();
        var ldb = lookup.ServiceProvider.GetRequiredService<AppDbContext>();
        var org = await ldb.Orgs.FirstOrDefaultAsync(o => o.ApiKey == key);
        if (org != null) ctx.RequestServices.GetRequiredService<ITenantContext>().OrgId = org.Id;
    }
    await next();
});

app.UseStaticFiles();
app.MapGet("/healthz", () => "ok");

// API tra cứu trạng thái RO (KH/đại lý tra qua biển số)
app.MapGet("/api/ro", async (string? plate, IRoService svc) =>
{
    var ros = await svc.ROsAsync(null, plate);
    return Results.Ok(ros.Select(r => new
    {
        r.Code, plate = r.Car.Plate, model = r.Car.Model, status = Ui.Status(r.Status).text,
        statusCode = Ui.Status(r.Status).code, total = r.Total, technician = r.Technician, createdAt = r.CreatedAt
    }));
});

// API cập nhật Nhắc bảo dưỡng định kỳ trên RO (Ser_RO_Update_Maintance_DL)
app.MapPost("/api/ro/{id:int}/maintenance-reminder", async (int id, UpdateMaintenanceReminderDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.UpdateMaintenanceReminderAsync(id, dto.ReminderDate, dto.ReminderKm, dto.WorkDoneSoon, dto.MemberNo, dto.UpdatedBy ?? "api");
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Nhật ký thao tác Lệnh sửa chữa (Ser_ROHistory)
app.MapGet("/api/ro-histories", async (int? roId, ROStatus? status, string? q, IRoService svc) =>
{
    var list = await svc.RoHistoriesAsync(roId, status, q);
    return Results.Ok(list.Select(h => new
    {
        h.Id,
        h.ROId,
        roCode = h.RO?.Code,
        plate = h.RO?.Car?.Plate,
        status = Ui.Status(h.Status).text,
        statusCode = Ui.Status(h.Status).code,
        statusValue = (int)h.Status,
        h.HistoryDate,
        h.UserCode,
        h.Note
    }));
});

app.MapGet("/api/ro-histories/summary", async (int? roId, IRoService svc) =>
{
    var s = await svc.GetRoHistorySummaryAsync(roId);
    return Results.Ok(new
    {
        s.TotalEntries,
        s.RejectCount,
        s.DistinctStatusCount,
        s.FirstEntryAt,
        s.LastEntryAt
    });
});

app.MapGet("/api/ro-histories/{id:int}", async (int id, IRoService svc) =>
{
    var h = await svc.GetRoHistoryAsync(id);
    if (h == null) return Results.NotFound(new { error = "Không tìm thấy dòng nhật ký." });
    return Results.Ok(new
    {
        h.Id,
        h.ROId,
        ro = h.RO != null ? new { h.RO.Id, h.RO.Code, h.RO.Status, plate = h.RO.Car?.Plate } : null,
        status = Ui.Status(h.Status).text,
        statusCode = Ui.Status(h.Status).code,
        statusValue = (int)h.Status,
        h.HistoryDate,
        h.UserCode,
        h.Note
    });
});

app.MapPost("/api/ro-histories", async (CreateRoHistoryDto dto, IRoService svc) =>
{
    try
    {
        if (dto.RoId <= 0) return Results.BadRequest(new { error = "Cần chọn lệnh sửa chữa (RoId)." });
        var id = await svc.AddRoHistoryAsync(dto.RoId, dto.Status, dto.Note, dto.UserCode ?? "api");
        return Results.Ok(new { roHistoryId = id, message = "Đã ghi nhật ký thao tác RO." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapDelete("/api/ro-histories/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteRoHistoryAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API danh mục phụ tùng & tồn kho
app.MapGet("/api/parts", async (string? q, bool? lowStock, IRoService svc) =>
{
    var parts = await svc.PartsAsync(q, lowStock);
    return Results.Ok(parts.Select(p => new
    {
        p.Id, p.Code, p.Name, p.Unit, p.CostPrice, p.SalePrice,
        p.InStock, p.MinStock, p.IsLowStock, p.Location, p.Model
    }));
});

app.MapPost("/api/parts/{id:int}/stock", async (int id, AdjustStockDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.AdjustStockAsync(id, dto.Quantity, dto.Mode ?? "add", dto.Note);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Báo cáo bảo hành & Bồi hoàn hãng (Ser_ROWarrantyReport)
app.MapGet("/api/warranty", async (WarrantyStatus? status, string? q, IRoService svc) =>
{
    var reports = await svc.WarrantyReportsAsync(status, q);
    return Results.Ok(reports.Select(w => new
    {
        w.Id,
        w.ReportNo,
        roCode = w.RO.Code,
        plate = w.Car.Plate,
        model = w.Car.Model,
        customer = w.Customer.Name,
        phone = w.Customer.Phone,
        odometer = w.Odometer,
        status = Ui.WarrantyStatus(w.Status).text,
        statusCode = Ui.WarrantyStatus(w.Status).code,
        statusValue = (int)w.Status,
        w.IssueDescription,
        w.DiagnosticResult,
        w.ErrorCodeCD,
        w.ErrorCodePN,
        w.ClaimAmount,
        w.ApprovedAmount,
        w.DecisionNote,
        w.RejectionReason,
        itemCount = w.Items.Count,
        w.CreatedAt,
        w.SubmittedAt,
        w.DecidedAt
    }));
});

app.MapGet("/api/warranty/{id:int}", async (int id, IRoService svc) =>
{
    var w = await svc.GetWarrantyReportAsync(id);
    if (w == null) return Results.NotFound(new { error = "Không tìm thấy Báo cáo bảo hành." });
    return Results.Ok(new
    {
        w.Id,
        w.ReportNo,
        ro = new { w.RO.Id, w.RO.Code, w.RO.Status, roTotal = w.RO.Total },
        car = new { w.Car.Id, w.Car.Plate, w.Car.Model, w.Car.Vin, w.Car.Year },
        customer = new { w.Customer.Id, w.Customer.Name, w.Customer.Phone, w.Customer.Email },
        odometer = w.Odometer,
        status = Ui.WarrantyStatus(w.Status).text,
        statusCode = Ui.WarrantyStatus(w.Status).code,
        statusValue = (int)w.Status,
        w.IssueDescription,
        w.DiagnosticResult,
        w.ErrorCodeCD,
        w.ErrorCodePN,
        partError = w.PartError != null ? new { w.PartError.Id, w.PartError.Code, w.PartError.Name } : null,
        w.ClaimAmount,
        w.ApprovedAmount,
        w.DecisionNote,
        w.RejectionReason,
        items = w.Items.Select(i => new
        {
            i.Id,
            type = Ui.Line(i.Type),
            i.Code,
            i.Name,
            i.Quantity,
            i.UnitPrice,
            i.Amount,
            i.IsAccepted,
            i.Note
        }),
        w.CreatedAt,
        w.SubmittedAt,
        w.DecidedAt
    });
});

app.MapPost("/api/warranty/from-ro", async (CreateWarrantyDto dto, IRoService svc) =>
{
    try
    {
        var id = await svc.CreateWarrantyReportFromROAsync(
            dto.RoId, dto.IssueDesc, dto.DiagResult, dto.ErrorCodeCD, dto.ErrorCodePN, dto.PartIdError, dto.CreatedBy ?? "api");
        return Results.Ok(new { warrantyReportId = id, message = "Đã lập Báo cáo bảo hành xe." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/warranty/{id:int}/transition", async (int id, TransitionWarrantyDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.TransitionWarrantyAsync(id, dto.ToStatus, dto.ApprovedAmount, dto.Note);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Đặt lịch hẹn dịch vụ (Ser_App / Ser_Appointment)
app.MapGet("/api/appointments", async (AppointmentStatus? status, string? q, DateTime? date, IRoService svc) =>
{
    var list = await svc.AppointmentsAsync(status, q, date);
    return Results.Ok(list.Select(a => new
    {
        a.Id,
        a.AppNo,
        plate = a.Car.Plate,
        model = a.Car.Model,
        customer = a.Customer.Name,
        phone = a.Customer.Phone,
        appointmentDate = a.AppointmentDate,
        serviceType = Ui.AppServiceType(a.ServiceType),
        serviceTypeValue = (int)a.ServiceType,
        status = Ui.AppointmentStatus(a.Status).text,
        statusCode = Ui.AppointmentStatus(a.Status).code,
        statusValue = (int)a.Status,
        a.Advisor,
        a.Cavity,
        a.CustomerRequest,
        a.Note,
        a.Source,
        roId = a.ROId,
        roCode = a.RO?.Code,
        a.CreatedAt,
        a.ConfirmedAt,
        a.CheckedInAt
    }));
});

app.MapGet("/api/appointments/{id:int}", async (int id, IRoService svc) =>
{
    var a = await svc.GetAppointmentAsync(id);
    if (a == null) return Results.NotFound(new { error = "Không tìm thấy lịch hẹn dịch vụ." });
    return Results.Ok(new
    {
        a.Id,
        a.AppNo,
        car = new { a.Car.Id, a.Car.Plate, a.Car.Model, a.Car.Vin, a.Car.Year },
        customer = new { a.Customer.Id, a.Customer.Name, a.Customer.Phone, a.Customer.Email },
        appointmentDate = a.AppointmentDate,
        serviceType = Ui.AppServiceType(a.ServiceType),
        serviceTypeValue = (int)a.ServiceType,
        status = Ui.AppointmentStatus(a.Status).text,
        statusCode = Ui.AppointmentStatus(a.Status).code,
        statusValue = (int)a.Status,
        a.Advisor,
        a.Cavity,
        a.CustomerRequest,
        a.Note,
        a.CancelReason,
        a.Source,
        ro = a.RO != null ? new { a.RO.Id, a.RO.Code, a.RO.Status } : null,
        a.CreatedAt,
        a.ConfirmedAt,
        a.CheckedInAt
    });
});

app.MapPost("/api/appointments", async (CreateAppointmentDto dto, IRoService svc) =>
{
    try
    {
        var appItem = new Appointment
        {
            CarId = dto.CarId,
            AppointmentDate = dto.AppointmentDate != default ? dto.AppointmentDate : DateTime.Today.AddHours(9),
            ServiceType = dto.ServiceType,
            Advisor = dto.Advisor,
            Cavity = dto.Cavity,
            CustomerRequest = dto.CustomerRequest ?? "",
            Note = dto.Note,
            Source = dto.Source ?? "Hotline"
        };
        var id = await svc.CreateAppointmentAsync(appItem);
        return Results.Ok(new { appointmentId = id, appNo = appItem.AppNo, message = "Đã đặt lịch hẹn thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/appointments/{id:int}/status", async (int id, TransitionAppointmentDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.TransitionAppointmentStatusAsync(id, dto.ToStatus, dto.Reason);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/appointments/{id:int}/checkin", async (int id, CheckInAppointmentDto dto, IRoService svc) =>
{
    var (ok, msg, roId) = await svc.CheckInAppointmentAsync(id, dto.Odometer, dto.Technician);
    return ok ? Results.Ok(new { message = msg, roId }) : Results.BadRequest(new { error = msg });
});

// API Quản lý Nhập kho phụ tùng (Ser_Inv_StockIn & Ser_Inv_StockInDetail)
app.MapGet("/api/stockin", async (StockInStatus? status, string? q, DateTime? fromDate, DateTime? toDate, IRoService svc) =>
{
    var list = await svc.StockInsAsync(status, q, fromDate, toDate);
    return Results.Ok(list.Select(s => new
    {
        s.Id,
        s.StockInNo,
        s.StockInDate,
        s.SupplierName,
        s.BillNo,
        type = Ui.StockInType(s.Type),
        typeValue = (int)s.Type,
        status = Ui.StockInStatus(s.Status).text,
        statusCode = Ui.StockInStatus(s.Status).code,
        statusValue = (int)s.Status,
        s.Description,
        s.CreatedBy,
        s.ApprovedBy,
        s.CreatedAt,
        s.FinishedAt,
        s.ItemCount,
        s.SubTotal,
        s.TotalVat,
        s.Total
    }));
});

app.MapGet("/api/stockin/{id:int}", async (int id, IRoService svc) =>
{
    var s = await svc.GetStockInAsync(id);
    if (s == null) return Results.NotFound(new { error = "Không tìm thấy phiếu nhập kho." });
    return Results.Ok(new
    {
        s.Id,
        s.StockInNo,
        s.StockInDate,
        s.SupplierName,
        s.BillNo,
        type = Ui.StockInType(s.Type),
        typeValue = (int)s.Type,
        status = Ui.StockInStatus(s.Status).text,
        statusCode = Ui.StockInStatus(s.Status).code,
        statusValue = (int)s.Status,
        s.Description,
        s.CreatedBy,
        s.ApprovedBy,
        s.CreatedAt,
        s.FinishedAt,
        s.SubTotal,
        s.TotalVat,
        s.Total,
        items = s.Items.Select(i => new
        {
            i.Id,
            i.PartId,
            i.PartCode,
            i.PartName,
            i.Unit,
            i.Quantity,
            i.UnitPrice,
            i.VatPercent,
            i.SubTotal,
            i.VatAmount,
            i.Amount,
            i.Location,
            i.Note,
            currentInStock = i.Part?.InStock
        })
    });
});

app.MapPost("/api/stockin", async (CreateStockInDto dto, IRoService svc) =>
{
    try
    {
        if (dto.Items == null || dto.Items.Count == 0)
            return Results.BadRequest(new { error = "Cần danh sách phụ tùng nhập kho (Items)." });

        var stockIn = new StockIn
        {
            SupplierName = dto.SupplierName?.Trim() ?? "",
            BillNo = dto.BillNo?.Trim(),
            StockInDate = dto.StockInDate ?? DateTime.Today,
            Type = dto.Type,
            Description = dto.Description?.Trim(),
            CreatedBy = "api"
        };

        var details = dto.Items.Select(i => new StockInDetail
        {
            PartId = i.PartId,
            Quantity = i.Quantity <= 0 ? 1 : i.Quantity,
            UnitPrice = i.UnitPrice ?? 0,
            VatPercent = i.VatPercent ?? 8,
            Note = i.Note?.Trim()
        }).ToList();

        var id = await svc.CreateStockInAsync(stockIn, details);
        return Results.Ok(new { stockInId = id, stockInNo = stockIn.StockInNo, message = "Đã lập phiếu nhập kho thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/stockin/{id:int}/transition", async (int id, TransitionStockInDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.TransitionStockInStatusAsync(id, dto.ToStatus, dto.ApprovedBy, dto.Note);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/stockin/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteStockInAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Quản lý Xuất kho phụ tùng (Ser_Inv_StockOut & Ser_Inv_StockOutDetail)
app.MapGet("/api/stockout", async (StockOutStatus? status, string? q, DateTime? fromDate, DateTime? toDate, int? roId, IRoService svc) =>
{
    var list = await svc.StockOutsAsync(status, q, fromDate, toDate, roId);
    return Results.Ok(list.Select(s => new
    {
        s.Id,
        s.StockOutNo,
        s.StockOutDate,
        type = Ui.StockOutType(s.Type),
        typeValue = (int)s.Type,
        status = Ui.StockOutStatus(s.Status).text,
        statusCode = Ui.StockOutStatus(s.Status).code,
        statusValue = (int)s.Status,
        roId = s.ROId,
        roCode = s.RO?.Code,
        plate = s.Car?.Plate,
        customer = s.Customer?.Name,
        s.RecipientName,
        s.Description,
        s.CreatedBy,
        s.ApprovedBy,
        s.CreatedAt,
        s.FinishedAt,
        s.ItemCount,
        s.SubTotal,
        s.TotalVat,
        s.Total
    }));
});

app.MapGet("/api/stockout/{id:int}", async (int id, IRoService svc) =>
{
    var s = await svc.GetStockOutAsync(id);
    if (s == null) return Results.NotFound(new { error = "Không tìm thấy phiếu xuất kho." });
    return Results.Ok(new
    {
        s.Id,
        s.StockOutNo,
        s.StockOutDate,
        type = Ui.StockOutType(s.Type),
        typeValue = (int)s.Type,
        status = Ui.StockOutStatus(s.Status).text,
        statusCode = Ui.StockOutStatus(s.Status).code,
        statusValue = (int)s.Status,
        ro = s.RO != null ? new { s.RO.Id, s.RO.Code, s.RO.Status } : null,
        car = s.Car != null ? new { s.Car.Id, s.Car.Plate, s.Car.Model } : null,
        customer = s.Customer != null ? new { s.Customer.Id, s.Customer.Name, s.Customer.Phone } : null,
        s.RecipientName,
        s.Description,
        s.CreatedBy,
        s.ApprovedBy,
        s.CreatedAt,
        s.FinishedAt,
        s.SubTotal,
        s.TotalVat,
        s.Total,
        items = s.Items.Select(i => new
        {
            i.Id,
            i.PartId,
            i.PartCode,
            i.PartName,
            i.Unit,
            i.Quantity,
            i.UnitPrice,
            i.VatPercent,
            i.SubTotal,
            i.VatAmount,
            i.Amount,
            i.Location,
            i.Note,
            currentInStock = i.Part?.InStock
        })
    });
});

app.MapPost("/api/stockout", async (CreateStockOutDto dto, IRoService svc) =>
{
    try
    {
        if (dto.Items == null || dto.Items.Count == 0)
            return Results.BadRequest(new { error = "Cần danh sách phụ tùng xuất kho (Items)." });

        var stockOut = new StockOut
        {
            Type = dto.Type,
            ROId = (dto.RoId.HasValue && dto.RoId.Value > 0) ? dto.RoId : null,
            RecipientName = dto.RecipientName?.Trim(),
            StockOutDate = dto.StockOutDate ?? DateTime.Today,
            Description = dto.Description?.Trim(),
            CreatedBy = "api"
        };

        var details = dto.Items.Select(i => new StockOutDetail
        {
            PartId = i.PartId,
            Quantity = i.Quantity <= 0 ? 1 : i.Quantity,
            UnitPrice = i.UnitPrice ?? 0,
            VatPercent = i.VatPercent ?? 8,
            Note = i.Note?.Trim()
        }).ToList();

        var id = await svc.CreateStockOutAsync(stockOut, details);
        return Results.Ok(new { stockOutId = id, stockOutNo = stockOut.StockOutNo, message = "Đã lập phiếu xuất kho thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/stockout/{id:int}/transition", async (int id, TransitionStockOutDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.TransitionStockOutStatusAsync(id, dto.ToStatus, dto.ApprovedBy, dto.Note);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/stockout/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteStockOutAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Chăm sóc Khách hàng 24h sau dịch vụ (Ser_CustomerCare24h)
app.MapGet("/api/customercare", async (CustomerCareStatus? status, string? q, IRoService svc) =>
{
    var list = await svc.CustomerCaresAsync(status, q);
    return Results.Ok(list.Select(c => new
    {
        c.Id,
        c.CareNo,
        roId = c.ROId,
        roCode = c.RO?.Code,
        plate = c.Car?.Plate,
        model = c.Car?.Model,
        customer = c.Customer?.Name,
        phone = c.Customer?.Phone,
        status = Ui.CustomerCareStatus(c.Status).text,
        statusCode = Ui.CustomerCareStatus(c.Status).code,
        statusValue = (int)c.Status,
        c.HasCarProblem,
        c.QualityRating,
        qualityRatingText = Ui.RatingText(c.QualityRating),
        c.StaffRating,
        staffRatingText = Ui.RatingText(c.StaffRating),
        c.WillingToReturn,
        c.FacilityRating,
        facilityRatingText = Ui.FacilityText(c.FacilityRating),
        c.CustomerFeedback,
        c.InternalNote,
        c.ContactedBy,
        c.ContactedDate,
        c.CreatedAt
    }));
});

app.MapGet("/api/customercare/{id:int}", async (int id, IRoService svc) =>
{
    var c = await svc.GetCustomerCareAsync(id);
    if (c == null) return Results.NotFound(new { error = "Không tìm thấy phiếu CSKH." });
    return Results.Ok(new
    {
        c.Id,
        c.CareNo,
        status = Ui.CustomerCareStatus(c.Status).text,
        statusCode = Ui.CustomerCareStatus(c.Status).code,
        statusValue = (int)c.Status,
        ro = new
        {
            c.RO.Id,
            c.RO.Code,
            status = Ui.Status(c.RO.Status).text,
            c.RO.Total,
            c.RO.FinishedAt,
            lines = c.RO.Lines.Select(l => new { l.Id, l.Name, l.Quantity, l.UnitPrice, l.Amount, type = Ui.Line(l.Type) })
        },
        car = new { c.Car.Id, c.Car.Plate, c.Car.Model, c.Car.Vin, c.Car.Year },
        customer = new { c.Customer.Id, c.Customer.Name, c.Customer.Phone, c.Customer.Email },
        survey = new
        {
            c.HasCarProblem,
            c.QualityRating,
            qualityRatingText = Ui.RatingText(c.QualityRating),
            c.StaffRating,
            staffRatingText = Ui.RatingText(c.StaffRating),
            c.WillingToReturn,
            c.FacilityRating,
            facilityRatingText = Ui.FacilityText(c.FacilityRating),
            c.CustomerFeedback,
            c.InternalNote
        },
        c.ContactedBy,
        c.ContactedDate,
        c.CreatedBy,
        c.CreatedAt
    });
});

app.MapPost("/api/customercare", async (CreateCustomerCareDto dto, IRoService svc) =>
{
    try
    {
        var care = new CustomerCare
        {
            ROId = dto.RoId,
            InternalNote = dto.InternalNote?.Trim(),
            CreatedBy = dto.CreatedBy ?? "api"
        };
        var id = await svc.CreateCustomerCareAsync(care);
        return Results.Ok(new { customerCareId = id, careNo = care.CareNo, message = "Đã lập phiếu CSKH thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/customercare/{id:int}/survey", async (int id, SubmitCareSurveyDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.UpdateCustomerCareSurveyAsync(
        id,
        dto.Status,
        dto.HasCarProblem,
        dto.QualityRating,
        dto.StaffRating,
        dto.WillingToReturn,
        dto.FacilityRating,
        dto.CustomerFeedback,
        dto.InternalNote,
        dto.ContactedBy);

    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/customercare/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteCustomerCareAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Quản lý Thu ngân & Phiếu thu thanh toán (Ser_Payment & Ser_PaymentDetail)
app.MapGet("/api/payments", async (PaymentStatus? status, string? q, DateTime? fromDate, DateTime? toDate, int? roId, IRoService svc) =>
{
    var list = await svc.PaymentsAsync(status, q, fromDate, toDate, roId);
    return Results.Ok(list.Select(p => new
    {
        p.Id,
        p.PaymentNo,
        p.PaymentDate,
        method = Ui.PaymentMethod(p.Method).text,
        methodValue = (int)p.Method,
        status = Ui.PaymentStatus(p.Status).text,
        statusCode = Ui.PaymentStatus(p.Status).code,
        statusValue = (int)p.Status,
        roId = p.ROId,
        roCode = p.RO?.Code,
        plate = p.Car?.Plate,
        customer = p.Customer?.Name,
        p.PayPersonName,
        p.PayPersonPhone,
        p.RoTotalAmount,
        p.DiscountAmount,
        p.ThirdPartyAmount,
        p.PayableAmount,
        p.PaymentAmount,
        p.TransactionRef,
        p.Cashier,
        p.Note,
        p.CreatedAt,
        p.CompletedAt
    }));
});

app.MapGet("/api/payments/{id:int}", async (int id, IRoService svc) =>
{
    var p = await svc.GetPaymentAsync(id);
    if (p == null) return Results.NotFound(new { error = "Không tìm thấy phiếu thu." });
    return Results.Ok(new
    {
        p.Id,
        p.PaymentNo,
        p.PaymentDate,
        method = Ui.PaymentMethod(p.Method).text,
        methodValue = (int)p.Method,
        status = Ui.PaymentStatus(p.Status).text,
        statusCode = Ui.PaymentStatus(p.Status).code,
        statusValue = (int)p.Status,
        ro = new { p.RO.Id, p.RO.Code, p.RO.Status, roTotal = p.RO.Total, lines = p.RO.Lines.Select(l => new { l.Id, l.Name, l.Quantity, l.UnitPrice, l.Amount, type = Ui.Line(l.Type) }) },
        car = new { p.Car.Id, p.Car.Plate, p.Car.Model, p.Car.Vin, p.Car.Year },
        customer = new { p.Customer.Id, p.Customer.Name, p.Customer.Phone, p.Customer.Email },
        p.PayPersonName,
        p.PayPersonPhone,
        p.PayPersonIdCard,
        p.RoTotalAmount,
        p.DiscountAmount,
        p.ThirdPartyAmount,
        p.PayableAmount,
        p.PaymentAmount,
        amountInWords = Ui.MoneyToWords(p.PaymentAmount),
        p.TransactionRef,
        p.Cashier,
        p.Note,
        p.CreatedAt,
        p.CompletedAt
    });
});

app.MapPost("/api/payments", async (CreatePaymentDto dto, IRoService svc) =>
{
    try
    {
        var payment = new Payment
        {
            ROId = dto.RoId,
            PaymentDate = dto.PaymentDate ?? DateTime.Today,
            Method = dto.Method,
            Status = dto.Status ?? PaymentStatus.Completed,
            PayPersonName = dto.PayPersonName?.Trim() ?? "",
            PayPersonPhone = dto.PayPersonPhone?.Trim(),
            PayPersonIdCard = dto.PayPersonIdCard?.Trim(),
            DiscountAmount = dto.DiscountAmount ?? 0,
            PaymentAmount = dto.PaymentAmount ?? 0,
            TransactionRef = dto.TransactionRef?.Trim(),
            Note = dto.Note?.Trim(),
            Cashier = dto.Cashier ?? "Thu ngân",
            CreatedBy = "api"
        };
        var id = await svc.CreatePaymentAsync(payment);
        return Results.Ok(new { paymentId = id, paymentNo = payment.PaymentNo, paymentAmount = payment.PaymentAmount, message = "Đã lập phiếu thu thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/payments/{id:int}/status", async (int id, TransitionPaymentDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.TransitionPaymentStatusAsync(id, dto.ToStatus, dto.Cashier, dto.Note);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/payments/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeletePaymentAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Quản lý Báo giá phụ tùng & dịch vụ (Ser_Inv_Quote & Ser_Inv_QuotePartItems)
app.MapGet("/api/quotes", async (QuoteStatus? status, string? q, DateTime? fromDate, DateTime? toDate, IRoService svc) =>
{
    var list = await svc.QuotesAsync(status, q, fromDate, toDate);
    return Results.Ok(list.Select(quote => new
    {
        quote.Id,
        quote.QuoteNo,
        quote.QuoteDate,
        quote.ValidUntil,
        customerId = quote.CustomerId,
        customerName = quote.CustomerName,
        customerPhone = quote.CustomerPhone,
        plate = quote.Car?.Plate,
        model = quote.Car?.Model,
        quote.RecipientName,
        paymentMethod = Ui.PaymentMethod(quote.PaymentMethod).text,
        paymentMethodValue = (int)quote.PaymentMethod,
        status = Ui.QuoteStatus(quote.Status).text,
        statusCode = Ui.QuoteStatus(quote.Status).code,
        statusValue = (int)quote.Status,
        quote.ItemCount,
        quote.SubTotal,
        quote.TotalDiscount,
        quote.TotalVat,
        quote.Total,
        stockOutId = quote.StockOutId,
        stockOutNo = quote.StockOut?.StockOutNo,
        roId = quote.ROId,
        roCode = quote.RO?.Code,
        quote.CreatedBy,
        quote.CreatedAt,
        quote.ConfirmedAt
    }));
});

app.MapGet("/api/quotes/{id:int}", async (int id, IRoService svc) =>
{
    var quote = await svc.GetQuoteAsync(id);
    if (quote == null) return Results.NotFound(new { error = "Không tìm thấy báo giá." });
    return Results.Ok(new
    {
        quote.Id,
        quote.QuoteNo,
        quote.QuoteDate,
        quote.ValidUntil,
        customer = quote.Customer != null ? new { quote.Customer.Id, quote.Customer.Code, quote.Customer.Name, quote.Customer.Phone, quote.Customer.Email } : null,
        quote.CustomerName,
        quote.CustomerPhone,
        quote.CustomerAddress,
        car = quote.Car != null ? new { quote.Car.Id, quote.Car.Plate, quote.Car.Model, quote.Car.Vin, quote.Car.Year } : null,
        quote.RecipientName,
        paymentMethod = Ui.PaymentMethod(quote.PaymentMethod).text,
        paymentMethodValue = (int)quote.PaymentMethod,
        status = Ui.QuoteStatus(quote.Status).text,
        statusCode = Ui.QuoteStatus(quote.Status).code,
        statusValue = (int)quote.Status,
        quote.Remark,
        quote.Note,
        quote.ItemCount,
        quote.SubTotal,
        quote.TotalDiscount,
        quote.TotalVat,
        quote.Total,
        stockOut = quote.StockOut != null ? new { quote.StockOut.Id, quote.StockOut.StockOutNo, quote.StockOut.Status } : null,
        ro = quote.RO != null ? new { quote.RO.Id, quote.RO.Code, quote.RO.Status } : null,
        quote.CreatedBy,
        quote.CreatedAt,
        quote.ConfirmedAt,
        items = quote.Items.Select(i => new
        {
            i.Id,
            i.PartId,
            i.PartCode,
            i.PartName,
            i.Unit,
            i.Quantity,
            i.UnitPrice,
            i.DiscountPercent,
            i.DiscountAmount,
            i.TaxableAmount,
            i.VatPercent,
            i.VatAmount,
            i.Amount,
            i.Note,
            inStock = i.Part?.InStock
        })
    });
});

app.MapPost("/api/quotes", async (CreateQuoteDto dto, IRoService svc) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(dto.CustomerName))
            return Results.BadRequest(new { error = "Vui lòng nhập tên khách hàng (CustomerName)." });

        if (dto.Items == null || dto.Items.Count == 0)
            return Results.BadRequest(new { error = "Cần danh sách phụ tùng báo giá (Items)." });

        var quote = new Quote
        {
            CustomerId = (dto.CustomerId.HasValue && dto.CustomerId.Value > 0) ? dto.CustomerId : null,
            CustomerName = dto.CustomerName.Trim(),
            CustomerPhone = dto.CustomerPhone?.Trim(),
            CustomerAddress = dto.CustomerAddress?.Trim(),
            CarId = (dto.CarId.HasValue && dto.CarId.Value > 0) ? dto.CarId : null,
            RecipientName = dto.RecipientName?.Trim(),
            PaymentMethod = dto.PaymentMethod,
            QuoteDate = dto.QuoteDate ?? DateTime.Today,
            ValidUntil = dto.ValidUntil ?? DateTime.Today.AddDays(15),
            Remark = dto.Remark?.Trim(),
            Note = dto.Note?.Trim(),
            CreatedBy = "api"
        };

        var items = dto.Items.Select(i => new QuoteItem
        {
            PartId = (i.PartId.HasValue && i.PartId.Value > 0) ? i.PartId : null,
            PartCode = i.PartCode ?? "PRT",
            PartName = i.PartName ?? "Phụ tùng",
            Unit = i.Unit ?? "Cái",
            Quantity = i.Quantity <= 0 ? 1 : i.Quantity,
            UnitPrice = i.UnitPrice ?? 0,
            DiscountPercent = i.DiscountPercent ?? 0,
            VatPercent = i.VatPercent ?? 8,
            Note = i.Note?.Trim()
        }).ToList();

        var id = await svc.CreateQuoteAsync(quote, items);
        return Results.Ok(new { quoteId = id, quoteNo = quote.QuoteNo, total = quote.Total, message = "Đã lập Báo giá thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/quotes/{id:int}/status", async (int id, TransitionQuoteDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.TransitionQuoteStatusAsync(id, dto.ToStatus);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/quotes/{id:int}/convert-stockout", async (int id, IRoService svc) =>
{
    var (ok, msg, stockOutId) = await svc.ConvertQuoteToStockOutAsync(id);
    return ok ? Results.Ok(new { message = msg, stockOutId }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/quotes/{id:int}/convert-ro", async (int id, ConvertQuoteRoDto dto, IRoService svc) =>
{
    var (ok, msg, roId) = await svc.ConvertQuoteToROAsync(id, dto.Technician);
    return ok ? Results.Ok(new { message = msg, roId }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/quotes/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteQuoteAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Quản lý Gói dịch vụ bảo dưỡng định kỳ (Ser_ServicePackage & Ser_ServicePackageServiceItems / PartItems)
app.MapGet("/api/servicepackages", async (string? q, bool? isPublic, bool? isActive, IRoService svc) =>
{
    var list = await svc.ServicePackagesAsync(q, isPublic, isActive);
    return Results.Ok(list.Select(p => new
    {
        p.Id,
        p.PackageNo,
        p.Name,
        p.TakingTimeHours,
        p.Description,
        p.IsPublic,
        scope = Ui.PackageScope(p.IsPublic).text,
        p.IsActive,
        status = Ui.PackageActive(p.IsActive).text,
        p.ItemCount,
        p.LaborCount,
        p.PartCount,
        p.LaborSubTotal,
        p.PartSubTotal,
        p.SubTotal,
        p.TotalVat,
        p.Total,
        p.CreatedBy,
        p.CreatedAt
    }));
});

app.MapGet("/api/servicepackages/{id:int}", async (int id, IRoService svc) =>
{
    var p = await svc.GetServicePackageAsync(id);
    if (p == null) return Results.NotFound(new { error = "Không tìm thấy gói dịch vụ." });
    return Results.Ok(new
    {
        p.Id,
        p.PackageNo,
        p.Name,
        p.TakingTimeHours,
        p.Description,
        p.IsPublic,
        scope = Ui.PackageScope(p.IsPublic).text,
        p.IsActive,
        status = Ui.PackageActive(p.IsActive).text,
        p.ItemCount,
        p.LaborCount,
        p.PartCount,
        p.LaborSubTotal,
        p.PartSubTotal,
        p.SubTotal,
        p.TotalVat,
        p.Total,
        p.CreatedBy,
        p.CreatedAt,
        items = p.Items.Select(i => new
        {
            i.Id,
            type = Ui.Line(i.Type),
            typeValue = (int)i.Type,
            i.PartId,
            i.Code,
            i.Name,
            i.Unit,
            i.Quantity,
            i.UnitPrice,
            i.VatPercent,
            i.SubTotal,
            i.VatAmount,
            i.Amount,
            expenseType = Ui.Expense(i.ExpenseType).text,
            expenseTypeValue = (int)i.ExpenseType,
            i.Note,
            inStock = i.Part?.InStock
        })
    });
});

app.MapPost("/api/servicepackages", async (CreateServicePackageDto dto, IRoService svc) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Results.BadRequest(new { error = "Vui lòng nhập tên gói dịch vụ (Name)." });

        var package = new ServicePackage
        {
            PackageNo = dto.PackageNo?.Trim() ?? "",
            Name = dto.Name.Trim(),
            TakingTimeHours = dto.TakingTimeHours ?? 1.0m,
            Description = dto.Description?.Trim(),
            IsPublic = dto.IsPublic ?? true,
            IsActive = dto.IsActive ?? true,
            CreatedBy = "api"
        };

        var items = (dto.Items ?? []).Select(i => new ServicePackageItem
        {
            Type = i.Type,
            PartId = (i.PartId.HasValue && i.PartId.Value > 0) ? i.PartId : null,
            Code = i.Code ?? "",
            Name = i.Name ?? "",
            Unit = i.Unit ?? (i.Type == LineType.Labor ? "Lần" : "Cái"),
            Quantity = i.Quantity <= 0 ? 1 : i.Quantity,
            UnitPrice = i.UnitPrice,
            VatPercent = i.VatPercent ?? 8,
            ExpenseType = i.ExpenseType ?? ExpenseType.Customer,
            Note = i.Note?.Trim()
        }).ToList();

        var id = await svc.CreateServicePackageAsync(package, items);
        return Results.Ok(new { packageId = id, packageNo = package.PackageNo, total = package.Total, message = "Đã tạo gói dịch vụ thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/servicepackages/{id:int}/apply-to-ro", async (int id, ApplyPackageRoDto dto, IRoService svc) =>
{
    var (ok, msg, itemsAdded) = await svc.ApplyServicePackageToROAsync(id, dto.RoId);
    return ok ? Results.Ok(new { message = msg, itemsAdded, roId = dto.RoId }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/servicepackages/{id:int}/apply-to-quote", async (int id, ApplyPackageQuoteDto dto, IRoService svc) =>
{
    var (ok, msg, itemsAdded) = await svc.ApplyServicePackageToQuoteAsync(id, dto.QuoteId);
    return ok ? Results.Ok(new { message = msg, itemsAdded, quoteId = dto.QuoteId }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/servicepackages/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteServicePackageAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Quản lý Đơn đặt hàng phụ tùng Nhà Cung Cấp (Ser_Order_Part & Ser_Order_PartDtl)
app.MapGet("/api/orderparts", async (OrderPartStatus? status, string? q, DateTime? fromDate, DateTime? toDate, IRoService svc) =>
{
    var list = await svc.OrderPartsAsync(status, q, fromDate, toDate);
    return Results.Ok(list.Select(o => new
    {
        o.Id,
        o.OrderPartNo,
        o.OrderDate,
        o.SupplierName,
        deliveryForm = Ui.OrderPartDeliveryForm(o.DeliveryForm).text,
        deliveryFormValue = (int)o.DeliveryForm,
        o.DeliveryLocation,
        o.EstimatedDeliverDate,
        o.VIN,
        roId = o.ROId,
        roCode = o.RO?.Code,
        plate = o.RO?.Car?.Plate,
        status = Ui.OrderPartStatus(o.Status).text,
        statusCode = Ui.OrderPartStatus(o.Status).code,
        statusValue = (int)o.Status,
        o.OrderSuppierNo,
        o.RequestSuppierDate,
        o.ResponseSuppierDate,
        o.Remark,
        stockInId = o.StockInId,
        stockInNo = o.StockIn?.StockInNo,
        o.ItemCount,
        o.TotalQuantityOrdered,
        o.TotalQuantityApproved,
        o.TotalQuantityReceived,
        o.SubTotalBeforeDiscount,
        o.TotalDiscount,
        o.TaxableAmount,
        o.TotalVat,
        o.Total,
        o.CreatedBy,
        o.CreatedAt,
        o.ApprovedAt,
        o.FinishedAt
    }));
});

app.MapGet("/api/orderparts/{id:int}", async (int id, IRoService svc) =>
{
    var o = await svc.GetOrderPartAsync(id);
    if (o == null) return Results.NotFound(new { error = "Không tìm thấy đơn đặt hàng phụ tùng." });
    return Results.Ok(new
    {
        o.Id,
        o.OrderPartNo,
        o.OrderDate,
        o.SupplierName,
        deliveryForm = Ui.OrderPartDeliveryForm(o.DeliveryForm).text,
        deliveryFormValue = (int)o.DeliveryForm,
        o.DeliveryLocation,
        o.EstimatedDeliverDate,
        o.VIN,
        ro = o.RO != null ? new { o.RO.Id, o.RO.Code, o.RO.Status, plate = o.RO.Car?.Plate, customer = o.RO.Customer?.Name } : null,
        status = Ui.OrderPartStatus(o.Status).text,
        statusCode = Ui.OrderPartStatus(o.Status).code,
        statusValue = (int)o.Status,
        o.OrderSuppierNo,
        o.RequestSuppierDate,
        o.ResponseSuppierDate,
        o.Remark,
        stockIn = o.StockIn != null ? new { o.StockIn.Id, o.StockIn.StockInNo, o.StockIn.Status } : null,
        o.ItemCount,
        o.TotalQuantityOrdered,
        o.TotalQuantityApproved,
        o.TotalQuantityReceived,
        o.SubTotalBeforeDiscount,
        o.TotalDiscount,
        o.TaxableAmount,
        o.TotalVat,
        o.Total,
        o.CreatedBy,
        o.CreatedAt,
        o.ApprovedAt,
        o.FinishedAt,
        lines = o.Lines.Select(l => new
        {
            l.Id,
            l.PartId,
            l.PartCode,
            l.PartName,
            l.Unit,
            l.Quantity,
            l.UnitPrice,
            l.DiscountRate,
            l.DiscountAmount,
            l.TaxableAmount,
            l.VatPercent,
            l.VatAmount,
            l.Amount,
            l.ApprovedQuantity,
            l.ReceivedQuantity,
            statusDtl = Ui.OrderPartStatus(l.StatusDtl).text,
            l.Note,
            inStock = l.Part?.InStock
        })
    });
});

app.MapPost("/api/orderparts", async (CreateOrderPartDto dto, IRoService svc) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(dto.SupplierName))
            return Results.BadRequest(new { error = "Vui lòng nhập tên nhà cung cấp (SupplierName)." });

        if (dto.DeliveryForm == OrderPartDeliveryForm.Warranty && string.IsNullOrWhiteSpace(dto.VIN))
            return Results.BadRequest(new { error = "Đơn đặt hàng bảo hành bắt buộc có số khung (VIN) xe." });

        if (dto.Items == null || dto.Items.Count == 0)
            return Results.BadRequest(new { error = "Cần danh sách phụ tùng đặt hàng (Items)." });

        var order = new OrderPart
        {
            SupplierName = dto.SupplierName.Trim(),
            DeliveryForm = dto.DeliveryForm,
            DeliveryLocation = string.IsNullOrWhiteSpace(dto.DeliveryLocation) ? "Kho phụ tùng chính" : dto.DeliveryLocation.Trim(),
            OrderDate = dto.OrderDate ?? DateTime.Today,
            EstimatedDeliverDate = dto.EstimatedDeliverDate,
            VIN = dto.VIN?.Trim(),
            ROId = (dto.ROId.HasValue && dto.ROId.Value > 0) ? dto.ROId : null,
            Remark = dto.Remark?.Trim(),
            CreatedBy = "api"
        };

        var lines = dto.Items.Select(i => new OrderPartLine
        {
            PartId = i.PartId,
            Quantity = i.Quantity <= 0 ? 1 : i.Quantity,
            UnitPrice = i.UnitPrice ?? 0,
            DiscountRate = i.DiscountRate ?? 0,
            VatPercent = i.VatPercent ?? 8,
            ApprovedQuantity = i.ApprovedQuantity ?? (i.Quantity <= 0 ? 1 : i.Quantity),
            Note = i.Note?.Trim()
        }).ToList();

        var id = await svc.CreateOrderPartAsync(order, lines);
        return Results.Ok(new { orderPartId = id, orderPartNo = order.OrderPartNo, total = order.Total, message = "Đã lập đơn đặt hàng phụ tùng thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/orderparts/{id:int}/transition", async (int id, TransitionOrderPartDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.TransitionOrderPartStatusAsync(id, dto.ToStatus, dto.SupplierOrderNo, dto.Note);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/orderparts/{id:int}/create-stockin", async (int id, CreateStockInFromOrderDto? dto, IRoService svc) =>
{
    var (ok, msg, stockInId) = await svc.CreateStockInFromOrderPartAsync(id, dto?.ApprovedBy);
    return ok ? Results.Ok(new { message = msg, stockInId }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/orderparts/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteOrderPartAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Quản lý Khoang sửa chữa & Cầu nâng xưởng dịch vụ (Ser_Cavity / Mst_Compartment)
app.MapGet("/api/cavities", async (CavityType? type, CavityStatus? status, string? q, IRoService svc) =>
{
    var list = await svc.CavitiesAsync(type, status, q);
    return Results.Ok(list.Select(c => new
    {
        c.Id,
        c.CavityNo,
        c.CavityName,
        cavityType = Ui.CavityType(c.CavityType).text,
        cavityTypeCode = Ui.CavityType(c.CavityType).code,
        cavityTypeValue = (int)c.CavityType,
        status = Ui.CavityStatus(c.Status).text,
        statusCode = Ui.CavityStatus(c.Status).code,
        statusValue = (int)c.Status,
        c.LiftEquipment,
        c.AreaZone,
        c.CurrentROId,
        roCode = c.CurrentRO?.Code,
        c.CurrentCarPlate,
        c.CurrentCarModel,
        c.CurrentTechnician,
        c.StartUseDate,
        c.ExpectedFinishDate,
        c.FinishUseDate,
        c.Note,
        c.IsActive,
        c.IsInUse,
        elapsedMinutes = c.ElapsedTime.HasValue ? (int)c.ElapsedTime.Value.TotalMinutes : (int?)null,
        c.CreatedAt
    }));
});

app.MapGet("/api/cavities/{id:int}", async (int id, IRoService svc) =>
{
    var c = await svc.GetCavityAsync(id);
    if (c == null) return Results.NotFound(new { error = "Không tìm thấy khoang sửa chữa." });
    return Results.Ok(new
    {
        c.Id,
        c.CavityNo,
        c.CavityName,
        cavityType = Ui.CavityType(c.CavityType).text,
        cavityTypeCode = Ui.CavityType(c.CavityType).code,
        cavityTypeValue = (int)c.CavityType,
        status = Ui.CavityStatus(c.Status).text,
        statusCode = Ui.CavityStatus(c.Status).code,
        statusValue = (int)c.Status,
        c.LiftEquipment,
        c.AreaZone,
        currentRO = c.CurrentRO != null ? new
        {
            c.CurrentRO.Id,
            c.CurrentRO.Code,
            status = Ui.Status(c.CurrentRO.Status).text,
            c.CurrentRO.Total,
            c.CurrentRO.IntakeNote,
            lines = c.CurrentRO.Lines.Select(l => new { l.Id, l.Name, l.Quantity, l.UnitPrice, l.Amount, type = Ui.Line(l.Type) })
        } : null,
        c.CurrentCarPlate,
        c.CurrentCarModel,
        c.CurrentTechnician,
        c.StartUseDate,
        c.ExpectedFinishDate,
        c.FinishUseDate,
        c.Note,
        c.IsActive,
        c.IsInUse,
        elapsedMinutes = c.ElapsedTime.HasValue ? (int)c.ElapsedTime.Value.TotalMinutes : (int?)null,
        c.CreatedAt
    });
});

app.MapPost("/api/cavities", async (CreateCavityDto dto, IRoService svc) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(dto.CavityName))
            return Results.BadRequest(new { error = "Vui lòng nhập tên khoang sửa chữa (CavityName)." });

        var cavity = new Cavity
        {
            CavityNo = dto.CavityNo?.Trim() ?? "",
            CavityName = dto.CavityName.Trim(),
            CavityType = dto.CavityType,
            LiftEquipment = dto.LiftEquipment?.Trim(),
            AreaZone = dto.AreaZone?.Trim(),
            Note = dto.Note?.Trim(),
            IsActive = dto.IsActive ?? true,
            CreatedBy = "api"
        };

        var id = await svc.CreateCavityAsync(cavity);
        return Results.Ok(new { cavityId = id, cavityNo = cavity.CavityNo, message = "Đã thêm khoang sửa chữa thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/cavities/{id:int}", async (int id, UpdateCavityDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.UpdateCavityAsync(id, dto.CavityName, dto.CavityType, dto.LiftEquipment, dto.AreaZone, dto.Note);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/cavities/{id:int}/assign", async (int id, AssignCarToCavityDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.AssignCarToCavityAsync(id, dto.RoId, dto.Technician, dto.ExpectedFinish);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/cavities/{id:int}/release", async (int id, ReleaseCavityDto? dto, IRoService svc) =>
{
    var (ok, msg) = await svc.ReleaseCavityAsync(id, dto?.NextRoStatus);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/cavities/{id:int}/status", async (int id, SetCavityStatusDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.SetCavityStatusAsync(id, dto.Status, dto.Note);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/cavities/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteCavityAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Tiếp nhận & kiểm tra xe ban đầu (Ser_ReceptionF & Ser_ReceptionFDtl)
app.MapGet("/api/receptions", async (ReceptionStatus? status, string? q, DateTime? fromDate, DateTime? toDate, IRoService svc) =>
{
    var list = await svc.ReceptionsAsync(status, q, fromDate, toDate);
    return Results.Ok(list.Select(s => new
    {
        s.Id,
        s.ReceptionNo,
        plate = s.Car.Plate,
        model = s.Car.Model,
        customer = s.Customer.Name,
        phone = s.Customer.Phone,
        s.Odometer,
        fuelLevel = s.FuelLevel,
        fuelLevelText = Ui.FuelLevelText(s.FuelLevel),
        s.LevelOfInspection,
        s.CustomerRequest,
        s.ValuablesInCar,
        s.ExteriorCondition,
        s.IsWarranty,
        s.IsInsurance,
        s.IsBackRepair,
        status = Ui.ReceptionStatus(s.Status).text,
        statusCode = Ui.ReceptionStatus(s.Status).code,
        statusValue = (int)s.Status,
        roId = s.ROId,
        roCode = s.RO?.Code,
        appointmentId = s.AppointmentId,
        appointmentNo = s.Appointment?.AppNo,
        itemCount = s.ItemCount,
        issuesCount = s.IssuesCount,
        s.CreatedBy,
        s.CreatedAt,
        s.DeliveryDateTime,
        s.DeliveryBy,
        s.DeliveryNote
    }));
});

app.MapGet("/api/receptions/{id:int}", async (int id, IRoService svc) =>
{
    var s = await svc.GetReceptionAsync(id);
    if (s == null) return Results.NotFound(new { error = "Không tìm thấy phiếu tiếp nhận xe." });
    return Results.Ok(new
    {
        s.Id,
        s.ReceptionNo,
        car = new { s.Car.Id, s.Car.Plate, s.Car.Model, s.Car.Vin, s.Car.Year },
        customer = new { s.Customer.Id, s.Customer.Name, s.Customer.Phone, s.Customer.Email },
        s.Odometer,
        fuelLevel = s.FuelLevel,
        fuelLevelText = Ui.FuelLevelText(s.FuelLevel),
        s.LevelOfInspection,
        s.CustomerRequest,
        s.ValuablesInCar,
        s.ExteriorCondition,
        s.IsWarranty,
        s.IsInsurance,
        s.IsBackRepair,
        status = Ui.ReceptionStatus(s.Status).text,
        statusCode = Ui.ReceptionStatus(s.Status).code,
        statusValue = (int)s.Status,
        ro = s.RO != null ? new { s.RO.Id, s.RO.Code, s.RO.Status, cavity = s.RO.Cavity?.CavityName } : null,
        appointment = s.Appointment != null ? new { s.Appointment.Id, s.Appointment.AppNo, s.Appointment.AppointmentDate } : null,
        s.ItemCount,
        s.IssuesCount,
        s.CreatedBy,
        s.CreatedAt,
        s.DeliveryDateTime,
        s.DeliveryBy,
        s.DeliveryNote,
        items = s.Items.Select(i => new
        {
            i.Id,
            i.Group,
            i.Code,
            i.Name,
            receptionStatus = Ui.AuditStatus(i.ReceptionStatus).text,
            receptionStatusValue = (int)i.ReceptionStatus,
            deliveryStatus = Ui.AuditStatus(i.DeliveryStatus).text,
            deliveryStatusValue = (int)i.DeliveryStatus,
            i.Note
        })
    });
});

app.MapPost("/api/receptions", async (CreateReceptionDto dto, IRoService svc) =>
{
    try
    {
        if (dto.CarId <= 0) return Results.BadRequest(new { error = "Vui lòng chọn CarId hợp lệ." });
        if (string.IsNullOrWhiteSpace(dto.CustomerRequest)) return Results.BadRequest(new { error = "Vui lòng nhập CustomerRequest." });

        var sheet = new ReceptionSheet
        {
            CarId = dto.CarId,
            AppointmentId = (dto.AppointmentId.HasValue && dto.AppointmentId.Value > 0) ? dto.AppointmentId : null,
            Odometer = dto.Odometer,
            FuelLevel = dto.FuelLevel >= 1 && dto.FuelLevel <= 4 ? dto.FuelLevel : 2,
            LevelOfInspection = string.IsNullOrWhiteSpace(dto.LevelOfInspection) ? "Bảo dưỡng 10.000 km" : dto.LevelOfInspection.Trim(),
            CustomerRequest = dto.CustomerRequest.Trim(),
            ValuablesInCar = dto.ValuablesInCar?.Trim(),
            ExteriorCondition = dto.ExteriorCondition?.Trim(),
            IsWarranty = dto.IsWarranty ?? false,
            IsInsurance = dto.IsInsurance ?? false,
            IsBackRepair = dto.IsBackRepair ?? false,
            CreatedBy = dto.CreatedBy ?? "api"
        };

        var items = dto.Items?.Select(i => new ReceptionItem
        {
            Group = i.Group,
            Code = i.Code,
            Name = i.Name,
            ReceptionStatus = i.ReceptionStatus,
            DeliveryStatus = AuditStatus.Good,
            Note = i.Note?.Trim()
        }).ToList();

        var id = await svc.CreateReceptionAsync(sheet, items);
        return Results.Ok(new { receptionId = id, receptionNo = sheet.ReceptionNo, message = "Đã lập phiếu tiếp nhận xe thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/receptions/{id:int}/create-ro", async (int id, CreateRoFromReceptionDto? dto, IRoService svc) =>
{
    var (ok, msg, roId) = await svc.CreateROFromReceptionAsync(id, dto?.Technician);
    return ok ? Results.Ok(new { message = msg, roId }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/receptions/{id:int}/deliver", async (int id, DeliverReceptionDto dto, IRoService svc) =>
{
    var items = dto.Items?.Select(i => (i.ItemId, i.DeliveryStatus)).ToList();
    var (ok, msg) = await svc.DeliverCarAsync(id, dto.DeliveryBy, dto.Note, items);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/receptions/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteReceptionAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Phân công thợ sửa chữa & Điều phối xưởng (Ser_AssignmentWork, Ser_AssignmentWorkEngineer, Ser_Engineer, Ser_GroupRepair)
app.MapGet("/api/assignments", async (AssignmentWorkStatus? status, string? q, DateTime? fromDate, DateTime? toDate, int? roId, IRoService svc) =>
{
    var list = await svc.AssignmentWorksAsync(status, q, fromDate, toDate, roId);
    return Results.Ok(list.Select(a => new
    {
        a.Id,
        a.AssignmentNo,
        roId = a.ROId,
        roCode = a.RO.Code,
        plate = a.RO.Car.Plate,
        model = a.RO.Car.Model,
        customer = a.RO.Customer.Name,
        phone = a.RO.Customer.Phone,
        status = Ui.AssignmentWorkStatus(a.Status).text,
        statusCode = Ui.AssignmentWorkStatus(a.Status).code,
        statusValue = (int)a.Status,
        primaryTechnician = a.PrimaryTechnician,
        engineerCount = a.EngineerCount,
        totalAssignedHours = a.TotalAssignedHours,
        a.SCCPlanStartDTime,
        a.SCCPlanFinishDTime,
        sccCavity = a.SCCCavity?.CavityName,
        a.SCDPlanStartDTime,
        a.SCDPlanFinishDTime,
        scdCavity = a.SCDCavity?.CavityName,
        a.SCSPlanStartDTime,
        a.SCSPlanFinishDTime,
        scsCavity = a.SCSCavity?.CavityName,
        a.Note,
        a.CreatedBy,
        a.CreatedAt,
        a.StartedAt,
        a.FinishedAt
    }));
});

app.MapGet("/api/assignments/{id:int}", async (int id, IRoService svc) =>
{
    var a = await svc.GetAssignmentWorkAsync(id);
    if (a == null) return Results.NotFound(new { error = "Không tìm thấy phiếu phân công công việc." });
    return Results.Ok(new
    {
        a.Id,
        a.AssignmentNo,
        ro = new { a.RO.Id, a.RO.Code, status = Ui.Status(a.RO.Status).text, a.RO.Total, a.RO.IntakeNote },
        car = new { a.RO.Car.Id, a.RO.Car.Plate, a.RO.Car.Model, a.RO.Car.Vin, a.RO.Car.Year },
        customer = new { a.RO.Customer.Id, a.RO.Customer.Name, a.RO.Customer.Phone, a.RO.Customer.Email },
        status = Ui.AssignmentWorkStatus(a.Status).text,
        statusCode = Ui.AssignmentWorkStatus(a.Status).code,
        statusValue = (int)a.Status,
        primaryTechnician = a.PrimaryTechnician,
        scc = new
        {
            a.SCCPlanStartDTime,
            a.SCCPlanFinishDTime,
            a.SCCActualStartDTime,
            a.SCCActualFinishDTime,
            cavity = a.SCCCavity != null ? new { a.SCCCavity.Id, a.SCCCavity.CavityNo, a.SCCCavity.CavityName } : null
        },
        scd = new
        {
            a.SCDPlanStartDTime,
            a.SCDPlanFinishDTime,
            a.SCDActualStartDTime,
            a.SCDActualFinishDTime,
            cavity = a.SCDCavity != null ? new { a.SCDCavity.Id, a.SCDCavity.CavityNo, a.SCDCavity.CavityName } : null
        },
        scs = new
        {
            a.SCSPlanStartDTime,
            a.SCSPlanFinishDTime,
            a.SCSActualStartDTime,
            a.SCSActualFinishDTime,
            cavity = a.SCSCavity != null ? new { a.SCSCavity.Id, a.SCSCavity.CavityNo, a.SCSCavity.CavityName } : null
        },
        engineers = a.Engineers.Select(e => new
        {
            e.Id,
            e.EngineerId,
            engineerNo = e.Engineer.EngineerNo,
            engineerName = e.Engineer.EngineerName,
            group = e.Engineer.GroupRepair?.GroupRName,
            skillLevel = e.Engineer.SkillLevel,
            workType = Ui.WorkType(e.WorkType).text,
            workTypeCode = Ui.WorkType(e.WorkType).code,
            workTypeValue = (int)e.WorkType,
            e.IsPrimary,
            e.AssignedHours,
            e.Note
        }),
        a.Note,
        a.CreatedBy,
        a.CreatedAt,
        a.StartedAt,
        a.FinishedAt
    });
});

app.MapPost("/api/assignments", async (CreateAssignmentDto dto, IRoService svc) =>
{
    try
    {
        if (dto.RoId <= 0) return Results.BadRequest(new { error = "Vui lòng chọn RoId hợp lệ." });

        var assignment = new AssignmentWork
        {
            ROId = dto.RoId,
            SCCPlanStartDTime = dto.SccPlanStart,
            SCCPlanFinishDTime = dto.SccPlanFinish,
            SCCCavityId = dto.SccCavityId,
            SCDPlanStartDTime = dto.ScdPlanStart,
            SCDPlanFinishDTime = dto.ScdPlanFinish,
            SCDCavityId = dto.ScdCavityId,
            SCSPlanStartDTime = dto.ScsPlanStart,
            SCSPlanFinishDTime = dto.ScsPlanFinish,
            SCSCavityId = dto.ScsCavityId,
            Note = dto.Note?.Trim(),
            CreatedBy = dto.CreatedBy ?? "api"
        };

        var engineers = dto.Engineers?.Select(e => new AssignmentEngineer
        {
            EngineerId = e.EngineerId,
            WorkType = e.WorkType,
            AssignedHours = e.AssignedHours ?? 1.0m,
            IsPrimary = e.IsPrimary ?? false,
            Note = e.Note?.Trim()
        }).ToList() ?? [];

        var id = await svc.CreateAssignmentWorkAsync(assignment, engineers);
        return Results.Ok(new { assignmentId = id, assignmentNo = assignment.AssignmentNo, message = "Đã lập phiếu phân công sửa chữa thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/assignments/{id:int}/start", async (int id, StartAssignmentDto? dto, IRoService svc) =>
{
    var (ok, msg) = await svc.StartAssignmentWorkAsync(id, dto?.StartedBy);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/assignments/{id:int}/complete", async (int id, CompleteAssignmentDto? dto, IRoService svc) =>
{
    var (ok, msg) = await svc.CompleteAssignmentWorkAsync(id, dto?.CompletedBy);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/assignments/{id:int}/cancel", async (int id, CancelAssignmentDto? dto, IRoService svc) =>
{
    var (ok, msg) = await svc.CancelAssignmentWorkAsync(id, dto?.Reason);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/assignments/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteAssignmentWorkAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapGet("/api/engineers", async (string? q, int? groupId, bool? isActive, IRoService svc) =>
{
    var list = await svc.EngineersAsync(q, groupId, isActive);
    return Results.Ok(list.Select(e => new
    {
        e.Id,
        e.EngineerNo,
        e.EngineerName,
        e.Phone,
        e.SkillLevel,
        e.Specialty,
        groupId = e.GroupRId,
        groupName = e.GroupRepair?.GroupRName,
        e.IsActive,
        e.CreatedAt
    }));
});

app.MapPost("/api/engineers", async (CreateEngineerDto dto, IRoService svc) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return Results.BadRequest(new { error = "Vui lòng nhập tên kỹ thuật viên." });
        var eng = new Engineer
        {
            EngineerNo = dto.Code?.Trim() ?? "",
            EngineerName = dto.Name.Trim(),
            Phone = dto.Phone?.Trim(),
            SkillLevel = string.IsNullOrWhiteSpace(dto.SkillLevel) ? "Bậc 3/7" : dto.SkillLevel.Trim(),
            Specialty = string.IsNullOrWhiteSpace(dto.Specialty) ? "Sửa chữa chung" : dto.Specialty.Trim(),
            GroupRId = dto.GroupId
        };
        var id = await svc.CreateEngineerAsync(eng);
        return Results.Ok(new { engineerId = id, engineerNo = eng.EngineerNo, message = "Đã thêm kỹ thuật viên thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/groups", async (string? q, bool? isActive, IRoService svc) =>
{
    var list = await svc.GroupRepairsAsync(q, isActive);
    return Results.Ok(list.Select(g => new
    {
        g.Id,
        g.GroupRNo,
        g.GroupRName,
        g.LeaderName,
        engineerCount = g.Engineers.Count,
        g.Note,
        g.IsActive,
        g.CreatedAt
    }));
});

app.MapPost("/api/groups", async (CreateGroupDto dto, IRoService svc) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return Results.BadRequest(new { error = "Vui lòng nhập tên tổ sửa chữa." });
        var grp = new GroupRepair
        {
            GroupRNo = dto.Code?.Trim() ?? "",
            GroupRName = dto.Name.Trim(),
            LeaderName = dto.Leader?.Trim() ?? "",
            Note = dto.Note?.Trim()
        };
        var id = await svc.CreateGroupRepairAsync(grp);
        return Results.Ok(new { groupId = id, groupRNo = grp.GroupRNo, message = "Đã thêm tổ sửa chữa thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// API Quản lý Bảo hiểm xe & Hồ sơ bồi thường (Ser_Insurance, Ser_InsuranceContract, Ser_InsuranceDebit)
app.MapGet("/api/insurance/companies", async (string? q, bool? isActive, IRoService svc) =>
{
    var list = await svc.InsuranceCompaniesAsync(q, isActive);
    return Results.Ok(list.Select(c => new
    {
        c.Id,
        c.InsNo,
        c.InsName,
        c.Address,
        c.Phone,
        c.Email,
        c.TaxCode,
        c.Hotline,
        c.IsActive,
        contractCount = c.Contracts.Count,
        claimCount = c.Claims.Count,
        c.CreatedAt
    }));
});

app.MapGet("/api/insurance/companies/{id:int}", async (int id, IRoService svc) =>
{
    var c = await svc.GetInsuranceCompanyAsync(id);
    if (c == null) return Results.NotFound(new { error = "Không tìm thấy hãng bảo hiểm." });
    return Results.Ok(new
    {
        c.Id,
        c.InsNo,
        c.InsName,
        c.Address,
        c.Phone,
        c.Email,
        c.TaxCode,
        c.Hotline,
        c.IsActive,
        contracts = c.Contracts.Select(ct => new
        {
            ct.Id,
            ct.ContractNo,
            ct.ContractCode,
            ct.StartDate,
            ct.FinishDate,
            paymentType = Ui.InsurancePaymentType(ct.PaymentType).text,
            ct.PaymentLimit,
            ct.DiscountLaborRate,
            ct.DiscountPartRate,
            ct.IsActive
        }),
        c.CreatedAt
    });
});

app.MapPost("/api/insurance/companies", async (CreateInsuranceCompanyDto dto, IRoService svc) =>
{
    try
    {
        var company = new InsuranceCompany
        {
            InsNo = dto.InsNo?.Trim() ?? "",
            InsName = dto.InsName.Trim(),
            Address = dto.Address?.Trim(),
            Phone = dto.Phone?.Trim(),
            Email = dto.Email?.Trim(),
            TaxCode = dto.TaxCode?.Trim(),
            Hotline = dto.Hotline?.Trim(),
            IsActive = true
        };
        var id = await svc.CreateInsuranceCompanyAsync(company);
        return Results.Ok(new { companyId = id, insNo = company.InsNo, message = "Đã thêm hãng bảo hiểm thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/insurance/contracts", async (int? companyId, bool? activeOnly, IRoService svc) =>
{
    var list = await svc.InsuranceContractsAsync(companyId, activeOnly);
    return Results.Ok(list.Select(ct => new
    {
        ct.Id,
        ct.ContractNo,
        ct.ContractCode,
        companyId = ct.InsuranceCompanyId,
        companyName = ct.InsuranceCompany.InsName,
        ct.StartDate,
        ct.FinishDate,
        paymentType = Ui.InsurancePaymentType(ct.PaymentType).text,
        paymentTypeValue = (int)ct.PaymentType,
        ct.PaymentLimit,
        ct.DiscountLaborRate,
        ct.DiscountPartRate,
        ct.Note,
        ct.IsActive,
        ct.CreatedAt
    }));
});

app.MapPost("/api/insurance/contracts", async (CreateInsuranceContractDto dto, IRoService svc) =>
{
    try
    {
        var ct = new InsuranceContract
        {
            ContractNo = dto.ContractNo?.Trim() ?? "",
            ContractCode = dto.ContractCode?.Trim() ?? "",
            InsuranceCompanyId = dto.InsuranceCompanyId,
            StartDate = dto.StartDate != default ? dto.StartDate : DateTime.Today,
            FinishDate = dto.FinishDate != default ? dto.FinishDate : DateTime.Today.AddYears(1),
            PaymentType = dto.PaymentType,
            PaymentLimit = dto.PaymentLimit > 0 ? dto.PaymentLimit : 500_000_000m,
            DiscountLaborRate = dto.DiscountLaborRate,
            DiscountPartRate = dto.DiscountPartRate,
            Note = dto.Note?.Trim(),
            IsActive = true
        };
        var id = await svc.CreateInsuranceContractAsync(ct);
        return Results.Ok(new { contractId = id, contractNo = ct.ContractNo, message = "Đã thêm hợp đồng bảo hiểm." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/insurance/claims", async (InsuranceClaimStatus? status, string? q, int? roId, int? companyId, IRoService svc) =>
{
    var list = await svc.InsuranceClaimsAsync(status, q, roId, companyId);
    return Results.Ok(list.Select(c => new
    {
        c.Id,
        c.ClaimNo,
        roId = c.ROId,
        roCode = c.RO.Code,
        plate = c.RO.Car.Plate,
        model = c.RO.Car.Model,
        customer = c.RO.Customer.Name,
        company = c.InsuranceCompany.InsName,
        c.PolicyNo,
        c.ClaimFileNo,
        c.SurveyorName,
        c.SurveyorPhone,
        c.AccidentDate,
        c.AccidentLocation,
        c.EstimatedAmount,
        c.ApprovedAmount,
        c.DeductibleAmount,
        c.PenaltyAmount,
        c.InsuranceAmount,
        c.CustomerAmount,
        status = Ui.InsuranceClaimStatus(c.Status).text,
        statusCode = Ui.InsuranceClaimStatus(c.Status).code,
        statusValue = (int)c.Status,
        itemCount = c.Items.Count,
        c.CreatedAt,
        c.SubmittedAt,
        c.ApprovedAt,
        c.SettledAt
    }));
});

app.MapGet("/api/insurance/claims/{id:int}", async (int id, IRoService svc) =>
{
    var c = await svc.GetInsuranceClaimAsync(id);
    if (c == null) return Results.NotFound(new { error = "Không tìm thấy hồ sơ bồi thường bảo hiểm." });
    return Results.Ok(new
    {
        c.Id,
        c.ClaimNo,
        ro = new { c.RO.Id, c.RO.Code, c.RO.Status, roTotal = c.RO.Total },
        car = new { c.RO.Car.Id, c.RO.Car.Plate, c.RO.Car.Model, c.RO.Car.Vin, c.RO.Car.Year },
        customer = new { c.RO.Customer.Id, c.RO.Customer.Name, c.RO.Customer.Phone },
        company = new { c.InsuranceCompany.Id, c.InsuranceCompany.InsNo, c.InsuranceCompany.InsName, c.InsuranceCompany.Phone, c.InsuranceCompany.Hotline },
        contract = c.InsuranceContract != null ? new { c.InsuranceContract.Id, c.InsuranceContract.ContractNo, c.InsuranceContract.PaymentLimit } : null,
        c.PolicyNo,
        c.ClaimFileNo,
        c.SurveyorName,
        c.SurveyorPhone,
        c.AccidentDate,
        c.AccidentLocation,
        c.AccidentDescription,
        c.EstimatedAmount,
        c.ApprovedAmount,
        c.DeductibleAmount,
        c.PenaltyAmount,
        c.InsuranceAmount,
        c.CustomerAmount,
        status = Ui.InsuranceClaimStatus(c.Status).text,
        statusCode = Ui.InsuranceClaimStatus(c.Status).code,
        statusValue = (int)c.Status,
        c.DecisionNote,
        c.RejectionReason,
        items = c.Items.Select(i => new
        {
            i.Id,
            type = Ui.Line(i.Type),
            i.Code,
            i.Name,
            i.Quantity,
            i.UnitPrice,
            i.EstimatedAmount,
            i.ApprovedAmount,
            i.IsApproved,
            i.Note
        }),
        c.CreatedBy,
        c.CreatedAt,
        c.SubmittedAt,
        c.ApprovedAt,
        c.SettledAt
    });
});

app.MapPost("/api/insurance/claims", async (CreateInsuranceClaimDto dto, IRoService svc) =>
{
    try
    {
        var claim = new InsuranceClaim
        {
            ROId = dto.RoId,
            InsuranceCompanyId = dto.CompanyId,
            InsuranceContractId = dto.ContractId,
            PolicyNo = dto.PolicyNo.Trim(),
            ClaimFileNo = dto.ClaimFileNo?.Trim(),
            SurveyorName = dto.SurveyorName?.Trim(),
            SurveyorPhone = dto.SurveyorPhone?.Trim(),
            AccidentDate = dto.AccidentDate ?? DateTime.Today,
            AccidentLocation = dto.AccidentLocation?.Trim(),
            AccidentDescription = dto.AccidentDescription?.Trim() ?? "Tổn thất thân vỏ xe",
            DeductibleAmount = dto.DeductibleAmount ?? 500_000m,
            PenaltyAmount = dto.PenaltyAmount ?? 0m,
            CreatedBy = dto.CreatedBy ?? "api",
            Status = InsuranceClaimStatus.Draft
        };

        var items = dto.Items?.Select(i => new InsuranceClaimItem
        {
            Type = i.Type,
            Code = i.Code,
            Name = i.Name,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            EstimatedAmount = i.EstimatedAmount ?? (i.Quantity * i.UnitPrice),
            ApprovedAmount = i.ApprovedAmount ?? (i.Quantity * i.UnitPrice),
            IsApproved = i.IsApproved ?? true,
            Note = i.Note
        }).ToList();

        var id = await svc.CreateInsuranceClaimAsync(claim, items);
        return Results.Ok(new { claimId = id, claimNo = claim.ClaimNo, message = "Đã lập hồ sơ bồi thường bảo hiểm." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/insurance/claims/from-ro", async (CreateInsuranceClaimFromRoDto dto, IRoService svc) =>
{
    try
    {
        var id = await svc.CreateInsuranceClaimFromROAsync(
            dto.RoId, dto.CompanyId, dto.ContractId, dto.PolicyNo, dto.ClaimFileNo,
            dto.SurveyorName, dto.SurveyorPhone, dto.AccidentDescription ?? "",
            dto.DeductibleAmount ?? 500_000m, dto.PenaltyAmount ?? 0m, dto.CreatedBy ?? "api");
        return Results.Ok(new { claimId = id, message = "Đã tạo hồ sơ bảo hiểm từ Lệnh sửa chữa RO." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/insurance/claims/{id:int}/transition", async (int id, TransitionInsuranceClaimDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.TransitionInsuranceClaimAsync(id, dto.ToStatus, dto.ApprovedAmount, dto.Note);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/insurance/claims/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteInsuranceClaimAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Chiến dịch Khuyến mãi CSKH (Ser_CampaignMarketing, Ser_CampaignMarketingPart)
app.MapGet("/api/campaigns", async (CampaignMarketingStatus? status, string? q, bool? currentOnly, IRoService svc) =>
{
    var list = await svc.CampaignMarketingsAsync(status, q, currentOnly);
    return Results.Ok(list.Select(c => new
    {
        c.Id,
        c.CamMarketingNo,
        c.CamMarketingName,
        c.CamMarketingDesc,
        c.EffDateStart,
        c.EffDateEnd,
        c.ConditionModel,
        c.ConditionPlateNo,
        c.ConditionVIN,
        c.DiscountLaborPercent,
        c.DiscountPartPercent,
        status = Ui.CampaignStatus(c.Status).text,
        statusCode = Ui.CampaignStatus(c.Status).code,
        statusValue = (int)c.Status,
        c.IsActiveNow,
        c.ItemCount,
        c.ROAppliedCount,
        c.TotalDiscountGranted,
        c.TotalRevenueGenerated,
        c.CreatedBy,
        c.CreatedAt,
        c.ApprovedBy,
        c.ApprovedAt
    }));
});

app.MapGet("/api/campaigns/{id:int}", async (int id, IRoService svc) =>
{
    var c = await svc.GetCampaignMarketingAsync(id);
    if (c == null) return Results.NotFound(new { error = "Không tìm thấy chiến dịch khuyến mãi." });
    return Results.Ok(new
    {
        c.Id,
        c.CamMarketingNo,
        c.CamMarketingName,
        c.CamMarketingDesc,
        c.EffDateStart,
        c.EffDateEnd,
        c.ConditionModel,
        c.ConditionPlateNo,
        c.ConditionVIN,
        c.DiscountLaborPercent,
        c.DiscountPartPercent,
        status = Ui.CampaignStatus(c.Status).text,
        statusCode = Ui.CampaignStatus(c.Status).code,
        statusValue = (int)c.Status,
        c.IsActiveNow,
        items = c.Items.Select(i => new
        {
            i.Id,
            i.PartId,
            i.PartCode,
            i.PartName,
            salePrice = i.Part?.SalePrice ?? 0,
            i.PercentDiscount,
            discountedPrice = (i.Part?.SalePrice ?? 0) * (1 - (i.PercentDiscount / 100m)),
            i.MaxQuantity,
            i.Note
        }),
        appliedROs = c.AppliedROs.Select(r => new
        {
            r.Id,
            r.Code,
            plate = r.Car?.Plate,
            model = r.Car?.Model,
            customer = r.Customer?.Name,
            status = Ui.Status(r.Status).text,
            discountAmount = r.CampaignDiscountAmount,
            total = r.Total
        }),
        c.TotalDiscountGranted,
        c.TotalRevenueGenerated,
        c.CreatedBy,
        c.CreatedAt,
        c.ApprovedBy,
        c.ApprovedAt
    });
});

app.MapPost("/api/campaigns", async (CreateCampaignDto dto, IRoService svc) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(dto.CamMarketingName))
            return Results.BadRequest(new { error = "Vui lòng nhập tên chiến dịch khuyến mãi." });

        var campaign = new CampaignMarketing
        {
            CamMarketingNo = dto.CamMarketingNo?.Trim() ?? "",
            CamMarketingName = dto.CamMarketingName.Trim(),
            CamMarketingDesc = dto.CamMarketingDesc?.Trim(),
            EffDateStart = dto.EffDateStart ?? DateTime.Today,
            EffDateEnd = dto.EffDateEnd ?? DateTime.Today.AddMonths(1),
            ConditionModel = dto.ConditionModel?.Trim(),
            ConditionPlateNo = dto.ConditionPlateNo?.Trim(),
            ConditionVIN = dto.ConditionVIN?.Trim(),
            DiscountLaborPercent = dto.DiscountLaborPercent ?? 0,
            DiscountPartPercent = dto.DiscountPartPercent ?? 0,
            CreatedBy = dto.CreatedBy ?? "api",
            Status = CampaignMarketingStatus.Active
        };

        var items = dto.Items?.Select(i => new CampaignMarketingItem
        {
            PartId = i.PartId,
            PartCode = i.PartCode?.Trim() ?? "",
            PartName = i.PartName?.Trim() ?? "",
            PercentDiscount = i.PercentDiscount ?? 15,
            MaxQuantity = i.MaxQuantity ?? 999,
            Note = i.Note?.Trim()
        }).ToList() ?? [];

        var id = await svc.CreateCampaignMarketingAsync(campaign, items);
        return Results.Ok(new { campaignId = id, camMarketingNo = campaign.CamMarketingNo, message = "Đã tạo chiến dịch khuyến mãi thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/campaigns/{id:int}/transition", async (int id, TransitionCampaignDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.TransitionCampaignStatusAsync(id, dto.ToStatus, dto.ApprovedBy);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/campaigns/{id:int}/apply-to-ro/{roId:int}", async (int id, int roId, IRoService svc) =>
{
    var (ok, msg, discount) = await svc.ApplyCampaignToROAsync(id, roId);
    return ok ? Results.Ok(new { message = msg, discountAmount = discount }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/campaigns/ro/{roId:int}/remove-campaign", async (int roId, IRoService svc) =>
{
    var (ok, msg) = await svc.RemoveCampaignFromROAsync(roId);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapGet("/api/campaigns/active-for-car/{carId:int}", async (int carId, IRoService svc) =>
{
    var list = await svc.GetEligibleCampaignsForCarAsync(carId);
    return Results.Ok(list.Select(c => new
    {
        c.Id,
        c.CamMarketingNo,
        c.CamMarketingName,
        c.DiscountLaborPercent,
        c.DiscountPartPercent,
        c.EffDateStart,
        c.EffDateEnd,
        c.ConditionModel,
        c.ConditionPlateNo,
        c.ConditionVIN
    }));
});

app.MapDelete("/api/campaigns/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteCampaignMarketingAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Quản lý Nhắc bảo dưỡng định kỳ xe (Ser_CustomerCareMace / MH 83)
app.MapGet("/api/caremaces", async (CustomerCareMaceStatus? status, MaceType? maceType, string? timeFilter, string? q, IRoService svc) =>
{
    var list = await svc.CustomerCareMacesAsync(status, maceType, timeFilter, q);
    return Results.Ok(list.Select(m => new
    {
        m.Id,
        m.MaceNo,
        carId = m.CarId,
        plate = m.Car?.Plate,
        model = m.Car?.Model,
        customerId = m.CustomerId,
        customer = m.Customer?.Name,
        phone = m.Customer?.Phone,
        maceType = Ui.MaceType(m.MaceType).text,
        maceTypeValue = (int)m.MaceType,
        m.LastKm,
        m.NextKm,
        m.MaceRecomentDate,
        status = Ui.CustomerCareMaceStatus(m.Status).text,
        statusCode = Ui.CustomerCareMaceStatus(m.Status).code,
        statusValue = (int)m.Status,
        m.ContactDate,
        m.ContactBy,
        m.ApointDate,
        m.Remark,
        m.IsOverdue,
        m.IsDueSoon,
        roId = m.ROId,
        roCode = m.RO?.Code,
        appointmentId = m.AppointmentId,
        appointmentNo = m.Appointment?.AppNo,
        m.CreatedAt,
        m.UpdatedAt
    }));
});

app.MapGet("/api/caremaces/{id:int}", async (int id, IRoService svc) =>
{
    var m = await svc.GetCustomerCareMaceAsync(id);
    if (m == null) return Results.NotFound(new { error = "Không tìm thấy phiếu nhắc bảo dưỡng." });
    return Results.Ok(new
    {
        m.Id,
        m.MaceNo,
        car = new { m.Car.Id, m.Car.Plate, m.Car.Model, m.Car.Vin, m.Car.Year },
        customer = new { m.Customer.Id, m.Customer.Name, m.Customer.Phone, m.Customer.Email },
        maceType = Ui.MaceType(m.MaceType).text,
        maceTypeValue = (int)m.MaceType,
        m.LastKm,
        m.NextKm,
        m.MaceRecomentDate,
        status = Ui.CustomerCareMaceStatus(m.Status).text,
        statusCode = Ui.CustomerCareMaceStatus(m.Status).code,
        statusValue = (int)m.Status,
        m.ContactDate,
        m.ContactBy,
        m.ApointDate,
        m.Remark,
        m.IsOverdue,
        m.IsDueSoon,
        ro = m.RO != null ? new { m.RO.Id, m.RO.Code, m.RO.Status, m.RO.Odometer, m.RO.FinishedAt } : null,
        appointment = m.Appointment != null ? new { m.Appointment.Id, m.Appointment.AppNo, m.Appointment.AppointmentDate, m.Appointment.Status } : null,
        m.CreatedBy,
        m.CreatedAt,
        m.UpdatedAt
    });
});

app.MapPost("/api/caremaces", async (CreateCareMaceDto dto, IRoService svc) =>
{
    try
    {
        if (dto.CarId <= 0) return Results.BadRequest(new { error = "Vui lòng chọn xe (CarId)." });

        var mace = new CustomerCareMace
        {
            CarId = dto.CarId,
            MaceType = dto.MaceType ?? MaceType.Standard6Months,
            LastKm = dto.LastKm ?? 0,
            NextKm = dto.NextKm ?? 0,
            MaceRecomentDate = dto.MaceRecomentDate ?? DateTime.Today.AddMonths(6),
            Remark = dto.Remark?.Trim(),
            CreatedBy = dto.CreatedBy ?? "api",
            Status = CustomerCareMaceStatus.Pending
        };

        var id = await svc.CreateCustomerCareMaceAsync(mace);
        return Results.Ok(new { maceId = id, maceNo = mace.MaceNo, message = "Đã tạo phiếu nhắc bảo dưỡng thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/caremaces/{id:int}/update-call", async (int id, UpdateCareMaceCallDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.UpdateCustomerCareMaceCallAsync(id, dto.Status, dto.ContactDate, dto.ApointDate, dto.Remark, dto.ContactBy);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/caremaces/{id:int}/convert-to-appointment", async (int id, ConvertMaceToAppointmentDto? dto, IRoService svc) =>
{
    var (ok, msg, appId) = await svc.ConvertMaceToAppointmentAsync(id, dto?.Advisor, dto?.Cavity, dto?.Note);
    return ok ? Results.Ok(new { message = msg, appointmentId = appId }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/caremaces/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteCustomerCareMaceAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Quản lý Kiểm kê & Điều chuyển / Cân đối kho phụ tùng (Ser_Inv_StockAdj & Ser_Inv_StockAdjDetail)
app.MapGet("/api/stockadjs", async (StockAdjStatus? status, StockAdjType? type, string? q, DateTime? fromDate, DateTime? toDate, IRoService svc) =>
{
    var list = await svc.StockAdjsAsync(status, type, q, fromDate, toDate);
    return Results.Ok(list.Select(s => new
    {
        s.Id,
        s.StockAdjNo,
        s.StockAdjDate,
        type = Ui.StockAdjType(s.Type).text,
        typeCode = Ui.StockAdjType(s.Type).code,
        typeValue = (int)s.Type,
        status = Ui.StockAdjStatus(s.Status).text,
        statusCode = Ui.StockAdjStatus(s.Status).code,
        statusValue = (int)s.Status,
        s.StorageCode,
        s.Remark,
        s.CreatedBy,
        s.CreatedAt,
        s.ApprovedBy,
        s.FinishedAt,
        s.TotalItems,
        s.TotalSystemQty,
        s.TotalActualQty,
        s.TotalDiffQty,
        s.TotalDiffAmount,
        s.HasDiscrepancy,
        s.DiscrepancyCount
    }));
});

app.MapGet("/api/stockadjs/{id:int}", async (int id, IRoService svc) =>
{
    var s = await svc.GetStockAdjAsync(id);
    if (s == null) return Results.NotFound(new { error = "Không tìm thấy phiếu kiểm kê kho." });
    return Results.Ok(new
    {
        s.Id,
        s.StockAdjNo,
        s.StockAdjDate,
        type = Ui.StockAdjType(s.Type).text,
        typeCode = Ui.StockAdjType(s.Type).code,
        typeValue = (int)s.Type,
        status = Ui.StockAdjStatus(s.Status).text,
        statusCode = Ui.StockAdjStatus(s.Status).code,
        statusValue = (int)s.Status,
        s.StorageCode,
        s.Remark,
        s.CreatedBy,
        s.CreatedAt,
        s.ApprovedBy,
        s.FinishedAt,
        s.TotalItems,
        s.TotalSystemQty,
        s.TotalActualQty,
        s.TotalDiffQty,
        s.TotalDiffAmount,
        s.HasDiscrepancy,
        s.DiscrepancyCount,
        items = s.Items.Select(i => new
        {
            i.Id,
            i.PartId,
            i.PartCode,
            i.PartName,
            i.Unit,
            i.CostPrice,
            i.SystemQuantity,
            i.ActualQuantity,
            i.DiffQuantity,
            i.DiffAmount,
            i.FromLocation,
            i.ToLocation,
            i.Note
        })
    });
});

app.MapPost("/api/stockadjs", async (CreateStockAdjDto dto, IRoService svc) =>
{
    try
    {
        if (dto.Items == null || dto.Items.Count == 0)
            return Results.BadRequest(new { error = "Vui lòng chọn danh sách phụ tùng kiểm kê (Items)." });

        var adj = new StockAdj
        {
            StockAdjNo = dto.StockAdjNo?.Trim() ?? "",
            Type = dto.Type ?? StockAdjType.CountBalance,
            StorageCode = string.IsNullOrWhiteSpace(dto.StorageCode) ? "KHO-CHINH" : dto.StorageCode.Trim().ToUpperInvariant(),
            StockAdjDate = dto.StockAdjDate ?? DateTime.Today,
            Remark = dto.Remark?.Trim(),
            CreatedBy = dto.CreatedBy ?? "api",
            Status = StockAdjStatus.Pending
        };

        var details = dto.Items.Select(i => new StockAdjDetail
        {
            PartId = i.PartId,
            ActualQuantity = i.ActualQuantity,
            ToLocation = i.ToLocation?.Trim(),
            Note = i.Note?.Trim()
        }).ToList();

        var id = await svc.CreateStockAdjAsync(adj, details);
        return Results.Ok(new { stockAdjId = id, stockAdjNo = adj.StockAdjNo, message = "Đã lập phiếu kiểm kê / điều chuyển kho thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/stockadjs/{id:int}/transition", async (int id, TransitionStockAdjDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.TransitionStockAdjStatusAsync(id, dto.ToStatus, dto.ApprovedBy, dto.Note);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/stockadjs/{id:int}/update-items", async (int id, UpdateStockAdjItemsDto dto, IRoService svc) =>
{
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Không có danh sách phụ tùng để cập nhật." });

    var updates = dto.Items.Select(i => (i.ItemId, i.ActualQuantity, i.ToLocation, i.Note)).ToList();
    var (ok, msg) = await svc.UpdateStockAdjItemsAsync(id, updates);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/stockadjs/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteStockAdjAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Bản tin kỹ thuật & Chiến dịch triệu hồi xe (Btl_Bulletin, Btl_BulletinDtl, Btl_Bulletin_VIN)
app.MapGet("/api/bulletins", async (BulletinStatus? status, string? q, bool? activeOnly, IRoService svc) =>
{
    var list = await svc.BulletinsAsync(status, q, activeOnly);
    return Results.Ok(list.Select(b => new
    {
        b.Id,
        b.BulletinNo,
        b.BulletinNoHMC,
        b.Title,
        b.Remark,
        b.Solution,
        b.CreateDate,
        b.DateExpired,
        b.IsActive,
        b.IsExpired,
        status = Ui.BulletinStatus(b.Status).text,
        statusCode = Ui.BulletinStatus(b.Status).code,
        statusValue = (int)b.Status,
        b.UserCreate,
        b.FileNameAttachment,
        b.TotalVinCount,
        b.CompletedVinCount,
        b.PendingVinCount,
        b.CompletionRate,
        b.CreatedAt
    }));
});

app.MapGet("/api/bulletins/{id:int}", async (int id, IRoService svc) =>
{
    var b = await svc.GetBulletinAsync(id);
    if (b == null) return Results.NotFound(new { error = "Không tìm thấy bản tin kỹ thuật." });
    return Results.Ok(new
    {
        b.Id,
        b.BulletinNo,
        b.BulletinNoHMC,
        b.Title,
        b.Remark,
        b.Solution,
        b.CreateDate,
        b.DateExpired,
        b.IsActive,
        b.IsExpired,
        status = Ui.BulletinStatus(b.Status).text,
        statusCode = Ui.BulletinStatus(b.Status).code,
        statusValue = (int)b.Status,
        b.UserCreate,
        b.FileNameAttachment,
        b.TotalVinCount,
        b.CompletedVinCount,
        b.PendingVinCount,
        b.CompletionRate,
        items = b.Items.Select(i => new
        {
            i.Id,
            type = Ui.Line(i.Type),
            typeValue = (int)i.Type,
            i.Code,
            i.Name,
            i.Unit,
            i.Quantity,
            i.UnitPrice,
            i.Amount,
            i.PartId,
            partCode = i.Part?.Code,
            i.Note
        }),
        targetVins = b.TargetVins.Select(v => new
        {
            v.Id,
            v.VinNo,
            v.PlateNo,
            v.Model,
            v.DealerCode,
            status = Ui.BulletinVinStatus(v.Status).text,
            statusCode = Ui.BulletinVinStatus(v.Status).code,
            statusValue = (int)v.Status,
            v.DateDone,
            v.DoneBy,
            v.ROId,
            v.RONo,
            v.Note
        }),
        appliedROs = b.AppliedROs.Select(r => new
        {
            r.Id,
            r.Code,
            plate = r.Car?.Plate,
            model = r.Car?.Model,
            status = Ui.Status(r.Status).text,
            r.Total
        })
    });
});

app.MapPost("/api/bulletins", async (CreateBulletinDto dto, IRoService svc) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return Results.BadRequest(new { error = "Vui lòng nhập tiêu đề Bản tin kỹ thuật." });

        var bulletin = new Bulletin
        {
            BulletinNo = dto.BulletinNo?.Trim() ?? "",
            BulletinNoHMC = dto.BulletinNoHMC?.Trim(),
            Title = dto.Title.Trim(),
            Remark = dto.Remark?.Trim(),
            Solution = dto.Solution?.Trim(),
            CreateDate = dto.CreateDate ?? DateTime.Today,
            DateExpired = dto.DateExpired,
            UserCreate = dto.UserCreate?.Trim() ?? "Hyundai Thành Công (HTC)",
            FileNameAttachment = dto.FileNameAttachment?.Trim(),
            IsActive = true,
            Status = BulletinStatus.Active,
            CreatedBy = "api"
        };

        var details = (dto.Items ?? []).Select(i => new BulletinDetail
        {
            Type = i.Type,
            PartId = i.PartId,
            Code = i.Code?.Trim() ?? "",
            Name = i.Name?.Trim() ?? "",
            Unit = i.Unit?.Trim() ?? "Cái",
            Quantity = i.Quantity <= 0 ? 1 : i.Quantity,
            UnitPrice = i.UnitPrice ?? 0,
            Note = i.Note?.Trim()
        }).ToList();

        var vins = (dto.TargetVins ?? []).Select(v => new BulletinVin
        {
            VinNo = v.VinNo.Trim().ToUpperInvariant(),
            PlateNo = v.PlateNo?.Trim(),
            Model = v.Model?.Trim(),
            DealerCode = v.DealerCode?.Trim() ?? "HYUNDAI-MAIN",
            Status = BulletinVinStatus.Pending,
            Note = v.Note?.Trim()
        }).ToList();

        var id = await svc.CreateBulletinAsync(bulletin, details, vins);
        return Results.Ok(new { bulletinId = id, bulletinNo = bulletin.BulletinNo, message = "Đã tạo bản tin kỹ thuật thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/bulletins/check-vin/{vin}", async (string vin, IRoService svc) =>
{
    var list = await svc.CheckVinBulletinsAsync(vin);
    return Results.Ok(list.Select(v => new
    {
        v.Id,
        v.BulletinId,
        bulletinNo = v.Bulletin.BulletinNo,
        bulletinNoHMC = v.Bulletin.BulletinNoHMC,
        title = v.Bulletin.Title,
        remark = v.Bulletin.Remark,
        solution = v.Bulletin.Solution,
        createDate = v.Bulletin.CreateDate,
        dateExpired = v.Bulletin.DateExpired,
        isExpired = v.Bulletin.IsExpired,
        v.VinNo,
        v.PlateNo,
        v.Model,
        status = Ui.BulletinVinStatus(v.Status).text,
        statusCode = Ui.BulletinVinStatus(v.Status).code,
        statusValue = (int)v.Status,
        v.DateDone,
        v.RONo,
        items = v.Bulletin.Items.Select(i => new
        {
            type = Ui.Line(i.Type),
            i.Code,
            i.Name,
            i.Quantity,
            i.UnitPrice
        })
    }));
});

app.MapPost("/api/bulletins/{id:int}/vins", async (int id, AddVinsDto dto, IRoService svc) =>
{
    if (dto.VinList == null || dto.VinList.Count == 0)
        return Results.BadRequest(new { error = "Vui lòng cung cấp danh sách số VIN." });

    var (ok, msg) = await svc.AddVinsToBulletinAsync(id, dto.VinList, dto.Model, dto.DealerCode);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/bulletins/vins/{vinId:int}/status", async (int vinId, UpdateBulletinVinStatusDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.UpdateBulletinVinStatusAsync(vinId, dto.Status, dto.DoneBy, dto.ROId, dto.RONo);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/bulletins/{id:int}/apply-to-ro", async (int id, ApplyBulletinToRoDto dto, IRoService svc) =>
{
    var (ok, msg, itemsAdded) = await svc.ApplyBulletinToROAsync(id, dto.RoId);
    return ok ? Results.Ok(new { message = msg, itemsAdded }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/bulletins/{id:int}/toggle-active", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.ToggleBulletinActiveAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/bulletins/{id:int}/transition", async (int id, TransitionBulletinDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.TransitionBulletinStatusAsync(id, dto.ToStatus);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/bulletins/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteBulletinAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Kiểm tra & Nghiệm thu xuất xưởng xe PDI (Dlr_PDIRequest & Dlr_PDIRequestDtl)
app.MapGet("/api/pdi-requests", async (PdiRequestStatus? status, string? q, DateTime? fromDate, DateTime? toDate, IRoService svc) =>
{
    var list = await svc.PdiRequestsAsync(status, q, fromDate, toDate);
    return Results.Ok(list.Select(p => new
    {
        p.Id,
        p.PdiReqNo,
        p.DealerCode,
        p.CreatedDate,
        p.ApprovedDate,
        p.Remark,
        p.FlagAccessory,
        p.CreatedBy,
        p.ApprovedBy,
        status = Ui.PdiRequestStatus(p.Status).text,
        statusCode = Ui.PdiRequestStatus(p.Status).code,
        statusValue = (int)p.Status,
        p.VinTotal,
        p.VinFTotal,
        p.VinPendingTotal,
        p.CompletionRate,
        p.IsAllPassed,
        p.CreatedAt,
        p.FinishedAt,
        items = p.Items.Select(i => new
        {
            i.Id,
            i.VIN,
            i.Model,
            i.Spec,
            i.Color,
            i.ContractNo,
            i.CustomerName,
            i.CustomerPhone,
            i.ExpectedDeliveryDate,
            i.FlagAccessory,
            i.AccessoryNote,
            status = Ui.PdiItemStatus(i.Status).text,
            statusCode = Ui.PdiItemStatus(i.Status).code,
            statusValue = (int)i.Status,
            i.Inspector,
            i.InspectionDate,
            i.PassedDate,
            i.ROId,
            i.RONo,
            roStatus = i.RO != null ? Ui.Status(i.RO.Status).text : null,
            i.TotalChecklistCount,
            i.PassedChecklistCount,
            i.IssueChecklistCount,
            i.IsReadyForDelivery
        })
    }));
});

app.MapGet("/api/pdi-requests/{id:int}", async (int id, IRoService svc) =>
{
    var p = await svc.GetPdiRequestAsync(id);
    if (p == null) return Results.NotFound(new { error = "Không tìm thấy phiếu yêu cầu PDI." });
    return Results.Ok(new
    {
        p.Id,
        p.PdiReqNo,
        p.DealerCode,
        p.CreatedDate,
        p.ApprovedDate,
        p.Remark,
        p.FlagAccessory,
        p.CreatedBy,
        p.ApprovedBy,
        status = Ui.PdiRequestStatus(p.Status).text,
        statusCode = Ui.PdiRequestStatus(p.Status).code,
        statusValue = (int)p.Status,
        p.VinTotal,
        p.VinFTotal,
        p.VinPendingTotal,
        p.CompletionRate,
        p.IsAllPassed,
        p.CreatedAt,
        p.FinishedAt,
        items = p.Items.Select(i => new
        {
            i.Id,
            i.VIN,
            i.Model,
            i.Spec,
            i.Color,
            i.EngineNo,
            i.BatteryNo,
            i.ContractNo,
            i.CustomerName,
            i.CustomerPhone,
            i.CustomerAddress,
            i.ExpectedDeliveryDate,
            i.FlagAccessory,
            i.AccessoryNote,
            status = Ui.PdiItemStatus(i.Status).text,
            statusCode = Ui.PdiItemStatus(i.Status).code,
            statusValue = (int)i.Status,
            i.Inspector,
            i.InspectionDate,
            i.PassedDate,
            i.InspectionNotes,
            i.ROId,
            i.RONo,
            ro = i.RO != null ? new { i.RO.Id, i.RO.Code, status = Ui.Status(i.RO.Status).text, i.RO.Technician } : null,
            i.TotalChecklistCount,
            i.PassedChecklistCount,
            i.IssueChecklistCount,
            i.IsReadyForDelivery,
            checklist = i.ChecklistItems.Select(c => new
            {
                c.Id,
                c.Group,
                c.Code,
                c.Name,
                status = Ui.AuditStatus(c.Status).text,
                statusCss = Ui.AuditStatus(c.Status).css,
                statusValue = (int)c.Status,
                c.Note
            })
        })
    });
});

app.MapGet("/api/pdi-requests/items/{itemId:int}", async (int itemId, IRoService svc) =>
{
    var i = await svc.GetPdiRequestItemAsync(itemId);
    if (i == null) return Results.NotFound(new { error = "Không tìm thấy xe PDI." });
    return Results.Ok(new
    {
        i.Id,
        pdiRequestId = i.PdiRequestId,
        pdiReqNo = i.PdiRequest?.PdiReqNo,
        i.VIN,
        i.Model,
        i.Spec,
        i.Color,
        i.EngineNo,
        i.BatteryNo,
        i.ContractNo,
        i.CustomerName,
        i.CustomerPhone,
        i.CustomerAddress,
        i.ExpectedDeliveryDate,
        i.FlagAccessory,
        i.AccessoryNote,
        status = Ui.PdiItemStatus(i.Status).text,
        statusCode = Ui.PdiItemStatus(i.Status).code,
        statusValue = (int)i.Status,
        i.Inspector,
        i.InspectionDate,
        i.PassedDate,
        i.InspectionNotes,
        i.ROId,
        i.RONo,
        i.TotalChecklistCount,
        i.PassedChecklistCount,
        i.IssueChecklistCount,
        i.IsReadyForDelivery,
        checklist = i.ChecklistItems.Select(c => new
        {
            c.Id,
            c.Group,
            c.Code,
            c.Name,
            status = Ui.AuditStatus(c.Status).text,
            statusCss = Ui.AuditStatus(c.Status).css,
            statusValue = (int)c.Status,
            c.Note
        })
    });
});

app.MapPost("/api/pdi-requests", async (CreatePdiRequestDto dto, IRoService svc) =>
{
    try
    {
        if (dto.Items == null || dto.Items.Count == 0)
            return Results.BadRequest(new { error = "Vui lòng nhập danh sách xe cần kiểm tra PDI." });

        var req = new PdiRequest
        {
            PdiReqNo = dto.PdiReqNo?.Trim() ?? "",
            DealerCode = string.IsNullOrWhiteSpace(dto.DealerCode) ? "HYUNDAI-MAIN" : dto.DealerCode.Trim(),
            CreatedDate = dto.CreatedDate ?? DateTime.Today,
            Remark = dto.Remark?.Trim(),
            CreatedBy = dto.CreatedBy ?? "api",
            FlagAccessory = dto.FlagAccessory ?? false,
            Status = PdiRequestStatus.Pending
        };

        var items = dto.Items.Select(x => new PdiRequestItem
        {
            VIN = x.VIN.Trim().ToUpperInvariant(),
            Model = string.IsNullOrWhiteSpace(x.Model) ? "Hyundai" : x.Model.Trim(),
            Spec = x.Spec?.Trim(),
            Color = x.Color?.Trim(),
            ContractNo = x.ContractNo?.Trim() ?? "",
            CustomerName = x.CustomerName?.Trim() ?? "",
            CustomerPhone = x.CustomerPhone?.Trim(),
            CustomerAddress = x.CustomerAddress?.Trim(),
            ExpectedDeliveryDate = x.ExpectedDeliveryDate ?? DateTime.Today.AddDays(2),
            FlagAccessory = x.FlagAccessory ?? req.FlagAccessory,
            AccessoryNote = x.AccessoryNote?.Trim()
        }).ToList();

        var id = await svc.CreatePdiRequestAsync(req, items);
        return Results.Ok(new { pdiRequestId = id, pdiReqNo = req.PdiReqNo, message = $"Đã tạo phiếu yêu cầu PDI thành công ({items.Count} xe)." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/pdi-requests/{id:int}/transition", async (int id, TransitionPdiDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.TransitionPdiRequestStatusAsync(id, dto.ToStatus, dto.ApprovedBy);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/pdi-requests/items/{itemId:int}/create-ro", async (int itemId, CreatePdiRoDto? dto, IRoService svc) =>
{
    var (ok, msg, roId) = await svc.CreateROFromPdiItemAsync(itemId, dto?.Technician);
    return ok ? Results.Ok(new { message = msg, roId }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/pdi-requests/items/{itemId:int}/checklist", async (int itemId, UpdatePdiChecklistDto dto, IRoService svc) =>
{
    var updates = dto.Items?.Select(i => (i.CheckId, i.Status, i.Note)).ToList() ?? [];
    var (ok, msg) = await svc.UpdatePdiItemChecklistAsync(itemId, updates, dto.Inspector, dto.Notes);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/pdi-requests/items/{itemId:int}/pass", async (int itemId, PassPdiDto? dto, IRoService svc) =>
{
    var (ok, msg) = await svc.PassPdiItemAsync(itemId, dto?.Inspector);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/pdi-requests/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeletePdiRequestAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Khiếu nại đơn hàng phụ tùng Nhà Cung Cấp TST/HTC (Ser_OrderComplain & Ser_OrderComplainAttachFile)
app.MapGet("/api/ordercomplains", async (DMSOrderComplainStatus? dmsStatus, TSTOrderComplainStatus? tstStatus, OrderComplainType? type, string? q, DateTime? fromDate, DateTime? toDate, IRoService svc) =>
{
    var list = await svc.OrderComplainsAsync(dmsStatus, tstStatus, type, q, fromDate, toDate);
    return Results.Ok(list.Select(c => new
    {
        c.Id,
        c.OrderComplainNo,
        c.DealerCode,
        c.DealerName,
        type = Ui.OrderComplainType(c.ComplainType).text,
        typeIcon = Ui.OrderComplainType(c.ComplainType).icon,
        typeValue = (int)c.ComplainType,
        c.OrderPartId,
        c.OrderPartNo,
        c.PartId,
        c.PartCode,
        c.PartName,
        c.Unit,
        c.Quantity,
        c.UnitPrice,
        c.Amount,
        c.VIN,
        c.Description,
        c.RequestOrderNo,
        c.TransportUnit,
        c.DeliveryDateTime,
        c.DeliveryBy,
        c.DeliveryLocation,
        c.ReceiveBy,
        c.AssembleDateTime,
        c.AssembleBy,
        dmsStatus = Ui.DMSOrderComplainStatus(c.DMSStatus).text,
        dmsStatusCode = Ui.DMSOrderComplainStatus(c.DMSStatus).code,
        dmsStatusValue = (int)c.DMSStatus,
        tstStatus = Ui.TSTOrderComplainStatus(c.TSTStatus).text,
        tstStatusCode = Ui.TSTOrderComplainStatus(c.TSTStatus).code,
        tstStatusValue = (int)c.TSTStatus,
        solution = c.TSTSolution.HasValue ? Ui.ComplainSolution(c.TSTSolution.Value).text : null,
        solutionValue = c.TSTSolution.HasValue ? (int)c.TSTSolution.Value : (int?)null,
        c.SolutionNote,
        c.AttachCount,
        c.CreatedBy,
        c.CreatedAt,
        c.SentAt,
        c.DecidedAt,
        c.FinishedAt
    }));
});

app.MapGet("/api/ordercomplains/{id:int}", async (int id, IRoService svc) =>
{
    var c = await svc.GetOrderComplainAsync(id);
    if (c == null) return Results.NotFound(new { error = "Không tìm thấy hồ sơ khiếu nại phụ tùng." });
    return Results.Ok(new
    {
        c.Id,
        c.OrderComplainNo,
        c.DealerCode,
        c.DealerName,
        type = Ui.OrderComplainType(c.ComplainType).text,
        typeIcon = Ui.OrderComplainType(c.ComplainType).icon,
        typeValue = (int)c.ComplainType,
        orderPart = c.OrderPart != null ? new { c.OrderPart.Id, c.OrderPart.OrderPartNo, c.OrderPart.SupplierName, c.OrderPart.OrderDate } : null,
        part = new { c.Part.Id, c.Part.Code, c.Part.Name, c.Part.Unit, c.Part.CostPrice, c.Part.InStock },
        c.Quantity,
        c.UnitPrice,
        c.Amount,
        c.VIN,
        c.Description,
        c.RequestOrderNo,
        c.TransportUnit,
        c.DeliveryDateTime,
        c.DeliveryBy,
        c.DeliveryLocation,
        c.ReceiveBy,
        c.AssembleDateTime,
        c.AssembleBy,
        dmsStatus = Ui.DMSOrderComplainStatus(c.DMSStatus).text,
        dmsStatusCode = Ui.DMSOrderComplainStatus(c.DMSStatus).code,
        dmsStatusValue = (int)c.DMSStatus,
        tstStatus = Ui.TSTOrderComplainStatus(c.TSTStatus).text,
        tstStatusCode = Ui.TSTOrderComplainStatus(c.TSTStatus).code,
        tstStatusValue = (int)c.TSTStatus,
        solution = c.TSTSolution.HasValue ? Ui.ComplainSolution(c.TSTSolution.Value).text : null,
        solutionValue = c.TSTSolution.HasValue ? (int)c.TSTSolution.Value : (int?)null,
        c.SolutionNote,
        c.CreatedBy,
        c.CreatedAt,
        c.SentAt,
        c.DecidedAt,
        c.FinishedAt,
        attachFiles = c.AttachFiles.Select(f => new
        {
            f.Id,
            f.ImageType,
            f.FileName,
            f.FilePath,
            f.Note,
            f.UploadedAt
        })
    });
});

app.MapPost("/api/ordercomplains", async (CreateOrderComplainDto dto, IRoService svc) =>
{
    try
    {
        if (dto.PartId <= 0)
            return Results.BadRequest(new { error = "Vui lòng chọn phụ tùng khiếu nại (PartId)." });

        if (dto.Quantity <= 0)
            return Results.BadRequest(new { error = "Số lượng khiếu nại phải lớn hơn 0." });

        if (string.IsNullOrWhiteSpace(dto.Description))
            return Results.BadRequest(new { error = "Vui lòng nhập mô tả chi tiết tình trạng hư hỏng / sai quy cách (Description)." });

        var complain = new OrderComplain
        {
            OrderComplainNo = dto.OrderComplainNo?.Trim() ?? "",
            DealerCode = string.IsNullOrWhiteSpace(dto.DealerCode) ? "HYUNDAI-MAIN" : dto.DealerCode.Trim(),
            DealerName = string.IsNullOrWhiteSpace(dto.DealerName) ? "Hyundai Giải Phóng" : dto.DealerName.Trim(),
            ComplainType = dto.ComplainType,
            OrderPartId = (dto.OrderPartId.HasValue && dto.OrderPartId.Value > 0) ? dto.OrderPartId : null,
            PartId = dto.PartId,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice ?? 0,
            VIN = dto.VIN?.Trim(),
            Description = dto.Description.Trim(),
            RequestOrderNo = dto.RequestOrderNo?.Trim(),
            TransportUnit = dto.TransportUnit?.Trim(),
            DeliveryDateTime = dto.DeliveryDateTime,
            DeliveryBy = dto.DeliveryBy?.Trim(),
            DeliveryLocation = string.IsNullOrWhiteSpace(dto.DeliveryLocation) ? "Kho phụ tùng chính" : dto.DeliveryLocation.Trim(),
            ReceiveBy = dto.ReceiveBy?.Trim(),
            AssembleDateTime = dto.AssembleDateTime,
            AssembleBy = dto.AssembleBy?.Trim(),
            CreatedBy = dto.CreatedBy ?? "Thủ kho"
        };

        var files = dto.AttachFiles?.Select(f => new OrderComplainAttachFile
        {
            ImageType = string.IsNullOrWhiteSpace(f.ImageType) ? "Ngoại quan hư hại" : f.ImageType.Trim(),
            FileName = f.FileName.Trim(),
            FilePath = string.IsNullOrWhiteSpace(f.FilePath) ? $"/uploads/complain/{f.FileName.Trim()}" : f.FilePath.Trim(),
            Note = f.Note?.Trim()
        }).ToList();

        var id = await svc.CreateOrderComplainAsync(complain, files);
        return Results.Ok(new { complainId = id, orderComplainNo = complain.OrderComplainNo, message = "Đã lập hồ sơ khiếu nại phụ tùng thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/ordercomplains/{id:int}/send-tst", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.SendOrderComplainToTSTAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/ordercomplains/{id:int}/review", async (int id, ReviewOrderComplainDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.ReviewOrderComplainAsync(id, dto.TSTStatus, dto.Solution, dto.SolutionNote);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/ordercomplains/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteOrderComplainAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Thư viện Kỹ thuật & Cẩm nang xử lý pan bệnh sửa chữa xe (Ser_Technical_Library / MH 63)
app.MapGet("/api/technicallibraries", async (string? model, TechnicalLibraryReRepairType? reRepairType, TechnicalLibraryType? type, bool? isActive, string? q, IRoService svc) =>
{
    var list = await svc.TechnicalLibrariesAsync(model, reRepairType, type, isActive, q);
    return Results.Ok(list.Select(t => new
    {
        t.Id,
        t.TechnicalLibraryCode,
        t.DealerCode,
        t.DealerName,
        t.PlateNo,
        t.Model,
        t.Engine,
        t.Gear,
        t.Version,
        type = Ui.TechnicalLibraryType(t.Type).text,
        typeCode = Ui.TechnicalLibraryType(t.Type).code,
        typeValue = (int)t.Type,
        reRepairType = Ui.TechnicalLibraryReRepairType(t.ReRepairType).text,
        reRepairTypeIcon = Ui.TechnicalLibraryReRepairType(t.ReRepairType).icon,
        reRepairTypeValue = (int)t.ReRepairType,
        t.ReRepairRemark,
        t.ReRepairFeedback,
        t.ExclusionTest,
        t.ReRepairReason,
        t.ReRepairSolution,
        status = Ui.TechnicalLibraryStatus(t.IsActive).text,
        statusCode = Ui.TechnicalLibraryStatus(t.IsActive).code,
        t.IsActive,
        roId = t.ROId,
        roCode = t.RO?.Code,
        t.CreatedBy,
        t.CreatedAt,
        t.ApprovedAt,
        t.ApprovedBy
    }));
});

app.MapGet("/api/technicallibraries/{id:int}", async (int id, IRoService svc) =>
{
    var t = await svc.GetTechnicalLibraryAsync(id);
    if (t == null) return Results.NotFound(new { error = "Không tìm thấy hồ sơ kỹ thuật." });
    return Results.Ok(new
    {
        t.Id,
        t.TechnicalLibraryCode,
        t.DealerCode,
        t.DealerName,
        t.PlateNo,
        t.Model,
        t.Engine,
        t.Gear,
        t.Version,
        type = Ui.TechnicalLibraryType(t.Type).text,
        typeCode = Ui.TechnicalLibraryType(t.Type).code,
        typeValue = (int)t.Type,
        reRepairType = Ui.TechnicalLibraryReRepairType(t.ReRepairType).text,
        reRepairTypeIcon = Ui.TechnicalLibraryReRepairType(t.ReRepairType).icon,
        reRepairTypeValue = (int)t.ReRepairType,
        t.ReRepairRemark,
        t.ReRepairFeedback,
        t.ExclusionTest,
        t.ReRepairReason,
        t.ReRepairSolution,
        status = Ui.TechnicalLibraryStatus(t.IsActive).text,
        statusCode = Ui.TechnicalLibraryStatus(t.IsActive).code,
        t.IsActive,
        ro = t.RO != null ? new { t.RO.Id, t.RO.Code, plate = t.RO.Car?.Plate, model = t.RO.Car?.Model, customer = t.RO.Customer?.Name } : null,
        t.CreatedBy,
        t.CreatedAt,
        t.ApprovedAt,
        t.ApprovedBy
    });
});

app.MapGet("/api/technicallibraries/by-code/{code}", async (string code, IRoService svc) =>
{
    var t = await svc.GetTechnicalLibraryByCodeAsync(code);
    if (t == null) return Results.NotFound(new { error = "Không tìm thấy mã hồ sơ kỹ thuật." });
    return Results.Ok(new
    {
        t.Id,
        t.TechnicalLibraryCode,
        t.DealerCode,
        t.DealerName,
        t.PlateNo,
        t.Model,
        t.Engine,
        t.Gear,
        t.Version,
        type = Ui.TechnicalLibraryType(t.Type).text,
        reRepairType = Ui.TechnicalLibraryReRepairType(t.ReRepairType).text,
        t.ReRepairRemark,
        t.ReRepairFeedback,
        t.ExclusionTest,
        t.ReRepairReason,
        t.ReRepairSolution,
        t.IsActive,
        t.CreatedBy,
        t.CreatedAt
    });
});

app.MapPost("/api/technicallibraries", async (CreateTechnicalLibraryDto dto, IRoService svc) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(dto.Model))
            return Results.BadRequest(new { error = "Vui lòng nhập dòng xe (Model)." });
        if (string.IsNullOrWhiteSpace(dto.ReRepairRemark))
            return Results.BadRequest(new { error = "Vui lòng nhập mô tả hiện tượng hư hỏng (ReRepairRemark)." });
        if (string.IsNullOrWhiteSpace(dto.ReRepairReason))
            return Results.BadRequest(new { error = "Vui lòng nhập nguyên nhân gốc rễ (ReRepairReason)." });
        if (string.IsNullOrWhiteSpace(dto.ReRepairSolution))
            return Results.BadRequest(new { error = "Vui lòng nhập biện pháp khắc phục (ReRepairSolution)." });

        var item = new TechnicalLibrary
        {
            TechnicalLibraryCode = dto.TechnicalLibraryCode?.Trim() ?? "",
            DealerCode = string.IsNullOrWhiteSpace(dto.DealerCode) ? "HYUNDAI-MAIN" : dto.DealerCode.Trim(),
            DealerName = string.IsNullOrWhiteSpace(dto.DealerName) ? "Hyundai Giải Phóng" : dto.DealerName.Trim(),
            PlateNo = dto.PlateNo?.Trim(),
            Model = dto.Model.Trim(),
            Engine = dto.Engine?.Trim(),
            Gear = dto.Gear?.Trim(),
            Version = dto.Version?.Trim(),
            ReRepairType = dto.ReRepairType,
            Type = dto.Type ?? TechnicalLibraryType.Normal,
            ReRepairRemark = dto.ReRepairRemark.Trim(),
            ReRepairFeedback = dto.ReRepairFeedback?.Trim(),
            ExclusionTest = dto.ExclusionTest?.Trim(),
            ReRepairReason = dto.ReRepairReason.Trim(),
            ReRepairSolution = dto.ReRepairSolution.Trim(),
            ROId = (dto.RoId.HasValue && dto.RoId.Value > 0) ? dto.RoId : null,
            CreatedBy = dto.CreatedBy ?? "api",
            IsActive = false
        };

        var id = await svc.CreateTechnicalLibraryAsync(item);
        return Results.Ok(new { technicalLibraryId = id, technicalLibraryCode = item.TechnicalLibraryCode, message = "Đã lưu hồ sơ cẩm nang kỹ thuật thành công (chờ thẩm định HQ)." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/technicallibraries/{id:int}/approve", async (int id, ApproveTechnicalLibraryDto? dto, IRoService svc) =>
{
    var (ok, msg) = await svc.ApproveTechnicalLibraryAsync(id, dto?.ApprovedBy);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/technicallibraries/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteTechnicalLibraryAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapGet("/api/technicallibraries/suggest-for-ro/{roId:int}", async (int roId, IRoService svc) =>
{
    var list = await svc.SearchSolutionsForRoAsync(roId);
    return Results.Ok(list.Select(t => new
    {
        t.Id,
        t.TechnicalLibraryCode,
        t.Model,
        type = Ui.TechnicalLibraryType(t.Type).text,
        reRepairType = Ui.TechnicalLibraryReRepairType(t.ReRepairType).text,
        t.ReRepairRemark,
        t.ReRepairReason,
        t.ReRepairSolution
    }));
});

// Master Services & Flat Rate Labor Operations (Ser_MST_Service)
app.MapGet("/api/services", async (ServiceROType? roType, string? model, bool? isActive, bool? flagWarranty, string? q, IRoService svc) =>
{
    var list = await svc.ServiceItemsAsync(roType, model, isActive, flagWarranty, q);
    return Results.Ok(list.Select(s => new
    {
        s.Id,
        s.Code,
        s.Name,
        roType = Ui.ServiceROType(s.ROType).text,
        roTypeCode = Ui.ServiceROType(s.ROType).code,
        s.StdManHour,
        s.Price,
        s.Cost,
        s.VatPercent,
        s.TotalWithVat,
        s.GrossProfit,
        s.GrossMargin,
        s.Model,
        s.FlagWarranty,
        s.Note,
        s.IsActive,
        repairLineCount = s.RepairLines.Count
    }));
});

app.MapGet("/api/services/{id:int}", async (int id, IRoService svc) =>
{
    var s = await svc.GetServiceItemAsync(id);
    if (s == null) return Results.NotFound(new { error = "Không tìm thấy công việc dịch vụ." });
    return Results.Ok(new
    {
        s.Id,
        s.Code,
        s.Name,
        roType = Ui.ServiceROType(s.ROType).text,
        roTypeCode = Ui.ServiceROType(s.ROType).code,
        s.StdManHour,
        s.Price,
        s.Cost,
        s.VatPercent,
        s.TotalWithVat,
        s.GrossProfit,
        s.GrossMargin,
        s.Model,
        s.FlagWarranty,
        s.Note,
        s.IsActive,
        s.CreatedAt,
        repairLines = s.RepairLines.Select(l => new
        {
            l.Id,
            roId = l.ROId,
            roCode = l.RO?.Code,
            plate = l.RO?.Car?.Plate,
            model = l.RO?.Car?.Model,
            expenseType = Ui.Expense(l.ExpenseType).text,
            l.Quantity,
            l.UnitPrice,
            l.Amount
        })
    });
});

app.MapGet("/api/services/by-code/{code}", async (string code, IRoService svc) =>
{
    var s = await svc.GetServiceItemByCodeAsync(code);
    if (s == null) return Results.NotFound(new { error = "Không tìm thấy mã công việc dịch vụ." });
    return Results.Ok(new
    {
        s.Id,
        s.Code,
        s.Name,
        roType = Ui.ServiceROType(s.ROType).text,
        s.StdManHour,
        s.Price,
        s.Cost,
        s.TotalWithVat,
        s.Model,
        s.FlagWarranty
    });
});

app.MapPost("/api/services", async (CreateServiceItemDto dto, IRoService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
        return Results.BadRequest(new { error = "Vui lòng nhập đầy đủ Code và Name." });

    var item = new ServiceItem
    {
        Code = dto.Code.Trim().ToUpperInvariant(),
        Name = dto.Name.Trim(),
        ROType = dto.ROType,
        StdManHour = dto.StdManHour > 0 ? dto.StdManHour : 1.0m,
        Price = dto.Price >= 0 ? dto.Price : 0,
        Cost = dto.Cost >= 0 ? dto.Cost : 0,
        VatPercent = dto.VatPercent >= 0 ? dto.VatPercent : 8,
        Model = string.IsNullOrWhiteSpace(dto.Model) ? null : dto.Model.Trim(),
        FlagWarranty = dto.FlagWarranty ?? false,
        Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim(),
        IsActive = dto.IsActive ?? true
    };

    try
    {
        var id = await svc.CreateServiceItemAsync(item);
        return Results.Created($"/api/services/{id}", new { id, item.Code, item.Name, message = "Đã tạo công việc dịch vụ chuẩn." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/services/{id:int}", async (int id, UpdateServiceItemDto dto, IRoService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.Name))
        return Results.BadRequest(new { error = "Tên công việc không được để trống." });

    var item = new ServiceItem
    {
        Id = id,
        Name = dto.Name.Trim(),
        ROType = dto.ROType,
        StdManHour = dto.StdManHour > 0 ? dto.StdManHour : 1.0m,
        Price = dto.Price >= 0 ? dto.Price : 0,
        Cost = dto.Cost >= 0 ? dto.Cost : 0,
        VatPercent = dto.VatPercent >= 0 ? dto.VatPercent : 8,
        Model = string.IsNullOrWhiteSpace(dto.Model) ? null : dto.Model.Trim(),
        FlagWarranty = dto.FlagWarranty,
        IsActive = dto.IsActive,
        Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim()
    };

    var (ok, msg) = await svc.UpdateServiceItemAsync(item);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/services/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteServiceItemAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/services/{id:int}/apply-to-ro", async (int id, ApplyServiceToRoDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.AddServiceItemToROAsync(dto.RoId, id, dto.ExpenseType ?? ExpenseType.Customer, dto.CustomHours, dto.CustomPrice, dto.Note);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// Car Model Master (Ser_Mst_Model / Mst_CarModelStd)
app.MapGet("/api/car-models", async (string? tradeMarkCode, CarModelSegment? segment, bool? isActive, string? q, IRoService svc) =>
{
    var list = await svc.CarModelsAsync(tradeMarkCode, segment, isActive, q);
    return Results.Ok(list.Select(m => new
    {
        m.Id,
        m.ModelCode,
        m.ModelName,
        m.TradeMarkCode,
        m.ProductionCode,
        m.DealerCode,
        segment = Ui.CarModelSegment(m.Segment).text,
        segmentCode = Ui.CarModelSegment(m.Segment).code,
        segmentValue = (int)m.Segment,
        m.ProductYear,
        m.IsActive,
        m.CreatedBy,
        m.CreatedAt,
        m.LogLUBy,
        m.LogLUDateTime
    }));
});

app.MapGet("/api/car-models/summary", async (IRoService svc) =>
{
    var s = await svc.GetCarModelSummaryAsync();
    return Results.Ok(new
    {
        s.TotalModels,
        s.ActiveModels,
        s.InactiveModels,
        s.TradeMarkCount,
        s.SegmentCount
    });
});

app.MapGet("/api/car-models/{id:int}", async (int id, IRoService svc) =>
{
    var m = await svc.GetCarModelAsync(id);
    if (m == null) return Results.NotFound(new { error = "Không tìm thấy dòng xe." });
    return Results.Ok(new
    {
        m.Id,
        m.ModelCode,
        m.ModelName,
        m.TradeMarkCode,
        m.ProductionCode,
        m.DealerCode,
        segment = Ui.CarModelSegment(m.Segment).text,
        segmentCode = Ui.CarModelSegment(m.Segment).code,
        segmentValue = (int)m.Segment,
        m.ProductYear,
        m.IsActive,
        m.CreatedBy,
        m.CreatedAt,
        m.LogLUBy,
        m.LogLUDateTime
    });
});

app.MapGet("/api/car-models/by-code/{code}", async (string code, IRoService svc) =>
{
    var m = await svc.GetCarModelByCodeAsync(code);
    if (m == null) return Results.NotFound(new { error = "Không tìm thấy mã dòng xe." });
    return Results.Ok(new
    {
        m.Id,
        m.ModelCode,
        m.ModelName,
        m.TradeMarkCode,
        segment = Ui.CarModelSegment(m.Segment).text,
        m.IsActive
    });
});

app.MapPost("/api/car-models", async (CreateCarModelDto dto, IRoService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.ModelCode) || string.IsNullOrWhiteSpace(dto.ModelName) || string.IsNullOrWhiteSpace(dto.TradeMarkCode))
        return Results.BadRequest(new { error = "Vui lòng nhập đầy đủ ModelCode, ModelName và TradeMarkCode." });

    var model = new CarModel
    {
        ModelCode = dto.ModelCode.Trim().ToUpperInvariant(),
        ModelName = dto.ModelName.Trim(),
        TradeMarkCode = dto.TradeMarkCode.Trim().ToUpperInvariant(),
        ProductionCode = string.IsNullOrWhiteSpace(dto.ProductionCode) ? null : dto.ProductionCode.Trim(),
        DealerCode = string.IsNullOrWhiteSpace(dto.DealerCode) ? null : dto.DealerCode.Trim(),
        Segment = dto.Segment ?? CarModelSegment.Sedan,
        ProductYear = dto.ProductYear,
        IsActive = dto.IsActive ?? true,
        CreatedBy = dto.CreatedBy ?? "api"
    };

    try
    {
        var id = await svc.CreateCarModelAsync(model);
        return Results.Created($"/api/car-models/{id}", new { id, model.ModelCode, model.ModelName, message = "Đã tạo dòng xe." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/car-models/{id:int}", async (int id, UpdateCarModelDto dto, IRoService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.ModelName))
        return Results.BadRequest(new { error = "Tên dòng xe không được để trống." });

    var model = new CarModel
    {
        Id = id,
        ModelName = dto.ModelName.Trim(),
        TradeMarkCode = dto.TradeMarkCode?.Trim() ?? "",
        ProductionCode = string.IsNullOrWhiteSpace(dto.ProductionCode) ? null : dto.ProductionCode.Trim(),
        DealerCode = string.IsNullOrWhiteSpace(dto.DealerCode) ? null : dto.DealerCode.Trim(),
        Segment = dto.Segment ?? CarModelSegment.Sedan,
        ProductYear = dto.ProductYear,
        IsActive = dto.IsActive ?? true,
        LogLUBy = dto.UpdatedBy ?? "api"
    };

    var (ok, msg) = await svc.UpdateCarModelAsync(model);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/car-models/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteCarModelAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// Bill of Materials — Định mức vật tư tối thiểu (Mst_BOM / Mst_BOMDtl)
app.MapGet("/api/boms", async (bool? isActive, string? q, IRoService svc) =>
{
    var list = await svc.BomsAsync(isActive, q);
    return Results.Ok(list.Select(b => new
    {
        b.Id,
        b.BomCode,
        b.BomDesc,
        b.Remark,
        b.IsActive,
        status = Ui.BomActive(b.IsActive).text,
        lineCount = b.Lines.Count,
        totalQtyMin = b.Lines.Sum(l => l.QtyMin),
        b.CreatedBy,
        b.CreatedAt,
        b.LogLUBy,
        b.LogLUDateTime
    }));
});

app.MapGet("/api/boms/summary", async (IRoService svc) =>
{
    var s = await svc.GetBomSummaryAsync();
    return Results.Ok(new
    {
        s.TotalBoms,
        s.ActiveBoms,
        s.InactiveBoms,
        s.TotalLines,
        s.DistinctParts
    });
});

app.MapGet("/api/boms/{id:int}", async (int id, IRoService svc) =>
{
    var b = await svc.GetBomAsync(id);
    if (b == null) return Results.NotFound(new { error = "Không tìm thấy định mức BOM." });
    return Results.Ok(new
    {
        b.Id,
        b.BomCode,
        b.BomDesc,
        b.Remark,
        b.IsActive,
        status = Ui.BomActive(b.IsActive).text,
        b.CreatedBy,
        b.CreatedAt,
        b.LogLUBy,
        b.LogLUDateTime,
        lines = b.Lines.Select(l => new
        {
            l.Id,
            l.PartCode,
            l.PartName,
            l.Unit,
            l.QtyMin
        })
    });
});

app.MapGet("/api/boms/by-code/{code}", async (string code, IRoService svc) =>
{
    var b = await svc.GetBomByCodeAsync(code);
    if (b == null) return Results.NotFound(new { error = "Không tìm thấy mã BOM." });
    return Results.Ok(new
    {
        b.Id,
        b.BomCode,
        b.BomDesc,
        b.IsActive,
        lineCount = b.Lines.Count
    });
});

app.MapPost("/api/boms", async (CreateBomDto dto, IRoService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.BomCode) || string.IsNullOrWhiteSpace(dto.BomDesc))
        return Results.BadRequest(new { error = "Vui lòng nhập Mã BOM (BomCode) và Diễn giải (BomDesc)." });
    if (dto.Lines == null || dto.Lines.Count == 0)
        return Results.BadRequest(new { error = "Cần danh sách phụ tùng trong định mức (Lines)." });

    var bom = new Bom
    {
        BomCode = dto.BomCode.Trim().ToUpperInvariant(),
        BomDesc = dto.BomDesc.Trim(),
        Remark = string.IsNullOrWhiteSpace(dto.Remark) ? null : dto.Remark.Trim(),
        IsActive = dto.IsActive ?? true,
        CreatedBy = dto.CreatedBy ?? "api"
    };
    var lines = dto.Lines.Select(l => new BomLine
    {
        PartCode = l.PartCode?.Trim() ?? "",
        PartName = l.PartName?.Trim() ?? "",
        Unit = string.IsNullOrWhiteSpace(l.Unit) ? "Cái" : l.Unit.Trim(),
        QtyMin = l.QtyMin <= 0 ? 1m : l.QtyMin
    }).ToList();

    try
    {
        var id = await svc.CreateBomAsync(bom, lines);
        return Results.Created($"/api/boms/{id}", new { id, bom.BomCode, bom.BomDesc, message = "Đã tạo định mức BOM." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/boms/{id:int}", async (int id, UpdateBomDto dto, IRoService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.BomDesc))
        return Results.BadRequest(new { error = "Diễn giải BOM không được để trống." });
    if (dto.Lines == null || dto.Lines.Count == 0)
        return Results.BadRequest(new { error = "Cần danh sách phụ tùng trong định mức (Lines)." });

    var bom = new Bom
    {
        Id = id,
        BomDesc = dto.BomDesc.Trim(),
        Remark = string.IsNullOrWhiteSpace(dto.Remark) ? null : dto.Remark.Trim(),
        IsActive = dto.IsActive ?? true,
        LogLUBy = dto.UpdatedBy ?? "api"
    };
    var lines = dto.Lines.Select(l => new BomLine
    {
        PartCode = l.PartCode?.Trim() ?? "",
        PartName = l.PartName?.Trim() ?? "",
        Unit = string.IsNullOrWhiteSpace(l.Unit) ? "Cái" : l.Unit.Trim(),
        QtyMin = l.QtyMin <= 0 ? 1m : l.QtyMin
    }).ToList();

    var (ok, msg) = await svc.UpdateBomAsync(bom, lines);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/boms/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteBomAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// Warehouse Location — Vị trí kệ/kho phụ tùng (Ser_Mst_Location)
app.MapGet("/api/warehouse-locations", async (string? dealerCode, LocationType? type, bool? isActive, string? q, IRoService svc) =>
{
    var list = await svc.WarehouseLocationsAsync(dealerCode, type, isActive, q);
    return Results.Ok(list.Select(l => new
    {
        l.Id,
        l.LocationCode,
        l.LocationName,
        l.DealerCode,
        l.StockNo,
        type = l.Type.ToString(),
        typeLabel = Ui.LocationTypeLabel(l.Type).text,
        l.Surface,
        l.Height,
        l.IsActive,
        status = Ui.LocationActive(l.IsActive).text,
        l.CreatedBy,
        l.CreatedAt,
        l.LogLUBy,
        l.LogLUDateTime
    }));
});

app.MapGet("/api/warehouse-locations/summary", async (IRoService svc) =>
{
    var s = await svc.GetWarehouseLocationSummaryAsync();
    return Results.Ok(new
    {
        s.TotalLocations,
        s.ActiveLocations,
        s.InactiveLocations,
        s.DealerCount,
        s.StockCount
    });
});

app.MapGet("/api/warehouse-locations/{id:int}", async (int id, IRoService svc) =>
{
    var l = await svc.GetWarehouseLocationAsync(id);
    if (l == null) return Results.NotFound(new { error = "Không tìm thấy vị trí kho." });
    return Results.Ok(new
    {
        l.Id,
        l.LocationCode,
        l.LocationName,
        l.DealerCode,
        l.StockNo,
        type = l.Type.ToString(),
        typeLabel = Ui.LocationTypeLabel(l.Type).text,
        l.Surface,
        l.Height,
        l.IsActive,
        status = Ui.LocationActive(l.IsActive).text,
        l.CreatedBy,
        l.CreatedAt,
        l.LogLUBy,
        l.LogLUDateTime
    });
});

app.MapGet("/api/warehouse-locations/by-code/{code}", async (string code, string? dealerCode, IRoService svc) =>
{
    var l = await svc.GetWarehouseLocationByCodeAsync(code, dealerCode);
    if (l == null) return Results.NotFound(new { error = "Không tìm thấy mã vị trí kho." });
    return Results.Ok(new
    {
        l.Id,
        l.LocationCode,
        l.LocationName,
        l.DealerCode,
        l.StockNo,
        type = l.Type.ToString(),
        l.IsActive
    });
});

app.MapPost("/api/warehouse-locations", async (CreateWarehouseLocationDto dto, IRoService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.LocationCode) || string.IsNullOrWhiteSpace(dto.LocationName) || string.IsNullOrWhiteSpace(dto.DealerCode))
        return Results.BadRequest(new { error = "Vui lòng nhập Mã vị trí (LocationCode), Tên vị trí (LocationName) và Đại lý (DealerCode)." });

    var loc = new WarehouseLocation
    {
        LocationCode = dto.LocationCode.Trim().ToUpperInvariant(),
        LocationName = dto.LocationName.Trim(),
        DealerCode = dto.DealerCode.Trim(),
        StockNo = string.IsNullOrWhiteSpace(dto.StockNo) ? null : dto.StockNo.Trim(),
        Type = dto.Type ?? LocationType.Rack,
        Surface = string.IsNullOrWhiteSpace(dto.Surface) ? null : dto.Surface.Trim(),
        Height = string.IsNullOrWhiteSpace(dto.Height) ? null : dto.Height.Trim(),
        IsActive = dto.IsActive ?? true,
        CreatedBy = dto.CreatedBy ?? "api"
    };

    try
    {
        var id = await svc.CreateWarehouseLocationAsync(loc);
        return Results.Created($"/api/warehouse-locations/{id}", new { id, loc.LocationCode, loc.LocationName, message = "Đã tạo vị trí kho." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/warehouse-locations/{id:int}", async (int id, UpdateWarehouseLocationDto dto, IRoService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.LocationCode) || string.IsNullOrWhiteSpace(dto.LocationName) || string.IsNullOrWhiteSpace(dto.DealerCode))
        return Results.BadRequest(new { error = "Vui lòng nhập Mã vị trí, Tên vị trí và Đại lý." });

    var loc = new WarehouseLocation
    {
        Id = id,
        LocationCode = dto.LocationCode.Trim().ToUpperInvariant(),
        LocationName = dto.LocationName.Trim(),
        DealerCode = dto.DealerCode.Trim(),
        StockNo = string.IsNullOrWhiteSpace(dto.StockNo) ? null : dto.StockNo.Trim(),
        Type = dto.Type ?? LocationType.Rack,
        Surface = string.IsNullOrWhiteSpace(dto.Surface) ? null : dto.Surface.Trim(),
        Height = string.IsNullOrWhiteSpace(dto.Height) ? null : dto.Height.Trim(),
        IsActive = dto.IsActive ?? true,
        LogLUBy = dto.UpdatedBy ?? "api"
    };

    var (ok, msg) = await svc.UpdateWarehouseLocationAsync(loc);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/warehouse-locations/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteWarehouseLocationAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// Working Calendar — Lịch làm việc của đại lý (Mst_Calendar)
app.MapGet("/api/working-calendars", async (string? dealerCode, int? year, int? month, CalendarDayStatus? status, IRoService svc) =>
{
    var list = await svc.WorkingCalendarsAsync(dealerCode, year, month, status);
    return Results.Ok(list.Select(c => new
    {
        c.Id,
        c.CalendarType,
        date = c.Date,
        c.StatusValue,
        status = Ui.CalendarDayStatus((CalendarDayStatus)c.StatusValue).text,
        statusCode = Ui.CalendarDayStatus((CalendarDayStatus)c.StatusValue).code,
        dayOfWeek = c.Date.DayOfWeek.ToString(),
        c.DealerCode,
        c.LogLUBy,
        c.LogLUDateTime
    }));
});

app.MapGet("/api/working-calendars/summary", async (IRoService svc) =>
{
    var s = await svc.GetWorkingCalendarSummaryAsync();
    return Results.Ok(new
    {
        s.TotalDays,
        s.WorkingDays,
        s.DayOffs,
        s.YearCount,
        s.DealerCount
    });
});

app.MapGet("/api/working-calendars/{id:int}", async (int id, IRoService svc) =>
{
    var c = await svc.GetWorkingCalendarAsync(id);
    if (c == null) return Results.NotFound(new { error = "Không tìm thấy ngày trong lịch làm việc." });
    return Results.Ok(new
    {
        c.Id,
        c.CalendarType,
        date = c.Date,
        c.StatusValue,
        status = Ui.CalendarDayStatus((CalendarDayStatus)c.StatusValue).text,
        statusCode = Ui.CalendarDayStatus((CalendarDayStatus)c.StatusValue).code,
        dayOfWeek = c.Date.DayOfWeek.ToString(),
        c.DealerCode,
        c.LogLUBy,
        c.LogLUDateTime
    });
});

app.MapPost("/api/working-calendars/{id:int}/status", async (int id, UpdateCalendarStatusDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.UpdateWorkingCalendarStatusAsync(id, dto.Status, dto.UpdatedBy);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/working-calendars/reset-year", async (ResetCalendarYearDto dto, IRoService svc) =>
{
    var (ok, msg, count) = await svc.ResetWorkingCalendarYearAsync(
        dto.CalendarType ?? "WORKINGDAY", dto.Year,
        dto.Monday, dto.Tuesday, dto.Wednesday, dto.Thursday, dto.Friday, dto.Saturday, dto.Sunday,
        dto.DealerCode, dto.UpdatedBy);
    return ok ? Results.Ok(new { message = msg, count }) : Results.BadRequest(new { error = msg });
});

app.MapGet("/api/working-calendars/date-to-check", async (DateTime? fromDate, int? workingDaysAhead, string? dealerCode, IRoService svc) =>
{
    var (ok, msg, resultDate) = await svc.GetWorkingDateToCheckAsync(
        fromDate ?? DateTime.Today, workingDaysAhead ?? 0, dealerCode);
    return ok ? Results.Ok(new { message = msg, resultDate }) : Results.BadRequest(new { error = msg });
});

// --- Suppliers & Return to Supplier Minimal APIs (Ser_Mst_Supplier, Ser_SupplierPayment) ---
app.MapGet("/api/suppliers", async (string? q, IRoService svc) =>
{
    var list = await svc.SuppliersAsync(q);
    return Results.Ok(list.Select(s => new
    {
        s.Id,
        s.Code,
        s.Name,
        s.Address,
        s.Phone,
        s.Email,
        s.ContactName,
        s.ContactPhone,
        s.TaxCode,
        s.IsActive,
        paymentCount = s.SupplierPayments.Count
    }));
});

app.MapPost("/api/suppliers", async (CreateSupplierDto dto, IRoService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
        return Results.BadRequest(new { error = "Vui lòng nhập đầy đủ Code và Name." });

    var s = new Supplier
    {
        Code = dto.Code.Trim().ToUpperInvariant(),
        Name = dto.Name.Trim(),
        Address = dto.Address?.Trim(),
        Phone = dto.Phone?.Trim(),
        Email = dto.Email?.Trim(),
        ContactName = dto.ContactName?.Trim(),
        ContactPhone = dto.ContactPhone?.Trim(),
        TaxCode = dto.TaxCode?.Trim(),
        IsActive = true
    };

    try
    {
        var id = await svc.CreateSupplierAsync(s);
        return Results.Created($"/api/suppliers/{id}", new { id, s.Code, s.Name, message = "Đã thêm nhà cung cấp mới." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/supplier-payments", async (SupplierPaymentStatus? status, SupplierPaymentType? type, string? q, DateTime? fromDate, DateTime? toDate, IRoService svc) =>
{
    var list = await svc.SupplierPaymentsAsync(status, type, q, fromDate, toDate);
    return Results.Ok(list.Select(p => new
    {
        p.Id,
        p.SupplierPaymentNo,
        p.PaymentDate,
        p.SupplierName,
        supplierCode = p.Supplier?.Code,
        paymentType = Ui.SupplierPaymentType(p.PaymentType).text,
        status = Ui.SupplierPaymentStatus(p.Status).text,
        statusCode = Ui.SupplierPaymentStatus(p.Status).code,
        p.OrderPartNo,
        p.TSTRequestNo,
        p.ItemCount,
        p.TotalQuantity,
        p.SubTotal,
        p.TotalVat,
        p.TotalAmount,
        p.CreatedBy,
        p.CreatedAt,
        p.ApprovedBy,
        p.ApprovedAt
    }));
});

app.MapGet("/api/supplier-payments/{id:int}", async (int id, IRoService svc) =>
{
    var p = await svc.GetSupplierPaymentAsync(id);
    if (p == null) return Results.NotFound(new { error = "Không tìm thấy phiếu xuất trả nhà cung cấp." });
    return Results.Ok(new
    {
        p.Id,
        p.SupplierPaymentNo,
        p.PaymentDate,
        p.SupplierId,
        p.SupplierName,
        p.Address,
        supplier = p.Supplier != null ? new { p.Supplier.Code, p.Supplier.Name, p.Supplier.Phone, p.Supplier.ContactName } : null,
        paymentType = Ui.SupplierPaymentType(p.PaymentType).text,
        status = Ui.SupplierPaymentStatus(p.Status).text,
        statusCode = Ui.SupplierPaymentStatus(p.Status).code,
        p.OrderPartId,
        p.OrderPartNo,
        p.TSTRequestNo,
        p.Description,
        p.CreatedBy,
        p.CreatedAt,
        p.ApprovedBy,
        p.ApprovedAt,
        p.ItemCount,
        p.TotalQuantity,
        p.SubTotal,
        p.TotalVat,
        p.TotalAmount,
        p.CanApprove,
        p.CanCancel,
        items = p.Items.Select(i => new
        {
            i.Id,
            i.PartId,
            partCode = i.Part?.Code,
            partName = i.Part?.Name,
            unit = i.Part?.Unit,
            i.StockInNo,
            i.LocationCode,
            i.QtyPay,
            i.Price,
            i.VatPercent,
            i.SubTotal,
            i.VatAmount,
            i.Amount,
            i.Reason
        })
    });
});

app.MapPost("/api/supplier-payments", async (CreateSupplierPaymentDto dto, IRoService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.SupplierName) && !dto.SupplierId.HasValue)
        return Results.BadRequest(new { error = "Vui lòng chỉ định Nhà cung cấp nhận hàng." });

    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Vui lòng thêm ít nhất một phụ tùng cần xuất trả." });

    var payment = new SupplierPayment
    {
        SupplierPaymentNo = dto.SupplierPaymentNo?.Trim() ?? "",
        SupplierId = dto.SupplierId,
        SupplierName = dto.SupplierName?.Trim() ?? "",
        Address = dto.Address?.Trim(),
        PaymentDate = dto.PaymentDate ?? DateTime.Today,
        PaymentType = dto.PaymentType,
        OrderPartId = dto.OrderPartId,
        OrderPartNo = dto.OrderPartNo?.Trim(),
        TSTRequestNo = dto.TSTRequestNo?.Trim(),
        Description = dto.Description?.Trim(),
        CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "API" : dto.CreatedBy.Trim()
    };

    var items = dto.Items.Select(i => new SupplierPaymentDetail
    {
        PartId = i.PartId,
        QtyPay = i.QtyPay,
        Price = i.Price ?? 0,
        VatPercent = i.VatPercent ?? 10,
        StockInNo = i.StockInNo?.Trim(),
        LocationCode = i.LocationCode?.Trim(),
        Reason = i.Reason?.Trim()
    }).ToList();

    try
    {
        var id = await svc.CreateSupplierPaymentAsync(payment, items);
        return Results.Created($"/api/supplier-payments/{id}", new { id, payment.SupplierPaymentNo, message = "Đã lập phiếu xuất trả NCC thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/supplier-payments/{id:int}/approve", async (int id, ApproveSupplierPaymentDto? dto, IRoService svc) =>
{
    var (ok, msg) = await svc.ApproveSupplierPaymentAsync(id, dto?.ApprovedBy);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/supplier-payments/{id:int}/cancel", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.CancelSupplierPaymentAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Yêu cầu xuất kho phụ tùng / vật tư dịch vụ (Ser_Inv_StockOutOrder - MH 125)
app.MapGet("/api/stockoutorders", async (StockOutOrderStatus? status, StockOutOrderPriority? priority, string? q, DateTime? fromDate, DateTime? toDate, int? roId, IRoService svc) =>
{
    var list = await svc.StockOutOrdersAsync(status, priority, q, fromDate, toDate, roId);
    return Results.Ok(list.Select(o => new
    {
        o.Id,
        o.OrderNo,
        o.OrderDate,
        o.RequestDeliveryTime,
        priority = Ui.StockOutOrderPriority(o.Priority).text,
        priorityCode = (int)o.Priority,
        status = Ui.StockOutOrderStatus(o.Status).text,
        statusCode = Ui.StockOutOrderStatus(o.Status).code,
        statusValue = (int)o.Status,
        roId = o.ROId,
        roCode = o.RO?.Code,
        plate = o.Car?.Plate,
        carModel = o.Car?.Model,
        customerName = o.Customer?.Name,
        cavityName = o.Cavity?.CavityName,
        o.RequesterName,
        o.Description,
        o.ItemCount,
        o.TotalRequestQuantity,
        o.TotalIssuedQuantity,
        o.TotalAmount,
        stockOutId = o.StockOutId,
        stockOutNo = o.StockOut?.StockOutNo
    }));
});

app.MapGet("/api/stockoutorders/{id:int}", async (int id, IRoService svc) =>
{
    var o = await svc.GetStockOutOrderAsync(id);
    if (o == null) return Results.NotFound(new { error = "Không tìm thấy phiếu yêu cầu xuất kho phụ tùng." });

    return Results.Ok(new
    {
        o.Id,
        o.OrderNo,
        o.OrderDate,
        o.RequestDeliveryTime,
        priority = Ui.StockOutOrderPriority(o.Priority).text,
        priorityCode = (int)o.Priority,
        status = Ui.StockOutOrderStatus(o.Status).text,
        statusCode = Ui.StockOutOrderStatus(o.Status).code,
        statusValue = (int)o.Status,
        roId = o.ROId,
        roCode = o.RO?.Code,
        plate = o.Car?.Plate,
        carModel = o.Car?.Model,
        customerName = o.Customer?.Name,
        customerPhone = o.Customer?.Phone,
        cavityName = o.Cavity?.CavityName,
        o.RequesterName,
        o.Description,
        o.CreatedBy,
        o.CreatedAt,
        o.ApprovedBy,
        o.ApprovedAt,
        o.RejectReason,
        o.ItemCount,
        o.TotalRequestQuantity,
        o.TotalIssuedQuantity,
        o.SubTotal,
        o.TotalVat,
        o.TotalAmount,
        stockOutId = o.StockOutId,
        stockOutNo = o.StockOut?.StockOutNo,
        canApprove = o.CanApprove,
        canIssue = o.CanIssue,
        canReject = o.CanReject,
        canDelete = o.CanDelete,
        items = o.Items.Select(i => new
        {
            i.Id,
            i.PartId,
            i.PartCode,
            i.PartName,
            i.Unit,
            i.RequestQuantity,
            i.IssuedQuantity,
            i.UnitPrice,
            i.VatPercent,
            i.SubTotal,
            i.VatAmount,
            i.Amount,
            i.Note,
            inStock = i.Part?.InStock ?? 0
        })
    });
});

app.MapPost("/api/stockoutorders", async (CreateStockOutOrderDto dto, IRoService svc) =>
{
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Phiếu yêu cầu cần ít nhất một phụ tùng." });

    try
    {
        var order = new StockOutOrder
        {
            OrderNo = dto.OrderNo?.Trim() ?? "",
            ROId = dto.RoId,
            OrderDate = dto.OrderDate ?? DateTime.Today,
            RequestDeliveryTime = dto.RequestDeliveryTime,
            Priority = dto.Priority,
            CavityId = dto.CavityId,
            RequesterName = dto.RequesterName?.Trim(),
            Description = dto.Description?.Trim(),
            CreatedBy = dto.CreatedBy ?? "api"
        };

        var items = dto.Items.Select(i => new StockOutOrderDetail
        {
            PartId = i.PartId,
            RequestQuantity = i.RequestQuantity <= 0 ? 1 : i.RequestQuantity,
            UnitPrice = i.UnitPrice ?? 0,
            VatPercent = i.VatPercent ?? 8,
            Note = i.Note?.Trim()
        }).ToList();

        var id = await svc.CreateStockOutOrderAsync(order, items);
        return Results.Created($"/api/stockoutorders/{id}", new { id, order.OrderNo, message = "Đã lập phiếu yêu cầu xuất kho thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/stockoutorders/{id:int}/approve", async (int id, ApproveStockOutOrderDto? dto, IRoService svc) =>
{
    var (ok, msg) = await svc.ApproveStockOutOrderAsync(id, dto?.ApprovedBy);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/stockoutorders/{id:int}/reject", async (int id, RejectStockOutOrderDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.RejectStockOutOrderAsync(id, dto.Reason);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/stockoutorders/{id:int}/issue", async (int id, IssueStockOutOrderDto? dto, IRoService svc) =>
{
    var (ok, msg, stockOutId) = await svc.IssueStockOutFromOrderAsync(id, dto?.IssuedBy);
    return ok ? Results.Ok(new { message = msg, stockOutId }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/stockoutorders/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteStockOutOrderAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Quản lý Phụ tùng nợ khách (Ser_Part_OO)
app.MapGet("/api/part-oos", async (string? q, bool? isConNo, PartOOStatus? status, IRoService svc) =>
{
    var list = await svc.PartOOsAsync(q, isConNo, status);
    return Results.Ok(list.Select(o => new
    {
        o.Id,
        o.OONo,
        o.PartId,
        o.PartCode,
        o.PartName,
        o.OOPlateNo,
        o.Model,
        o.SoLuongNo,
        o.SoLuongTra,
        o.SoLuongConNo,
        o.IsConNoKhach,
        o.IsStockAvailable,
        currentInStock = o.Part?.InStock ?? 0,
        o.CVDV,
        o.NgayDatHang,
        o.NgayVeDuKien,
        o.NgayHenTra,
        o.GhiChu,
        status = Ui.PartOOStatus(o.Status).text,
        statusCode = Ui.PartOOStatus(o.Status).code,
        o.ROId,
        roCode = o.RO?.Code,
        o.CreatedAt,
        o.FinishedAt,
        o.ReturnedBy
    }));
});

app.MapGet("/api/part-oos/{id:int}", async (int id, IRoService svc) =>
{
    var o = await svc.GetPartOOAsync(id);
    if (o == null) return Results.NotFound(new { error = "Không tìm thấy phiếu nợ phụ tùng." });
    return Results.Ok(new
    {
        o.Id,
        o.OONo,
        o.PartId,
        o.PartCode,
        o.PartName,
        o.OOPlateNo,
        o.Model,
        o.SoLuongNo,
        o.SoLuongTra,
        o.SoLuongConNo,
        o.IsConNoKhach,
        o.IsStockAvailable,
        currentInStock = o.Part?.InStock ?? 0,
        salePrice = o.Part?.SalePrice ?? 0,
        totalOwedAmount = o.TotalOwedAmount,
        remainingAmount = o.RemainingAmount,
        o.CVDV,
        o.NgayDatHang,
        o.NgayVeDuKien,
        o.NgayHenTra,
        o.GhiChu,
        status = Ui.PartOOStatus(o.Status).text,
        statusCode = Ui.PartOOStatus(o.Status).code,
        o.ROId,
        roCode = o.RO?.Code,
        o.CarId,
        o.CustomerId,
        customerName = o.Customer?.Name,
        o.CreatedBy,
        o.CreatedAt,
        o.FinishedAt,
        o.ReturnedBy
    });
});

app.MapGet("/api/part-oos/stock-alerts", async (IRoService svc) =>
{
    var list = await svc.GetPartOOStockAlertsAsync();
    return Results.Ok(list.Select(o => new
    {
        o.Id,
        o.OONo,
        o.PartCode,
        o.PartName,
        o.OOPlateNo,
        o.Model,
        o.SoLuongConNo,
        currentInStock = o.Part?.InStock ?? 0,
        o.CVDV,
        o.NgayHenTra,
        customerName = o.Customer?.Name
    }));
});

app.MapGet("/api/part-oos/by-plate/{plate}", async (string plate, IRoService svc) =>
{
    var list = await svc.GetPartOOsByPlateAsync(plate);
    return Results.Ok(list.Select(o => new
    {
        o.Id,
        o.OONo,
        o.PartCode,
        o.PartName,
        o.OOPlateNo,
        o.SoLuongNo,
        o.SoLuongTra,
        o.SoLuongConNo,
        status = Ui.PartOOStatus(o.Status).text
    }));
});

app.MapPost("/api/part-oos", async (CreatePartOODto dto, IRoService svc) =>
{
    try
    {
        var item = new PartOO
        {
            PartId = dto.PartId,
            OOPlateNo = dto.OOPlateNo,
            Model = dto.Model,
            SoLuongNo = dto.SoLuongNo,
            CVDV = dto.CVDV,
            NgayDatHang = dto.NgayDatHang,
            NgayVeDuKien = dto.NgayVeDuKien,
            NgayHenTra = dto.NgayHenTra,
            GhiChu = dto.GhiChu,
            ROId = dto.ROId,
            CreatedBy = dto.CVDV ?? "api"
        };
        var id = await svc.CreatePartOOAsync(item);
        return Results.Created($"/api/part-oos/{id}", new { id, item.OONo, message = "Đã lập phiếu nợ phụ tùng thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/part-oos/{id:int}", async (int id, UpdatePartOODto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.UpdatePartOOAsync(id, dto.Model, dto.SoLuongNo, dto.SoLuongTra, dto.CVDV, dto.NgayDatHang, dto.NgayVeDuKien, dto.NgayHenTra, dto.GhiChu);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/part-oos/{id:int}/return", async (int id, ReturnPartOODto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.ReturnPartOOAsync(id, dto.ReturnQty, dto.DeductStock ?? true, dto.ReturnedBy, dto.Note);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/part-oos/{id:int}/cancel", async (int id, CancelPartOODto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.CancelPartOOAsync(id, dto.Reason);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/part-oos/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeletePartOOAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Quản lý Công nợ khách hàng dịch vụ (Ser_CusDebit, Ser_CusDebitPayment, Ser_InvReportCusDebitRpt - MH 54)
app.MapGet("/api/cusdebits", async (int? customerId, CusDebitStatus? status, CusDebitType? type, string? q, bool? isOverdue, DateTime? fromDate, DateTime? toDate, IRoService svc) =>
{
    var list = await svc.CusDebitsAsync(customerId, status, type, q, isOverdue, fromDate, toDate);
    return Results.Ok(list.Select(d => new
    {
        d.Id,
        d.DebitNo,
        d.DebitDate,
        d.CustomerId,
        customerName = d.Customer?.Name,
        customerPhone = d.Customer?.Phone,
        d.CarId,
        plate = d.Car?.Plate,
        carModel = d.Car?.Model,
        d.ROId,
        roCode = d.RO?.Code,
        type = Ui.CusDebitType(d.DebitType).text,
        typeCode = (int)d.DebitType,
        status = Ui.CusDebitStatus(d.Status).text,
        statusCode = Ui.CusDebitStatus(d.Status).code,
        statusValue = (int)d.Status,
        d.DebitAmount,
        d.PaidAmount,
        d.RemainAmount,
        d.DueDate,
        d.IsOverdue,
        d.CanPay,
        d.Description,
        d.CreatedBy,
        d.CreatedAt,
        d.ClearedAt
    }));
});

app.MapGet("/api/cusdebits/summaries", async (string? q, bool? onlyHasDebit, IRoService svc) =>
{
    var list = await svc.CustomerDebitSummariesAsync(q, onlyHasDebit);
    return Results.Ok(list);
});

app.MapGet("/api/cusdebits/customer/{customerId:int}", async (int customerId, IRoService svc) =>
{
    try
    {
        var (customer, debits, payments, totalDebit, totalPaid, remainingDebit) = await svc.GetCustomerDebitProfileAsync(customerId);
        return Results.Ok(new
        {
            customer = new
            {
                customer.Id,
                customer.Code,
                customer.Name,
                customer.Phone,
                customer.Email,
                cars = customer.Cars.Select(c => new { c.Id, c.Plate, c.Model, c.Vin, c.Year })
            },
            totalDebit,
            totalPaid,
            remainingDebit,
            hasDebit = remainingDebit > 0,
            debits = debits.Select(d => new
            {
                d.Id,
                d.DebitNo,
                d.DebitDate,
                type = Ui.CusDebitType(d.DebitType).text,
                typeValue = (int)d.DebitType,
                status = Ui.CusDebitStatus(d.Status).text,
                statusCode = Ui.CusDebitStatus(d.Status).code,
                d.DebitAmount,
                d.PaidAmount,
                d.RemainAmount,
                d.DueDate,
                d.IsOverdue,
                plate = d.Car?.Plate,
                roCode = d.RO?.Code,
                d.Description
            }),
            payments = payments.Select(p => new
            {
                p.Id,
                p.PaymentNo,
                p.PaymentDate,
                p.PaymentAmount,
                method = Ui.PaymentMethod(p.Method).text,
                p.PayPersonName,
                p.PayPersonPhone,
                p.PayPersonIdCard,
                p.TransactionRef,
                p.Note,
                p.Collector,
                debitNo = p.CusDebit?.DebitNo
            })
        });
    }
    catch (Exception ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

app.MapGet("/api/cusdebits/{id:int}", async (int id, IRoService svc) =>
{
    var d = await svc.GetCusDebitAsync(id);
    if (d == null) return Results.NotFound(new { error = "Không tìm thấy khoản công nợ." });
    return Results.Ok(new
    {
        d.Id,
        d.DebitNo,
        d.DebitDate,
        customer = new { d.Customer.Id, d.Customer.Code, d.Customer.Name, d.Customer.Phone },
        car = d.Car != null ? new { d.Car.Id, d.Car.Plate, d.Car.Model } : null,
        ro = d.RO != null ? new { d.RO.Id, d.RO.Code, d.RO.Status, d.RO.Total } : null,
        type = Ui.CusDebitType(d.DebitType).text,
        typeValue = (int)d.DebitType,
        status = Ui.CusDebitStatus(d.Status).text,
        statusCode = Ui.CusDebitStatus(d.Status).code,
        statusValue = (int)d.Status,
        d.DebitAmount,
        d.PaidAmount,
        d.RemainAmount,
        d.DueDate,
        d.IsOverdue,
        d.CanPay,
        d.Description,
        d.CreatedBy,
        d.CreatedAt,
        d.ClearedAt,
        payments = d.Payments.Select(p => new
        {
            p.Id,
            p.PaymentNo,
            p.PaymentDate,
            p.PaymentAmount,
            method = Ui.PaymentMethod(p.Method).text,
            p.PayPersonName,
            p.Collector
        })
    });
});

app.MapPost("/api/cusdebits", async (CreateCusDebitDto dto, IRoService svc) =>
{
    try
    {
        var debit = new CusDebit
        {
            CustomerId = dto.CustomerId,
            CarId = dto.CarId,
            ROId = dto.ROId,
            DebitType = dto.DebitType ?? CusDebitType.RO,
            DebitAmount = dto.DebitAmount,
            DebitDate = dto.DebitDate ?? DateTime.Today,
            DueDate = dto.DueDate ?? DateTime.Today.AddDays(30),
            Description = dto.Description?.Trim(),
            CreatedBy = dto.CreatedBy ?? "api"
        };
        var id = await svc.CreateCusDebitAsync(debit);
        return Results.Created($"/api/cusdebits/{id}", new { id, debit.DebitNo, message = "Đã ghi nhận công nợ thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/cusdebits/create-from-ro", async (CreateDebitFromRoDto dto, IRoService svc) =>
{
    var (ok, msg, debitId) = await svc.CreateDebitFromROAsync(dto.ROId, dto.Amount, dto.DueDate, dto.Note);
    return ok ? Results.Ok(new { message = msg, debitId }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/cusdebits/{id:int}/cancel", async (int id, CancelCusDebitDto? dto, IRoService svc) =>
{
    var (ok, msg) = await svc.CancelCusDebitAsync(id, dto?.Reason);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/cusdebits/payments", async (CreateCusDebitPaymentDto dto, IRoService svc) =>
{
    try
    {
        var payment = new CusDebitPayment
        {
            CustomerId = dto.CustomerId,
            CusDebitId = dto.CusDebitId,
            PaymentAmount = dto.PaymentAmount,
            PaymentDate = dto.PaymentDate ?? DateTime.Today,
            Method = dto.Method ?? PaymentMethod.Cash,
            PayPersonName = dto.PayPersonName ?? "",
            PayPersonIdCard = dto.PayPersonIdCard?.Trim(),
            PayPersonPhone = dto.PayPersonPhone?.Trim(),
            TransactionRef = dto.TransactionRef?.Trim(),
            Note = dto.Note?.Trim(),
            Collector = dto.Collector ?? "Thu ngân"
        };
        var id = await svc.CreateCusDebitPaymentAsync(payment);
        return Results.Created($"/api/cusdebits/payments/{id}", new { id, payment.PaymentNo, message = "Đã lập phiếu thu nợ thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/cusdebits/payments/{paymentId:int}", async (int paymentId, IRoService svc) =>
{
    var p = await svc.GetCusDebitPaymentAsync(paymentId);
    if (p == null) return Results.NotFound(new { error = "Không tìm thấy phiếu thu nợ." });
    return Results.Ok(new
    {
        p.Id,
        p.PaymentNo,
        p.PaymentDate,
        p.PaymentAmount,
        method = Ui.PaymentMethod(p.Method).text,
        methodValue = (int)p.Method,
        p.PayPersonName,
        p.PayPersonIdCard,
        p.PayPersonPhone,
        p.TransactionRef,
        p.Note,
        p.Collector,
        customer = new { p.Customer.Id, p.Customer.Code, p.Customer.Name, p.Customer.Phone },
        debit = p.CusDebit != null ? new { p.CusDebit.Id, p.CusDebit.DebitNo, p.CusDebit.DebitAmount, p.CusDebit.RemainAmount } : null
    });
});

app.MapDelete("/api/cusdebits/payments/{paymentId:int}", async (int paymentId, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteCusDebitPaymentAsync(paymentId);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// =========================================================================
// API Quản lý Công nợ Nhà Cung Cấp & Thanh toán nợ NCC (Ser_SupplierDebit, Ser_SupplierDebitPayment / MH 56)
// =========================================================================

app.MapGet("/api/supplierdebits", async (int? supplierId, SupplierDebitStatus? status, SupplierDebitType? type, string? q, bool? isOverdue, DateTime? fromDate, DateTime? toDate, IRoService svc) =>
{
    var debits = await svc.SupplierDebitsAsync(supplierId, status, type, q, isOverdue, fromDate, toDate);
    return Results.Ok(debits.Select(d => new
    {
        d.Id,
        d.DebitNo,
        supplier = new { d.Supplier.Id, d.Supplier.Code, d.Supplier.Name, d.Supplier.Phone, d.Supplier.BankAccount, d.Supplier.BankName },
        stockIn = d.StockIn != null ? new { d.StockIn.Id, d.StockIn.StockInNo, d.StockIn.BillNo, d.StockIn.Total } : null,
        orderPart = d.OrderPart != null ? new { d.OrderPart.Id, d.OrderPart.OrderPartNo } : null,
        debitType = Ui.SupplierDebitType(d.DebitType).text,
        type = (int)d.DebitType,
        status = Ui.SupplierDebitStatus(d.Status).text,
        statusCode = Ui.SupplierDebitStatus(d.Status).code,
        statusValue = (int)d.Status,
        d.DebitDate,
        d.DueDate,
        d.DebitAmount,
        d.PaidAmount,
        d.RemainAmount,
        d.IsOverdue,
        d.CanPay,
        d.Description,
        d.CreatedBy,
        d.CreatedAt,
        paymentCount = d.Payments.Count
    }));
});

app.MapGet("/api/supplierdebits/summaries", async (string? q, bool? onlyHasDebit, IRoService svc) =>
{
    var summaries = await svc.SupplierDebitSummariesAsync(q, onlyHasDebit);
    return Results.Ok(summaries);
});

app.MapGet("/api/supplierdebits/supplier/{supplierId:int}", async (int supplierId, IRoService svc) =>
{
    try
    {
        var (supplier, debits, payments, totalDebit, totalPaid, remainingDebit) = await svc.GetSupplierDebitProfileAsync(supplierId);
        return Results.Ok(new
        {
            supplier = new { supplier.Id, supplier.Code, supplier.Name, supplier.Phone, supplier.Email, supplier.Address, supplier.ContactName, supplier.ContactPhone, supplier.TaxCode, supplier.BankAccount, supplier.BankName },
            summary = new
            {
                totalDebit,
                totalPaid,
                remainingDebit,
                hasDebit = remainingDebit > 0,
                debitCount = debits.Count,
                paymentCount = payments.Count,
                overdueCount = debits.Count(d => d.IsOverdue)
            },
            debits = debits.Select(d => new
            {
                d.Id, d.DebitNo, d.DebitAmount, d.PaidAmount, d.RemainAmount,
                status = Ui.SupplierDebitStatus(d.Status).text,
                statusCode = Ui.SupplierDebitStatus(d.Status).code,
                d.DebitDate, d.DueDate, d.IsOverdue,
                stockInNo = d.StockIn?.StockInNo,
                orderPartNo = d.OrderPart?.OrderPartNo,
                d.Description
            }),
            payments = payments.Select(p => new
            {
                p.Id, p.PaymentNo, p.PaymentAmount, p.PaymentDate,
                method = Ui.PaymentMethod(p.Method).text,
                p.PayPersonName, p.BankAccount, p.BankName, p.TransactionRef, p.Cashier, p.Note,
                debitNo = p.SupplierDebit?.DebitNo
            })
        });
    }
    catch (Exception ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

app.MapGet("/api/supplierdebits/{id:int}", async (int id, IRoService svc) =>
{
    var d = await svc.GetSupplierDebitAsync(id);
    if (d == null) return Results.NotFound(new { error = "Không tìm thấy khoản nợ NCC." });
    return Results.Ok(new
    {
        d.Id,
        d.DebitNo,
        supplier = new { d.Supplier.Id, d.Supplier.Code, d.Supplier.Name, d.Supplier.Phone, d.Supplier.Address, d.Supplier.BankAccount, d.Supplier.BankName },
        stockIn = d.StockIn != null ? new { d.StockIn.Id, d.StockIn.StockInNo, d.StockIn.BillNo, d.StockIn.Total, d.StockIn.StockInDate, itemCount = d.StockIn.Items.Count } : null,
        orderPart = d.OrderPart != null ? new { d.OrderPart.Id, d.OrderPart.OrderPartNo } : null,
        debitType = Ui.SupplierDebitType(d.DebitType).text,
        status = Ui.SupplierDebitStatus(d.Status).text,
        statusCode = Ui.SupplierDebitStatus(d.Status).code,
        d.DebitDate,
        d.DueDate,
        d.DebitAmount,
        d.PaidAmount,
        d.RemainAmount,
        d.IsOverdue,
        d.CanPay,
        d.Description,
        d.CreatedBy,
        d.CreatedAt,
        payments = d.Payments.Select(p => new
        {
            p.Id, p.PaymentNo, p.PaymentAmount, p.PaymentDate,
            method = Ui.PaymentMethod(p.Method).text,
            p.PayPersonName, p.BankAccount, p.BankName, p.TransactionRef, p.Note, p.Cashier
        })
    });
});

app.MapPost("/api/supplierdebits", async (CreateSupplierDebitDto dto, IRoService svc) =>
{
    try
    {
        var debit = new SupplierDebit
        {
            SupplierId = dto.SupplierId,
            StockInId = dto.StockInId,
            OrderPartId = dto.OrderPartId,
            DebitType = dto.DebitType ?? SupplierDebitType.StockIn,
            DebitAmount = dto.DebitAmount,
            DebitDate = dto.DebitDate ?? DateTime.Today,
            DueDate = dto.DueDate ?? DateTime.Today.AddDays(30),
            Description = dto.Description,
            CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "Kế toán kho" : dto.CreatedBy
        };
        var id = await svc.CreateSupplierDebitAsync(debit);
        return Results.Created($"/api/supplierdebits/{id}", new { id, debit.DebitNo, message = "Đã ghi nhận công nợ NCC thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/supplierdebits/from-stockin", async (CreateSupplierDebitFromStockInDto dto, IRoService svc) =>
{
    var (ok, msg, debitId) = await svc.CreateSupplierDebitFromStockInAsync(dto.StockInId, dto.SupplierId, dto.DueDate, dto.Note);
    return ok ? Results.Ok(new { debitId, message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/supplierdebits/{id:int}/cancel", async (int id, CancelSupplierDebitDto? dto, IRoService svc) =>
{
    var (ok, msg) = await svc.CancelSupplierDebitAsync(id, dto?.Reason);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/supplierdebits/payments", async (CreateSupplierDebitPaymentDto dto, IRoService svc) =>
{
    try
    {
        var payment = new SupplierDebitPayment
        {
            SupplierId = dto.SupplierId,
            SupplierDebitId = dto.SupplierDebitId,
            PaymentAmount = dto.PaymentAmount,
            PaymentDate = dto.PaymentDate ?? DateTime.Today,
            Method = dto.Method ?? PaymentMethod.BankTransfer,
            PayPersonName = dto.PayPersonName ?? "",
            PayPersonIdCard = dto.PayPersonIdCard,
            PayPersonPhone = dto.PayPersonPhone,
            BankAccount = dto.BankAccount,
            BankName = dto.BankName,
            TransactionRef = dto.TransactionRef,
            Note = dto.Note,
            Cashier = string.IsNullOrWhiteSpace(dto.Cashier) ? "Thủ quỹ" : dto.Cashier
        };
        var id = await svc.CreateSupplierDebitPaymentAsync(payment, allocateFifoIfNoDebit: true);
        return Results.Created($"/api/supplierdebits/payments/{id}", new { id, payment.PaymentNo, message = "Đã lập phiếu chi nợ NCC thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/supplierdebits/payments/{paymentId:int}", async (int paymentId, IRoService svc) =>
{
    var p = await svc.GetSupplierDebitPaymentAsync(paymentId);
    if (p == null) return Results.NotFound(new { error = "Không tìm thấy phiếu chi." });
    return Results.Ok(new
    {
        p.Id,
        p.PaymentNo,
        p.PaymentAmount,
        p.PaymentDate,
        method = Ui.PaymentMethod(p.Method).text,
        p.PayPersonName,
        p.PayPersonIdCard,
        p.PayPersonPhone,
        p.BankAccount,
        p.BankName,
        p.TransactionRef,
        p.Note,
        p.Cashier,
        p.CreatedAt,
        supplier = new { p.Supplier.Id, p.Supplier.Code, p.Supplier.Name, p.Supplier.Phone },
        debit = p.SupplierDebit != null ? new { p.SupplierDebit.Id, p.SupplierDebit.DebitNo, p.SupplierDebit.DebitAmount, p.SupplierDebit.RemainAmount } : null
    });
});

app.MapDelete("/api/supplierdebits/payments/{paymentId:int}", async (int paymentId, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteSupplierDebitPaymentAsync(paymentId);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// =========================================================================
// API Tra cứu & Chia sẻ Lịch sử Sửa chữa Toàn Hệ thống Đại lý (DealerHistoryShareMng)
// =========================================================================

app.MapGet("/api/dealer-history/search", async (string? q, string? dealer, DateTime? fromDate, DateTime? toDate, IRoService svc) =>
{
    var list = await svc.SearchDealerHistoryAsync(q, dealer, fromDate, toDate);
    return Results.Ok(list.Select(r => new
    {
        r.Id,
        r.RecordNo,
        r.DealerCode,
        r.DealerName,
        r.PlateNo,
        r.FrameNo,
        r.EngineNo,
        r.ModelName,
        r.ProductYear,
        r.CusName,
        r.CusPhone,
        r.RONo,
        r.ROId,
        r.CheckInDate,
        r.ActualDeliveryDate,
        r.Odometer,
        r.ServiceAdvisor,
        r.Technician,
        r.CustomerRequest,
        r.CarStatus,
        r.RepairResult,
        r.TotalLaborAmount,
        r.TotalPartAmount,
        r.TotalAmount,
        r.FlagClaim,
        r.ClaimNo,
        r.ClaimStatus,
        r.LaborCount,
        r.PartCount,
        dealerBadge = Ui.DealerBadge(r.DealerCode).text,
        dealerBadgeCss = Ui.DealerBadge(r.DealerCode).css,
        claimBadge = Ui.ClaimBadge(r.FlagClaim, r.ClaimNo).text,
        claimBadgeCss = Ui.ClaimBadge(r.FlagClaim, r.ClaimNo).css
    }));
});

app.MapGet("/api/dealer-history/vehicle/{plateOrVin}", async (string plateOrVin, IRoService svc) =>
{
    var summary = await svc.GetVehicleServiceSummaryAsync(plateOrVin);
    if (summary == null) return Results.NotFound(new { error = $"Không tìm thấy lịch sử xe '{plateOrVin}'." });
    return Results.Ok(summary);
});

app.MapGet("/api/dealer-history/{id:int}", async (int id, IRoService svc) =>
{
    var r = await svc.GetDealerHistoryRecordAsync(id);
    if (r == null) return Results.NotFound(new { error = "Không tìm thấy hồ sơ lịch sử sửa chữa." });
    return Results.Ok(new
    {
        r.Id,
        r.RecordNo,
        r.DealerCode,
        r.DealerName,
        r.PlateNo,
        r.FrameNo,
        r.EngineNo,
        r.TradeMarkName,
        r.ModelName,
        r.ColorCode,
        r.ProductYear,
        r.CusName,
        r.CusPhone,
        r.CusAddress,
        r.RONo,
        r.ROId,
        r.CheckInDate,
        r.ActualDeliveryDate,
        r.Odometer,
        r.ServiceAdvisor,
        r.Technician,
        r.CustomerRequest,
        r.CarStatus,
        r.RepairResult,
        r.TotalLaborAmount,
        r.TotalPartAmount,
        r.TotalAmount,
        r.FlagClaim,
        r.ClaimNo,
        r.ClaimStatus,
        r.CreatedBy,
        r.CreatedAt,
        items = r.Items.Select(i => new
        {
            i.Id,
            itemType = Ui.Line(i.ItemType),
            typeValue = (int)i.ItemType,
            i.Code,
            i.Name,
            i.Unit,
            i.Quantity,
            i.UnitPrice,
            i.Amount,
            expenseType = Ui.Expense(i.ExpenseType).text,
            expenseCss = Ui.Expense(i.ExpenseType).css,
            i.Technician,
            i.Result,
            i.Remark
        })
    });
});

app.MapPost("/api/dealer-history", async (CreateDealerHistoryRecordDto dto, IRoService svc) =>
{
    try
    {
        var record = new DealerHistoryRecord
        {
            DealerCode = dto.DealerCode?.Trim() ?? "HTC-CG",
            DealerName = dto.DealerName?.Trim() ?? "Hyundai Cầu Giấy",
            PlateNo = dto.PlateNo?.Trim() ?? "",
            FrameNo = dto.FrameNo?.Trim() ?? "",
            EngineNo = dto.EngineNo?.Trim(),
            TradeMarkName = dto.TradeMarkName?.Trim() ?? "Hyundai",
            ModelName = dto.ModelName?.Trim() ?? "",
            ColorCode = dto.ColorCode?.Trim(),
            ProductYear = dto.ProductYear > 0 ? dto.ProductYear : DateTime.Today.Year,
            CusName = dto.CusName?.Trim() ?? "",
            CusPhone = dto.CusPhone?.Trim(),
            CusAddress = dto.CusAddress?.Trim(),
            RONo = dto.RONo?.Trim() ?? "",
            CheckInDate = dto.CheckInDate ?? DateTime.Now,
            ActualDeliveryDate = dto.ActualDeliveryDate,
            Odometer = dto.Odometer,
            ServiceAdvisor = dto.ServiceAdvisor?.Trim() ?? "CVDV",
            Technician = dto.Technician?.Trim(),
            CustomerRequest = dto.CustomerRequest?.Trim(),
            CarStatus = dto.CarStatus?.Trim(),
            RepairResult = dto.RepairResult?.Trim() ?? "Đã hoàn tất dịch vụ",
            FlagClaim = dto.FlagClaim ?? false,
            ClaimNo = dto.ClaimNo?.Trim(),
            ClaimStatus = dto.ClaimStatus?.Trim(),
            CreatedBy = "api"
        };

        var items = (dto.Items ?? []).Select(i => new DealerHistoryItem
        {
            ItemType = i.ItemType,
            Code = i.Code?.Trim() ?? "",
            Name = i.Name?.Trim() ?? "",
            Unit = i.Unit?.Trim() ?? "Cái",
            Quantity = i.Quantity <= 0 ? 1 : i.Quantity,
            UnitPrice = i.UnitPrice,
            ExpenseType = i.ExpenseType ?? ExpenseType.Customer,
            Technician = i.Technician?.Trim(),
            Result = i.Result?.Trim(),
            Remark = i.Remark?.Trim()
        }).ToList();

        var id = await svc.CreateDealerHistoryRecordAsync(record, items);
        return Results.Created($"/api/dealer-history/{id}", new { id, record.RecordNo, message = "Đã lưu hồ sơ lịch sử sửa chữa thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/dealer-history/sync-ro/{roId:int}", async (int roId, IRoService svc) =>
{
    var (ok, msg, recordId) = await svc.SyncLocalRoToHistoryAsync(roId);
    return ok ? Results.Ok(new { message = msg, recordId }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/dealer-history/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteDealerHistoryRecordAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// =========================================================================
// MINIMAL APIS — QUẢN LÝ CÔNG NỢ BẢO HIỂM XE & BỒI THƯỜNG (MH 55 / Ser_InsuranceDebit)
// =========================================================================

app.MapGet("/api/insurancedebits/summaries", async (string? q, bool? onlyHasDebit, IRoService svc) =>
{
    var list = await svc.InsuranceCompanyDebitSummariesAsync(q, onlyHasDebit);
    return Results.Ok(list);
});

app.MapGet("/api/insurancedebits/company/{companyId:int}", async (int companyId, IRoService svc) =>
{
    try
    {
        var (company, summary, debits, payments) = await svc.GetInsuranceCompanyDebitProfileAsync(companyId);
        return Results.Ok(new
        {
            company = new { company.Id, company.InsNo, company.InsName, company.Phone, company.Hotline, company.Email, company.Address, company.TaxCode },
            summary,
            debits = debits.Select(d => new
            {
                d.Id, d.DebitNo, d.RONo, d.ROId, d.PlateNo, d.CarModel, d.CustomerName, d.ClaimNo, d.PolicyNo,
                type = d.DebitType.ToString(),
                status = d.Status.ToString(),
                d.DebitDate, d.DueDate, d.DebitAmount, d.PaidAmount, d.RemainAmount, d.IsOverdue, d.CanPay,
                d.Description, d.CreatedAt
            }),
            payments = payments.Select(p => new
            {
                p.Id, p.PaymentNo, p.InsuranceDebitId, p.PaymentDate, p.PaymentAmount,
                method = p.Method.ToString(),
                status = p.Status.ToString(),
                p.PayPersonName, p.BankAccount, p.BankName, p.TransactionRef, p.Note, p.Cashier
            })
        });
    }
    catch (Exception ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

app.MapGet("/api/insurancedebits", async (int? companyId, InsuranceDebitStatus? status, InsuranceDebitType? type, string? q, bool? isOverdue, DateTime? fromDate, DateTime? toDate, IRoService svc) =>
{
    var list = await svc.InsuranceDebitsAsync(companyId, status, type, q, isOverdue, fromDate, toDate);
    return Results.Ok(list.Select(d => new
    {
        d.Id, d.DebitNo, d.InsuranceCompanyId, d.InsNo, d.InsName,
        d.ROId, d.RONo, d.PlateNo, d.CarModel, d.CustomerName, d.InsuranceClaimId, d.ClaimNo, d.PolicyNo,
        type = d.DebitType.ToString(),
        status = d.Status.ToString(),
        d.DebitDate, d.DueDate, d.DebitAmount, d.PaidAmount, d.RemainAmount, d.IsOverdue, d.CanPay,
        d.Description, d.CreatedBy, d.CreatedAt
    }));
});

app.MapGet("/api/insurancedebits/{id:int}", async (int id, IRoService svc) =>
{
    var d = await svc.GetInsuranceDebitAsync(id);
    if (d == null) return Results.NotFound(new { error = "Không tìm thấy khoản nợ bảo hiểm." });
    return Results.Ok(new
    {
        d.Id, d.DebitNo, d.InsuranceCompanyId, d.InsNo, d.InsName,
        d.ROId, d.RONo, d.PlateNo, d.CarModel, d.CustomerName, d.InsuranceClaimId, d.ClaimNo, d.PolicyNo,
        type = d.DebitType.ToString(),
        status = d.Status.ToString(),
        d.DebitDate, d.DueDate, d.DebitAmount, d.PaidAmount, d.RemainAmount, d.IsOverdue, d.CanPay,
        d.Description, d.CreatedBy, d.CreatedAt, d.ClearedAt, d.CancelledReason,
        payments = d.Payments.Select(p => new
        {
            p.Id, p.PaymentNo, p.PaymentDate, p.PaymentAmount,
            method = p.Method.ToString(),
            status = p.Status.ToString(),
            p.PayPersonName, p.TransactionRef, p.Note
        })
    });
});

app.MapPost("/api/insurancedebits", async (CreateInsuranceDebitDto dto, IRoService svc) =>
{
    try
    {
        var debit = new InsuranceDebit
        {
            InsuranceCompanyId = dto.InsuranceCompanyId,
            ROId = dto.RoId,
            InsuranceClaimId = dto.InsuranceClaimId,
            DebitType = dto.DebitType ?? InsuranceDebitType.RO,
            DebitAmount = dto.DebitAmount,
            DebitDate = dto.DebitDate ?? DateTime.Today,
            DueDate = dto.DueDate ?? DateTime.Today.AddDays(30),
            Description = dto.Description,
            CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "API" : dto.CreatedBy
        };
        var id = await svc.CreateInsuranceDebitAsync(debit);
        return Results.Created($"/api/insurancedebits/{id}", new { id, debit.DebitNo, message = "Đã ghi nhận công nợ bồi thường bảo hiểm thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/insurancedebits/from-ro", async (CreateInsuranceDebitFromRoDto dto, IRoService svc) =>
{
    var (ok, msg, debitId) = await svc.CreateInsuranceDebitFromRoAsync(dto.RoId, dto.CompanyId, dto.DebitAmount, dto.DueDate, dto.Note);
    return ok
        ? Results.Created($"/api/insurancedebits/{debitId}", new { debitId, message = msg })
        : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/insurancedebits/from-claim", async (CreateInsuranceDebitFromClaimDto dto, IRoService svc) =>
{
    var (ok, msg, debitId) = await svc.CreateInsuranceDebitFromClaimAsync(dto.ClaimId, dto.DueDate, dto.Note);
    return ok
        ? Results.Created($"/api/insurancedebits/{debitId}", new { debitId, message = msg })
        : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/insurancedebits/{id:int}/cancel", async (int id, CancelInsuranceDebitDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.CancelInsuranceDebitAsync(id, dto.Reason);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapGet("/api/insurancedebits/payments", async (int? companyId, int? debitId, DateTime? fromDate, DateTime? toDate, IRoService svc) =>
{
    var list = await svc.InsuranceDebitPaymentsAsync(companyId, debitId, fromDate, toDate);
    return Results.Ok(list.Select(p => new
    {
        p.Id, p.PaymentNo, p.InsuranceCompanyId, p.InsNo, p.InsName, p.InsuranceDebitId,
        p.PaymentDate, p.PaymentAmount,
        method = p.Method.ToString(),
        status = p.Status.ToString(),
        p.PayPersonName, p.PayPersonIdCard, p.PayPersonPhone, p.BankAccount, p.BankName, p.TransactionRef, p.Note, p.Cashier, p.CreatedAt
    }));
});

app.MapGet("/api/insurancedebits/payments/{paymentId:int}", async (int paymentId, IRoService svc) =>
{
    var p = await svc.GetInsuranceDebitPaymentAsync(paymentId);
    if (p == null) return Results.NotFound(new { error = "Không tìm thấy phiếu thu tiền bảo hiểm." });
    return Results.Ok(new
    {
        p.Id, p.PaymentNo, p.InsuranceCompanyId, p.InsNo, p.InsName, p.InsuranceDebitId,
        p.PaymentDate, p.PaymentAmount,
        method = p.Method.ToString(),
        status = p.Status.ToString(),
        p.PayPersonName, p.PayPersonIdCard, p.PayPersonPhone, p.BankAccount, p.BankName, p.TransactionRef, p.Note, p.Cashier, p.CreatedAt,
        debit = p.InsuranceDebit == null ? null : new
        {
            p.InsuranceDebit.DebitNo,
            p.InsuranceDebit.RONo,
            p.InsuranceDebit.PlateNo,
            p.InsuranceDebit.CustomerName,
            p.InsuranceDebit.DebitAmount,
            p.InsuranceDebit.PaidAmount,
            p.InsuranceDebit.RemainAmount
        }
    });
});

app.MapPost("/api/insurancedebits/payments", async (CreateInsuranceDebitPaymentDto dto, IRoService svc) =>
{
    try
    {
        var payment = new InsuranceDebitPayment
        {
            InsuranceCompanyId = dto.InsuranceCompanyId,
            InsuranceDebitId = dto.InsuranceDebitId,
            PaymentAmount = dto.PaymentAmount,
            PaymentDate = dto.PaymentDate ?? DateTime.Today,
            Method = dto.Method ?? PaymentMethod.BankTransfer,
            PayPersonName = dto.PayPersonName ?? "",
            PayPersonIdCard = dto.PayPersonIdCard,
            PayPersonPhone = dto.PayPersonPhone,
            BankAccount = dto.BankAccount,
            BankName = dto.BankName,
            TransactionRef = dto.TransactionRef,
            Note = dto.Note,
            Cashier = string.IsNullOrWhiteSpace(dto.Cashier) ? "Thu ngân" : dto.Cashier,
            CreatedBy = "API"
        };
        var id = await svc.CreateInsuranceDebitPaymentAsync(payment, allocateFifoIfNoDebit: true);
        return Results.Created($"/api/insurancedebits/payments/{id}", new { id, payment.PaymentNo, message = "Đã lập phiếu thu tiền bảo hiểm bồi thường thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapDelete("/api/insurancedebits/payments/{paymentId:int}", async (int paymentId, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteInsuranceDebitPaymentAsync(paymentId);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// =========================================================================
// API Quản lý Khách đoàn & Hợp đồng Đội xe (Ser_CustomerGroup, Ser_CustomerGroupCustomer - MNU_QT_DL_QUANLYKHACHDOAN)
// =========================================================================

app.MapGet("/api/customer-groups", async (string? q, bool? isActive, bool? creditExceededOnly, IRoService svc) =>
{
    var list = await svc.CustomerGroupSummariesAsync(q, isActive, creditExceededOnly);
    return Results.Ok(list);
});

app.MapGet("/api/customer-groups/{id:int}", async (int id, IRoService svc) =>
{
    var g = await svc.GetCustomerGroupAsync(id);
    if (g == null) return Results.NotFound(new { error = "Không tìm thấy khách đoàn." });
    return Results.Ok(new
    {
        g.Id,
        g.GroupNo,
        g.GroupName,
        g.TaxCode,
        g.Address,
        g.Telephone,
        g.Fax,
        g.Email,
        g.ContactPerson,
        g.ContactPhone,
        g.Description,
        g.IsActive,
        g.DiscountPercentLabor,
        g.DiscountPercentPart,
        g.CreditLimit,
        g.PaymentTermDays,
        g.ContractNo,
        g.ContractStartDate,
        g.ContractEndDate,
        g.CreatedAt,
        g.UpdatedAt,
        members = g.Members.Select(m => new
        {
            m.Id,
            m.CarId,
            m.PlateNo,
            model = m.Car?.Model,
            vin = m.Car?.Vin,
            year = m.Car?.Year,
            customerId = m.CustomerId,
            customerName = m.Customer?.Name,
            customerPhone = m.Customer?.Phone,
            m.DriverName,
            m.DriverPhone,
            m.JoinedDate,
            m.IsActive,
            m.Note
        }),
        recentROs = g.RepairOrders.Select(r => new
        {
            r.Id,
            r.Code,
            plate = r.Car?.Plate,
            model = r.Car?.Model,
            status = Ui.Status(r.Status).text,
            r.Total,
            r.CustomerGroupDiscountAmount,
            r.CreatedAt,
            r.FinishedAt
        })
    });
});

app.MapPost("/api/customer-groups", async (CreateCustomerGroupDto dto, IRoService svc) =>
{
    try
    {
        var group = new CustomerGroup
        {
            GroupNo = dto.GroupNo?.Trim() ?? "",
            GroupName = dto.GroupName.Trim(),
            TaxCode = dto.TaxCode?.Trim(),
            Address = dto.Address?.Trim(),
            Telephone = dto.Telephone?.Trim(),
            Fax = dto.Fax?.Trim(),
            Email = dto.Email?.Trim(),
            ContactPerson = dto.ContactPerson?.Trim(),
            ContactPhone = dto.ContactPhone?.Trim(),
            Description = dto.Description?.Trim(),
            IsActive = dto.IsActive ?? true,
            DiscountPercentLabor = dto.DiscountPercentLabor ?? 0,
            DiscountPercentPart = dto.DiscountPercentPart ?? 0,
            CreditLimit = dto.CreditLimit ?? 0,
            PaymentTermDays = dto.PaymentTermDays ?? 30,
            ContractNo = dto.ContractNo?.Trim(),
            ContractStartDate = dto.ContractStartDate,
            ContractEndDate = dto.ContractEndDate,
            CreatedBy = "API"
        };
        var id = await svc.CreateCustomerGroupAsync(group);
        return Results.Created($"/api/customer-groups/{id}", new { id, group.GroupNo, group.GroupName, message = "Đã tạo khách đoàn thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/customer-groups/{id:int}", async (int id, UpdateCustomerGroupDto dto, IRoService svc) =>
{
    var group = new CustomerGroup
    {
        GroupName = dto.GroupName,
        TaxCode = dto.TaxCode,
        Address = dto.Address,
        Telephone = dto.Telephone,
        Fax = dto.Fax,
        Email = dto.Email,
        ContactPerson = dto.ContactPerson,
        ContactPhone = dto.ContactPhone,
        Description = dto.Description,
        IsActive = dto.IsActive ?? true,
        DiscountPercentLabor = dto.DiscountPercentLabor ?? 0,
        DiscountPercentPart = dto.DiscountPercentPart ?? 0,
        CreditLimit = dto.CreditLimit ?? 0,
        PaymentTermDays = dto.PaymentTermDays ?? 30,
        ContractNo = dto.ContractNo,
        ContractStartDate = dto.ContractStartDate,
        ContractEndDate = dto.ContractEndDate
    };
    var (ok, msg) = await svc.UpdateCustomerGroupAsync(id, group);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/customer-groups/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteCustomerGroupAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapGet("/api/customer-groups/check-car/{carId:int}", async (int carId, IRoService svc) =>
{
    var member = await svc.CheckCarCustomerGroupAsync(carId);
    if (member == null) return Results.Ok(new { belongsToGroup = false });
    return Results.Ok(new
    {
        belongsToGroup = true,
        groupId = member.CustomerGroupId,
        groupNo = member.CustomerGroup.GroupNo,
        groupName = member.CustomerGroup.GroupName,
        discountPercentLabor = member.CustomerGroup.DiscountPercentLabor,
        discountPercentPart = member.CustomerGroup.DiscountPercentPart,
        creditLimit = member.CustomerGroup.CreditLimit,
        paymentTermDays = member.CustomerGroup.PaymentTermDays,
        member.DriverName,
        member.DriverPhone
    });
});

app.MapGet("/api/customer-groups/check-plate/{plate}", async (string plate, IRoService svc) =>
{
    var member = await svc.CheckPlateCustomerGroupAsync(plate);
    if (member == null) return Results.Ok(new { belongsToGroup = false });
    return Results.Ok(new
    {
        belongsToGroup = true,
        groupId = member.CustomerGroupId,
        groupNo = member.CustomerGroup.GroupNo,
        groupName = member.CustomerGroup.GroupName,
        discountPercentLabor = member.CustomerGroup.DiscountPercentLabor,
        discountPercentPart = member.CustomerGroup.DiscountPercentPart,
        creditLimit = member.CustomerGroup.CreditLimit,
        paymentTermDays = member.CustomerGroup.PaymentTermDays,
        member.DriverName,
        member.DriverPhone
    });
});

app.MapPost("/api/customer-groups/{id:int}/members", async (int id, AddCustomerGroupMemberDto dto, IRoService svc) =>
{
    var (ok, msg, memberId) = await svc.AddMemberToCustomerGroupAsync(id, dto.CarId, dto.DriverName, dto.DriverPhone, dto.Note);
    return ok ? Results.Ok(new { memberId, message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/customer-groups/members/{memberId:int}", async (int memberId, IRoService svc) =>
{
    var (ok, msg) = await svc.RemoveMemberFromCustomerGroupAsync(memberId);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/customer-groups/{id:int}/apply-ro/{roId:int}", async (int id, int roId, IRoService svc) =>
{
    var (ok, msg, discountAmount) = await svc.ApplyCustomerGroupDiscountToRoAsync(roId, id);
    return ok ? Results.Ok(new { discountAmount, message = msg }) : Results.BadRequest(new { error = msg });
});

// =========================================================================
// MINIMAL APIS — QUẢN LÝ ĐỀ NGHỊ CUNG CẤP GIÁ PHỤ TÙNG NCC TST / HTC (Req_PartPrice / Req_PartPriceDtl)
// =========================================================================

app.MapGet("/api/part-price-requests", async (DMSReqPartPriceStatus? dmsStatus, TSTReqPartPriceStatus? tstStatus, string? q, DateTime? fromDate, DateTime? toDate, IRoService svc) =>
{
    var list = await svc.PartPriceRequestsAsync(dmsStatus, tstStatus, q, fromDate, toDate);
    return Results.Ok(list.Select(r => new
    {
        r.Id,
        r.ReqPartPriceNo,
        r.DealerCode,
        r.DealerName,
        r.Description,
        r.TSTReqPartPriceID,
        r.TSTSentDate,
        dmsStatus = Ui.DMSReqPartPriceStatus(r.DMSStatus).text,
        dmsStatusCode = Ui.DMSReqPartPriceStatus(r.DMSStatus).code,
        dmsStatusValue = (int)r.DMSStatus,
        tstStatus = Ui.TSTReqPartPriceStatus(r.TSTStatus).text,
        tstStatusCode = Ui.TSTReqPartPriceStatus(r.TSTStatus).code,
        tstStatusValue = (int)r.TSTStatus,
        r.FlagIsCheck,
        r.IsUpdatePrice,
        r.UpdatedPriceAt,
        r.EffectiveDate,
        r.EstimatedResponseDate,
        r.CreatedBy,
        r.CreatedAt,
        r.ApprovedBy,
        r.ApprovedAt,
        r.ROId,
        roCode = r.RO?.Code,
        r.VIN,
        r.CarModel,
        r.TotalItems,
        r.TotalPricedAmount,
        r.CanSend,
        r.CanSimulateResponse,
        r.CanApprove,
        r.CanCreateOrderPart,
        r.IsApproved
    }));
});

app.MapGet("/api/part-price-requests/{id:int}", async (int id, IRoService svc) =>
{
    var r = await svc.GetPartPriceRequestAsync(id);
    if (r == null) return Results.NotFound(new { error = "Không tìm thấy phiếu đề nghị giá." });
    return Results.Ok(new
    {
        r.Id,
        r.ReqPartPriceNo,
        r.DealerCode,
        r.DealerName,
        r.Description,
        r.TSTReqPartPriceID,
        r.TSTSentDate,
        dmsStatus = Ui.DMSReqPartPriceStatus(r.DMSStatus).text,
        dmsStatusCode = Ui.DMSReqPartPriceStatus(r.DMSStatus).code,
        dmsStatusValue = (int)r.DMSStatus,
        tstStatus = Ui.TSTReqPartPriceStatus(r.TSTStatus).text,
        tstStatusCode = Ui.TSTReqPartPriceStatus(r.TSTStatus).code,
        tstStatusValue = (int)r.TSTStatus,
        r.FlagIsCheck,
        r.IsUpdatePrice,
        r.UpdatedPriceAt,
        r.EffectiveDate,
        r.EstimatedResponseDate,
        r.CreatedBy,
        r.CreatedAt,
        r.ApprovedBy,
        r.ApprovedAt,
        r.ROId,
        ro = r.RO == null ? null : new { r.RO.Id, r.RO.Code, plate = r.RO.Car?.Plate, model = r.RO.Car?.Model, customerName = r.RO.Customer?.Name },
        r.VIN,
        r.CarModel,
        r.TotalItems,
        r.TotalPricedAmount,
        r.CanSend,
        r.CanSimulateResponse,
        r.CanApprove,
        r.CanCreateOrderPart,
        r.IsApproved,
        items = r.Items.Select(i => new
        {
            i.Id,
            i.PartId,
            i.DMSPartCode,
            i.VieName,
            i.VINCode,
            deliveryForm = Ui.PartPriceDeliveryForm(i.DeliveryForm).text,
            deliveryFormValue = (int)i.DeliveryForm,
            i.Quantity,
            i.Unit,
            i.Remark,
            i.TSTPartCode,
            i.TSTPrice,
            i.DateEffect,
            i.Amount,
            status = Ui.ReqPartPriceLineStatus(i.Status).text,
            statusCode = Ui.ReqPartPriceLineStatus(i.Status).code,
            statusValue = (int)i.Status
        })
    });
});

app.MapPost("/api/part-price-requests", async (CreatePartPriceRequestDto dto, IRoService svc) =>
{
    try
    {
        if (dto.Items == null || dto.Items.Count == 0)
            return Results.BadRequest(new { error = "Phiếu đề nghị giá phải có ít nhất 01 dòng phụ tùng (Items)." });

        var req = new PartPriceRequest
        {
            ReqPartPriceNo = dto.ReqPartPriceNo?.Trim() ?? "",
            DealerCode = dto.DealerCode?.Trim() ?? "HTC-CG",
            DealerName = dto.DealerName?.Trim() ?? "Hyundai Cầu Giấy",
            Description = dto.Description?.Trim() ?? "",
            FlagIsCheck = dto.FlagIsCheck ?? false,
            ROId = dto.ROId,
            VIN = dto.VIN?.Trim(),
            CarModel = dto.CarModel?.Trim(),
            CreatedBy = dto.CreatedBy?.Trim() ?? "API"
        };

        var items = dto.Items.Select(i => new PartPriceRequestLine
        {
            PartId = i.PartId,
            DMSPartCode = i.DMSPartCode?.Trim().ToUpperInvariant() ?? "",
            VieName = i.VieName?.Trim() ?? "",
            VINCode = i.VINCode?.Trim() ?? dto.VIN?.Trim(),
            DeliveryForm = i.DeliveryForm ?? (req.FlagIsCheck ? PartPriceDeliveryForm.VOR : PartPriceDeliveryForm.Regular),
            Quantity = i.Quantity <= 0 ? 1 : i.Quantity,
            Unit = string.IsNullOrWhiteSpace(i.Unit) ? "Cái" : i.Unit.Trim(),
            Remark = i.Remark?.Trim(),
            Status = ReqPartPriceLineStatus.Pending
        }).ToList();

        var id = await svc.CreatePartPriceRequestAsync(req, items);
        return Results.Created($"/api/part-price-requests/{id}", new { id, req.ReqPartPriceNo, message = "Đã lập phiếu đề nghị cung cấp giá phụ tùng thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/part-price-requests/{id:int}/send", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.SendPartPriceRequestToTSTAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/part-price-requests/{id:int}/response", async (int id, SimulatePartPriceResponseDto dto, IRoService svc) =>
{
    if (dto.Items == null || dto.Items.Count == 0)
        return Results.BadRequest(new { error = "Vui lòng cung cấp danh sách đơn giá phản hồi (Items)." });

    var list = dto.Items.Select(i => (i.LineId, i.TSTPartCode, i.TSTPrice, i.DateEffect ?? DateTime.Today)).ToList();
    var (ok, msg) = await svc.SimulateTSTResponseAsync(id, list);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/part-price-requests/{id:int}/approve", async (int id, ApprovePartPriceRequestDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.ApprovePartPriceRequestAsync(id, dto.ApprovedBy, dto.SyncToCatalog ?? true);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/part-price-requests/{id:int}/convert-order", async (int id, ConvertPriceRequestToOrderDto dto, IRoService svc) =>
{
    var (ok, msg, orderPartId) = await svc.ConvertToOrderPartAsync(id, dto.CreatedBy);
    return ok ? Results.Ok(new { orderPartId, message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/part-price-requests/{id:int}/cancel", async (int id, CancelPartPriceRequestDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.CancelPartPriceRequestAsync(id, dto.Reason);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/part-price-requests/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeletePartPriceRequestAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// =========================================================================
// MINIMAL APIS — TỪ ĐIỂN MÃ LỖI PHÀN NÀN & CHẨN ĐOÁN DỊCH VỤ XE
// (Ser_MST_ROComplaintDiagnosticError - MNU_QT_DL_QUANLYMALOIPHANNANVACHANDOAN)
// =========================================================================

app.MapGet("/api/complaint-diagnostic-errors", async (ComplaintErrorType? type, VehicleSystemGroup? group, string? q, bool? isActive, IRoService svc) =>
{
    var list = await svc.ComplaintDiagnosticErrorsAsync(type, group, q, isActive);
    return Results.Ok(list.Select(e => new
    {
        e.Id,
        e.ErrorCode,
        e.ErrorName,
        errorType = e.ErrorTypeName,
        errorTypeCode = e.ErrorTypeCode,
        errorTypeValue = (int)e.ErrorType,
        systemGroup = e.SystemGroupName,
        systemGroupValue = (int)e.SystemGroup,
        e.ErrorDesc,
        e.Remark,
        e.FlagActive,
        e.UsageCount,
        e.CreatedBy,
        e.CreatedAt,
        e.UpdatedAt
    }));
});

app.MapGet("/api/complaint-diagnostic-errors/summary", async (IRoService svc) =>
{
    var s = await svc.GetComplaintDiagnosticSummaryAsync();
    return Results.Ok(s);
});

app.MapGet("/api/complaint-diagnostic-errors/complaints", async (VehicleSystemGroup? group, IRoService svc) =>
{
    var list = await svc.GetActiveComplaintsAsync(group);
    return Results.Ok(list.Select(e => new
    {
        e.Id,
        e.ErrorCode,
        e.ErrorName,
        systemGroup = e.SystemGroupName,
        e.ErrorDesc,
        e.UsageCount
    }));
});

app.MapGet("/api/complaint-diagnostic-errors/diagnostics", async (VehicleSystemGroup? group, IRoService svc) =>
{
    var list = await svc.GetActiveDiagnosticsAsync(group);
    return Results.Ok(list.Select(e => new
    {
        e.Id,
        e.ErrorCode,
        e.ErrorName,
        systemGroup = e.SystemGroupName,
        e.ErrorDesc,
        e.Remark,
        e.UsageCount
    }));
});

app.MapGet("/api/complaint-diagnostic-errors/{id:int}", async (int id, IRoService svc) =>
{
    var e = await svc.GetComplaintDiagnosticErrorAsync(id);
    if (e == null) return Results.NotFound(new { error = "Không tìm thấy mã lỗi." });
    return Results.Ok(new
    {
        e.Id,
        e.ErrorCode,
        e.ErrorName,
        errorType = e.ErrorTypeName,
        errorTypeCode = e.ErrorTypeCode,
        errorTypeValue = (int)e.ErrorType,
        systemGroup = e.SystemGroupName,
        systemGroupValue = (int)e.SystemGroup,
        e.ErrorDesc,
        e.Remark,
        e.FlagActive,
        e.UsageCount,
        e.CreatedBy,
        e.CreatedAt,
        e.UpdatedAt
    });
});

app.MapGet("/api/complaint-diagnostic-errors/by-code/{code}", async (string code, IRoService svc) =>
{
    var e = await svc.GetComplaintDiagnosticErrorByCodeAsync(code);
    if (e == null) return Results.NotFound(new { error = $"Không tìm thấy mã lỗi '{code}'." });
    return Results.Ok(new
    {
        e.Id,
        e.ErrorCode,
        e.ErrorName,
        errorType = e.ErrorTypeName,
        errorTypeCode = e.ErrorTypeCode,
        errorTypeValue = (int)e.ErrorType,
        systemGroup = e.SystemGroupName,
        systemGroupValue = (int)e.SystemGroup,
        e.ErrorDesc,
        e.Remark,
        e.FlagActive,
        e.UsageCount,
        e.CreatedBy,
        e.CreatedAt,
        e.UpdatedAt
    });
});

app.MapPost("/api/complaint-diagnostic-errors", async (CreateComplaintDiagnosticErrorDto dto, IRoService svc) =>
{
    try
    {
        var err = new ComplaintDiagnosticError
        {
            ErrorCode = dto.ErrorCode,
            ErrorName = dto.ErrorName,
            ErrorType = dto.ErrorType,
            SystemGroup = dto.SystemGroup,
            ErrorDesc = dto.ErrorDesc,
            Remark = dto.Remark,
            FlagActive = dto.FlagActive ?? true,
            CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "API" : dto.CreatedBy.Trim()
        };
        var id = await svc.CreateComplaintDiagnosticErrorAsync(err);
        return Results.Created($"/api/complaint-diagnostic-errors/{id}", new { id, err.ErrorCode, message = "Đã tạo mã lỗi thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/complaint-diagnostic-errors/{id:int}", async (int id, UpdateComplaintDiagnosticErrorDto dto, IRoService svc) =>
{
    var update = new ComplaintDiagnosticError
    {
        ErrorCode = dto.ErrorCode,
        ErrorName = dto.ErrorName,
        ErrorType = dto.ErrorType,
        SystemGroup = dto.SystemGroup,
        ErrorDesc = dto.ErrorDesc,
        Remark = dto.Remark,
        FlagActive = dto.FlagActive
    };
    var (ok, msg) = await svc.UpdateComplaintDiagnosticErrorAsync(id, update);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/complaint-diagnostic-errors/{id:int}/toggle-active", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.ToggleComplaintDiagnosticErrorActiveAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/complaint-diagnostic-errors/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteComplaintDiagnosticErrorAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/complaint-diagnostic-errors/apply-to-ro", async (ApplyErrorToRoDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.ApplyErrorToROAsync(dto.RoId, dto.ErrorId, dto.Target ?? "AUTO");
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// =========================================================================
// MINIMAL APIS — CHĂM SÓC KHÁCH HÀNG 72H & KIỂM SOÁT PHẢN TU RE-REPAIR
// (Ser_CustomerCare72h - MNU_QT_DL_QUANLYCHAMSOCKHACHHANG72H)
// =========================================================================

app.MapGet("/api/customercare72h", async (CustomerCare72hStatus? status, string? q, bool? needFeedbackOnly, DateTime? fromDate, DateTime? toDate, IRoService svc) =>
{
    var list = await svc.CustomerCare72hsAsync(status, q, needFeedbackOnly, fromDate, toDate);
    return Results.Ok(list.Select(c => new
    {
        c.Id,
        c.Care72No,
        c.ROId,
        roCode = c.RO?.Code,
        c.CarId,
        plate = c.Car?.Plate,
        model = c.Car?.Model,
        c.CustomerId,
        customerName = c.Customer?.Name,
        customerPhone = c.Customer?.Phone,
        status = Ui.CustomerCare72hStatus(c.Status).text,
        statusCode = Ui.CustomerCare72hStatus(c.Status).code,
        statusValue = (int)c.Status,
        c.ROFinishedDate,
        c.ScheduledDate,
        c.ContactedDate,
        c.ContactedBy,
        c.ServiceExplained,
        c.BasicNeedsMet,
        c.HasTechnicalProblem,
        c.ProblemDetails,
        c.FixedRightFirstTime,
        firftStatus = Ui.FirftBadge(c.FixedRightFirstTime).text,
        c.SatisfactionRating,
        satisfactionText = Ui.SatisfactionStars(c.SatisfactionRating),
        c.CustomerFeedback,
        c.IsReRepairAlert,
        c.ReRepairAction,
        c.ReRepairROId,
        reRepairRoCode = c.ReRepairRO?.Code,
        c.InternalNote,
        c.CreatedAt,
        c.CreatedBy
    }));
});

app.MapGet("/api/customercare72h/summary", async (IRoService svc) =>
{
    var summary = await svc.GetCustomerCare72hSummaryAsync();
    return Results.Ok(summary);
});

app.MapGet("/api/customercare72h/{id:int}", async (int id, IRoService svc) =>
{
    var c = await svc.GetCustomerCare72hAsync(id);
    if (c == null) return Results.NotFound(new { error = "Không tìm thấy phiếu CSKH 72h." });
    return Results.Ok(new
    {
        c.Id,
        c.Care72No,
        c.ROId,
        roCode = c.RO?.Code,
        roFinishedAt = c.RO?.FinishedAt,
        roTotal = c.RO?.Total,
        c.CarId,
        plate = c.Car?.Plate,
        model = c.Car?.Model,
        vin = c.Car?.Vin,
        c.CustomerId,
        customerName = c.Customer?.Name,
        customerPhone = c.Customer?.Phone,
        customerEmail = c.Customer?.Email,
        status = Ui.CustomerCare72hStatus(c.Status).text,
        statusCode = Ui.CustomerCare72hStatus(c.Status).code,
        statusValue = (int)c.Status,
        c.ROFinishedDate,
        c.ScheduledDate,
        c.ContactedDate,
        c.ContactedBy,
        c.ServiceExplained,
        c.BasicNeedsMet,
        c.HasTechnicalProblem,
        c.ProblemDetails,
        c.FixedRightFirstTime,
        firftStatus = Ui.FirftBadge(c.FixedRightFirstTime).text,
        c.SatisfactionRating,
        satisfactionText = Ui.SatisfactionStars(c.SatisfactionRating),
        c.CustomerFeedback,
        c.IsReRepairAlert,
        c.ReRepairAction,
        c.ReRepairROId,
        reRepairRoCode = c.ReRepairRO?.Code,
        c.ReRepairAppointmentId,
        c.InternalNote,
        c.CreatedAt,
        c.CreatedBy,
        lines = c.RO?.Lines.Select(l => new
        {
            l.Id,
            type = l.Type.ToString(),
            l.Name,
            l.Quantity,
            l.UnitPrice,
            l.Amount,
            partCode = l.Part?.Code
        })
    });
});

app.MapPost("/api/customercare72h", async (CreateCustomerCare72hDto dto, IRoService svc) =>
{
    try
    {
        var care = new CustomerCare72h
        {
            ROId = dto.RoId,
            InternalNote = dto.InternalNote?.Trim(),
            CreatedBy = dto.CreatedBy ?? "api"
        };
        var id = await svc.CreateCustomerCare72hAsync(care);
        return Results.Created($"/api/customercare72h/{id}", new { id, care.Care72No, message = "Đã lập phiếu CSKH 72h thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/customercare72h/generate-from-ro/{roId:int}", async (int roId, IRoService svc) =>
{
    try
    {
        var id = await svc.GenerateCustomerCare72hFromROAsync(roId, "api");
        return Results.Ok(new { id, message = "Đã kích hoạt phiếu CSKH 72h từ Lệnh sửa chữa hoàn tất." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/customercare72h/{id:int}/survey", async (int id, SubmitCare72hSurveyDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.UpdateCustomerCare72hSurveyAsync(id, dto.Status,
        dto.ServiceExplained, dto.BasicNeedsMet, dto.HasTechnicalProblem, dto.ProblemDetails,
        dto.FixedRightFirstTime, dto.SatisfactionRating, dto.CustomerFeedback, dto.ReRepairAction,
        dto.InternalNote, dto.ContactedBy);

    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/customercare72h/{id:int}/re-repair", async (int id, CreateReRepairFromCare72hDto dto, IRoService svc) =>
{
    var (ok, msg, roId) = await svc.CreateReRepairFromCare72hAsync(id, dto.Technician, dto.Note);
    return ok ? Results.Ok(new { message = msg, roId }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/customercare72h/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteCustomerCare72hAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Chăm sóc sinh nhật khách hàng & Voucher tri ân (Ser_CustomerCareBth / FrmCSCCustomerCareDOB)
app.MapGet("/api/customer-care-birthdays", async (int? month, CustomerCareBirthdayStatus? status, string? q, bool? todayOnly, IRoService svc) =>
{
    var list = await svc.CustomerCareBirthdaysAsync(month, status, q, todayOnly);
    return Results.Ok(list.Select(c => new
    {
        c.Id,
        c.CareBthNo,
        c.CustomerId,
        customerName = c.Customer.Name,
        customerPhone = c.Customer.Phone,
        customerCode = c.Customer.Code,
        c.CarId,
        plate = c.Car?.Plate,
        model = c.Car?.Model,
        c.DateOfBirth,
        c.DateBth,
        birthMonth = c.BirthMonth,
        birthDay = c.BirthDay,
        currentAge = c.CurrentAge,
        isTodayBirthday = c.IsTodayBirthday,
        isThisMonthBirthday = c.IsThisMonthBirthday,
        daysUntilBirthday = c.DaysUntilBirthday,
        status = Ui.CustomerCareBirthdayStatus(c.Status).text,
        statusCode = Ui.CustomerCareBirthdayStatus(c.Status).code,
        statusValue = (int)c.Status,
        c.ContactDate,
        c.ContactedBy,
        channel = Ui.BirthdayContactChannel(c.ContactChannel).text,
        channelValue = (int)c.ContactChannel,
        c.Remark,
        c.GiftVoucherCode,
        c.GiftVoucherValue,
        c.DiscountPercent,
        c.VoucherValidUntil,
        c.IsVoucherUsed,
        c.UsedInROId,
        usedInRoCode = c.UsedInRO?.Code,
        c.AppointmentId,
        appointmentNo = c.Appointment?.AppNo,
        c.CreatedAt,
        c.CreatedBy
    }));
});

app.MapGet("/api/customer-care-birthdays/summary", async (IRoService svc) =>
{
    var summary = await svc.GetCustomerCareBirthdaySummaryAsync();
    return Results.Ok(summary);
});

app.MapGet("/api/customer-care-birthdays/{id:int}", async (int id, IRoService svc) =>
{
    var c = await svc.GetCustomerCareBirthdayAsync(id);
    if (c == null) return Results.NotFound(new { error = "Không tìm thấy phiếu CSKH sinh nhật." });
    return Results.Ok(new
    {
        c.Id,
        c.CareBthNo,
        c.CustomerId,
        customerName = c.Customer.Name,
        customerPhone = c.Customer.Phone,
        customerCode = c.Customer.Code,
        customerEmail = c.Customer.Email,
        c.CarId,
        plate = c.Car?.Plate,
        model = c.Car?.Model,
        c.DateOfBirth,
        c.DateBth,
        birthMonth = c.BirthMonth,
        birthDay = c.BirthDay,
        currentAge = c.CurrentAge,
        isTodayBirthday = c.IsTodayBirthday,
        isThisMonthBirthday = c.IsThisMonthBirthday,
        daysUntilBirthday = c.DaysUntilBirthday,
        status = Ui.CustomerCareBirthdayStatus(c.Status).text,
        statusCode = Ui.CustomerCareBirthdayStatus(c.Status).code,
        statusValue = (int)c.Status,
        c.ContactDate,
        c.ContactedBy,
        channel = Ui.BirthdayContactChannel(c.ContactChannel).text,
        channelValue = (int)c.ContactChannel,
        c.Remark,
        c.GiftVoucherCode,
        c.GiftVoucherValue,
        c.DiscountPercent,
        c.VoucherValidUntil,
        c.IsVoucherUsed,
        c.UsedInROId,
        usedInRoCode = c.UsedInRO?.Code,
        c.AppointmentId,
        appointmentNo = c.Appointment?.AppNo,
        c.CreatedAt,
        c.CreatedBy
    });
});

app.MapPost("/api/customer-care-birthdays", async (CreateCustomerCareBirthdayDto dto, IRoService svc) =>
{
    try
    {
        var care = new CustomerCareBirthday
        {
            CustomerId = dto.CustomerId,
            DateOfBirth = dto.DateOfBirth,
            DateBth = dto.DateOfBirth.HasValue ? CustomerCareBirthday.CalculateDateBth(dto.DateOfBirth.Value, DateTime.Today.Year) : DateTime.Today,
            GiftVoucherCode = dto.GiftVoucherCode?.Trim(),
            GiftVoucherValue = dto.GiftVoucherValue ?? 300_000m,
            DiscountPercent = dto.DiscountPercent ?? 10m,
            Remark = dto.Remark?.Trim(),
            CreatedBy = dto.CreatedBy ?? "api"
        };
        var id = await svc.CreateCustomerCareBirthdayAsync(care);
        return Results.Created($"/api/customer-care-birthdays/{id}", new { id, care.CareBthNo, message = "Đã lập phiếu CSKH sinh nhật thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/customer-care-birthdays/scan-auto", async (int? year, IRoService svc) =>
{
    var (gen, skip) = await svc.ScanAndGenerateBirthdayCaresAsync(year, "api-scanner");
    return Results.Ok(new { generated = gen, skipped = skip, message = $"Quét tự động hoàn tất: {gen} mới, {skip} đã có." });
});

app.MapPost("/api/customer-care-birthdays/{id:int}/contact", async (int id, UpdateBirthdayContactDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.UpdateCustomerCareBirthdayContactAsync(id, dto.Status, dto.Channel, dto.Remark, dto.GiftVoucherCode, dto.GiftVoucherValue, dto.DiscountPercent, dto.ValidUntil, dto.ContactedBy);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/customer-care-birthdays/{id:int}/book-appointment", async (int id, BookBirthdayAppointmentDto dto, IRoService svc) =>
{
    var (ok, msg, appId) = await svc.BookAppointmentFromBirthdayCareAsync(id, dto.AppointmentDate, dto.ServiceType, dto.Note);
    return ok ? Results.Ok(new { message = msg, appointmentId = appId }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/customer-care-birthdays/{id:int}/apply-to-ro/{roId:int}", async (int id, int roId, IRoService svc) =>
{
    var (ok, msg) = await svc.ApplyBirthdayVoucherToROAsync(id, roId);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/customer-care-birthdays/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteCustomerCareBirthdayAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Định mức giờ công bảo hành tiêu chuẩn Flat Rate (Ser_MST_ROWarrantyWork & Ser_MST_ROWarrantyType)
app.MapGet("/api/warranty-works", async (string? model, WarrantyLaborGroup? group, WarrantyCoverageType? coverage, string? q, bool? isActive, IRoService svc) =>
{
    var list = await svc.WarrantyWorksAsync(model, group, coverage, q, isActive);
    return Results.Ok(list.Select(w => new
    {
        w.Id,
        w.Code,
        w.Name,
        w.Model,
        laborGroup = Ui.WarrantyLaborGroup(w.LaborGroup).text,
        laborGroupIcon = Ui.WarrantyLaborGroup(w.LaborGroup).icon,
        laborGroupValue = (int)w.LaborGroup,
        coverageType = Ui.WarrantyCoverageType(w.CoverageType).text,
        coverageTypeCode = Ui.WarrantyCoverageType(w.CoverageType).code,
        coverageTypeValue = (int)w.CoverageType,
        w.AppTypeCode,
        w.EngineType,
        w.RateHour,
        w.RatePrice,
        w.Price,
        w.VatPercent,
        w.TotalWithVat,
        w.RequiredPhotos,
        w.Remark,
        w.FlagActive,
        w.UsageCount,
        w.CreatedBy,
        w.CreatedAt,
        w.UpdatedAt
    }));
});

app.MapGet("/api/warranty-works/summary", async (IRoService svc) =>
{
    var s = await svc.GetWarrantyWorkSummaryAsync();
    return Results.Ok(s);
});

app.MapGet("/api/warranty-works/by-model", async (string model, IRoService svc) =>
{
    var list = await svc.WarrantyWorksAsync(model, null, null, null, true);
    return Results.Ok(list.Select(w => new
    {
        w.Id,
        w.Code,
        w.Name,
        w.Model,
        w.LaborGroup,
        laborGroupName = Ui.WarrantyLaborGroup(w.LaborGroup).text,
        w.RateHour,
        w.RatePrice,
        w.Price,
        w.TotalWithVat
    }));
});

app.MapGet("/api/warranty-works/{id:int}", async (int id, IRoService svc) =>
{
    var w = await svc.GetWarrantyWorkAsync(id);
    if (w == null) return Results.NotFound(new { error = "Không tìm thấy công việc bảo hành định mức." });
    return Results.Ok(new
    {
        w.Id,
        w.Code,
        w.Name,
        w.Model,
        laborGroup = Ui.WarrantyLaborGroup(w.LaborGroup).text,
        laborGroupIcon = Ui.WarrantyLaborGroup(w.LaborGroup).icon,
        laborGroupValue = (int)w.LaborGroup,
        coverageType = Ui.WarrantyCoverageType(w.CoverageType).text,
        coverageTypeCode = Ui.WarrantyCoverageType(w.CoverageType).code,
        coverageTypeValue = (int)w.CoverageType,
        w.AppTypeCode,
        w.EngineType,
        w.RateHour,
        w.RatePrice,
        w.Price,
        w.VatPercent,
        w.TotalWithVat,
        w.RequiredPhotos,
        w.Remark,
        w.FlagActive,
        w.UsageCount,
        w.CreatedBy,
        w.CreatedAt,
        w.UpdatedAt,
        appliedLines = w.RepairLines.Select(l => new
        {
            l.Id,
            roId = l.ROId,
            roCode = l.RO?.Code,
            plate = l.RO?.Car?.Plate,
            model = l.RO?.Car?.Model,
            customer = l.RO?.Customer?.Name,
            hours = l.Quantity,
            rate = l.UnitPrice,
            amount = l.Amount,
            roStatus = l.RO != null ? Ui.Status(l.RO.Status).text : ""
        })
    });
});

app.MapGet("/api/warranty-works/by-code/{code}", async (string code, IRoService svc) =>
{
    var w = await svc.GetWarrantyWorkByCodeAsync(code);
    if (w == null) return Results.NotFound(new { error = $"Không tìm thấy mã công việc bảo hành '{code}'." });
    return Results.Ok(new
    {
        w.Id,
        w.Code,
        w.Name,
        w.Model,
        laborGroup = Ui.WarrantyLaborGroup(w.LaborGroup).text,
        laborGroupValue = (int)w.LaborGroup,
        coverageType = Ui.WarrantyCoverageType(w.CoverageType).text,
        w.RateHour,
        w.RatePrice,
        w.Price,
        w.TotalWithVat,
        w.FlagActive
    });
});

app.MapPost("/api/warranty-works", async (CreateWarrantyWorkDto dto, IRoService svc) =>
{
    try
    {
        var work = new WarrantyWork
        {
            Code = dto.Code,
            Name = dto.Name,
            Model = string.IsNullOrWhiteSpace(dto.Model) ? "Tất cả dòng xe" : dto.Model.Trim(),
            LaborGroup = dto.LaborGroup,
            CoverageType = dto.CoverageType ?? WarrantyCoverageType.NewCar,
            AppTypeCode = dto.AppTypeCode?.Trim(),
            EngineType = dto.EngineType?.Trim(),
            RateHour = dto.RateHour <= 0 ? 1.0m : dto.RateHour,
            RatePrice = dto.RatePrice <= 0 ? 300_000m : dto.RatePrice,
            VatPercent = dto.VatPercent ?? 8,
            RequiredPhotos = dto.RequiredPhotos?.Trim(),
            Remark = dto.Remark?.Trim(),
            FlagActive = dto.FlagActive ?? true,
            CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "API" : dto.CreatedBy.Trim()
        };
        var id = await svc.CreateWarrantyWorkAsync(work);
        return Results.Created($"/api/warranty-works/{id}", new { id, work.Code, message = "Đã tạo công việc bảo hành định mức thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/warranty-works/{id:int}", async (int id, UpdateWarrantyWorkDto dto, IRoService svc) =>
{
    var update = new WarrantyWork
    {
        Code = dto.Code,
        Name = dto.Name,
        Model = dto.Model,
        LaborGroup = dto.LaborGroup,
        CoverageType = dto.CoverageType,
        AppTypeCode = dto.AppTypeCode,
        EngineType = dto.EngineType,
        RateHour = dto.RateHour,
        RatePrice = dto.RatePrice,
        VatPercent = dto.VatPercent,
        RequiredPhotos = dto.RequiredPhotos,
        Remark = dto.Remark,
        FlagActive = dto.FlagActive,
        UpdatedBy = dto.UpdatedBy ?? "API"
    };
    var (ok, msg) = await svc.UpdateWarrantyWorkAsync(id, update);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/warranty-works/{id:int}/toggle-active", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.ToggleWarrantyWorkActiveAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/warranty-works/{id:int}/apply-to-ro", async (int id, ApplyWarrantyWorkToRoDto dto, IRoService svc) =>
{
    var (ok, msg, lineId) = await svc.ApplyWarrantyWorkToRoAsync(id, dto.RoId, dto.CustomHours, dto.Note);
    return ok ? Results.Ok(new { message = msg, lineId }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/warranty-works/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteWarrantyWorkAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// --- Maintenance Interval & Milestone Settings Minimal APIs (Ser_MST_ROMaintanceSetting) ---
app.MapGet("/api/maintenance-settings", async (int? minKm, int? maxKm, MaintenanceLevel? level, bool? flagWarranty, bool? flagActive, string? q, IRoService svc) =>
    Results.Ok(await svc.MaintenanceSettingsAsync(minKm, maxKm, level, flagWarranty, flagActive, q)));

app.MapGet("/api/maintenance-settings/summary", async (IRoService svc) =>
    Results.Ok(await svc.GetMaintenanceSettingSummaryAsync()));

app.MapGet("/api/maintenance-settings/suggest", async (int km, IRoService svc) =>
    Results.Ok(await svc.SuggestMaintenanceForKmAsync(km)));

app.MapGet("/api/maintenance-settings/{id:int}", async (int id, IRoService svc) =>
{
    var item = await svc.GetMaintenanceSettingAsync(id);
    return item != null ? Results.Ok(item) : Results.NotFound(new { error = "Không tìm thấy thiết lập bảo dưỡng." });
});

app.MapGet("/api/maintenance-settings/by-romsid/{romsId}", async (string romsId, IRoService svc) =>
{
    var item = await svc.GetMaintenanceSettingByRomsIdAsync(romsId);
    return item != null ? Results.Ok(item) : Results.NotFound(new { error = $"Không tìm thấy thiết lập với ROMSID '{romsId}'." });
});

app.MapPost("/api/maintenance-settings", async (CreateMaintenanceSettingDto dto, IRoService svc) =>
{
    try
    {
        var setting = new MaintenanceSetting
        {
            ROMSID = dto.ROMSID,
            Name = dto.Name ?? "",
            Km = dto.Km,
            Maintances = dto.Maintances ?? 1,
            Level = dto.Level ?? MaintenanceLevel.Level1Minor,
            MonthsInterval = dto.MonthsInterval ?? 6,
            TakingTimeHours = dto.TakingTimeHours ?? 1.0m,
            EstimatedCost = dto.EstimatedCost ?? 650_000m,
            ServicePackageId = dto.ServicePackageId,
            RequiredChecklist = dto.RequiredChecklist,
            Description = dto.Description,
            FlagWarranty = dto.FlagWarranty ?? true,
            FlagActive = dto.FlagActive ?? true,
            CreatedBy = dto.CreatedBy ?? "API"
        };
        var id = await svc.CreateMaintenanceSettingAsync(setting);
        return Results.Created($"/api/maintenance-settings/{id}", new { id, romsId = setting.ROMSID });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/maintenance-settings/{id:int}", async (int id, UpdateMaintenanceSettingDto dto, IRoService svc) =>
{
    var input = new MaintenanceSetting
    {
        ROMSID = dto.ROMSID,
        Name = dto.Name,
        Km = dto.Km,
        Maintances = dto.Maintances,
        Level = dto.Level,
        MonthsInterval = dto.MonthsInterval,
        TakingTimeHours = dto.TakingTimeHours,
        EstimatedCost = dto.EstimatedCost,
        ServicePackageId = dto.ServicePackageId,
        RequiredChecklist = dto.RequiredChecklist,
        Description = dto.Description,
        FlagWarranty = dto.FlagWarranty,
        FlagActive = dto.FlagActive,
        UpdatedBy = dto.UpdatedBy ?? "API"
    };
    var (ok, msg) = await svc.UpdateMaintenanceSettingAsync(id, input);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/maintenance-settings/{id:int}/toggle-active", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.ToggleMaintenanceSettingActiveAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/maintenance-settings/{id:int}/apply-to-ro", async (int id, ApplyMaintenanceToRoDto dto, IRoService svc) =>
{
    var (ok, msg) = await svc.ApplyMaintenanceToRoAsync(id, dto.RoId, dto.AddPackageCombo ?? true);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/maintenance-settings/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteMaintenanceSettingAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// --- Warranty Type Catalog Minimal APIs (Ser_MST_ROWarrantyType / Ser_MST_ROWarrantyType_PhotoType / Ser_MST_ROWarrantyPhotoType) ---
app.MapGet("/api/warranty-types", async (WarrantyTypeCode? typeCode, bool? flagActive, string? q, IRoService svc) =>
{
    var list = await svc.WarrantyTypesAsync(typeCode, flagActive, q);
    return Results.Ok(list.Select(t => new
    {
        t.Id,
        t.ROWTID,
        typeCode = t.TypeCode.ToString(),
        typeCodeValue = (int)t.TypeCode,
        t.TypeName,
        detailCode = t.DetailCode.ToString(),
        detailCodeValue = (int)t.DetailCode,
        t.DetailName,
        t.PhotoTypeDisplay,
        t.FlagActive,
        photoCount = t.Photos.Count,
        photos = t.Photos.Select(p => new { p.ROWPTCode, p.ROWPTName })
    }));
});

app.MapGet("/api/warranty-types/summary", async (IRoService svc) =>
    Results.Ok(await svc.GetWarrantyTypeSummaryAsync()));

app.MapGet("/api/warranty-types/photo-types", async (bool? flagActive, IRoService svc) =>
    Results.Ok(await svc.WarrantyPhotoTypesAsync(flagActive)));

app.MapGet("/api/warranty-types/{id:int}", async (int id, IRoService svc) =>
{
    var t = await svc.GetWarrantyTypeAsync(id);
    if (t == null) return Results.NotFound(new { error = "Không tìm thấy loại bảo hành." });
    return Results.Ok(new
    {
        t.Id,
        t.ROWTID,
        typeCode = t.TypeCode.ToString(),
        typeCodeValue = (int)t.TypeCode,
        t.TypeName,
        detailCode = t.DetailCode.ToString(),
        detailCodeValue = (int)t.DetailCode,
        t.DetailName,
        t.PhotoTypeDisplay,
        t.FlagActive,
        t.LogLuDateTime,
        t.LogLUBy,
        photos = t.Photos.Select(p => new { p.Id, p.ROWPTCode, p.ROWPTName })
    });
});

app.MapPost("/api/warranty-types", async (CreateWarrantyTypeDto dto, IRoService svc) =>
{
    try
    {
        var type = new WarrantyType
        {
            ROWTID = dto.ROWTID ?? "",
            TypeCode = dto.TypeCode,
            TypeName = dto.TypeName ?? "",
            DetailCode = dto.DetailCode,
            DetailName = dto.DetailName ?? "",
            FlagActive = dto.FlagActive ?? true
        };
        var photos = (dto.PhotoCodes ?? []).Select(c => new WarrantyTypePhoto { ROWPTCode = c }).ToList();
        var id = await svc.CreateWarrantyTypeAsync(type, photos);
        return Results.Created($"/api/warranty-types/{id}", new { id, typeCode = type.TypeCode.ToString(), detailCode = type.DetailCode.ToString(), message = "Đã tạo loại bảo hành RO thành công." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/warranty-types/{id:int}", async (int id, UpdateWarrantyTypeDto dto, IRoService svc) =>
{
    var input = new WarrantyType
    {
        TypeCode = dto.TypeCode,
        TypeName = dto.TypeName ?? "",
        DetailCode = dto.DetailCode,
        DetailName = dto.DetailName ?? "",
        FlagActive = dto.FlagActive,
        UpdatedBy = dto.UpdatedBy ?? "API"
    };
    var photos = dto.PhotoCodes?.Select(c => new WarrantyTypePhoto { ROWPTCode = c }).ToList();
    var (ok, msg) = await svc.UpdateWarrantyTypeAsync(id, input, photos);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/warranty-types/{id:int}/toggle-active", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.ToggleWarrantyTypeActiveAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/warranty-types/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteWarrantyTypeAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Chia sẻ phụ tùng giữa các đại lý trong mạng lưới (SP_SharePart / SP_SharePart_Detail)
app.MapGet("/api/share-parts", async (string? dealerCode, string? q, DateTime? fromDate, DateTime? toDate, IRoService svc) =>
{
    var list = await svc.SharePartsAsync(dealerCode, q, fromDate, toDate);
    return Results.Ok(list.Select(s => new
    {
        s.Id,
        s.SharePartNo,
        s.DealerCode,
        s.DealerName,
        s.CreatedDate,
        s.CreatedBy,
        s.FlagLatest,
        s.Remark,
        s.ItemCount,
        s.TotalQuantityShare
    }));
});

app.MapGet("/api/share-parts/summary", async (IRoService svc) =>
{
    var s = await svc.GetSharePartSummaryAsync();
    return Results.Ok(new
    {
        s.TotalSheets,
        s.TotalLines,
        s.TotalQuantityShare,
        s.DealerCount,
        s.PartCount
    });
});

app.MapGet("/api/share-parts/{id:int}", async (int id, IRoService svc) =>
{
    var s = await svc.GetSharePartAsync(id);
    if (s == null) return Results.NotFound(new { error = "Không tìm thấy phiếu chia sẻ phụ tùng." });
    return Results.Ok(new
    {
        s.Id,
        s.SharePartNo,
        s.DealerCode,
        s.DealerName,
        s.CreatedDate,
        s.CreatedBy,
        s.FlagLatest,
        s.Remark,
        s.ItemCount,
        s.TotalQuantityShare,
        lines = s.Lines.Select(l => new
        {
            l.Id,
            l.PartId,
            partCode = l.Part?.Code,
            partName = l.Part?.Name,
            unit = l.Part?.Unit,
            inStock = l.Part?.InStock,
            minStock = l.Part?.MinStock,
            l.QuantityShare,
            l.DealerCode,
            l.Remark
        })
    });
});

app.MapPost("/api/share-parts", async (CreateSharePartDto dto, IRoService svc) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(dto.DealerCode))
            return Results.BadRequest(new { error = "Cần mã đại lý chia sẻ (DealerCode)." });
        if (dto.Lines == null || dto.Lines.Count == 0)
            return Results.BadRequest(new { error = "Cần danh sách phụ tùng chia sẻ (Lines)." });

        var sheet = new SharePart
        {
            DealerCode = dto.DealerCode.Trim(),
            DealerName = dto.DealerName?.Trim() ?? "",
            Remark = dto.Remark?.Trim(),
            CreatedBy = dto.CreatedBy ?? "api"
        };
        var lines = dto.Lines.Select(l => new SharePartLine
        {
            PartId = l.PartId,
            QuantityShare = l.QuantityShare,
            DealerCode = string.IsNullOrWhiteSpace(l.DealerCode) ? dto.DealerCode.Trim() : l.DealerCode.Trim(),
            Remark = l.Remark?.Trim()
        }).ToList();

        var id = await svc.CreateSharePartAsync(sheet, lines);
        return Results.Ok(new { sharePartId = id, sharePartNo = sheet.SharePartNo, message = "Đã lập phiếu chia sẻ phụ tùng." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapDelete("/api/share-parts/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteSharePartAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Danh mục Loại công việc dịch vụ (Ser_MST_ServiceType)
app.MapGet("/api/service-types", async (string? dealerCode, string? q, IRoService svc) =>
{
    var list = await svc.ServiceTypesAsync(dealerCode, q);
    return Results.Ok(list.Select(t => new
    {
        t.Id,
        t.TypeName,
        t.DealerCode,
        serviceItemCount = t.ServiceItemCount,
        t.CreatedBy,
        t.CreatedAt,
        t.LogLUBy,
        t.LogLUDateTime
    }));
});

app.MapGet("/api/service-types/summary", async (IRoService svc) =>
{
    var s = await svc.GetServiceTypeSummaryAsync();
    return Results.Ok(new { s.TotalTypes, s.DealerCount, s.UsedTypes, s.UnusedTypes });
});

app.MapGet("/api/service-types/{id:int}", async (int id, IRoService svc) =>
{
    var t = await svc.GetServiceTypeAsync(id);
    if (t == null) return Results.NotFound(new { error = "Không tìm thấy loại công việc." });
    return Results.Ok(new
    {
        t.Id,
        t.TypeName,
        t.DealerCode,
        serviceItemCount = t.ServiceItemCount,
        serviceItems = t.ServiceItems.Select(s => new { s.Id, s.Code, s.Name, s.Price }),
        t.CreatedBy,
        t.CreatedAt,
        t.LogLUBy,
        t.LogLUDateTime
    });
});

app.MapPost("/api/service-types", async (CreateServiceTypeDto dto, IRoService svc) =>
{
    try
    {
        var type = new ServiceType
        {
            TypeName = dto.TypeName?.Trim() ?? "",
            DealerCode = string.IsNullOrWhiteSpace(dto.DealerCode) ? null : dto.DealerCode.Trim().ToUpperInvariant(),
            CreatedBy = dto.CreatedBy ?? "api"
        };
        var id = await svc.CreateServiceTypeAsync(type);
        return Results.Ok(new { serviceTypeId = id, message = "Đã thêm loại công việc vào danh mục." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/service-types/{id:int}", async (int id, UpdateServiceTypeDto dto, IRoService svc) =>
{
    var type = new ServiceType
    {
        Id = id,
        TypeName = dto.TypeName?.Trim() ?? "",
        DealerCode = string.IsNullOrWhiteSpace(dto.DealerCode) ? null : dto.DealerCode.Trim().ToUpperInvariant(),
        LogLUBy = dto.UpdatedBy ?? "api"
    };
    var (ok, msg) = await svc.UpdateServiceTypeAsync(type);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/service-types/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteServiceTypeAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Danh mục Nhóm vật tư / Loại vật tư (Ser_MST_PartGroup)
app.MapGet("/api/part-groups", async (string? dealerCode, string? q, bool? isActive, IRoService svc) =>
{
    var list = await svc.PartGroupsAsync(dealerCode, q, isActive);
    return Results.Ok(list.Select(g => new
    {
        g.Id,
        g.GroupCode,
        g.GroupName,
        g.DealerCode,
        g.ParentId,
        g.FamilyId,
        g.OrderId,
        g.IsActive,
        childCount = g.ChildCount,
        g.CreatedBy,
        g.CreatedAt,
        g.LogLUBy,
        g.LogLUDateTime
    }));
});

app.MapGet("/api/part-groups/summary", async (IRoService svc) =>
{
    var s = await svc.GetPartGroupSummaryAsync();
    return Results.Ok(new { s.TotalGroups, s.RootGroups, s.ChildGroups, s.DealerCount, s.ActiveGroups });
});

app.MapGet("/api/part-groups/{id:int}", async (int id, IRoService svc) =>
{
    var g = await svc.GetPartGroupAsync(id);
    if (g == null) return Results.NotFound(new { error = "Không tìm thấy nhóm vật tư." });
    return Results.Ok(new
    {
        g.Id,
        g.GroupCode,
        g.GroupName,
        g.DealerCode,
        g.ParentId,
        parentName = g.Parent?.GroupName,
        g.FamilyId,
        g.OrderId,
        g.IsActive,
        childCount = g.ChildCount,
        children = g.Children.Select(c => new { c.Id, c.GroupCode, c.GroupName }),
        g.CreatedBy,
        g.CreatedAt,
        g.LogLUBy,
        g.LogLUDateTime
    });
});

app.MapPost("/api/part-groups", async (CreatePartGroupDto dto, IRoService svc) =>
{
    try
    {
        var group = new PartGroup
        {
            GroupCode = dto.GroupCode?.Trim() ?? "",
            GroupName = dto.GroupName?.Trim() ?? "",
            DealerCode = string.IsNullOrWhiteSpace(dto.DealerCode) ? null : dto.DealerCode.Trim().ToUpperInvariant(),
            ParentId = (dto.ParentId.HasValue && dto.ParentId.Value > 0) ? dto.ParentId : null,
            OrderId = dto.OrderId,
            CreatedBy = dto.CreatedBy ?? "api"
        };
        var id = await svc.CreatePartGroupAsync(group);
        return Results.Ok(new { partGroupId = id, group.FamilyId, message = "Đã thêm nhóm vật tư vào danh mục." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/part-groups/{id:int}", async (int id, UpdatePartGroupDto dto, IRoService svc) =>
{
    var group = new PartGroup
    {
        Id = id,
        GroupCode = dto.GroupCode?.Trim() ?? "",
        GroupName = dto.GroupName?.Trim() ?? "",
        DealerCode = string.IsNullOrWhiteSpace(dto.DealerCode) ? null : dto.DealerCode.Trim().ToUpperInvariant(),
        ParentId = (dto.ParentId.HasValue && dto.ParentId.Value > 0) ? dto.ParentId : null,
        OrderId = dto.OrderId,
        LogLUBy = dto.UpdatedBy ?? "api"
    };
    var (ok, msg) = await svc.UpdatePartGroupAsync(group);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/part-groups/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeletePartGroupAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Lịch sử giá bán phụ tùng theo ngày hiệu lực (Ser_Inv_PartPrice)
app.MapGet("/api/part-prices", async (string? q, bool? isActive, DateTime? dateFrom, DateTime? dateTo, int? partId, IRoService svc) =>
{
    var list = await svc.PartPricesAsync(q, isActive, dateFrom, dateTo, partId);
    return Results.Ok(list.Select(p => new
    {
        p.Id,
        p.PartId,
        partCode = p.Part?.Code,
        partName = p.Part?.Name,
        p.Price,
        p.DateEffect,
        p.Remark,
        p.IsActive,
        p.CreatedBy,
        p.CreatedAt,
        p.LogLUBy,
        p.LogLUDateTime
    }));
});

app.MapGet("/api/part-prices/summary", async (IRoService svc) =>
{
    var s = await svc.GetPartPriceSummaryAsync();
    return Results.Ok(new { s.TotalPrices, s.ActivePrices, s.InactivePrices, s.PartCount, s.TstPartCount, s.AvgPrice });
});

app.MapGet("/api/part-prices/{id:int}", async (int id, IRoService svc) =>
{
    var p = await svc.GetPartPriceAsync(id);
    if (p == null) return Results.NotFound(new { error = "Không tìm thấy dòng giá phụ tùng." });
    return Results.Ok(new
    {
        p.Id,
        p.PartId,
        part = p.Part != null ? new { p.Part.Id, p.Part.Code, p.Part.Name, p.Part.Unit, p.Part.SalePrice } : null,
        p.Price,
        p.DateEffect,
        p.Remark,
        p.IsActive,
        p.CreatedBy,
        p.CreatedAt,
        p.LogLUBy,
        p.LogLUDateTime
    });
});

app.MapGet("/api/part-prices/history/{partId:int}", async (int partId, IRoService svc) =>
{
    var list = await svc.GetPartPriceHistoryAsync(partId);
    return Results.Ok(list.Select(p => new { p.Id, p.Price, p.DateEffect, p.Remark, p.IsActive }));
});

app.MapPost("/api/part-prices", async (CreatePartPriceDto dto, IRoService svc) =>
{
    try
    {
        var row = new PartPrice
        {
            PartId = dto.PartId,
            Price = dto.Price,
            DateEffect = dto.DateEffect ?? DateTime.Today,
            Remark = dto.Remark?.Trim(),
            CreatedBy = dto.CreatedBy ?? "api"
        };
        var id = await svc.CreatePartPriceAsync(row);
        return Results.Ok(new { partPriceId = id, message = "Đã lập giá bán phụ tùng." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/part-prices/{id:int}", async (int id, UpdatePartPriceDto dto, IRoService svc) =>
{
    var row = new PartPrice
    {
        Id = id,
        PartId = dto.PartId,
        Price = dto.Price,
        DateEffect = dto.DateEffect ?? DateTime.Today,
        Remark = dto.Remark?.Trim(),
        IsActive = dto.IsActive ?? true,
        LogLUBy = dto.UpdatedBy ?? "api"
    };
    var (ok, msg) = await svc.UpdatePartPriceAsync(row);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/part-prices/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeletePartPriceAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Thiết lập nguồn gốc model xe theo số khung (Mst_VINModelOrginal)
app.MapGet("/api/vin-model-origins", async (string? q, bool? isActive, string? modelCode, string? orginalCode, IRoService svc) =>
{
    var list = await svc.VinModelOriginsAsync(q, isActive, modelCode, orginalCode);
    return Results.Ok(list.Select(x => new
    {
        x.Id, x.VINCode, x.ModelCode, x.OrginalCode, x.IsActive, x.Remark,
        x.CreatedBy, x.CreatedAt, x.LogLUBy, x.LogLUDateTime
    }));
});

app.MapGet("/api/vin-model-origins/summary", async (IRoService svc) =>
{
    var s = await svc.GetVinModelOriginSummaryAsync();
    return Results.Ok(new
    {
        s.TotalRecords, s.ActiveRecords, s.InactiveRecords,
        s.ModelCount, s.OrginalCount, s.Vin4Count, s.Vin5Count
    });
});

app.MapGet("/api/vin-model-origins/{id:int}", async (int id, IRoService svc) =>
{
    var x = await svc.GetVinModelOriginAsync(id);
    if (x == null) return Results.NotFound(new { error = "Không tìm thấy dòng nguồn gốc model xe." });
    return Results.Ok(new { x.Id, x.VINCode, x.ModelCode, x.OrginalCode, x.IsActive, x.Remark, x.CreatedBy, x.CreatedAt, x.LogLUBy, x.LogLUDateTime });
});

app.MapGet("/api/vin-model-origins/by-vin/{vinCode}", async (string vinCode, IRoService svc) =>
{
    var x = await svc.GetVinModelOriginByVinAsync(vinCode);
    if (x == null) return Results.NotFound(new { error = $"Không tìm thấy nguồn gốc cho VIN '{vinCode}'." });
    return Results.Ok(new { x.Id, x.VINCode, x.ModelCode, x.OrginalCode, x.IsActive, x.Remark });
});

app.MapPost("/api/vin-model-origins", async (CreateVinModelOriginDto dto, IRoService svc) =>
{
    try
    {
        var row = new VinModelOrigin
        {
            VINCode = dto.VINCode ?? "",
            ModelCode = dto.ModelCode ?? "",
            OrginalCode = dto.OrginalCode ?? "",
            Remark = dto.Remark?.Trim(),
            CreatedBy = dto.CreatedBy ?? "api"
        };
        var id = await svc.CreateVinModelOriginAsync(row);
        return Results.Ok(new { vinModelOriginId = id, message = "Đã thêm nguồn gốc model xe." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/vin-model-origins/{id:int}", async (int id, UpdateVinModelOriginDto dto, IRoService svc) =>
{
    var row = new VinModelOrigin
    {
        Id = id,
        VINCode = dto.VINCode ?? "",
        ModelCode = dto.ModelCode ?? "",
        OrginalCode = dto.OrginalCode ?? "",
        IsActive = dto.IsActive ?? true,
        Remark = dto.Remark?.Trim(),
        LogLUBy = dto.UpdatedBy ?? "api"
    };
    var (ok, msg) = await svc.UpdateVinModelOriginAsync(row);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/vin-model-origins/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteVinModelOriginAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/vin-model-origins/import", async (ImportVinModelOriginsDto dto, IRoService svc) =>
{
    var rows = (dto.Rows ?? []).Select(r => new VinModelOrigin
    {
        VINCode = r.VINCode ?? "",
        ModelCode = r.ModelCode ?? "",
        OrginalCode = r.OrginalCode ?? ""
    }).ToList();
    var (ok, msg, added, updated) = await svc.ImportVinModelOriginsAsync(rows, dto.UserCode ?? "api");
    return ok ? Results.Ok(new { message = msg, added, updated }) : Results.BadRequest(new { error = msg });
});

// API Danh mục Thương hiệu xe (Ser_Mst_TradeMark)
app.MapGet("/api/trade-marks", async (string? q, bool? isActive, string? dealerCode, IRoService svc) =>
{
    var list = await svc.TradeMarksAsync(q, isActive, dealerCode);
    return Results.Ok(list.Select(x => new
    {
        x.Id,
        x.TradeMarkCode,
        x.TradeMarkName,
        x.DealerCode,
        x.IsActive,
        x.Logo,
        x.CreatedBy,
        x.CreatedAt,
        x.LogLUBy,
        x.LogLUDateTime
    }));
});

app.MapGet("/api/trade-marks/summary", async (IRoService svc) =>
{
    var s = await svc.GetTradeMarkSummaryAsync();
    return Results.Ok(new
    {
        s.TotalTradeMarks,
        s.ActiveTradeMarks,
        s.InactiveTradeMarks,
        s.DealerCount,
        s.WithLogoCount
    });
});

app.MapGet("/api/trade-marks/{id:int}", async (int id, IRoService svc) =>
{
    var x = await svc.GetTradeMarkAsync(id);
    if (x == null) return Results.NotFound(new { error = "Không tìm thấy thương hiệu xe." });
    return Results.Ok(new
    {
        x.Id,
        x.TradeMarkCode,
        x.TradeMarkName,
        x.DealerCode,
        x.IsActive,
        x.Logo,
        x.CreatedBy,
        x.CreatedAt,
        x.LogLUBy,
        x.LogLUDateTime
    });
});

app.MapGet("/api/trade-marks/by-code/{tradeMarkCode}", async (string tradeMarkCode, string? dealerCode, IRoService svc) =>
{
    var x = await svc.GetTradeMarkByCodeAsync(tradeMarkCode, dealerCode ?? "");
    if (x == null) return Results.NotFound(new { error = $"Không tìm thấy thương hiệu '{tradeMarkCode}'." });
    return Results.Ok(new { x.Id, x.TradeMarkCode, x.TradeMarkName, x.DealerCode, x.IsActive, x.Logo });
});

app.MapPost("/api/trade-marks", async (CreateTradeMarkDto dto, IRoService svc) =>
{
    try
    {
        var row = new TradeMark
        {
            TradeMarkCode = dto.TradeMarkCode ?? "",
            TradeMarkName = dto.TradeMarkName ?? "",
            DealerCode = dto.DealerCode ?? "",
            Logo = dto.Logo?.Trim(),
            CreatedBy = dto.CreatedBy ?? "api"
        };
        var id = await svc.CreateTradeMarkAsync(row);
        return Results.Ok(new { tradeMarkId = id, message = "Đã thêm thương hiệu xe." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/trade-marks/{id:int}", async (int id, UpdateTradeMarkDto dto, IRoService svc) =>
{
    var row = new TradeMark
    {
        Id = id,
        TradeMarkCode = dto.TradeMarkCode ?? "",
        TradeMarkName = dto.TradeMarkName ?? "",
        DealerCode = dto.DealerCode ?? "",
        IsActive = dto.IsActive ?? true,
        Logo = dto.Logo?.Trim(),
        LogLUBy = dto.UpdatedBy ?? "api"
    };
    var (ok, msg) = await svc.UpdateTradeMarkAsync(row);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/trade-marks/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteTradeMarkAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

// API Ảnh minh chứng Tiếp nhận - Giao xe theo dòng xe (Ser_Mst_ModelAudImage)
app.MapGet("/api/model-audit-images", async (string? modelCode, string? audType, bool? isActive, string? q, IRoService svc) =>
{
    var list = await svc.ModelAuditImagesAsync(modelCode, audType, isActive, q);
    return Results.Ok(list.Select(x => new
    {
        x.Id,
        x.ModelCode,
        x.ReceptionFAudType,
        x.FilePath,
        x.Remark,
        x.IsActive,
        x.CreatedBy,
        x.CreatedAt,
        x.LogLUBy,
        x.LogLUDateTime
    }));
});

app.MapGet("/api/model-audit-images/summary", async (IRoService svc) =>
{
    var s = await svc.GetModelAuditImageSummaryAsync();
    return Results.Ok(new { s.TotalImages, s.ActiveImages, s.InactiveImages, s.ModelCount, s.AudTypeCount });
});

app.MapGet("/api/model-audit-images/{id:int}", async (int id, IRoService svc) =>
{
    var x = await svc.GetModelAuditImageAsync(id);
    if (x == null) return Results.NotFound(new { error = "Không tìm thấy ảnh minh chứng." });
    return Results.Ok(new
    {
        x.Id,
        x.ModelCode,
        x.ReceptionFAudType,
        x.FilePath,
        x.Remark,
        x.IsActive,
        x.CreatedBy,
        x.CreatedAt,
        x.LogLUBy,
        x.LogLUDateTime
    });
});

app.MapPost("/api/model-audit-images", async (CreateModelAuditImageDto dto, IRoService svc) =>
{
    try
    {
        var row = new ModelAuditImage
        {
            ModelCode = dto.ModelCode ?? "",
            ReceptionFAudType = dto.ReceptionFAudType ?? "",
            FilePath = dto.FilePath ?? "",
            Remark = dto.Remark,
            CreatedBy = dto.CreatedBy ?? "api"
        };
        var id = await svc.CreateModelAuditImageAsync(row);
        return Results.Ok(new { modelAuditImageId = id, message = "Đã thêm ảnh minh chứng vào danh mục." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/model-audit-images/{id:int}", async (int id, UpdateModelAuditImageDto dto, IRoService svc) =>
{
    var row = new ModelAuditImage
    {
        Id = id,
        ModelCode = dto.ModelCode ?? "",
        ReceptionFAudType = dto.ReceptionFAudType ?? "",
        FilePath = dto.FilePath ?? "",
        Remark = dto.Remark,
        IsActive = dto.IsActive ?? true,
        LogLUBy = dto.UpdatedBy ?? "api"
    };
    var (ok, msg) = await svc.UpdateModelAuditImageAsync(row);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapDelete("/api/model-audit-images/{id:int}", async (int id, IRoService svc) =>
{
    var (ok, msg) = await svc.DeleteModelAuditImageAsync(id);
    return ok ? Results.Ok(new { message = msg }) : Results.BadRequest(new { error = msg });
});

app.MapPost("/api/orgs/register", async (RegisterOrgDto dto, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Name)) return Results.BadRequest(new { error = "Cần Name." });
    var org = new Org { Name = dto.Name.Trim(), ApiKey = "svc_" + Guid.NewGuid().ToString("N") };
    db.Orgs.Add(org); await db.SaveChangesAsync();
    return Results.Ok(new { orgId = org.Id, apiKey = org.ApiKey });
});

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();

record RegisterOrgDto(string Name);
record UpdateMaintenanceReminderDto(DateTime? ReminderDate, int? ReminderKm, bool WorkDoneSoon, string? MemberNo, string? UpdatedBy);
record CreateRoHistoryDto(int RoId, ROStatus Status, string? Note, string? UserCode);
record AdjustStockDto(decimal Quantity, string? Mode, string? Note);
record CreateWarrantyDto(int RoId, string IssueDesc, string DiagResult, string? ErrorCodeCD, string? ErrorCodePN, int? PartIdError, string? CreatedBy);
record TransitionWarrantyDto(WarrantyStatus ToStatus, decimal? ApprovedAmount, string? Note);
record CreateAppointmentDto(int CarId, DateTime AppointmentDate, AppointmentServiceType ServiceType, string? Advisor, string? Cavity, string? CustomerRequest, string? Note, string? Source);
record TransitionAppointmentDto(AppointmentStatus ToStatus, string? Reason);
record CheckInAppointmentDto(int Odometer, string? Technician);
record CreateStockInDto(string SupplierName, string? BillNo, DateTime? StockInDate, StockInType Type, string? Description, List<CreateStockInItemDto> Items);
record CreateStockInItemDto(int PartId, decimal Quantity, decimal? UnitPrice, decimal? VatPercent, string? Note);
record TransitionStockInDto(StockInStatus ToStatus, string? ApprovedBy, string? Note);
record CreateStockOutDto(StockOutType Type, int? RoId, string? RecipientName, DateTime? StockOutDate, string? Description, List<CreateStockOutItemDto> Items);
record CreateStockOutItemDto(int PartId, decimal Quantity, decimal? UnitPrice, decimal? VatPercent, string? Note);
record TransitionStockOutDto(StockOutStatus ToStatus, string? ApprovedBy, string? Note);
record CreateCustomerCareDto(int RoId, string? InternalNote, string? CreatedBy);
record SubmitCareSurveyDto(CustomerCareStatus Status, bool HasCarProblem, int? QualityRating, int? StaffRating, bool? WillingToReturn, int? FacilityRating, string? CustomerFeedback, string? InternalNote, string? ContactedBy);
record CreatePaymentDto(int RoId, DateTime? PaymentDate, PaymentMethod Method, PaymentStatus? Status, string? PayPersonName, string? PayPersonPhone, string? PayPersonIdCard, decimal? DiscountAmount, decimal? PaymentAmount, string? TransactionRef, string? Note, string? Cashier);
record TransitionPaymentDto(PaymentStatus ToStatus, string? Cashier, string? Note);
record CreateQuoteDto(int? CustomerId, string CustomerName, string? CustomerPhone, string? CustomerAddress, int? CarId, string? RecipientName, PaymentMethod PaymentMethod, DateTime? QuoteDate, DateTime? ValidUntil, string? Remark, string? Note, List<CreateQuoteItemDto> Items);
record CreateQuoteItemDto(int? PartId, string? PartCode, string? PartName, string? Unit, decimal Quantity, decimal? UnitPrice, decimal? DiscountPercent, decimal? VatPercent, string? Note);
record TransitionQuoteDto(QuoteStatus ToStatus);
record ConvertQuoteRoDto(string? Technician);
record CreateServicePackageDto(string? PackageNo, string Name, decimal? TakingTimeHours, string? Description, bool? IsPublic, bool? IsActive, List<CreateServicePackageItemDto>? Items);
record CreateServicePackageItemDto(LineType Type, int? PartId, string? Code, string? Name, string? Unit, decimal Quantity, decimal UnitPrice, decimal? VatPercent, ExpenseType? ExpenseType, string? Note);
record ApplyPackageRoDto(int RoId);
record ApplyPackageQuoteDto(int QuoteId);
record CreateOrderPartDto(string SupplierName, OrderPartDeliveryForm DeliveryForm, string? DeliveryLocation, DateTime? OrderDate, DateTime? EstimatedDeliverDate, string? VIN, int? ROId, string? Remark, List<CreateOrderPartItemDto> Items);
record CreateOrderPartItemDto(int PartId, decimal Quantity, decimal? UnitPrice, decimal? DiscountRate, decimal? VatPercent, decimal? ApprovedQuantity, string? Note);
record TransitionOrderPartDto(OrderPartStatus ToStatus, string? SupplierOrderNo, string? Note);
record CreateStockInFromOrderDto(string? ApprovedBy);
record CreateCavityDto(string? CavityNo, string CavityName, CavityType CavityType, string? LiftEquipment, string? AreaZone, string? Note, bool? IsActive);
record UpdateCavityDto(string CavityName, CavityType CavityType, string? LiftEquipment, string? AreaZone, string? Note);
record AssignCarToCavityDto(int RoId, string? Technician, DateTime? ExpectedFinish);
record ReleaseCavityDto(ROStatus? NextRoStatus);
record SetCavityStatusDto(CavityStatus Status, string? Note);
record CreateReceptionDto(int CarId, int? AppointmentId, int Odometer, int FuelLevel, string? LevelOfInspection, string CustomerRequest, string? ValuablesInCar, string? ExteriorCondition, bool? IsWarranty, bool? IsInsurance, bool? IsBackRepair, string? CreatedBy, List<CreateReceptionItemDto>? Items);
record CreateReceptionItemDto(string Group, string Code, string Name, AuditStatus ReceptionStatus, string? Note);
record CreateRoFromReceptionDto(string? Technician);
record DeliverReceptionDto(string? DeliveryBy, string? Note, List<DeliverReceptionItemDto>? Items);
record DeliverReceptionItemDto(int ItemId, AuditStatus DeliveryStatus);
record CreateAssignmentDto(int RoId, DateTime? SccPlanStart, DateTime? SccPlanFinish, int? SccCavityId, DateTime? ScdPlanStart, DateTime? ScdPlanFinish, int? ScdCavityId, DateTime? ScsPlanStart, DateTime? ScsPlanFinish, int? ScsCavityId, string? Note, string? CreatedBy, List<CreateAssignmentEngineerDto>? Engineers);
record CreateAssignmentEngineerDto(int EngineerId, WorkType WorkType, decimal? AssignedHours, bool? IsPrimary, string? Note);
record StartAssignmentDto(string? StartedBy);
record CompleteAssignmentDto(string? CompletedBy);
record CancelAssignmentDto(string? Reason);
record CreateEngineerDto(string? Code, string Name, string? Phone, string? SkillLevel, string? Specialty, int? GroupId);
record CreateGroupDto(string? Code, string Name, string? Leader, string? Note);
record CreateInsuranceCompanyDto(string? InsNo, string InsName, string? Address, string? Phone, string? Email, string? TaxCode, string? Hotline);
record CreateInsuranceContractDto(string? ContractNo, string? ContractCode, int InsuranceCompanyId, DateTime StartDate, DateTime FinishDate, InsurancePaymentType PaymentType, decimal PaymentLimit, decimal DiscountLaborRate, decimal DiscountPartRate, string? Note);
record CreateInsuranceClaimDto(int RoId, int CompanyId, int? ContractId, string PolicyNo, string? ClaimFileNo, string? SurveyorName, string? SurveyorPhone, DateTime? AccidentDate, string? AccidentLocation, string? AccidentDescription, decimal? DeductibleAmount, decimal? PenaltyAmount, string? CreatedBy, List<CreateInsuranceClaimItemDto>? Items);
record CreateInsuranceClaimItemDto(LineType Type, string Code, string Name, decimal Quantity, decimal UnitPrice, decimal? EstimatedAmount, decimal? ApprovedAmount, bool? IsApproved, string? Note);
record CreateInsuranceClaimFromRoDto(int RoId, int CompanyId, int? ContractId, string PolicyNo, string? ClaimFileNo, string? SurveyorName, string? SurveyorPhone, string? AccidentDescription, decimal? DeductibleAmount, decimal? PenaltyAmount, string? CreatedBy);
record TransitionInsuranceClaimDto(InsuranceClaimStatus ToStatus, decimal? ApprovedAmount, string? Note);
record CreateCampaignDto(string? CamMarketingNo, string CamMarketingName, string? CamMarketingDesc, DateTime? EffDateStart, DateTime? EffDateEnd, string? ConditionModel, string? ConditionPlateNo, string? ConditionVIN, decimal? DiscountLaborPercent, decimal? DiscountPartPercent, string? CreatedBy, List<CreateCampaignItemDto>? Items);
record CreateCampaignItemDto(int PartId, string? PartCode, string? PartName, decimal? PercentDiscount, decimal? MaxQuantity, string? Note);
record TransitionCampaignDto(CampaignMarketingStatus ToStatus, string? ApprovedBy);
record CreateCareMaceDto(int CarId, MaceType? MaceType, int? LastKm, int? NextKm, DateTime? MaceRecomentDate, string? Remark, string? CreatedBy);
record UpdateCareMaceCallDto(CustomerCareMaceStatus Status, DateTime? ContactDate, DateTime? ApointDate, string? Remark, string? ContactBy);
record ConvertMaceToAppointmentDto(string? Advisor, string? Cavity, string? Note);
record CreateStockAdjDto(string? StockAdjNo, StockAdjType? Type, string? StorageCode, DateTime? StockAdjDate, string? Remark, string? CreatedBy, List<CreateStockAdjItemDto> Items);
record CreateStockAdjItemDto(int PartId, decimal ActualQuantity, string? ToLocation, string? Note);
record TransitionStockAdjDto(StockAdjStatus ToStatus, string? ApprovedBy, string? Note);
record UpdateStockAdjItemsDto(List<UpdateStockAdjItemLineDto> Items);
record UpdateStockAdjItemLineDto(int ItemId, decimal ActualQuantity, string? ToLocation, string? Note);
record CreateBulletinDto(string? BulletinNo, string? BulletinNoHMC, string Title, string? Remark, string? Solution, DateTime? CreateDate, DateTime? DateExpired, string? UserCreate, string? FileNameAttachment, List<CreateBulletinItemDto>? Items, List<CreateBulletinVinDto>? TargetVins);
record CreateBulletinItemDto(LineType Type, int? PartId, string? Code, string Name, string? Unit, decimal Quantity, decimal? UnitPrice, string? Note);
record CreateBulletinVinDto(string VinNo, string? PlateNo, string? Model, string? DealerCode, string? Note);
record AddVinsDto(List<string> VinList, string? Model, string? DealerCode);
record UpdateBulletinVinStatusDto(BulletinVinStatus Status, string? DoneBy, int? ROId, string? RONo);
record ApplyBulletinToRoDto(int RoId);
record TransitionBulletinDto(BulletinStatus ToStatus);
record CreatePdiRequestDto(string? PdiReqNo, string? DealerCode, DateTime? CreatedDate, string? Remark, string? CreatedBy, bool? FlagAccessory, List<CreatePdiItemDto> Items);
record CreatePdiItemDto(string VIN, string Model, string? Spec, string? Color, string? ContractNo, string? CustomerName, string? CustomerPhone, string? CustomerAddress, DateTime? ExpectedDeliveryDate, bool? FlagAccessory, string? AccessoryNote);
record TransitionPdiDto(PdiRequestStatus ToStatus, string? ApprovedBy);
record CreatePdiRoDto(string? Technician);
record UpdatePdiChecklistDto(string? Inspector, string? Notes, List<UpdatePdiChecklineDto>? Items);
record UpdatePdiChecklineDto(int CheckId, AuditStatus Status, string? Note);
record PassPdiDto(string? Inspector);
record CreateOrderComplainDto(string? OrderComplainNo, string? DealerCode, string? DealerName, OrderComplainType ComplainType, int? OrderPartId, int PartId, decimal Quantity, decimal? UnitPrice, string? VIN, string Description, string? RequestOrderNo, string? TransportUnit, DateTime? DeliveryDateTime, string? DeliveryBy, string? DeliveryLocation, string? ReceiveBy, DateTime? AssembleDateTime, string? AssembleBy, string? CreatedBy, List<CreateOrderComplainAttachDto>? AttachFiles);
record CreateOrderComplainAttachDto(string ImageType, string FileName, string? FilePath, string? Note);
record ReviewOrderComplainDto(TSTOrderComplainStatus TSTStatus, ComplainSolution Solution, string? SolutionNote);
record CreateTechnicalLibraryDto(string? TechnicalLibraryCode, string? DealerCode, string? DealerName, string? PlateNo, string Model, string? Engine, string? Gear, string? Version, TechnicalLibraryReRepairType ReRepairType, TechnicalLibraryType? Type, string ReRepairRemark, string? ReRepairFeedback, string? ExclusionTest, string ReRepairReason, string ReRepairSolution, int? RoId, string? CreatedBy);
record ApproveTechnicalLibraryDto(string? ApprovedBy);
record CreateServiceItemDto(string Code, string Name, ServiceROType ROType, decimal StdManHour, decimal Price, decimal Cost, decimal VatPercent, string? Model, bool? FlagWarranty, string? Note, bool? IsActive);
record UpdateServiceItemDto(string Name, ServiceROType ROType, decimal StdManHour, decimal Price, decimal Cost, decimal VatPercent, string? Model, bool FlagWarranty, bool IsActive, string? Note);
record ApplyServiceToRoDto(int RoId, ExpenseType? ExpenseType, decimal? CustomHours, decimal? CustomPrice, string? Note);
record CreateCarModelDto(string ModelCode, string ModelName, string TradeMarkCode, string? ProductionCode, string? DealerCode, CarModelSegment? Segment, int? ProductYear, bool? IsActive, string? CreatedBy);
record UpdateCarModelDto(string ModelName, string? TradeMarkCode, string? ProductionCode, string? DealerCode, CarModelSegment? Segment, int? ProductYear, bool? IsActive, string? UpdatedBy);
record CreateBomDto(string BomCode, string BomDesc, string? Remark, bool? IsActive, string? CreatedBy, List<CreateBomLineDto> Lines);
record UpdateBomDto(string BomDesc, string? Remark, bool? IsActive, string? UpdatedBy, List<CreateBomLineDto> Lines);
record CreateBomLineDto(string PartCode, string? PartName, string? Unit, decimal QtyMin);
record CreateWarehouseLocationDto(string LocationCode, string LocationName, string DealerCode, string? StockNo, LocationType? Type, string? Surface, string? Height, bool? IsActive, string? CreatedBy);
record UpdateWarehouseLocationDto(string LocationCode, string LocationName, string DealerCode, string? StockNo, LocationType? Type, string? Surface, string? Height, bool? IsActive, string? UpdatedBy);
record UpdateCalendarStatusDto(CalendarDayStatus Status, string? UpdatedBy);
record ResetCalendarYearDto(string? CalendarType, int Year, CalendarDayStatus Monday, CalendarDayStatus Tuesday, CalendarDayStatus Wednesday, CalendarDayStatus Thursday, CalendarDayStatus Friday, CalendarDayStatus Saturday, CalendarDayStatus Sunday, string? DealerCode, string? UpdatedBy);
record CreateSupplierDto(string Code, string Name, string? Address, string? Phone, string? Email, string? ContactName, string? ContactPhone, string? TaxCode);
record CreateSupplierPaymentDto(string? SupplierPaymentNo, int? SupplierId, string? SupplierName, string? Address, DateTime? PaymentDate, SupplierPaymentType PaymentType, int? OrderPartId, string? OrderPartNo, string? TSTRequestNo, string? Description, string? CreatedBy, List<CreateSupplierPaymentItemDto> Items);
record CreateSupplierPaymentItemDto(int PartId, decimal QtyPay, decimal? Price, decimal? VatPercent, string? StockInNo, string? LocationCode, string? Reason);
record ApproveSupplierPaymentDto(string? ApprovedBy);
record CreateStockOutOrderDto(string? OrderNo, int? RoId, DateTime? OrderDate, DateTime? RequestDeliveryTime, StockOutOrderPriority Priority, int? CavityId, string? RequesterName, string? Description, string? CreatedBy, List<CreateStockOutOrderItemDto> Items);
record CreateStockOutOrderItemDto(int PartId, decimal RequestQuantity, decimal? UnitPrice, decimal? VatPercent, string? Note);
record ApproveStockOutOrderDto(string? ApprovedBy);
record RejectStockOutOrderDto(string Reason);
record IssueStockOutOrderDto(string? IssuedBy);
record CreatePartOODto(int PartId, string OOPlateNo, string? Model, decimal SoLuongNo, string? CVDV, DateTime? NgayDatHang, DateTime? NgayVeDuKien, DateTime? NgayHenTra, string? GhiChu, int? ROId);
record UpdatePartOODto(string? Model, decimal SoLuongNo, decimal SoLuongTra, string? CVDV, DateTime? NgayDatHang, DateTime? NgayVeDuKien, DateTime? NgayHenTra, string? GhiChu);
record ReturnPartOODto(decimal ReturnQty, bool? DeductStock, string? ReturnedBy, string? Note);
record CancelPartOODto(string Reason);
record CreateCusDebitDto(int CustomerId, int? CarId, int? ROId, CusDebitType? DebitType, decimal DebitAmount, DateTime? DebitDate, DateTime? DueDate, string? Description, string? CreatedBy);
record CreateDebitFromRoDto(int ROId, decimal? Amount, DateTime? DueDate, string? Note);
record CancelCusDebitDto(string? Reason);
record CreateCusDebitPaymentDto(int CustomerId, int? CusDebitId, decimal PaymentAmount, DateTime? PaymentDate, PaymentMethod? Method, string? PayPersonName, string? PayPersonIdCard, string? PayPersonPhone, string? TransactionRef, string? Note, string? Collector);
record CreateSupplierDebitDto(int SupplierId, int? StockInId, int? OrderPartId, SupplierDebitType? DebitType, decimal DebitAmount, DateTime? DebitDate, DateTime? DueDate, string? Description, string? CreatedBy);
record CreateSupplierDebitFromStockInDto(int StockInId, int? SupplierId, DateTime? DueDate, string? Note);
record CancelSupplierDebitDto(string? Reason);
record CreateSupplierDebitPaymentDto(int SupplierId, int? SupplierDebitId, decimal PaymentAmount, DateTime? PaymentDate, PaymentMethod? Method, string? PayPersonName, string? PayPersonIdCard, string? PayPersonPhone, string? BankAccount, string? BankName, string? TransactionRef, string? Note, string? Cashier);
record CreateDealerHistoryRecordDto(string? DealerCode, string? DealerName, string PlateNo, string? FrameNo, string? EngineNo, string? TradeMarkName, string ModelName, string? ColorCode, int ProductYear, string CusName, string? CusPhone, string? CusAddress, string RONo, DateTime? CheckInDate, DateTime? ActualDeliveryDate, int Odometer, string? ServiceAdvisor, string? Technician, string? CustomerRequest, string? CarStatus, string? RepairResult, bool? FlagClaim, string? ClaimNo, string? ClaimStatus, List<CreateDealerHistoryItemDto>? Items);
record CreateDealerHistoryItemDto(LineType ItemType, string Code, string Name, string? Unit, decimal Quantity, decimal UnitPrice, ExpenseType? ExpenseType, string? Technician, string? Result, string? Remark);

record CreateInsuranceDebitDto(int InsuranceCompanyId, int? RoId, int? InsuranceClaimId, InsuranceDebitType? DebitType, decimal DebitAmount, DateTime? DebitDate, DateTime? DueDate, string? Description, string? CreatedBy);
record CreateInsuranceDebitFromRoDto(int RoId, int? CompanyId, decimal? DebitAmount, DateTime? DueDate, string? Note);
record CreateInsuranceDebitFromClaimDto(int ClaimId, DateTime? DueDate, string? Note);
record CancelInsuranceDebitDto(string? Reason);
record CreateInsuranceDebitPaymentDto(int InsuranceCompanyId, int? InsuranceDebitId, decimal PaymentAmount, DateTime? PaymentDate, PaymentMethod? Method, string? PayPersonName, string? PayPersonIdCard, string? PayPersonPhone, string? BankAccount, string? BankName, string? TransactionRef, string? Note, string? Cashier);

record CreateCustomerGroupDto(string? GroupNo, string GroupName, string? TaxCode, string? Address, string? Telephone, string? Fax, string? Email, string? ContactPerson, string? ContactPhone, string? Description, bool? IsActive, decimal? DiscountPercentLabor, decimal? DiscountPercentPart, decimal? CreditLimit, int? PaymentTermDays, string? ContractNo, DateTime? ContractStartDate, DateTime? ContractEndDate);
record UpdateCustomerGroupDto(string GroupName, string? TaxCode, string? Address, string? Telephone, string? Fax, string? Email, string? ContactPerson, string? ContactPhone, string? Description, bool? IsActive, decimal? DiscountPercentLabor, decimal? DiscountPercentPart, decimal? CreditLimit, int? PaymentTermDays, string? ContractNo, DateTime? ContractStartDate, DateTime? ContractEndDate);
record AddCustomerGroupMemberDto(int CarId, string? DriverName, string? DriverPhone, string? Note);

record CreatePartPriceRequestDto(string? ReqPartPriceNo, string? DealerCode, string? DealerName, string Description, bool? FlagIsCheck, int? ROId, string? VIN, string? CarModel, string? CreatedBy, List<CreatePartPriceRequestLineDto> Items);
record CreatePartPriceRequestLineDto(int? PartId, string DMSPartCode, string VieName, string? VINCode, PartPriceDeliveryForm? DeliveryForm, decimal Quantity, string? Unit, string? Remark);
record SimulatePartPriceResponseDto(List<SimulatePartPriceLineItemDto> Items);
record SimulatePartPriceLineItemDto(int LineId, string? TSTPartCode, decimal TSTPrice, DateTime? DateEffect);
record ApprovePartPriceRequestDto(string? ApprovedBy, bool? SyncToCatalog);
record ConvertPriceRequestToOrderDto(string? CreatedBy);
record CancelPartPriceRequestDto(string? Reason);

record CreateComplaintDiagnosticErrorDto(string ErrorCode, string ErrorName, ComplaintErrorType ErrorType, VehicleSystemGroup SystemGroup, string? ErrorDesc, string? Remark, bool? FlagActive, string? CreatedBy);
record UpdateComplaintDiagnosticErrorDto(string ErrorCode, string ErrorName, ComplaintErrorType ErrorType, VehicleSystemGroup SystemGroup, string? ErrorDesc, string? Remark, bool FlagActive);
record ApplyErrorToRoDto(int RoId, int ErrorId, string? Target);

record CreateCustomerCare72hDto(int RoId, string? InternalNote, string? CreatedBy);
record SubmitCare72hSurveyDto(CustomerCare72hStatus Status, bool? ServiceExplained, bool? BasicNeedsMet, bool HasTechnicalProblem, string? ProblemDetails, bool? FixedRightFirstTime, int? SatisfactionRating, string? CustomerFeedback, string? ReRepairAction, string? InternalNote, string? ContactedBy);
record CreateReRepairFromCare72hDto(string? Technician, string? Note);

record CreateCustomerCareBirthdayDto(int CustomerId, DateTime? DateOfBirth, string? GiftVoucherCode, decimal? GiftVoucherValue, decimal? DiscountPercent, string? Remark, string? CreatedBy);
record UpdateBirthdayContactDto(CustomerCareBirthdayStatus Status, BirthdayContactChannel Channel, string? Remark, string? GiftVoucherCode, decimal GiftVoucherValue, decimal DiscountPercent, DateTime? ValidUntil, string? ContactedBy);
record BookBirthdayAppointmentDto(DateTime AppointmentDate, AppointmentServiceType ServiceType, string? Note);

record CreateWarrantyWorkDto(string Code, string Name, string? Model, WarrantyLaborGroup LaborGroup, WarrantyCoverageType? CoverageType, string? AppTypeCode, string? EngineType, decimal RateHour, decimal RatePrice, int? VatPercent, string? RequiredPhotos, string? Remark, bool? FlagActive, string? CreatedBy);
record UpdateWarrantyWorkDto(string Code, string Name, string Model, WarrantyLaborGroup LaborGroup, WarrantyCoverageType CoverageType, string? AppTypeCode, string? EngineType, decimal RateHour, decimal RatePrice, int VatPercent, string? RequiredPhotos, string? Remark, bool FlagActive, string? UpdatedBy);
record ApplyWarrantyWorkToRoDto(int RoId, decimal? CustomHours, string? Note);

record CreateMaintenanceSettingDto(string ROMSID, string? Name, int Km, int? Maintances, MaintenanceLevel? Level, int? MonthsInterval, decimal? TakingTimeHours, decimal? EstimatedCost, int? ServicePackageId, string? RequiredChecklist, string? Description, bool? FlagWarranty, bool? FlagActive, string? CreatedBy);
record UpdateMaintenanceSettingDto(string ROMSID, string Name, int Km, int Maintances, MaintenanceLevel Level, int MonthsInterval, decimal TakingTimeHours, decimal EstimatedCost, int? ServicePackageId, string? RequiredChecklist, string? Description, bool FlagWarranty, bool FlagActive, string? UpdatedBy);
record ApplyMaintenanceToRoDto(int RoId, bool? AddPackageCombo);

record CreateWarrantyTypeDto(string? ROWTID, WarrantyTypeCode TypeCode, string? TypeName, WarrantyTypeDetailCode DetailCode, string? DetailName, bool? FlagActive, List<string>? PhotoCodes);
record UpdateWarrantyTypeDto(WarrantyTypeCode TypeCode, string? TypeName, WarrantyTypeDetailCode DetailCode, string? DetailName, bool FlagActive, List<string>? PhotoCodes, string? UpdatedBy);

// Chia sẻ phụ tùng giữa các đại lý (SP_SharePart / SP_SharePart_Detail)
record CreateSharePartDto(string DealerCode, string? DealerName, string? Remark, string? CreatedBy, List<CreateSharePartLineDto> Lines);
record CreateSharePartLineDto(int PartId, decimal QuantityShare, string? DealerCode, string? Remark);

// Danh mục Loại công việc dịch vụ (Ser_MST_ServiceType)
record CreateServiceTypeDto(string TypeName, string? DealerCode, string? CreatedBy);
record UpdateServiceTypeDto(string TypeName, string? DealerCode, string? UpdatedBy);

// Danh mục Nhóm vật tư / Loại vật tư (Ser_MST_PartGroup)
record CreatePartGroupDto(string GroupCode, string GroupName, string? DealerCode, int? ParentId, int? OrderId, string? CreatedBy);
record UpdatePartGroupDto(string GroupCode, string GroupName, string? DealerCode, int? ParentId, int? OrderId, string? UpdatedBy);

// Lịch sử giá bán phụ tùng theo ngày hiệu lực (Ser_Inv_PartPrice)
record CreatePartPriceDto(int PartId, decimal Price, DateTime? DateEffect, string? Remark, string? CreatedBy);
record UpdatePartPriceDto(int PartId, decimal Price, DateTime? DateEffect, string? Remark, bool? IsActive, string? UpdatedBy);

// Thiết lập nguồn gốc model xe theo số khung (Mst_VINModelOrginal)
record CreateVinModelOriginDto(string VINCode, string ModelCode, string OrginalCode, string? Remark, string? CreatedBy);
record UpdateVinModelOriginDto(string VINCode, string ModelCode, string OrginalCode, bool? IsActive, string? Remark, string? UpdatedBy);
record ImportVinModelOriginsDto(List<ImportVinModelOriginRowDto>? Rows, string? UserCode);
record ImportVinModelOriginRowDto(string VINCode, string ModelCode, string OrginalCode);

// Danh mục Thương hiệu xe (Ser_Mst_TradeMark)
record CreateTradeMarkDto(string TradeMarkCode, string TradeMarkName, string? DealerCode, string? Logo, string? CreatedBy);
record UpdateTradeMarkDto(string TradeMarkCode, string TradeMarkName, string? DealerCode, bool? IsActive, string? Logo, string? UpdatedBy);

// Ảnh minh chứng Tiếp nhận - Giao xe theo dòng xe (Ser_Mst_ModelAudImage)
record CreateModelAuditImageDto(string ModelCode, string ReceptionFAudType, string FilePath, string? Remark, string? CreatedBy);
record UpdateModelAuditImageDto(string ModelCode, string ReceptionFAudType, string FilePath, bool? IsActive, string? Remark, string? UpdatedBy);



