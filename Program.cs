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
