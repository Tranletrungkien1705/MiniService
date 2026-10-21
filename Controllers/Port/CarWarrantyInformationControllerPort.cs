using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 CarWarrantyInformationController.cs — Car_Warranty_Information / Ser_MST_ROWarrantyRenewal
// Gate #18: CarWarrantyInformationController.cs:277-520 (GetWHByPlateNoVIN)
//   • Biz 1: BizCarSv.Report.Special.Warranty.cs:185-336 (Rpt_DMSSer_Car_Warranty_Information_WH)
//            → gọi dịch vụ NGOÀI wsDMSSale.Rpt_DMSSer_Car_Warranty_Information_WH (:246-262) — KHÔNG port (GAP).
//   • Biz 2: BizCarSv.AssignmentOfWork.cs:1819-1972 (Ser_MST_ROWarrantyRenewal_Get) — SQL thật.
// Gate #19: CarWarrantyInformationController.cs:34-276 (GetByPlateNoVIN)
//   • Biz 1: BizCarSv.Report.Special.Warranty.cs:32-184 (Rpt_DMSSer_Car_Warranty_Information)
//            → gọi dịch vụ NGOÀI wsDMSSale.Rpt_DMSSer_Car_Warranty_Information (:99-108) — KHÔNG port (GAP).
//   • Biz 2: BizCarSv.AssignmentOfWork.cs:1819-1972 (Ser_MST_ROWarrantyRenewal_Get) — CÙNG hàm biz, cùng SQL.
[ApiController]
[Route("api/carwarrantyinformation")]
public class CarWarrantyInformationControllerPort : ControllerBase
{
    private readonly AppDbContext _db;

    public CarWarrantyInformationControllerPort(AppDbContext db) { _db = db; }

    /// <summary>
    /// Lấy thông tin bảo hành theo Biển số / VIN (bản WH). Gate :277-520 (#18).
    /// Kết quả: lst_Car_Warranty_Information (dịch vụ ngoài DMS Sale — GAP, trả rỗng) +
    /// lst_Car_Warranty_InformationDtl (Ser_MST_ROWarrantyRenewal — port thật).
    /// </summary>
    [HttpPost("getwhbyplatenovin")]
    public async Task<IActionResult> GetWHByPlateNoVIN(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CarWarrantyInformation_GetWHByPlateNoVINReq? req = null)
    {
        // Biz :206 — mã lỗi mặc định của Rpt_DMSSer_Car_Warranty_Information_WH.
        const string errorCode = "Rpt_DMSSer_Car_Warranty_Information_WH";
        try
        {
            // Gate :371-372 — strPlateNo = StdStr(PlateNo); strVIN = StdStr(VIN).
            // strPlateNo chỉ dùng cho dịch vụ ngoài (GAP); strVIN dùng cho Ser_MST_ROWarrantyRenewal_Get.
            var vin = string.IsNullOrWhiteSpace(req?.VIN) ? null : req.VIN!.Trim();

            // GAP: lst_Car_Warranty_Information lấy từ dịch vụ ngoài DMS Sale
            // (Biz :246-262 wsDMSSale.Rpt_DMSSer_Car_Warranty_Information_WH) — KHÔNG port, trả rỗng.
            var warrantyInfo = new List<object>();

            // Biz :1886-1904 — Ser_MST_ROWarrantyRenewal inner join Ser_MST_ROWarrantyRenewalCategory.
            // Gate :375 — strVINLikeCondition = GenLikeCondition2Percent(strVIN) ⇒ LIKE '%x%'.
            // Biz KHÔNG dùng strFt_RecordStart/RecordCount ⇒ không phân trang.
            var dtl = await QueryWarrantyDtlAsync(vin);

            // Gate :480-485 — Data = new { lst_Car_Warranty_Information, lst_Car_Warranty_InformationDtl }; SetSuccess().
            // A.11 — JSON PascalCase ⇒ Lst_...
            return Ok(new
            {
                Success = true,
                Data = new
                {
                    Lst_Car_Warranty_Information = warrantyInfo,
                    Lst_Car_Warranty_InformationDtl = dtl,
                }
            });
        }
        catch (Exception ex)
        {
            return PortResult.Fail(errorCode, "Exception", ex.Message);
        }
    }

    /// <summary>
    /// #19 — Lấy thông tin bảo hành theo Biển số / VIN. Gate :34-276.
    /// Khác biệt so với #18: biz 1 là `Rpt_DMSSer_Car_Warranty_Information` (KHÔNG có hậu tố _WH),
    /// nên mã lỗi mặc định cũng KHÔNG có hậu tố `_WH` (biz :54). Biz 2 giống #18.
    /// </summary>
    [HttpPost("getbyplatenovin")]
    public async Task<IActionResult> GetByPlateNoVIN(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CarWarrantyInformation_GetByPlateNoVINReq? req = null)
    {
        // Biz :54 — mã lỗi mặc định của Rpt_DMSSer_Car_Warranty_Information.
        const string errorCode = "Rpt_DMSSer_Car_Warranty_Information";
        try
        {
            // Gate :128-129 — strPlateNo = StdStr(PlateNo); strVIN = StdStr(VIN).
            // strPlateNo chỉ dùng cho dịch vụ ngoài (GAP); strVIN dùng cho Ser_MST_ROWarrantyRenewal_Get.
            var vin = string.IsNullOrWhiteSpace(req?.VIN) ? null : req.VIN!.Trim();

            // GAP: lst_Car_Warranty_Information lấy từ dịch vụ ngoài DMS Sale
            // (Biz :99-108 wsDMSSale.Rpt_DMSSer_Car_Warranty_Information) — KHÔNG port, trả rỗng.
            var warrantyInfo = new List<object>();

            // Biz :1886-1904 — cùng hàm biz `Ser_MST_ROWarrantyRenewal_Get` với #18.
            // Gate :132 + :198 — strVINLikeCondition = GenLikeCondition2Percent(strVIN) ⇒ LIKE '%x%'.
            // strLogLUDateTimeFrom = "" / strLogLUDateTimeTo = "" ⇒ không lọc LogLUDateTime.
            var dtl = await QueryWarrantyDtlAsync(vin);

            // Gate :236-241 — Data = new { lst_Car_Warranty_Information, lst_Car_Warranty_InformationDtl }; SetSuccess().
            return Ok(new
            {
                Success = true,
                Data = new
                {
                    Lst_Car_Warranty_Information = warrantyInfo,
                    Lst_Car_Warranty_InformationDtl = dtl,
                }
            });
        }
        catch (Exception ex)
        {
            // Biz :54 — mã lỗi mặc định của biz.
            return PortResult.Fail(errorCode, "Exception", ex.Message);
        }
    }

    /// <summary>
    /// Biz :1886-1904 (AssignmentOfWork.cs) — dùng chung cho cả 2 action:
    /// <code>
    /// select smrowr.*, smrowrc.WrtReneCateName
    ///   from Ser_MST_ROWarrantyRenewal smrowr
    ///   inner join Ser_MST_ROWarrantyRenewalCategory smrowrc
    ///     on smrowr.WrtReneCateCode = smrowrc.WrtReneCateCode
    ///   where (1=1) &lt;smrowr.VIN like '%x%' nếu có&gt;
    /// </code>
    /// Gate :132 + :198 — strVINLikeCondition = GenLikeCondition2Percent(strVIN).
    /// strLogLUDateTimeFrom/To = "" ⇒ không lọc LogLUDateTime. Biz không dùng RecordStart/RecordCount ⇒ không phân trang.
    /// </summary>
    private async Task<List<CarWarrantyInformation_GetWHByPlateNoVINDtlRow>> QueryWarrantyDtlAsync(string? vin)
    {
        var rq = _db.Set<Ser_MST_ROWarrantyRenewal>().AsNoTracking().AsQueryable();
        if (vin != null)
        {
            // Biz :1910 — smrowr.VIN like '%x%' (GenLikeCondition2Percent).
            var pattern = PortQuery.Contains(vin);
            rq = rq.Where(r => EF.Functions.Like((r.VIN ?? "").ToLower(), pattern, PortQuery.Esc));
        }

        return await (from r in rq
                     join c in _db.Set<Ser_MST_ROWarrantyRenewalCategory>().AsNoTracking()
                         on r.WrtReneCateCode equals c.WrtReneCateCode
                     // Gate :231-232 — ToListof<Car_Warranty_InformationDtl> (model nguồn chỉ 4 cột).
                     select new CarWarrantyInformation_GetWHByPlateNoVINDtlRow
                     {
                         WrtReneCateName = c.WrtReneCateName,
                         WrtReneDate = r.WrtReneDate,
                         Remark = r.Remark,
                         LogLUDateTime = r.LogLUDateTime,
                     }).ToListAsync();
    }
}

/// <summary>
/// Body của action #18 `GetWHByPlateNoVIN` — gate :277-281 nhận DealerCode/PlateNo/VIN (A.14).
/// </summary>
public class CarWarrantyInformation_GetWHByPlateNoVINReq
{
    public string? DealerCode { get; set; }
    public string? PlateNo { get; set; }
    public string? VIN { get; set; }
}

/// <summary>
/// Body của action #19 `GetByPlateNoVIN` — gate :34-37 nhận DealerCode/PlateNo/VIN.
/// Cùng hình dạng với `GetWHByPlateNoVINReq` nhưng đặt tên theo hạng mục (A.24).
/// DealerCode chỉ dùng cho log (gate :127 ghi đè bằng orgUserUI.DealerCode);
/// PlateNo chỉ dùng cho biz 1 (ngoài — GAP); VIN dùng cho biz 2 (LIKE '%x%').
/// </summary>
public class CarWarrantyInformation_GetByPlateNoVINReq
{
    public string? DealerCode { get; set; }
    public string? PlateNo { get; set; }
    public string? VIN { get; set; }
}

/// <summary>
/// Dòng lst_Car_Warranty_InformationDtl — model nguồn `12.Dev.Common\...\CarSv\Car_Warranty_InformationDtl.cs:9-15`.
/// Dùng chung cho cả 2 action (cùng model nguồn, chỉ 4 cột được select).
/// </summary>
public class CarWarrantyInformation_GetWHByPlateNoVINDtlRow
{
    public string? WrtReneCateName { get; set; }
    public DateTime? WrtReneDate { get; set; }
    public string? Remark { get; set; }
    public DateTime? LogLUDateTime { get; set; }
}
