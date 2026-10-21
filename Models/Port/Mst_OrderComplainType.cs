using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 Mst_OrderComplainTypeController.Search — Mst_OrderComplainType
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\Mst_OrderComplainTypeController.cs:41-253
//   Biz:  D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Master.cs:61157-61295 (Mst_OrderComplainType_Get)
//         -> 61297-61439 (Mst_OrderComplainType_GetX)
public class Mst_OrderComplainType
{
    public string OrderComplainType { get; set; } = "";
    public string? OrderComplainTypeName { get; set; }
    public string? FlagActive { get; set; }
    public string? LogLUBy { get; set; }
    public string? LogLUDTime { get; set; }
}

public class Mst_OrderComplainTypeConfiguration : IEntityTypeConfiguration<Mst_OrderComplainType>
{
    public void Configure(EntityTypeBuilder<Mst_OrderComplainType> builder)
    {
        builder.ToTable("Mst_OrderComplainType");
        builder.HasKey(x => x.OrderComplainType);

        builder.Property(x => x.OrderComplainType).HasMaxLength(50);
        builder.Property(x => x.OrderComplainTypeName).HasMaxLength(250);
        builder.Property(x => x.FlagActive).HasMaxLength(1);
        builder.Property(x => x.LogLUBy).HasMaxLength(50);
        builder.Property(x => x.LogLUDTime).HasMaxLength(50);
    }
}

public class Mst_OrderComplainTypeSeeder : IPortSeeder
{
    // Order danh mục 10-99 (PortInfra.cs:16-17) — chọn 14 (11, 12, 13 đã dùng).
    public int Order => 14;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Mst_OrderComplainType>();
        if (await set.AnyAsync()) return;

        set.AddRange(
            new Mst_OrderComplainType { OrderComplainType = "OCT01", OrderComplainTypeName = "Khiếu nại dịch vụ bảo dưỡng", FlagActive = "1", LogLUBy = "port", LogLUDTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
            new Mst_OrderComplainType { OrderComplainType = "OCT02", OrderComplainTypeName = "Khiếu nại phụ tùng thay thế", FlagActive = "1", LogLUBy = "port", LogLUDTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
            new Mst_OrderComplainType { OrderComplainType = "OCT03", OrderComplainTypeName = "Khiếu nại thái độ nhân viên", FlagActive = "0", LogLUBy = "port", LogLUDTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") });

        await db.SaveChangesAsync();
    }
}
