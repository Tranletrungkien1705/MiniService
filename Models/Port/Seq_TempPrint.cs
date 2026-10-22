using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiniService.Models.Port;

// Entity bảng sequence phục vụ SequenceGetForDMS với SequenceType = TempPrint
// Nguồn Biz: Master.cs:195: htMap.Add(TConst.SequenceTypeDMS.TempPrint, new string[] { "Seq_TempPrint", "{0}{1}{2:00000}", "100000" });
public class Seq_TempPrint
{
    public long AutoId { get; set; }
}

public class Seq_TempPrintConfiguration : IEntityTypeConfiguration<Seq_TempPrint>
{
    public void Configure(EntityTypeBuilder<Seq_TempPrint> builder)
    {
        builder.ToTable("Seq_TempPrint");
        builder.HasKey(x => x.AutoId);
        builder.Property(x => x.AutoId).ValueGeneratedOnAdd();
    }
}
