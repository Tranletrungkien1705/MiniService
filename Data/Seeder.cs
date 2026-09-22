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
    }

    private static async Task MigratePostgresAsync(AppDbContext db)
    {
        if (!db.Database.IsNpgsql()) return;
        var def = TenantContext.DefaultOrgId;
        var tables = new[] { "Customers", "Cars", "ROs", "Lines", "Parts", "WarrantyReports", "WarrantyReportItems", "Appointments", "StockIns", "StockInDetails", "StockOuts", "StockOutDetails", "CustomerCares", "Payments", "Quotes", "QuoteItems", "ServicePackages", "ServicePackageItems", "OrderParts", "OrderPartLines" };
        var sql = new List<string>
        {
            "CREATE TABLE IF NOT EXISTS miniservice.\"Orgs\" (\"Id\" uuid PRIMARY KEY, \"Name\" text NOT NULL DEFAULT '', \"ApiKey\" text NOT NULL DEFAULT '', \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Orgs_ApiKey\" ON miniservice.\"Orgs\" (\"ApiKey\")",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"AppointmentId\" integer NULL",
            "ALTER TABLE miniservice.\"StockOuts\" ADD COLUMN IF NOT EXISTS \"QuoteId\" integer NULL",
            "ALTER TABLE miniservice.\"StockIns\" ADD COLUMN IF NOT EXISTS \"OrderPartId\" integer NULL",
            "ALTER TABLE miniservice.\"StockIns\" ADD COLUMN IF NOT EXISTS \"OrderPartNo\" text NULL",
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
            );"
        };

        foreach (var sql in sqls)
        {
            try { await db.Database.ExecuteSqlRawAsync(sql); } catch { }
        }
    }
}
