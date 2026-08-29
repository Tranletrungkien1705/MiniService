using Microsoft.AspNetCore.Mvc;
using MiniService.Models;
using MiniService.Services;

namespace MiniService.Controllers;

/// <summary>
/// API v1 (JSON) cho SPA client-side + tích hợp. Mọi màn UI đọc/ghi qua đây → dễ soi API (Swagger).
/// Tenant xác định qua header X-Api-Key (middleware).
/// </summary>
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class ApiV1Controller(IRoService svc, ICache cache, ILogger<ApiV1Controller> log, IIntegrationService integ) : ControllerBase
{
    // ---- Dashboard ----
    /// <summary>Số liệu tổng quan (headcount RO, doanh thu tháng...). Có cache Redis 15s.</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        const string key = "svc:dash";
        var cached = await cache.GetAsync<SvcDash>(key);
        if (cached == null)
        {
            cached = await svc.DashboardAsync();
            await cache.SetAsync(key, cached, TimeSpan.FromSeconds(15));
            Response.Headers["X-Cache"] = cache.Enabled ? "MISS" : "OFF";
        }
        else Response.Headers["X-Cache"] = "HIT";
        // ValueTuple không serialize field → project sang object có tên.
        return Ok(new
        {
            cached.OpenRO, cached.InGarage, cached.DoneToday, cached.RevenueMonth, cached.Cars,
            byStatus = cached.ByStatus.Select(s => new { status = (int)s.Status, statusText = Ui.Status(s.Status).text, count = s.Count })
        });
    }

    // ---- Khách hàng ----
    [HttpGet("customers")]
    public async Task<IActionResult> Customers([FromQuery] string? q)
        => Ok((await svc.CustomersAsync(q)).Select(c => new { c.Id, c.Code, c.Name, c.Phone, c.Email, c.Address, c.TaxCode, c.DealerCode }));

    [HttpPost("customers")]
    public async Task<IActionResult> CreateCustomer([FromBody] Customer c)
    {
        if (string.IsNullOrWhiteSpace(c.Name)) return BadRequest(new { error = "Cần tên khách hàng." });
        var id = await svc.CreateCustomerAsync(c);
        await cache.RemoveByPrefixAsync("svc:");
        log.LogInformation("Tạo khách hàng {Name} (id={Id})", c.Name, id);
        return Ok(new { id });
    }

    // ---- Xe ----
    [HttpGet("cars")]
    public async Task<IActionResult> Cars([FromQuery] string? q)
        => Ok((await svc.CarsAsync(q)).Select(c => new { c.Id, c.Plate, c.Model, c.Year, c.Vin, c.EngineNo, c.Color, c.CurrentKm, c.CustomerId, customerName = c.Customer != null ? c.Customer.Name : null }));

    [HttpPost("cars")]
    public async Task<IActionResult> CreateCar([FromBody] Car car)
    {
        if (string.IsNullOrWhiteSpace(car.Plate)) return BadRequest(new { error = "Cần biển số." });
        var id = await svc.CreateCarAsync(car);
        return Ok(new { id });
    }

    // ---- Lệnh sửa chữa (RO) ----
    [HttpGet("ros")]
    public async Task<IActionResult> ROs([FromQuery] ROStatus? status, [FromQuery] string? q)
    {
        var ros = await svc.ROsAsync(status, q);
        return Ok(ros.Select(r => new
        {
            r.Id, r.Code, plate = r.Car.Plate, model = r.Car.Model, customer = r.Customer.Name,
            status = (int)r.Status, statusText = Ui.Status(r.Status).text, total = r.Total,
            eInvoice = r.EInvoiceCode, createdAt = r.CreatedAt
        }));
    }

    [HttpGet("ros/{id:int}")]
    public async Task<IActionResult> RO(int id)
    {
        var r = await svc.GetROAsync(id);
        if (r == null) return NotFound();
        // Tra chéo bảo hiểm (MiniInsurance) + bảo hành (MiniStamp) cho chiếc xe này — quyết ai chịu chi phí.
        var vs = await integ.LookupVehicleAsync(r.Car.Plate, r.Car.Vin);
        return Ok(new
        {
            r.Id, r.Code, status = (int)r.Status, statusText = Ui.Status(r.Status).text,
            r.Odometer, r.Technician, r.IntakeNote, r.CustomerRequest, r.ServiceAdvisor, r.ExpectedDelivery, r.CreatedAt,
            car = new { r.Car.Plate, r.Car.Model, r.Car.Year, r.Car.Vin, r.Car.EngineNo, r.Car.Color, r.Car.CurrentKm },
            customer = new { r.Customer.Name, r.Customer.Phone, r.Customer.Address, r.Customer.TaxCode },
            lines = r.Lines.Select(l => new { l.Id, type = (int)l.Type, l.Name, l.Quantity, l.UnitPrice, l.Amount }),
            r.LaborTotal, r.PartTotal, r.Total,
            eInvoice = new { code = r.EInvoiceCode, status = r.EInvoiceStatus, error = r.EInvoiceError, at = r.EInvoiceAt },
            vehicleStatus = new
            {
                insuranceFound = vs.InsuranceFound, insured = vs.Insured, policyCode = vs.PolicyCode,
                insurer = vs.Insurer, insuranceEnd = vs.InsuranceEnd,
                warrantyFound = vs.WarrantyFound, warrantyActive = vs.WarrantyActive,
                warrantyEnd = vs.WarrantyEnd, warrantyDaysLeft = vs.WarrantyDaysLeft, product = vs.Product
            },
            allowedNext = RoService.AllowedNext(r.Status).Select(s => new { value = (int)s, text = Ui.Status(s).text })
        });
    }

    [HttpPost("ros")]
    public async Task<IActionResult> CreateRO([FromBody] CreateRoReq req)
    {
        var id = await svc.CreateROAsync(new RepairOrder
        {
            CarId = req.CarId, Odometer = req.Odometer, IntakeNote = req.IntakeNote, Technician = req.Technician,
            CustomerRequest = req.CustomerRequest, ServiceAdvisor = req.ServiceAdvisor, ExpectedDelivery = req.ExpectedDelivery
        });
        await cache.RemoveByPrefixAsync("svc:");
        return Ok(new { id });
    }

    [HttpPost("ros/{id:int}/lines")]
    public async Task<IActionResult> AddLine(int id, [FromBody] AddLineReq req)
    {
        await svc.AddLineAsync(id, req.Type, req.Name, req.Quantity, req.UnitPrice);
        return Ok(new { ok = true });
    }

    [HttpDelete("ros/{id:int}/lines/{lineId:int}")]
    public async Task<IActionResult> RemoveLine(int id, int lineId) { await svc.RemoveLineAsync(lineId); return Ok(new { ok = true }); }

    [HttpPost("ros/{id:int}/transition")]
    public async Task<IActionResult> Transition(int id, [FromBody] TransitionReq req)
    {
        var (ok, msg) = await svc.TransitionAsync(id, req.To);
        await cache.RemoveByPrefixAsync("svc:");
        log.LogInformation("RO {Id} chuyển trạng thái → {To}: {Result}", id, req.To, ok ? "OK" : msg);
        return ok ? Ok(new { ok, msg }) : BadRequest(new { ok, msg });
    }

    /// <summary>Xuất HĐĐT thủ công: đẩy hóa đơn sang MiniTVAN (tích hợp thuế).</summary>
    [HttpPost("ros/{id:int}/einvoice")]
    public async Task<IActionResult> IssueEInvoice(int id)
    {
        var (ok, msg) = await svc.IssueEInvoiceAsync(id);
        return ok ? Ok(new { ok, msg }) : BadRequest(new { ok, msg });
    }
}

public record CreateRoReq(int CarId, int Odometer, string? IntakeNote, string? Technician, string? CustomerRequest, string? ServiceAdvisor, DateTime? ExpectedDelivery);
public record AddLineReq(LineType Type, string Name, decimal Quantity, decimal UnitPrice);
public record TransitionReq(ROStatus To);
