using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 DlrMstDealerContractFormPrintController.GetTempPrint
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\DlrMstDealerContractFormPrintController.cs:303-582 (GetTempPrint)
//   Biz:
//     - Dlr_Mst_DealerContractFormPrint_Get (Master.cs:1232-1456)
//     - Mst_TempPrintGroup_Get (Master.cs:607-832)
//     - SequenceGetForDMS (Master.cs:31-161, Master.cs:164-244)
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
    /// </summary>
    [HttpPost("getallprinttype")]
    public async Task<IActionResult> GetAllPrintType([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] DlrMstDealerContractFormPrint_GetAllPrintTypeReq? req = null)
    {
        const string errorCodeDefault = "MstTempPrintTypeGetAllActive";
        try
        {
            var flagActive = req?.FlagActive ?? "1";

            var query = _db.Set<Mst_TempPrintType>().AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(flagActive))
            {
                query = query.Where(x => EF.Functions.Like((x.FlagActive ?? "").ToLower(), PortQuery.Equal(flagActive), PortQuery.Esc));
            }

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

            var data = list.Select((x, i) => new
            {
                MyIdxSeq = (long)i,
                x.TempPrintType,
                x.TempPrintTypeName,
                x.FlagActive,
                x.LogLUBy,
                x.LogLUDTime
            }).ToList();

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

    /// <summary>
    /// Lấy mẫu in theo TempPrintType.
    /// Gate: DlrMstDealerContractFormPrintController.cs:303-582 GetTempPrint(string TempPrintType)
    /// Luồng:
    /// 1. Chuẩn hoá TempPrintType: StdStr(TempPrintType) (:380)
    /// 2. Nếu rỗng -> trả objMst_TempPrintGroup_New rỗng (LinkPDF="", TempGrpPrintBody=null, TempPrint=null), Success = false (:251-252)
    /// 3. Nếu có:
    ///    - Lấy Dlr_Mst_DealerContractFormPrint theo TempPrintType (:408-419)
    ///    - Nếu tìm thấy -> lấy TempPrint và TempPrintBody từ dòng đầu (:423-425)
    ///    - Nếu không thấy -> sinh TempPrint từ SequenceGetForDMS("TempPrint") (:465-485);
    ///      sau đó lấy Mst_TempPrintGroup theo TempPrintType (:484-495) để lấy TempGrpPrintBody (:500)
    ///    - Dịch vụ ngoài ReportBro (PostReport/LinkPDF): GAP (không bịa link PDF giả, trả linkPdf = "")
    ///    - Trả Success = true, Data = { LinkPDF, TempGrpPrintBody, TempPrint } (:244-245)
    /// </summary>
    [HttpPost("gettempprint")]
    public async Task<IActionResult> GetTempPrint([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] DlrMstDealerContractFormPrint_GetTempPrintReq? req = null)
    {
        const string errorCodeDefault = "MstTempPrintTypeGetAllActive";
        try
        {
            // Gate :380 string strTempPrintType = TUtils.CUtils.StdStr(TempPrintType);
            var strTempPrintType = req?.TempPrintType?.Trim();
            if (string.IsNullOrEmpty(strTempPrintType))
            {
                strTempPrintType = null;
            }

            var result = new DlrMstDealerContractFormPrint_GetTempPrintResult
            {
                LinkPDF = "",
                TempGrpPrintBody = null,
                TempPrint = null
            };

            // Gate :402 if (!CUtils.IsNullOrEmpty(strTempPrintType))
            if (string.IsNullOrEmpty(strTempPrintType))
            {
                // Gate :250-253: strTempPrintType rỗng -> Success mặc định của JsonResultEntry = false
                return Ok(new
                {
                    Success = false,
                    Data = result
                });
            }

            string linkPdf = "";
            string tempPrint = "";
            string printTempBody = "";

            // Gate :408-420: ListDlr_Mst_DealerContractFormPrint (where TempPrintType = ...)
            var listDlr = await _db.Set<Dlr_Mst_DealerContractFormPrint>()
                .AsNoTracking()
                .Where(x => EF.Functions.Like((x.TempPrintType ?? "").ToLower(), PortQuery.Equal(strTempPrintType), PortQuery.Esc))
                .OrderBy(x => x.TempPrint)
                .ToListAsync();

            if (listDlr.Count > 0)
            {
                // Gate :423-425
                var objDlr = listDlr[0];
                tempPrint = objDlr.TempPrint ?? "";
                printTempBody = objDlr.TempPrintBody ?? "";
                // Gate :426-460 PostReport ReportBro: dịch vụ ngoài không có ở backend -> GAP
            }
            else
            {
                // Gate :465-485: SequenceGetForDMS(TempPrint)
                // Format Master.cs:195, :230-238: {0}{1}{2:00000} với {0}=yyMM, {1}="TempPrint", {2}=seq % 100000
                long seq = await PortSeq.NextAsync<Seq_TempPrint>(_db);
                var now = DateTime.Now;
                var prefix = now.ToString("yyMM");
                tempPrint = $"{prefix}TempPrint{(seq % 100000):00000}";

                // Gate :484-495: ListMst_TempPrintGroup (where TempPrintType = ...)
                var listGroup = await _db.Set<Mst_TempPrintGroup>()
                    .AsNoTracking()
                    .Where(x => EF.Functions.Like((x.TempPrintType ?? "").ToLower(), PortQuery.Equal(strTempPrintType), PortQuery.Esc))
                    .OrderBy(x => x.TempPrintType)
                    .ToListAsync();

                if (listGroup.Count > 0)
                {
                    // Gate :499-501
                    var objGroup = listGroup[0];
                    printTempBody = objGroup.TempGrpPrintBody ?? "";
                    // Gate :502-534 PostReport ReportBro: dịch vụ ngoài không có ở backend -> GAP
                }
            }

            // Gate :538-543
            result.LinkPDF = linkPdf;
            result.TempGrpPrintBody = printTempBody;
            result.TempPrint = tempPrint;

            return Ok(new
            {
                Success = true,
                Data = result
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

public class DlrMstDealerContractFormPrint_GetTempPrintReq
{
    public string? TempPrintType { get; set; }
}

public class DlrMstDealerContractFormPrint_GetTempPrintResult
{
    public string? LinkPDF { get; set; }
    public string? TempGrpPrintBody { get; set; }
    public string? TempPrint { get; set; }
}
