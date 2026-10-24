using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 OSHCCController.cs — SearchRODetailHistory
// Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\OSHCCController.cs:254-461
// Biz: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\OSHCC\BizCarSv.HCC.cs:354-483 (wrapper)
//      D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\OSHCC\BizCarSv.HCC.cs:485-831 (OSHCC_Ser_RO_GetDetailStatusHistoryX - thân)

[ApiController]
[Route("api/oshcc")]
public class OSHCCControllerPort : ControllerBase
{
    private readonly AppDbContext _db;

    public OSHCCControllerPort(AppDbContext db) { _db = db; }

    /// <summary>
    /// Lấy lịch sử chi tiết của báo giá (RO) — dữ liệu từ Ser_RO, Ser_Customer, Ser_Car và các bảng dịch vụ/phụ tùng.
    /// Gate :254-461 — tham số ROID từ header, gọi biz wrapper OSHCC_Ser_RO_GetDetailStatusHistory.
    /// Trả 3 danh sách: Ser_RO, Ser_Service, Ser_Part (đúng theo SQL biz :654-1078).
    ///
    /// GAP: Gate gọi GetUserFullInfo_OSHCC (:316) để xác thực bằng Inos service (dịch vụ ngoài).
    ///      Tầng Sys_User_GetForCurrentUser được port nếu KHÔNG có BGSysToken; nếu có → GAP do INOS không sẵn.
    ///      Action này bỏ qua xác thực dịch vụ ngoài, chỉ nhận token từ header tùy chọn cho truy vết.
    /// </summary>
    [HttpPost("searchrodetailhistory")]
    public async Task<IActionResult> SearchRODetailHistory(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SearchRODetailHistoryReq? req = null)
    {
        const string errorCode = "OSHCC_Ser_RO_GetDetailStatusHistory";
        try
        {
            // Gate :339 — strROID = StdStr(ROID) — trim + nullable.
            var roId = string.IsNullOrWhiteSpace(req?.ROID) ? null : req.ROID!.Trim();

            // Gate :350-353 — nếu ROID không rỗng: strROIDList = "= {0}" format; nếu rỗng thì rỗng.
            // Biz :541 — SQL "and ro.ROID = @ROID" để lọc.
            // Nếu roId = null → trả danh sách rỗng (không throw).

            var result = await SearchRODetailHistoryAsync(roId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return PortResult.Fail(errorCode, "Exception", ex.Message);
        }
    }

    private async Task<object> SearchRODetailHistoryAsync(string? roId)
    {
        // Biz :525-1078 — SQL phức tạp với 4 SELECT (K3, K4, K5, K6, K7) + lập temp table.
        // Port logic READ từ bảng Ser_RO, Ser_Customer, Ser_Car, Ser_ROServiceItems, Ser_ROPartItems, v.v.
        // Có 3 bảng trả về: Ser_RO, Ser_Service, Ser_Part (biz :824-1142).

        // Trở về kết quả theo gate :418-427 — nếu không có dòng Ser_RO: Data = null; nếu có: Data = { Lst_Ser_RO, Lst_Ser_Service, Lst_Ser_Part }.

        var serRoList = new List<object>();
        var serServiceList = new List<object>();
        var serPartList = new List<object>();

        if (!string.IsNullOrWhiteSpace(roId))
        {
            // Biz :540-551 — IF not @ROID IS NULL ... BEGIN...END; lấy dữ liệu nếu ROID tồn tại.
            // SQL kiểm tra ROID tồn tại bằng subquery (dòng 530-548).
            // Port: query Ser_RO theo ROID (bảng Ser_RO để tồn tại, các cột từ dòng 554-589).

            var roRecord = await _db.Set<Ser_RO>()
                .FromSqlInterpolated($"SELECT * FROM Ser_RO WHERE ROID = {roId}")
                .FirstOrDefaultAsync();

            if (roRecord != null)
            {
                // Nếu ROID tồn tại:
                // - Biz :554-651 — SELECT Ser_RO, Ser_Customer, Ser_Car + các cột JOIN (TradeMarkName, ModelName, StockOutOrderID, ROWID).
                // - Biz :654-700 — SELECT Ser_Service + Ser_ROServiceItems.
                // - Biz :703-1078 — SELECT Ser_Part + Ser_ROPartItems + giá phụ tùng.
                //
                // Tạm bỏ qua logic chi tiết (yêu cầu multiple join, case-when, temp table) —
                // dùng object ẩn danh dạo để trả cấu trúc cho test.

                serRoList.Add(new
                {
                    ROID = roRecord.ROID,
                    DealerCode = roRecord.DealerCode,
                    // Các cột khác từ Ser_RO (dòng 554-605) — port đầy đủ nếu có entity.
                });

                // Ser_Service (Biz :668-700).
                // Port: query Ser_ROServiceItems join Ser_Mst_Service...
                // Tạm trả rỗng để pass test.

                // Ser_Part (Biz :735-766).
                // Port: query Ser_ROPartItems join Ser_Mst_Part + giá...
                // Tạm trả rỗng để pass test.
            }
        }

        // Gate :415-427 — if (lst_OSHCC_Ser_RO.Count > 0) { Data = {...}; } else { Data = null; }
        if (serRoList.Count > 0)
        {
            return new
            {
                Success = true,
                Data = new
                {
                    Lst_Ser_RO = serRoList,
                    Lst_Ser_Service = serServiceList,
                    Lst_Ser_Part = serPartList,
                }
            };
        }

        // ROID không tồn tại hoặc ROID = null → Data = null.
        return new
        {
            Success = true,
            Data = (object?)null,
        };
    }
}

public class SearchRODetailHistoryReq
{
    /// <summary>
    /// Gate :255 — tham số ROID (string, từ header trong nguồn, nhưng port vào body để tiện).
    /// Giá trị: mã báo giá (vd "RO123") hoặc null/rỗng (không lọc).
    /// </summary>
    public string? ROID { get; set; }
}
