using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 BtlBulletinDealerController.cs — Btl_Bulletin
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\BtlBulletinDealerController.cs:273-512 (SearchWHDL)
//         :513-756 (GetAllActiveByVIN)
//   Biz:  D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Bulletin.cs:262-423 (Blt_Bulletin_GetByVin_WHDL)
//         :100-261 (Blt_Bulletin_GetByVin_DL)
//         -> Blt_Bulletin_GetByVinX (thân hàm KHÔNG có trong source_excerpt.txt — xem GAP)
[ApiController]
[Route("api/btlbulletdealer")]
public class BtlBulletinDealerControllerPort : ControllerBase
{
    private readonly AppDbContext _db;

    public BtlBulletinDealerControllerPort(AppDbContext db) => _db = db;

    /// <summary>
    /// Tìm thông báo (bulletin) theo số VIN của đại lý.
    /// Gate: BtlBulletinDealerController.cs:273-512 SearchWHDL(DealerCode, VIN, Ft_PageIndex = 0, Ft_PageSize = 10).
    /// Biz:  BizCarSv.Bulletin.cs:262-423 -> Blt_Bulletin_GetByVinX.
    /// </summary>
    [HttpPost("searchwhdl")]
    public async Task<IActionResult> SearchWHDL(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] BtlBulletinDealer_SearchWHDLReq? req = null)
    {
        // Gate :276-277 — Ft_PageIndex = 0, Ft_PageSize = 10 (giá trị mặc định của chữ ký nguồn).
        var pageIndex = req?.Ft_PageIndex ?? 0;
        var pageSize = req?.Ft_PageSize ?? 10;

        // Gate :372 — strVIN = StdStr(VIN) (Trim, rỗng -> null).
        var strVIN = StdStr(req?.VIN);

        // Gate :371 — strDealerCode = "" (dòng gán DealerCode từ orgUserUI bị comment tại :370) ⇒ bộ lọc DealerCode LUÔN rỗng.
        // Gate :374-378 — các danh sách điều kiện:
        //   strDealerCodeConditionList = GenEqualConditionSingle("")        ⇒ bỏ mệnh đề
        //   strVINConditionList        = GenEqualConditionSingle(strVIN)    ⇒ VIN = @VIN
        //   strStatusConditionList     = GenEqualConditionSingle(TConst.Stage.Pending)
        //   strIsActive                = TConst.Flag.Active
        //   strDateExpiredConditionList = GenGreaterOrEqualCondition(hôm nay) ⇒ DateExpired >= hôm nay
        // Gate :397-407 — VIN rỗng ⇒ ném Btl_Bulletin_Dealer_SearchWHDL_InvalidBulletinNoNullOrEmpty.
        if (string.IsNullOrEmpty(strVIN))
        {
            return PortResult.Fail("Btl_Bulletin_Dealer_SearchWHDL_InvalidBulletinNoNullOrEmpty",
                "Check.Description", "Số VIN bắt buộc nhập");
        }

        // Gate :378 — dtimeSys.ToString("yyyy-MM-dd").
        var today = DateTime.Now.ToString("yyyy-MM-dd");

        // Gate :393-394 — strFt_RecordStart = pageIndex * pageSize, strFt_RecordCount = pageSize.
        var recordStart = (long)pageIndex * pageSize;
        var recordCount = pageSize;

        // Biz :353-378 — Blt_Bulletin_GetByVinX với các bộ lọc trên (các danh sách "" ⇒ bỏ mệnh đề).
        var query = _db.Set<Btl_Bulletin>().AsNoTracking()
            .Where(x => EF.Functions.Like((x.VIN ?? "").ToLower(), PortQuery.Equal(strVIN), PortQuery.Esc))
            .Where(x => EF.Functions.Like((x.Status ?? "").ToLower(), PortQuery.Equal("PENDING"), PortQuery.Esc))
            .Where(x => EF.Functions.Like((x.FlagActive ?? "").ToLower(), PortQuery.Equal("1"), PortQuery.Esc))
            // GenGreaterOrEqualCondition(hôm nay) ⇒ DateExpired >= hôm nay (so chuỗi yyyy-MM-dd như nguồn).
            .Where(x => string.Compare(x.DateExpired, today) >= 0);

        // Gate :461 — itemCount lấy từ MySummaryTable.MyCount; gate :466 ghi đè bằng số dòng trả về.
        var itemCount = await query.CountAsync();

        // Gate :469-473 — pageSize = 0 ⇒ pageSize = PageSizeConfig (giá trị KHÔNG có trong excerpt — xem GAP);
        // pageCount = (itemCount % pageSize == 0) ? itemCount / pageSize : itemCount / pageSize + 1.
        var pageCount = 0;
        if (pageSize == 0)
        {
            // GAP: PageSizeConfig của BaseController nguồn không có trong source_excerpt.txt.
            pageCount = 0;
        }
        else
        {
            pageCount = (itemCount % pageSize == 0) ? itemCount / pageSize : itemCount / pageSize + 1;
        }

        // Gate :385-388 — PageInfo(pageIndex, pageSize) { DataList = new List<Btl_Bulletin>() }.
        // Gate :468 — pageInfo.DataList.AddRange(lst_Btl_Bulletin).
        var dataList = new List<Btl_Bulletin>();
        if (recordCount > 0 && recordStart >= 0)
        {
            dataList = await query
                .OrderBy(x => x.BulletinID)
                .Skip((int)Math.Min(recordStart, int.MaxValue))
                .Take((int)Math.Min(recordCount, int.MaxValue))
                .ToListAsync();
        }

        // Gate :476-478 — pageInfo.ItemCount = itemCount; pageInfo.PageCount = pageCount; msgResult._objResult = pageInfo.
        // Gate :483 — Result<CMyMsgResult>(msgResult) ⇒ Success = true, Data = PageInfo.
        return Ok(new
        {
            Success = true,
            Data = new PageInfo<Btl_Bulletin>(dataList, itemCount, pageCount, pageIndex, pageSize)
        });
    }

    /// <summary>TUtils.CUtils.StdStr: Trim, rỗng -> null (PORT_RULES mục 3).</summary>
    private static string? StdStr(string? value)
    {
        var s = value?.Trim();
        return string.IsNullOrEmpty(s) ? null : s;
    }

    /// <summary>
    /// Lấy các bản tin (bulletin) còn hiệu lực theo số VIN của đại lý.
    /// Gate: BtlBulletinDealerController.cs:513-756 GetAllActiveByVIN(VIN).
    /// Biz:  BizCarSv.Bulletin.cs:100-261 Blt_Bulletin_GetByVin_DL -> Blt_Bulletin_GetByVinX.
    /// </summary>
    [HttpPost("getallactivebyvin")]
    public async Task<IActionResult> GetAllActiveByVIN(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] BtlBulletinDealer_GetAllActiveByVINReq? req = null)
    {
        // Gate :601 — strVIN = StdStr(VIN) (Trim, rỗng -> null).
        var strVIN = StdStr(req?.VIN);

        // Gate :603-606 — các danh sách điều kiện:
        //   strDealerCodeConditionList = "" (gate :603)          ⇒ bỏ mệnh đề
        //   strVINConditionList        = GenEqualConditionSingle(strVIN) ⇒ VIN = @VIN
        //   strStatusConditionList     = "" (gate :605)          ⇒ bỏ mệnh đề
        //   strIsActive                = TConst.Flag.Active      ⇒ FlagActive = '1'
        // Gate :609-610 — pageIndex = 0, pageSize = 0 (PageInfo rỗng, không phân trang).
        // Gate :620-621 — strFt_RecordStart = Ft_RecordStart, strFt_RecordCount = Ft_RecordCount (BaseController).
        var recordStart = Ft_RecordStart;
        var recordCount = Ft_RecordCount;

        // Biz :191-216 — Blt_Bulletin_GetByVinX với các bộ lọc trên (các danh sách "" ⇒ bỏ mệnh đề).
        var query = _db.Set<Btl_Bulletin>().AsNoTracking()
            .Where(x => EF.Functions.Like((x.FlagActive ?? "").ToLower(), PortQuery.Equal("1"), PortQuery.Esc));
        if (strVIN != null)
            query = query.Where(x => EF.Functions.Like((x.VIN ?? "").ToLower(), PortQuery.Equal(strVIN), PortQuery.Esc));

        // Gate :620-621 — Ft_RecordStart / Ft_RecordCount của BaseController (giá trị KHÔNG có trong excerpt — xem GAP).
        if (recordStart > 0)
            query = query.Skip((int)Math.Min(recordStart, int.MaxValue));
        if (recordCount > 0)
            query = query.Take((int)Math.Min(recordCount, int.MaxValue));

        var lst_Btl_Bulletin = await query.ToListAsync();

        // Gate :674-694 — Bỏ bản tin hết hạn: DateExpired rỗng hoặc "0001-01-01" ⇒ bỏ;
        // còn lại giữ khi (DateExpired - hôm nay).TotalDays >= 0.
        var dtBulletinActive = new List<Btl_Bulletin>();
        foreach (var dtrw in lst_Btl_Bulletin)
        {
            var strDateExpired = dtrw.DateExpired ?? "";
            if (!string.IsNullOrEmpty(strDateExpired) && strDateExpired != DateTime.MinValue.ToString("yyyy-MM-dd"))
            {
                if (!DateTime.TryParse(strDateExpired, out var dateExpiredCur)) continue;
                var dateExpiredCal = new DateTime(dateExpiredCur.Year, dateExpiredCur.Month, dateExpiredCur.Day);
                var datenow = DateTime.Now;
                var datenowCal = new DateTime(datenow.Year, datenow.Month, datenow.Day);
                var day = (dateExpiredCal - datenowCal).TotalDays;
                if (day >= 0) dtBulletinActive.Add(dtrw);
            }
        }

        // Gate :720-722 — pageInfo.DataList.AddRange(lst_Btl_BulletinDtl); msgResult._objResult = pageInfo.
        // Gate :727 — Result<CMyMsgResult>(msgResult) ⇒ Success = true, Data = PageInfo.
        return Ok(new
        {
            Success = true,
            Data = new PageInfo<Btl_Bulletin>(dtBulletinActive, 0, 0, 0, 0)
        });
    }

    // Gate :620-621 — Ft_RecordStart / Ft_RecordCount là thuộc tính của BaseController nguồn; giá trị KHÔNG có trong
    // source_excerpt.txt ⇒ GAP, không tự đặt giá trị (dùng 0 = không cắt dòng).
    private const int Ft_RecordStart = 0;
    private const int Ft_RecordCount = 0;
}

/// <summary>
/// Body của SearchWHDL — 4 tham số của gate (BtlBulletinDealerController.cs:273-278).
/// Ft_PageIndex = 0, Ft_PageSize = 10 là giá trị mặc định của chữ ký nguồn.
/// </summary>
public class BtlBulletinDealer_SearchWHDLReq
{
    public string? DealerCode { get; set; }
    public string? VIN { get; set; }
    public int? Ft_PageIndex { get; set; }
    public int? Ft_PageSize { get; set; }
}

/// <summary>
/// Body của GetAllActiveByVIN — 1 tham số của gate (BtlBulletinDealerController.cs:513-515).
/// </summary>
public class BtlBulletinDealer_GetAllActiveByVINReq
{
    public string? VIN { get; set; }
}
