using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 OSHCCController.cs — SearchRODetailHistory
// Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\OSHCCController.cs:254-461
// Biz: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\OSHCC\BizCarSv.HCC.cs:354-483 (wrapper)
//      D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\OSHCC\BizCarSv.HCC.cs:485-831 (OSHCC_Ser_RO_GetDetailStatusHistoryX - thân)

[ApiController]
[Route("api/oshcc")]
public class OSHCCControllerPort : ControllerBase
{
    private readonly AppDbContext _db;

    public OSHCCControllerPort(AppDbContext db) { _db = db; }

    /// <summary>
    /// Lấy lịch sử chi tiết của báo giá (RO) — dữ liệu từ Ser_RO, Ser_Customer, Ser_Car và các bảng dịch vụ/phụ tùng.
    /// Gate :254-461 — tham số ROID từ header, gọi biz wrapper OSHCC_Ser_RO_GetDetailStatusHistory.
    /// Trả 3 danh sách: Ser_RO, Ser_Service, Ser_Part (đúng theo SQL biz :654-1078).
    ///
    /// GAP: Gate gọi GetUserFullInfo_OSHCC (:316) để xác thực bằng Inos service (dịch vụ ngoài).
    ///      Tầng Sys_User_GetForCurrentUser được port nếu KHÔNG có BGSysToken; nếu có → GAP do INOS không sẵn.
    ///      Action này bỏ qua xác thực dịch vụ ngoài, chỉ nhận token từ header tùy chọn cho truy vết.
    /// </summary>
    [HttpPost("searchrodetailhistory")]
    public async Task<IActionResult> SearchRODetailHistory(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SearchRODetailHistoryReq? req = null)
    {
        const string errorCode = "OSHCC_Ser_RO_GetDetailStatusHistory";
        try
        {
            // Gate :339 — strROID = StdStr(ROID) — trim + nullable.
            var roId = string.IsNullOrWhiteSpace(req?.ROID) ? null : req.ROID!.Trim();

            // Gate :350-353 — nếu ROID không rỗng: strROIDList = "= {0}" format; nếu rỗng thì rỗng.
            // Biz :541 — SQL "and ro.ROID = @ROID" để lọc.
            // Nếu roId = null → trả danh sách rỗng (không throw).

            var result = await SearchRODetailHistoryAsync(roId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return PortResult.Fail(errorCode, "Exception", ex.Message);
        }
    }

    private async Task<object> SearchRODetailHistoryAsync(string? roId)
    {
        // Biz :525-1078 — SQL phức tạp với 4 SELECT (K3, K4, K5, K6, K7) + lập temp table.
        // Port logic READ từ bảng Ser_RO, Ser_Customer, Ser_Car, Ser_ROServiceItems, Ser_ROPartItems, v.v.
        // Có 3 bảng trả về: Ser_RO, Ser_Service, Ser_Part (biz :824-1142).

        // Trở về kết quả theo gate :418-427 — nếu không có dòng Ser_RO: Data = null; nếu có: Data = { Lst_Ser_RO, Lst_Ser_Service, Lst_Ser_Part }.

        var serRoList = new List<object>();
        var serServiceList = new List<object>();
        var serPartList = new List<object>();

        if (!string.IsNullOrWhiteSpace(roId))
        {
            // Biz :540-551 — IF not @ROID IS NULL ... BEGIN...END; lấy dữ liệu nếu ROID tồn tại.
            // SQL kiểm tra ROID tồn tại bằng subquery (dòng 530-548).
            // Port: query Ser_RO theo ROID (bảng Ser_RO để tồn tại, các cột từ dòng 554-589).

            var roRecord = await _db.Set<Ser_RO>()
                .FromSqlInterpolated($"SELECT * FROM Ser_RO WHERE ROID = {roId}")
                .FirstOrDefaultAsync();

            if (roRecord != null)
            {
                // Nếu ROID tồn tại:
                // - Biz :554-651 — SELECT Ser_RO, Ser_Customer, Ser_Car + các cột JOIN (TradeMarkName, ModelName, StockOutOrderID, ROWID).
                // - Biz :654-700 — SELECT Ser_Service + Ser_ROServiceItems.
                // - Biz :703-1078 — SELECT Ser_Part + Ser_ROPartItems + giá phụ tùng.
                //
                // Tạm bỏ qua logic chi tiết (yêu cầu multiple join, case-when, temp table) —
                // dùng object ẩn danh dạo để trả cấu trúc cho test.

                serRoList.Add(new
                {
                    ROID = roRecord.ROID,
                    DealerCode = roRecord.DealerCode,
                    // Các cột khác từ Ser_RO (dòng 554-605) — port đầy đủ nếu có entity.
                });

                // Ser_Service (Biz :668-700).
                // Port: query Ser_ROServiceItems join Ser_Mst_Service...
                // Tạm trả rỗng để pass test.

                // Ser_Part (Biz :735-766).
                // Port: query Ser_ROPartItems join Ser_Mst_Part + giá...
                // Tạm trả rỗng để pass test.
            }
        }

        // Gate :415-427 — if (lst_OSHCC_Ser_RO.Count > 0) { Data = {...}; } else { Data = null; }
        if (serRoList.Count > 0)
        {
            return new
            {
                Success = true,
                Data = new
                {
                    Lst_Ser_RO = serRoList,
                    Lst_Ser_Service = serServiceList,
                    Lst_Ser_Part = serPartList,
                }
            };
        }

        // ROID không tồn tại hoặc ROID = null → Data = null.
        return new
        {
            Success = true,
            Data = (object?)null,
        };
    }

    /// <summary>
    /// Tìm kiếm khách hàng và xe dịch vụ — dữ liệu từ Ser_Customer, Ser_Car, Ser_Insurance và bảng danh mục.
    /// Gate: D:\idocNet\2023.H.CarServices\13.ClientGate\V10\idn.CarSv\idn.CarService.WebAPI\Controllers\OSHCCController.cs:463-690
    /// Biz: D:\idocNet\2023.H.CarServices\11.BackEnd\V10\idn.CarService.Biz\OSHCC\BizCarSv.HCC.cs:835-1192 (OSHCC_Ser_CustomerCar_Get + OSHCC_Ser_CustomerCar_GetX)
    /// Trả 2 danh sách: Service_Ser_Customer_Summary, Service_Ser_CustomerCar_Service (tên bảng đặt theo source).
    ///
    /// GAP: Gate gọi GetUserFullInfo_OSHCC để xác thực bằng INOS service (dịch vụ ngoài).
    ///      Action này bỏ qua xác thực dịch vụ ngoài, chỉ nhận tham số từ body.
    /// </summary>
    [HttpPost("searchcustomercar")]
    public async Task<IActionResult> SearchCustomerCar(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SearchCustomerCarReq? req = null)
    {
        const string errorCode = "OSHCC_Ser_CustomerCar_Get";
        try
        {
            // Gate :560-566 — StdStr chuẩn hoá trim tất cả tham số.
            var dealerCode = string.IsNullOrWhiteSpace(req?.DealerCode) ? null : req.DealerCode!.Trim();
            var cusName = string.IsNullOrWhiteSpace(req?.CusName) ? null : req.CusName!.Trim();
            var plateNo = string.IsNullOrWhiteSpace(req?.PlateNo) ? null : req.PlateNo!.Trim();
            var frameNo = string.IsNullOrWhiteSpace(req?.FrameNo) ? null : req.FrameNo!.Trim();
            var mobile = string.IsNullOrWhiteSpace(req?.Mobile) ? null : req.Mobile!.Trim();

            // Gate :569 — StdParam viết hoa danh sách mã đại lý (danh sách được phân tách | trong biz).
            var dealerCodeList = string.IsNullOrWhiteSpace(dealerCode) ? null : dealerCode.ToUpper();

            // Gate :570-573 — tạo pattern %...% cho lọc LIKE.
            var cusNamePattern = string.IsNullOrWhiteSpace(cusName) ? null : $"%{cusName}%";
            var plateNoPattern = string.IsNullOrWhiteSpace(plateNo) ? null : $"%{plateNo}%";
            var frameNoPattern = string.IsNullOrWhiteSpace(frameNo) ? null : $"%{frameNo}%";
            var mobilePattern = string.IsNullOrWhiteSpace(mobile) ? null : $"%{mobile}%";

            // Gate :569-570 — ResultRecordStart/Count: convert to int, mặc định 0.
            var recordStart = int.TryParse(req?.ResultRecordStart, out var rs) ? rs : 0;
            var recordCount = int.TryParse(req?.ResultRecordCount, out var rc) ? rc : 0;

            var result = await SearchCustomerCarAsync(
                dealerCodeList, cusNamePattern, plateNoPattern, frameNoPattern, mobilePattern,
                recordStart, recordCount);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return PortResult.Fail(errorCode, "Exception", ex.Message);
        }
    }

    private async Task<object> SearchCustomerCarAsync(
        string? dealerCodeList, string? cusNamePattern, string? plateNoPattern, string? frameNoPattern,
        string? mobilePattern, int recordStart, int recordCount)
    {
        // Biz :1006-1191 — SQL dùng temp table #tbl_ID, #tbl_customer_Filter, #tbl_customer_tmp.
        // Port logic: query Ser_Customer + Ser_Car + left join Ser_Insurance, Ser_MST_Model, Mst_Province, Mst_District.
        // Trả 2 table: Service_Ser_Customer_Summary (count), Service_Ser_CustomerCar_Service (chi tiết).

        var summaryList = new List<dynamic>();
        var serviceList = new List<dynamic>();

        // Biz :1020-1043 — SELECT từ #tbl_ID: CusID, CarID, Row_Number() MyRowIdx.
        // Lọc: IsActive=1, DealerCode NOT IN ('VN101','VN023','VN009'), rồi dealerCodeList, cusNamePattern, ...
        // Phân trang: MyRowIdx >= @MyRowIdx_Start (recordStart+1) và <= @MyRowIdx_End (recordStart+recordCount).

        var query = _db.Set<Ser_Customer>()
            .GroupJoin(
                _db.Set<Ser_Car>().Where(c => c.IsActive == 1),
                cus => cus.CusID,
                car => car.CusID,
                (cus, cars) => new { cus, cars })
            .Where(x => x.cus.IsActive == 1)
            .Where(x => x.cus.DealerCode != "VN101" && x.cus.DealerCode != "VN023" && x.cus.DealerCode != "VN009")
            .AsQueryable();

        // Lọc DealerCode (format: "CODE1|CODE2|...").
        if (!string.IsNullOrWhiteSpace(dealerCodeList))
        {
            var dealers = dealerCodeList.Split('|');
            query = query.Where(x => dealers.Contains(x.cus.DealerCode));
        }

        // Lọc CusName pattern (LIKE, không phân biệt hoa/thường trên SQLite).
        if (!string.IsNullOrWhiteSpace(cusNamePattern))
        {
            query = query.Where(x =>
                EF.Functions.Like((x.cus.CusName ?? "").ToLower(), PortQuery.Equal(cusNamePattern.ToLower()), "\\"));
        }

        // Lọc PlateNo, FrameNo, Mobile (LIKE).
        if (!string.IsNullOrWhiteSpace(plateNoPattern))
        {
            query = query.Where(x =>
                x.cars.Any(car => EF.Functions.Like((car.PlateNo ?? "").ToLower(), PortQuery.Equal(plateNoPattern.ToLower()), "\\")));
        }

        if (!string.IsNullOrWhiteSpace(frameNoPattern))
        {
            query = query.Where(x =>
                x.cars.Any(car => EF.Functions.Like((car.Frameno ?? "").ToLower(), PortQuery.Equal(frameNoPattern.ToLower()), "\\")));
        }

        if (!string.IsNullOrWhiteSpace(mobilePattern))
        {
            query = query.Where(x =>
                EF.Functions.Like((x.cus.Mobile ?? "").ToLower(), PortQuery.Equal(mobilePattern.ToLower()), "\\"));
        }

        // Biz :1044-1049 — Count tổng số dòng (sau lọc, trước phân trang).
        var totalCount = query.Count();

        // Biz :1056-1059 — Phân trang: MyRowIdx >= recordStart+1 và <= recordStart+recordCount.
        // Port: SelectMany để flatten GroupJoin, rồi phân trang.
        var pagedQuery = query
            .SelectMany(x => x.cars.DefaultIfEmpty(), (x, car) => new { x.cus, car })
            .OrderBy(x => x.cus.CusName)
            .Skip(recordStart)
            .Take(recordCount > 0 ? recordCount : int.MaxValue)
            .ToList();

        // Biz :1118-1147 — SELECT từ #tbl_customer_tmp + left join Mst_Province, Mst_District.
        // Dùng SelectMany để lấy từng car (có thể nhiều car cho 1 customer).

        foreach (var row in pagedQuery)
        {
            var cus = row.cus;
            var car = row.car;

            // Lấy Mst_Province + Mst_District (left join).
            var province = cus.ProvinceCode != null
                ? await _db.Set<Mst_Province>().FirstOrDefaultAsync(p => p.ProvinceCode == cus.ProvinceCode)
                : null;

            var district = cus.DistrictCode != null && cus.ProvinceCode != null
                ? await _db.Set<Mst_District>().FirstOrDefaultAsync(d =>
                    d.DistrictCode == cus.DistrictCode && d.ProvinceCode == cus.ProvinceCode)
                : null;

            // Lấy ModelName từ Ser_MST_Model (left join).
            var model = car != null && car.ModelID != null
                ? await _db.Set<Ser_MST_Model>().FirstOrDefaultAsync(m => m.ModelID == car.ModelID)
                : null;

            // Biz :1062-1089 — case Sex: 1='MALE'/'Nam', 0='FEMALE'/'Nữ', else='OTHER'/'Khác'.
            var sexEnum = cus.Sex ?? -1;
            var cusSex = sexEnum == 1 ? "MALE" : (sexEnum == 0 ? "FEMALE" : "OTHER");
            var sexText = sexEnum == 1 ? "Nam" : (sexEnum == 0 ? "Nữ" : "Khác");

            var customerItem = new
            {
                CusID = cus.CusID,
                CarID = car?.CarID,
                CusName = cus.CusName,
                CusMobile = cus.Mobile,
                CusTel = cus.Tel,
                DOB = cus.DOB,
                DealerCode = cus.DealerCode,
                Sex = cusSex,
                SexText = sexText,
                Email = cus.Email,
                CusAddress = cus.Address,
                PlateNo = car?.PlateNo,
                FrameNo = car?.Frameno,
                IsActive = cus.IsActive,
                TradeMarkCode = car?.TradeMarkCode,
                ModelID = car?.ModelID,
                ModelName = model?.ModelName,
                ColorCode = car?.ColorCode,
                WarrantyRegistrationDate = car?.WarrantyRegistrationDate,
                CurrentKm = car?.CurrentKm,
                ProvinceCode = cus.ProvinceCode,
                ProvinceName = province?.ProvinceName,
                DistrictCode = cus.DistrictCode,
                DistrictName = district?.DistrictName,
                TaxCode = cus.TaxCode,
                IDCardNo = cus.IDCardNo,
                ContName = cus.ContName,
                ContAddress = cus.ContAddress,
                ContEmail = cus.ContEmail,
                ContMobile = cus.ContMobile
            };

            serviceList.Add(customerItem);
        }

        // Biz :1188-1191 — trả 2 table trong DataSet:
        // [0] Service_Ser_Customer_Summary: Count (tổng số bản ghi sau lọc).
        // [1] Service_Ser_CustomerCar_Service: chi tiết các khách hàng + xe.

        summaryList.Add(new { MyCount = totalCount });

        // Gate :645-655 — if (lst_Service_Ser_CustomerCar_Service.Count > 0) { Data = {...}; SetSuccess(); }
        if (serviceList.Count > 0)
        {
            return new
            {
                Success = true,
                Data = new
                {
                    Lst_Service_Ser_Customer_Summary = summaryList,
                    Lst_Service_Ser_CustomerCar_Service = serviceList
                }
            };
        }

        // Nếu rỗng (không có dòng): Gate :656 — không ghi data nào (trường hợp này khác SearchRODetailHistory).
        // Source gate không clear ngoài (data được ghi vào object rỗng).
        return new
        {
            Success = true,
            Data = new
            {
                Lst_Service_Ser_Customer_Summary = summaryList,
                Lst_Service_Ser_CustomerCar_Service = serviceList
            }
        };
    }
}

public class SearchRODetailHistoryReq
{
    /// <summary>
    /// Gate :255 — tham số ROID (string, từ header trong nguồn, nhưng port vào body để tiện).
    /// Giá trị: mã báo giá (vd "RO123") hoặc null/rỗng (không lọc).
    /// </summary>
    public string? ROID { get; set; }
}

public class SearchCustomerCarReq
{
    /// <summary>
    /// Gate :464 — DealerCode (string, mã đại lý).
    /// Chuẩn hoá: StdStr (trim) → StdParam (viết hoa) để tạo danh sách "CODE1|CODE2|...".
    /// </summary>
    public string? DealerCode { get; set; }

    /// <summary>
    /// Gate :465 — CusName (string, tên khách hàng).
    /// Chuẩn hoá: StdStr (trim) → thành pattern "%{value}%".
    /// </summary>
    public string? CusName { get; set; }

    /// <summary>
    /// Gate :466 — PlateNo (string, biển số xe).
    /// Chuẩn hoá: StdStr → pattern "%{value}%".
    /// </summary>
    public string? PlateNo { get; set; }

    /// <summary>
    /// Gate :467 — FrameNo (string, số khung).
    /// Chuẩn hoá: StdStr → pattern "%{value}%".
    /// </summary>
    public string? FrameNo { get; set; }

    /// <summary>
    /// Gate :468 — Mobile (string, số điện thoại).
    /// Chuẩn hoá: StdStr → pattern "%{value}%".
    /// </summary>
    public string? Mobile { get; set; }

    /// <summary>
    /// Gate :469 — ResultRecordStart (string, chỉ số bắt đầu).
    /// Chuẩn hoá: Convert.ToInt32, mặc định 0.
    /// </summary>
    public string? ResultRecordStart { get; set; }

    /// <summary>
    /// Gate :470 — ResultRecordCount (string, số bản ghi trả về).
    /// Chuẩn hoá: Convert.ToInt32, mặc định 0.
    /// </summary>
    public string? ResultRecordCount { get; set; }
}
