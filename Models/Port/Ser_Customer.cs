using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiniService.Models.Port;

// port 1:1 OSHCCController/SearchRODetailHistory — Ser_Customer (stub)
// Nguồn: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\OSHCC\BizCarSv.HCC.cs:635-649 (join)
//        Các cột từ dòng 594-930 (select CusID, CusName, CusAddress, CusTel, CusMobile, TaxCode).
public class Ser_Customer
{
    public string CusID { get; set; } = "";
    public string? DealerCode { get; set; }
    public string? CusName { get; set; }
    public string? ContName { get; set; }
    public string? Address { get; set; }
    public string? ContAddress { get; set; }
    public string? Tel { get; set; }
    public string? ContTel { get; set; }
    public string? Mobile { get; set; }
    public string? ContMobile { get; set; }
    public string? TaxCode { get; set; }
}

public class Ser_CustomerConfiguration : IEntityTypeConfiguration<Ser_Customer>
{
    public void Configure(EntityTypeBuilder<Ser_Customer> builder)
    {
        builder.ToTable("Ser_Customer");
        builder.HasKey(x => new { x.CusID, x.DealerCode });
    }
}
