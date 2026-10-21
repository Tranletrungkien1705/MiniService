using Microsoft.AspNetCore.Mvc;
using MiniService.Data;

namespace MiniService.Controllers.Port;

// port 1:1 CommonController.cs GetDTime (gate CommonController.cs:40-178; biz Common.cs:444-455).
// Action khong tham so nghiep vu, khong doc/ghi bang: chi tra gio he thong.
[ApiController]
[Route("api/common")]
public class CommonControllerPort : ControllerBase
{
    [HttpPost("getdtime")]
    public IActionResult GetDTime()
    {
        // Gate :43-48 doc header bang StdStr (Trim().Normalize(), rong -> null). Chi Tid/AppTId vao ket qua (:56-57).
        var strTid = StdStr(Request.Headers["Tid"].FirstOrDefault());
        var strAppTId = StdStr(Request.Headers["AppTId"].FirstOrDefault());

        // Gate :50,58 TTime = DateTime.Now "yyyy-MM-dd HH:mm:ss".
        var dtimeSys = DateTime.Now;

        // Gate :53-60 ErrCode mac dinh "0", Type = "CommonGetDTimeSv".
        var msgResult = new Common_GetDTimeResult
        {
            TId = strTid,
            AppTId = strAppTId,
            TTime = dtimeSys.ToString("yyyy-MM-dd HH:mm:ss"),
            ErrCode = "0",
            Type = "CommonGetDTimeSv",
            // Gate :139 Result = GetRemark(mdsResult); biz Common.cs:450 Remark = DateTime.Now "yyyy-MM-dd HH:mm:ss.ffffff".
            Result = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff"),
        };

        // Gate :143 Result<XLib.CMyMsgResult>(msgResult).
        return Ok(msgResult);
    }

    private static string? StdStr(string? value)
    {
        if (value == null) return null;
        var v = value.Trim().Normalize();
        return v.Length == 0 ? null : v;
    }
}

// Hinh dang XLib.CMyMsgResult cua nguon (CommonController.cs:53-67,139,143).
public class Common_GetDTimeResult
{
    public string? TId { get; set; }
    public string? AppTId { get; set; }
    public string? TTime { get; set; }
    public string? ErrCode { get; set; }
    public string? Type { get; set; }
    public string? Result { get; set; }
}
