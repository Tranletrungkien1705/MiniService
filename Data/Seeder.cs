using Microsoft.EntityFrameworkCore;
using MiniService.Models;

namespace MiniService.Data;

public static class Seeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        await MigratePostgresAsync(db);

        if (!await db.Orgs.AnyAsync(o => o.Id == TenantContext.DefaultOrgId))
        {
            db.Orgs.Add(new Org { Id = TenantContext.DefaultOrgId, Name = "Demo Service", ApiKey = TenantContext.DefaultApiKey });
            await db.SaveChangesAsync();
        }
        if (!await db.Customers.AnyAsync())
        {
            var c1 = new Customer { Code = "KH0001", Name = "Nguyễn Văn An", Phone = "0901111111", Email = "an@gmail.com",
                Cars = [ new Car { Plate = "30A-123.45", Model = "Hyundai Accent 2022", Year = 2022, Vin = "RLHXXAC001" } ] };
            var c2 = new Customer { Code = "KH0002", Name = "Trần Thị Bình", Phone = "0902222222",
                Cars = [ new Car { Plate = "51G-678.90", Model = "Hyundai Tucson 2023", Year = 2023, Vin = "RLHXXTC002" } ] };
            db.Customers.AddRange(c1, c2);
            await db.SaveChangesAsync();

            // 1 RO đang sửa để demo
            var car = await db.Cars.FirstAsync();
            var ro = new RepairOrder { Code = "ROSEED-001", CarId = car.Id, CustomerId = car.CustomerId, Status = ROStatus.InGarage,
                Odometer = 25400, IntakeNote = "Bảo dưỡng 20.000km + thay dầu", Technician = "Thợ Hùng", CreatedBy = "seed", IntakeAt = DateTime.Now.AddHours(-3),
                Lines = [
                    new RepairLine { Type = LineType.Labor, Name = "Công bảo dưỡng cấp 20.000km", Quantity = 1, UnitPrice = 500000 },
                    new RepairLine { Type = LineType.Part, Name = "Dầu động cơ Hyundai 5W-30 (4L)", Quantity = 1, UnitPrice = 650000 },
                    new RepairLine { Type = LineType.Part, Name = "Lọc dầu", Quantity = 1, UnitPrice = 180000 },
                ] };
            db.ROs.Add(ro);
            await db.SaveChangesAsync();
        }
        // Tồn kho phụ tùng
        if (!await db.Parts.AnyAsync())
        {
            db.Parts.AddRange(
                new Part { Code = "PT001", Name = "Dầu động cơ Hyundai 5W-30 (4L)", Unit = "bình", Price = 650000, OnHand = 40, MinStock = 10 },
                new Part { Code = "PT002", Name = "Lọc dầu", Unit = "cái", Price = 180000, OnHand = 60, MinStock = 15 },
                new Part { Code = "PT003", Name = "Lọc gió động cơ", Unit = "cái", Price = 220000, OnHand = 8, MinStock = 10 },
                new Part { Code = "PT004", Name = "Má phanh trước", Unit = "bộ", Price = 850000, OnHand = 12, MinStock = 6 },
                new Part { Code = "PT005", Name = "Bugi", Unit = "cái", Price = 120000, OnHand = 3, MinStock = 20 });
            await db.SaveChangesAsync();
        }
    }

    private static async Task MigratePostgresAsync(AppDbContext db)
    {
        if (!db.Database.IsNpgsql()) return;
        var def = TenantContext.DefaultOrgId;
        var tables = new[] { "Customers", "Cars", "ROs", "Lines" };
        var sql = new List<string>
        {
            "CREATE TABLE IF NOT EXISTS miniservice.\"Orgs\" (\"Id\" uuid PRIMARY KEY, \"Name\" text NOT NULL DEFAULT '', \"ApiKey\" text NOT NULL DEFAULT '', \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Orgs_ApiKey\" ON miniservice.\"Orgs\" (\"ApiKey\")",
        };
        foreach (var t in tables) sql.Add($"ALTER TABLE miniservice.\"{t}\" ADD COLUMN IF NOT EXISTS \"OrgId\" uuid NOT NULL DEFAULT '{def}'");
        // cột tích hợp HĐĐT (thêm mới)
        sql.Add("ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"EInvoiceCode\" text");
        sql.Add("ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"EInvoiceStatus\" text");
        sql.Add("ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"EInvoiceError\" text");
        sql.Add("ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"EInvoiceAt\" timestamp");
        sql.Add("ALTER TABLE miniservice.\"Customers\" ADD COLUMN IF NOT EXISTS \"Address\" text");
        sql.Add("ALTER TABLE miniservice.\"Customers\" ADD COLUMN IF NOT EXISTS \"TaxCode\" text");
        sql.Add("ALTER TABLE miniservice.\"Customers\" ADD COLUMN IF NOT EXISTS \"DealerCode\" text");
        sql.Add("ALTER TABLE miniservice.\"Cars\" ADD COLUMN IF NOT EXISTS \"EngineNo\" text");
        sql.Add("ALTER TABLE miniservice.\"Cars\" ADD COLUMN IF NOT EXISTS \"Color\" text");
        sql.Add("ALTER TABLE miniservice.\"Cars\" ADD COLUMN IF NOT EXISTS \"CurrentKm\" integer NOT NULL DEFAULT 0");
        sql.Add("ALTER TABLE miniservice.\"Lines\" ADD COLUMN IF NOT EXISTS \"PartId\" integer");
        // Bảng MỚI tồn kho/xuất kho/thanh toán — EnsureCreated không tạo trên DB đã tồn tại → CREATE tường minh.
        sql.Add(@"CREATE TABLE IF NOT EXISTS miniservice.""Parts"" (""Id"" serial PRIMARY KEY, ""OrgId"" uuid NOT NULL DEFAULT '" + def + @"',
            ""Code"" text NOT NULL DEFAULT '', ""Name"" text NOT NULL DEFAULT '', ""Unit"" text NOT NULL DEFAULT 'cái',
            ""Price"" numeric(18,2) NOT NULL DEFAULT 0, ""OnHand"" integer NOT NULL DEFAULT 0, ""MinStock"" integer NOT NULL DEFAULT 5)");
        sql.Add(@"CREATE TABLE IF NOT EXISTS miniservice.""StockOuts"" (""Id"" serial PRIMARY KEY, ""OrgId"" uuid NOT NULL DEFAULT '" + def + @"',
            ""Code"" text NOT NULL DEFAULT '', ""PartId"" integer NOT NULL DEFAULT 0, ""PartName"" text NOT NULL DEFAULT '',
            ""Quantity"" integer NOT NULL DEFAULT 0, ""UnitPrice"" numeric(18,2) NOT NULL DEFAULT 0,
            ""ROId"" integer, ""ROCode"" text, ""Reason"" text, ""CreatedAt"" timestamp NOT NULL DEFAULT now())");
        sql.Add(@"CREATE TABLE IF NOT EXISTS miniservice.""Payments"" (""Id"" serial PRIMARY KEY, ""OrgId"" uuid NOT NULL DEFAULT '" + def + @"',
            ""ROId"" integer NOT NULL DEFAULT 0, ""ROCode"" text NOT NULL DEFAULT '', ""Amount"" numeric(18,2) NOT NULL DEFAULT 0,
            ""Method"" integer NOT NULL DEFAULT 0, ""Note"" text, ""PaidAt"" timestamp NOT NULL DEFAULT now())");
        foreach (var s in sql) try { await db.Database.ExecuteSqlRawAsync(s); } catch { }
    }
}
