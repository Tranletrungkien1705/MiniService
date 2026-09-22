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

public class Customer : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public List<Car> Cars { get; set; } = [];
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
    public List<RepairLine> Lines { get; set; } = [];
    public List<WarrantyReport> WarrantyReports { get; set; } = [];
    public List<StockOut> StockOuts { get; set; } = [];

    public decimal Total => Lines.Sum(l => l.Amount);
    public decimal LaborTotal => Lines.Where(l => l.Type == LineType.Labor).Sum(l => l.Amount);
    public decimal PartTotal => Lines.Where(l => l.Type == LineType.Part).Sum(l => l.Amount);
    public decimal CustomerTotal => Lines.Where(l => l.ExpenseType == ExpenseType.Customer).Sum(l => l.Amount);
    public decimal WarrantyTotal => Lines.Where(l => l.ExpenseType == ExpenseType.Warranty).Sum(l => l.Amount);
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
    public string CreatedBy { get; set; } = "web";          // Người tạo phiếu
    public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo
    public DateTime? FinishedAt { get; set; }               // Ngày duyệt nhập kho
    public string? ApprovedBy { get; set; }                 // Người duyệt nhập kho

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
    public string? RecipientName { get; set; }              // Người nhận hàng / Kỹ thuật viên / Khách
    public string? Description { get; set; }                // Diễn giải / Lý do xuất kho
    public string CreatedBy { get; set; } = "web";          // Người lập phiếu
    public DateTime CreatedAt { get; set; } = DateTime.Now; // Thời điểm lập phiếu
    public DateTime? FinishedAt { get; set; }               // Ngày hoàn tất xuất kho (trừ tồn)
    public string? ApprovedBy { get; set; }                 // Thủ kho duyệt xuất

    public RepairOrder? RO { get; set; }
    public Customer? Customer { get; set; }
    public Car? Car { get; set; }
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
