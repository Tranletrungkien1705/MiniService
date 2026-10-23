using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MiniService.Data;

namespace MiniService.Controllers.Port;

// port 1:1 DlrPDIRequestController.cs — DlrPDIRequest / GetByDlrPDIReqNoDL (#39) + GetByDlrPDIReqNoHQ (#41)
//        + GetByDlrPDIReqNoWHHQ (#42) + GetROForCreateDL (#43) + ExportWHHQ (#47)
// Nguồn #39/#41: DlrPDIRequestController.cs:1416-1632 -> BizCarSv.DMSSales.cs:329-473 DlrPDIRequest_Get_FromDMSSalesHQ
//        -> BizCarSv.DMSSales.cs:619-720 DlrPDIRequest_Get_FromDMSSalesX
//        -> gọi dịch vụ ngoài wsDMSSale.DlrPDIRequest_Get_OS (WebService DMS Sale, không có bảng trong DB nội bộ).
// Nguồn #42: DlrPDIRequestController.cs:1633-1849 GetByDlrPDIReqNoWHHQ -> BizCarSv.DMSSales.cs:474-618
//        DlrPDIRequest_GetWH_FromDMSSalesHQ -> BizCarSv.DMSSales.cs:723-826 DlrPDIRequest_GetWH_FromDMSSalesX
//        -> gọi dịch vụ ngoài wsDMSSale.DlrPDIRequest_GetWH_OS (BizCarSv.DMSSales.cs:779-793);
//        toàn bộ dữ liệu Dlr_PDIRequest/Dlr_PDIRequestDtl nằm ở DMS Sale, KHÔNG có bảng trong DB nội bộ.
// Nguồn #43: DlrPDIRequestController.cs:1853-2300 GetROForCreateDL
//   -> DlrPDIRequest_Get_FromDMSSalesDL -> wsDMSSale.DlrPDIRequest_Get_OS (DMS Sale ngoài)
//   -> SerCustomerCarSync_FromPDIDL -> wsDMSSale.GetInforForSerCustomerCarSync (DMS Sale ngoài)
//   -> Ser_CustomerCar_Get_DL (Ser_CustomerCar_GetX)
// Nguồn #47: DlrPDIRequestController.cs:3043-3288 ExportWHHQ
//   -> BizCarSv.DMSSales.cs:474-618 DlrPDIRequest_GetWH_FromDMSSalesHQ
//   -> BizCarSv.DMSSales.cs:723-826 DlrPDIRequest_GetWH_FromDMSSalesX
//   -> gọi dịch vụ ngoài wsDMSSale.DlrPDIRequest_GetWH_OS (BizCarSv.DMSSales.cs:779-793)
//   -> xuất Excel 2 sheet: Dlr_PDIRequest và Dlr_PDIRequestDtl qua GenExcelExportFilePath.
[ApiController]
[Route("api/dlrpdirequest")]
public class DlrPDIRequestControllerPort : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment? _env;

    public DlrPDIRequestControllerPort(AppDbContext db, IWebHostEnvironment? env = null)
    {
        _db = db;
        _env = env;
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

            // Gate :1592-1599
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

            var lstDlrPdiRequest = new List<object>();
            var lstDlrPdiRequestDtl = new List<object>();

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
    /// Lấy thông tin lệnh RO tạo từ yêu cầu PDI (GetROForCreateDL) — hạng mục #43.
    /// Chuỗi nguồn: gate DlrPDIRequestController.cs:1853-2300 GetROForCreateDL
    ///   1. Check input DlrPDIReqNo (rỗng -> DlrPDIRequest_GetROForCreateDL_DlrPDIReqNoEmpty)
    ///      và VIN (rỗng -> DlrPDIRequest_GetROForCreateDL_VINEmpty).
    ///   2. Gọi DlrPDIRequest_Get_FromDMSSalesDL (BizCarSv.DMSSales.cs:39-183 -> DlrPDIRequest_Get_FromDMSSalesX:619-720
    ///      -> wsDMSSale.DlrPDIRequest_Get_OS: WebService ngoài DMS Sales).
    ///      Đích không có dịch vụ ngoài DMS Sales (GAP: NO_BACKEND) nên dừng tại bước này và báo lỗi
    ///      DlrPDIRequest_GetROForCreateDL_DlrPDIReqNoNotFound theo guard nguồn gate :2043.
    /// </summary>
    [HttpPost("getroforcreatedl")]
    public Task<IActionResult> GetROForCreateDL(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] DlrPDIRequest_GetROForCreateDLReq? req = null)
    {
        // Gate :1873: string strErrorCodeDefault = "DlrPDIRequest_GetROForCreateDL";
        const string errorCodeDefault = "DlrPDIRequest_GetROForCreateDL";

        try
        {
            // Gate :1940-1953: Refine and Check Input DlrPDIReqNo
            // string strDlrPDIReqNo = TUtils.CUtils.StdStr(DlrPDIReqNo);
            var dlrPdiReqNo = string.IsNullOrWhiteSpace(req?.DlrPDIReqNo) ? null : req.DlrPDIReqNo!.Trim();
            if (string.IsNullOrEmpty(dlrPdiReqNo))
            {
                // Gate :1949: TError.ErridNCarService.DlrPDIRequest_GetROForCreateDL_DlrPDIReqNoEmpty
                return Task.FromResult<IActionResult>(PortResult.Fail(
                    "DlrPDIRequest_GetROForCreateDL_DlrPDIReqNoEmpty",
                    "Check.Input.DlrPDIReqNo",
                    req?.DlrPDIReqNo));
            }

            // Gate :1955-1967: Refine and Check Input VIN
            // string strVIN = TUtils.CUtils.StdStr(VIN);
            var vin = string.IsNullOrWhiteSpace(req?.VIN) ? null : req.VIN!.Trim();
            if (string.IsNullOrEmpty(vin))
            {
                // Gate :1963: TError.ErridNCarService.DlrPDIRequest_GetROForCreateDL_VINEmpty
                return Task.FromResult<IActionResult>(PortResult.Fail(
                    "DlrPDIRequest_GetROForCreateDL_VINEmpty",
                    "Check.Input.VIN",
                    req?.VIN));
            }

            // Gate :2003-2020: mdsResult = _biz.DlrPDIRequest_Get_FromDMSSalesDL(...)
            // -> BizCarSv.DMSSales.cs:39-183 -> DlrPDIRequest_Get_FromDMSSalesX
            // -> wsDMSSale.DlrPDIRequest_Get_OS (DMS Sale ngoài).
            // Do đích KHÔNG có dịch vụ ngoài DMS Sale và không có bảng Dlr_PDIRequest (NO_BACKEND),
            // kết quả trả về từ DMS Sales rỗng -> gate :2034 dt_Dlr_PDIRequest == null || dt_Dlr_PDIRequest.Rows.Count < 0
            // ném lỗi DlrPDIRequest_GetROForCreateDL_DlrPDIReqNoNotFound.
            return Task.FromResult<IActionResult>(PortResult.Fail(
                "DlrPDIRequest_GetROForCreateDL_DlrPDIReqNoNotFound",
                "Check.Input.DlrPDIReqNo",
                dlrPdiReqNo));
        }
        catch (Exception ex)
        {
            return Task.FromResult<IActionResult>(PortResult.Fail(errorCodeDefault, "Exception", ex.Message));
        }
    }

    /// <summary>
    /// Xuất Excel yêu cầu PDI WH HQ — hạng mục #47.
    /// Chuỗi nguồn: gate DlrPDIRequestController.cs:3043-3288 ExportWHHQ
    ///   -> BizCarSv.DMSSales.cs:474-618 DlrPDIRequest_GetWH_FromDMSSalesHQ
    ///   -> BizCarSv.DMSSales.cs:723-826 DlrPDIRequest_GetWH_FromDMSSalesX
    ///   -> wsDMSSale.DlrPDIRequest_GetWH_OS (BizCarSv.DMSSales.cs:779-793).
    /// Toàn bộ dữ liệu nằm ở WebService ngoài DMS Sale; đích không kết nối dịch vụ ngoài này (GAP: NO_BACKEND).
    /// Xuất file Excel 2 sheet ("Dlr_PDIRequest" và "Dlr_PDIRequestDtl") với dòng tiêu đề theo đúng từ điển
    /// cột nguồn DlrPDIRequestExportDicColums (:3340-3355) và DlrPDIRequestDtlExportDicColums (:3356-3374),
    /// dữ liệu rỗng, trả URL file Excel: GetRequestBaseAddress() + url (gate :3254).
    /// </summary>
    [HttpPost("exportwhhq")]
    public Task<IActionResult> ExportWHHQ(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] DlrPDIRequest_ExportWHHQReq? req = null)
    {
        // Gate :3068, :3073: string strErrorCodeDefault = "DlrPDIRequestExportWHHQ";
        const string strErrorCodeDefault = "DlrPDIRequestExportWHHQ";

        try
        {
            // Gate :3142-3148: Refine and Check Input
            var dealerCode = string.IsNullOrWhiteSpace(req?.DealerCode) ? null : req.DealerCode.Trim();
            var dlrPdiReqNo = string.IsNullOrWhiteSpace(req?.DlrPDIReqNo) ? null : req.DlrPDIReqNo.Trim();
            var createdDateFrom = string.IsNullOrWhiteSpace(req?.CreatedDateFrom) ? null : req.CreatedDateFrom.Trim();
            var createdDateTo = string.IsNullOrWhiteSpace(req?.CreatedDateTo) ? null : req.CreatedDateTo.Trim();
            var vin = string.IsNullOrWhiteSpace(req?.VIN) ? null : req.VIN.Trim();
            var dlrContractNo = string.IsNullOrWhiteSpace(req?.DlrContractNo) ? null : req.DlrContractNo.Trim();
            var dlrPdiReqStatus = string.IsNullOrWhiteSpace(req?.DlrPDIReqStatus) ? null : req.DlrPDIReqStatus.Trim();

            // Nguồn: BizCarSv.DMSSales.cs:779-793 gọi wsDMSSale.DlrPDIRequest_GetWH_OS
            // Đích không có dịch vụ ngoài DMS Sale -> danh sách rỗng (GAP: NO_BACKEND)
            var lstDlrPdiRequest = new List<Dlr_PDIRequestExportItem>();
            var lstDlrPdiRequestDtl = new List<Dlr_PDIRequestDtlExportItem>();

            // Gate :3340-3355: DlrPDIRequestExportDicColums()
            var dicColNamesDlrPdiRequest = new Dictionary<string, string>
            {
                { "DealerCode", "Mã đại lý" },
                { "DlrPDIReqNo", "Số YC PDI" },
                { "CreatedDate", "Ngày tạo" },
                { "ApprovedDate", "Ngày duyệt" },
                { "Remark", "Nội dung" },
                { "DlrPDIReqStatus", "Trạng thái" },
                { "CreatedBy", "Người tạo" },
                { "FlagAccessory", "Phụ kiện" },
                { "VinTotal", "Số lượng xe" },
                { "VinFTotal", "Số xe hoàn thành" }
            };

            // Gate :3356-3374: DlrPDIRequestDtlExportDicColums()
            var dicColNamesDlrPdiRequestDtl = new Dictionary<string, string>
            {
                { "DlrPDIReqNo", "Số YC PDI" },
                { "VIN", "VIN" },
                { "CVModelCode", "Mã model" },
                { "MCMModelName", "Tên model" },
                { "CVSpecCode", "Mã Spec" },
                { "MCSSpecDescription", "Tên Spec" },
                { "MCCColorNameVn", "Tên màu TV" },
                { "DCDDlvExpectedDate", "Ngày giao xe dự kiến" },
                { "DlrContractNo", "Số hợp đồng" },
                { "DDCFullName", "Tên khách hàng" },
                { "RONo", "Số báo giá PDI" },
                { "ROCreatedDate", "Ngày tạo BG PDI" },
                { "ROStatus", "TT báo giá" }
            };

            // Gate :3249-3252: GenExcelExportFilePath("QuanLyYeuCauPDI", ref url)
            var webRoot = _env?.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var (filePath, virtualPath) = PortExcel.GenExcelExportFilePath(webRoot, "QuanLyYeuCauPDI");

            // Gate :3251-3252:
            // ExcelExport.ExportToExcelFromList(lst_Dlr_PDIRequest, dicColNamesDlrPDIRequest, filePath, "Dlr_PDIRequest");
            // ExcelExport.ExportToExcelFromList(lst_Dlr_PDIRequestDtl, dicColNamesDlrPDIRequestDtl, filePath, "Dlr_PDIRequestDtl");
            ExportMultiSheetExcel(filePath, lstDlrPdiRequest, dicColNamesDlrPdiRequest, "Dlr_PDIRequest",
                lstDlrPdiRequestDtl, dicColNamesDlrPdiRequestDtl, "Dlr_PDIRequestDtl");

            // Gate :3254: msgResult._objResult = string.Format("{0}{1}", GetRequestBaseAddress(), url);
            var baseAddress = PortExcel.GetRequestBaseAddress(Request);
            var fullUrl = $"{baseAddress}{virtualPath}";

            return Task.FromResult<IActionResult>(Ok(new
            {
                Success = true,
                Data = fullUrl
            }));
        }
        catch (Exception ex)
        {
            return Task.FromResult<IActionResult>(PortResult.Fail(strErrorCodeDefault, "Exception", ex.Message));
        }
    }

    private static void ExportMultiSheetExcel(
        string filePath,
        IEnumerable<Dlr_PDIRequestExportItem> list1,
        IReadOnlyDictionary<string, string> dic1,
        string sheet1Name,
        IEnumerable<Dlr_PDIRequestDtlExportItem> list2,
        IReadOnlyDictionary<string, string> dic2,
        string sheet2Name)
    {
        using var wb = new ClosedXML.Excel.XLWorkbook();

        // Sheet 1
        var ws1 = wb.Worksheets.Add(sheet1Name);
        var cols1 = dic1.ToList();
        for (var c = 0; c < cols1.Count; c++)
        {
            ws1.Cell(1, c + 1).Value = cols1[c].Value;
        }
        var r1 = 2;
        foreach (var row in list1)
        {
            if (row is null) continue;
            var t = row.GetType();
            for (var c = 0; c < cols1.Count; c++)
            {
                var p = t.GetProperty(cols1[c].Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                var v = p?.GetValue(row);
                ws1.Cell(r1, c + 1).Value = v switch
                {
                    null => ClosedXML.Excel.Blank.Value,
                    DateTime d => d,
                    double d => d,
                    int i => i,
                    long l => l,
                    decimal m => m,
                    _ => Convert.ToString(v) ?? ""
                };
            }
            r1++;
        }
        ws1.Columns().AdjustToContents();

        // Sheet 2
        var ws2 = wb.Worksheets.Add(sheet2Name);
        var cols2 = dic2.ToList();
        for (var c = 0; c < cols2.Count; c++)
        {
            ws2.Cell(1, c + 1).Value = cols2[c].Value;
        }
        var r2 = 2;
        foreach (var row in list2)
        {
            if (row is null) continue;
            var t = row.GetType();
            for (var c = 0; c < cols2.Count; c++)
            {
                var p = t.GetProperty(cols2[c].Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                var v = p?.GetValue(row);
                ws2.Cell(r2, c + 1).Value = v switch
                {
                    null => ClosedXML.Excel.Blank.Value,
                    DateTime d => d,
                    double d => d,
                    int i => i,
                    long l => l,
                    decimal m => m,
                    _ => Convert.ToString(v) ?? ""
                };
            }
            r2++;
        }
        ws2.Columns().AdjustToContents();

        wb.SaveAs(filePath);
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

public class DlrPDIRequest_GetROForCreateDLReq
{
    public string? DlrPDIReqNo { get; set; }
    public string? VIN { get; set; }
}

public class DlrPDIRequest_ExportWHHQReq
{
    public string? DealerCode { get; set; }
    public string? DlrPDIReqNo { get; set; }
    public string? CreatedDateFrom { get; set; }
    public string? CreatedDateTo { get; set; }
    public string? VIN { get; set; }
    public string? DlrContractNo { get; set; }
    public string? DlrPDIReqStatus { get; set; }
}

public class Dlr_PDIRequestExportItem
{
    public string? DealerCode { get; set; }
    public string? DlrPDIReqNo { get; set; }
    public string? CreatedDate { get; set; }
    public string? ApprovedDate { get; set; }
    public string? Remark { get; set; }
    public string? DlrPDIReqStatus { get; set; }
    public string? CreatedBy { get; set; }
    public string? FlagAccessory { get; set; }
    public string? VinTotal { get; set; }
    public string? VinFTotal { get; set; }
}

public class Dlr_PDIRequestDtlExportItem
{
    public string? DlrPDIReqNo { get; set; }
    public string? VIN { get; set; }
    public string? CVModelCode { get; set; }
    public string? MCMModelName { get; set; }
    public string? CVSpecCode { get; set; }
    public string? MCSSpecDescription { get; set; }
    public string? MCCColorNameVn { get; set; }
    public string? DCDDlvExpectedDate { get; set; }
    public string? DlrContractNo { get; set; }
    public string? DDCFullName { get; set; }
    public string? RONo { get; set; }
    public string? ROCreatedDate { get; set; }
    public string? ROStatus { get; set; }
}
