using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 ReqPartPriceController.cs — Req_PartPriceDtl
// Nguồn: BizCarSv.SuggestPrice.cs:530-545 (select t.MyIdxSeq, rppdt.*, mdf.DeliveryFormName ... inner join Mst_DeliveryForm).
// Model nguồn: 12.Dev.Common\idn.CarService.Common\Models\CarSv\Req_PartPrice.cs:23-36
// GAP: DDL nguồn chưa đối chiếu ⇒ composite key (ReqPartPriceNo, DMSPartCode) — port chỉ ĐỌC.
public class Req_PartPriceDtl
{
    public string ReqPartPriceNo { get; set; } = "";
    public string DMSPartCode { get; set; } = "";
    public string? VieName { get; set; }
    public string? VINCode { get; set; }
    public string? DeliveryFormCode { get; set; }
    public string? Remark { get; set; }
    // Port rules: MyValue/float của nguồn ⇒ double?.
    public double? TSTPrice { get; set; }
    public DateTime? DateEffect { get; set; }
    public string? TSTPartCode { get; set; }
    public string? FlagIsCheck { get; set; }
}

public class Req_PartPriceDtlConfiguration : IEntityTypeConfiguration<Req_PartPriceDtl>
{
    public void Configure(EntityTypeBuilder<Req_PartPriceDtl> b)
    {
        b.ToTable("Req_PartPriceDtl");
        b.HasKey(x => new { x.ReqPartPriceNo, x.DMSPartCode });
        b.Property(x => x.ReqPartPriceNo).HasMaxLength(50);
        b.Property(x => x.DMSPartCode).HasMaxLength(50);
        b.Property(x => x.VieName).HasMaxLength(500);
        b.Property(x => x.VINCode).HasMaxLength(50);
        b.Property(x => x.DeliveryFormCode).HasMaxLength(50);
        b.Property(x => x.Remark).HasMaxLength(500);
        b.Property(x => x.TSTPartCode).HasMaxLength(50);
        b.Property(x => x.FlagIsCheck).HasMaxLength(10);
    }
}

/// <summary>Dữ liệu demo nghiệp vụ cho port GetByReqPartPriceNoDL.</summary>
public class Req_PartPriceDtlSeeder : IPortSeeder
{
    // Order nghiệp vụ >= 200; 212 (Req_PartPrice) đã dùng ⇒ chọn 213.
    public int Order => 213;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Req_PartPriceDtl>();
        if (await set.AnyAsync()) return;
        set.AddRange(
            new Req_PartPriceDtl { ReqPartPriceNo = "PP001", DMSPartCode = "P001", VieName = "Lọc gió", VINCode = "VIN001",
                DeliveryFormCode = "DF01", Remark = "", TSTPrice = 100, DateEffect = new DateTime(2026, 10, 1),
                TSTPartCode = "TST-P001", FlagIsCheck = "0" },
            new Req_PartPriceDtl { ReqPartPriceNo = "PP001", DMSPartCode = "P002", VieName = "Má phanh", VINCode = "VIN001",
                DeliveryFormCode = "DF02", Remark = "", TSTPrice = 200, DateEffect = new DateTime(2026, 10, 1),
                TSTPartCode = "TST-P002", FlagIsCheck = "0" },
            new Req_PartPriceDtl { ReqPartPriceNo = "PP002", DMSPartCode = "P003", VieName = "Bu-gi", VINCode = "VIN002",
                DeliveryFormCode = "DF01", Remark = "", TSTPrice = 300, DateEffect = new DateTime(2026, 10, 5),
                TSTPartCode = "TST-P003", FlagIsCheck = "1" },
            new Req_PartPriceDtl { ReqPartPriceNo = "PP003", DMSPartCode = "P004", VieName = "Dầu máy", VINCode = "VIN003",
                DeliveryFormCode = "DF01", Remark = "", TSTPrice = 400, DateEffect = new DateTime(2026, 10, 8),
                TSTPartCode = "TST-P004", FlagIsCheck = "1" },
            new Req_PartPriceDtl { ReqPartPriceNo = "PP003", DMSPartCode = "P005", VieName = "Ắc quy", VINCode = "VIN003",
                DeliveryFormCode = "DF99", Remark = "", TSTPrice = 500, DateEffect = new DateTime(2026, 10, 8),
                TSTPartCode = "TST-P005", FlagIsCheck = "1" });
        await db.SaveChangesAsync();
    }
}
