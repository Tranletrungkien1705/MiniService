using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models;
using MiniService.Services;
using Serilog;
using Serilog.Sinks.OpenSearch;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// ---- Logging xuyên suốt: Serilog structured, đẩy Elasticsearch (ELK) khi có ELASTIC_URL, kèm CorrelationId ----
var elasticUrl = Environment.GetEnvironmentVariable("ELASTIC_URL");
var logCfg = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("app", "miniservice")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}");
if (!string.IsNullOrWhiteSpace(elasticUrl))
    logCfg = logCfg.WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri(elasticUrl))
    {
        AutoRegisterTemplate = false,   // Bonsai chặn PUT template; index tự tạo (tên khớp whitelist *events*)
        IndexFormat = "fleet-events-{0:yyyy.MM.dd}",
        BatchPostingLimit = 20,
        Period = TimeSpan.FromSeconds(3),
        ModifyConnectionSettings = c =>
        {
            var user = Environment.GetEnvironmentVariable("ELASTIC_USER");
            var pass = Environment.GetEnvironmentVariable("ELASTIC_PASS");
            return string.IsNullOrEmpty(user) ? c : c.BasicAuthentication(user, pass);
        }
    });
Log.Logger = logCfg.CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.WebHost.UseUrls($"http://0.0.0.0:{Environment.GetEnvironmentVariable("PORT") ?? "8080"}");

var conn = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=miniservice.db";
builder.Services.AddDbContext<AppDbContext>(o =>
{
    if (DbUtil.IsPostgres(conn)) o.UseNpgsql(DbUtil.ToNpgsql(conn));
    else o.UseSqlite(conn);
});
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ICache, RedisCache>();          // Redis cache (mềm, fallback no-op)
builder.Services.AddScoped<IIntegrationService, IntegrationService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IRoService, RoService>();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();                  // Swagger/OpenAPI
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "MiniService API", Version = "v1", Description = "Car Service (RO) — API-first cho SPA + tích hợp HĐĐT" }));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
    await Seeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());

app.UseMiddleware<CorrelationMiddleware>();   // gán/đọc X-Correlation-Id trước tiên
app.UseSerilogRequestLogging();               // log mỗi request kèm CorrelationId

app.UseDefaultFiles();   // wwwroot/index.html = SPA client-side là trang chính "/"
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "MiniService API v1"); c.RoutePrefix = "swagger"; });

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

// Nhập KH thật từ DB dự án chính (CarService.Ser_Customer). Cần X-Api-Key (tenant).
app.MapPost("/api/import/customers", async (List<ImportCustomerDto> items, AppDbContext db) =>
{
    if (items == null || items.Count == 0) return Results.BadRequest(new { error = "Danh sách rỗng." });
    int added = 0, skipped = 0;
    var existing = await db.Customers.Select(c => c.Phone).ToListAsync();
    var seen = new HashSet<string>(existing.Where(p => p != null)!);
    foreach (var it in items)
    {
        if (string.IsNullOrWhiteSpace(it.Name)) { skipped++; continue; }
        if (!string.IsNullOrWhiteSpace(it.Phone) && !seen.Add(it.Phone)) { skipped++; continue; }
        db.Customers.Add(new Customer
        {
            Code = string.IsNullOrWhiteSpace(it.DealerCode) ? "KH" + (added + 1).ToString("D5") : $"{it.DealerCode}-{it.Phone}",
            Name = it.Name.Trim(), Phone = it.Phone, Email = it.Email,
            Address = it.Address, TaxCode = it.TaxCode, DealerCode = it.DealerCode
        });
        added++;
    }
    await db.SaveChangesAsync();
    return Results.Ok(new { added, skipped, total = items.Count });
});

// Nhập xe + chủ xe thật từ CarService.Ser_Car (kèm tạo/khớp khách theo SĐT).
app.MapPost("/api/import/cars", async (List<ImportCarDto> items, AppDbContext db) =>
{
    if (items == null || items.Count == 0) return Results.BadRequest(new { error = "Danh sách rỗng." });
    int cars = 0, custAdded = 0, skipped = 0;
    var custByPhone = await db.Customers.Where(c => c.Phone != null).ToDictionaryAsync(c => c.Phone!, c => c.Id);
    var existingPlates = (await db.Cars.Select(c => c.Plate).ToListAsync()).ToHashSet();
    foreach (var it in items)
    {
        if (string.IsNullOrWhiteSpace(it.Plate)) { skipped++; continue; }
        if (!existingPlates.Add(it.Plate.Trim())) { skipped++; continue; }
        int custId;
        if (!string.IsNullOrWhiteSpace(it.OwnerPhone) && custByPhone.TryGetValue(it.OwnerPhone, out var cid)) custId = cid;
        else
        {
            var cus = new Customer { Code = "KH-" + (it.OwnerPhone ?? Guid.NewGuid().ToString("N")[..6]), Name = it.OwnerName ?? "Khách", Phone = it.OwnerPhone, DealerCode = it.DealerCode };
            db.Customers.Add(cus); await db.SaveChangesAsync();
            custId = cus.Id; custAdded++;
            if (!string.IsNullOrWhiteSpace(it.OwnerPhone)) custByPhone[it.OwnerPhone] = custId;
        }
        db.Cars.Add(new Car { Plate = it.Plate.Trim(), Model = it.Model ?? "", Year = it.Year, Vin = it.Vin, EngineNo = it.EngineNo, Color = it.Color, CurrentKm = it.CurrentKm, CustomerId = custId });
        cars++;
    }
    await db.SaveChangesAsync();
    return Results.Ok(new { cars, customersAdded = custAdded, skipped, total = items.Count });
});

// Nhập RO thật từ Ser_RO (khớp/ tạo xe+khách theo biển số; 1 dòng tổng chi phí; trạng thái = Đã giao).
app.MapPost("/api/import/ros", async (List<ImportRoDto> items, AppDbContext db) =>
{
    if (items == null || items.Count == 0) return Results.BadRequest(new { error = "Danh sách rỗng." });
    int ros = 0, skipped = 0;
    var carByPlate = await db.Cars.ToDictionaryAsync(c => c.Plate, c => c);
    var custByPhone = (await db.Customers.Where(c => c.Phone != null).ToListAsync()).GroupBy(c => c.Phone!).ToDictionary(g => g.Key, g => g.First().Id);
    var existingCodes = (await db.ROs.Select(r => r.Code).ToListAsync()).ToHashSet();
    foreach (var it in items)
    {
        if (string.IsNullOrWhiteSpace(it.RoNo) || string.IsNullOrWhiteSpace(it.Plate)) { skipped++; continue; }
        if (!existingCodes.Add(it.RoNo)) { skipped++; continue; }
        // khách
        int custId;
        if (!string.IsNullOrWhiteSpace(it.OwnerPhone) && custByPhone.TryGetValue(it.OwnerPhone, out var cid)) custId = cid;
        else { var cus = new Customer { Code = "KH-" + (it.OwnerPhone ?? Guid.NewGuid().ToString("N")[..6]), Name = it.OwnerName ?? "Khách", Phone = it.OwnerPhone }; db.Customers.Add(cus); await db.SaveChangesAsync(); custId = cus.Id; if (it.OwnerPhone != null) custByPhone[it.OwnerPhone] = custId; }
        // xe
        if (!carByPlate.TryGetValue(it.Plate.Trim(), out var car))
        { car = new Car { Plate = it.Plate.Trim(), Model = it.Model ?? "", CustomerId = custId }; db.Cars.Add(car); await db.SaveChangesAsync(); carByPlate[car.Plate] = car; }
        var ro = new RepairOrder
        {
            Code = it.RoNo.Trim(), CarId = car.Id, CustomerId = car.CustomerId, Status = ROStatus.Finished,
            CreatedBy = "import", CreatedAt = it.CreatedAt == default ? DateTime.Now : it.CreatedAt, FinishedAt = it.CreatedAt,
            Lines = it.Total > 0 ? new() { new RepairLine { Type = LineType.Labor, Name = "Chi phí dịch vụ (nhập từ CarService)", Quantity = 1, UnitPrice = it.Total } } : new()
        };
        db.ROs.Add(ro); ros++;
    }
    await db.SaveChangesAsync();
    return Results.Ok(new { ros, skipped, total = items.Count });
});

app.MapPost("/api/orgs/register", async (RegisterOrgDto dto, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Name)) return Results.BadRequest(new { error = "Cần Name." });
    var org = new Org { Name = dto.Name.Trim(), ApiKey = "svc_" + Guid.NewGuid().ToString("N") };
    db.Orgs.Add(org); await db.SaveChangesAsync();
    return Results.Ok(new { orgId = org.Id, apiKey = org.ApiKey });
});

app.MapControllers();   // API v1 ([ApiController]) cho SPA
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

try { Log.Information("MiniService khởi động (Redis={Redis}, Elastic={Elastic})", !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("REDIS_URL")), !string.IsNullOrEmpty(elasticUrl)); app.Run(); }
finally { Log.CloseAndFlush(); }

record RegisterOrgDto(string Name);
record ImportCustomerDto(string? Name, string? Phone, string? Email, string? Address, string? TaxCode, string? DealerCode);
record ImportCarDto(string? Plate, string? Model, int Year, string? Vin, string? EngineNo, string? Color, int CurrentKm, string? OwnerName, string? OwnerPhone, string? DealerCode);
record ImportRoDto(string? RoNo, string? Plate, string? Model, string? OwnerName, string? OwnerPhone, decimal Total, DateTime CreatedAt);
