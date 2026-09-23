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

/// <summary>Trạng thái Phiếu yêu cầu kiểm tra xuất xưởng xe PDI — theo Dlr_PDIRequest idn.CarService.</summary>
public enum PdiRequestStatus
{
    Draft = 0,     // DRAFT     — Dự thảo / Lập yêu cầu
    Pending = 1,   // PENDING   — Chờ xưởng dịch vụ tiếp nhận
    Approved = 2,  // APPROVED  — Xưởng tiếp nhận & đang tiến hành kiểm tra
    Completed = 3, // COMPLETED — Đã nghiệm thu hoàn tất / Sẵn sàng giao xe (Ready for Delivery)
    Cancelled = 4  // CANCELLED — Đã hủy yêu cầu PDI
}

/// <summary>Trạng thái kiểm tra kỹ thuật từng xe trong phiếu PDI — theo Dlr_PDIRequestDtl idn.CarService.</summary>
public enum PdiItemStatus
{
    Pending = 0,    // Chờ kiểm tra
    InProgress = 1, // Đang kiểm tra & lắp đặt phụ kiện
    Passed = 2,     // Đạt chuẩn PDI (Sẵn sàng bàn giao xe)
    Failed = 3      // Không đạt / Cần khắc phục kỹ thuật
}

/// <summary>Trạng thái khiếu nại đơn hàng phụ tùng tại Đại lý DMS — theo DMSOrderComplainStatus idn.CarService.</summary>
public enum DMSOrderComplainStatus
{
    Pending = 0,    // P: Mới tạo / Chờ gửi khiếu nại sang NCC TST
    Sent = 1,       // A: Đã gửi khiếu nại sang TST chờ thẩm định
    Finished = 2,   // F: Hoàn tất giải quyết khiếu nại (đã nhận bồi thường/đổi hàng)
    Cancelled = 3   // C: Hủy khiếu nại
}

/// <summary>Trạng thái thẩm định khiếu nại của Nhà cung cấp TST / HTC — theo TSTOrderComplainStatus idn.CarService.</summary>
public enum TSTOrderComplainStatus
{
    Processing = 1,  // 1: Chờ tiếp nhận duyệt
    UnderReview = 15,// 15: Đang thẩm định & giám định hư hỏng
    Rejected = 21,   // 21: Không chấp thuận bồi thường khiếu nại
    Approved = 31    // 31: Chấp thuận khiếu nại bồi thường
}

/// <summary>Phân loại khiếu nại phụ tùng — theo Mst_OrderComplainType idn.CarService.</summary>
public enum OrderComplainType
{
    DamagedInTransit = 0, // Hàng vỡ móp, nứt vỡ, móp méo do vận chuyển
    WrongPart = 1,        // Giao sai mã phụ tùng / Sai quy cách
    Shortage = 2,         // Thiếu hụt số lượng so với đơn đặt hàng & phiếu giao
    QualityDefect = 3,    // Lỗi chất lượng sản xuất / Khuyết tật xuất xưởng
    PackagingBreach = 4   // Bao bì rách nát, tem mác rách niêm phong
}

/// <summary>Phương án giải quyết từ Nhà cung cấp TST/HTC — theo TSTSolution idn.CarService.</summary>
public enum ComplainSolution
{
    ReplaceNew = 0,   // Đổi mới phụ tùng 1:1 (Giao bù hàng chuẩn)
    CreditDebt = 1,   // Bồi hoàn tiền / Cấn trừ công nợ đại lý
    ReturnRefund = 2, // Thu hồi hàng lỗi & hoàn lại tiền
    RejectClaim = 3   // Từ chối bồi hoàn (Lỗi do ngoại lực/bảo quản)
}

/// <summary>Phân loại cẩm nang kỹ thuật — theo Type trong Ser_Technical_Library idn.CarService.</summary>
public enum TechnicalLibraryType
{
    Normal = 0,    // "0" — Cẩm nang kỹ thuật thông thường / Tài liệu tiêu chuẩn xưởng
    ReRepair = 1   // "1" — Xử lý pan bệnh khó & Phản tu lặp lại (Re-Repair Case)
}

/// <summary>Phân loại hệ thống kỹ thuật xe — theo ReRepairType trong Ser_Technical_Library idn.CarService.</summary>
public enum TechnicalLibraryReRepairType
{
    Engine = 0,          // Động cơ & Nhiên liệu
    Transmission = 1,    // Hộp số & Hệ thống truyền lực
    Electrical = 2,      // Hệ thống Điện - Điện tử ô tô & Cảm biến
    Chassis = 3,         // Khung gầm, Hệ thống treo & Lái
    BrakeADAS = 4,       // Phanh an toàn & Hệ thống hỗ trợ lái ADAS (SmartSense)
    AirConditioning = 5, // Hệ thống Điều hòa nhiệt độ ô tô (HVAC)
    BodyPaint = 6        // Thân vỏ & Sơn (Body & Paint)
}

/// <summary>Phân loại nghiệp vụ công việc dịch vụ — theo ROTYPE trong Ser_MST_Service idn.CarService.</summary>
public enum ServiceROType
{
    BDD = 0, // Bảo dưỡng định kỳ (Periodic Maintenance)
    SCC = 1, // Sửa chữa chung máy - gầm - điện (General Repair)
    SCD = 2, // Đồng sơn & Sơn sấy thân vỏ (Body & Paint)
    SCS = 3, // Dịch vụ sửa chữa nhanh (Quick Service / Express Service)
    PDI = 4, // Kiểm tra nghiệm thu xe mới xuất xưởng (Pre-Delivery Inspection)
    SPK = 5  // Chăm sóc & Phụ kiện xe (Detailing & Accessories)
}

/// <summary>Trạng thái Phiếu xuất trả Nhà cung cấp — theo Ser_SupplierPayment trong idn.CarService.</summary>
public enum SupplierPaymentStatus
{
    Pending = 0,   // P: Mới tạo / Chờ duyệt xuất kho
    Approved = 1,  // A: Đã duyệt xuất kho trả hàng & Đã trừ tồn kho
    Cancelled = 2  // C: Đã hủy phiếu xuất trả
}

/// <summary>Loại yêu cầu xuất trả hàng cho Nhà cung cấp — theo PaymentType trong Ser_SupplierPayment idn.CarService.</summary>
public enum SupplierPaymentType
{
    ReturnDefective = 0,   // "0" — Hàng lỗi chất lượng / Hư hại xuất xưởng
    ReturnSurplus = 1,     // "1" — Giao thừa / Sai quy cách so với đơn đặt hàng
    RecallWarranty = 2,    // "2" — Thu hồi bảo hành kỹ thuật theo yêu cầu Hãng HTC
    ConsignmentReturn = 3  // "3" — Trả hàng ký gửi / Tồn kho thỏa thuận hợp đồng
}

/// <summary>Trạng thái Phiếu yêu cầu xuất kho phụ tùng dịch vụ — theo Ser_Inv_StockOutOrder Status (Pending, Accept, Finish, Rejected) idn.CarService.</summary>
public enum StockOutOrderStatus
{
    Pending = 0,   // Mới tạo / Chờ xuất (FlagPending)
    Approved = 1,  // Đã duyệt / Sẵn sàng xuất (FlagAccept)
    Completed = 2, // Đã xuất kho hoàn tất / Đã tạo phiếu xuất (FlagFinish)
    Rejected = 3   // Đã từ chối / Hủy yêu cầu (FlagRejected)
}

/// <summary>Mức độ ưu tiên yêu cầu xuất phụ tùng — theo Priority trong Ser_Inv_StockOutOrder idn.CarService.</summary>
public enum StockOutOrderPriority
{
    Normal = 0,    // Bình thường / Tiêu chuẩn
    Urgent = 1,    // Ưu tiên / Khẩn cấp
    Emergency = 2  // Hỏa tốc / Dừng xe (VOR)
}

/// <summary>Trạng thái Phiếu nợ phụ tùng khách hàng — theo Ser_Part_OO trong idn.CarService.</summary>
public enum PartOOStatus
{
    Owed = 0,       // 0: Còn nợ khách / Đang chờ hàng về kho
    Arrived = 1,    // 1: Hàng đã về kho / Sẵn sàng hẹn khách đến lắp
    Completed = 2,  // 2: Đã trả đủ / Đã lắp bù hoàn tất cho khách
    Cancelled = 3   // 3: Đã hủy nợ / Khách từ chối hoặc bồi hoàn tiền
}

/// <summary>Phân loại công nợ khách hàng dịch vụ — theo Ser_CusDebit DebitType idn.CarService (1: RO, 2: Part, 3: Other).</summary>
public enum CusDebitType
{
    RO = 1,       // 1: Nợ Lệnh sửa chữa xe (Repair Order)
    Part = 2,     // 2: Nợ mua Phụ tùng / Bán lẻ xuất kho
    Other = 3     // 3: Nợ dịch vụ khác / Gia công ngoài
}

/// <summary>Trạng thái công nợ khách hàng — theo Ser_CusDebit idn.CarService.</summary>
public enum CusDebitStatus
{
    Active = 1,    // 1: Còn nợ (Active / Unpaid / Partially Paid)
    Cleared = 2,   // 2: Đã tất toán (Cleared / Fully Paid)
    Cancelled = 3  // 3: Đã hủy nợ (Cancelled)
}

/// <summary>Phân loại công nợ Nhà cung cấp — theo Ser_SupplierDebit DebitType trong idn.CarService (DebitType = '3').</summary>
public enum SupplierDebitType
{
    StockIn = 1,        // 1: Nợ tiền hàng Nhập kho phụ tùng (StockIn)
    Shipping = 2,       // 2: Cước phí kho vận / Vận chuyển linh kiện
    EmergencyOrder = 3, // 3: Đơn hàng khẩn cấp đặt nhanh (VOR)
    Other = 4           // 4: Phát sinh khác / Dịch vụ ngoài
}

/// <summary>Trạng thái công nợ Nhà cung cấp — theo Ser_SupplierDebit trong idn.CarService.</summary>
public enum SupplierDebitStatus
{
    Active = 1,    // 1: Còn nợ NCC (Active / Unpaid / Partially Paid)
    Cleared = 2,   // 2: Đã tất toán đủ (Cleared / Fully Paid)
    Cancelled = 3  // 3: Đã hủy phiếu nợ (Cancelled)
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
    public List<CusDebit> CusDebits { get; set; } = [];
    public List<CusDebitPayment> CusDebitPayments { get; set; } = [];
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
    public int? PdiRequestId { get; set; }
    public string? PdiReqNo { get; set; }
    public PdiRequest? PdiRequest { get; set; }
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
    public List<PdiRequestItem> PdiRequestItems { get; set; } = [];
    public List<TechnicalLibrary> TechnicalLibraries { get; set; } = [];
    public List<StockOutOrder> StockOutOrders { get; set; } = [];
    public List<PartOO> PartOOs { get; set; } = [];
    public List<CusDebit> CusDebits { get; set; } = [];

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

    public List<PartOO> PartOOs { get; set; } = [];

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
    public int? ServiceItemId { get; set; }         // Liên kết danh mục công việc chuẩn nếu có (Ser_MST_Service)
    public decimal? StdManHour { get; set; }        // Giờ công tiêu chuẩn định mức (StdManHour)
    public string Name { get; set; } = "";
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Amount => Quantity * UnitPrice;
    public RepairOrder RO { get; set; } = null!;
    public Part? Part { get; set; }
    public ServiceItem? ServiceItem { get; set; }
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
    public List<SupplierDebit> SupplierDebits { get; set; } = [];

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
    public int? StockOutOrderId { get; set; }               // Yêu cầu xuất kho phụ tùng gốc nếu xuất theo yêu cầu
    public StockOutOrder? StockOutOrder { get; set; }

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
    public List<OrderComplain> Complains { get; set; } = [];
    public List<SupplierPayment> SupplierPayments { get; set; } = [];

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

/// <summary>Yêu cầu kiểm tra & nghiệm thu xe mới xuất xưởng PDI (Pre-Delivery Inspection) — Dlr_PDIRequest trong idn.CarService.</summary>
public class PdiRequest : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string PdiReqNo { get; set; } = "";             // Số phiếu PDI (VD: PDI260427-001)
    public string DealerCode { get; set; } = "HYUNDAI-MAIN";// Mã đại lý
    public DateTime CreatedDate { get; set; } = DateTime.Today; // Ngày lập yêu cầu
    public DateTime? ApprovedDate { get; set; }            // Ngày xưởng tiếp nhận
    public string? Remark { get; set; }                    // Diễn giải / Nội dung yêu cầu từ Sales
    public PdiRequestStatus Status { get; set; } = PdiRequestStatus.Pending; // Trạng thái phiếu PDI
    public bool FlagAccessory { get; set; } = false;       // Yêu cầu lắp thêm phụ kiện bàn giao
    public string CreatedBy { get; set; } = "Phòng Bán hàng (DMS Sales)"; // Bộ phận / Người tạo yêu cầu
    public string? ApprovedBy { get; set; }                // Cố vấn / Quản đốc xưởng tiếp nhận
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? FinishedAt { get; set; }              // Thời điểm hoàn tất nghiệm thu toàn bộ xe

    public List<PdiRequestItem> Items { get; set; } = [];
    public List<RepairOrder> RepairOrders { get; set; } = [];

    public int VinTotal => Items.Count;
    public int VinFTotal => Items.Count(i => i.Status == PdiItemStatus.Passed);
    public int VinPendingTotal => Items.Count(i => i.Status == PdiItemStatus.Pending || i.Status == PdiItemStatus.InProgress);
    public decimal CompletionRate => VinTotal == 0 ? 0 : Math.Round((decimal)VinFTotal * 100 / VinTotal, 1);
    public bool IsAllPassed => VinTotal > 0 && VinFTotal == VinTotal;
}

/// <summary>Chi tiết xe trong phiếu yêu cầu PDI — Dlr_PDIRequestDtl trong idn.CarService.</summary>
public class PdiRequestItem : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int PdiRequestId { get; set; }
    public string VIN { get; set; } = "";                  // Số khung xe (17 ký tự, ví dụ: RLHXX...)
    public string Model { get; set; } = "";                // Dòng xe (Santa Fe, Tucson, Creta, Accent...)
    public string? Spec { get; set; }                      // Phiên bản xe (1.5 Cao cấp, 2.0 Turbo, Tiêu chuẩn...)
    public string? Color { get; set; }                     // Màu sơn ngoại thất (Trắng ngọc trai, Đen, Đỏ, Bạc...)
    public string? EngineNo { get; set; }                  // Số máy
    public string? BatteryNo { get; set; }                 // Số sê-ri ắc quy
    public DateTime ExpectedDeliveryDate { get; set; } = DateTime.Today.AddDays(2); // Ngày dự kiến giao xe cho khách hàng
    public string ContractNo { get; set; } = "";           // Số hợp đồng mua bán xe
    public string CustomerName { get; set; } = "";         // Tên khách hàng mua xe
    public string? CustomerPhone { get; set; }             // Số điện thoại khách hàng
    public string? CustomerAddress { get; set; }           // Địa chỉ khách hàng
    public bool FlagAccessory { get; set; } = false;       // Lắp thêm gói phụ kiện giao xe
    public string? AccessoryNote { get; set; }             // Danh mục phụ kiện (Dán film cách nhiệt, Trải sàn da 5D, Camera hành trình, Phủ ceramic...)
    public PdiItemStatus Status { get; set; } = PdiItemStatus.Pending; // Trạng thái kiểm tra
    public string? Inspector { get; set; }                 // Kỹ thuật viên phụ trách PDI
    public DateTime? InspectionDate { get; set; }          // Thời điểm tiến hành kiểm tra
    public DateTime? PassedDate { get; set; }              // Thời điểm nghiệm thu ĐẠT chuẩn giao xe
    public string? InspectionNotes { get; set; }           // Ghi chú chi tiết của KTV kiểm tra
    public int? ROId { get; set; }                         // Lệnh kiểm tra / sửa chữa hoàn thiện RO tạo ra cho xe này
    public string? RONo { get; set; }                      // Số lệnh RO

    public PdiRequest PdiRequest { get; set; } = null!;
    public RepairOrder? RO { get; set; }
    public List<PdiChecklistItem> ChecklistItems { get; set; } = [];

    public int TotalChecklistCount => ChecklistItems.Count;
    public int PassedChecklistCount => ChecklistItems.Count(c => c.Status == AuditStatus.Good);
    public int IssueChecklistCount => ChecklistItems.Count(c => c.Status == AuditStatus.Attention || c.Status == AuditStatus.Replace);
    public bool IsReadyForDelivery => Status == PdiItemStatus.Passed;
}

/// <summary>Hạng mục checklist kiểm tra kỹ thuật PDI — chuẩn 25 điểm kiểm tra xuất xưởng xe Hyundai.</summary>
public class PdiChecklistItem : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int PdiRequestItemId { get; set; }
    public string Group { get; set; } = "";                // Nhóm kiểm tra (Khoang động cơ, Ngoại thất & Thân vỏ, Hệ thống điện & Đèn, Nội thất & Tiện nghi, Bàn giao & Phụ kiện)
    public string Code { get; set; } = "";                 // Mã hạng mục (PDI.DCO.DAU, PDI.VO.SON, PDI.DEN.PHA...)
    public string Name { get; set; } = "";                 // Tên tiêu chí kiểm tra
    public AuditStatus Status { get; set; } = AuditStatus.Good; // Kết quả kiểm tra (Tốt/Đạt, Cần chú ý, Cần khắc phục/thay, K/A)
    public string? Note { get; set; }                      // Nhận xét chi tiết của kỹ thuật viên

    public PdiRequestItem PdiRequestItem { get; set; } = null!;
}

/// <summary>Phiếu khiếu nại đơn hàng phụ tùng Nhà Cung Cấp TST / HTC — Ser_OrderComplain trong idn.CarService.</summary>
public class OrderComplain : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string OrderComplainNo { get; set; } = "";             // Số khiếu nại (VD: KN260427-001)
    public string DealerCode { get; set; } = "HYUNDAI-MAIN";      // Mã đại lý khiếu nại
    public string DealerName { get; set; } = "Hyundai Giải Phóng"; // Tên đại lý
    public OrderComplainType ComplainType { get; set; } = OrderComplainType.DamagedInTransit; // Loại khiếu nại
    public int? OrderPartId { get; set; }                         // Đơn đặt hàng liên quan (OrderPart)
    public string? OrderPartNo { get; set; }                      // Số đơn hàng đặt phụ tùng (DocNoSO)
    public int PartId { get; set; }                               // Phụ tùng khiếu nại
    public string PartCode { get; set; } = "";                    // Mã phụ tùng (PartCode / ItemCode)
    public string PartName { get; set; } = "";                    // Tên phụ tùng (VieName)
    public string Unit { get; set; } = "Cái";                     // Đơn vị tính
    public decimal Quantity { get; set; } = 1;                    // Số lượng khiếu nại (bắt buộc > 0)
    public decimal UnitPrice { get; set; }                        // Đơn giá phụ tùng tại thời điểm mua
    public string? VIN { get; set; }                              // Số khung xe liên quan (nếu phụ tùng theo xe)
    public string Description { get; set; } = "";                 // Tình trạng hỏng hóc & nguyên nhân ban đầu (Malfunction)
    public string? RequestOrderNo { get; set; }                   // Số yêu cầu giao hàng / Số vận đơn phiếu DO (DocNoDO)
    public string? TransportUnit { get; set; }                    // Đơn vị vận tải (Viettel Post, Vận tải Thành Công...)
    public DateTime? DeliveryDateTime { get; set; }               // Ngày giờ nhận hàng thực tế
    public string? DeliveryBy { get; set; }                       // Người / tài xế bên vận chuyển bàn giao
    public string? DeliveryLocation { get; set; } = "Kho phụ tùng chính"; // Địa điểm nhận hàng
    public string? ReceiveBy { get; set; }                        // Thủ kho / Người nhận hàng tại đại lý
    public DateTime? AssembleDateTime { get; set; }               // Ngày giờ phát hiện lỗi khi lắp ráp
    public string? AssembleBy { get; set; }                       // Kỹ thuật viên phát hiện lỗi
    public DMSOrderComplainStatus DMSStatus { get; set; } = DMSOrderComplainStatus.Pending; // Trạng thái khiếu nại tại DMS (P / A / F / C)
    public TSTOrderComplainStatus TSTStatus { get; set; } = TSTOrderComplainStatus.Processing; // Trạng thái xử lý của NCC TST (1 / 15 / 21 / 31)
    public ComplainSolution? TSTSolution { get; set; }            // Phương án bồi thường từ NCC
    public string? SolutionNote { get; set; }                     // Ghi chú chi tiết phương án giải quyết từ TST
    public string CreatedBy { get; set; } = "Thủ kho";            // Người tạo hồ sơ khiếu nại
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? SentAt { get; set; }                         // Thời điểm gửi hồ sơ khiếu nại sang NCC
    public DateTime? DecidedAt { get; set; }                      // Thời điểm NCC phê duyệt quyết định
    public DateTime? FinishedAt { get; set; }                     // Thời điểm đóng hoàn tất khiếu nại

    public OrderPart? OrderPart { get; set; }
    public Part Part { get; set; } = null!;
    public List<OrderComplainAttachFile> AttachFiles { get; set; } = [];

    public decimal Amount => Quantity * UnitPrice;
    public int AttachCount => AttachFiles.Count;
    public bool CanSend => DMSStatus == DMSOrderComplainStatus.Pending;
    public bool CanReview => DMSStatus == DMSOrderComplainStatus.Sent;
    public bool IsApproved => TSTStatus == TSTOrderComplainStatus.Approved;
    public bool IsRejected => TSTStatus == TSTOrderComplainStatus.Rejected;
}

/// <summary>Ảnh chứng cứ đính kèm hồ sơ khiếu nại — Ser_OrderComplainAttachFile trong idn.CarService.</summary>
public class OrderComplainAttachFile : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int OrderComplainId { get; set; }
    public string ImageType { get; set; } = "Ngoại quan hư hại";  // Phân loại: Ngoại quan hư hại / Tem nhãn bao bì / Mã dập nổi chi tiết / Biên bản giao vận
    public string FileName { get; set; } = "";                    // Tên file ảnh
    public string FilePath { get; set; } = "";                    // Đường dẫn / URL
    public string? Note { get; set; }                             // Chú thích ảnh
    public DateTime UploadedAt { get; set; } = DateTime.Now;

    public OrderComplain OrderComplain { get; set; } = null!;
}

/// <summary>Hồ sơ Thư viện Kỹ thuật & Cẩm nang xử lý pan bệnh — Ser_Technical_Library trong idn.CarService.</summary>
public class TechnicalLibrary : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string TechnicalLibraryCode { get; set; } = "";        // Mã hồ sơ cẩm nang kỹ thuật (VD: TLIB260427-001)
    public string DealerCode { get; set; } = "HYUNDAI-MAIN";      // Mã đại lý lập hồ sơ (DealerCode)
    public string DealerName { get; set; } = "Hyundai Giải Phóng"; // Tên đại lý
    public string? PlateNo { get; set; }                          // Biển số xe phát hiện pan bệnh
    public string Model { get; set; } = "";                       // Dòng xe (Model: Tucson, Santa Fe, Accent, Creta...)
    public string? Engine { get; set; }                           // Loại động cơ (Engine)
    public string? Gear { get; set; }                             // Loại hộp số (Gear)
    public string? Version { get; set; }                          // Phiên bản xe (Version)
    public TechnicalLibraryReRepairType ReRepairType { get; set; } = TechnicalLibraryReRepairType.Engine; // Phân loại kỹ thuật
    public string ReRepairRemark { get; set; } = "";              // Hiện tượng hư hỏng / Triệu chứng pan bệnh cụ thể (ReRepairRemark)
    public string? ReRepairFeedback { get; set; }                 // Phản ánh khách hàng & Kỹ thuật viên (ReRepairFeedback)
    public string? ExclusionTest { get; set; }                    // Phương pháp kiểm tra loại trừ & Đo đạc thông số thực nghiệm (ExclusionTest)
    public string ReRepairReason { get; set; } = "";              // Nguyên nhân hư hỏng gốc rễ (ReRepairReason)
    public string ReRepairSolution { get; set; } = "";            // Biện pháp khắc phục triệt để đã xử lý thành công (ReRepairSolution)
    public TechnicalLibraryType Type { get; set; } = TechnicalLibraryType.Normal; // Loại cẩm nang (0: Thường, 1: Phản tu)
    public bool IsActive { get; set; } = false;                   // Trạng thái phê duyệt (false: Chờ thẩm định HQ, true: Đã duyệt ban hành áp dụng)
    public int? ROId { get; set; }                                // Lệnh sửa chữa phát hiện pan bệnh (nếu có)
    public string CreatedBy { get; set; } = "Kỹ thuật viên";      // Người tạo hồ sơ
    public DateTime CreatedAt { get; set; } = DateTime.Now;       // Ngày lập hồ sơ
    public DateTime? ApprovedAt { get; set; }                     // Ngày HQ phê duyệt
    public string? ApprovedBy { get; set; }                       // Người phê duyệt ban hành

    public RepairOrder? RO { get; set; }

    public bool IsApproved => IsActive;
    public string StatusText => IsActive ? "Đã duyệt ban hành" : "Chờ thẩm định HQ";
}

/// <summary>Danh mục Công việc Dịch vụ & Giờ công Tiêu chuẩn Flat Rate — Ser_MST_Service trong idn.CarService.</summary>
public class ServiceItem : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";                    // Mã công việc dịch vụ (SerCode, VD: BDD-01, SCC-ENG-02, SCD-PNT-01)
    public string Name { get; set; } = "";                    // Tên công việc dịch vụ (SerName)
    public ServiceROType ROType { get; set; } = ServiceROType.SCC; // Loại dịch vụ (BDD, SCC, SCD, SCS, PDI, SPK)
    public decimal StdManHour { get; set; } = 1.0m;           // Giờ công định mức Flat Rate Hour (StdManHour)
    public decimal Price { get; set; }                        // Đơn giá giờ công / Tiền công niêm yết trước thuế (Price)
    public decimal Cost { get; set; }                         // Chi phí giá vốn giờ công thợ định mức (Cost)
    public decimal VatPercent { get; set; } = 8;              // Thuế suất VAT (%)
    public string? Model { get; set; }                        // Dòng xe áp dụng (VD: Tất cả dòng xe, Accent, Tucson, Santa Fe...)
    public bool FlagWarranty { get; set; } = false;           // Cờ công việc áp dụng chế độ Bảo hành hãng HTC (FlagWarranty)
    public string? Note { get; set; }                         // Hướng dẫn kỹ thuật / Quy trình thao tác
    public bool IsActive { get; set; } = true;                // Cờ hiệu lực hoạt động (IsActive)
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // Ngày tạo

    public List<RepairLine> RepairLines { get; set; } = [];

    public decimal TotalWithVat => Math.Round(Price * (1 + VatPercent / 100m), 2);
    public decimal GrossProfit => Price - Cost;
    public decimal GrossMargin => Price > 0 ? Math.Round((Price - Cost) / Price * 100m, 1) : 0;
}

/// <summary>Danh mục Nhà cung cấp Phụ tùng & Dịch vụ ngoài — Ser_Mst_Supplier trong idn.CarService.</summary>
public class Supplier : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";                     // Mã NCC (SupplierCode)
    public string Name { get; set; } = "";                     // Tên Nhà cung cấp (SupplierName)
    public string? Address { get; set; }                      // Địa chỉ trụ sở / kho
    public string? Phone { get; set; }                        // Số điện thoại bàn / hotline
    public string? Email { get; set; }                        // Email liên hệ đặt hàng
    public string? ContactName { get; set; }                  // Người phụ trách liên hệ
    public string? ContactPhone { get; set; }                 // Di động người liên hệ
    public string? TaxCode { get; set; }                      // Mã số thuế
    public string? BankAccount { get; set; }                  // Số tài khoản ngân hàng thụ hưởng
    public string? BankName { get; set; }                     // Tên ngân hàng & chi nhánh
    public bool IsActive { get; set; } = true;                // Cờ hoạt động
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // Ngày tạo

    public List<SupplierPayment> SupplierPayments { get; set; } = [];
    public List<SupplierDebit> SupplierDebits { get; set; } = [];
    public List<SupplierDebitPayment> SupplierDebitPayments { get; set; } = [];
}

/// <summary>Phiếu xuất trả phụ tùng cho Nhà cung cấp — Ser_SupplierPayment trong idn.CarService.</summary>
public class SupplierPayment : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string SupplierPaymentNo { get; set; } = "";        // Số phiếu xuất trả (VD: SPN-260427-001 hoặc PXNCC-VS058-...)
    public int? SupplierId { get; set; }                      // ID Nhà cung cấp
    public string SupplierName { get; set; } = "";            // Tên Nhà cung cấp
    public string? Address { get; set; }                      // Địa chỉ NCC
    public DateTime PaymentDate { get; set; } = DateTime.Today;// Ngày xuất trả kho
    public SupplierPaymentType PaymentType { get; set; } = SupplierPaymentType.ReturnDefective; // Loại YC xuất
    public SupplierPaymentStatus Status { get; set; } = SupplierPaymentStatus.Pending; // Trạng thái phiếu (P/A/C)
    public int? OrderPartId { get; set; }                     // Đơn đặt hàng liên quan nếu có
    public string? OrderPartNo { get; set; }                  // Số đơn hàng NCC (OrderPartNo)
    public string? TSTRequestNo { get; set; }                 // Số yêu cầu xuất NCC / Mã vụ việc bảo hành đổi trả TST
    public string? Description { get; set; }                  // Diễn giải / Lý do xuất trả
    public string CreatedBy { get; set; } = "Thủ kho";        // Người tạo phiếu
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // Ngày tạo
    public string? ApprovedBy { get; set; }                   // Người duyệt xuất
    public DateTime? ApprovedAt { get; set; }                 // Ngày duyệt xuất

    public Supplier? Supplier { get; set; }
    public OrderPart? OrderPart { get; set; }
    public List<SupplierPaymentDetail> Items { get; set; } = [];

    public int ItemCount => Items.Count;
    public decimal TotalQuantity => Items.Sum(x => x.QtyPay);
    public decimal SubTotal => Items.Sum(x => x.SubTotal);
    public decimal TotalVat => Items.Sum(x => x.VatAmount);
    public decimal TotalAmount => Items.Sum(x => x.Amount);

    public bool CanApprove => Status == SupplierPaymentStatus.Pending;
    public bool CanCancel => Status == SupplierPaymentStatus.Pending;
}

/// <summary>Chi tiết phụ tùng xuất trả Nhà cung cấp — Ser_SupplierPaymentDtl trong idn.CarService.</summary>
public class SupplierPaymentDetail : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int SupplierPaymentId { get; set; }
    public int PartId { get; set; }
    public int? StockInId { get; set; }                       // ID Phiếu nhập kho gốc
    public string? StockInNo { get; set; }                    // Số phiếu nhập kho gốc
    public decimal QtyPay { get; set; }                       // Số lượng trả NCC
    public decimal Price { get; set; }                        // Đơn giá nhập / Giá xuất trả
    public decimal VatPercent { get; set; } = 10;             // Thuế suất VAT (%)
    public decimal QtyInventory { get; set; }                 // Tồn kho tại thời điểm lập phiếu
    public string? LocationCode { get; set; }                 // Mã vị trí kệ kho xuất trả
    public string? Reason { get; set; }                       // Ghi chú / Lý do chi tiết dòng

    public SupplierPayment SupplierPayment { get; set; } = null!;
    public Part Part { get; set; } = null!;

    public decimal SubTotal => Math.Round(QtyPay * Price, 2);
    public decimal VatAmount => Math.Round(SubTotal * (VatPercent / 100m), 2);
    public decimal Amount => SubTotal + VatAmount;
}

/// <summary>Phiếu Yêu cầu xuất kho phụ tùng / vật tư dịch vụ — Ser_Inv_StockOutOrder trong idn.CarService (MH 125).</summary>
public class StockOutOrder : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string OrderNo { get; set; } = "";                     // Số phiếu yêu cầu (VD: SOO-260427-001)
    public DateTime OrderDate { get; set; } = DateTime.Today;     // Ngày yêu cầu xuất kho (StockOutOrderTime)
    public DateTime? RequestDeliveryTime { get; set; }            // Thời gian yêu cầu giao vật tư (RequestDeliveryTime)
    public StockOutOrderPriority Priority { get; set; } = StockOutOrderPriority.Normal; // Độ ưu tiên
    public StockOutOrderStatus Status { get; set; } = StockOutOrderStatus.Pending;     // Trạng thái phiếu (P/A/F/R)
    public int? ROId { get; set; }                                // Lệnh sửa chữa RO liên kết (ROID)
    public int? CustomerId { get; set; }                          // Chủ xe / Khách hàng (CusID)
    public int? CarId { get; set; }                               // Xe đang sửa chữa
    public int? CavityId { get; set; }                            // Khoang sửa chữa nhận vật tư
    public int? StockOutId { get; set; }                          // Phiếu xuất kho sau khi kho cấp phát (StockOutID)
    public string? RequesterName { get; set; }                    // Kỹ thuật viên / CVDV lập yêu cầu (UserCode)
    public string? Description { get; set; }                      // Lý do yêu cầu / Diễn giải kỹ thuật (Description)
    public string CreatedBy { get; set; } = "web";                // Người lập phiếu
    public DateTime CreatedAt { get; set; } = DateTime.Now;       // Thời gian lập
    public string? ApprovedBy { get; set; }                       // Thủ kho / Quản đốc duyệt tiếp nhận
    public DateTime? ApprovedAt { get; set; }                     // Thời gian duyệt
    public string? RejectReason { get; set; }                     // Lý do từ chối nếu bị hủy

    public RepairOrder? RO { get; set; }
    public Customer? Customer { get; set; }
    public Car? Car { get; set; }
    public Cavity? Cavity { get; set; }
    public StockOut? StockOut { get; set; }
    public List<StockOutOrderDetail> Items { get; set; } = [];

    public int ItemCount => Items.Count;
    public decimal TotalRequestQuantity => Items.Sum(x => x.RequestQuantity);
    public decimal TotalIssuedQuantity => Items.Sum(x => x.IssuedQuantity);
    public decimal SubTotal => Items.Sum(x => x.SubTotal);
    public decimal TotalVat => Items.Sum(x => x.VatAmount);
    public decimal TotalAmount => Items.Sum(x => x.Amount);

    public bool CanApprove => Status == StockOutOrderStatus.Pending;
    public bool CanIssue => Status == StockOutOrderStatus.Approved || Status == StockOutOrderStatus.Pending;
    public bool CanReject => Status == StockOutOrderStatus.Pending || Status == StockOutOrderStatus.Approved;
    public bool CanDelete => Status == StockOutOrderStatus.Pending || Status == StockOutOrderStatus.Rejected;
}

/// <summary>Chi tiết phụ tùng yêu cầu xuất kho — Ser_Inv_StockOutOrderDetail trong idn.CarService.</summary>
public class StockOutOrderDetail : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int StockOutOrderId { get; set; }
    public int PartId { get; set; }                               // ID phụ tùng trong kho
    public string PartCode { get; set; } = "";                    // Mã phụ tùng (PartCode)
    public string PartName { get; set; } = "";                    // Tên phụ tùng (VieName)
    public string Unit { get; set; } = "Cái";                     // Đơn vị tính (Unit)
    public decimal RequestQuantity { get; set; } = 1;             // Số lượng yêu cầu xuất (OrderQuantity / SOOQuantity)
    public decimal IssuedQuantity { get; set; } = 0;              // Số lượng đã thực xuất kho (SOQuantity)
    public decimal UnitPrice { get; set; }                        // Đơn giá tham chiếu (Price)
    public decimal VatPercent { get; set; } = 8;                  // VAT (%)
    public string? Note { get; set; }                             // Ghi chú dòng yêu cầu (Description)

    public StockOutOrder StockOutOrder { get; set; } = null!;
    public Part Part { get; set; } = null!;

    public decimal SubTotal => Math.Round(RequestQuantity * UnitPrice, 2);
    public decimal VatAmount => Math.Round(SubTotal * (VatPercent / 100m), 2);
    public decimal Amount => SubTotal + VatAmount;
}

/// <summary>Phiếu theo dõi Phụ tùng nợ khách hàng (Part Out of Stock / Backorder) — Ser_Part_OO trong idn.CarService.</summary>
public class PartOO : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string OONo { get; set; } = "";             // Mã phiếu nợ phụ tùng (VD: OO260427-001)
    public int PartId { get; set; }                    // Phụ tùng nợ (PartID)
    public string PartCode { get; set; } = "";         // Mã phụ tùng (PartCode)
    public string PartName { get; set; } = "";         // Tên phụ tùng (VieName)
    public string OOPlateNo { get; set; } = "";        // Biển số xe nợ phụ tùng (OOPlateNo)
    public string? Model { get; set; }                 // Dòng xe / Loại xe (LoaiXe)
    public decimal SoLuongNo { get; set; } = 1;        // Số lượng nợ khách (SoLuongNo, bắt buộc > 0)
    public decimal SoLuongTra { get; set; } = 0;       // Số lượng đã trả (SoLuongTra, 0 <= SoLuongTra <= SoLuongNo)
    public string? CVDV { get; set; }                  // Cố vấn dịch vụ phụ trách (CVDV)
    public DateTime? NgayDatHang { get; set; }         // Ngày đặt hàng NCC (NgayDatHang)
    public DateTime? NgayVeDuKien { get; set; }        // Ngày dự kiến hàng về kho (NgayVeDuKien)
    public DateTime? NgayHenTra { get; set; }          // Ngày hẹn khách đến lắp/nhận (NgayHenTra)
    public string? GhiChu { get; set; }                // Ghi chú lý do nợ / thỏa thuận với khách (GhiChu)
    public PartOOStatus Status { get; set; } = PartOOStatus.Owed; // Trạng thái phiếu
    public int? ROId { get; set; }                     // Lệnh sửa chữa phát sinh nợ phụ tùng nếu có
    public int? CarId { get; set; }                    // Xe trong hệ thống nếu có
    public int? CustomerId { get; set; }               // Khách hàng trong hệ thống nếu có
    public string CreatedBy { get; set; } = "CVDV";    // Người lập phiếu
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? FinishedAt { get; set; }          // Ngày hoàn tất trả phụ tùng
    public string? ReturnedBy { get; set; }            // Kỹ thuật viên lắp ráp / Người bàn giao phụ tùng

    public Part Part { get; set; } = null!;
    public RepairOrder? RO { get; set; }
    public Car? Car { get; set; }
    public Customer? Customer { get; set; }

    public decimal SoLuongConNo => Math.Max(0, SoLuongNo - SoLuongTra); // Số lượng còn nợ khách (SoLuongConNoKhach)
    public bool IsConNoKhach => SoLuongConNo > 0 && Status != PartOOStatus.Cancelled; // Cờ còn nợ khách
    public bool IsStockAvailable => Part != null && Part.InStock >= SoLuongConNo && IsConNoKhach; // Phụ tùng đã về kho đủ số lượng để trả
    public decimal TotalOwedAmount => Part != null ? SoLuongNo * Part.SalePrice : 0;
    public decimal RemainingAmount => Part != null ? SoLuongConNo * Part.SalePrice : 0;
}

/// <summary>Phiếu ghi nhận công nợ khách hàng dịch vụ — Ser_CusDebit trong idn.CarService.</summary>
public class CusDebit : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string DebitNo { get; set; } = "";                             // Mã ghi nợ (VD: CDB260427-001)
    public int CustomerId { get; set; }                                  // Khách hàng ghi nợ (CusID)
    public int? CarId { get; set; }                                       // Xe dịch vụ (nếu có)
    public int? ROId { get; set; }                                        // Lệnh sửa chữa phát sinh công nợ (ROID)
    public CusDebitType DebitType { get; set; } = CusDebitType.RO;        // Phân loại công nợ (DebitType = '1')
    public CusDebitStatus Status { get; set; } = CusDebitStatus.Active;  // Trạng thái nợ (Active / Cleared / Cancelled)
    public DateTime DebitDate { get; set; } = DateTime.Today;             // Ngày phát sinh nợ (DebitDate)
    public DateTime? DueDate { get; set; }                                // Hạn thanh toán công nợ
    public decimal DebitAmount { get; set; }                              // Số tiền nợ gốc phát sinh (DebitAmount)
    public decimal PaidAmount { get; set; } = 0;                          // Số tiền đã thanh toán (PaymentAmount)
    public string? Description { get; set; }                              // Lý do ghi nợ / Ghi chú (Note)
    public string CreatedBy { get; set; } = "CVDV";                       // Người lập phiếu ghi nợ
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ClearedAt { get; set; }                              // Thời điểm tất toán nợ

    public Customer Customer { get; set; } = null!;
    public Car? Car { get; set; }
    public RepairOrder? RO { get; set; }
    public List<CusDebitPayment> Payments { get; set; } = [];

    public decimal RemainAmount => Math.Max(0, DebitAmount - PaidAmount); // Dư nợ còn lại (Deb)
    public bool IsOverdue => Status == CusDebitStatus.Active && DueDate.HasValue && DueDate.Value.Date < DateTime.Today;
    public bool CanPay => Status == CusDebitStatus.Active && RemainAmount > 0;
}

/// <summary>Phiếu thu nợ khách hàng dịch vụ — Ser_Payment / SerCusDebitPayment trong idn.CarService (PaymentType = '1').</summary>
public class CusDebitPayment : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string PaymentNo { get; set; } = "";                             // Số phiếu thu nợ (VD: CDP260427-001)
    public int CustomerId { get; set; }                                  // Khách hàng nộp tiền (CusID)
    public int? CusDebitId { get; set; }                                  // Khoản nợ cụ thể được cấn trừ (nếu có)
    public DateTime PaymentDate { get; set; } = DateTime.Today;             // Ngày thu tiền (PayDate)
    public decimal PaymentAmount { get; set; }                            // Số tiền thu nợ (PaymentAmount)
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;        // Hình thức thanh toán (Tiền mặt / CK / Thẻ)
    public string PayPersonName { get; set; } = "";                       // Người nộp tiền (PayPersonName)
    public string? PayPersonIdCard { get; set; }                          // CMND/CCCD người nộp (PayPersonIDCardNo)
    public string? PayPersonPhone { get; set; }                           // SĐT người nộp
    public string? TransactionRef { get; set; }                           // Mã giao dịch ngân hàng / POS
    public string? Note { get; set; }                                     // Diễn giải / Lý do thu (Note)
    public string Collector { get; set; } = "Thu ngân";                   // Nhân viên thu nợ
    public string CreatedBy { get; set; } = "web";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Customer Customer { get; set; } = null!;
    public CusDebit? CusDebit { get; set; }
}

/// <summary>DTO tổng hợp công nợ theo khách hàng — Ser_InvReportCusDebitRpt / Ser_CusDebitPayment trong idn.CarService.</summary>
public class CustomerDebitSummaryDto
{
    public int CustomerId { get; set; }
    public string CustomerCode { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string? Phone { get; set; }
    public string? PlateNo { get; set; }
    public string? CarModel { get; set; }
    public decimal TotalDebitAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal RemainingDebit => Math.Max(0, TotalDebitAmount - TotalPaidAmount);
    public int ActiveDebitCount { get; set; }
    public int OverdueDebitCount { get; set; }
    public bool HasDebit => RemainingDebit > 0;
    public DateTime? LastDebitDate { get; set; }
    public DateTime? LastPaymentDate { get; set; }
}

/// <summary>Phiếu ghi nhận công nợ Nhà Cung Cấp — Ser_SupplierDebit trong idn.CarService (DebitType = '3').</summary>
public class SupplierDebit : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string DebitNo { get; set; } = "";                                 // Mã ghi nợ (VD: SDB260427-001)
    public int SupplierId { get; set; }                                       // Nhà cung cấp ghi nợ (SupplierID)
    public int? StockInId { get; set; }                                       // Phiếu nhập kho phát sinh nợ (StockInID)
    public int? OrderPartId { get; set; }                                     // Đơn đặt hàng liên quan nếu có
    public SupplierDebitType DebitType { get; set; } = SupplierDebitType.StockIn; // Phân loại nợ
    public SupplierDebitStatus Status { get; set; } = SupplierDebitStatus.Active; // Trạng thái nợ (Active / Cleared / Cancelled)
    public DateTime DebitDate { get; set; } = DateTime.Today;                 // Ngày phát sinh nợ (DebitDate)
    public DateTime? DueDate { get; set; }                                    // Hạn thanh toán công nợ
    public decimal DebitAmount { get; set; }                                  // Số tiền nợ phát sinh (DebitAmount)
    public decimal PaidAmount { get; set; } = 0;                              // Số tiền đã thanh toán (PaymentAmount)
    public string? Description { get; set; }                                  // Lý do ghi nợ / Ghi chú (Note)
    public string CreatedBy { get; set; } = "Kế toán";                        // Người lập phiếu ghi nợ
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ClearedAt { get; set; }                                  // Thời điểm tất toán nợ

    public Supplier Supplier { get; set; } = null!;
    public StockIn? StockIn { get; set; }
    public OrderPart? OrderPart { get; set; }
    public List<SupplierDebitPayment> Payments { get; set; } = [];

    public decimal RemainAmount => Math.Max(0, DebitAmount - PaidAmount);     // Dư nợ còn lại phải trả NCC (Deb)
    public bool IsOverdue => Status == SupplierDebitStatus.Active && DueDate.HasValue && DueDate.Value.Date < DateTime.Today;
    public bool CanPay => Status == SupplierDebitStatus.Active && RemainAmount > 0;
}

/// <summary>Phiếu chi thanh toán nợ Nhà Cung Cấp — Ser_Payment / Ser_SupplierDebitPayment trong idn.CarService (PaymentType = '3').</summary>
public class SupplierDebitPayment : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string PaymentNo { get; set; } = "";                                 // Số phiếu chi (VD: SDP260427-001 hoặc PC260427-001)
    public int SupplierId { get; set; }                                         // Nhà cung cấp được thanh toán (SupplierID)
    public int? SupplierDebitId { get; set; }                                   // Khoản nợ cụ thể được cấn trừ (nếu có)
    public DateTime PaymentDate { get; set; } = DateTime.Today;                // Ngày chi tiền (PayDate)
    public decimal PaymentAmount { get; set; }                                  // Số tiền chi trả NCC (PaymentAmount)
    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;    // Hình thức thanh toán (CK / Tiền mặt)
    public string PayPersonName { get; set; } = "";                             // Người nhận tiền đại diện NCC (PayPersonName)
    public string? PayPersonIdCard { get; set; }                                // CMND/CCCD người nhận (PayPersonIDCardNo)
    public string? PayPersonPhone { get; set; }                                 // SĐT người nhận
    public string? BankAccount { get; set; }                                    // Tài khoản ngân hàng thụ hưởng
    public string? BankName { get; set; }                                       // Ngân hàng thụ hưởng
    public string? TransactionRef { get; set; }                                 // Mã giao dịch ngân hàng / Ủy nhiệm chi UNC
    public string? Note { get; set; }                                           // Diễn giải / Lý do chi tiền (Note)
    public string Cashier { get; set; } = "Thủ quỹ";                            // Kế toán thanh toán / Thủ quỹ chi
    public string CreatedBy { get; set; } = "web";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Supplier Supplier { get; set; } = null!;
    public SupplierDebit? SupplierDebit { get; set; }
}

/// <summary>DTO tổng hợp công nợ theo Nhà cung cấp — Ser_SupplierDebitPayment / MH 56 trong idn.CarService.</summary>
public class SupplierDebitSummaryDto
{
    public int SupplierId { get; set; }
    public string SupplierCode { get; set; } = "";
    public string SupplierName { get; set; } = "";
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? ContactName { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public decimal TotalDebitAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal RemainingDebit => Math.Max(0, TotalDebitAmount - TotalPaidAmount);
    public int ActiveDebitCount { get; set; }
    public int OverdueDebitCount { get; set; }
    public bool HasDebit => RemainingDebit > 0;
    public DateTime? LastDebitDate { get; set; }
    public DateTime? LastPaymentDate { get; set; }
}

/// <summary>Hồ sơ Lịch sử sửa chữa xe chia sẻ toàn hệ thống đại lý — DealerHistoryShareMng trong idn.CarService.</summary>
public class DealerHistoryRecord : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string RecordNo { get; set; } = "";                        // Mã hồ sơ (VD: DHR260427-001)
    public string DealerCode { get; set; } = "HTC-CG";                // Mã đại lý thực hiện (DealerCode)
    public string DealerName { get; set; } = "Hyundai Cầu Giấy";      // Tên đại lý (DealerName)
    public string PlateNo { get; set; } = "";                         // Biển số xe (PlateNo)
    public string FrameNo { get; set; } = "";                         // Số khung VIN (FrameNo)
    public string? EngineNo { get; set; }                             // Số máy (EngineNo)
    public string TradeMarkName { get; set; } = "Hyundai";            // Hiệu xe (TradeMarkName)
    public string ModelName { get; set; } = "";                       // Model xe (ModelName)
    public string? ColorCode { get; set; }                            // Màu xe (ColorCode)
    public int ProductYear { get; set; }                              // Năm sản xuất (ProductYear)
    public string CusName { get; set; } = "";                         // Tên khách hàng / chủ xe (CusName)
    public string? CusPhone { get; set; }                             // SĐT liên hệ (Mobile/Tel)
    public string? CusAddress { get; set; }                           // Địa chỉ khách hàng
    public string RONo { get; set; } = "";                            // Số lệnh sửa chữa (NormalizedRONo)
    public int? ROId { get; set; }                                    // ID lệnh RO tại đại lý hiện tại (nếu có)
    public DateTime CheckInDate { get; set; } = DateTime.Now;         // Ngày vào xưởng (CheckInDate)
    public DateTime? ActualDeliveryDate { get; set; }                 // Ngày giao xe thực tế (ActualDeliveryDate)
    public int Odometer { get; set; }                                 // Số Km tại thời điểm vào xưởng (Km)
    public string ServiceAdvisor { get; set; } = "CVDV";              // Cố vấn dịch vụ phụ trách (NormalizedCreator)
    public string? Technician { get; set; }                           // Kỹ thuật viên chính
    public string? CustomerRequest { get; set; }                      // Yêu cầu của khách hàng (CusRequest)
    public string? CarStatus { get; set; }                            // Tình trạng xe khi tiếp nhận
    public string? RepairResult { get; set; }                         // Kết quả sửa chữa
    public decimal TotalLaborAmount { get; set; }                     // Tiền công lao động
    public decimal TotalPartAmount { get; set; }                      // Tiền phụ tùng
    public decimal TotalAmount { get; set; }                          // Tổng chi phí quyết toán
    public bool FlagClaim { get; set; } = false;                      // Cờ hồ sơ bảo hành / khiếu nại (FlagClaim)
    public string? ClaimNo { get; set; }                              // Mã số khiếu nại / bảo hành nếu có (ClaimNo)
    public string? ClaimStatus { get; set; }                          // Trạng thái xử lý bảo hành (ACCE/REJ...)
    public string CreatedBy { get; set; } = "system";                 // Người lập / đồng bộ
    public DateTime CreatedAt { get; set; } = DateTime.Now;           // Thời điểm tạo

    public List<DealerHistoryItem> Items { get; set; } = [];

    public int LaborCount => Items.Count(i => i.ItemType == LineType.Labor);
    public int PartCount => Items.Count(i => i.ItemType == LineType.Part);
}

/// <summary>Chi tiết hạng mục công việc / phụ tùng trong lịch sử sửa chữa — DealerHistoryItem (Ser_ROServiceItems / Ser_ROPartItems).</summary>
public class DealerHistoryItem : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int DealerHistoryRecordId { get; set; }
    public LineType ItemType { get; set; } = LineType.Labor;          // Loại: Công lao động / Phụ tùng
    public string Code { get; set; } = "";                            // Mã công việc (SerCode) hoặc Mã phụ tùng (PartCode)
    public string Name { get; set; } = "";                            // Tên công việc (SerName) hoặc Tên phụ tùng (VieName)
    public string Unit { get; set; } = "Giờ";                         // ĐVT (Unit)
    public decimal Quantity { get; set; } = 1;                        // Số lượng / Giờ công
    public decimal UnitPrice { get; set; }                            // Đơn giá (Price)
    public decimal Amount { get; set; }                               // Thành tiền
    public ExpenseType ExpenseType { get; set; } = ExpenseType.Customer; // Đối tượng thanh toán (Khách / Bảo hành / Bảo hiểm)
    public string? Technician { get; set; }                           // Thợ phụ trách (AssignmentLabor)
    public string? Result { get; set; }                               // Kết quả thực hiện (KetQua)
    public string? Remark { get; set; }                               // Ghi chú / Nguyên nhân pan bệnh

    public DealerHistoryRecord DealerHistoryRecord { get; set; } = null!;
}

/// <summary>DTO Tổng hợp hồ sơ lịch sử dịch vụ xe toàn hệ thống — VehicleHistorySummaryDto.</summary>
public class VehicleHistorySummaryDto
{
    public string PlateNo { get; set; } = "";
    public string FrameNo { get; set; } = "";
    public string? EngineNo { get; set; }
    public string TradeMarkName { get; set; } = "Hyundai";
    public string ModelName { get; set; } = "";
    public string? ColorCode { get; set; }
    public int ProductYear { get; set; }
    public string CusName { get; set; } = "";
    public string? CusPhone { get; set; }
    public string? CusAddress { get; set; }
    public int CurrentKm { get; set; }
    public int TotalVisits { get; set; }
    public decimal TotalSpent { get; set; }
    public int TotalClaims { get; set; }
    public DateTime? FirstVisitDate { get; set; }
    public DateTime? LastVisitDate { get; set; }
    public string? LastDealerName { get; set; }
    public string? LastServiceAdvisor { get; set; }
    public List<DealerHistoryRecord> Records { get; set; } = [];
}





