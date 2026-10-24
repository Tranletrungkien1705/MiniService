using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiniService.Models.Port;

// port 1:1 OSHCCController/SearchRODetailHistory — Ser_MST_Part (stub)
// Nguồn: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\OSHCC\BizCarSv.HCC.cs:752-766 (left join)
//        Các cột: PartID, PartCode, EngName, VieName, Unit, DealerCode.
public class Ser_MST_Part
{
    public string PartID { get; set; } = "";
    public string? DealerCode { get; set; }
    public string? PartCode { get; set; }
    public string? EngName { get; set; }
    public string? VieName { get; set; }
    public string? Unit { get; set; }
}

public class Ser_MST_PartConfiguration : IEntityTypeConfiguration<Ser_MST_Part>
{
    public void Configure(EntityTypeBuilder<Ser_MST_Part> builder)
    {
        builder.ToTable("Ser_MST_Part");
        builder.HasKey(x => new { x.PartID, x.DealerCode });
    }
}
