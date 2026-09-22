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
}
