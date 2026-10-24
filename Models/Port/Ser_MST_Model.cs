using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiniService.Models.Port;

// port 1:1 OSHCCController/SearchRODetailHistory — Ser_MST_Model (stub)
// Nguồn: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\OSHCC\BizCarSv.HCC.cs:639-640 (left join)
//        Các cột: ModelID, ModelName.
public class Ser_MST_Model
{
    public string ModelID { get; set; } = "";
    public string? ModelName { get; set; }
}

public class Ser_MST_ModelConfiguration : IEntityTypeConfiguration<Ser_MST_Model>
{
    public void Configure(EntityTypeBuilder<Ser_MST_Model> builder)
    {
        builder.ToTable("Ser_MST_Model");
        builder.HasKey(x => x.ModelID);
    }
}
