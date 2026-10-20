using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 BtlBulletinDealerController.cs — Btl_Bulletin
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\BtlBulletinDealerController.cs:35-272 (SearchDL)
//         :273-512 (SearchWHDL)
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
    /// Tìm thông báo (bulletin) theo số VIN của đại lý — bản DL (dùng chung biz Blt_Bulletin_GetByVin_DL với GetAllActiveByVIN).
    /// Gate: BtlBulletinDealerController.cs:35-272 SearchDL(DealerCode, VIN, Ft_PageIndex = 0, Ft_PageSize = 10).
    /// Biz:  BizCarSv.Bulletin.cs:100-261 Blt_Bulletin_GetByVin_DL -> Blt_Bulletin_GetByVinX.
    /// </summary>
    [HttpPost("searchdl")]
    public async Task<IActionResult> SearchDL(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] BtlBulletinDealer_SearchDLReq? req = null)
    {
        // Gate :38-39 — Ft_PageIndex = 0, Ft_PageSize = 10 (giá trị mặc định của chữ ký nguồn).
        var pageIndex = req?.Ft_PageIndex ?? 0;
        var pageSize = req?.Ft_PageSize ?? 10;

        // Gate :134 — strVIN = StdStr(VIN) (Trim, rỗng -> null).
        var strVIN = StdStr(req?.VIN);

        // Gate :133 — strDealerCode = "" (dòng gán DealerCode từ orgUserUI bị comment tại :132) ⇒ bộ lọc DealerCode LUÔN rỗng.
        // Gate :136-140 — các danh sách điều kiện:
        //   strDealerCodeConditionList = GenEqualConditionSingle("")        ⇒ bỏ mệnh đề
        //   strVINConditionList        = GenEqualConditionSingle(strVIN)    ⇒ VIN = @VIN
        //   strStatusConditionList     = GenEqualConditionSingle(TConst.Stage.Pending)
        //   strIsActive                = TConst.Flag.Active
        //   strDateExpiredConditionList = GenGreaterOrEqualCondition(hôm nay) ⇒ DateExpired >= hôm nay
        // Gate :159-169 — VIN rỗng ⇒ ném Btl_Bulletin_Dealer_SearchDL_InvalidBulletinNoNullOrEmpty.
        if (string.IsNullOrEmpty(strVIN))
        {
            return PortResult.Fail("Btl_Bulletin_Dealer_SearchDL_InvalidBulletinNoNullOrEmpty",
                "Check.Description", "Số VIN bắt buộc nhập");
        }

        // Gate :140 — dtimeSys.ToString("yyyy-MM-dd").
        var today = DateTime.Now.ToString("yyyy-MM-dd");

        // Gate :155-156 — strFt_RecordStart = pageIndex * pageSize, strFt_RecordCount = pageSize.
        var recordStart = (long)pageIndex * pageSize;
        var recordCount = pageSize;

        // Biz :191-216 — Blt_Bulletin_GetByVinX với các bộ lọc trên (các danh sách "" ⇒ bỏ mệnh đề).
        var query = _db.Set<Btl_Bulletin>().AsNoTracking()
            .Where(x => EF.Functions.Like((x.VIN ?? "").ToLower(), PortQuery.Equal(strVIN), PortQuery.Esc))
            .Where(x => EF.Functions.Like((x.Status ?? "").ToLower(), PortQuery.Equal("PENDING"), PortQuery.Esc))
            .Where(x => EF.Functions.Like((x.FlagActive ?? "").ToLower(), PortQuery.Equal("1"), PortQuery.Esc))
            // GenGreaterOrEqualCondition(hôm nay) ⇒ DateExpired >= hôm nay (so chuỗi yyyy-MM-dd như nguồn).
            .Where(x => string.Compare(x.DateExpired, today) >= 0);

        // Gate :223 — itemCount lấy từ MySummaryTable.MyCount.
        var itemCount = await query.CountAsync();

        // Gate :229-233 — pageSize = 0 ⇒ pageSize = PageSizeConfig (giá trị KHÔNG có trong excerpt — xem GAP);
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

        // Gate :147-150 — PageInfo(pageIndex, pageSize) { DataList = new List<Btl_Bulletin>() }.
        // Gate :226-228 — pageInfo.DataList.AddRange(lst_Btl_Bulletin).
        var dataList = new List<Btl_Bulletin>();
        if (recordCount > 0 && recordStart >= 0)
        {
            dataList = await query
                .OrderBy(x => x.BulletinID)
                .Skip((int)Math.Min(recordStart, int.MaxValue))
                .Take((int)Math.Min(recordCount, int.MaxValue))
                .ToListAsync();
        }

        // Gate :236-238 — pageInfo.ItemCount = itemCount; pageInfo.PageCount = pageCount; msgResult._objResult = pageInfo.
        // Gate :243 — Result<CMyMsgResult>(msgResult) ⇒ Success = true, Data = PageInfo.
        return Ok(new
        {
            Success = true,
            Data = new PageInfo<Btl_Bulletin>(dataList, itemCount, pageCount, pageIndex, pageSize)
        });
    }

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

    /// <summary>
    /// Lấy chi tiết bản tin (Btl_BulletinDtl) còn hiệu lực theo số VIN — bản MapStatusP.
    /// Gate: BtlBulletinDealerController.cs:757-999 GetAllActiveByVINMapStatusP(VIN).
    /// Biz:  BizCarSv.Bulletin.cs:100-261 Blt_Bulletin_GetByVin_DL -> Blt_Bulletin_GetByVinX.
    /// </summary>
    [HttpPost("getallactivebyvinmapstatusp")]
    public async Task<IActionResult> GetAllActiveByVINMapStatusP(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] BtlBulletinDealer_GetAllActiveByVINMapStatusPReq? req = null)
    {
        // Gate :845 — strVIN = StdStr(VIN) (Trim, rỗng -> null).
        var strVIN = StdStr(req?.VIN);

        // Gate :847-850 — các danh sách điều kiện:
        //   strDealerCodeConditionList = "" (gate :847)                    ⇒ bỏ mệnh đề
        //   strVINConditionList        = GenEqualConditionSingle(strVIN)   ⇒ VIN = @VIN
        //   strStatusConditionList     = GenEqualConditionSingle(TConst.Stage.Pending) ⇒ Status = 'PENDING'
        //   strIsActive                = TConst.Flag.Active                ⇒ FlagActive = '1'
        // Gate :853-854 — pageIndex = 0, pageSize = 0 (PageInfo rỗng, không phân trang).
        // Gate :863-864 — strFt_RecordStart = Ft_RecordStart, strFt_RecordCount = Ft_RecordCount (BaseController).
        var recordStart = Ft_RecordStart;
        var recordCount = Ft_RecordCount;

        // Biz :191-216 — Blt_Bulletin_GetByVinX với các bộ lọc trên (các danh sách "" ⇒ bỏ mệnh đề).
        var query = _db.Set<Btl_Bulletin>().AsNoTracking()
            .Where(x => EF.Functions.Like((x.FlagActive ?? "").ToLower(), PortQuery.Equal("1"), PortQuery.Esc))
            .Where(x => EF.Functions.Like((x.Status ?? "").ToLower(), PortQuery.Equal("PENDING"), PortQuery.Esc));
        if (strVIN != null)
            query = query.Where(x => EF.Functions.Like((x.VIN ?? "").ToLower(), PortQuery.Equal(strVIN), PortQuery.Esc));

        // Gate :863-864 — Ft_RecordStart / Ft_RecordCount của BaseController (giá trị KHÔNG có trong excerpt — xem GAP).
        if (recordStart > 0)
            query = query.Skip((int)Math.Min(recordStart, int.MaxValue));
        if (recordCount > 0)
            query = query.Take((int)Math.Min(recordCount, int.MaxValue));

        var lst_Btl_Bulletin = await query.ToListAsync();

        // Gate :917-937 — Bỏ bản tin hết hạn: DateExpired rỗng hoặc "0001-01-01" ⇒ bỏ;
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

        // Gate :941-959 — Bỏ chi tiết của bản tin hết hạn: chỉ giữ Btl_BulletinDtl có BulletinID thuộc dtBulletinActive
        // (so bằng StdParam + StringEqual ⇒ phân biệt hoa/thường, xem LESSONS A.17).
        var activeBulletinIds = new HashSet<string>(
            dtBulletinActive.Select(x => StdParam(x.BulletinID) ?? ""), StringComparer.Ordinal);
        var lst_Btl_BulletinDtl = await _db.Set<Btl_BulletinDtl>().AsNoTracking()
            .Where(x => activeBulletinIds.Contains(x.BulletinID))
            .ToListAsync();

        // Gate :963-965 — pageInfo.DataList.AddRange(lst_Btl_BulletinDtl); msgResult._objResult = pageInfo.
        // Gate :970 — Result<CMyMsgResult>(msgResult) ⇒ Success = true, Data = PageInfo.
        return Ok(new
        {
            Success = true,
            Data = new PageInfo<Btl_BulletinDtl>(lst_Btl_BulletinDtl, 0, 0, 0, 0)
        });
    }

    /// <summary>TUtils.CUtils.StdParam: Trim + ToUpper, rỗng -> null (PORT_RULES mục 3).</summary>
    private static string? StdParam(string? value)
    {
        var s = value?.Trim().ToUpperInvariant();
        return string.IsNullOrEmpty(s) ? null : s;
    }

    /// <summary>
    /// Lấy danh sách bản tin (bulletin) còn hiệu lực theo VIN cho lệnh sửa chữa (RO).
    /// Gate: BtlBulletinDealerController.cs:1001-1467 GetForRO(VIN, ROID, FlagDataWH).
    /// Biz:  BizCarSv.Bulletin.cs:100-261 Blt_Bulletin_GetByVin_DL -> Blt_Bulletin_GetByVinX;
    ///       BizCarSv.Service01.cs:7451-7585 Ser_RO_Get_DL / :7723-7857 Ser_RO_Get_WH_DL (nhánh ROID);
    ///       BizCarSv.WarrantyReport.cs:303-455 Ser_ROWarrantyReport_GetWHDL (lọc bản tin đã có báo cáo bảo hành).
    /// </summary>
    [HttpPost("getforro")]
    public async Task<IActionResult> GetForRO(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] BtlBulletinDealer_GetForROReq? req = null)
    {
        // Gate :1091-1102 — VIN rỗng ⇒ BtlBulletinDealerGetForRO_VINEmpty (kiểm TRƯỚC khi StdStr).
        if (string.IsNullOrEmpty(req?.VIN))
        {
            return PortResult.Fail("BtlBulletinDealerGetForRO_VINEmpty", "Check.Input.VIN", req?.VIN);
        }

        // Gate :1104-1113 — chuẩn hoá tham số.
        var strVIN = StdStr(req!.VIN);
        var strROID = StdStr(req.ROID);
        // Gate :1106 — strDealerCode = StdStr(orgUserUI.DealerCode); ở đích không có phiên đăng nhập ⇒ nhận qua body
        // (LESSONS A.14), không truyền thì null ⇒ bộ lọc DealerCode rỗng như nguồn khi DealerCode trống.
        var strDealerCode = StdStr(req.DealerCode);
        // Gate :1113 — bFlagDataWH = Flag2Bool(FlagDataWH).
        var bFlagDataWH = Flag2Bool(req.FlagDataWH);

        // Gate :1112 — strROCreate = "" (chỉ gán khi có ROID).
        string? strROCreate = null;

        // Gate :1132-1236 — nhánh ROID: kiểm RO tồn tại + CreatedDate hợp lệ.
        if (!string.IsNullOrEmpty(req.ROID))
        {
            // Gate :1143-1196 — bFlagDataWH ? Ser_RO_Get_WH_DL : Ser_RO_Get_DL (lọc ROID = @ROID, DealerCode = @DealerCode).
            // GAP: thân Ser_RO_GetX / Ser_RO_GetWHX không có trong source_excerpt.txt ⇒ chỉ port bộ lọc ROID/DealerCode
            //   trên bảng Ser_RO (cột CreatedDate là cột gate dùng), không port cột khác.
            var roQuery = _db.Set<Ser_RO>().AsNoTracking()
                .Where(x => EF.Functions.Like((x.ROID ?? "").ToLower(), PortQuery.Equal(req.ROID), PortQuery.Esc));
            if (strDealerCode != null)
                roQuery = roQuery.Where(x => EF.Functions.Like((x.DealerCode ?? "").ToLower(), PortQuery.Equal(strDealerCode), PortQuery.Esc));
            var dt_Ser_RO = await roQuery.OrderBy(x => x.ROID).FirstOrDefaultAsync();

            // Gate :1208-1219 — không có dòng Ser_RO ⇒ BtlBulletinDealerGetForRO_ROIDNotFound.
            if (dt_Ser_RO == null)
            {
                return PortResult.Fail("BtlBulletinDealerGetForRO_ROIDNotFound", "Check.Input.ROID", req.ROID);
            }

            // Gate :1221-1234 — CreatedDate không phải ngày ⇒ BtlBulletinDealerGetForRO_InvalidROCreate.
            strROCreate = StdStr(dt_Ser_RO.CreatedDate);
            if (!IsDateTime(strROCreate))
            {
                return PortResult.Fail("BtlBulletinDealerGetForRO_InvalidROCreate",
                    "Check.Input.ROID", req.ROID, "Check.RO.CreatedDate", strROCreate);
            }
        }

        // Gate :1242-1265 — Blt_Bulletin_GetByVin_DL: VIN = @VIN, FlagActive = '1', Status = 'PENDING',
        //   DealerCode rỗng (gate :1108) ⇒ bỏ mệnh đề; Ft_RecordStart/Ft_RecordCount của BaseController (GAP, dùng 0).
        // Gate :1104 — StdStr(VIN) rỗng ⇒ strVIN = null ⇒ GenEqualConditionSingle(null) = "" ⇒ bỏ mệnh đề VIN.
        var bulletinQuery = _db.Set<Btl_Bulletin>().AsNoTracking()
            .Where(x => EF.Functions.Like((x.FlagActive ?? "").ToLower(), PortQuery.Equal("1"), PortQuery.Esc))
            .Where(x => EF.Functions.Like((x.Status ?? "").ToLower(), PortQuery.Equal("PENDING"), PortQuery.Esc));
        if (strVIN != null)
            bulletinQuery = bulletinQuery.Where(x => EF.Functions.Like((x.VIN ?? "").ToLower(), PortQuery.Equal(strVIN), PortQuery.Esc));
        if (strDealerCode != null)
            bulletinQuery = bulletinQuery.Where(x => EF.Functions.Like((x.DealerCode ?? "").ToLower(), PortQuery.Equal(strDealerCode), PortQuery.Esc));

        var dtBulletin = await bulletinQuery.OrderBy(x => x.BulletinID).ToListAsync();

        // Gate :1283-1315 — duyệt NGƯỢC dtBulletinVIN: DateExpired rỗng hoặc "0001-01-01" ⇒ giữ nguyên;
        //   còn lại: so (DateExpired - ngày tạo RO nếu có, ngược lại hôm nay).TotalDays >= 0 ⇒ giữ, < 0 ⇒ bỏ.
        var checkDateExpired = false;
        for (var i = dtBulletin.Count - 1; i >= 0; i--)
        {
            var strDateExpired = StdStr(dtBulletin[i].DateExpired);
            if (!string.IsNullOrEmpty(strDateExpired) && strDateExpired != DateTime.MinValue.ToString("yyyy-MM-dd"))
            {
                var dateExpiredCur = GetDateTimeValue(strDateExpired);
                var dateExpiredCal = new DateTime(dateExpiredCur.Year, dateExpiredCur.Month, dateExpiredCur.Day);

                var datenow = DateTime.Now;
                var datenowCal = new DateTime(datenow.Year, datenow.Month, datenow.Day);
                // Gate :1296-1301 — BG đã tạo ⇒ so với ngày tạo BG; chưa tạo ⇒ so với hôm nay.
                if (!string.IsNullOrEmpty(strROCreate))
                {
                    var dateROCreate = GetDateTimeValue(strROCreate);
                    datenowCal = new DateTime(dateROCreate.Year, dateROCreate.Month, dateROCreate.Day);
                }

                var day = (dateExpiredCal - datenowCal).TotalDays;
                if (day >= 0)
                {
                    checkDateExpired = true;
                }
                else
                {
                    dtBulletin.RemoveAt(i);
                }
            }
        }

        // Gate :1319-1322 — check = còn ít nhất 1 dòng Btl_Bulletin_VIN sau khi lọc hạn.
        var check = dtBulletin.Count > 0;

        // Gate :1324-1409 — chỉ khi còn dòng mới gọi Ser_ROWarrantyReport_GetWHDL và lọc bản tin đã có báo cáo bảo hành.
        if (dtBulletin.Count > 0)
        {
            // Gate :1331-1338 — DealerCode = @DealerCode, Status in SENT,CONF,ACCE, FrameNo like %VIN%.
            // GAP: thân Ser_ROWarrantyReport_GetWHX không có trong source_excerpt.txt ⇒ chỉ port bộ lọc DealerCode/FrameNo
            //   trên bảng Ser_ROWarrantyReportServiceItems (cột BulletinID là cột gate dùng), không port cột khác.
            var warrantyQuery = _db.Set<Ser_ROWarrantyReportServiceItems>().AsNoTracking();
            if (strDealerCode != null)
                warrantyQuery = warrantyQuery.Where(x => EF.Functions.Like((x.DealerCode ?? "").ToLower(), PortQuery.Equal(strDealerCode), PortQuery.Esc));
            if (strVIN != null)
                warrantyQuery = warrantyQuery.Where(x => EF.Functions.Like((x.FrameNo ?? "").ToLower(), PortQuery.Contains(strVIN), PortQuery.Esc));
            var warrantyBulletinIds = new HashSet<string>(
                (await warrantyQuery.Select(x => x.BulletinID).ToListAsync()).Where(x => x != null).Select(x => x!), StringComparer.Ordinal);

            // Gate :1379-1389 — bản tin đã có dòng báo cáo bảo hành ⇒ bỏ.
            if (warrantyBulletinIds.Count > 0)
            {
                for (var i = dtBulletin.Count - 1; i >= 0; i--)
                {
                    if (warrantyBulletinIds.Contains(dtBulletin[i].BulletinID)) dtBulletin.RemoveAt(i);
                }
            }

            // Gate :1391-1406 — !(check && checkDateExpired) ⇒ xoá sạch; ngược lại chỉ giữ bản tin còn trong dtBulletinVIN.
            if (!(check && checkDateExpired))
            {
                dtBulletin.Clear();
            }
            else
            {
                var remainingIds = new HashSet<string>(dtBulletin.Select(x => x.BulletinID), StringComparer.Ordinal);
                for (var i = dtBulletin.Count - 1; i >= 0; i--)
                {
                    if (!remainingIds.Contains(dtBulletin[i].BulletinID)) dtBulletin.RemoveAt(i);
                }
            }
        }

        // Gate :1422-1433 — bGet_Btl_Bulletin = ("*" != null && Length > 0) = true ⇒ Data = { Lst_Btl_Bulletin }.
        // Gate :1438 — Result<CMyMsgResult>(msgResult) ⇒ Success = true.
        return Ok(new
        {
            Success = true,
            Data = new { Lst_Btl_Bulletin = dtBulletin }
        });
    }

    /// <summary>TUtils.CUtils.Flag2Bool: chỉ "1" là true, mọi giá trị khác (kể cả null) là false.</summary>
    private static bool Flag2Bool(string? value) => value == "1";

    /// <summary>TUtils.CUtils.IsDateTime: chuỗi rỗng/null ⇒ false; còn lại thử parse ngày.</summary>
    private static bool IsDateTime(string? value)
        => !string.IsNullOrEmpty(value) && DateTime.TryParse(value, out _);

    /// <summary>TUtils.CUtils.GetDateTimeValue: parse ngày, không parse được ⇒ DateTime.MinValue.</summary>
    private static DateTime GetDateTimeValue(string? value)
        => DateTime.TryParse(value, out var d) ? d : DateTime.MinValue;

    // Gate :863-864 — Ft_RecordStart / Ft_RecordCount là thuộc tính của BaseController nguồn; giá trị KHÔNG có trong
    // source_excerpt.txt ⇒ GAP, không tự đặt giá trị (dùng 0 = không cắt dòng).
    private const int Ft_RecordStart = 0;
    private const int Ft_RecordCount = 0;
}

/// <summary>
/// Body của SearchDL — 4 tham số của gate (BtlBulletinDealerController.cs:35-40).
/// Ft_PageIndex = 0, Ft_PageSize = 10 là giá trị mặc định của chữ ký nguồn.
/// </summary>
public class BtlBulletinDealer_SearchDLReq
{
    public string? DealerCode { get; set; }
    public string? VIN { get; set; }
    public int? Ft_PageIndex { get; set; }
    public int? Ft_PageSize { get; set; }
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

/// <summary>
/// Body của GetForRO — 3 tham số của gate (BtlBulletinDealerController.cs:1001-1005) + DealerCode lấy từ phiên đăng nhập
/// (gate :1106 `orgUserUI.DealerCode`; đích không có phiên ⇒ nhận tuỳ chọn qua body, LESSONS A.14).
/// </summary>
public class BtlBulletinDealer_GetForROReq
{
    public string? VIN { get; set; }
    public string? ROID { get; set; }
    public string? FlagDataWH { get; set; }
    public string? DealerCode { get; set; }
}

/// <summary>
/// Body của GetAllActiveByVINMapStatusP — 1 tham số của gate (BtlBulletinDealerController.cs:757-759).
/// </summary>
public class BtlBulletinDealer_GetAllActiveByVINMapStatusPReq
{
    public string? VIN { get; set; }
}
