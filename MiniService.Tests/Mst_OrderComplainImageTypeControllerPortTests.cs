using System.Text.Json;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
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
    private readonly TestWebHostEnvironment _env;

    public Mst_OrderComplainImageTypeControllerPortTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
        _db = new AppDbContext(opt, new TenantContext());
        _db.Database.EnsureCreated();
        new Mst_OrderComplainImageTypeSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        _env = new TestWebHostEnvironment();
        Directory.CreateDirectory(_env.WebRootPath);
        _controller = new Mst_OrderComplainImageTypeControllerPort(_db, _env);
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
        try { Directory.Delete(_env.WebRootPath, true); } catch { }
    }

    /// <summary>Môi trường web giả cho test (WebRootPath riêng, xoá sau khi chạy).</summary>
    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "test_wwwroot_" + Guid.NewGuid().ToString("N"));
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string ApplicationName { get; set; } = "MiniService.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public string EnvironmentName { get; set; } = "Development";
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

    // ---- ExportExcel (Gate :440-641 -> Biz :60804-61087) ----

    private (string physicalPath, List<string> header, List<string> firstCol) ReadExport(JsonElement root)
    {
        var url = root.GetProperty("Data").GetString()!;
        // Gate :605 — Data = GetRequestBaseAddress() + url (url ảo /TempFiles/yyyy-MM-dd/...xlsx).
        Assert.StartsWith("http://127.0.0.1:8500/TempFiles/", url);
        var physical = Path.Combine(_env.WebRootPath, url.Substring("http://127.0.0.1:8500/".Length).Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(physical), "File Excel phải tồn tại: " + physical);
        using var wb = new XLWorkbook(physical);
        var ws = wb.Worksheets.First();
        Assert.Equal("Mst_OrderComplainImageType", ws.Name);
        var header = new List<string>();
        for (var c = 1; c <= 3; c++) header.Add(ws.Cell(1, c).GetString());
        var firstCol = new List<string>();
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (var r = 2; r <= lastRow; r++) firstCol.Add(ws.Cell(r, 1).GetString());
        return (physical, header, firstCol);
    }

    [Fact]
    public async Task ExportExcel_BodyRong_XuatTatCa5Dong()
    {
        // Gate :529-538 — cả 3 tham số rỗng ⇒ GenEqualCondition2 trả "" ⇒ không mệnh đề lọc ⇒ 5 dòng seed.
        var root = Root(await _controller.ExportExcel(null));
        Assert.True(root.GetProperty("Success").GetBoolean());

        var (_, header, firstCol) = ReadExport(root);
        // Gate :645-653 — 3 cột, nhãn nguyên văn (kể cả "khiếu nạy").
        Assert.Equal(new[] { "Mã loại ảnh khiếu nại", "Tên loại ảnh khiếu nạy", "Trạng thái" }, header);
        Assert.Equal(new[] { "IMG01", "IMG02", "IMG03", "IMG04", "IMG05" }, firstCol);
    }

    [Fact]
    public async Task ExportExcel_LocFlagActive1_Ra3Dong()
    {
        // Biz :61040 — FlagActive = '1' ⇒ 3 dòng (IMG01..IMG03).
        var root = Root(await _controller.ExportExcel(new Mst_OrderComplainImageType_ExportReq { FlagActive = "1" }));
        var (_, _, firstCol) = ReadExport(root);
        Assert.Equal(new[] { "IMG01", "IMG02", "IMG03" }, firstCol);
    }

    [Fact]
    public async Task ExportExcel_LocFlagActive0_Ra2Dong()
    {
        // Biz :61040 — FlagActive = '0' ⇒ 2 dòng (IMG04..IMG05) — bộ lọc thứ hai ra số dòng khác.
        var root = Root(await _controller.ExportExcel(new Mst_OrderComplainImageType_ExportReq { FlagActive = "0" }));
        var (_, _, firstCol) = ReadExport(root);
        Assert.Equal(new[] { "IMG04", "IMG05" }, firstCol);
    }

    [Fact]
    public async Task ExportExcel_LocMaVaTen_Ra1Dong()
    {
        // Biz :61038-61039 — so BẰNG (không wildcard) trên mã và tên; tên so không phân biệt hoa/thường như SQL Server.
        // (SQLite: lower() chỉ hạ ASCII ⇒ dùng tên ASCII để kiểm đúng ngữ nghĩa "=" của nguồn.)
        var row = await _db.Set<Mst_OrderComplainImageType>().FirstAsync(x => x.OrderComplainImageType == "IMG02");
        row.OrderComplainImageName = "Anh bien so";
        await _db.SaveChangesAsync();

        var root = Root(await _controller.ExportExcel(new Mst_OrderComplainImageType_ExportReq
        {
            OrderComplainTypeCode = "IMG02",
            OrderComplainTypeName = "ANH BIEN SO"
        }));
        var (_, _, firstCol) = ReadExport(root);
        Assert.Equal(new[] { "IMG02" }, firstCol);
    }

    [Fact]
    public async Task ExportExcel_LocKhongKhop_FileChiCoDongTieuDe()
    {
        // Không dòng nào khớp ⇒ file vẫn sinh, chỉ có dòng tiêu đề (Gate :603 luôn gọi ExportToExcelFromList).
        var root = Root(await _controller.ExportExcel(new Mst_OrderComplainImageType_ExportReq { OrderComplainTypeCode = "KHONGCO" }));
        var (_, header, firstCol) = ReadExport(root);
        Assert.Equal(3, header.Count);
        Assert.Empty(firstCol);
    }
}
