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
        return View(ro);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyPackage(int id, int packageId)
    {
        var (ok, msg, _) = await svc.ApplyServicePackageToROAsync(packageId, id);
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
