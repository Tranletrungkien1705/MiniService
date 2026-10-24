using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiniService.Models.Port;

// port 1:1 OSHCCController/SearchRODetailHistory — Ser_Mst_TradeMark (stub)
// Nguồn: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\OSHCC\BizCarSv.HCC.cs:641-643 (left join)
//        Các cột: TradeMarkCode, TradeMarkName, DealerCode.
public class Ser_Mst_TradeMark
{
    public string TradeMarkCode { get; set; } = "";
    public string? DealerCode { get; set; }
    public string? TradeMarkName { get; set; }
}

public class Ser_Mst_TradeMarkConfiguration : IEntityTypeConfiguration<Ser_Mst_TradeMark>
{
    public void Configure(EntityTypeBuilder<Ser_Mst_TradeMark> builder)
    {
        builder.ToTable("Ser_Mst_TradeMark");
        builder.HasKey(x => new { x.TradeMarkCode, x.DealerCode });
    }
}
