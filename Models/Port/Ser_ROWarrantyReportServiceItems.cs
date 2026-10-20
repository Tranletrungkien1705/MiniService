using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 BtlBulletinDealerController.cs — Ser_ROWarrantyReportServiceItems
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\BtlBulletinDealerController.cs:1326-1389 (GetForRO — lọc bản tin đã có báo cáo bảo hành)
//   Biz:  D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.WarrantyReport.cs:303-455 (Ser_ROWarrantyReport_GetWHDL)
//         -> Ser_ROWarrantyReport_GetWHX (thân hàm KHÔNG có trong source_excerpt.txt — xem GAP)
// Cột: gate :1376-1383 chỉ dùng `BulletinID` để đối chiếu với Btl_BulletinVIN.BulletinID; DDL đầy đủ KHÔNG có trong excerpt.
public class Ser_ROWarrantyReportServiceItems
{
    // DDL nguồn không có trong excerpt — dùng ROWarrantyReportServiceItemID làm khoá chính (cột định danh dòng).
    public string ROWarrantyReportServiceItemID { get; set; } = "";

    // Gate :1381-1383 — dt_ROWarrantyServiceItem.Select("BulletinID = '<bulletinId>'").
    public string? BulletinID { get; set; }

    // Gate :1331 — strDealerCodeConditionList = GenEqualCondition2(strDealerCode) (bộ lọc DealerCode của Ser_ROWarrantyReport_GetWHDL).
    public string? DealerCode { get; set; }

    // Gate :1334 — strFrameNoConditionList = GenLikeCondition2Percent(strVIN) (bộ lọc FrameNo của Ser_ROWarrantyReport_GetWHDL).
    public string? FrameNo { get; set; }
}

public class Ser_ROWarrantyReportServiceItemsConfiguration : IEntityTypeConfiguration<Ser_ROWarrantyReportServiceItems>
{
    public void Configure(EntityTypeBuilder<Ser_ROWarrantyReportServiceItems> builder)
    {
        builder.ToTable("Ser_ROWarrantyReportServiceItems");
        builder.HasKey(x => x.ROWarrantyReportServiceItemID);

        builder.Property(x => x.ROWarrantyReportServiceItemID).HasMaxLength(50);
        builder.Property(x => x.BulletinID).HasMaxLength(50);
        builder.Property(x => x.DealerCode).HasMaxLength(50);
        builder.Property(x => x.FrameNo).HasMaxLength(50);
    }
}

/// <summary>
/// Dữ liệu mẫu Ser_ROWarrantyReportServiceItems. Nguồn chỉ có hàm ĐỌC (Ser_ROWarrantyReport_GetWHDL) nên dữ liệu mẫu
/// là phần thêm của port để kiểm chứng việc bỏ bản tin đã có báo cáo bảo hành (gate :1379-1389).
/// </summary>
public class Ser_ROWarrantyReportServiceItemsSeeder : IPortSeeder
{
    // Order nghiệp vụ >= 200 (PortInfra.cs:16) — chọn 217, chưa dùng trong Models/Port (212-216 đã dùng).
    public int Order => 217;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Ser_ROWarrantyReportServiceItems>();
        if (await set.AnyAsync()) return;

        set.AddRange(
            // Bản tin BLT001 đã có báo cáo bảo hành ⇒ bị bỏ khỏi kết quả GetForRO.
            new Ser_ROWarrantyReportServiceItems { ROWarrantyReportServiceItemID = "ROWSI001", BulletinID = "BLT001", DealerCode = "DL01", FrameNo = "RLHXXAC001" },
            // Bản tin khác (BLT999) — không ảnh hưởng bản tin nào của seed Btl_Bulletin.
            new Ser_ROWarrantyReportServiceItems { ROWarrantyReportServiceItemID = "ROWSI002", BulletinID = "BLT999", DealerCode = "DL01", FrameNo = "RLHXXAC001" });

        await db.SaveChangesAsync();
    }
}
