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

public class CarWarrantyInformationControllerPortTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly CarWarrantyInformationControllerPort _controller;

    public CarWarrantyInformationControllerPortTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
        _db = new AppDbContext(opt, new TenantContext());
        _db.Database.EnsureCreated();
        new Ser_MST_ROWarrantyRenewalCategorySeeder().SeedAsync(_db).GetAwaiter().GetResult();
        new Ser_MST_ROWarrantyRenewalSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        _controller = new CarWarrantyInformationControllerPort(_db);
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

    private static int DtlCount(JsonElement root)
        => root.GetProperty("Data").GetProperty("Lst_Car_Warranty_InformationDtl").GetArrayLength();

    // ===== #18 GetWHByPlateNoVIN =====

    [Fact]
    public async Task BodyRong_Tra4Dtl()
    {
        var root = Root(await _controller.GetWHByPlateNoVIN(null));
        Assert.True(root.GetProperty("Success").GetBoolean());
        Assert.Equal(4, DtlCount(root));
        Assert.Equal(0, root.GetProperty("Data").GetProperty("Lst_Car_Warranty_Information").GetArrayLength());
    }

    [Fact]
    public async Task LocVinRLHXXAC001_2Dtl()
    {
        var root = Root(await _controller.GetWHByPlateNoVIN(
            new CarWarrantyInformation_GetWHByPlateNoVINReq { VIN = "RLHXXAC001" }));
        Assert.Equal(2, DtlCount(root));
        var list = root.GetProperty("Data").GetProperty("Lst_Car_Warranty_InformationDtl");
        Assert.Equal("Gia hạn bảo hành 1 năm", list[0].GetProperty("WrtReneCateName").GetString());
    }

    [Fact]
    public async Task LocVinRLHXXAC002_1Dtl()
    {
        var root = Root(await _controller.GetWHByPlateNoVIN(
            new CarWarrantyInformation_GetWHByPlateNoVINReq { VIN = "RLHXXAC002" }));
        Assert.Equal(1, DtlCount(root));
    }

    [Fact]
    public async Task LocVinTienTo_3Dtl()
    {
        var root = Root(await _controller.GetWHByPlateNoVIN(
            new CarWarrantyInformation_GetWHByPlateNoVINReq { VIN = "RLHXXAC" }));
        Assert.Equal(3, DtlCount(root));
    }

    [Fact]
    public async Task KhongKhop_TraRong()
    {
        var root = Root(await _controller.GetWHByPlateNoVIN(
            new CarWarrantyInformation_GetWHByPlateNoVINReq { VIN = "KHONGCO" }));
        Assert.Equal(0, DtlCount(root));
    }

    [Fact]
    public async Task LoiDb_TraMaRpt_DMSSer()
    {
        var conn2 = new SqliteConnection("DataSource=:memory:");
        conn2.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn2).Options;
        var dead = new AppDbContext(opt, new TenantContext());
        dead.Dispose();
        var bad = new CarWarrantyInformationControllerPort(dead);
        var result = await bad.GetWHByPlateNoVIN(null);
        var bad400 = Assert.IsType<BadRequestObjectResult>(result);
        var json = JsonDocument.Parse(JsonSerializer.Serialize(bad400.Value)).RootElement;
        Assert.Equal("ErridNCarService.Rpt_DMSSer_Car_Warranty_Information_WH",
            json.GetProperty("ErrorCode").GetString());
        conn2.Dispose();
    }

    // ===== #19 GetByPlateNoVIN =====

    [Fact]
    public async Task GetByPlateNoVIN_BodyRong_Tra4Dtl()
    {
        var root = Root(await _controller.GetByPlateNoVIN(null));
        Assert.True(root.GetProperty("Success").GetBoolean());
        Assert.Equal(4, DtlCount(root));
        Assert.Equal(0, root.GetProperty("Data").GetProperty("Lst_Car_Warranty_Information").GetArrayLength());
    }

    [Fact]
    public async Task GetByPlateNoVIN_LocVinRLHXXAC001_2Dtl()
    {
        var root = Root(await _controller.GetByPlateNoVIN(
            new CarWarrantyInformation_GetByPlateNoVINReq { VIN = "RLHXXAC001" }));
        Assert.Equal(2, DtlCount(root));
        var list = root.GetProperty("Data").GetProperty("Lst_Car_Warranty_InformationDtl");
        Assert.Equal("Gia hạn bảo hành 1 năm", list[0].GetProperty("WrtReneCateName").GetString());
    }

    [Fact]
    public async Task GetByPlateNoVIN_LocVinTienTo_3Dtl()
    {
        var root = Root(await _controller.GetByPlateNoVIN(
            new CarWarrantyInformation_GetByPlateNoVINReq { VIN = "RLHXXAC" }));
        Assert.Equal(3, DtlCount(root));
    }

    [Fact]
    public async Task GetByPlateNoVIN_KhongKhop_TraRong()
    {
        var root = Root(await _controller.GetByPlateNoVIN(
            new CarWarrantyInformation_GetByPlateNoVINReq { VIN = "KHONGCO" }));
        Assert.Equal(0, DtlCount(root));
    }

    [Fact]
    public async Task GetByPlateNoVIN_LoiDb_TraMaRpt_DMSSer_KhongWh()
    {
        var conn2 = new SqliteConnection("DataSource=:memory:");
        conn2.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn2).Options;
        var dead = new AppDbContext(opt, new TenantContext());
        dead.Dispose();
        var bad = new CarWarrantyInformationControllerPort(dead);
        var result = await bad.GetByPlateNoVIN(null);
        var bad400 = Assert.IsType<BadRequestObjectResult>(result);
        var json = JsonDocument.Parse(JsonSerializer.Serialize(bad400.Value)).RootElement;
        Assert.Equal("ErridNCarService.Rpt_DMSSer_Car_Warranty_Information",
            json.GetProperty("ErrorCode").GetString());
        conn2.Dispose();
    }
}
