using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 DlrMstDealerContractFormPrintController.GetTempPrint — Mst_TempPrintGroup
// Nguồn DDL: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\05.Refs.Biz\Migrate\ToanNH\Mirgate dữ liệu khi chuyển lên WEB\Mẫu in.txt:44-59
// Biz Get: Master.cs:607-832 (Mst_TempPrintGroup_Get)
public class Mst_TempPrintGroup
{
    public string TempPrintType { get; set; } = "";
    public string DealerCode { get; set; } = "";
    public string? TempGrpPrintBody { get; set; }
    public string? Remark { get; set; }
    public string? FlagActive { get; set; }
    public string? LogLUDTimeUTC { get; set; }
    public string? LogLUBy { get; set; }
}

public class Mst_TempPrintGroupConfiguration : IEntityTypeConfiguration<Mst_TempPrintGroup>
{
    public void Configure(EntityTypeBuilder<Mst_TempPrintGroup> builder)
    {
        builder.ToTable("Mst_TempPrintGroup");
        builder.HasKey(x => x.TempPrintType);

        builder.Property(x => x.TempPrintType).HasMaxLength(50);
        builder.Property(x => x.DealerCode).HasMaxLength(50);
        builder.Property(x => x.TempGrpPrintBody);
        builder.Property(x => x.Remark).HasMaxLength(400);
        builder.Property(x => x.FlagActive).HasMaxLength(1);
        builder.Property(x => x.LogLUDTimeUTC).HasMaxLength(50);
        builder.Property(x => x.LogLUBy).HasMaxLength(200);
    }
}

public class Mst_TempPrintGroupSeeder : IPortSeeder
{
    // Order danh mục 10-99 — chọn 19 (11..15, 18 đã dùng)
    public int Order => 19;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Mst_TempPrintGroup>();
        if (await set.AnyAsync()) return;

        set.AddRange(
            new Mst_TempPrintGroup
            {
                TempPrintType = "PRINT_TYPE_03",
                DealerCode = "HTC",
                TempGrpPrintBody = "{\"template\": \"group_body_default_03\", \"outputFormat\": \"pdf\"}",
                Remark = "Mẫu in mặc định nhóm 03",
                FlagActive = "1",
                LogLUDTimeUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                LogLUBy = "port"
            }
        );

        await db.SaveChangesAsync();
    }
}
