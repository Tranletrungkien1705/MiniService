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
