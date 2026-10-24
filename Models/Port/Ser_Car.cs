using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiniService.Models.Port;

// port 1:1 OSHCCController/SearchRODetailHistory — Ser_Car (stub)
// Nguồn: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\OSHCC\BizCarSv.HCC.cs:637-642 (join)
//        Các cột từ dòng 620-642 (select CarID, PlateNo, TradeMarkCode, ModelID, ColorCode, Frameno, EngineNo, ProductYear).
public class Ser_Car
{
    public string CarID { get; set; } = "";
    public string? DealerCode { get; set; }
    public string? CusID { get; set; }
    public string? PlateNo { get; set; }
    public string? TradeMarkCode { get; set; }
    public string? ModelID { get; set; }
    public string? ColorCode { get; set; }
    public string? Frameno { get; set; }
    public string? EngineNo { get; set; }
    public string? ProductYear { get; set; }
}

public class Ser_CarConfiguration : IEntityTypeConfiguration<Ser_Car>
{
    public void Configure(EntityTypeBuilder<Ser_Car> builder)
    {
        builder.ToTable("Ser_Car");
        builder.HasKey(x => new { x.CarID, x.DealerCode });
    }
}
