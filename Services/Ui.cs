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
}
