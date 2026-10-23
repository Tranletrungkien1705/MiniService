using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MiniService.Controllers.Port;
using Xunit;

namespace MiniService.Tests;

/// <summary>
/// Test port 1:1 FileController/CheckFileExist.
/// Nguồn: gate FileController.cs:40-184 — chỉ System.IO.File.Exists, không DB/không WA.
/// Mã lỗi File_CheckFileExist_InvalidInput (Error.CarSv.cs:56); TConst.Flag.Active="1"/Inactive="0" (Const.Main.cs:32-33).
/// </summary>
public class FileControllerPortTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly string _basePath;
    private readonly FileControllerPort _controller;

    public FileControllerPortTests()
    {
        var uniq = Guid.NewGuid().ToString("N").Substring(0, 8);
        _tempRoot = Path.Combine(Path.GetTempPath(), "port48_" + uniq);
        Directory.CreateDirectory(_tempRoot);
        _basePath = _tempRoot + Path.DirectorySeparatorChar;

        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["_strConfig_FileLocal_WS"] = _basePath })
            .Build();
        _controller = new FileControllerPort(cfg);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = null! }
        };
    }

    [Fact]
    public void CheckFileExist_MissingFile_ReturnsActive()
    {
        var res = _controller.CheckFileExist(new FileControllerPort.File_CheckFileExistReq { strFilePathServer = "missing.txt" });
        var ok = Assert.IsType<OkObjectResult>(res);
        var data = ok.Value!.GetType().GetProperty("Data")!.GetValue(ok.Value);
        var success = ok.Value!.GetType().GetProperty("Success")!.GetValue(ok.Value);
        Assert.Equal(true, success);
        Assert.Equal("1", data);
    }

    [Fact]
    public void CheckFileExist_ExistingFile_ReturnsInactive()
    {
        File.WriteAllText(_basePath + "exists.txt", "x");
        var res = _controller.CheckFileExist(new FileControllerPort.File_CheckFileExistReq { strFilePathServer = "exists.txt" });
        var ok = Assert.IsType<OkObjectResult>(res);
        var data = ok.Value!.GetType().GetProperty("Data")!.GetValue(ok.Value);
        Assert.Equal("0", data);
    }

    [Fact]
    public void CheckFileExist_TrimmedInput_ReturnsInactive()
    {
        File.WriteAllText(_basePath + "exists.txt", "x");
        var res = _controller.CheckFileExist(new FileControllerPort.File_CheckFileExistReq { strFilePathServer = " exists.txt " });
        var ok = Assert.IsType<OkObjectResult>(res);
        var data = ok.Value!.GetType().GetProperty("Data")!.GetValue(ok.Value);
        Assert.Equal("0", data);
    }

    [Fact]
    public void CheckFileExist_EmptyInput_ReturnsInvalidInput()
    {
        var res = _controller.CheckFileExist(new FileControllerPort.File_CheckFileExistReq { strFilePathServer = " " });
        var bad = Assert.IsType<BadRequestObjectResult>(res);
        var err = bad.Value!.GetType().GetProperty("ErrorCode")!.GetValue(bad.Value) as string;
        Assert.Equal("ErridNCarService.File_CheckFileExist_InvalidInput", err);
    }

    [Fact]
    public void CheckFileExist_NullBody_ReturnsInvalidInput()
    {
        var res = _controller.CheckFileExist(null);
        var bad = Assert.IsType<BadRequestObjectResult>(res);
        var err = bad.Value!.GetType().GetProperty("ErrorCode")!.GetValue(bad.Value) as string;
        Assert.Equal("ErridNCarService.File_CheckFileExist_InvalidInput", err);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempRoot, recursive: true); } catch { }
    }
}
