using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 JDPMstJDPowerTermController.SearchHQ — JDP_Mst_JDPowerTerm
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\JDPMstJDPowerTermController.cs:42-258
//   Biz:  D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Master.cs:84169-84316 (JDP_Mst_JDPowerTerm_Get)
//         -> 84318-84453 (JDP_Mst_JDPowerTerm_GetX)
// Cột: BizCarSv.Master.cs:84370-84406 (`select t.*` / `select jmjt.*`) + điều kiện lọc :84421-84427
//      (JDPTermCode, JDPTermName, JDPStartDate, JPDEndDate, CreatedDate, CreatedBy, FlagActive).
// DDL đầy đủ của bảng không có trong excerpt ⇒ chỉ khai các cột nguồn dùng (GAP: cột khác chưa port).
public class JDP_Mst_JDPowerTerm
{
    public string JDPTermCode { get; set; } = "";
    public string? JDPTermName { get; set; }
    public string? JDPStartDate { get; set; }
    public string? JPDEndDate { get; set; }
    public string? FlagActive { get; set; }
    public string? CreatedBy { get; set; }
    public string? CreatedDate { get; set; }
}

public class JDP_Mst_JDPowerTermConfiguration : IEntityTypeConfiguration<JDP_Mst_JDPowerTerm>
{
    public void Configure(EntityTypeBuilder<JDP_Mst_JDPowerTerm> b)
    {
        b.ToTable("JDP_Mst_JDPowerTerm");
        b.HasKey(x => x.JDPTermCode);

        b.Property(x => x.JDPTermCode).HasMaxLength(50);
        b.Property(x => x.JDPTermName).HasMaxLength(250);
        b.Property(x => x.JDPStartDate).HasMaxLength(50);
        b.Property(x => x.JPDEndDate).HasMaxLength(50);
        b.Property(x => x.FlagActive).HasMaxLength(1);
        b.Property(x => x.CreatedBy).HasMaxLength(50);
        b.Property(x => x.CreatedDate).HasMaxLength(50);
    }
}

public class JDP_Mst_JDPowerTermSeeder : IPortSeeder
{
    // Order danh mục 10-99 (PortInfra.cs:16-17) — grep Models/Port: 11-15, 18, 19 đã dùng ⇒ chọn 16.
    public int Order => 16;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<JDP_Mst_JDPowerTerm>();
        if (await set.AnyAsync()) return;

        set.AddRange(
            new JDP_Mst_JDPowerTerm { JDPTermCode = "JDP01", JDPTermName = "Kỳ J.D. Power Quý 1", JDPStartDate = "2026-01-01", JPDEndDate = "2026-03-31", FlagActive = "1", CreatedBy = "port", CreatedDate = "2026-01-01 08:00:00" },
            new JDP_Mst_JDPowerTerm { JDPTermCode = "JDP02", JDPTermName = "Kỳ J.D. Power Quý 2", JDPStartDate = "2026-04-01", JPDEndDate = "2026-06-30", FlagActive = "1", CreatedBy = "port", CreatedDate = "2026-04-01 08:00:00" },
            new JDP_Mst_JDPowerTerm { JDPTermCode = "JDP03", JDPTermName = "Kỳ J.D. Power Quý 3", JDPStartDate = "2026-07-01", JPDEndDate = "2026-09-30", FlagActive = "0", CreatedBy = "port", CreatedDate = "2026-07-01 08:00:00" });

        await db.SaveChangesAsync();
    }
}
