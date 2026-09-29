namespace MiniService.Models;

// =============================================================================================
// PORT 1:1 từ nguồn thật 2021.1.TCMotor/Dev/DMSCarSv (WinForm) — nghiệp vụ dịch vụ/phụ tùng.
// Tên cột lấy từ hằng số Tbl<Bảng>.<Cột> trong TERP.HTCServiceClient/Common/DbDefine.cs;
// công thức lấy từ TERP.BizCarSv/*.cs. Mỗi trường dẫn nguồn file:dòng ngay tại chỗ.
// =============================================================================================

// ---------- (1) Ser_ROStockRequisition — Phiếu yêu cầu xuất kho phụ tùng theo Lệnh sửa chữa ----
// Nguồn màn: Views/Services/FrmROStockRequisitionList.cs + Views/Services/FrmROStockRequisition.cs
// Hằng cột : Common/DbDefine.cs:1054-1064 (class TblSerROStockRequisition)

/// <summary>Trạng thái phiếu yêu cầu xuất kho — TERP.Constants/Const.Main.cs:197-201.</summary>
public static class StockReqStatus
{
    public const string Create = "CRE";   // Const.Main.cs:199 — Mới tạo
    public const string Finish = "FNS";   // Const.Main.cs:200 — Đã xuất kho
}

/// <summary>
/// Phiếu yêu cầu xuất kho phụ tùng cho một RO (Ser_ROStockRequisition).
/// Port 1:1 FrmROStockRequisitionList.cs:30-38 (cột grid) + FrmROStockRequisition.cs:94-104 (ô form).
/// </summary>
public class RoStockRequisition : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string StkReqId { get; set; } = "";                  // STKREQID — DbDefine.cs:1056
    public string? DealerCode { get; set; }                      // DEALERCODE — DbDefine.cs:1057
    public int? ROId { get; set; }                               // ROID — DbDefine.cs:1058 (khóa RO bên đích)
    public string RONo { get; set; } = "";                       // TblSerRO.RONo — FrmROStockRequisitionList.cs:34
    public string? Assistant { get; set; }                       // TblSerRO.Assistant (cố vấn dịch vụ) — :36
    public string? PlateNo { get; set; }                         // TblSerCar.PlateNo — :30
    public string? FrameNo { get; set; }                         // TblSerCar.FrameNo — FrmROStockRequisition.cs:103
    public DateTime CreatedDate { get; set; } = DateTime.Now;    // CREATEDDATE — DbDefine.cs:1059
    public string Status { get; set; } = StockReqStatus.Create;  // STATUS — DbDefine.cs:1061
    public string? Note { get; set; }                            // NOTE — DbDefine.cs:1060

    /// <summary>RORESTOCKREQUISITION = "PX-" + RONo — SerROStockRequisitionCollection.cs:75.</summary>
    public string ROReStockRequisition => "PX-" + RONo;
    /// <summary>STATUSNAME: CRE thì "Chưa xuất", còn lại "Đã xuất kho" — SerROStockRequisitionCollection.cs:77-81.</summary>
    public string StatusName => Status == StockReqStatus.Create ? "Chưa xuất" : "Đã xuất kho";
    /// <summary>Ô "số lệnh sửa chữa" trên form chi tiết = "LS-" + RONo — FrmROStockRequisition.cs:94.</summary>
    public string LSNo => "LS-" + RONo;
}

// ---------- (2) Ser_Inv_Quote — Báo giá phụ tùng ------------------------------------------------
// Nguồn màn: Views/Inventory/FrmPartQuotationSearch.cs
// Hằng cột : Common/DbDefine.cs:495-523 (TblSerInvQuote + TblSerInvQuotePartItems)

/// <summary>
/// Báo giá phụ tùng (Ser_Inv_Quote). Port 1:1 FrmPartQuotationSearch.cs:40-48.
/// StatusValue/StatusText suy từ SOStatus (trạng thái phiếu xuất) — BizCarSv.Inventory.Quote.cs:1806-1822.
/// </summary>
public class PartQuotation : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string QuoteNo { get; set; } = "";                     // QUOTENO — DbDefine.cs:498
    public string? Remark { get; set; }                           // REMARK — DbDefine.cs:499
    public string? Creator { get; set; }                          // CREATOR — DbDefine.cs:500
    public DateTime CreatedDate { get; set; } = DateTime.Now;     // CREATEDDATE — DbDefine.cs:501
    public string? LogLuBy { get; set; }                          // LOGLUBY (người sửa cuối) — DbDefine.cs:502
    public string? Note { get; set; }                             // NOTE — DbDefine.cs:504
    public string? ReceiveName { get; set; }                      // RECEIVENAME — DbDefine.cs:505
    public string? PaymentMethod { get; set; }                     // PAYMENTMETHOD — DbDefine.cs:506
    /// <summary>SOSTATUS = Ser_Inv_StockOut.Status; NULL nghĩa là chưa có phiếu xuất — Quote.cs:1806.</summary>
    public string? SOStatus { get; set; }
    public int? CustomerId { get; set; }                          // CusID (join ser_customer) — Quote.cs:1757
    public Customer? Customer { get; set; }
    public List<PartQuotationItem> Items { get; set; } = [];

    /// <summary>
    /// SUMAMOUNT = sum(isnull(Quantity,0)*Price + isnull(Quantity,0)*Price*0.01*VAT)
    /// — BizCarSv.Inventory.Quote.cs:1773-1775. CHÚ Ý: bản tổng KHÔNG nhân Factor (khác dòng chi tiết).
    /// </summary>
    public decimal SumAmount => Items.Sum(i => i.Quantity * i.Price + i.Quantity * i.Price * 0.01m * i.VAT);

    /// <summary>STATUSVALUE — BizCarSv.Inventory.Quote.cs:1807-1814.</summary>
    public string StatusValue => SOStatus switch
    {
        null or "" => "1",   // chưa có phiếu xuất thì coi là Mới tạo
        "1" => "3",
        "3" => "4",
        "4" => "5",
        "5" => "6",
        _ => ""
    };

    /// <summary>STATUSTEXT — BizCarSv.Inventory.Quote.cs:1815-1822.</summary>
    public string StatusText => SOStatus switch
    {
        null or "" => "Mới tạo",
        "1" => "Đã tạo phiếu xuất",
        "3" => "Đã xuất",
        "4" => "Đã điều chỉnh phiếu xuất",
        "5" => "Đã hủy phiếu xuất",
        _ => ""
    };
}

/// <summary>Dòng phụ tùng của báo giá (Ser_Inv_QuotePartItems) — DbDefine.cs:513-523.</summary>
public class PartQuotationItem : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int QuoteId { get; set; }                   // QUOTEID — DbDefine.cs:515
    public int? PartId { get; set; }                   // PARTID — DbDefine.cs:516
    public string PartCode { get; set; } = "";         // Ser_MST_Part.PartCode (join) — Quote.cs:1888
    public string PartName { get; set; } = "";         // Ser_MST_Part.VieName (join) — Quote.cs:1888
    public decimal Quantity { get; set; } = 1;         // QUANTITY — DbDefine.cs:520
    public decimal Price { get; set; }                 // PRICE — DbDefine.cs:518
    public decimal VAT { get; set; }                   // VAT — DbDefine.cs:517
    public int? PartPriceId { get; set; }              // PARTPRICEID — DbDefine.cs:521
    public decimal Factor { get; set; } = 1;           // FACTOR, nguồn dùng isnull(...,1) — Quote.cs:1883
    public string? Note { get; set; }                  // NOTE — DbDefine.cs:519
    public PartQuotation Quote { get; set; } = null!;

    /// <summary>
    /// Amount = Qty*Price*Factor + Qty*Price*0.01*VAT*Factor — BizCarSv.Inventory.Quote.cs:1884.
    /// Dòng chi tiết CÓ nhân Factor, khác công thức tổng đầu phiếu — giữ nguyên chỗ lệch của nguồn.
    /// </summary>
    public decimal Amount => Quantity * Price * Factor + Quantity * Price * 0.01m * VAT * Factor;
}

// ---------- (3) Ser_Part_Order — Đơn đặt hàng phụ tùng -----------------------------------------
// Nguồn màn: Views/PartOrder/FrmOrderPartSearch.cs
// Hằng cột : Common/DbDefine.cs (class TblSer_Part_Order / TblSer_Part_OrderDetail)

/// <summary>Trạng thái đơn đặt phụ tùng — TERP.Constants/Const.Main.cs:417-429.</summary>
public static class PartOrderStatus
{
    public const string Create = "1";        // Const.Main.cs:420 — Mới tạo
    public const string Confirm = "CONF";    // Const.Main.cs:421 — Xác nhận
    public const string SIExecuting = "2";   // Const.Main.cs:422 — Hàng đang về
    public const string Finish = "3";        // Const.Main.cs:423 — Kết thúc / Hoàn thành
}

/// <summary>
/// Đơn đặt hàng phụ tùng (Ser_Part_Order). Port 1:1 FrmOrderPartSearch.cs:107-116 (cột grid).
/// Bộ lọc + công thức: BizCarSv.PartOrder.cs:2540-2646 (Ser_Part_OrderGet_StatusList).
/// </summary>
public class PartOrder : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string OrderNo { get; set; } = "";                  // ORDERNO — FrmOrderPartSearch.cs:109
    public string? OrderNoUser { get; set; }                   // ORDERNOUSER — :116
    public string? ConfirmNo { get; set; }                     // CONFIRMNO — :111
    public string? DealerCode { get; set; }                    // DEALERCODE
    public int? SupplierId { get; set; }                       // SUPPLIERID
    public string? SupplierName { get; set; }                  // Ser_MST_Supplier.SupplierName (join) — :110
    public DateTime CreateDate { get; set; } = DateTime.Now;   // CREATEDATE — :107
    public DateTime? SendDate { get; set; }                    // SENDDATE — :112 và :113
    public DateTime? ReceivePartDate { get; set; }             // RECEIVEPARTDATE
    public DateTime? ApprovedDate { get; set; }                // APPROVEDDATE
    public string? UserCreate { get; set; }                    // USERCREATE — :108
    public string? UserApproved { get; set; }                  // USERAPPROVED
    public string Status { get; set; } = PartOrderStatus.Create; // STATUS
    public string? NewStatus { get; set; }                     // NEWSTATUS
    public string? HTCConfirm { get; set; }                    // HTCCONFIRM
    /// <summary>ISACTIVE — nguồn LUÔN lọc si.IsActive = '1' (PartOrder.cs:2549 và :2640).</summary>
    public string IsActive { get; set; } = "1";
    public string? PartialShipment { get; set; }               // PARTIALSHIPMENT
    public string? TypeOrder { get; set; }                     // TYPEORDER
    public string? TypeTransport { get; set; }                 // TYPETRANSPORT
    public string? VIN { get; set; }                           // VIN
    public string? CusCharges { get; set; }                    // CUSCHARGES
    public List<PartOrderDetail> Details { get; set; } = [];

    /// <summary>
    /// AMOUNT = SUM(isnull(Cost,0)*isnull(Quantity,0)*(100+isnull(VAT,0))/100)
    /// — BizCarSv.PartOrder.cs:2599-2604 (#tbl_Amount).
    /// </summary>
    public decimal Amount => Details.Sum(d => d.Cost * d.Quantity * (100 + d.VAT) / 100m);
    /// <summary>SumQuantity = sum(Quantity) — BizCarSv.PartOrder.cs:2570.</summary>
    public decimal SumQuantity => Details.Sum(d => d.Quantity);
    /// <summary>SumDeliveryQuantity = sum(DeliveryQuantity) — BizCarSv.PartOrder.cs:2569.</summary>
    public decimal SumDeliveryQuantity => Details.Sum(d => d.DeliveryQuantity ?? 0);

    /// <summary>STATUSTEXT — BizCarSv.PartOrder.cs:2613-2624.</summary>
    public string StatusText => Status switch
    {
        PartOrderStatus.Create => "Mới tạo",
        PartOrderStatus.Confirm => "Xác nhận",
        PartOrderStatus.SIExecuting => "Hàng đang về",
        PartOrderStatus.Finish => "Hoàn thành",
        _ => ""
    };
}

/// <summary>
/// Dòng phụ tùng của đơn đặt hàng (Ser_Part_OrderDetail).
/// Port 1:1 FrmOrderPartSearch.cs:156-172 và :125-130 (6 cột kho BO/MIP/OH/OO/SOQ/ICC).
/// </summary>
public class PartOrderDetail : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int OrderPartId { get; set; }               // ORDERPARTID
    public int? PartId { get; set; }                   // PARTID
    public string PartCode { get; set; } = "";         // Ser_MST_Part.PartCode (join) — :157
    public string PartName { get; set; } = "";         // Ser_MST_Part.VieName (join) — :158
    public string? EngName { get; set; }               // Ser_MST_Part.EngName — PartOrder.cs:2694
    public string? Unit { get; set; }                  // Ser_MST_Part.Unit — :161
    public string? Model { get; set; }                 // MODEL — :167
    public decimal Quantity { get; set; }              // QUANTITY — :160
    public decimal? OrderQuantity { get; set; }        // ORDERQUANTITY
    public decimal? DeliveryQuantity { get; set; }     // DELIVERYQUANTITY — :172
    public decimal Cost { get; set; }                  // COST — :163
    public decimal VAT { get; set; }                   // VAT — :164
    public decimal? Discount { get; set; }             // DISCOUNT
    public decimal Factor { get; set; } = 1;           // FACTOR
    public string? Note { get; set; }                  // NOTE — :168
    public string? HTCConfirm { get; set; }            // HTCCONFIRM — :169
    public DateTime? LastDateDelivery { get; set; }    // LASTDATEDELIVERY — :171
    // 6 cột tồn/đặt của nguồn (PartOrder.cs:2670-2675) — grid FrmOrderPartSearch.cs:125-130
    public decimal? MIP { get; set; }                  // hàng đang trên đường
    public decimal? OO { get; set; }                   // đang đặt (on order)
    public decimal? BO { get; set; }                   // nợ đơn (back order)
    public decimal? OH { get; set; }                   // tồn kho (on hand)
    public decimal? SOQ { get; set; }                  // số lượng đặt đề xuất
    public decimal? ICC { get; set; }                  // cột ICC của nguồn (chưa rõ nghĩa, xem mục 5 báo cáo)
    public PartOrder Order { get; set; } = null!;

    /// <summary>BEFORETAX = isnull(Cost,0)*isnull(Quantity,0) — BizCarSv.PartOrder.cs:2679.</summary>
    public decimal BeforeTax => Cost * Quantity;
    /// <summary>AFTERTAX = isnull(Cost,0)*isnull(Quantity,0)*(100+isnull(VAT,0))/100 — PartOrder.cs:2680.</summary>
    public decimal AfterTax => Cost * Quantity * (100 + VAT) / 100m;
    /// <summary>
    /// PENDINGDELIVERYQTY: DeliveryQuantity NULL thì bằng Quantity; Quantity-DeliveryQuantity &lt;= 0 thì 0;
    /// còn lại là Quantity-DeliveryQuantity — BizCarSv.PartOrder.cs:2682-2687.
    /// </summary>
    public decimal PendingDeliveryQty => DeliveryQuantity == null ? Quantity
        : (Quantity - DeliveryQuantity.Value <= 0 ? 0 : Quantity - DeliveryQuantity.Value);
}
