using Microsoft.AspNetCore.Mvc;
using MiniService.Models;
using MiniService.Services;

namespace MiniService.Controllers;

/// <summary>API tồn kho / xuất kho / quyết toán (JSON) — module bổ sung để đồng bộ với hệ CarService thật.</summary>
[ApiController]
[Route("api/v1/inventory")]
[Produces("application/json")]
public class InventoryApiController(IInventoryService inv) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard() => Ok(await inv.DashboardAsync());

    /// <summary>Danh sách phụ tùng + tồn kho.</summary>
    [HttpGet("parts")]
    public async Task<IActionResult> Parts([FromQuery] string? q)
        => Ok((await inv.PartsAsync(q)).Select(p => new { p.Id, p.Code, p.Name, p.Unit, p.Price, p.OnHand, p.MinStock, lowStock = p.LowStock, stockValue = p.StockValue }));

    [HttpPost("parts")]
    public async Task<IActionResult> CreatePart([FromBody] Part p)
    {
        var (ok, msg, id) = await inv.CreatePartAsync(p);
        return ok ? Ok(new { id, msg }) : BadRequest(new { error = msg });
    }

    /// <summary>Nhập kho.</summary>
    [HttpPost("parts/{id:int}/receive")]
    public async Task<IActionResult> Receive(int id, [FromBody] QtyReq req)
    {
        var (ok, msg) = await inv.ReceiveAsync(id, req.Qty);
        return ok ? Ok(new { ok, msg }) : BadRequest(new { ok, msg });
    }

    /// <summary>Xuất kho (không cho âm tồn).</summary>
    [HttpPost("issue")]
    public async Task<IActionResult> Issue([FromBody] IssueReq req)
    {
        var (ok, msg) = await inv.IssueStockAsync(req.PartId, req.Qty, req.RoId, req.Reason);
        return ok ? Ok(new { ok, msg }) : BadRequest(new { ok, msg });
    }

    [HttpGet("stockouts")]
    public async Task<IActionResult> StockOuts([FromQuery] int? roId)
        => Ok((await inv.StockOutsAsync(roId)).Select(s => new { s.Id, s.Code, s.PartName, s.Quantity, s.UnitPrice, amount = s.Amount, roCode = s.ROCode, s.Reason, s.CreatedAt }));

    /// <summary>Quyết toán RO: ghi thanh toán + tự xuất kho phụ tùng.</summary>
    [HttpPost("settle")]
    public async Task<IActionResult> Settle([FromBody] SettleReq req)
    {
        var (ok, msg, total, issued) = await inv.SettleAsync(req.RoId, req.Method, req.Note);
        return ok ? Ok(new { ok, msg, total, issued }) : BadRequest(new { ok, msg });
    }
}

public record QtyReq(int Qty);
public record IssueReq(int PartId, int Qty, int? RoId, string? Reason);
public record SettleReq(int RoId, PayMethod Method, string? Note);
