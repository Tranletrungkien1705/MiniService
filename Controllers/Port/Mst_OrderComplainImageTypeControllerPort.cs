using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 Mst_OrderComplainImageTypeController.cs — Mst_OrderComplainImageType
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\Mst_OrderComplainImageTypeController.cs:253-436 (GetAllActive)
//   Biz:  D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Master.cs:60804-60942 (Mst_OrderComplainImageType_Get)
//         -> 60944-61087 (Mst_OrderComplainImageType_GetX)
[ApiController]
[Route("api/mst_ordercomplainimagetype")]
public class Mst_OrderComplainImageTypeControllerPort : ControllerBase
{
    private readonly AppDbContext _db;

    public Mst_OrderComplainImageTypeControllerPort(AppDbContext db) => _db = db;

    /// <summary>
    /// Lấy danh sách loại ảnh khiếu nại đang hoạt động (FlagActive = '1').
    /// Gate: Mst_OrderComplainImageTypeController.cs:253-436 GetAllActive() — không tham số, không phân trang
    /// (pageIndex = 0, pageSize = 0, Ft_RecordStart/Ft_RecordCount lấy từ BaseController).
    /// Biz:  BizCarSv.Master.cs:60804-60942 -> 60944-61087.
    /// </summary>
    [HttpPost("getallactive")]
    public async Task<IActionResult> GetAllActive([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] object? req = null)
    {
        // Gate :334-339 — ba bộ lọc của nguồn:
        //   strOrderComplainImageTypeConditionList = ""            (không lọc mã)
        //   strOrderComplainImageNameConditionList = ""            (không lọc tên)
        //   strFlagActiveConditionList = GenEqualCondition2("1")   (FlagActive = '1')
        //   strIs_Get_Mst_OrderComplainImageType = GenEqualCondition2("1") (bật khối chi tiết)
        // Biz :61038-61040 — BuildClause("and", "<cột>", <danh sách>, "@p"): danh sách rỗng ⇒ bỏ mệnh đề.
        var query = _db.Set<Mst_OrderComplainImageType>().AsNoTracking()
            .Where(x => EF.Functions.Like((x.FlagActive ?? "").ToLower(), PortQuery.Equal("1"), PortQuery.Esc));

        // Biz :61002-61013 — `select distinct identity(bigint,0,1) MyIdxSeq, mocit.OrderComplainImageType … order by mocit.OrderComplainImageType`
        // (khoá một cột ⇒ distinct không làm mất dòng; thứ tự theo mã).
        var list = await query
            .OrderBy(x => x.OrderComplainImageType)
            .Select(x => new
            {
                x.OrderComplainImageType,
                x.OrderComplainImageName,
                x.FlagActive,
                x.LogLUBy,
                x.LogLUDTime
            })
            .ToListAsync();

        // Biz :61003 — identity(bigint, 0, 1) MyIdxSeq: số thứ tự 0,1,2… theo thứ tự đã sắp.
        var data = list.Select((x, i) => new
        {
            MyIdxSeq = (long)i,
            x.OrderComplainImageType,
            x.OrderComplainImageName,
            x.FlagActive,
            x.LogLUBy,
            x.LogLUDTime
        }).ToList();

        // Gate :342-347, :400-402 — PageInfo(pageIndex = 0, pageSize = 0) { DataList = <danh sách> }.
        // Gate :407 — Result<CMyMsgResult>(msgResult) ⇒ Success = true, Data = PageInfo.
        return Ok(new
        {
            Success = true,
            Data = new PageInfo<object>(data, data.Count, 0, 0, 0)
        });
    }
}
