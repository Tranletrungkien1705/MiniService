using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniService.Data;

namespace MiniService.Models.Port;

// port 1:1 OSHCCController/SearchROHistoryList — tầng GATE + tầng BIZ

public class OSHCC_Ser_ROHistory
{
    public string? ROID { get; set; }
    public string? RONo { get; set; }
    public string? DealerCode { get; set; }
    public string? Status { get; set; }
    public string? StatusName { get; set; }
    public string? CusID { get; set; }
    public string? CusName { get; set; }
    public string? CarID { get; set; }
    public string? FrameNo { get; set; }
    public string? PlateNo { get; set; }
    public string? ModelID { get; set; }
    public string? ModelName { get; set; }
    public string? TradeMarkCode { get; set; }
    public string? TradeMarkName { get; set; }
    public string? TradeMarkNameModel { get; set; }
    public string? DealerName { get; set; }
    public string? CheckinDate { get; set; }
    public string? tmpCheckInDate { get; set; }
    public string? RORONo { get; set; }
    public string? NormalizedRONo { get; set; }
    public string? NormalizedCreator { get; set; }
    public string? Creator { get; set; }
}

public class OSHCC_Ser_ROHistoryConfiguration : IEntityTypeConfiguration<OSHCC_Ser_ROHistory>
{
    public void Configure(EntityTypeBuilder<OSHCC_Ser_ROHistory> builder)
    {
        builder.HasNoKey();
    }
}

/// <summary>
/// Dữ liệu mẫu cho OSHCC_Ser_ROHistory (dùng để kiểm chứng SearchROHistoryList).
/// </summary>
public class OSHCC_Ser_ROHistorySeeder : IPortSeeder
{
    public int Order => 17;

    public async Task SeedAsync(AppDbContext db)
    {
        // Dữ liệu mẫu: tạo Ser_RO, Ser_Customer, Ser_Car, Ser_MST_Model, Ser_Mst_TradeMark, Mst_Dealer
        // để query trả về dữ liệu OSHCC_Ser_ROHistory.
        // Nhưng đối với port 1:1, chỉ tạo dữ liệu khi cần test thực tế.
        await Task.CompletedTask;
    }
}
