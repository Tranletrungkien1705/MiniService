namespace MiniService.Models;

public class Org
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
public interface IOrgOwned { Guid OrgId { get; set; } }

/// <summary>Trạng thái Lệnh Sửa Chữa (RO) — theo state machine idn.CarService.</summary>
public enum ROStatus
{
    Created = 0,       // CRE  — Lập báo giá
    Printed = 1,       // PRT  — In báo giá, chờ KH ký
    Wait4Part = 2,     // W4P  — Đặt phụ tùng, chờ về
    HasPart = 3,       // HPA  — Phụ tùng đã về
    HasRO = 4,         // HRO  — Lập lệnh sửa chữa chính thức
    InGarage = 5,      // INGA — Xe vào xưởng, đang sửa
    Repaired = 6,      // RPRD — Sửa xong, chờ kiểm tra
    CheckEnd = 7,      // CEND — Kiểm tra chất lượng xong
    Paid = 8,          // PAID — Khách đã thanh toán
    Finished = 9,      // FNS  — Giao xe, hoàn tất
    Rejected = 10,     // REJ  — KH từ chối / hủy
    NotResponding = 11 // NORE — Không liên lạc được
}

public enum LineType { Labor = 0, Part = 1 }   // Công / Phụ tùng

/// <summary>Đối tượng chịu chi phí cho từng hạng mục dịch vụ — theo Ser_ROType idn.CarService.</summary>
public enum ExpenseType
{
    Customer = 0,    // ROREPAIR  — Khách hàng thanh toán
    Warranty = 1,    // ROWARRANTY — Bảo hành hãng (HTC/HMC chi trả)
    Insurance = 2,   // ROINSURANCE — Bảo hiểm bồi thường
    Internal = 3     // LOCAL — Nội bộ đại lý hỗ trợ
}

/// <summary>Trạng thái Báo cáo bảo hành (Warranty Report) — theo Ser_WarrantyReport_Status idn.CarService.</summary>
public enum WarrantyStatus
{
    Pending = 0,     // PEND   — Lập báo cáo / Chưa gửi
    Sent = 1,        // SENT   — Đã gửi HTC xem xét
    Confirmed = 2,   // CONF   — HTC đã xác nhận / Chờ duyệt bồi hoàn
    Accepted = 3,    // ACCE   — Hãng chấp thuận bồi hoàn
    Rejected = 4,    // REJ    — Hãng từ chối bồi hoàn
    Reverted = 5     // REVERT — Yêu cầu đại lý bổ sung hồ sơ
}

/// <summary>Trạng thái Cuộc hẹn dịch vụ — theo SerAppStatus idn.CarService.</summary>
public enum AppointmentStatus
{
    Pending = 0,     // PEND   — 1: Mới tạo / Chờ xác nhận
    Contacted = 1,   // CONT   — 5: Đã liên hệ & Chưa xác nhận
    Confirmed = 2,   // CONF   — 2: Đã xác nhận hẹn
    CheckedIn = 3,   // RECV   — 3: Tiếp nhận vào xưởng / Lập RO
    Cancelled = 4    // CANC   — 4: Hủy hẹn
}

/// <summary>Loại dịch vụ đặt hẹn — theo Mst_Ser_AppType idn.CarService.</summary>
public enum AppointmentServiceType
{
    Maintenance = 0,    // Bảo dưỡng định kỳ
    Repair = 1,         // Sửa chữa chung
    BodyPaint = 2,      // Đồng sơn
    WarrantyCheck = 3,  // Kiểm tra & Bảo hành
    Care = 4            // Chăm sóc & Làm đẹp xe
}

/// <summary>Trạng thái Phiếu Nhập kho — theo Ser_Inv_StockIn idn.CarService.</summary>
public enum StockInStatus
{
    Pending = 0,     // 1: Pending   — Mới tạo / Chờ kiểm tra & duyệt
    Executing = 1,   // 2: Executing — Đang kiểm hàng / Tiến hành nhập
    Finished = 2,    // 3: Finished  — Hoàn tất / Đã nhập kho (tăng tồn kho & giá vốn)
    Rejected = 3     // 5: Reject    — Đã hủy phiếu nhập
}

/// <summary>Hình thức nhập kho phụ tùng — theo Ser_Inv_StockInType idn.CarService.</summary>
public enum StockInType
{
    Normal = 0,      // 1: Nomarl      — Nhập mua hàng (Chính hãng HTC / Nhà cung cấp ngoài)
    Adjustment = 1,  // 2: StockInAdj  — Nhập điều chỉnh sau kiểm kê kho
    Return = 2       // 3: Return      — Nhập thu hồi / hoàn trả từ xưởng dịch vụ
}

/// <summary>Trạng thái Phiếu Xuất kho — theo Ser_Inv_StockOut idn.CarService.</summary>
public enum StockOutStatus
{
    Pending = 0,     // 1: Pending   — Mới tạo / Chờ soạn hàng & duyệt xuất
    Executing = 1,   // 2: Executing — Đang soạn hàng / Tiến hành xuất kho
    Finished = 2,    // 3: Finished  — Hoàn tất / Đã xuất kho (giảm tồn kho thực tế)
    Rejected = 3     // 5: Reject    — Đã hủy phiếu xuất
}

/// <summary>Hình thức xuất kho phụ tùng — theo Ser_Inv_StockOutType idn.CarService.</summary>
public enum StockOutType
{
    Service = 0,     // 1: Xuất dịch vụ (Sửa chữa theo lệnh RO)
    Normal = 1,      // 2: Xuất thương mại (Bán lẻ / Khách mua ngoài)
    Warranty = 2,    // 3: Xuất bảo hành hãng HTC
    Internal = 3     // 4: Xuất sử dụng nội bộ xưởng / tiêu hao
}

/// <summary>Trạng thái Chăm sóc khách hàng — theo SerCareStatus idn.CarService.</summary>
public enum CustomerCareStatus
{
    Pending = 0,            // PEND  — Chưa liên hệ / Chờ gọi khảo sát
    ContactedSatisfied = 1, // CIFB  — Đã liên hệ - Đã phản hồi (Hài lòng)
    NeedFeedback = 2,       // CINFB — Đã liên hệ - Cần phản hồi (Khiếu nại / Xe gặp sự cố)
    Rejected = 3            // REJ   — Không liên hệ được / Khách bận hoặc từ chối
}

/// <summary>Trạng thái Phiếu thu / Quyết toán thanh toán — theo Ser_Payment idn.CarService.</summary>
public enum PaymentStatus
{
    Draft = 0,     // DRAFT — Lập phiếu / Chờ thu tiền
    Completed = 1, // COMP  — Đã thu tiền / Hoàn tất thanh toán
    Cancelled = 2  // CANC  — Đã hủy phiếu thu
}

/// <summary>Hình thức thanh toán — theo Ser_PaymentType / Mst_PaymentType idn.CarService.</summary>
public enum PaymentMethod
{
    Cash = 0,         // Tiền mặt
    BankTransfer = 1, // Chuyển khoản ngân hàng
    PosCard = 2,      // Quẹt thẻ POS (ATM/Visa/MasterCard)
    Insurance = 3,    // Bảo hiểm bảo lãnh chi trả
    Internal = 4      // Nội bộ đại lý hỗ trợ
}

/// <summary>Trạng thái Báo giá phụ tùng & dịch vụ — theo Ser_Inv_Quote idn.CarService.</summary>
public enum QuoteStatus
{
    Draft = 0,     // 1: Mới tạo (Chờ gửi khách hàng)
    Sent = 1,      // 2: Đã gửi khách hàng xem xét
    Confirmed = 2, // 3: Khách hàng chấp thuận báo giá
    Converted = 3, // 4: Đã chuyển đổi thành Xuất kho hoặc Lệnh sửa chữa (RO)
    Rejected = 4   // 5: Khách từ chối / Hủy báo giá
}

/// <summary>Trạng thái Đơn đặt hàng phụ tùng — theo OrderPartStatus (P, A, F, R) idn.CarService.</summary>
public enum OrderPartStatus
{
    Pending = 0,   // P: Chờ duyệt / Mới tạo
    Approved = 1,  // A: Đã duyệt gửi NCC
    Finished = 2,  // F: Hoàn tất (Đã nhập kho xong)
    Rejected = 3   // R: Hủy đơn hàng / Từ chối
}

/// <summary>Hình thức đặt hàng phụ tùng — theo Mst_DeliveryForm idn.CarService.</summary>
public enum OrderPartDeliveryForm
{
    Normal = 0,    // 1: Đặt thường (Định kỳ / Bổ sung kho)
    Warranty = 1,  // 2: Đặt bảo hành hãng (Bắt buộc VIN)
    UrgentVOR = 2  // 3: Đặt khẩn cấp / Cấp bách (Xe nằm chờ phụ tùng - Vehicle Off Road)
}

/// <summary>Phân loại khoang sửa chữa — theo Mst_Compartment idn.CarService.</summary>
public enum CavityType
{
    EM = 0,    // Express Maintenance — Khoang bảo dưỡng nhanh
    GR = 1,    // General Repair — Khoang sửa chữa chung / Gầm máy
    BP = 2,    // Body & Paint — Khoang đồng sơn / Gò hàn / Buồng sơn sấy
    KCS = 3,   // Quality Control — Khoang kiểm tra chất lượng xuất xưởng
    Wash = 4   // Car Wash — Khoang rửa xe & Vệ sinh hoàn thiện
}

/// <summary>Trạng thái khoang sửa chữa — theo Ser_Cavity / StatusUse idn.CarService.</summary>
public enum CavityStatus
{
    Available = 0,    // 0: Trống / Sẵn sàng tiếp nhận xe
    Occupied = 1,     // 1: Đang có xe làm dịch vụ / Chiếm dụng
    Maintenance = 2,  // 2: Đang bảo trì cầu nâng / thiết bị xưởng
    Inactive = 3      // 3: Tạm ngừng sử dụng
}

/// <summary>Trạng thái Phiếu tiếp nhận & kiểm tra xe — theo Ser_ReceptionF (Pending = 'P', Approve/Delivery = 'A') idn.CarService.</summary>
public enum ReceptionStatus
{
    Pending = 0,    // P: Đang tiếp nhận / Chờ lập RO
    InService = 1,  // Đang sửa chữa / Đã lập lệnh RO vào xưởng
    Delivered = 2,  // A: Đã nghiệm thu bàn giao xe cho khách
    Cancelled = 3   // Đã hủy phiếu tiếp nhận
}

/// <summary>Kết quả kiểm tra hạng mục tiếp nhận xe — theo Ser_ReceptionFDtl (AuditStatus) idn.CarService.</summary>
public enum AuditStatus
{
    Good = 0,       // Tốt / Đạt tiêu chuẩn
    Attention = 1,  // Cần chú ý / Theo dõi
    Replace = 2,    // Cần sửa chữa / Thay thế
    NA = 3          // Không có / Không áp dụng
}

/// <summary>Trạng thái Phiếu phân công thợ sửa chữa — theo Ser_AssignmentWork idn.CarService.</summary>
public enum AssignmentWorkStatus
{
    Assigned = 0,    // ASSIGNED   — Mới phân công / Chờ KTV nhận việc
    InProgress = 1,  // IN_PROG    — Đang sửa chữa / Xe trong khoang (tương ứng RO InGarage)
    Completed = 2,   // COMPLETED  — Đã hoàn tất sửa chữa / Nghiệm thu nội bộ (tương ứng RO Repaired)
    Cancelled = 3    // CANCELLED  — Hủy phân công
}

/// <summary>Loại công việc sửa chữa — theo Ser_AssignmentWork WorkType idn.CarService.</summary>
public enum WorkType
{
    SCC = 0,         // Sửa chữa chung & Gầm máy (General Repair)
    SCD = 1,         // Sửa chữa đồng & Gò hàn thân vỏ (Body Repair)
    SCS = 2          // Sửa chữa sơn & Sấy hoàn thiện (Paint / Spray Booth)
}

/// <summary>Trạng thái Hồ sơ bồi thường bảo hiểm — theo Ser_Insurance / Ser_InsuranceDebit idn.CarService.</summary>
public enum InsuranceClaimStatus
{
    Draft = 0,      // DRAFT     — Lập hồ sơ / Khảo sát tổn thất
    Submitted = 1,  // SUBMITTED — Đã gửi hồ sơ cho Hãng bảo hiểm thẩm định
    Approved = 2,   // APPROVED  — Bảo hiểm chấp thuận bảo lãnh bồi thường
    Settled = 3,    // SETTLED   — Đã quyết toán hoàn tất / xuất hóa đơn
    Rejected = 4    // REJECTED  — Hãng bảo hiểm từ chối bồi thường
}

/// <summary>Hình thức thanh toán bồi thường bảo hiểm — theo Ser_InsuranceContract TypePayment idn.CarService.</summary>
public enum InsurancePaymentType
{
    DirectGuarantee = 0,   // Bảo lãnh thanh toán trực tiếp (Bảo hiểm trả tiền cho đại lý)
    CustomerReimburse = 1  // Khách tự trả trước, bảo hiểm bồi hoàn sau cho khách
}

/// <summary>Trạng thái Chiến dịch khuyến mãi dịch vụ — theo CamMarketingStatus idn.CarService.</summary>
public enum CampaignMarketingStatus
{
    Draft = 0,     // DRAFT     — Dự thảo / Lập kế hoạch
    Active = 1,    // ACTIVE    — Đang triển khai / Có hiệu lực
    Finished = 2,  // FINISHED  — Đã kết thúc chiến dịch
    Cancelled = 3  // CANCELLED — Đã hủy bỏ
}

/// <summary>Phương pháp tính mốc bảo dưỡng định kỳ — theo Ser_CustomerCareMace MaceType idn.CarService.</summary>
public enum MaceType
{
    Advisor = 1,          // 1: CVDV chỉ định
    Standard6Months = 2,  // 2: Thời hạn sau 6 tháng
    FrequencyFvx = 3      // 3: Thời hạn theo tần suất vào xưởng
}

/// <summary>Trạng thái Phiếu nhắc bảo dưỡng định kỳ — theo Ser_CustomerCareMace Status idn.CarService.</summary>
public enum CustomerCareMaceStatus
{
    Pending = 0,       // 0: Chưa liên hệ (Pending / PEND)
    Contacted = 1,     // 1: Đã liên hệ (Contacted / CONT)
    NotContacted = 2,  // 2: Không liên hệ được (Not Contacted / NOCONT)
    Booked = 3,        // 3: Đã chốt hẹn bảo dưỡng (Booked)
    Cancelled = 4      // 4: Khách từ chối / Hủy (Cancelled)
}

/// <summary>Hình thức kiểm kê / điều chuyển kho phụ tùng — theo Ser_Inv_StockAdj idn.CarService.</summary>
public enum StockAdjType
{
    CountBalance = 0,     // 0: Kiểm kê cân đối kho (Discrepancy count adjustment)
    LocationTransfer = 1, // 1: Điều chuyển vị trí kệ kho (Rack / Bin location transfer)
    DamageScrap = 2       // 2: Hao hụt / Hư hỏng / Thanh lý phụ tùng (Damage / Scrap)
}

/// <summary>Trạng thái Phiếu kiểm kê / điều chuyển kho — theo Ser_StockAdj idn.CarService.</summary>
public enum StockAdjStatus
{
    Pending = 0,    // 0: DRAFT / PENDING — Mới tạo / Chờ kiểm kê (Create = "0")
    Executing = 1,  // 1: EXECUTING — Đang kiểm đếm / Đang xử lý
    Finished = 2,   // 2: FINISHED — Đã duyệt hoàn tất / Đã cập nhật tồn kho (Finished = "1")
    Rejected = 3    // 3: REJECTED — Hủy phiếu kiểm kê
}

/// <summary>Trạng thái Bản tin kỹ thuật & Chiến dịch triệu hồi — theo Btl_Bulletin idn.CarService.</summary>
public enum BulletinStatus
{
    Draft = 0,     // DRAFT    — Dự thảo / Đang soạn thảo
    Active = 1,    // ACTIVE   — Có hiệu lực / Đang áp dụng triệu hồi
    Finished = 2,  // FINISHED — Đã kết thúc chiến dịch
    Cancelled = 3  // CANCEL   — Đã hủy bỏ
}

/// <summary>Trạng thái xử lý xe theo số khung VIN trong Bản tin kỹ thuật — theo Btl_Bulletin_VIN Status ('P', 'F') idn.CarService.</summary>
public enum BulletinVinStatus
{
    Pending = 0,   // P — Chờ xử lý / Chưa thực hiện triệu hồi
    Completed = 1  // F — Đã thực hiện xong theo Lệnh RO
}

public class Customer : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public List<Car> Cars { get; set; } = [];
    public List<Quote> Quotes { get; set; } = [];
}

public class Car : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Plate { get; set; } = "";      // biển số
    public string? Vin { get; set; }
    public string Model { get; set; } = "";
    public int Year { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
}

/// <summary>Lệnh sửa chữa (Repair Order) — chứng từ trung tâm.</summary>
public class RepairOrder : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";
    public int CarId { get; set; }
    public int CustomerId { get; set; }
    public ROStatus Status { get; set; } = ROStatus.Created;
    public int Odometer { get; set; }             // số km
    public string? IntakeNote { get; set; }       // ghi nhận tình trạng khi nhận xe
    public string? Technician { get; set; }       // thợ phụ trách
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? IntakeAt { get; set; }        // xe vào xưởng (INGA)
    public DateTime? FinishedAt { get; set; }      // giao xe (FNS)

    public Car Car { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public int? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }
    public int? CavityId { get; set; }
    public Cavity? Cavity { get; set; }
    public int? ReceptionSheetId { get; set; }
    public ReceptionSheet? ReceptionSheet { get; set; }
    public int? CampaignMarketingId { get; set; }
    public CampaignMarketing? CampaignMarketing { get; set; }
    public decimal CampaignDiscountAmount { get; set; } = 0;
    public int? BulletinId { get; set; }
    public Bulletin? Bulletin { get; set; }
    public List<RepairLine> Lines { get; set; } = [];
    public List<WarrantyReport> WarrantyReports { get; set; } = [];
    public List<StockOut> StockOuts { get; set; } = [];
    public List<CustomerCare> CustomerCares { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];
    public List<Quote> Quotes { get; set; } = [];
    public List<OrderPart> OrderParts { get; set; } = [];
    public List<ReceptionSheet> ReceptionSheets { get; set; } = [];
    public List<AssignmentWork> AssignmentWorks { get; set; } = [];
    public List<InsuranceClaim> InsuranceClaims { get; set; } = [];
    public List<CustomerCareMace> CustomerCareMaces { get; set; } = [];

    public decimal Total => Math.Max(0, Lines.Sum(l => l.Amount) - CampaignDiscountAmount);
    public decimal GrossTotal => Lines.Sum(l => l.Amount);
    public decimal LaborTotal => Lines.Where(l => l.Type == LineType.Labor).Sum(l => l.Amount);
    public decimal PartTotal => Lines.Where(l => l.Type == LineType.Part).Sum(l => l.Amount);
    public decimal CustomerTotal => Math.Max(0, Lines.Where(l => l.ExpenseType == ExpenseType.Customer).Sum(l => l.Amount) - CampaignDiscountAmount);
    public decimal WarrantyTotal => Lines.Where(l => l.ExpenseType == ExpenseType.Warranty).Sum(l => l.Amount);
    public decimal InsuranceTotal => Lines.Where(l => l.ExpenseType == ExpenseType.Insurance).Sum(l => l.Amount);
    public decimal PaidAmount => Payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.PaymentAmount);
    public decimal RemainingBalance => Math.Max(0, CustomerTotal - PaidAmount);
}

public class Part : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";          // Mã phụ tùng (PartCode, VD: 26300-35505)
    public string Name { get; set; } = "";          // Tên phụ tùng (VieName)
    public string Unit { get; set; } = "Cái";       // Đơn vị tính (DVT)
    public decimal CostPrice { get; set; }          // Giá vốn/nhập (Cost)
    public decimal SalePrice { get; set; }          // Giá bán niêm yết (Price)
    public decimal InStock { get; set; }            // Tồn kho hiện tại (InStockQuantity)
    public decimal MinStock { get; set; }           // Tồn tối thiểu cảnh báo (MinQuantity)
    public string? Location { get; set; }           // Vị trí kệ/kho
    public string? Model { get; set; }              // Dòng xe tương thích
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsLowStock => InStock <= MinStock;
}

public class RepairLine : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int ROId { get; set; }
    public LineType Type { get; set; }
    public ExpenseType ExpenseType { get; set; } = ExpenseType.Customer;
    public int? PartId { get; set; }                // Liên kết danh mục phụ tùng nếu có
    public string Name { get; set; } = "";
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Amount => Quantity * UnitPrice;
    public RepairOrder RO { get; set; } = null!;
    public Part? Part { get; set; }
}

/// <summary>Báo cáo bảo hành (Warranty Report) — Ser_ROWarrantyReport trong idn.CarService.</summary>
public class WarrantyReport : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string ReportNo { get; set; } = "";          // Số BCBH (VD: WAR260427-001)
    public int ROId { get; set; }                       // Lệnh sửa chữa gốc
    public int CarId { get; set; }                      // Xe được bảo hành
    public int CustomerId { get; set; }                 // Khách hàng
    public int Odometer { get; set; }                   // Số km ghi nhận
    public WarrantyStatus Status { get; set; } = WarrantyStatus.Pending;

    // Kỹ thuật & Hiện tượng sự cố (CusRequest, CarStatus, ErrorCode...)
    public string IssueDescription { get; set; } = "";  // Triệu chứng hư hỏng / phàn nàn của KH
    public string DiagnosticResult { get; set; } = "";  // Kết quả chẩn đoán kỹ thuật viên
    public string ErrorCodeCD { get; set; } = "";       // Mã chẩn đoán kỹ thuật (DTC)
    public string ErrorCodePN { get; set; } = "";       // Mã hiện tượng hư hỏng
    public int? PartIDError { get; set; }               // Phụ tùng hỏng hóc gây sự cố

    // Bồi hoàn & Phê duyệt hãng
    public decimal ClaimAmount { get; set; }            // Tổng tiền đề nghị bồi hoàn (VNĐ)
    public decimal? ApprovedAmount { get; set; }        // Số tiền hãng duyệt thanh toán
    public string? RejectionReason { get; set; }        // Lý do từ chối / lý do yêu cầu bổ sung
    public string? DecisionNote { get; set; }           // Ý kiến phản hồi của nhà phân phối HTC

    public string CreatedBy { get; set; } = "web";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? SubmittedAt { get; set; }          // Ngày gửi HTC
    public DateTime? DecidedAt { get; set; }            // Ngày duyệt / từ chối

    public RepairOrder RO { get; set; } = null!;
    public Car Car { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Part? PartError { get; set; }
    public List<WarrantyReportItem> Items { get; set; } = [];

    public decimal LaborClaimTotal => Items.Where(i => i.Type == LineType.Labor).Sum(i => i.Amount);
    public decimal PartClaimTotal => Items.Where(i => i.Type == LineType.Part).Sum(i => i.Amount);
}

/// <summary>Hạng mục công / phụ tùng trong BCBH — Ser_ROWarrantyReportServiceItems / PartItems.</summary>
public class WarrantyReportItem : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int WarrantyReportId { get; set; }
    public LineType Type { get; set; }
    public int? PartId { get; set; }                    // Phụ tùng bảo hành (nếu là Part)
    public string Code { get; set; } = "";              // Mã công hoặc mã phụ tùng
    public string Name { get; set; } = "";              // Tên hạng mục
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Amount => Quantity * UnitPrice;
    public bool IsAccepted { get; set; } = true;        // Trạng thái chấp thuận hạng mục này
    public string? Note { get; set; }

    public WarrantyReport WarrantyReport { get; set; } = null!;
    public Part? Part { get; set; }
}

/// <summary>Cuộc hẹn dịch vụ (Service Appointment) — Ser_App trong idn.CarService.</summary>
public class Appointment : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string AppNo { get; set; } = "";             // Số cuộc hẹn (VD: APP260427-001)
    public int CarId { get; set; }                      // Xe hẹn
    public int CustomerId { get; set; }                 // Khách hàng
    public DateTime AppointmentDate { get; set; } = DateTime.Today.AddHours(9); // Ngày giờ hẹn
    public AppointmentServiceType ServiceType { get; set; } = AppointmentServiceType.Maintenance; // Loại hẹn
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending; // Trạng thái
    public string? Advisor { get; set; }                // Cố vấn dịch vụ (CVDV tiếp nhận)
    public string? Cavity { get; set; }                 // Khoang sửa chữa dự kiến (VD: Khoang bảo dưỡng nhanh)
    public string CustomerRequest { get; set; } = "";   // Yêu cầu của khách / Triệu chứng xe
    public string? Note { get; set; }                   // Ghi chú nội bộ
    public string? CancelReason { get; set; }           // Lý do hủy hẹn
    public string Source { get; set; } = "Hotline";     // Nguồn đặt: Hotline / App / Website / Trực tiếp
    public int? ROId { get; set; }                      // Lệnh sửa chữa sinh ra khi tiếp nhận xe (CheckedIn)
    public string CreatedBy { get; set; } = "web";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ConfirmedAt { get; set; }          // Thời điểm xác nhận
    public DateTime? CheckedInAt { get; set; }          // Thời điểm xe vào xưởng tiếp nhận (tạo RO)

    public Car Car { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public RepairOrder? RO { get; set; }
    public int? ReceptionSheetId { get; set; }
    public ReceptionSheet? ReceptionSheet { get; set; }
    public int? CustomerCareMaceId { get; set; }
    public CustomerCareMace? CustomerCareMace { get; set; }
}

/// <summary>Phiếu Nhập kho phụ tùng — Ser_Inv_StockIn trong idn.CarService.</summary>
public class StockIn : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string StockInNo { get; set; } = "";             // Số phiếu nhập (VD: NK260427-001)
    public DateTime StockInDate { get; set; } = DateTime.Today; // Ngày nhập kho
    public string SupplierName { get; set; } = "";          // Tên nhà cung cấp (SupplierName)
    public string? BillNo { get; set; }                     // Số hóa đơn / Số chứng từ giao hàng (BillNo)
    public StockInType Type { get; set; } = StockInType.Normal; // Loại phiếu nhập (StockInType)
    public StockInStatus Status { get; set; } = StockInStatus.Pending; // Trạng thái phiếu nhập
    public string? Description { get; set; }                // Diễn giải / Ghi chú (Description)
    public int? OrderPartId { get; set; }                   // Đơn đặt hàng phụ tùng gốc (nếu nhập từ đơn đặt)
    public string? OrderPartNo { get; set; }                // Số đơn đặt hàng NCC
    public string CreatedBy { get; set; } = "web";          // Người tạo phiếu
    public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo
    public DateTime? FinishedAt { get; set; }               // Ngày duyệt nhập kho
    public string? ApprovedBy { get; set; }                 // Người duyệt nhập kho

    public OrderPart? OrderPart { get; set; }
    public List<StockInDetail> Items { get; set; } = [];

    public decimal SubTotal => Items.Sum(i => i.Quantity * i.UnitPrice);
    public decimal TotalVat => Items.Sum(i => i.VatAmount);
    public decimal Total => Items.Sum(i => i.Amount);
    public int ItemCount => Items.Count;
}

/// <summary>Chi tiết phụ tùng nhập kho — Ser_Inv_StockInDetail trong idn.CarService.</summary>
public class StockInDetail : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int StockInId { get; set; }
    public int PartId { get; set; }                         // ID phụ tùng trong danh mục
    public string PartCode { get; set; } = "";              // Mã phụ tùng (PartCode)
    public string PartName { get; set; } = "";              // Tên phụ tùng (VieName)
    public string Unit { get; set; } = "Cái";               // Đơn vị tính (Unit)
    public decimal Quantity { get; set; } = 1;              // Số lượng nhập (Quantity)
    public decimal UnitPrice { get; set; }                  // Đơn giá nhập trước thuế (Price)
    public decimal VatPercent { get; set; } = 8;            // Thuế suất VAT % (VAT)
    public string? Location { get; set; }                   // Vị trí lưu kho thực tế (ActualLocation)
    public string? Note { get; set; }                       // Ghi chú dòng (Description)

    public StockIn StockIn { get; set; } = null!;
    public Part Part { get; set; } = null!;

    public decimal SubTotal => Quantity * UnitPrice;
    public decimal VatAmount => Math.Round(SubTotal * (VatPercent / 100m), 2);
    public decimal Amount => SubTotal + VatAmount;
}

/// <summary>Phiếu Xuất kho phụ tùng / vật tư — Ser_Inv_StockOut trong idn.CarService.</summary>
public class StockOut : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string StockOutNo { get; set; } = "";             // Số phiếu xuất (VD: XK260427-001)
    public DateTime StockOutDate { get; set; } = DateTime.Today; // Ngày xuất kho
    public StockOutType Type { get; set; } = StockOutType.Service; // Loại phiếu xuất (StockOutType)
    public StockOutStatus Status { get; set; } = StockOutStatus.Pending; // Trạng thái phiếu xuất
    public int? ROId { get; set; }                          // Lệnh sửa chữa gắn liền (nếu có)
    public int? CustomerId { get; set; }                    // Khách hàng nhận / chủ xe
    public int? CarId { get; set; }                         // Xe nhận phụ tùng
    public int? QuoteId { get; set; }                       // Báo giá phụ tùng gốc nếu chuyển từ báo giá
    public string? RecipientName { get; set; }              // Người nhận hàng / Kỹ thuật viên / Khách
    public string? Description { get; set; }                // Diễn giải / Lý do xuất kho
    public string CreatedBy { get; set; } = "web";          // Người lập phiếu
    public DateTime CreatedAt { get; set; } = DateTime.Now; // Thời điểm lập phiếu
    public DateTime? FinishedAt { get; set; }               // Ngày hoàn tất xuất kho (trừ tồn)
    public string? ApprovedBy { get; set; }                 // Thủ kho duyệt xuất

    public RepairOrder? RO { get; set; }
    public Customer? Customer { get; set; }
    public Car? Car { get; set; }
    public Quote? Quote { get; set; }
    public List<StockOutDetail> Items { get; set; } = [];

    public decimal SubTotal => Items.Sum(i => i.Quantity * i.UnitPrice);
    public decimal TotalVat => Items.Sum(i => i.VatAmount);
    public decimal Total => Items.Sum(i => i.Amount);
    public int ItemCount => Items.Count;
}

/// <summary>Chi tiết phụ tùng xuất kho — Ser_Inv_StockOutDetail trong idn.CarService.</summary>
public class StockOutDetail : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int StockOutId { get; set; }
    public int PartId { get; set; }                         // ID phụ tùng trong kho
    public string PartCode { get; set; } = "";              // Mã phụ tùng (PartCode)
    public string PartName { get; set; } = "";              // Tên phụ tùng (VieName)
    public string Unit { get; set; } = "Cái";               // Đơn vị tính (Unit)
    public decimal Quantity { get; set; } = 1;              // Số lượng xuất (Quantity)
    public decimal UnitPrice { get; set; }                  // Đơn giá xuất trước thuế (Price)
    public decimal VatPercent { get; set; } = 8;            // Thuế suất VAT % (VAT)
    public string? Location { get; set; }                   // Vị trí kho xuất (ActualLocation)
    public string? Note { get; set; }                       // Ghi chú dòng phụ tùng

    public StockOut StockOut { get; set; } = null!;
    public Part Part { get; set; } = null!;

    public decimal SubTotal => Quantity * UnitPrice;
    public decimal VatAmount => Math.Round(SubTotal * (VatPercent / 100m), 2);
    public decimal Amount => SubTotal + VatAmount;
}

/// <summary>Phiếu chăm sóc khách hàng 24h sau dịch vụ — Ser_CustomerCare24h trong idn.CarService.</summary>
public class CustomerCare : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string CareNo { get; set; } = "";             // Số phiếu CSKH (VD: CC260427-001)
    public int ROId { get; set; }                        // Lệnh sửa chữa gắn liền
    public int CarId { get; set; }                       // Xe làm dịch vụ
    public int CustomerId { get; set; }                  // Khách hàng
    public CustomerCareStatus Status { get; set; } = CustomerCareStatus.Pending;

    // 5 câu hỏi khảo sát chuẩn Hyundai Car Service (Ser_CustomerCare24h)
    public bool HasCarProblem { get; set; } = false;     // YourCarProblem24: Xe có vấn đề gì sau dịch vụ không?
    public int? QualityRating { get; set; }              // YourSatisfyQSv24: Hài lòng chất lượng dịch vụ (1: Rất hài lòng, 2: Hài lòng, 3: Bình thường, 4: Không hài lòng)
    public int? StaffRating { get; set; }                // FyourCSSH24: Thái độ phục vụ của CVDV / kỹ thuật (1: Rất tốt, 2: Tốt, 3: Bình thường, 4: Chưa tốt)
    public bool? WillingToReturn { get; set; } = true;   // YourRIWN24: Sẵn sàng quay lại xưởng dịch vụ? (true: Sẵn sàng, false: Phân vân / Không)
    public int? FacilityRating { get; set; }             // WFBasicNeeds24: Đánh giá cơ sở vật chất phòng chờ (1: Rất tốt, 2: Đạt yêu cầu, 3: Cần cải thiện)
    public string? CustomerFeedback { get; set; }        // Note24: Ý kiến góp ý / chi tiết khiếu nại của khách
    public string? InternalNote { get; set; }            // Ghi chú nội bộ xử lý khiếu nại

    public string? ContactedBy { get; set; }             // Nhân viên CSKH gọi điện
    public DateTime? ContactedDate { get; set; }         // Thời điểm liên hệ
    public string CreatedBy { get; set; } = "system";    // Tự động tạo khi xe FNS hoặc tạo tay
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public RepairOrder RO { get; set; } = null!;
    public Car Car { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
}

/// <summary>Phiếu thu tiền & Quyết toán thanh toán dịch vụ — Ser_Payment trong idn.CarService.</summary>
public class Payment : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string PaymentNo { get; set; } = "";             // Số phiếu thu (VD: PT260427-001)
    public int ROId { get; set; }                           // Lệnh sửa chữa thanh toán
    public int CustomerId { get; set; }                     // Khách hàng / Chủ xe
    public int CarId { get; set; }                          // Xe làm dịch vụ
    public DateTime PaymentDate { get; set; } = DateTime.Today; // Ngày thu tiền
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash; // Phương thức thanh toán
    public PaymentStatus Status { get; set; } = PaymentStatus.Completed; // Trạng thái phiếu thu
    public string PayPersonName { get; set; } = "";          // Người nộp tiền (PayPersonName)
    public string? PayPersonPhone { get; set; }              // SĐT người nộp tiền
    public string? PayPersonIdCard { get; set; }             // CMND/CCCD người nộp (PayPersonIDCardNo)
    public decimal RoTotalAmount { get; set; }               // Tổng chi phí RO
    public decimal DiscountAmount { get; set; }              // Chiết khấu / Giảm giá trực tiếp (AmountDiscountOther)
    public decimal ThirdPartyAmount { get; set; }            // Phần tiền bảo hành / bảo hiểm chi trả
    public decimal PayableAmount { get; set; }               // Số tiền khách hàng cần thanh toán
    public decimal PaymentAmount { get; set; }               // Số tiền thực thu (PaymentAmount)
    public string? TransactionRef { get; set; }              // Mã tham chiếu GD / Mã chuẩn chi ngân hàng / POS
    public string? Note { get; set; }                        // Diễn giải / Lý do thu tiền (Note)
    public string Cashier { get; set; } = "Thu ngân";       // Nhân viên thu ngân lập phiếu
    public string CreatedBy { get; set; } = "web";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }               // Thời điểm hoàn tất thu tiền

    public RepairOrder RO { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Car Car { get; set; } = null!;
}

/// <summary>Báo giá phụ tùng & dịch vụ — Ser_Inv_Quote trong idn.CarService.</summary>
public class Quote : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string QuoteNo { get; set; } = "";             // Số báo giá (VD: BG260427-001)
    public DateTime QuoteDate { get; set; } = DateTime.Today; // Ngày lập báo giá
    public DateTime? ValidUntil { get; set; }             // Hiệu lực báo giá đến ngày
    public int? CustomerId { get; set; }                  // Khách hàng trong hệ thống (nếu có)
    public string CustomerName { get; set; } = "";        // Tên khách hàng / Tên doanh nghiệp
    public string? CustomerPhone { get; set; }            // Số điện thoại
    public string? CustomerAddress { get; set; }          // Địa chỉ khách hàng
    public int? CarId { get; set; }                       // Xe liên quan nếu có
    public string? RecipientName { get; set; }            // Người nhận báo giá (ReceiveName)
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash; // Hình thức thanh toán dự kiến
    public QuoteStatus Status { get; set; } = QuoteStatus.Draft; // Trạng thái báo giá
    public string? Remark { get; set; }                   // Điều khoản thanh toán & hiệu lực (Remark)
    public string? Note { get; set; }                     // Ghi chú tư vấn kỹ thuật (Note)
    public int? StockOutId { get; set; }                  // Phiếu xuất kho sinh ra từ báo giá (nếu đã chuyển xuất kho)
    public int? ROId { get; set; }                        // Lệnh sửa chữa sinh ra từ báo giá (nếu chuyển RO)
    public string CreatedBy { get; set; } = "web";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ConfirmedAt { get; set; }            // Ngày khách hàng chấp thuận

    public Customer? Customer { get; set; }
    public Car? Car { get; set; }
    public StockOut? StockOut { get; set; }
    public RepairOrder? RO { get; set; }
    public List<QuoteItem> Items { get; set; } = [];

    public decimal SubTotal => Items.Sum(i => i.Quantity * i.UnitPrice);
    public decimal TotalDiscount => Items.Sum(i => i.DiscountAmount);
    public decimal TotalVat => Items.Sum(i => i.VatAmount);
    public decimal Total => Items.Sum(i => i.Amount);
    public int ItemCount => Items.Count;
}

/// <summary>Chi tiết phụ tùng / hạng mục báo giá — Ser_Inv_QuotePartItems trong idn.CarService.</summary>
public class QuoteItem : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int QuoteId { get; set; }
    public int? PartId { get; set; }                      // ID phụ tùng trong kho (nếu có)
    public string PartCode { get; set; } = "";            // Mã phụ tùng (PartCode)
    public string PartName { get; set; } = "";            // Tên phụ tùng (VieName)
    public string Unit { get; set; } = "Cái";             // Đơn vị tính (Unit)
    public decimal Quantity { get; set; } = 1;            // Số lượng báo giá (Quantity)
    public decimal UnitPrice { get; set; }                // Đơn giá bán trước thuế/chiết khấu (Price)
    public decimal DiscountPercent { get; set; } = 0;     // Chiết khấu dòng (%)
    public decimal VatPercent { get; set; } = 8;          // Thuế suất VAT (%)
    public string? Note { get; set; }                     // Ghi chú phụ tùng

    public Quote Quote { get; set; } = null!;
    public Part? Part { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;
    public decimal DiscountAmount => Math.Round(LineTotal * (DiscountPercent / 100m), 2);
    public decimal TaxableAmount => LineTotal - DiscountAmount;
    public decimal VatAmount => Math.Round(TaxableAmount * (VatPercent / 100m), 2);
    public decimal Amount => TaxableAmount + VatAmount;
}

/// <summary>Gói dịch vụ bảo dưỡng định kỳ xe — Ser_ServicePackage trong idn.CarService.</summary>
public class ServicePackage : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string PackageNo { get; set; } = "";             // Mã gói dịch vụ (VD: PKG-BD-5K, PKG-BD-10K)
    public string Name { get; set; } = "";                  // Tên gói dịch vụ (ServicePackageName)
    public decimal TakingTimeHours { get; set; } = 1.0m;    // Thời gian định mức dự kiến (giờ) (TakingTime)
    public string? Description { get; set; }                // Diễn giải / Chi tiết nội dung gói
    public bool IsPublic { get; set; } = true;              // Cờ áp dụng toàn hệ thống hãng hay riêng đại lý (IsPublicFlag)
    public bool IsActive { get; set; } = true;              // Đang áp dụng / Tạm dừng
    public string CreatedBy { get; set; } = "web";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<ServicePackageItem> Items { get; set; } = [];

    public decimal LaborSubTotal => Items.Where(i => i.Type == LineType.Labor).Sum(i => i.Quantity * i.UnitPrice);
    public decimal PartSubTotal => Items.Where(i => i.Type == LineType.Part).Sum(i => i.Quantity * i.UnitPrice);
    public decimal SubTotal => Items.Sum(i => i.Quantity * i.UnitPrice);
    public decimal TotalVat => Items.Sum(i => i.VatAmount);
    public decimal Total => Items.Sum(i => i.Amount);
    public int ItemCount => Items.Count;
    public int LaborCount => Items.Count(i => i.Type == LineType.Labor);
    public int PartCount => Items.Count(i => i.Type == LineType.Part);
}

/// <summary>Chi tiết hạng mục công việc / phụ tùng gói dịch vụ — Ser_ServicePackageServiceItems / Ser_ServicePackagePartItems.</summary>
public class ServicePackageItem : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int ServicePackageId { get; set; }
    public LineType Type { get; set; }                      // Công thợ (Labor) hoặc Phụ tùng (Part)
    public int? PartId { get; set; }                        // Liên kết danh mục phụ tùng kho (nếu là Part)
    public string Code { get; set; } = "";                  // Mã công việc (SerCode) hoặc Mã phụ tùng (PartCode)
    public string Name { get; set; } = "";                  // Tên công việc hoặc Tên phụ tùng
    public string Unit { get; set; } = "Lần";               // Đơn vị tính (Lần / Giờ / Cái / Can / Bộ...)
    public decimal Quantity { get; set; } = 1;              // Số lượng hoặc Số giờ công định mức (ActManHour)
    public decimal UnitPrice { get; set; }                  // Đơn giá bán trước thuế
    public decimal VatPercent { get; set; } = 8;            // Thuế suất VAT (%)
    public ExpenseType ExpenseType { get; set; } = ExpenseType.Customer; // Đối tượng thanh toán
    public string? Note { get; set; }                       // Ghi chú chi tiết

    public ServicePackage ServicePackage { get; set; } = null!;
    public Part? Part { get; set; }

    public decimal SubTotal => Quantity * UnitPrice;
    public decimal VatAmount => Math.Round(SubTotal * (VatPercent / 100m), 2);
    public decimal Amount => SubTotal + VatAmount;
}

/// <summary>Đơn đặt hàng phụ tùng Nhà Cung Cấp — Ser_Order_Part trong idn.CarService.</summary>
public class OrderPart : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string OrderPartNo { get; set; } = "";             // Số đơn hàng (VD: PO260427-001)
    public DateTime OrderDate { get; set; } = DateTime.Today; // Ngày lập đơn hàng
    public string SupplierName { get; set; } = "";            // Tên NCC (Hyundai Thành Công / Mobis...)
    public OrderPartDeliveryForm DeliveryForm { get; set; } = OrderPartDeliveryForm.Normal; // Hình thức đặt
    public string DeliveryLocation { get; set; } = "Kho phụ tùng chính"; // Địa điểm nhận hàng
    public DateTime? EstimatedDeliverDate { get; set; }       // Ngày giao hàng dự kiến
    public string? VIN { get; set; }                          // Số khung xe (bắt buộc nếu bảo hành)
    public int? ROId { get; set; }                            // Lệnh sửa chữa gắn liền nếu đặt cho xe chờ phụ tùng (Wait4Part)
    public OrderPartStatus Status { get; set; } = OrderPartStatus.Pending; // Trạng thái đơn hàng (P/A/F/R)
    public string? OrderSuppierNo { get; set; }               // Số đơn hàng phía NCC xác nhận
    public DateTime? RequestSuppierDate { get; set; }         // Ngày gửi đơn cho NCC
    public DateTime? ResponseSuppierDate { get; set; }        // Ngày NCC phản hồi xác nhận
    public string? Remark { get; set; }                       // Ghi chú / Điều khoản giao nhận
    public int? StockInId { get; set; }                       // Phiếu nhập kho đã sinh khi nhận hàng (Finished)
    public string CreatedBy { get; set; } = "web";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ApprovedAt { get; set; }                 // Ngày duyệt đơn hàng
    public DateTime? FinishedAt { get; set; }                 // Ngày hoàn tất nhập kho

    public RepairOrder? RO { get; set; }
    public StockIn? StockIn { get; set; }
    public List<OrderPartLine> Lines { get; set; } = [];

    public decimal SubTotalBeforeDiscount => Lines.Sum(l => l.SubTotalBeforeDiscount);
    public decimal TotalDiscount => Lines.Sum(l => l.DiscountAmount);
    public decimal TaxableAmount => Lines.Sum(l => l.TaxableAmount);
    public decimal TotalVat => Lines.Sum(l => l.VatAmount);
    public decimal Total => Lines.Sum(l => l.Amount);
    public decimal TotalQuantityOrdered => Lines.Sum(l => l.Quantity);
    public decimal TotalQuantityApproved => Lines.Sum(l => l.ApprovedQuantity);
    public decimal TotalQuantityReceived => Lines.Sum(l => l.ReceivedQuantity);
    public int ItemCount => Lines.Count;
}

/// <summary>Chi tiết dòng phụ tùng đặt hàng NCC — Ser_Order_PartDtl trong idn.CarService.</summary>
public class OrderPartLine : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int OrderPartId { get; set; }
    public string OrderPartNo { get; set; } = "";
    public int PartId { get; set; }                           // Phụ tùng trong danh mục
    public string PartCode { get; set; } = "";                // Mã phụ tùng (PartCode)
    public string PartName { get; set; } = "";                // Tên phụ tùng (VieName)
    public string Unit { get; set; } = "Cái";                 // Đơn vị tính (Unit)
    public decimal Quantity { get; set; } = 1;                // Số lượng đặt (QtyOrd)
    public decimal UnitPrice { get; set; }                    // Đơn giá trước chiết khấu (Price / UPBeforeDc)
    public decimal DiscountRate { get; set; } = 0;            // % Chiết khấu dòng (DiscountRate)
    public decimal VatPercent { get; set; } = 8;              // % Thuế suất VAT (VAT)
    public decimal ApprovedQuantity { get; set; } = 1;        // Số lượng duyệt cung cấp (QtyAppr)
    public decimal ReceivedQuantity { get; set; } = 0;        // Số lượng thực tế đã nhập kho
    public OrderPartStatus StatusDtl { get; set; } = OrderPartStatus.Pending; // Trạng thái dòng
    public string? Note { get; set; }                         // Ghi chú dòng (Remark)

    public OrderPart OrderPart { get; set; } = null!;
    public Part Part { get; set; } = null!;

    public decimal SubTotalBeforeDiscount => Quantity * UnitPrice;
    public decimal DiscountAmount => Math.Round(SubTotalBeforeDiscount * (DiscountRate / 100m), 2);
    public decimal TaxableAmount => SubTotalBeforeDiscount - DiscountAmount;
    public decimal VatAmount => Math.Round(TaxableAmount * (VatPercent / 100m), 2);
    public decimal Amount => TaxableAmount + VatAmount;
}

/// <summary>Khoang sửa chữa & Cầu nâng trong xưởng dịch vụ — Ser_Cavity trong idn.CarService.</summary>
public class Cavity : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string CavityNo { get; set; } = "";             // Mã khoang (CavityNo, VD: KH-EM-01, KH-GR-01)
    public string CavityName { get; set; } = "";           // Tên khoang (CavityName, VD: Khoang Bảo Dưỡng Nhanh #1)
    public CavityType CavityType { get; set; } = CavityType.EM; // Loại khoang (CavityType)
    public CavityStatus Status { get; set; } = CavityStatus.Available; // Trạng thái khoang
    public string? LiftEquipment { get; set; }             // Loại cầu nâng / Thiết bị (Cầu 2 trụ, Cầu cắt kéo, Buồng sơn...)
    public string? AreaZone { get; set; }                  // Khu vực xưởng (Tầng 1, Xưởng gầm máy, Xưởng đồng sơn...)
    public int? CurrentROId { get; set; }                  // Lệnh RO hiện đang trên khoang
    public string? CurrentCarPlate { get; set; }           // Biển số xe trên khoang
    public string? CurrentCarModel { get; set; }           // Model xe trên khoang
    public string? CurrentTechnician { get; set; }         // Kỹ thuật viên phụ trách trên khoang
    public DateTime? StartUseDate { get; set; }            // Thời gian xe bắt đầu vào khoang
    public DateTime? ExpectedFinishDate { get; set; }      // Thời gian dự kiến hoàn tất trên khoang
    public DateTime? FinishUseDate { get; set; }           // Thời gian xe rời khoang gần nhất
    public string? Note { get; set; }                      // Ghi chú khoang
    public bool IsActive { get; set; } = true;             // Đang kích hoạt
    public string CreatedBy { get; set; } = "web";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public RepairOrder? CurrentRO { get; set; }

    public bool IsInUse => Status == CavityStatus.Occupied;
    public TimeSpan? ElapsedTime => (StartUseDate.HasValue && Status == CavityStatus.Occupied) ? (DateTime.Now - StartUseDate.Value) : null;
}

/// <summary>Phiếu tiếp nhận & kiểm tra xe ban đầu (Walk-around Reception & Inspection Form) — Ser_ReceptionF trong idn.CarService.</summary>
public class ReceptionSheet : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string ReceptionNo { get; set; } = "";             // Số phiếu tiếp nhận (VD: TN260427-001)
    public int CarId { get; set; }                            // Xe làm dịch vụ
    public int CustomerId { get; set; }                       // Khách hàng / Chủ xe
    public int? AppointmentId { get; set; }                   // Cuộc hẹn gốc (nếu có đặt trước)
    public int? ROId { get; set; }                            // Lệnh sửa chữa sinh ra từ tiếp nhận
    public int Odometer { get; set; }                         // Số km đồng hồ khi tiếp nhận xe (Km)
    public int FuelLevel { get; set; } = 2;                   // Mức nhiên liệu (1: 1/4, 2: 1/2, 3: 3/4, 4: Đầy bình)
    public string LevelOfInspection { get; set; } = "Bảo dưỡng 10.000 km"; // Cấp kiểm tra / bảo dưỡng (LevelOfInspection)
    public string CustomerRequest { get; set; } = "";         // Yêu cầu của khách / Triệu chứng xe (CusRequest)
    public string? ValuablesInCar { get; set; }               // Đồ đạc / Tài sản khách để lại trên xe
    public string? ExteriorCondition { get; set; }            // Ghi chú trầy xước / móp méo bên ngoài thân vỏ
    public bool IsWarranty { get; set; } = false;             // Xe có hạng mục bảo hành hãng (WarrantlyStatus)
    public bool IsInsurance { get; set; } = false;            // Xe có làm bảo hiểm (InsuaranceStatus)
    public bool IsBackRepair { get; set; } = false;           // Xe làm lại / sửa lại lỗi tái phát (BackRepairStatus)
    public ReceptionStatus Status { get; set; } = ReceptionStatus.Pending; // Trạng thái phiếu (P / A)
    public string CreatedBy { get; set; } = "Cố vấn dịch vụ"; // Cố vấn dịch vụ tiếp nhận xe (CreatedBy)
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // Thời điểm tiếp nhận xe
    public DateTime? DeliveryDateTime { get; set; }           // Thời điểm bàn giao trả xe cho khách (DeliveryDateTime)
    public string? DeliveryBy { get; set; }                   // Cố vấn bàn giao xe (DeliveryBy)
    public string? DeliveryNote { get; set; }                 // Ý kiến khách hàng khi nhận bàn giao (DeliveryRemark)

    public Car Car { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Appointment? Appointment { get; set; }
    public RepairOrder? RO { get; set; }
    public List<ReceptionItem> Items { get; set; } = [];

    public int ItemCount => Items.Count;
    public int IssuesCount => Items.Count(i => i.ReceptionStatus == AuditStatus.Attention || i.ReceptionStatus == AuditStatus.Replace);
}

/// <summary>Chi tiết hạng mục kiểm tra quanh xe — Ser_ReceptionFDtl trong idn.CarService.</summary>
public class ReceptionItem : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int ReceptionSheetId { get; set; }
    public string Group { get; set; } = "";                   // Nhóm hạng mục (Khoang lái, Thân vỏ & Đèn, Khoang động cơ, Lốp xe & Phanh, Cốp sau & Đồ nghề)
    public string Code { get; set; } = "";                    // Mã hạng mục (KHOANGLAI.DTL, KHOANGDONGCO.DDC...)
    public string Name { get; set; } = "";                    // Tên hạng mục kiểm tra
    public AuditStatus ReceptionStatus { get; set; } = AuditStatus.Good; // Trạng thái khi tiếp nhận (Đạt, Theo dõi, Cần thay, K/A)
    public AuditStatus DeliveryStatus { get; set; } = AuditStatus.Good;  // Trạng thái khi bàn giao xe
    public string? Note { get; set; }                         // Ghi chú chi tiết hạng mục

    public ReceptionSheet ReceptionSheet { get; set; } = null!;
}

/// <summary>Tổ kỹ thuật / Nhóm thợ sửa chữa xưởng dịch vụ — Ser_GroupRepair trong idn.CarService.</summary>
public class GroupRepair : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string GroupRNo { get; set; } = "";             // Mã tổ thợ (VD: TO-SCC-01, TO-DS-01)
    public string GroupRName { get; set; } = "";           // Tên tổ kỹ thuật (VD: Tổ Sửa chữa chung & Gầm máy)
    public string LeaderName { get; set; } = "";           // Tổ trưởng kỹ thuật
    public string? Note { get; set; }                      // Ghi chú phạm vi nhiệm vụ
    public bool IsActive { get; set; } = true;             // Đang hoạt động
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<Engineer> Engineers { get; set; } = [];
}

/// <summary>Kỹ thuật viên / Thợ sửa chữa dịch vụ xe — Ser_Engineer trong idn.CarService.</summary>
public class Engineer : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string EngineerNo { get; set; } = "";           // Mã nhân viên kỹ thuật (VD: KTV-001)
    public string EngineerName { get; set; } = "";         // Họ tên KTV
    public string? Phone { get; set; }                     // Số điện thoại liên hệ
    public string SkillLevel { get; set; } = "Bậc 3/7";   // Bậc thợ / Trình độ tay nghề
    public string Specialty { get; set; } = "Sửa chữa chung"; // Chuyên môn chính (SCC, Đồng sơn, Điện tử...)
    public int? GroupRId { get; set; }                     // Tổ nhóm thợ trực thuộc
    public bool IsActive { get; set; } = true;             // Đang công tác
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public GroupRepair? GroupRepair { get; set; }
    public List<AssignmentEngineer> Assignments { get; set; } = [];
}

/// <summary>Phiếu phân công công việc & Điều phối lệnh sửa chữa — Ser_AssignmentWork trong idn.CarService.</summary>
public class AssignmentWork : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string AssignmentNo { get; set; } = "";         // Mã phiếu phân công (VD: PC260427-001)
    public int ROId { get; set; }                          // Lệnh sửa chữa gốc
    public AssignmentWorkStatus Status { get; set; } = AssignmentWorkStatus.Assigned; // Trạng thái phân công

    // Phân công Sửa chữa chung (SCC)
    public DateTime? SCCPlanStartDTime { get; set; }       // Giờ kế hoạch bắt đầu SCC
    public DateTime? SCCPlanFinishDTime { get; set; }      // Giờ kế hoạch hoàn tất SCC
    public DateTime? SCCActualStartDTime { get; set; }     // Giờ thực tế bắt đầu SCC
    public DateTime? SCCActualFinishDTime { get; set; }    // Giờ thực tế kết thúc SCC
    public int? SCCCavityId { get; set; }                  // Khoang sửa chữa chung

    // Phân công Sửa chữa đồng (SCD)
    public DateTime? SCDPlanStartDTime { get; set; }       // Giờ kế hoạch bắt đầu SCD
    public DateTime? SCDPlanFinishDTime { get; set; }      // Giờ kế hoạch hoàn tất SCD
    public DateTime? SCDActualStartDTime { get; set; }     // Giờ thực tế bắt đầu SCD
    public DateTime? SCDActualFinishDTime { get; set; }    // Giờ thực tế kết thúc SCD
    public int? SCDCavityId { get; set; }                  // Khoang gò hàn / kéo nắn thân vỏ

    // Phân công Sửa chữa sơn (SCS)
    public DateTime? SCSPlanStartDTime { get; set; }       // Giờ kế hoạch bắt đầu SCS
    public DateTime? SCSPlanFinishDTime { get; set; }      // Giờ kế hoạch hoàn tất SCS
    public DateTime? SCSActualStartDTime { get; set; }     // Giờ thực tế bắt đầu SCS
    public DateTime? SCSActualFinishDTime { get; set; }    // Giờ thực tế kết thúc SCS
    public int? SCSCavityId { get; set; }                  // Buồng sơn sấy / Khoang pha sơn

    public string? Note { get; set; }                      // Ý kiến chỉ đạo kỹ thuật của Quản đốc
    public string CreatedBy { get; set; } = "Quản đốc xưởng"; // Người lập phiếu phân công
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? StartedAt { get; set; }               // Thời điểm bắt đầu công việc
    public DateTime? FinishedAt { get; set; }              // Thời điểm hoàn tất công việc

    public RepairOrder RO { get; set; } = null!;
    public Cavity? SCCCavity { get; set; }
    public Cavity? SCDCavity { get; set; }
    public Cavity? SCSCavity { get; set; }
    public List<AssignmentEngineer> Engineers { get; set; } = [];

    public string PrimaryTechnician => Engineers.FirstOrDefault(e => e.IsPrimary)?.Engineer?.EngineerName
        ?? Engineers.FirstOrDefault()?.Engineer?.EngineerName ?? "Chưa chỉ định";

    public bool HasSCC => SCCPlanStartDTime.HasValue || SCCCavityId.HasValue;
    public bool HasSCD => SCDPlanStartDTime.HasValue || SCDCavityId.HasValue;
    public bool HasSCS => SCSPlanStartDTime.HasValue || SCSCavityId.HasValue;
    public int EngineerCount => Engineers.Count;
    public decimal TotalAssignedHours => Engineers.Sum(e => e.AssignedHours);
}

/// <summary>Chi tiết kỹ thuật viên tham gia phân công công việc — Ser_AssignmentWorkEngineer trong idn.CarService.</summary>
public class AssignmentEngineer : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int AssignmentWorkId { get; set; }
    public int EngineerId { get; set; }
    public WorkType WorkType { get; set; } = WorkType.SCC; // Công đoạn sửa chữa (SCC, SCD, SCS)
    public bool IsPrimary { get; set; } = true;            // Cờ kỹ thuật viên chính chịu trách nhiệm
    public decimal AssignedHours { get; set; } = 1.0m;     // Định mức giờ công giao cho KTV
    public string? Note { get; set; }                      // Nhiệm vụ cụ thể

    public AssignmentWork AssignmentWork { get; set; } = null!;
    public Engineer Engineer { get; set; } = null!;
}

/// <summary>Hãng / Công ty Bảo hiểm liên kết — Ser_Insurance trong idn.CarService.</summary>
public class InsuranceCompany : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string InsNo { get; set; } = "";             // Mã hãng BH (VD: BH-BV, BH-PVI, BH-PTI)
    public string InsName { get; set; } = "";           // Tên hãng bảo hiểm (VD: Bảo hiểm Bảo Việt)
    public string? Address { get; set; }                // Địa chỉ trụ sở/chi nhánh
    public string? Phone { get; set; }                  // Đường dây nóng / Điện thoại
    public string? Email { get; set; }                  // Email nhận hồ sơ bồi thường
    public string? TaxCode { get; set; }                // Mã số thuế
    public string? Hotline { get; set; }                // Tổng đài cứu hộ / bồi thường
    public bool IsActive { get; set; } = true;          // Đang hợp tác
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<InsuranceContract> Contracts { get; set; } = [];
    public List<InsuranceClaim> Claims { get; set; } = [];
}

/// <summary>Hợp đồng liên kết bảo hiểm với xưởng dịch vụ — Ser_InsuranceContract trong idn.CarService.</summary>
public class InsuranceContract : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string ContractNo { get; set; } = "";        // Số hợp đồng (VD: HD-BV/2026/01)
    public string ContractCode { get; set; } = "";      // Mã hợp đồng quản lý (VD: HDBV-01)
    public int InsuranceCompanyId { get; set; }         // Hãng bảo hiểm ký kết
    public DateTime StartDate { get; set; }             // Ngày bắt đầu hiệu lực
    public DateTime FinishDate { get; set; }            // Ngày hết hạn hiệu lực
    public InsurancePaymentType PaymentType { get; set; } = InsurancePaymentType.DirectGuarantee; // Hình thức thanh toán
    public decimal PaymentLimit { get; set; } = 500_000_000m; // Hạn mức bảo lãnh thanh toán (VNĐ)
    public decimal DiscountLaborRate { get; set; } = 10m;     // Chiết khấu tiền công cho bảo hiểm (%)
    public decimal DiscountPartRate { get; set; } = 5m;       // Chiết khấu phụ tùng cho bảo hiểm (%)
    public string? Note { get; set; }                   // Điều khoản đặc biệt
    public bool IsActive { get; set; } = true;          // Còn hiệu lực
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public InsuranceCompany InsuranceCompany { get; set; } = null!;
    public List<InsuranceClaim> Claims { get; set; } = [];
}

/// <summary>Hồ sơ yêu cầu bồi thường bảo hiểm xe — Ser_Insurance / Ser_InsuranceDebit trong idn.CarService.</summary>
public class InsuranceClaim : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string ClaimNo { get; set; } = "";           // Mã hồ sơ bồi thường (VD: CLM260427-001)
    public int ROId { get; set; }                       // Lệnh sửa chữa gắn với hồ sơ bồi thường
    public int InsuranceCompanyId { get; set; }         // Công ty bảo hiểm bồi thường
    public int? InsuranceContractId { get; set; }       // Hợp đồng bảo hiểm áp dụng
    public string PolicyNo { get; set; } = "";          // Số giấy chứng nhận bảo hiểm / Số đơn bảo hiểm
    public string? ClaimFileNo { get; set; }            // Mã vụ tổn thất / Số hồ sơ của hãng bảo hiểm
    public string? SurveyorName { get; set; }           // Họ tên Giám định viên đại diện bảo hiểm
    public string? SurveyorPhone { get; set; }          // Số điện thoại Giám định viên
    public DateTime AccidentDate { get; set; } = DateTime.Today; // Ngày xảy ra sự vụ tổn thất
    public string? AccidentLocation { get; set; }       // Nơi xảy ra sự việc (địa điểm tai nạn)
    public string AccidentDescription { get; set; } = ""; // Tình trạng va chạm & hiện trường tai nạn
    public decimal EstimatedAmount { get; set; }        // Tổng chi phí sửa chữa dự toán (VNĐ)
    public decimal ApprovedAmount { get; set; }         // Số tiền bảo hiểm đồng ý duyệt bồi thường (VNĐ)
    public decimal DeductibleAmount { get; set; } = 500_000m; // Mức khấu trừ / Miễn thường khách chịu (InsuranceMienThuong)
    public decimal PenaltyAmount { get; set; } = 0;     // Số tiền chế tài giảm trừ bồi thường nếu có (InsuranceCheTai)
    public decimal InsuranceAmount { get; set; }        // Số tiền bảo hiểm bảo lãnh trả xưởng = ApprovedAmount - DeductibleAmount - PenaltyAmount
    public decimal CustomerAmount { get; set; }         // Số tiền khách hàng tự chi trả = DeductibleAmount + PenaltyAmount + (EstimatedAmount - ApprovedAmount)
    public InsuranceClaimStatus Status { get; set; } = InsuranceClaimStatus.Draft; // Trạng thái hồ sơ
    public string? DecisionNote { get; set; }           // Ghi chú quyết định / Ý kiến bảo lãnh của hãng bảo hiểm
    public string? RejectionReason { get; set; }        // Lý do từ chối bồi thường nếu có
    public string CreatedBy { get; set; } = "Cố vấn dịch vụ"; // Cố vấn phụ trách hồ sơ
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? SubmittedAt { get; set; }          // Ngày gửi hồ sơ giám định
    public DateTime? ApprovedAt { get; set; }           // Ngày hãng bảo hiểm duyệt bảo lãnh
    public DateTime? SettledAt { get; set; }            // Ngày quyết toán hoàn tất

    public RepairOrder RO { get; set; } = null!;
    public InsuranceCompany InsuranceCompany { get; set; } = null!;
    public InsuranceContract? InsuranceContract { get; set; }
    public List<InsuranceClaimItem> Items { get; set; } = [];

    public int ItemCount => Items.Count;
}

/// <summary>Chi tiết hạng mục bồi thường bảo hiểm — Ser_InsuranceDebitDetail / RO Line trong idn.CarService.</summary>
public class InsuranceClaimItem : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int InsuranceClaimId { get; set; }
    public LineType Type { get; set; } = LineType.Labor; // Tiền công hoặc Phụ tùng
    public string Code { get; set; } = "";               // Mã phụ tùng hoặc mã công việc (Gò, Hàn, Sơn, Thay thế...)
    public string Name { get; set; } = "";               // Tên hạng mục sửa chữa phục hồi
    public decimal Quantity { get; set; } = 1;           // Số lượng
    public decimal UnitPrice { get; set; }               // Đơn giá
    public decimal EstimatedAmount { get; set; }         // Số tiền yêu cầu bồi thường
    public decimal ApprovedAmount { get; set; }          // Số tiền bảo hiểm chấp thuận chi trả
    public bool IsApproved { get; set; } = true;         // Được duyệt bồi thường hay bị loại trừ
    public string? Note { get; set; }                    // Diễn giải / Đánh giá giám định

    public InsuranceClaim InsuranceClaim { get; set; } = null!;
}

/// <summary>Chiến dịch Khuyến mãi & Tiếp thị Dịch vụ xe — Ser_CampaignMarketing trong idn.CarService.</summary>
public class CampaignMarketing : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string CamMarketingNo { get; set; } = "";             // Mã chiến dịch (VD: KM-HE2026, KM-ACCENT-01)
    public string CamMarketingName { get; set; } = "";           // Tên chiến dịch
    public string? CamMarketingDesc { get; set; }                // Nội dung & Thể lệ chương trình
    public DateTime EffDateStart { get; set; } = DateTime.Today; // Ngày bắt đầu hiệu lực
    public DateTime EffDateEnd { get; set; } = DateTime.Today.AddMonths(1); // Ngày kết thúc hiệu lực
    public DateTime? WarrantyDateStart { get; set; }             // Đk: Ngày kích hoạt bảo hành từ (áp dụng đời xe)
    public DateTime? WarrantyDateEnd { get; set; }               // Đk: Ngày kích hoạt bảo hành đến
    public string? ConditionModel { get; set; }                  // Đk dòng xe (VD: Accent, Tucson, Santa Fe...)
    public string? ConditionPlateNo { get; set; }                // Đk đầu biển số xe (VD: 29, 30, 51...)
    public string? ConditionVIN { get; set; }                    // Đk chuỗi ký tự trong VIN (VD: RLH...)
    public decimal DiscountLaborPercent { get; set; } = 0;       // % Giảm giá tiền công chung
    public decimal DiscountPartPercent { get; set; } = 0;        // % Giảm giá phụ tùng chung
    public CampaignMarketingStatus Status { get; set; } = CampaignMarketingStatus.Active; // Trạng thái
    public string CreatedBy { get; set; } = "Cố vấn dịch vụ";   // Người tạo chiến dịch
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ApprovedAt { get; set; }                    // Ngày duyệt áp dụng
    public string? ApprovedBy { get; set; }                      // Người phê duyệt

    public List<CampaignMarketingItem> Items { get; set; } = [];
    public List<RepairOrder> AppliedROs { get; set; } = [];

    public int ItemCount => Items.Count;
    public int ROAppliedCount => AppliedROs.Count;
    public bool IsActiveNow => Status == CampaignMarketingStatus.Active && DateTime.Today >= EffDateStart.Date && DateTime.Today <= EffDateEnd.Date;
    public decimal TotalDiscountGranted => AppliedROs.Sum(r => r.CampaignDiscountAmount);
    public decimal TotalRevenueGenerated => AppliedROs.Sum(r => r.Total);
}

/// <summary>Chi tiết phụ tùng áp dụng ưu đãi trong chiến dịch — Ser_CampaignMarketingPart trong idn.CarService.</summary>
public class CampaignMarketingItem : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int CampaignMarketingId { get; set; }
    public int PartId { get; set; }                              // ID Phụ tùng trong kho
    public string PartCode { get; set; } = "";                   // Mã phụ tùng (PartCode)
    public string PartName { get; set; } = "";                   // Tên phụ tùng (VieName)
    public decimal PercentDiscount { get; set; } = 15;           // % Chiết khấu giảm giá dòng phụ tùng này
    public decimal MaxQuantity { get; set; } = 999;              // Số lượng tối đa áp dụng mỗi xe
    public string? Note { get; set; }                            // Ghi chú hạng mục

    public CampaignMarketing CampaignMarketing { get; set; } = null!;
    public Part Part { get; set; } = null!;
}

/// <summary>Phiếu nhắc bảo dưỡng định kỳ xe — Ser_CustomerCareMace trong idn.CarService.</summary>
public class CustomerCareMace : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string MaceNo { get; set; } = "";             // Mã phiếu nhắc bảo dưỡng (VD: MC260427-001)
    public int? ROId { get; set; }                       // Lệnh sửa chữa gần nhất kích hoạt mốc nhắc
    public int CarId { get; set; }                       // Xe cần bảo dưỡng
    public int CustomerId { get; set; }                  // Khách hàng / Chủ xe
    public MaceType MaceType { get; set; } = MaceType.Standard6Months; // Phương thức tính mốc nhắc (CVDV chỉ định, 6 tháng, Tần suất vào xưởng)
    public int LastKm { get; set; }                      // Số km lần bảo dưỡng trước đó
    public int NextKm { get; set; }                      // Mốc km dự kiến tiếp theo (5.000, 10.000, 15.000, 20.000...)
    public DateTime MaceRecomentDate { get; set; }       // Ngày khuyến nghị bảo dưỡng
    public CustomerCareMaceStatus Status { get; set; } = CustomerCareMaceStatus.Pending; // Trạng thái chăm sóc
    public DateTime? ContactDate { get; set; }           // Thời điểm liên hệ
    public string? ContactBy { get; set; }               // Nhân viên CSKH thực hiện gọi điện
    public DateTime? ApointDate { get; set; }            // Ngày hẹn khách đồng ý mang xe đến
    public string? Remark { get; set; }                  // Ghi chú cuộc gọi / Phản hồi của khách
    public int? AppointmentId { get; set; }              // Cuộc hẹn sinh ra khi chuyển đổi thành công
    public string CreatedBy { get; set; } = "system";    // Tự động sinh từ RO hoặc do CVDV lập
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    public RepairOrder? RO { get; set; }
    public Car Car { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Appointment? Appointment { get; set; }

    public bool IsOverdue => Status == CustomerCareMaceStatus.Pending && DateTime.Today > MaceRecomentDate.Date;
    public bool IsDueSoon => Status == CustomerCareMaceStatus.Pending && DateTime.Today <= MaceRecomentDate.Date && MaceRecomentDate.Date <= DateTime.Today.AddDays(7);
}

/// <summary>Phiếu kiểm kê & Điều chuyển / Điều chỉnh kho phụ tùng — Ser_Inv_StockAdj trong idn.CarService.</summary>
public class StockAdj : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string StockAdjNo { get; set; } = "";             // Số phiếu điều chỉnh (VD: KK260427-001, DC260427-001)
    public DateTime StockAdjDate { get; set; } = DateTime.Today; // Ngày kiểm kê / điều chuyển
    public StockAdjType Type { get; set; } = StockAdjType.CountBalance; // Loại kiểm kê / điều chuyển
    public StockAdjStatus Status { get; set; } = StockAdjStatus.Pending; // Trạng thái phiếu
    public string StorageCode { get; set; } = "KHO-CHINH";  // Mã kho phụ tùng
    public string? Remark { get; set; }                     // Diễn giải / Mục đích kiểm kê
    public string CreatedBy { get; set; } = "Thủ kho";      // Người tạo phiếu
    public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo phiếu
    public string? ApprovedBy { get; set; }                 // Người phê duyệt điều chỉnh
    public DateTime? FinishedAt { get; set; }               // Ngày hoàn tất cập nhật kho

    public List<StockAdjDetail> Items { get; set; } = [];

    public int TotalItems => Items.Count;
    public decimal TotalSystemQty => Items.Sum(i => i.SystemQuantity);
    public decimal TotalActualQty => Items.Sum(i => i.ActualQuantity);
    public decimal TotalDiffQty => Items.Sum(i => i.DiffQuantity);
    public decimal TotalDiffAmount => Items.Sum(i => i.DiffAmount);
    public bool HasDiscrepancy => Items.Any(i => i.DiffQuantity != 0);
    public int DiscrepancyCount => Items.Count(i => i.DiffQuantity != 0);
}

/// <summary>Chi tiết phụ tùng kiểm kê & điều chuyển kho — Ser_Inv_StockAdjDetail trong idn.CarService.</summary>
public class StockAdjDetail : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int StockAdjId { get; set; }
    public int PartId { get; set; }                         // Phụ tùng trong danh mục
    public string PartCode { get; set; } = "";              // Mã phụ tùng (PartCode)
    public string PartName { get; set; } = "";              // Tên phụ tùng (VieName)
    public string Unit { get; set; } = "Cái";               // Đơn vị tính (Unit)
    public decimal CostPrice { get; set; }                  // Giá vốn tại thời điểm kiểm kê
    public decimal SystemQuantity { get; set; }             // Số lượng tồn sổ sách (BalanceQuantity)
    public decimal ActualQuantity { get; set; }             // Số lượng thực tế kiểm đếm / chuyển (Quantity)
    public string? FromLocation { get; set; }               // Vị trí lưu kho hiện tại (BalanceLocationID)
    public string? ToLocation { get; set; }                 // Vị trí kệ kho chuyển đến (InStockLocationId)
    public string? Note { get; set; }                       // Nguyên nhân chênh lệch / Ghi chú

    public StockAdj StockAdj { get; set; } = null!;
    public Part Part { get; set; } = null!;

    public decimal DiffQuantity => ActualQuantity - SystemQuantity;
    public decimal DiffAmount => (ActualQuantity - SystemQuantity) * CostPrice;
}

/// <summary>Bản tin kỹ thuật dịch vụ & Chiến dịch triệu hồi xe — Btl_Bulletin trong idn.CarService.</summary>
public class Bulletin : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string BulletinNo { get; set; } = "";             // Số bản tin HTC (VD: TSB-2026-001, CAM-RECALL-HYU01)
    public string? BulletinNoHMC { get; set; }               // Số bản tin tập đoàn Hyundai toàn cầu (VD: HMC-TSB-24-01-002)
    public string Title { get; set; } = "";                  // Tiêu đề bản tin kỹ thuật
    public string? Remark { get; set; }                      // Hiện tượng hư hỏng / Nguyên nhân kỹ thuật
    public string? Solution { get; set; }                    // Hướng dẫn xử lý / Phương án khắc phục tiêu chuẩn HTC
    public DateTime CreateDate { get; set; } = DateTime.Today; // Ngày ban hành bản tin
    public DateTime? DateExpired { get; set; }               // Hạn áp dụng bản tin
    public bool IsActive { get; set; } = true;               // Cờ hiệu lực (IsActive)
    public BulletinStatus Status { get; set; } = BulletinStatus.Active; // Trạng thái bản tin
    public string UserCreate { get; set; } = "Hyundai Thành Công (HTC)"; // Người / Đơn vị ban hành
    public string? FileNameAttachment { get; set; }          // Tài liệu kỹ thuật / Hướng dẫn đính kèm
    public string CreatedBy { get; set; } = "web";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<BulletinDetail> Items { get; set; } = [];
    public List<BulletinVin> TargetVins { get; set; } = [];
    public List<RepairOrder> AppliedROs { get; set; } = [];

    public int TotalVinCount => TargetVins.Count;
    public int CompletedVinCount => TargetVins.Count(v => v.Status == BulletinVinStatus.Completed);
    public int PendingVinCount => TargetVins.Count(v => v.Status == BulletinVinStatus.Pending);
    public decimal CompletionRate => TotalVinCount == 0 ? 0 : Math.Round((decimal)CompletedVinCount * 100 / TotalVinCount, 1);
    public bool IsExpired => DateExpired.HasValue && DateTime.Today > DateExpired.Value.Date;
}

/// <summary>Hạng mục công việc & Phụ tùng trong Bản tin kỹ thuật — Btl_BulletinDtl trong idn.CarService.</summary>
public class BulletinDetail : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int BulletinId { get; set; }
    public LineType Type { get; set; } = LineType.Labor;     // Công lao động hoặc Phụ tùng thay thế
    public int? PartId { get; set; }                         // Phụ tùng liên kết trong kho nếu là Part
    public string Code { get; set; } = "";                   // Mã công việc (SerCode) hoặc Mã phụ tùng (PartCode)
    public string Name { get; set; } = "";                   // Tên công việc (SerName) hoặc Tên phụ tùng (PartName)
    public string Unit { get; set; } = "Cái";                // Đơn vị tính
    public decimal Quantity { get; set; } = 1;               // Số lượng / Giờ công quy định
    public decimal UnitPrice { get; set; } = 0;              // Đơn giá định mức bồi hoàn hãng HTC (0đ hoặc giá hãng thanh toán)
    public string? Note { get; set; }                        // Hướng dẫn kỹ thuật cụ thể

    public Bulletin Bulletin { get; set; } = null!;
    public Part? Part { get; set; }

    public decimal Amount => Quantity * UnitPrice;
}

/// <summary>Danh sách xe áp dụng theo số khung VIN — Btl_Bulletin_VIN trong idn.CarService.</summary>
public class BulletinVin : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int BulletinId { get; set; }
    public string VinNo { get; set; } = "";                  // Số khung xe (VinNo)
    public string? PlateNo { get; set; }                     // Biển số xe (nếu đã đăng ký)
    public string? Model { get; set; }                       // Dòng xe tương thích (Accent, Tucson, Santa Fe...)
    public string? DealerCode { get; set; } = "HYUNDAI-MAIN";// Đại lý phụ trách thực hiện
    public BulletinVinStatus Status { get; set; } = BulletinVinStatus.Pending; // Trạng thái thực hiện (P: Pending, F: Finished)
    public DateTime? DateDone { get; set; }                  // Thời điểm hoàn tất xử lý
    public string? DoneBy { get; set; }                      // Kỹ thuật viên / CVDV thực hiện
    public int? ROId { get; set; }                           // Lệnh sửa chữa RO đã xử lý
    public string? RONo { get; set; }                        // Số Lệnh sửa chữa RO
    public string? Note { get; set; }                        // Ghi chú kiểm tra xe

    public Bulletin Bulletin { get; set; } = null!;
    public RepairOrder? RO { get; set; }
}


