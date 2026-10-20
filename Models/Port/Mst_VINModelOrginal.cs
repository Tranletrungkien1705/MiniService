using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 Mst_VINModelOrginalController.ExportTemplate — Mst_VINModelOrginal
public class Mst_VINModelOrginal
{
    public string VINCode { get; set; } = "";
    public string? ModelCode { get; set; }
    public string? OrginalCode { get; set; }
    public string? FlagActive { get; set; }
    public string? LogLUBy { get; set; }
    public string? LogLUDTime { get; set; }
}

public class Mst_VINModelOrginalConfiguration : IEntityTypeConfiguration<Mst_VINModelOrginal>
{
    public void Configure(EntityTypeBuilder<Mst_VINModelOrginal> builder)
    {
        builder.ToTable("Mst_VINModelOrginal");
        builder.HasKey(x => x.VINCode);

        builder.Property(x => x.VINCode).HasMaxLength(50);
        builder.Property(x => x.ModelCode).HasMaxLength(50);
        builder.Property(x => x.OrginalCode).HasMaxLength(50);
        builder.Property(x => x.FlagActive).HasMaxLength(1);
        builder.Property(x => x.LogLUBy).HasMaxLength(50);
        builder.Property(x => x.LogLUDTime).HasMaxLength(50);
    }
}

public class Mst_VINModelOrginalSeeder : IPortSeeder
{
    public int Order => 18;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Mst_VINModelOrginal>();
        if (await set.AnyAsync()) return;

        set.AddRange(
            new Mst_VINModelOrginal { VINCode = "VIN001", ModelCode = "MOD01", OrginalCode = "ORG01", FlagActive = "1", LogLUBy = "port", LogLUDTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
            new Mst_VINModelOrginal { VINCode = "VIN002", ModelCode = "MOD02", OrginalCode = "ORG02", FlagActive = "1", LogLUBy = "port", LogLUDTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
        );

        await db.SaveChangesAsync();
    }
}
