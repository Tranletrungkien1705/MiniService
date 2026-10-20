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
/// Test port 1:1 BtlBulletinDealerController/GetForRO (SQLite in-memory + seeder của module).
/// Nguồn: gate BtlBulletinDealerController.cs:1001-1467 -> BizCarSv.Bulletin.cs:100-261 (Blt_Bulletin_GetByVin_DL)
///        + BizCarSv.Service01.cs:7451-7585 (Ser_RO_Get_DL) + BizCarSv.WarrantyReport.cs:303-455 (Ser_ROWarrantyReport_GetWHDL).
/// </summary>
public class BtlBulletinDealerGetForROTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly BtlBulletinDealerControllerPort _controller;

    public BtlBulletinDealerGetForROTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
        _db = new AppDbContext(opt, new TenantContext());
        _db.Database.EnsureCreated();
        new Btl_BulletinSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        new Btl_BulletinDtlSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        new Ser_ROSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        new Ser_ROWarrantyReportServiceItemsSeeder().SeedAsync(_db).GetAwaiter().GetResult();
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

    private static JsonElement Bad(IActionResult result)
    {
        var bad = Assert.IsType<BadRequestObjectResult>(result);
        return JsonDocument.Parse(JsonSerializer.Serialize(bad.Value)).RootElement;
    }

    [Fact]
    public async Task BodyRong_TraLoiVINEmpty()
    {
        // Gate :1091-1102 — VIN rỗng ⇒ BtlBulletinDealerGetForRO_VINEmpty.
        var root = Bad(await _controller.GetForRO(null));
        Assert.False(root.GetProperty("Success").GetBoolean());
        Assert.Equal("ErridNCarService.BtlBulletinDealerGetForRO_VINEmpty", root.GetProperty("ErrorCode").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("Params").GetProperty("Check.Input.VIN").ValueKind);
    }

    [Fact]
    public async Task VINChiCoKhoangTrang_BoMenhDeVIN_TraBanTinPendingConHan()
    {
        // Gate :1091 — IsNullOrEmpty("   ") = false ⇒ qua guard; gate :1104 StdStr("   ") = null ⇒ bỏ mệnh đề VIN.
        // Còn FlagActive = '1' + Status = 'PENDING' ⇒ BLT001, BLT006; BLT001 đã có báo cáo BH ⇒ 1 dòng (BLT006).
        var root = Root(await _controller.GetForRO(new BtlBulletinDealer_GetForROReq { VIN = "   " }));
        Assert.True(root.GetProperty("Success").GetBoolean());
        var list = root.GetProperty("Data").GetProperty("Lst_Btl_Bulletin");
        Assert.Equal(1, list.GetArrayLength());
        Assert.Equal("BLT006", list[0].GetProperty("BulletinID").GetString());
    }

    [Fact]
    public async Task ROIDKhongTonTai_TraLoiROIDNotFound()
    {
        // Gate :1208-1219 — Ser_RO không có dòng ⇒ BtlBulletinDealerGetForRO_ROIDNotFound.
        var root = Bad(await _controller.GetForRO(new BtlBulletinDealer_GetForROReq { VIN = "RLHXXAC001", ROID = "RO404" }));
        Assert.Equal("ErridNCarService.BtlBulletinDealerGetForRO_ROIDNotFound", root.GetProperty("ErrorCode").GetString());
        Assert.Equal("RO404", root.GetProperty("Params").GetProperty("Check.Input.ROID").GetString());
    }

    [Fact]
    public async Task ROIDCreatedDateSai_TraLoiInvalidROCreate()
    {
        // Gate :1221-1234 — CreatedDate không phải ngày ⇒ BtlBulletinDealerGetForRO_InvalidROCreate.
        var root = Bad(await _controller.GetForRO(new BtlBulletinDealer_GetForROReq { VIN = "RLHXXAC001", ROID = "RO002" }));
        Assert.Equal("ErridNCarService.BtlBulletinDealerGetForRO_InvalidROCreate", root.GetProperty("ErrorCode").GetString());
        Assert.Equal("khong-phai-ngay", root.GetProperty("Params").GetProperty("Check.RO.CreatedDate").GetString());
    }

    [Fact]
    public async Task VIN1_KhongROID_BoBanTinDaCoBaoCaoBaoHanh()
    {
        // Gate :1242-1265 — VIN = @VIN, FlagActive = '1', Status = 'PENDING' ⇒ BLT001.
        // Gate :1379-1389 — BLT001 đã có dòng Ser_ROWarrantyReportServiceItems ⇒ bị bỏ ⇒ 0 dòng.
        var root = Root(await _controller.GetForRO(new BtlBulletinDealer_GetForROReq { VIN = "RLHXXAC001" }));
        Assert.True(root.GetProperty("Success").GetBoolean());
        Assert.Equal(0, root.GetProperty("Data").GetProperty("Lst_Btl_Bulletin").GetArrayLength());
    }

    [Fact]
    public async Task VIN2_KhongROID_TraBanTinPendingConHan()
    {
        // Bộ lọc thứ hai ra số dòng khác: VIN RLHXXTC002 ⇒ BLT006 (BLT005 Status DONE bị lọc), chưa có báo cáo BH ⇒ 1 dòng.
        var root = Root(await _controller.GetForRO(new BtlBulletinDealer_GetForROReq { VIN = "RLHXXTC002" }));
        var list = root.GetProperty("Data").GetProperty("Lst_Btl_Bulletin");
        Assert.Equal(1, list.GetArrayLength());
        Assert.Equal("BLT006", list[0].GetProperty("BulletinID").GetString());
    }

    [Fact]
    public async Task VIN1_CoROID_SoHanTheoNgayTaoRO_VaBoBanTinDaCoBaoCao()
    {
        // Gate :1296-1301 — có RO ⇒ so DateExpired với ngày tạo RO (RO001 = hôm qua) thay vì hôm nay.
        // BLT001 (DateExpired = mai) vẫn còn hạn nhưng đã có báo cáo BH ⇒ bị bỏ; BLT004 (hết hạn hôm qua) còn hạn
        //   theo ngày tạo RO ⇒ được giữ ⇒ 1 dòng (BLT004).
        // GAP: bộ lọc DealerCode/FrameNo của Ser_ROWarrantyReport_GetWHDL (gate :1331-1334) không port được vì thân
        //   Ser_ROWarrantyReport_GetWHX không có trong excerpt ⇒ test không truyền DealerCode (bộ lọc rỗng như nguồn khi trống).
        var root = Root(await _controller.GetForRO(new BtlBulletinDealer_GetForROReq { VIN = "RLHXXAC001", ROID = "RO001" }));
        Assert.True(root.GetProperty("Success").GetBoolean());
        var list = root.GetProperty("Data").GetProperty("Lst_Btl_Bulletin");
        Assert.Equal(1, list.GetArrayLength());
        Assert.Equal("BLT004", list[0].GetProperty("BulletinID").GetString());
    }

    [Fact]
    public async Task VIN1_CoROID_BanTinHetHanTheoNgayTaoRO_BiBo()
    {
        // Gate :1303-1312 — DateExpired < ngày tạo RO ⇒ bỏ dòng; BLT001 hết hạn theo RO (tạo 10 ngày trước) ⇒ 0 dòng.
        var ro = await _db.Set<Ser_RO>().FirstAsync(x => x.ROID == "RO001");
        ro.CreatedDate = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd HH:mm:ss");
        await _db.SaveChangesAsync();

        var root = Root(await _controller.GetForRO(new BtlBulletinDealer_GetForROReq { VIN = "RLHXXAC001", ROID = "RO001" }));
        Assert.Equal(0, root.GetProperty("Data").GetProperty("Lst_Btl_Bulletin").GetArrayLength());
    }

    [Fact]
    public async Task VIN2_CoROID_TraBanTinConHan()
    {
        // VIN RLHXXTC002 + ROID hợp lệ ⇒ BLT006 (chưa có báo cáo BH) ⇒ 1 dòng.
        var root = Root(await _controller.GetForRO(new BtlBulletinDealer_GetForROReq { VIN = "RLHXXTC002", ROID = "RO001" }));
        var list = root.GetProperty("Data").GetProperty("Lst_Btl_Bulletin");
        Assert.Equal(1, list.GetArrayLength());
        Assert.Equal("BLT006", list[0].GetProperty("BulletinID").GetString());
    }

    [Fact]
    public async Task VINKhongCo_TraDanhSachRong()
    {
        // VIN không có dòng nào ⇒ Lst_Btl_Bulletin rỗng (không phải null).
        var root = Root(await _controller.GetForRO(new BtlBulletinDealer_GetForROReq { VIN = "KHONGCO" }));
        Assert.True(root.GetProperty("Success").GetBoolean());
        Assert.Equal(0, root.GetProperty("Data").GetProperty("Lst_Btl_Bulletin").GetArrayLength());
    }
}
