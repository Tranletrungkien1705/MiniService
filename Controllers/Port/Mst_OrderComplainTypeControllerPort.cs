using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 Mst_OrderComplainTypeController.Search — Mst_OrderComplainType
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\Mst_OrderComplainTypeController.cs:41-253
//   Biz:  D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Master.cs:61157-61295 (Mst_OrderComplainType_Get)
//         -> 61297-61439 (Mst_OrderComplainType_GetX)
[ApiController]
[Route("api/mst_ordercomplaintype")]
public class Mst_OrderComplainTypeControllerPort : ControllerBase
{
    private readonly AppDbContext _db;

    public Mst_OrderComplainTypeControllerPort(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Tìm kiếm loại khiếu nại (Search).
    /// Gate: Mst_OrderComplainTypeController.cs:41-253 Search(OrderComplainType, OrderComplainTypeName, FlagActive, Ft_PageIndex, Ft_PageSize)
    /// Biz: BizCarSv.Master.cs:61157-61295 (Mst_OrderComplainType_Get) -> 61297-61439 (Mst_OrderComplainType_GetX).
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> Search(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] Mst_OrderComplainType_SearchReq? req = null)
    {
        var pageIndex = req?.Ft_PageIndex ?? 0;
        var pageSize = req?.Ft_PageSize ?? 10;
        if (pageSize <= 0) pageSize = 10;

        var query = _db.Set<Mst_OrderComplainType>().AsNoTracking().AsQueryable();

        // Biz :61038 / 61391 — OrderComplainType condition
        if (!string.IsNullOrEmpty(req?.OrderComplainType))
            query = query.Where(x => EF.Functions.Like((x.OrderComplainType ?? "").ToLower(), PortQuery.Equal(req.OrderComplainType), PortQuery.Esc));

        // Biz :61039 / 61392 — OrderComplainTypeName condition
        if (!string.IsNullOrEmpty(req?.OrderComplainTypeName))
            query = query.Where(x => EF.Functions.Like((x.OrderComplainTypeName ?? "").ToLower(), PortQuery.Equal(req.OrderComplainTypeName), PortQuery.Esc));

        // Biz :61040 / 61393 — FlagActive condition
        if (!string.IsNullOrEmpty(req?.FlagActive))
            query = query.Where(x => EF.Functions.Like((x.FlagActive ?? "").ToLower(), PortQuery.Equal(req.FlagActive), PortQuery.Esc));

        var itemCount = await query.CountAsync();

        // Biz :61373-61380 — Phân trang theo MyIdxSeq (0-based, Order by OrderComplainType asc)
        var list = await query
            .OrderBy(x => x.OrderComplainType)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.OrderComplainType,
                x.OrderComplainTypeName,
                x.FlagActive,
                x.LogLUBy,
                x.LogLUDTime
            })
            .ToListAsync();

        var recordStart = pageIndex * pageSize;
        var data = list.Select((x, i) => new
        {
            MyIdxSeq = (long)(recordStart + i),
            x.OrderComplainType,
            x.OrderComplainTypeName,
            x.FlagActive,
            x.LogLUBy,
            x.LogLUDTime
        }).ToList();

        var pageCount = (itemCount <= 0 || pageSize <= 0) ? 0 : (int)Math.Ceiling((double)itemCount / pageSize);

        return Ok(new
        {
            Success = true,
            Data = new PageInfo<object>(data, itemCount, pageCount, pageIndex, pageSize)
        });
    }
}

public class Mst_OrderComplainType_SearchReq
{
    public string? OrderComplainType { get; set; }
    public string? OrderComplainTypeName { get; set; }
    public string? FlagActive { get; set; }
    public int Ft_PageIndex { get; set; } = 0;
    public int Ft_PageSize { get; set; } = 10;
}
