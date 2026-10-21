using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 DealerHistoryShareMngController.GetListClaimByPlateNoHQ — DealerHistoryShareMngController / iCIC_ListClaimByPlateNo_HQ
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\DealerHistoryShareMngController.cs:1197-1380
//   Biz:  D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\BizCarSv.Master.cs:83887-84090 (iCIC_ListClaimByPlateNo_HQ)
[ApiController]
[Route("api/dealerhistorysharemng")]
public class DealerHistoryShareMngControllerPort : ControllerBase
{
    private readonly AppDbContext _db;

    public DealerHistoryShareMngControllerPort(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lấy danh sách khiếu nại theo Biển số (HQ). Gate :1197-1380.
    /// Biz :83887 — iCIC_ListClaimByPlateNo_HQ: gọi dịch vụ ngoài (DMS Claim GetByPlateNo) - GAP (trả rỗng).
    /// </summary>
    [HttpPost("getlistclaimbyplatenohq")]
    public async Task<IActionResult> GetListClaimByPlateNoHQ(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] DealerHistoryShareMng_GetListClaimByPlateNoHQReq? req = null)
    {
        const string errorCode = "DealerHistoryShareMngGetListClaimByPlateNoHQ";
        try
        {
            var plateNo = string.IsNullOrWhiteSpace(req?.PlateNo) ? null : req.PlateNo.Trim();

            // GAP: lấy từ dịch vụ ngoài DMS Claim GetByPlateNo (Biz :559-595 postWebApi_DmsClaimGetByPlateNo) — KHÔNG port, trả rỗng.
            var lstClaimByPlateNo = new List<object>();

            // Nếu plateNo có giá trị hoặc không, biz nguồn gọi postWebApi_DmsClaimGetByPlateNo. Ở đích ta trả về danh sách rỗng (GAP).
            return Ok(new
            {
                Success = true,
                Data = new
                {
                    Lst_Claim_ByPlateNo = lstClaimByPlateNo
                }
            });
        }
        catch (Exception ex)
        {
            return PortResult.Fail(errorCode, "Exception", ex.Message);
        }
    }
}

public class DealerHistoryShareMng_GetListClaimByPlateNoHQReq
{
    public string? DealerCode { get; set; }
    public string? PlateNo { get; set; }
}
