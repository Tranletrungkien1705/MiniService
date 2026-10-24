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

public class MstProvinceControllerPortTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly MstProvinceControllerPort _controller;

    public MstProvinceControllerPortTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
        _db = new AppDbContext(opt, new TenantContext());
        _db.Database.EnsureCreated();
        new Mst_ProvinceSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        _controller = new MstProvinceControllerPort(_db);
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

    private static JsonElement BadRoot(IActionResult result)
    {
        var bad = Assert.IsType<BadRequestObjectResult>(result);
        return JsonDocument.Parse(JsonSerializer.Serialize(bad.Value)).RootElement;
    }

    // ===== Search =====

    [Fact]
    public async Task BodyRong_Tra4Dong()
    {
        // Body rỗng: lấy tất cả 4 dòng seeded.
        var req = new Mst_Province_SearchReq();
        var root = Root(await _controller.Search(req));
        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.Equal(4, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal(1, data.GetProperty("PageCount").GetInt32());
        var list = data.GetProperty("DataList");
        Assert.Equal(4, list.GetArrayLength());
    }

    [Fact]
    public async Task FilterProvinceCode_Tra1Dong()
    {
        // Filter ProvinceCode = "01" → 1 dòng (Hà Nội).
        var req = new Mst_Province_SearchReq { ProvinceCode = "01" };
        var root = Root(await _controller.Search(req));
        var data = root.GetProperty("Data");
        Assert.Equal(1, data.GetProperty("ItemCount").GetInt32());
        var list = data.GetProperty("DataList");
        Assert.Equal(1, list.GetArrayLength());
        Assert.Equal("01", list[0].GetProperty("ProvinceCode").GetString());
        Assert.Equal("Hà Nội", list[0].GetProperty("ProvinceName").GetString());
    }

    [Fact]
    public async Task FilterFlagActive1_Tra3Dong()
    {
        // Filter FlagActive = "1" → 3 dòng (01, 02, 04).
        var req = new Mst_Province_SearchReq { FlagActive = "1" };
        var root = Root(await _controller.Search(req));
        var data = root.GetProperty("Data");
        Assert.Equal(3, data.GetProperty("ItemCount").GetInt32());
        var list = data.GetProperty("DataList");
        Assert.Equal(3, list.GetArrayLength());
    }

    [Fact]
    public async Task FilterFlagActive0_Tra1Dong()
    {
        // Filter FlagActive = "0" → 1 dòng (03, Đà Nẵng).
        var req = new Mst_Province_SearchReq { FlagActive = "0" };
        var root = Root(await _controller.Search(req));
        var data = root.GetProperty("Data");
        Assert.Equal(1, data.GetProperty("ItemCount").GetInt32());
        var list = data.GetProperty("DataList");
        Assert.Equal(1, list.GetArrayLength());
        Assert.Equal("03", list[0].GetProperty("ProvinceCode").GetString());
    }

    [Fact]
    public async Task FilterProvinceName_TraDongCoChứa()
    {
        // Filter ProvinceName LIKE "Hồ" → 1 dòng (TP. Hồ Chí Minh).
        var req = new Mst_Province_SearchReq { ProvinceName = "Hồ" };
        var root = Root(await _controller.Search(req));
        var data = root.GetProperty("Data");
        Assert.Equal(1, data.GetProperty("ItemCount").GetInt32());
        var list = data.GetProperty("DataList");
        Assert.Equal(1, list.GetArrayLength());
        Assert.Contains("Hồ", list[0].GetProperty("ProvinceName").GetString()!);
    }

    [Fact]
    public async Task BangRong_TraPageInfoRong()
    {
        // Xoá toàn bộ dữ liệu.
        var set = _db.Set<Mst_Province>();
        set.RemoveRange(await set.ToListAsync());
        await _db.SaveChangesAsync();

        var req = new Mst_Province_SearchReq();
        var root = Root(await _controller.Search(req));
        var data = root.GetProperty("Data");
        Assert.Equal(0, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal(0, data.GetProperty("PageCount").GetInt32());
        var list = data.GetProperty("DataList");
        Assert.Equal(0, list.GetArrayLength());
    }

    [Fact]
    public async Task Pagination_PageIndex1_Tra2Dong()
    {
        // Phân trang: PageIndex=0, PageSize=2 → 2 dòng đầu (01, 02).
        // PageIndex=1, PageSize=2 → 2 dòng tiếp (03, 04).
        var req = new Mst_Province_SearchReq { Ft_PageIndex = 1, Ft_PageSize = 2 };
        var root = Root(await _controller.Search(req));
        var data = root.GetProperty("Data");
        Assert.Equal(4, data.GetProperty("ItemCount").GetInt32());
        Assert.Equal(2, data.GetProperty("PageCount").GetInt32());
        Assert.Equal(1, data.GetProperty("PageIndex").GetInt32());
        var list = data.GetProperty("DataList");
        Assert.Equal(2, list.GetArrayLength());
        // Dòng thứ 3 (03) và 4 (04).
        Assert.Equal("03", list[0].GetProperty("ProvinceCode").GetString());
        Assert.Equal("04", list[1].GetProperty("ProvinceCode").GetString());
    }

    // ===== GetByProvinceCode =====

    [Fact]
    public async Task GetByProvinceCode_ProvinceCode01_TraHaNoi()
    {
        // Gate: MstProvinceController.cs:149-202 GetByProvinceCode(string ProvinceCode)
        // Biz: BizCarSv.Master.cs:5999 (Mst_Province_Get) -> 6144 (Mst_Province_GetX).
        // Lọc: ProvinceCode = "01" → Hà Nội.
        var req = new Mst_Province_GetByProvinceCodeReq { ProvinceCode = "01" };
        var root = Root(await _controller.GetByProvinceCode(req));
        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.Equal("01", data.GetProperty("ProvinceCode").GetString());
        Assert.Equal("Hà Nội", data.GetProperty("ProvinceName").GetString());
    }

    [Fact]
    public async Task GetByProvinceCode_BodyRong_TraNull()
    {
        // ProvinceCode rỗng → trả null.
        var req = new Mst_Province_GetByProvinceCodeReq();
        var root = Root(await _controller.GetByProvinceCode(req));
        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.Equal(JsonValueKind.Null, data.ValueKind);
    }

    [Fact]
    public async Task GetByProvinceCode_ProvinceCodeKhongTon_TraNull()
    {
        // ProvinceCode không tồn tại → trả null.
        var req = new Mst_Province_GetByProvinceCodeReq { ProvinceCode = "XX" };
        var root = Root(await _controller.GetByProvinceCode(req));
        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.Equal(JsonValueKind.Null, data.ValueKind);
    }

    // ===== GetAllActive =====

    [Fact]
    public async Task GetAllActive_TraCac3Dong()
    {
        // GetAllActive: lấy tất cả dòng có FlagActive = "1" → 3 dòng (01, 02, 04).
        // Gate trả List<Mst_Province> trực tiếp (không PageInfo).
        var root = Root(await _controller.GetAllActive());
        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.Equal(3, data.GetArrayLength());
        Assert.Equal("01", data[0].GetProperty("ProvinceCode").GetString());
        Assert.Equal("02", data[1].GetProperty("ProvinceCode").GetString());
        Assert.Equal("04", data[2].GetProperty("ProvinceCode").GetString());
    }

    [Fact]
    public async Task GetAllActive_BangRong_TraNull()
    {
        // GetAllActive khi không có dòng FlagActive = "1" → trả null (gate logic).
        var set = _db.Set<Mst_Province>();
        set.RemoveRange(await set.ToListAsync());
        await _db.SaveChangesAsync();

        var root = Root(await _controller.GetAllActive());
        var data = root.GetProperty("Data");
        Assert.Equal(JsonValueKind.Null, data.ValueKind);
    }

    // ===== Delete =====

    [Fact]
    public async Task Delete_ProvinceCode01_XoaThanhCong()
    {
        // Gate: MstProvinceController.cs:340-390 Delete(string ProvinceCode).
        // Xoá tỉnh có mã "01" (Hà Nội) → thành công, Success=true, Data=null.
        var req = new Mst_Province_DeleteReq { ProvinceCode = "01" };
        var root = Root(await _controller.Delete(req));
        Assert.True(root.GetProperty("Success").GetBoolean());
        var data = root.GetProperty("Data");
        Assert.Equal(JsonValueKind.Null, data.ValueKind);

        // Kiểm tra: dòng "01" đã bị xoá.
        var item = await _db.Set<Mst_Province>().FirstOrDefaultAsync(x => x.ProvinceCode == "01");
        Assert.Null(item);
    }

    [Fact]
    public async Task Delete_ProvinceCodeRong_TraLoi()
    {
        // Gate: MstProvinceController.cs:357-384 kiểm ProvinceCode không rỗng.
        // ProvinceCode rỗng → trả lỗi "invalid_provincecode" (tương đương Check.ProvinceCode).
        var req = new Mst_Province_DeleteReq();
        var root = BadRoot(await _controller.Delete(req));
        Assert.False(root.GetProperty("Success").GetBoolean());
        var errorCode = root.GetProperty("ErrorCode").GetString();
        Assert.Equal("ErridNCarService.Mst_Province_Delete", errorCode);
    }

    [Fact]
    public async Task Delete_ProvinceCodeKhongTon_TraLoi()
    {
        // Xoá tỉnh không tồn tại → trả lỗi NotFound.
        var req = new Mst_Province_DeleteReq { ProvinceCode = "XX" };
        var root = BadRoot(await _controller.Delete(req));
        Assert.False(root.GetProperty("Success").GetBoolean());
        var errorCode = root.GetProperty("ErrorCode").GetString();
        Assert.Equal("ErridNCarService.Mst_Province_Delete", errorCode);
    }

    [Fact]
    public async Task Delete_SauXoa_CacDongConLaiVanO()
    {
        // Xoá dòng "02" (TP. Hồ Chí Minh) → các dòng khác vẫn tồn tại.
        var beforeCount = await _db.Set<Mst_Province>().CountAsync();
        var req = new Mst_Province_DeleteReq { ProvinceCode = "02" };
        await _controller.Delete(req);

        var afterCount = await _db.Set<Mst_Province>().CountAsync();
        Assert.Equal(beforeCount - 1, afterCount);

        // Dòng "01" vẫn tồn tại.
        var item01 = await _db.Set<Mst_Province>().FirstOrDefaultAsync(x => x.ProvinceCode == "01");
        Assert.NotNull(item01);
        Assert.Equal("Hà Nội", item01.ProvinceName);
    }
}
