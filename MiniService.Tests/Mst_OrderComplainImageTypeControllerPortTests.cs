using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MiniService.Controllers.Port;
using MiniService.Data;
using MiniService.Models.Port;
using Xunit;

namespace MiniService.Tests;

/// <summary>
/// Test port 1:1 Mst_OrderComplainImageTypeController/GetAllActive (SQLite in-memory + seeder của module).
/// Nguồn: gate Mst_OrderComplainImageTypeController.cs:253-436 -> BizCarSv.Master.cs:60804-61087.
/// </summary>
public class Mst_OrderComplainImageTypeControllerPortTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly Mst_OrderComplainImageTypeControllerPort _controller;

    public Mst_OrderComplainImageTypeControllerPortTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
        _db = new AppDbContext(opt, new TenantContext());
        _db.Database.EnsureCreated();
        new Mst_OrderComplainImageTypeSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        _controller = new Mst_OrderComplainImageTypeControllerPort(_db);
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
    public async Task GetAllActive_BodyRong_TraChiDongFlagActive1()
    {
        // Seed 5 dòng: IMG01..IMG03 FlagActive = "1", IMG04..IMG05 = "0" ⇒ nguồn lọc FlagActive = '1' ⇒ 3 dòng.
        var root = Root(await _controller.GetAllActive(null));

        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.Equal(3, data.GetProperty("DataList").GetArrayLength());
        Assert.Equal(0, data.GetProperty("PageIndex").GetInt32());
        Assert.Equal(0, data.GetProperty("PageSize").GetInt32());

        var list = data.GetProperty("DataList");
        Assert.Equal("IMG01", list[0].GetProperty("OrderComplainImageType").GetString());
        Assert.Equal("IMG02", list[1].GetProperty("OrderComplainImageType").GetString());
        Assert.Equal("IMG03", list[2].GetProperty("OrderComplainImageType").GetString());
        // Biz :61003 — identity(bigint, 0, 1) ⇒ MyIdxSeq 0,1,2.
        Assert.Equal(0, list[0].GetProperty("MyIdxSeq").GetInt64());
        Assert.Equal(2, list[2].GetProperty("MyIdxSeq").GetInt64());
        // Cột tên giữ nguyên văn OrderComplainImageName.
        Assert.Equal("Ảnh toàn xe", list[0].GetProperty("OrderComplainImageName").GetString());
    }

    [Fact]
    public async Task GetAllActive_BodyCoDuLieu_VanBoQuaThamSo()
    {
        // Gate :253 — GetAllActive() không nhận tham số ⇒ body có dữ liệu cũng không đổi kết quả.
        var root = Root(await _controller.GetAllActive(new { FlagActive = "0" }));
        Assert.Equal(3, root.GetProperty("Data").GetProperty("DataList").GetArrayLength());
    }

    [Fact]
    public async Task GetAllActive_KhongCoDongActive_TraDanhSachRong()
    {
        // Bộ lọc thứ hai: tắt hết FlagActive ⇒ 0 dòng (khác 3 dòng của ca mặc định).
        foreach (var row in await _db.Set<Mst_OrderComplainImageType>().ToListAsync())
            row.FlagActive = "0";
        await _db.SaveChangesAsync();

        var root = Root(await _controller.GetAllActive(null));
        Assert.True(root.GetProperty("Success").GetBoolean());
        Assert.Equal(0, root.GetProperty("Data").GetProperty("DataList").GetArrayLength());
    }
}
