using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 DlrMstDealerContractFormPrintController.GetAllPrintType — Mst_TempPrintType
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\DlrMstDealerContractFormPrintController.cs:35-214
//   Biz:  D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\Master.cs:314-538 (Mst_TempPrintType_Get)
public class Mst_TempPrintType
{
    public string TempPrintType { get; set; } = "";
    public string? TempPrintTypeName { get; set; }
    public string? FlagActive { get; set; }
    public string? LogLUBy { get; set; }
    public string? LogLUDTime { get; set; }
}

public class Mst_TempPrintTypeConfiguration : IEntityTypeConfiguration<Mst_TempPrintType>
{
    public void Configure(EntityTypeBuilder<Mst_TempPrintType> builder)
    {
        builder.ToTable("Mst_TempPrintType");
        builder.HasKey(x => x.TempPrintType);

        builder.Property(x => x.TempPrintType).HasMaxLength(50);
        builder.Property(x => x.TempPrintTypeName).HasMaxLength(250);
        builder.Property(x => x.FlagActive).HasMaxLength(1);
        builder.Property(x => x.LogLUBy).HasMaxLength(50);
        builder.Property(x => x.LogLUDTime).HasMaxLength(50);
    }
}

public class Mst_TempPrintTypeSeeder : IPortSeeder
{
    // Order danh mục 10-99 — chọn 15 (11, 12, 13, 14, 18 đã dùng)
    public int Order => 15;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Mst_TempPrintType>();
        if (await set.AnyAsync()) return;

        set.AddRange(
            new Mst_TempPrintType
            {
                TempPrintType = "PRINT_TYPE_01",
                TempPrintTypeName = "In hợp đồng mua bán xe",
                FlagActive = "1",
                LogLUBy = "port",
                LogLUDTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            },
            new Mst_TempPrintType
            {
                TempPrintType = "PRINT_TYPE_02",
                TempPrintTypeName = "In phụ lục hợp đồng",
                FlagActive = "1",
                LogLUBy = "port",
                LogLUDTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            },
            new Mst_TempPrintType
            {
                TempPrintType = "PRINT_TYPE_03",
                TempPrintTypeName = "In biên bản giao xe (ngừng dùng)",
                FlagActive = "0",
                LogLUBy = "port",
                LogLUDTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            }
        );

        await db.SaveChangesAsync();
    }
}
