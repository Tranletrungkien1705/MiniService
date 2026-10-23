using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MiniService.Data;

namespace MiniService.Controllers.Port;

// port 1:1 DlrPDIRequestController.cs — DlrPDIRequest / GetByDlrPDIReqNoDL (#39) + GetByDlrPDIReqNoHQ (#41)
//        + GetByDlrPDIReqNoWHHQ (#42)
// Nguồn #39/#41: DlrPDIRequestController.cs:1416-1632 -> BizCarSv.DMSSales.cs:329-473 DlrPDIRequest_Get_FromDMSSalesHQ
//        -> BizCarSv.DMSSales.cs:619-720 DlrPDIRequest_Get_FromDMSSalesX
//        -> gọi dịch vụ ngoài wsDMSSale.DlrPDIRequest_Get_OS (WebService DMS Sale, không có bảng trong DB nội bộ).
// Nguồn #42: DlrPDIRequestController.cs:1633-1849 GetByDlrPDIReqNoWHHQ -> BizCarSv.DMSSales.cs:474-618
//        DlrPDIRequest_GetWH_FromDMSSalesHQ -> BizCarSv.DMSSales.cs:723-826 DlrPDIRequest_GetWH_FromDMSSalesX
//        -> gọi dịch vụ ngoài wsDMSSale.DlrPDIRequest_GetWH_OS (BizCarSv.DMSSales.cs:779-793);
//        toàn bộ dữ liệu Dlr_PDIRequest/Dlr_PDIRequestDtl nằm ở DMS Sale, KHÔNG có bảng trong DB nội bộ.
[ApiController]
[Route("api/dlrpdirequest")]
public class DlrPDIRequestControllerPort : ControllerBase
{
    private readonly AppDbContext _db;

    public DlrPDIRequestControllerPort(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lấy thông tin yêu cầu PDI từ DMS Sales theo số yêu cầu PDI (DlrPDIReqNo) cho Dealer.
    /// Toàn bộ dữ liệu ở nguồn được lấy từ WebService ngoài: wsDMSSale.DlrPDIRequest_Get_OS.
    /// Hệ thống đích không kết nối dịch vụ ngoài này (GAP: NO_BACKEND) -> trả danh sách rỗng.
    /// </summary>
    [HttpPost("getbydlrpdireqnodl")]
    public Task<IActionResult> GetByDlrPDIReqNoDL(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] DlrPDIRequest_GetByDlrPDIReqNoDLReq? req = null)
    {
        // Biz :66 — mã lỗi mặc định của DlrPDIRequest_Get_FromDMSSalesDL
        const string errorCode = "DlrPDIRequest_Get_FromDMSSalesDL";

        try
        {
            // Gate :1071: string strDlrPDIReqNo = TUtils.CUtils.StdStr(DlrPDIReqNo);
            var dlrPdiReqNo = string.IsNullOrWhiteSpace(req?.DlrPDIReqNo) ? null : req.DlrPDIReqNo!.Trim();

            // Nguồn: BizCarSv.DMSSales.cs:675-689 gọi wsDMSSale.DlrPDIRequest_Get_OS
            // Đích không có dịch vụ ngoài DMS Sale -> GAP: trả rỗng
            var lstDlrPdiRequest = new List<object>();
            var lstDlrPdiRequestDtl = new List<object>();

            // Gate :1158-1165
            return Task.FromResult<IActionResult>(Ok(new
            {
                Success = true,
                Data = new
                {
                    Lst_Dlr_PDIRequest = lstDlrPdiRequest,
                    Lst_Dlr_PDIRequestDtl = lstDlrPdiRequestDtl
                }
            }));
        }
        catch (Exception ex)
        {
            return Task.FromResult<IActionResult>(PortResult.Fail(errorCode, "Exception", ex.Message));
        }
    }

    /// <summary>
    /// Lấy thông tin yêu cầu PDI từ DMS Sales theo số yêu cầu PDI (DlrPDIReqNo) cho HQ (#41).
    /// Nguồn: DlrPDIRequestController.cs:1416-1632 -> BizCarSv.DMSSales.cs:329-473 DlrPDIRequest_Get_FromDMSSalesHQ
    ///        -> BizCarSv.DMSSales.cs:619-720 DlrPDIRequest_Get_FromDMSSalesX
    ///        -> gọi dịch vụ ngoài wsDMSSale.DlrPDIRequest_Get_OS.
    /// Hệ thống đích không kết nối dịch vụ ngoài này (GAP: NO_BACKEND) -> trả danh sách rỗng.
    /// </summary>
    [HttpPost("getbydlrpdireqnohq")]
    public Task<IActionResult> GetByDlrPDIReqNoHQ(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] DlrPDIRequest_GetByDlrPDIReqNoHQReq? req = null)
    {
        // BizCarSv.DMSSales.cs:577 — mã lỗi mặc định của DlrPDIRequest_Get_FromDMSSalesHQ
        const string errorCode = "DlrPDIRequest_Get_FromDMSSalesHQ";

        try
        {
            // Gate :1505: string strDlrPDIReqNo = TUtils.CUtils.StdStr(DlrPDIReqNo);
            var dlrPdiReqNo = string.IsNullOrWhiteSpace(req?.DlrPDIReqNo) ? null : req.DlrPDIReqNo!.Trim();

            // Nguồn: BizCarSv.DMSSales.cs:675-689 gọi wsDMSSale.DlrPDIRequest_Get_OS
            // Đích không có dịch vụ ngoài DMS Sale -> GAP: trả danh sách rỗng
            var lstDlrPdiRequest = new List<object>();
            var lstDlrPdiRequestDtl = new List<object>();

            // Gate :1592-1599:
            // objJsonResultEntry.Data = new
            // {
            //     lst_Dlr_PDIRequest = lst_Dlr_PDIRequest,
            //     lst_Dlr_PDIRequestDtl = lst_Dlr_PDIRequestDtl
            // };
            // objJsonResultEntry.SetSuccess();
            return Task.FromResult<IActionResult>(Ok(new
            {
                Success = true,
                Data = new
                {
                    Lst_Dlr_PDIRequest = lstDlrPdiRequest,
                    Lst_Dlr_PDIRequestDtl = lstDlrPdiRequestDtl
                }
            }));
        }
        catch (Exception ex)
        {
            return Task.FromResult<IActionResult>(PortResult.Fail(errorCode, "Exception", ex.Message));
        }
    }

    /// <summary>
    /// Lấy yêu cầu PDI từ DMS Sales HQ (warehouse view) theo số yêu cầu PDI — hạng mục #42.
    /// Chuỗi nguồn: gate DlrPDIRequestController.cs:1633-1849 GetByDlrPDIReqNoWHHQ
    ///   -> biz BizCarSv.DMSSales.cs:474-618 DlrPDIRequest_GetWH_FromDMSSalesHQ
    ///   -> biz BizCarSv.DMSSales.cs:723-826 DlrPDIRequest_GetWH_FromDMSSalesX
    ///   -> dịch vụ ngoài wsDMSSale.DlrPDIRequest_GetWH_OS (BizCarSv.DMSSales.cs:779-793).
    /// Toàn bộ dữ liệu Dlr_PDIRequest / Dlr_PDIRequestDtl nằm ở DMS Sale; đích không có dịch vụ ngoài này
    /// (GAP: NO_BACKEND) -> trả danh sách rỗng, KHÔNG tự dựng bảng/kết quả.
    /// </summary>
    [HttpPost("getbydlrpdireqnohwhq")]
    public Task<IActionResult> GetByDlrPDIReqNoWHHQ(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] DlrPDIRequest_GetByDlrPDIReqNoWHHQReq? req = null)
    {
        // Gate :1652 strErrorCodeDefault = "DlrPDIRequestGetByDlrPDIReqNoWHHQ";
        // BizCarSv.DMSSales.cs:501 strErrorCodeDefault = TError.ErridNCarService.DlrPDIRequest_GetWH_FromDMSSalesHQ
        const string errorCode = "DlrPDIRequest_GetWH_FromDMSSalesHQ";

        try
        {
            // Gate :1722: string strDlrPDIReqNo = TUtils.CUtils.StdStr(DlrPDIReqNo);
            var dlrPdiReqNo = string.IsNullOrWhiteSpace(req?.DlrPDIReqNo) ? null : req.DlrPDIReqNo!.Trim();

            // Gate :1727: strDlrPDIReqStatus = TUtils.CUtils.StdStr(TConst.Stage.Approved) — chỉ dùng để dựng
            // strWhereClause_Dlr_PDIRequest (gate :3292-3336) rồi chuyển sang DMS Sale; không đọc DB nội bộ.
            // Gate :1737-1738: strFt_RecordStart = "0", strFt_RecordCount = "1".
            // BizCarSv.DMSSales.cs:779-793: dữ liệu lấy từ wsDMSSale.DlrPDIRequest_GetWH_OS.
            // Đích không có dịch vụ ngoài DMS Sale -> GAP: trả danh sách rỗng.
            var lstDlrPdiRequest = new List<object>();
            var lstDlrPdiRequestDtl = new List<object>();

            // Gate :1809-1814:
            // objJsonResultEntry.Data = new { lst_Dlr_PDIRequest = lst_Dlr_PDIRequest,
            //                                  lst_Dlr_PDIRequestDtl = lst_Dlr_PDIRequestDtl };
            // objJsonResultEntry.SetSuccess();
            return Task.FromResult<IActionResult>(Ok(new
            {
                Success = true,
                Data = new
                {
                    Lst_Dlr_PDIRequest = lstDlrPdiRequest,
                    Lst_Dlr_PDIRequestDtl = lstDlrPdiRequestDtl
                }
            }));
        }
        catch (Exception ex)
        {
            return Task.FromResult<IActionResult>(PortResult.Fail(errorCode, "Exception", ex.Message));
        }
    }
}

public class DlrPDIRequest_GetByDlrPDIReqNoDLReq
{
    public string? DlrPDIReqNo { get; set; }
}

public class DlrPDIRequest_GetByDlrPDIReqNoHQReq
{
    public string? DlrPDIReqNo { get; set; }
}

public class DlrPDIRequest_GetByDlrPDIReqNoWHHQReq
{
    public string? DlrPDIReqNo { get; set; }
}
