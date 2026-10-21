using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 CarWarrantyInformationController.cs — Ser_MST_ROWarrantyRenewal
// Nguồn: BizCarSv.AssignmentOfWork.cs:1886-1904
//   select smrowr.*, smrowrc.WrtReneCateName from Ser_MST_ROWarrantyRenewal smrowr
//     inner join Ser_MST_ROWarrantyRenewalCategory smrowrc on smrowr.WrtReneCateCode = smrowrc.WrtReneCateCode
//     where (1=1) <VIN like '%x%' nếu có>
// Model nguồn Car_Warranty_InformationDtl (12.Dev.Common\...\CarSv\Car_Warranty_InformationDtl.cs:9-15)
//   chỉ dùng 4 cột: WrtReneCateName, WrtReneDate, Remark, LogLUDateTime.
// DDL đầy đủ KHÔNG có trong excerpt (GAP) — chỉ khai cột dùng trong truy vấn.
public class Ser_MST_ROWarrantyRenewal
{
    // GAP: DDL không có trong excerpt — dùng ROWarrantyRenewalID (đặt theo mẫu Ser_ROWarrantyReportServiceItems) làm khoá chính.
    public string ROWarrantyRenewalID { get; set; } = "";

    // Biz :1893 — khoá join sang category.
    public string? WrtReneCateCode { get; set; }

    // Gate :375 + Biz :1910 — `smrowr.VIN` lọc LIKE '%x%'.
    public string? VIN { get; set; }

    // Model nguồn Car_Warranty_InformationDtl.
    public DateTime? WrtReneDate { get; set; }
    public string? Remark { get; set; }
    public DateTime? LogLUDateTime { get; set; }
}

public class Ser_MST_ROWarrantyRenewalConfiguration : IEntityTypeConfiguration<Ser_MST_ROWarrantyRenewal>
{
    public void Configure(EntityTypeBuilder<Ser_MST_ROWarrantyRenewal> b)
    {
        b.ToTable("Ser_MST_ROWarrantyRenewal");
        b.HasKey(x => x.ROWarrantyRenewalID);
        b.Property(x => x.ROWarrantyRenewalID).HasMaxLength(50);
        b.Property(x => x.WrtReneCateCode).HasMaxLength(50);
        b.Property(x => x.VIN).HasMaxLength(50);
        b.Property(x => x.Remark).HasMaxLength(500);
    }
}

/// <summary>Dữ liệu mẫu lịch sử gia hạn bảo hành (Ser_MST_ROWarrantyRenewal).</summary>
public class Ser_MST_ROWarrantyRenewalSeeder : IPortSeeder
{
    // Order nghiệp vụ 219 — chạy sau danh mục (218) để inner join có dữ liệu.
    public int Order => 219;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Ser_MST_ROWarrantyRenewal>();
        if (await set.AnyAsync()) return;
        set.AddRange(
            // REN001/REN003 cùng VIN RLHXXAC001 (bộ lọc VIN ra 2 dòng).
            new Ser_MST_ROWarrantyRenewal
            {
                ROWarrantyRenewalID = "REN001", WrtReneCateCode = "CATE01", VIN = "RLHXXAC001",
                WrtReneDate = new DateTime(2026, 1, 10), Remark = "Gia hạn lần 1", LogLUDateTime = new DateTime(2026, 1, 15)
            },
            new Ser_MST_ROWarrantyRenewal
            {
                ROWarrantyRenewalID = "REN002", WrtReneCateCode = "CATE02", VIN = "RLHXXAC002",
                WrtReneDate = new DateTime(2026, 2, 20), Remark = "Gia hạn lần 2", LogLUDateTime = new DateTime(2026, 2, 25)
            },
            new Ser_MST_ROWarrantyRenewal
            {
                ROWarrantyRenewalID = "REN003", WrtReneCateCode = "CATE01", VIN = "RLHXXAC001",
                WrtReneDate = new DateTime(2026, 3, 10), Remark = "Gia hạn lần 3", LogLUDateTime = new DateTime(2026, 3, 15)
            },
            new Ser_MST_ROWarrantyRenewal
            {
                ROWarrantyRenewalID = "REN004", WrtReneCateCode = "CATE01", VIN = "OTHER999",
                WrtReneDate = new DateTime(2026, 4, 1), Remark = "Gia hạn xe khác", LogLUDateTime = new DateTime(2026, 4, 2)
            },
            // REN005: WrtReneCateCode CATE99 không tồn tại ⇒ bị inner join loại (kiểm chứng join thật).
            new Ser_MST_ROWarrantyRenewal
            {
                ROWarrantyRenewalID = "REN005", WrtReneCateCode = "CATE99", VIN = "RLHXXAC001",
                WrtReneDate = new DateTime(2026, 5, 1), Remark = "Mồ côi danh mục", LogLUDateTime = new DateTime(2026, 5, 2)
            });
        await db.SaveChangesAsync();
    }
}
