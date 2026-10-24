using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiniService.Models.Port;

// port 1:1 OSHCCController/SearchRODetailHistory — Ser_ROServiceItems (stub)
// Nguồn: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\OSHCC\BizCarSv.HCC.cs:683-700 (select from Ser_ROServiceItems)
//        Các cột: itemid, ROID, SerID, Note, ExpenseType.
public class Ser_ROServiceItems
{
    public int ItemID { get; set; }
    public string? ROID { get; set; }
    public string? SerID { get; set; }
    public string? Note { get; set; }
    public string? ExpenseType { get; set; }
}

public class Ser_ROServiceItemsConfiguration : IEntityTypeConfiguration<Ser_ROServiceItems>
{
    public void Configure(EntityTypeBuilder<Ser_ROServiceItems> builder)
    {
        builder.ToTable("Ser_ROServiceItems");
        builder.HasKey(x => x.ItemID);
    }
}
