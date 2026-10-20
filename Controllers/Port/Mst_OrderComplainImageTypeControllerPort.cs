using Microsoft.AspNetCore.Hosting;
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
    private readonly IWebHostEnvironment? _env;

    public Mst_OrderComplainImageTypeControllerPort(AppDbContext db, IWebHostEnvironment? env = null)
    {
        _db = db;
        _env = env;
    }

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

    /// <summary>
    /// Xuất Excel danh sách loại ảnh khiếu nại theo bộ lọc.
    /// Gate: Mst_OrderComplainImageTypeController.cs:440-641 ExportExcel(OrderComplainTypeCode, OrderComplainTypeName, FlagActive)
    ///   -> _biz.Mst_OrderComplainImageType_Get (BizCarSv.Master.cs:60804-60942) -> Mst_OrderComplainImageType_GetX (:60944-61087).
    /// Gate :600-605 — GetImportDicColums() (3 cột) -> GenExcelExportFilePath -> ExcelExport.ExportToExcelFromList
    ///   -> msgResult._objResult = GetRequestBaseAddress() + url.
    /// </summary>
    [HttpPost("exportexcel")]
    public async Task<IActionResult> ExportExcel(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] Mst_OrderComplainImageType_ExportReq? req = null)
    {
        // Gate :529-538 — GenEqualCondition2(x): rỗng ⇒ "" (bỏ mệnh đề), có giá trị ⇒ "='x'" (so bằng, không wildcard).
        // Biz :61038-61040 — BuildClause("and", "<cột>", <danh sách>, "@p"): danh sách rỗng ⇒ bỏ mệnh đề.
        var query = _db.Set<Mst_OrderComplainImageType>().AsNoTracking().AsQueryable();

        // Biz :61038 — mocit.OrderComplainImageType
        if (!string.IsNullOrEmpty(req?.OrderComplainTypeCode))
            query = query.Where(x => EF.Functions.Like((x.OrderComplainImageType ?? "").ToLower(), PortQuery.Equal(req!.OrderComplainTypeCode!), PortQuery.Esc));

        // Biz :61039 — mocit.OrderComplainImageName
        if (!string.IsNullOrEmpty(req?.OrderComplainTypeName))
            query = query.Where(x => EF.Functions.Like((x.OrderComplainImageName ?? "").ToLower(), PortQuery.Equal(req!.OrderComplainTypeName!), PortQuery.Esc));

        // Biz :61040 — mocit.FlagActive
        if (!string.IsNullOrEmpty(req?.FlagActive))
            query = query.Where(x => EF.Functions.Like((x.FlagActive ?? "").ToLower(), PortQuery.Equal(req!.FlagActive!), PortQuery.Esc));

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
        var rows = list.Select((x, i) => new
        {
            MyIdxSeq = (long)i,
            x.OrderComplainImageType,
            x.OrderComplainImageName,
            x.FlagActive,
            x.LogLUBy,
            x.LogLUDTime
        }).ToList();

        // Gate :600, :645-653 — GetImportDicColums(): 3 cột, nhãn giữ NGUYÊN VĂN (kể cả lỗi chính tả "khiếu nạy" của nguồn).
        var dicColNames = new Dictionary<string, string>
        {
            { "OrderComplainImageType", "Mã loại ảnh khiếu nại" },
            { "OrderComplainImageName", "Tên loại ảnh khiếu nạy" },
            { "FlagActive", "Trạng thái" },
        };

        // Gate :602 — GenExcelExportFilePath(typeof(TModels.Mst_OrderComplainImageType).Name, ref url).
        var webRoot = _env?.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var (filePath, virtualPath) = PortExcel.GenExcelExportFilePath(webRoot, nameof(Mst_OrderComplainImageType));

        // Gate :603 — ExcelExport.ExportToExcelFromList(lst, dicColNames, filePath, "Mst_OrderComplainImageType").
        PortExcel.ExportToExcelFromList(rows, dicColNames, filePath, nameof(Mst_OrderComplainImageType));

        // Gate :605 — msgResult._objResult = GetRequestBaseAddress() + url; Gate :611 — Result<CMyMsgResult>(msgResult) ⇒ Success = true.
        return Ok(new
        {
            Success = true,
            Data = PortExcel.GetRequestBaseAddress(Request) + virtualPath
        });
    }
}

/// <summary>
/// Body của ExportExcel — 3 tham số của gate (Mst_OrderComplainImageTypeController.cs:440-444).
/// Gate không có giá trị mặc định; rỗng ⇒ GenEqualCondition2 trả "" ⇒ bỏ mệnh đề lọc.
/// </summary>
public class Mst_OrderComplainImageType_ExportReq
{
    public string? OrderComplainTypeCode { get; set; }
    public string? OrderComplainTypeName { get; set; }
    public string? FlagActive { get; set; }
}
