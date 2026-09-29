using Microsoft.AspNetCore.Mvc;
using MiniService.Models;
using MiniService.Services;

namespace MiniService.Controllers;

// =============================================================================================
// ĐỢT 2 — API cho ba màn port 1:1 (xem Models/EntitiesCarSv2.cs để biết đường trace nguồn).
// MỌI tham số tìm kiếm đều nullable ⇒ gọi không tham số vẫn 200, không rơi 400 body rỗng.
// =============================================================================================

/// <summary>
/// (A) Tra cứu / mở lệnh sửa chữa — port 1:1 FrmROSearchInfo (Ser_RO).
/// </summary>
[ApiController]
[Route("api/v1/rosearch")]
public class SerRoSearchApiController(ISerRoSearchService svc) : ControllerBase
{
    /// <summary>Danh sách 8 ô tick trạng thái đúng như form nguồn (FrmROSearchInfo.cs:98-140).</summary>
    [HttpGet("statusboxes")]
    public IActionResult StatusBoxes() => Ok(SerRoSearchStatusBox.All
        .Select(b => new { key = b.key, caption = b.caption, codes = b.codes }));

    /// <summary>
    /// Ô tìm của form: từ ngày / đến ngày (ngày vào xưởng), biển số, số khung, tên khách, 8 ô trạng thái.
    /// `status` nhận nhiều lần hoặc chuỗi phân cách '|' (nguồn ghép bằng Util.StretchListString).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search(
        DateTime? checkInFrom, DateTime? checkInTo,
        DateTime? actualDeliveryFrom, DateTime? actualDeliveryTo,
        string? status, string? plateNo, string? frameNo, string? quotationNo,
        string? cusName, string? dealerCode,
        // Bản WEB của cùng màn có phân trang (Ft_PageIndex/Ft_PageSize, ROSearchInfoController.cs:56-57);
        // bỏ trống thì chạy đúng bản WinForm (TOP 500).
        int? pageIndex, int? pageSize)
    {
        var codes = (status ?? "").Split(['|', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var (rows, itemCount, pageCount) = await svc.SearchAsync(new SerRoSearchArgs(
            checkInFrom, checkInTo, actualDeliveryFrom, actualDeliveryTo,
            codes.Length > 0 ? codes : null, plateNo, frameNo, quotationNo, cusName, dealerCode,
            pageIndex, pageSize));
        return Ok(new { total = rows.Count, itemCount, pageCount, pageIndex = pageIndex ?? 0, pageSize, rows });
    }
}

/// <summary>
/// (B) Phụ tùng của RO đang đợi hàng — port 1:1 FrmPartROSearch (Ser_ROPartItems).
/// </summary>
[ApiController]
[Route("api/v1/ropartneeds")]
public class RoPartNeedApiController(IRoPartNeedService svc) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(int? pageSize, int? page, string? dealerCode)
    {
        var (rows, total, pageCount) = await svc.SearchAsync(pageSize ?? 30, page ?? 1, dealerCode);
        return Ok(new
        {
            total, pageCount, page = page ?? 1, pageSize = pageSize ?? 30,
            pageTitle = $"Trang: {page ?? 1}/{pageCount}",   // lbl_PageTitle — FrmPartROSearch.cs:181
            rows
        });
    }

    /// <summary>Nút "Chọn" của nguồn — xem chú thích lệch trong RoPartNeedService.SelectAsync.</summary>
    [HttpPost("select")]
    public async Task<IActionResult> Select([FromBody] SelectPartNeedReq req)
    {
        var (ok, msg, chosen) = await svc.SelectAsync(req.PartIds ?? [], req.PageSize ?? 30, req.Page ?? 1, req.DealerCode);
        return ok ? Ok(new { message = msg, count = chosen.Count, rows = chosen })
                  : BadRequest(new { error = msg });
    }
}

public record SelectPartNeedReq(int[]? PartIds, int? PageSize, int? Page, string? DealerCode);

/// <summary>
/// (C) Bảng kê phiếu thu dịch vụ — port 1:1 FrmReportSerPaymentGet (Ser_Payment).
/// </summary>
[ApiController]
[Route("api/v1/serpayments")]
public class SerPaymentReportApiController(ISerPaymentReportService svc) : ControllerBase
{
    /// <summary>
    /// Mặc định của form: từ ngày = ngày đầu tháng, đến ngày = hôm nay (FrmReportSerPaymentGet.cs:182-183).
    /// Không truyền thì đích áp đúng mặc định đó.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search(DateTime? fromDate, DateTime? toDate, string? type, string? dealerCode)
    {
        var f = fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var t = toDate ?? DateTime.Today;
        var types = (type ?? "").Split(['|', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var (rows, sum) = await svc.SearchAsync(f, t, types.Length > 0 ? types : null, dealerCode);
        return Ok(new
        {
            fromDate = f.ToString("yyyy-MM-dd"), toDate = t.ToString("yyyy-MM-dd"),
            total = rows.Count, sumAmount = sum, rows
        });
    }

    /// <summary>Lập phiếu thu — 4 guard ĐANG CHẠY của BizCarSv.Debit.cs:69-134.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSerPaymentReq req)
    {
        var (ok, msg, row) = await svc.CreateAsync(req.DealerCode, req.CusId, req.PaymentAmount,
            req.PayPersonName, req.PayPersonIDCardNo, req.PayDate, req.Note,
            req.InsNo, req.InsName, req.InsPhone, req.SupplierId, req.PaymentType);
        return ok ? Ok(new { message = msg, row }) : BadRequest(new { error = msg });
    }
}

/// <summary>
/// PaymentAmount để dạng CHUỖI đúng như nguồn (strPaymentAmount) để guard "không phải số"
/// chạy được — nếu khai decimal thì ASP.NET tự trả 400 và mất thông điệp nghiệp vụ.
/// </summary>
public record CreateSerPaymentReq(
    string? DealerCode, int? CusId, string? PaymentAmount, string? PayPersonName,
    string? PayPersonIDCardNo, DateTime? PayDate, string? Note, string? InsNo,
    string? InsName, string? InsPhone, int? SupplierId, string? PaymentType);
