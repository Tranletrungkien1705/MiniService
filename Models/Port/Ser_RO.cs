using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 BtlBulletinDealerController.cs — Ser_RO
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\BtlBulletinDealerController.cs:1134-1236 (GetForRO — nhánh ROID)
//   Biz:  D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Service01.cs:7451-7585 (Ser_RO_Get_DL)
//         :7723-7857 (Ser_RO_Get_WH_DL) -> Ser_RO_GetX / Ser_RO_GetWHX (thân hàm KHÔNG có trong source_excerpt.txt — xem GAP)
// Cột: gate :1207-1221 chỉ đọc `TConst.TblSer_RO.CreatedDate` (dòng đầu của bảng Ser_RO); DDL đầy đủ KHÔNG có trong excerpt.
//   Cột dưới đây là cột gate DÙNG (khoá + CreatedDate), không phải DDL đầy đủ.
public class Ser_RO
{
    // Khoá chính: DDL nguồn không có trong excerpt — dùng ROID (cột lọc strROIDList, gate :1136).
    public string ROID { get; set; } = "";

    // Gate :1221 — strROCreate = StdStr(dt_Ser_RO.Rows[0][TConst.TblSer_RO.CreatedDate]).
    public string? CreatedDate { get; set; }

    // Gate :1137 — strDealerCodeList = GenEqualConditionSingle(orgUserUI.DealerCode) (bộ lọc DealerCode của Ser_RO_Get_DL).
    public string? DealerCode { get; set; }
}

public class Ser_ROConfiguration : IEntityTypeConfiguration<Ser_RO>
{
    public void Configure(EntityTypeBuilder<Ser_RO> builder)
    {
        builder.ToTable("Ser_RO");
        builder.HasKey(x => x.ROID);

        builder.Property(x => x.ROID).HasMaxLength(50);
        builder.Property(x => x.CreatedDate).HasMaxLength(50);
        builder.Property(x => x.DealerCode).HasMaxLength(50);
    }
}

/// <summary>
/// Dữ liệu mẫu Ser_RO. Nguồn chỉ có hàm ĐỌC (Ser_RO_Get_DL / Ser_RO_Get_WH_DL) nên dữ liệu mẫu là phần thêm
/// của port để kiểm chứng nhánh ROID của GetForRO (tìm thấy RO / không tìm thấy / CreatedDate không hợp lệ).
/// </summary>
public class Ser_ROSeeder : IPortSeeder
{
    // Order nghiệp vụ >= 200 (PortInfra.cs:16) — chọn 216, chưa dùng trong Models/Port (212/213/214/215 đã dùng).
    public int Order => 216;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Ser_RO>();
        if (await set.AnyAsync()) return;

        var yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss");

        set.AddRange(
            // RO hợp lệ, CreatedDate đúng định dạng ⇒ nhánh ROID đi tiếp.
            new Ser_RO { ROID = "RO001", CreatedDate = yesterday, DealerCode = "DL01" },
            // RO có CreatedDate không phải ngày ⇒ BtlBulletinDealerGetForRO_InvalidROCreate.
            new Ser_RO { ROID = "RO002", CreatedDate = "khong-phai-ngay", DealerCode = "DL01" });

        await db.SaveChangesAsync();
    }
}
