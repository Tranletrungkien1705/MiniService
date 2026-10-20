using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 BtlBulletinDealerController.cs — Btl_BulletinDtl
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\BtlBulletinDealerController.cs:757-999 (GetAllActiveByVINMapStatusP)
//         :941-959 — lấy bảng "Btl_BulletinDtl" từ DataSet của biz, chỉ giữ chi tiết của bản tin còn hiệu lực.
//   Biz:  D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Bulletin.cs:100-261 (Blt_Bulletin_GetByVin_DL)
//         -> Blt_Bulletin_GetByVinX (thân hàm KHÔNG có trong source_excerpt.txt — xem GAP trong controller port).
// Cột: gate :861 `strRt_Cols_Btl_Bulletin = "*"` (biz trả mọi cột của bảng); DDL đầy đủ KHÔNG có trong excerpt.
//   Cột dưới đây là cột gate/biz DÙNG (khoá nối BulletinID + cột hiển thị), không phải DDL đầy đủ.
public class Btl_BulletinDtl
{
    // Gate :947/:951 — StdParam(dtrw["BulletinID"]) dùng để nối với Btl_Bulletin.BulletinID.
    public string BulletinID { get; set; } = "";

    // DDL nguồn không có trong excerpt — dùng BulletinDtlID làm khoá chính (cột định danh dòng chi tiết).
    public string BulletinDtlID { get; set; } = "";

    // Cột hiển thị của chi tiết bản tin (DDL đầy đủ chưa đối chiếu — xem GAP).
    public string? SerCode { get; set; }
    public string? SerName { get; set; }
    public string? PartCode { get; set; }
    public string? PartName { get; set; }
    public string? Remark { get; set; }
}

public class Btl_BulletinDtlConfiguration : IEntityTypeConfiguration<Btl_BulletinDtl>
{
    public void Configure(EntityTypeBuilder<Btl_BulletinDtl> builder)
    {
        builder.ToTable("Btl_BulletinDtl");
        builder.HasKey(x => x.BulletinDtlID);

        builder.Property(x => x.BulletinDtlID).HasMaxLength(50);
        builder.Property(x => x.BulletinID).HasMaxLength(50);
        builder.Property(x => x.SerCode).HasMaxLength(50);
        builder.Property(x => x.SerName).HasMaxLength(250);
        builder.Property(x => x.PartCode).HasMaxLength(50);
        builder.Property(x => x.PartName).HasMaxLength(250);
        builder.Property(x => x.Remark).HasMaxLength(500);
    }
}

/// <summary>
/// Dữ liệu mẫu Btl_BulletinDtl. Nguồn chỉ có hàm ĐỌC (Blt_Bulletin_GetByVin_DL) nên dữ liệu mẫu là phần thêm
/// của port để kiểm chứng việc lọc chi tiết theo bản tin còn hiệu lực (gate :945-957).
/// </summary>
public class Btl_BulletinDtlSeeder : IPortSeeder
{
    // Order nghiệp vụ >= 200 (PortInfra.cs:16) — chọn 215, chưa dùng trong Models/Port (212/213/214 đã dùng).
    public int Order => 215;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Btl_BulletinDtl>();
        if (await set.AnyAsync()) return;

        set.AddRange(
            // Chi tiết của BLT001 (còn hiệu lực) ⇒ được trả.
            new Btl_BulletinDtl { BulletinDtlID = "BLTD001", BulletinID = "BLT001", SerCode = "SRV01", SerName = "Kiểm tra ắc quy", PartCode = "P001", PartName = "Ắc quy", Remark = "Thay mới" },
            new Btl_BulletinDtl { BulletinDtlID = "BLTD002", BulletinID = "BLT001", SerCode = "SRV02", SerName = "Vệ sinh cọc bình", PartCode = "P002", PartName = "Dây cọc bình", Remark = "" },
            // Chi tiết của BLT002 (còn hiệu lực) ⇒ được trả.
            new Btl_BulletinDtl { BulletinDtlID = "BLTD003", BulletinID = "BLT002", SerCode = "SRV03", SerName = "Cập nhật phần mềm", PartCode = "P003", PartName = "Bộ điều khiển", Remark = "" },
            // Chi tiết của BLT004 (bản tin đã hết hạn) ⇒ bị bỏ.
            new Btl_BulletinDtl { BulletinDtlID = "BLTD004", BulletinID = "BLT004", SerCode = "SRV04", SerName = "Thay túi khí", PartCode = "P004", PartName = "Túi khí", Remark = "Hết hạn" },
            // Chi tiết của BLT003 (FlagActive = 0) ⇒ bị bỏ.
            new Btl_BulletinDtl { BulletinDtlID = "BLTD006", BulletinID = "BLT003", SerCode = "SRV06", SerName = "Ngưng áp dụng", PartCode = "P006", PartName = "Phụ tùng cũ", Remark = "Ngưng hiệu lực" },
            // Chi tiết của BLT006 (còn hiệu lực) ⇒ được trả.
            new Btl_BulletinDtl { BulletinDtlID = "BLTD005", BulletinID = "BLT006", SerCode = "SRV05", SerName = "Kiểm tra túi khí", PartCode = "P005", PartName = "Cảm biến túi khí", Remark = "" });

        await db.SaveChangesAsync();
    }
}
