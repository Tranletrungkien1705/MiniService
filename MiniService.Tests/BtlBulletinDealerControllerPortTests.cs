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

/// <summary>
/// Test port 1:1 BtlBulletinDealerController/SearchWHDL (SQLite in-memory + seeder của module).
/// Nguồn: gate BtlBulletinDealerController.cs:273-512 -> BizCarSv.Bulletin.cs:262-423 (Blt_Bulletin_GetByVin_WHDL).
/// </summary>
public class BtlBulletinDealerControllerPortTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly BtlBulletinDealerControllerPort _controller;

    public BtlBulletinDealerControllerPortTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
        _db = new AppDbContext(opt, new TenantContext());
        _db.Database.EnsureCreated();
        new Btl_BulletinSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        _controller = new BtlBulletinDealerControllerPort(_db);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = null! }
        };
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
    public async Task SearchWHDL_BodyRong_TraLoiVINBatBuoc()
    {
        // Gate :397-407 — VIN rỗng ⇒ Btl_Bulletin_Dealer_SearchWHDL_InvalidBulletinNoNullOrEmpty.
        var bad = Assert.IsType<BadRequestObjectResult>(await _controller.SearchWHDL(null));
        var root = JsonDocument.Parse(JsonSerializer.Serialize(bad.Value)).RootElement;
        Assert.False(root.GetProperty("Success").GetBoolean());
        Assert.Equal("ErridNCarService.Btl_Bulletin_Dealer_SearchWHDL_InvalidBulletinNoNullOrEmpty",
            root.GetProperty("ErrorCode").GetString());
        Assert.Equal("Số VIN bắt buộc nhập", root.GetProperty("Params").GetProperty("Check.Description").GetString());
    }

    [Fact]
    public async Task SearchWHDL_VINChiCoKhoangTrang_CungTraLoiVINBatBuoc()
    {
        // Gate :372 — StdStr("   ") = null ⇒ cùng nhánh lỗi.
        var bad = Assert.IsType<BadRequestObjectResult>(
            await _controller.SearchWHDL(new BtlBulletinDealer_SearchWHDLReq { VIN = "   " }));
        var root = JsonDocument.Parse(JsonSerializer.Serialize(bad.Value)).RootElement;
        Assert.Equal("ErridNCarService.Btl_Bulletin_Dealer_SearchWHDL_InvalidBulletinNoNullOrEmpty",
            root.GetProperty("ErrorCode").GetString());
    }

    [Fact]
    public async Task SearchWHDL_VIN1_ChiTraDongPendingActiveConHan()
    {
        // Seed: VIN RLHXXAC001 có 4 dòng, chỉ BLT001 đủ Pending + Active + còn hạn.
        var root = Root(await _controller.SearchWHDL(new BtlBulletinDealer_SearchWHDLReq { VIN = "RLHXXAC001" }));

        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.Equal(1, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal(1, data.GetProperty("PageCount").GetInt32());
        Assert.Equal(0, data.GetProperty("PageIndex").GetInt32());
        Assert.Equal(10, data.GetProperty("PageSize").GetInt32());
        var list = data.GetProperty("DataList");
        Assert.Equal(1, list.GetArrayLength());
        Assert.Equal("BLT001", list[0].GetProperty("BulletinID").GetString());
    }

    [Fact]
    public async Task SearchWHDL_VIN2_Ra1DongKhac()
    {
        // Bộ lọc thứ hai ra số dòng khác: VIN RLHXXTC002 ⇒ BLT006 (BLT005 Status DONE bị lọc).
        var root = Root(await _controller.SearchWHDL(new BtlBulletinDealer_SearchWHDLReq { VIN = "RLHXXTC002" }));
        var data = root.GetProperty("Data");
        Assert.Equal(1, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal("BLT006", data.GetProperty("DataList")[0].GetProperty("BulletinID").GetString());
    }

    [Fact]
    public async Task SearchWHDL_VINKhongCo_TraDanhSachRong()
    {
        // VIN không có dòng nào ⇒ DataList rỗng, ItemCount = 0, PageCount = 0.
        var root = Root(await _controller.SearchWHDL(new BtlBulletinDealer_SearchWHDLReq { VIN = "KHONGCO" }));
        var data = root.GetProperty("Data");
        Assert.Equal(0, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal(0, data.GetProperty("PageCount").GetInt32());
        Assert.Equal(0, data.GetProperty("DataList").GetArrayLength());
    }

    [Fact]
    public async Task SearchWHDL_PageSize0_DataListRongItemCountVanDung()
    {
        // Gate :393-394 — Ft_RecordCount = 0 ⇒ không lấy dòng nào; ItemCount vẫn là tổng.
        var root = Root(await _controller.SearchWHDL(new BtlBulletinDealer_SearchWHDLReq { VIN = "RLHXXAC001", Ft_PageSize = 0 }));
        var data = root.GetProperty("Data");
        Assert.Equal(1, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal(0, data.GetProperty("DataList").GetArrayLength());
    }
}
