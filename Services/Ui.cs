using MiniService.Models;

namespace MiniService.Services;

public static class Ui
{
    public static (string text, string code, string css) Status(ROStatus s) => s switch
    {
        ROStatus.Created => ("Lập báo giá", "CRE", "secondary"),
        ROStatus.Printed => ("Chờ KH ký", "PRT", "secondary"),
        ROStatus.Wait4Part => ("Đợi phụ tùng", "W4P", "warning"),
        ROStatus.HasPart => ("Đã có phụ tùng", "HPA", "info"),
        ROStatus.HasRO => ("Chờ sửa", "HRO", "info"),
        ROStatus.InGarage => ("Đang sửa", "INGA", "primary"),
        ROStatus.Repaired => ("Sửa xong", "RPRD", "primary"),
        ROStatus.CheckEnd => ("Đã kiểm tra", "CEND", "info"),
        ROStatus.Paid => ("Đã thanh toán", "PAID", "success"),
        ROStatus.Finished => ("Hoàn tất", "FNS", "success"),
        ROStatus.Rejected => ("Đã hủy", "REJ", "danger"),
        ROStatus.NotResponding => ("Không liên lạc", "NORE", "dark"),
        _ => (s.ToString(), "", "secondary")
    };
    public static string Line(LineType t) => t == LineType.Labor ? "Công" : "Phụ tùng";

    public static (string text, string code, string css) WarrantyStatus(WarrantyStatus s) => s switch
    {
        Models.WarrantyStatus.Pending => ("Chờ gửi duyệt", "PEND", "secondary"),
        Models.WarrantyStatus.Sent => ("Đã gửi HTC", "SENT", "warning"),
        Models.WarrantyStatus.Confirmed => ("HTC đã xác nhận", "CONF", "info"),
        Models.WarrantyStatus.Accepted => ("Hãng chấp thuận", "ACCE", "success"),
        Models.WarrantyStatus.Rejected => ("Hãng từ chối", "REJ", "danger"),
        Models.WarrantyStatus.Reverted => ("Yêu cầu bổ sung", "REVERT", "dark"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string css) Expense(ExpenseType t) => t switch
    {
        ExpenseType.Customer => ("Khách trả", "secondary"),
        ExpenseType.Warranty => ("Bảo hành hãng", "primary"),
        ExpenseType.Insurance => ("Bảo hiểm", "info"),
        ExpenseType.Internal => ("Nội bộ", "dark"),
        _ => (t.ToString(), "secondary")
    };

    public static (string text, string code, string css) AppointmentStatus(AppointmentStatus s) => s switch
    {
        Models.AppointmentStatus.Pending => ("Mới tạo", "PEND", "secondary"),
        Models.AppointmentStatus.Contacted => ("Đã liên hệ", "CONT", "info"),
        Models.AppointmentStatus.Confirmed => ("Đã xác nhận", "CONF", "primary"),
        Models.AppointmentStatus.CheckedIn => ("Đã tiếp nhận", "RECV", "success"),
        Models.AppointmentStatus.Cancelled => ("Đã hủy", "CANC", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static string AppServiceType(AppointmentServiceType t) => t switch
    {
        AppointmentServiceType.Maintenance => "Bảo dưỡng định kỳ",
        AppointmentServiceType.Repair => "Sửa chữa chung",
        AppointmentServiceType.BodyPaint => "Đồng sơn",
        AppointmentServiceType.WarrantyCheck => "Kiểm tra bảo hành",
        AppointmentServiceType.Care => "Chăm sóc làm đẹp",
        _ => t.ToString()
    };

    public static (string text, string code, string css) StockInStatus(StockInStatus s) => s switch
    {
        Models.StockInStatus.Pending => ("Chờ duyệt kho", "PEND", "warning"),
        Models.StockInStatus.Executing => ("Đang kiểm hàng", "EXEC", "info"),
        Models.StockInStatus.Finished => ("Đã nhập kho", "FNS", "success"),
        Models.StockInStatus.Rejected => ("Đã hủy", "REJ", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static string StockInType(StockInType t) => t switch
    {
        Models.StockInType.Normal => "Nhập mua hàng",
        Models.StockInType.Adjustment => "Nhập điều chỉnh",
        Models.StockInType.Return => "Nhập hoàn trả",
        _ => t.ToString()
    };

    public static (string text, string code, string css) OrderPartStatus(OrderPartStatus s) => s switch
    {
        Models.OrderPartStatus.Pending => ("Chờ duyệt", "PEND", "warning"),
        Models.OrderPartStatus.Approved => ("Đã duyệt NCC", "APPR", "info"),
        Models.OrderPartStatus.Finished => ("Đã nhập kho", "FNS", "success"),
        Models.OrderPartStatus.Rejected => ("Đã hủy", "REJ", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string css) OrderPartDeliveryForm(OrderPartDeliveryForm f) => f switch
    {
        Models.OrderPartDeliveryForm.Normal => ("Đặt thường", "secondary"),
        Models.OrderPartDeliveryForm.Warranty => ("Đặt bảo hành", "info"),
        Models.OrderPartDeliveryForm.UrgentVOR => ("Khẩn cấp (VOR)", "danger"),
        _ => (f.ToString(), "secondary")
    };

    public static (string text, string code, string css) CavityStatus(CavityStatus s) => s switch
    {
        Models.CavityStatus.Available => ("Trống / Sẵn sàng", "AVAIL", "success"),
        Models.CavityStatus.Occupied => ("Đang có xe sửa", "OCCU", "danger"),
        Models.CavityStatus.Maintenance => ("Đang bảo trì", "MAIN", "warning"),
        Models.CavityStatus.Inactive => ("Tạm ngưng", "INAC", "secondary"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string code, string css) ReceptionStatus(ReceptionStatus s) => s switch
    {
        Models.ReceptionStatus.Pending => ("Tiếp nhận xe", "PEND", "warning"),
        Models.ReceptionStatus.InService => ("Đang sửa chữa", "INSC", "primary"),
        Models.ReceptionStatus.Delivered => ("Đã bàn giao xe", "DELV", "success"),
        Models.ReceptionStatus.Cancelled => ("Đã hủy phiếu", "CANC", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string code, string css) AssignmentWorkStatus(AssignmentWorkStatus s) => s switch
    {
        Models.AssignmentWorkStatus.Assigned => ("Chờ nhận việc", "ASSIGNED", "warning"),
        Models.AssignmentWorkStatus.InProgress => ("Đang thi công", "IN_PROG", "primary"),
        Models.AssignmentWorkStatus.Completed => ("Hoàn tất sửa", "COMPLETED", "success"),
        Models.AssignmentWorkStatus.Cancelled => ("Đã hủy", "CANCELLED", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string code, string css, string icon) WorkType(WorkType t) => t switch
    {
        Models.WorkType.SCC => ("Sửa chữa chung", "SCC", "primary", "bi-wrench-adjustable"),
        Models.WorkType.SCD => ("Sửa chữa đồng", "SCD", "warning", "bi-hammer"),
        Models.WorkType.SCS => ("Sửa chữa sơn", "SCS", "info", "bi-paint-bucket"),
        _ => (t.ToString(), "", "secondary", "bi-tools")
    };

    public static string FuelLevelText(int level) => level switch
    {
        1 => "1/4 bình (E)",
        2 => "1/2 bình",
        3 => "3/4 bình",
        4 => "Đầy bình (F)",
        _ => $"{level}/4 bình"
    };

    public static (string text, string css, string icon) AuditStatus(AuditStatus s) => s switch
    {
        Models.AuditStatus.Good => ("Đạt / Tốt", "success", "bi-check-circle-fill"),
        Models.AuditStatus.Attention => ("Cần theo dõi", "warning", "bi-exclamation-triangle-fill"),
        Models.AuditStatus.Replace => ("Cần sửa / Thay", "danger", "bi-x-circle-fill"),
        Models.AuditStatus.NA => ("Không có (K/A)", "secondary", "bi-dash-circle"),
        _ => (s.ToString(), "secondary", "bi-question-circle")
    };

    public static readonly string[] InspectionLevels =
    [
        "Bảo dưỡng cấp 1 (5.000 km)",
        "Bảo dưỡng cấp 2 (10.000 km)",
        "Bảo dưỡng cấp 3 (20.000 km)",
        "Bảo dưỡng cấp 4 (40.000 km / 80.000 km)",
        "Kiểm tra & Sửa chữa chung",
        "Đồng sơn & Thân vỏ",
        "Kiểm tra bảo hành hãng",
        "Chăm sóc & Làm đẹp xe"
    ];

    public static (string text, string code, string icon, string css) CavityType(CavityType t) => t switch
    {
        Models.CavityType.EM => ("Bảo dưỡng nhanh (EM)", "EM", "bi-lightning-charge", "info"),
        Models.CavityType.GR => ("Sửa chữa chung (GR)", "GR", "bi-tools", "primary"),
        Models.CavityType.BP => ("Đồng sơn (BP)", "BP", "bi-palette", "warning"),
        Models.CavityType.KCS => ("Kiểm tra KCS (QC)", "KCS", "bi-shield-check", "success"),
        Models.CavityType.Wash => ("Rửa xe & Vệ sinh", "WASH", "bi-droplet-half", "secondary"),
        _ => (t.ToString(), "", "bi-wrench", "secondary")
    };

    public static (string text, string code, string css) StockOutStatus(StockOutStatus s) => s switch
    {
        Models.StockOutStatus.Pending => ("Chờ xuất kho", "PEND", "warning"),
        Models.StockOutStatus.Executing => ("Đang soạn hàng", "EXEC", "info"),
        Models.StockOutStatus.Finished => ("Đã xuất kho", "FNS", "success"),
        Models.StockOutStatus.Rejected => ("Đã hủy", "REJ", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static string StockOutType(StockOutType t) => t switch
    {
        Models.StockOutType.Service => "Xuất dịch vụ (RO)",
        Models.StockOutType.Normal => "Xuất bán lẻ",
        Models.StockOutType.Warranty => "Xuất bảo hành",
        Models.StockOutType.Internal => "Xuất nội bộ",
        _ => t.ToString()
    };

    public static (string text, string code, string css) CustomerCareStatus(CustomerCareStatus s) => s switch
    {
        Models.CustomerCareStatus.Pending => ("Chờ gọi CSKH", "PEND", "warning"),
        Models.CustomerCareStatus.ContactedSatisfied => ("Hài lòng", "CIFB", "success"),
        Models.CustomerCareStatus.NeedFeedback => ("Cần phản hồi", "CINFB", "danger"),
        Models.CustomerCareStatus.Rejected => ("Không liên hệ", "REJ", "secondary"),
        _ => (s.ToString(), "", "secondary")
    };

    public static string RatingText(int? r) => r switch
    {
        1 => "⭐⭐⭐⭐ Rất tốt / Rất hài lòng",
        2 => "⭐⭐⭐ Tốt / Hài lòng",
        3 => "⭐⭐ Bình thường",
        4 => "⭐ Chưa tốt / Không hài lòng",
        _ => "—"
    };

    public static string FacilityText(int? f) => f switch
    {
        1 => "Rất tốt, tiện nghi",
        2 => "Đạt yêu cầu cơ bản",
        3 => "Cần nâng cấp / cải thiện",
        _ => "—"
    };

    public static (string text, string code, string css) PaymentStatus(PaymentStatus s) => s switch
    {
        Models.PaymentStatus.Draft => ("Lập phiếu", "DRAFT", "warning"),
        Models.PaymentStatus.Completed => ("Đã thu tiền", "COMP", "success"),
        Models.PaymentStatus.Cancelled => ("Đã hủy", "CANC", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string icon, string css) PaymentMethod(PaymentMethod m) => m switch
    {
        Models.PaymentMethod.Cash => ("Tiền mặt", "bi-cash", "success"),
        Models.PaymentMethod.BankTransfer => ("Chuyển khoản", "bi-bank", "primary"),
        Models.PaymentMethod.PosCard => ("Quẹt thẻ POS", "bi-credit-card", "info"),
        Models.PaymentMethod.Insurance => ("Bảo hiểm bảo lãnh", "bi-shield-check", "warning"),
        Models.PaymentMethod.Internal => ("Nội bộ hỗ trợ", "bi-building", "secondary"),
        _ => (m.ToString(), "bi-cash", "secondary")
    };

    public static (string text, string code, string css) QuoteStatus(QuoteStatus s) => s switch
    {
        Models.QuoteStatus.Draft => ("Mới tạo", "CREA", "secondary"),
        Models.QuoteStatus.Sent => ("Đã gửi KH", "SENT", "info"),
        Models.QuoteStatus.Confirmed => ("Khách đồng ý", "CONF", "primary"),
        Models.QuoteStatus.Converted => ("Đã chuyển đổi", "CONV", "success"),
        Models.QuoteStatus.Rejected => ("Đã hủy", "REJ", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string css) PackageScope(bool isPublic) =>
        isPublic ? ("Toàn hệ thống", "primary") : ("Nội bộ đại lý", "secondary");

    public static (string text, string css) PackageActive(bool isActive) =>
        isActive ? ("Đang áp dụng", "success") : ("Tạm dừng", "secondary");

    public static (string text, string code, string css) InsuranceClaimStatus(InsuranceClaimStatus s) => s switch
    {
        Models.InsuranceClaimStatus.Draft => ("Lập hồ sơ", "DRAFT", "secondary"),
        Models.InsuranceClaimStatus.Submitted => ("Chờ duyệt BH", "SUBMITTED", "warning"),
        Models.InsuranceClaimStatus.Approved => ("Đã bảo lãnh", "APPROVED", "primary"),
        Models.InsuranceClaimStatus.Settled => ("Đã quyết toán", "SETTLED", "success"),
        Models.InsuranceClaimStatus.Rejected => ("Từ chối bồi thường", "REJECTED", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string css) InsurancePaymentType(InsurancePaymentType t) => t switch
    {
        Models.InsurancePaymentType.DirectGuarantee => ("Bảo lãnh trực tiếp", "success"),
        Models.InsurancePaymentType.CustomerReimburse => ("Khách hoàn ứng", "info"),
        _ => (t.ToString(), "secondary")
    };

    public static (string text, string code, string css) CampaignStatus(CampaignMarketingStatus s) => s switch
    {
        Models.CampaignMarketingStatus.Draft => ("Dự thảo", "DRAFT", "secondary"),
        Models.CampaignMarketingStatus.Active => ("Đang chạy", "ACTIVE", "success"),
        Models.CampaignMarketingStatus.Finished => ("Kết thúc", "FNS", "info"),
        Models.CampaignMarketingStatus.Cancelled => ("Đã hủy", "CANC", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string css) CampaignScope(string? model, string? plate, string? vin)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(model)) parts.Add($"Model: {model}");
        if (!string.IsNullOrWhiteSpace(plate)) parts.Add($"Biển: {plate}*");
        if (!string.IsNullOrWhiteSpace(vin)) parts.Add($"VIN: *{vin}*");
        if (parts.Count == 0) return ("Toàn bộ xe Hyundai", "primary");
        return (string.Join(" · ", parts), "info");
    }

    public static (string text, string css) MaceType(MaceType t) => t switch
    {
        Models.MaceType.Advisor => ("CVDV chỉ định", "info"),
        Models.MaceType.Standard6Months => ("Định kỳ 6 tháng", "primary"),
        Models.MaceType.FrequencyFvx => ("Tần suất vào xưởng (Fvx)", "success"),
        _ => (t.ToString(), "secondary")
    };

    public static (string text, string code, string css) CustomerCareMaceStatus(CustomerCareMaceStatus s) => s switch
    {
        Models.CustomerCareMaceStatus.Pending => ("Chờ gọi nhắc", "PEND", "warning"),
        Models.CustomerCareMaceStatus.Contacted => ("Đã liên hệ", "CONT", "info"),
        Models.CustomerCareMaceStatus.NotContacted => ("Không nghe máy", "NOCONT", "secondary"),
        Models.CustomerCareMaceStatus.Booked => ("Đã chốt hẹn", "BOOKED", "success"),
        Models.CustomerCareMaceStatus.Cancelled => ("Từ chối / Hủy", "CANC", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static string MoneyToWords(decimal total)
    {
        if (total <= 0) return "Không đồng";
        long number = (long)Math.Round(total);
        string[] units = ["", "nghìn", "triệu", "tỷ", "nghìn tỷ", "triệu tỷ"];
        string[] digits = ["không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín"];

        string ReadThreeDigits(int n, bool full)
        {
            int h = n / 100, t = (n % 100) / 10, u = n % 10;
            if (n == 0) return full ? "không trăm" : "";
            var sb = new System.Text.StringBuilder();
            if (h > 0 || full) sb.Append(digits[h] + " trăm ");
            if (t > 1) { sb.Append(digits[t] + " mươi "); if (u == 1) sb.Append("mốt"); else if (u == 5) sb.Append("lăm"); else if (u > 0) sb.Append(digits[u]); }
            else if (t == 1) { sb.Append("mười "); if (u == 5) sb.Append("lăm"); else if (u > 0) sb.Append(digits[u]); }
            else if (u > 0) { if (h > 0 || full) sb.Append("lẻ "); sb.Append(digits[u]); }
            return sb.ToString().Trim();
        }

        var groups = new List<int>();
        while (number > 0) { groups.Add((int)(number % 1000)); number /= 1000; }
        var result = new List<string>();
        for (int i = groups.Count - 1; i >= 0; i--)
        {
            if (groups[i] == 0) continue;
            var text = ReadThreeDigits(groups[i], i < groups.Count - 1);
            if (!string.IsNullOrWhiteSpace(text))
            {
                result.Add(text + (i > 0 ? " " + units[i] : ""));
            }
        }
        var str = string.Join(" ", result).Trim() + " đồng chẵn";
        return char.ToUpper(str[0]) + str.Substring(1);
    }

    public static (string text, string code, string css) StockAdjStatus(StockAdjStatus s) => s switch
    {
        Models.StockAdjStatus.Pending => ("Chờ kiểm kê", "PEND", "warning"),
        Models.StockAdjStatus.Executing => ("Đang kiểm đếm", "EXEC", "info"),
        Models.StockAdjStatus.Finished => ("Đã hoàn tất", "FNS", "success"),
        Models.StockAdjStatus.Rejected => ("Đã hủy", "REJ", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string code, string css) StockAdjType(StockAdjType t) => t switch
    {
        Models.StockAdjType.CountBalance => ("Kiểm kê cân đối kho", "KK", "primary"),
        Models.StockAdjType.LocationTransfer => ("Điều chuyển vị trí kệ", "DC", "info"),
        Models.StockAdjType.DamageScrap => ("Hao hụt / Hư hỏng", "HH", "danger"),
        _ => (t.ToString(), "", "secondary")
    };

    public static (string text, string code, string css) BulletinStatus(BulletinStatus s) => s switch
    {
        Models.BulletinStatus.Draft => ("Dự thảo", "DRAFT", "secondary"),
        Models.BulletinStatus.Active => ("Có hiệu lực", "ACTIVE", "success"),
        Models.BulletinStatus.Finished => ("Đã kết thúc", "FNS", "info"),
        Models.BulletinStatus.Cancelled => ("Đã hủy", "CANC", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string code, string css) BulletinVinStatus(BulletinVinStatus s) => s switch
    {
        Models.BulletinVinStatus.Pending => ("Chưa xử lý", "PEND", "warning"),
        Models.BulletinVinStatus.Completed => ("Đã hoàn thành", "FNS", "success"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string code, string css) PdiRequestStatus(PdiRequestStatus s) => s switch
    {
        Models.PdiRequestStatus.Draft => ("Dự thảo", "DRAFT", "secondary"),
        Models.PdiRequestStatus.Pending => ("Chờ tiếp nhận", "PEND", "warning"),
        Models.PdiRequestStatus.Approved => ("Đang thực hiện", "APPR", "info"),
        Models.PdiRequestStatus.Completed => ("Đã hoàn tất PDI", "COMP", "success"),
        Models.PdiRequestStatus.Cancelled => ("Đã hủy", "CANC", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string code, string css) PdiItemStatus(PdiItemStatus s) => s switch
    {
        Models.PdiItemStatus.Pending => ("Chờ kiểm tra", "PEND", "warning"),
        Models.PdiItemStatus.InProgress => ("Đang kiểm tra", "IN_PROG", "info"),
        Models.PdiItemStatus.Passed => ("Đạt chuẩn (Sẵn sàng giao)", "PASSED", "success"),
        Models.PdiItemStatus.Failed => ("Không đạt / Cần sửa", "FAIL", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string code, string css) DMSOrderComplainStatus(DMSOrderComplainStatus s) => s switch
    {
        Models.DMSOrderComplainStatus.Pending => ("Mới tạo (Chờ gửi)", "P", "warning"),
        Models.DMSOrderComplainStatus.Sent => ("Đã gửi NCC TST", "A", "info"),
        Models.DMSOrderComplainStatus.Finished => ("Đã giải quyết", "F", "success"),
        Models.DMSOrderComplainStatus.Cancelled => ("Đã hủy", "C", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string code, string css) TSTOrderComplainStatus(TSTOrderComplainStatus s) => s switch
    {
        Models.TSTOrderComplainStatus.Processing => ("Chờ tiếp nhận", "1", "warning"),
        Models.TSTOrderComplainStatus.UnderReview => ("Đang thẩm định", "15", "info"),
        Models.TSTOrderComplainStatus.Approved => ("Chấp thuận bồi thường", "31", "success"),
        Models.TSTOrderComplainStatus.Rejected => ("Từ chối bồi thường", "21", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string icon, string css) OrderComplainType(OrderComplainType t) => t switch
    {
        Models.OrderComplainType.DamagedInTransit => ("Vỡ móp / Hư hại vận chuyển", "bi-truck", "danger"),
        Models.OrderComplainType.WrongPart => ("Sai mã / Nhầm quy cách", "bi-shuffle", "warning"),
        Models.OrderComplainType.Shortage => ("Thiếu hụt số lượng", "bi-dash-circle", "info"),
        Models.OrderComplainType.QualityDefect => ("Lỗi chất lượng xuất xưởng", "bi-wrench-adjustable", "danger"),
        Models.OrderComplainType.PackagingBreach => ("Bao bì rách / Hỏng tem niêm", "bi-box-seam", "secondary"),
        _ => (t.ToString(), "bi-question-circle", "secondary")
    };

    public static (string text, string css) ComplainSolution(ComplainSolution s) => s switch
    {
        Models.ComplainSolution.ReplaceNew => ("Đổi mới phụ tùng 1:1", "success"),
        Models.ComplainSolution.CreditDebt => ("Bồi hoàn trừ công nợ", "primary"),
        Models.ComplainSolution.ReturnRefund => ("Thu hồi hoàn tiền", "info"),
        Models.ComplainSolution.RejectClaim => ("Từ chối bồi thường", "danger"),
        _ => (s.ToString(), "secondary")
    };

    public static (string text, string code, string css) TechnicalLibraryType(TechnicalLibraryType t) => t switch
    {
        Models.TechnicalLibraryType.Normal => ("Cẩm nang chuẩn", "NORM", "info"),
        Models.TechnicalLibraryType.ReRepair => ("Phản tu / Pan khó", "RREP", "danger"),
        _ => (t.ToString(), "", "secondary")
    };

    public static (string text, string icon, string css) TechnicalLibraryReRepairType(TechnicalLibraryReRepairType t) => t switch
    {
        Models.TechnicalLibraryReRepairType.Engine => ("Động cơ & Nhiên liệu", "bi-fuel-pump", "danger"),
        Models.TechnicalLibraryReRepairType.Transmission => ("Hộp số & Truyền động", "bi-gear-wide-connected", "primary"),
        Models.TechnicalLibraryReRepairType.Electrical => ("Hệ thống Điện & Cảm biến", "bi-lightning-charge", "warning"),
        Models.TechnicalLibraryReRepairType.Chassis => ("Khung gầm & Treo lái", "bi-diagram-3", "secondary"),
        Models.TechnicalLibraryReRepairType.BrakeADAS => ("Phanh an toàn & ADAS", "bi-shield-check", "success"),
        Models.TechnicalLibraryReRepairType.AirConditioning => ("Điều hòa nhiệt độ (AC)", "bi-snow", "info"),
        Models.TechnicalLibraryReRepairType.BodyPaint => ("Thân vỏ & Sơn", "bi-palette", "dark"),
        _ => (t.ToString(), "bi-tools", "secondary")
    };

    public static (string text, string code, string css) TechnicalLibraryStatus(bool isActive) =>
        isActive ? ("Đã duyệt ban hành", "ACTIVE", "success") : ("Chờ thẩm định HQ", "PEND", "warning");

    public static (string text, string code, string css) ServiceROType(ServiceROType t) => t switch
    {
        Models.ServiceROType.BDD => ("Bảo dưỡng định kỳ", "BDD", "primary"),
        Models.ServiceROType.SCC => ("Sửa chữa chung", "SCC", "info"),
        Models.ServiceROType.SCD => ("Đồng sơn sấy", "SCD", "warning"),
        Models.ServiceROType.SCS => ("Dịch vụ nhanh", "SCS", "success"),
        Models.ServiceROType.PDI => ("Kiểm tra PDI", "PDI", "secondary"),
        Models.ServiceROType.SPK => ("Phụ kiện & Chăm sóc", "SPK", "danger"),
        _ => (t.ToString(), "", "secondary")
    };

    public static (string text, string css) ServiceItemActive(bool isActive) =>
        isActive ? ("Đang áp dụng", "success") : ("Tạm dừng", "secondary");

    public static (string text, string code, string css) CarModelSegment(CarModelSegment s) => s switch
    {
        Models.CarModelSegment.Sedan => ("Sedan", "SED", "primary"),
        Models.CarModelSegment.SUV => ("SUV / CUV", "SUV", "info"),
        Models.CarModelSegment.MPV => ("MPV / 7 chỗ", "MPV", "warning"),
        Models.CarModelSegment.Commercial => ("Thương mại / Bán tải", "COM", "secondary"),
        Models.CarModelSegment.EV => ("Xe điện / Hybrid", "EV", "success"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string css) CarModelActive(bool isActive) =>
        isActive ? ("Đang áp dụng", "success") : ("Tạm dừng", "secondary");

    public static (string text, string css) BomActive(bool isActive) =>
        isActive ? ("Đang áp dụng", "success") : ("Tạm dừng", "secondary");

    public static (string text, string code, string css) SupplierPaymentStatus(SupplierPaymentStatus s) => s switch
    {
        Models.SupplierPaymentStatus.Pending => ("Chờ duyệt xuất", "PEND", "warning"),
        Models.SupplierPaymentStatus.Approved => ("Đã xuất trả kho", "APPR", "success"),
        Models.SupplierPaymentStatus.Cancelled => ("Đã hủy", "CANC", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string icon, string css) SupplierPaymentType(SupplierPaymentType t) => t switch
    {
        Models.SupplierPaymentType.ReturnDefective => ("Hàng lỗi / Hư hại", "bi-exclamation-triangle", "danger"),
        Models.SupplierPaymentType.ReturnSurplus => ("Thừa / Sai quy cách", "bi-shuffle", "warning"),
        Models.SupplierPaymentType.RecallWarranty => ("Thu hồi bảo hành HTC", "bi-shield-check", "primary"),
        Models.SupplierPaymentType.ConsignmentReturn => ("Trả hàng ký gửi / Tồn chậm", "bi-arrow-left-right", "info"),
        _ => (t.ToString(), "bi-box-seam", "secondary")
    };

    public static (string text, string code, string css) StockOutOrderStatus(StockOutOrderStatus s) => s switch
    {
        Models.StockOutOrderStatus.Pending => ("Chờ xuất kho", "PEND", "warning"),
        Models.StockOutOrderStatus.Approved => ("Đã duyệt / Sẵn sàng", "APPR", "info"),
        Models.StockOutOrderStatus.Completed => ("Đã xuất hoàn tất", "FNS", "success"),
        Models.StockOutOrderStatus.Rejected => ("Đã từ chối / Hủy", "REJ", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string icon, string css) StockOutOrderPriority(StockOutOrderPriority p) => p switch
    {
        Models.StockOutOrderPriority.Normal => ("Bình thường", "bi-check2", "secondary"),
        Models.StockOutOrderPriority.Urgent => ("Khẩn cấp", "bi-exclamation-circle", "warning"),
        Models.StockOutOrderPriority.Emergency => ("Hỏa tốc (VOR)", "bi-lightning-fill", "danger"),
        _ => (p.ToString(), "bi-info-circle", "secondary")
    };

    public static (string text, string code, string css) PartOOStatus(PartOOStatus s) => s switch
    {
        Models.PartOOStatus.Owed => ("Còn nợ khách", "OWED", "warning"),
        Models.PartOOStatus.Arrived => ("Hàng đã về kho", "ARRV", "info"),
        Models.PartOOStatus.Completed => ("Đã trả đủ", "FNS", "success"),
        Models.PartOOStatus.Cancelled => ("Đã hủy", "CANC", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string code, string css) CusDebitStatus(CusDebitStatus s) => s switch
    {
        Models.CusDebitStatus.Active => ("Còn nợ", "ACTIVE", "warning"),
        Models.CusDebitStatus.Cleared => ("Đã tất toán", "CLEARED", "success"),
        Models.CusDebitStatus.Cancelled => ("Đã hủy", "CANCELLED", "secondary"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string icon, string css) CusDebitType(CusDebitType t) => t switch
    {
        Models.CusDebitType.RO => ("Lệnh sửa chữa (RO)", "bi-clipboard2-pulse", "primary"),
        Models.CusDebitType.Part => ("Phụ tùng / Bán lẻ", "bi-box-seam", "info"),
        Models.CusDebitType.Other => ("Dịch vụ khác", "bi-three-dots", "secondary"),
        _ => (t.ToString(), "bi-tag", "secondary")
    };

    public static (string text, string code, string css) SupplierDebitStatus(SupplierDebitStatus s) => s switch
    {
        Models.SupplierDebitStatus.Active => ("Còn nợ NCC", "ACTIVE", "warning"),
        Models.SupplierDebitStatus.Cleared => ("Đã tất toán", "CLEARED", "success"),
        Models.SupplierDebitStatus.Cancelled => ("Đã hủy", "CANCELLED", "secondary"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string icon, string css) SupplierDebitType(SupplierDebitType t) => t switch
    {
        Models.SupplierDebitType.StockIn => ("Nhập kho phụ tùng", "bi-box-arrow-in-down", "primary"),
        Models.SupplierDebitType.Shipping => ("Cước vận chuyển", "bi-truck", "info"),
        Models.SupplierDebitType.EmergencyOrder => ("Đơn hàng khẩn (VOR)", "bi-lightning-fill", "danger"),
        Models.SupplierDebitType.Other => ("Phát sinh khác", "bi-three-dots", "secondary"),
        _ => (t.ToString(), "bi-tag", "secondary")
    };

    public static (string text, string code, string css) InsuranceDebitStatus(InsuranceDebitStatus s) => s switch
    {
        Models.InsuranceDebitStatus.Active => ("Còn nợ bảo hiểm", "ACTIVE", "warning"),
        Models.InsuranceDebitStatus.Cleared => ("Đã tất toán", "CLEARED", "success"),
        Models.InsuranceDebitStatus.Cancelled => ("Đã hủy nợ", "CANCELLED", "secondary"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string icon, string css) InsuranceDebitType(InsuranceDebitType t) => t switch
    {
        Models.InsuranceDebitType.RO => ("Bồi thường theo RO", "bi-wrench-adjustable-circle", "primary"),
        Models.InsuranceDebitType.Claim => ("Hồ sơ bồi thường BH", "bi-shield-check", "info"),
        Models.InsuranceDebitType.DirectAdjustment => ("Điều chỉnh bổ sung", "bi-sliders", "secondary"),
        _ => (t.ToString(), "bi-tag", "secondary")
    };

    public static (string text, string code, string css) InsuranceDebitPaymentStatus(InsuranceDebitPaymentStatus s) => s switch
    {
        Models.InsuranceDebitPaymentStatus.Confirmed => ("Đã xác nhận", "CONFIRMED", "success"),
        Models.InsuranceDebitPaymentStatus.Cancelled => ("Đã hủy", "CANCELLED", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string css) DealerBadge(string dealerCode) => dealerCode switch
    {
        "HTC-CG" => ("Hyundai Cầu Giấy", "primary"),
        "HTC-PDV" => ("Hyundai Phạm Văn Đồng", "info"),
        "HTC-DD" => ("Hyundai Đông Đô", "success"),
        "HTC-HD" => ("Hyundai Hà Đông", "warning"),
        "HTC-SG" => ("Hyundai Sài Gòn 1S", "danger"),
        "HTC-MAIN" => ("Đại lý hiện tại", "dark"),
        _ => (dealerCode, "secondary")
    };

    public static (string text, string css) ClaimBadge(bool flagClaim, string? claimNo) =>
        flagClaim
            ? (!string.IsNullOrWhiteSpace(claimNo) ? ($"Bảo hành: {claimNo}", "warning") : ("Hồ sơ bảo hành", "warning"))
            : ("Dịch vụ thường", "secondary");

    public static (string text, string css) CustomerGroupStatus(bool isActive) =>
        isActive ? ("Đang hoạt động", "success") : ("Tạm dừng", "secondary");

    public static (string text, string css, string icon) CreditStatus(decimal creditLimit, decimal currentDebt)
    {
        if (creditLimit <= 0) return ("Không giới hạn", "info", "bi-infinity");
        if (currentDebt > creditLimit) return ("Vượt hạn mức", "danger", "bi-exclamation-octagon-fill");
        if (currentDebt >= creditLimit * 0.8m) return ("Sắp chạm mức", "warning", "bi-exclamation-triangle-fill");
        return ("An toàn", "success", "bi-shield-check");
    }

    public static (string text, string code, string css) DMSReqPartPriceStatus(DMSReqPartPriceStatus s) => s switch
    {
        Models.DMSReqPartPriceStatus.Draft => ("Mới tạo (Chờ gửi)", "DRAFT", "secondary"),
        Models.DMSReqPartPriceStatus.Sent => ("Đã gửi NCC TST", "SENT", "warning"),
        Models.DMSReqPartPriceStatus.Responded => ("NCC đã báo giá", "RESP", "info"),
        Models.DMSReqPartPriceStatus.Approved => ("Đại lý đã duyệt giá", "APPR", "success"),
        Models.DMSReqPartPriceStatus.Cancelled => ("Đã hủy đề nghị", "CANC", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string code, string css) TSTReqPartPriceStatus(TSTReqPartPriceStatus s) => s switch
    {
        Models.TSTReqPartPriceStatus.Pending => ("Chờ tiếp nhận", "PEND", "secondary"),
        Models.TSTReqPartPriceStatus.Processing => ("Đang thẩm định giá", "PROC", "warning"),
        Models.TSTReqPartPriceStatus.Priced => ("Đã cấp báo giá", "PRICED", "success"),
        Models.TSTReqPartPriceStatus.Rejected => ("Từ chối cấp giá", "REJ", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string icon, string css) PartPriceDeliveryForm(PartPriceDeliveryForm f) => f switch
    {
        Models.PartPriceDeliveryForm.VOR => ("Khẩn cấp xe nằm xưởng (VOR)", "bi-lightning-fill", "danger"),
        Models.PartPriceDeliveryForm.Regular => ("Đặt hàng định kỳ (ĐHĐK)", "bi-calendar-check", "primary"),
        Models.PartPriceDeliveryForm.Air => ("Đường hàng không hỏa tốc (Bay)", "bi-airplane", "info"),
        Models.PartPriceDeliveryForm.Sea => ("Đường biển container (Tàu)", "bi-water", "secondary"),
        _ => (f.ToString(), "bi-box", "secondary")
    };

    public static (string text, string code, string css) ReqPartPriceLineStatus(ReqPartPriceLineStatus s) => s switch
    {
        Models.ReqPartPriceLineStatus.Pending => ("Chờ giá", "PEND", "warning"),
        Models.ReqPartPriceLineStatus.Priced => ("Đã có giá", "PRICED", "success"),
        Models.ReqPartPriceLineStatus.Rejected => ("Hết hàng / Từ chối", "REJ", "danger"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string code, string css) CustomerCare72hStatus(CustomerCare72hStatus s) => s switch
    {
        Models.CustomerCare72hStatus.Pending => ("Chờ liên hệ 72h", "PEND", "warning"),
        Models.CustomerCare72hStatus.ContactedSatisfied => ("Đã liên hệ - Hài lòng", "CIFB", "success"),
        Models.CustomerCare72hStatus.NeedFeedback => ("Cần phản hồi - Phản tu", "CINFB", "danger"),
        Models.CustomerCare72hStatus.Rejected => ("Khách từ chối / Bận", "REJ", "secondary"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string css) FirftBadge(bool? isFirft) => isFirft switch
    {
        true => ("Đúng ngay lần đầu (FIRFT)", "success"),
        false => ("Có sự cố tái phát", "danger"),
        _ => ("Chưa xác định", "secondary")
    };

    public static string SatisfactionStars(int? stars) => stars switch
    {
        5 => "★★★★★ (Rất hài lòng)",
        4 => "★★★★☆ (Hài lòng)",
        3 => "★★★☆☆ (Bình thường)",
        2 => "★★☆☆☆ (Không hài lòng)",
        1 => "★☆☆☆☆ (Rất thất vọng)",
        _ => "Chưa đánh giá"
    };

    public static (string text, string code, string css) CustomerCareBirthdayStatus(CustomerCareBirthdayStatus s) => s switch
    {
        Models.CustomerCareBirthdayStatus.Pending => ("Chưa liên hệ", "PEND", "warning"),
        Models.CustomerCareBirthdayStatus.Contacted => ("Đã liên hệ", "CONT", "success"),
        Models.CustomerCareBirthdayStatus.NotContacted => ("Không liên hệ được", "NOCONT", "secondary"),
        _ => (s.ToString(), "", "secondary")
    };

    public static (string text, string icon, string css) BirthdayContactChannel(BirthdayContactChannel c) => c switch
    {
        Models.BirthdayContactChannel.Call => ("Gọi điện thoại", "bi-telephone-fill", "primary"),
        Models.BirthdayContactChannel.SMS => ("Tin nhắn SMS", "bi-chat-dots-fill", "info"),
        Models.BirthdayContactChannel.Zalo => ("Zalo ZNS", "bi-chat-heart-fill", "success"),
        Models.BirthdayContactChannel.InPerson => ("Tại xưởng dịch vụ", "bi-person-check-fill", "secondary"),
        _ => (c.ToString(), "bi-chat", "light")
    };

    public static (string text, string icon, string css) WarrantyLaborGroup(WarrantyLaborGroup g) => g switch
    {
        Models.WarrantyLaborGroup.Engine => ("Động cơ & Nhiên liệu", "bi-fuel-pump", "danger"),
        Models.WarrantyLaborGroup.Transmission => ("Hộp số & Truyền động", "bi-gear-wide-connected", "primary"),
        Models.WarrantyLaborGroup.Electrical => ("Điện & Điện tử", "bi-lightning-charge", "warning"),
        Models.WarrantyLaborGroup.BrakeSteering => ("Phanh & Lái", "bi-slash-circle", "info"),
        Models.WarrantyLaborGroup.ChassisSuspension => ("Khung gầm & Treo", "bi-shield-shaded", "secondary"),
        Models.WarrantyLaborGroup.BodyInterior => ("Thân vỏ & Nội thất", "bi-car-front", "dark"),
        Models.WarrantyLaborGroup.SoftwareECU => ("Lập trình ECU & Phần mềm", "bi-cpu", "success"),
        _ => (g.ToString(), "bi-tools", "secondary")
    };

    public static (string text, string code, string css) WarrantyCoverageType(WarrantyCoverageType t) => t switch
    {
        Models.WarrantyCoverageType.NewCar => ("Bảo hành xe mới", "W1", "primary"),
        Models.WarrantyCoverageType.GenuinePart => ("Bảo hành phụ tùng", "W2", "info"),
        Models.WarrantyCoverageType.Goodwill => ("Bảo hành thiện chí", "W3", "warning"),
        Models.WarrantyCoverageType.CampaignRecall => ("Chiến dịch / Triệu hồi", "W4", "danger"),
        Models.WarrantyCoverageType.ExtendedWarranty => ("Bảo hành gia hạn", "W5", "success"),
        _ => (t.ToString(), "", "secondary")
    };

    public static (string text, string css) CusPersonType(string? personType) => personType switch
    {
        "Organization" => ("Tổ chức / Doanh nghiệp", "info"),
        "Personal" => ("Cá nhân", "secondary"),
        _ => (personType ?? "Cá nhân", "secondary")
    };

    /// <summary>Nhãn hệ số giá: < 1 giảm giá, = 1 giá gốc, > 1 tăng giá.</summary>
    public static (string text, string css) FactorBadge(decimal factor) => factor switch
    {
        < 1m => ($"Giảm giá (×{factor:0.####})", "success"),
        > 1m => ($"Tăng giá (×{factor:0.####})", "warning"),
        _ => ("Giá gốc (×1)", "secondary")
    };

    /// <summary>Biểu tượng hành động cho một dòng nhật ký thao tác RO (Ser_ROHistory).</summary>
    public static (string icon, string css) HistoryAction(ROStatus s) => s switch
    {
        ROStatus.Created => ("bi-plus-circle", "secondary"),
        ROStatus.Printed => ("bi-printer", "secondary"),
        ROStatus.Wait4Part => ("bi-hourglass-split", "warning"),
        ROStatus.HasPart => ("bi-box-seam", "info"),
        ROStatus.HasRO => ("bi-clipboard2-check", "info"),
        ROStatus.InGarage => ("bi-wrench-adjustable", "primary"),
        ROStatus.Repaired => ("bi-check2-circle", "primary"),
        ROStatus.CheckEnd => ("bi-shield-check", "info"),
        ROStatus.Paid => ("bi-cash-coin", "success"),
        ROStatus.Finished => ("bi-flag-fill", "success"),
        ROStatus.Rejected => ("bi-x-octagon", "danger"),
        ROStatus.NotResponding => ("bi-telephone-x", "dark"),
        _ => ("bi-dot", "secondary")
    };
}

