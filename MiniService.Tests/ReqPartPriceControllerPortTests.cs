using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MiniService.Controllers.Port;
using MiniService.Data;
using MiniService.Models.Port;
using Xunit;

namespace MiniService.Tests;

public class ReqPartPriceControllerPortTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly ReqPartPriceControllerPort _controller;

    public ReqPartPriceControllerPortTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
        _db = new AppDbContext(opt, new TenantContext());
        _db.Database.EnsureCreated();
        new Mst_DealerSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        new Mst_DeliveryFormSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        new Req_PartPriceSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        new Req_PartPriceDtlSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        _controller = new ReqPartPriceControllerPort(_db);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = null! }
        };
    }

    public void Dispose()
    {
        try { _db.Dispose(); } catch { }
        _conn.Dispose();
    }

    private static JsonElement Root(IActionResult result)
    {
        var ok = Assert.IsType<OkObjectResult>(result);
        return JsonDocument.Parse(JsonSerializer.Serialize(ok.Value)).RootElement;
    }

    private static (int rpp, int dtl) Counts(JsonElement data)
        => (data.GetProperty("Lst_Req_PartPrice").GetArrayLength(),
            data.GetProperty("Lst_Req_PartPriceDtl").GetArrayLength());

    [Fact]
    public async Task BodyRong_TraRpp3Dtl4_VaMapName()
    {
        var root = Root(await _controller.GetByReqPartPriceNoDL(null));
        Assert.True(root.GetProperty("Success").GetBoolean());
        var (rpp, dtl) = Counts(root.GetProperty("Data"));
        Assert.Equal(3, rpp);
        Assert.Equal(4, dtl);

        var list = root.GetProperty("Data").GetProperty("Lst_Req_PartPrice");
        Assert.Equal("PP001", list[0].GetProperty("ReqPartPriceNo").GetString());
        Assert.Equal("PP002", list[1].GetProperty("ReqPartPriceNo").GetString());
        Assert.Equal("PP003", list[2].GetProperty("ReqPartPriceNo").GetString());
        Assert.Equal(0, list[0].GetProperty("MyIdxSeq").GetInt32());
        Assert.Equal(2, list[2].GetProperty("MyIdxSeq").GetInt32());

        Assert.Equal("P", list[0].GetProperty("DMSReqPartPriceStatusName").GetString());
        Assert.Equal("A", list[1].GetProperty("DMSReqPartPriceStatusName").GetString());
        Assert.Equal("F", list[2].GetProperty("DMSReqPartPriceStatusName").GetString());
        Assert.Equal("Chờ duyệt", list[0].GetProperty("TSTReqPartPriceStatusName").GetString());
        Assert.Equal("Đã duyệt, chờ hoàn thiện", list[1].GetProperty("TSTReqPartPriceStatusName").GetString());
        Assert.Equal("Đã hoàn thiện", list[2].GetProperty("TSTReqPartPriceStatusName").GetString());

        Assert.Equal("2026-10-01", list[0].GetProperty("CreateDTime").GetString());
        Assert.Equal(JsonValueKind.Null, list[1].GetProperty("TSTSentDate").ValueKind);

        var dtlList = root.GetProperty("Data").GetProperty("Lst_Req_PartPriceDtl");
        var p001 = dtlList.EnumerateArray().First(x => x.GetProperty("DMSPartCode").GetString() == "P001");
        Assert.Equal("PP001", p001.GetProperty("ReqPartPriceNo").GetString());
        Assert.Equal("Giao hàng tận nơi", p001.GetProperty("DeliveryFormName").GetString());
    }

    [Fact]
    public async Task LocTheoSo_Dung1Rpp2Dtl()
    {
        var root = Root(await _controller.GetByReqPartPriceNoDL(
            new ReqPartPrice_GetByReqPartPriceNoDLReq { ReqPartPriceNo = "PP001" }));
        var (rpp, dtl) = Counts(root.GetProperty("Data"));
        Assert.Equal(1, rpp);
        Assert.Equal(2, dtl);
    }

    [Fact]
    public async Task LikeKhongWildcard_ChiPP_KhopTatCa()
    {
        var root = Root(await _controller.GetByReqPartPriceNoDL(
            new ReqPartPrice_GetByReqPartPriceNoDLReq { ReqPartPriceNo = "PP" }));
        var (rpp, _) = Counts(root.GetProperty("Data"));
        Assert.Equal(3, rpp);
    }

    [Fact]
    public async Task KhongKhop_TraRong()
    {
        var root = Root(await _controller.GetByReqPartPriceNoDL(
            new ReqPartPrice_GetByReqPartPriceNoDLReq { ReqPartPriceNo = "KHONGCO" }));
        var (rpp, dtl) = Counts(root.GetProperty("Data"));
        Assert.Equal(0, rpp);
        Assert.Equal(0, dtl);
    }

    [Fact]
    public async Task LoiDb_TraMaReq_PartPrice_Get_DL()
    {
        var conn2 = new SqliteConnection("DataSource=:memory:");
        conn2.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn2).Options;
        var dead = new AppDbContext(opt, new TenantContext());
        dead.Dispose();
        var bad = new ReqPartPriceControllerPort(dead);
        var result = await bad.GetByReqPartPriceNoDL(null);
        var bad400 = Assert.IsType<BadRequestObjectResult>(result);
        var json = JsonDocument.Parse(JsonSerializer.Serialize(bad400.Value)).RootElement;
        Assert.Equal("ErridNCarService.Req_PartPrice_Get_DL", json.GetProperty("ErrorCode").GetString());
        conn2.Dispose();
    }
}
