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

public class MstSerAppTypeControllerPortTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly MstSerAppTypeControllerPort _controller;

    public MstSerAppTypeControllerPortTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
        _db = new AppDbContext(opt, new TenantContext());
        _db.Database.EnsureCreated();
        new Mst_Ser_AppTypeSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        _controller = new MstSerAppTypeControllerPort(_db);
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

    // ===== GetAllActive =====

    [Fact]
    public async Task BodyRong_Tra4Dong()
    {
        // Gate:125-130 body rỗng, pageIndex=0, pageSize=0.
        // Biz: SELECT * order by AppTypeCode asc (không lọc FlagActive, OrgID, NetworkID).
        var root = Root(await _controller.GetAllActive(null));
        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.Equal(4, data.GetArrayLength());

        // Kiểm tra thứ tự AppTypeCode asc (biz:428-429).
        Assert.Equal("APP_A", data[0].GetProperty("AppTypeCode").GetString());
        Assert.Equal("APP_B", data[1].GetProperty("AppTypeCode").GetString());
        Assert.Equal("APP_C", data[2].GetProperty("AppTypeCode").GetString());
        Assert.Equal("APP_D", data[3].GetProperty("AppTypeCode").GetString());

        // Kiểm tra MyIdxSeq (0-based, biz:422, 468).
        Assert.Equal(0, data[0].GetProperty("MyIdxSeq").GetInt64());
        Assert.Equal(1, data[1].GetProperty("MyIdxSeq").GetInt64());
        Assert.Equal(2, data[2].GetProperty("MyIdxSeq").GetInt64());
        Assert.Equal(3, data[3].GetProperty("MyIdxSeq").GetInt64());
    }

    [Fact]
    public async Task BangRong_TraNull()
    {
        // Gate :125-130 pageInfo.DataList = new List. Nếu bảng rỗng, Data = null (A.5).
        // Xoá toàn bộ dữ liệu.
        var set = _db.Set<Mst_Ser_AppType>();
        set.RemoveRange(await set.ToListAsync());
        await _db.SaveChangesAsync();

        var root = Root(await _controller.GetAllActive(null));
        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.True(data.ValueKind == JsonValueKind.Null);
    }
}
