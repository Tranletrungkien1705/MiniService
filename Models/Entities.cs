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

public class Customer : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    // Bổ sung theo cột thật của Ser_Customer (CarService.Main): địa chỉ, MST, mã đại lý quản lý.
    public string? Address { get; set; }
    public string? TaxCode { get; set; }        // TaxCode
    public string? DealerCode { get; set; }     // DealerCode — đại lý quản lý KH
    public List<Car> Cars { get; set; } = [];
}

public class Car : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Plate { get; set; } = "";      // biển số (PlateNo)
    public string? Vin { get; set; }             // FrameNo
    public string Model { get; set; } = "";
    public int Year { get; set; }                // ProductYear
    // Bổ sung theo cột thật Ser_Car
    public string? EngineNo { get; set; }
    public string? Color { get; set; }           // ColorCode
    public int CurrentKm { get; set; }
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
    public string? CustomerRequest { get; set; }  // yêu cầu của khách hàng
    public string? ServiceAdvisor { get; set; }   // cố vấn dịch vụ tiếp nhận
    public string? Technician { get; set; }       // thợ phụ trách
    public DateTime? ExpectedDelivery { get; set; } // ngày hẹn trả xe
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? IntakeAt { get; set; }        // xe vào xưởng (INGA)
    public DateTime? FinishedAt { get; set; }      // giao xe (FNS)

    // Tích hợp HĐĐT: khi quyết toán (PAID) tự đẩy hóa đơn sang MiniTVAN → lên cơ quan thuế.
    public string? EInvoiceCode { get; set; }      // mã tra cứu CQT cấp
    public string? EInvoiceStatus { get; set; }    // Accepted / Rejected / Error
    public string? EInvoiceError { get; set; }
    public DateTime? EInvoiceAt { get; set; }
    // Bồi thường bảo hiểm (tích hợp MiniInsurance) khi xe còn BH sửa sau tai nạn
    public string? InsuranceClaimCode { get; set; }
    public string? InsuranceClaimStatus { get; set; }

    public Car Car { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public List<RepairLine> Lines { get; set; } = [];

    public decimal Total => Lines.Sum(l => l.Amount);
    public decimal LaborTotal => Lines.Where(l => l.Type == LineType.Labor).Sum(l => l.Amount);
    public decimal PartTotal => Lines.Where(l => l.Type == LineType.Part).Sum(l => l.Amount);
}

public class RepairLine : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int ROId { get; set; }
    public LineType Type { get; set; }
    public string Name { get; set; } = "";
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public int? PartId { get; set; }              // dòng phụ tùng gắn mã kho → xuất kho
    public decimal Amount => Quantity * UnitPrice;
    public RepairOrder RO { get; set; } = null!;
}

// ===== TỒN KHO / XUẤT KHO (Ser_Car_Inv_*) =====
/// <summary>Phụ tùng (master) + tồn kho hiện có.</summary>
public class Part : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "cái";
    public decimal Price { get; set; }            // giá bán
    public int OnHand { get; set; }               // tồn kho
    public int MinStock { get; set; } = 5;        // định mức cảnh báo
    public bool LowStock => OnHand <= MinStock;
    public decimal StockValue => OnHand * Price;
}

/// <summary>Phiếu xuất kho — giảm tồn, gắn với RO nếu xuất cho sửa chữa.</summary>
public class StockOut : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";
    public int PartId { get; set; }
    public string PartName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int? ROId { get; set; }
    public string? ROCode { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public decimal Amount => Quantity * UnitPrice;
}

/// <summary>Thanh toán / quyết toán RO.</summary>
public enum PayMethod { Cash = 0, Transfer = 1, Card = 2 }
public class Payment : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int ROId { get; set; }
    public string ROCode { get; set; } = "";
    public decimal Amount { get; set; }
    public PayMethod Method { get; set; }
    public string? Note { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.Now;
}
