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
        ViewBag.ServicePackages = await svc.ServicePackagesForSelectAsync();
        ViewBag.EligibleCampaigns = await svc.GetEligibleCampaignsForCarAsync(ro.CarId);
        ViewBag.PendingBulletins = (!string.IsNullOrWhiteSpace(ro.Car?.Vin))
            ? await svc.CheckVinBulletinsAsync(ro.Car.Vin)
            : new List<BulletinVin>();
        return View(ro);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyBulletin(int id, int bulletinId)
    {
        var (ok, msg, _) = await svc.ApplyBulletinToROAsync(bulletinId, id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyPackage(int id, int packageId)
    {
        var (ok, msg, _) = await svc.ApplyServicePackageToROAsync(packageId, id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyCampaign(int id, int campaignId)
    {
        var (ok, msg, _) = await svc.ApplyCampaignToROAsync(campaignId, id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveCampaign(int id)
    {
        var (ok, msg) = await svc.RemoveCampaignFromROAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
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

public class CustomerCareController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(CustomerCareStatus? status, string? q)
    {
        ViewBag.Status = status;
        ViewBag.Q = q;
        var list = await svc.CustomerCaresAsync(status, q);
        return View(list);
    }

    public async Task<IActionResult> Create(int? roId)
    {
        ViewBag.ROs = await svc.ROsEligibleForCustomerCareAsync();
        ViewBag.SelectedROId = roId;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int roId, string? internalNote)
    {
        if (roId <= 0)
        {
            TempData["Error"] = "Vui lòng chọn Lệnh sửa chữa hoàn tất.";
            ViewBag.ROs = await svc.ROsEligibleForCustomerCareAsync();
            return View();
        }

        try
        {
            var care = new CustomerCare
            {
                ROId = roId,
                Status = CustomerCareStatus.Pending,
                InternalNote = internalNote?.Trim(),
                CreatedBy = "web"
            };

            var id = await svc.CreateCustomerCareAsync(care);
            TempData["Success"] = $"Đã tạo phiếu CSKH {care.CareNo} thành công.";
            return RedirectToAction(nameof(Detail), new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ViewBag.ROs = await svc.ROsEligibleForCustomerCareAsync();
            return View();
        }
    }

    public async Task<IActionResult> Detail(int id)
    {
        var care = await svc.GetCustomerCareAsync(id);
        if (care == null) return NotFound();
        return View(care);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitSurvey(int id, CustomerCareStatus status, bool hasCarProblem,
        int? qualityRating, int? staffRating, bool? willingToReturn, int? facilityRating,
        string? customerFeedback, string? internalNote, string? contactedBy)
    {
        var (ok, msg) = await svc.UpdateCustomerCareSurveyAsync(id, status, hasCarProblem,
            qualityRating, staffRating, willingToReturn, facilityRating,
            customerFeedback, internalNote, contactedBy);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteCustomerCareAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return ok ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Detail), new { id });
    }
}

public class PaymentController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(PaymentStatus? status, string? q, DateTime? fromDate, DateTime? toDate, int? roId)
    {
        ViewBag.Status = status;
        ViewBag.Q = q;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.ROId = roId;
        var list = await svc.PaymentsAsync(status, q, fromDate, toDate, roId);
        return View(list);
    }

    public async Task<IActionResult> Create(int? roId)
    {
        ViewBag.ROs = await svc.ROsForPaymentAsync();
        ViewBag.SelectedROId = roId;
        if (roId.HasValue && roId.Value > 0)
        {
            var ro = await svc.GetROAsync(roId.Value);
            ViewBag.PreloadedRO = ro;
        }
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int roId, DateTime paymentDate, PaymentMethod method, PaymentStatus status,
        string payPersonName, string? payPersonPhone, string? payPersonIdCard, decimal discountAmount,
        decimal paymentAmount, string? transactionRef, string? note, string? cashier)
    {
        if (roId <= 0)
        {
            TempData["Error"] = "Vui lòng chọn Lệnh sửa chữa (RO) cần thanh toán.";
            ViewBag.ROs = await svc.ROsForPaymentAsync();
            ViewBag.SelectedROId = roId;
            return View();
        }

        if (string.IsNullOrWhiteSpace(payPersonName))
        {
            TempData["Error"] = "Vui lòng nhập tên người nộp tiền.";
            ViewBag.ROs = await svc.ROsForPaymentAsync();
            ViewBag.SelectedROId = roId;
            return View();
        }

        try
        {
            var payment = new Payment
            {
                ROId = roId,
                PaymentDate = paymentDate != default ? paymentDate : DateTime.Today,
                Method = method,
                Status = status,
                PayPersonName = payPersonName.Trim(),
                PayPersonPhone = payPersonPhone?.Trim(),
                PayPersonIdCard = payPersonIdCard?.Trim(),
                DiscountAmount = Math.Max(0, discountAmount),
                PaymentAmount = paymentAmount,
                TransactionRef = transactionRef?.Trim(),
                Note = note?.Trim(),
                Cashier = string.IsNullOrWhiteSpace(cashier) ? "Thu ngân" : cashier.Trim(),
                CreatedBy = "web"
            };

            var id = await svc.CreatePaymentAsync(payment);
            TempData["Success"] = $"Đã lập phiếu thu {payment.PaymentNo} thành công (Số tiền: {payment.PaymentAmount:N0}đ).";
            return RedirectToAction(nameof(Detail), new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ViewBag.ROs = await svc.ROsForPaymentAsync();
            ViewBag.SelectedROId = roId;
            return View();
        }
    }

    public async Task<IActionResult> Detail(int id)
    {
        var payment = await svc.GetPaymentAsync(id);
        if (payment == null) return NotFound();
        ViewBag.Next = RoService.AllowedNextPayment(payment.Status);
        return View(payment);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(int id, PaymentStatus to, string? note)
    {
        var (ok, msg) = await svc.TransitionPaymentStatusAsync(id, to, "Thu ngân", note);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeletePaymentAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return ok ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Detail), new { id });
    }
}

public class QuoteController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(QuoteStatus? status, string? q, DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.Status = status;
        ViewBag.Q = q;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        var list = await svc.QuotesAsync(status, q, fromDate, toDate);
        return View(list);
    }

    public async Task<IActionResult> Create(int? customerId, int? carId)
    {
        ViewBag.Customers = await svc.CustomersForSelectAsync();
        ViewBag.Cars = await svc.CarsForSelectAsync();
        ViewBag.Parts = await svc.PartsForSelectAsync();
        ViewBag.SelectedCustomerId = customerId;
        ViewBag.SelectedCarId = carId;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int? customerId, string customerName, string? customerPhone, string? customerAddress,
        int? carId, string? recipientName, PaymentMethod paymentMethod, DateTime? quoteDate, DateTime? validUntil,
        string? remark, string? note, int[]? partIds, string[]? customCodes, string[]? customNames, string[]? units,
        decimal[]? quantities, decimal[]? unitPrices, decimal[]? discountPercents, decimal[]? vatPercents, string[]? itemNotes)
    {
        if (string.IsNullOrWhiteSpace(customerName))
        {
            TempData["Error"] = "Vui lòng nhập tên khách hàng.";
            ViewBag.Customers = await svc.CustomersForSelectAsync();
            ViewBag.Cars = await svc.CarsForSelectAsync();
            ViewBag.Parts = await svc.PartsForSelectAsync();
            return View();
        }

        if (partIds == null || partIds.Length == 0)
        {
            TempData["Error"] = "Vui lòng thêm ít nhất một phụ tùng / dịch vụ vào báo giá.";
            ViewBag.Customers = await svc.CustomersForSelectAsync();
            ViewBag.Cars = await svc.CarsForSelectAsync();
            ViewBag.Parts = await svc.PartsForSelectAsync();
            return View();
        }

        try
        {
            var quote = new Quote
            {
                CustomerId = (customerId.HasValue && customerId.Value > 0) ? customerId : null,
                CustomerName = customerName.Trim(),
                CustomerPhone = customerPhone?.Trim(),
                CustomerAddress = customerAddress?.Trim(),
                CarId = (carId.HasValue && carId.Value > 0) ? carId : null,
                RecipientName = recipientName?.Trim(),
                PaymentMethod = paymentMethod,
                QuoteDate = quoteDate ?? DateTime.Today,
                ValidUntil = validUntil ?? DateTime.Today.AddDays(15),
                Remark = remark?.Trim(),
                Note = note?.Trim(),
                CreatedBy = "web"
            };

            var items = new List<QuoteItem>();
            for (int i = 0; i < partIds.Length; i++)
            {
                var pid = partIds[i];
                var qty = (quantities != null && quantities.Length > i && quantities[i] > 0) ? quantities[i] : 1;
                var price = (unitPrices != null && unitPrices.Length > i && unitPrices[i] >= 0) ? unitPrices[i] : 0;
                var disc = (discountPercents != null && discountPercents.Length > i && discountPercents[i] >= 0) ? discountPercents[i] : 0;
                var vat = (vatPercents != null && vatPercents.Length > i && vatPercents[i] >= 0) ? vatPercents[i] : 8;
                var n = (itemNotes != null && itemNotes.Length > i) ? itemNotes[i]?.Trim() : null;

                var item = new QuoteItem
                {
                    PartId = pid > 0 ? pid : null,
                    Quantity = qty,
                    UnitPrice = price,
                    DiscountPercent = disc,
                    VatPercent = vat,
                    Note = n
                };

                if (pid <= 0)
                {
                    item.PartCode = (customCodes != null && customCodes.Length > i) ? customCodes[i]?.Trim() ?? "PRT" : "PRT";
                    item.PartName = (customNames != null && customNames.Length > i) ? customNames[i]?.Trim() ?? "Phụ tùng / Dịch vụ" : "Phụ tùng / Dịch vụ";
                    item.Unit = (units != null && units.Length > i) ? units[i]?.Trim() ?? "Cái" : "Cái";
                }

                items.Add(item);
            }

            var id = await svc.CreateQuoteAsync(quote, items);
            TempData["Success"] = $"Đã lập Báo giá {quote.QuoteNo} thành công (Tổng tiền: {quote.Total:N0}đ).";
            return RedirectToAction(nameof(Detail), new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ViewBag.Customers = await svc.CustomersForSelectAsync();
            ViewBag.Cars = await svc.CarsForSelectAsync();
            ViewBag.Parts = await svc.PartsForSelectAsync();
            return View();
        }
    }

    public async Task<IActionResult> Detail(int id)
    {
        var quote = await svc.GetQuoteAsync(id);
        if (quote == null) return NotFound();
        ViewBag.Next = RoService.AllowedNextQuote(quote.Status);
        ViewBag.ServicePackages = await svc.ServicePackagesForSelectAsync();
        return View(quote);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyPackage(int id, int packageId)
    {
        var (ok, msg, _) = await svc.ApplyServicePackageToQuoteAsync(packageId, id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    public async Task<IActionResult> Print(int id)
    {
        var quote = await svc.GetQuoteAsync(id);
        if (quote == null) return NotFound();
        return View(quote);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(int id, QuoteStatus to)
    {
        var (ok, msg) = await svc.TransitionQuoteStatusAsync(id, to);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConvertToStockOut(int id)
    {
        var (ok, msg, stockOutId) = await svc.ConvertQuoteToStockOutAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        if (ok && stockOutId.HasValue)
        {
            return RedirectToAction("Detail", "StockOut", new { id = stockOutId.Value });
        }
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConvertToRO(int id, string? technician)
    {
        var (ok, msg, roId) = await svc.ConvertQuoteToROAsync(id, technician);
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
        var (ok, msg) = await svc.DeleteQuoteAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return ok ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Detail), new { id });
    }
}

public class ServicePackageController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(string? q, bool? isPublic, bool? isActive)
    {
        ViewBag.Q = q;
        ViewBag.IsPublic = isPublic;
        ViewBag.IsActive = isActive;
        var list = await svc.ServicePackagesAsync(q, isPublic, isActive);
        return View(list);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Parts = await svc.PartsForSelectAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string packageNo, string name, decimal takingTimeHours, string? description, bool isPublic, bool isActive,
        int[] itemTypes, int[]? partIds, string[]? codes, string[]? names, string[]? units, decimal[]? quantities, decimal[]? unitPrices, decimal[]? vatPercents, int[]? expenseTypes, string[]? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Vui lòng nhập tên gói dịch vụ.";
            ViewBag.Parts = await svc.PartsForSelectAsync();
            return View();
        }

        try
        {
            var package = new ServicePackage
            {
                PackageNo = packageNo?.Trim() ?? "",
                Name = name.Trim(),
                TakingTimeHours = takingTimeHours > 0 ? takingTimeHours : 1.0m,
                Description = description?.Trim(),
                IsPublic = isPublic,
                IsActive = isActive,
                CreatedBy = "web"
            };

            var items = new List<ServicePackageItem>();
            if (itemTypes != null && itemTypes.Length > 0)
            {
                for (int i = 0; i < itemTypes.Length; i++)
                {
                    var type = (LineType)itemTypes[i];
                    var pid = (partIds != null && partIds.Length > i && partIds[i] > 0) ? (int?)partIds[i] : null;
                    var c = (codes != null && codes.Length > i) ? codes[i]?.Trim() ?? "" : "";
                    var n = (names != null && names.Length > i) ? names[i]?.Trim() ?? "" : "";
                    var u = (units != null && units.Length > i) ? units[i]?.Trim() ?? (type == LineType.Labor ? "Lần" : "Cái") : (type == LineType.Labor ? "Lần" : "Cái");
                    var q = (quantities != null && quantities.Length > i && quantities[i] > 0) ? quantities[i] : 1;
                    var p = (unitPrices != null && unitPrices.Length > i && unitPrices[i] >= 0) ? unitPrices[i] : 0;
                    var vat = (vatPercents != null && vatPercents.Length > i && vatPercents[i] >= 0) ? vatPercents[i] : 8;
                    var exp = (expenseTypes != null && expenseTypes.Length > i) ? (ExpenseType)expenseTypes[i] : ExpenseType.Customer;
                    var note = (notes != null && notes.Length > i) ? notes[i]?.Trim() : null;

                    if (string.IsNullOrWhiteSpace(n) && !pid.HasValue) continue;

                    items.Add(new ServicePackageItem
                    {
                        Type = type,
                        PartId = pid,
                        Code = c,
                        Name = n,
                        Unit = u,
                        Quantity = q,
                        UnitPrice = p,
                        VatPercent = vat,
                        ExpenseType = exp,
                        Note = note
                    });
                }
            }

            var id = await svc.CreateServicePackageAsync(package, items);
            TempData["Success"] = $"Đã tạo gói dịch vụ '{package.PackageNo} - {package.Name}' ({items.Count} hạng mục).";
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
        var package = await svc.GetServicePackageAsync(id);
        if (package == null) return NotFound();
        var openStatuses = new[] { ROStatus.Created, ROStatus.Printed, ROStatus.HasRO, ROStatus.Wait4Part, ROStatus.HasPart, ROStatus.InGarage };
        var allROs = await svc.ROsAsync(null, null);
        ViewBag.EligibleROs = allROs.Where(r => openStatuses.Contains(r.Status)).OrderByDescending(r => r.CreatedAt).ToList();
        var allQuotes = await svc.QuotesAsync(null, null, null, null);
        ViewBag.EligibleQuotes = allQuotes.Where(q => q.Status is QuoteStatus.Draft or QuoteStatus.Sent or QuoteStatus.Confirmed).OrderByDescending(q => q.QuoteDate).ToList();
        return View(package);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyToRO(int id, int roId)
    {
        var (ok, msg, _) = await svc.ApplyServicePackageToROAsync(id, roId);
        TempData[ok ? "Success" : "Error"] = msg;
        if (ok) return RedirectToAction("Detail", "RO", new { id = roId });
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyToQuote(int id, int quoteId)
    {
        var (ok, msg, _) = await svc.ApplyServicePackageToQuoteAsync(id, quoteId);
        TempData[ok ? "Success" : "Error"] = msg;
        if (ok) return RedirectToAction("Detail", "Quote", new { id = quoteId });
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteServicePackageAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }
}

public class OrderPartController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(OrderPartStatus? status, string? q, DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.Status = status;
        ViewBag.Q = q;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        var list = await svc.OrderPartsAsync(status, q, fromDate, toDate);
        return View(list);
    }

    public async Task<IActionResult> Create(int? roId)
    {
        ViewBag.Parts = await svc.PartsForSelectAsync();
        ViewBag.ROs = await svc.ROsWaitingForPartAsync();
        ViewBag.SelectedROId = roId;
        if (roId.HasValue && roId.Value > 0)
        {
            var ro = await svc.GetROAsync(roId.Value);
            ViewBag.PreloadedRO = ro;
        }
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string supplierName, OrderPartDeliveryForm deliveryForm, string? deliveryLocation,
        DateTime? orderDate, DateTime? estimatedDeliverDate, string? vin, int? roId, string? remark,
        int[] partIds, decimal[] quantities, decimal[] unitPrices, decimal[] discountRates, decimal[] vatPercents, string[]? notes)
    {
        if (string.IsNullOrWhiteSpace(supplierName))
        {
            TempData["Error"] = "Vui lòng nhập tên nhà cung cấp.";
            ViewBag.Parts = await svc.PartsForSelectAsync();
            ViewBag.ROs = await svc.ROsWaitingForPartAsync();
            ViewBag.SelectedROId = roId;
            return View();
        }

        if (deliveryForm == OrderPartDeliveryForm.Warranty && string.IsNullOrWhiteSpace(vin))
        {
            TempData["Error"] = "Đơn đặt hàng bảo hành bắt buộc phải có số khung (VIN) xe.";
            ViewBag.Parts = await svc.PartsForSelectAsync();
            ViewBag.ROs = await svc.ROsWaitingForPartAsync();
            ViewBag.SelectedROId = roId;
            return View();
        }

        if (partIds == null || partIds.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn ít nhất một phụ tùng cần đặt hàng.";
            ViewBag.Parts = await svc.PartsForSelectAsync();
            ViewBag.ROs = await svc.ROsWaitingForPartAsync();
            ViewBag.SelectedROId = roId;
            return View();
        }

        try
        {
            var order = new OrderPart
            {
                SupplierName = supplierName.Trim(),
                DeliveryForm = deliveryForm,
                DeliveryLocation = string.IsNullOrWhiteSpace(deliveryLocation) ? "Kho phụ tùng chính" : deliveryLocation.Trim(),
                OrderDate = orderDate ?? DateTime.Today,
                EstimatedDeliverDate = estimatedDeliverDate,
                VIN = vin?.Trim(),
                ROId = (roId.HasValue && roId.Value > 0) ? roId : null,
                Remark = remark?.Trim(),
                CreatedBy = "web"
            };

            var lines = new List<OrderPartLine>();
            for (int i = 0; i < partIds.Length; i++)
            {
                if (partIds[i] <= 0) continue;
                var qty = (quantities != null && i < quantities.Length) ? quantities[i] : 1;
                var price = (unitPrices != null && i < unitPrices.Length) ? unitPrices[i] : 0;
                var disc = (discountRates != null && i < discountRates.Length) ? discountRates[i] : 0;
                var vat = (vatPercents != null && i < vatPercents.Length) ? vatPercents[i] : 8;
                var note = (notes != null && i < notes.Length) ? notes[i] : null;

                lines.Add(new OrderPartLine
                {
                    PartId = partIds[i],
                    Quantity = qty <= 0 ? 1 : qty,
                    UnitPrice = price,
                    DiscountRate = disc < 0 ? 0 : disc,
                    VatPercent = vat < 0 ? 0 : vat,
                    ApprovedQuantity = qty <= 0 ? 1 : qty,
                    Note = note?.Trim()
                });
            }

            if (lines.Count == 0)
            {
                TempData["Error"] = "Chưa có dòng phụ tùng hợp lệ.";
                ViewBag.Parts = await svc.PartsForSelectAsync();
                ViewBag.ROs = await svc.ROsWaitingForPartAsync();
                ViewBag.SelectedROId = roId;
                return View();
            }

            var id = await svc.CreateOrderPartAsync(order, lines);
            TempData["Success"] = $"Đã lập đơn đặt hàng {order.OrderPartNo} thành công (Tổng tiền: {order.Total:N0}đ).";
            return RedirectToAction(nameof(Detail), new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ViewBag.Parts = await svc.PartsForSelectAsync();
            ViewBag.ROs = await svc.ROsWaitingForPartAsync();
            ViewBag.SelectedROId = roId;
            return View();
        }
    }

    public async Task<IActionResult> Detail(int id)
    {
        var order = await svc.GetOrderPartAsync(id);
        if (order == null) return NotFound();
        ViewBag.Next = RoService.AllowedNextOrderPart(order.Status);
        return View(order);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(int id, OrderPartStatus to, string? supplierOrderNo, string? note)
    {
        var (ok, msg) = await svc.TransitionOrderPartStatusAsync(id, to, supplierOrderNo, note);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStockIn(int id, string? approvedBy)
    {
        var (ok, msg, stockInId) = await svc.CreateStockInFromOrderPartAsync(id, approvedBy);
        TempData[ok ? "Success" : "Error"] = msg;
        if (ok && stockInId.HasValue)
        {
            return RedirectToAction("Detail", "StockIn", new { id = stockInId.Value });
        }
        return RedirectToAction(nameof(Detail), new { id });
    }

    public async Task<IActionResult> Print(int id)
    {
        var order = await svc.GetOrderPartAsync(id);
        if (order == null) return NotFound();
        return View(order);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteOrderPartAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return ok ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Detail), new { id });
    }
}

public class CavityController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(CavityType? type, CavityStatus? status, string? q)
    {
        ViewBag.Type = type;
        ViewBag.Status = status;
        ViewBag.Q = q;
        ViewBag.EligibleROs = await svc.ROsEligibleForCavityAsync();
        return View(await svc.CavitiesAsync(type, status, q));
    }

    public async Task<IActionResult> Detail(int id)
    {
        var cavity = await svc.GetCavityAsync(id);
        if (cavity == null) return NotFound();
        ViewBag.EligibleROs = await svc.ROsEligibleForCavityAsync();
        return View(cavity);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? cavityNo, string cavityName, CavityType type, string? liftEquipment, string? areaZone, string? note)
    {
        if (string.IsNullOrWhiteSpace(cavityName))
        {
            TempData["Error"] = "Vui lòng nhập tên khoang sửa chữa.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var cavity = new Cavity
            {
                CavityNo = cavityNo?.Trim() ?? "",
                CavityName = cavityName.Trim(),
                CavityType = type,
                LiftEquipment = liftEquipment?.Trim(),
                AreaZone = areaZone?.Trim(),
                Note = note?.Trim()
            };
            await svc.CreateCavityAsync(cavity);
            TempData["Success"] = $"Đã thêm khoang sửa chữa '{cavity.CavityNo} - {cavity.CavityName}'.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string cavityName, CavityType type, string? liftEquipment, string? areaZone, string? note)
    {
        var (ok, msg) = await svc.UpdateCavityAsync(id, cavityName, type, liftEquipment, areaZone, note);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int cavityId, int roId, string? technician, DateTime? expectedFinish)
    {
        var (ok, msg) = await svc.AssignCarToCavityAsync(cavityId, roId, technician, expectedFinish);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Release(int cavityId, ROStatus? nextRoStatus)
    {
        var (ok, msg) = await svc.ReleaseCavityAsync(cavityId, nextRoStatus);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int cavityId, CavityStatus status, string? note)
    {
        var (ok, msg) = await svc.SetCavityStatusAsync(cavityId, status, note);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteCavityAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }
}

public class ReceptionController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(ReceptionStatus? status, string? q, DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.Status = status;
        ViewBag.Q = q;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        var list = await svc.ReceptionsAsync(status, q, fromDate, toDate);
        return View(list);
    }

    public async Task<IActionResult> Create(int? appointmentId, int? carId)
    {
        ViewBag.Cars = await svc.CarsForSelectAsync();
        ViewBag.Appointments = await svc.AppointmentsEligibleForReceptionAsync();
        ViewBag.DefaultChecklist = svc.GetDefaultChecklistItems();
        ViewBag.InspectionLevels = Ui.InspectionLevels;

        Appointment? app = null;
        if (appointmentId.HasValue && appointmentId.Value > 0)
        {
            app = await svc.GetAppointmentAsync(appointmentId.Value);
            if (app != null)
            {
                ViewBag.SelectedAppointment = app;
                ViewBag.SelectedCarId = app.CarId;
            }
        }
        else if (carId.HasValue && carId.Value > 0)
        {
            ViewBag.SelectedCarId = carId.Value;
        }

        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int carId, int? appointmentId, int odometer, int fuelLevel,
        string levelOfInspection, string customerRequest, string? valuablesInCar, string? exteriorCondition,
        bool isWarranty, bool isInsurance, bool isBackRepair, string? createdBy,
        string[]? groups, string[]? codes, string[]? names, int[]? receptionStatuses, string[]? notes)
    {
        if (carId <= 0)
        {
            TempData["Error"] = "Vui lòng chọn xe tiếp nhận dịch vụ.";
            return RedirectToAction(nameof(Create), new { appointmentId, carId });
        }

        if (string.IsNullOrWhiteSpace(customerRequest))
        {
            TempData["Error"] = "Vui lòng nhập yêu cầu của khách hàng hoặc triệu chứng xe.";
            return RedirectToAction(nameof(Create), new { appointmentId, carId });
        }

        try
        {
            var sheet = new ReceptionSheet
            {
                CarId = carId,
                AppointmentId = (appointmentId.HasValue && appointmentId.Value > 0) ? appointmentId : null,
                Odometer = odometer,
                FuelLevel = fuelLevel >= 1 && fuelLevel <= 4 ? fuelLevel : 2,
                LevelOfInspection = string.IsNullOrWhiteSpace(levelOfInspection) ? "Bảo dưỡng 10.000 km" : levelOfInspection.Trim(),
                CustomerRequest = customerRequest.Trim(),
                ValuablesInCar = valuablesInCar?.Trim(),
                ExteriorCondition = exteriorCondition?.Trim(),
                IsWarranty = isWarranty,
                IsInsurance = isInsurance,
                IsBackRepair = isBackRepair,
                CreatedBy = !string.IsNullOrWhiteSpace(createdBy) ? createdBy.Trim() : "Cố vấn dịch vụ"
            };

            var items = new List<ReceptionItem>();
            if (codes != null && codes.Length > 0)
            {
                for (int i = 0; i < codes.Length; i++)
                {
                    var code = codes[i];
                    var grp = (groups != null && groups.Length > i) ? groups[i] : "";
                    var nm = (names != null && names.Length > i) ? names[i] : "";
                    var st = (receptionStatuses != null && receptionStatuses.Length > i) ? (AuditStatus)receptionStatuses[i] : AuditStatus.Good;
                    var nt = (notes != null && notes.Length > i) ? notes[i] : null;

                    items.Add(new ReceptionItem
                    {
                        Group = grp,
                        Code = code,
                        Name = nm,
                        ReceptionStatus = st,
                        DeliveryStatus = AuditStatus.Good,
                        Note = nt?.Trim()
                    });
                }
            }

            var id = await svc.CreateReceptionAsync(sheet, items);
            TempData["Success"] = $"Đã tạo phiếu tiếp nhận & kiểm tra xe {sheet.ReceptionNo} thành công!";
            return RedirectToAction(nameof(Detail), new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Create), new { appointmentId, carId });
        }
    }

    public async Task<IActionResult> Detail(int id)
    {
        var sheet = await svc.GetReceptionAsync(id);
        if (sheet == null) return NotFound();
        return View(sheet);
    }

    public async Task<IActionResult> Print(int id)
    {
        var sheet = await svc.GetReceptionAsync(id);
        if (sheet == null) return NotFound();
        return View(sheet);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRO(int id, string? technician)
    {
        var (ok, msg, roId) = await svc.CreateROFromReceptionAsync(id, technician);
        TempData[ok ? "Success" : "Error"] = msg;
        if (ok && roId.HasValue) return RedirectToAction("Detail", "RO", new { id = roId.Value });
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deliver(int id, string? deliveryBy, string? note, int[]? itemIds, int[]? deliveryStatuses)
    {
        List<(int itemId, AuditStatus status)>? deliveryItems = null;
        if (itemIds != null && deliveryStatuses != null && itemIds.Length == deliveryStatuses.Length)
        {
            deliveryItems = [];
            for (int i = 0; i < itemIds.Length; i++)
            {
                deliveryItems.Add((itemIds[i], (AuditStatus)deliveryStatuses[i]));
            }
        }

        var (ok, msg) = await svc.DeliverCarAsync(id, deliveryBy, note, deliveryItems);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? reason)
    {
        var (ok, msg) = await svc.CancelReceptionAsync(id, reason);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteReceptionAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }
}

public class AssignmentWorkController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(AssignmentWorkStatus? status, string? q, DateTime? fromDate, DateTime? toDate, int? roId)
    {
        ViewBag.Status = status;
        ViewBag.Q = q;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.ROId = roId;

        var list = await svc.AssignmentWorksAsync(status, q, fromDate, toDate, roId);
        return View(list);
    }

    public async Task<IActionResult> Create(int? roId)
    {
        ViewBag.SelectedROId = roId;
        ViewBag.EligibleROs = await svc.ROsEligibleForAssignmentAsync();
        ViewBag.Cavities = await svc.CavitiesForSelectAsync();
        ViewBag.Engineers = await svc.EngineersForSelectAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int roId,
        DateTime? sccPlanStart, DateTime? sccPlanFinish, int? sccCavityId,
        DateTime? scdPlanStart, DateTime? scdPlanFinish, int? scdCavityId,
        DateTime? scsPlanStart, DateTime? scsPlanFinish, int? scsCavityId,
        string? note, string? createdBy,
        int[]? engineerIds, int[]? workTypes, decimal[]? assignedHours, int? primaryEngineerId)
    {
        if (roId <= 0)
        {
            TempData["Error"] = "Vui lòng chọn Lệnh sửa chữa (RO) cần phân công.";
            return RedirectToAction(nameof(Create), new { roId });
        }

        try
        {
            var assignment = new AssignmentWork
            {
                ROId = roId,
                Status = AssignmentWorkStatus.Assigned,
                SCCPlanStartDTime = sccPlanStart,
                SCCPlanFinishDTime = sccPlanFinish,
                SCCCavityId = (sccCavityId.HasValue && sccCavityId.Value > 0) ? sccCavityId : null,
                SCDPlanStartDTime = scdPlanStart,
                SCDPlanFinishDTime = scdPlanFinish,
                SCDCavityId = (scdCavityId.HasValue && scdCavityId.Value > 0) ? scdCavityId : null,
                SCSPlanStartDTime = scsPlanStart,
                SCSPlanFinishDTime = scsPlanFinish,
                SCSCavityId = (scsCavityId.HasValue && scsCavityId.Value > 0) ? scsCavityId : null,
                Note = note?.Trim(),
                CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "Quản đốc xưởng" : createdBy.Trim()
            };

            var engineers = new List<AssignmentEngineer>();
            if (engineerIds != null && engineerIds.Length > 0)
            {
                for (int i = 0; i < engineerIds.Length; i++)
                {
                    var engId = engineerIds[i];
                    if (engId <= 0) continue;
                    var wt = (workTypes != null && workTypes.Length > i) ? (WorkType)workTypes[i] : WorkType.SCC;
                    var hrs = (assignedHours != null && assignedHours.Length > i && assignedHours[i] > 0) ? assignedHours[i] : 1.0m;
                    var isPrim = primaryEngineerId.HasValue ? (engId == primaryEngineerId.Value) : (i == 0);

                    engineers.Add(new AssignmentEngineer
                    {
                        EngineerId = engId,
                        WorkType = wt,
                        AssignedHours = hrs,
                        IsPrimary = isPrim
                    });
                }
            }

            var id = await svc.CreateAssignmentWorkAsync(assignment, engineers);
            TempData["Success"] = $"Đã lập phiếu phân công sửa chữa {assignment.AssignmentNo} thành công!";
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
        var item = await svc.GetAssignmentWorkAsync(id);
        if (item == null) return NotFound();
        ViewBag.Next = RoService.AllowedNextAssignment(item.Status);
        return View(item);
    }

    public async Task<IActionResult> Print(int id)
    {
        var item = await svc.GetAssignmentWorkAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int id, string? startedBy)
    {
        var (ok, msg) = await svc.StartAssignmentWorkAsync(id, startedBy);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id, string? completedBy)
    {
        var (ok, msg) = await svc.CompleteAssignmentWorkAsync(id, completedBy);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? reason)
    {
        var (ok, msg) = await svc.CancelAssignmentWorkAsync(id, reason);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteAssignmentWorkAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Engineers(string? q, int? groupId)
    {
        ViewBag.Q = q;
        ViewBag.GroupId = groupId;
        ViewBag.Groups = await svc.GroupRepairsForSelectAsync();
        var engineers = await svc.EngineersAsync(q, groupId, null);
        var groups = await svc.GroupRepairsAsync(null, null);
        ViewBag.AllGroups = groups;
        return View(engineers);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEngineer(string? code, string name, string? phone, string? skillLevel, string? specialty, int? groupId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Vui lòng nhập họ tên kỹ thuật viên.";
            return RedirectToAction(nameof(Engineers));
        }

        try
        {
            var eng = new Engineer
            {
                EngineerNo = code?.Trim() ?? "",
                EngineerName = name.Trim(),
                Phone = phone?.Trim(),
                SkillLevel = string.IsNullOrWhiteSpace(skillLevel) ? "Bậc 3/7" : skillLevel.Trim(),
                Specialty = string.IsNullOrWhiteSpace(specialty) ? "Sửa chữa chung" : specialty.Trim(),
                GroupRId = (groupId.HasValue && groupId.Value > 0) ? groupId : null
            };
            await svc.CreateEngineerAsync(eng);
            TempData["Success"] = $"Đã thêm kỹ thuật viên '{eng.EngineerNo} - {eng.EngineerName}'.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Engineers));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGroup(string? code, string name, string? leader, string? note)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Vui lòng nhập tên tổ sửa chữa.";
            return RedirectToAction(nameof(Engineers));
        }

        try
        {
            var grp = new GroupRepair
            {
                GroupRNo = code?.Trim() ?? "",
                GroupRName = name.Trim(),
                LeaderName = leader?.Trim() ?? "",
                Note = note?.Trim()
            };
            await svc.CreateGroupRepairAsync(grp);
            TempData["Success"] = $"Đã thêm tổ thợ '{grp.GroupRNo} - {grp.GroupRName}'.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Engineers));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEngineer(int id)
    {
        var (ok, msg) = await svc.DeleteEngineerAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Engineers));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var (ok, msg) = await svc.DeleteGroupRepairAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Engineers));
    }
}

public class InsuranceController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(InsuranceClaimStatus? status, string? q, int? companyId)
    {
        ViewBag.Status = status;
        ViewBag.Q = q;
        ViewBag.CompanyId = companyId;
        ViewBag.Companies = await svc.InsuranceCompaniesForSelectAsync();

        var claims = await svc.InsuranceClaimsAsync(status, q, null, companyId);
        var allClaims = await svc.InsuranceClaimsAsync(null, null, null, null);

        ViewBag.TotalCount = allClaims.Count;
        ViewBag.PendingCount = allClaims.Count(c => c.Status == InsuranceClaimStatus.Draft || c.Status == InsuranceClaimStatus.Submitted);
        ViewBag.ApprovedCount = allClaims.Count(c => c.Status == InsuranceClaimStatus.Approved);
        ViewBag.SettledCount = allClaims.Count(c => c.Status == InsuranceClaimStatus.Settled);
        ViewBag.TotalInsuranceAmount = allClaims.Where(c => c.Status == InsuranceClaimStatus.Approved || c.Status == InsuranceClaimStatus.Settled)
            .Sum(c => c.InsuranceAmount);

        return View(claims);
    }

    public async Task<IActionResult> Create(int? roId)
    {
        ViewBag.ROs = await svc.ROsEligibleForInsuranceAsync();
        ViewBag.Companies = await svc.InsuranceCompaniesForSelectAsync();
        ViewBag.Contracts = await svc.InsuranceContractsForSelectAsync();

        RepairOrder? ro = null;
        if (roId.HasValue && roId.Value > 0)
        {
            ro = await svc.GetROAsync(roId.Value);
        }
        ViewBag.SelectedRO = ro;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int roId, int companyId, int? contractId, string policyNo, string? claimFileNo,
        string? surveyorName, string? surveyorPhone, DateTime accidentDate, string? accidentLocation,
        string? accidentDescription, decimal deductibleAmount, decimal penaltyAmount, string? createdBy,
        int[]? lineTypes, string[]? itemCodes, string[]? itemNames, decimal[]? itemQtys, decimal[]? itemPrices,
        decimal[]? itemEstAmounts, decimal[]? itemAppAmounts, string[]? itemNotes)
    {
        if (roId <= 0 || companyId <= 0 || string.IsNullOrWhiteSpace(policyNo))
        {
            TempData["Error"] = "Vui lòng chọn Lệnh sửa chữa RO, Hãng bảo hiểm và nhập Số đơn bảo hiểm.";
            return RedirectToAction(nameof(Create), new { roId });
        }

        try
        {
            var items = new List<InsuranceClaimItem>();
            if (itemCodes != null && itemCodes.Length > 0)
            {
                for (int i = 0; i < itemCodes.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(itemNames?[i])) continue;
                    var type = (lineTypes != null && i < lineTypes.Length && lineTypes[i] == 1) ? LineType.Part : LineType.Labor;
                    var qty = (itemQtys != null && i < itemQtys.Length) ? itemQtys[i] : 1;
                    var price = (itemPrices != null && i < itemPrices.Length) ? itemPrices[i] : 0;
                    var est = (itemEstAmounts != null && i < itemEstAmounts.Length && itemEstAmounts[i] > 0) ? itemEstAmounts[i] : (qty * price);
                    var app = (itemAppAmounts != null && i < itemAppAmounts.Length && itemAppAmounts[i] > 0) ? itemAppAmounts[i] : est;
                    var note = (itemNotes != null && i < itemNotes.Length) ? itemNotes[i] : null;

                    items.Add(new InsuranceClaimItem
                    {
                        Type = type,
                        Code = itemCodes[i].Trim(),
                        Name = itemNames[i].Trim(),
                        Quantity = qty,
                        UnitPrice = price,
                        EstimatedAmount = est,
                        ApprovedAmount = app,
                        IsApproved = true,
                        Note = note
                    });
                }
            }

            int claimId;
            if (items.Count > 0)
            {
                var claim = new InsuranceClaim
                {
                    ROId = roId,
                    InsuranceCompanyId = companyId,
                    InsuranceContractId = (contractId.HasValue && contractId.Value > 0) ? contractId.Value : null,
                    PolicyNo = policyNo.Trim(),
                    ClaimFileNo = claimFileNo?.Trim(),
                    SurveyorName = surveyorName?.Trim(),
                    SurveyorPhone = surveyorPhone?.Trim(),
                    AccidentDate = accidentDate != default ? accidentDate : DateTime.Today,
                    AccidentLocation = accidentLocation?.Trim(),
                    AccidentDescription = accidentDescription?.Trim() ?? "Tổn thất thân vỏ xe",
                    DeductibleAmount = deductibleAmount,
                    PenaltyAmount = penaltyAmount,
                    CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "Cố vấn dịch vụ" : createdBy.Trim(),
                    Status = InsuranceClaimStatus.Draft
                };
                claimId = await svc.CreateInsuranceClaimAsync(claim, items);
            }
            else
            {
                claimId = await svc.CreateInsuranceClaimFromROAsync(
                    roId, companyId, contractId, policyNo, claimFileNo, surveyorName, surveyorPhone,
                    accidentDescription ?? "", deductibleAmount, penaltyAmount, createdBy ?? "Cố vấn dịch vụ");
            }

            TempData["Success"] = "Đã lập Hồ sơ bồi thường bảo hiểm thành công.";
            return RedirectToAction(nameof(Detail), new { id = claimId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Create), new { roId });
        }
    }

    public async Task<IActionResult> Detail(int id)
    {
        var claim = await svc.GetInsuranceClaimAsync(id);
        if (claim == null) return NotFound();

        ViewBag.AllowedNext = RoService.AllowedInsuranceNext(claim.Status);
        return View(claim);
    }

    public async Task<IActionResult> Print(int id)
    {
        var claim = await svc.GetInsuranceClaimAsync(id);
        if (claim == null) return NotFound();
        return View(claim);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(int id, InsuranceClaimStatus toStatus, decimal? approvedAmount, string? note)
    {
        var (ok, msg) = await svc.TransitionInsuranceClaimAsync(id, toStatus, approvedAmount, note);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteInsuranceClaimAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Companies(string? q)
    {
        ViewBag.Q = q;
        var companies = await svc.InsuranceCompaniesAsync(q);
        ViewBag.Contracts = await svc.InsuranceContractsAsync(null, null);
        return View(companies);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCompany(string? insNo, string insName, string? phone, string? email, string? address, string? taxCode, string? hotline)
    {
        if (string.IsNullOrWhiteSpace(insName))
        {
            TempData["Error"] = "Vui lòng nhập tên công ty bảo hiểm.";
            return RedirectToAction(nameof(Companies));
        }

        try
        {
            var c = new InsuranceCompany
            {
                InsNo = insNo?.Trim() ?? "",
                InsName = insName.Trim(),
                Phone = phone?.Trim(),
                Email = email?.Trim(),
                Address = address?.Trim(),
                TaxCode = taxCode?.Trim(),
                Hotline = hotline?.Trim(),
                IsActive = true
            };
            await svc.CreateInsuranceCompanyAsync(c);
            TempData["Success"] = $"Đã thêm hãng bảo hiểm '{c.InsName}'.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Companies));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateContract(string? contractNo, string? contractCode, int companyId, DateTime startDate, DateTime finishDate, InsurancePaymentType paymentType, decimal paymentLimit, decimal discountLaborRate, decimal discountPartRate, string? note)
    {
        if (companyId <= 0)
        {
            TempData["Error"] = "Vui lòng chọn hãng bảo hiểm ký hợp đồng.";
            return RedirectToAction(nameof(Companies));
        }

        try
        {
            var ct = new InsuranceContract
            {
                ContractNo = contractNo?.Trim() ?? "",
                ContractCode = contractCode?.Trim() ?? "",
                InsuranceCompanyId = companyId,
                StartDate = startDate != default ? startDate : DateTime.Today,
                FinishDate = finishDate != default ? finishDate : DateTime.Today.AddYears(1),
                PaymentType = paymentType,
                PaymentLimit = paymentLimit > 0 ? paymentLimit : 500_000_000m,
                DiscountLaborRate = discountLaborRate,
                DiscountPartRate = discountPartRate,
                Note = note?.Trim(),
                IsActive = true
            };
            await svc.CreateInsuranceContractAsync(ct);
            TempData["Success"] = $"Đã tạo hợp đồng bảo hiểm '{ct.ContractNo}'.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Companies));
    }
}

public class CampaignController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(CampaignMarketingStatus? status, string? q, bool? currentOnly)
    {
        ViewBag.Status = status;
        ViewBag.Q = q;
        ViewBag.CurrentOnly = currentOnly;
        var list = await svc.CampaignMarketingsAsync(status, q, currentOnly);
        return View(list);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Parts = await svc.PartsForSelectAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string? camMarketingNo, string camMarketingName, string? camMarketingDesc,
        DateTime effDateStart, DateTime effDateEnd,
        string? conditionModel, string? conditionPlateNo, string? conditionVIN,
        decimal discountLaborPercent, decimal discountPartPercent,
        string? createdBy,
        int[]? partIds, string[]? partCodes, string[]? partNames, decimal[]? percentDiscounts, decimal[]? maxQuantities, string[]? notes)
    {
        if (string.IsNullOrWhiteSpace(camMarketingName))
        {
            TempData["Error"] = "Vui lòng nhập tên chiến dịch khuyến mãi.";
            ViewBag.Parts = await svc.PartsForSelectAsync();
            return View();
        }

        try
        {
            var campaign = new CampaignMarketing
            {
                CamMarketingNo = camMarketingNo?.Trim() ?? "",
                CamMarketingName = camMarketingName.Trim(),
                CamMarketingDesc = camMarketingDesc?.Trim(),
                EffDateStart = effDateStart != default ? effDateStart : DateTime.Today,
                EffDateEnd = effDateEnd != default ? effDateEnd : DateTime.Today.AddMonths(1),
                ConditionModel = conditionModel?.Trim(),
                ConditionPlateNo = conditionPlateNo?.Trim(),
                ConditionVIN = conditionVIN?.Trim(),
                DiscountLaborPercent = discountLaborPercent,
                DiscountPartPercent = discountPartPercent,
                CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "Cố vấn dịch vụ" : createdBy.Trim(),
                Status = CampaignMarketingStatus.Active
            };

            var items = new List<CampaignMarketingItem>();
            if (partIds != null && partIds.Length > 0)
            {
                for (int i = 0; i < partIds.Length; i++)
                {
                    if (partIds[i] <= 0) continue;
                    var code = (partCodes != null && i < partCodes.Length) ? partCodes[i] : "";
                    var name = (partNames != null && i < partNames.Length) ? partNames[i] : "";
                    var disc = (percentDiscounts != null && i < percentDiscounts.Length) ? percentDiscounts[i] : 10;
                    var maxQ = (maxQuantities != null && i < maxQuantities.Length && maxQuantities[i] > 0) ? maxQuantities[i] : 999;
                    var note = (notes != null && i < notes.Length) ? notes[i] : null;

                    items.Add(new CampaignMarketingItem
                    {
                        PartId = partIds[i],
                        PartCode = code.Trim(),
                        PartName = name.Trim(),
                        PercentDiscount = disc,
                        MaxQuantity = maxQ,
                        Note = note?.Trim()
                    });
                }
            }

            var id = await svc.CreateCampaignMarketingAsync(campaign, items);
            TempData["Success"] = $"Đã tạo chiến dịch khuyến mãi '{campaign.CamMarketingNo} - {campaign.CamMarketingName}' thành công!";
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
        var campaign = await svc.GetCampaignMarketingAsync(id);
        if (campaign == null) return NotFound();
        ViewBag.Next = RoService.AllowedNextCampaign(campaign.Status);
        ViewBag.EligibleROs = await svc.ROsEligibleForCampaignAsync(id);
        return View(campaign);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(int id, CampaignMarketingStatus to, string? approvedBy)
    {
        var (ok, msg) = await svc.TransitionCampaignStatusAsync(id, to, approvedBy);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyToRO(int campaignId, int roId)
    {
        var (ok, msg, _) = await svc.ApplyCampaignToROAsync(campaignId, roId);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id = campaignId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromRO(int roId, int returnCampaignId)
    {
        var (ok, msg) = await svc.RemoveCampaignFromROAsync(roId);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id = returnCampaignId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteCampaignMarketingAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }
}

public class CareMaceController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(CustomerCareMaceStatus? status, MaceType? maceType, string? timeFilter, string? q)
    {
        ViewBag.Status = status;
        ViewBag.MaceType = maceType;
        ViewBag.TimeFilter = timeFilter;
        ViewBag.Q = q;

        var list = await svc.CustomerCareMacesAsync(status, maceType, timeFilter, q);
        ViewBag.PendingCount = list.Count(m => m.Status == CustomerCareMaceStatus.Pending);
        ViewBag.OverdueCount = list.Count(m => m.IsOverdue);
        ViewBag.BookedCount = list.Count(m => m.Status == CustomerCareMaceStatus.Booked);
        return View(list);
    }

    public async Task<IActionResult> Create(int? carId)
    {
        ViewBag.Cars = await svc.CarsForSelectAsync();
        ViewBag.SelectedCarId = carId;
        if (carId.HasValue && carId.Value > 0)
        {
            var (recDate, mType, nextKm) = await svc.CalculateNextMaintenanceAsync(carId.Value);
            ViewBag.SuggestedDate = recDate.ToString("yyyy-MM-dd");
            ViewBag.SuggestedMaceType = mType;
            ViewBag.SuggestedNextKm = nextKm;
        }
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int carId, MaceType maceType, int lastKm, int nextKm, DateTime maceRecomentDate, string? remark, string? createdBy)
    {
        if (carId <= 0)
        {
            TempData["Error"] = "Vui lòng chọn xe cần lập mốc bảo dưỡng.";
            ViewBag.Cars = await svc.CarsForSelectAsync();
            return View();
        }

        try
        {
            var mace = new CustomerCareMace
            {
                CarId = carId,
                MaceType = maceType,
                LastKm = lastKm,
                NextKm = nextKm,
                MaceRecomentDate = maceRecomentDate != default ? maceRecomentDate : DateTime.Today.AddMonths(6),
                Remark = remark?.Trim(),
                CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "Cố vấn dịch vụ" : createdBy.Trim(),
                Status = CustomerCareMaceStatus.Pending
            };

            var id = await svc.CreateCustomerCareMaceAsync(mace);
            TempData["Success"] = $"Đã lập phiếu nhắc bảo dưỡng {mace.MaceNo} cho mốc {mace.NextKm:N0} km.";
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
        var item = await svc.GetCustomerCareMaceAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCall(int id, CustomerCareMaceStatus status, DateTime? contactDate, DateTime? apointDate, string? remark, string? contactBy)
    {
        var (ok, msg) = await svc.UpdateCustomerCareMaceCallAsync(id, status, contactDate, apointDate, remark, contactBy);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConvertToAppointment(int id, string? advisor, string? cavity, string? note)
    {
        var (ok, msg, appId) = await svc.ConvertMaceToAppointmentAsync(id, advisor, cavity, note);
        TempData[ok ? "Success" : "Error"] = msg;
        if (ok && appId.HasValue)
        {
            return RedirectToAction("Detail", "Appointment", new { id = appId.Value });
        }
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteCustomerCareMaceAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }
}

public class StockAdjController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(StockAdjStatus? status, StockAdjType? type, string? q, DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.Status = status;
        ViewBag.Type = type;
        ViewBag.Q = q;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        var list = await svc.StockAdjsAsync(status, type, q, fromDate, toDate);
        return View(list);
    }

    public async Task<IActionResult> Create(StockAdjType? type, bool? lowStockOnly)
    {
        ViewBag.Type = type ?? StockAdjType.CountBalance;
        ViewBag.LowStockOnly = lowStockOnly ?? false;
        var parts = await svc.PartsAsync(null, lowStockOnly);
        ViewBag.Parts = parts;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        StockAdjType type,
        string? storageCode,
        DateTime? stockAdjDate,
        string? remark,
        string? createdBy,
        int[]? partIds,
        decimal[]? actualQtys,
        string[]? toLocations,
        string[]? notes)
    {
        if (partIds == null || partIds.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn ít nhất một phụ tùng cần kiểm kê / điều chuyển.";
            return RedirectToAction(nameof(Create), new { type });
        }

        try
        {
            var adj = new StockAdj
            {
                Type = type,
                StorageCode = string.IsNullOrWhiteSpace(storageCode) ? "KHO-CHINH" : storageCode.Trim().ToUpperInvariant(),
                StockAdjDate = stockAdjDate ?? DateTime.Today,
                Remark = remark?.Trim(),
                CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "Thủ kho" : createdBy.Trim(),
                Status = StockAdjStatus.Pending
            };

            var items = new List<StockAdjDetail>();
            for (int i = 0; i < partIds.Length; i++)
            {
                var pid = partIds[i];
                if (pid <= 0) continue;

                var actQty = (actualQtys != null && actualQtys.Length > i) ? actualQtys[i] : 0;
                var toLoc = (toLocations != null && toLocations.Length > i) ? toLocations[i]?.Trim() : null;
                var n = (notes != null && notes.Length > i) ? notes[i]?.Trim() : null;

                items.Add(new StockAdjDetail
                {
                    PartId = pid,
                    ActualQuantity = Math.Max(0, actQty),
                    ToLocation = toLoc,
                    Note = n
                });
            }

            if (items.Count == 0)
            {
                TempData["Error"] = "Vui lòng chọn phụ tùng hợp lệ.";
                return RedirectToAction(nameof(Create), new { type });
            }

            var id = await svc.CreateStockAdjAsync(adj, items);
            TempData["Success"] = $"Đã lập phiếu {adj.StockAdjNo} thành công ({items.Count} phụ tùng)!";
            return RedirectToAction(nameof(Detail), new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Create), new { type });
        }
    }

    public async Task<IActionResult> Detail(int id)
    {
        var item = await svc.GetStockAdjAsync(id);
        if (item == null) return NotFound();
        ViewBag.Next = RoService.AllowedNextStockAdj(item.Status);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(int id, StockAdjStatus to, string? approvedBy, string? note)
    {
        var (ok, msg) = await svc.TransitionStockAdjStatusAsync(id, to, approvedBy, note);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateItems(int id, int[]? itemIds, decimal[]? actualQtys, string[]? toLocations, string[]? notes)
    {
        if (itemIds == null || itemIds.Length == 0)
        {
            TempData["Error"] = "Không có danh sách phụ tùng để cập nhật.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var updates = new List<(int itemId, decimal actualQty, string? toLoc, string? note)>();
        for (int i = 0; i < itemIds.Length; i++)
        {
            var iid = itemIds[i];
            var qty = (actualQtys != null && actualQtys.Length > i) ? actualQtys[i] : 0;
            var loc = (toLocations != null && toLocations.Length > i) ? toLocations[i] : null;
            var n = (notes != null && notes.Length > i) ? notes[i] : null;
            updates.Add((iid, qty, loc, n));
        }

        var (ok, msg) = await svc.UpdateStockAdjItemsAsync(id, updates);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    public async Task<IActionResult> Print(int id)
    {
        var item = await svc.GetStockAdjAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteStockAdjAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }
}

public class BulletinController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(BulletinStatus? status, string? q, bool? activeOnly, string? checkVin)
    {
        ViewBag.Status = status;
        ViewBag.Q = q;
        ViewBag.ActiveOnly = activeOnly;
        ViewBag.CheckVin = checkVin;

        if (!string.IsNullOrWhiteSpace(checkVin))
        {
            ViewBag.MatchedVins = await svc.CheckVinBulletinsAsync(checkVin);
        }

        var list = await svc.BulletinsAsync(status, q, activeOnly);
        return View(list);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var item = await svc.GetBulletinAsync(id);
        if (item == null) return NotFound();
        ViewBag.EligibleROs = await svc.ROsAsync(null, null);
        return View(item);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Parts = await svc.PartsForSelectAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string? bulletinNo, string? bulletinNoHMC, string title, string? remark, string? solution,
        DateTime createDate, DateTime? dateExpired, string? userCreate, string? fileNameAttachment,
        LineType[]? itemTypes, int[]? itemPartIds, string[]? itemCodes, string[]? itemNames, string[]? itemUnits, decimal[]? itemQuantities, decimal[]? itemUnitPrices, string[]? itemNotes,
        string? vinListRaw, string? targetModel)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            TempData["Error"] = "Vui lòng nhập tiêu đề Bản tin kỹ thuật.";
            ViewBag.Parts = await svc.PartsForSelectAsync();
            return View();
        }

        try
        {
            var bulletin = new Bulletin
            {
                BulletinNo = bulletinNo?.Trim() ?? "",
                BulletinNoHMC = bulletinNoHMC?.Trim(),
                Title = title.Trim(),
                Remark = remark?.Trim(),
                Solution = solution?.Trim(),
                CreateDate = createDate != default ? createDate : DateTime.Today,
                DateExpired = dateExpired,
                UserCreate = string.IsNullOrWhiteSpace(userCreate) ? "Hyundai Thành Công (HTC)" : userCreate.Trim(),
                FileNameAttachment = fileNameAttachment?.Trim(),
                IsActive = true,
                Status = BulletinStatus.Active,
                CreatedBy = "web"
            };

            var details = new List<BulletinDetail>();
            if (itemNames != null && itemNames.Length > 0)
            {
                for (int i = 0; i < itemNames.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(itemNames[i])) continue;
                    var type = (itemTypes != null && i < itemTypes.Length) ? itemTypes[i] : LineType.Labor;
                    var partId = (itemPartIds != null && i < itemPartIds.Length && itemPartIds[i] > 0) ? (int?)itemPartIds[i] : null;
                    var code = (itemCodes != null && i < itemCodes.Length) ? itemCodes[i]?.Trim() ?? "" : "";
                    var unit = (itemUnits != null && i < itemUnits.Length) ? itemUnits[i]?.Trim() ?? "Cái" : "Cái";
                    var qty = (itemQuantities != null && i < itemQuantities.Length) ? itemQuantities[i] : 1;
                    var price = (itemUnitPrices != null && i < itemUnitPrices.Length) ? itemUnitPrices[i] : 0;
                    var note = (itemNotes != null && i < itemNotes.Length) ? itemNotes[i]?.Trim() : null;

                    details.Add(new BulletinDetail
                    {
                        Type = type,
                        PartId = partId,
                        Code = code,
                        Name = itemNames[i].Trim(),
                        Unit = unit,
                        Quantity = qty <= 0 ? 1 : qty,
                        UnitPrice = price < 0 ? 0 : price,
                        Note = note
                    });
                }
            }

            var vins = new List<BulletinVin>();
            if (!string.IsNullOrWhiteSpace(vinListRaw))
            {
                var rawTokens = vinListRaw.Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                var cars = await svc.CarsAsync(null);

                foreach (var tok in rawTokens)
                {
                    var clean = tok.Trim().ToUpperInvariant();
                    if (string.IsNullOrWhiteSpace(clean)) continue;
                    if (vins.Any(v => v.VinNo == clean)) continue;

                    var matchedCar = cars.FirstOrDefault(c => c.Vin != null && c.Vin.ToUpper() == clean);
                    vins.Add(new BulletinVin
                    {
                        VinNo = clean,
                        PlateNo = matchedCar?.Plate,
                        Model = !string.IsNullOrWhiteSpace(targetModel) ? targetModel.Trim() : (matchedCar?.Model ?? ""),
                        DealerCode = "HYUNDAI-MAIN",
                        Status = BulletinVinStatus.Pending
                    });
                }
            }

            var id = await svc.CreateBulletinAsync(bulletin, details, vins);
            TempData["Success"] = $"Đã ban hành bản tin kỹ thuật {bulletin.BulletinNo} thành công (Áp dụng cho {vins.Count} xe theo số VIN).";
            return RedirectToAction(nameof(Detail), new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ViewBag.Parts = await svc.PartsForSelectAsync();
            return View();
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var (ok, msg) = await svc.ToggleBulletinActiveAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(int id, BulletinStatus to)
    {
        var (ok, msg) = await svc.TransitionBulletinStatusAsync(id, to);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateVinStatus(int vinId, BulletinVinStatus status, int bulletinId, string? doneBy)
    {
        var (ok, msg) = await svc.UpdateBulletinVinStatusAsync(vinId, status, doneBy);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id = bulletinId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVins(int bulletinId, string vinListRaw, string? targetModel)
    {
        if (string.IsNullOrWhiteSpace(vinListRaw))
        {
            TempData["Error"] = "Vui lòng nhập ít nhất một số VIN.";
            return RedirectToAction(nameof(Detail), new { id = bulletinId });
        }

        var tokens = vinListRaw.Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        var (ok, msg) = await svc.AddVinsToBulletinAsync(bulletinId, tokens, targetModel);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id = bulletinId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyToRO(int bulletinId, int roId)
    {
        var (ok, msg, _) = await svc.ApplyBulletinToROAsync(bulletinId, roId);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction("Detail", "RO", new { id = roId });
    }

    public async Task<IActionResult> Print(int id)
    {
        var item = await svc.GetBulletinAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteBulletinAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }
}

public class PdiController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(PdiRequestStatus? status, string? q, DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.Status = status;
        ViewBag.Q = q;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        var list = await svc.PdiRequestsAsync(status, q, fromDate, toDate);
        return View(list);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string? pdiReqNo,
        string? dealerCode,
        DateTime? createdDate,
        string? remark,
        string? createdBy,
        bool flagAccessory,
        string[]? vins,
        string[]? models,
        string[]? specs,
        string[]? colors,
        string[]? contractNos,
        string[]? customerNames,
        string[]? customerPhones,
        DateTime[]? expectedDates,
        bool[]? itemAccessories,
        string[]? accessoryNotes)
    {
        if (vins == null || vins.Length == 0 || string.IsNullOrWhiteSpace(vins[0]))
        {
            TempData["Error"] = "Vui lòng nhập ít nhất một xe (Số khung VIN) cần kiểm tra xuất xưởng PDI.";
            return View();
        }

        try
        {
            var req = new PdiRequest
            {
                PdiReqNo = pdiReqNo?.Trim() ?? "",
                DealerCode = string.IsNullOrWhiteSpace(dealerCode) ? "HYUNDAI-MAIN" : dealerCode.Trim(),
                CreatedDate = createdDate ?? DateTime.Today,
                Remark = remark?.Trim(),
                CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "Phòng Bán hàng (DMS Sales)" : createdBy.Trim(),
                FlagAccessory = flagAccessory,
                Status = PdiRequestStatus.Pending
            };

            var items = new List<PdiRequestItem>();
            for (int i = 0; i < vins.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(vins[i])) continue;
                var itemVin = vins[i].Trim().ToUpperInvariant();
                var itemModel = (models != null && models.Length > i) ? models[i]?.Trim() ?? "Hyundai" : "Hyundai";
                var itemSpec = (specs != null && specs.Length > i) ? specs[i]?.Trim() : null;
                var itemColor = (colors != null && colors.Length > i) ? colors[i]?.Trim() : null;
                var itemContract = (contractNos != null && contractNos.Length > i) ? contractNos[i]?.Trim() ?? "" : "";
                var itemCustomer = (customerNames != null && customerNames.Length > i) ? customerNames[i]?.Trim() ?? "" : "";
                var itemPhone = (customerPhones != null && customerPhones.Length > i) ? customerPhones[i]?.Trim() : null;
                var itemDate = (expectedDates != null && expectedDates.Length > i && expectedDates[i] != default) ? expectedDates[i] : DateTime.Today.AddDays(2);
                var itemHasAcc = (itemAccessories != null && itemAccessories.Length > i) ? itemAccessories[i] : flagAccessory;
                var itemAccNote = (accessoryNotes != null && accessoryNotes.Length > i) ? accessoryNotes[i]?.Trim() : null;

                items.Add(new PdiRequestItem
                {
                    VIN = itemVin,
                    Model = itemModel,
                    Spec = itemSpec,
                    Color = itemColor,
                    ContractNo = itemContract,
                    CustomerName = itemCustomer,
                    CustomerPhone = itemPhone,
                    ExpectedDeliveryDate = itemDate,
                    FlagAccessory = itemHasAcc,
                    AccessoryNote = itemAccNote,
                    Status = PdiItemStatus.Pending
                });
            }

            var id = await svc.CreatePdiRequestAsync(req, items);
            TempData["Success"] = $"Đã lập Phiếu yêu cầu PDI {req.PdiReqNo} thành công ({items.Count} xe cần kiểm tra).";
            return RedirectToAction(nameof(Detail), new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View();
        }
    }

    public async Task<IActionResult> Detail(int id, int? selectedItemId)
    {
        var req = await svc.GetPdiRequestAsync(id);
        if (req == null) return NotFound();
        ViewBag.Next = RoService.AllowedNextPdiRequest(req.Status);
        ViewBag.SelectedItemId = selectedItemId ?? req.Items.FirstOrDefault()?.Id;
        return View(req);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(int id, PdiRequestStatus to, string? approvedBy)
    {
        var (ok, msg) = await svc.TransitionPdiRequestStatusAsync(id, to, approvedBy);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRO(int id, int itemId, string? technician)
    {
        var (ok, msg, roId) = await svc.CreateROFromPdiItemAsync(itemId, technician);
        TempData[ok ? "Success" : "Error"] = msg;
        if (ok && roId.HasValue)
        {
            return RedirectToAction("Detail", "RO", new { id = roId.Value });
        }
        return RedirectToAction(nameof(Detail), new { id, selectedItemId = itemId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateChecklist(
        int id,
        int itemId,
        string? inspector,
        string? notes,
        int[] checkIds,
        int[] statuses,
        string[]? itemNotes)
    {
        var updates = new List<(int checkId, AuditStatus status, string? note)>();
        if (checkIds != null && checkIds.Length > 0)
        {
            for (int i = 0; i < checkIds.Length; i++)
            {
                var s = (statuses != null && statuses.Length > i) ? (AuditStatus)statuses[i] : AuditStatus.Good;
                var n = (itemNotes != null && itemNotes.Length > i) ? itemNotes[i]?.Trim() : null;
                updates.Add((checkIds[i], s, n));
            }
        }

        var (ok, msg) = await svc.UpdatePdiItemChecklistAsync(itemId, updates, inspector, notes);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id, selectedItemId = itemId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PassItem(int id, int itemId, string? inspector)
    {
        var (ok, msg) = await svc.PassPdiItemAsync(itemId, inspector);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id, selectedItemId = itemId });
    }

    public async Task<IActionResult> Print(int id, int? itemId)
    {
        var req = await svc.GetPdiRequestAsync(id);
        if (req == null) return NotFound();
        ViewBag.SelectedItem = itemId.HasValue ? req.Items.FirstOrDefault(i => i.Id == itemId.Value) : req.Items.FirstOrDefault();
        return View(req);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeletePdiRequestAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }
}

public class OrderComplainController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(DMSOrderComplainStatus? dmsStatus, TSTOrderComplainStatus? tstStatus, OrderComplainType? type, string? q, DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.DMSStatus = dmsStatus;
        ViewBag.TSTStatus = tstStatus;
        ViewBag.Type = type;
        ViewBag.Q = q;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        var list = await svc.OrderComplainsAsync(dmsStatus, tstStatus, type, q, fromDate, toDate);
        return View(list);
    }

    public async Task<IActionResult> Create(int? orderPartId)
    {
        ViewBag.Parts = await svc.PartsForSelectAsync();
        ViewBag.OrderParts = await svc.OrderPartsForComplainSelectAsync();
        ViewBag.SelectedOrderPartId = orderPartId;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string? orderComplainNo,
        string? dealerCode,
        string? dealerName,
        OrderComplainType complainType,
        int? orderPartId,
        int partId,
        decimal quantity,
        decimal? unitPrice,
        string? vin,
        string description,
        string? requestOrderNo,
        string? transportUnit,
        DateTime? deliveryDateTime,
        string? deliveryBy,
        string? deliveryLocation,
        string? receiveBy,
        DateTime? assembleDateTime,
        string? assembleBy,
        string? createdBy,
        string[]? imageTypes,
        string[]? fileNames,
        string[]? filePaths,
        string[]? notes)
    {
        if (partId <= 0)
        {
            TempData["Error"] = "Vui lòng chọn phụ tùng cần khiếu nại.";
            ViewBag.Parts = await svc.PartsForSelectAsync();
            ViewBag.OrderParts = await svc.OrderPartsForComplainSelectAsync();
            ViewBag.SelectedOrderPartId = orderPartId;
            return View();
        }

        if (quantity <= 0)
        {
            TempData["Error"] = "Số lượng khiếu nại phải lớn hơn 0.";
            ViewBag.Parts = await svc.PartsForSelectAsync();
            ViewBag.OrderParts = await svc.OrderPartsForComplainSelectAsync();
            ViewBag.SelectedOrderPartId = orderPartId;
            return View();
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            TempData["Error"] = "Vui lòng nhập mô tả chi tiết tình trạng hư hỏng / sai quy cách.";
            ViewBag.Parts = await svc.PartsForSelectAsync();
            ViewBag.OrderParts = await svc.OrderPartsForComplainSelectAsync();
            ViewBag.SelectedOrderPartId = orderPartId;
            return View();
        }

        try
        {
            var complain = new OrderComplain
            {
                OrderComplainNo = orderComplainNo?.Trim() ?? "",
                DealerCode = string.IsNullOrWhiteSpace(dealerCode) ? "HYUNDAI-MAIN" : dealerCode.Trim(),
                DealerName = string.IsNullOrWhiteSpace(dealerName) ? "Hyundai Giải Phóng" : dealerName.Trim(),
                ComplainType = complainType,
                OrderPartId = (orderPartId.HasValue && orderPartId.Value > 0) ? orderPartId : null,
                PartId = partId,
                Quantity = quantity,
                UnitPrice = unitPrice ?? 0,
                VIN = vin?.Trim(),
                Description = description.Trim(),
                RequestOrderNo = requestOrderNo?.Trim(),
                TransportUnit = transportUnit?.Trim(),
                DeliveryDateTime = deliveryDateTime,
                DeliveryBy = deliveryBy?.Trim(),
                DeliveryLocation = string.IsNullOrWhiteSpace(deliveryLocation) ? "Kho phụ tùng chính" : deliveryLocation.Trim(),
                ReceiveBy = receiveBy?.Trim(),
                AssembleDateTime = assembleDateTime,
                AssembleBy = assembleBy?.Trim(),
                CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "Thủ kho" : createdBy.Trim()
            };

            var files = new List<OrderComplainAttachFile>();
            if (fileNames != null && fileNames.Length > 0)
            {
                for (int i = 0; i < fileNames.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(fileNames[i])) continue;
                    var typeStr = (imageTypes != null && imageTypes.Length > i) ? imageTypes[i]?.Trim() ?? "Ngoại quan hư hại" : "Ngoại quan hư hại";
                    var pathStr = (filePaths != null && filePaths.Length > i && !string.IsNullOrWhiteSpace(filePaths[i])) ? filePaths[i]!.Trim() : $"/uploads/complain/{fileNames[i].Trim()}";
                    var noteStr = (notes != null && notes.Length > i) ? notes[i]?.Trim() : null;
                    files.Add(new OrderComplainAttachFile
                    {
                        ImageType = typeStr,
                        FileName = fileNames[i].Trim(),
                        FilePath = pathStr,
                        Note = noteStr
                    });
                }
            }

            var id = await svc.CreateOrderComplainAsync(complain, files);
            TempData["Success"] = $"Đã lập hồ sơ khiếu nại phụ tùng {complain.OrderComplainNo} thành công.";
            return RedirectToAction(nameof(Detail), new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ViewBag.Parts = await svc.PartsForSelectAsync();
            ViewBag.OrderParts = await svc.OrderPartsForComplainSelectAsync();
            ViewBag.SelectedOrderPartId = orderPartId;
            return View();
        }
    }

    public async Task<IActionResult> Detail(int id)
    {
        var c = await svc.GetOrderComplainAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SendTST(int id)
    {
        var (ok, msg) = await svc.SendOrderComplainToTSTAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, TSTOrderComplainStatus tstStatus, ComplainSolution solution, string? solutionNote)
    {
        var (ok, msg) = await svc.ReviewOrderComplainAsync(id, tstStatus, solution, solutionNote);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    public async Task<IActionResult> Print(int id)
    {
        var c = await svc.GetOrderComplainAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteOrderComplainAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }
}

public class TechnicalLibraryController(IRoService svc) : Controller
{
    public async Task<IActionResult> Index(string? model, TechnicalLibraryReRepairType? reRepairType, TechnicalLibraryType? type, bool? isActive, string? q)
    {
        ViewBag.Model = model;
        ViewBag.ReRepairType = reRepairType;
        ViewBag.Type = type;
        ViewBag.IsActive = isActive;
        ViewBag.Q = q;
        ViewBag.Models = await svc.GetDistinctModelsAsync();

        var list = await svc.TechnicalLibrariesAsync(model, reRepairType, type, isActive, q);
        return View(list);
    }

    public async Task<IActionResult> Create(int? roId)
    {
        ViewBag.Models = await svc.GetDistinctModelsAsync();
        if (roId.HasValue && roId.Value > 0)
        {
            var ro = await svc.GetROAsync(roId.Value);
            if (ro != null)
            {
                ViewBag.RO = ro;
                ViewBag.PreModel = ro.Car?.Model;
                ViewBag.PrePlate = ro.Car?.Plate;
                ViewBag.PreRemark = ro.IntakeNote;
            }
        }
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string model,
        TechnicalLibraryReRepairType reRepairType,
        TechnicalLibraryType type,
        string reRepairRemark,
        string reRepairReason,
        string reRepairSolution,
        string? plateNo,
        string? engine,
        string? gear,
        string? version,
        string? reRepairFeedback,
        string? exclusionTest,
        int? roId,
        string? dealerCode,
        string? dealerName,
        string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            TempData["Error"] = "Vui lòng nhập hoặc chọn dòng xe (Model).";
            return RedirectToAction(nameof(Create), new { roId });
        }
        if (string.IsNullOrWhiteSpace(reRepairRemark))
        {
            TempData["Error"] = "Vui lòng mô tả hiện tượng hư hỏng / triệu chứng pan bệnh.";
            return RedirectToAction(nameof(Create), new { roId });
        }
        if (string.IsNullOrWhiteSpace(reRepairReason))
        {
            TempData["Error"] = "Vui lòng nhập nguyên nhân hư hỏng gốc rễ.";
            return RedirectToAction(nameof(Create), new { roId });
        }
        if (string.IsNullOrWhiteSpace(reRepairSolution))
        {
            TempData["Error"] = "Vui lòng nhập biện pháp khắc phục triệt để đã xử lý.";
            return RedirectToAction(nameof(Create), new { roId });
        }

        var item = new TechnicalLibrary
        {
            Model = model.Trim(),
            ReRepairType = reRepairType,
            Type = type,
            ReRepairRemark = reRepairRemark.Trim(),
            ReRepairReason = reRepairReason.Trim(),
            ReRepairSolution = reRepairSolution.Trim(),
            PlateNo = plateNo?.Trim(),
            Engine = engine?.Trim(),
            Gear = gear?.Trim(),
            Version = version?.Trim(),
            ReRepairFeedback = reRepairFeedback?.Trim(),
            ExclusionTest = exclusionTest?.Trim(),
            ROId = (roId.HasValue && roId.Value > 0) ? roId : null,
            DealerCode = string.IsNullOrWhiteSpace(dealerCode) ? "HYUNDAI-MAIN" : dealerCode.Trim(),
            DealerName = string.IsNullOrWhiteSpace(dealerName) ? "Hyundai Giải Phóng" : dealerName.Trim(),
            CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "Kỹ thuật viên" : createdBy.Trim(),
            IsActive = false
        };

        var id = await svc.CreateTechnicalLibraryAsync(item);
        TempData["Success"] = $"Đã lập hồ sơ cẩm nang kỹ thuật {item.TechnicalLibraryCode} thành công.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var item = await svc.GetTechnicalLibraryAsync(id);
        if (item == null) return NotFound();

        if (item.ROId.HasValue)
        {
            ViewBag.RelatedSolutions = await svc.SearchSolutionsForRoAsync(item.ROId.Value);
        }
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, string? approvedBy)
    {
        var (ok, msg) = await svc.ApproveTechnicalLibraryAsync(id, approvedBy);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.DeleteTechnicalLibraryAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Print(int id)
    {
        var item = await svc.GetTechnicalLibraryAsync(id);
        if (item == null) return NotFound();
        return View(item);
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
