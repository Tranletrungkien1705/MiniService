using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiniService.Models.Port;

// port 1:1 OSHCCController/SearchRODetailHistory — Ser_ROPartItems (stub)
// Nguồn: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\OSHCC\BizCarSv.HCC.cs:751-765 (select from Ser_ROPartItems)
//        Các cột: itemid, PartID, ROID, Quantity, ExpenseType.
public class Ser_ROPartItems
{
    public int ItemID { get; set; }
    public string? PartID { get; set; }
    public string? ROID { get; set; }
    public double? Quantity { get; set; }
    public string? ExpenseType { get; set; }
}

public class Ser_ROPartItemsConfiguration : IEntityTypeConfiguration<Ser_ROPartItems>
{
    public void Configure(EntityTypeBuilder<Ser_ROPartItems> builder)
    {
        builder.ToTable("Ser_ROPartItems");
        builder.HasKey(x => x.ItemID);
    }
}
