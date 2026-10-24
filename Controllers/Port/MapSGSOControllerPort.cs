using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 MapSGSOController.cs — Map_SG_SO (GetByGroupCodeDL)
// Nguồn gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\MapSGSOController.cs:240-433
// Biz: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\System.cs:15222 (Map_SG_SO_Get_DL)
//      -> 15347 (Map_SG_SO_GetX).
// GAP: CheckGatewayAuthentication, GetUserFullInfo, Inos_GetUserInfoByToken, Sys_Access_CheckDenyCarSv — xác thực/phân quyền/dịch vụ ngoài Inos, đích không có.
[ApiController]
[Route("api/mapsgso")]
public class MapSGSOControllerPort : ControllerBase
{
    private readonly AppDbContext _db;

    public MapSGSOControllerPort(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lấy danh sách Map_SG_SO theo nhóm (GetByGroupCodeDL).
    /// Gate: MapSGSOController.cs:240-433 GetByGroupCodeDL(GroupCode)
    /// Biz: System.cs:15222 (Map_SG_SO_Get_DL) -> 15347 (Map_SG_SO_GetX).
    /// Lọc: GroupCode (StdStr), PartnerCode = "Web", DealerCode từ user session, Sys_Object.FlagActive = "1", Sys_Object.ObjectType in ('BTN', 'MENU').
    /// Trả: Map_SG_SO + Sys_ObjectNotMap.
    /// </summary>
    [HttpPost("getbyusercode")]
    public async Task<IActionResult> GetByGroupCodeDL(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] MapSGSOGetByGroupCodeDLReq? req = null)
    {
        var groupCode = req?.GroupCode ?? "";
        var dealerCode = "DEALER_PORT"; // Giá trị cố định từ DealerCode của user (simulate từ session, trong đích không có user session thật)
        var partnerCode = "Web"; // Hằng số TConst.Sys_Partner.Web

        // Lấy danh sách Map_SG_SO với Sys_Object active
        var mapSGSOQuery = _db.Set<Map_SG_SO>()
            .AsNoTracking()
            .Where(x => x.PartnerCode == partnerCode && x.DealerCode == dealerCode);

        // Lọc theo GroupCode nếu có
        if (!string.IsNullOrEmpty(groupCode))
        {
            mapSGSOQuery = mapSGSOQuery.Where(x => x.GroupCode == groupCode);
        }

        // Join Sys_Object để kiểm FlagActive và ObjectType
        var mapSGSOList = await mapSGSOQuery
            .Join(
                _db.Set<Sys_Object>().Where(x => x.FlagActive == "1"),
                msgso => new { msgso.ObjectCode, msgso.DealerCode, msgso.PartnerCode },
                so => new { so.ObjectCode, so.DealerCode, so.PartnerCode },
                (msgso, so) => new
                {
                    msgso.GroupCode,
                    msgso.ObjectCode,
                    msgso.DealerCode,
                    msgso.PartnerCode,
                    so.ObjectName,
                    so.ObjectType,
                    so.BizUserType
                }
            )
            .Where(x => x.ObjectType == "BTN" || x.ObjectType == "MENU")
            .OrderBy(x => x.GroupCode)
            .ToListAsync();

        // Lấy danh sách Sys_Object active + BTN/MENU
        var allObjects = await _db.Set<Sys_Object>()
            .AsNoTracking()
            .Where(x => x.FlagActive == "1"
                && (x.ObjectType == "BTN" || x.ObjectType == "MENU")
                && x.PartnerCode == partnerCode
                && x.DealerCode == dealerCode)
            .ToListAsync();

        // Lấy danh sách đã map từ kết quả trên (client-side)
        var mappedObjectCodes = mapSGSOList
            .Select(x => new { x.ObjectCode, x.DealerCode, x.PartnerCode })
            .Distinct()
            .ToList();

        // Lọc ra các object chưa map
        var sysObjectNotMapList = allObjects
            .Where(x => !mappedObjectCodes.Any(m => m.ObjectCode == x.ObjectCode && m.DealerCode == x.DealerCode && m.PartnerCode == x.PartnerCode))
            .ToList();

        return Ok(new
        {
            Success = true,
            Data = new
            {
                LstMapSGSO = mapSGSOList,
                LstSysObjectNotMap = sysObjectNotMapList
            }
        });
    }
}

public class MapSGSOGetByGroupCodeDLReq
{
    public string? GroupCode { get; set; }
}
