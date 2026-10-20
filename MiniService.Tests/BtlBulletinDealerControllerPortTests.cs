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
        new Btl_BulletinDtlSeeder().SeedAsync(_db).GetAwaiter().GetResult();
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

    // ---- SearchDL (Gate :35-272 -> Biz :100-261 Blt_Bulletin_GetByVin_DL) ----

    [Fact]
    public async Task SearchDL_BodyRong_TraLoiVINBatBuoc()
    {
        // Gate :159-169 — VIN rỗng ⇒ Btl_Bulletin_Dealer_SearchDL_InvalidBulletinNoNullOrEmpty.
        var bad = Assert.IsType<BadRequestObjectResult>(await _controller.SearchDL(null));
        var root = JsonDocument.Parse(JsonSerializer.Serialize(bad.Value)).RootElement;
        Assert.False(root.GetProperty("Success").GetBoolean());
        Assert.Equal("ErridNCarService.Btl_Bulletin_Dealer_SearchDL_InvalidBulletinNoNullOrEmpty",
            root.GetProperty("ErrorCode").GetString());
        Assert.Equal("Số VIN bắt buộc nhập", root.GetProperty("Params").GetProperty("Check.Description").GetString());
    }

    [Fact]
    public async Task SearchDL_VINChiCoKhoangTrang_CungTraLoiVINBatBuoc()
    {
        // Gate :134 — StdStr("   ") = null ⇒ cùng nhánh lỗi.
        var bad = Assert.IsType<BadRequestObjectResult>(
            await _controller.SearchDL(new BtlBulletinDealer_SearchDLReq { VIN = "   " }));
        var root = JsonDocument.Parse(JsonSerializer.Serialize(bad.Value)).RootElement;
        Assert.Equal("ErridNCarService.Btl_Bulletin_Dealer_SearchDL_InvalidBulletinNoNullOrEmpty",
            root.GetProperty("ErrorCode").GetString());
    }

    [Fact]
    public async Task SearchDL_VIN1_ChiTraDongPendingActiveConHan()
    {
        // Seed: VIN RLHXXAC001 có 4 dòng, chỉ BLT001 đủ Pending + Active + còn hạn.
        var root = Root(await _controller.SearchDL(new BtlBulletinDealer_SearchDLReq { VIN = "RLHXXAC001" }));

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
    public async Task SearchDL_VIN2_Ra1DongKhac()
    {
        // Bộ lọc thứ hai ra số dòng khác: VIN RLHXXTC002 ⇒ BLT006 (BLT005 Status DONE bị lọc).
        var root = Root(await _controller.SearchDL(new BtlBulletinDealer_SearchDLReq { VIN = "RLHXXTC002" }));
        var data = root.GetProperty("Data");
        Assert.Equal(1, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal("BLT006", data.GetProperty("DataList")[0].GetProperty("BulletinID").GetString());
    }

    [Fact]
    public async Task SearchDL_VINKhongCo_TraDanhSachRong()
    {
        // VIN không có dòng nào ⇒ DataList rỗng, ItemCount = 0, PageCount = 0.
        var root = Root(await _controller.SearchDL(new BtlBulletinDealer_SearchDLReq { VIN = "KHONGCO" }));
        var data = root.GetProperty("Data");
        Assert.Equal(0, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal(0, data.GetProperty("PageCount").GetInt32());
        Assert.Equal(0, data.GetProperty("DataList").GetArrayLength());
    }

    [Fact]
    public async Task SearchDL_PageSize0_DataListRongItemCountVanDung()
    {
        // Gate :155-156 — Ft_RecordCount = 0 ⇒ không lấy dòng nào; ItemCount vẫn là tổng.
        var root = Root(await _controller.SearchDL(new BtlBulletinDealer_SearchDLReq { VIN = "RLHXXAC001", Ft_PageSize = 0 }));
        var data = root.GetProperty("Data");
        Assert.Equal(1, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal(0, data.GetProperty("DataList").GetArrayLength());
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

    // ---- GetAllActiveByVINMapStatusP (Gate :757-999 -> Biz :100-261 Blt_Bulletin_GetByVin_DL) ----

    [Fact]
    public async Task GetAllActiveByVINMapStatusP_BodyRong_TraChiTietCuaBanTinPendingConHan()
    {
        // Gate :845 — VIN rỗng ⇒ bỏ mệnh đề VIN; gate :849-850 — Status = PENDING, FlagActive = '1'.
        // Gate :917-937 — bỏ bản tin hết hạn (BLT004).
        // Seed: bản tin PENDING + Active + còn hạn = BLT001, BLT006 ⇒ chi tiết BLTD001, BLTD002, BLTD005 = 3 dòng.
        var root = Root(await _controller.GetAllActiveByVINMapStatusP(null));

        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        var list = data.GetProperty("DataList");
        Assert.Equal(3, list.GetArrayLength());
        Assert.Equal("BLTD001", list[0].GetProperty("BulletinDtlID").GetString());
        Assert.Equal("BLTD002", list[1].GetProperty("BulletinDtlID").GetString());
        Assert.Equal("BLTD005", list[2].GetProperty("BulletinDtlID").GetString());
        // Gate :853-854 — pageIndex = 0, pageSize = 0.
        Assert.Equal(0, data.GetProperty("PageIndex").GetInt32());
        Assert.Equal(0, data.GetProperty("PageSize").GetInt32());
    }

    [Fact]
    public async Task GetAllActiveByVINMapStatusP_VIN1_ChiTraChiTietCuaVINDo()
    {
        // Gate :848 — VIN = @VIN; BLT001, BLT002 còn hạn ⇒ chi tiết BLTD001, BLTD002 (2 dòng).
        var root = Root(await _controller.GetAllActiveByVINMapStatusP(
            new BtlBulletinDealer_GetAllActiveByVINMapStatusPReq { VIN = "RLHXXAC001" }));
        var list = root.GetProperty("Data").GetProperty("DataList");
        Assert.Equal(2, list.GetArrayLength());
        Assert.Equal("BLTD001", list[0].GetProperty("BulletinDtlID").GetString());
        Assert.Equal("BLTD002", list[1].GetProperty("BulletinDtlID").GetString());
    }

    [Fact]
    public async Task GetAllActiveByVINMapStatusP_VIN2_RaSoDongKhac()
    {
        // Bộ lọc thứ hai ra số dòng khác: VIN RLHXXTC002 ⇒ BLT006 (PENDING, còn hạn) ⇒ chi tiết BLTD005 (1 dòng).
        var root = Root(await _controller.GetAllActiveByVINMapStatusP(
            new BtlBulletinDealer_GetAllActiveByVINMapStatusPReq { VIN = "RLHXXTC002" }));
        var list = root.GetProperty("Data").GetProperty("DataList");
        Assert.Equal(1, list.GetArrayLength());
        Assert.Equal("BLTD005", list[0].GetProperty("BulletinDtlID").GetString());
    }

    [Fact]
    public async Task GetAllActiveByVINMapStatusP_VINKhongCo_TraDanhSachRong()
    {
        // VIN không có dòng nào ⇒ DataList rỗng (PageInfo, không phải null).
        var root = Root(await _controller.GetAllActiveByVINMapStatusP(
            new BtlBulletinDealer_GetAllActiveByVINMapStatusPReq { VIN = "KHONGCO" }));
        Assert.True(root.GetProperty("Success").GetBoolean());
        Assert.Equal(0, root.GetProperty("Data").GetProperty("DataList").GetArrayLength());
    }

    [Fact]
    public async Task GetAllActiveByVINMapStatusP_BanTinHetHan_ChiTietBiBo()
    {
        // Gate :917-937 — BLT004 hết hạn ⇒ bị bỏ khỏi dtBulletinActive; gate :945-957 — chi tiết BLTD004 cũng bị bỏ.
        var root = Root(await _controller.GetAllActiveByVINMapStatusP(
            new BtlBulletinDealer_GetAllActiveByVINMapStatusPReq { VIN = "RLHXXAC001" }));
        var list = root.GetProperty("Data").GetProperty("DataList");
        Assert.DoesNotContain(list.EnumerateArray(), x => x.GetProperty("BulletinDtlID").GetString() == "BLTD004");
    }
}
