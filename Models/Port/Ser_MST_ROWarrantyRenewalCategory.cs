using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 CarWarrantyInformationController.cs — Ser_MST_ROWarrantyRenewalCategory
// Nguồn: BizCarSv.AssignmentOfWork.cs:1892-1893
//   inner join Ser_MST_ROWarrantyRenewalCategory smrowrc on smrowr.WrtReneCateCode = smrowrc.WrtReneCateCode
// DDL đầy đủ KHÔNG có trong excerpt (GAP) — chỉ khai cột dùng trong truy vấn.
public class Ser_MST_ROWarrantyRenewalCategory
{
    // Biz :1893 — khoá join.
    public string WrtReneCateCode { get; set; } = "";

    // Biz :1889 — `smrowrc.WrtReneCateName` (cột duy nhất lấy từ bảng category).
    public string? WrtReneCateName { get; set; }
}

public class Ser_MST_ROWarrantyRenewalCategoryConfiguration : IEntityTypeConfiguration<Ser_MST_ROWarrantyRenewalCategory>
{
    public void Configure(EntityTypeBuilder<Ser_MST_ROWarrantyRenewalCategory> b)
    {
        b.ToTable("Ser_MST_ROWarrantyRenewalCategory");
        b.HasKey(x => x.WrtReneCateCode);
        b.Property(x => x.WrtReneCateCode).HasMaxLength(50);
        b.Property(x => x.WrtReneCateName).HasMaxLength(200);
    }
}

/// <summary>Dữ liệu mẫu danh mục gia hạn bảo hành — chạy trước seeder Ser_MST_ROWarrantyRenewal (Order 219).</summary>
public class Ser_MST_ROWarrantyRenewalCategorySeeder : IPortSeeder
{
    // Order nghiệp vụ 218 — chưa dùng (212-217 đã dùng).
    public int Order => 218;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Ser_MST_ROWarrantyRenewalCategory>();
        if (await set.AnyAsync()) return;
        set.AddRange(
            new Ser_MST_ROWarrantyRenewalCategory { WrtReneCateCode = "CATE01", WrtReneCateName = "Gia hạn bảo hành 1 năm" },
            new Ser_MST_ROWarrantyRenewalCategory { WrtReneCateCode = "CATE02", WrtReneCateName = "Gia hạn bảo hành 2 năm" });
        await db.SaveChangesAsync();
    }
}
