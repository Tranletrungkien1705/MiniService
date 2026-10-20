using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 ReqPartPriceController.cs — Req_PartPrice
// Nguồn:
//   Gate: ReqPartPriceController.cs:1818-2114 (GetByReqPartPriceNoDL)
//   Biz:  BizCarSv.SuggestPrice.cs:136-270 (Req_PartPrice_Get_DL) -> :413-613 (Req_PartPrice_GetX)
//   Model nguồn: 12.Dev.Common\idn.CarService.Common\Models\CarSv\Req_PartPrice.cs:9-22
// Cột (theo model nguồn, KHÔNG tính 2 cột Name do gate tự sinh):
//   ReqPartPriceNo · Description · FlagIsCheck · TSTReqPartPriceID · CreateDTime · CreateBy
//   · DMSReqPartPriceStatus · TSTReqPartPriceStatus · TSTSentDate
// Thêm cột DealerCode chỉ để join Mst_Dealer (không có trong model nguồn — SQL nguồn `rpp.*`).
public class Req_PartPrice
{
    public string ReqPartPriceNo { get; set; } = "";
    public string? Description { get; set; }
    public string? FlagIsCheck { get; set; }
    // GAP: DDL nguồn chưa đối chiếu kiểu của TSTReqPartPriceID (int vs string) — port dùng string cho an toàn.
    public string? TSTReqPartPriceID { get; set; }
    public DateTime? CreateDTime { get; set; }
    public string? CreateBy { get; set; }
    public string? DMSReqPartPriceStatus { get; set; }
    public string? TSTReqPartPriceStatus { get; set; }
    public DateTime? TSTSentDate { get; set; }

    // Biz :514-517 — rpp.DealerCode (join Mst_Dealer). KHÔNG xuất ra response (model nguồn không có cột).
    public string? DealerCode { get; set; }
}

public class Req_PartPriceConfiguration : IEntityTypeConfiguration<Req_PartPrice>
{
    public void Configure(EntityTypeBuilder<Req_PartPrice> b)
    {
        b.ToTable("Req_PartPrice");
        b.HasKey(x => x.ReqPartPriceNo);
        b.Property(x => x.ReqPartPriceNo).HasMaxLength(50);
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.FlagIsCheck).HasMaxLength(10);
        b.Property(x => x.TSTReqPartPriceID).HasMaxLength(50);
        b.Property(x => x.CreateBy).HasMaxLength(50);
        b.Property(x => x.DMSReqPartPriceStatus).HasMaxLength(10);
        b.Property(x => x.TSTReqPartPriceStatus).HasMaxLength(10);
        b.Property(x => x.DealerCode).HasMaxLength(50);
    }
}

/// <summary>Dữ liệu demo nghiệp vụ cho port GetByReqPartPriceNoDL.</summary>
public class Req_PartPriceSeeder : IPortSeeder
{
    // Order nghiệp vụ >= 200; 212 chưa dùng.
    public int Order => 212;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Req_PartPrice>();
        if (await set.AnyAsync()) return;
        set.AddRange(
            // PP001: DMS=P (Pending), TST=1 (Chờ duyệt), DealerCode hợp lệ.
            new Req_PartPrice
            {
                ReqPartPriceNo = "PP001", Description = "Đề nghị báo giá đợt 1",
                FlagIsCheck = "0", TSTReqPartPriceID = "TST-001",
                CreateDTime = new DateTime(2026, 10, 1), CreateBy = "user1",
                DMSReqPartPriceStatus = "P", TSTReqPartPriceStatus = "1",
                TSTSentDate = new DateTime(2026, 10, 2), DealerCode = "DL01"
            },
            // PP002: DMS=A (Approved), TST=2 (Đã duyệt, chờ hoàn thiện).
            new Req_PartPrice
            {
                ReqPartPriceNo = "PP002", Description = "Đề nghị báo giá đợt 2",
                FlagIsCheck = "1", TSTReqPartPriceID = "TST-002",
                CreateDTime = new DateTime(2026, 10, 5), CreateBy = "user2",
                DMSReqPartPriceStatus = "A", TSTReqPartPriceStatus = "2",
                TSTSentDate = null, DealerCode = "DL01"
            },
            // PP003: DMS=F (Finished), TST=4 (Đã hoàn thiện).
            new Req_PartPrice
            {
                ReqPartPriceNo = "PP003", Description = "Đề nghị báo giá đợt 3",
                FlagIsCheck = "1", TSTReqPartPriceID = "TST-003",
                CreateDTime = new DateTime(2026, 10, 8), CreateBy = "user1",
                DMSReqPartPriceStatus = "F", TSTReqPartPriceStatus = "4",
                TSTSentDate = new DateTime(2026, 10, 9), DealerCode = "DL02"
            },
            // PP004: DealerCode không tồn tại (DL99) ⇒ bị inner join Mst_Dealer loại.
            new Req_PartPrice
            {
                ReqPartPriceNo = "PP004", Description = "Đề nghị báo giá đợt 4 (dealer mồ côi)",
                FlagIsCheck = "0", TSTReqPartPriceID = "TST-004",
                CreateDTime = new DateTime(2026, 10, 10), CreateBy = "user3",
                DMSReqPartPriceStatus = "R", TSTReqPartPriceStatus = null,
                TSTSentDate = null, DealerCode = "DL99"
            });
        await db.SaveChangesAsync();
    }
}
