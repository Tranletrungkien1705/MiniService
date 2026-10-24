using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 MstProvinceController.cs — Mst_Province (Search)
// Nguồn gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\MstProvinceController.cs:41-272
// Biz: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Master.cs:5999 (Mst_Province_Get)
//      -> 6144 (Mst_Province_GetX).
// GAP: CheckGatewayAuthentication, GetUserFullInfo, Sys_Access_CheckDenyCarSv — xác thực/phân quyền của nguồn, đích không có.
[ApiController]
[Route("api/mst_province")]
public class MstProvinceControllerPort : ControllerBase
{
    private readonly AppDbContext _db;

    public MstProvinceControllerPort(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lấy tất cả tỉnh/thành phố đang hoạt động (GetAllActive).
    /// Gate: MstProvinceController.cs:274-458 GetAllActive()
    /// Biz: BizCarSv.Master.cs:5999 (Mst_Province_Get) -> 6144 (Mst_Province_GetX).
    /// Lọc: FlagActive = "1" (Biz:356).
    /// </summary>
    [HttpGet("getallactive")]
    public async Task<IActionResult> GetAllActive()
    {
        var query = _db.Set<Mst_Province>()
            .AsNoTracking()
            .Where(x => x.FlagActive == "1");

        var list = await query
            .OrderBy(x => x.ProvinceCode)
            .ToListAsync();

        var pageInfo = new PageInfo<Mst_Province>(list, list.Count, 1, 0, 0);
        return Ok(new
        {
            Success = true,
            Data = pageInfo
        });
    }

    /// <summary>
    /// Tìm kiếm tỉnh/thành phố (Search).
    /// Gate: MstProvinceController.cs:41-272 Search(ProvinceCode, ProvinceName, FlagActive, CreatedDate, CreatedBy, AreaCode, Ft_PageIndex, Ft_PageSize)
    /// Biz: BizCarSv.Master.cs:5999 (Mst_Province_Get) -> 6144 (Mst_Province_GetX).
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> Search(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] Mst_Province_SearchReq? req = null)
    {
        var pageIndex = req?.Ft_PageIndex ?? 0;
        var pageSize = req?.Ft_PageSize ?? 10;
        if (pageSize <= 0) pageSize = 10;

        var query = _db.Set<Mst_Province>().AsNoTracking().AsQueryable();

        // Biz:6216 — ProvinceCode condition: GenEqualCondition2 (dùng =)
        if (!string.IsNullOrEmpty(req?.ProvinceCode))
            query = query.Where(x => EF.Functions.Like((x.ProvinceCode ?? "").ToLower(), PortQuery.Equal(req.ProvinceCode), PortQuery.Esc));

        // Biz:6217 — ProvinceName condition: GenLikeCondition2Percent (dùng LIKE)
        if (!string.IsNullOrEmpty(req?.ProvinceName))
            query = query.Where(x => EF.Functions.Like((x.ProvinceName ?? "").ToLower(), PortQuery.Contains(req.ProvinceName), PortQuery.Esc));

        // Biz:6218 — FlagActive condition: GenEqualCondition2 (dùng =)
        if (!string.IsNullOrEmpty(req?.FlagActive))
            query = query.Where(x => EF.Functions.Like((x.FlagActive ?? "").ToLower(), PortQuery.Equal(req.FlagActive), PortQuery.Esc));

        // Biz:6219 — CreatedDate condition: GenLikeCondition2Percent (dùng LIKE)
        if (!string.IsNullOrEmpty(req?.CreatedDate))
            query = query.Where(x => EF.Functions.Like((x.CreatedDate ?? "").ToLower(), PortQuery.Contains(req.CreatedDate), PortQuery.Esc));

        // Biz:6220 — CreatedBy condition: GenLikeCondition2Percent (dùng LIKE)
        if (!string.IsNullOrEmpty(req?.CreatedBy))
            query = query.Where(x => EF.Functions.Like((x.CreatedBy ?? "").ToLower(), PortQuery.Contains(req.CreatedBy), PortQuery.Esc));

        // Biz:6221 — AreaCode condition: GenEqualCondition2 (dùng =)
        if (!string.IsNullOrEmpty(req?.AreaCode))
            query = query.Where(x => EF.Functions.Like((x.AreaCode ?? "").ToLower(), PortQuery.Equal(req.AreaCode), PortQuery.Esc));

        var itemCount = await query.CountAsync();

        // Biz:6202-6245 — select * from Mst_Province, phân trang và không order by nào cụ thể ⇒ order by ProvinceCode asc (khớp seeder)
        var list = await query
            .OrderBy(x => x.ProvinceCode)
            .Skip((pageIndex * pageSize <= 0) ? 0 : (int)Math.Min(pageIndex * pageSize, int.MaxValue))
            .Take((int)Math.Min(pageSize, int.MaxValue))
            .Select(x => new
            {
                x.ProvinceCode,
                x.ProvinceName,
                x.FlagActive,
                x.CreatedDate,
                x.CreatedBy,
                x.AreaCode
            })
            .ToListAsync();

        var data = list.Select((x, i) => new
        {
            MyIdxSeq = (long)(pageIndex * pageSize + i),
            x.ProvinceCode,
            x.ProvinceName,
            x.FlagActive,
            x.CreatedDate,
            x.CreatedBy,
            x.AreaCode
        }).ToList();

        var pageCount = (itemCount <= 0 || pageSize <= 0) ? 0 : (int)Math.Ceiling((double)itemCount / pageSize);

        return Ok(new
        {
            Success = true,
            Data = new PageInfo<object>(data, itemCount, pageCount, pageIndex, pageSize)
        });
    }
}

public class Mst_Province_SearchReq
{
    public string? ProvinceCode { get; set; }
    public string? ProvinceName { get; set; }
    public string? FlagActive { get; set; }
    public string? CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? AreaCode { get; set; }
    public int Ft_PageIndex { get; set; } = 0;
    public int Ft_PageSize { get; set; } = 10;
}
