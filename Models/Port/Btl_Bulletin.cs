using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 BtlBulletinDealerController.cs — Btl_Bulletin
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\BtlBulletinDealerController.cs:273-512 (SearchWHDL)
//         :513-756 (GetAllActiveByVIN)
//   Biz:  D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Bulletin.cs:262-423 (Blt_Bulletin_GetByVin_WHDL)
//         :100-261 (Blt_Bulletin_GetByVin_DL)
//         -> Blt_Bulletin_GetByVinX (thân hàm KHÔNG có trong source_excerpt.txt — xem GAP trong controller port)
// Cột: gate :391 `strRt_Cols_Btl_Bulletin = "*"` (trả mọi cột của bảng); DDL đầy đủ KHÔNG có trong excerpt.
//   Các cột dưới đây là cột được gate/biz DÙNG (bộ lọc + sắp xếp), không phải DDL đầy đủ.
public class Btl_Bulletin
{
    // Khoá chính: DDL nguồn không có trong excerpt — dùng BulletinID (cột lọc strBulletinIDConditionList, gate :427).
    public string BulletinID { get; set; } = "";

    // Gate :428 — strBulletinNoConditionList (SearchWHDL truyền "").
    public string? BulletinNo { get; set; }

    // Gate :429 — strCreateDateConditionList (SearchWHDL truyền "").
    public string? CreateDate { get; set; }

    // Gate :430 — strDateExpiredConditionList = GenGreaterOrEqualCondition(hôm nay) ⇒ DateExpired >= hôm nay.
    public string? DateExpired { get; set; }

    // Gate :431 — strRemarkConditionList (SearchWHDL truyền "").
    public string? Remark { get; set; }

    // Gate :432 — strUserCreateConditionList (SearchWHDL truyền "").
    public string? UserCreate { get; set; }

    // Gate :433 — strDealerCodeConditionList (SearchWHDL luôn truyền "" vì strDealerCode = "" tại gate :371).
    public string? DealerCode { get; set; }

    // Gate :434 — strVINConditionList = GenEqualConditionSingle(VIN) ⇒ VIN = @VIN.
    public string? VIN { get; set; }

    // Gate :435 — strIsActive = TConst.Flag.Active.
    // Gate :606 (GetAllActiveByVIN) — strIsActive = TConst.Flag.Active.
    public string? FlagActive { get; set; }

    // Gate :436 — strStatusConditionList = GenEqualConditionSingle(TConst.Stage.Pending).
    public string? Status { get; set; }
}

public class Btl_BulletinConfiguration : IEntityTypeConfiguration<Btl_Bulletin>
{
    public void Configure(EntityTypeBuilder<Btl_Bulletin> builder)
    {
        builder.ToTable("Btl_Bulletin");
        builder.HasKey(x => x.BulletinID);

        builder.Property(x => x.BulletinID).HasMaxLength(50);
        builder.Property(x => x.BulletinNo).HasMaxLength(50);
        builder.Property(x => x.CreateDate).HasMaxLength(50);
        builder.Property(x => x.DateExpired).HasMaxLength(50);
        builder.Property(x => x.Remark).HasMaxLength(500);
        builder.Property(x => x.UserCreate).HasMaxLength(50);
        builder.Property(x => x.DealerCode).HasMaxLength(50);
        builder.Property(x => x.VIN).HasMaxLength(50);
        builder.Property(x => x.FlagActive).HasMaxLength(1);
        builder.Property(x => x.Status).HasMaxLength(50);
    }
}

/// <summary>
/// Dữ liệu mẫu Btl_Bulletin. Nguồn chỉ có hàm ĐỌC (Blt_Bulletin_GetByVin_WHDL, Blt_Bulletin_GetByVin_DL) nên dữ liệu mẫu là phần thêm
/// của port để kiểm chứng bộ lọc VIN / Status / FlagActive / DateExpired.
/// </summary>
public class Btl_BulletinSeeder : IPortSeeder
{
    // Order nghiệp vụ >= 200 (PortInfra.cs:16) — chọn 214, chưa dùng trong Models/Port.
    public int Order => 214;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Btl_Bulletin>();
        if (await set.AnyAsync()) return;

        var today = DateTime.Now.ToString("yyyy-MM-dd");
        var yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        var tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

        set.AddRange(
            // VIN khớp, Pending, Active, còn hạn ⇒ 1 dòng.
            new Btl_Bulletin { BulletinID = "BLT001", BulletinNo = "TB-001", CreateDate = yesterday, DateExpired = tomorrow, Remark = "Triệu hồi ắc quy", UserCreate = "port", DealerCode = "DL01", VIN = "RLHXXAC001", FlagActive = "1", Status = "PENDING" },
            // Cùng VIN nhưng Status khác Pending ⇒ bị lọc.
            new Btl_Bulletin { BulletinID = "BLT002", BulletinNo = "TB-002", CreateDate = yesterday, DateExpired = tomorrow, Remark = "Đã xử lý", UserCreate = "port", DealerCode = "DL01", VIN = "RLHXXAC001", FlagActive = "1", Status = "DONE" },
            // Cùng VIN, Pending, nhưng FlagActive = 0 ⇒ bị lọc.
            new Btl_Bulletin { BulletinID = "BLT003", BulletinNo = "TB-003", CreateDate = yesterday, DateExpired = tomorrow, Remark = "Ngưng hiệu lực", UserCreate = "port", DealerCode = "DL01", VIN = "RLHXXAC001", FlagActive = "0", Status = "PENDING" },
            // Cùng VIN, Pending, Active, nhưng đã hết hạn ⇒ bị lọc (DateExpired >= hôm nay).
            new Btl_Bulletin { BulletinID = "BLT004", BulletinNo = "TB-004", CreateDate = yesterday, DateExpired = yesterday, Remark = "Hết hạn", UserCreate = "port", DealerCode = "DL01", VIN = "RLHXXAC001", FlagActive = "1", Status = "PENDING" },
            // VIN khác ⇒ bị lọc.
            new Btl_Bulletin { BulletinID = "BLT005", BulletinNo = "TB-005", CreateDate = yesterday, DateExpired = tomorrow, Remark = "Xe khác", UserCreate = "port", DealerCode = "DL01", VIN = "RLHXXTC002", FlagActive = "1", Status = "DONE" },
            // VIN thứ hai khớp đủ điều kiện ⇒ ca thứ hai ra 1 dòng.
            new Btl_Bulletin { BulletinID = "BLT006", BulletinNo = "TB-006", CreateDate = yesterday, DateExpired = tomorrow, Remark = "Triệu hồi túi khí", UserCreate = "port", DealerCode = "DL01", VIN = "RLHXXTC002", FlagActive = "1", Status = "PENDING" },
            // VIN thứ ba: không có dòng nào ⇒ ca rỗng.
            new Btl_Bulletin { BulletinID = "BLT007", BulletinNo = "TB-007", CreateDate = yesterday, DateExpired = tomorrow, Remark = "Xe thứ ba", UserCreate = "port", DealerCode = "DL01", VIN = "RLHXXZZ999", FlagActive = "1", Status = "DONE" });

        await db.SaveChangesAsync();
    }
}
