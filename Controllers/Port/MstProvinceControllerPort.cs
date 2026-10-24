using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 MstProvinceController.cs — Mst_Province/Search
// Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\MstProvinceController.cs:27-96 (Search)
// Biz: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Master.cs:5999 (Mst_Province_Get) -> 6144 (Mst_Province_GetX)
// GAP: CheckGatewayAuthentication, CheckAuthorize, API_Get, Sys_Access_CheckDenyCarSv — xác thực/phân quyền của nguồn, đích không có.
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

        var data = list.Count > 0 ? list : null;
        return Ok(new
        {
            Success = true,
            Data = data
        });
    }

    /// <summary>
    /// Lấy chi tiết tỉnh/thành phố theo mã (GetByProvinceCode).
    /// Gate: MstProvinceController.cs:149-202 GetByProvinceCode(string ProvinceCode)
    /// Biz: BizCarSv.Master.cs:5999 (Mst_Province_Get) -> 6144 (Mst_Province_GetX).
    /// Lọc: ProvinceCode = req.ProvinceCode (Biz:6216), Ft_RecordStart=0, Ft_RecordCount=1.
    /// Trả về: dòng đầu hoặc null nếu không tìm được.
    /// </summary>
    [HttpPost("getbyprovincecheck")]
    public async Task<IActionResult> GetByProvinceCode(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] Mst_Province_GetByProvinceCodeReq? req = null)
    {
        var provinceCode = req?.ProvinceCode;
        if (string.IsNullOrEmpty(provinceCode))
            return Ok(new { Success = true, Data = (object?)null });

        var query = _db.Set<Mst_Province>()
            .AsNoTracking()
            .AsQueryable();

        // Biz:6216 — ProvinceCode condition: GenEqualCondition2 (dùng =)
        query = query.Where(x => EF.Functions.Like((x.ProvinceCode ?? "").ToLower(), PortQuery.Equal(provinceCode), PortQuery.Esc));

        var item = await query.FirstOrDefaultAsync();

        return Ok(new
        {
            Success = true,
            Data = item
        });
    }

    /// <summary>
    /// Tìm kiếm tỉnh/thành phố (Search).
    /// Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\MstProvinceController.cs:27-96
    /// Biz: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Master.cs:5999 (Mst_Province_Get)
    ///      -> 6144 (Mst_Province_GetX).
    /// Tham số: KeyWord (lọc ProvinceName), FlagActive (lọc bằng IN), Ft_PageIndex, Ft_PageSize.
    /// Bộ lọc ProvinceName: dùng LIKE %KeyWord% (Gate:691; Biz:6217).
    /// Bộ lọc FlagActive: tách chuỗi "0,1" bằng dấu phẩy, dùng IN (Gate:695-700; Biz:6218).
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> Search(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] Mst_Province_SearchReq? req = null)
    {
        var pageIndex = req?.Ft_PageIndex ?? 0;
        var pageSize = req?.Ft_PageSize ?? 10;
        if (pageSize <= 0) pageSize = 10; // GAP: BaseController.PageSizeConfig không có trong excerpt, dùng hard-code 10.

        var query = _db.Set<Mst_Province>().AsNoTracking().AsQueryable();

        // Biz:6217 — ProvinceName condition: GenLikeCondition2Percent (dùng LIKE %KeyWord%)
        if (!string.IsNullOrEmpty(req?.KeyWord))
            query = query.Where(x => EF.Functions.Like(x.ProvinceName ?? "", PortQuery.Contains(req.KeyWord), PortQuery.Esc));

        // Biz:6218 — FlagActive condition: GenEqualCondition2 (dùng IN khi tách chuỗi "0,1")
        if (!string.IsNullOrEmpty(req?.FlagActive))
        {
            var flagList = req.FlagActive.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();
            if (flagList.Count > 0)
                query = query.Where(x => flagList.Contains(x.FlagActive ?? ""));
        }

        var itemCount = await query.CountAsync();

        // Biz:6202-6245 — select * from Mst_Province, phân trang (không order by cụ thể) ⇒ order by ProvinceCode asc
        var recordStart = (pageIndex * pageSize <= 0) ? 0 : (int)Math.Min(pageIndex * pageSize, int.MaxValue);
        var recordCount = (int)Math.Min(pageSize, int.MaxValue);

        var list = await query
            .OrderBy(x => x.ProvinceCode)
            .Skip(recordStart)
            .Take(recordCount)
            .ToListAsync();

        var pageCount = (itemCount <= 0 || pageSize <= 0) ? 0 : (int)Math.Ceiling((double)itemCount / pageSize);

        return Ok(new
        {
            Success = true,
            Data = new PageInfo<Mst_Province>(list, itemCount, pageCount, pageIndex, pageSize)
        });
    }

    /// <summary>
    /// Xoá tỉnh/thành phố (Delete).
    /// Gate: MstProvinceController.cs:340-390 Delete(string ProvinceCode).
    /// Kiểm tra: ProvinceCode không rỗng; nếu rỗng trả lỗi "invalid_provincecode".
    /// Xoá dòng theo ProvinceCode (khóa chính).
    /// </summary>
    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] Mst_Province_DeleteReq? req = null)
    {
        var provinceCode = req?.ProvinceCode;
        if (string.IsNullOrEmpty(provinceCode))
            return PortResult.Fail("Mst_Province_Delete", "Check.ProvinceCode", null);

        var item = await _db.Set<Mst_Province>().FirstOrDefaultAsync(x => x.ProvinceCode == provinceCode);
        if (item == null)
            return PortResult.Fail("Mst_Province_Delete", "NotFound", null);

        _db.Set<Mst_Province>().Remove(item);
        await _db.SaveChangesAsync();

        return Ok(new { Success = true, Data = (object?)null });
    }
}

public class Mst_Province_GetByProvinceCodeReq
{
    public string? ProvinceCode { get; set; }
}

public class Mst_Province_SearchReq
{
    // Gate:27 — tham số từ source
    public string? KeyWord { get; set; }
    public string? FlagActive { get; set; }
    public int Ft_PageIndex { get; set; } = 0;
    public int Ft_PageSize { get; set; } = 10;
}

public class Mst_Province_DeleteReq
{
    public string? ProvinceCode { get; set; }
}
