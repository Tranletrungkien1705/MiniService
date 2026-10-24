using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 MapSGSOController.cs — Sys_Object
// Nguồn gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\MapSGSOController.cs:240-433
// Biz: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\System.cs:15222 → Map_SG_SO_GetX:15347
public class Sys_Object
{
    public string ObjectCode { get; set; } = "";
    public string DealerCode { get; set; } = "";
    public string PartnerCode { get; set; } = "";
    public string? ObjectName { get; set; }
    public string? ObjectType { get; set; }
    public string? BizUserType { get; set; }
    public string? FlagActive { get; set; }
}

public class Sys_ObjectConfiguration : IEntityTypeConfiguration<Sys_Object>
{
    public void Configure(EntityTypeBuilder<Sys_Object> b)
    {
        b.ToTable("Sys_Object");
        b.HasKey(x => new { x.ObjectCode, x.DealerCode, x.PartnerCode });
        b.Property(x => x.ObjectCode).HasMaxLength(50);
        b.Property(x => x.DealerCode).HasMaxLength(50);
        b.Property(x => x.PartnerCode).HasMaxLength(50);
        b.Property(x => x.ObjectName).HasMaxLength(250);
        b.Property(x => x.ObjectType).HasMaxLength(50);
        b.Property(x => x.BizUserType).HasMaxLength(50);
        b.Property(x => x.FlagActive).HasMaxLength(50);
    }
}

/// <summary>Dữ liệu demo Sys_Object.</summary>
public class Sys_ObjectSeeder : IPortSeeder
{
    public int Order => 68;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Sys_Object>();
        if (await set.AnyAsync()) return;
        set.AddRange(
            new Sys_Object { ObjectCode = "BTN_CREATE", DealerCode = "DEALER_PORT", PartnerCode = "Web", ObjectName = "Nút tạo mới", ObjectType = "BTN", BizUserType = "Admin", FlagActive = "1" },
            new Sys_Object { ObjectCode = "BTN_EDIT", DealerCode = "DEALER_PORT", PartnerCode = "Web", ObjectName = "Nút chỉnh sửa", ObjectType = "BTN", BizUserType = "Admin", FlagActive = "1" },
            new Sys_Object { ObjectCode = "BTN_DELETE", DealerCode = "DEALER_PORT", PartnerCode = "Web", ObjectName = "Nút xóa", ObjectType = "BTN", BizUserType = "Admin", FlagActive = "1" },
            new Sys_Object { ObjectCode = "MENU_HOME", DealerCode = "DEALER_PORT", PartnerCode = "Web", ObjectName = "Menu chính", ObjectType = "MENU", BizUserType = "User", FlagActive = "1" },
            new Sys_Object { ObjectCode = "MENU_SETTINGS", DealerCode = "DEALER_PORT", PartnerCode = "Web", ObjectName = "Menu cài đặt", ObjectType = "MENU", BizUserType = "Admin", FlagActive = "0" });
        await db.SaveChangesAsync();
    }
}
