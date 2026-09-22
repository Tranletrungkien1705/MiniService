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
