using System.Security.Cryptography;
using System.Text;
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

public class DlrMstDealerContractFormPrintPortTests : IDisposable
{
    private static readonly byte[] DesKey = new byte[8] { 3, 2, 1, 4, 5, 6, 7, 8 };
    private static readonly byte[] DesIv = new byte[8] { 8, 2, 3, 4, 5, 6, 7, 1 };

    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly DlrMstDealerContractFormPrintControllerPort _controller;

    public DlrMstDealerContractFormPrintPortTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
        _db = new AppDbContext(opt, new TenantContext());
        _db.Database.EnsureCreated();

        new Mst_TempPrintTypeSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        new Mst_TempPrintGroupSeeder().SeedAsync(_db).GetAwaiter().GetResult();
        new Dlr_Mst_DealerContractFormPrintSeeder().SeedAsync(_db).GetAwaiter().GetResult();

        _controller = new DlrMstDealerContractFormPrintControllerPort(_db);
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

    private static string EncryptDes(string text)
    {
        using var algorithm = DES.Create();
        using var transform = algorithm.CreateEncryptor(DesKey, DesIv);
        byte[] inputBuffer = Encoding.Unicode.GetBytes(text);
        byte[] outputBuffer = transform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
        return Convert.ToBase64String(outputBuffer);
    }

    [Fact]
    public async Task CreateTempPrintGroup_FileKhongHopLe_BaoLoi()
    {
        var bad = Assert.IsType<BadRequestObjectResult>(await _controller.CreateTempPrintGroup(new DlrMstDealerContractFormPrint_CreateTempPrintGroupReq
        {
            FileName = "sample.docx",
            FileContent = "abc",
            TempPrintType = "PRINT_TYPE_01"
        }));

        var root = JsonDocument.Parse(JsonSerializer.Serialize(bad.Value)).RootElement;
        Assert.False(root.GetProperty("Success").GetBoolean());
        Assert.Equal("ErridNCarService.Mst_TempPrintGroup_Import_InvalidFile", root.GetProperty("ErrorCode").GetString());
    }

    [Fact]
    public async Task CreateTempPrintGroup_TempPrintTypeRong_BaoLoi()
    {
        string rawJson = JsonSerializer.Serialize(new
        {
            UserId = "user1",
            Title = "Title 1",
            Detail = new { section = "data" }
        });
        string encrypted = EncryptDes(rawJson);

        var bad = Assert.IsType<BadRequestObjectResult>(await _controller.CreateTempPrintGroup(new DlrMstDealerContractFormPrint_CreateTempPrintGroupReq
        {
            FileName = "template.rtmpl",
            FileContent = encrypted,
            TempPrintType = ""
        }));

        var root = JsonDocument.Parse(JsonSerializer.Serialize(bad.Value)).RootElement;
        Assert.False(root.GetProperty("Success").GetBoolean());
        Assert.Equal("ErridNCarService.Mst_TempPrintGroup_Save_InvalidTempPrint", root.GetProperty("ErrorCode").GetString());
    }

    [Fact]
    public async Task CreateTempPrintGroup_TempPrintTypeKhongTonTai_BaoLoi()
    {
        string rawJson = JsonSerializer.Serialize(new
        {
            UserId = "user1",
            Title = "Title 1",
            Detail = new { section = "data" }
        });
        string encrypted = EncryptDes(rawJson);

        var bad = Assert.IsType<BadRequestObjectResult>(await _controller.CreateTempPrintGroup(new DlrMstDealerContractFormPrint_CreateTempPrintGroupReq
        {
            FileName = "template.rtmpl",
            FileContent = encrypted,
            TempPrintType = "NOT_EXISTING_TYPE"
        }));

        var root = JsonDocument.Parse(JsonSerializer.Serialize(bad.Value)).RootElement;
        Assert.False(root.GetProperty("Success").GetBoolean());
        Assert.Equal("ErridNCarService.Mst_TempPrintType_CheckDB_TempPrintTypeNotFound", root.GetProperty("ErrorCode").GetString());
    }

    [Fact]
    public async Task CreateTempPrintGroup_ThanhCong_RoundTrip()
    {
        string rawJson = JsonSerializer.Serialize(new
        {
            UserId = "admin",
            Title = "Contract Form",
            Detail = new { field1 = "value1", table = new[] { 1, 2, 3 } }
        });
        string encrypted = EncryptDes(rawJson);

        var ok = Assert.IsType<OkObjectResult>(await _controller.CreateTempPrintGroup(new DlrMstDealerContractFormPrint_CreateTempPrintGroupReq
        {
            FileName = "template_contract.rtmpl",
            FileContent = encrypted,
            TempPrintType = "PRINT_TYPE_01",
            DealerCode = "HTC",
            Remark = "Test save round-trip"
        }));

        var root = JsonDocument.Parse(JsonSerializer.Serialize(ok.Value)).RootElement;
        Assert.True(root.GetProperty("Success").GetBoolean());

        var savedGroup = await _db.Set<Mst_TempPrintGroup>()
            .FirstOrDefaultAsync(x => x.TempPrintType == "PRINT_TYPE_01");
        Assert.NotNull(savedGroup);
        Assert.Equal("HTC", savedGroup.DealerCode);
        Assert.Equal("Test save round-trip", savedGroup.Remark);
        Assert.Contains("value1", savedGroup.TempGrpPrintBody ?? "");
    }
}
