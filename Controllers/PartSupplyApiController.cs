using Microsoft.AspNetCore.Mvc;
using MiniService.Models;
using MiniService.Services;

namespace MiniService.Controllers;

// =============================================================================================
// API cho 3 màn port 1:1 từ 2021.1.TCMotor/Dev/DMSCarSv. Mỗi endpoint trả đúng bộ cột mà grid
// nguồn hiển thị, để trang .html bên đích dựng lại được cùng một bảng.
// =============================================================================================

/// <summary>
/// Phiếu yêu cầu xuất kho phụ tùng theo RO (Ser_ROStockRequisition).
/// Port 1:1 Views/Services/FrmROStockRequisitionList.cs + Views/Services/FrmROStockRequisition.cs.
/// </summary>
[ApiController]
[Route("api/v1/stockreqs")]
[Produces("application/json")]
public class StockReqApiController(IStockReqService svc) : ControllerBase
{
    /// <summary>Tìm phiếu theo khoảng ngày / số phiếu / trạng thái / cố vấn dịch vụ.</summary>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] string? roNo, [FromQuery] string? status, [FromQuery] string? assistant)
    {
        var rows = await svc.SearchAsync(from, to, roNo, status, assistant);
        // Bộ cột grid nguồn: RONo(=ROReStockRequisition), StatusName, Note, Assistant, PlateNo, CreatedDate
        // — FrmROStockRequisitionList.cs:61-66.
        return Ok(rows.Select((r, i) => new
        {
            index = i + 1, r.Id, r.StkReqId, roReStockRequisition = r.ROReStockRequisition,
            r.RONo, lsNo = r.LSNo, statusName = r.StatusName, r.Status, r.Note,
            r.Assistant, r.PlateNo, r.FrameNo, r.CreatedDate, r.DealerCode, r.ROId
        }));
    }

    /// <summary>Chi tiết phiếu + danh sách phụ tùng của RO (grid dưới của form nguồn).</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var r = await svc.GetAsync(id);
        if (r == null) return NotFound();
        var lines = await svc.PartLinesAsync(id);
        return Ok(new
        {
            r.Id, r.StkReqId, roReStockRequisition = r.ROReStockRequisition, lsNo = r.LSNo,
            r.RONo, r.Assistant, r.PlateNo, r.FrameNo, r.CreatedDate, r.Status,
            statusName = r.StatusName, r.Note,
            // Cột grid phụ tùng của nguồn: PartCode / VieName / Location / NEED / Unit — FrmROStockRequisition.cs:70-76.
            parts = lines.Select((l, i) => new { index = i + 1, partId = l.PartId, partName = l.Name, need = l.Quantity, l.UnitPrice })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockReqReq req)
    {
        var (ok, msg, id) = await svc.CreateAsync(req.RoId, req.Note, req.Assistant);
        return ok ? Ok(new { id, msg }) : BadRequest(new { error = msg });
    }

    /// <summary>Nút "Tạo phiếu xuất" của nguồn — FrmROStockRequisition.cs:340-348.</summary>
    [HttpPost("{id:int}/finish")]
    public async Task<IActionResult> Finish(int id)
    {
        var (ok, msg) = await svc.FinishAsync(id);
        return ok ? Ok(new { ok, msg }) : BadRequest(new { ok, msg });
    }
}

/// <summary>
/// Báo giá phụ tùng (Ser_Inv_Quote). Port 1:1 Views/Inventory/FrmPartQuotationSearch.cs.
/// </summary>
[ApiController]
[Route("api/v1/partquotations")]
[Produces("application/json")]
public class PartQuotationApiController(IPartQuotationService svc) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? quoteNo, [FromQuery] string? customerName,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? statusValues)
    {
        var sv = string.IsNullOrWhiteSpace(statusValues) ? null : statusValues.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var rows = await svc.SearchAsync(quoteNo, customerName, from, to, sv);
        // Bộ cột grid nguồn: QuoteNo, CreatedDate, LogluBy, SumAmount, Note, CusName, Address, Phone, StatusText
        // — FrmPartQuotationSearch.cs:40-48.
        return Ok(rows.Select((q, i) => new
        {
            index = i + 1, q.Id, q.QuoteNo, q.CreatedDate, logLuBy = q.LogLuBy, sumAmount = q.SumAmount,
            q.Note, cusName = q.Customer?.Name, address = q.Customer?.Address, phone = q.Customer?.Phone,
            statusText = q.StatusText, statusValue = q.StatusValue, soStatus = q.SOStatus, itemCount = q.Items.Count
        }));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var q = await svc.GetAsync(id);
        if (q == null) return NotFound();
        return Ok(new
        {
            q.Id, q.QuoteNo, q.Remark, q.Creator, q.CreatedDate, logLuBy = q.LogLuBy, q.Note,
            q.ReceiveName, q.PaymentMethod, soStatus = q.SOStatus,
            statusValue = q.StatusValue, statusText = q.StatusText, sumAmount = q.SumAmount,
            customer = q.Customer == null ? null : new { q.Customer.Name, q.Customer.Address, q.Customer.Phone, q.Customer.TaxCode },
            items = q.Items.Select((it, i) => new
            {
                index = i + 1, it.Id, partId = it.PartId, it.PartCode, it.PartName,
                it.Quantity, it.Price, it.VAT, it.Factor, it.Note, amount = it.Amount
            })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartQuotationReq req)
    {
        var (ok, msg, id) = await svc.CreateAsync(new PartQuotation
        {
            QuoteNo = req.QuoteNo ?? "", CustomerId = req.CustomerId, Note = req.Note, Remark = req.Remark,
            Creator = req.Creator, LogLuBy = req.Creator, ReceiveName = req.ReceiveName, PaymentMethod = req.PaymentMethod
        });
        return ok ? Ok(new { id, msg }) : BadRequest(new { error = msg });
    }

    [HttpPost("{id:int}/items")]
    public async Task<IActionResult> AddItem(int id, [FromBody] AddQuoteItemReq req)
    {
        var (ok, msg) = await svc.AddItemAsync(id, new PartQuotationItem
        {
            PartId = req.PartId, PartCode = req.PartCode ?? "", PartName = req.PartName ?? "",
            Quantity = req.Quantity, Price = req.Price, VAT = req.VAT, Factor = req.Factor, Note = req.Note
        });
        return ok ? Ok(new { ok, msg }) : BadRequest(new { ok, msg });
    }

    [HttpPost("{id:int}/sostatus")]
    public async Task<IActionResult> SetStockOutStatus(int id, [FromBody] SoStatusReq req)
    {
        var (ok, msg) = await svc.SetStockOutStatusAsync(id, req.SOStatus);
        return ok ? Ok(new { ok, msg }) : BadRequest(new { ok, msg });
    }
}

/// <summary>
/// Đơn đặt hàng phụ tùng (Ser_Part_Order). Port 1:1 Views/PartOrder/FrmOrderPartSearch.cs.
/// </summary>
[ApiController]
[Route("api/v1/partorders")]
[Produces("application/json")]
public class PartOrderApiController(IPartOrderService svc) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? orderNo, [FromQuery] string? orderNoUser,
        [FromQuery] string? confirmNo, [FromQuery] int? supplierId, [FromQuery] string? statuses,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? partCode)
    {
        var st = string.IsNullOrWhiteSpace(statuses) ? null : statuses.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var rows = await svc.SearchAsync(orderNo, orderNoUser, confirmNo, supplierId, st, from, to, partCode);
        // Bộ cột grid nguồn: CreateDate, UserCreate, OrderNo, SupplierName, ConfirmNo, SendDate, AMOUNT,
        // STATUSTEXT, OrderNoUser — FrmOrderPartSearch.cs:107-116.
        return Ok(rows.Select((o, i) => new
        {
            index = i + 1, o.Id, o.CreateDate, userCreate = o.UserCreate, o.OrderNo, o.SupplierName,
            o.ConfirmNo, o.SendDate, amount = o.Amount, statusText = o.StatusText, o.Status,
            orderNoUser = o.OrderNoUser, sumQuantity = o.SumQuantity, sumDeliveryQuantity = o.SumDeliveryQuantity,
            o.VIN, detailCount = o.Details.Count
        }));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var o = await svc.GetAsync(id);
        if (o == null) return NotFound();
        return Ok(new
        {
            o.Id, o.OrderNo, orderNoUser = o.OrderNoUser, o.ConfirmNo, o.DealerCode, supplierId = o.SupplierId,
            o.SupplierName, o.CreateDate, o.SendDate, o.ReceivePartDate, o.ApprovedDate, userCreate = o.UserCreate,
            userApproved = o.UserApproved, o.Status, statusText = o.StatusText, newStatus = o.NewStatus,
            htcConfirm = o.HTCConfirm, isActive = o.IsActive, partialShipment = o.PartialShipment,
            typeOrder = o.TypeOrder, typeTransport = o.TypeTransport, o.VIN, cusCharges = o.CusCharges,
            amount = o.Amount, sumQuantity = o.SumQuantity, sumDeliveryQuantity = o.SumDeliveryQuantity,
            // Cột grid chi tiết của nguồn — FrmOrderPartSearch.cs:156-172 và :125-130.
            details = o.Details.Select((d, i) => new
            {
                index = i + 1, d.Id, partId = d.PartId, d.PartCode, d.PartName, d.EngName, d.Unit, d.Model,
                d.Quantity, orderQuantity = d.OrderQuantity, deliveryQuantity = d.DeliveryQuantity,
                d.Cost, d.VAT, d.Discount, d.Factor, d.Note, htcConfirm = d.HTCConfirm,
                lastDateDelivery = d.LastDateDelivery,
                d.MIP, d.OO, d.BO, d.OH, d.SOQ, d.ICC,
                beforeTax = d.BeforeTax, afterTax = d.AfterTax, pendingDeliveryQty = d.PendingDeliveryQty
            })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartOrderReq req)
    {
        var (ok, msg, id) = await svc.CreateAsync(new PartOrder
        {
            OrderNo = req.OrderNo ?? "", OrderNoUser = req.OrderNoUser, ConfirmNo = req.ConfirmNo,
            SupplierId = req.SupplierId, SupplierName = req.SupplierName, UserCreate = req.UserCreate,
            SendDate = req.SendDate, VIN = req.VIN, TypeOrder = req.TypeOrder, TypeTransport = req.TypeTransport
        });
        return ok ? Ok(new { id, msg }) : BadRequest(new { error = msg });
    }

    [HttpPost("{id:int}/details")]
    public async Task<IActionResult> AddDetail(int id, [FromBody] AddPartOrderDetailReq req)
    {
        var (ok, msg) = await svc.AddDetailAsync(id, new PartOrderDetail
        {
            PartId = req.PartId, PartCode = req.PartCode ?? "", PartName = req.PartName ?? "",
            EngName = req.EngName, Unit = req.Unit, Model = req.Model, Quantity = req.Quantity,
            OrderQuantity = req.OrderQuantity, Cost = req.Cost, VAT = req.VAT, Discount = req.Discount,
            Factor = req.Factor, Note = req.Note,
            MIP = req.MIP, OO = req.OO, BO = req.BO, OH = req.OH, SOQ = req.SOQ, ICC = req.ICC
        });
        return ok ? Ok(new { ok, msg }) : BadRequest(new { ok, msg });
    }

    [HttpPost("details/{detailId:int}/deliver")]
    public async Task<IActionResult> Deliver(int detailId, [FromBody] DeliverReq req)
    {
        var (ok, msg) = await svc.DeliverAsync(detailId, req.Qty);
        return ok ? Ok(new { ok, msg }) : BadRequest(new { ok, msg });
    }

    [HttpPost("{id:int}/status")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] PartOrderStatusReq req)
    {
        var (ok, msg) = await svc.SetStatusAsync(id, req.Status);
        return ok ? Ok(new { ok, msg }) : BadRequest(new { ok, msg });
    }
}

public record CreateStockReqReq(int RoId, string? Note, string? Assistant);
public record CreatePartQuotationReq(string? QuoteNo, int? CustomerId, string? Note, string? Remark, string? Creator, string? ReceiveName, string? PaymentMethod);
public record AddQuoteItemReq(int? PartId, string? PartCode, string? PartName, decimal Quantity, decimal Price, decimal VAT, decimal Factor, string? Note);
public record SoStatusReq(string? SOStatus);
public record CreatePartOrderReq(string? OrderNo, string? OrderNoUser, string? ConfirmNo, int? SupplierId, string? SupplierName, string? UserCreate, DateTime? SendDate, string? VIN, string? TypeOrder, string? TypeTransport);
public record AddPartOrderDetailReq(int? PartId, string? PartCode, string? PartName, string? EngName, string? Unit, string? Model,
    decimal Quantity, decimal? OrderQuantity, decimal Cost, decimal VAT, decimal? Discount, decimal Factor, string? Note,
    decimal? MIP, decimal? OO, decimal? BO, decimal? OH, decimal? SOQ, decimal? ICC);
public record DeliverReq(decimal Qty);
public record PartOrderStatusReq(string Status);
