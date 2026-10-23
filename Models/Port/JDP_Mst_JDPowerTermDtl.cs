using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 JDPMstJDPowerTermController.GetByJDPTermCodeHQ — JDP_Mst_JDPowerTermDtl
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\JDPMstJDPowerTermController.cs:478-691
//   Biz:  D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Master.cs:84169-84316 (JDP_Mst_JDPowerTerm_Get)
//         -> 84318-84453 (JDP_Mst_JDPowerTerm_GetX)
// Cột: BizCarSv.Master.cs:84408-84416 (`select jmjtd.*` inner join JDP_Mst_JDPowerTermDtl on JDPTermCode)
//      + khoá nối :84413 (JDPTermCode).
// DDL đầy đủ của bảng không có trong excerpt ⇒ chỉ khai các cột nguồn dùng (GAP: cột khác chưa port).
public class JDP_Mst_JDPowerTermDtl
{
    // Biz :84413 — khoá nối `t.JDPTermCode = jmjtd.JDPTermCode`.
    public string JDPTermCode { get; set; } = "";

    // DDL nguồn không có trong excerpt — dùng VIN làm khoá chính (cột định danh dòng chi tiết của kỳ khảo sát).
    public string VIN { get; set; } = "";
}

public class JDP_Mst_JDPowerTermDtlConfiguration : IEntityTypeConfiguration<JDP_Mst_JDPowerTermDtl>
{
    public void Configure(EntityTypeBuilder<JDP_Mst_JDPowerTermDtl> b)
    {
        b.ToTable("JDP_Mst_JDPowerTermDtl");
        b.HasKey(x => new { x.JDPTermCode, x.VIN });

        b.Property(x => x.JDPTermCode).HasMaxLength(50);
        b.Property(x => x.VIN).HasMaxLength(50);
    }
}

public class JDP_Mst_JDPowerTermDtlSeeder : IPortSeeder
{
    // Order nghiệp vụ >= 200 (PortInfra.cs:16) — grep Models/Port: 212-220 đã dùng ⇒ chọn 221.
    public int Order => 221;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<JDP_Mst_JDPowerTermDtl>();
        if (await set.AnyAsync()) return;

        set.AddRange(
            // Chi tiết của JDP01 (2 VIN) — JDP01 có FlagActive = "1".
            new JDP_Mst_JDPowerTermDtl { JDPTermCode = "JDP01", VIN = "VINJDP0101" },
            new JDP_Mst_JDPowerTermDtl { JDPTermCode = "JDP01", VIN = "VINJDP0102" },
            // Chi tiết của JDP02 (1 VIN).
            new JDP_Mst_JDPowerTermDtl { JDPTermCode = "JDP02", VIN = "VINJDP0201" },
            // Chi tiết của JDP03 (1 VIN) — JDP03 có FlagActive = "0".
            new JDP_Mst_JDPowerTermDtl { JDPTermCode = "JDP03", VIN = "VINJDP0301" });

        await db.SaveChangesAsync();
    }
}
