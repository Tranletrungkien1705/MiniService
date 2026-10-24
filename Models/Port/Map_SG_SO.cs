using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 MapSGSOController.cs — Map_SG_SO
// Nguồn gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\MapSGSOController.cs:240-433
// Biz: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\System.cs:15222 → Map_SG_SO_GetX:15347
public class Map_SG_SO
{
    public string GroupCode { get; set; } = "";
    public string ObjectCode { get; set; } = "";
    public string DealerCode { get; set; } = "";
    public string PartnerCode { get; set; } = "";
}

public class Map_SG_SOConfiguration : IEntityTypeConfiguration<Map_SG_SO>
{
    public void Configure(EntityTypeBuilder<Map_SG_SO> b)
    {
        b.ToTable("Map_SG_SO");
        b.HasKey(x => new { x.GroupCode, x.ObjectCode, x.DealerCode, x.PartnerCode });
        b.Property(x => x.GroupCode).HasMaxLength(50);
        b.Property(x => x.ObjectCode).HasMaxLength(50);
        b.Property(x => x.DealerCode).HasMaxLength(50);
        b.Property(x => x.PartnerCode).HasMaxLength(50);
    }
}

/// <summary>Dữ liệu demo Map_SG_SO.</summary>
public class Map_SG_SOSeeder : IPortSeeder
{
    public int Order => 70;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Map_SG_SO>();
        if (await set.AnyAsync()) return;
        set.AddRange(
            new Map_SG_SO { GroupCode = "GRP001", ObjectCode = "BTN_CREATE", DealerCode = "DEALER_PORT", PartnerCode = "Web" },
            new Map_SG_SO { GroupCode = "GRP001", ObjectCode = "BTN_EDIT", DealerCode = "DEALER_PORT", PartnerCode = "Web" },
            new Map_SG_SO { GroupCode = "GRP002", ObjectCode = "MENU_HOME", DealerCode = "DEALER_PORT", PartnerCode = "Web" });
        await db.SaveChangesAsync();
    }
}
