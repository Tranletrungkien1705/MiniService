using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 DlrMstDealerContractFormPrintController.GetTempPrint — Dlr_Mst_DealerContractFormPrint
// Nguồn DDL: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\05.Refs.Biz\Migrate\ToanNH\Mirgate dữ liệu khi chuyển lên WEB\Mẫu in.txt:23-41
// Biz Get: Master.cs:1232-1456 (Dlr_Mst_DealerContractFormPrint_Get)
public class Dlr_Mst_DealerContractFormPrint
{
    public string TempPrint { get; set; } = "";
    public string DealerCode { get; set; } = "";
    public string? TempPrintType { get; set; }
    public string? TempPrintName { get; set; }
    public string? TempPrintBody { get; set; }
    public string? CreateDTimeUTC { get; set; }
    public string? CreateBy { get; set; }
    public string? Remark { get; set; }
    public string? FlagActive { get; set; }
    public string? LogLUDTimeUTC { get; set; }
    public string? LogLUBy { get; set; }
}

public class Dlr_Mst_DealerContractFormPrintConfiguration : IEntityTypeConfiguration<Dlr_Mst_DealerContractFormPrint>
{
    public void Configure(EntityTypeBuilder<Dlr_Mst_DealerContractFormPrint> builder)
    {
        builder.ToTable("Dlr_Mst_DealerContractFormPrint");
        builder.HasKey(x => x.TempPrint);

        builder.Property(x => x.TempPrint).HasMaxLength(50);
        builder.Property(x => x.DealerCode).HasMaxLength(50);
        builder.Property(x => x.TempPrintType).HasMaxLength(50);
        builder.Property(x => x.TempPrintName).HasMaxLength(200);
        builder.Property(x => x.TempPrintBody);
        builder.Property(x => x.CreateDTimeUTC).HasMaxLength(50);
        builder.Property(x => x.CreateBy).HasMaxLength(200);
        builder.Property(x => x.Remark).HasMaxLength(400);
        builder.Property(x => x.FlagActive).HasMaxLength(1);
        builder.Property(x => x.LogLUDTimeUTC).HasMaxLength(50);
        builder.Property(x => x.LogLUBy).HasMaxLength(200);
    }
}

public class Dlr_Mst_DealerContractFormPrintSeeder : IPortSeeder
{
    // Order danh mục/nghiệp vụ: chọn 220 (chưa dùng trong Models/Port)
    public int Order => 220;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Dlr_Mst_DealerContractFormPrint>();
        if (await set.AnyAsync()) return;

        set.AddRange(
            new Dlr_Mst_DealerContractFormPrint
            {
                TempPrint = "TP001",
                DealerCode = "HTC",
                TempPrintType = "PRINT_TYPE_01",
                TempPrintName = "Mẫu in hợp đồng mua bán xe cá nhân",
                TempPrintBody = "{\"template\": \"body_contract_01\", \"outputFormat\": \"pdf\"}",
                CreateDTimeUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                CreateBy = "port",
                Remark = "Seed demo 1",
                FlagActive = "1",
                LogLUDTimeUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                LogLUBy = "port"
            },
            new Dlr_Mst_DealerContractFormPrint
            {
                TempPrint = "TP002",
                DealerCode = "HTC",
                TempPrintType = "PRINT_TYPE_02",
                TempPrintName = "Mẫu in phụ lục hợp đồng mở rộng",
                TempPrintBody = "{\"template\": \"body_appendix_02\", \"outputFormat\": \"pdf\"}",
                CreateDTimeUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                CreateBy = "port",
                Remark = "Seed demo 2",
                FlagActive = "1",
                LogLUDTimeUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                LogLUBy = "port"
            }
        );

        await db.SaveChangesAsync();
    }
}
