using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 MstSerAppTypeController.cs — Mst_Ser_AppType (GetAllActive).
// Nguồn: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\MstSerAppTypeController.cs:40-219 (gate GetAllActive)
// Biz:   D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Service.cs:327-576 (Mst_Ser_AppType_Get)
// Model nguồn: idn.CarService.Common.Models\Mst_Ser_AppType.cs (AppTypeCode, AppTypeName).
// Không lọc FlagActive, OrgID, NetworkID; thứ tự AppTypeCode asc.
// MyIdxSeq = row number (0-based) sinh bởi SQL, không phải cột bảng.
public class Mst_Ser_AppType
{
    public string AppTypeCode { get; set; } = "";
    public string? AppTypeName { get; set; }
    public long MyIdxSeq { get; set; } // row number (0-based) từ SQL (biz:422, 463).
}

public class Mst_Ser_AppTypeConfiguration : IEntityTypeConfiguration<Mst_Ser_AppType>
{
    public void Configure(EntityTypeBuilder<Mst_Ser_AppType> b)
    {
        b.ToTable("Mst_Ser_AppType");
        b.HasKey(x => x.AppTypeCode);
        b.Property(x => x.AppTypeCode).HasMaxLength(50);
        b.Property(x => x.AppTypeName).HasMaxLength(250);
        b.Ignore(x => x.MyIdxSeq); // computed by SELECT, không lưu trong DB
    }
}

/// <summary>Dữ liệu demo cho GetAllActive.</summary>
public class Mst_Ser_AppTypeSeeder : IPortSeeder
{
    // Order danh mục 10-99; grep Models/Port: 11, 12 đã dùng ⇒ chọn 13.
    public int Order => 10;

    public async Task SeedAsync(AppDbContext db)
    {
        var set = db.Set<Mst_Ser_AppType>();
        if (await set.AnyAsync()) return;
        set.AddRange(
            new Mst_Ser_AppType { AppTypeCode = "APP_A", AppTypeName = "Ứng dụng A", },
            new Mst_Ser_AppType { AppTypeCode = "APP_B", AppTypeName = "Ứng dụng B", },
            new Mst_Ser_AppType { AppTypeCode = "APP_C", AppTypeName = "Ứng dụng C", },
            new Mst_Ser_AppType { AppTypeCode = "APP_D", AppTypeName = "Ứng dụng D", }
        );
        await db.SaveChangesAsync();
    }
}
