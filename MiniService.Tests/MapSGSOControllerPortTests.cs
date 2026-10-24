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

public class MapSGSOControllerPortTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly MapSGSOControllerPort _controller;

    public MapSGSOControllerPortTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
        _db = new AppDbContext(opt, new TenantContext());
        _db.Database.EnsureCreated();
        new Sys_ObjectSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        new Sys_GroupSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        new Map_SG_SOSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        _controller = new MapSGSOControllerPort(_db);
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

    // ===== GetByGroupCodeDL =====

    [Fact]
    public async Task BodyRong_TraDayduMap()
    {
        // Body rỗng: lấy tất cả Map_SG_SO active (GRP001 BTN_CREATE, BTN_EDIT; GRP002 MENU_HOME).
        var req = new MapSGSOGetByGroupCodeDLReq();
        var root = Root(await _controller.GetByGroupCodeDL(req));
        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        var mapList = data.GetProperty("LstMapSGSO");
        Assert.Equal(3, mapList.GetArrayLength());
        // Kiểm dòng đầu: GRP001 BTN_CREATE
        Assert.Equal("GRP001", mapList[0].GetProperty("GroupCode").GetString());
        Assert.Equal("BTN_CREATE", mapList[0].GetProperty("ObjectCode").GetString());
    }

    [Fact]
    public async Task FilterGroupCodeGRP001_Tra2Dong()
    {
        // Filter GroupCode = "GRP001" → 2 dòng (BTN_CREATE, BTN_EDIT).
        var req = new MapSGSOGetByGroupCodeDLReq { GroupCode = "GRP001" };
        var root = Root(await _controller.GetByGroupCodeDL(req));
        var data = root.GetProperty("Data");
        var mapList = data.GetProperty("LstMapSGSO");
        Assert.Equal(2, mapList.GetArrayLength());
        Assert.Equal("GRP001", mapList[0].GetProperty("GroupCode").GetString());
        Assert.Equal("GRP001", mapList[1].GetProperty("GroupCode").GetString());
    }

    [Fact]
    public async Task ObjectNotMap_TraBTN_DELETE()
    {
        // Sys_ObjectNotMap: BTN_DELETE không map vào bất kỳ GRP nào (chỉ có GRP001 map BTN_CREATE/BTN_EDIT, GRP002 map MENU_HOME).
        var req = new MapSGSOGetByGroupCodeDLReq();
        var root = Root(await _controller.GetByGroupCodeDL(req));
        var data = root.GetProperty("Data");
        var notMapList = data.GetProperty("LstSysObjectNotMap");
        Assert.Equal(1, notMapList.GetArrayLength()); // Chỉ BTN_DELETE
        Assert.Equal("BTN_DELETE", notMapList[0].GetProperty("ObjectCode").GetString());
    }

    [Fact]
    public async Task FilterGroupCodeGRP002_TraMENUSong()
    {
        // Filter GroupCode = "GRP002" → 1 dòng (MENU_HOME).
        var req = new MapSGSOGetByGroupCodeDLReq { GroupCode = "GRP002" };
        var root = Root(await _controller.GetByGroupCodeDL(req));
        var data = root.GetProperty("Data");
        var mapList = data.GetProperty("LstMapSGSO");
        Assert.Equal(1, mapList.GetArrayLength());
        Assert.Equal("MENU_HOME", mapList[0].GetProperty("ObjectCode").GetString());
    }
}
