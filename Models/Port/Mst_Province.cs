using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 MstProvinceController.cs — Mst_Province
// Nguồn gate: D:\idocNet\2023.H.CarServices\13.ClientGate\...\MstProvinceController.cs:41-272
// Biz Search: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Master.cs:5999 → Mst_Province_GetX:6144
public class Mst_Province
{
    public string ProvinceCode { get; set; } = "";
    public string? ProvinceName { get; set; }
    public string? FlagActive { get; set; }
    public string? CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? AreaCode { get; set; }
}

public class Mst_ProvinceConfiguration : IEntityTypeConfiguration<Mst_Province>
{
    public void Configure(EntityTypeBuilder<Mst_Province> b)
    {
        b.ToTable("Mst_Province");
        b.HasKey(x => x.ProvinceCode);
        b.Property(x => x.ProvinceCode).HasMaxLength(50);
        b.Property(x => x.ProvinceName).HasMaxLength(250);
        b.Property(x => x.FlagActive).HasMaxLength(50);
        b.Property(x => x.CreatedDate).HasMaxLength(50);
        b.Property(x => x.CreatedBy).HasMaxLength(50);
        b.Property(x => x.AreaCode).HasMaxLength(50);
    }
}

/// <summary>Dữ liệu demo danh mục tỉnh/thành phố.</summary>
public class Mst_ProvinceSeeder : IPortSeeder
{
    // Order danh mục 10-99; grep Models/Port: 11, 12 đã dùng ⇒ chọn 13.
    public int Order => 20;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Mst_Province>();
        if (await set.AnyAsync()) return;
        set.AddRange(
            new Mst_Province { ProvinceCode = "01", ProvinceName = "Hà Nội", FlagActive = "1", CreatedDate = "2024-01-01", CreatedBy = "admin", AreaCode = "N" },
            new Mst_Province { ProvinceCode = "02", ProvinceName = "TP. Hồ Chí Minh", FlagActive = "1", CreatedDate = "2024-01-01", CreatedBy = "admin", AreaCode = "S" },
            new Mst_Province { ProvinceCode = "03", ProvinceName = "Đà Nẵng", FlagActive = "0", CreatedDate = "2024-01-01", CreatedBy = "admin", AreaCode = "C" },
            new Mst_Province { ProvinceCode = "04", ProvinceName = "Hải Phòng", FlagActive = "1", CreatedDate = "2024-01-02", CreatedBy = "user1", AreaCode = "N" });
        await db.SaveChangesAsync();
    }
}
