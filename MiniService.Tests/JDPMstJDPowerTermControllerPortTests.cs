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
/// Test port 1:1 JDPMstJDPowerTermController/SearchHQ (SQLite in-memory + seeder của module).
/// Nguồn: gate JDPMstJDPowerTermController.cs:42-258 -> BizCarSv.Master.cs:84169-84316 -> 84318-84453.
/// </summary>
public class JDPMstJDPowerTermControllerPortTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly JDPMstJDPowerTermControllerPort _controller;

    public JDPMstJDPowerTermControllerPortTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
        _db = new AppDbContext(opt, new TenantContext());
        _db.Database.EnsureCreated();
        new JDP_Mst_JDPowerTermSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        new JDP_Mst_JDPowerTermDtlSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        _controller = new JDPMstJDPowerTermControllerPort(_db);
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
    public async Task SearchHQ_BodyRong_TraTatCa3Dong()
    {
        // Gate :140/:146 — JDPTermCode/FlagActive rỗng ⇒ không mệnh đề lọc ⇒ 3 dòng seed.
        var root = Root(await _controller.SearchHQ(null));

        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.Equal(3, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal(1, data.GetProperty("PageCount").GetInt32());
        Assert.Equal(0, data.GetProperty("PageIndex").GetInt32());
        Assert.Equal(10, data.GetProperty("PageSize").GetInt32());

        var list = data.GetProperty("DataList");
        Assert.Equal(3, list.GetArrayLength());
        // Biz :84371/:84405 — order by JDPTermCode asc.
        Assert.Equal("JDP01", list[0].GetProperty("JDPTermCode").GetString());
        Assert.Equal("JDP03", list[2].GetProperty("JDPTermCode").GetString());
    }

    [Fact]
    public async Task SearchHQ_LocFlagActive1_Ra2Dong()
    {
        // Gate :146 — GenEqualCondition2("1") ⇒ FlagActive = '1' ⇒ JDP01, JDP02.
        var root = Root(await _controller.SearchHQ(new JDPMstJDPowerTerm_SearchHQReq { FlagActive = "1" }));
        var data = root.GetProperty("Data");
        Assert.Equal(2, data.GetProperty("ItemCount").GetInt32());
        var list = data.GetProperty("DataList");
        Assert.Equal(2, list.GetArrayLength());
        Assert.Equal("JDP01", list[0].GetProperty("JDPTermCode").GetString());
        Assert.Equal("JDP02", list[1].GetProperty("JDPTermCode").GetString());
    }

    [Fact]
    public async Task SearchHQ_LocFlagActive0_Ra1Dong()
    {
        // Bộ lọc thứ hai ra số dòng khác: FlagActive = '0' ⇒ JDP03.
        var root = Root(await _controller.SearchHQ(new JDPMstJDPowerTerm_SearchHQReq { FlagActive = "0" }));
        var data = root.GetProperty("Data");
        Assert.Equal(1, data.GetProperty("ItemCount").GetInt32());
        var list = data.GetProperty("DataList");
        Assert.Equal(1, list.GetArrayLength());
        Assert.Equal("JDP03", list[0].GetProperty("JDPTermCode").GetString());
    }

    [Fact]
    public async Task SearchHQ_LocJDPTermCode_ChuaChuoi_Ra1Dong()
    {
        // Gate :140 — GenLikeCondition2Percent ⇒ like '%02%' (không phân biệt hoa/thường như SQL Server).
        var root = Root(await _controller.SearchHQ(new JDPMstJDPowerTerm_SearchHQReq { JDPTermCode = " jdp02 " }));
        var data = root.GetProperty("Data");
        Assert.Equal(1, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal("JDP02", data.GetProperty("DataList")[0].GetProperty("JDPTermCode").GetString());
    }

    [Fact]
    public async Task SearchHQ_PhanTrang_PageSize1_PageIndex1_Ra1Dong()
    {
        // Gate :159-160 + Biz :84353-84354 — RecordStart = 1*1 = 1 ⇒ MyRowIdx 2..2 ⇒ JDP02.
        var root = Root(await _controller.SearchHQ(new JDPMstJDPowerTerm_SearchHQReq { Ft_PageIndex = 1, Ft_PageSize = 1 }));
        var data = root.GetProperty("Data");
        Assert.Equal(3, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal(3, data.GetProperty("PageCount").GetInt32());
        var list = data.GetProperty("DataList");
        Assert.Equal(1, list.GetArrayLength());
        Assert.Equal("JDP02", list[0].GetProperty("JDPTermCode").GetString());
    }

    [Fact]
    public async Task SearchHQ_PageSize0_DataListRong_ItemCountVanLaTong()
    {
        // Gate :216-220 — pageSize = 0 ⇒ DataList rỗng, PageCount = 0, ItemCount vẫn là tổng (bài học A.7).
        var root = Root(await _controller.SearchHQ(new JDPMstJDPowerTerm_SearchHQReq { Ft_PageSize = 0 }));
        var data = root.GetProperty("Data");
        Assert.Equal(3, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal(0, data.GetProperty("PageCount").GetInt32());
        Assert.Equal(0, data.GetProperty("DataList").GetArrayLength());
    }

    // ---------------------------------------------------------------------------------------------
    // SearchWHHQ — gate JDPMstJDPowerTermController.cs:260-476 -> BizCarSv.Master.cs:84458-84604
    //   -> 84606-84739 (JDP_Mst_JDPowerTerm_Get_WHX). Cùng bảng JDP_Mst_JDPowerTerm, cùng 3 dòng seed.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public async Task SearchWHHQ_BodyRong_TraTatCa3Dong()
    {
        // Gate :358/:364 — JDPTermCode/FlagActive rỗng ⇒ không mệnh đề lọc ⇒ 3 dòng seed.
        var root = Root(await _controller.SearchWHHQ(null));

        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.Equal(3, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal(1, data.GetProperty("PageCount").GetInt32());
        Assert.Equal(0, data.GetProperty("PageIndex").GetInt32());
        Assert.Equal(10, data.GetProperty("PageSize").GetInt32());

        var list = data.GetProperty("DataList");
        Assert.Equal(3, list.GetArrayLength());
        // Biz :84659/:84692 — order by JDPTermCode asc.
        Assert.Equal("JDP01", list[0].GetProperty("JDPTermCode").GetString());
        Assert.Equal("JDP03", list[2].GetProperty("JDPTermCode").GetString());
    }

    [Fact]
    public async Task SearchWHHQ_LocFlagActive1_Ra2Dong()
    {
        // Gate :364 — GenEqualCondition2("1") ⇒ FlagActive = '1' ⇒ JDP01, JDP02.
        var root = Root(await _controller.SearchWHHQ(new JDPMstJDPowerTerm_SearchWHHQReq { FlagActive = "1" }));
        var data = root.GetProperty("Data");
        Assert.Equal(2, data.GetProperty("ItemCount").GetInt32());
        var list = data.GetProperty("DataList");
        Assert.Equal(2, list.GetArrayLength());
        Assert.Equal("JDP01", list[0].GetProperty("JDPTermCode").GetString());
        Assert.Equal("JDP02", list[1].GetProperty("JDPTermCode").GetString());
    }

    [Fact]
    public async Task SearchWHHQ_LocFlagActive0_Ra1Dong()
    {
        // Bộ lọc thứ hai ra số dòng khác: FlagActive = '0' ⇒ JDP03.
        var root = Root(await _controller.SearchWHHQ(new JDPMstJDPowerTerm_SearchWHHQReq { FlagActive = "0" }));
        var data = root.GetProperty("Data");
        Assert.Equal(1, data.GetProperty("ItemCount").GetInt32());
        var list = data.GetProperty("DataList");
        Assert.Equal(1, list.GetArrayLength());
        Assert.Equal("JDP03", list[0].GetProperty("JDPTermCode").GetString());
    }

    [Fact]
    public async Task SearchWHHQ_LocJDPTermCode_ChuaChuoi_Ra1Dong()
    {
        // Gate :358 — GenLikeCondition2Percent ⇒ like '%02%' (không phân biệt hoa/thường như SQL Server).
        var root = Root(await _controller.SearchWHHQ(new JDPMstJDPowerTerm_SearchWHHQReq { JDPTermCode = " jdp02 " }));
        var data = root.GetProperty("Data");
        Assert.Equal(1, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal("JDP02", data.GetProperty("DataList")[0].GetProperty("JDPTermCode").GetString());
    }

    [Fact]
    public async Task SearchWHHQ_PhanTrang_PageSize1_PageIndex1_Ra1Dong()
    {
        // Gate :377-378 + Biz :84641-84642 — RecordStart = 1*1 = 1 ⇒ MyRowIdx 2..2 ⇒ JDP02.
        var root = Root(await _controller.SearchWHHQ(new JDPMstJDPowerTerm_SearchWHHQReq { Ft_PageIndex = 1, Ft_PageSize = 1 }));
        var data = root.GetProperty("Data");
        Assert.Equal(3, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal(3, data.GetProperty("PageCount").GetInt32());
        var list = data.GetProperty("DataList");
        Assert.Equal(1, list.GetArrayLength());
        Assert.Equal("JDP02", list[0].GetProperty("JDPTermCode").GetString());
    }

    [Fact]
    public async Task SearchWHHQ_PageSize0_DataListRong_ItemCountVanLaTong()
    {
        // Gate :434-441 — pageSize = 0 ⇒ DataList rỗng, PageCount = 0, ItemCount vẫn là tổng (bài học A.7).
        var root = Root(await _controller.SearchWHHQ(new JDPMstJDPowerTerm_SearchWHHQReq { Ft_PageSize = 0 }));
        var data = root.GetProperty("Data");
        Assert.Equal(3, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal(0, data.GetProperty("PageCount").GetInt32());
        Assert.Equal(0, data.GetProperty("DataList").GetArrayLength());
    }

    // ---------------------------------------------------------------------------------------------
    // GetByJDPTermCodeHQ — gate JDPMstJDPowerTermController.cs:478-691 -> BizCarSv.Master.cs:84169-84316
    //   -> 84318-84453 (JDP_Mst_JDPowerTerm_GetX). Trả 2 danh sách: kỳ + chi tiết (JDP_Mst_JDPowerTermDtl).
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public async Task GetByJDPTermCodeHQ_BodyRong_TraHaiDanhSachRong()
    {
        // Gate :564/:567 — JDPTermCode rỗng ⇒ không mệnh đề lọc.
        // Gate :576-577 + Biz :84353-84354 — Ft_RecordCount của BaseController KHÔNG có trong excerpt (GAP, dùng 0)
        //   ⇒ nFilterRecordEnd = 0 ⇒ `MyRowIdx <= 0` ⇒ 0 dòng; hai danh sách khởi tạo rỗng (gate :579-580) ⇒ Data = 2 mảng rỗng.
        var root = Root(await _controller.GetByJDPTermCodeHQ(null));

        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.Equal(0, data.GetProperty("lst_JDP_Mst_JDPowerTerm").GetArrayLength());
        Assert.Equal(0, data.GetProperty("lst_JDP_Mst_JDPowerTermDtl").GetArrayLength());
    }

    [Fact]
    public async Task GetByJDPTermCodeHQ_LocJDP02_TraHaiDanhSachRong()
    {
        // Gate :567 — GenLikeCondition2Percent ⇒ like '%02%' (không phân biệt hoa/thường như SQL Server).
        // Cùng GAP Ft_RecordCount = 0 ⇒ nguồn trả 0 dòng cho cả hai danh sách.
        var root = Root(await _controller.GetByJDPTermCodeHQ(new JDPMstJDPowerTerm_GetByJDPTermCodeHQReq { JDPTermCode = " jdp02 " }));
        var data = root.GetProperty("Data");
        Assert.Equal(0, data.GetProperty("lst_JDP_Mst_JDPowerTerm").GetArrayLength());
        Assert.Equal(0, data.GetProperty("lst_JDP_Mst_JDPowerTermDtl").GetArrayLength());
    }

    [Fact]
    public async Task GetByJDPTermCodeHQ_KhongCoKy_TraHaiDanhSachRong()
    {
        // Gate :579-580 — hai danh sách khởi tạo rỗng; không khớp dòng nào ⇒ Data = 2 mảng rỗng (không null).
        var root = Root(await _controller.GetByJDPTermCodeHQ(new JDPMstJDPowerTerm_GetByJDPTermCodeHQReq { JDPTermCode = "KHONGCO" }));
        var data = root.GetProperty("Data");
        Assert.Equal(0, data.GetProperty("lst_JDP_Mst_JDPowerTerm").GetArrayLength());
        Assert.Equal(0, data.GetProperty("lst_JDP_Mst_JDPowerTermDtl").GetArrayLength());
    }
}
