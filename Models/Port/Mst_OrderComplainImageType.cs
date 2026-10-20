using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 Mst_OrderComplainImageTypeController.cs — Mst_OrderComplainImageType
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\Mst_OrderComplainImageTypeController.cs:253-436 (GetAllActive)
//   Biz:  D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Master.cs:60804-60942 (Mst_OrderComplainImageType_Get)
//         -> 60944-61087 (Mst_OrderComplainImageType_GetX)
// Cột: DDL nguồn (labs-audit/DB_COT.tsv:7212-7216) — OrderComplainImageType · OrderComplainImageName · FlagActive · LogLUBy · LogLUDTime.
// 🔴 Cột tên là `OrderComplainImageName` (KHÔNG có chữ "Type") — giữ nguyên văn, "sửa cho cân đối" là sai cột.
public class Mst_OrderComplainImageType
{
    // Khoá chính một cột (BizCarSv.Master.cs:61002-61006 `select distinct … mocit.OrderComplainImageType`).
    public string OrderComplainImageType { get; set; } = "";

    // BizCarSv.Master.cs:61039 — bộ lọc tên dùng cột này.
    public string? OrderComplainImageName { get; set; }

    // BizCarSv.Master.cs:61040 — bộ lọc FlagActive.
    public string? FlagActive { get; set; }

    public string? LogLUBy { get; set; }
    public string? LogLUDTime { get; set; }
}

public class Mst_OrderComplainImageTypeConfiguration : IEntityTypeConfiguration<Mst_OrderComplainImageType>
{
    public void Configure(EntityTypeBuilder<Mst_OrderComplainImageType> builder)
    {
        builder.ToTable("Mst_OrderComplainImageType");
        builder.HasKey(x => x.OrderComplainImageType);

        builder.Property(x => x.OrderComplainImageType).HasMaxLength(50);
        builder.Property(x => x.OrderComplainImageName).HasMaxLength(250);
        builder.Property(x => x.FlagActive).HasMaxLength(1);
        builder.Property(x => x.LogLUBy).HasMaxLength(50);
        builder.Property(x => x.LogLUDTime).HasMaxLength(50);
    }
}

/// <summary>
/// Dữ liệu mẫu danh mục loại ảnh khiếu nại. Nguồn chỉ có hàm ĐỌC (không có Create/Update trong
/// BizCarSv.Master.cs) nên dữ liệu mẫu là phần thêm của port để kiểm chứng bộ lọc FlagActive.
/// </summary>
public class Mst_OrderComplainImageTypeSeeder : IPortSeeder
{
    // Order danh mục 10-99 (PortInfra.cs:16-17) — chọn 11, chưa dùng trong Models/Port.
    public int Order => 11;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Mst_OrderComplainImageType>();
        if (await set.AnyAsync()) return;

        set.AddRange(
            new Mst_OrderComplainImageType { OrderComplainImageType = "IMG01", OrderComplainImageName = "Ảnh toàn xe", FlagActive = "1", LogLUBy = "port", LogLUDTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
            new Mst_OrderComplainImageType { OrderComplainImageType = "IMG02", OrderComplainImageName = "Ảnh biển số", FlagActive = "1", LogLUBy = "port", LogLUDTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
            new Mst_OrderComplainImageType { OrderComplainImageType = "IMG03", OrderComplainImageName = "Ảnh phụ tùng lỗi", FlagActive = "1", LogLUBy = "port", LogLUDTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
            new Mst_OrderComplainImageType { OrderComplainImageType = "IMG04", OrderComplainImageName = "Ảnh hoá đơn", FlagActive = "0", LogLUBy = "port", LogLUDTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
            new Mst_OrderComplainImageType { OrderComplainImageType = "IMG05", OrderComplainImageName = "Ảnh giấy tờ xe", FlagActive = "0", LogLUBy = "port", LogLUDTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") });

        await db.SaveChangesAsync();
    }
}
