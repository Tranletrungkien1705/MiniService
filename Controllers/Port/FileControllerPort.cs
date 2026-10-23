using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MiniService.Data;

namespace MiniService.Controllers.Port;

// port 1:1 FileController.cs — File (local file check)
// Nguồn: gate D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\FileController.cs:40-184
// Action chỉ kiểm tra file trên đĩa (System.IO.File.Exists), KHÔNG truy vấn DB, KHÔNG gọi WA_*/BIZ riêng,
// KHÔNG chép tài khoản/mật khẩu viết cứng của nguồn (BaseController.cs:295 — không chép giá trị).
// Bỏ CheckGatewayAuthentication / GetUserFullInfo_File / INOS / Sys_User_GetForCurrentUser: gọi dịch vụ ngoài — ghi GAP.
[ApiController]
[Route("api/file")]
public class FileControllerPort : ControllerBase
{
    private readonly IConfiguration _config;

    public FileControllerPort(IConfiguration config)
    {
        _config = config;
    }

    // DTO request theo đúng body nguồn (gate FileController.cs:40 — tham số `strFilePathServer` kiểu string)
    public class File_CheckFileExistReq
    {
        public string? strFilePathServer { get; set; }
    }

    // Nguồn: FileController.cs:126-127 — StdStr + gán strReturn = TConst.Flag.Inactive
    // TConst.Flag.Active = "1", TConst.Flag.Inactive = "0" (Const.Main.cs:32-33 — grep nguồn)
    private const string TConstFlagActive = "1";
    private const string TConstFlagInactive = "0";

    /// <summary>
    /// Port 1:1 action CheckFileExist (gate FileController.cs:40-184).
    /// Logic thuần trong gate: chuẩn hoá StdStr → nếu rỗng ném File_CheckFileExist_InvalidInput (:126-136),
    /// ghép chuỗi `_strConfig_FileLocal_WS` + strFilePath (KHÔNG Path.Combine — :141),
    /// File.Exists(...) → gán Active khi THIẾU (:142-145), ngược lại giữ Inactive (:127).
    /// Trả `Data = strReturn` (string, Success=true — gate :150-151); mã lỗi mặc định File_CheckFileExist (:57).
    /// </summary>
    [HttpPost("checkfileexist")]
    public IActionResult CheckFileExist(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] File_CheckFileExistReq? req = null)
    {
        try
        {
            // Nguồn: FileController.cs:126 — TUtils.CUtils.StdStr(strFilePathServer) = Trim().Normalize(), rỗng → null
            var strFilePath = StdStr(req?.strFilePathServer);

            // Nguồn: FileController.cs:129-136 — IsNullOrEmpty(strFilePath) → ném File_CheckFileExist_InvalidInput
            if (string.IsNullOrEmpty(strFilePath))
            {
                return PortResult.Fail("File_CheckFileExist_InvalidInput", "Check.strFilePathServer", null);
            }

            // Nguồn: FileController.cs:141 — ghép CHUỖI (không Path.Combine)
            var basePath = _config["_strConfig_FileLocal_WS"] ?? string.Empty;
            var filePath = basePath + strFilePath;

            // Nguồn: FileController.cs:127 — khởi tạo Inactive; :142-145 — nếu !File.Exists thì gán Active
            var strReturn = TConstFlagInactive;
            if (!System.IO.File.Exists(filePath))
            {
                strReturn = TConstFlagActive;
            }

            // Nguồn: FileController.cs:150-151 — Success=true, Data = strReturn (string)
            return Ok(new
            {
                Success = true,
                Data = strReturn
            });
        }
        catch (Exception ex)
        {
            // Nguồn: FileController.cs:157-165 — BuildMsgResultFromException dùng strErrorCodeDefault = "File_CheckFileExist" (:57)
            return PortResult.Fail("File_CheckFileExist", "Exception", ex.Message);
        }
    }

    // Port TUtils.CUtils.StdStr (xem PORT_RULES_MiniService.md mục 3): Trim().Normalize(), rỗng → null.
    private static string? StdStr(string? value)
    {
        if (value == null) return null;
        var s = value.Trim().Normalize();
        return s.Length == 0 ? null : s;
    }
}
