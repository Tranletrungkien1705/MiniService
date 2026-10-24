using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiniService.Models.Port;

// port 1:1 OSHCCController/SearchCustomerCar — Ser_Insurance
// Nguồn: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\OSHCC\BizCarSv.HCC.cs:1031-1035 (left join để lọc InsNo)
//        Các cột: InsNO, DealerCode.
public class Ser_Insurance
{
    public string? InsNO { get; set; }
    public string? DealerCode { get; set; }
}

public class Ser_InsuranceConfiguration : IEntityTypeConfiguration<Ser_Insurance>
{
    public void Configure(EntityTypeBuilder<Ser_Insurance> builder)
    {
        builder.ToTable("Ser_Insurance");
        builder.HasNoKey();
    }
}
