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

/// <summary>Trạng thái Chăm sóc khách hàng sau dịch vụ 72h — theo Ser_CustomerCare72h idn.CarService.</summary>
public enum CustomerCare72hStatus
{
    Pending = 0,            // PEND  — Chưa liên hệ / Chờ khảo sát kỹ thuật 72h
    ContactedSatisfied = 1, // CIFB  — Đã liên hệ - Hài lòng (Xe chạy tốt, kỹ thuật ổn định)
    NeedFeedback = 2,       // CINFB — Đã liên hệ - Cần phản hồi (Xe có sự cố kỹ thuật / Pan tái phát Re-Repair)
    Rejected = 3            // REJ   — Không liên hệ được / Khách bận từ chối tiếp chuyện
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

/// <summary>Phân loại công nợ Hãng bảo hiểm — theo Ser_InsuranceDebit DebitType trong idn.CarService (DebitType = '2').</summary>
public enum InsuranceDebitType
{
    RO = 1,                 // 1: Nợ bồi thường theo Lệnh sửa chữa xe (Repair Order)
    Claim = 2,              // 2: Nợ bồi thường theo Hồ sơ duyệt bảo hiểm (InsuranceClaim)
    DirectAdjustment = 3    // 3: Ghi nợ điều chỉnh bổ sung / Giám định phát sinh
}

/// <summary>Trạng thái công nợ Hãng bảo hiểm — theo Ser_InsuranceDebit trong idn.CarService.</summary>
public enum InsuranceDebitStatus
{
    Active = 1,    // 1: Còn nợ (Active / Unpaid / Partially Paid)
    Cleared = 2,   // 2: Đã tất toán đủ (Cleared / Fully Paid)
    Cancelled = 3  // 3: Đã hủy khoản nợ (Cancelled)
}

/// <summary>Trạng thái Phiếu thu tiền bảo hiểm bồi thường — theo Ser_Payment / Ser_InsuranceDebitPayment.</summary>
public enum InsuranceDebitPaymentStatus
{
    Confirmed = 1,  // 1: Đã xác nhận thu tiền / Giấy báo có
    Cancelled = 2   // 2: Đã hủy phiếu thu
}

public class Customer : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? Gender { get; set; }
    public int? CustomerGroupId { get; set; }
    public CustomerGroup? CustomerGroup { get; set; }
    public List<Car> Cars { get; set; } = [];
    public List<Quote> Quotes { get; set; } = [];
    public List<CusDebit> CusDebits { get; set; } = [];
    public List<CusDebitPayment> CusDebitPayments { get; set; } = [];
    public List<CustomerGroupMember> GroupMemberships { get; set; } = [];
    public List<CustomerCareBirthday> BirthdayCares { get; set; } = [];
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
    public List<CustomerGroupMember> GroupMemberships { get; set; } = [];
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
    public string? ErrorCodePN { get; set; }      // Mã lỗi phàn nàn của khách hàng (PN)
    public string? ErrorCodeCD { get; set; }      // Mã chuẩn đoán kỹ thuật viên (CD / DTC)
    public string? DiagnosticResult { get; set; } // Kết quả chuẩn đoán kỹ thuật xưởng
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
    public int? CustomerGroupId { get; set; }
    public CustomerGroup? CustomerGroup { get; set; }
    public decimal CustomerGroupDiscountAmount { get; set; } = 0;
    public decimal BirthdayDiscountAmount { get; set; } = 0;
    public int? BirthdayCareId { get; set; }
    public string? BirthdayVoucherCode { get; set; }
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
    public List<CustomerCare72h> CustomerCare72hs { get; set; } = [];
    public bool IsReRepair { get; set; } = false;         // Cờ phản tu / sửa chữa lại do khách phản ánh lỗi 72h
    public int? ReRepairParentROId { get; set; }          // Lệnh sửa chữa gốc bị phản tu
    public List<PdiRequestItem> PdiRequestItems { get; set; } = [];
    public List<TechnicalLibrary> TechnicalLibraries { get; set; } = [];
    public List<StockOutOrder> StockOutOrders { get; set; } = [];
    public List<PartOO> PartOOs { get; set; } = [];
    public List<CusDebit> CusDebits { get; set; } = [];
    public List<InsuranceDebit> InsuranceDebits { get; set; } = [];
    public List<PartPriceRequest> PartPriceRequests { get; set; } = [];
    public List<CustomerCareBirthday> CustomerCareBirthdays { get; set; } = [];
    public int? MaintenanceSettingId { get; set; }
    public MaintenanceSetting? MaintenanceSetting { get; set; }
    public string? MaintenanceMilestone { get; set; }     // Tên mốc chu kỳ bảo dưỡng (VD: BD-20K (Cấp 3 - 20.000 km))

    // Ngày hẹn giao xe dự kiến cho khách (Ser_RO.PlanedDeliveryDate) — cam kết thời điểm trả xe
    public DateTime? PlanedDeliveryDate { get; set; }
    public string? DeliveryDateRemark { get; set; }       // Lý do điều chỉnh ngày hẹn giao xe (Remark)
    public List<RoDeliveryDateHistory> DeliveryDateHistories { get; set; } = [];
    public List<RoHistory> Histories { get; set; } = [];

    // Nhắc bảo dưỡng định kỳ (Ser_RO.ReminderMaintanceDate / ReminderMaintanceKm / WorkDoneSoon / MemberNo)
    // Cập nhật qua nghiệp vụ Ser_RO_Update_Maintance_DL — chỉ cho phép khi RO chưa Paid/Finished.
    public DateTime? ReminderMaintanceDate { get; set; }  // Ngày khuyến nghị bảo dưỡng lần kế tiếp
    public int? ReminderMaintanceKm { get; set; }         // Mốc km khuyến nghị bảo dưỡng lần kế tiếp
    public bool WorkDoneSoon { get; set; } = false;       // Cờ khách sắp đến kỳ bảo dưỡng (WorkDoneSoon)
    public string? MemberNo { get; set; }                 // Số thẻ thành viên / hội viên (MemberNo)

    public decimal Total => Math.Max(0, Lines.Sum(l => l.Amount) - CampaignDiscountAmount - CustomerGroupDiscountAmount - BirthdayDiscountAmount);
    public decimal GrossTotal => Lines.Sum(l => l.Amount);
    public decimal LaborTotal => Lines.Where(l => l.Type == LineType.Labor).Sum(l => l.Amount);
    public decimal PartTotal => Lines.Where(l => l.Type == LineType.Part).Sum(l => l.Amount);
    public decimal CustomerTotal => Math.Max(0, Lines.Where(l => l.ExpenseType == ExpenseType.Customer).Sum(l => l.Amount) - CampaignDiscountAmount - CustomerGroupDiscountAmount - BirthdayDiscountAmount);
    public decimal WarrantyTotal => Lines.Where(l => l.ExpenseType == ExpenseType.Warranty).Sum(l => l.Amount);
    public decimal InsuranceTotal => Lines.Where(l => l.ExpenseType == ExpenseType.Insurance).Sum(l => l.Amount);
    public decimal PaidAmount => Payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.PaymentAmount);
    public decimal RemainingBalance => Math.Max(0, CustomerTotal - PaidAmount);
}

/// <summary>Lịch sử điều chỉnh Ngày hẹn giao xe dự kiến — Ser_Ro_PlanedDeliveryDate_His trong idn.CarService.
/// Mỗi lần đổi ngày hẹn giao xe, dòng cũ bị đánh dấu FlagCurrent = false và ghi thêm 1 dòng mới FlagCurrent = true.</summary>
public class RoDeliveryDateHistory : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int ROId { get; set; }                          // Lệnh sửa chữa (ROID)
    public DateTime PlanedDeliveryDate { get; set; }       // Ngày hẹn giao xe dự kiến tại thời điểm ghi
    public string? Remark { get; set; }                    // Lý do điều chỉnh (Remark)
    public bool FlagCurrent { get; set; } = true;          // true: giá trị đang hiệu lực; false: giá trị cũ
    public string CreatedBy { get; set; } = "web";         // Người ghi (CreatedBy)
    public DateTime CreatedAt { get; set; } = DateTime.Now; // Thời điểm ghi (CreatedDate)

    public RepairOrder RO { get; set; } = null!;
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
    public List<PartPriceRequestLine> PartPriceRequestLines { get; set; } = [];

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
    public int? WarrantyWorkId { get; set; }        // Liên kết định mức công việc bảo hành hãng (Ser_MST_ROWarrantyWork)
    public WarrantyWork? WarrantyWork { get; set; }
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
    public int? CustomerCareBirthdayId { get; set; }
    public CustomerCareBirthday? CustomerCareBirthday { get; set; }
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

/// <summary>Phiếu chăm sóc khách hàng 72h sau dịch vụ & Kiểm soát pan tái phát Re-Repair — Ser_CustomerCare72h trong idn.CarService.</summary>
public class CustomerCare72h : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Care72No { get; set; } = "";           // Số phiếu CSKH 72h (VD: CC72-260427-001)
    public int ROId { get; set; }                        // Lệnh sửa chữa gốc cần khảo sát 72h
    public int CarId { get; set; }                       // Xe làm dịch vụ
    public int CustomerId { get; set; }                  // Khách hàng / Chủ xe
    public CustomerCare72hStatus Status { get; set; } = CustomerCare72hStatus.Pending;

    public DateTime ROFinishedDate { get; set; } = DateTime.Today; // Thời điểm xe xuất xưởng
    public DateTime ScheduledDate { get; set; } = DateTime.Today.AddDays(3); // Hẹn gọi sau 72h (3 ngày)
    public DateTime? ContactedDate { get; set; }         // Thời điểm liên hệ thực tế
    public string? ContactedBy { get; set; }             // Nhân viên CSKH thực hiện cuộc gọi

    // Bộ 6 câu hỏi khảo sát kỹ thuật tiêu chuẩn Hyundai CSI 72h (Ser_CustomerCare72h idn.CarService)
    public bool? ServiceExplained { get; set; } = true;  // FyourCSSH: CVDV có giải thích chi tiết nội dung và chi phí không?
    public bool? BasicNeedsMet { get; set; } = true;     // WFBasicNeeds: Xưởng có giải quyết triệt để yêu cầu tiếp nhận ban đầu không?
    public bool HasTechnicalProblem { get; set; } = false; // YourCarProblem: Sau 72h xe có phát sinh lỗi/tiếng kêu bất thường không?
    public string? ProblemDetails { get; set; }          // Mô tả sự cố kỹ thuật hoặc pan bệnh tái phát
    public bool? FixedRightFirstTime { get; set; } = true; // YourRIWN: Sửa chữa dứt điểm ngay lần đầu tiên (FIRFT)?
    public int? SatisfactionRating { get; set; } = 5;    // YourSatisfyQSv: Mức độ hài lòng kỹ thuật (1-5 sao)
    public string? CustomerFeedback { get; set; }        // YourHopeOfOur: Góp ý / Kỳ vọng của khách hàng

    // Nghiệp vụ Xử lý Pan tái phát / Phản tu (Re-Repair Handling)
    public bool IsReRepairAlert { get; set; } = false;   // Báo động phản tu: xe cần kiểm tra sửa lại khẩn cấp
    public string? ReRepairAction { get; set; }          // Biện pháp xử lý của xưởng (Hẹn tái khám, Bảo hành dịch vụ 0đ)
    public int? ReRepairROId { get; set; }               // Lệnh sửa chữa phản tu (Re-Repair RO) được sinh ra
    public int? ReRepairAppointmentId { get; set; }      // Cuộc hẹn đón tiếp xe quay lại xử lý
    public string? InternalNote { get; set; }            // Ghi chú nội bộ xử lý phản hồi
    public string CreatedBy { get; set; } = "system";    // Tự động tạo khi RO hoàn tất hoặc tạo thủ công
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public RepairOrder RO { get; set; } = null!;
    public Car Car { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public RepairOrder? ReRepairRO { get; set; }
    public Appointment? ReRepairAppointment { get; set; }
}

public class CustomerCare72hSummaryDto
{
    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int SatisfiedCount { get; set; }
    public int NeedFeedbackCount { get; set; }
    public int RejectedCount { get; set; }
    public decimal FirftRate { get; set; }             // Tỷ lệ Sửa đúng lần đầu (FIRFT %)
    public decimal AverageSatisfaction { get; set; }    // Điểm đánh giá CSI 72h trung bình (thang 5)
    public int ReRepairAlertCount { get; set; }        // Số xe bị sự cố cần đón tiếp phản tu
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
    public List<InsuranceDebit> Debits { get; set; } = [];
    public List<InsuranceDebitPayment> Payments { get; set; } = [];
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
    public List<InsuranceDebit> Debits { get; set; } = [];

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

/// <summary>Phiếu ghi nhận công nợ Hãng Bảo Hiểm — Ser_InsuranceDebit trong idn.CarService (DebitType = '2').</summary>
public class InsuranceDebit : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string DebitNo { get; set; } = "";                                 // Mã ghi nợ (VD: IDB260427-001)
    public int InsuranceCompanyId { get; set; }                               // Hãng bảo hiểm ghi nợ (InsNo/InsuranceCompanyId)
    public string InsNo { get; set; } = "";                                   // Mã hãng bảo hiểm (InsNo)
    public string InsName { get; set; } = "";                                 // Tên hãng bảo hiểm (InsName)
    public int? InsuranceContractId { get; set; }                             // Hợp đồng bảo hiểm áp dụng nếu có
    public int? ROId { get; set; }                                            // Lệnh sửa chữa phát sinh công nợ (ROID)
    public string? RONo { get; set; }                                         // Số Lệnh sửa chữa (RONo)
    public string? PlateNo { get; set; }                                      // Biển số xe được bảo hiểm (PlateNo)
    public string? CarModel { get; set; }                                     // Model xe
    public string? CustomerName { get; set; }                                 // Tên chủ xe / Người thụ hưởng (CusName)
    public int? InsuranceClaimId { get; set; }                                // Hồ sơ bồi thường bảo hiểm liên kết nếu có
    public string? ClaimNo { get; set; }                                      // Số hồ sơ bồi thường (ClaimNo)
    public string? PolicyNo { get; set; }                                     // Số đơn bảo hiểm / Số GCNBH (PolicyNo)
    public InsuranceDebitType DebitType { get; set; } = InsuranceDebitType.RO; // Phân loại nợ (1: RO, 2: Claim, 3: DirectAdjustment)
    public InsuranceDebitStatus Status { get; set; } = InsuranceDebitStatus.Active; // Trạng thái nợ
    public DateTime DebitDate { get; set; } = DateTime.Today;                 // Ngày phát sinh công nợ (DebitDate)
    public DateTime? DueDate { get; set; }                                    // Hạn thanh toán bồi thường
    public decimal DebitAmount { get; set; }                                  // Số tiền nợ bồi thường gốc (DebitAmount)
    public decimal PaidAmount { get; set; } = 0;                              // Số tiền đã thanh toán (PaymentAmount)
    public string? Description { get; set; }                                  // Lý do ghi nợ / Nội dung sự vụ (Note)
    public string CreatedBy { get; set; } = "CVDV";                           // Người lập phiếu ghi nợ
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ClearedAt { get; set; }                                  // Thời điểm tất toán nợ
    public string? CancelledReason { get; set; }                              // Lý do hủy / Từ chối bồi thường

    public InsuranceCompany InsuranceCompany { get; set; } = null!;
    public InsuranceContract? InsuranceContract { get; set; }
    public RepairOrder? RO { get; set; }
    public InsuranceClaim? InsuranceClaim { get; set; }
    public List<InsuranceDebitPayment> Payments { get; set; } = [];

    public decimal RemainAmount => Math.Max(0, DebitAmount - PaidAmount);     // Dư nợ còn lại phải thu từ bảo hiểm (Deb)
    public bool IsOverdue => Status == InsuranceDebitStatus.Active && DueDate.HasValue && DueDate.Value.Date < DateTime.Today;
    public bool CanPay => Status == InsuranceDebitStatus.Active && RemainAmount > 0;
}

/// <summary>Phiếu thu tiền thanh toán bồi thường Bảo Hiểm — Ser_Payment / Ser_InsuranceDebitPayment trong idn.CarService (PaymentType = '2').</summary>
public class InsuranceDebitPayment : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string PaymentNo { get; set; } = "";                                 // Số phiếu thu (VD: IPM260427-001 hoặc PT-BH-001)
    public int InsuranceCompanyId { get; set; }                                 // Hãng bảo hiểm thanh toán (InsuranceCompanyId)
    public string InsNo { get; set; } = "";                                     // Mã hãng (InsNo)
    public string InsName { get; set; } = "";                                   // Tên hãng bảo hiểm
    public int? InsuranceDebitId { get; set; }                                  // Khoản nợ bảo hiểm cụ thể được cấn trừ (nếu có)
    public DateTime PaymentDate { get; set; } = DateTime.Today;                 // Ngày thu tiền bồi thường (PayDate)
    public decimal PaymentAmount { get; set; }                                  // Số tiền thu bồi thường (PaymentAmount)
    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;    // Hình thức thanh toán (chủ yếu là Chuyển khoản ngân hàng)
    public string PayPersonName { get; set; } = "";                             // Đại diện nộp tiền / Giám định viên / Kế toán hãng (PayPersonName)
    public string? PayPersonIdCard { get; set; }                                // CMND/CCCD người nộp (PayPersonIDCardNo)
    public string? PayPersonPhone { get; set; }                                 // SĐT liên hệ
    public string? BankAccount { get; set; }                                    // Tài khoản ngân hàng nhận tiền
    public string? BankName { get; set; }                                       // Ngân hàng nhận tiền
    public string? TransactionRef { get; set; }                                 // Mã giao dịch ngân hàng / Giấy báo có
    public string? Note { get; set; }                                           // Lý do nộp tiền bồi thường (Note)
    public string Cashier { get; set; } = "Thu ngân";                           // Nhân viên thu tiền
    public InsuranceDebitPaymentStatus Status { get; set; } = InsuranceDebitPaymentStatus.Confirmed; // Trạng thái phiếu thu
    public string CreatedBy { get; set; } = "web";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public InsuranceCompany InsuranceCompany { get; set; } = null!;
    public InsuranceDebit? InsuranceDebit { get; set; }
}

/// <summary>DTO Tổng hợp công nợ theo Hãng Bảo Hiểm — Ser_InsuranceDebitPayment / SerInvReportInsuranceDebitRpt.</summary>
public class InsuranceCompanyDebitSummaryDto
{
    public int InsuranceCompanyId { get; set; }
    public string InsNo { get; set; } = "";
    public string InsName { get; set; } = "";
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxCode { get; set; }
    public string? Hotline { get; set; }
    public decimal TotalDebitAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal RemainingDebit => Math.Max(0, TotalDebitAmount - TotalPaidAmount);
    public int ActiveDebitCount { get; set; }
    public int OverdueDebitCount { get; set; }
    public bool HasDebit => RemainingDebit > 0;
    public DateTime? LastDebitDate { get; set; }
    public DateTime? LastPaymentDate { get; set; }
}

/// <summary>Khách đoàn / Nhóm khách hàng doanh nghiệp & đội xe — Ser_CustomerGroup trong idn.CarService (MNU_QT_DL_QUANLYKHACHDOAN).</summary>
public class CustomerGroup : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string GroupNo { get; set; } = "";             // Mã khách đoàn (GroupNo)
    public string GroupName { get; set; } = "";           // Tên khách đoàn (GroupName)
    public string? TaxCode { get; set; }                 // Mã số thuế (TaxCode)
    public string? Address { get; set; }                 // Địa chỉ (Address)
    public string? Telephone { get; set; }               // Điện thoại bàn / Hotline (TelePhone)
    public string? Fax { get; set; }                     // Fax
    public string? Email { get; set; }                   // Email
    public string? ContactPerson { get; set; }          // Người đại diện / Phụ trách đội xe
    public string? ContactPhone { get; set; }           // SĐT người đại diện
    public string? Description { get; set; }             // Mô tả / Điều khoản hợp đồng (Description)
    public bool IsActive { get; set; } = true;           // Trạng thái hiệu lực (IsActive)

    // Chính sách ưu đãi đoàn & Hạn mức tín dụng / công nợ
    public decimal DiscountPercentLabor { get; set; } = 0;  // % Chiết khấu tiền công (0 - 100%)
    public decimal DiscountPercentPart { get; set; } = 0;   // % Chiết khấu phụ tùng (0 - 100%)
    public decimal CreditLimit { get; set; } = 0;           // Hạn mức tín dụng / công nợ tối đa (VNĐ)
    public int PaymentTermDays { get; set; } = 30;          // Thời hạn thanh toán (ngày)
    public string? ContractNo { get; set; }                 // Số hợp đồng dịch vụ đội xe
    public DateTime? ContractStartDate { get; set; }        // Ngày bắt đầu hợp đồng
    public DateTime? ContractEndDate { get; set; }          // Ngày kết thúc hợp đồng

    public string CreatedBy { get; set; } = "Hệ thống";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    public List<CustomerGroupMember> Members { get; set; } = [];
    public List<RepairOrder> RepairOrders { get; set; } = [];
    public List<Customer> Customers { get; set; } = [];
}

/// <summary>Thành viên xe / tài xế thuộc Khách đoàn — Ser_CustomerGroupCustomer trong idn.CarService.</summary>
public class CustomerGroupMember : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int CustomerGroupId { get; set; }
    public CustomerGroup CustomerGroup { get; set; } = null!;

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int CarId { get; set; }
    public Car Car { get; set; } = null!;

    public string PlateNo { get; set; } = "";             // Biển số xe thuộc đoàn (PlateNo)
    public string? DriverName { get; set; }              // Tên tài xế phụ trách xe (CusName)
    public string? DriverPhone { get; set; }             // Điện thoại tài xế (Mobile)
    public string? Note { get; set; }                    // Ghi chú xe (Description)
    public DateTime JoinedDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
}

/// <summary>DTO Tổng hợp chỉ số kinh doanh & công nợ theo Khách đoàn (Fleet Group Summary).</summary>
public class CustomerGroupSummaryDto
{
    public int Id { get; set; }
    public string GroupNo { get; set; } = "";
    public string GroupName { get; set; } = "";
    public string? TaxCode { get; set; }
    public string? Address { get; set; }
    public string? Telephone { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public decimal DiscountPercentLabor { get; set; }
    public decimal DiscountPercentPart { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermDays { get; set; }
    public string? ContractNo { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public bool IsActive { get; set; }
    public int MemberCount { get; set; }
    public int ROCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalDiscountGiven { get; set; }
    public decimal CurrentDebt { get; set; }
    public bool IsCreditExceeded => CreditLimit > 0 && CurrentDebt > CreditLimit;
}

/// <summary>Trạng thái đề nghị báo giá tại Đại lý (DMS) — DMSReqPartPriceStatus trong Ser_Inv_Quote/Req_PartPrice (P, A, F, C).</summary>
public enum DMSReqPartPriceStatus
{
    Draft = 0,       // Mới tạo (DRAFT / PEND)
    Sent = 1,        // Đã gửi NCC TST/HTC (SENT / A)
    Responded = 2,   // NCC đã phản hồi báo giá (RESP)
    Approved = 3,    // Đại lý chấp thuận đơn giá (APPR / FNS)
    Cancelled = 4    // Đã hủy đề nghị (CANC)
}

/// <summary>Trạng thái xử lý tại NCC TST/HTC — TSTReqPartPriceStatus trong idn.CarService (1, 15, 31, 21).</summary>
public enum TSTReqPartPriceStatus
{
    Pending = 0,     // Chờ tiếp nhận (1)
    Processing = 1,  // Đang thẩm định đơn giá (15)
    Priced = 2,      // Đã cấp báo giá (31)
    Rejected = 3     // Từ chối cung cấp giá (21)
}

/// <summary>Hình thức cung ứng phụ tùng — Mst_DeliveryForm / Req_PartPrice trong idn.CarService.</summary>
public enum PartPriceDeliveryForm
{
    VOR = 0,         // Khẩn cấp xe nằm xưởng dừng lăn bánh (Vehicle Off Road)
    Regular = 1,     // Đặt hàng định kỳ bổ sung kho (ĐHĐK)
    Air = 2,         // Đường hàng không hỏa tốc (Bay)
    Sea = 3          // Đường biển container (Tàu)
}

/// <summary>Trạng thái từng dòng phụ tùng đề nghị giá — Req_PartPriceDtl trong idn.CarService.</summary>
public enum ReqPartPriceLineStatus
{
    Pending = 0,     // Chờ NCC cấp giá
    Priced = 1,      // Đã có giá
    Rejected = 2     // Không cung cấp / Từ chối
}

/// <summary>Phiếu Đề nghị cung cấp giá phụ tùng Nhà Cung Cấp TST / HTC — Req_PartPrice trong idn.CarService (MNU_QT_DL_DENHICUNGCAPGIA).</summary>
public class PartPriceRequest : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string ReqPartPriceNo { get; set; } = "";             // Số đề nghị giá (VD: RPP260427-001)
    public string DealerCode { get; set; } = "HTC-CG";          // Mã đại lý lập đề nghị (DealerCode)
    public string DealerName { get; set; } = "Hyundai Cầu Giấy"; // Tên đại lý (DealerName)
    public string Description { get; set; } = "";                // Nội dung / Lý do đề nghị giá (Description)
    public string? TSTReqPartPriceID { get; set; }               // Mã phiếu NCC tiếp nhận phản hồi (TSTReqPartPriceID)
    public DateTime? TSTSentDate { get; set; }                   // Thời điểm gửi hồ sơ sang NCC (TSTSentDate)
    public DMSReqPartPriceStatus DMSStatus { get; set; } = DMSReqPartPriceStatus.Draft; // Trạng thái DMS (DMSReqPartPriceStatus)
    public TSTReqPartPriceStatus TSTStatus { get; set; } = TSTReqPartPriceStatus.Pending; // Trạng thái NCC (TSTReqPartPriceStatus)
    public bool FlagIsCheck { get; set; } = false;               // Cờ ưu tiên kiểm tra nhanh VOR (FlagIsCheck)
    public bool IsUpdatePrice { get; set; } = false;             // Cờ đã cập nhật đơn giá vào danh mục Part (IsUpdatePrice)
    public DateTime? UpdatedPriceAt { get; set; }                // Thời điểm cập nhật giá vào danh mục Part
    public DateTime? EffectiveDate { get; set; }                 // Ngày bắt đầu hiệu lực của đơn giá
    public DateTime? EstimatedResponseDate { get; set; }         // Ngày dự kiến có đơn giá từ NCC
    public string CreatedBy { get; set; } = "Thủ kho";           // Người lập phiếu đề nghị
    public DateTime CreatedAt { get; set; } = DateTime.Now;      // Ngày lập
    public string? ApprovedBy { get; set; }                      // Người phê duyệt đơn giá
    public DateTime? ApprovedAt { get; set; }                    // Thời điểm duyệt
    public int? ROId { get; set; }                               // Lệnh sửa chữa liên quan nếu có (ROId)
    public string? VIN { get; set; }                             // Số khung VIN xe liên quan
    public string? CarModel { get; set; }                        // Dòng xe tương thích (Model)

    public RepairOrder? RO { get; set; }
    public List<PartPriceRequestLine> Items { get; set; } = [];

    public int TotalItems => Items.Count;
    public decimal TotalPricedAmount => Items.Sum(i => i.Amount);
    public bool CanSend => DMSStatus == DMSReqPartPriceStatus.Draft;
    public bool CanSimulateResponse => DMSStatus == DMSReqPartPriceStatus.Sent;
    public bool CanApprove => DMSStatus == DMSReqPartPriceStatus.Responded;
    public bool CanCancel => DMSStatus == DMSReqPartPriceStatus.Draft || DMSStatus == DMSReqPartPriceStatus.Sent;
    public bool CanCreateOrderPart => DMSStatus == DMSReqPartPriceStatus.Approved;
    public bool IsApproved => DMSStatus == DMSReqPartPriceStatus.Approved;
}

/// <summary>Chi tiết dòng đề nghị cung cấp giá phụ tùng — Req_PartPriceDtl trong idn.CarService.</summary>
public class PartPriceRequestLine : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int PartPriceRequestId { get; set; }
    public int? PartId { get; set; }                             // Phụ tùng trong kho nếu đã có mã PartId
    public string DMSPartCode { get; set; } = "";                // Mã phụ tùng yêu cầu (DMSPartCode)
    public string VieName { get; set; } = "";                    // Tên tiếng Việt phụ tùng (VieName)
    public string? VINCode { get; set; }                         // Số khung VIN xe tra cứu sơ đồ Microcat (VINCode)
    public PartPriceDeliveryForm DeliveryForm { get; set; } = PartPriceDeliveryForm.VOR; // Hình thức giao hàng (DeliveryFormCode)
    public decimal Quantity { get; set; } = 1;                   // Số lượng yêu cầu
    public string Unit { get; set; } = "Cái";                    // Đơn vị tính (DVT)
    public string? Remark { get; set; }                          // Ghi chú kỹ thuật, vị trí lắp đặt (Remark)
    public string? TSTPartCode { get; set; }                     // Mã phụ tùng do TST cấp chuẩn hóa (TSTPartCode)
    public decimal TSTPrice { get; set; } = 0;                   // Đơn giá NCC báo cấp (TSTPrice)
    public DateTime? DateEffect { get; set; }                    // Ngày hiệu lực đơn giá (DateEffect)
    public ReqPartPriceLineStatus Status { get; set; } = ReqPartPriceLineStatus.Pending;

    public PartPriceRequest PartPriceRequest { get; set; } = null!;
    public Part? Part { get; set; }

    public decimal Amount => Quantity * TSTPrice;
}

/// <summary>Phân loại mã lỗi dịch vụ ô tô — Ser_MST_ROComplaintDiagnosticError_ErrorTypeCode (PN: Phàn nàn KH, CD: Chuẩn đoán KT).</summary>
public enum ComplaintErrorType
{
    Complaint = 1,   // PN: Khách hàng phàn nàn / Hiện tượng sự cố (Customer Complaint / Symptom)
    Diagnostic = 2   // CD: Kỹ thuật viên chuẩn đoán / Mã chẩn đoán máy quét (Diagnostic Trouble Code / DTC)
}

/// <summary>Phân loại nhóm hệ thống kỹ thuật xe ô tô tiêu chuẩn xưởng dịch vụ.</summary>
public enum VehicleSystemGroup
{
    Engine = 1,         // Động cơ & Hệ thống nhiên liệu
    Transmission = 2,   // Hộp số & Hệ thống dẫn động
    Chassis = 3,        // Khung gầm, Phanh & Hệ thống lái
    Electrical = 4,     // Điện - Điện tử, Đèn còi & Cảm biến
    HVAC = 5,           // Điều hòa không khí & Thông gió cabin
    BodyPaint = 6,      // Thân vỏ, Cửa kính & Sơn xe
    General = 7         // Hệ thống tổng hợp / Khác
}

/// <summary>Từ điển Mã lỗi phàn nàn & Chẩn đoán kỹ thuật xưởng dịch vụ — Ser_MST_ROComplaintDiagnosticError trong idn.CarService (MNU_QT_DL_QUANLYMALOIPHANNANVACHANDOAN).</summary>
public class ComplaintDiagnosticError : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string ErrorCode { get; set; } = "";                     // Mã lỗi duy nhất (VD: PN-ENG-01, CD-P0300)
    public string ErrorName { get; set; } = "";                     // Tên lỗi / Mô tả tóm tắt hiện tượng (ErrorName)
    public ComplaintErrorType ErrorType { get; set; } = ComplaintErrorType.Complaint; // Loại lỗi (PN / CD)
    public VehicleSystemGroup SystemGroup { get; set; } = VehicleSystemGroup.Engine;   // Phân nhóm hệ thống
    public string? ErrorDesc { get; set; }                          // Mô tả chi tiết triệu chứng & phương pháp kiểm tra (ErrorDesc)
    public string? Remark { get; set; }                             // Ghi chú kỹ thuật, nguyên nhân & khuyến cáo khắc phục (Remark)
    public bool FlagActive { get; set; } = true;                    // Trạng thái hiệu lực (FlagActive = 1/0)
    public int UsageCount { get; set; } = 0;                        // Tần suất xuất hiện trên Lệnh sửa chữa RO / Báo cáo BH
    public string CreatedBy { get; set; } = "Quản đốc";             // Người lập mã
    public DateTime CreatedAt { get; set; } = DateTime.Now;         // Ngày tạo
    public DateTime? UpdatedAt { get; set; }                        // Ngày cập nhật gần nhất

    // Helper properties hiển thị giao diện UI
    public string ErrorTypeCode => ErrorType == ComplaintErrorType.Complaint ? "PN" : "CD";
    public string ErrorTypeName => ErrorType == ComplaintErrorType.Complaint ? "Phàn nàn của KH (PN)" : "Chuẩn đoán kỹ thuật (CD)";
    public string SystemGroupName => SystemGroup switch
    {
        VehicleSystemGroup.Engine => "Động cơ & Nhiên liệu",
        VehicleSystemGroup.Transmission => "Hộp số & Dẫn động",
        VehicleSystemGroup.Chassis => "Khung gầm & Phanh/Lái",
        VehicleSystemGroup.Electrical => "Điện - Điện tử & Cảm biến",
        VehicleSystemGroup.HVAC => "Điều hòa cabin (AC)",
        VehicleSystemGroup.BodyPaint => "Thân vỏ & Đồng sơn",
        _ => "Tổng hợp / Khác"
    };
    public string BadgeTypeClass => ErrorType == ComplaintErrorType.Complaint ? "bg-warning text-dark" : "bg-primary text-white";
    public string BadgeGroupClass => SystemGroup switch
    {
        VehicleSystemGroup.Engine => "bg-danger text-white",
        VehicleSystemGroup.Transmission => "bg-info text-dark",
        VehicleSystemGroup.Chassis => "bg-secondary text-white",
        VehicleSystemGroup.Electrical => "bg-warning text-dark",
        VehicleSystemGroup.HVAC => "bg-success text-white",
        VehicleSystemGroup.BodyPaint => "bg-dark text-white",
        _ => "bg-light text-dark"
    };
}

/// <summary>DTO Tổng hợp chỉ số từ điển mã lỗi phàn nàn & chẩn đoán xưởng dịch vụ.</summary>
public class ComplaintDiagnosticSummaryDto
{
    public int TotalErrors { get; set; }
    public int ComplaintCount { get; set; } // PN
    public int DiagnosticCount { get; set; } // CD
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
    public int EngineCount { get; set; }
    public int TransmissionCount { get; set; }
    public int ChassisCount { get; set; }
    public int ElectricalCount { get; set; }
    public int HvacCount { get; set; }
    public int BodyPaintCount { get; set; }
    public int TotalUsageCount { get; set; }
    public List<ComplaintDiagnosticError> TopUsedErrors { get; set; } = [];
}

/// <summary>Trạng thái Phiếu Chăm sóc sinh nhật khách hàng — theo Ser_CustomerCareBth Status (0: Chưa liên hệ, 1: Đã liên hệ, 2: Không liên hệ) trong idn.CarService.</summary>
public enum CustomerCareBirthdayStatus
{
    Pending = 0,      // 0: Chưa liên hệ (Chờ gọi chúc mừng & tặng voucher)
    Contacted = 1,    // 1: Đã liên hệ (Đã gửi lời chúc, cấp mã voucher tri ân)
    NotContacted = 2  // 2: Không liên hệ được (Máy bận, không nghe máy, sai số)
}

/// <summary>Kênh liên hệ chăm sóc chúc mừng sinh nhật khách hàng.</summary>
public enum BirthdayContactChannel
{
    Call = 0,        // Gọi điện thoại trực tiếp
    SMS = 1,         // Tin nhắn SMS Brandname
    Zalo = 2,        // Tin nhắn Zalo ZNS / CSKH
    InPerson = 3     // Trực tiếp tại xưởng dịch vụ
}

/// <summary>Phiếu Chăm sóc & Chúc mừng sinh nhật khách hàng — Ser_CustomerCareBth trong idn.CarService (Quản lý Chúc mừng SN Khách hàng - FrmCSCCustomerCareDOB).</summary>
public class CustomerCareBirthday : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string CareBthNo { get; set; } = "";                                 // Mã phiếu CSKH SN (VD: BTH260427-001)
    public int CustomerId { get; set; }                                         // Khách hàng sinh nhật (CusId)
    public int? CarId { get; set; }                                              // Xe đại diện của khách (CarId)
    public DateTime? DateOfBirth { get; set; }                                   // Ngày tháng năm sinh gốc của khách (DOB)
    public DateTime DateBth { get; set; }                                        // Ngày sinh nhật trong năm (DateBth, luật 29/02 lùi 28/02 năm thường)
    public CustomerCareBirthdayStatus Status { get; set; } = CustomerCareBirthdayStatus.Pending; // Trạng thái 0/1/2
    public DateTime? ContactDate { get; set; }                                   // Thời điểm liên hệ
    public string? ContactedBy { get; set; }                                     // Nhân viên CSKH liên hệ
    public BirthdayContactChannel ContactChannel { get; set; } = BirthdayContactChannel.Call; // Kênh liên hệ
    public string? Remark { get; set; }                                          // Ghi chú chăm sóc / Lời chúc / Phản hồi của KH

    // Quà tặng & Voucher tri ân sinh nhật tiêu chuẩn đại lý Hyundai
    public string? GiftVoucherCode { get; set; }                                 // Mã voucher quà tặng (VD: BDAY-2026-X9Y2)
    public decimal GiftVoucherValue { get; set; } = 300_000m;                    // Trị giá voucher quà tặng (VNĐ)
    public decimal DiscountPercent { get; set; } = 10m;                          // Tỷ lệ giảm giá công/phụ tùng (%)
    public DateTime? VoucherValidUntil { get; set; }                             // Hạn sử dụng voucher (hết tháng sinh + 30 ngày)
    public bool IsVoucherUsed { get; set; } = false;                             // Cờ đã áp dụng voucher vào RO
    public int? UsedInROId { get; set; }                                         // Lệnh sửa chữa đã sử dụng voucher

    // Đặt lịch hẹn làm dịch vụ nhân dịp sinh nhật
    public int? AppointmentId { get; set; }                                      // Lịch hẹn đón tiếp phát sinh từ cuộc gọi CSKH

    // Nhật ký & Audit (khớp lược đồ Ser_CustomerCareBth nguồn)
    public string CreatedBy { get; set; } = "system";                            // Người tạo (hệ thống quét hoặc nhân viên tạo)
    public DateTime CreatedAt { get; set; } = DateTime.Now;                      // Ngày tạo
    public DateTime? UpdatedAt { get; set; }                                     // Cập nhật gần nhất
    public string? UpdatedBy { get; set; }
    public DateTime? LogLuDateTime { get; set; }                                 // Thời điểm cập nhật cuối (LogLuDateTime nguồn)
    public string? LogLUBy { get; set; }                                         // Người cập nhật cuối (LogLUBy nguồn)

    public Customer Customer { get; set; } = null!;
    public Car? Car { get; set; }
    public RepairOrder? UsedInRO { get; set; }
    public Appointment? Appointment { get; set; }

    // Helpers tính toán hiển thị UI
    public int BirthMonth => DateBth.Month;
    public int BirthDay => DateBth.Day;
    public int CurrentAge => DateOfBirth.HasValue ? Math.Max(0, DateTime.Today.Year - DateOfBirth.Value.Year) : 0;
    public bool IsTodayBirthday => DateBth.Month == DateTime.Today.Month && DateBth.Day == DateTime.Today.Day;
    public bool IsThisMonthBirthday => DateBth.Month == DateTime.Today.Month;
    public int DaysUntilBirthday
    {
        get
        {
            var today = DateTime.Today;
            var target = new DateTime(today.Year, DateBth.Month, Math.Min(DateBth.Day, DateTime.DaysInMonth(today.Year, DateBth.Month)));
            if (target < today) target = target.AddYears(1);
            return (target - today).Days;
        }
    }

    /// <summary>Luật tính ngày sinh nhật trong năm: Nếu sinh ngày 29/02 mà năm đích không nhuận thì lùi về 28/02.</summary>
    public static DateTime CalculateDateBth(DateTime dob, int targetYear)
    {
        int month = dob.Month;
        int day = dob.Day;
        if (month == 2 && day == 29 && !DateTime.IsLeapYear(targetYear))
        {
            day = 28;
        }
        return new DateTime(targetYear, month, day);
    }
}

/// <summary>DTO Tổng hợp chỉ số Chăm sóc sinh nhật khách hàng — Ser_CustomerCareBth_Sumary trong idn.CarService.</summary>
public class CustomerCareBirthdaySummaryDto
{
    public int TotalCount { get; set; }
    public int ThisMonthCount { get; set; }
    public int TodayCount { get; set; }
    public int PendingCount { get; set; }
    public int ContactedCount { get; set; }
    public int NotContactedCount { get; set; }
    public int VouchersIssuedCount { get; set; }
    public int VouchersUsedCount { get; set; }
    public decimal TotalVoucherValue { get; set; }
    public decimal ContactCompletionRate => TotalCount > 0 ? Math.Round((decimal)ContactedCount * 100m / TotalCount, 1) : 0m;
}

/// <summary>Nhóm kỹ thuật công việc bảo hành xe — theo Ser_MST_ROWarrantyWork trong idn.CarService.</summary>
public enum WarrantyLaborGroup
{
    Engine = 1,            // Động cơ & Hệ thống nhiên liệu
    Transmission = 2,      // Hộp số & Hệ thống truyền động
    Electrical = 3,        // Hệ thống điện & Điện tử
    BrakeSteering = 4,     // Hệ thống phanh & Lái
    ChassisSuspension = 5, // Khung gầm & Treo
    BodyInterior = 6,      // Thân vỏ & Nội thất
    SoftwareECU = 7        // Lập trình & Cập nhật phần mềm ECU
}

/// <summary>Phân loại chính sách bảo hành hãng — theo Ser_MST_ROWarrantyType trong idn.CarService.</summary>
public enum WarrantyCoverageType
{
    NewCar = 1,            // W1: Bảo hành xe mới tiêu chuẩn (3-5 năm / 100.000 km)
    GenuinePart = 2,       // W2: Bảo hành phụ tùng thay thế chính hãng (12 tháng / 20.000 km)
    Goodwill = 3,          // W3: Bảo hành thiện chí (Đại lý & Hãng hỗ trợ KH thân thiết)
    CampaignRecall = 4,    // W4: Chiến dịch kỹ thuật & Triệu hồi (Recall Campaign)
    ExtendedWarranty = 5   // W5: Bảo hành gia hạn mở rộng
}

/// <summary>Công việc bảo hành định mức & Đơn giá hãng chi trả — Ser_MST_ROWarrantyWork trong idn.CarService.</summary>
public class WarrantyWork : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";          // Mã công việc bảo hành (ROWWorkCode, VD: WRT-ENG-001)
    public string Name { get; set; } = "";          // Tên công việc bảo hành (ROWWorkName)
    public string Model { get; set; } = "";         // Dòng xe áp dụng (VD: SantaFe, Tucson, Creta, Accent...)
    public WarrantyLaborGroup LaborGroup { get; set; } = WarrantyLaborGroup.Engine; // Nhóm kỹ thuật
    public WarrantyCoverageType CoverageType { get; set; } = WarrantyCoverageType.NewCar; // Loại bảo hành áp dụng
    public string? AppTypeCode { get; set; }        // Mã phê duyệt HTC/HTV
    public string? EngineType { get; set; }         // Loại động cơ / Remark (VD: SmartStream D2.2, Kappa 1.4 MPI...)
    public decimal RateHour { get; set; } = 1.0m;   // Giờ công định mức bảo hành Flat Rate (RateHour)
    public decimal RatePrice { get; set; } = 300_000m; // Đơn giá giờ công bảo hành do hãng duyệt chi trả (RatePrice)
    public decimal Price { get; set; }              // Thành tiền công bảo hành chuẩn (Price = RateHour * RatePrice)
    public int VatPercent { get; set; } = 8;        // Thuế suất VAT (%)
    public decimal TotalWithVat => Math.Round(Price * (1 + VatPercent / 100m), 0);
    public string? RequiredPhotos { get; set; }     // Danh sách hồ sơ ảnh bắt buộc (VIN, ODO, Lỗi, Nghiệm thu)
    public string? Remark { get; set; }             // Ghi chú kỹ thuật / điều kiện bảo hành
    public bool FlagActive { get; set; } = true;    // Trạng thái hiệu lực (FlagActive)
    public string CreatedBy { get; set; } = "Hãng HTC";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public List<RepairLine> RepairLines { get; set; } = [];

    public int UsageCount => RepairLines.Count;
}

/// <summary>DTO Tổng hợp chỉ số Định mức giờ công bảo hành — Ser_MST_ROWarrantyWork trong idn.CarService.</summary>
public class WarrantyWorkSummaryDto
{
    public int TotalWorks { get; set; }
    public int ActiveWorks { get; set; }
    public int InactiveWorks { get; set; }
    public int TotalModels { get; set; }
    public decimal AvgRateHour { get; set; }
    public decimal AvgPrice { get; set; }
    public int TotalClaimsApplied { get; set; }
}

/// <summary>Phân loại cấp độ bảo dưỡng định kỳ xe — theo Ser_MST_ROMaintanceSetting Maintances trong idn.CarService.</summary>
public enum MaintenanceLevel
{
    Initial1K = 0,            // Cấp 0: Bảo dưỡng lần đầu (1.000 km)
    Level1Minor = 1,          // Cấp 1: Bảo dưỡng nhỏ (5.000 km, 15k, 25k...)
    Level2Medium = 2,         // Cấp 2: Bảo dưỡng trung bình (10.000 km, 30k, 50k, 70k, 90k...)
    Level3Major = 3,          // Cấp 3: Bảo dưỡng trung bình lớn (20.000 km, 60k, 100k...)
    Level4Comprehensive = 4   // Cấp 4: Bảo dưỡng lớn toàn diện (40.000 km, 80k, 120k...)
}

/// <summary>Thiết lập chu kỳ & Định mức cấp bảo dưỡng định kỳ xe — Ser_MST_ROMaintanceSetting trong idn.CarService (MNU_QT_DL_THIETLAPBAODUONG).</summary>
public class MaintenanceSetting : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string ROMSID { get; set; } = "";                                    // Mã thiết lập bảo dưỡng (ROMSID chuẩn nguồn: VD ROMS-01K, ROMS-05K, ROMS-10K, ROMS-20K...)
    public string Name { get; set; } = "";                                       // Tên mốc bảo dưỡng (VD: Bảo dưỡng Cấp 1 - 5.000 km)
    public int Km { get; set; }                                                  // Mốc số Kilomet tiêu chuẩn (Km chuẩn nguồn: 1000, 5000, 10000, 20000...)
    public int Maintances { get; set; } = 1;                                     // Số lần / Cấp độ bảo dưỡng thỏa mãn CSBH (Maintances chuẩn nguồn: 0, 1, 2, 3, 4)
    public MaintenanceLevel Level { get; set; } = MaintenanceLevel.Level1Minor;  // Cấp độ bảo dưỡng
    public int MonthsInterval { get; set; } = 6;                                 // Thời gian khuyến nghị (tháng)
    public decimal TakingTimeHours { get; set; } = 1.0m;                         // Định mức thời gian thực hiện (giờ)
    public decimal EstimatedCost { get; set; } = 650_000m;                       // Chi phí bảo dưỡng ước tính tham khảo (VNĐ)
    public int? ServicePackageId { get; set; }                                   // Gói dịch vụ bảo dưỡng tương ứng (nếu có)
    public ServicePackage? ServicePackage { get; set; }
    public string? RequiredChecklist { get; set; }                               // Hạng mục kiểm tra & thay thế bắt buộc chuẩn HTC
    public string? Description { get; set; }                                     // Mô tả nội dung kỹ thuật
    public bool FlagWarranty { get; set; } = true;                               // Yêu cầu bắt buộc để duy trì chính sách bảo hành (CSBH)
    public bool FlagActive { get; set; } = true;                                 // Cờ hoạt động (FlagActive chuẩn nguồn)
    public DateTime? LogLuDateTime { get; set; } = DateTime.Now;                 // Thời gian cập nhật cuối (LogLUDateTime chuẩn nguồn)
    public string? LogLUBy { get; set; } = "web";                                // Người cập nhật cuối (LogLUBy chuẩn nguồn)
    public string CreatedBy { get; set; } = "Hệ thống HTC";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public List<RepairOrder> RepairOrders { get; set; } = [];

    public int UsageCount => RepairOrders.Count;
    public string LevelName => Level switch
    {
        MaintenanceLevel.Initial1K => "Lần đầu (1.000 km)",
        MaintenanceLevel.Level1Minor => "Cấp 1 - Nhỏ (5.000 km)",
        MaintenanceLevel.Level2Medium => "Cấp 2 - Trung bình (10.000 km)",
        MaintenanceLevel.Level3Major => "Cấp 3 - Trung bình lớn (20.000 km)",
        MaintenanceLevel.Level4Comprehensive => "Cấp 4 - Lớn toàn diện (40.000 km)",
        _ => $"Cấp {Maintances}"
    };
    public string LevelBadgeClass => Level switch
    {
        MaintenanceLevel.Initial1K => "bg-secondary",
        MaintenanceLevel.Level1Minor => "bg-info text-dark",
        MaintenanceLevel.Level2Medium => "bg-primary",
        MaintenanceLevel.Level3Major => "bg-warning text-dark",
        MaintenanceLevel.Level4Comprehensive => "bg-danger",
        _ => "bg-secondary"
    };
}

/// <summary>DTO Tổng hợp chỉ số Thiết lập bảo dưỡng — Ser_MST_ROMaintanceSetting.</summary>
public class MaintenanceSettingSummaryDto
{
    public int TotalSettings { get; set; }
    public int ActiveSettings { get; set; }
    public int InactiveSettings { get; set; }
    public int WarrantyRequiredCount { get; set; }
    public int LinkedPackageCount { get; set; }
    public decimal AvgLaborHours { get; set; }
    public decimal AvgEstimatedCost { get; set; }
    public int MaxKm { get; set; }
    public int TotalROsApplied { get; set; }
}

/// <summary>DTO Kết quả gợi ý & tư vấn mốc bảo dưỡng theo số Km thực tế của xe.</summary>
public class MaintenanceSuggestionDto
{
    public int CurrentKm { get; set; }
    public MaintenanceSetting? MatchedSetting { get; set; }
    public MaintenanceSetting? NextSetting { get; set; }
    public int KmDifference { get; set; }
    public string StatusAdvice { get; set; } = "";        // Đúng hạn / Sắp đến hạn / Quá hạn bảo dưỡng
    public string LevelBadgeClass { get; set; } = "";
    public ServicePackage? SuggestedPackage { get; set; }
    public bool IsWarrantyCompliant { get; set; } = true; // Đánh giá xe tuân thủ chính sách bảo hành
    public string AdviceNote { get; set; } = "";
}

/// <summary>Loại bảo hành chính của Lệnh sửa chữa (RO) — theo Ser_MST_ROWarrantyType ROWTypeCode trong idn.CarService.</summary>
public enum WarrantyTypeCode
{
    XM = 0,  // Xe mới / Bảo hành tiêu chuẩn (Standard Warranty)
    SB = 1,  // Sửa chữa bảo hành (Warranty Repair)
    PT = 2,  // Phụ tùng bảo hành (Warranty Part)
    TC = 3,  // Bảo hành thiện chí / Hỗ trợ khách hàng (Goodwill)
    BT = 4   // Bảo hành bổ sung / Mở rộng (Extended Warranty)
}

/// <summary>Loại chi tiết của loại bảo hành RO — theo Ser_MST_ROWarrantyType ROWTypeDtlCode trong idn.CarService.</summary>
public enum WarrantyTypeDetailCode
{
    A = 0,  // Loại A — Hư hỏng do lỗi sản xuất
    B = 1,  // Loại B — Hư hỏng do linh kiện
    P = 2,  // Loại P — Phụ tùng
    W = 3,  // Loại W — Công việc bảo hành
    S = 4,  // Loại S — Sửa chữa
    R = 5,  // Loại R — Bổ sung / phát sinh
    C = 6   // Loại C — Chi phí khác
}

/// <summary>Danh mục Loại bảo hành RO (Loại chính × Loại chi tiết) — Ser_MST_ROWarrantyType trong idn.CarService (FrmMstWarrantyTypeMng).</summary>
public class WarrantyType : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string ROWTID { get; set; } = "";                 // Khoá tự tăng nguồn (ROWTID = @@Identity của Ser_MST_ROWarrantyType)
    public WarrantyTypeCode TypeCode { get; set; } = WarrantyTypeCode.XM;          // Loại chính (ROWTypeCode: XM/SB/PT/TC/BT)
    public string TypeName { get; set; } = "";               // Tên loại chính (ROWTypeName)
    public WarrantyTypeDetailCode DetailCode { get; set; } = WarrantyTypeDetailCode.A; // Loại chi tiết (ROWTypeDtlCode: A/B/P/W/S/R/C)
    public string DetailName { get; set; } = "";             // Tên loại chi tiết (ROWTypeDtlName)
    public string? PhotoTypeDisplay { get; set; }            // Chuỗi hiển thị loại ảnh (ROWPhotoType) — dựng lại từ bảng chi tiết
    public bool FlagActive { get; set; } = true;             // Trạng thái hiệu lực (FlagActive)
    public DateTime? LogLuDateTime { get; set; } = DateTime.Now; // Thời gian cập nhật cuối (LogLUDateTime)
    public string? LogLUBy { get; set; } = "web";            // Người cập nhật cuối (LogLUBy)
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public List<WarrantyTypePhoto> Photos { get; set; } = []; // Danh sách loại ảnh chứng minh bắt buộc (1-n)

    public string TypeCodeText => TypeCode switch
    {
        WarrantyTypeCode.XM => "XM — Xe mới",
        WarrantyTypeCode.SB => "SB — Sửa chữa BH",
        WarrantyTypeCode.PT => "PT — Phụ tùng BH",
        WarrantyTypeCode.TC => "TC — Thiện chí",
        WarrantyTypeCode.BT => "BT — Bổ sung",
        _ => TypeCode.ToString()
    };
    public string DetailCodeText => DetailCode.ToString();
    public int PhotoCount => Photos.Count;
}

/// <summary>Loại ảnh chứng minh bắt buộc cho một loại bảo hành RO — Ser_MST_ROWarrantyType_PhotoType trong idn.CarService (1 loại BH đòi NHIỀU loại ảnh).</summary>
public class WarrantyTypePhoto : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int WarrantyTypeId { get; set; }                  // FK tới WarrantyType (ROWTID)
    public WarrantyType? WarrantyType { get; set; }
    public string ROWPTCode { get; set; } = "";              // Mã loại ảnh (ROWPTCode) — tra ở master WarrantyPhotoType
    public string? ROWPTName { get; set; }                   // Tên loại ảnh (ROWPTName)
}

/// <summary>Danh mục Loại ảnh bảo hành (master gốc) — Ser_MST_ROWarrantyPhotoType trong idn.CarService (FrmMstWarrantyTypeMng).</summary>
public class WarrantyPhotoType : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string ROWPTCode { get; set; } = "";              // Mã loại ảnh (ROWPTCode)
    public string ROWPTName { get; set; } = "";              // Tên loại ảnh (ROWPTName)
    public bool FlagActive { get; set; } = true;             // Trạng thái hiệu lực (FlagActive)
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>DTO Tổng hợp chỉ số Danh mục Loại bảo hành RO — Ser_MST_ROWarrantyType.</summary>
public class WarrantyTypeSummaryDto
{
    public int TotalTypes { get; set; }
    public int ActiveTypes { get; set; }
    public int InactiveTypes { get; set; }
    public int TotalPhotoTypes { get; set; }
    public int TypesWithPhotos { get; set; }
    public int DistinctMainCodes { get; set; }
    public int DistinctDetailCodes { get; set; }
}

/// <summary>Loại chỉ tiêu kinh doanh đại lý — Mst_DealerTargetType trong idn.CarService (MH 168).</summary>
public enum DealerTargetType
{
    Revenue = 0,        // DT   — Doanh thu dịch vụ
    RO = 1,             // RO   — Số lệnh sửa chữa
    Vehicle = 2,        // XE   — Số lượt xe vào xưởng
    PartRevenue = 3,    // DTPT — Doanh thu phụ tùng
    LaborRevenue = 4,   // DTGC — Doanh thu giờ công
    CSI = 5,            // CSI  — Điểm hài lòng khách hàng
    Other = 6           // KHAC — Chỉ tiêu khác
}

/// <summary>Kỳ chỉ tiêu kinh doanh đại lý (theo năm) — Mst_DealerTarget trong idn.CarService.</summary>
public class DealerTarget : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int TargetYear { get; set; }                       // Năm chỉ tiêu (TargetYear, 1900..2100)
    public string? Remark { get; set; }                       // Ghi chú kỳ chỉ tiêu
    public string CreatedBy { get; set; } = "web";            // Người lập (CreatedBy)
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // Ngày lập (CreatedDateTime)
    public string? UpdatedBy { get; set; }                    // Người cập nhật (UpdateBy)
    public DateTime? UpdatedAt { get; set; }                  // Ngày cập nhật (UpdateDateTime)

    public List<DealerTargetDetail> Details { get; set; } = [];

    public int DetailCount => Details.Count;
    public int DealerCount => Details.Select(d => d.DealerCode).Distinct().Count();
    public int MonthCount => Details.Select(d => d.TargetMonth).Distinct().Count();
    public decimal TotalTargetValue => Details.Sum(d => d.TargetValue);
}

/// <summary>Chi tiết chỉ tiêu kinh doanh theo đại lý / tháng / loại chỉ tiêu — Mst_DealerTargetDetail trong idn.CarService.</summary>
public class DealerTargetDetail : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int DealerTargetId { get; set; }                   // FK tới kỳ chỉ tiêu (DealerTarget)
    public int TargetYear { get; set; }                       // Năm chỉ tiêu (TargetYear)
    public string DealerCode { get; set; } = "";              // Mã đại lý (DealerCode)
    public string? DealerName { get; set; }                   // Tên đại lý (DealerName)
    public DateTime TargetMonth { get; set; }                 // Tháng chỉ tiêu (TargetMonth, chuẩn hoá về ngày 01)
    public DealerTargetType TargetType { get; set; } = DealerTargetType.Revenue; // Loại chỉ tiêu (TargetTypeCode)
    public long TargetValue { get; set; }                     // Giá trị chỉ tiêu (TargetValue, >= 0)
    public string CreatedBy { get; set; } = "web";            // Người lập (CreatedBy)
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // Ngày lập (CreatedDateTime)

    public DealerTarget DealerTarget { get; set; } = null!;

    public int TargetMonthNumber => TargetMonth.Month;        // Số tháng (1..12)
    public string TargetMonthText => TargetMonth.ToString("MM/yyyy");
}

/// <summary>DTO Tổng hợp chỉ số Chỉ tiêu kinh doanh đại lý — Mst_DealerTarget / Mst_DealerTargetDetail.</summary>
public class DealerTargetSummaryDto
{
    public int TotalPeriods { get; set; }
    public int TotalDetails { get; set; }
    public int TotalDealers { get; set; }
    public int TotalTypes { get; set; }
    public long TotalTargetValue { get; set; }
    public int CurrentYear { get; set; }
    public int CurrentYearDetails { get; set; }
    public long CurrentYearTargetValue { get; set; }
}

/// <summary>Loại khách hàng dịch vụ (danh mục gốc) — Ser_MST_CustomerType trong idn.CarService.
/// Dùng làm tầng dự phòng hệ số giá khi chưa cấu hình riêng cho từng dịch vụ.</summary>
public class CustomerType : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string CusTypeCode { get; set; } = "";             // Mã loại khách hàng (CusTypeID nguồn)
    public string CusTypeName { get; set; } = "";             // Tên loại khách hàng (CusTypeName)
    public decimal CusFactor { get; set; } = 1.0m;            // Hệ số giá mặc định của loại khách (CusFactor)
    public string CusPersonType { get; set; } = "Personal";   // Cá nhân / Tổ chức (CusPersonType)
    public bool IsActive { get; set; } = true;                // Trạng thái hiệu lực (IsActive)
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<CusServiceFactor> ServiceFactors { get; set; } = [];
}

/// <summary>Hệ số giá DỊCH VỤ theo loại khách hàng — Ser_Mst_CusServiceFactor trong idn.CarService.
/// Nguồn tra bảng này ở CSDL TRUNG TÂM; giá hiệu lực = Service.Price × COALESCE(Factor, CusType.CusFactor, 1).</summary>
public class CusServiceFactor : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int ServiceItemId { get; set; }                    // Dịch vụ áp dụng (SerID → ServiceItem)
    public ServiceItem ServiceItem { get; set; } = null!;
    public int CustomerTypeId { get; set; }                   // Loại khách hàng áp dụng (CusTypeID → CustomerType)
    public CustomerType CustomerType { get; set; } = null!;
    public decimal Factor { get; set; } = 1.0m;               // Hệ số giá dịch vụ (Factor)
    public string? DealerCode { get; set; }                   // Mã đại lý áp dụng (DealerCode)
    public string? LogLUBy { get; set; }                      // Người cập nhật cuối (LogLUBy)
    public DateTime? LogLUDateTime { get; set; }              // Thời gian cập nhật cuối (LogLUDateTime)
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>DTO một dòng ma trận hệ số giá dịch vụ × loại khách hàng (kèm giá hiệu lực).</summary>
public class CusServiceFactorRowDto
{
    public int ServiceItemId { get; set; }
    public string ServiceCode { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public decimal BasePrice { get; set; }                    // Giá niêm yết gốc của dịch vụ (Service.Price)
    public int CustomerTypeId { get; set; }
    public string CusTypeCode { get; set; } = "";
    public string CusTypeName { get; set; } = "";
    public decimal Factor { get; set; }                       // Hệ số hiệu lực (đã áp tầng dự phòng)
    public decimal EffectivePrice { get; set; }               // Giá hiệu lực = BasePrice × Factor
    public bool IsCustomized { get; set; }                    // true nếu có cấu hình riêng (không dùng hệ số mặc định)
}

/// <summary>DTO Tổng hợp chỉ số ma trận hệ số giá dịch vụ — Ser_Mst_CusServiceFactor.</summary>
public class CusServiceFactorSummaryDto
{
    public int TotalServices { get; set; }
    public int TotalCustomerTypes { get; set; }
    public int TotalCells { get; set; }                       // Tổng số ô ma trận (dịch vụ × loại khách)
    public int CustomizedCells { get; set; }                  // Số ô có cấu hình hệ số riêng
    public decimal AvgFactor { get; set; }
    public decimal MinFactor { get; set; }
    public decimal MaxFactor { get; set; }
}

/// <summary>Nhật ký thao tác trên Lệnh sửa chữa (audit log) — Ser_ROHistory trong idn.CarService.
/// Mỗi lần tạo / cập nhật / chuyển trạng thái / hủy RO đều ghi 1 dòng: ai làm, lúc nào, ở trạng thái nào, ghi chú gì.
/// Nguồn lưu ở CSDL TRUNG TÂM (CmCenter) và đồng bộ sang kho (WH).</summary>
public class RoHistory : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int ROId { get; set; }                             // Lệnh sửa chữa (ROID)
    public RepairOrder? RO { get; set; }
    public ROStatus Status { get; set; }                      // Trạng thái RO tại thời điểm thao tác (Status)
    public DateTime HistoryDate { get; set; } = DateTime.Now; // Ngày thao tác (HistoryDate)
    public string? UserCode { get; set; }                     // Người thao tác (UserCode)
    public string? Note { get; set; }                         // Ghi chú / diễn giải thao tác (Note)
}

/// <summary>DTO tổng hợp nhật ký thao tác RO — phục vụ màn hình tra cứu lịch sử.</summary>
public class RoHistorySummaryDto
{
    public int TotalEntries { get; set; }
    public int RejectCount { get; set; }                      // Số lần RO bị hủy (REJ)
    public int DistinctStatusCount { get; set; }              // Số trạng thái khác nhau đã đi qua
    public DateTime? FirstEntryAt { get; set; }
    public DateTime? LastEntryAt { get; set; }
}

/// <summary>Phân loại dòng xe theo cấp độ thương mại — theo Ser_Mst_Model / Mst_CarModelStd idn.CarService.
/// Dùng để phân nhóm danh mục dòng xe phục vụ tra cứu, báo giá và định mức giờ công.</summary>
public enum CarModelSegment
{
    Sedan = 0,      // Sedan — Xe du lịch 4 cửa (Accent, Elantra, Grand i10...)
    SUV = 1,        // SUV / CUV — Xe thể thao đa dụng (Tucson, Santa Fe, Creta...)
    MPV = 2,        // MPV / Xe 7 chỗ — Đa dụng gia đình (Custin, Stargazer...)
    Commercial = 3, // Xe thương mại / Bán tải — (Porter, Mighty, Solati...)
    EV = 4          // Xe điện / Hybrid — (Ioniq, Kona Electric...)
}

/// <summary>Danh mục Dòng xe (Car Model Master) — Ser_Mst_Model trong idn.CarService.
/// Quản lý danh mục dòng xe theo thương hiệu (TradeMarkCode) và đại lý (DealerCode),
/// liên kết với danh mục chuẩn hãng Mst_CarModelStd qua ModelCode.
/// Nguồn lưu ở CSDL TRUNG TÂM (CmCenter) và đồng bộ sang kho (WH).</summary>
public class CarModel : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string ModelCode { get; set; } = "";               // Mã dòng xe chuẩn hãng (ModelCode, VD: ACC, TUC, SAN)
    public string ModelName { get; set; } = "";               // Tên dòng xe (ModelName, VD: Hyundai Accent)
    public string TradeMarkCode { get; set; } = "";           // Mã thương hiệu (TradeMarkCode, VD: HMC, HTC)
    public string? ProductionCode { get; set; }               // Mã sản xuất / mã nội bộ nhà máy (ProductionCode)
    public string? DealerCode { get; set; }                   // Mã đại lý áp dụng (DealerCode)
    public CarModelSegment Segment { get; set; } = CarModelSegment.Sedan; // Phân khúc dòng xe
    public int? ProductYear { get; set; }                     // Năm sản xuất / đời xe áp dụng
    public bool IsActive { get; set; } = true;                // Cờ hiệu lực hoạt động (IsActive / FlagActive)
    public string CreatedBy { get; set; } = "web";            // Người tạo (CreatedBy)
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // Ngày tạo (CreatedDate)
    public string? LogLUBy { get; set; }                      // Người cập nhật cuối (LogLUBy)
    public DateTime? LogLUDateTime { get; set; }              // Thời gian cập nhật cuối (LogLUDateTime)
}

/// <summary>DTO tổng hợp chỉ số danh mục dòng xe — phục vụ màn hình quản lý Model.</summary>
public class CarModelSummaryDto
{
    public int TotalModels { get; set; }
    public int ActiveModels { get; set; }
    public int InactiveModels { get; set; }
    public int TradeMarkCount { get; set; }                   // Số thương hiệu khác nhau
    public int SegmentCount { get; set; }                     // Số phân khúc khác nhau
}

/// <summary>Định mức vật tư tối thiểu (Bill of Materials) — Mst_BOM trong idn.CarService.
/// Mỗi BOM là một bộ danh mục phụ tùng tối thiểu (theo mã BOMCode) dùng để kiểm tra/đối chiếu
/// tồn kho tối thiểu và gợi ý phụ tùng cần dự trữ. Nguồn lưu ở CSDL TRUNG TÂM (CmCenter) và đồng bộ sang kho (WH).</summary>
public class Bom : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string BomCode { get; set; } = "";                 // Mã BOM (BOMCode)
    public string BomDesc { get; set; } = "";                 // Diễn giải BOM (BOMDesc)
    public string? Remark { get; set; }                       // Ghi chú (Remark)
    public bool IsActive { get; set; } = true;                // Cờ hiệu lực hoạt động (FlagActive)
    public string CreatedBy { get; set; } = "web";            // Người tạo (LogLUBy lần đầu)
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // Ngày tạo
    public string? LogLUBy { get; set; }                      // Người cập nhật cuối (LogLUBy)
    public DateTime? LogLUDateTime { get; set; }              // Thời gian cập nhật cuối (LogLUDateTime)

    public List<BomLine> Lines { get; set; } = [];
}

/// <summary>Dòng chi tiết định mức vật tư — Mst_BOMDtl trong idn.CarService.
/// Mỗi dòng là một phụ tùng (PartCode) kèm số lượng tối thiểu (QtyMin) trong bộ BOM.</summary>
public class BomLine : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int BomId { get; set; }                            // BOM cha (BOMCode -> Bom)
    public Bom BomHeader { get; set; } = null!;               // BOM cha (navigation)
    public string PartCode { get; set; } = "";                // Mã phụ tùng (PartCode)
    public string PartName { get; set; } = "";                // Tên phụ tùng (PartName)
    public string Unit { get; set; } = "Cái";                 // Đơn vị tính (Unit)
    public decimal QtyMin { get; set; } = 1m;                 // Số lượng tối thiểu (QtyMin)
    public string? LogLUBy { get; set; }                      // Người cập nhật cuối (LogLUBy)
    public DateTime? LogLUDateTime { get; set; }              // Thời gian cập nhật cuối (LogLUDateTime)
}

/// <summary>DTO tổng hợp chỉ số danh mục BOM vật tư — phục vụ màn hình quản lý BOM.</summary>
public class BomSummaryDto
{
    public int TotalBoms { get; set; }
    public int ActiveBoms { get; set; }
    public int InactiveBoms { get; set; }
    public int TotalLines { get; set; }                       // Tổng số dòng phụ tùng trên mọi BOM
    public int DistinctParts { get; set; }                    // Số phụ tùng khác nhau xuất hiện trong các BOM
}

/// <summary>Tài khoản ngân hàng của đại lý (Dealer Bank Account) — Mst_DealerBankAccount trong idn.CarService.
/// Danh mục tài khoản ngân hàng nhận thanh toán của đại lý, dùng để in/in ấn trên phiếu thu, báo giá,
/// hợp đồng và hướng dẫn khách hàng chuyển khoản. Mỗi đại lý (DealerCode) có thể có nhiều tài khoản,
/// sắp xếp theo thứ tự ưu tiên (Idx). Nguồn lưu ở CSDL TRUNG TÂM (CmCenter).</summary>
public class DealerBankAccount : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string DealerCode { get; set; } = "";              // Mã đại lý sở hữu tài khoản (DealerCode)
    public string AccountNo { get; set; } = "";               // Số tài khoản ngân hàng (AccountNo)
    public int Idx { get; set; }                              // Thứ tự ưu tiên hiển thị (Idx)
    public string AccountName { get; set; } = "";             // Tên chủ tài khoản (AccountName)
    public string AccountBankName { get; set; } = "";         // Tên ngân hàng (AccountBankName)
    public string? AccountBankBin { get; set; }               // Mã BIN ngân hàng — dùng cho VietQR (AccountBankBin)
    public bool IsActive { get; set; } = true;                // Cờ hiệu lực hoạt động (FlagActive)
    public string? Remark { get; set; }                       // Ghi chú (Remark)
    public string CreatedBy { get; set; } = "web";            // Người tạo (LogLUBy lần đầu)
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // Ngày tạo
    public string? LogLUBy { get; set; }                      // Người cập nhật cuối (LogLUBy)
    public DateTime? LogLUDateTime { get; set; }              // Thời gian cập nhật cuối (LogLUDateTime)
}

/// <summary>DTO tổng hợp chỉ số danh mục tài khoản ngân hàng đại lý — phục vụ màn hình quản lý.</summary>
public class DealerBankAccountSummaryDto
{
    public int TotalAccounts { get; set; }
    public int ActiveAccounts { get; set; }
    public int InactiveAccounts { get; set; }
    public int DealerCount { get; set; }                      // Số đại lý khác nhau có tài khoản
    public int BankCount { get; set; }                        // Số ngân hàng khác nhau
}
