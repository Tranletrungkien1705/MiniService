using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MiniService.Data;
using MiniService.Models;
using MiniService.Services;
using Xunit;

namespace MiniService.Tests;

/// <summary>Test tồn kho / xuất kho / quyết toán.</summary>
public class InventoryTests
{
    private static (InventoryService inv, AppDbContext db) New()
    {
        var cn = new SqliteConnection("DataSource=:memory:"); cn.Open();
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(cn).Options;
        var db = new AppDbContext(opts, new TenantContext());
        db.Database.EnsureCreated();
        return (new InventoryService(db, NullLogger<InventoryService>.Instance), db);
    }

    [Fact]
    public async Task XuatKho_GiamTon()
    {
        var (inv, db) = New();
        db.Parts.Add(new Part { Code = "P1", Name = "Lọc dầu", OnHand = 10, Price = 100000 }); await db.SaveChangesAsync();
        var pid = (await db.Parts.FirstAsync()).Id;
        var (ok, _) = await inv.IssueStockAsync(pid, 3, null, "test");
        Assert.True(ok);
        Assert.Equal(7, (await db.Parts.FirstAsync()).OnHand);
        Assert.Equal(1, await db.StockOuts.CountAsync());
    }

    [Fact]
    public async Task XuatKho_QuaTon_ThiChan()
    {
        var (inv, db) = New();
        db.Parts.Add(new Part { Code = "P1", Name = "Bugi", OnHand = 2, Price = 100000 }); await db.SaveChangesAsync();
        var pid = (await db.Parts.FirstAsync()).Id;
        var (ok, msg) = await inv.IssueStockAsync(pid, 5, null, "test");
        Assert.False(ok);
        Assert.Contains("không đủ", msg);
        Assert.Equal(2, (await db.Parts.FirstAsync()).OnHand);   // tồn không đổi
    }

    [Fact]
    public async Task QuyetToan_GhiThanhToan_VaXuatKhoPhuTung()
    {
        var (inv, db) = New();
        var part = new Part { Code = "P1", Name = "Lọc dầu", OnHand = 10, Price = 180000 };
        db.Parts.Add(part);
        var cus = new Customer { Name = "KH", Code = "K1" }; db.Customers.Add(cus);
        var car = new Car { Plate = "30A-1", Customer = cus }; db.Cars.Add(car);
        await db.SaveChangesAsync();
        var ro = new RepairOrder { Code = "RO1", CarId = car.Id, CustomerId = cus.Id, Status = ROStatus.CheckEnd,
            Lines = { new RepairLine { Type = LineType.Part, Name = "Lọc dầu", Quantity = 2, UnitPrice = 180000, PartId = part.Id },
                      new RepairLine { Type = LineType.Labor, Name = "Công", Quantity = 1, UnitPrice = 200000 } } };
        db.ROs.Add(ro); await db.SaveChangesAsync();

        var (ok, _, total, issued) = await inv.SettleAsync(ro.Id, PayMethod.Cash, null);
        Assert.True(ok);
        Assert.Equal(560000, total);          // 2*180k + 200k
        Assert.Equal(1, issued);              // 1 dòng phụ tùng có PartId → xuất kho
        Assert.Equal(8, (await db.Parts.FirstAsync()).OnHand);   // 10 - 2
        Assert.Equal(1, await db.Payments.CountAsync());
    }

    [Fact]
    public async Task QuyetToan_LanHai_ThiChan()
    {
        var (inv, db) = New();
        var cus = new Customer { Name = "KH", Code = "K1" }; db.Customers.Add(cus);
        var car = new Car { Plate = "30A-1", Customer = cus }; db.Cars.Add(car); await db.SaveChangesAsync();
        var ro = new RepairOrder { Code = "RO1", CarId = car.Id, CustomerId = cus.Id, Status = ROStatus.CheckEnd,
            Lines = { new RepairLine { Type = LineType.Labor, Name = "Công", Quantity = 1, UnitPrice = 100000 } } };
        db.ROs.Add(ro); await db.SaveChangesAsync();
        await inv.SettleAsync(ro.Id, PayMethod.Cash, null);
        var (ok, msg, _, _) = await inv.SettleAsync(ro.Id, PayMethod.Cash, null);
        Assert.False(ok);
        Assert.Contains("đã được quyết toán", msg);
    }
}
