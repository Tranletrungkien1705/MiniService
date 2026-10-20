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

    // ---- GetAllActiveByVIN (Gate :513-756 -> Biz :100-261 Blt_Bulletin_GetByVin_DL) ----

    [Fact]
    public async Task GetAllActiveByVIN_BodyRong_TraTatCaBanTinConHan()
    {
        // Gate :601 — VIN rỗng ⇒ strVINConditionList rỗng ⇒ chỉ lọc FlagActive = '1' (gate :606).
        // Gate :680-693 — bỏ dòng DateExpired rỗng / "0001-01-01" / đã hết hạn.
        // Seed: BLT001, BLT002, BLT005, BLT006, BLT007 có DateExpired = mai; BLT003 FlagActive = 0, BLT004 hết hạn ⇒ 5 dòng.
        var root = Root(await _controller.GetAllActiveByVIN(null));

        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.Equal(5, data.GetProperty("DataList").GetArrayLength());
        // Gate :609-610 — pageIndex = 0, pageSize = 0.
        Assert.Equal(0, data.GetProperty("PageIndex").GetInt32());
        Assert.Equal(0, data.GetProperty("PageSize").GetInt32());
    }

    [Fact]
    public async Task GetAllActiveByVIN_VIN1_ChiTraBanTinCuaVINConHan()
    {
        // Gate :604 — VIN = @VIN; BLT003 (FlagActive = 0) và BLT004 (hết hạn) bị bỏ ⇒ 2 dòng (BLT001, BLT002).
        var root = Root(await _controller.GetAllActiveByVIN(new BtlBulletinDealer_GetAllActiveByVINReq { VIN = "RLHXXAC001" }));
        var list = root.GetProperty("Data").GetProperty("DataList");
        Assert.Equal(2, list.GetArrayLength());
        Assert.Equal("BLT001", list[0].GetProperty("BulletinID").GetString());
        Assert.Equal("BLT002", list[1].GetProperty("BulletinID").GetString());
    }

    [Fact]
    public async Task GetAllActiveByVIN_VIN2_Ra1DongKhac()
    {
        // Bộ lọc thứ hai ra số dòng khác: VIN RLHXXTC002 ⇒ BLT005, BLT006 (2 dòng).
        var root = Root(await _controller.GetAllActiveByVIN(new BtlBulletinDealer_GetAllActiveByVINReq { VIN = "RLHXXTC002" }));
        var list = root.GetProperty("Data").GetProperty("DataList");
        Assert.Equal(2, list.GetArrayLength());
        Assert.Equal("BLT005", list[0].GetProperty("BulletinID").GetString());
        Assert.Equal("BLT006", list[1].GetProperty("BulletinID").GetString());
    }

    [Fact]
    public async Task GetAllActiveByVIN_VINKhongCo_TraDanhSachRong()
    {
        // VIN không có dòng nào ⇒ DataList rỗng (PageInfo, không phải null).
        var root = Root(await _controller.GetAllActiveByVIN(new BtlBulletinDealer_GetAllActiveByVINReq { VIN = "KHONGCO" }));
        Assert.True(root.GetProperty("Success").GetBoolean());
        Assert.Equal(0, root.GetProperty("Data").GetProperty("DataList").GetArrayLength());
    }

    [Fact]
    public async Task GetAllActiveByVIN_DateExpiredRong_HoacMinValue_BiBo()
    {
        // Gate :681 — DateExpired rỗng hoặc "0001-01-01" ⇒ KHÔNG vào dtBulletinActive (khác SearchWHDL).
        // BLT001 (rỗng) và BLT002 ("0001-01-01") bị bỏ; BLT003 FlagActive = 0 cũng bị lọc ⇒ 0 dòng.
        var row = await _db.Set<Btl_Bulletin>().FirstAsync(x => x.BulletinID == "BLT001");
        row.DateExpired = null;
        var row2 = await _db.Set<Btl_Bulletin>().FirstAsync(x => x.BulletinID == "BLT002");
        row2.DateExpired = "0001-01-01";
        await _db.SaveChangesAsync();

        var root = Root(await _controller.GetAllActiveByVIN(new BtlBulletinDealer_GetAllActiveByVINReq { VIN = "RLHXXAC001" }));
        var list = root.GetProperty("Data").GetProperty("DataList");
        Assert.Equal(0, list.GetArrayLength());
    }
}
