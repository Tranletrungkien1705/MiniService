using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 JDPMstJDPowerTermController.cs — JDP_Mst_JDPowerTerm
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\JDPMstJDPowerTermController.cs:42-258 (SearchHQ)
//   Biz:  D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Master.cs:84169-84316 (JDP_Mst_JDPowerTerm_Get)
//         -> 84318-84453 (JDP_Mst_JDPowerTerm_GetX)
//   Gate: JDPMstJDPowerTermController.cs:260-476 (SearchWHHQ)
//   Biz:  BizCarSv.Master.cs:84458-84604 (JDP_Mst_JDPowerTerm_Get_WH)
//         -> 84606-84739 (JDP_Mst_JDPowerTerm_Get_WHX)
// Bỏ qua có chủ đích (GAP, không mô phỏng): CheckGatewayAuthentication (BaseController.cs:291-299),
// GetUserFullInfo (BaseController.cs:441-575 — Inos_GetUserInfoByToken / Inos_GetMyOrgUserList / Sys_User_GetForCurrentUser),
// Sys_Access_CheckDenyCarSv (BizCarSv.Master.cs:84237-84241) — đều là xác thực/phân quyền của nguồn, đích không có.
[ApiController]
[Route("api/jdpmstjdpowerterm")]
public class JDPMstJDPowerTermControllerPort : ControllerBase
{
    private readonly AppDbContext _db;

    public JDPMstJDPowerTermControllerPort(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Tìm kiếm kỳ J.D. Power (SearchHQ).
    /// Gate: JDPMstJDPowerTermController.cs:42-258 SearchHQ(JDPTermCode, FlagActive, Ft_PageIndex = 0, Ft_PageSize = 10)
    /// Biz: BizCarSv.Master.cs:84169-84316 (JDP_Mst_JDPowerTerm_Get) -> 84318-84453 (JDP_Mst_JDPowerTerm_GetX).
    /// </summary>
    [HttpPost("searchhq")]
    public async Task<IActionResult> SearchHQ(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JDPMstJDPowerTerm_SearchHQReq? req = null)
    {
        // Gate :45-46 — Ft_PageIndex = 0, Ft_PageSize = 10 (giá trị mặc định của chữ ký nguồn).
        var pageIndex = req?.Ft_PageIndex ?? 0;
        var pageSize = req?.Ft_PageSize ?? 10;

        // Gate :136-137 — StdStr: Trim, rỗng -> null.
        var jdpTermCode = StdStr(req?.JDPTermCode);
        var flagActive = StdStr(req?.FlagActive);

        // Gate :140 — GenLikeCondition2Percent(strJDPTermCode): rỗng -> "" (không lọc), ngược lại like '%x%'.
        var jdpTermCodeList = string.IsNullOrEmpty(jdpTermCode) ? "" : "%" + jdpTermCode + "%";
        // Gate :146 — GenEqualCondition2(FlagActive): rỗng -> "" (không lọc), ngược lại = 'x'.
        var flagActiveList = string.IsNullOrEmpty(flagActive) ? "" : flagActive;

        // Gate :159-160 — strFt_RecordStart = pageIndex * pageSize; strFt_RecordCount = pageSize.
        // Biz :84353-84354 — nFilterRecordStart = RecordStart + 1; nFilterRecordEnd = RecordStart + RecordCount.
        var recordStart = (long)pageIndex * pageSize;
        var recordEnd = recordStart + pageSize;

        var query = _db.Set<JDP_Mst_JDPowerTerm>().AsNoTracking().AsQueryable();

        // Biz :84421 / :84381 — t.JDPTermCode like '%x%'
        if (!string.IsNullOrEmpty(jdpTermCodeList))
            query = query.Where(x => EF.Functions.Like((x.JDPTermCode ?? "").ToLower(), PortQuery.Contains(jdpTermCode!), PortQuery.Esc));

        // Biz :84427 / :84378 — t.FlagActive = 'x'
        if (!string.IsNullOrEmpty(flagActiveList))
            query = query.Where(x => EF.Functions.Like((x.FlagActive ?? "").ToLower(), PortQuery.Equal(flagActive!), PortQuery.Esc));

        // Biz :84386 — select Count(0) MyCount from #tbl_JDP_Mst_JDPowerTerm_Filter
        var itemCount = await query.CountAsync();

        // Biz :84370-84371 — Row_Number() over(order by t.JDPTermCode asc) MyRowIdx
        // Biz :84394-84395 — MyRowIdx >= nFilterRecordStart and MyRowIdx <= nFilterRecordEnd (1-based)
        // Biz :84405 — order by t.MyRowIdx asc
        var list = (recordStart < 0 || pageSize <= 0)
            ? new List<JDP_Mst_JDPowerTerm>()
            : await query
                .OrderBy(x => x.JDPTermCode)
                .Skip((int)Math.Min(recordStart, int.MaxValue))
                .Take((int)Math.Min(pageSize, int.MaxValue))
                .ToListAsync();

        // Gate :216-220 — pageSize == 0 -> PageSizeConfig; pageCount = ceil(itemCount / pageSize).
        var pageCount = (itemCount <= 0 || pageSize <= 0) ? 0 : (int)Math.Ceiling((double)itemCount / pageSize);

        // Gate :153-156, :215-224 — PageInfo(pageIndex, pageSize) { DataList, ItemCount, PageCount }.
        return Ok(new
        {
            Success = true,
            Data = new PageInfo<JDP_Mst_JDPowerTerm>(list, itemCount, pageCount, pageIndex, pageSize)
        });
    }

    /// <summary>
    /// Tìm kiếm kỳ J.D. Power cho kho (SearchWHHQ).
    /// Gate: JDPMstJDPowerTermController.cs:260-476 SearchWHHQ(JDPTermCode, FlagActive, Ft_PageIndex = 0, Ft_PageSize = 10)
    /// Biz: BizCarSv.Master.cs:84458-84604 (JDP_Mst_JDPowerTerm_Get_WH) -> 84606-84739 (JDP_Mst_JDPowerTerm_Get_WHX).
    /// </summary>
    [HttpPost("searchwhhq")]
    public async Task<IActionResult> SearchWHHQ(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JDPMstJDPowerTerm_SearchWHHQReq? req = null)
    {
        // Gate :263-264 — Ft_PageIndex = 0, Ft_PageSize = 10 (giá trị mặc định của chữ ký nguồn).
        var pageIndex = req?.Ft_PageIndex ?? 0;
        var pageSize = req?.Ft_PageSize ?? 10;

        // Gate :354-355 — StdStr: Trim, rỗng -> null.
        var jdpTermCode = StdStr(req?.JDPTermCode);
        var flagActive = StdStr(req?.FlagActive);

        // Gate :358 — GenLikeCondition2Percent(strJDPTermCode): rỗng -> "" (không lọc), ngược lại like '%x%'.
        var jdpTermCodeList = string.IsNullOrEmpty(jdpTermCode) ? "" : "%" + jdpTermCode + "%";
        // Gate :364 — GenEqualCondition2(FlagActive): rỗng -> "" (không lọc), ngược lại = 'x'.
        var flagActiveList = string.IsNullOrEmpty(flagActive) ? "" : flagActive;

        // Gate :377-378 — strFt_RecordStart = pageIndex * pageSize; strFt_RecordCount = pageSize.
        // Biz :84641-84642 — nFilterRecordStart = RecordStart + 1; nFilterRecordEnd = RecordStart + RecordCount.
        var recordStart = (long)pageIndex * pageSize;

        var query = _db.Set<JDP_Mst_JDPowerTerm>().AsNoTracking().AsQueryable();

        // Biz :84708 / :84669 — t.JDPTermCode like '%x%'
        if (!string.IsNullOrEmpty(jdpTermCodeList))
            query = query.Where(x => EF.Functions.Like((x.JDPTermCode ?? "").ToLower(), PortQuery.Contains(jdpTermCode!), PortQuery.Esc));

        // Biz :84714 / :84666 — t.FlagActive = 'x'
        if (!string.IsNullOrEmpty(flagActiveList))
            query = query.Where(x => EF.Functions.Like((x.FlagActive ?? "").ToLower(), PortQuery.Equal(flagActive!), PortQuery.Esc));

        // Biz :84674 — select Count(0) MyCount from #tbl_JDP_Mst_JDPowerTerm_Filter
        var itemCount = await query.CountAsync();

        // Biz :84659 — Row_Number() over(order by t.JDPTermCode asc) MyRowIdx
        // Biz :84682-84683 — MyRowIdx >= nFilterRecordStart and MyRowIdx <= nFilterRecordEnd (1-based)
        // Biz :84692 — order by t.MyRowIdx asc
        var list = (recordStart < 0 || pageSize <= 0)
            ? new List<JDP_Mst_JDPowerTerm>()
            : await query
                .OrderBy(x => x.JDPTermCode)
                .Skip((int)Math.Min(recordStart, int.MaxValue))
                .Take((int)Math.Min(pageSize, int.MaxValue))
                .ToListAsync();

        // Gate :434-441 — pageSize == 0 -> PageSizeConfig; pageCount = ceil(itemCount / pageSize).
        var pageCount = (itemCount <= 0 || pageSize <= 0) ? 0 : (int)Math.Ceiling((double)itemCount / pageSize);

        // Gate :371-374, :433-442 — PageInfo(pageIndex, pageSize) { DataList, ItemCount, PageCount }.
        return Ok(new
        {
            Success = true,
            Data = new PageInfo<JDP_Mst_JDPowerTerm>(list, itemCount, pageCount, pageIndex, pageSize)
        });
    }

    /// <summary>TUtils.CUtils.StdStr: Trim, rỗng -> null (PORT_RULES mục 3).</summary>
    private static string? StdStr(string? s)
    {
        if (s == null) return null;
        var t = s.Trim();
        return t.Length == 0 ? null : t;
    }
}

public class JDPMstJDPowerTerm_SearchHQReq
{
    public string? JDPTermCode { get; set; }
    public string? FlagActive { get; set; }
    public int Ft_PageIndex { get; set; } = 0;
    public int Ft_PageSize { get; set; } = 10;
}

/// <summary>Body của SearchWHHQ (gate JDPMstJDPowerTermController.cs:260-265).</summary>
public class JDPMstJDPowerTerm_SearchWHHQReq
{
    public string? JDPTermCode { get; set; }
    public string? FlagActive { get; set; }
    public int Ft_PageIndex { get; set; } = 0;
    public int Ft_PageSize { get; set; } = 10;
}
