using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models;
using MiniService.Services;

namespace MiniService.Controllers;

public class HomeController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index() { ViewBag.Dash = await svc.DashboardAsync(); return View(); }
}

public class CustomerController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(string? q) { ViewBag.Q = q; return View(await svc.CustomersAsync(q)); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? phone, string? email, string plate, string model, int year, string? vin)
    {
        if (string.IsNullOrWhiteSpace(name)) { TempData["Error"] = "Cần tên khách hàng."; return RedirectToAction(nameof(Index)); }
        var c = new Customer { Name = name.Trim(), Phone = phone, Email = email };
        if (!string.IsNullOrWhiteSpace(plate))
            c.Cars.Add(new Car { Plate = plate.Trim(), Model = model ?? "", Year = year, Vin = vin });
        await svc.CreateCustomerAsync(c);
        TempData["Success"] = "Đã tạo khách hàng.";
        return RedirectToAction(nameof(Index));
    }
}

public class CarController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(string? q) { ViewBag.Q = q; return View(await svc.CarsAsync(q)); }
}

public class PartController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(string? q, bool? lowStock)
    {
        ViewBag.Q = q;
        ViewBag.LowStock = lowStock ?? false;
        var parts = await svc.PartsAsync(q, lowStock);
        return View(parts);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string code, string name, string? unit, decimal costPrice, decimal salePrice, decimal inStock, decimal minStock, string? location, string? model)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Cần mã và tên phụ tùng.";
            return RedirectToAction(nameof(Index));
        }
        try
        {
            var part = new Part
            {
                Code = code.Trim(),
                Name = name.Trim(),
                Unit = string.IsNullOrWhiteSpace(unit) ? "Cái" : unit.Trim(),
                CostPrice = costPrice,
                SalePrice = salePrice,
                InStock = inStock,
                MinStock = minStock,
                Location = location?.Trim(),
                Model = model?.Trim()
            };
            await svc.CreatePartAsync(part);
            TempData["Success"] = $"Đã thêm phụ tùng '{part.Code} - {part.Name}'.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustStock(int id, decimal qty, string mode, string? note)
    {
        var (ok, msg) = await svc.AdjustStockAsync(id, qty, mode, note);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }
}

public class ROController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(ROStatus? status, string? q)
    {
        ViewBag.Status = status; ViewBag.Q = q;
        return View(await svc.ROsAsync(status, q));
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Cars = await svc.CarsForSelectAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int carId, int odometer, string? intakeNote, string? technician)
    {
        if (carId <= 0) { TempData["Error"] = "Chọn xe."; ViewBag.Cars = await svc.CarsForSelectAsync(); return View(); }
        var id = await svc.CreateROAsync(new RepairOrder { CarId = carId, Odometer = odometer, IntakeNote = intakeNote, Technician = technician, CreatedBy = "web" });
        TempData["Success"] = "Đã tạo RO (Lập báo giá).";
        return RedirectToAction(nameof(Detail), new { id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var ro = await svc.GetROAsync(id);
        if (ro == null) return NotFound();
        ViewBag.Next = RoService.AllowedNext(ro.Status);
        ViewBag.Parts = await svc.PartsForSelectAsync();
        return View(ro);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLine(int id, LineType type, string name, decimal quantity, decimal unitPrice, int? partId = null, ExpenseType expenseType = ExpenseType.Customer)
    {
        if (string.IsNullOrWhiteSpace(name) && (!partId.HasValue || partId.Value <= 0))
        {
            TempData["Error"] = "Cần tên dòng hoặc chọn phụ tùng.";
            return RedirectToAction(nameof(Detail), new { id });
        }
        try
        {
            await svc.AddLineAsync(id, type, name, quantity, unitPrice, partId, expenseType);
            TempData["Success"] = "Đã thêm dòng.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveLine(int id, int lineId)
    {
        await svc.RemoveLineAsync(lineId);
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(int id, ROStatus to)
    {
        var (ok, msg) = await svc.TransitionAsync(id, to);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteROAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return ok ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Detail), new { id });
    }
}

public class WarrantyController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(WarrantyStatus? status, string? q)
    {
        ViewBag.Status = status;
        ViewBag.Q = q;
        var list = await svc.WarrantyReportsAsync(status, q);
        return View(list);
    }

    public async Task<IActionResult> Create(int? roId)
    {
        ViewBag.ROs = await svc.ROsEligibleForWarrantyAsync();
        ViewBag.Parts = await svc.PartsForSelectAsync();
        ViewBag.SelectedRoId = roId;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int roId, string issueDesc, string diagResult, string? errCodeCD, string? errCodePN, int? partIdError)
    {
        if (roId <= 0)
        {
            TempData["Error"] = "Vui lòng chọn lệnh sửa chữa (RO).";
            return RedirectToAction(nameof(Create));
        }
        try
        {
            var id = await svc.CreateWarrantyReportFromROAsync(roId, issueDesc, diagResult, errCodeCD, errCodePN, partIdError, "web");
            TempData["Success"] = "Đã lập Báo cáo bảo hành xe.";
            return RedirectToAction(nameof(Detail), new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Create), new { roId });
        }
    }

    public async Task<IActionResult> Detail(int id)
    {
        var report = await svc.GetWarrantyReportAsync(id);
        if (report == null) return NotFound();
        ViewBag.Next = RoService.AllowedNextWarranty(report.Status);
        return View(report);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(int id, WarrantyStatus to, decimal? approvedAmount, string? note)
    {
        var (ok, msg) = await svc.TransitionWarrantyAsync(id, to, approvedAmount, note);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteWarrantyReportAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return ok ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Detail), new { id });
    }
}

public class AppointmentController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(AppointmentStatus? status, string? q, DateTime? date)
    {
        ViewBag.Status = status;
        ViewBag.Q = q;
        ViewBag.Date = date?.ToString("yyyy-MM-dd");
        var list = await svc.AppointmentsAsync(status, q, date);
        return View(list);
    }

    public async Task<IActionResult> Create(int? carId)
    {
        ViewBag.Cars = await svc.CarsForSelectAsync();
        ViewBag.SelectedCarId = carId;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int carId, DateTime appointmentDate, AppointmentServiceType serviceType, string? advisor, string? cavity, string customerRequest, string? note, string? source)
    {
        if (carId <= 0)
        {
            TempData["Error"] = "Vui lòng chọn thông tin xe.";
            ViewBag.Cars = await svc.CarsForSelectAsync();
            return View();
        }
        if (string.IsNullOrWhiteSpace(customerRequest))
        {
            TempData["Error"] = "Vui lòng nhập yêu cầu của khách hàng hoặc lý do đặt hẹn.";
            ViewBag.Cars = await svc.CarsForSelectAsync();
            return View();
        }

        try
        {
            var app = new Appointment
            {
                CarId = carId,
                AppointmentDate = appointmentDate != default ? appointmentDate : DateTime.Today.AddHours(9),
                ServiceType = serviceType,
                Advisor = advisor?.Trim(),
                Cavity = cavity?.Trim(),
                CustomerRequest = customerRequest.Trim(),
                Note = note?.Trim(),
                Source = string.IsNullOrWhiteSpace(source) ? "Hotline" : source.Trim(),
                CreatedBy = "web"
            };

            var id = await svc.CreateAppointmentAsync(app);
            TempData["Success"] = $"Đã đặt lịch hẹn {app.AppNo} thành công.";
            return RedirectToAction(nameof(Detail), new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ViewBag.Cars = await svc.CarsForSelectAsync();
            return View();
        }
    }

    public async Task<IActionResult> Detail(int id)
    {
        var app = await svc.GetAppointmentAsync(id);
        if (app == null) return NotFound();
        ViewBag.Next = RoService.AllowedNextAppointment(app.Status);
        return View(app);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus toStatus, string? cancelReason)
    {
        var (ok, msg) = await svc.TransitionAppointmentStatusAsync(id, toStatus, cancelReason);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(int id, int odometer, string? technician)
    {
        var (ok, msg, roId) = await svc.CheckInAppointmentAsync(id, odometer, technician);
        TempData[ok ? "Success" : "Error"] = msg;
        if (ok && roId.HasValue)
        {
            return RedirectToAction("Detail", "RO", new { id = roId.Value });
        }
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteAppointmentAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return ok ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Detail), new { id });
    }
}

public class StockInController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(StockInStatus? status, string? q, DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.Status = status;
        ViewBag.Q = q;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        var list = await svc.StockInsAsync(status, q, fromDate, toDate);
        return View(list);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Parts = await svc.PartsForSelectAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string supplierName, string? billNo, DateTime stockInDate, StockInType type, string? description,
        int[] partIds, decimal[] quantities, decimal[] unitPrices, decimal[] vatPercents, string[]? notes)
    {
        if (string.IsNullOrWhiteSpace(supplierName))
        {
            TempData["Error"] = "Vui lòng nhập tên nhà cung cấp (SupplierName).";
            ViewBag.Parts = await svc.PartsForSelectAsync();
            return View();
        }

        if (partIds == null || partIds.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn ít nhất một phụ tùng nhập kho.";
            ViewBag.Parts = await svc.PartsForSelectAsync();
            return View();
        }

        try
        {
            var stockIn = new StockIn
            {
                SupplierName = supplierName.Trim(),
                BillNo = billNo?.Trim(),
                StockInDate = stockInDate != default ? stockInDate : DateTime.Today,
                Type = type,
                Description = description?.Trim(),
                CreatedBy = "web"
            };

            var items = new List<StockInDetail>();
            for (int i = 0; i < partIds.Length; i++)
            {
                if (partIds[i] <= 0) continue;
                var qty = (quantities != null && i < quantities.Length) ? quantities[i] : 1;
                var price = (unitPrices != null && i < unitPrices.Length) ? unitPrices[i] : 0;
                var vat = (vatPercents != null && i < vatPercents.Length) ? vatPercents[i] : 8;
                var note = (notes != null && i < notes.Length) ? notes[i] : null;

                items.Add(new StockInDetail
                {
                    PartId = partIds[i],
                    Quantity = qty <= 0 ? 1 : qty,
                    UnitPrice = price,
                    VatPercent = vat < 0 ? 0 : vat,
                    Note = note?.Trim()
                });
            }

            if (items.Count == 0)
            {
                TempData["Error"] = "Chưa có dòng phụ tùng hợp lệ.";
                ViewBag.Parts = await svc.PartsForSelectAsync();
                return View();
            }

            var id = await svc.CreateStockInAsync(stockIn, items);
            TempData["Success"] = $"Đã lập phiếu nhập kho {stockIn.StockInNo} thành công.";
            return RedirectToAction(nameof(Detail), new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ViewBag.Parts = await svc.PartsForSelectAsync();
            return View();
        }
    }

    public async Task<IActionResult> Detail(int id)
    {
        var stockIn = await svc.GetStockInAsync(id);
        if (stockIn == null) return NotFound();
        ViewBag.Next = RoService.AllowedNextStockIn(stockIn.Status);
        return View(stockIn);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(int id, StockInStatus to, string? note)
    {
        var (ok, msg) = await svc.TransitionStockInStatusAsync(id, to, "Kế toán kho", note);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteStockInAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return ok ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Detail), new { id });
    }
}

public class StockOutController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(StockOutStatus? status, string? q, DateTime? fromDate, DateTime? toDate, int? roId)
    {
        ViewBag.Status = status;
        ViewBag.Q = q;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.ROId = roId;
        var list = await svc.StockOutsAsync(status, q, fromDate, toDate, roId);
        return View(list);
    }

    public async Task<IActionResult> Create(int? roId)
    {
        ViewBag.Parts = await svc.PartsForSelectAsync();
        ViewBag.ROs = await svc.ROsForStockOutAsync();
        ViewBag.SelectedROId = roId;
        if (roId.HasValue && roId.Value > 0)
        {
            var ro = await svc.GetROAsync(roId.Value);
            ViewBag.PreloadedRO = ro;
        }
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StockOutType type, int? roId, string? recipientName, DateTime stockOutDate, string? description,
        int[] partIds, decimal[] quantities, decimal[] unitPrices, decimal[] vatPercents, string[]? notes)
    {
        if (partIds == null || partIds.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn ít nhất một phụ tùng xuất kho.";
            ViewBag.Parts = await svc.PartsForSelectAsync();
            ViewBag.ROs = await svc.ROsForStockOutAsync();
            ViewBag.SelectedROId = roId;
            return View();
        }

        try
        {
            var stockOut = new StockOut
            {
                Type = type,
                ROId = (roId.HasValue && roId.Value > 0) ? roId.Value : null,
                RecipientName = recipientName?.Trim(),
                StockOutDate = stockOutDate != default ? stockOutDate : DateTime.Today,
                Description = description?.Trim(),
                CreatedBy = "web"
            };

            var items = new List<StockOutDetail>();
            for (int i = 0; i < partIds.Length; i++)
            {
                if (partIds[i] <= 0) continue;
                var qty = (quantities != null && i < quantities.Length) ? quantities[i] : 1;
                var price = (unitPrices != null && i < unitPrices.Length) ? unitPrices[i] : 0;
                var vat = (vatPercents != null && i < vatPercents.Length) ? vatPercents[i] : 8;
                var note = (notes != null && i < notes.Length) ? notes[i] : null;

                items.Add(new StockOutDetail
                {
                    PartId = partIds[i],
                    Quantity = qty <= 0 ? 1 : qty,
                    UnitPrice = price,
                    VatPercent = vat < 0 ? 0 : vat,
                    Note = note?.Trim()
                });
            }

            if (items.Count == 0)
            {
                TempData["Error"] = "Chưa có dòng phụ tùng hợp lệ.";
                ViewBag.Parts = await svc.PartsForSelectAsync();
                ViewBag.ROs = await svc.ROsForStockOutAsync();
                ViewBag.SelectedROId = roId;
                return View();
            }

            var id = await svc.CreateStockOutAsync(stockOut, items);
            TempData["Success"] = $"Đã lập phiếu xuất kho {stockOut.StockOutNo} thành công.";
            return RedirectToAction(nameof(Detail), new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ViewBag.Parts = await svc.PartsForSelectAsync();
            ViewBag.ROs = await svc.ROsForStockOutAsync();
            ViewBag.SelectedROId = roId;
            return View();
        }
    }

    public async Task<IActionResult> Detail(int id)
    {
        var stockOut = await svc.GetStockOutAsync(id);
        if (stockOut == null) return NotFound();
        ViewBag.Next = RoService.AllowedNextStockOut(stockOut.Status);
        return View(stockOut);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(int id, StockOutStatus to, string? note)
    {
        var (ok, msg) = await svc.TransitionStockOutStatusAsync(id, to, "Thủ kho", note);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteStockOutAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return ok ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Detail), new { id });
    }
}

public class OrgController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var orgs = await db.Orgs.IgnoreQueryFilters().OrderBy(o => o.CreatedAt).ToListAsync();
        Request.Cookies.TryGetValue(TenantContext.CookieName, out var curKey);
        ViewBag.CurrentKey = curKey ?? TenantContext.DefaultApiKey;
        return View(orgs);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) { TempData["Error"] = "Cần tên tổ chức."; return RedirectToAction(nameof(Index)); }
        var org = new Org { Name = name.Trim(), ApiKey = "svc_" + Guid.NewGuid().ToString("N") };
        db.Orgs.Add(org); await db.SaveChangesAsync();
        SetCookies(org.ApiKey, org.Name);
        TempData["Success"] = $"Đã tạo & chuyển sang \"{org.Name}\".";
        return RedirectToAction("Index", "Home");
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Switch(string apiKey)
    {
        var org = await db.Orgs.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.ApiKey == apiKey);
        if (org == null) { TempData["Error"] = "Không tìm thấy."; return RedirectToAction(nameof(Index)); }
        SetCookies(org.ApiKey, org.Name);
        return RedirectToAction("Index", "Home");
    }
    public IActionResult Reset()
    {
        Response.Cookies.Delete(TenantContext.CookieName); Response.Cookies.Delete("org_name");
        return RedirectToAction("Index", "Home");
    }
    private void SetCookies(string k, string n)
    {
        var o = new CookieOptions { IsEssential = true, Expires = DateTimeOffset.UtcNow.AddDays(30) };
        Response.Cookies.Append(TenantContext.CookieName, k, o); Response.Cookies.Append("org_name", n, o);
    }
}
