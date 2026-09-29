using Microsoft.EntityFrameworkCore;
using MiniService.Models;

namespace MiniService.Data;

public static class Seeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        await MigratePostgresAsync(db);
        await MigrateSqliteAsync(db);

        if (!await db.Orgs.AnyAsync(o => o.Id == TenantContext.DefaultOrgId))
        {
            db.Orgs.Add(new Org { Id = TenantContext.DefaultOrgId, Name = "Demo Service", ApiKey = TenantContext.DefaultApiKey });
            await db.SaveChangesAsync();
        }
        if (!await db.Customers.AnyAsync())
        {
            var c1 = new Customer { Code = "KH0001", Name = "Nguyễn Văn An", Phone = "0901111111", Email = "an@gmail.com",
                Cars = [ new Car { Plate = "30A-123.45", Model = "Hyundai Accent 2022", Year = 2022, Vin = "RLHXXAC001" } ] };
            var c2 = new Customer { Code = "KH0002", Name = "Trần Thị Bình", Phone = "0902222222",
                Cars = [ new Car { Plate = "51G-678.90", Model = "Hyundai Tucson 2023", Year = 2023, Vin = "RLHXXTC002" } ] };
            db.Customers.AddRange(c1, c2);
            await db.SaveChangesAsync();

            // 1 RO đang sửa để demo
            var car = await db.Cars.FirstAsync();
            var ro = new RepairOrder { Code = "ROSEED-001", CarId = car.Id, CustomerId = car.CustomerId, Status = ROStatus.InGarage,
                Odometer = 25400, IntakeNote = "Bảo dưỡng 20.000km + thay dầu", Technician = "Thợ Hùng", CreatedBy = "seed", IntakeAt = DateTime.Now.AddHours(-3),
                Lines = [
                    new RepairLine { Type = LineType.Labor, Name = "Công bảo dưỡng cấp 20.000km", Quantity = 1, UnitPrice = 500000 },
                    new RepairLine { Type = LineType.Part, Name = "Dầu động cơ Hyundai 5W-30 (4L)", Quantity = 1, UnitPrice = 650000 },
                    new RepairLine { Type = LineType.Part, Name = "Lọc dầu", Quantity = 1, UnitPrice = 180000 },
                ] };
            db.ROs.Add(ro);
            await db.SaveChangesAsync();
        }
        // Tồn kho phụ tùng
        if (!await db.Parts.AnyAsync())
        {
            db.Parts.AddRange(
                new Part { Code = "PT001", Name = "Dầu động cơ Hyundai 5W-30 (4L)", Unit = "bình", Price = 650000, OnHand = 40, MinStock = 10 },
                new Part { Code = "PT002", Name = "Lọc dầu", Unit = "cái", Price = 180000, OnHand = 60, MinStock = 15 },
                new Part { Code = "PT003", Name = "Lọc gió động cơ", Unit = "cái", Price = 220000, OnHand = 8, MinStock = 10 },
                new Part { Code = "PT004", Name = "Má phanh trước", Unit = "bộ", Price = 850000, OnHand = 12, MinStock = 6 },
                new Part { Code = "PT005", Name = "Bugi", Unit = "cái", Price = 120000, OnHand = 3, MinStock = 20 });
            await db.SaveChangesAsync();
        }

        // ===== Dữ liệu mẫu cho 3 màn port 1:1 từ DMSCarSv =====
        // (1) Ser_ROStockRequisition — phiếu yêu cầu xuất kho theo RO đang sửa.
        if (!await db.StockReqs.AnyAsync())
        {
            var ro = await db.ROs.Include(r => r.Car).FirstOrDefaultAsync();
            if (ro != null)
                db.StockReqs.Add(new RoStockRequisition
                {
                    StkReqId = "STKSEED-001", ROId = ro.Id, RONo = ro.Code, PlateNo = ro.Car.Plate,
                    FrameNo = ro.Car.Vin, Assistant = "CVDV Nam", Note = "Xuất dầu + lọc cho bảo dưỡng 20.000km",
                    Status = StockReqStatus.Create, CreatedDate = DateTime.Now.AddHours(-2)
                });
            await db.SaveChangesAsync();
        }
        // (2) Ser_Inv_Quote — báo giá phụ tùng (1 phiếu mới tạo, 1 phiếu đã tạo phiếu xuất).
        if (!await db.PartQuotations.AnyAsync())
        {
            var cus = await db.Customers.OrderBy(c => c.Id).FirstOrDefaultAsync();
            db.PartQuotations.AddRange(
                new PartQuotation
                {
                    QuoteNo = "BGSEED-001", CustomerId = cus?.Id, Creator = "seed", LogLuBy = "seed",
                    Note = "Báo giá thay má phanh", ReceiveName = cus?.Name, PaymentMethod = "CASH",
                    SOStatus = null,   // chưa có phiếu xuất ⇒ StatusText = "Mới tạo"
                    Items =
                    [
                        new PartQuotationItem { PartCode = "PT004", PartName = "Má phanh trước", Quantity = 1, Price = 850000, VAT = 10, Factor = 1 },
                        new PartQuotationItem { PartCode = "PT002", PartName = "Lọc dầu", Quantity = 2, Price = 180000, VAT = 10, Factor = 1 },
                    ]
                },
                new PartQuotation
                {
                    QuoteNo = "BGSEED-002", CustomerId = cus?.Id, Creator = "seed", LogLuBy = "seed",
                    Note = "Báo giá bugi cả bộ", SOStatus = "1",   // ⇒ StatusText = "Đã tạo phiếu xuất"
                    Items = [ new PartQuotationItem { PartCode = "PT005", PartName = "Bugi", Quantity = 4, Price = 120000, VAT = 10, Factor = 1 } ]
                });
            await db.SaveChangesAsync();
        }
        // (3) Ser_Part_Order — đơn đặt hàng phụ tùng (1 đơn hàng đang về dở).
        if (!await db.PartOrders.AnyAsync())
        {
            db.PartOrders.Add(new PartOrder
            {
                OrderNo = "DHSEED-001", OrderNoUser = "NCC-2026-01", ConfirmNo = "CF-001",
                SupplierId = 1, SupplierName = "Hyundai Thành Công — Kho phụ tùng",
                UserCreate = "seed", SendDate = DateTime.Now.AddDays(-3),
                Status = PartOrderStatus.SIExecuting, IsActive = "1",
                Details =
                [
                    new PartOrderDetail { PartCode = "PT003", PartName = "Lọc gió động cơ", Unit = "cái",
                        Quantity = 20, Cost = 180000, VAT = 10, Factor = 1, DeliveryQuantity = 12, OH = 8, OO = 20, BO = 8,
                        LastDateDelivery = DateTime.Now.AddDays(-1) },
                    new PartOrderDetail { PartCode = "PT005", PartName = "Bugi", Unit = "cái",
                        Quantity = 40, Cost = 95000, VAT = 10, Factor = 1, DeliveryQuantity = null, OH = 3, OO = 40, BO = 17 },
                ]
            });
            await db.SaveChangesAsync();
        }

        // ===== ĐỢT 2 — dữ liệu mẫu cho 3 màn mới (chỉ thêm, KHÔNG sửa dữ liệu mẫu đợt 1) =====
        // (A) FrmROSearchInfo cần nhiều trạng thái + tên hãng để cột "Hãng - Model" có dữ liệu.
        foreach (var c in await db.Cars.Where(x => x.TradeMarkName == null).ToListAsync())
            c.TradeMarkName = "Hyundai";                    // ser_mst_TradeMark.TradeMarkName
        foreach (var r in await db.ROs.Where(x => x.DealerCode == null).ToListAsync())
            r.DealerCode = "VNN054";                        // đại lý mẫu, đúng dạng mã nguồn
        await db.SaveChangesAsync();

        // Một RO "Đợi phụ tùng" (W4P) có dòng phụ tùng gắn mã kho — nuôi cả màn (A) và màn (B).
        if (!await db.ROs.AnyAsync(r => r.Code == "ROSEED-W4P"))
        {
            var car = await db.Cars.OrderBy(c => c.Id).Skip(1).FirstOrDefaultAsync()
                      ?? await db.Cars.OrderBy(c => c.Id).FirstOrDefaultAsync();
            var pt003 = await db.Parts.FirstOrDefaultAsync(p => p.Code == "PT003");
            var pt005 = await db.Parts.FirstOrDefaultAsync(p => p.Code == "PT005");
            if (car != null)
            {
                db.ROs.Add(new RepairOrder
                {
                    Code = "ROSEED-W4P", CarId = car.Id, CustomerId = car.CustomerId,
                    Status = ROStatus.Wait4Part, DealerCode = "VNN054", Odometer = 41200,
                    CustomerRequest = "Thay lọc gió + bugi, chờ phụ tùng về",
                    ServiceAdvisor = "CVDV Nam", Technician = "Thợ Sơn", CreatedBy = "CVDV Nam",
                    IntakeAt = DateTime.Today.AddDays(-1).AddHours(9),
                    ExpectedDelivery = DateTime.Today.AddDays(2).AddHours(17),
                    Lines =
                    [
                        new RepairLine { Type = LineType.Part, Name = "Lọc gió động cơ", Quantity = 2,
                            UnitPrice = 220000, PartId = pt003?.Id, Vat = 10, Note = "Đặt hàng NCC" },
                        new RepairLine { Type = LineType.Part, Name = "Bugi", Quantity = 4,
                            UnitPrice = 120000, PartId = pt005?.Id, Vat = 10, Note = "Đặt cả bộ" },
                    ]
                });
                await db.SaveChangesAsync();
            }
        }
        // Một RO đã thanh toán xong (PAID) để cột "Ngày thanh toán" và màu nền có dữ liệu.
        if (!await db.ROs.AnyAsync(r => r.Code == "ROSEED-PAID"))
        {
            var car = await db.Cars.OrderBy(c => c.Id).FirstOrDefaultAsync();
            if (car != null)
            {
                db.ROs.Add(new RepairOrder
                {
                    Code = "ROSEED-PAID", CarId = car.Id, CustomerId = car.CustomerId,
                    Status = ROStatus.Paid, DealerCode = "VNN054", Odometer = 30100,
                    CustomerRequest = "Bảo dưỡng 30.000km", ServiceAdvisor = "CVDV Nam",
                    CreatedBy = "CVDV Hòa", IntakeAt = DateTime.Today.AddDays(-5).AddHours(8),
                    ExpectedDelivery = DateTime.Today.AddDays(-5).AddHours(17),
                    PaidCreatedDate = DateTime.Today.AddDays(-5).AddHours(16),
                    FinishedAt = DateTime.Today.AddDays(-5).AddHours(17),
                    Lines = [ new RepairLine { Type = LineType.Labor, Name = "Công bảo dưỡng 30.000km", Quantity = 1, UnitPrice = 700000 } ]
                });
                await db.SaveChangesAsync();
            }
        }
        // Bổ sung tên tiếng Anh cho phụ tùng (cột grid "Tên TA" của màn B).
        var engNames = new Dictionary<string, string>
        {
            ["PT001"] = "Engine oil 5W-30 (4L)", ["PT002"] = "Oil filter", ["PT003"] = "Air filter",
            ["PT004"] = "Front brake pad set", ["PT005"] = "Spark plug"
        };
        foreach (var p in await db.Parts.Where(x => x.EngName == null).ToListAsync())
            if (engNames.TryGetValue(p.Code, out var en)) p.EngName = en;
        await db.SaveChangesAsync();

        // (C) Hai phiếu thu mẫu: 1 của khách hàng ('1'), 1 của hãng bảo hiểm ('2').
        if (!await db.Payments.AnyAsync(p => p.PaymentNo != null))
        {
            var cus = await db.Customers.OrderBy(c => c.Id).FirstOrDefaultAsync();
            var d = "VNN054";
            var day = DateTime.Today.AddDays(-2);
            db.Payments.AddRange(
                new Payment
                {
                    ROId = 0, ROCode = "", PaymentNo = $"PT-{d}-{day:yyMMdd}-001", DealerCode = d,
                    CustomerId = cus?.Id, Amount = 1_350_000m, PayPersonName = "Nguyễn Văn An",
                    PayPersonIDCardNo = "001099123456", PaidAt = day.AddHours(10),
                    Note = "Thu tiền bảo dưỡng 20.000km", PaymentType = SerPaymentType.Customer
                },
                new Payment
                {
                    ROId = 0, ROCode = "", PaymentNo = $"PT-{d}-{day:yyMMdd}-002", DealerCode = d,
                    CustomerId = null, Amount = 8_700_000m, PayPersonName = "Trần Thị Bảo",
                    PayPersonIDCardNo = "001199654321", PaidAt = day.AddHours(15),
                    Note = "Bảo hiểm chi trả sửa chữa sau tai nạn", PaymentType = SerPaymentType.Insurance,
                    InsNo = "PVI", InsName = "Bảo hiểm PVI", InsPhone = "02439412111"
                });
            await db.SaveChangesAsync();
        }
        // Phiếu thu sinh từ quyết toán RO (InventoryService.SettleAsync) không có DealerCode/PaymentType
        // ⇒ vá ngược để chúng xuất hiện trên bảng kê đúng như nguồn (p.PaymentType in ('1','2')).
        foreach (var p in await db.Payments.Where(x => x.DealerCode == null).ToListAsync())
        {
            p.DealerCode = "VNN054";
            if (string.IsNullOrWhiteSpace(p.PaymentType)) p.PaymentType = SerPaymentType.Customer;
        }
        await db.SaveChangesAsync();
    }

    private static async Task MigratePostgresAsync(AppDbContext db)
    {
        if (!db.Database.IsNpgsql()) return;
        var def = TenantContext.DefaultOrgId;
        var tables = new[] { "Customers", "Cars", "ROs", "Lines" };
        var sql = new List<string>
        {
            "CREATE TABLE IF NOT EXISTS miniservice.\"Orgs\" (\"Id\" uuid PRIMARY KEY, \"Name\" text NOT NULL DEFAULT '', \"ApiKey\" text NOT NULL DEFAULT '', \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Orgs_ApiKey\" ON miniservice.\"Orgs\" (\"ApiKey\")",
        };
        foreach (var t in tables) sql.Add($"ALTER TABLE miniservice.\"{t}\" ADD COLUMN IF NOT EXISTS \"OrgId\" uuid NOT NULL DEFAULT '{def}'");
        // cột tích hợp HĐĐT (thêm mới)
        sql.Add("ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"EInvoiceCode\" text");
        sql.Add("ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"EInvoiceStatus\" text");
        sql.Add("ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"EInvoiceError\" text");
        sql.Add("ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"EInvoiceAt\" timestamp");
        sql.Add("ALTER TABLE miniservice.\"Customers\" ADD COLUMN IF NOT EXISTS \"Address\" text");
        sql.Add("ALTER TABLE miniservice.\"Customers\" ADD COLUMN IF NOT EXISTS \"TaxCode\" text");
        sql.Add("ALTER TABLE miniservice.\"Customers\" ADD COLUMN IF NOT EXISTS \"DealerCode\" text");
        sql.Add("ALTER TABLE miniservice.\"Cars\" ADD COLUMN IF NOT EXISTS \"EngineNo\" text");
        sql.Add("ALTER TABLE miniservice.\"Cars\" ADD COLUMN IF NOT EXISTS \"Color\" text");
        sql.Add("ALTER TABLE miniservice.\"Cars\" ADD COLUMN IF NOT EXISTS \"CurrentKm\" integer NOT NULL DEFAULT 0");
        sql.Add("ALTER TABLE miniservice.\"Lines\" ADD COLUMN IF NOT EXISTS \"PartId\" integer");
        sql.Add("ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"CustomerRequest\" text");
        sql.Add("ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"ServiceAdvisor\" text");
        sql.Add("ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"ExpectedDelivery\" timestamp");
        // Bảng MỚI tồn kho/xuất kho/thanh toán — EnsureCreated không tạo trên DB đã tồn tại → CREATE tường minh.
        sql.Add(@"CREATE TABLE IF NOT EXISTS miniservice.""Parts"" (""Id"" serial PRIMARY KEY, ""OrgId"" uuid NOT NULL DEFAULT '" + def + @"',
            ""Code"" text NOT NULL DEFAULT '', ""Name"" text NOT NULL DEFAULT '', ""Unit"" text NOT NULL DEFAULT 'cái',
            ""Price"" numeric(18,2) NOT NULL DEFAULT 0, ""OnHand"" integer NOT NULL DEFAULT 0, ""MinStock"" integer NOT NULL DEFAULT 5)");
        sql.Add(@"CREATE TABLE IF NOT EXISTS miniservice.""StockOuts"" (""Id"" serial PRIMARY KEY, ""OrgId"" uuid NOT NULL DEFAULT '" + def + @"',
            ""Code"" text NOT NULL DEFAULT '', ""PartId"" integer NOT NULL DEFAULT 0, ""PartName"" text NOT NULL DEFAULT '',
            ""Quantity"" integer NOT NULL DEFAULT 0, ""UnitPrice"" numeric(18,2) NOT NULL DEFAULT 0,
            ""ROId"" integer, ""ROCode"" text, ""Reason"" text, ""CreatedAt"" timestamp NOT NULL DEFAULT now())");
        sql.Add(@"CREATE TABLE IF NOT EXISTS miniservice.""Payments"" (""Id"" serial PRIMARY KEY, ""OrgId"" uuid NOT NULL DEFAULT '" + def + @"',
            ""ROId"" integer NOT NULL DEFAULT 0, ""ROCode"" text NOT NULL DEFAULT '', ""Amount"" numeric(18,2) NOT NULL DEFAULT 0,
            ""Method"" integer NOT NULL DEFAULT 0, ""Note"" text, ""PaidAt"" timestamp NOT NULL DEFAULT now())");

        // ===== Bảng MỚI cho 3 màn port 1:1 từ DMSCarSv (Ser_ROStockRequisition / Ser_Inv_Quote / Ser_Part_Order).
        // EnsureCreated KHÔNG tạo bảng mới trên DB đã tồn tại ⇒ phải CREATE tường minh như 3 bảng ở trên.
        sql.Add(@"CREATE TABLE IF NOT EXISTS miniservice.""StockReqs"" (""Id"" serial PRIMARY KEY, ""OrgId"" uuid NOT NULL DEFAULT '" + def + @"',
            ""StkReqId"" text NOT NULL DEFAULT '', ""DealerCode"" text, ""ROId"" integer, ""RONo"" text NOT NULL DEFAULT '',
            ""Assistant"" text, ""PlateNo"" text, ""FrameNo"" text, ""CreatedDate"" timestamp NOT NULL DEFAULT now(),
            ""Status"" text NOT NULL DEFAULT 'CRE', ""Note"" text)");
        sql.Add(@"CREATE TABLE IF NOT EXISTS miniservice.""PartQuotations"" (""Id"" serial PRIMARY KEY, ""OrgId"" uuid NOT NULL DEFAULT '" + def + @"',
            ""QuoteNo"" text NOT NULL DEFAULT '', ""Remark"" text, ""Creator"" text, ""CreatedDate"" timestamp NOT NULL DEFAULT now(),
            ""LogLuBy"" text, ""Note"" text, ""ReceiveName"" text, ""PaymentMethod"" text, ""SOStatus"" text, ""CustomerId"" integer)");
        sql.Add(@"CREATE TABLE IF NOT EXISTS miniservice.""PartQuotationItems"" (""Id"" serial PRIMARY KEY, ""OrgId"" uuid NOT NULL DEFAULT '" + def + @"',
            ""QuoteId"" integer NOT NULL DEFAULT 0, ""PartId"" integer, ""PartCode"" text NOT NULL DEFAULT '', ""PartName"" text NOT NULL DEFAULT '',
            ""Quantity"" numeric(18,2) NOT NULL DEFAULT 1, ""Price"" numeric(18,2) NOT NULL DEFAULT 0, ""VAT"" numeric(18,2) NOT NULL DEFAULT 0,
            ""PartPriceId"" integer, ""Factor"" numeric(18,4) NOT NULL DEFAULT 1, ""Note"" text)");
        sql.Add(@"CREATE TABLE IF NOT EXISTS miniservice.""PartOrders"" (""Id"" serial PRIMARY KEY, ""OrgId"" uuid NOT NULL DEFAULT '" + def + @"',
            ""OrderNo"" text NOT NULL DEFAULT '', ""OrderNoUser"" text, ""ConfirmNo"" text, ""DealerCode"" text,
            ""SupplierId"" integer, ""SupplierName"" text, ""CreateDate"" timestamp NOT NULL DEFAULT now(),
            ""SendDate"" timestamp, ""ReceivePartDate"" timestamp, ""ApprovedDate"" timestamp,
            ""UserCreate"" text, ""UserApproved"" text, ""Status"" text NOT NULL DEFAULT '1', ""NewStatus"" text,
            ""HTCConfirm"" text, ""IsActive"" text NOT NULL DEFAULT '1', ""PartialShipment"" text,
            ""TypeOrder"" text, ""TypeTransport"" text, ""VIN"" text, ""CusCharges"" text)");
        sql.Add(@"CREATE TABLE IF NOT EXISTS miniservice.""PartOrderDetails"" (""Id"" serial PRIMARY KEY, ""OrgId"" uuid NOT NULL DEFAULT '" + def + @"',
            ""OrderPartId"" integer NOT NULL DEFAULT 0, ""PartId"" integer, ""PartCode"" text NOT NULL DEFAULT '',
            ""PartName"" text NOT NULL DEFAULT '', ""EngName"" text, ""Unit"" text, ""Model"" text,
            ""Quantity"" numeric(18,2) NOT NULL DEFAULT 0, ""OrderQuantity"" numeric(18,2), ""DeliveryQuantity"" numeric(18,2),
            ""Cost"" numeric(18,2) NOT NULL DEFAULT 0, ""VAT"" numeric(18,2) NOT NULL DEFAULT 0, ""Discount"" numeric(18,2),
            ""Factor"" numeric(18,4) NOT NULL DEFAULT 1, ""Note"" text, ""HTCConfirm"" text, ""LastDateDelivery"" timestamp,
            ""MIP"" numeric(18,2), ""OO"" numeric(18,2), ""BO"" numeric(18,2), ""OH"" numeric(18,2),
            ""SOQ"" numeric(18,2), ""ICC"" numeric(18,2))");
        sql.Add(@"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_StockReqs_OrgId_StkReqId"" ON miniservice.""StockReqs"" (""OrgId"", ""StkReqId"")");
        sql.Add(@"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PartQuotations_OrgId_QuoteNo"" ON miniservice.""PartQuotations"" (""OrgId"", ""QuoteNo"")");
        sql.Add(@"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PartOrders_OrgId_OrderNo"" ON miniservice.""PartOrders"" (""OrgId"", ""OrderNo"")");

        // ===== ĐỢT 2 — cột thêm cho 3 màn FrmROSearchInfo / FrmPartROSearch / FrmReportSerPaymentGet =====
        sql.Add("ALTER TABLE miniservice.\"Cars\" ADD COLUMN IF NOT EXISTS \"TradeMarkName\" text");
        sql.Add("ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"DealerCode\" text");
        sql.Add("ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"PaidCreatedDate\" timestamp");
        sql.Add("ALTER TABLE miniservice.\"Lines\" ADD COLUMN IF NOT EXISTS \"Vat\" numeric(18,2) NOT NULL DEFAULT 0");
        sql.Add("ALTER TABLE miniservice.\"Lines\" ADD COLUMN IF NOT EXISTS \"Note\" text");
        sql.Add("ALTER TABLE miniservice.\"Parts\" ADD COLUMN IF NOT EXISTS \"EngName\" text");
        sql.Add("ALTER TABLE miniservice.\"Parts\" ADD COLUMN IF NOT EXISTS \"IsActive\" text NOT NULL DEFAULT '1'");
        sql.Add("ALTER TABLE miniservice.\"Payments\" ADD COLUMN IF NOT EXISTS \"PaymentNo\" text");
        sql.Add("ALTER TABLE miniservice.\"Payments\" ADD COLUMN IF NOT EXISTS \"DealerCode\" text");
        sql.Add("ALTER TABLE miniservice.\"Payments\" ADD COLUMN IF NOT EXISTS \"CustomerId\" integer");
        sql.Add("ALTER TABLE miniservice.\"Payments\" ADD COLUMN IF NOT EXISTS \"PayPersonName\" text");
        sql.Add("ALTER TABLE miniservice.\"Payments\" ADD COLUMN IF NOT EXISTS \"PayPersonIDCardNo\" text");
        sql.Add("ALTER TABLE miniservice.\"Payments\" ADD COLUMN IF NOT EXISTS \"InsNo\" text");
        sql.Add("ALTER TABLE miniservice.\"Payments\" ADD COLUMN IF NOT EXISTS \"InsName\" text");
        sql.Add("ALTER TABLE miniservice.\"Payments\" ADD COLUMN IF NOT EXISTS \"InsPhone\" text");
        sql.Add("ALTER TABLE miniservice.\"Payments\" ADD COLUMN IF NOT EXISTS \"SupplierId\" integer");
        sql.Add("ALTER TABLE miniservice.\"Payments\" ADD COLUMN IF NOT EXISTS \"PaymentType\" text NOT NULL DEFAULT '1'");
        sql.Add(@"CREATE INDEX IF NOT EXISTS ""IX_Payments_OrgId_PaidAt"" ON miniservice.""Payments"" (""OrgId"", ""PaidAt"")");

        foreach (var s in sql) try { await db.Database.ExecuteSqlRawAsync(s); } catch { }
    }

    /// <summary>
    /// SQLite: EnsureCreated CHỈ tạo schema khi file DB chưa tồn tại, nên DB dev cũ sẽ THIẾU 5 bảng mới
    /// của 3 màn port từ DMSCarSv (lỗi runtime "no such table"). Tạo tường minh, giống cách làm cho Postgres.
    /// </summary>
    private static async Task MigrateSqliteAsync(AppDbContext db)
    {
        if (!db.Database.IsSqlite()) return;
        var sql = new[]
        {
            // ===== ĐỢT 2, ĐO ĐƯỢC BẰNG TAY: `miniservice.db` trong repo mới có 5 bảng
            // (Cars, Customers, Lines, Orgs, ROs) — THIẾU cả Parts/StockOuts/Payments. Nhánh Postgres
            // đã tạo 3 bảng đó từ trước, nhánh SQLite thì CHƯA ⇒ chạy app trên DB repo nổ ngay
            // `SQLite Error 1: 'no such table: Parts'` (đã dựng lại được lỗi này). Bù đúng 3 bảng,
            // giữ nguyên thứ tự/kiểu như MigratePostgresAsync để hai nhánh không lệch nhau.
            @"CREATE TABLE IF NOT EXISTS ""Parts"" (""Id"" INTEGER PRIMARY KEY AUTOINCREMENT, ""OrgId"" TEXT NOT NULL,
                ""Code"" TEXT NOT NULL DEFAULT '', ""Name"" TEXT NOT NULL DEFAULT '', ""Unit"" TEXT NOT NULL DEFAULT 'cái',
                ""Price"" TEXT NOT NULL DEFAULT '0', ""OnHand"" INTEGER NOT NULL DEFAULT 0, ""MinStock"" INTEGER NOT NULL DEFAULT 5,
                ""EngName"" TEXT NULL, ""IsActive"" TEXT NOT NULL DEFAULT '1')",
            @"CREATE TABLE IF NOT EXISTS ""StockOuts"" (""Id"" INTEGER PRIMARY KEY AUTOINCREMENT, ""OrgId"" TEXT NOT NULL,
                ""Code"" TEXT NOT NULL DEFAULT '', ""PartId"" INTEGER NOT NULL DEFAULT 0, ""PartName"" TEXT NOT NULL DEFAULT '',
                ""Quantity"" INTEGER NOT NULL DEFAULT 0, ""UnitPrice"" TEXT NOT NULL DEFAULT '0',
                ""ROId"" INTEGER NULL, ""ROCode"" TEXT NULL, ""Reason"" TEXT NULL, ""CreatedAt"" TEXT NOT NULL)",
            @"CREATE TABLE IF NOT EXISTS ""Payments"" (""Id"" INTEGER PRIMARY KEY AUTOINCREMENT, ""OrgId"" TEXT NOT NULL,
                ""ROId"" INTEGER NOT NULL DEFAULT 0, ""ROCode"" TEXT NOT NULL DEFAULT '', ""Amount"" TEXT NOT NULL DEFAULT '0',
                ""Method"" INTEGER NOT NULL DEFAULT 0, ""Note"" TEXT NULL, ""PaidAt"" TEXT NOT NULL,
                ""PaymentNo"" TEXT NULL, ""DealerCode"" TEXT NULL, ""CustomerId"" INTEGER NULL,
                ""PayPersonName"" TEXT NULL, ""PayPersonIDCardNo"" TEXT NULL, ""InsNo"" TEXT NULL,
                ""InsName"" TEXT NULL, ""InsPhone"" TEXT NULL, ""SupplierId"" INTEGER NULL,
                ""PaymentType"" TEXT NOT NULL DEFAULT '1')",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Parts_OrgId_Code"" ON ""Parts"" (""OrgId"", ""Code"")",
            @"CREATE TABLE IF NOT EXISTS ""StockReqs"" (""Id"" INTEGER PRIMARY KEY AUTOINCREMENT, ""OrgId"" TEXT NOT NULL,
                ""StkReqId"" TEXT NOT NULL DEFAULT '', ""DealerCode"" TEXT NULL, ""ROId"" INTEGER NULL, ""RONo"" TEXT NOT NULL DEFAULT '',
                ""Assistant"" TEXT NULL, ""PlateNo"" TEXT NULL, ""FrameNo"" TEXT NULL, ""CreatedDate"" TEXT NOT NULL,
                ""Status"" TEXT NOT NULL DEFAULT 'CRE', ""Note"" TEXT NULL)",
            @"CREATE TABLE IF NOT EXISTS ""PartQuotations"" (""Id"" INTEGER PRIMARY KEY AUTOINCREMENT, ""OrgId"" TEXT NOT NULL,
                ""QuoteNo"" TEXT NOT NULL DEFAULT '', ""Remark"" TEXT NULL, ""Creator"" TEXT NULL, ""CreatedDate"" TEXT NOT NULL,
                ""LogLuBy"" TEXT NULL, ""Note"" TEXT NULL, ""ReceiveName"" TEXT NULL, ""PaymentMethod"" TEXT NULL,
                ""SOStatus"" TEXT NULL, ""CustomerId"" INTEGER NULL)",
            @"CREATE TABLE IF NOT EXISTS ""PartQuotationItems"" (""Id"" INTEGER PRIMARY KEY AUTOINCREMENT, ""OrgId"" TEXT NOT NULL,
                ""QuoteId"" INTEGER NOT NULL DEFAULT 0, ""PartId"" INTEGER NULL, ""PartCode"" TEXT NOT NULL DEFAULT '',
                ""PartName"" TEXT NOT NULL DEFAULT '', ""Quantity"" TEXT NOT NULL DEFAULT '1', ""Price"" TEXT NOT NULL DEFAULT '0',
                ""VAT"" TEXT NOT NULL DEFAULT '0', ""PartPriceId"" INTEGER NULL, ""Factor"" TEXT NOT NULL DEFAULT '1', ""Note"" TEXT NULL)",
            @"CREATE TABLE IF NOT EXISTS ""PartOrders"" (""Id"" INTEGER PRIMARY KEY AUTOINCREMENT, ""OrgId"" TEXT NOT NULL,
                ""OrderNo"" TEXT NOT NULL DEFAULT '', ""OrderNoUser"" TEXT NULL, ""ConfirmNo"" TEXT NULL, ""DealerCode"" TEXT NULL,
                ""SupplierId"" INTEGER NULL, ""SupplierName"" TEXT NULL, ""CreateDate"" TEXT NOT NULL, ""SendDate"" TEXT NULL,
                ""ReceivePartDate"" TEXT NULL, ""ApprovedDate"" TEXT NULL, ""UserCreate"" TEXT NULL, ""UserApproved"" TEXT NULL,
                ""Status"" TEXT NOT NULL DEFAULT '1', ""NewStatus"" TEXT NULL, ""HTCConfirm"" TEXT NULL,
                ""IsActive"" TEXT NOT NULL DEFAULT '1', ""PartialShipment"" TEXT NULL, ""TypeOrder"" TEXT NULL,
                ""TypeTransport"" TEXT NULL, ""VIN"" TEXT NULL, ""CusCharges"" TEXT NULL)",
            @"CREATE TABLE IF NOT EXISTS ""PartOrderDetails"" (""Id"" INTEGER PRIMARY KEY AUTOINCREMENT, ""OrgId"" TEXT NOT NULL,
                ""OrderPartId"" INTEGER NOT NULL DEFAULT 0, ""PartId"" INTEGER NULL, ""PartCode"" TEXT NOT NULL DEFAULT '',
                ""PartName"" TEXT NOT NULL DEFAULT '', ""EngName"" TEXT NULL, ""Unit"" TEXT NULL, ""Model"" TEXT NULL,
                ""Quantity"" TEXT NOT NULL DEFAULT '0', ""OrderQuantity"" TEXT NULL, ""DeliveryQuantity"" TEXT NULL,
                ""Cost"" TEXT NOT NULL DEFAULT '0', ""VAT"" TEXT NOT NULL DEFAULT '0', ""Discount"" TEXT NULL,
                ""Factor"" TEXT NOT NULL DEFAULT '1', ""Note"" TEXT NULL, ""HTCConfirm"" TEXT NULL, ""LastDateDelivery"" TEXT NULL,
                ""MIP"" TEXT NULL, ""OO"" TEXT NULL, ""BO"" TEXT NULL, ""OH"" TEXT NULL, ""SOQ"" TEXT NULL, ""ICC"" TEXT NULL)",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_StockReqs_OrgId_StkReqId"" ON ""StockReqs"" (""OrgId"", ""StkReqId"")",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PartQuotations_OrgId_QuoteNo"" ON ""PartQuotations"" (""OrgId"", ""QuoteNo"")",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PartOrders_OrgId_OrderNo"" ON ""PartOrders"" (""OrgId"", ""OrderNo"")",
        };
        foreach (var s in sql) try { await db.Database.ExecuteSqlRawAsync(s); } catch { }

        // ===== ĐỢT 2 — THÊM CỘT vào bảng ĐÃ TỒN TẠI =====
        // SQLite KHÔNG có "ADD COLUMN IF NOT EXISTS" và EnsureCreated là NO-OP khi file DB đã có
        // ⇒ chạy lại app trên DB cũ sẽ "no such column" dù build xanh (bẫy #28).
        // Vì vậy hỏi pragma_table_info TRƯỚC, chỉ ALTER khi cột thật sự thiếu.
        // (a) Bù các cột nhánh Postgres đã ALTER từ trước mà nhánh SQLite bỏ sót — đo trên `miniservice.db`
        //     của repo: Cars thiếu EngineNo/Color/CurrentKm, ROs thiếu CustomerRequest/ServiceAdvisor/
        //     ExpectedDelivery, Lines thiếu PartId, Customers thiếu Address/TaxCode/DealerCode.
        await AddColumnIfMissingAsync(db, "Customers", "Address", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "Customers", "TaxCode", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "Customers", "DealerCode", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "Cars", "EngineNo", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "Cars", "Color", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "Cars", "CurrentKm", "INTEGER NOT NULL DEFAULT 0");
        await AddColumnIfMissingAsync(db, "ROs", "CustomerRequest", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "ROs", "ServiceAdvisor", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "ROs", "ExpectedDelivery", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "ROs", "InsuranceClaimCode", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "ROs", "InsuranceClaimStatus", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "ROs", "LoyaltyInfo", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "Lines", "PartId", "INTEGER NULL");
        // (b) Cột riêng của ba màn đợt 2.
        await AddColumnIfMissingAsync(db, "Cars", "TradeMarkName", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "ROs", "DealerCode", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "ROs", "PaidCreatedDate", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "Lines", "Vat", "TEXT NOT NULL DEFAULT '0'");
        await AddColumnIfMissingAsync(db, "Lines", "Note", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "Parts", "EngName", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "Parts", "IsActive", "TEXT NOT NULL DEFAULT '1'");
        await AddColumnIfMissingAsync(db, "Payments", "PaymentNo", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "Payments", "DealerCode", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "Payments", "CustomerId", "INTEGER NULL");
        await AddColumnIfMissingAsync(db, "Payments", "PayPersonName", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "Payments", "PayPersonIDCardNo", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "Payments", "InsNo", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "Payments", "InsName", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "Payments", "InsPhone", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "Payments", "SupplierId", "INTEGER NULL");
        await AddColumnIfMissingAsync(db, "Payments", "PaymentType", "TEXT NOT NULL DEFAULT '1'");
        try { await db.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_Payments_OrgId_PaidAt"" ON ""Payments"" (""OrgId"", ""PaidAt"")"); } catch { }
    }

    /// <summary>
    /// Thêm một cột vào bảng SQLite ĐÃ TỒN TẠI, chỉ khi cột chưa có.
    /// Đọc `pragma_table_info('&lt;bảng&gt;')` thay vì gõ tay DDL hay bọc try/catch mù — nếu ALTER
    /// thất bại vì lý do KHÁC (kiểu sai, khoá) thì lỗi phải nổ ra để thấy, không được nuốt.
    /// Tên bảng/cột do chính mã này truyền (không phải dữ liệu người dùng) nên nội suy vào SQL là an toàn,
    /// vẫn chặn thêm bằng bộ lọc ký tự để không bao giờ thành đường tiêm.
    /// </summary>
    private static async Task AddColumnIfMissingAsync(AppDbContext db, string table, string column, string ddlType)
    {
        if (!db.Database.IsSqlite()) return;
        if (!SafeIdent(table) || !SafeIdent(column)) throw new ArgumentException($"Tên không hợp lệ: {table}.{column}");
        var conn = db.Database.GetDbConnection();
        var opened = conn.State != System.Data.ConnectionState.Open;
        if (opened) await conn.OpenAsync();
        try
        {
            bool has = false;
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT COUNT(1) FROM pragma_table_info('{table}') WHERE name = '{column}'";
                has = Convert.ToInt32(await cmd.ExecuteScalarAsync() ?? 0) > 0;
            }
            if (has) return;
            await using var alter = conn.CreateCommand();
            alter.CommandText = $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {ddlType}";
            await alter.ExecuteNonQueryAsync();
        }
        finally { if (opened) await conn.CloseAsync(); }
    }

    private static bool SafeIdent(string s) =>
        !string.IsNullOrEmpty(s) && s.All(c => char.IsLetterOrDigit(c) || c == '_');
}
