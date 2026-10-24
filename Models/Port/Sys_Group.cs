using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 MapSGSOController.cs — Sys_Group
// Nguồn gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\MapSGSOController.cs:240-433
// Biz: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\System.cs:15222 → Map_SG_SO_GetX:15347
public class Sys_Group
{
    public string GroupCode { get; set; } = "";
    public string DealerCode { get; set; } = "";
    public string PartnerCode { get; set; } = "";
    public string? GroupName { get; set; }
}

public class Sys_GroupConfiguration : IEntityTypeConfiguration<Sys_Group>
{
    public void Configure(EntityTypeBuilder<Sys_Group> b)
    {
        b.ToTable("Sys_Group");
        b.HasKey(x => new { x.GroupCode, x.DealerCode, x.PartnerCode });
        b.Property(x => x.GroupCode).HasMaxLength(50);
        b.Property(x => x.DealerCode).HasMaxLength(50);
        b.Property(x => x.PartnerCode).HasMaxLength(50);
        b.Property(x => x.GroupName).HasMaxLength(250);
    }
}

/// <summary>Dữ liệu demo Sys_Group.</summary>
public class Sys_GroupSeeder : IPortSeeder
{
    public int Order => 69;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Sys_Group>();
        if (await set.AnyAsync()) return;
        set.AddRange(
            new Sys_Group { GroupCode = "GRP001", DealerCode = "DEALER_PORT", PartnerCode = "Web", GroupName = "Nhóm Admin" },
            new Sys_Group { GroupCode = "GRP002", DealerCode = "DEALER_PORT", PartnerCode = "Web", GroupName = "Nhóm User" });
        await db.SaveChangesAsync();
    }
}
