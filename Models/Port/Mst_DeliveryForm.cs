using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 ReqPartPriceController.cs — Mst_DeliveryForm (chỉ cột dùng cho port này)
// Nguồn: BizCarSv.SuggestPrice.cs:539 — `inner join [_strConfig_DBName_Main].[dbo].Mst_DeliveryForm mdf on rppdt.DeliveryFormCode = mdf.DeliveryFormCode`.
// Ở đích: bảng local SQLite. Danh mục đầy đủ chưa được port (GAP) — chỉ khai 2 cột: khoá + tên để in ra ở response (model Req_PartPriceDtl có trường DeliveryFormName).
public class Mst_DeliveryForm
{
    public string DeliveryFormCode { get; set; } = "";
    public string? DeliveryFormName { get; set; }
}

public class Mst_DeliveryFormConfiguration : IEntityTypeConfiguration<Mst_DeliveryForm>
{
    public void Configure(EntityTypeBuilder<Mst_DeliveryForm> b)
    {
        b.ToTable("Mst_DeliveryForm");
        b.HasKey(x => x.DeliveryFormCode);
        b.Property(x => x.DeliveryFormCode).HasMaxLength(50);
        b.Property(x => x.DeliveryFormName).HasMaxLength(250);
    }
}

/// <summary>Dữ liệu demo cho join — KHÔNG phải danh mục đầy đủ của nguồn.</summary>
public class Mst_DeliveryFormSeeder : IPortSeeder
{
    // Order danh mục 10-99; 11 (đã dùng), 12 (Mst_Dealer) ⇒ chọn 13.
    public int Order => 13;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Mst_DeliveryForm>();
        if (await set.AnyAsync()) return;
        set.AddRange(
            new Mst_DeliveryForm { DeliveryFormCode = "DF01", DeliveryFormName = "Giao hàng tận nơi" },
            new Mst_DeliveryForm { DeliveryFormCode = "DF02", DeliveryFormName = "Nhận tại kho" });
        await db.SaveChangesAsync();
    }
}
