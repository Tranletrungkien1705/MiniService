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
        if (!await db.Parts.AnyAsync())
        {
            var p1 = new Part { Code = "26300-35505", Name = "Lọc dầu động cơ chính hãng Hyundai", Unit = "Cái", CostPrice = 90000, SalePrice = 180000, InStock = 28, MinStock = 10, Location = "K1-A01", Model = "Accent / Tucson / Elantra" };
            var p2 = new Part { Code = "28113-1R100", Name = "Lọc gió động cơ Hyundai Accent", Unit = "Cái", CostPrice = 120000, SalePrice = 240000, InStock = 14, MinStock = 8, Location = "K1-A04", Model = "Accent" };
            var p3 = new Part { Code = "05100-00441", Name = "Dầu nhờn động cơ Hyundai Premium 5W-30 (Can 4L)", Unit = "Can", CostPrice = 420000, SalePrice = 650000, InStock = 35, MinStock = 15, Location = "K-DAU-01", Model = "Tất cả dòng xe" };
            var p4 = new Part { Code = "58101-C1A00", Name = "Bộ má phanh đĩa trước", Unit = "Bộ", CostPrice = 850000, SalePrice = 1350000, InStock = 6, MinStock = 5, Location = "K2-B02", Model = "Santa Fe / Tucson" };
            var p5 = new Part { Code = "18846-11070", Name = "Bugi đánh lửa Iridium cao cấp", Unit = "Cái", CostPrice = 110000, SalePrice = 220000, InStock = 3, MinStock = 8, Location = "K1-C03", Model = "Tucson / Creta" };
            var p6 = new Part { Code = "97133-D3000", Name = "Lọc gió điều hòa than hoạt tính", Unit = "Cái", CostPrice = 140000, SalePrice = 280000, InStock = 19, MinStock = 6, Location = "K1-A08", Model = "Tucson / Santa Fe" };
            db.Parts.AddRange(p1, p2, p3, p4, p5, p6);
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
            var oilPart = await db.Parts.FirstOrDefaultAsync(p => p.Code == "05100-00441");
            var filterPart = await db.Parts.FirstOrDefaultAsync(p => p.Code == "26300-35505");
            var ro = new RepairOrder { Code = "ROSEED-001", CarId = car.Id, CustomerId = car.CustomerId, Status = ROStatus.InGarage,
                Odometer = 25400, IntakeNote = "Bảo dưỡng 20.000km + thay dầu", Technician = "Thợ Hùng", CreatedBy = "seed", IntakeAt = DateTime.Now.AddHours(-3),
                Lines = [
                    new RepairLine { Type = LineType.Labor, ExpenseType = ExpenseType.Customer, Name = "Công bảo dưỡng cấp 20.000km", Quantity = 1, UnitPrice = 500000 },
                    new RepairLine { Type = LineType.Part, ExpenseType = ExpenseType.Customer, PartId = oilPart?.Id, Name = oilPart?.Name ?? "Dầu động cơ Hyundai 5W-30 (4L)", Quantity = 1, UnitPrice = oilPart?.SalePrice ?? 650000 },
                    new RepairLine { Type = LineType.Part, ExpenseType = ExpenseType.Customer, PartId = filterPart?.Id, Name = filterPart?.Name ?? "Lọc dầu chính hãng", Quantity = 1, UnitPrice = filterPart?.SalePrice ?? 180000 },
                ] };
            db.ROs.Add(ro);
            await db.SaveChangesAsync();
        }

        if (!await db.WarrantyReports.AnyAsync())
        {
            var car1 = await db.Cars.FirstAsync();
            var car2 = await db.Cars.OrderByDescending(c => c.Id).FirstAsync();
            var sparkPart = await db.Parts.FirstOrDefaultAsync(p => p.Code == "18846-11070");
            var filterPart = await db.Parts.FirstOrDefaultAsync(p => p.Code == "26300-35505");

            // Tạo RO bảo hành cho Car 2 (Tucson)
            var roWar = new RepairOrder
            {
                Code = "ROSEED-WAR-001",
                CarId = car2.Id,
                CustomerId = car2.CustomerId,
                Status = ROStatus.Repaired,
                Odometer = 18200,
                IntakeNote = "Bảo hành: Động cơ rung giật khi tăng tốc, đèn Check Engine sáng",
                Technician = "KTV Quang",
                CreatedBy = "seed",
                IntakeAt = DateTime.Now.AddDays(-2),
                Lines = [
                    new RepairLine { Type = LineType.Labor, ExpenseType = ExpenseType.Warranty, Name = "Công kiểm tra & thay thế bugi đánh lửa theo bảo hành", Quantity = 1, UnitPrice = 350000 },
                    new RepairLine { Type = LineType.Part, ExpenseType = ExpenseType.Warranty, PartId = sparkPart?.Id, Name = sparkPart?.Name ?? "Bugi đánh lửa Iridium cao cấp", Quantity = 4, UnitPrice = sparkPart?.SalePrice ?? 220000 }
                ]
            };
            db.ROs.Add(roWar);
            await db.SaveChangesAsync();

            // 1 BCBH đang chờ HTC duyệt (Status = Sent)
            var w1 = new WarrantyReport
            {
                ReportNo = "WAR260427-001",
                ROId = roWar.Id,
                CarId = car2.Id,
                CustomerId = car2.CustomerId,
                Odometer = 18200,
                Status = WarrantyStatus.Sent,
                IssueDescription = "Động cơ rung giật khi tăng tốc ở dải tốc độ 40-60 km/h, đèn Check Engine nhấp nháy.",
                DiagnosticResult = "Lỗi bỏ lửa xy-lanh số 2 (Misfire Cylinder 2) do bugi nứt sứ cách điện, điện áp rò rỉ.",
                ErrorCodeCD = "P0302",
                ErrorCodePN = "ENG-IGN-02",
                PartIDError = sparkPart?.Id,
                ClaimAmount = 1230000,
                CreatedBy = "KTV Quang",
                CreatedAt = DateTime.Now.AddDays(-1),
                SubmittedAt = DateTime.Now.AddHours(-18),
                Items = [
                    new WarrantyReportItem { Type = LineType.Labor, Code = "LAB-IGN-01", Name = "Công kiểm tra & thay thế bugi", Quantity = 1, UnitPrice = 350000 },
                    new WarrantyReportItem { Type = LineType.Part, PartId = sparkPart?.Id, Code = sparkPart?.Code ?? "18846-11070", Name = sparkPart?.Name ?? "Bugi đánh lửa Iridium", Quantity = 4, UnitPrice = sparkPart?.SalePrice ?? 220000 }
                ]
            };

            // 1 BCBH đã được hãng HTC chấp thuận bồi hoàn (Status = Accepted)
            var ro1 = await db.ROs.FirstAsync();
            var w2 = new WarrantyReport
            {
                ReportNo = "WAR260420-002",
                ROId = ro1.Id,
                CarId = car1.Id,
                CustomerId = car1.CustomerId,
                Odometer = 24900,
                Status = WarrantyStatus.Accepted,
                IssueDescription = "Phát hiện rò rỉ dầu qua gioăng nắp giàn cò xupap khi kiểm tra định kỳ.",
                DiagnosticResult = "Gioăng cao su nắp dàn cò bị co ngót biến dạng sớm trong thời gian bảo hành tiêu chuẩn.",
                ErrorCodeCD = "P051B",
                ErrorCodePN = "ENG-LEAK-01",
                PartIDError = filterPart?.Id,
                ClaimAmount = 980000,
                ApprovedAmount = 980000,
                DecisionNote = "Hãng HTC chấp thuận bồi hoàn 100% chi phí phụ tùng và công theo diện bảo hành 3 năm / 100.000km.",
                CreatedBy = "KTV Hùng",
                CreatedAt = DateTime.Now.AddDays(-7),
                SubmittedAt = DateTime.Now.AddDays(-6),
                DecidedAt = DateTime.Now.AddDays(-3),
                Items = [
                    new WarrantyReportItem { Type = LineType.Labor, Code = "LAB-GSK-01", Name = "Công thay thế gioăng nắp dàn cò", Quantity = 1, UnitPrice = 450000 },
                    new WarrantyReportItem { Type = LineType.Part, Code = "22441-2B002", Name = "Gioăng cao su nắp dàn cò chính hãng", Quantity = 1, UnitPrice = 530000 }
                ]
            };

            db.WarrantyReports.AddRange(w1, w2);
            await db.SaveChangesAsync();
        }

        if (!await db.Appointments.AnyAsync())
        {
            var car1 = await db.Cars.FirstAsync();
            var car2 = await db.Cars.OrderByDescending(c => c.Id).FirstAsync();
            var ro1 = await db.ROs.FirstAsync();

            // 1. Cuộc hẹn đã tiếp nhận vào xưởng & liên kết với RO1 (Status = CheckedIn)
            var app1 = new Appointment
            {
                AppNo = "APP260427-001",
                CarId = car1.Id,
                CustomerId = car1.CustomerId,
                AppointmentDate = DateTime.Today.AddHours(8).AddMinutes(30),
                ServiceType = AppointmentServiceType.Maintenance,
                Status = AppointmentStatus.CheckedIn,
                Advisor = "CVDV Tuấn",
                Cavity = "K01 - Khoang bảo dưỡng nhanh",
                CustomerRequest = "Bảo dưỡng định kỳ 20.000km, thay dầu nhớt và lọc dầu chính hãng.",
                Note = "Khách hàng thân thiết, ưu tiên khoang nhanh.",
                Source = "Hyundai Me",
                ROId = ro1.Id,
                CreatedBy = "seed",
                CreatedAt = DateTime.Now.AddDays(-1),
                ConfirmedAt = DateTime.Now.AddHours(-4),
                CheckedInAt = DateTime.Now.AddHours(-3)
            };
            db.Appointments.Add(app1);
            await db.SaveChangesAsync();

            ro1.AppointmentId = app1.Id;

            // 2. Cuộc hẹn hôm nay đã xác nhận, sẵn sàng đón tiếp (Status = Confirmed)
            var app2 = new Appointment
            {
                AppNo = "APP260427-002",
                CarId = car2.Id,
                CustomerId = car2.CustomerId,
                AppointmentDate = DateTime.Today.AddHours(14),
                ServiceType = AppointmentServiceType.Repair,
                Status = AppointmentStatus.Confirmed,
                Advisor = "CVDV Hương",
                Cavity = "K02 - Khoang gầm máy số 1",
                CustomerRequest = "Kiểm tra hệ thống phanh trước, thỉnh thoảng có tiếng kêu rít khi đạp phanh ở tốc độ chậm.",
                Note = "Chuẩn bị sẵn má phanh 58101-C1A00 trong kho.",
                Source = "Hotline",
                CreatedBy = "seed",
                CreatedAt = DateTime.Now.AddDays(-1),
                ConfirmedAt = DateTime.Now.AddHours(-2)
            };

            // 3. Cuộc hẹn mới tạo chờ liên hệ xác nhận (Status = Pending)
            var app3 = new Appointment
            {
                AppNo = "APP260428-003",
                CarId = car1.Id,
                CustomerId = car1.CustomerId,
                AppointmentDate = DateTime.Today.AddDays(1).AddHours(9).AddMinutes(30),
                ServiceType = AppointmentServiceType.Care,
                Status = AppointmentStatus.Pending,
                Advisor = "CVDV Tuấn",
                Cavity = "K05 - Phòng sơn sấy tiêu chuẩn",
                CustomerRequest = "Đánh bóng toàn thân xe và vệ sinh nội thất khử mùi diệt khuẩn.",
                Note = "Khách yêu cầu hoàn thành trước 17h cùng ngày.",
                Source = "Website",
                CreatedBy = "web",
                CreatedAt = DateTime.Now.AddHours(-1)
            };

            // 4. Cuộc hẹn đã bị hủy do khách bận (Status = Cancelled)
            var app4 = new Appointment
            {
                AppNo = "APP260426-004",
                CarId = car2.Id,
                CustomerId = car2.CustomerId,
                AppointmentDate = DateTime.Today.AddDays(-1).AddHours(10),
                ServiceType = AppointmentServiceType.WarrantyCheck,
                Status = AppointmentStatus.Cancelled,
                Advisor = "CVDV Hương",
                CustomerRequest = "Kiểm tra đèn cảnh báo túi khí thỉnh thoảng chớp sáng.",
                CancelReason = "Khách hàng bận đi công tác đột xuất, xin dời sang tuần sau.",
                Source = "Hotline",
                CreatedBy = "seed",
                CreatedAt = DateTime.Now.AddDays(-2)
            };

            db.Appointments.AddRange(app2, app3, app4);
            await db.SaveChangesAsync();
        }

        if (!await db.StockIns.AnyAsync())
        {
            var pOil = await db.Parts.FirstOrDefaultAsync(p => p.Code == "05100-00441");
            var pFilter = await db.Parts.FirstOrDefaultAsync(p => p.Code == "26300-35505");
            var pAir = await db.Parts.FirstOrDefaultAsync(p => p.Code == "28113-1R100");
            var pBrake = await db.Parts.FirstOrDefaultAsync(p => p.Code == "58101-C1A00");

            if (pOil != null && pFilter != null && pAir != null && pBrake != null)
            {
                // 1. Phiếu nhập kho đã hoàn tất (Finished) từ Hyundai Thành Công
                var s1 = new StockIn
                {
                    StockInNo = "NK260420-001",
                    StockInDate = DateTime.Today.AddDays(-7),
                    SupplierName = "Công ty CP Liên doanh Ô tô Hyundai Thành Công Việt Nam (HTC)",
                    BillNo = "HD-2026/004812",
                    Type = StockInType.Normal,
                    Status = StockInStatus.Finished,
                    Description = "Nhập định kỳ phụ tùng bảo dưỡng tiêu chuẩn theo đơn đặt hàng PO-2604-01.",
                    CreatedBy = "Thủ kho Tuấn",
                    CreatedAt = DateTime.Now.AddDays(-7).AddHours(-4),
                    ApprovedBy = "Kế toán trưởng",
                    FinishedAt = DateTime.Now.AddDays(-7).AddHours(-2),
                    Items = [
                        new StockInDetail { PartId = pOil.Id, PartCode = pOil.Code, PartName = pOil.Name, Unit = pOil.Unit, Quantity = 20, UnitPrice = 420000, VatPercent = 8, Location = pOil.Location, Note = "Dầu nhớt chính hãng theo lô HTC-2026-04" },
                        new StockInDetail { PartId = pFilter.Id, PartCode = pFilter.Code, PartName = pFilter.Name, Unit = pFilter.Unit, Quantity = 30, UnitPrice = 90000, VatPercent = 8, Location = pFilter.Location, Note = "Lọc dầu động cơ tiêu chuẩn" },
                        new StockInDetail { PartId = pAir.Id, PartCode = pAir.Code, PartName = pAir.Name, Unit = pAir.Unit, Quantity = 15, UnitPrice = 120000, VatPercent = 8, Location = pAir.Location, Note = "Lọc gió động cơ Accent" }
                    ]
                };

                // 2. Phiếu nhập kho đang chờ kiểm hàng (Pending) từ Mobis
                var s2 = new StockIn
                {
                    StockInNo = "NK260427-002",
                    StockInDate = DateTime.Today,
                    SupplierName = "Công ty TNHH Phụ tùng Mobis Việt Nam",
                    BillNo = "MBS-88219/26",
                    Type = StockInType.Normal,
                    Status = StockInStatus.Pending,
                    Description = "Bổ sung má phanh gấp cho xe làm dịch vụ trong tuần.",
                    CreatedBy = "Thủ kho Tuấn",
                    CreatedAt = DateTime.Now.AddHours(-2),
                    Items = [
                        new StockInDetail { PartId = pBrake.Id, PartCode = pBrake.Code, PartName = pBrake.Name, Unit = pBrake.Unit, Quantity = 10, UnitPrice = 850000, VatPercent = 8, Location = pBrake.Location, Note = "Má phanh trước chính hãng Mobis" }
                    ]
                };

                db.StockIns.AddRange(s1, s2);
                await db.SaveChangesAsync();
            }
        }

        if (!await db.StockOuts.AnyAsync())
        {
            var pOil = await db.Parts.FirstOrDefaultAsync(p => p.Code == "05100-00441");
            var pFilter = await db.Parts.FirstOrDefaultAsync(p => p.Code == "26300-35505");
            var pBrake = await db.Parts.FirstOrDefaultAsync(p => p.Code == "58101-C1A00");
            var ro1 = await db.ROs.Include(r => r.Car).Include(r => r.Customer).FirstOrDefaultAsync();

            if (pOil != null && pFilter != null && ro1 != null)
            {
                // 1. Phiếu xuất kho phụ tùng sửa chữa hoàn tất cho RO1 (Status = Finished)
                var x1 = new StockOut
                {
                    StockOutNo = "XK260427-001",
                    StockOutDate = DateTime.Today,
                    Type = StockOutType.Service,
                    Status = StockOutStatus.Finished,
                    ROId = ro1.Id,
                    CarId = ro1.CarId,
                    CustomerId = ro1.CustomerId,
                    RecipientName = ro1.Technician ?? "Thợ Hùng",
                    Description = $"Xuất vật tư bảo dưỡng định kỳ 20.000km cho lệnh {ro1.Code} (xe {ro1.Car.Plate}).",
                    CreatedBy = "Thủ kho Tuấn",
                    CreatedAt = DateTime.Now.AddHours(-3),
                    ApprovedBy = "Thủ kho Tuấn",
                    FinishedAt = DateTime.Now.AddHours(-2).AddMinutes(-45),
                    Items = [
                        new StockOutDetail { PartId = pOil.Id, PartCode = pOil.Code, PartName = pOil.Name, Unit = pOil.Unit, Quantity = 1, UnitPrice = pOil.SalePrice, VatPercent = 8, Location = pOil.Location, Note = "Dầu nhớt Hyundai 5W-30 can 4L" },
                        new StockOutDetail { PartId = pFilter.Id, PartCode = pFilter.Code, PartName = pFilter.Name, Unit = pFilter.Unit, Quantity = 1, UnitPrice = pFilter.SalePrice, VatPercent = 8, Location = pFilter.Location, Note = "Lọc dầu chính hãng" }
                    ]
                };

                // 2. Phiếu xuất kho đang chuẩn bị soạn hàng cho khách lẻ / sửa chữa (Status = Pending)
                var x2 = new StockOut
                {
                    StockOutNo = "XK260427-002",
                    StockOutDate = DateTime.Today,
                    Type = StockOutType.Normal,
                    Status = StockOutStatus.Pending,
                    RecipientName = "Nguyễn Hoàng Long (Khách vãng lai)",
                    Description = "Xuất bán lẻ phụ tùng má phanh trước cho khách tự thay thế.",
                    CreatedBy = "Thủ kho Tuấn",
                    CreatedAt = DateTime.Now.AddHours(-1),
                    Items = [
                        new StockOutDetail { PartId = pBrake != null ? pBrake.Id : pFilter.Id, PartCode = pBrake != null ? pBrake.Code : pFilter.Code, PartName = pBrake != null ? pBrake.Name : pFilter.Name, Unit = pBrake != null ? pBrake.Unit : pFilter.Unit, Quantity = 1, UnitPrice = pBrake != null ? pBrake.SalePrice : pFilter.SalePrice, VatPercent = 8, Location = pBrake?.Location, Note = "Bán lẻ thanh toán ngay" }
                    ]
                };

                db.StockOuts.AddRange(x1, x2);
                await db.SaveChangesAsync();
            }
        }

        if (!await db.CustomerCares.AnyAsync())
        {
            var car1 = await db.Cars.FirstAsync();
            var car2 = await db.Cars.OrderByDescending(c => c.Id).FirstAsync();
            var oilPart = await db.Parts.FirstOrDefaultAsync(p => p.Code == "05100-00441");
            var filterPart = await db.Parts.FirstOrDefaultAsync(p => p.Code == "26300-35505");
            var brakePart = await db.Parts.FirstOrDefaultAsync(p => p.Code == "58101-C1A00");

            // Tạo các RO hoàn tất (Finished) để demo quy trình CSKH 24h
            var roFns1 = new RepairOrder
            {
                Code = "ROSEED-FNS-001",
                CarId = car1.Id,
                CustomerId = car1.CustomerId,
                Status = ROStatus.Finished,
                Odometer = 26000,
                IntakeNote = "Bảo dưỡng cấp 25.000km, kiểm tra dầu phanh & nước làm mát.",
                Technician = "Thợ Hùng",
                CreatedBy = "seed",
                CreatedAt = DateTime.Now.AddDays(-2),
                IntakeAt = DateTime.Now.AddDays(-2).AddHours(1),
                FinishedAt = DateTime.Now.AddDays(-1).AddHours(-4),
                Lines = [
                    new RepairLine { Type = LineType.Labor, ExpenseType = ExpenseType.Customer, Name = "Công bảo dưỡng 25.000km", Quantity = 1, UnitPrice = 450000 },
                    new RepairLine { Type = LineType.Part, ExpenseType = ExpenseType.Customer, PartId = oilPart?.Id, Name = oilPart?.Name ?? "Dầu nhớt động cơ", Quantity = 1, UnitPrice = oilPart?.SalePrice ?? 650000 },
                    new RepairLine { Type = LineType.Part, ExpenseType = ExpenseType.Customer, PartId = filterPart?.Id, Name = filterPart?.Name ?? "Lọc dầu động cơ", Quantity = 1, UnitPrice = filterPart?.SalePrice ?? 180000 }
                ]
            };

            var roFns2 = new RepairOrder
            {
                Code = "ROSEED-FNS-002",
                CarId = car2.Id,
                CustomerId = car2.CustomerId,
                Status = ROStatus.Finished,
                Odometer = 18800,
                IntakeNote = "Thay má phanh trước & láng đĩa phanh.",
                Technician = "KTV Quang",
                CreatedBy = "seed",
                CreatedAt = DateTime.Now.AddDays(-1),
                IntakeAt = DateTime.Now.AddDays(-1).AddHours(2),
                FinishedAt = DateTime.Now.AddHours(-6),
                Lines = [
                    new RepairLine { Type = LineType.Labor, ExpenseType = ExpenseType.Customer, Name = "Công thay má phanh & láng đĩa trước", Quantity = 1, UnitPrice = 400000 },
                    new RepairLine { Type = LineType.Part, ExpenseType = ExpenseType.Customer, PartId = brakePart?.Id, Name = brakePart?.Name ?? "Bộ má phanh đĩa trước", Quantity = 1, UnitPrice = brakePart?.SalePrice ?? 1350000 }
                ]
            };

            var roFns3 = new RepairOrder
            {
                Code = "ROSEED-FNS-003",
                CarId = car1.Id,
                CustomerId = car1.CustomerId,
                Status = ROStatus.Finished,
                Odometer = 26200,
                IntakeNote = "Thay lọc gió điều hòa than hoạt tính và khử mùi dàn lạnh.",
                Technician = "Thợ Hùng",
                CreatedBy = "seed",
                CreatedAt = DateTime.Today.AddHours(-5),
                IntakeAt = DateTime.Today.AddHours(-4),
                FinishedAt = DateTime.Today.AddHours(-1),
                Lines = [
                    new RepairLine { Type = LineType.Labor, ExpenseType = ExpenseType.Customer, Name = "Công vệ sinh khử mùi điều hòa", Quantity = 1, UnitPrice = 250000 },
                    new RepairLine { Type = LineType.Part, ExpenseType = ExpenseType.Customer, Name = "Lọc gió điều hòa than hoạt tính", Quantity = 1, UnitPrice = 280000 }
                ]
            };

            db.ROs.AddRange(roFns1, roFns2, roFns3);
            await db.SaveChangesAsync();

            // 1. Phiếu CSKH 24h đã khảo sát - Khách hàng rất hài lòng (CIFB)
            var c1 = new CustomerCare
            {
                CareNo = "CC260426-001",
                ROId = roFns1.Id,
                CarId = roFns1.CarId,
                CustomerId = roFns1.CustomerId,
                Status = CustomerCareStatus.ContactedSatisfied,
                HasCarProblem = false,
                QualityRating = 1, // Rất tốt
                StaffRating = 1,   // Rất tốt
                WillingToReturn = true,
                FacilityRating = 1,// Rất tốt
                CustomerFeedback = "Xe chạy rất bốc và êm sau bảo dưỡng. Cố vấn dịch vụ tư vấn rõ ràng, xưởng bàn giao xe sạch sẽ.",
                InternalNote = "Khách hàng hài lòng cao. Gửi thư cảm ơn tự động.",
                ContactedBy = "CSKH Thu Trang",
                ContactedDate = DateTime.Now.AddHours(-18),
                CreatedBy = "system",
                CreatedAt = DateTime.Now.AddDays(-1).AddHours(-4)
            };

            // 2. Phiếu CSKH 24h đã liên hệ - Phát sinh khiếu nại kỹ thuật cần xử lý (CINFB)
            var c2 = new CustomerCare
            {
                CareNo = "CC260427-002",
                ROId = roFns2.Id,
                CarId = roFns2.CarId,
                CustomerId = roFns2.CustomerId,
                Status = CustomerCareStatus.NeedFeedback,
                HasCarProblem = true,
                QualityRating = 3, // Bình thường
                StaffRating = 2,   // Tốt
                WillingToReturn = true,
                FacilityRating = 2,// Đạt yêu cầu
                CustomerFeedback = "Sau khi thay má phanh đi chậm vẫn nghe tiếng ken két nhỏ ở bánh trước bên phụ khi rà phanh nhẹ. Nhờ xưởng kiểm tra lại.",
                InternalNote = "Đã chuyển thông tin cho KTV Quang & Tổ trưởng sửa chữa. CVDV gọi hẹn khách mang xe vào căn chỉnh miễn phí sáng mai.",
                ContactedBy = "CSKH Thu Trang",
                ContactedDate = DateTime.Now.AddHours(-4),
                CreatedBy = "system",
                CreatedAt = DateTime.Now.AddHours(-6)
            };

            // 3. Phiếu CSKH 24h mới sinh ra khi giao xe, đang chờ liên hệ (PEND)
            var c3 = new CustomerCare
            {
                CareNo = "CC260427-003",
                ROId = roFns3.Id,
                CarId = roFns3.CarId,
                CustomerId = roFns3.CustomerId,
                Status = CustomerCareStatus.Pending,
                HasCarProblem = false,
                CreatedBy = "system",
                CreatedAt = DateTime.Now.AddHours(-1)
            };

            db.CustomerCares.AddRange(c1, c2, c3);
            await db.SaveChangesAsync();
        }

        if (!await db.Payments.AnyAsync())
        {
            var roFns1 = await db.ROs.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Code == "ROSEED-FNS-001");
            var roFns2 = await db.ROs.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Code == "ROSEED-FNS-002");
            var ro1 = await db.ROs.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Code == "ROSEED-001");

            var payments = new List<Payment>();

            if (roFns1 != null)
            {
                payments.Add(new Payment
                {
                    PaymentNo = "PT260426-001",
                    ROId = roFns1.Id,
                    CustomerId = roFns1.CustomerId,
                    CarId = roFns1.CarId,
                    PaymentDate = DateTime.Today.AddDays(-1),
                    Method = PaymentMethod.Cash,
                    Status = PaymentStatus.Completed,
                    PayPersonName = "Nguyễn Văn An",
                    PayPersonPhone = "0901111111",
                    RoTotalAmount = roFns1.Total,
                    DiscountAmount = 0,
                    ThirdPartyAmount = roFns1.WarrantyTotal,
                    PayableAmount = roFns1.CustomerTotal,
                    PaymentAmount = roFns1.CustomerTotal,
                    Note = "Khách thanh toán tiền mặt đầy đủ tại quầy thu ngân sau khi nghiệm thu xe.",
                    Cashier = "Thu ngân Mai",
                    CreatedBy = "seed",
                    CreatedAt = DateTime.Now.AddDays(-1).AddHours(-4),
                    CompletedAt = DateTime.Now.AddDays(-1).AddHours(-4)
                });
            }

            if (roFns2 != null)
            {
                payments.Add(new Payment
                {
                    PaymentNo = "PT260427-002",
                    ROId = roFns2.Id,
                    CustomerId = roFns2.CustomerId,
                    CarId = roFns2.CarId,
                    PaymentDate = DateTime.Today,
                    Method = PaymentMethod.PosCard,
                    Status = PaymentStatus.Completed,
                    PayPersonName = "Trần Thị Bình",
                    PayPersonPhone = "0902222222",
                    RoTotalAmount = roFns2.Total,
                    DiscountAmount = 50000,
                    ThirdPartyAmount = roFns2.WarrantyTotal,
                    PayableAmount = roFns2.CustomerTotal - 50000,
                    PaymentAmount = roFns2.CustomerTotal - 50000,
                    TransactionRef = "POS-MB-982143",
                    Note = "Quẹt thẻ Visa Vietcombank qua máy POS MB Bank. Áp dụng voucher giảm giá 50.000đ thành viên thân thiết.",
                    Cashier = "Thu ngân Mai",
                    CreatedBy = "seed",
                    CreatedAt = DateTime.Now.AddHours(-6),
                    CompletedAt = DateTime.Now.AddHours(-6)
                });
            }

            if (ro1 != null)
            {
                payments.Add(new Payment
                {
                    PaymentNo = "PT260427-003",
                    ROId = ro1.Id,
                    CustomerId = ro1.CustomerId,
                    CarId = ro1.CarId,
                    PaymentDate = DateTime.Today,
                    Method = PaymentMethod.BankTransfer,
                    Status = PaymentStatus.Draft,
                    PayPersonName = "Nguyễn Văn An",
                    PayPersonPhone = "0901111111",
                    RoTotalAmount = ro1.Total,
                    DiscountAmount = 0,
                    ThirdPartyAmount = ro1.WarrantyTotal,
                    PayableAmount = ro1.CustomerTotal,
                    PaymentAmount = ro1.CustomerTotal,
                    TransactionRef = "MB-FT260427-0091",
                    Note = "Khách hẹn chuyển khoản qua tài khoản ngân hàng đại lý MB Bank khi đến nhận xe.",
                    Cashier = "Thu ngân Mai",
                    CreatedBy = "seed",
                    CreatedAt = DateTime.Now.AddHours(-1)
                });
            }

            if (payments.Count > 0)
            {
                db.Payments.AddRange(payments);
                await db.SaveChangesAsync();
            }
        }

        if (!await db.Quotes.AnyAsync())
        {
            var car1 = await db.Cars.Include(c => c.Customer).FirstAsync();
            var car2 = await db.Cars.Include(c => c.Customer).OrderByDescending(c => c.Id).FirstAsync();
            var pOil = await db.Parts.FirstOrDefaultAsync(p => p.Code == "05100-00441");
            var pFilter = await db.Parts.FirstOrDefaultAsync(p => p.Code == "26300-35505");
            var pAir = await db.Parts.FirstOrDefaultAsync(p => p.Code == "28113-1R100");
            var pBrake = await db.Parts.FirstOrDefaultAsync(p => p.Code == "58101-C1A00");
            var pSpark = await db.Parts.FirstOrDefaultAsync(p => p.Code == "18846-11070");
            var stockOut1 = await db.StockOuts.FirstOrDefaultAsync(s => s.StockOutNo == "XK260427-001");

            var quotes = new List<Quote>();

            // 1. Báo giá mới lập (Draft) - Tư vấn phụ tùng bảo dưỡng cấp trung bình
            var q1 = new Quote
            {
                QuoteNo = "BG260427-001",
                QuoteDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(15),
                CustomerId = car1.CustomerId,
                CustomerName = car1.Customer.Name,
                CustomerPhone = car1.Customer.Phone,
                CustomerAddress = "Cầu Giấy, Hà Nội",
                CarId = car1.Id,
                RecipientName = car1.Customer.Name,
                PaymentMethod = PaymentMethod.Cash,
                Status = QuoteStatus.Draft,
                Remark = "Báo giá có giá trị trong 15 ngày kể từ ngày lập. Phụ tùng chính hãng bảo hành 12 tháng hoặc 20.000km.",
                Note = "Khách hàng tham khảo chi phí bảo dưỡng định kỳ tiếp theo.",
                CreatedBy = "CVDV Tuấn",
                CreatedAt = DateTime.Now.AddHours(-3),
                Items = [
                    new QuoteItem { PartId = pOil?.Id, PartCode = pOil?.Code ?? "05100-00441", PartName = pOil?.Name ?? "Dầu nhớt động cơ Hyundai 5W-30", Unit = pOil?.Unit ?? "Can", Quantity = 1, UnitPrice = pOil?.SalePrice ?? 650000, DiscountPercent = 5, VatPercent = 8, Note = "Giảm giá 5% thành viên" },
                    new QuoteItem { PartId = pFilter?.Id, PartCode = pFilter?.Code ?? "26300-35505", PartName = pFilter?.Name ?? "Lọc dầu động cơ chính hãng", Unit = pFilter?.Unit ?? "Cái", Quantity = 1, UnitPrice = pFilter?.SalePrice ?? 180000, DiscountPercent = 0, VatPercent = 8, Note = "Thay mới theo dầu" },
                    new QuoteItem { PartId = pAir?.Id, PartCode = pAir?.Code ?? "28113-1R100", PartName = pAir?.Name ?? "Lọc gió động cơ Accent", Unit = pAir?.Unit ?? "Cái", Quantity = 1, UnitPrice = pAir?.SalePrice ?? 240000, DiscountPercent = 0, VatPercent = 8, Note = "Vệ sinh hoặc thay thế" }
                ]
            };
            quotes.Add(q1);

            // 2. Báo giá đã được khách hàng đồng ý (Confirmed) - Chờ xuất kho hoặc đưa xe vào xưởng
            var q2 = new Quote
            {
                QuoteNo = "BG260427-002",
                QuoteDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(10),
                CustomerId = car2.CustomerId,
                CustomerName = car2.Customer.Name,
                CustomerPhone = car2.Customer.Phone,
                CustomerAddress = "Thanh Xuân, Hà Nội",
                CarId = car2.Id,
                RecipientName = car2.Customer.Name,
                PaymentMethod = PaymentMethod.BankTransfer,
                Status = QuoteStatus.Confirmed,
                Remark = "Giá đã bao gồm phụ tùng chính hãng Mobis và thuế GTGT 8%. Cam kết phụ tùng mới 100%.",
                Note = "Khách xác nhận thay má phanh và 4 bugi đánh lửa, hẹn chiều mang xe qua xưởng.",
                CreatedBy = "CVDV Hương",
                CreatedAt = DateTime.Now.AddHours(-2),
                ConfirmedAt = DateTime.Now.AddHours(-1),
                Items = [
                    new QuoteItem { PartId = pBrake?.Id, PartCode = pBrake?.Code ?? "58101-C1A00", PartName = pBrake?.Name ?? "Bộ má phanh đĩa trước", Unit = pBrake?.Unit ?? "Bộ", Quantity = 1, UnitPrice = pBrake?.SalePrice ?? 1350000, DiscountPercent = 0, VatPercent = 8, Note = "Má phanh chính hãng" },
                    new QuoteItem { PartId = pSpark?.Id, PartCode = pSpark?.Code ?? "18846-11070", PartName = pSpark?.Name ?? "Bugi đánh lửa Iridium cao cấp", Unit = pSpark?.Unit ?? "Cái", Quantity = 4, UnitPrice = pSpark?.SalePrice ?? 220000, DiscountPercent = 5, VatPercent = 8, Note = "Chiết khấu combo bugi 5%" }
                ]
            };
            quotes.Add(q2);

            // 3. Báo giá đã chuyển đổi thành Phiếu xuất kho (Converted)
            var q3 = new Quote
            {
                QuoteNo = "BG260420-003",
                QuoteDate = DateTime.Today.AddDays(-7),
                ValidUntil = DateTime.Today.AddDays(7),
                CustomerId = car1.CustomerId,
                CustomerName = car1.Customer.Name,
                CustomerPhone = car1.Customer.Phone,
                CustomerAddress = "Cầu Giấy, Hà Nội",
                CarId = car1.Id,
                RecipientName = "Thợ Hùng",
                PaymentMethod = PaymentMethod.Cash,
                Status = QuoteStatus.Converted,
                StockOutId = stockOut1?.Id,
                Remark = "Đã xuất kho theo phiếu XK260427-001.",
                Note = "Báo giá vật tư bảo dưỡng đã xuất kho cho xe vào bảo dưỡng cấp 20.000km.",
                CreatedBy = "CVDV Tuấn",
                CreatedAt = DateTime.Now.AddDays(-7),
                ConfirmedAt = DateTime.Now.AddDays(-7).AddHours(1),
                Items = [
                    new QuoteItem { PartId = pOil?.Id, PartCode = pOil?.Code ?? "05100-00441", PartName = pOil?.Name ?? "Dầu nhờn động cơ Hyundai 5W-30", Unit = pOil?.Unit ?? "Can", Quantity = 1, UnitPrice = pOil?.SalePrice ?? 650000, DiscountPercent = 0, VatPercent = 8, Note = "Dầu động cơ 4L" },
                    new QuoteItem { PartId = pFilter?.Id, PartCode = pFilter?.Code ?? "26300-35505", PartName = pFilter?.Name ?? "Lọc dầu động cơ", Unit = pFilter?.Unit ?? "Cái", Quantity = 1, UnitPrice = pFilter?.SalePrice ?? 180000, DiscountPercent = 0, VatPercent = 8, Note = "Lọc dầu chính hãng" }
                ]
            };
            quotes.Add(q3);

            db.Quotes.AddRange(quotes);
            await db.SaveChangesAsync();

            if (stockOut1 != null)
            {
                stockOut1.QuoteId = q3.Id;
                await db.SaveChangesAsync();
            }
        }

        if (!await db.ServicePackages.AnyAsync())
        {
            var pOil = await db.Parts.FirstOrDefaultAsync(p => p.Code == "05100-00441");
            var pFilter = await db.Parts.FirstOrDefaultAsync(p => p.Code == "26300-35505");
            var pAir = await db.Parts.FirstOrDefaultAsync(p => p.Code == "28113-1R100");
            var pCabin = await db.Parts.FirstOrDefaultAsync(p => p.Code == "97133-D3000");
            var pBrake = await db.Parts.FirstOrDefaultAsync(p => p.Code == "58101-C1A00");
            var pSpark = await db.Parts.FirstOrDefaultAsync(p => p.Code == "18846-11070");

            var packages = new List<ServicePackage>();

            // 1. Gói BD Cấp 1 (5.000 km)
            packages.Add(new ServicePackage
            {
                PackageNo = "PKG-BD-5K",
                Name = "Gói bảo dưỡng định kỳ Cấp 1 (5.000 km)",
                TakingTimeHours = 0.8m,
                Description = "Kiểm tra tổng quát 20 hạng mục tiêu chuẩn Hyundai, thay dầu máy & lọc nhớt động cơ chính hãng.",
                IsPublic = true,
                IsActive = true,
                CreatedBy = "system",
                CreatedAt = DateTime.Now.AddDays(-30),
                Items = [
                    new ServicePackageItem { Type = LineType.Labor, Code = "SRV-BD-01", Name = "Công bảo dưỡng cấp 1 (kiểm tra gầm, siết ốc, áp suất lốp, nước rửa kính)", Unit = "Lần", Quantity = 1, UnitPrice = 250000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Định mức 0.8 giờ công" },
                    new ServicePackageItem { Type = LineType.Part, PartId = pOil?.Id, Code = pOil?.Code ?? "05100-00441", Name = pOil?.Name ?? "Dầu nhờn động cơ Hyundai 5W-30 (Can 4L)", Unit = pOil?.Unit ?? "Can", Quantity = 1, UnitPrice = pOil?.SalePrice ?? 650000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Dầu nhớt chính hãng HTC" },
                    new ServicePackageItem { Type = LineType.Part, PartId = pFilter?.Id, Code = pFilter?.Code ?? "26300-35505", Name = pFilter?.Name ?? "Lọc dầu động cơ chính hãng Hyundai", Unit = pFilter?.Unit ?? "Cái", Quantity = 1, UnitPrice = pFilter?.SalePrice ?? 180000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Thay mới lọc dầu" }
                ]
            });

            // 2. Gói BD Cấp 2 (10.000 km)
            packages.Add(new ServicePackage
            {
                PackageNo = "PKG-BD-10K",
                Name = "Gói bảo dưỡng định kỳ Cấp 2 (10.000 km)",
                TakingTimeHours = 1.2m,
                Description = "Bao gồm cấp 1 + Vệ sinh lọc gió động cơ, lọc gió máy lạnh, bảo dưỡng 4 cụm phanh đĩa và đảo lốp cân mâm.",
                IsPublic = true,
                IsActive = true,
                CreatedBy = "system",
                CreatedAt = DateTime.Now.AddDays(-25),
                Items = [
                    new ServicePackageItem { Type = LineType.Labor, Code = "SRV-BD-02", Name = "Công bảo dưỡng cấp 2, bảo dưỡng hệ thống phanh 4 bánh & đảo lốp", Unit = "Lần", Quantity = 1, UnitPrice = 380000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Định mức 1.2 giờ công" },
                    new ServicePackageItem { Type = LineType.Part, PartId = pOil?.Id, Code = pOil?.Code ?? "05100-00441", Name = pOil?.Name ?? "Dầu nhờn động cơ Hyundai 5W-30 (Can 4L)", Unit = pOil?.Unit ?? "Can", Quantity = 1, UnitPrice = pOil?.SalePrice ?? 650000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Dầu nhớt chính hãng" },
                    new ServicePackageItem { Type = LineType.Part, PartId = pFilter?.Id, Code = pFilter?.Code ?? "26300-35505", Name = pFilter?.Name ?? "Lọc dầu động cơ chính hãng Hyundai", Unit = pFilter?.Unit ?? "Cái", Quantity = 1, UnitPrice = pFilter?.SalePrice ?? 180000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Thay mới lọc dầu" },
                    new ServicePackageItem { Type = LineType.Part, PartId = pAir?.Id, Code = pAir?.Code ?? "28113-1R100", Name = pAir?.Name ?? "Lọc gió động cơ Hyundai Accent", Unit = pAir?.Unit ?? "Cái", Quantity = 1, UnitPrice = pAir?.SalePrice ?? 240000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Vệ sinh hoặc thay mới lọc gió" }
                ]
            });

            // 3. Gói BD Cấp 3 (20.000 km)
            packages.Add(new ServicePackage
            {
                PackageNo = "PKG-BD-20K",
                Name = "Gói bảo dưỡng định kỳ Cấp 3 (20.000 km)",
                TakingTimeHours = 1.8m,
                Description = "Bảo dưỡng trung bình: Thay dầu nhớt, lọc dầu, lọc gió động cơ, lọc gió cabin máy lạnh than hoạt tính, vệ sinh kim phun buồng đốt.",
                IsPublic = true,
                IsActive = true,
                CreatedBy = "system",
                CreatedAt = DateTime.Now.AddDays(-20),
                Items = [
                    new ServicePackageItem { Type = LineType.Labor, Code = "SRV-BD-03", Name = "Công bảo dưỡng cấp 3 toàn diện khoang máy, hệ thống phanh và gầm xe", Unit = "Lần", Quantity = 1, UnitPrice = 520000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Định mức 1.8 giờ công" },
                    new ServicePackageItem { Type = LineType.Part, PartId = pOil?.Id, Code = pOil?.Code ?? "05100-00441", Name = pOil?.Name ?? "Dầu nhờn động cơ Hyundai 5W-30 (Can 4L)", Unit = pOil?.Unit ?? "Can", Quantity = 1, UnitPrice = pOil?.SalePrice ?? 650000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Dầu nhớt chính hãng" },
                    new ServicePackageItem { Type = LineType.Part, PartId = pFilter?.Id, Code = pFilter?.Code ?? "26300-35505", Name = pFilter?.Name ?? "Lọc dầu động cơ chính hãng Hyundai", Unit = pFilter?.Unit ?? "Cái", Quantity = 1, UnitPrice = pFilter?.SalePrice ?? 180000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Thay mới lọc dầu" },
                    new ServicePackageItem { Type = LineType.Part, PartId = pAir?.Id, Code = pAir?.Code ?? "28113-1R100", Name = pAir?.Name ?? "Lọc gió động cơ Hyundai Accent", Unit = pAir?.Unit ?? "Cái", Quantity = 1, UnitPrice = pAir?.SalePrice ?? 240000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Thay mới lọc gió động cơ" },
                    new ServicePackageItem { Type = LineType.Part, PartId = pCabin?.Id, Code = pCabin?.Code ?? "97133-D3000", Name = pCabin?.Name ?? "Lọc gió điều hòa than hoạt tính", Unit = pCabin?.Unit ?? "Cái", Quantity = 1, UnitPrice = pCabin?.SalePrice ?? 280000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Khử mùi diệt khuẩn điều hòa" }
                ]
            });

            // 4. Gói BD Cấp 4 (40.000 km) - Đại tu định kỳ
            packages.Add(new ServicePackage
            {
                PackageNo = "PKG-BD-40K",
                Name = "Gói bảo dưỡng định kỳ Cấp 4 (40.000 km) — Đại dưỡng toàn diện",
                TakingTimeHours = 3.0m,
                Description = "Gói bảo dưỡng lớn cao cấp theo khuyến cáo Hyundai Motor: Thay dầu máy, lọc dầu, lọc gió động cơ, lọc gió điều hòa than hoạt tính, thay 4 bugi Iridium mới, thay má phanh đĩa.",
                IsPublic = true,
                IsActive = true,
                CreatedBy = "system",
                CreatedAt = DateTime.Now.AddDays(-15),
                Items = [
                    new ServicePackageItem { Type = LineType.Labor, Code = "SRV-BD-04", Name = "Công đại dưỡng toàn diện cấp 40.000km (khoang động cơ, gầm, phanh, bugi, điện)", Unit = "Lần", Quantity = 1, UnitPrice = 900000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Định mức 3.0 giờ công thợ bậc cao" },
                    new ServicePackageItem { Type = LineType.Part, PartId = pOil?.Id, Code = pOil?.Code ?? "05100-00441", Name = pOil?.Name ?? "Dầu nhờn động cơ Hyundai 5W-30 (Can 4L)", Unit = pOil?.Unit ?? "Can", Quantity = 1, UnitPrice = pOil?.SalePrice ?? 650000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Dầu nhớt chính hãng" },
                    new ServicePackageItem { Type = LineType.Part, PartId = pFilter?.Id, Code = pFilter?.Code ?? "26300-35505", Name = pFilter?.Name ?? "Lọc dầu động cơ chính hãng Hyundai", Unit = pFilter?.Unit ?? "Cái", Quantity = 1, UnitPrice = pFilter?.SalePrice ?? 180000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Thay mới lọc dầu" },
                    new ServicePackageItem { Type = LineType.Part, PartId = pAir?.Id, Code = pAir?.Code ?? "28113-1R100", Name = pAir?.Name ?? "Lọc gió động cơ Hyundai Accent", Unit = pAir?.Unit ?? "Cái", Quantity = 1, UnitPrice = pAir?.SalePrice ?? 240000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Thay mới lọc gió động cơ" },
                    new ServicePackageItem { Type = LineType.Part, PartId = pCabin?.Id, Code = pCabin?.Code ?? "97133-D3000", Name = pCabin?.Name ?? "Lọc gió điều hòa than hoạt tính", Unit = pCabin?.Unit ?? "Cái", Quantity = 1, UnitPrice = pCabin?.SalePrice ?? 280000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Khử mùi diệt khuẩn điều hòa" },
                    new ServicePackageItem { Type = LineType.Part, PartId = pSpark?.Id, Code = pSpark?.Code ?? "18846-11070", Name = pSpark?.Name ?? "Bugi đánh lửa Iridium cao cấp", Unit = pSpark?.Unit ?? "Cái", Quantity = 4, UnitPrice = pSpark?.SalePrice ?? 220000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Bộ 4 bugi đánh lửa Iridium" },
                    new ServicePackageItem { Type = LineType.Part, PartId = pBrake?.Id, Code = pBrake?.Code ?? "58101-C1A00", Name = pBrake?.Name ?? "Bộ má phanh đĩa trước", Unit = pBrake?.Unit ?? "Bộ", Quantity = 1, UnitPrice = pBrake?.SalePrice ?? 1350000, VatPercent = 8, ExpenseType = ExpenseType.Customer, Note = "Thay má phanh an toàn" }
                ]
            });

            db.ServicePackages.AddRange(packages);
            await db.SaveChangesAsync();
        }

        if (!await db.OrderParts.AnyAsync())
        {
            var pOil = await db.Parts.FirstOrDefaultAsync(p => p.Code == "05100-00441");
            var pFilter = await db.Parts.FirstOrDefaultAsync(p => p.Code == "26300-35505");
            var pAir = await db.Parts.FirstOrDefaultAsync(p => p.Code == "28113-1R100");
            var pBrake = await db.Parts.FirstOrDefaultAsync(p => p.Code == "58101-C1A00");
            var pSpark = await db.Parts.FirstOrDefaultAsync(p => p.Code == "18846-11070");
            var sFinished = await db.StockIns.FirstOrDefaultAsync(s => s.StockInNo == "NK260420-001");
            var ro1 = await db.ROs.Include(r => r.Car).FirstOrDefaultAsync();

            var orderParts = new List<OrderPart>();

            // 1. Đơn đặt hàng định kỳ đã nhập kho hoàn tất (Status = Finished)
            var op1 = new OrderPart
            {
                OrderPartNo = "PO260420-001",
                OrderDate = DateTime.Today.AddDays(-7),
                SupplierName = "Công ty CP Liên doanh Ô tô Hyundai Thành Công Việt Nam (HTC)",
                DeliveryForm = OrderPartDeliveryForm.Normal,
                DeliveryLocation = "Kho phụ tùng chính - Cầu Giấy",
                EstimatedDeliverDate = DateTime.Today.AddDays(-7),
                Status = OrderPartStatus.Finished,
                OrderSuppierNo = "HTC-SO-2026-8812",
                RequestSuppierDate = DateTime.Today.AddDays(-8),
                ResponseSuppierDate = DateTime.Today.AddDays(-7),
                Remark = "Đơn đặt hàng định kỳ bổ sung vật tư dầu nhờn & lọc bảo dưỡng nhanh.",
                StockInId = sFinished?.Id,
                CreatedBy = "Quản lý kho",
                CreatedAt = DateTime.Now.AddDays(-8),
                ApprovedAt = DateTime.Now.AddDays(-8).AddHours(2),
                FinishedAt = DateTime.Now.AddDays(-7),
                Lines = [
                    new OrderPartLine { PartId = pOil?.Id ?? 1, PartCode = pOil?.Code ?? "05100-00441", PartName = pOil?.Name ?? "Dầu nhờn động cơ Hyundai 5W-30", Unit = pOil?.Unit ?? "Can", Quantity = 20, UnitPrice = 420000, DiscountRate = 3, VatPercent = 8, ApprovedQuantity = 20, ReceivedQuantity = 20, StatusDtl = OrderPartStatus.Finished, Note = "Lô dầu chính hãng HTC" },
                    new OrderPartLine { PartId = pFilter?.Id ?? 2, PartCode = pFilter?.Code ?? "26300-35505", PartName = pFilter?.Name ?? "Lọc dầu động cơ chính hãng Hyundai", Unit = pFilter?.Unit ?? "Cái", Quantity = 30, UnitPrice = 90000, DiscountRate = 5, VatPercent = 8, ApprovedQuantity = 30, ReceivedQuantity = 30, StatusDtl = OrderPartStatus.Finished, Note = "Lọc nhớt tiêu chuẩn" },
                    new OrderPartLine { PartId = pAir?.Id ?? 3, PartCode = pAir?.Code ?? "28113-1R100", PartName = pAir?.Name ?? "Lọc gió động cơ Hyundai Accent", Unit = pAir?.Unit ?? "Cái", Quantity = 15, UnitPrice = 120000, DiscountRate = 0, VatPercent = 8, ApprovedQuantity = 15, ReceivedQuantity = 15, StatusDtl = OrderPartStatus.Finished, Note = "Lọc gió cho xe Accent" }
                ]
            };
            orderParts.Add(op1);

            // 2. Đơn đặt hàng khẩn cấp (UrgentVOR) đã được duyệt, chờ hàng về kho (Status = Approved)
            var op2 = new OrderPart
            {
                OrderPartNo = "PO260426-002",
                OrderDate = DateTime.Today.AddDays(-1),
                SupplierName = "Công ty TNHH Phụ tùng Mobis Việt Nam",
                DeliveryForm = OrderPartDeliveryForm.UrgentVOR,
                DeliveryLocation = "Xưởng dịch vụ Hyundai Cầu Giấy - Khoang nhận hàng nhanh",
                EstimatedDeliverDate = DateTime.Today.AddDays(1),
                ROId = ro1?.Id,
                VIN = ro1?.Car?.Vin ?? "RLHXXTC002",
                Status = OrderPartStatus.Approved,
                OrderSuppierNo = "MBS-PO-2604-0992",
                RequestSuppierDate = DateTime.Today.AddDays(-1),
                ResponseSuppierDate = DateTime.Today,
                Remark = "Đơn đặt hàng phụ tùng khẩn cấp VOR phục vụ xe bảo dưỡng & sửa chữa gầm phanh.",
                CreatedBy = "CVDV Tuấn",
                CreatedAt = DateTime.Now.AddDays(-1),
                ApprovedAt = DateTime.Now.AddHours(-10),
                Lines = [
                    new OrderPartLine { PartId = pBrake?.Id ?? 4, PartCode = pBrake?.Code ?? "58101-C1A00", PartName = pBrake?.Name ?? "Bộ má phanh đĩa trước", Unit = pBrake?.Unit ?? "Bộ", Quantity = 10, UnitPrice = 850000, DiscountRate = 2, VatPercent = 8, ApprovedQuantity = 10, ReceivedQuantity = 0, StatusDtl = OrderPartStatus.Approved, Note = "Giao hàng hỏa tốc trong 24h" }
                ]
            };
            orderParts.Add(op2);

            // 3. Đơn đặt hàng mới lập chờ duyệt (Status = Pending) bổ sung bugi sắp hết
            var op3 = new OrderPart
            {
                OrderPartNo = "PO260427-003",
                OrderDate = DateTime.Today,
                SupplierName = "Công ty TNHH Phụ tùng Mobis Việt Nam",
                DeliveryForm = OrderPartDeliveryForm.Normal,
                DeliveryLocation = "Kho phụ tùng chính - Kệ K1-C03",
                EstimatedDeliverDate = DateTime.Today.AddDays(3),
                Status = OrderPartStatus.Pending,
                Remark = "Bổ sung lượng tồn kho tối thiểu cho Bugi Iridium (hiện tại còn 3 cái, dưới mức 8 cái).",
                CreatedBy = "Thủ kho Tuấn",
                CreatedAt = DateTime.Now.AddHours(-2),
                Lines = [
                    new OrderPartLine { PartId = pSpark?.Id ?? 5, PartCode = pSpark?.Code ?? "18846-11070", PartName = pSpark?.Name ?? "Bugi đánh lửa Iridium cao cấp", Unit = pSpark?.Unit ?? "Cái", Quantity = 20, UnitPrice = 110000, DiscountRate = 5, VatPercent = 8, ApprovedQuantity = 20, ReceivedQuantity = 0, StatusDtl = OrderPartStatus.Pending, Note = "Đặt bù tồn kho an toàn" }
                ]
            };
            orderParts.Add(op3);

            db.OrderParts.AddRange(orderParts);
            await db.SaveChangesAsync();

            if (sFinished != null)
            {
                sFinished.OrderPartId = op1.Id;
                sFinished.OrderPartNo = op1.OrderPartNo;
                await db.SaveChangesAsync();
            }
        }

        if (!await db.Cavities.AnyAsync())
        {
            var ro1 = await db.ROs.Include(r => r.Car).FirstOrDefaultAsync(r => r.Code == "ROSEED-001");
            var roWar = await db.ROs.Include(r => r.Car).FirstOrDefaultAsync(r => r.Code == "ROSEED-WAR-001");

            var cavities = new List<Cavity>
            {
                new Cavity
                {
                    CavityNo = "KH-EM-01",
                    CavityName = "Khoang Bảo Dưỡng Nhanh #1 (EM 1)",
                    CavityType = CavityType.EM,
                    Status = CavityStatus.Occupied,
                    LiftEquipment = "Cầu nâng cắt kéo Werther 3.5T (Italy)",
                    AreaZone = "Xưởng dịch vụ tầng 1 - Khu bảo dưỡng nhanh",
                    CurrentROId = ro1?.Id,
                    CurrentCarPlate = ro1?.Car?.Plate ?? "30A-123.45",
                    CurrentCarModel = ro1?.Car?.Model ?? "Hyundai Accent 2022",
                    CurrentTechnician = ro1?.Technician ?? "Thợ Hùng",
                    StartUseDate = DateTime.Now.AddHours(-3),
                    ExpectedFinishDate = DateTime.Now.AddHours(1),
                    Note = "Bảo dưỡng cấp 20.000km, thay dầu máy và lọc dầu.",
                    CreatedBy = "seed"
                },
                new Cavity
                {
                    CavityNo = "KH-EM-02",
                    CavityName = "Khoang Bảo Dưỡng Nhanh #2 (EM 2)",
                    CavityType = CavityType.EM,
                    Status = CavityStatus.Available,
                    LiftEquipment = "Cầu nâng cắt kéo Werther 3.5T (Italy)",
                    AreaZone = "Xưởng dịch vụ tầng 1 - Khu bảo dưỡng nhanh",
                    Note = "Sẵn sàng đón xe đặt hẹn trực tuyến.",
                    CreatedBy = "seed"
                },
                new Cavity
                {
                    CavityNo = "KH-GR-01",
                    CavityName = "Cầu Nâng Sửa Chữa Gầm Máy #1 (GR 1)",
                    CavityType = CavityType.GR,
                    Status = CavityStatus.Available,
                    LiftEquipment = "Cầu nâng 2 trụ Corghi 4.0T (Italy)",
                    AreaZone = "Xưởng cơ khí & gầm máy chính",
                    Note = "Cầu nâng định kỳ bảo dưỡng đạt chuẩn an toàn.",
                    CreatedBy = "seed"
                },
                new Cavity
                {
                    CavityNo = "KH-GR-02",
                    CavityName = "Cầu Nâng Sửa Chữa Gầm Máy #2 (GR 2)",
                    CavityType = CavityType.GR,
                    Status = CavityStatus.Occupied,
                    LiftEquipment = "Cầu nâng 2 trụ Corghi 4.0T (Italy)",
                    AreaZone = "Xưởng cơ khí & gầm máy chính",
                    CurrentROId = roWar?.Id,
                    CurrentCarPlate = roWar?.Car?.Plate ?? "51G-678.90",
                    CurrentCarModel = roWar?.Car?.Model ?? "Hyundai Tucson 2023",
                    CurrentTechnician = roWar?.Technician ?? "KTV Quang",
                    StartUseDate = DateTime.Now.AddHours(-2),
                    ExpectedFinishDate = DateTime.Now.AddHours(2),
                    Note = "Xử lý sự cố bảo hành rung giật động cơ, thay thế bugi Iridium.",
                    CreatedBy = "seed"
                },
                new Cavity
                {
                    CavityNo = "KH-GR-03",
                    CavityName = "Cầu Cân Chỉnh Góc Đặt Bánh Xe (Alignment)",
                    CavityType = CavityType.GR,
                    Status = CavityStatus.Available,
                    LiftEquipment = "Cầu nâng 4 trụ chuyên dụng + Máy cân chỉnh Hunter 3D Hawkeye (USA)",
                    AreaZone = "Khu kiểm tra góc lái và cân bằng động",
                    Note = "Thiết bị đo góc camber, caster, toe kỹ thuật số.",
                    CreatedBy = "seed"
                },
                new Cavity
                {
                    CavityNo = "KH-BP-01",
                    CavityName = "Khoang Kéo Nắn Khung Vỏ Xe Tai Nạn (BP 1)",
                    CavityType = CavityType.BP,
                    Status = CavityStatus.Available,
                    LiftEquipment = "Giàn kéo nắn khung xe điện tử Car-O-Liner (Sweden) + Máy hàn bấm điểm",
                    AreaZone = "Xưởng đồng sơn & phục hồi va chạm",
                    Note = "Chuyên phục hồi xe tai nạn nặng.",
                    CreatedBy = "seed"
                },
                new Cavity
                {
                    CavityNo = "KH-BP-02",
                    CavityName = "Buồng Sơn Sấy Tiêu Chuẩn Cao Cấp (Spray Booth)",
                    CavityType = CavityType.BP,
                    Status = CavityStatus.Available,
                    LiftEquipment = "Buồng sơn sấy nhiệt Blowtherm Italia, hệ thống lọc bụi than hoạt tính",
                    AreaZone = "Khu vực sơn hấp & sấy nhiệt cao",
                    Note = "Đạt tiêu chuẩn sơn gốc nước Hyundai Toàn cầu.",
                    CreatedBy = "seed"
                },
                new Cavity
                {
                    CavityNo = "KH-KCS-01",
                    CavityName = "Khoang Kiểm Tra Chất Lượng Xuất Xưởng (KCS / QC)",
                    CavityType = CavityType.KCS,
                    Status = CavityStatus.Available,
                    LiftEquipment = "Băng thử phanh & trượt ngang Maha (Germany), máy kiểm tra đèn pha",
                    AreaZone = "Khu nghiệm thu kỹ thuật trước khi giao xe",
                    Note = "Kiểm tra chất lượng 100% xe trước khi bàn giao cho khách hàng.",
                    CreatedBy = "seed"
                },
                new Cavity
                {
                    CavityNo = "KH-WASH-01",
                    CavityName = "Khoang Rửa Xe & Chăm Sóc Hoàn Thiện (Car Wash)",
                    CavityType = CavityType.Wash,
                    Status = CavityStatus.Available,
                    LiftEquipment = "Hệ thống rửa bọt tuyết áp lực cao Karcher + Cầu nâng 1 trụ rửa xe",
                    AreaZone = "Khu vực giao xe & bàn giao",
                    Note = "Rửa xe miễn phí cho mọi khách hàng bảo dưỡng định kỳ.",
                    CreatedBy = "seed"
                }
            };

            db.Cavities.AddRange(cavities);
            await db.SaveChangesAsync();

            if (ro1 != null)
            {
                ro1.CavityId = cavities[0].Id;
            }
            if (roWar != null)
            {
                roWar.CavityId = cavities[3].Id;
            }
            await db.SaveChangesAsync();
        }

        if (!await db.ReceptionSheets.AnyAsync())
        {
            var car1 = await db.Cars.FirstAsync();
            var car2 = await db.Cars.OrderByDescending(c => c.Id).FirstAsync();
            var ro1 = await db.ROs.FirstOrDefaultAsync(r => r.Code == "ROSEED-001");
            var roWar = await db.ROs.FirstOrDefaultAsync(r => r.Code == "ROSEED-WAR-001");
            var app2 = await db.Appointments.FirstOrDefaultAsync();

            var receptions = new List<ReceptionSheet>();

            // 1. Phiếu tiếp nhận xe đã bàn giao hoàn tất (Delivered)
            var rec1 = new ReceptionSheet
            {
                ReceptionNo = "TN260427-001",
                CarId = car1.Id,
                CustomerId = car1.CustomerId,
                ROId = ro1?.Id,
                Odometer = 25400,
                FuelLevel = 3,
                LevelOfInspection = "Bảo dưỡng cấp 2 (10.000 km)",
                CustomerRequest = "Bảo dưỡng định kỳ 20.000 km, thay dầu máy, lọc nhớt và kiểm tra phanh.",
                ValuablesInCar = "Kính mắt Rayban trong hộc đồ, không để tiền mặt hay đồ quý.",
                ExteriorCondition = "Vết xước nhẹ cản trước bên phụ (đã ghi nhận cùng khách).",
                Status = ReceptionStatus.Delivered,
                CreatedBy = "CVDV Hoàng",
                CreatedAt = DateTime.Now.AddHours(-4),
                DeliveryDateTime = DateTime.Now.AddHours(-1),
                DeliveryBy = "CVDV Hoàng",
                DeliveryNote = "Khách hàng đã nhận xe sạch sẽ, hài lòng về tiến độ phục vụ.",
                Items = [
                    new ReceptionItem { Group = "Khoang lái", Code = "KHOANGLAI.DTL", Name = "Bảng đồng hồ & Đèn cảnh báo taplo", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Khoang lái", Code = "KHOANGLAI.COI", Name = "Còi xe & Hệ thống tín hiệu âm thanh", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Khoang lái", Code = "KHOANGLAI.GMBP", Name = "Cần gạt mưa & Vòi xịt rửa kính", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Khoang lái", Code = "KHOANGLAI.HTAT", Name = "Dây đai an toàn & Hệ thống túi khí", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Khoang lái", Code = "KHOANGLAI.HTDH", Name = "Hệ thống điều hòa nhiệt độ & Quạt gió", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Ngoại quan & Thân vỏ", Code = "TRUOCVASAUXE.DT", Name = "Cụm đèn chiếu sáng trước (Pha/Cos/Xi-nhan)", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Ngoại quan & Thân vỏ", Code = "TRUOCVASAUXE.DS", Name = "Cụm đèn sau (Đèn hậu/Phanh/Lùi)", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Ngoại quan & Thân vỏ", Code = "THANVO.XUOC", Name = "Kiểm tra trầy xước / móp méo thân vỏ xe", ReceptionStatus = AuditStatus.Attention, DeliveryStatus = AuditStatus.Good, Note = "Xước nhẹ cản trước" },
                    new ReceptionItem { Group = "Khoang động cơ", Code = "KHOANGDONGCO.DDC", Name = "Mức & Tình trạng dầu nhớt động cơ", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Khoang động cơ", Code = "KHOANGDONGCO.DP", Name = "Mức dầu phanh / Dầu ly hợp", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Khoang động cơ", Code = "KHOANGDONGCO.NLM", Name = "Mức nước làm mát động cơ & Bình phụ", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Lốp xe & Phanh", Code = "LOPXE.BXTT", Name = "Bánh xe trước trái (Áp suất & Gai lốp)", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Lốp xe & Phanh", Code = "LOPXE.BXTP", Name = "Bánh xe trước phải (Áp suất & Gai lốp)", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Lốp xe & Phanh", Code = "LOPXE.BXST", Name = "Bánh xe sau trái (Áp suất & Gai lốp)", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Lốp xe & Phanh", Code = "LOPXE.BXSP", Name = "Bánh xe sau phải (Áp suất & Gai lốp)", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Cốp sau & Dụng cụ", Code = "COPSAU.BDC", Name = "Bộ đồ nghề sửa chữa & Kích nâng", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Cốp sau & Dụng cụ", Code = "COPSAU.LDP", Name = "Lốp xe dự phòng & Áp suất lốp phụ", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good }
                ]
            };
            receptions.Add(rec1);

            // 2. Phiếu tiếp nhận đang trong xưởng sửa chữa (InService), có hạng mục cần chú ý
            var rec2 = new ReceptionSheet
            {
                ReceptionNo = "TN260427-002",
                CarId = car2.Id,
                CustomerId = car2.CustomerId,
                ROId = roWar?.Id,
                Odometer = 18200,
                FuelLevel = 2,
                LevelOfInspection = "Kiểm tra bảo hành hãng",
                CustomerRequest = "Động cơ rung giật khi đạp ga 50 km/h, đèn Check Engine nhấp nháy.",
                ValuablesInCar = "Không có đồ đạc quý trên xe.",
                ExteriorCondition = "Thân vỏ nguyên bản không trầy xước.",
                IsWarranty = true,
                Status = ReceptionStatus.InService,
                CreatedBy = "CVDV Quang",
                CreatedAt = DateTime.Now.AddHours(-2),
                Items = [
                    new ReceptionItem { Group = "Khoang lái", Code = "KHOANGLAI.DTL", Name = "Bảng đồng hồ & Đèn cảnh báo taplo", ReceptionStatus = AuditStatus.Replace, DeliveryStatus = AuditStatus.Good, Note = "Đèn Check Engine sáng báo lỗi P0302" },
                    new ReceptionItem { Group = "Khoang lái", Code = "KHOANGLAI.COI", Name = "Còi xe & Hệ thống tín hiệu âm thanh", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Khoang lái", Code = "KHOANGLAI.HTDH", Name = "Hệ thống điều hòa nhiệt độ & Quạt gió", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Ngoại quan & Thân vỏ", Code = "TRUOCVASAUXE.DT", Name = "Cụm đèn chiếu sáng trước (Pha/Cos/Xi-nhan)", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Khoang động cơ", Code = "KHOANGDONGCO.DDC", Name = "Mức & Tình trạng dầu nhớt động cơ", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Khoang động cơ", Code = "KHOANGDONGCO.LG", Name = "Tình trạng lọc gió động cơ & lọc máy lạnh", ReceptionStatus = AuditStatus.Attention, DeliveryStatus = AuditStatus.Good, Note = "Lọc gió bám bụi nhiều, cần vệ sinh thổi bụi" },
                    new ReceptionItem { Group = "Lốp xe & Phanh", Code = "LOPXE.BXTP", Name = "Bánh xe trước phải (Áp suất & Gai lốp)", ReceptionStatus = AuditStatus.Attention, DeliveryStatus = AuditStatus.Good, Note = "Áp suất 2.1 bar, hơi non (tiêu chuẩn 2.3 bar)" },
                    new ReceptionItem { Group = "Cốp sau & Dụng cụ", Code = "COPSAU.LDP", Name = "Lốp xe dự phòng & Áp suất lốp phụ", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good }
                ]
            };
            receptions.Add(rec2);

            // 3. Phiếu tiếp nhận mới lập chờ tạo RO (Pending)
            var rec3 = new ReceptionSheet
            {
                ReceptionNo = "TN260427-003",
                CarId = car1.Id,
                CustomerId = car1.CustomerId,
                AppointmentId = app2?.Id,
                Odometer = 26500,
                FuelLevel = 4,
                LevelOfInspection = "Bảo dưỡng cấp 1 (5.000 km)",
                CustomerRequest = "Khách hàng chuẩn bị đi công tác xa, yêu cầu kiểm tra kỹ hệ thống lái, phanh, áp suất lốp và mức dầu nhờn.",
                ValuablesInCar = "Không có",
                ExteriorCondition = "Thân vỏ sạch sẽ, không móp méo.",
                Status = ReceptionStatus.Pending,
                CreatedBy = "CVDV Hương",
                CreatedAt = DateTime.Now.AddMinutes(-30),
                Items = [
                    new ReceptionItem { Group = "Khoang lái", Code = "KHOANGLAI.DTL", Name = "Bảng đồng hồ & Đèn cảnh báo taplo", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Khoang lái", Code = "KHOANGLAI.GMBP", Name = "Cần gạt mưa & Vòi xịt rửa kính", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Khoang động cơ", Code = "KHOANGDONGCO.DDC", Name = "Mức & Tình trạng dầu nhớt động cơ", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Khoang động cơ", Code = "KHOANGDONGCO.DP", Name = "Mức dầu phanh / Dầu ly hợp", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Lốp xe & Phanh", Code = "LOPXE.BXTT", Name = "Bánh xe trước trái (Áp suất & Gai lốp)", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
                    new ReceptionItem { Group = "Cốp sau & Dụng cụ", Code = "COPSAU.BDC", Name = "Bộ đồ nghề sửa chữa & Kích nâng", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good }
                ]
            };
            receptions.Add(rec3);

            db.ReceptionSheets.AddRange(receptions);
            await db.SaveChangesAsync();

            if (ro1 != null)
            {
                ro1.ReceptionSheetId = rec1.Id;
            }
            if (roWar != null)
            {
                roWar.ReceptionSheetId = rec2.Id;
            }
            if (app2 != null)
            {
                app2.ReceptionSheetId = rec3.Id;
            }
            await db.SaveChangesAsync();
        }

        if (!await db.GroupRepairs.AnyAsync())
        {
            var grp1 = new GroupRepair
            {
                GroupRNo = "TO-SCC-01",
                GroupRName = "Tổ Sửa chữa chung & Gầm máy #1",
                LeaderName = "Nguyễn Văn Hùng",
                Note = "Phụ trách bảo dưỡng định kỳ cấp trung bình/lớn, sửa chữa gầm phanh, động cơ và hộp số tiêu chuẩn Hyundai.",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-60)
            };
            var grp2 = new GroupRepair
            {
                GroupRNo = "TO-DS-01",
                GroupRName = "Tổ Đồng sơn & Phục hồi va chạm",
                LeaderName = "Trần Đình Trọng",
                Note = "Chuyên phục hồi xe va chạm, kéo nắn khung thân vỏ, gò hàn và sơn sấy phòng hấp nhiệt cao.",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-60)
            };
            var grp3 = new GroupRepair
            {
                GroupRNo = "TO-EM-01",
                GroupRName = "Tổ Bảo dưỡng nhanh (Express Maintenance)",
                LeaderName = "Lê Văn Tuấn",
                Note = "Quy trình bảo dưỡng 60 phút 2 kỹ thuật viên song hành theo chuẩn Hyundai Global.",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-60)
            };

            db.GroupRepairs.AddRange(grp1, grp2, grp3);
            await db.SaveChangesAsync();

            var eng1 = new Engineer
            {
                EngineerNo = "KTV-001",
                EngineerName = "Nguyễn Văn Hùng",
                Phone = "0912345601",
                SkillLevel = "Kỹ thuật viên trưởng (Bậc 6/7)",
                Specialty = "Sửa chữa chung, Gầm máy & Động cơ",
                GroupRId = grp1.Id,
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-60)
            };
            var eng2 = new Engineer
            {
                EngineerNo = "KTV-002",
                EngineerName = "Lê Văn Tuấn",
                Phone = "0912345602",
                SkillLevel = "Bậc 4/7",
                Specialty = "Bảo dưỡng định kỳ EM & Hệ thống phanh",
                GroupRId = grp3.Id,
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-50)
            };
            var eng3 = new Engineer
            {
                EngineerNo = "KTV-003",
                EngineerName = "Trần Đình Trọng",
                Phone = "0912345603",
                SkillLevel = "Kỹ thuật viên trưởng Đồng Sơn (Bậc 5/7)",
                Specialty = "Gò hàn, Kéo nắn khung vỏ (SCD)",
                GroupRId = grp2.Id,
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-45)
            };
            var eng4 = new Engineer
            {
                EngineerNo = "KTV-004",
                EngineerName = "Phạm Văn Quang",
                Phone = "0912345604",
                SkillLevel = "Thợ sơn chuyên gia (Bậc 5/7)",
                Specialty = "Pha màu vi tính & Sơn hấp gốc nước (SCS)",
                GroupRId = grp2.Id,
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-40)
            };
            var eng5 = new Engineer
            {
                EngineerNo = "KTV-005",
                EngineerName = "Vũ Thành Đạt",
                Phone = "0912345605",
                SkillLevel = "Bậc 3/7",
                Specialty = "Chẩn đoán điện - điện tử & Hộp điều khiển ECU",
                GroupRId = grp1.Id,
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-30)
            };

            db.Engineers.AddRange(eng1, eng2, eng3, eng4, eng5);
            await db.SaveChangesAsync();
        }

        if (!await db.AssignmentWorks.AnyAsync())
        {
            var ro1 = await db.ROs.FirstOrDefaultAsync(r => r.Code == "ROSEED-001");
            var roWar = await db.ROs.FirstOrDefaultAsync(r => r.Code == "ROSEED-WAR-001");
            var roFns1 = await db.ROs.FirstOrDefaultAsync(r => r.Code == "ROSEED-FNS-001");

            var cavEm1 = await db.Cavities.FirstOrDefaultAsync(c => c.CavityNo == "KH-EM-01");
            var cavGr2 = await db.Cavities.FirstOrDefaultAsync(c => c.CavityNo == "KH-GR-02");

            var engHung = await db.Engineers.FirstOrDefaultAsync(e => e.EngineerNo == "KTV-001");
            var engTuan = await db.Engineers.FirstOrDefaultAsync(e => e.EngineerNo == "KTV-002");
            var engDat = await db.Engineers.FirstOrDefaultAsync(e => e.EngineerNo == "KTV-005");

            // 1. Phân công đã hoàn tất (Completed)
            if (roFns1 != null)
            {
                var assign1 = new AssignmentWork
                {
                    AssignmentNo = "PC260426-001",
                    ROId = roFns1.Id,
                    Status = AssignmentWorkStatus.Completed,
                    SCCCavityId = cavEm1?.Id,
                    SCCPlanStartDTime = DateTime.Today.AddDays(-1).AddHours(8),
                    SCCPlanFinishDTime = DateTime.Today.AddDays(-1).AddHours(10),
                    SCCActualStartDTime = DateTime.Today.AddDays(-1).AddHours(8).AddMinutes(10),
                    SCCActualFinishDTime = DateTime.Today.AddDays(-1).AddHours(9).AddMinutes(50),
                    Note = "Bảo dưỡng 5.000km xe Accent, hoàn tất đúng giờ, bàn giao xe sạch sẽ.",
                    CreatedBy = "Quản đốc xưởng",
                    CreatedAt = DateTime.Today.AddDays(-1).AddHours(7).AddMinutes(45),
                    StartedAt = DateTime.Today.AddDays(-1).AddHours(8).AddMinutes(10),
                    FinishedAt = DateTime.Today.AddDays(-1).AddHours(9).AddMinutes(50),
                    Engineers = [
                        new AssignmentEngineer
                        {
                            EngineerId = engTuan?.Id ?? 2,
                            WorkType = WorkType.SCC,
                            IsPrimary = true,
                            AssignedHours = 1.0m,
                            Note = "Thực hiện kiểm tra 20 hạng mục và thay dầu máy"
                        }
                    ]
                };
                db.AssignmentWorks.Add(assign1);
            }

            // 2. Phân công đang thi công trong xưởng (InProgress)
            if (ro1 != null)
            {
                var assign2 = new AssignmentWork
                {
                    AssignmentNo = "PC260427-002",
                    ROId = ro1.Id,
                    Status = AssignmentWorkStatus.InProgress,
                    SCCCavityId = cavEm1?.Id,
                    SCCPlanStartDTime = DateTime.Now.AddHours(-3),
                    SCCPlanFinishDTime = DateTime.Now.AddHours(1),
                    SCCActualStartDTime = DateTime.Now.AddHours(-3),
                    Note = "Xe bảo dưỡng cấp 20.000km + thay dầu và lọc dầu chính hãng. Cần siết lực ốc bánh xe 120Nm.",
                    CreatedBy = "Quản đốc xưởng",
                    CreatedAt = DateTime.Now.AddHours(-3).AddMinutes(-30),
                    StartedAt = DateTime.Now.AddHours(-3),
                    Engineers = [
                        new AssignmentEngineer
                        {
                            EngineerId = engHung?.Id ?? 1,
                            WorkType = WorkType.SCC,
                            IsPrimary = true,
                            AssignedHours = 1.5m,
                            Note = "Chịu trách nhiệm chính kiểm tra toàn diện và ký biên bản KCS"
                        },
                        new AssignmentEngineer
                        {
                            EngineerId = engTuan?.Id ?? 2,
                            WorkType = WorkType.SCC,
                            IsPrimary = false,
                            AssignedHours = 1.0m,
                            Note = "Hỗ trợ thay lọc dầu và vệ sinh lọc gió"
                        }
                    ]
                };
                db.AssignmentWorks.Add(assign2);
            }

            // 3. Phân công mới lập chờ thợ nhận việc (Assigned)
            if (roWar != null)
            {
                var assign3 = new AssignmentWork
                {
                    AssignmentNo = "PC260427-003",
                    ROId = roWar.Id,
                    Status = AssignmentWorkStatus.Assigned,
                    SCCCavityId = cavGr2?.Id,
                    SCCPlanStartDTime = DateTime.Now.AddHours(1),
                    SCCPlanFinishDTime = DateTime.Now.AddHours(3),
                    Note = "Kiểm tra chẩn đoán lỗi bỏ lửa P0302 và thay thế 4 bugi đánh lửa Iridium theo diện bảo hành hãng.",
                    CreatedBy = "Quản đốc xưởng",
                    CreatedAt = DateTime.Now.AddMinutes(-40),
                    Engineers = [
                        new AssignmentEngineer
                        {
                            EngineerId = engDat?.Id ?? 5,
                            WorkType = WorkType.SCC,
                            IsPrimary = true,
                            AssignedHours = 1.8m,
                            Note = "Kiểm tra tín hiệu mô-bin đánh lửa và đo khe hở bugi mới"
                        }
                    ]
                };
                db.AssignmentWorks.Add(assign3);
            }

            await db.SaveChangesAsync();
        }

        if (!await db.InsuranceCompanies.AnyAsync())
        {
            var bv = new InsuranceCompany
            {
                InsNo = "BH-BV",
                InsName = "Tổng công ty Bảo hiểm Bảo Việt (BaoViet Insurance)",
                Address = "Số 7 Lý Thường Kiệt, Hoàn Kiếm, Hà Nội",
                Phone = "024.3826.2614",
                Email = "giamdinh.baoviet@baoviet.com.vn",
                TaxCode = "0100111761",
                Hotline = "1900 558899",
                IsActive = true
            };
            var pvi = new InsuranceCompany
            {
                InsNo = "BH-PVI",
                InsName = "Tổng công ty Bảo hiểm PVI (PVI Insurance)",
                Address = "Tòa nhà PVI Tower, Số 1 Phạm Văn Bạch, Cầu Giấy, Hà Nội",
                Phone = "024.3733.5588",
                Email = "hotroboihoan@pvi.com.vn",
                TaxCode = "0105315570",
                Hotline = "1900 545458",
                IsActive = true
            };
            var pti = new InsuranceCompany
            {
                InsNo = "BH-PTI",
                InsName = "Tổng công ty CP Bảo hiểm Bưu điện (PTI)",
                Address = "Tầng 8, Tòa nhà Harec, Số 4A Láng Hạ, Ba Đình, Hà Nội",
                Phone = "024.3772.4466",
                Email = "claim.auto@pti.com.vn",
                TaxCode = "0100778409",
                Hotline = "1900 545475",
                IsActive = true
            };
            var mic = new InsuranceCompany
            {
                InsNo = "BH-MIC",
                InsName = "Tổng công ty CP Bảo hiểm Quân đội (MIC)",
                Address = "Tầng 15, Tòa nhà MIPEC, 229 Tây Sơn, Đống Đa, Hà Nội",
                Phone = "024.6285.3388",
                Email = "giamdinhmic@mic.vn",
                TaxCode = "0102422588",
                Hotline = "1900 558891",
                IsActive = true
            };

            db.InsuranceCompanies.AddRange(bv, pvi, pti, mic);
            await db.SaveChangesAsync();

            // Seed Contracts
            var ctBv = new InsuranceContract
            {
                ContractNo = "HD-BV/2026/01",
                ContractCode = "HDBV-HYUNDAI-2026",
                InsuranceCompanyId = bv.Id,
                StartDate = new DateTime(2026, 1, 1),
                FinishDate = new DateTime(2026, 12, 31),
                PaymentType = InsurancePaymentType.DirectGuarantee,
                PaymentLimit = 1_000_000_000m,
                DiscountLaborRate = 10.0m,
                DiscountPartRate = 5.0m,
                Note = "Bảo lãnh sửa chữa sơn gò, phụ tùng thay thế thân vỏ tiêu chuẩn chính hãng Hyundai",
                IsActive = true
            };
            var ctPvi = new InsuranceContract
            {
                ContractNo = "HD-PVI/2026/03",
                ContractCode = "HDPVI-HYUNDAI-2026",
                InsuranceCompanyId = pvi.Id,
                StartDate = new DateTime(2026, 1, 1),
                FinishDate = new DateTime(2026, 12, 31),
                PaymentType = InsurancePaymentType.DirectGuarantee,
                PaymentLimit = 800_000_000m,
                DiscountLaborRate = 8.0m,
                DiscountPartRate = 5.0m,
                Note = "Hợp đồng liên kết bảo hiểm vật chất xe toàn diện cho khách hàng Hyundai",
                IsActive = true
            };
            db.InsuranceContracts.AddRange(ctBv, ctPvi);
            await db.SaveChangesAsync();

            // Seed Claims
            var ros = await db.ROs.Include(r => r.Car).Include(r => r.Customer).ToListAsync();
            var ro1 = ros.FirstOrDefault();
            var ro2 = ros.Skip(1).FirstOrDefault();

            if (ro1 != null)
            {
                var claim1 = new InsuranceClaim
                {
                    ClaimNo = "BH260427-001",
                    ROId = ro1.Id,
                    InsuranceCompanyId = bv.Id,
                    InsuranceContractId = ctBv.Id,
                    PolicyNo = "BV-VCX-2026-88992",
                    ClaimFileNo = "HS-26-04-0012",
                    SurveyorName = "Nguyễn Văn Tuấn (Giám định viên BV)",
                    SurveyorPhone = "0912.345.678",
                    AccidentDate = DateTime.Today.AddDays(-2),
                    AccidentLocation = "Ngã tư Nguyễn Trãi - Khuất Duy Tiến, Thanh Xuân, Hà Nội",
                    AccidentDescription = "Xe va quẹt góc cản trước bên phụ khi chuyển làn, rách ba đờ sốc trước và xước cụm đèn pha.",
                    EstimatedAmount = 8_200_000m,
                    ApprovedAmount = 7_700_000m,
                    DeductibleAmount = 500_000m,
                    PenaltyAmount = 0m,
                    InsuranceAmount = 7_200_000m,
                    CustomerAmount = 1_000_000m,
                    Status = InsuranceClaimStatus.Approved,
                    DecisionNote = "Bảo Việt chấp thuận bảo lãnh bồi thường 7.700.000đ. Khách hàng chịu mức miễn thường 500.000đ theo đơn BH.",
                    CreatedBy = "CVDV Hoàng",
                    CreatedAt = DateTime.Now.AddDays(-2),
                    SubmittedAt = DateTime.Now.AddDays(-1),
                    ApprovedAt = DateTime.Now.AddHours(-6),
                    Items = [
                        new InsuranceClaimItem { Type = LineType.Labor, Code = "CV-GO-01", Name = "Công gò nắn phục hồi cản trước", Quantity = 1, UnitPrice = 1200000, EstimatedAmount = 1200000, ApprovedAmount = 1200000, IsApproved = true, Note = "Giám định duyệt 100%" },
                        new InsuranceClaimItem { Type = LineType.Labor, Code = "CV-SON-02", Name = "Công sơn sấy hấp cản trước xe", Quantity = 1, UnitPrice = 1800000, EstimatedAmount = 1800000, ApprovedAmount = 1800000, IsApproved = true, Note = "Sơn chuẩn pha vi tính" },
                        new InsuranceClaimItem { Type = LineType.Part, Code = "86511-S8000", Name = "Ba đờ sốc (cản trước) chính hãng", Quantity = 1, UnitPrice = 4200000, EstimatedAmount = 4200000, ApprovedAmount = 4200000, IsApproved = true, Note = "Duyệt thay mới do rách gãy tai bắt ốc" },
                        new InsuranceClaimItem { Type = LineType.Part, Code = "CV-DAN-03", Name = "Đánh bóng phục hồi chóa đèn pha phụ", Quantity = 1, UnitPrice = 1000000, EstimatedAmount = 1000000, ApprovedAmount = 500000, IsApproved = true, Note = "Bảo hiểm duyệt giảm trừ công đánh bóng chóa" }
                    ]
                };
                db.InsuranceClaims.Add(claim1);
            }

            if (ro2 != null)
            {
                var claim2 = new InsuranceClaim
                {
                    ClaimNo = "BH260427-002",
                    ROId = ro2.Id,
                    InsuranceCompanyId = pvi.Id,
                    InsuranceContractId = ctPvi.Id,
                    PolicyNo = "PVI-VC-2026-44319",
                    ClaimFileNo = "PVI-HN-9921",
                    SurveyorName = "Lê Hoàng Long (PVI Cầu Giấy)",
                    SurveyorPhone = "0988.112.233",
                    AccidentDate = DateTime.Today.AddDays(-1),
                    AccidentLocation = "Bãi đỗ xe Big C Thăng Long, Cầu Giấy, Hà Nội",
                    AccidentDescription = "Lùi xe va phải cột bê tông, móp méo nắp cốp sau và vỡ cụm đèn hậu bên lái.",
                    EstimatedAmount = 6_500_000m,
                    ApprovedAmount = 6_500_000m,
                    DeductibleAmount = 500_000m,
                    PenaltyAmount = 0m,
                    InsuranceAmount = 6_000_000m,
                    CustomerAmount = 500_000m,
                    Status = InsuranceClaimStatus.Submitted,
                    DecisionNote = null,
                    CreatedBy = "CVDV Thắng",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    SubmittedAt = DateTime.Now.AddHours(-3),
                    Items = [
                        new InsuranceClaimItem { Type = LineType.Labor, Code = "CV-GO-03", Name = "Công gò phục hồi nắp cốp sau", Quantity = 1, UnitPrice = 1500000, EstimatedAmount = 1500000, ApprovedAmount = 1500000, IsApproved = true },
                        new InsuranceClaimItem { Type = LineType.Labor, Code = "CV-SON-04", Name = "Sơn nắp cốp sau và góc hông", Quantity = 1, UnitPrice = 1800000, EstimatedAmount = 1800000, ApprovedAmount = 1800000, IsApproved = true },
                        new InsuranceClaimItem { Type = LineType.Part, Code = "92401-D3000", Name = "Cụm đèn hậu ngoài bên lái", Quantity = 1, UnitPrice = 3200000, EstimatedAmount = 3200000, ApprovedAmount = 3200000, IsApproved = true }
                    ]
                };
                db.InsuranceClaims.Add(claim2);
            }

            await db.SaveChangesAsync();
        }

        if (!await db.CampaignMarketings.AnyAsync())
        {
            var pOil = await db.Parts.FirstOrDefaultAsync(p => p.Code == "05100-00441");
            var pFilter = await db.Parts.FirstOrDefaultAsync(p => p.Code == "26300-35505");
            var pAir = await db.Parts.FirstOrDefaultAsync(p => p.Code == "28113-1R100");
            var pCabin = await db.Parts.FirstOrDefaultAsync(p => p.Code == "97133-D3000");

            var cam1 = new CampaignMarketing
            {
                CamMarketingNo = "KM-HE2026",
                CamMarketingName = "Chiến dịch Chăm sóc xe Đón hè 2026",
                CamMarketingDesc = "Chương trình ưu đãi dịch vụ lớn nhất hè 2026: Giảm 15% Dầu nhớt & Lọc dầu chính hãng, giảm 10% tiền công bảo dưỡng cho tất cả khách hàng làm dịch vụ tại xưởng.",
                EffDateStart = DateTime.Today.AddDays(-15),
                EffDateEnd = DateTime.Today.AddDays(45),
                DiscountLaborPercent = 10,
                DiscountPartPercent = 5,
                Status = CampaignMarketingStatus.Active,
                CreatedBy = "Ban Marketing",
                ApprovedBy = "Giám đốc dịch vụ",
                ApprovedAt = DateTime.Today.AddDays(-15),
                Items = [
                    new CampaignMarketingItem { PartId = pOil?.Id ?? 3, PartCode = pOil?.Code ?? "05100-00441", PartName = pOil?.Name ?? "Dầu nhớt 5W-30", PercentDiscount = 15, MaxQuantity = 2, Note = "Dầu nhờn động cơ giảm 15%" },
                    new CampaignMarketingItem { PartId = pFilter?.Id ?? 1, PartCode = pFilter?.Code ?? "26300-35505", PartName = pFilter?.Name ?? "Lọc dầu chính hãng", PercentDiscount = 20, MaxQuantity = 1, Note = "Lọc nhớt giảm 20%" },
                    new CampaignMarketingItem { PartId = pCabin?.Id ?? 6, PartCode = pCabin?.Code ?? "97133-D3000", PartName = pCabin?.Name ?? "Lọc gió điều hòa than hoạt tính", PercentDiscount = 15, MaxQuantity = 1, Note = "Lọc máy lạnh đón hè giảm 15%" }
                ]
            };

            var cam2 = new CampaignMarketing
            {
                CamMarketingNo = "KM-ACCENT-01",
                CamMarketingName = "Tri ân Khách hàng Hyundai Accent",
                CamMarketingDesc = "Ưu đãi chuyên biệt cho các chủ xe Hyundai Accent: Giảm 20% lọc gió động cơ và má phanh, giảm 15% tiền công bảo dưỡng định kỳ.",
                EffDateStart = DateTime.Today.AddDays(-10),
                EffDateEnd = DateTime.Today.AddDays(20),
                ConditionModel = "Accent",
                DiscountLaborPercent = 15,
                DiscountPartPercent = 10,
                Status = CampaignMarketingStatus.Active,
                CreatedBy = "CVDV Tuấn",
                ApprovedBy = "Giám đốc dịch vụ",
                ApprovedAt = DateTime.Today.AddDays(-10),
                Items = [
                    new CampaignMarketingItem { PartId = pAir?.Id ?? 2, PartCode = pAir?.Code ?? "28113-1R100", PartName = pAir?.Name ?? "Lọc gió động cơ Hyundai Accent", PercentDiscount = 20, MaxQuantity = 1, Note = "Lọc gió Accent giảm 20%" },
                    new CampaignMarketingItem { PartId = pFilter?.Id ?? 1, PartCode = pFilter?.Code ?? "26300-35505", PartName = pFilter?.Name ?? "Lọc dầu động cơ Hyundai", PercentDiscount = 15, MaxQuantity = 1, Note = "Lọc dầu Accent giảm 15%" }
                ]
            };

            var cam3 = new CampaignMarketing
            {
                CamMarketingNo = "KM-KTTQ-FREE",
                CamMarketingName = "Miễn phí 100% Tiền công kiểm tra xe 20 hạng mục",
                CamMarketingDesc = "Kiểm tra toàn diện khoang động cơ, hệ thống phanh, gầm xe và lốp xe theo tiêu chuẩn Hyundai toàn cầu.",
                EffDateStart = DateTime.Today.AddDays(-5),
                EffDateEnd = DateTime.Today.AddDays(25),
                DiscountLaborPercent = 100,
                DiscountPartPercent = 0,
                Status = CampaignMarketingStatus.Active,
                CreatedBy = "CVDV Hương",
                ApprovedBy = "Giám đốc dịch vụ",
                ApprovedAt = DateTime.Today.AddDays(-5)
            };

            var cam4 = new CampaignMarketing
            {
                CamMarketingNo = "KM-XUAN2026",
                CamMarketingName = "Chiến dịch Du xuân An toàn 2026",
                CamMarketingDesc = "Chương trình bảo dưỡng đầu xuân, tặng voucher thay dầu nhớt và kiểm tra ắc quy xe.",
                EffDateStart = DateTime.Today.AddDays(-75),
                EffDateEnd = DateTime.Today.AddDays(-15),
                DiscountLaborPercent = 10,
                DiscountPartPercent = 10,
                Status = CampaignMarketingStatus.Finished,
                CreatedBy = "Ban Marketing",
                ApprovedBy = "Giám đốc đại lý",
                ApprovedAt = DateTime.Today.AddDays(-75)
            };

            db.CampaignMarketings.AddRange(cam1, cam2, cam3, cam4);
            await db.SaveChangesAsync();

            // Gắn chiến dịch khuyến mãi cam2 cho RO demo ROSEED-FNS-001 (Accent)
            var roFns1 = await db.ROs.FirstOrDefaultAsync(r => r.Code == "ROSEED-FNS-001");
            if (roFns1 != null)
            {
                roFns1.CampaignMarketingId = cam2.Id;
                roFns1.CampaignDiscountAmount = 145000m;
                await db.SaveChangesAsync();
            }
        }

        if (!await db.CustomerCareMaces.AnyAsync())
        {
            var car1 = await db.Cars.FirstAsync();
            var car2 = await db.Cars.OrderByDescending(c => c.Id).FirstAsync();
            var ro1 = await db.ROs.FirstOrDefaultAsync(r => r.Code == "ROSEED-001");
            var roFns = await db.ROs.FirstOrDefaultAsync(r => r.Code == "ROSEED-FNS-001");
            var app1 = await db.Appointments.FirstOrDefaultAsync(a => a.AppNo == "APP260427-001");

            // 1. Xe Accent (Car 1): Quá hạn bảo dưỡng mốc 30.000 km, đang chờ CSKH liên hệ (Pending / Overdue)
            var mace1 = new CustomerCareMace
            {
                MaceNo = "MC260415-001",
                ROId = ro1?.Id,
                CarId = car1.Id,
                CustomerId = car1.CustomerId,
                MaceType = MaceType.Standard6Months,
                LastKm = 25400,
                NextKm = 30000,
                MaceRecomentDate = DateTime.Today.AddDays(-12), // Quá hạn 12 ngày
                Status = CustomerCareMaceStatus.Pending,
                Remark = "Xe đã quá hạn bảo dưỡng cấp 3 (30.000km). Cần ưu tiên gọi điện nhắc khách.",
                CreatedBy = "system",
                CreatedAt = DateTime.Today.AddDays(-20)
            };

            // 2. Xe Tucson (Car 2): Mốc 25.000 km sắp đến hạn trong tuần này (DueSoon), đã liên hệ khách (Contacted)
            var mace2 = new CustomerCareMace
            {
                MaceNo = "MC260425-002",
                CarId = car2.Id,
                CustomerId = car2.CustomerId,
                MaceType = MaceType.FrequencyFvx,
                LastKm = 18200,
                NextKm = 25000,
                MaceRecomentDate = DateTime.Today.AddDays(4), // Còn 4 ngày nữa đến hạn
                Status = CustomerCareMaceStatus.Contacted,
                ContactDate = DateTime.Today.AddDays(-1),
                ContactBy = "CVDV Tuấn",
                ApointDate = DateTime.Today.AddDays(5).AddHours(9),
                Remark = "Khách hàng đồng ý đem xe vào xưởng sáng thứ 7 để thay dầu và kiểm tra định kỳ.",
                CreatedBy = "CVDV Tuấn",
                CreatedAt = DateTime.Today.AddDays(-5)
            };

            // 3. Xe Accent (Car 1): Đã chốt hẹn bảo dưỡng (Booked) liên kết với cuộc hẹn APP260427-001
            var mace3 = new CustomerCareMace
            {
                MaceNo = "MC260420-003",
                ROId = roFns?.Id,
                CarId = car1.Id,
                CustomerId = car1.CustomerId,
                MaceType = MaceType.Standard6Months,
                LastKm = 20000,
                NextKm = 25000,
                MaceRecomentDate = DateTime.Today,
                Status = CustomerCareMaceStatus.Booked,
                ContactDate = DateTime.Today.AddDays(-2),
                ContactBy = "CVDV Tuấn",
                ApointDate = DateTime.Today.AddHours(8).AddMinutes(30),
                AppointmentId = app1?.Id,
                Remark = "Đã chốt lịch hẹn qua điện thoại. Xe đã tiếp nhận vào khoang EM sáng nay.",
                CreatedBy = "system",
                CreatedAt = DateTime.Today.AddDays(-10)
            };

            // 4. Xe Tucson (Car 2): Mốc bảo dưỡng định kỳ 6 tháng tới (Pending / Future)
            var mace4 = new CustomerCareMace
            {
                MaceNo = "MC260427-004",
                CarId = car2.Id,
                CustomerId = car2.CustomerId,
                MaceType = MaceType.Standard6Months,
                LastKm = 18200,
                NextKm = 30000,
                MaceRecomentDate = DateTime.Today.AddMonths(5),
                Status = CustomerCareMaceStatus.Pending,
                Remark = "Mốc bảo dưỡng dự kiến tiếp theo theo chu kỳ tiêu chuẩn 6 tháng.",
                CreatedBy = "system",
                CreatedAt = DateTime.Today
            };

            db.CustomerCareMaces.AddRange(mace1, mace2, mace3, mace4);
            await db.SaveChangesAsync();
        }

        if (!await db.StockAdjs.AnyAsync())
        {
            var pOil = await db.Parts.FirstOrDefaultAsync(p => p.Code == "05100-00441");
            var pFilter = await db.Parts.FirstOrDefaultAsync(p => p.Code == "26300-35505");
            var pAir = await db.Parts.FirstOrDefaultAsync(p => p.Code == "28113-1R100");
            var pBrake = await db.Parts.FirstOrDefaultAsync(p => p.Code == "58101-C1A00");
            var pSpark = await db.Parts.FirstOrDefaultAsync(p => p.Code == "18846-11070");
            var pCabin = await db.Parts.FirstOrDefaultAsync(p => p.Code == "97133-D3000");

            // 1. Phiếu kiểm kê cân đối kho tháng 4 (Finished) - Phát hiện thừa 1 lọc dầu do trả lại xưởng
            var adj1 = new StockAdj
            {
                StockAdjNo = "KK260420-001",
                StockAdjDate = DateTime.Today.AddDays(-7),
                Type = StockAdjType.CountBalance,
                Status = StockAdjStatus.Finished,
                StorageCode = "KHO-CHINH",
                Remark = "Kiểm kê định kỳ tháng 4 kho phụ tùng chính và cân đối chênh lệch thực tế.",
                CreatedBy = "Thủ kho Tuấn",
                CreatedAt = DateTime.Today.AddDays(-7).AddHours(-4),
                ApprovedBy = "Kế toán trưởng",
                FinishedAt = DateTime.Today.AddDays(-7).AddHours(-1),
                Items = [
                    new StockAdjDetail {
                        PartId = pOil?.Id ?? 3,
                        PartCode = pOil?.Code ?? "05100-00441",
                        PartName = pOil?.Name ?? "Dầu nhờn động cơ Hyundai 5W-30 (Can 4L)",
                        Unit = pOil?.Unit ?? "Can",
                        CostPrice = pOil?.CostPrice ?? 420000,
                        SystemQuantity = 35,
                        ActualQuantity = 35,
                        FromLocation = pOil?.Location ?? "K-DAU-01",
                        ToLocation = pOil?.Location ?? "K-DAU-01",
                        Note = "Khớp số lượng sổ sách"
                    },
                    new StockAdjDetail {
                        PartId = pFilter?.Id ?? 1,
                        PartCode = pFilter?.Code ?? "26300-35505",
                        PartName = pFilter?.Name ?? "Lọc dầu động cơ chính hãng Hyundai",
                        Unit = pFilter?.Unit ?? "Cái",
                        CostPrice = pFilter?.CostPrice ?? 90000,
                        SystemQuantity = 27,
                        ActualQuantity = 28,
                        FromLocation = pFilter?.Location ?? "K1-A01",
                        ToLocation = pFilter?.Location ?? "K1-A01",
                        Note = "Thừa 1 cái do hoàn trả từ lệnh RO chưa nhập kịp"
                    },
                    new StockAdjDetail {
                        PartId = pAir?.Id ?? 2,
                        PartCode = pAir?.Code ?? "28113-1R100",
                        PartName = pAir?.Name ?? "Lọc gió động cơ Hyundai Accent",
                        Unit = pAir?.Unit ?? "Cái",
                        CostPrice = pAir?.CostPrice ?? 120000,
                        SystemQuantity = 14,
                        ActualQuantity = 14,
                        FromLocation = pAir?.Location ?? "K1-A04",
                        ToLocation = pAir?.Location ?? "K1-A04",
                        Note = "Khớp số lượng"
                    }
                ]
            };

            // 2. Phiếu điều chuyển kệ kho (Executing) - Chuyển bugi và má phanh sang kệ A gần xưởng bảo dưỡng
            var adj2 = new StockAdj
            {
                StockAdjNo = "DC260427-001",
                StockAdjDate = DateTime.Today,
                Type = StockAdjType.LocationTransfer,
                Status = StockAdjStatus.Executing,
                StorageCode = "KHO-CHINH",
                Remark = "Quy hoạch lại vị trí kệ kho: điều chuyển phụ tùng thay nhanh sang kệ K1-A gần cửa xuất xưởng.",
                CreatedBy = "Thủ kho Tuấn",
                CreatedAt = DateTime.Now.AddHours(-2),
                Items = [
                    new StockAdjDetail {
                        PartId = pSpark?.Id ?? 5,
                        PartCode = pSpark?.Code ?? "18846-11070",
                        PartName = pSpark?.Name ?? "Bugi đánh lửa Iridium cao cấp",
                        Unit = pSpark?.Unit ?? "Cái",
                        CostPrice = pSpark?.CostPrice ?? 110000,
                        SystemQuantity = pSpark?.InStock ?? 3,
                        ActualQuantity = pSpark?.InStock ?? 3,
                        FromLocation = "K1-C03",
                        ToLocation = "K1-A02",
                        Note = "Chuyển sang kệ bảo dưỡng nhanh"
                    },
                    new StockAdjDetail {
                        PartId = pBrake?.Id ?? 4,
                        PartCode = pBrake?.Code ?? "58101-C1A00",
                        PartName = pBrake?.Name ?? "Bộ má phanh đĩa trước",
                        Unit = pBrake?.Unit ?? "Bộ",
                        CostPrice = pBrake?.CostPrice ?? 850000,
                        SystemQuantity = pBrake?.InStock ?? 6,
                        ActualQuantity = pBrake?.InStock ?? 6,
                        FromLocation = "K2-B02",
                        ToLocation = "K1-A03",
                        Note = "Chuyển từ tầng 2 xuống tầng 1"
                    }
                ]
            };

            // 3. Phiếu kiểm kê đột xuất vật tư lọc máy lạnh (Pending)
            var adj3 = new StockAdj
            {
                StockAdjNo = "KK260427-002",
                StockAdjDate = DateTime.Today,
                Type = StockAdjType.CountBalance,
                Status = StockAdjStatus.Pending,
                StorageCode = "KHO-CHINH",
                Remark = "Kiểm tra đối chiếu tồn thực tế lọc gió điều hòa chuẩn bị cho chiến dịch khuyến mãi mùa hè.",
                CreatedBy = "Thủ kho Tuấn",
                CreatedAt = DateTime.Now.AddMinutes(-30),
                Items = [
                    new StockAdjDetail {
                        PartId = pCabin?.Id ?? 6,
                        PartCode = pCabin?.Code ?? "97133-D3000",
                        PartName = pCabin?.Name ?? "Lọc gió điều hòa than hoạt tính",
                        Unit = pCabin?.Unit ?? "Cái",
                        CostPrice = pCabin?.CostPrice ?? 140000,
                        SystemQuantity = pCabin?.InStock ?? 19,
                        ActualQuantity = pCabin?.InStock ?? 19,
                        FromLocation = pCabin?.Location ?? "K1-A08",
                        ToLocation = pCabin?.Location ?? "K1-A08",
                        Note = "Chuẩn bị đếm thực tế"
                    }
                ]
            };

            db.StockAdjs.AddRange(adj1, adj2, adj3);
            await db.SaveChangesAsync();
        }

        if (!await db.Bulletins.AnyAsync())
        {
            var pSpark = await db.Parts.FirstOrDefaultAsync(p => p.Code == "18846-11070");
            var carAccent = await db.Cars.FirstOrDefaultAsync(c => c.Plate == "30A-123.45");
            var carTucson = await db.Cars.FirstOrDefaultAsync(c => c.Plate == "51G-678.90");

            var b1 = new Bulletin
            {
                BulletinNo = "TSB-2026-001",
                BulletinNoHMC = "HMC-TSB-24-01-002",
                Title = "Chiến dịch nâng cấp phần mềm điều khiển hộp số & Kiểm tra cụm bugi đánh lửa xe Tucson 2023",
                Remark = "Một số xe Tucson 2023 có hiện tượng rung giật nhẹ khi chuyển số 2 sang 3 ở tốc độ thấp. Hãng Hyundai Thành Công ban hành bản tin kỹ thuật cập nhật phần mềm TCU và thay thế bộ bugi Iridium nếu phát hiện hao mòn bất thường.",
                Solution = "1. Kết nối máy chẩn đoán GDS Mobile cập nhật phần mềm hộp số TCU lên phiên bản v2.4.\n2. Kiểm tra bộ 4 bugi đánh lửa, thay mới bộ bugi chính hãng mã 18846-11070 nếu phát hiện đóng muội than hoặc khe hở điện cực > 1.1mm.\n3. Chạy thử xe và xác nhận không còn mã lỗi P0300.",
                CreateDate = DateTime.Today.AddDays(-20),
                DateExpired = DateTime.Today.AddMonths(5),
                IsActive = true,
                Status = BulletinStatus.Active,
                UserCreate = "Phòng Kỹ thuật Dịch vụ HTC",
                FileNameAttachment = "TSB-2026-001_TCU_Software_Update.pdf",
                CreatedBy = "seed",
                Items = [
                    new BulletinDetail { Type = LineType.Labor, Code = "CV-TSB-TCU", Name = "Công nâng cấp phần mềm điều khiển TCU bằng GDS Mobile", Unit = "Lần", Quantity = 1, UnitPrice = 0, Note = "Miễn phí theo chính sách bảo hành HTC" },
                    new BulletinDetail { Type = LineType.Part, PartId = pSpark?.Id ?? 5, Code = "18846-11070", Name = "Bugi đánh lửa Iridium cao cấp", Unit = "Cái", Quantity = 4, UnitPrice = 220000, Note = "Bồi hoàn theo giá niêm yết bảo hành" }
                ],
                TargetVins = [
                    new BulletinVin { VinNo = "RLHXXTC002", PlateNo = carTucson?.Plate ?? "51G-678.90", Model = "Hyundai Tucson 2023", DealerCode = "HYUNDAI-MAIN", Status = BulletinVinStatus.Pending, Note = "Xe đang làm dịch vụ tại xưởng, cần xử lý trong lệnh sửa chữa" },
                    new BulletinVin { VinNo = "RLHXXTC003", PlateNo = "51H-992.11", Model = "Hyundai Tucson 2023", DealerCode = "HYUNDAI-MAIN", Status = BulletinVinStatus.Pending, Note = "Chờ xe vào xưởng kiểm tra" },
                    new BulletinVin { VinNo = "RLHXXTC004", PlateNo = "51K-123.88", Model = "Hyundai Tucson 2023", DealerCode = "HYUNDAI-MAIN", Status = BulletinVinStatus.Completed, DateDone = DateTime.Today.AddDays(-5), DoneBy = "KTV Hùng", RONo = "RO-TC004-DONE", Note = "Đã cập nhật TCU và thay 4 bugi thành công" }
                ]
            };

            var b2 = new Bulletin
            {
                BulletinNo = "CAM-RECALL-HYU01",
                BulletinNoHMC = "HMC-RC-23-08-015",
                Title = "Chiến dịch triệu hồi kiểm tra & thay thế chốt khóa nắp ca-pô an toàn xe Hyundai Accent 2022",
                Remark = "Hãng Hyundai thông báo chương trình triệu hồi an toàn kiểm tra cụm cơ cấu lò xo chốt phụ nắp ca-pô có thể bị kẹt rỉ sét sau thời gian dài vận hành trong điều kiện khí hậu nóng ẩm.",
                Solution = "Kiểm tra cơ cấu ngàm khóa nắp ca-pô. Thay thế cụm chốt khóa an toàn mới đã xử lý chống ăn mòn và tra mỡ bôi trơn chuyên dụng chịu nhiệt.",
                CreateDate = DateTime.Today.AddDays(-40),
                DateExpired = DateTime.Today.AddMonths(3),
                IsActive = true,
                Status = BulletinStatus.Active,
                UserCreate = "Cục Đăng kiểm & HTC",
                FileNameAttachment = "RECALL_CAPO_ACCENT_2022.pdf",
                CreatedBy = "seed",
                Items = [
                    new BulletinDetail { Type = LineType.Labor, Code = "CV-RC-CAPO", Name = "Công kiểm tra & thay thế cụm chốt khóa an toàn nắp ca-pô", Unit = "Lần", Quantity = 1, UnitPrice = 0, Note = "Triệu hồi an toàn miễn phí 100%" },
                    new BulletinDetail { Type = LineType.Part, Code = "81140-1R000", Name = "Cụm chốt khóa an toàn nắp ca-pô chính hãng cải tiến", Unit = "Bộ", Quantity = 1, UnitPrice = 180000, Note = "Hãng cấp miễn phí" }
                ],
                TargetVins = [
                    new BulletinVin { VinNo = "RLHXXAC001", PlateNo = carAccent?.Plate ?? "30A-123.45", Model = "Hyundai Accent 2022", DealerCode = "HYUNDAI-MAIN", Status = BulletinVinStatus.Pending, Note = "Xe Accent trong danh sách triệu hồi, cần liên hệ khách hàng hoặc làm ngay khi vào xưởng" },
                    new BulletinVin { VinNo = "RLHXXAC007", PlateNo = "30E-881.23", Model = "Hyundai Accent 2022", DealerCode = "HYUNDAI-MAIN", Status = BulletinVinStatus.Pending, Note = "Đang chờ liên hệ" },
                    new BulletinVin { VinNo = "RLHXXAC008", PlateNo = "30F-445.67", Model = "Hyundai Accent 2022", DealerCode = "HYUNDAI-MAIN", Status = BulletinVinStatus.Completed, DateDone = DateTime.Today.AddDays(-12), DoneBy = "KTV Trọng", RONo = "RO-AC008-RC", Note = "Đã thay thế chốt khóa cải tiến" }
                ]
            };

            var b3 = new Bulletin
            {
                BulletinNo = "TSB-2025-012",
                BulletinNoHMC = "HMC-TSB-23-11-008",
                Title = "Thông báo kỹ thuật: Kiểm tra siết lực bu-lông càng A trước xe Hyundai Santa Fe",
                Remark = "Kiểm tra mô-men siết bu-lông liên kết càng chữ A phía trước đạt tiêu chuẩn 135 Nm để loại trừ tiếng kêu lục cục khi đánh lái hết lái qua gờ giảm tốc.",
                Solution = "Dùng cần siết lực chuyên dụng kiểm tra và siết chặt lại bu-lông càng chữ A theo đúng thông số kỹ thuật xuất xưởng.",
                CreateDate = DateTime.Today.AddDays(-120),
                DateExpired = DateTime.Today.AddDays(-10),
                IsActive = false,
                Status = BulletinStatus.Finished,
                UserCreate = "Phòng Kỹ thuật Dịch vụ HTC",
                FileNameAttachment = "TSB_SANTAFE_CANGA_TORQUE.pdf",
                CreatedBy = "seed",
                Items = [
                    new BulletinDetail { Type = LineType.Labor, Code = "CV-TSB-CANGA", Name = "Công kiểm tra siết lực bu-lông càng A trước", Unit = "Lần", Quantity = 1, UnitPrice = 0, Note = "Đã hoàn thành đợt kiểm tra" }
                ],
                TargetVins = [
                    new BulletinVin { VinNo = "RLHXXSF001", PlateNo = "30H-111.22", Model = "Hyundai Santa Fe 2023", DealerCode = "HYUNDAI-MAIN", Status = BulletinVinStatus.Completed, DateDone = DateTime.Today.AddDays(-30), DoneBy = "KTV Hùng", RONo = "RO-SF001-TSB" },
                    new BulletinVin { VinNo = "RLHXXSF002", PlateNo = "30H-333.44", Model = "Hyundai Santa Fe 2023", DealerCode = "HYUNDAI-MAIN", Status = BulletinVinStatus.Completed, DateDone = DateTime.Today.AddDays(-25), DoneBy = "KTV Hùng", RONo = "RO-SF002-TSB" }
                ]
            };

            db.Bulletins.AddRange(b1, b2, b3);
            await db.SaveChangesAsync();
        }

        if (!await db.PdiRequests.AnyAsync())
        {
            var pOil = await db.Parts.FirstOrDefaultAsync(p => p.Code == "05100-00441");

            // 1. Phiếu PDI đã hoàn tất (Completed) - Hyundai Santa Fe 2.5T HTRAC Cao cấp
            var pdi1 = new PdiRequest
            {
                PdiReqNo = "PDI260425-001",
                DealerCode = "HYUNDAI-MAIN",
                CreatedDate = DateTime.Today.AddDays(-2),
                ApprovedDate = DateTime.Today.AddDays(-2).AddHours(1),
                FinishedAt = DateTime.Today.AddDays(-1).AddHours(16),
                Remark = "Kiểm tra xuất xưởng PDI và lắp đặt trọn gói phụ kiện giao xe cho khách hàng VIP nhận xe cuối tuần.",
                FlagAccessory = true,
                CreatedBy = "Showroom KD 1",
                ApprovedBy = "Quản đốc xưởng",
                Status = PdiRequestStatus.Completed
            };

            var roPdi1 = new RepairOrder
            {
                Code = "RO-PDI260425-001",
                CarId = (await db.Cars.FirstAsync()).Id,
                CustomerId = (await db.Customers.FirstAsync()).Id,
                Status = ROStatus.Finished,
                Odometer = 12,
                IntakeNote = "Kiểm tra kỹ thuật xuất xưởng PDI tiêu chuẩn Hyundai + Lắp phụ kiện theo HĐ HD-XE-2026/089",
                Technician = "KTV Minh",
                CreatedBy = "PDI Dispatch",
                CreatedAt = DateTime.Today.AddDays(-2).AddHours(1),
                IntakeAt = DateTime.Today.AddDays(-2).AddHours(1),
                FinishedAt = DateTime.Today.AddDays(-1).AddHours(16),
                PdiReqNo = pdi1.PdiReqNo
            };
            roPdi1.Lines.Add(new RepairLine { Type = LineType.Labor, Name = "Kiểm tra kỹ thuật xuất xưởng PDI tiêu chuẩn Hyundai (25 điểm)", Quantity = 1, UnitPrice = 350000, ExpenseType = ExpenseType.Internal });
            roPdi1.Lines.Add(new RepairLine { Type = LineType.Labor, Name = "Vệ sinh làm sạch & Rửa xe hoàn thiện giao xe mới", Quantity = 1, UnitPrice = 150000, ExpenseType = ExpenseType.Internal });
            roPdi1.Lines.Add(new RepairLine { Type = LineType.Labor, Name = "Lắp đặt gói phụ kiện giao xe: Dán phim cách nhiệt Lumax USA, Bọc sàn da 5D, Camera hành trình Vietmap SpeedMap M1", Quantity = 1, UnitPrice = 650000, ExpenseType = ExpenseType.Internal });
            db.ROs.Add(roPdi1);
            await db.SaveChangesAsync();

            var item1 = new PdiRequestItem
            {
                VIN = "RLHXXSF20240089",
                Model = "Hyundai Santa Fe 2024",
                Spec = "2.5T HTRAC Cao cấp (Xăng Turbo)",
                Color = "Trắng ngọc trai (Glacier White)",
                EngineNo = "G4KP-RC88129",
                BatteryNo = "AGM-80AH-9921",
                ContractNo = "HD-XE-2026/089",
                CustomerName = "Trần Đức Thịnh",
                CustomerPhone = "0912.889.922",
                CustomerAddress = "Vinhomes Riverside, Long Biên, Hà Nội",
                ExpectedDeliveryDate = DateTime.Today.AddDays(1),
                FlagAccessory = true,
                AccessoryNote = "Dán phim cách nhiệt Lumax USA cao cấp, Bọc sàn da 5D, Camera hành trình Vietmap SpeedMap M1, Phủ ceramic bề mặt sơn",
                Status = PdiItemStatus.Passed,
                Inspector = "KTV Minh",
                InspectionDate = DateTime.Today.AddDays(-2).AddHours(2),
                PassedDate = DateTime.Today.AddDays(-1).AddHours(16),
                InspectionNotes = "Toàn bộ 25 hạng mục kiểm tra đạt tiêu chuẩn xuất xưởng Hyundai Toàn cầu. Đã hoàn thiện dán phim và rửa xe sạch sẽ. Xe sẵn sàng bàn giao.",
                ROId = roPdi1.Id,
                RONo = roPdi1.Code,
                ChecklistItems = CreateDefaultChecklist()
            };
            pdi1.Items.Add(item1);
            db.PdiRequests.Add(pdi1);
            await db.SaveChangesAsync();

            // 2. Phiếu PDI đang kiểm tra & hoàn thiện (Approved / InProgress) - Hyundai Creta 1.5 AT Premium
            var pdi2 = new PdiRequest
            {
                PdiReqNo = "PDI260427-002",
                DealerCode = "HYUNDAI-MAIN",
                CreatedDate = DateTime.Today,
                ApprovedDate = DateTime.Today.AddHours(-3),
                Remark = "Kiểm tra kỹ thuật PDI xe mới bàn giao từ nhà máy Hyundai Thành Công Ninh Bình và lắp gói phụ kiện.",
                FlagAccessory = true,
                CreatedBy = "Showroom KD 2",
                ApprovedBy = "Quản đốc xưởng",
                Status = PdiRequestStatus.Approved
            };

            var roPdi2 = new RepairOrder
            {
                Code = "RO-PDI260427-002",
                CarId = (await db.Cars.OrderByDescending(c => c.Id).FirstAsync()).Id,
                CustomerId = (await db.Customers.OrderByDescending(c => c.Id).FirstAsync()).Id,
                Status = ROStatus.InGarage,
                Odometer = 8,
                IntakeNote = "Kiểm tra kỹ thuật xuất xưởng PDI tiêu chuẩn Hyundai + Lắp phụ kiện theo HĐ HD-XE-2026/102",
                Technician = "KTV Tuấn",
                CreatedBy = "PDI Dispatch",
                CreatedAt = DateTime.Today.AddHours(-3),
                IntakeAt = DateTime.Today.AddHours(-3),
                PdiReqNo = pdi2.PdiReqNo
            };
            roPdi2.Lines.Add(new RepairLine { Type = LineType.Labor, Name = "Kiểm tra kỹ thuật xuất xưởng PDI tiêu chuẩn Hyundai (25 điểm)", Quantity = 1, UnitPrice = 350000, ExpenseType = ExpenseType.Internal });
            roPdi2.Lines.Add(new RepairLine { Type = LineType.Labor, Name = "Lắp đặt gói phụ kiện giao xe: Cảm biến áp suất lốp Steelmate & Dán film cách nhiệt Lumax", Quantity = 1, UnitPrice = 450000, ExpenseType = ExpenseType.Internal });
            db.ROs.Add(roPdi2);
            await db.SaveChangesAsync();

            var chkCreta = CreateDefaultChecklist();
            var chkFilm = chkCreta.FirstOrDefault(c => c.Code == "PDI.ACC.ITEMS");
            if (chkFilm != null) { chkFilm.Status = AuditStatus.Attention; chkFilm.Note = "Đang dán phim cách nhiệt sườn xe, chờ sấy nhiệt hoàn tất"; }

            var item2 = new PdiRequestItem
            {
                VIN = "RLHXXCR20240102",
                Model = "Hyundai Creta 2024",
                Spec = "1.5 AT Premium (Bản Cao cấp 2 tông màu)",
                Color = "Đỏ mận - Mui Đen thể thao",
                EngineNo = "SmartStream-G1.5-8841",
                BatteryNo = "CMF-60AH-1123",
                ContractNo = "HD-XE-2026/102",
                CustomerName = "Lê Thanh Hằng",
                CustomerPhone = "0983.551.229",
                CustomerAddress = "Cầu Giấy, Hà Nội",
                ExpectedDeliveryDate = DateTime.Today.AddDays(2),
                FlagAccessory = true,
                AccessoryNote = "Dán phim cách nhiệt Lumax, Cảm biến áp suất lốp Steelmate hiển thị màn hình zin, Thảm lót sàn cao su đúc nguyên khối",
                Status = PdiItemStatus.InProgress,
                Inspector = "KTV Tuấn",
                InspectionDate = DateTime.Today.AddHours(-2),
                InspectionNotes = "Đã kiểm tra động cơ, ắc quy, hệ thống chiếu sáng và lái đạt chuẩn. Đang lắp đặt phụ kiện trong khoang đồng sơn.",
                ROId = roPdi2.Id,
                RONo = roPdi2.Code,
                ChecklistItems = chkCreta
            };
            pdi2.Items.Add(item2);
            db.PdiRequests.Add(pdi2);
            await db.SaveChangesAsync();

            // 3. Phiếu PDI mới lập (Pending) - Hyundai Accent 2024 1.5 AT Đặc biệt
            var pdi3 = new PdiRequest
            {
                PdiReqNo = "PDI260427-003",
                DealerCode = "HYUNDAI-MAIN",
                CreatedDate = DateTime.Today,
                Remark = "Xe Accent thế hệ mới vừa hạ xe lồng về kho đại lý sáng nay, yêu cầu xưởng tiếp nhận kiểm tra PDI chuẩn bị giao xe tuần tới.",
                FlagAccessory = false,
                CreatedBy = "Showroom KD 1",
                Status = PdiRequestStatus.Pending
            };

            var item3 = new PdiRequestItem
            {
                VIN = "RLHXXAC20240115",
                Model = "Hyundai Accent 2024",
                Spec = "1.5 AT Đặc biệt (All New Accent)",
                Color = "Bạc ánh kim (Sleek Silver)",
                EngineNo = "SmartStream-G1.5-9952",
                BatteryNo = "CMF-55AH-7721",
                ContractNo = "HD-XE-2026/115",
                CustomerName = "Nguyễn Văn Toàn",
                CustomerPhone = "0904.332.188",
                CustomerAddress = "Hà Đông, Hà Nội",
                ExpectedDeliveryDate = DateTime.Today.AddDays(4),
                FlagAccessory = false,
                Status = PdiItemStatus.Pending,
                ChecklistItems = CreateDefaultChecklist()
            };
            pdi3.Items.Add(item3);
            db.PdiRequests.Add(pdi3);
            await db.SaveChangesAsync();
        }

        if (!await db.OrderComplains.AnyAsync())
        {
            var pOil = await db.Parts.FirstOrDefaultAsync(p => p.Code == "26300-35505");
            var pBrake = await db.Parts.FirstOrDefaultAsync(p => p.Code == "58101-C1A00");
            var pSpark = await db.Parts.FirstOrDefaultAsync(p => p.Code == "18846-11070");
            var orderPart = await db.OrderParts.FirstOrDefaultAsync();

            // 1. Khiếu nại đã giải quyết hoàn tất & TST chấp thuận đổi mới (Approved / Finished)
            var c1 = new OrderComplain
            {
                OrderComplainNo = "KN260425-001",
                DealerCode = "HYUNDAI-MAIN",
                DealerName = "Hyundai Giải Phóng",
                ComplainType = OrderComplainType.DamagedInTransit,
                OrderPartId = orderPart?.Id,
                OrderPartNo = orderPart?.OrderPartNo ?? "PO260427-001",
                PartId = pOil?.Id ?? 1,
                PartCode = pOil?.Code ?? "26300-35505",
                PartName = pOil?.Name ?? "Lọc dầu động cơ chính hãng Hyundai",
                Unit = pOil?.Unit ?? "Cái",
                Quantity = 2,
                UnitPrice = pOil?.CostPrice ?? 90000,
                RequestOrderNo = "DO-TST-2026-0411",
                TransportUnit = "Vận tải Đất Việt Express (Xe tải 29H-441.82)",
                DeliveryDateTime = DateTime.Today.AddDays(-2).AddHours(9),
                DeliveryBy = "Tài xế Nguyễn Văn Dũng",
                DeliveryLocation = "Kho phụ tùng chính - Tầng 1",
                ReceiveBy = "Thủ kho Tuấn",
                Description = "Thùng carton ngoài bị bẹp dập trong quá trình xếp dỡ vận chuyển. Khi mở hộp kiểm tra có 02 lọc dầu bị móp méo đầu ren và cong vênh van cao su một chiều, không thể lắp ráp lên xe an toàn.",
                DMSStatus = DMSOrderComplainStatus.Finished,
                TSTStatus = TSTOrderComplainStatus.Approved,
                TSTSolution = ComplainSolution.ReplaceNew,
                SolutionNote = "Bộ phận Bảo hành TST/HTC đã tiếp nhận và xác nhận hư hại do khâu chằng buộc vận chuyển. Đồng ý đổi mới 02 lọc dầu nguyên seal trong chuyến xe giao hàng kế tiếp ngày 28/04.",
                CreatedBy = "Thủ kho Tuấn",
                CreatedAt = DateTime.Today.AddDays(-2).AddHours(10),
                SentAt = DateTime.Today.AddDays(-2).AddHours(11),
                DecidedAt = DateTime.Today.AddDays(-1).AddHours(15),
                FinishedAt = DateTime.Today.AddDays(-1).AddHours(16),
                AttachFiles = [
                    new OrderComplainAttachFile
                    {
                        ImageType = "Ngoại quan hư hại",
                        FileName = "LocDau_MopRen_01.jpg",
                        FilePath = "/img/complain/locdau_mop_01.jpg",
                        Note = "Vết móp méo sâu 4mm trên bề mặt ren bắt vào lốc máy"
                    },
                    new OrderComplainAttachFile
                    {
                        ImageType = "Bao bì rách vỡ",
                        FileName = "ThungCarton_BepDap.jpg",
                        FilePath = "/img/complain/carton_bep.jpg",
                        Note = "Góc thùng carton bị vật nặng đè rách nát"
                    },
                    new OrderComplainAttachFile
                    {
                        ImageType = "Biên bản giao vận",
                        FileName = "BienBanDongKiem_DatViet.pdf",
                        FilePath = "/docs/complain/bb_dongkiem_0411.pdf",
                        Note = "Biên bản kiểm hàng đồng kiểm có chữ ký xác nhận của tài xế Dũng"
                    }
                ]
            };

            // 2. Khiếu nại đã gửi TST đang thẩm định giám định mã lỗi (Sent / UnderReview)
            var c2 = new OrderComplain
            {
                OrderComplainNo = "KN260427-002",
                DealerCode = "HYUNDAI-MAIN",
                DealerName = "Hyundai Giải Phóng",
                ComplainType = OrderComplainType.WrongPart,
                OrderPartId = orderPart?.Id,
                OrderPartNo = orderPart?.OrderPartNo ?? "PO260427-001",
                PartId = pBrake?.Id ?? 4,
                PartCode = pBrake?.Code ?? "58101-C1A00",
                PartName = pBrake?.Name ?? "Bộ má phanh đĩa trước",
                Unit = pBrake?.Unit ?? "Bộ",
                Quantity = 1,
                UnitPrice = pBrake?.CostPrice ?? 850000,
                VIN = "RLHXXTC002",
                RequestOrderNo = "DO-TST-2026-0425",
                TransportUnit = "Viettel Post (Vận đơn VT-HN-88219)",
                DeliveryDateTime = DateTime.Today.AddHours(-4),
                DeliveryBy = "Shipper Viettel Post Hoàng Nam",
                DeliveryLocation = "Kho phụ tùng chính",
                ReceiveBy = "Thủ kho Tuấn",
                AssembleDateTime = DateTime.Today.AddHours(-2),
                AssembleBy = "KTV Quang",
                Description = "Đơn đặt hàng yêu cầu bộ má phanh trước cho xe Tucson/Santa Fe (mã 58101-C1A00), tem vỏ hộp ghi đúng mã nhưng ruột bên trong là má phanh xe Grand i10 kích thước nhỏ hơn 30%, KTV đưa vào gá không vừa đĩa.",
                DMSStatus = DMSOrderComplainStatus.Sent,
                TSTStatus = TSTOrderComplainStatus.UnderReview,
                SolutionNote = "TST đang yêu cầu thủ kho tổng TST rà soát lại lô đóng gói ngày 24/04 từ nhà máy.",
                CreatedBy = "Thủ kho Tuấn",
                CreatedAt = DateTime.Today.AddHours(-3),
                SentAt = DateTime.Today.AddHours(-1),
                AttachFiles = [
                    new OrderComplainAttachFile
                    {
                        ImageType = "Tem nhãn & Barcode",
                        FileName = "TemNhan_VoHop_58101.jpg",
                        FilePath = "/img/complain/tem_vohop.jpg",
                        Note = "Tem dán ngoài hộp ghi mã 58101-C1A00"
                    },
                    new OrderComplainAttachFile
                    {
                        ImageType = "Ngoại quan hư hại",
                        FileName = "SoSanh_MaPhanh_ThucTe.jpg",
                        FilePath = "/img/complain/ma_phanh_lech.jpg",
                        Note = "So sánh kích thước má thực tế ngắn hơn cùm phanh xe Tucson"
                    }
                ]
            };

            // 3. Khiếu nại mới lập chờ gửi phê duyệt (Pending / Processing)
            var c3 = new OrderComplain
            {
                OrderComplainNo = "KN260427-003",
                DealerCode = "HYUNDAI-MAIN",
                DealerName = "Hyundai Giải Phóng",
                ComplainType = OrderComplainType.QualityDefect,
                PartId = pSpark?.Id ?? 5,
                PartCode = pSpark?.Code ?? "18846-11070",
                PartName = pSpark?.Name ?? "Bugi đánh lửa Iridium cao cấp",
                Unit = pSpark?.Unit ?? "Cái",
                Quantity = 1,
                UnitPrice = pSpark?.CostPrice ?? 110000,
                TransportUnit = "Giao trực tiếp từ Kho trung chuyển HTC",
                DeliveryDateTime = DateTime.Today.AddMinutes(-45),
                DeliveryBy = "Nhân viên kho HTC Lê Dũng",
                DeliveryLocation = "Kho phụ tùng chính",
                ReceiveBy = "Thủ kho Tuấn",
                Description = "Khi mở nắp hộp kiểm tra kỹ thuật trước khi nhập kệ, phát hiện phần sứ cách điện của bugi bị rạn nứt một đường dài từ chân cực ren lên đầu chụp, có nguy cơ đánh lửa phóng điện ra ngoài thân máy.",
                DMSStatus = DMSOrderComplainStatus.Pending,
                TSTStatus = TSTOrderComplainStatus.Processing,
                CreatedBy = "Thủ kho Tuấn",
                CreatedAt = DateTime.Today.AddMinutes(-30),
                AttachFiles = [
                    new OrderComplainAttachFile
                    {
                        ImageType = "Ngoại quan hư hại",
                        FileName = "Bugi_NutSu_ChiTiet.jpg",
                        FilePath = "/img/complain/bugi_nut_su.jpg",
                        Note = "Vết rạn nứt trên lớp men sứ cách điện"
                    }
                ]
            };

            db.OrderComplains.AddRange(c1, c2, c3);
            await db.SaveChangesAsync();
        }

        if (!await db.TechnicalLibraries.AnyAsync())
        {
            var ro1 = await db.ROs.FirstOrDefaultAsync();
            var t1 = new TechnicalLibrary
            {
                TechnicalLibraryCode = "TLIB260427-001",
                DealerCode = "HYUNDAI-MAIN",
                DealerName = "Hyundai Giải Phóng",
                PlateNo = "30A-123.45",
                Model = "Hyundai Tucson",
                Engine = "SmartStream G1.6 T-GDi",
                Gear = "7DCT",
                Version = "1.6T Đặc biệt",
                ReRepairType = TechnicalLibraryReRepairType.Engine,
                Type = TechnicalLibraryType.ReRepair,
                IsActive = true,
                ROId = ro1?.Id,
                ReRepairRemark = "Xe tăng tốc từ 40-60 km/h bị hụt ga, giật cục nhẹ, đèn Check Engine nhấp nháy báo lỗi mã DTC P0014 (Exhaust Camshaft Position Timing - Over-Advanced Bank 1).",
                ReRepairFeedback = "Khách hàng phản ánh xe đã thay dầu bảo dưỡng 1 tuần trước nhưng hiện tượng rung giật vẫn tái diễn khi vượt xe trên cao tốc. Xưởng đã kiểm tra bugi và bô-bin nhưng không hết.",
                ExclusionTest = "Đo áp suất dầu động cơ đạt chuẩn 2.4 bar tại 1500 RPM. Kiểm tra điện trở van điều khiển dầu cam xả OCV (Oil Control Valve) đo được 7.2 Ohm (chuẩn 6.9 - 7.9 Ohm). Tuy nhiên khi kích hoạt van OCV bằng máy chẩn đoán GDS Mobile thì lõi van phản hồi trễ 0.8s do cặn dầu bám nghẹt lưới lọc vi mô van OCV cam xả.",
                ReRepairReason = "Lưới lọc dầu vi mô (oil micro-filter) lắp trước cổng van OCV cam xả bị mạt muội than dầu cũ bám két làm giảm lưu lượng dầu cấp vào bánh răng điều khiển góc mở sớm CVVT cam xả.",
                ReRepairSolution = "Tháo cụm van OCV cam xả, ngâm rửa siêu âm làm sạch lưới lọc dầu vi mô bằng dung dịch tẩy cặn chuyên dụng Hyundai Carb Cleaner; vệ sinh rãnh dẫn dầu nắp quy-lát; lắp lại xiết lực 10 Nm; xóa lỗi DTC và thực hiện quy trình CVVT Learn Adaptation trên máy GDS. Chạy thử nghiệm 25 km xe mượt mà, không tái diễn.",
                CreatedBy = "KTV Trưởng Nguyễn Văn Minh",
                CreatedAt = DateTime.Now.AddDays(-3),
                ApprovedAt = DateTime.Now.AddDays(-2),
                ApprovedBy = "Phòng Kỹ thuật HTC (HQ)"
            };

            var t2 = new TechnicalLibrary
            {
                TechnicalLibraryCode = "TLIB260427-002",
                DealerCode = "HYUNDAI-MAIN",
                DealerName = "Hyundai Giải Phóng",
                PlateNo = "51G-678.90",
                Model = "Hyundai Santa Fe",
                Engine = "SmartStream D2.2 CRDi",
                Gear = "8DCT ướt",
                Version = "2.2D Cao cấp",
                ReRepairType = TechnicalLibraryReRepairType.AirConditioning,
                Type = TechnicalLibraryType.Normal,
                IsActive = true,
                ReRepairRemark = "Điều hòa làm lạnh yếu hoặc mất lạnh ngắt quãng khi xe chạy đường trường trên 30 phút, dừng xe nổ máy tại chỗ thì mát lại bình thường.",
                ReRepairFeedback = "Khách hàng mang xe kiểm tra ở gara ngoài đã nạp lại ga R134a và thay lọc gió điều hòa nhưng chạy cao tốc trời nắng vẫn bị ngắt lốc lạnh.",
                ExclusionTest = "Đo áp suất ga bằng đồng hồ chuyên dụng: Áp suất thấp (Low side) dao động bất thường từ 1.8 đến 3.8 bar, áp suất cao (High side) ổn định 14.5 bar. Kiểm tra tín hiệu PWM điều khiển van điện tử ECV (Electronic Control Valve) trên lốc nén: khi mất lạnh, hộp FATC vẫn phát tín hiệu xung PWM 80% nhưng độ mở van cơ khí bị kẹt không đổi.",
                ReRepairReason = "Van điều khiển dung tích biến thiên ECV của lốc nén (Compressor) bị mòn kẹt lò xo van hồi lưu khi lốc đạt nhiệt độ cao sau thời gian hoạt động liên tục.",
                ReRepairSolution = "Thu hồi môi chất lạnh ga R134a; tháo phe hãm thay thế van điều khiển điện tử ECV mới chính hãng mã 97674-2S000; hút chân không hệ thống tối thiểu 30 phút; nạp lại đúng định lượng 550g ± 25g ga R134a và bổ sung 30ml dầu bôi trơn lốc PAG46. Kiểm tra nhiệt độ cửa gió đạt 5.5°C ổn định.",
                CreatedBy = "CVDV Hoàng Quốc Bảo",
                CreatedAt = DateTime.Now.AddDays(-2),
                ApprovedAt = DateTime.Now.AddDays(-1),
                ApprovedBy = "Chuyên viên Kỹ thuật HTC Trần Đức"
            };

            var t3 = new TechnicalLibrary
            {
                TechnicalLibraryCode = "TLIB260427-003",
                DealerCode = "HYUNDAI-MAIN",
                DealerName = "Hyundai Giải Phóng",
                PlateNo = "30E-888.66",
                Model = "Hyundai Accent",
                Engine = "Kappa 1.4 MPI",
                Gear = "6AT",
                Version = "1.4 AT Đặc biệt",
                ReRepairType = TechnicalLibraryReRepairType.Electrical,
                Type = TechnicalLibraryType.ReRepair,
                IsActive = false,
                ReRepairRemark = "Vô lăng rung nhẹ và có tiếng kêu 'cục... cục' ở trục lái khi đánh lái chết tại chỗ hoặc khi xe di chuyển tốc độ chậm < 15 km/h vào cua.",
                ReRepairFeedback = "Khách hàng băn khoăn về an toàn khi lái xe, cảm nhận tay lái có độ rơ lớn hơn bình thường sau khi đi qua đoạn đường gồ ghề ngập nước.",
                ExclusionTest = "Kiểm tra thước lái cơ khí, rô-tuyn lái trong/ngoài và khớp các-đăng cột lái đều chắc chắn, không rơ lỏng. Dùng máy chẩn đoán GDS kiểm tra mã lỗi hệ thống MDPS không có lỗi DTC. Lắc nhẹ vô lăng xác định tiếng kêu phát ra từ khớp nối hoa thị đệm giảm chấn cao su bên trong cụm mô tơ trợ lực điện MDPS.",
                ReRepairReason = "Vòng đệm cao su giảm chấn khớp nối mô tơ trợ lực lái MDPS (Flexible Coupler) bị lão hóa nứt vỡ sau 4 năm sử dụng, tạo khe hở va đập giữa các cánh hoa thị truyền lực.",
                ReRepairSolution = "Hạ cụm cột lái MDPS; tách vỏ mô tơ trợ lực điện; làm sạch vụn cao su cũ; thay thế đệm hoa thị giảm chấn MDPS mới chính hãng mã 56315-2K000FFF; lắp lại và hiệu chỉnh điểm 0 cảm biến góc lái (Steering Angle Sensor ASP Calibration) trên GDS Mobile.",
                CreatedBy = "KTV Đỗ Hoàng Nam",
                CreatedAt = DateTime.Now.AddDays(-1)
            };

            var t4 = new TechnicalLibrary
            {
                TechnicalLibraryCode = "TLIB260427-004",
                DealerCode = "HYUNDAI-MAIN",
                DealerName = "Hyundai Giải Phóng",
                PlateNo = "29H-921.34",
                Model = "Hyundai Creta",
                Engine = "SmartStream G1.5",
                Gear = "IVT",
                Version = "1.5 Cao cấp (SmartSense)",
                ReRepairType = TechnicalLibraryReRepairType.BrakeADAS,
                Type = TechnicalLibraryType.Normal,
                IsActive = true,
                ReRepairRemark = "Hệ thống cảnh báo va chạm trước FCA và hỗ trợ giữ làn LKA thỉnh thoảng báo tạm ngắt 'Check SmartSense System' trên màn hình đồng hồ khi trời mưa lớn hoặc sáng sớm sương mù.",
                ReRepairFeedback = "Khách hàng lo ngại camera trên kính lái bị hỏng vì lỗi chỉ xuất hiện ngắt quãng lúc thời tiết ẩm ướt.",
                ExclusionTest = "Đọc lỗi máy GDS: DTC C161108 - CAN Communication Time-out Camera. Kiểm tra ngoại quan mặt kính chắn gió phía trước cụm Multifunction Front Camera phát hiện có lớp màng hơi nước đọng mờ bên trong nắp chụp ốp camera.",
                ReRepairReason = "Nắp chụp chống chói camera kính lái bị hở gioăng xốp đệm viền, làm hơi ẩm điều hòa bốc lên ngưng tụ thành sương che khuất ống kính cảm biến hình ảnh CMOS.",
                ReRepairSolution = "Tháo ốp gương chiếu hậu và nắp bảo vệ camera kính lái; vệ sinh sạch mặt trong kính lái và tròng kính camera bằng khăn sợi microfiber chuyên dụng; thay mới dải đệm mút xốp chống ẩm chính hãng 86190-C9000; dùng máy chẩn đoán GDS chạy quy trình Front Radar & Camera Calibration Alignment.",
                CreatedBy = "KTV Trưởng Nguyễn Văn Minh",
                CreatedAt = DateTime.Now.AddDays(-5),
                ApprovedAt = DateTime.Now.AddDays(-4),
                ApprovedBy = "Phòng Dịch vụ HTC"
            };

            db.TechnicalLibraries.AddRange(t1, t2, t3, t4);
            await db.SaveChangesAsync();
        }

        if (!await db.ServiceItems.AnyAsync())
        {
            var services = new List<ServiceItem>
            {
                // BDD: Bảo dưỡng định kỳ
                new() {
                    Code = "BDD-010",
                    Name = "Bảo dưỡng định kỳ Cấp 1 (5.000 km)",
                    ROType = ServiceROType.BDD,
                    StdManHour = 0.8m,
                    Price = 250000,
                    Cost = 100000,
                    VatPercent = 8,
                    Model = "Tất cả các dòng xe Hyundai",
                    FlagWarranty = false,
                    Note = "Kiểm tra gầm, siết ốc, áp suất lốp, nước làm mát, nước rửa kính"
                },
                new() {
                    Code = "BDD-020",
                    Name = "Bảo dưỡng định kỳ Cấp 2 (10.000 km)",
                    ROType = ServiceROType.BDD,
                    StdManHour = 1.2m,
                    Price = 380000,
                    Cost = 150000,
                    VatPercent = 8,
                    Model = "Tất cả các dòng xe Hyundai",
                    FlagWarranty = false,
                    Note = "Bảo dưỡng hệ thống phanh 4 bánh, đảo lốp, vệ sinh lọc gió động cơ & điều hòa"
                },
                new() {
                    Code = "BDD-030",
                    Name = "Bảo dưỡng định kỳ Cấp 3 (20.000 km)",
                    ROType = ServiceROType.BDD,
                    StdManHour = 1.8m,
                    Price = 520000,
                    Cost = 220000,
                    VatPercent = 8,
                    Model = "Tất cả các dòng xe Hyundai",
                    FlagWarranty = false,
                    Note = "Bảo dưỡng phanh, kiểm tra góc đặt bánh xe, vệ sinh họng hút ga, thay lọc điều hòa"
                },
                new() {
                    Code = "BDD-040",
                    Name = "Bảo dưỡng định kỳ Cấp 4 (40.000 km) - Đại tu định kỳ",
                    ROType = ServiceROType.BDD,
                    StdManHour = 3.0m,
                    Price = 900000,
                    Cost = 400000,
                    VatPercent = 8,
                    Model = "Tất cả các dòng xe Hyundai",
                    FlagWarranty = false,
                    Note = "Đại dưỡng toàn diện khoang máy, siết gầm, thay bugi, thay dầu phanh, xúc xả két nước"
                },

                // SCC: Sửa chữa chung máy - gầm - điện
                new() {
                    Code = "SCC-ENG-01",
                    Name = "Cân chỉnh góc đặt bánh xe 3D laser Hunter & Cân bằng động 4 bánh",
                    ROType = ServiceROType.SCC,
                    StdManHour = 1.0m,
                    Price = 450000,
                    Cost = 180000,
                    VatPercent = 8,
                    Model = "Tất cả các dòng xe",
                    FlagWarranty = false,
                    Note = "Cân chỉnh góc Camber, Caster, Toe-in bằng hệ thống camera cảm biến Hunter HawkEye Elite"
                },
                new() {
                    Code = "SCC-ENG-02",
                    Name = "Thay cụm bi may-ơ & bạc đạn bánh trước",
                    ROType = ServiceROType.SCC,
                    StdManHour = 1.5m,
                    Price = 550000,
                    Cost = 250000,
                    VatPercent = 8,
                    Model = "Accent / Elantra / Creta",
                    FlagWarranty = false,
                    Note = "Ép tháo lắp bạc đạn moay-ơ bằng máy ép thủy lực chuyên dụng 20 tấn"
                },
                new() {
                    Code = "SCC-BRK-01",
                    Name = "Thay bộ má phanh đĩa trước & Láng bề mặt đĩa phanh",
                    ROType = ServiceROType.SCC,
                    StdManHour = 1.2m,
                    Price = 420000,
                    Cost = 160000,
                    VatPercent = 8,
                    Model = "Tất cả các dòng xe",
                    FlagWarranty = false,
                    Note = "Láng phẳng bề mặt đĩa phanh triệt tiêu hiện tượng rung giật và tiếng kêu rít khi đạp phanh"
                },
                new() {
                    Code = "SCC-WAR-01",
                    Name = "Thay thế cụm thước lái trợ lực điện MDPS & Khớp nối hoa thị (Bảo hành hãng)",
                    ROType = ServiceROType.SCC,
                    StdManHour = 2.2m,
                    Price = 750000,
                    Cost = 350000,
                    VatPercent = 8,
                    Model = "Accent / Creta / Tucson",
                    FlagWarranty = true,
                    Note = "Theo quy trình bảo hành HTC: Triệt tiêu tiếng kêu lộc cộc trục lái, cân chỉnh cảm biến góc lái SAS"
                },
                new() {
                    Code = "SCC-WAR-02",
                    Name = "Thay thế lốc nén điều hòa không khí Compressor (Bảo hành HTC)",
                    ROType = ServiceROType.SCC,
                    StdManHour = 2.5m,
                    Price = 850000,
                    Cost = 400000,
                    VatPercent = 8,
                    Model = "Santa Fe / Tucson",
                    FlagWarranty = true,
                    Note = "Bảo hành thay mới lốc lạnh chính hãng, xúc rửa đường ống ga, nạp dầu bôi trơn PAG và hút chân không"
                },

                // SCD: Sơn sấy & Đồng sơn thân vỏ
                new() {
                    Code = "SCD-PNT-01",
                    Name = "Sơn sấy cản trước hoàn thiện (Sơn 3 lớp tiêu chuẩn phòng hấp)",
                    ROType = ServiceROType.SCD,
                    StdManHour = 2.5m,
                    Price = 1100000,
                    Cost = 450000,
                    VatPercent = 8,
                    Model = "Tất cả các dòng xe",
                    FlagWarranty = false,
                    Note = "Quy trình sơn sấy phòng nhiệt PPG/Dupont, bao gồm pha màu vi tính và đánh bóng hoàn thiện"
                },
                new() {
                    Code = "SCD-BOD-02",
                    Name = "Gò nắn phục hồi biến dạng ba-đờ-sốc & Tai xe trước",
                    ROType = ServiceROType.SCD,
                    StdManHour = 2.0m,
                    Price = 700000,
                    Cost = 300000,
                    VatPercent = 8,
                    Model = "Tất cả các dòng xe",
                    FlagWarranty = false,
                    Note = "Kéo nắn khung tai xe trên giàn cân chỉnh thân vỏ Car-O-Liner chính xác từng milimet"
                },
                new() {
                    Code = "SCD-PNT-03",
                    Name = "Sơn sấy cánh cửa bên phụ (Gồm phủ bóng chống xước cao cấp)",
                    ROType = ServiceROType.SCD,
                    StdManHour = 3.0m,
                    Price = 1350000,
                    Cost = 550000,
                    VatPercent = 8,
                    Model = "Santa Fe / Custin / Tucson",
                    FlagWarranty = false,
                    Note = "Sơn dặm vá hoặc cả cánh cửa, bảo hành độ đồng màu 99% theo code màu gốc nhà sản xuất"
                },

                // SCS: Dịch vụ sửa chữa nhanh Quick Service
                new() {
                    Code = "SCS-QCK-01",
                    Name = "Dịch vụ Sửa chữa nhanh Express Service 60 phút (2 KTV song hành)",
                    ROType = ServiceROType.SCS,
                    StdManHour = 1.0m,
                    Price = 300000,
                    Cost = 120000,
                    VatPercent = 8,
                    Model = "Tất cả các dòng xe",
                    FlagWarranty = false,
                    Note = "Quy trình Quick Service khép kín: thay dầu nhớt, kiểm tra 20 điểm an toàn trong 60 phút"
                },
                new() {
                    Code = "SCS-CLN-02",
                    Name = "Vệ sinh diệt khuẩn giàn lạnh điều hòa bằng công nghệ nội soi Nano Bạc",
                    ROType = ServiceROType.SCS,
                    StdManHour = 0.8m,
                    Price = 450000,
                    Cost = 150000,
                    VatPercent = 8,
                    Model = "Tất cả các dòng xe",
                    FlagWarranty = false,
                    Note = "Đưa camera nội soi vào dàn lạnh, xịt dung dịch tẩy ố mốc và phủ nano kháng khuẩn ion bạc"
                },

                // PDI: Kiểm tra xuất xưởng xe mới
                new() {
                    Code = "PDI-STD-01",
                    Name = "Kiểm tra nghiệm thu 25 hạng mục xe mới xuất xưởng (PDI tiêu chuẩn HTC)",
                    ROType = ServiceROType.PDI,
                    StdManHour = 1.5m,
                    Price = 350000,
                    Cost = 150000,
                    VatPercent = 8,
                    Model = "Tất cả xe mới",
                    FlagWarranty = true,
                    Note = "Checklist 5 nhóm: Khoang máy & dung dịch, thân vỏ sơn lốp, hệ thống đèn còi, tiện nghi điện tử, lái thử"
                },
                new() {
                    Code = "PDI-BLU-02",
                    Name = "Cài đặt & Kích hoạt viễn thông Hyundai Bluelink / Hộp điều khiển BCM",
                    ROType = ServiceROType.PDI,
                    StdManHour = 0.5m,
                    Price = 180000,
                    Cost = 60000,
                    VatPercent = 8,
                    Model = "Creta / Tucson / Santa Fe / Custin",
                    FlagWarranty = true,
                    Note = "Kết nối mạng di động kích hoạt SIM viễn thông và tài khoản ứng dụng Hyundai Bluelink"
                },

                // SPK: Phụ kiện & Chăm sóc xe Detailing
                new() {
                    Code = "SPK-ACC-01",
                    Name = "Lắp đặt bộ camera hành trình 2 mắt trước/sau tích hợp GPS và hiển thị tốc độ",
                    ROType = ServiceROType.SPK,
                    StdManHour = 1.0m,
                    Price = 300000,
                    Cost = 100000,
                    VatPercent = 8,
                    Model = "Tất cả các dòng xe",
                    FlagWarranty = false,
                    Note = "Đi dây thẩm mỹ giấu kín cột A, đấu nguồn an toàn qua cầu chì ACC dự phòng không cắt trích dây"
                },
                new() {
                    Code = "SPK-CER-02",
                    Name = "Phủ Ceramic 9H siêu bóng bảo vệ bề mặt sơn toàn thân xe",
                    ROType = ServiceROType.SPK,
                    StdManHour = 4.5m,
                    Price = 3500000,
                    Cost = 1200000,
                    VatPercent = 8,
                    Model = "Tất cả các dòng xe",
                    FlagWarranty = false,
                    Note = "Hiệu chỉnh bề mặt sơn 3 bước đánh bóng xóa xước xoáy, phủ 2 lớp Ceramic 9H và sấy đèn sấy hồng ngoại"
                }
            };

            db.ServiceItems.AddRange(services);
            await db.SaveChangesAsync();
        }

        if (!await db.Suppliers.AnyAsync())
        {
            var s1 = new Supplier { Code = "HTC", Name = "Công ty Cổ phần Liên doanh Ô tô Hyundai Thành Công Việt Nam", Address = "Tòa nhà Epic Tower, Nam Từ Liêm, Hà Nội", Phone = "024.3826.2614", Email = "parts@hyundai-thanhcong.vn", ContactName = "Nguyễn Hoàng Minh", ContactPhone = "0912.345.678", TaxCode = "0102872391", IsActive = true };
            var s2 = new Supplier { Code = "MOBIS", Name = "Công ty TNHH Phụ tùng Hyundai Mobis Việt Nam", Address = "KCN Đình Trám, Việt Yên, Bắc Giang", Phone = "0204.387.9999", Email = "order@mobis.co.kr", ContactName = "Kim Jung Wook", ContactPhone = "0988.777.666", TaxCode = "2400589123", IsActive = true };
            var s3 = new Supplier { Code = "CASTROL", Name = "Công ty TNHH Castrol BP Petco Việt Nam", Address = "Quận 1, TP. Hồ Chí Minh", Phone = "028.3821.9153", Email = "dauthuongmai@castrol.com", ContactName = "Lê Quang Vinh", ContactPhone = "0903.888.999", TaxCode = "0300628284", IsActive = true };
            var s4 = new Supplier { Code = "DENSO", Name = "Công ty TNHH Denso Việt Nam", Address = "KCN Thăng Long, Đông Anh, Hà Nội", Phone = "024.3881.1601", Email = "sales@denso.com.vn", ContactName = "Phạm Tuấn Anh", ContactPhone = "0915.222.333", TaxCode = "0101183569", IsActive = true };
            db.Suppliers.AddRange(s1, s2, s3, s4);
            await db.SaveChangesAsync();
        }

        if (!await db.SupplierPayments.AnyAsync())
        {
            var supHtc = await db.Suppliers.FirstOrDefaultAsync(s => s.Code == "HTC");
            var supMobis = await db.Suppliers.FirstOrDefaultAsync(s => s.Code == "MOBIS");
            var sparkPart = await db.Parts.FirstOrDefaultAsync(p => p.Code == "18846-11070");
            var brakePart = await db.Parts.FirstOrDefaultAsync(p => p.Code == "58101-C1A00");

            if (supMobis != null && sparkPart != null)
            {
                var pay1 = new SupplierPayment
                {
                    SupplierPaymentNo = $"PXNCC-{DateTime.Today:yyMMdd}-001",
                    SupplierId = supMobis.Id,
                    SupplierName = supMobis.Name,
                    Address = supMobis.Address,
                    PaymentDate = DateTime.Today.AddDays(-2),
                    PaymentType = SupplierPaymentType.ReturnDefective,
                    Status = SupplierPaymentStatus.Approved,
                    OrderPartNo = "PO260427-001",
                    TSTRequestNo = "TST-CLAIM-2026-089",
                    Description = "Xuất trả 1 bugi đánh lửa Iridium bị nứt sứ cách điện theo biên bản kiểm định bảo hành kỹ thuật NPP TST",
                    CreatedBy = "Thủ kho Hùng",
                    CreatedAt = DateTime.Now.AddDays(-2),
                    ApprovedBy = "Trưởng kho dịch vụ Tuấn",
                    ApprovedAt = DateTime.Now.AddDays(-2).AddHours(2),
                    Items = [
                        new SupplierPaymentDetail
                        {
                            PartId = sparkPart.Id,
                            QtyPay = 1,
                            Price = sparkPart.CostPrice > 0 ? sparkPart.CostPrice : 110000,
                            VatPercent = 10,
                            QtyInventory = sparkPart.InStock + 1,
                            LocationCode = sparkPart.Location,
                            StockInNo = "PN-260427-001",
                            Reason = "Lỗi nứt sứ cách điện, kiểm định TST duyệt đổi mới bù trừ công nợ"
                        }
                    ]
                };
                db.SupplierPayments.Add(pay1);
            }

            if (supHtc != null && brakePart != null)
            {
                var pay2 = new SupplierPayment
                {
                    SupplierPaymentNo = $"PXNCC-{DateTime.Today:yyMMdd}-002",
                    SupplierId = supHtc.Id,
                    SupplierName = supHtc.Name,
                    Address = supHtc.Address,
                    PaymentDate = DateTime.Today,
                    PaymentType = SupplierPaymentType.ReturnSurplus,
                    Status = SupplierPaymentStatus.Pending,
                    OrderPartNo = "PO260427-002",
                    TSTRequestNo = "HTC-RET-2026-015",
                    Description = "Xuất trả 2 bộ má phanh đĩa trước giao thừa so với đơn đặt hàng PO đại lý",
                    CreatedBy = "Thủ kho Hùng",
                    CreatedAt = DateTime.Now,
                    Items = [
                        new SupplierPaymentDetail
                        {
                            PartId = brakePart.Id,
                            QtyPay = 2,
                            Price = brakePart.CostPrice > 0 ? brakePart.CostPrice : 850000,
                            VatPercent = 10,
                            QtyInventory = brakePart.InStock,
                            LocationCode = brakePart.Location,
                            StockInNo = "PN-260427-002",
                            Reason = "Giao thừa 2 bộ so với số lượng đặt hàng ban đầu"
                        }
                    ]
                };
                db.SupplierPayments.Add(pay2);
            }

            await db.SaveChangesAsync();
        }

        if (!await db.StockOutOrders.AnyAsync())
        {
            var ro1 = await db.ROs.Include(r => r.Car).Include(r => r.Customer).FirstOrDefaultAsync(r => r.Code.EndsWith("-001"));
            var ro2 = await db.ROs.Include(r => r.Car).Include(r => r.Customer).FirstOrDefaultAsync(r => r.Code.EndsWith("-002"));
            var ro3 = await db.ROs.Include(r => r.Car).Include(r => r.Customer).FirstOrDefaultAsync(r => r.Code.EndsWith("-003"));
            var p1 = await db.Parts.FirstOrDefaultAsync(p => p.Code == "26300-35505");
            var p2 = await db.Parts.FirstOrDefaultAsync(p => p.Code == "28113-1R100");
            var p3 = await db.Parts.FirstOrDefaultAsync(p => p.Code == "58101-C1A00");
            var cav1 = await db.Cavities.FirstOrDefaultAsync(c => c.CavityNo == "BAY-01");
            var cav2 = await db.Cavities.FirstOrDefaultAsync(c => c.CavityNo == "BAY-02");
            var cav3 = await db.Cavities.FirstOrDefaultAsync(c => c.CavityNo == "BAY-03");
            var stockOut1 = await db.StockOuts.FirstOrDefaultAsync();

            if (ro1 != null && p1 != null && p2 != null)
            {
                var soo1 = new StockOutOrder
                {
                    OrderNo = $"SOO{DateTime.Today:yyMMdd}-001",
                    OrderDate = DateTime.Today.AddDays(-1),
                    RequestDeliveryTime = DateTime.Today.AddDays(-1).AddHours(9),
                    Priority = StockOutOrderPriority.Emergency,
                    Status = StockOutOrderStatus.Completed,
                    ROId = ro1.Id,
                    CustomerId = ro1.CustomerId,
                    CarId = ro1.CarId,
                    CavityId = cav1?.Id,
                    StockOutId = stockOut1?.Id,
                    RequesterName = "KTV Nguyễn Văn Toàn",
                    Description = "Yêu cầu phụ tùng khẩn cấp bảo dưỡng cấp 40,000km xe Hyundai Tucson",
                    CreatedBy = "KTV Toàn",
                    CreatedAt = DateTime.Now.AddDays(-1).AddHours(-4),
                    ApprovedBy = "Thủ kho Hùng",
                    ApprovedAt = DateTime.Now.AddDays(-1).AddHours(-3),
                    Items = [
                        new StockOutOrderDetail
                        {
                            PartId = p1.Id,
                            PartCode = p1.Code,
                            PartName = p1.Name,
                            Unit = p1.Unit,
                            RequestQuantity = 1,
                            IssuedQuantity = 1,
                            UnitPrice = p1.SalePrice > 0 ? p1.SalePrice : 150000,
                            VatPercent = 8,
                            Note = "Lọc dầu động cơ máy xăng chính hãng"
                        },
                        new StockOutOrderDetail
                        {
                            PartId = p2.Id,
                            PartCode = p2.Code,
                            PartName = p2.Name,
                            Unit = p2.Unit,
                            RequestQuantity = 1,
                            IssuedQuantity = 1,
                            UnitPrice = p2.SalePrice > 0 ? p2.SalePrice : 220000,
                            VatPercent = 8,
                            Note = "Lọc gió động cơ định kỳ"
                        }
                    ]
                };
                db.StockOutOrders.Add(soo1);
                if (stockOut1 != null) stockOut1.StockOutOrderId = soo1.Id;
            }

            if (ro2 != null && p3 != null)
            {
                var soo2 = new StockOutOrder
                {
                    OrderNo = $"SOO{DateTime.Today:yyMMdd}-002",
                    OrderDate = DateTime.Today,
                    RequestDeliveryTime = DateTime.Today.AddHours(14),
                    Priority = StockOutOrderPriority.Urgent,
                    Status = StockOutOrderStatus.Approved,
                    ROId = ro2.Id,
                    CustomerId = ro2.CustomerId,
                    CarId = ro2.CarId,
                    CavityId = cav2?.Id,
                    RequesterName = "CVDV Trần Văn Long",
                    Description = "Yêu cầu lĩnh má phanh đĩa trước thay thế theo phản ánh mòn đĩa phanh của khách",
                    CreatedBy = "CVDV Long",
                    CreatedAt = DateTime.Now.AddHours(-3),
                    ApprovedBy = "Quản đốc xưởng Tuấn",
                    ApprovedAt = DateTime.Now.AddHours(-2),
                    Items = [
                        new StockOutOrderDetail
                        {
                            PartId = p3.Id,
                            PartCode = p3.Code,
                            PartName = p3.Name,
                            Unit = p3.Unit,
                            RequestQuantity = 1,
                            IssuedQuantity = 0,
                            UnitPrice = p3.SalePrice > 0 ? p3.SalePrice : 1250000,
                            VatPercent = 8,
                            Note = "Má phanh đĩa trước chuẩn Hyundai SantaFe"
                        }
                    ]
                };
                db.StockOutOrders.Add(soo2);
            }

            if (ro3 != null && p1 != null)
            {
                var soo3 = new StockOutOrder
                {
                    OrderNo = $"SOO{DateTime.Today:yyMMdd}-003",
                    OrderDate = DateTime.Today,
                    RequestDeliveryTime = DateTime.Today.AddHours(16),
                    Priority = StockOutOrderPriority.Normal,
                    Status = StockOutOrderStatus.Pending,
                    ROId = ro3.Id,
                    CustomerId = ro3.CustomerId,
                    CarId = ro3.CarId,
                    CavityId = cav3?.Id,
                    RequesterName = "KTV Lê Minh Đức",
                    Description = "Yêu cầu chuẩn bị vật tư lọc nhớt cho ca bảo dưỡng buổi chiều",
                    CreatedBy = "KTV Đức",
                    CreatedAt = DateTime.Now.AddHours(-1),
                    Items = [
                        new StockOutOrderDetail
                        {
                            PartId = p1.Id,
                            PartCode = p1.Code,
                            PartName = p1.Name,
                            Unit = p1.Unit,
                            RequestQuantity = 1,
                            IssuedQuantity = 0,
                            UnitPrice = p1.SalePrice > 0 ? p1.SalePrice : 150000,
                            VatPercent = 8,
                            Note = "Phục vụ thay nhớt máy"
                        }
                    ]
                };
                db.StockOutOrders.Add(soo3);
            }

            await db.SaveChangesAsync();
        }
    }

    private static List<PdiChecklistItem> CreateDefaultChecklist() =>
    [
        new PdiChecklistItem { Group = "Khoang động cơ & Dung dịch", Code = "PDI.ENG.OIL", Name = "Mức dầu động cơ & độ kín khít nắp châm, que thăm dầu", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Khoang động cơ & Dung dịch", Code = "PDI.ENG.COOLANT", Name = "Mức nước làm mát trong két nước tản nhiệt & bình nước phụ", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Khoang động cơ & Dung dịch", Code = "PDI.ENG.BRAKE", Name = "Mức dầu phanh / dầu ly hợp trong bình chứa", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Khoang động cơ & Dung dịch", Code = "PDI.ENG.WIPER", Name = "Mức nước rửa kính chắn gió & kiểm tra hoạt động vòi phun", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Khoang động cơ & Dung dịch", Code = "PDI.ENG.BATTERY", Name = "Điện áp bình ắc quy (>= 12.6V) & siết chặt cọc bình (+/-)", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Khoang động cơ & Dung dịch", Code = "PDI.ENG.LEAK", Name = "Kiểm tra rò rỉ dung dịch đường ống nhiên liệu, ống gió", Status = AuditStatus.Good },

        new PdiChecklistItem { Group = "Ngoại thất, Thân vỏ & Lốp xe", Code = "PDI.EXT.PAINT", Name = "Bề mặt sơn toàn thân xe (không trầy xước, không ố, đồng màu sơn)", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Ngoại thất, Thân vỏ & Lốp xe", Code = "PDI.EXT.PANEL", Name = "Khe hở và độ khít các tấm ốp nắp ca-pô, 4 cánh cửa, nắp cốp sau", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Ngoại thất, Thân vỏ & Lốp xe", Code = "PDI.EXT.GLASS", Name = "Kính chắn gió, kính sườn và kính hậu (không rạn nứt, ố mốc)", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Ngoại thất, Thân vỏ & Lốp xe", Code = "PDI.EXT.TIRE", Name = "Áp suất 4 lốp xe & lốp dự phòng theo tem thông số cột B", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Ngoại thất, Thân vỏ & Lốp xe", Code = "PDI.EXT.WHEEL", Name = "Độ siết bu-lông bánh xe theo tiêu chuẩn lực 120Nm, mâm xe hoàn hảo", Status = AuditStatus.Good },

        new PdiChecklistItem { Group = "Hệ thống chiếu sáng & Tín hiệu", Code = "PDI.LGT.HEAD", Name = "Cụm đèn pha, cốt, đèn ban ngày DRL và đèn sương mù trước", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Hệ thống chiếu sáng & Tín hiệu", Code = "PDI.LGT.SIGNAL", Name = "Đèn báo rẽ (xi-nhan trước/sau/gương) và đèn cảnh báo Hazard", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Hệ thống chiếu sáng & Tín hiệu", Code = "PDI.LGT.TAIL", Name = "Cụm đèn hậu, đèn phanh trên cao và đèn soi biển số", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Hệ thống chiếu sáng & Tín hiệu", Code = "PDI.LGT.HORN", Name = "Còi xe, âm lượng tín hiệu báo động chống trộm", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Hệ thống chiếu sáng & Tín hiệu", Code = "PDI.LGT.WIPER", Name = "Cần gạt mưa trước & sau hoạt động êm ái, gạt sạch nước", Status = AuditStatus.Good },

        new PdiChecklistItem { Group = "Nội thất & Tiện nghi điện tử", Code = "PDI.INT.AC", Name = "Hệ thống điều hòa AC (độ làm lạnh sâu, cửa gió, sấy kính)", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Nội thất & Tiện nghi điện tử", Code = "PDI.INT.SCREEN", Name = "Màn hình giải trí AVN cảm ứng, Apple CarPlay / Android Auto, Bluetooth", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Nội thất & Tiện nghi điện tử", Code = "PDI.INT.DASH", Name = "Cụm đồng hồ taplo điện tử (không báo đèn check lỗi động cơ/túi khí)", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Nội thất & Tiện nghi điện tử", Code = "PDI.INT.WINDOW", Name = "Kính cửa sổ chỉnh điện 4 cánh, chức năng 1 chạm chống kẹt an toàn", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Nội thất & Tiện nghi điện tử", Code = "PDI.INT.MIRROR", Name = "Gương chiếu hậu chỉnh & gập điện, sấy gương, gương trong xe", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Nội thất & Tiện nghi điện tử", Code = "PDI.INT.SEAT", Name = "Chỉnh ghế (điện/cơ), dây đai an toàn 3 điểm mọi vị trí ngồi", Status = AuditStatus.Good },

        new PdiChecklistItem { Group = "Phụ kiện lắp thêm & Bàn giao", Code = "PDI.ACC.ITEMS", Name = "Lắp đặt hoàn thiện gói phụ kiện cam kết (Dán film, Trải sàn, Camera...)", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Phụ kiện lắp thêm & Bàn giao", Code = "PDI.ACC.TOOLS", Name = "Bộ dụng cụ theo xe (kích nâng xe, tay quay bánh xe, móc kéo, tam giác phản quang)", Status = AuditStatus.Good },
        new PdiChecklistItem { Group = "Phụ kiện lắp thêm & Bàn giao", Code = "PDI.ACC.KEYS", Name = "Bàn giao đủ 2 chìa khóa Smartkey, sổ bảo hành HTC, sách hướng dẫn sử dụng", Status = AuditStatus.Good }
    ];

    private static async Task MigratePostgresAsync(AppDbContext db)
    {
        if (!db.Database.IsNpgsql()) return;
        var def = TenantContext.DefaultOrgId;
        var tables = new[] { "Customers", "Cars", "ROs", "Lines", "Parts", "WarrantyReports", "WarrantyReportItems", "Appointments", "StockIns", "StockInDetails", "StockOuts", "StockOutDetails", "CustomerCares", "Payments", "Quotes", "QuoteItems", "ServicePackages", "ServicePackageItems", "OrderParts", "OrderPartLines", "Cavities", "ReceptionSheets", "ReceptionItems", "GroupRepairs", "Engineers", "AssignmentWorks", "AssignmentEngineers", "InsuranceCompanies", "InsuranceContracts", "InsuranceClaims", "InsuranceClaimItems", "CampaignMarketings", "CampaignMarketingItems", "CustomerCareMaces", "StockAdjs", "StockAdjDetails", "Bulletins", "BulletinDetails", "BulletinVins", "PdiRequests", "PdiRequestItems", "PdiChecklistItems", "OrderComplains", "OrderComplainAttachFiles", "TechnicalLibraries", "ServiceItems", "Suppliers", "SupplierPayments", "SupplierPaymentDetails", "StockOutOrders", "StockOutOrderDetails" };
        var sql = new List<string>
        {
            "CREATE TABLE IF NOT EXISTS miniservice.\"Orgs\" (\"Id\" uuid PRIMARY KEY, \"Name\" text NOT NULL DEFAULT '', \"ApiKey\" text NOT NULL DEFAULT '', \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Orgs_ApiKey\" ON miniservice.\"Orgs\" (\"ApiKey\")",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"AppointmentId\" integer NULL",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"CavityId\" integer NULL",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"ReceptionSheetId\" integer NULL",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"CampaignMarketingId\" integer NULL",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"CampaignDiscountAmount\" numeric(18,2) NOT NULL DEFAULT 0",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"BulletinId\" integer NULL",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"PdiRequestId\" integer NULL",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"PdiReqNo\" text NULL",
            "ALTER TABLE miniservice.\"Appointments\" ADD COLUMN IF NOT EXISTS \"ReceptionSheetId\" integer NULL",
            "ALTER TABLE miniservice.\"Appointments\" ADD COLUMN IF NOT EXISTS \"CustomerCareMaceId\" integer NULL",
            "ALTER TABLE miniservice.\"StockOuts\" ADD COLUMN IF NOT EXISTS \"QuoteId\" integer NULL",
            "ALTER TABLE miniservice.\"StockOuts\" ADD COLUMN IF NOT EXISTS \"StockOutOrderId\" integer NULL",
            "ALTER TABLE miniservice.\"StockIns\" ADD COLUMN IF NOT EXISTS \"OrderPartId\" integer NULL",
            "ALTER TABLE miniservice.\"StockIns\" ADD COLUMN IF NOT EXISTS \"OrderPartNo\" text NULL",
            "ALTER TABLE miniservice.\"Lines\" ADD COLUMN IF NOT EXISTS \"ServiceItemId\" integer NULL",
            "ALTER TABLE miniservice.\"Lines\" ADD COLUMN IF NOT EXISTS \"StdManHour\" numeric(5,2) NULL",
            "CREATE TABLE IF NOT EXISTS miniservice.\"TechnicalLibraries\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"TechnicalLibraryCode\" text NOT NULL, \"DealerCode\" text NOT NULL, \"DealerName\" text NOT NULL, \"PlateNo\" text NULL, \"Model\" text NOT NULL, \"Engine\" text NULL, \"Gear\" text NULL, \"Version\" text NULL, \"ReRepairType\" integer NOT NULL, \"ReRepairRemark\" text NOT NULL, \"ReRepairFeedback\" text NULL, \"ExclusionTest\" text NULL, \"ReRepairReason\" text NOT NULL, \"ReRepairSolution\" text NOT NULL, \"Type\" integer NOT NULL, \"IsActive\" boolean NOT NULL DEFAULT false, \"ROId\" integer NULL, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"ApprovedAt\" timestamp NULL, \"ApprovedBy\" text NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_TechnicalLibraries_OrgId_TechnicalLibraryCode\" ON miniservice.\"TechnicalLibraries\" (\"OrgId\", \"TechnicalLibraryCode\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"ServiceItems\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"Code\" text NOT NULL, \"Name\" text NOT NULL, \"ROType\" integer NOT NULL, \"StdManHour\" numeric(5,2) NOT NULL, \"Price\" numeric(18,2) NOT NULL, \"Cost\" numeric(18,2) NOT NULL, \"VatPercent\" numeric(5,2) NOT NULL, \"Model\" text NULL, \"FlagWarranty\" boolean NOT NULL DEFAULT false, \"Note\" text NULL, \"IsActive\" boolean NOT NULL DEFAULT true, \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_ServiceItems_OrgId_Code\" ON miniservice.\"ServiceItems\" (\"OrgId\", \"Code\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"Suppliers\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"Code\" text NOT NULL, \"Name\" text NOT NULL, \"Address\" text NULL, \"Phone\" text NULL, \"Email\" text NULL, \"ContactName\" text NULL, \"ContactPhone\" text NULL, \"TaxCode\" text NULL, \"IsActive\" boolean NOT NULL DEFAULT true, \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Suppliers_OrgId_Code\" ON miniservice.\"Suppliers\" (\"OrgId\", \"Code\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"SupplierPayments\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"SupplierPaymentNo\" text NOT NULL, \"SupplierId\" integer NULL, \"SupplierName\" text NOT NULL, \"Address\" text NULL, \"PaymentDate\" timestamp NOT NULL, \"PaymentType\" integer NOT NULL, \"Status\" integer NOT NULL, \"OrderPartId\" integer NULL, \"OrderPartNo\" text NULL, \"TSTRequestNo\" text NULL, \"Description\" text NULL, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"ApprovedBy\" text NULL, \"ApprovedAt\" timestamp NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_SupplierPayments_OrgId_SupplierPaymentNo\" ON miniservice.\"SupplierPayments\" (\"OrgId\", \"SupplierPaymentNo\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"SupplierPaymentDetails\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"SupplierPaymentId\" integer NOT NULL, \"PartId\" integer NOT NULL, \"StockInId\" integer NULL, \"StockInNo\" text NULL, \"QtyPay\" numeric(18,2) NOT NULL, \"Price\" numeric(18,2) NOT NULL, \"VatPercent\" numeric(5,2) NOT NULL, \"QtyInventory\" numeric(18,2) NOT NULL, \"LocationCode\" text NULL, \"Reason\" text NULL)",
            "CREATE TABLE IF NOT EXISTS miniservice.\"StockOutOrders\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"OrderNo\" text NOT NULL, \"OrderDate\" timestamp NOT NULL, \"RequestDeliveryTime\" timestamp NULL, \"Priority\" integer NOT NULL, \"Status\" integer NOT NULL, \"ROId\" integer NULL, \"CustomerId\" integer NULL, \"CarId\" integer NULL, \"CavityId\" integer NULL, \"StockOutId\" integer NULL, \"RequesterName\" text NULL, \"Description\" text NULL, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"ApprovedBy\" text NULL, \"ApprovedAt\" timestamp NULL, \"RejectReason\" text NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_StockOutOrders_OrgId_OrderNo\" ON miniservice.\"StockOutOrders\" (\"OrgId\", \"OrderNo\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"StockOutOrderDetails\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"StockOutOrderId\" integer NOT NULL, \"PartId\" integer NOT NULL, \"PartCode\" text NOT NULL, \"PartName\" text NOT NULL, \"Unit\" text NOT NULL, \"RequestQuantity\" numeric(18,2) NOT NULL, \"IssuedQuantity\" numeric(18,2) NOT NULL, \"UnitPrice\" numeric(18,2) NOT NULL, \"VatPercent\" numeric(5,2) NOT NULL, \"Note\" text NULL)",
            "CREATE INDEX IF NOT EXISTS \"IX_StockOutOrderDetails_OrgId_StockOutOrderId\" ON miniservice.\"StockOutOrderDetails\" (\"OrgId\", \"StockOutOrderId\")",
        };
        foreach (var t in tables) sql.Add($"ALTER TABLE miniservice.\"{t}\" ADD COLUMN IF NOT EXISTS \"OrgId\" uuid NOT NULL DEFAULT '{def}'");
        foreach (var s in sql) try { await db.Database.ExecuteSqlRawAsync(s); } catch { }
    }

    private static async Task MigrateSqliteAsync(AppDbContext db)
    {
        if (db.Database.IsNpgsql()) return;
        var sqls = new[]
        {
            @"CREATE TABLE IF NOT EXISTS ""WarrantyReports"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""ReportNo"" TEXT NOT NULL,
                ""ROId"" INTEGER NOT NULL,
                ""CarId"" INTEGER NOT NULL,
                ""CustomerId"" INTEGER NOT NULL,
                ""Odometer"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""IssueDescription"" TEXT NOT NULL,
                ""DiagnosticResult"" TEXT NOT NULL,
                ""ErrorCodeCD"" TEXT NOT NULL,
                ""ErrorCodePN"" TEXT NOT NULL,
                ""PartIDError"" INTEGER NULL,
                ""ClaimAmount"" TEXT NOT NULL,
                ""ApprovedAmount"" TEXT NULL,
                ""RejectionReason"" TEXT NULL,
                ""DecisionNote"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""SubmittedAt"" TEXT NULL,
                ""DecidedAt"" TEXT NULL,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""CarId"") REFERENCES ""Cars"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""CustomerId"") REFERENCES ""Customers"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""PartIDError"") REFERENCES ""Parts"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_WarrantyReports_OrgId_ReportNo"" ON ""WarrantyReports"" (""OrgId"", ""ReportNo"");",
            @"CREATE TABLE IF NOT EXISTS ""WarrantyReportItems"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""WarrantyReportId"" INTEGER NOT NULL,
                ""Type"" INTEGER NOT NULL,
                ""PartId"" INTEGER NULL,
                ""Code"" TEXT NOT NULL,
                ""Name"" TEXT NOT NULL,
                ""Quantity"" TEXT NOT NULL,
                ""UnitPrice"" TEXT NOT NULL,
                ""IsAccepted"" INTEGER NOT NULL,
                ""Note"" TEXT NULL,
                FOREIGN KEY (""WarrantyReportId"") REFERENCES ""WarrantyReports"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""PartId"") REFERENCES ""Parts"" (""Id"") ON DELETE SET NULL
            );",
            @"ALTER TABLE ""Lines"" ADD COLUMN ""ExpenseType"" INTEGER NOT NULL DEFAULT 0;",
            @"CREATE TABLE IF NOT EXISTS ""Appointments"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""AppNo"" TEXT NOT NULL,
                ""CarId"" INTEGER NOT NULL,
                ""CustomerId"" INTEGER NOT NULL,
                ""AppointmentDate"" TEXT NOT NULL,
                ""ServiceType"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""Advisor"" TEXT NULL,
                ""Cavity"" TEXT NULL,
                ""CustomerRequest"" TEXT NOT NULL,
                ""Note"" TEXT NULL,
                ""CancelReason"" TEXT NULL,
                ""Source"" TEXT NOT NULL,
                ""ROId"" INTEGER NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""ConfirmedAt"" TEXT NULL,
                ""CheckedInAt"" TEXT NULL,
                FOREIGN KEY (""CarId"") REFERENCES ""Cars"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""CustomerId"") REFERENCES ""Customers"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Appointments_OrgId_AppNo"" ON ""Appointments"" (""OrgId"", ""AppNo"");",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""AppointmentId"" INTEGER NULL;",
            @"CREATE TABLE IF NOT EXISTS ""StockIns"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""StockInNo"" TEXT NOT NULL,
                ""StockInDate"" TEXT NOT NULL,
                ""SupplierName"" TEXT NOT NULL,
                ""BillNo"" TEXT NULL,
                ""Type"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""Description"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""FinishedAt"" TEXT NULL,
                ""ApprovedBy"" TEXT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_StockIns_OrgId_StockInNo"" ON ""StockIns"" (""OrgId"", ""StockInNo"");",
            @"CREATE TABLE IF NOT EXISTS ""StockInDetails"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""StockInId"" INTEGER NOT NULL,
                ""PartId"" INTEGER NOT NULL,
                ""PartCode"" TEXT NOT NULL,
                ""PartName"" TEXT NOT NULL,
                ""Unit"" TEXT NOT NULL,
                ""Quantity"" TEXT NOT NULL,
                ""UnitPrice"" TEXT NOT NULL,
                ""VatPercent"" TEXT NOT NULL,
                ""Location"" TEXT NULL,
                ""Note"" TEXT NULL,
                FOREIGN KEY (""StockInId"") REFERENCES ""StockIns"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""PartId"" ) REFERENCES ""Parts"" (""Id"") ON DELETE RESTRICT
            );",
            @"CREATE TABLE IF NOT EXISTS ""StockOuts"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""StockOutNo"" TEXT NOT NULL,
                ""StockOutDate"" TEXT NOT NULL,
                ""Type"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""ROId"" INTEGER NULL,
                ""CustomerId"" INTEGER NULL,
                ""CarId"" INTEGER NULL,
                ""RecipientName"" TEXT NULL,
                ""Description"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""FinishedAt"" TEXT NULL,
                ""ApprovedBy"" TEXT NULL,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""CustomerId"") REFERENCES ""Customers"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""CarId"") REFERENCES ""Cars"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_StockOuts_OrgId_StockOutNo"" ON ""StockOuts"" (""OrgId"", ""StockOutNo"");",
            @"CREATE TABLE IF NOT EXISTS ""StockOutDetails"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""StockOutId"" INTEGER NOT NULL,
                ""PartId"" INTEGER NOT NULL,
                ""PartCode"" TEXT NOT NULL,
                ""PartName"" TEXT NOT NULL,
                ""Unit"" TEXT NOT NULL,
                ""Quantity"" TEXT NOT NULL,
                ""UnitPrice"" TEXT NOT NULL,
                ""VatPercent"" TEXT NOT NULL,
                ""Location"" TEXT NULL,
                ""Note"" TEXT NULL,
                FOREIGN KEY (""StockOutId"") REFERENCES ""StockOuts"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""PartId"") REFERENCES ""Parts"" (""Id"") ON DELETE RESTRICT
            );",
            @"CREATE TABLE IF NOT EXISTS ""CustomerCares"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""CareNo"" TEXT NOT NULL,
                ""ROId"" INTEGER NOT NULL,
                ""CarId"" INTEGER NOT NULL,
                ""CustomerId"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""HasCarProblem"" INTEGER NOT NULL,
                ""QualityRating"" INTEGER NULL,
                ""StaffRating"" INTEGER NULL,
                ""WillingToReturn"" INTEGER NULL,
                ""FacilityRating"" INTEGER NULL,
                ""CustomerFeedback"" TEXT NULL,
                ""InternalNote"" TEXT NULL,
                ""ContactedBy"" TEXT NULL,
                ""ContactedDate"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""CarId"") REFERENCES ""Cars"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""CustomerId"") REFERENCES ""Customers"" (""Id"") ON DELETE RESTRICT
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CustomerCares_OrgId_CareNo"" ON ""CustomerCares"" (""OrgId"", ""CareNo"");",
            @"CREATE TABLE IF NOT EXISTS ""Payments"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""PaymentNo"" TEXT NOT NULL,
                ""ROId"" INTEGER NOT NULL,
                ""CustomerId"" INTEGER NOT NULL,
                ""CarId"" INTEGER NOT NULL,
                ""PaymentDate"" TEXT NOT NULL,
                ""Method"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""PayPersonName"" TEXT NOT NULL,
                ""PayPersonPhone"" TEXT NULL,
                ""PayPersonIdCard"" TEXT NULL,
                ""RoTotalAmount"" TEXT NOT NULL,
                ""DiscountAmount"" TEXT NOT NULL,
                ""ThirdPartyAmount"" TEXT NOT NULL,
                ""PayableAmount"" TEXT NOT NULL,
                ""PaymentAmount"" TEXT NOT NULL,
                ""TransactionRef"" TEXT NULL,
                ""Note"" TEXT NULL,
                ""Cashier"" TEXT NOT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""CompletedAt"" TEXT NULL,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""CustomerId"") REFERENCES ""Customers"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""CarId"") REFERENCES ""Cars"" (""Id"") ON DELETE RESTRICT
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Payments_OrgId_PaymentNo"" ON ""Payments"" (""OrgId"", ""PaymentNo"");",
            @"CREATE TABLE IF NOT EXISTS ""Quotes"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""QuoteNo"" TEXT NOT NULL,
                ""QuoteDate"" TEXT NOT NULL,
                ""ValidUntil"" TEXT NULL,
                ""CustomerId"" INTEGER NULL,
                ""CustomerName"" TEXT NOT NULL,
                ""CustomerPhone"" TEXT NULL,
                ""CustomerAddress"" TEXT NULL,
                ""CarId"" INTEGER NULL,
                ""RecipientName"" TEXT NULL,
                ""PaymentMethod"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""Remark"" TEXT NULL,
                ""Note"" TEXT NULL,
                ""StockOutId"" INTEGER NULL,
                ""ROId"" INTEGER NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""ConfirmedAt"" TEXT NULL,
                FOREIGN KEY (""CustomerId"") REFERENCES ""Customers"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""CarId"") REFERENCES ""Cars"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""StockOutId"") REFERENCES ""StockOuts"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Quotes_OrgId_QuoteNo"" ON ""Quotes"" (""OrgId"", ""QuoteNo"");",
            @"CREATE TABLE IF NOT EXISTS ""QuoteItems"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""QuoteId"" INTEGER NOT NULL,
                ""PartId"" INTEGER NULL,
                ""PartCode"" TEXT NOT NULL,
                ""PartName"" TEXT NOT NULL,
                ""Unit"" TEXT NOT NULL,
                ""Quantity"" TEXT NOT NULL,
                ""UnitPrice"" TEXT NOT NULL,
                ""DiscountPercent"" TEXT NOT NULL,
                ""VatPercent"" TEXT NOT NULL,
                ""Note"" TEXT NULL,
                FOREIGN KEY (""QuoteId"") REFERENCES ""Quotes"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""PartId"") REFERENCES ""Parts"" (""Id"") ON DELETE SET NULL
            );",
            @"ALTER TABLE ""StockOuts"" ADD COLUMN ""QuoteId"" INTEGER NULL;",
            @"CREATE TABLE IF NOT EXISTS ""ServicePackages"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""PackageNo"" TEXT NOT NULL,
                ""Name"" TEXT NOT NULL,
                ""TakingTimeHours"" TEXT NOT NULL,
                ""Description"" TEXT NULL,
                ""IsPublic"" INTEGER NOT NULL,
                ""IsActive"" INTEGER NOT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ServicePackages_OrgId_PackageNo"" ON ""ServicePackages"" (""OrgId"", ""PackageNo"");",
            @"CREATE TABLE IF NOT EXISTS ""ServicePackageItems"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""ServicePackageId"" INTEGER NOT NULL,
                ""Type"" INTEGER NOT NULL,
                ""PartId"" INTEGER NULL,
                ""Code"" TEXT NOT NULL,
                ""Name"" TEXT NOT NULL,
                ""Unit"" TEXT NOT NULL,
                ""Quantity"" TEXT NOT NULL,
                ""UnitPrice"" TEXT NOT NULL,
                ""VatPercent"" TEXT NOT NULL,
                ""ExpenseType"" INTEGER NOT NULL,
                ""Note"" TEXT NULL,
                FOREIGN KEY (""ServicePackageId"") REFERENCES ""ServicePackages"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""PartId"") REFERENCES ""Parts"" (""Id"") ON DELETE SET NULL
            );",
            @"ALTER TABLE ""StockIns"" ADD COLUMN ""OrderPartId"" INTEGER NULL;",
            @"ALTER TABLE ""StockIns"" ADD COLUMN ""OrderPartNo"" TEXT NULL;",
            @"CREATE TABLE IF NOT EXISTS ""OrderParts"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""OrderPartNo"" TEXT NOT NULL,
                ""OrderDate"" TEXT NOT NULL,
                ""SupplierName"" TEXT NOT NULL,
                ""DeliveryForm"" INTEGER NOT NULL,
                ""DeliveryLocation"" TEXT NOT NULL,
                ""EstimatedDeliverDate"" TEXT NULL,
                ""VIN"" TEXT NULL,
                ""ROId"" INTEGER NULL,
                ""Status"" INTEGER NOT NULL,
                ""OrderSuppierNo"" TEXT NULL,
                ""RequestSuppierDate"" TEXT NULL,
                ""ResponseSuppierDate"" TEXT NULL,
                ""Remark"" TEXT NULL,
                ""StockInId"" INTEGER NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""ApprovedAt"" TEXT NULL,
                ""FinishedAt"" TEXT NULL,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""StockInId"") REFERENCES ""StockIns"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_OrderParts_OrgId_OrderPartNo"" ON ""OrderParts"" (""OrgId"", ""OrderPartNo"");",
            @"CREATE TABLE IF NOT EXISTS ""OrderPartLines"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""OrderPartId"" INTEGER NOT NULL,
                ""OrderPartNo"" TEXT NOT NULL,
                ""PartId"" INTEGER NOT NULL,
                ""PartCode"" TEXT NOT NULL,
                ""PartName"" TEXT NOT NULL,
                ""Unit"" TEXT NOT NULL,
                ""Quantity"" TEXT NOT NULL,
                ""UnitPrice"" TEXT NOT NULL,
                ""DiscountRate"" TEXT NOT NULL,
                ""VatPercent"" TEXT NOT NULL,
                ""ApprovedQuantity"" TEXT NOT NULL,
                ""ReceivedQuantity"" TEXT NOT NULL,
                ""StatusDtl"" INTEGER NOT NULL,
                ""Note"" TEXT NULL,
                FOREIGN KEY (""OrderPartId"") REFERENCES ""OrderParts"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""PartId"") REFERENCES ""Parts"" (""Id"") ON DELETE RESTRICT
            );",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""CavityId"" INTEGER NULL;",
            @"CREATE TABLE IF NOT EXISTS ""Cavities"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""CavityNo"" TEXT NOT NULL,
                ""CavityName"" TEXT NOT NULL,
                ""CavityType"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""LiftEquipment"" TEXT NULL,
                ""AreaZone"" TEXT NULL,
                ""CurrentROId"" INTEGER NULL,
                ""CurrentCarPlate"" TEXT NULL,
                ""CurrentCarModel"" TEXT NULL,
                ""CurrentTechnician"" TEXT NULL,
                ""StartUseDate"" TEXT NULL,
                ""ExpectedFinishDate"" TEXT NULL,
                ""FinishUseDate"" TEXT NULL,
                ""Note"" TEXT NULL,
                ""IsActive"" INTEGER NOT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                FOREIGN KEY (""CurrentROId"") REFERENCES ""ROs"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Cavities_OrgId_CavityNo"" ON ""Cavities"" (""OrgId"", ""CavityNo"");",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""ReceptionSheetId"" INTEGER NULL;",
            @"ALTER TABLE ""Appointments"" ADD COLUMN ""ReceptionSheetId"" INTEGER NULL;",
            @"CREATE TABLE IF NOT EXISTS ""ReceptionSheets"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""ReceptionNo"" TEXT NOT NULL,
                ""CarId"" INTEGER NOT NULL,
                ""CustomerId"" INTEGER NOT NULL,
                ""AppointmentId"" INTEGER NULL,
                ""ROId"" INTEGER NULL,
                ""Odometer"" INTEGER NOT NULL,
                ""FuelLevel"" INTEGER NOT NULL,
                ""LevelOfInspection"" TEXT NOT NULL,
                ""CustomerRequest"" TEXT NOT NULL,
                ""ValuablesInCar"" TEXT NULL,
                ""ExteriorCondition"" TEXT NULL,
                ""IsWarranty"" INTEGER NOT NULL,
                ""IsInsurance"" INTEGER NOT NULL,
                ""IsBackRepair"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""DeliveryDateTime"" TEXT NULL,
                ""DeliveryBy"" TEXT NULL,
                ""DeliveryNote"" TEXT NULL,
                FOREIGN KEY (""CarId"") REFERENCES ""Cars"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""CustomerId"") REFERENCES ""Customers"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""AppointmentId"") REFERENCES ""Appointments"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ReceptionSheets_OrgId_ReceptionNo"" ON ""ReceptionSheets"" (""OrgId"", ""ReceptionNo"");",
            @"CREATE TABLE IF NOT EXISTS ""ReceptionItems"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""ReceptionSheetId"" INTEGER NOT NULL,
                ""Group"" TEXT NOT NULL,
                ""Code"" TEXT NOT NULL,
                ""Name"" TEXT NOT NULL,
                ""ReceptionStatus"" INTEGER NOT NULL,
                ""DeliveryStatus"" INTEGER NOT NULL,
                ""Note"" TEXT NULL,
                FOREIGN KEY (""ReceptionSheetId"") REFERENCES ""ReceptionSheets"" (""Id"") ON DELETE CASCADE
            );",
            @"CREATE TABLE IF NOT EXISTS ""GroupRepairs"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""GroupRNo"" TEXT NOT NULL,
                ""GroupRName"" TEXT NOT NULL,
                ""LeaderName"" TEXT NOT NULL,
                ""Note"" TEXT NULL,
                ""IsActive"" INTEGER NOT NULL,
                ""CreatedAt"" TEXT NOT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_GroupRepairs_OrgId_GroupRNo"" ON ""GroupRepairs"" (""OrgId"", ""GroupRNo"");",
            @"CREATE TABLE IF NOT EXISTS ""Engineers"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""EngineerNo"" TEXT NOT NULL,
                ""EngineerName"" TEXT NOT NULL,
                ""Phone"" TEXT NULL,
                ""SkillLevel"" TEXT NOT NULL,
                ""Specialty"" TEXT NOT NULL,
                ""GroupRId"" INTEGER NULL,
                ""IsActive"" INTEGER NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                FOREIGN KEY (""GroupRId"") REFERENCES ""GroupRepairs"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Engineers_OrgId_EngineerNo"" ON ""Engineers"" (""OrgId"", ""EngineerNo"");",
            @"CREATE TABLE IF NOT EXISTS ""AssignmentWorks"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""AssignmentNo"" TEXT NOT NULL,
                ""ROId"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""SCCPlanStartDTime"" TEXT NULL,
                ""SCCPlanFinishDTime"" TEXT NULL,
                ""SCCActualStartDTime"" TEXT NULL,
                ""SCCActualFinishDTime"" TEXT NULL,
                ""SCCCavityId"" INTEGER NULL,
                ""SCDPlanStartDTime"" TEXT NULL,
                ""SCDPlanFinishDTime"" TEXT NULL,
                ""SCDActualStartDTime"" TEXT NULL,
                ""SCDActualFinishDTime"" TEXT NULL,
                ""SCDCavityId"" INTEGER NULL,
                ""SCSPlanStartDTime"" TEXT NULL,
                ""SCSPlanFinishDTime"" TEXT NULL,
                ""SCSActualStartDTime"" TEXT NULL,
                ""SCSActualFinishDTime"" TEXT NULL,
                ""SCSCavityId"" INTEGER NULL,
                ""Note"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""StartedAt"" TEXT NULL,
                ""FinishedAt"" TEXT NULL,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""SCCCavityId"") REFERENCES ""Cavities"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""SCDCavityId"") REFERENCES ""Cavities"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""SCSCavityId"") REFERENCES ""Cavities"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_AssignmentWorks_OrgId_AssignmentNo"" ON ""AssignmentWorks"" (""OrgId"", ""AssignmentNo"");",
            @"CREATE TABLE IF NOT EXISTS ""AssignmentEngineers"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""AssignmentWorkId"" INTEGER NOT NULL,
                ""EngineerId"" INTEGER NOT NULL,
                ""WorkType"" INTEGER NOT NULL,
                ""IsPrimary"" INTEGER NOT NULL,
                ""AssignedHours"" TEXT NOT NULL,
                ""Note"" TEXT NULL,
                FOREIGN KEY (""AssignmentWorkId"") REFERENCES ""AssignmentWorks"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""EngineerId"") REFERENCES ""Engineers"" (""Id"") ON DELETE RESTRICT
            );",
            @"CREATE TABLE IF NOT EXISTS ""InsuranceCompanies"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""InsNo"" TEXT NOT NULL,
                ""InsName"" TEXT NOT NULL,
                ""Address"" TEXT NULL,
                ""Phone"" TEXT NULL,
                ""Email"" TEXT NULL,
                ""TaxCode"" TEXT NULL,
                ""Hotline"" TEXT NULL,
                ""IsActive"" INTEGER NOT NULL,
                ""CreatedAt"" TEXT NOT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_InsuranceCompanies_OrgId_InsNo"" ON ""InsuranceCompanies"" (""OrgId"", ""InsNo"");",
            @"CREATE TABLE IF NOT EXISTS ""InsuranceContracts"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""ContractNo"" TEXT NOT NULL,
                ""ContractCode"" TEXT NOT NULL,
                ""InsuranceCompanyId"" INTEGER NOT NULL,
                ""StartDate"" TEXT NOT NULL,
                ""FinishDate"" TEXT NOT NULL,
                ""PaymentType"" INTEGER NOT NULL,
                ""PaymentLimit"" TEXT NOT NULL,
                ""DiscountLaborRate"" TEXT NOT NULL,
                ""DiscountPartRate"" TEXT NOT NULL,
                ""Note"" TEXT NULL,
                ""IsActive"" INTEGER NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                FOREIGN KEY (""InsuranceCompanyId"") REFERENCES ""InsuranceCompanies"" (""Id"") ON DELETE CASCADE
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_InsuranceContracts_OrgId_ContractNo"" ON ""InsuranceContracts"" (""OrgId"", ""ContractNo"");",
            @"CREATE TABLE IF NOT EXISTS ""InsuranceClaims"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""ClaimNo"" TEXT NOT NULL,
                ""ROId"" INTEGER NOT NULL,
                ""InsuranceCompanyId"" INTEGER NOT NULL,
                ""InsuranceContractId"" INTEGER NULL,
                ""PolicyNo"" TEXT NOT NULL,
                ""ClaimFileNo"" TEXT NULL,
                ""SurveyorName"" TEXT NULL,
                ""SurveyorPhone"" TEXT NULL,
                ""AccidentDate"" TEXT NOT NULL,
                ""AccidentLocation"" TEXT NULL,
                ""AccidentDescription"" TEXT NOT NULL,
                ""EstimatedAmount"" TEXT NOT NULL,
                ""ApprovedAmount"" TEXT NOT NULL,
                ""DeductibleAmount"" TEXT NOT NULL,
                ""PenaltyAmount"" TEXT NOT NULL,
                ""InsuranceAmount"" TEXT NOT NULL,
                ""CustomerAmount"" TEXT NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""DecisionNote"" TEXT NULL,
                ""RejectionReason"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""SubmittedAt"" TEXT NULL,
                ""ApprovedAt"" TEXT NULL,
                ""SettledAt"" TEXT NULL,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""InsuranceCompanyId"") REFERENCES ""InsuranceCompanies"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""InsuranceContractId"") REFERENCES ""InsuranceContracts"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_InsuranceClaims_OrgId_ClaimNo"" ON ""InsuranceClaims"" (""OrgId"", ""ClaimNo"");",
            @"CREATE TABLE IF NOT EXISTS ""InsuranceClaimItems"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""InsuranceClaimId"" INTEGER NOT NULL,
                ""Type"" INTEGER NOT NULL,
                ""Code"" TEXT NOT NULL,
                ""Name"" TEXT NOT NULL,
                ""Quantity"" TEXT NOT NULL,
                ""UnitPrice"" TEXT NOT NULL,
                ""EstimatedAmount"" TEXT NOT NULL,
                ""ApprovedAmount"" TEXT NOT NULL,
                ""IsApproved"" INTEGER NOT NULL,
                ""Note"" TEXT NULL,
                FOREIGN KEY (""InsuranceClaimId"") REFERENCES ""InsuranceClaims"" (""Id"") ON DELETE CASCADE
            );",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""CampaignMarketingId"" INTEGER NULL;",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""CampaignDiscountAmount"" TEXT NULL;",
            @"UPDATE ""ROs"" SET ""CampaignDiscountAmount"" = '0' WHERE ""CampaignDiscountAmount"" IS NULL;",
            @"CREATE TABLE IF NOT EXISTS ""CampaignMarketings"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""CamMarketingNo"" TEXT NOT NULL,
                ""CamMarketingName"" TEXT NOT NULL,
                ""CamMarketingDesc"" TEXT NULL,
                ""EffDateStart"" TEXT NOT NULL,
                ""EffDateEnd"" TEXT NOT NULL,
                ""WarrantyDateStart"" TEXT NULL,
                ""WarrantyDateEnd"" TEXT NULL,
                ""ConditionModel"" TEXT NULL,
                ""ConditionPlateNo"" TEXT NULL,
                ""ConditionVIN"" TEXT NULL,
                ""DiscountLaborPercent"" TEXT NOT NULL,
                ""DiscountPartPercent"" TEXT NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""ApprovedAt"" TEXT NULL,
                ""ApprovedBy"" TEXT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CampaignMarketings_OrgId_CamMarketingNo"" ON ""CampaignMarketings"" (""OrgId"", ""CamMarketingNo"");",
            @"CREATE TABLE IF NOT EXISTS ""CampaignMarketingItems"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""CampaignMarketingId"" INTEGER NOT NULL,
                ""PartId"" INTEGER NOT NULL,
                ""PartCode"" TEXT NOT NULL,
                ""PartName"" TEXT NOT NULL,
                ""PercentDiscount"" TEXT NOT NULL,
                ""MaxQuantity"" TEXT NOT NULL,
                ""Note"" TEXT NULL,
                FOREIGN KEY (""CampaignMarketingId"") REFERENCES ""CampaignMarketings"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""PartId"") REFERENCES ""Parts"" (""Id"") ON DELETE RESTRICT
            );",
            @"CREATE TABLE IF NOT EXISTS ""CustomerCareMaces"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""MaceNo"" TEXT NOT NULL,
                ""ROId"" INTEGER NULL,
                ""CarId"" INTEGER NOT NULL,
                ""CustomerId"" INTEGER NOT NULL,
                ""MaceType"" INTEGER NOT NULL,
                ""LastKm"" INTEGER NOT NULL,
                ""NextKm"" INTEGER NOT NULL,
                ""MaceRecomentDate"" TEXT NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""ContactDate"" TEXT NULL,
                ""ContactBy"" TEXT NULL,
                ""ApointDate"" TEXT NULL,
                ""Remark"" TEXT NULL,
                ""AppointmentId"" INTEGER NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""UpdatedAt"" TEXT NULL,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""CarId"") REFERENCES ""Cars"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""CustomerId"") REFERENCES ""Customers"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""AppointmentId"") REFERENCES ""Appointments"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CustomerCareMaces_OrgId_MaceNo"" ON ""CustomerCareMaces"" (""OrgId"", ""MaceNo"");",
            @"ALTER TABLE ""Appointments"" ADD COLUMN ""CustomerCareMaceId"" INTEGER NULL;",
            @"CREATE TABLE IF NOT EXISTS ""StockAdjs"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""StockAdjNo"" TEXT NOT NULL,
                ""StockAdjDate"" TEXT NOT NULL,
                ""Type"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""StorageCode"" TEXT NOT NULL,
                ""Remark"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""ApprovedBy"" TEXT NULL,
                ""FinishedAt"" TEXT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_StockAdjs_OrgId_StockAdjNo"" ON ""StockAdjs"" (""OrgId"", ""StockAdjNo"");",
            @"CREATE TABLE IF NOT EXISTS ""StockAdjDetails"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""StockAdjId"" INTEGER NOT NULL,
                ""PartId"" INTEGER NOT NULL,
                ""PartCode"" TEXT NOT NULL,
                ""PartName"" TEXT NOT NULL,
                ""Unit"" TEXT NOT NULL,
                ""CostPrice"" TEXT NOT NULL,
                ""SystemQuantity"" TEXT NOT NULL,
                ""ActualQuantity"" TEXT NOT NULL,
                ""FromLocation"" TEXT NULL,
                ""ToLocation"" TEXT NULL,
                ""Note"" TEXT NULL,
                FOREIGN KEY (""StockAdjId"") REFERENCES ""StockAdjs"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""PartId"") REFERENCES ""Parts"" (""Id"") ON DELETE RESTRICT
            );",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""BulletinId"" INTEGER NULL;",
            @"CREATE TABLE IF NOT EXISTS ""Bulletins"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""BulletinNo"" TEXT NOT NULL,
                ""BulletinNoHMC"" TEXT NULL,
                ""Title"" TEXT NOT NULL,
                ""Remark"" TEXT NULL,
                ""Solution"" TEXT NULL,
                ""CreateDate"" TEXT NOT NULL,
                ""DateExpired"" TEXT NULL,
                ""IsActive"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""UserCreate"" TEXT NOT NULL,
                ""FileNameAttachment"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Bulletins_OrgId_BulletinNo"" ON ""Bulletins"" (""OrgId"", ""BulletinNo"");",
            @"CREATE TABLE IF NOT EXISTS ""BulletinDetails"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""BulletinId"" INTEGER NOT NULL,
                ""Type"" INTEGER NOT NULL,
                ""PartId"" INTEGER NULL,
                ""Code"" TEXT NOT NULL,
                ""Name"" TEXT NOT NULL,
                ""Unit"" TEXT NOT NULL,
                ""Quantity"" TEXT NOT NULL,
                ""UnitPrice"" TEXT NOT NULL,
                ""Note"" TEXT NULL,
                FOREIGN KEY (""BulletinId"") REFERENCES ""Bulletins"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""PartId"") REFERENCES ""Parts"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE TABLE IF NOT EXISTS ""BulletinVins"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""BulletinId"" INTEGER NOT NULL,
                ""VinNo"" TEXT NOT NULL,
                ""PlateNo"" TEXT NULL,
                ""Model"" TEXT NULL,
                ""DealerCode"" TEXT NULL,
                ""Status"" INTEGER NOT NULL,
                ""DateDone"" TEXT NULL,
                ""DoneBy"" TEXT NULL,
                ""ROId"" INTEGER NULL,
                ""RONo"" TEXT NULL,
                ""Note"" TEXT NULL,
                FOREIGN KEY (""BulletinId"") REFERENCES ""Bulletins"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE INDEX IF NOT EXISTS ""IX_BulletinVins_OrgId_BulletinId_VinNo"" ON ""BulletinVins"" (""OrgId"", ""BulletinId"", ""VinNo"");",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""PdiRequestId"" INTEGER NULL;",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""PdiReqNo"" TEXT NULL;",
            @"CREATE TABLE IF NOT EXISTS ""PdiRequests"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""PdiReqNo"" TEXT NOT NULL,
                ""DealerCode"" TEXT NOT NULL,
                ""CreatedDate"" TEXT NOT NULL,
                ""ApprovedDate"" TEXT NULL,
                ""Remark"" TEXT NULL,
                ""Status"" INTEGER NOT NULL,
                ""FlagAccessory"" INTEGER NOT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""ApprovedBy"" TEXT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""FinishedAt"" TEXT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PdiRequests_OrgId_PdiReqNo"" ON ""PdiRequests"" (""OrgId"", ""PdiReqNo"");",
            @"CREATE TABLE IF NOT EXISTS ""PdiRequestItems"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""PdiRequestId"" INTEGER NOT NULL,
                ""VIN"" TEXT NOT NULL,
                ""Model"" TEXT NOT NULL,
                ""Spec"" TEXT NULL,
                ""Color"" TEXT NULL,
                ""EngineNo"" TEXT NULL,
                ""BatteryNo"" TEXT NULL,
                ""ExpectedDeliveryDate"" TEXT NOT NULL,
                ""ContractNo"" TEXT NOT NULL,
                ""CustomerName"" TEXT NOT NULL,
                ""CustomerPhone"" TEXT NULL,
                ""CustomerAddress"" TEXT NULL,
                ""FlagAccessory"" INTEGER NOT NULL,
                ""AccessoryNote"" TEXT NULL,
                ""Status"" INTEGER NOT NULL,
                ""Inspector"" TEXT NULL,
                ""InspectionDate"" TEXT NULL,
                ""PassedDate"" TEXT NULL,
                ""InspectionNotes"" TEXT NULL,
                ""ROId"" INTEGER NULL,
                ""RONo"" TEXT NULL,
                FOREIGN KEY (""PdiRequestId"") REFERENCES ""PdiRequests"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE INDEX IF NOT EXISTS ""IX_PdiRequestItems_OrgId_PdiRequestId_VIN"" ON ""PdiRequestItems"" (""OrgId"", ""PdiRequestId"", ""VIN"");",
            @"CREATE TABLE IF NOT EXISTS ""PdiChecklistItems"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""PdiRequestItemId"" INTEGER NOT NULL,
                ""Group"" TEXT NOT NULL,
                ""Code"" TEXT NOT NULL,
                ""Name"" TEXT NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""Note"" TEXT NULL,
                FOREIGN KEY (""PdiRequestItemId"") REFERENCES ""PdiRequestItems"" (""Id"") ON DELETE CASCADE
            );",
            @"CREATE TABLE IF NOT EXISTS ""OrderComplains"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""OrderComplainNo"" TEXT NOT NULL,
                ""DealerCode"" TEXT NOT NULL,
                ""DealerName"" TEXT NOT NULL,
                ""ComplainType"" INTEGER NOT NULL,
                ""OrderPartId"" INTEGER NULL,
                ""OrderPartNo"" TEXT NULL,
                ""PartId"" INTEGER NOT NULL,
                ""PartCode"" TEXT NOT NULL,
                ""PartName"" TEXT NOT NULL,
                ""Unit"" TEXT NOT NULL,
                ""Quantity"" TEXT NOT NULL,
                ""UnitPrice"" TEXT NOT NULL,
                ""VIN"" TEXT NULL,
                ""Description"" TEXT NOT NULL,
                ""RequestOrderNo"" TEXT NULL,
                ""TransportUnit"" TEXT NULL,
                ""DeliveryDateTime"" TEXT NULL,
                ""DeliveryBy"" TEXT NULL,
                ""DeliveryLocation"" TEXT NULL,
                ""ReceiveBy"" TEXT NULL,
                ""AssembleDateTime"" TEXT NULL,
                ""AssembleBy"" TEXT NULL,
                ""DMSStatus"" INTEGER NOT NULL,
                ""TSTStatus"" INTEGER NOT NULL,
                ""TSTSolution"" INTEGER NULL,
                ""SolutionNote"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""SentAt"" TEXT NULL,
                ""DecidedAt"" TEXT NULL,
                ""FinishedAt"" TEXT NULL,
                FOREIGN KEY (""OrderPartId"") REFERENCES ""OrderParts"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""PartId"") REFERENCES ""Parts"" (""Id"") ON DELETE RESTRICT
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_OrderComplains_OrgId_OrderComplainNo"" ON ""OrderComplains"" (""OrgId"", ""OrderComplainNo"");",
            @"CREATE TABLE IF NOT EXISTS ""OrderComplainAttachFiles"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""OrderComplainId"" INTEGER NOT NULL,
                ""ImageType"" TEXT NOT NULL,
                ""FileName"" TEXT NOT NULL,
                ""FilePath"" TEXT NOT NULL,
                ""Note"" TEXT NULL,
                ""UploadedAt"" TEXT NOT NULL,
                FOREIGN KEY (""OrderComplainId"") REFERENCES ""OrderComplains"" (""Id"") ON DELETE CASCADE
            );",
            @"CREATE TABLE IF NOT EXISTS ""TechnicalLibraries"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""TechnicalLibraryCode"" TEXT NOT NULL,
                ""DealerCode"" TEXT NOT NULL,
                ""DealerName"" TEXT NOT NULL,
                ""PlateNo"" TEXT NULL,
                ""Model"" TEXT NOT NULL,
                ""Engine"" TEXT NULL,
                ""Gear"" TEXT NULL,
                ""Version"" TEXT NULL,
                ""ReRepairType"" INTEGER NOT NULL,
                ""ReRepairRemark"" TEXT NOT NULL,
                ""ReRepairFeedback"" TEXT NULL,
                ""ExclusionTest"" TEXT NULL,
                ""ReRepairReason"" TEXT NOT NULL,
                ""ReRepairSolution"" TEXT NOT NULL,
                ""Type"" INTEGER NOT NULL,
                ""IsActive"" INTEGER NOT NULL,
                ""ROId"" INTEGER NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""ApprovedAt"" TEXT NULL,
                ""ApprovedBy"" TEXT NULL,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_TechnicalLibraries_OrgId_TechnicalLibraryCode"" ON ""TechnicalLibraries"" (""OrgId"", ""TechnicalLibraryCode"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_TechnicalLibraries_OrgId_Model"" ON ""TechnicalLibraries"" (""OrgId"", ""Model"");",
            @"ALTER TABLE ""Lines"" ADD COLUMN ""ServiceItemId"" INTEGER NULL;",
            @"ALTER TABLE ""Lines"" ADD COLUMN ""StdManHour"" TEXT NULL;",
            @"CREATE TABLE IF NOT EXISTS ""ServiceItems"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""Code"" TEXT NOT NULL,
                ""Name"" TEXT NOT NULL,
                ""ROType"" INTEGER NOT NULL,
                ""StdManHour"" TEXT NOT NULL,
                ""Price"" TEXT NOT NULL,
                ""Cost"" TEXT NOT NULL,
                ""VatPercent"" TEXT NOT NULL,
                ""Model"" TEXT NULL,
                ""FlagWarranty"" INTEGER NOT NULL,
                ""Note"" TEXT NULL,
                ""IsActive"" INTEGER NOT NULL,
                ""CreatedAt"" TEXT NOT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ServiceItems_OrgId_Code"" ON ""ServiceItems"" (""OrgId"", ""Code"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_ServiceItems_OrgId_ROType"" ON ""ServiceItems"" (""OrgId"", ""ROType"");",
            @"CREATE TABLE IF NOT EXISTS ""Suppliers"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""Code"" TEXT NOT NULL,
                ""Name"" TEXT NOT NULL,
                ""Address"" TEXT NULL,
                ""Phone"" TEXT NULL,
                ""Email"" TEXT NULL,
                ""ContactName"" TEXT NULL,
                ""ContactPhone"" TEXT NULL,
                ""TaxCode"" TEXT NULL,
                ""IsActive"" INTEGER NOT NULL,
                ""CreatedAt"" TEXT NOT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Suppliers_OrgId_Code"" ON ""Suppliers"" (""OrgId"", ""Code"");",
            @"CREATE TABLE IF NOT EXISTS ""SupplierPayments"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""SupplierPaymentNo"" TEXT NOT NULL,
                ""SupplierId"" INTEGER NULL,
                ""SupplierName"" TEXT NOT NULL,
                ""Address"" TEXT NULL,
                ""PaymentDate"" TEXT NOT NULL,
                ""PaymentType"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""OrderPartId"" INTEGER NULL,
                ""OrderPartNo"" TEXT NULL,
                ""TSTRequestNo"" TEXT NULL,
                ""Description"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""ApprovedBy"" TEXT NULL,
                ""ApprovedAt"" TEXT NULL,
                FOREIGN KEY (""SupplierId"") REFERENCES ""Suppliers"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""OrderPartId"") REFERENCES ""OrderParts"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_SupplierPayments_OrgId_SupplierPaymentNo"" ON ""SupplierPayments"" (""OrgId"", ""SupplierPaymentNo"");",
            @"CREATE TABLE IF NOT EXISTS ""SupplierPaymentDetails"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""SupplierPaymentId"" INTEGER NOT NULL,
                ""PartId"" INTEGER NOT NULL,
                ""StockInId"" INTEGER NULL,
                ""StockInNo"" TEXT NULL,
                ""QtyPay"" TEXT NOT NULL,
                ""Price"" TEXT NOT NULL,
                ""VatPercent"" TEXT NOT NULL,
                ""QtyInventory"" TEXT NOT NULL,
                ""LocationCode"" TEXT NULL,
                ""Reason"" TEXT NULL,
                FOREIGN KEY (""SupplierPaymentId"") REFERENCES ""SupplierPayments"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""PartId"") REFERENCES ""Parts"" (""Id"") ON DELETE RESTRICT
            );",
            @"CREATE INDEX IF NOT EXISTS ""IX_SupplierPaymentDetails_OrgId_SupplierPaymentId"" ON ""SupplierPaymentDetails"" (""OrgId"", ""SupplierPaymentId"");",
            @"ALTER TABLE ""StockOuts"" ADD COLUMN ""StockOutOrderId"" INTEGER NULL;",
            @"CREATE TABLE IF NOT EXISTS ""StockOutOrders"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""OrderNo"" TEXT NOT NULL,
                ""OrderDate"" TEXT NOT NULL,
                ""RequestDeliveryTime"" TEXT NULL,
                ""Priority"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""ROId"" INTEGER NULL,
                ""CustomerId"" INTEGER NULL,
                ""CarId"" INTEGER NULL,
                ""CavityId"" INTEGER NULL,
                ""StockOutId"" INTEGER NULL,
                ""RequesterName"" TEXT NULL,
                ""Description"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""ApprovedBy"" TEXT NULL,
                ""ApprovedAt"" TEXT NULL,
                ""RejectReason"" TEXT NULL,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""CustomerId"") REFERENCES ""Customers"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""CarId"") REFERENCES ""Cars"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""CavityId"") REFERENCES ""Cavities"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""StockOutId"") REFERENCES ""StockOuts"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_StockOutOrders_OrgId_OrderNo"" ON ""StockOutOrders"" (""OrgId"", ""OrderNo"");",
            @"CREATE TABLE IF NOT EXISTS ""StockOutOrderDetails"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""StockOutOrderId"" INTEGER NOT NULL,
                ""PartId"" INTEGER NOT NULL,
                ""PartCode"" TEXT NOT NULL,
                ""PartName"" TEXT NOT NULL,
                ""Unit"" TEXT NOT NULL,
                ""RequestQuantity"" TEXT NOT NULL,
                ""IssuedQuantity"" TEXT NOT NULL,
                ""UnitPrice"" TEXT NOT NULL,
                ""VatPercent"" TEXT NOT NULL,
                ""Note"" TEXT NULL,
                FOREIGN KEY (""StockOutOrderId"") REFERENCES ""StockOutOrders"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""PartId"") REFERENCES ""Parts"" (""Id"") ON DELETE RESTRICT
            );",
            @"CREATE INDEX IF NOT EXISTS ""IX_StockOutOrderDetails_OrgId_StockOutOrderId"" ON ""StockOutOrderDetails"" (""OrgId"", ""StockOutOrderId"");"
        };

        foreach (var sql in sqls)
        {
            try { await db.Database.ExecuteSqlRawAsync(sql); } catch { }
        }
    }
}
