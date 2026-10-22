using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 DlrMstDealerContractFormPrintController.GetAllPrintType — Mst_TempPrintType
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\DlrMstDealerContractFormPrintController.cs:35-214 (GetAllPrintType)
//   Biz:  D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\Master.cs:314-538 (Mst_TempPrintType_Get)
[ApiController]
[Route("api/dlrmstdealercontractformprint")]
public class DlrMstDealerContractFormPrintControllerPort : ControllerBase
{
    private readonly AppDbContext _db;

    public DlrMstDealerContractFormPrintControllerPort(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lấy danh sách mẫu in đang kích hoạt (FlagActive = "1").
    /// Gate: DlrMstDealerContractFormPrintController.cs:35-214 GetAllPrintType()
    ///   - strWhereClauseMst_TempPrintType_By_TempPrintType(:131-134, :1058-1069): FlagActive = "1"
    ///   - PageInfo(pageIndex=0, pageSize=0), DataList rỗng rồi nạp danh sách từ Mst_TempPrintType
    /// Biz: Master.cs:314-538 (Mst_TempPrintType_Get)
    ///   - Sắp xếp: mtpt.TempPrintType asc
    ///   - MyIdxSeq tăng dần từ 0
    /// </summary>
    [HttpPost("getallprinttype")]
    public async Task<IActionResult> GetAllPrintType([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] DlrMstDealerContractFormPrint_GetAllPrintTypeReq? req = null)
    {
        const string errorCodeDefault = "MstTempPrintTypeGetAllActive";
        try
        {
            // Gate :102-105 & :1065: lọc FlagActive = "1"
            // Nếu client truyền FlagActive riêng (cho phép kiểm tra), dùng giá trị đó; mặc định theo gate là "1"
            var flagActive = req?.FlagActive ?? "1";

            var query = _db.Set<Mst_TempPrintType>().AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(flagActive))
            {
                query = query.Where(x => EF.Functions.Like((x.FlagActive ?? "").ToLower(), PortQuery.Equal(flagActive), PortQuery.Esc));
            }

            // Biz :568-569 mtpt.TempPrintType asc
            var list = await query
                .OrderBy(x => x.TempPrintType)
                .Select(x => new
                {
                    x.TempPrintType,
                    x.TempPrintTypeName,
                    x.FlagActive,
                    x.LogLUBy,
                    x.LogLUDTime
                })
                .ToListAsync();

            // Biz :562 MyIdxSeq
            var data = list.Select((x, i) => new
            {
                MyIdxSeq = (long)i,
                x.TempPrintType,
                x.TempPrintTypeName,
                x.FlagActive,
                x.LogLUBy,
                x.LogLUDTime
            }).ToList();

            // Gate :88-93, :119-122 pageIndex=0, pageSize=0
            var pageInfo = new PageInfo<object>(data, data.Count, 0, 0, 0);

            return Ok(new
            {
                Success = true,
                Data = pageInfo
            });
        }
        catch (Exception ex)
        {
            return PortResult.Fail(errorCodeDefault, "Exception", ex.Message);
        }
    }
}

public class DlrMstDealerContractFormPrint_GetAllPrintTypeReq
{
    public string? FlagActive { get; set; }
}
