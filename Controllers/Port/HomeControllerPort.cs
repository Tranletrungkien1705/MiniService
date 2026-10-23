using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MiniService.Controllers.Port;

// port 1:1 HomeController.cs — Index
// Nguồn: gate D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\HomeController.cs:12-15
//   public ActionResult Index() { return View(); }
// Gate KHÔNG gọi service/biz nào (source_excerpt.txt:11-13 "GATE goi service:" rỗng), không đọc/ghi bảng,
// không có mã lỗi (source_excerpt.txt:16-18: 0 chỗ ném lỗi, không có mã lỗi, không có bảng Crd_/Mst_/...).
// Vì vậy action chỉ trả kết quả thành công rỗng; KHÔNG tự chế dữ liệu cho View (View của nguồn không nằm trong nguồn trích).
[ApiController]
[Route("api/home")]
public class HomeControllerPort : ControllerBase
{
    /// <summary>
    /// Port 1:1 action Index (gate HomeController.cs:12-15). Không tham số, không nghiệp vụ: nguồn chỉ `return View()`.
    /// Ở tầng API port: trả Success = true, Data = null (không có dữ liệu nào để trả vì nguồn không lấy dữ liệu).
    /// </summary>
    [HttpPost("index")]
    public IActionResult Index([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] Home_IndexReq? req = null)
    {
        return Ok(new
        {
            Success = true,
            Data = (object?)null
        });
    }
}

// Body của action: nguồn Index() không có tham số ⇒ DTO rỗng (nhận body để không lỗi khi client gửi kèm).
public class Home_IndexReq
{
}
