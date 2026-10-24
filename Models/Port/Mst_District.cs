using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 OSHCCController/SearchCustomerCar — Mst_District
// Nguồn: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\OSHCC\BizCarSv.HCC.cs:1151-1153 (left join)
//        Các cột: DistrictCode, DistrictName, ProvinceCode.
public class Mst_District
{
    public string DistrictCode { get; set; } = "";
    public string? ProvinceCode { get; set; }
    public string? DistrictName { get; set; }
}

public class Mst_DistrictConfiguration : IEntityTypeConfiguration<Mst_District>
{
    public void Configure(EntityTypeBuilder<Mst_District> builder)
    {
        builder.ToTable("Mst_District");
        builder.HasKey(x => new { x.DistrictCode, x.ProvinceCode });
    }
}

/// <summary>Dữ liệu demo quận/huyện.</summary>
public class Mst_DistrictSeeder : IPortSeeder
{
    // Order danh mục 10-99; grep Models/Port: 20 (Province) ⇒ chọn 21.
    public int Order => 21;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Mst_District>();
        if (await set.AnyAsync()) return;
        set.AddRange(
            new Mst_District { DistrictCode = "D01", ProvinceCode = "01", DistrictName = "Hà Đông" },
            new Mst_District { DistrictCode = "D02", ProvinceCode = "01", DistrictName = "Tây Hồ" },
            new Mst_District { DistrictCode = "D03", ProvinceCode = "02", DistrictName = "Quận 1" },
            new Mst_District { DistrictCode = "D04", ProvinceCode = "02", DistrictName = "Quận 3" });
        await db.SaveChangesAsync();
    }
}
