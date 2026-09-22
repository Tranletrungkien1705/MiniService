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
        if (!await db.Parts.AnyAsync())
        {
            var p1 = new Part { Code = "26300-35505", Name = "Lọc dầu động cơ chính hãng Hyundai", Unit = "Cái", CostPrice = 90000, SalePrice = 180000, InStock = 28, MinStock = 10, Location = "K1-A01", Model = "Accent / Tucson / Elantra" };
            var p2 = new Part { Code = "28113-1R100", Name = "Lọc gió động cơ Hyundai Accent", Unit = "Cái", CostPrice = 120000, SalePrice = 240000, InStock = 14, MinStock = 8, Location = "K1-A04", Model = "Accent" };
            var p3 = new Part { Code = "05100-00441", Name = "Dầu nhờn động cơ Hyundai Premium 5W-30 (Can 4L)", Unit = "Can", CostPrice = 420000, SalePrice = 650000, InStock = 35, MinStock = 15, Location = "K-DAU-01", Model = "Tất cả dòng xe" };
            var p4 = new Part { Code = "58101-C1A00", Name = "Bộ má phanh đĩa trước", Unit = "Bộ", CostPrice = 850000, SalePrice = 1350000, InStock = 6, MinStock = 5, Location = "K2-B02", Model = "Santa Fe / Tucson" };
            var p5 = new Part { Code = "18846-11070", Name = "Bugi đánh lửa Iridium cao cấp", Unit = "Cái", CostPrice = 110000, SalePrice = 220000, InStock = 3, MinStock = 8, Location = "K1-C03", Model = "Tucson / Creta" };
            var p6 = new Part { Code = "97133-D3000", Name = "Lọc gió điều hòa than hoạt tính", Unit = "Cái", CostPrice = 140000, SalePrice = 280000, InStock = 19, MinStock = 6, Location = "K1-A08", Model = "Tucson / Santa Fe" };
            db.Parts.AddRange(p1, p2, p3, p4, p5, p6);
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
            var oilPart = await db.Parts.FirstOrDefaultAsync(p => p.Code == "05100-00441");
            var filterPart = await db.Parts.FirstOrDefaultAsync(p => p.Code == "26300-35505");
            var ro = new RepairOrder { Code = "ROSEED-001", CarId = car.Id, CustomerId = car.CustomerId, Status = ROStatus.InGarage,
                Odometer = 25400, IntakeNote = "Bảo dưỡng 20.000km + thay dầu", Technician = "Thợ Hùng", CreatedBy = "seed", IntakeAt = DateTime.Now.AddHours(-3),
                Lines = [
                    new RepairLine { Type = LineType.Labor, Name = "Công bảo dưỡng cấp 20.000km", Quantity = 1, UnitPrice = 500000 },
                    new RepairLine { Type = LineType.Part, PartId = oilPart?.Id, Name = oilPart?.Name ?? "Dầu động cơ Hyundai 5W-30 (4L)", Quantity = 1, UnitPrice = oilPart?.SalePrice ?? 650000 },
                    new RepairLine { Type = LineType.Part, PartId = filterPart?.Id, Name = filterPart?.Name ?? "Lọc dầu chính hãng", Quantity = 1, UnitPrice = filterPart?.SalePrice ?? 180000 },
                ] };
            db.ROs.Add(ro);
            await db.SaveChangesAsync();
        }
    }

    private static async Task MigratePostgresAsync(AppDbContext db)
    {
        if (!db.Database.IsNpgsql()) return;
        var def = TenantContext.DefaultOrgId;
        var tables = new[] { "Customers", "Cars", "ROs", "Lines", "Parts" };
        var sql = new List<string>
        {
            "CREATE TABLE IF NOT EXISTS miniservice.\"Orgs\" (\"Id\" uuid PRIMARY KEY, \"Name\" text NOT NULL DEFAULT '', \"ApiKey\" text NOT NULL DEFAULT '', \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Orgs_ApiKey\" ON miniservice.\"Orgs\" (\"ApiKey\")",
        };
        foreach (var t in tables) sql.Add($"ALTER TABLE miniservice.\"{t}\" ADD COLUMN IF NOT EXISTS \"OrgId\" uuid NOT NULL DEFAULT '{def}'");
        foreach (var s in sql) try { await db.Database.ExecuteSqlRawAsync(s); } catch { }
    }
}
