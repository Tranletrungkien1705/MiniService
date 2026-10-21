using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 CarWarrantyInformationController.cs — Car_Warranty_Information / Ser_MST_ROWarrantyRenewal
// Gate: CarWarrantyInformationController.cs:277-520 (GetWHByPlateNoVIN)
//   • Biz 1: BizCarSv.Report.Special.Warranty.cs:185-336 (Rpt_DMSSer_Car_Warranty_Information_WH)
//            → gọi dịch vụ NGOÀI wsDMSSale.Rpt_DMSSer_Car_Warranty_Information_WH (:246-262) — KHÔNG port (GAP).
//   • Biz 2: BizCarSv.AssignmentOfWork.cs:1819-1972 (Ser_MST_ROWarrantyRenewal_Get)
//            → SQL thật trên Ser_MST_ROWarrantyRenewal inner join Ser_MST_ROWarrantyRenewalCategory (:1886-1904).
[ApiController]
[Route("api/carwarrantyinformation")]
public class CarWarrantyInformationControllerPort : ControllerBase
{
    private readonly AppDbContext _db;

    public CarWarrantyInformationControllerPort(AppDbContext db) { _db = db; }

    /// <summary>
    /// Lấy thông tin bảo hành theo Biển số / VIN. Gate :277-520.
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

            // Biz :1886-1904 — Ser_MST_ROWarrantyRenewal smrowr inner join Ser_MST_ROWarrantyRenewalCategory smrowrc
            //   on smrowr.WrtReneCateCode = smrowrc.WrtReneCateCode
            //   where (1=1) <VIN LIKE '%x%' nếu có> ; select smrowr.*, smrowrc.WrtReneCateName.
            // Gate :375 — strVINLikeCondition = GenLikeCondition2Percent(strVIN) = "like %{VIN}%" ⇒ LIKE '%x%'.
            // Biz KHÔNG dùng strFt_RecordStart/RecordCount ⇒ không phân trang.
            var rq = _db.Set<Ser_MST_ROWarrantyRenewal>().AsNoTracking().AsQueryable();
            if (vin != null)
            {
                var pattern = PortQuery.Contains(vin);
                rq = rq.Where(r => EF.Functions.Like((r.VIN ?? "").ToLower(), pattern, PortQuery.Esc));
            }

            var dtl = await (from r in rq
                             join c in _db.Set<Ser_MST_ROWarrantyRenewalCategory>().AsNoTracking()
                                 on r.WrtReneCateCode equals c.WrtReneCateCode
                             // Gate :474-475 — mdsResult.Tables["Ser_MST_ROWarrantyRenewal"]
                             // → ToListof<Car_Warranty_InformationDtl> (chỉ 4 cột).
                             select new CarWarrantyInformation_GetWHByPlateNoVINDtlRow
                             {
                                 WrtReneCateName = c.WrtReneCateName,
                                 WrtReneDate = r.WrtReneDate,
                                 Remark = r.Remark,
                                 LogLUDateTime = r.LogLUDateTime,
                             }).ToListAsync();

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
            // Biz :206 — mã lỗi mặc định của biz.
            return PortResult.Fail(errorCode, "Exception", ex.Message);
        }
    }
}

/// <summary>
/// Body của action — gate :277-281 nhận DealerCode/PlateNo/VIN. A.14: đích nhận qua JSON body.
/// DealerCode chỉ dùng cho log (gate :323), KHÔNG dùng trong truy vấn; PlateNo chỉ dùng cho dịch vụ ngoài (GAP);
/// VIN dùng cho bộ lọc Ser_MST_ROWarrantyRenewal.
/// </summary>
public class CarWarrantyInformation_GetWHByPlateNoVINReq
{
    public string? DealerCode { get; set; }
    public string? PlateNo { get; set; }
    public string? VIN { get; set; }
}

/// <summary>
/// Dòng lst_Car_Warranty_InformationDtl — model nguồn 12.Dev.Common\...\CarSv\Car_Warranty_InformationDtl.cs:9-15.
/// </summary>
public class CarWarrantyInformation_GetWHByPlateNoVINDtlRow
{
    public string? WrtReneCateName { get; set; }
    public DateTime? WrtReneDate { get; set; }
    public string? Remark { get; set; }
    public DateTime? LogLUDateTime { get; set; }
}
