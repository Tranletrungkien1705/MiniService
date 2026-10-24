using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiniService.Models.Port;

// port 1:1 OSHCCController/SearchRODetailHistory — Ser_Mst_Service (stub)
// Nguồn: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\OSHCC\BizCarSv.HCC.cs:684-698 (left join)
//        Các cột: SerID, SerCode, SerName.
public class Ser_Mst_Service
{
    public string SerID { get; set; } = "";
    public string? SerCode { get; set; }
    public string? SerName { get; set; }
}

public class Ser_Mst_ServiceConfiguration : IEntityTypeConfiguration<Ser_Mst_Service>
{
    public void Configure(EntityTypeBuilder<Ser_Mst_Service> builder)
    {
        builder.ToTable("Ser_Mst_Service");
        builder.HasKey(x => x.SerID);
    }
}
