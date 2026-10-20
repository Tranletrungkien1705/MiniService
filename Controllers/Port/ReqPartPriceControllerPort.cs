using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 ReqPartPriceController.cs — Req_PartPrice
// Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\ReqPartPriceController.cs:1818-2114
//       + :3042-3087 (strWhereClause_Req_PartPrice_Search)
// Biz:  D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.SuggestPrice.cs:136-270 (Req_PartPrice_Get_DL)
//       -> :413-613 (Req_PartPrice_GetX)
[ApiController]
[Route("api/reqpartprice")]
public class ReqPartPriceControllerPort : ControllerBase
{
    private readonly AppDbContext _db;

    public ReqPartPriceControllerPort(AppDbContext db) { _db = db; }

    /// <summary>
    /// Lấy đề nghị báo giá + dòng chi tiết theo Số đề nghị (like). Gate :1818-2114.
    /// Body nhận qua JSON (A.14); route giữ chính tả nguồn "NoDL".
    /// </summary>
    [HttpPost("getbyreqpartpricenoDL")]
    public async Task<IActionResult> GetByReqPartPriceNoDL(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] ReqPartPrice_GetByReqPartPriceNoDLReq? req = null)
    {
        try
        {
            // Gate :1900 — `CUtils.StdStr(ReqPartPriceNo)` = Trim + normalize, rỗng → null.
            var reqPartPriceNo = string.IsNullOrWhiteSpace(req?.ReqPartPriceNo) ? null : req.ReqPartPriceNo!.Trim();

            // Gate :1910-1911 + BaseController:43-44 — Ft_RecordStart = "0", Ft_RecordCount = "123456000".
            // Biz :446-447 — nFilterRecordStart = 0; nFilterRecordEnd = 0 + 123456000 - 1.
            long recordStart = 0;
            long recordCount = 123456000;
            var recordEnd = recordStart + recordCount - 1;

            // Gate :1913-1916 + :3050-3053 — Where: nếu non-empty thì `Req_PartPrice.ReqPartPriceNo like '%x%'`.
            var query = _db.Set<Req_PartPrice>().AsNoTracking().AsQueryable();
            if (reqPartPriceNo != null)
            {
                var pattern = PortQuery.Contains(reqPartPriceNo);
                query = query.Where(x => EF.Functions.Like((x.ReqPartPriceNo ?? "").ToLower(), pattern, PortQuery.Esc));
            }

            // Biz :461-473 — #tbl_Req_PartPrice_Draft: `select distinct rpp.ReqPartPriceNo from Req_PartPrice rpp left join Req_PartPriceDtl rppdt on rpp.ReqPartPriceNo=rppdt.ReqPartPriceNo where (1=1) <where> order by rpp.ReqPartPriceNo asc`.
            // Khoá ReqPartPriceNo là duy nhất ⇒ distinct không mất dòng.
            var draftNos = await query
                .OrderBy(x => x.ReqPartPriceNo)
                .Select(x => x.ReqPartPriceNo)
                .ToListAsync();

            // Biz :475-487 — Summary + #tbl_Req_PartPrice_Filter (cắt MyIdxSeq trong [start, end]).
            // MyIdxSeq nguồn = identity(bigint, 0, 1) ⇒ vị trí 0..N-1 trong draftNos (đã sort theo ReqPartPriceNo).
            var filteredNos = new List<string>();
            if (draftNos.Count > 0 && recordStart >= 0 && recordCount > 0)
            {
                for (var i = 0; i < draftNos.Count; i++)
                {
                    if (i >= recordStart && i <= recordEnd) filteredNos.Add(draftNos[i]);
                }
            }

            // Biz :507-522 — Req_PartPrice: `select t.MyIdxSeq, rpp.*, md.DealerName from #tbl_Req_PartPrice_Filter t inner join Req_PartPrice rpp on t.ReqPartPriceNo=rpp.ReqPartPriceNo inner join Mst_Dealer md on rpp.DealerCode=md.DealerCode order by t.MyIdxSeq asc`.
            // inner join Mst_Dealer ⇒ Req_PartPrice có DealerCode không tồn tại bị loại (GAP: cột DealerName không có trong model nguồn nên không trả ra).
            var rppList = new List<ReqPartPrice_GetByReqPartPriceNoDLRow>();
            if (filteredNos.Count > 0)
            {
                var rppData = await (
                    from rpp in _db.Set<Req_PartPrice>().AsNoTracking()
                    join md in _db.Set<Mst_Dealer>().AsNoTracking() on rpp.DealerCode equals md.DealerCode
                    where filteredNos.Contains(rpp.ReqPartPriceNo)
                    select new { rpp }
                ).ToListAsync();

                var rppByNo = rppData.ToDictionary(x => x.rpp.ReqPartPriceNo, x => x.rpp);
                for (var i = 0; i < filteredNos.Count; i++)
                {
                    if (!rppByNo.TryGetValue(filteredNos[i], out var rpp)) continue;
                    rppList.Add(new ReqPartPrice_GetByReqPartPriceNoDLRow
                    {
                        MyIdxSeq = i,
                        ReqPartPriceNo = rpp.ReqPartPriceNo,
                        Description = rpp.Description,
                        FlagIsCheck = rpp.FlagIsCheck,
                        TSTReqPartPriceID = rpp.TSTReqPartPriceID,
                        // Gate :2016-2034 — CreateDTime: IsDateTime ⇒ "yyyy-MM-dd"; null ⇒ null.
                        CreateDTime = FormatDate(rpp.CreateDTime),
                        CreateBy = rpp.CreateBy,
                        DMSReqPartPriceStatus = rpp.DMSReqPartPriceStatus,
                        TSTReqPartPriceStatus = rpp.TSTReqPartPriceStatus,
                        // Gate :2036-2054 — TSTSentDate cùng định dạng.
                        TSTSentDate = FormatDate(rpp.TSTSentDate),
                        // Gate :1976-1996 / :1998-2014 — bổ sung Name sau khi đọc bảng.
                        DMSReqPartPriceStatusName = MapDmsName(rpp.DMSReqPartPriceStatus),
                        TSTReqPartPriceStatusName = MapTstName(rpp.TSTReqPartPriceStatus),
                    });
                }
            }

            // Biz :530-545 — Req_PartPriceDtl: `select t.MyIdxSeq, rppdt.*, mdf.DeliveryFormName from #tbl_Req_PartPrice_Filter t inner join Req_PartPriceDtl rppdt on t.ReqPartPriceNo=rppdt.ReqPartPriceNo inner join Mst_DeliveryForm mdf on rppdt.DeliveryFormCode=mdf.DeliveryFormCode order by t.MyIdxSeq asc`.
            var dtlList = new List<ReqPartPrice_GetByReqPartPriceNoDLDtlRow>();
            if (filteredNos.Count > 0)
            {
                var dtlData = await (
                    from rppdt in _db.Set<Req_PartPriceDtl>().AsNoTracking()
                    join mdf in _db.Set<Mst_DeliveryForm>().AsNoTracking() on rppdt.DeliveryFormCode equals mdf.DeliveryFormCode
                    where filteredNos.Contains(rppdt.ReqPartPriceNo)
                    select new { rppdt, mdf }
                ).ToListAsync();

                var order = new Dictionary<string, int>();
                for (var i = 0; i < filteredNos.Count; i++) order[filteredNos[i]] = i;

                foreach (var row in dtlData.OrderBy(x => order[x.rppdt.ReqPartPriceNo]))
                {
                    dtlList.Add(new ReqPartPrice_GetByReqPartPriceNoDLDtlRow
                    {
                        MyIdxSeq = order[row.rppdt.ReqPartPriceNo],
                        ReqPartPriceNo = row.rppdt.ReqPartPriceNo,
                        DMSPartCode = row.rppdt.DMSPartCode,
                        VieName = row.rppdt.VieName,
                        VINCode = row.rppdt.VINCode,
                        DeliveryFormCode = row.rppdt.DeliveryFormCode,
                        Remark = row.rppdt.Remark,
                        // mdf.DeliveryFormName từ join Mst_DeliveryForm (Biz :535, :539-540).
                        DeliveryFormName = row.mdf.DeliveryFormName,
                        TSTPrice = row.rppdt.TSTPrice,
                        DateEffect = row.rppdt.DateEffect,
                        TSTPartCode = row.rppdt.TSTPartCode,
                        FlagIsCheck = row.rppdt.FlagIsCheck,
                    });
                }
            }

            // Gate :2071-2077 — Data = new { Lst_Req_PartPrice, Lst_Req_PartPriceDtl }; objJsonResultEntry.SetSuccess().
            // Gate :2084 — Result<CMyMsgResult>(msgResult) ⇒ Success = true.
            return Ok(new
            {
                Success = true,
                Data = new
                {
                    Lst_Req_PartPrice = rppList,
                    Lst_Req_PartPriceDtl = dtlList,
                }
            });
        }
        catch (Exception ex)
        {
            // Biz :656 — mã lỗi mặc định của biz = "Req_PartPrice_Get_DL".
            return PortResult.Fail("Req_PartPrice_Get_DL", "Exception", ex.Message);
        }
    }

    // Gate :1976-1996 — DMSReqPartPriceStatus ⇒ tên (TConst.DMSReqPartPriceStatus.*, Const.Main.cs:19358-19364).
    // Gate gán strName = "" trước khi kiểm từng giá trị.
    private static string MapDmsName(string? status)
    {
        var s = (status ?? "").Trim();
        return s switch
        {
            "P" => "P",   // DMSReqPartPriceStatus.Pending   (:19360)
            "A" => "A",   // DMSReqPartPriceStatus.Approved  (:19361)
            "F" => "F",   // DMSReqPartPriceStatus.Finished  (:19362)
            "R" => "R",   // DMSReqPartPriceStatus.Rejected  (:19363)
            _ => "",
        };
    }

    // Gate :1998-2014 — TSTReqPartPriceStatus ⇒ tên tiếng Việt (Const.Main.cs:19366-19371).
    private static string MapTstName(string? status)
    {
        var s = (status ?? "").Trim();
        return s switch
        {
            "1" => "Chờ duyệt",                 // TSTReqPartPriceStatus.Pending   (:19368)
            "2" => "Đã duyệt, chờ hoàn thiện",  // TSTReqPartPriceStatus.Approved  (:19369)
            "4" => "Đã hoàn thiện",             // TSTReqPartPriceStatus.Finished  (:19370)
            _ => "",
        };
    }

    // Gate :2016-2054 — CreateDTime / TSTSentDate: StdStr != "" ⇒ IsDateTime ? ToString("yyyy-MM-dd") : Substring(0,10).
    // Ở đích kiểu đã là DateTime? ⇒ format "yyyy-MM-dd"; null giữ null.
    private static string? FormatDate(DateTime? value)
        => value?.ToString("yyyy-MM-dd");
}

/// <summary>
/// Body của action — gate chỉ nhận 1 tham số `string ReqPartPriceNo` (ReqPartPriceController.cs:1819).
/// A.14: đích nhận qua JSON body.
/// </summary>
public class ReqPartPrice_GetByReqPartPriceNoDLReq
{
    public string? ReqPartPriceNo { get; set; }
}

/// <summary>
/// Dòng Req_PartPrice trong Data — theo model nguồn 12.Dev.Common\...\CarSv\Req_PartPrice.cs:9-22
/// (thêm MyIdxSeq = identity(bigint,0,1) và 2 cột Name do gate tự sinh ở :1963-2014).
/// </summary>
public class ReqPartPrice_GetByReqPartPriceNoDLRow
{
    public int MyIdxSeq { get; set; }
    public string ReqPartPriceNo { get; set; } = "";
    public string? Description { get; set; }
    public string? FlagIsCheck { get; set; }
    public string? TSTReqPartPriceID { get; set; }
    public string? CreateDTime { get; set; }
    public string? CreateBy { get; set; }
    public string? DMSReqPartPriceStatus { get; set; }
    public string? TSTReqPartPriceStatus { get; set; }
    public string? TSTSentDate { get; set; }
    public string? DMSReqPartPriceStatusName { get; set; }
    public string? TSTReqPartPriceStatusName { get; set; }
}

/// <summary>
/// Dòng Req_PartPriceDtl trong Data — theo model nguồn 12.Dev.Common\...\CarSv\Req_PartPrice.cs:23-36
/// (thêm MyIdxSeq và cột DeliveryFormName từ join Mst_DeliveryForm).
/// </summary>
public class ReqPartPrice_GetByReqPartPriceNoDLDtlRow
{
    public int MyIdxSeq { get; set; }
    public string ReqPartPriceNo { get; set; } = "";
    public string DMSPartCode { get; set; } = "";
    public string? VieName { get; set; }
    public string? VINCode { get; set; }
    public string? DeliveryFormCode { get; set; }
    public string? Remark { get; set; }
    public string? DeliveryFormName { get; set; }
    public double? TSTPrice { get; set; }
    public DateTime? DateEffect { get; set; }
    public string? TSTPartCode { get; set; }
    public string? FlagIsCheck { get; set; }
}
