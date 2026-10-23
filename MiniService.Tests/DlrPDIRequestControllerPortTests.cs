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
/// Test port 1:1 DlrPDIRequestController:
/// - GetByDlrPDIReqNoWHHQ (#42)
/// - GetROForCreateDL (#43)
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
        if (result is OkObjectResult ok)
        {
            return JsonDocument.Parse(JsonSerializer.Serialize(ok.Value)).RootElement;
        }
        if (result is ObjectResult obj)
        {
            return JsonDocument.Parse(JsonSerializer.Serialize(obj.Value)).RootElement;
        }
        throw new InvalidOperationException("Unsupported action result type");
    }

    [Fact]
    public async Task GetByDlrPDIReqNoWHHQ_BodyRong_TraHaiDanhSachRong()
    {
        var root = Root(await _controller.GetByDlrPDIReqNoWHHQ(null));

        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.Equal(0, data.GetProperty("Lst_Dlr_PDIRequest").GetArrayLength());
        Assert.Equal(0, data.GetProperty("Lst_Dlr_PDIRequestDtl").GetArrayLength());
    }

    [Fact]
    public async Task GetByDlrPDIReqNoWHHQ_CoDlrPDIReqNo_VanTraRong()
    {
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
        var root = Root(await _controller.GetByDlrPDIReqNoWHHQ(
            new DlrPDIRequest_GetByDlrPDIReqNoWHHQReq()));
        Assert.True(root.GetProperty("Success").GetBoolean());
    }

    [Fact]
    public async Task GetROForCreateDL_BodyRong_TraLoiDlrPDIReqNoEmpty()
    {
        var root = Root(await _controller.GetROForCreateDL(null));
        Assert.False(root.GetProperty("Success").GetBoolean());
        Assert.Contains("DlrPDIRequest_GetROForCreateDL_DlrPDIReqNoEmpty", root.GetProperty("ErrorCode").GetString()!);
    }

    [Fact]
    public async Task GetROForCreateDL_ThieuVIN_TraLoiVINEmpty()
    {
        var root = Root(await _controller.GetROForCreateDL(new DlrPDIRequest_GetROForCreateDLReq
        {
            DlrPDIReqNo = "PDI001",
            VIN = ""
        }));
        Assert.False(root.GetProperty("Success").GetBoolean());
        Assert.Contains("DlrPDIRequest_GetROForCreateDL_VINEmpty", root.GetProperty("ErrorCode").GetString()!);
    }

    [Fact]
    public async Task GetROForCreateDL_DuThamSo_DMSKhongCo_TraLoiDlrPDIReqNoNotFound()
    {
        var root = Root(await _controller.GetROForCreateDL(new DlrPDIRequest_GetROForCreateDLReq
        {
            DlrPDIReqNo = "PDI001",
            VIN = "VIN123456789"
        }));
        Assert.False(root.GetProperty("Success").GetBoolean());
        Assert.Contains("DlrPDIRequest_GetROForCreateDL_DlrPDIReqNoNotFound", root.GetProperty("ErrorCode").GetString()!);
    }
}
