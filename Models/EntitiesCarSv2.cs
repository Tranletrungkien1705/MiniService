namespace MiniService.Models;

// =============================================================================================
// ĐỢT 2 — PORT 1:1 ba màn nghiệp vụ lõi của luồng dịch vụ, nguồn thật:
//   D:\idocNet\2021.1.TCMotor\Dev\DMSCarSv\V20.2023.Release.V2   (nhánh Release mới nhất)
//
//   (A) Views/Services/FrmROSearchInfo.cs          -> Ser_RO         (tra cứu / mở lệnh sửa chữa)
//   (B) Views/PartOrder/FrmPartROSearch.cs         -> Ser_ROPartItems (phụ tùng RO đang đợi hàng)
//   (C) Views/Debit/FrmReportSerPaymentGet.cs      -> Ser_Payment    (bảng kê phiếu thu — quyết toán)
//
// Đường trace tới SQL thật (repo 2023.H.CarServices phần lớn là PROXY web service tự sinh):
//   form -> DbServices/*.cs -> HTCWSCarSv/WSCarSv.asmx.cs -> TERP.BizCarSv/*.cs
//   (A) SerROService.cs:528  -> WSCarSv.asmx.cs:10149 -> BizCarSv.Service.RO.cs:498
//       ⚠ BẪY SINH ĐÔI: asmx gọi `Ser_RO_GetStatusList02_New20230220`, KHÔNG gọi hàm cùng tên
//       `Ser_RO_GetStatusList02` ở BizCarSv.Service01.cs:1214 (hàm đó KHÔNG có chỗ gọi ⇒ mã chết).
//   (B) PartOrderService.cs:27 -> WSCarSv.asmx.cs:23909 -> BizCarSv.PartOrder.cs:3985
//   (C) DebitService.cs:644    -> WSCarSv.asmx.cs:18647 -> BizCarSv.Debit.cs:3628
//
// Từ điển tên cột: hằng `Tbl<Bảng>.<Cột>` trong TERP.HTCServiceClient/Common/DbDefine.cs
// (DB_BANG.tsv/DB_COT.tsv của _audit KHÔNG phủ hai dự án này — bẫy #20).
// =============================================================================================

/// <summary>
/// Mã trạng thái Lệnh sửa chữa (Ser_RO.STATUS) — TERP.Constants/Const.Main.cs:172-186.
/// Đích đã có enum <see cref="ROStatus"/> trùng ý nghĩa; lớp này giữ đúng MÃ CHỮ của nguồn
/// để bộ lọc trên trang gửi/nhận cùng bảng mã với WinForm.
/// </summary>
public static class SerRoStage
{
    public const string Create = "CRE";          // Const.Main.cs:174 — Lập báo giá
    public const string Print = "PRT";           // :175 — In báo giá
    public const string Wait4Part = "W4P";       // :176 — Đợi phụ tùng
    public const string HasPart = "HPA";         // :177 — Đã có phụ tùng
    public const string HasRO = "HRO";           // :178 — Lập lệnh sửa chữa
    public const string RejectRO = "REJ";        // :179 — Hủy
    public const string InGarage = "INGA";       // :180 — Vào sửa chữa
    public const string CheckEnd = "CEND";       // :181 — Kiểm tra cuối cùng
    public const string Repaired = "RPRD";       // :182 — Sửa xong
    public const string Paid = "PAID";           // :183 — Đã thanh toán
    public const string Finished = "FNS";        // :184 — Đã hoàn thành
    public const string NotResponding = "NORE";  // :185 — Chưa dùng

    /// <summary>Đổi enum đích -> mã chữ nguồn. Cùng bảng mã với Services/Ui.cs.</summary>
    public static string Of(ROStatus s) => s switch
    {
        ROStatus.Created => Create,
        ROStatus.Printed => Print,
        ROStatus.Wait4Part => Wait4Part,
        ROStatus.HasPart => HasPart,
        ROStatus.HasRO => HasRO,
        ROStatus.InGarage => InGarage,
        ROStatus.Repaired => Repaired,
        ROStatus.CheckEnd => CheckEnd,
        ROStatus.Paid => Paid,
        ROStatus.Finished => Finished,
        ROStatus.Rejected => RejectRO,
        ROStatus.NotResponding => NotResponding,
        _ => ""
    };

    /// <summary>Mã chữ nguồn -> enum đích. Trả null nếu mã lạ (không đoán).</summary>
    public static ROStatus? Parse(string? code) => (code ?? "").Trim().ToUpperInvariant() switch
    {
        "CRE" => ROStatus.Created,
        "PRT" => ROStatus.Printed,
        "W4P" => ROStatus.Wait4Part,
        "HPA" => ROStatus.HasPart,
        "HRO" => ROStatus.HasRO,
        "REJ" => ROStatus.Rejected,
        "INGA" => ROStatus.InGarage,
        "CEND" => ROStatus.CheckEnd,
        "RPRD" => ROStatus.Repaired,
        "PAID" => ROStatus.Paid,
        "FNS" => ROStatus.Finished,
        "NORE" => ROStatus.NotResponding,
        _ => null
    };

    /// <summary>
    /// STATUSNAME của màn tra cứu RO — port nguyên khối `case` ĐANG CHẠY
    /// (BizCarSv.Service.RO.cs:683-694). CHÚ Ý ba điểm lệch của chính nguồn, KHÔNG tự sửa:
    ///  • 'W4P','HPA','NORE' ra "Hủy, Hẹn lại" — trong khi hằng của form ghi "Không dùng"
    ///    (FrmROSearchInfo.cs:39) nên nhánh tô màu theo hằng đó không bao giờ khớp.
    ///  • 'CRE' ra "Chờ sửa" nhưng ô tick "Chờ sửa" của form chỉ gửi 'PRT','HRO' (:101-105).
    ///  • mã lạ ra "Không xác định" (:692).
    /// </summary>
    public static string StatusName(ROStatus s) => Of(s) switch
    {
        Create or Print or HasRO => "Chờ sửa",              // Service.RO.cs:684
        InGarage => "Đang sửa",                             // :685
        Repaired => "Sửa xong",                             // :686
        CheckEnd => "Kiểm tra cuối cùng",                   // :687
        Paid => "Thanh toán xong",                          // :688
        Finished => "Đã giao xe",                           // :689
        RejectRO => "Lệnh hủy",                             // :690
        Wait4Part or HasPart or NotResponding => "Hủy, Hẹn lại", // :691
        _ => "Không xác định"                               // :692
    };
}

/// <summary>
/// Tám ô tick trạng thái của màn tra cứu RO — port 1:1 `GetStringStatus()`
/// FrmROSearchInfo.cs:98-140. Mỗi ô đẩy MỘT HOẶC NHIỀU mã vào danh sách lọc;
/// KHÔNG tick ô nào ⇒ danh sách rỗng ⇒ KHÔNG lọc trạng thái (:138).
/// Mặc định designer: KHÔNG ô nào `Checked = true` ⇒ mở màn là không lọc.
/// </summary>
public static class SerRoSearchStatusBox
{
    /// <summary>Tên ô -> nhãn trên form -> tập mã gửi xuống SQL.</summary>
    public static readonly (string key, string caption, string[] codes)[] All =
    [
        // chkChoSua (:101-105) — CHÚ Ý nguồn KHÔNG gửi 'CRE' dù case ở biz coi 'CRE' là "Chờ sửa".
        ("choSua",         "Chờ sửa",         new[] { SerRoStage.Print, SerRoStage.HasRO }),
        ("dangSua",        "Đang sửa",        new[] { SerRoStage.InGarage }),                     // :106-109
        ("suaXong",        "Sửa xong",        new[] { SerRoStage.Repaired }),                     // :110-113
        ("kiemTraCuoi",    "K.Tra C.Cùng",    new[] { SerRoStage.CheckEnd }),                     // :115-118
        ("thanhToanXong",  "Thanh toán xong", new[] { SerRoStage.Paid }),                         // :120-123
        ("daGiaoXe",       "Đã giao xe",      new[] { SerRoStage.Finished }),                     // :124-127
        // chkKhongDung (:128-133) — nhãn form "Không dùng", nhưng biz trả "Hủy, Hẹn lại".
        ("khongDung",      "Không dùng",      new[] { SerRoStage.Wait4Part, SerRoStage.HasPart, SerRoStage.NotResponding }),
        ("lenhHuy",        "Lệnh hủy",        new[] { SerRoStage.RejectRO }),                     // :134-137
    ];
}

/// <summary>
/// (A) Một dòng grid của màn tra cứu RO. Mười hai cột hiển thị lấy đúng thứ tự
/// FrmROSearchInfo.cs:59-70 + caption designer FrmROSearchInfo.designer.cs:129-225.
/// </summary>
public class SerRoSearchRow
{
    public int RoId { get; set; }                    // ro.ROID — Service.RO.cs:610
    public string RoNo { get; set; } = "";           // ro.RONo — :611
    /// <summary>RORONO = 'BG-' + RONo — Service.RO.cs:728; cột grid "Số báo giá" (:67).</summary>
    public string RoRoNo { get; set; } = "";
    /// <summary>'LS-'+RONo nếu DealerCode trùng đại lý gọi, ngược lại '******' — :729-732.</summary>
    public string NormalizedRoNo { get; set; } = "";
    public string? DealerCode { get; set; }          // ro.DealerCode — :612
    public string? PlateNo { get; set; }             // isnull(ro.PlateNo, car.PlateNo) — :660
    public string? FrameNo { get; set; }             // isnull(ro.FrameNo, car.FrameNo) — :662
    public string? TradeMarkNameModel { get; set; }  // tm.TradeMarkName+' - '+mdl.ModelName — :697
    public string? ColorCode { get; set; }           // isnull(ro.ColorCode, car.ColorCode) — :664
    public string? CusName { get; set; }             // isnull(ro.CusName, isnull(cus.ContName, cus.CusName)) — :651
    public string? OwnerName { get; set; }           // cus.CusName OwnerName — :650
    public string? CusRequest { get; set; }          // ro.CusRequest — :613
    public DateTime? CheckInDate { get; set; }       // ro.CheckInDate — :615
    public DateTime? PlanedDeliveryDate { get; set; }// ro.PlanedDeliveryDate — :619
    public DateTime? PaidCreatedDate { get; set; }   // ro.PaidCreatedDate — :648
    /// <summary>Cột grid nhãn "CVDV" nhưng bind vào ro.Creator, KHÔNG phải ro.Assistant (:66).</summary>
    public string? Creator { get; set; }
    /// <summary>
    /// CreatorName ("Tên CVDV") — chỉ có ở bản WEB của cùng màn này
    /// (<c>2023.H.CarServices/.../Models/CarSv/ROSearchInfo.cs:20</c>), tính bằng
    /// <c>isnull(su.UserName, ro.Creator)</c> (<c>BizCarSv.Master.cs:75384</c>).
    /// Đích chưa có bảng người dùng ⇒ luôn rơi vào nhánh fallback <c>ro.Creator</c>.
    /// </summary>
    public string? CreatorName { get; set; }
    /// <summary>
    /// MyRowIdx của bản WEB: <c>Row_Number() over(order by ro.CheckInDate asc, ro.ROID asc)</c>
    /// (<c>BizCarSv.Master.cs:81102</c>). Chỉ có nghĩa khi gọi kèm phân trang.
    /// </summary>
    public int MyRowIdx { get; set; }
    /// <summary>NormalizedCreator — che '******' khi khác đại lý (:733-736).</summary>
    public string NormalizedCreator { get; set; } = "";
    public string StatusCode { get; set; } = "";     // ro.Status — :628
    public string StatusName { get; set; } = "";     // case ... StatusName — :683-694
    /// <summary>Màu nền ô biển số/số báo giá theo StatusName — FrmROSearchInfo.cs:389-427.</summary>
    public string RowColor { get; set; } = "";
}

/// <summary>
/// (B) Một dòng grid màn "phụ tùng của RO đang đợi hàng" — BizCarSv.PartOrder.cs:4044-4060.
/// Chỉ những cột CÓ TRONG SQL; xem báo cáo về 4 cột grid nguồn bind vào field không tồn tại.
/// </summary>
public class RoPartNeedRow
{
    public string? DealerCode { get; set; }   // ro.Dealercode — PartOrder.cs:4049
    public int RoId { get; set; }             // ro.Roid — :4050
    public string RoNo { get; set; } = "";    // 'BG-'+ro.RoNo as RONO — :4051
    public int? PartId { get; set; }          // rop.Partid — :4052
    public string PartCode { get; set; } = "";// part.PartCode — :4053
    public string? EngName { get; set; }      // part.engName — :4054
    public string VieName { get; set; } = "";// part.VieName — :4055
    public string? Unit { get; set; }         // part.Unit — :4056
    public decimal Quantity { get; set; }     // rop.QUANTITY — :4057 (grid nhãn "SL Cần")
    public decimal Vat { get; set; }          // rop.VAT — :4058
    public string? Note { get; set; }         // rop.NOTE — :4059
    public int MyRowIdx { get; set; }         // Row_Number() over (order by part.PartId desc) — :4060
}

/// <summary>Loại phiếu thu (Ser_Payment.PAYMENTTYPE) — BizCarSv.Debit.cs:3688-3693.</summary>
public static class SerPaymentType
{
    public const string Customer = "1";   // Debit.cs:3689 — N'Khách hàng'
    public const string Insurance = "2";  // Debit.cs:3690 — N'Bảo hiểm'

    /// <summary>PAYMENTTYPETEXT — nguồn trả '' cho mọi mã khác 1/2 (Debit.cs:3691).</summary>
    public static string Text(string? t) => t switch
    {
        Customer => "Khách hàng",
        Insurance => "Bảo hiểm",
        _ => ""
    };
}

/// <summary>
/// (C) Một dòng bảng kê phiếu thu — BizCarSv.Debit.cs:3679-3693.
/// Mười một cột hiển thị theo VisibleIndex của FrmReportSerPaymentGet.designer.cs:132-239.
/// </summary>
public class SerPaymentRow
{
    public int PaymentId { get; set; }             // p.PaymentID (cột ẩn) — DbDefine.cs:1529
    public string? PaymentNo { get; set; }         // p.PaymentNo "Số phiếu thu" — DbDefine.cs:1530
    public DateTime PayDate { get; set; }          // p.PayDate "Ngày" — DbDefine.cs:1536
    public int? CusId { get; set; }                // p.CusID (cột ẩn) — DbDefine.cs:1532
    public string? CusName { get; set; }           // cus.cusname "Tên khách hàng" — Debit.cs:3681
    public string? CusPhone { get; set; }          // isnull(cus.tel,cus.mobile) "Số ĐT khách hàng" — :3682
    public string? PayPersonName { get; set; }     // p.PayPersonName "Người thanh toán" — DbDefine.cs:1534
    /// <summary>
    /// p.PayPersonIDCardNo. Cột grid nguồn bind sai tên field ("PAYPERSONIDCAR",
    /// FrmReportSerPaymentGet.cs:56) nên trên WinForm LUÔN rỗng — bẫy #7 (tên thành viên ≠ tên cột DB).
    /// Đích trả đúng dữ liệu và khai lệch này trong báo cáo.
    /// </summary>
    public string? PayPersonIDCardNo { get; set; }
    public string? InsNo { get; set; }             // p.InsNo "Mã hãng BH" — DbDefine.cs:456
    public string? InsName { get; set; }           // ins.InsVieName "Tên hãng BH" — Debit.cs:3683
    public string? InsPhone { get; set; }          // ins.TelePhone InsPhone "Số ĐT hãng BH" — :3684
    public int? SupplierId { get; set; }           // p.SupplierID (cột ẩn) — DbDefine.cs:1118
    public string? Note { get; set; }              // p.Note "Ghi chú" — DbDefine.cs:1537
    public decimal PaymentAmount { get; set; }     // p.PaymentAmount "Tổng số" — DbDefine.cs:1533
    public string? PaymentType { get; set; }       // p.PaymentType
    public string PaymentTypeText { get; set; } = "";// case p.PaymentType ... — Debit.cs:3688-3693
    public int? RoId { get; set; }                 // (đích) phiếu thu sinh từ quyết toán RO nào
    public string? RoCode { get; set; }            // (đích) số RO tương ứng
}
