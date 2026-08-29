using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models;
using MiniService.Services;
using Xunit;

namespace MiniService.Tests;

/// <summary>
/// Test nghiệp vụ RO: state machine, ràng buộc thêm dòng, xuất HĐĐT.
/// Dùng SQLite in-memory + fake tích hợp (không gọi mạng thật).
/// </summary>
public class RoServiceTests
{
    // Tích hợp giả: luôn trả về "đã cấp mã" để test luồng, không gọi HTTP.
    private sealed class FakeIntegration : IIntegrationService
    {
        public Task<EInvoiceResult> PushEInvoiceAsync(RepairOrder ro) =>
            Task.FromResult(new EInvoiceResult(true, "Accepted", "TCT-TEST-001", null));
        public Task NotifyCustomerAsync(RepairOrder ro) => Task.CompletedTask;
        public Task<VehicleStatus> LookupVehicleAsync(string? plate, string? vin) =>
            Task.FromResult(new VehicleStatus(false, false, null, null, null, false, false, null, null, null));
        public Task<ClaimResult> FileInsuranceClaimAsync(string? plate, decimal amount, string? description) =>
            Task.FromResult(new ClaimResult(true, "BT2608-001", "Đã khai báo", null));
    }

    private static (RoService svc, AppDbContext db) NewSvc()
    {
        var cn = new SqliteConnection("DataSource=:memory:"); cn.Open();
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(cn).Options;
        var db = new AppDbContext(opts, new TenantContext());
        db.Database.EnsureCreated();
        return (new RoService(db, new FakeIntegration()), db);
    }

    private static async Task<int> SeedRoAsync(RoService svc, AppDbContext db, bool withLine = true)
    {
        var cusId = await svc.CreateCustomerAsync(new Customer { Name = "KH Test", Phone = "0900000000" });
        var carId = await svc.CreateCarAsync(new Car { Plate = "30A-000.01", Model = "Test", CustomerId = cusId });
        var roId = await svc.CreateROAsync(new RepairOrder { CarId = carId, CustomerId = cusId });
        if (withLine) await svc.AddLineAsync(roId, LineType.Labor, "Công thay dầu", 1, 200_000);
        return roId;
    }

    // ---- State machine ----
    [Theory]
    [InlineData(ROStatus.Created, ROStatus.Printed, true)]
    [InlineData(ROStatus.Created, ROStatus.Paid, false)]        // nhảy cóc → cấm
    [InlineData(ROStatus.CheckEnd, ROStatus.Paid, true)]
    [InlineData(ROStatus.Paid, ROStatus.Finished, true)]
    [InlineData(ROStatus.Finished, ROStatus.Created, false)]     // đã kết thúc → cấm
    public async Task Transition_TheoStateMachine(ROStatus from, ROStatus to, bool expectOk)
    {
        var (svc, db) = NewSvc();
        var roId = await SeedRoAsync(svc, db);
        var ro = await db.ROs.FirstAsync(); ro.Status = from; await db.SaveChangesAsync();

        var (ok, _) = await svc.TransitionAsync(roId, to);
        Assert.Equal(expectOk, ok);
    }

    [Fact]
    public void AllowedNext_Finished_LaRong()
        => Assert.Empty(RoService.AllowedNext(ROStatus.Finished));

    // ---- Quyết toán (PAID) tự xuất HĐĐT ----
    [Fact]
    public async Task Paid_TuDongXuatHDDT_LuuMaCQT()
    {
        var (svc, db) = NewSvc();
        var roId = await SeedRoAsync(svc, db);
        var ro = await db.ROs.FirstAsync(); ro.Status = ROStatus.CheckEnd; await db.SaveChangesAsync();

        var (ok, _) = await svc.TransitionAsync(roId, ROStatus.Paid);
        Assert.True(ok);
        var after = await db.ROs.FirstAsync();
        Assert.Equal("TCT-TEST-001", after.EInvoiceCode);       // đã gắn mã CQT
    }

    [Fact]
    public async Task XuatHDDT_KhiChuaCoDong_ThiChan()
    {
        var (svc, db) = NewSvc();
        var roId = await SeedRoAsync(svc, db, withLine: false);   // total = 0
        var (ok, msg) = await svc.IssueEInvoiceAsync(roId);
        Assert.False(ok);
        Assert.Contains("chi phí", msg);
    }

    [Fact]
    public async Task XuatHDDT_LanHai_ThiChan_DaCoMa()
    {
        var (svc, db) = NewSvc();
        var roId = await SeedRoAsync(svc, db);
        await svc.IssueEInvoiceAsync(roId);                       // lần 1 OK
        var (ok, msg) = await svc.IssueEInvoiceAsync(roId);       // lần 2 chặn
        Assert.False(ok);
        Assert.Contains("đã có", msg);
    }

    // ---- Thêm dòng ----
    [Fact]
    public async Task Total_BangTongCacDong()
    {
        var (svc, db) = NewSvc();
        var roId = await SeedRoAsync(svc, db);                    // 1 dòng 200k
        await svc.AddLineAsync(roId, LineType.Part, "Lọc dầu", 2, 150_000);  // +300k
        var ro = await svc.GetROAsync(roId);
        Assert.Equal(500_000, ro!.Total);
    }
}
