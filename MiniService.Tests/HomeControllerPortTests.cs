using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MiniService.Controllers.Port;
using Xunit;

namespace MiniService.Tests;

/// <summary>
/// Test port 1:1 HomeController/Index.
/// Nguồn: gate D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\HomeController.cs:12-15
///   `public ActionResult Index() { return View(); }` — không tham số, không gọi service/biz, không đọc/ghi bảng, không mã lỗi.
/// </summary>
public class HomeControllerPortTests
{
    private static JsonElement Root(IActionResult result)
    {
        var ok = Assert.IsType<OkObjectResult>(result);
        return JsonDocument.Parse(JsonSerializer.Serialize(ok.Value)).RootElement;
    }

    [Fact]
    public void Index_BodyRong_TraSuccessVaDataNull()
    {
        // Gate :12-15 — Index() không tham số ⇒ body rỗng vẫn trả thành công, không dữ liệu.
        var root = Root(new HomeControllerPort().Index(null));

        Assert.True(root.GetProperty("Success").GetBoolean());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("Data").ValueKind);
    }

    [Fact]
    public void Index_BodyCoDuLieu_VanBoQuaThamSo()
    {
        // Gate :12-15 — action không nhận tham số ⇒ body có dữ liệu cũng không đổi kết quả.
        var root = Root(new HomeControllerPort().Index(new Home_IndexReq()));

        Assert.True(root.GetProperty("Success").GetBoolean());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("Data").ValueKind);
    }
}
