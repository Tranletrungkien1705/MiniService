using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 ReqPartPriceController.cs — Mst_Dealer (chỉ cột dùng cho port này)
// Nguồn: BizCarSv.SuggestPrice.cs:516 — `inner join [_strConfig_DBName_Main].[dbo].Mst_Dealer md on rpp.DealerCode = md.DealerCode`.
// Ở đích: 1 DB SQLite ⇒ join bảng local. Danh mục Mst_Dealer đầy đủ chưa được port (GAP), chỉ khai cột dùng cho join/existence.
// Lưu ý: `md.DealerName` được SELECT ở SQL nguồn nhưng model Req_PartPrice KHÔNG có trường này ⇒ giá trị bị bỏ ở tầng chuyển model; chỉ có tác dụng "row phải tồn tại" (inner join).
public class Mst_Dealer
{
    public string DealerCode { get; set; } = "";
    public string? DealerName { get; set; }
}

public class Mst_DealerConfiguration : IEntityTypeConfiguration<Mst_Dealer>
{
    public void Configure(EntityTypeBuilder<Mst_Dealer> b)
    {
        b.ToTable("Mst_Dealer");
        b.HasKey(x => x.DealerCode);
        b.Property(x => x.DealerCode).HasMaxLength(50);
        b.Property(x => x.DealerName).HasMaxLength(250);
    }
}

/// <summary>Dữ liệu demo cho join — KHÔNG phải danh mục đầy đủ của nguồn.</summary>
public class Mst_DealerSeeder : IPortSeeder
{
    // Order danh mục 10-99; grep Models/Port: 11 đã dùng ⇒ chọn 12.
    public int Order => 12;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Mst_Dealer>();
        if (await set.AnyAsync()) return;
        set.AddRange(
            new Mst_Dealer { DealerCode = "DL01", DealerName = "Đại lý Hà Nội" },
            new Mst_Dealer { DealerCode = "DL02", DealerName = "Đại lý TP.HCM" });
        await db.SaveChangesAsync();
    }
}
