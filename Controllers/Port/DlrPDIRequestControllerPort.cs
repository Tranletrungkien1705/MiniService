using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MiniService.Data;

namespace MiniService.Controllers.Port;

// port 1:1 DlrPDIRequestController.cs — DlrPDIRequest / GetByDlrPDIReqNoDL (#39)
// Nguồn: DlrPDIRequestController.cs:982-1198 -> BizCarSv.DMSSales.cs:39-183 DlrPDIRequest_Get_FromDMSSalesDL
//        -> BizCarSv.DMSSales.cs:619-720 DlrPDIRequest_Get_FromDMSSalesX
//        -> gọi dịch vụ ngoài wsDMSSale.DlrPDIRequest_Get_OS (WebService DMS Sale, không có bảng trong DB nội bộ).
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
    /// Lấy thông tin yêu cầu PDI từ DMS Sales theo số yêu cầu PDI (DlrPDIReqNo).
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

            // Gate :1158-1165:
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
}

public class DlrPDIRequest_GetByDlrPDIReqNoDLReq
{
    public string? DlrPDIReqNo { get; set; }
}
