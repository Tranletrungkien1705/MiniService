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
}
