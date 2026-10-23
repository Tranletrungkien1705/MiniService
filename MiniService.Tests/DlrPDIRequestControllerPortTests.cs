using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MiniService.Controllers.Port;
using MiniService.Data;
using Xunit;

namespace MiniService.Tests;

/// <summary>
/// Test port 1:1 DlrPDIRequestController/GetByDlrPDIReqNoWHHQ (#42).
/// Nguồn: gate DlrPDIRequestController.cs:1633-1849 -> BizCarSv.DMSSales.cs:474-618
/// DlrPDIRequest_GetWH_FromDMSSalesHQ -> :723-826 DlrPDIRequest_GetWH_FromDMSSalesX
/// -> dịch vụ ngoài wsDMSSale.DlrPDIRequest_GetWH_OS (:779-793).
/// Đích KHÔNG có dịch vụ ngoài DMS Sale và KHÔNG có bảng Dlr_PDIRequest/Dlr_PDIRequestDtl
/// (GAP: NO_BACKEND) ⇒ mọi ca đều trả Success = true với 2 danh sách rỗng; không có bộ lọc nội bộ nào
/// để ra số dòng khác nhau (ghi GAP trong TOM TAT).
/// </summary>
public class DlrPDIRequestControllerPortTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly DlrPDIRequestControllerPort _controller;

    public DlrPDIRequestControllerPortTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
        _db = new AppDbContext(opt, new TenantContext());
        _db.Database.EnsureCreated();
        _controller = new DlrPDIRequestControllerPort(_db);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = null! }
        };
        _controller.ControllerContext.HttpContext.Request.Scheme = "http";
        _controller.ControllerContext.HttpContext.Request.Host = new HostString("127.0.0.1", 8500);
    }

    public void Dispose()
    {
        _db.Dispose();
        _conn.Dispose();
    }

    private static JsonElement Root(IActionResult result)
    {
        var ok = Assert.IsType<OkObjectResult>(result);
        return JsonDocument.Parse(JsonSerializer.Serialize(ok.Value)).RootElement;
    }

    [Fact]
    public async Task GetByDlrPDIReqNoWHHQ_BodyRong_TraHaiDanhSachRong()
    {
        // Gate :1809-1814 — luôn set Success và Data = { lst_Dlr_PDIRequest, lst_Dlr_PDIRequestDtl }.
        var root = Root(await _controller.GetByDlrPDIReqNoWHHQ(null));

        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.Equal(0, data.GetProperty("Lst_Dlr_PDIRequest").GetArrayLength());
        Assert.Equal(0, data.GetProperty("Lst_Dlr_PDIRequestDtl").GetArrayLength());
    }

    [Fact]
    public async Task GetByDlrPDIReqNoWHHQ_CoDlrPDIReqNo_VanTraRong()
    {
        // Gate :1722 — DlrPDIReqNo chỉ dùng dựng where clause cho DMS Sale (gate :3298-3301 like '%x%');
        // đích không có nguồn dữ liệu ngoài ⇒ rỗng. Đây là ca "lọc" duy nhất kiểm được ở đích.
        var root = Root(await _controller.GetByDlrPDIReqNoWHHQ(
            new DlrPDIRequest_GetByDlrPDIReqNoWHHQReq { DlrPDIReqNo = "  PDI0001  " }));

        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.Equal(0, data.GetProperty("Lst_Dlr_PDIRequest").GetArrayLength());
        Assert.Equal(0, data.GetProperty("Lst_Dlr_PDIRequestDtl").GetArrayLength());
    }

    [Fact]
    public async Task GetByDlrPDIReqNoWHHQ_BodyCoDuLieuKhac_VanKhongNemLoi()
    {
        // DTO chỉ có DlrPDIReqNo; body lạ không gây lỗi (gate cũng bỏ qua tham số khác).
        var root = Root(await _controller.GetByDlrPDIReqNoWHHQ(
            new DlrPDIRequest_GetByDlrPDIReqNoWHHQReq()));
        Assert.True(root.GetProperty("Success").GetBoolean());
    }
}
