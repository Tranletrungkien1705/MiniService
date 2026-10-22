using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 DlrMstDealerContractFormPrintController
// Nguồn:
//   Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\DlrMstDealerContractFormPrintController.cs
//     - GetAllPrintType: 35-214
//     - GetTempPrint: 303-582
//     - CreateTempPrintGroup: 835-1025
//   Biz:
//     - Dlr_Mst_DealerContractFormPrint_Get (Master.cs:1232-1456)
//     - Mst_TempPrintGroup_Get (Master.cs:607-832)
//     - Mst_TempPrintGroup_Save (Master.cs:833-1163)
//     - SequenceGetForDMS (Master.cs:31-161, Master.cs:164-244)
[ApiController]
[Route("api/dlrmstdealercontractformprint")]
public class DlrMstDealerContractFormPrintControllerPort : ControllerBase
{
    private static readonly byte[] DesKey = new byte[8] { 3, 2, 1, 4, 5, 6, 7, 8 };
    private static readonly byte[] DesIv = new byte[8] { 8, 2, 3, 4, 5, 6, 7, 1 };

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
    /// </summary>
    [HttpPost("gettempprint")]
    public async Task<IActionResult> GetTempPrint([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] DlrMstDealerContractFormPrint_GetTempPrintReq? req = null)
    {
        const string errorCodeDefault = "MstTempPrintTypeGetAllActive";
        try
        {
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

            if (string.IsNullOrEmpty(strTempPrintType))
            {
                return Ok(new
                {
                    Success = false,
                    Data = result
                });
            }

            string linkPdf = "";
            string tempPrint = "";
            string printTempBody = "";

            var listDlr = await _db.Set<Dlr_Mst_DealerContractFormPrint>()
                .AsNoTracking()
                .Where(x => EF.Functions.Like((x.TempPrintType ?? "").ToLower(), PortQuery.Equal(strTempPrintType), PortQuery.Esc))
                .OrderBy(x => x.TempPrint)
                .ToListAsync();

            if (listDlr.Count > 0)
            {
                var objDlr = listDlr[0];
                tempPrint = objDlr.TempPrint ?? "";
                printTempBody = objDlr.TempPrintBody ?? "";
            }
            else
            {
                long seq = await PortSeq.NextAsync<Seq_TempPrint>(_db);
                var now = DateTime.Now;
                var prefix = now.ToString("yyMM");
                tempPrint = $"{prefix}TempPrint{(seq % 100000):00000}";

                var listGroup = await _db.Set<Mst_TempPrintGroup>()
                    .AsNoTracking()
                    .Where(x => EF.Functions.Like((x.TempPrintType ?? "").ToLower(), PortQuery.Equal(strTempPrintType), PortQuery.Esc))
                    .OrderBy(x => x.TempPrintType)
                    .ToListAsync();

                if (listGroup.Count > 0)
                {
                    var objGroup = listGroup[0];
                    printTempBody = objGroup.TempGrpPrintBody ?? "";
                }
            }

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

    /// <summary>
    /// Tạo / Lưu cấu hình mẫu in nhóm từ file template (.rtmpl đã mã hóa DES).
    /// Gate: DlrMstDealerContractFormPrintController.cs:835-1025 CreateTempPrintGroup(HttpPostedFileWrapper file, string tempPrintType)
    /// Biz: Master.cs:833-1163 Mst_TempPrintGroup_Save
    /// </summary>
    [HttpPost("createtempprintgroup")]
    public async Task<IActionResult> CreateTempPrintGroup([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] DlrMstDealerContractFormPrint_CreateTempPrintGroupReq? req = null)
    {
        const string strErrorCodeDefault = "MstTempPrintGroupCreateTempPrintGroup";
        try
        {
            // Gate :918-925 Kiểm tra đuôi file import (.rtmpl)
            var fileName = req?.FileName?.Trim() ?? "";
            var ext = System.IO.Path.GetExtension(fileName).ToLower();
            if (!string.Equals(ext, ".rtmpl", StringComparison.OrdinalIgnoreCase))
            {
                // Gate :928-936
                return PortResult.Fail(
                    "Mst_TempPrintGroup_Import_InvalidFile",
                    "Check.ext", ext
                );
            }

            // Gate :944-947 Đọc nội dung file đã mã hóa DES và giải mã sang JSON
            string jsonEncrypted = req?.FileContent ?? "";
            string jsonDecrypted;
            try
            {
                jsonDecrypted = DecryptDes(jsonEncrypted);
            }
            catch (Exception)
            {
                return PortResult.Fail(
                    "Mst_TempPrintGroup_Import_InvalidFile",
                    "Check.ext", ext
                );
            }

            // Parse JSON từ file: data { UserId, Title, Detail }
            JsonNode? dataNode;
            try
            {
                dataNode = JsonNode.Parse(jsonDecrypted);
            }
            catch (Exception)
            {
                return PortResult.Fail(
                    "Mst_TempPrintGroup_Import_InvalidFile",
                    "Check.ext", ext
                );
            }

            var detail = dataNode?["Detail"];

            // Gate :954-963 Xây dựng object report json bọc ReportServer
            var watermark = "";
            var objJson = new
            {
                outputFormat = "pdf",
                isTestData = false,
                data = new
                {
                    DataTable = new List<object>()
                },
                watermark = watermark,
                report = detail
            };
            string serializedObject = JsonSerializer.Serialize(objJson);

            // Biz :901-904 Chuẩn hóa tham số
            // string strDealerCode = TUtils.CUtils.StdParam(objDealerCode);
            // string strTempPrintType = TUtils.CUtils.StdParam(objTempPrintType);
            // string strTempPrintBody = TUtils.CUtils.StdStr(objTempPrintBody);
            // string strRemark = TUtils.CUtils.StdStr(objRemark);
            var strDealerCode = req?.DealerCode?.Trim().ToUpper();
            if (string.IsNullOrEmpty(strDealerCode))
            {
                strDealerCode = null;
            }

            var strTempPrintType = req?.TempPrintType?.Trim().ToUpper();
            if (string.IsNullOrEmpty(strTempPrintType))
            {
                strTempPrintType = null;
            }

            var strTempPrintBody = serializedObject?.Trim();
            var strRemark = req?.Remark?.Trim();
            if (string.IsNullOrEmpty(strRemark))
            {
                strRemark = null;
            }

            var strWAUserCode = req?.WAUserCode?.Trim();
            if (string.IsNullOrEmpty(strWAUserCode))
            {
                strWAUserCode = "port";
            }

            // Biz :914-924 Check strTempPrintType NullOrEmpty
            if (string.IsNullOrEmpty(strTempPrintType))
            {
                return PortResult.Fail(
                    "Mst_TempPrintGroup_Save_InvalidTempPrint",
                    "Check.strTempPrintType", strTempPrintType
                );
            }

            // Biz :948-954 Mst_TempPrintType_CheckDB(strTempPrintType, FlagExist="1", FlagActive="1")
            var tempType = await _db.Set<Mst_TempPrintType>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => EF.Functions.Like((x.TempPrintType ?? "").ToLower(), PortQuery.Equal(strTempPrintType), PortQuery.Esc));

            if (tempType == null)
            {
                return PortResult.Fail(
                    "Mst_TempPrintType_CheckDB_TempPrintTypeNotFound",
                    "Check.TempPrintType", strTempPrintType
                );
            }

            if (!string.Equals(tempType.FlagActive, "1", StringComparison.Ordinal))
            {
                return PortResult.Fail(
                    "Mst_TempPrintType_CheckDB_FlagActiveNotMatched",
                    "Check.TempPrintType", strTempPrintType,
                    "Check.strFlagActiveListToCheck", "1",
                    "DB.FlagActive", tempType.FlagActive
                );
            }

            // Biz :938-944 Lấy log ngày tạo nếu đã tồn tại
            var existingGroup = await _db.Set<Mst_TempPrintGroup>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => EF.Functions.Like((x.TempPrintType ?? "").ToLower(), PortQuery.Equal(strTempPrintType), PortQuery.Esc));

            var nowUtcStr = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            // Biz :960-1099 SaveTemp & SaveDB: Xóa bản ghi cũ cùng TempPrintType ở cả Mst_TempPrintGroup và Dlr_Mst_DealerContractFormPrint, sau đó Insert vào Mst_TempPrintGroup
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Delete Dlr_Mst_DealerContractFormPrint where TempPrintType = strTempPrintType
                var dlrList = await _db.Set<Dlr_Mst_DealerContractFormPrint>()
                    .Where(x => EF.Functions.Like((x.TempPrintType ?? "").ToLower(), PortQuery.Equal(strTempPrintType), PortQuery.Esc))
                    .ToListAsync();
                if (dlrList.Count > 0)
                {
                    _db.Set<Dlr_Mst_DealerContractFormPrint>().RemoveRange(dlrList);
                }

                // Delete Mst_TempPrintGroup where TempPrintType = strTempPrintType
                var grpList = await _db.Set<Mst_TempPrintGroup>()
                    .Where(x => EF.Functions.Like((x.TempPrintType ?? "").ToLower(), PortQuery.Equal(strTempPrintType), PortQuery.Esc))
                    .ToListAsync();
                if (grpList.Count > 0)
                {
                    _db.Set<Mst_TempPrintGroup>().RemoveRange(grpList);
                }

                await _db.SaveChangesAsync();

                // Insert into Mst_TempPrintGroup
                var newGroup = new Mst_TempPrintGroup
                {
                    TempPrintType = strTempPrintType,
                    DealerCode = strDealerCode ?? "",
                    TempGrpPrintBody = strTempPrintBody,
                    Remark = strRemark,
                    FlagActive = "1",
                    LogLUDTimeUTC = nowUtcStr,
                    LogLUBy = strWAUserCode
                };
                await _db.Set<Mst_TempPrintGroup>().AddAsync(newGroup);
                await _db.SaveChangesAsync();

                await tx.CommitAsync();

                return Ok(new
                {
                    Success = true,
                    Data = (object?)null
                });
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            return PortResult.Fail(strErrorCodeDefault, "Exception", ex.Message);
        }
    }

    /// <summary>
    /// Giải mã DES Unicode theo idn.CarService.Utils.CUtils.Decrypt (key/iv chuẩn nguồn)
    /// </summary>
    private static string DecryptDes(string text)
    {
        using var algorithm = DES.Create();
        using var transform = algorithm.CreateDecryptor(DesKey, DesIv);
        byte[] inputBuffer = Convert.FromBase64String(text);
        byte[] outputBuffer = transform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
        return Encoding.Unicode.GetString(outputBuffer);
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

public class DlrMstDealerContractFormPrint_CreateTempPrintGroupReq
{
    public string? FileName { get; set; }
    public string? FileContent { get; set; }
    public string? TempPrintType { get; set; }
    public string? DealerCode { get; set; }
    public string? Remark { get; set; }
    public string? WAUserCode { get; set; }
}
