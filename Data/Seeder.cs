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

        if (!await db.CarModels.AnyAsync())
        {
            var models = new List<CarModel>
            {
                new() { ModelCode = "ACC", ModelName = "Hyundai Accent", TradeMarkCode = "HMC", ProductionCode = "RB", Segment = CarModelSegment.Sedan, ProductYear = 2023, IsActive = true, CreatedBy = "Hệ thống HTC" },
                new() { ModelCode = "ELA", ModelName = "Hyundai Elantra", TradeMarkCode = "HMC", ProductionCode = "CN7", Segment = CarModelSegment.Sedan, ProductYear = 2023, IsActive = true, CreatedBy = "Hệ thống HTC" },
                new() { ModelCode = "GRI", ModelName = "Hyundai Grand i10", TradeMarkCode = "HMC", ProductionCode = "AI3", Segment = CarModelSegment.Sedan, ProductYear = 2023, IsActive = true, CreatedBy = "Hệ thống HTC" },
                new() { ModelCode = "TUC", ModelName = "Hyundai Tucson", TradeMarkCode = "HMC", ProductionCode = "NX4", Segment = CarModelSegment.SUV, ProductYear = 2023, IsActive = true, CreatedBy = "Hệ thống HTC" },
                new() { ModelCode = "SAN", ModelName = "Hyundai Santa Fe", TradeMarkCode = "HMC", ProductionCode = "MX5", Segment = CarModelSegment.SUV, ProductYear = 2023, IsActive = true, CreatedBy = "Hệ thống HTC" },
                new() { ModelCode = "CRE", ModelName = "Hyundai Creta", TradeMarkCode = "HMC", ProductionCode = "SU2", Segment = CarModelSegment.SUV, ProductYear = 2023, IsActive = true, CreatedBy = "Hệ thống HTC" },
                new() { ModelCode = "CUS", ModelName = "Hyundai Custin", TradeMarkCode = "HMC", ProductionCode = "KA4", Segment = CarModelSegment.MPV, ProductYear = 2023, IsActive = true, CreatedBy = "Hệ thống HTC" },
                new() { ModelCode = "STA", ModelName = "Hyundai Stargazer", TradeMarkCode = "HMC", ProductionCode = "KS", Segment = CarModelSegment.MPV, ProductYear = 2023, IsActive = true, CreatedBy = "Hệ thống HTC" },
                new() { ModelCode = "POR", ModelName = "Hyundai Porter II", TradeMarkCode = "HTC", ProductionCode = "HR", Segment = CarModelSegment.Commercial, ProductYear = 2023, IsActive = true, CreatedBy = "Hệ thống HTC" },
                new() { ModelCode = "ION", ModelName = "Hyundai Ioniq 5", TradeMarkCode = "HMC", ProductionCode = "NE", Segment = CarModelSegment.EV, ProductYear = 2023, IsActive = true, CreatedBy = "Hệ thống HTC" }
            };
            db.CarModels.AddRange(models);
            await db.SaveChangesAsync();
        }

        // Seed Định mức vật tư tối thiểu (Mst_BOM / Mst_BOMDtl)
        if (!await db.Boms.AnyAsync())
        {
            var bom1 = new Bom
            {
                BomCode = "BOM-BD-1000",
                BomDesc = "Định mức phụ tùng bảo dưỡng cấp 1.000 km",
                Remark = "Bộ phụ tùng tối thiểu cho bảo dưỡng định kỳ 1.000 km (kiểm tra & thay dầu).",
                IsActive = true,
                CreatedBy = "Hệ thống HTC",
                Lines =
                [
                    new() { PartCode = "26300-35505", PartName = "Lọc dầu động cơ", Unit = "Cái", QtyMin = 1 },
                    new() { PartCode = "26320-2G000", PartName = "Lọc nhớt hộp số", Unit = "Cái", QtyMin = 1 },
                    new() { PartCode = "97133-2E210", PartName = "Lọc gió điều hòa", Unit = "Cái", QtyMin = 1 }
                ]
            };
            var bom2 = new Bom
            {
                BomCode = "BOM-BD-10000",
                BomDesc = "Định mức phụ tùng bảo dưỡng cấp 10.000 km",
                Remark = "Bộ phụ tùng tối thiểu cho bảo dưỡng định kỳ 10.000 km (thay dầu, lọc gió, bugi).",
                IsActive = true,
                CreatedBy = "Hệ thống HTC",
                Lines =
                [
                    new() { PartCode = "26300-35505", PartName = "Lọc dầu động cơ", Unit = "Cái", QtyMin = 1 },
                    new() { PartCode = "28113-1R100", PartName = "Lọc gió động cơ", Unit = "Cái", QtyMin = 1 },
                    new() { PartCode = "97133-2E210", PartName = "Lọc gió điều hòa", Unit = "Cái", QtyMin = 1 },
                    new() { PartCode = "18849-11051", PartName = "Bugi đánh lửa", Unit = "Cái", QtyMin = 4 }
                ]
            };
            var bom3 = new Bom
            {
                BomCode = "BOM-BD-40000",
                BomDesc = "Định mức phụ tùng bảo dưỡng cấp 40.000 km",
                Remark = "Bộ phụ tùng tối thiểu cho bảo dưỡng định kỳ 40.000 km (dầu hộp số, dây curoa, lọc nhiên liệu).",
                IsActive = true,
                CreatedBy = "Hệ thống HTC",
                Lines =
                [
                    new() { PartCode = "26300-35505", PartName = "Lọc dầu động cơ", Unit = "Cái", QtyMin = 1 },
                    new() { PartCode = "31922-2H900", PartName = "Lọc nhiên liệu", Unit = "Cái", QtyMin = 1 },
                    new() { PartCode = "25212-2B000", PartName = "Dây curoa tổng", Unit = "Sợi", QtyMin = 1 },
                    new() { PartCode = "18849-11051", PartName = "Bugi đánh lửa", Unit = "Cái", QtyMin = 4 }
                ]
            };
            db.Boms.AddRange(bom1, bom2, bom3);
            await db.SaveChangesAsync();
        }

        if (!await db.Suppliers.AnyAsync())
        {
            var s1 = new Supplier { Code = "HTC", Name = "Công ty Cổ phần Liên doanh Ô tô Hyundai Thành Công Việt Nam", Address = "Tòa nhà Epic Tower, Nam Từ Liêm, Hà Nội", Phone = "024.3826.2614", Email = "parts@hyundai-thanhcong.vn", ContactName = "Nguyễn Hoàng Minh", ContactPhone = "0912.345.678", TaxCode = "0102872391", BankAccount = "0011004568899", BankName = "Vietcombank - CN Sở Giao Dịch Hà Nội", IsActive = true };
            var s2 = new Supplier { Code = "MOBIS", Name = "Công ty TNHH Phụ tùng Hyundai Mobis Việt Nam", Address = "KCN Đình Trám, Việt Yên, Bắc Giang", Phone = "0204.387.9999", Email = "order@mobis.co.kr", ContactName = "Kim Jung Wook", ContactPhone = "0988.777.666", TaxCode = "2400589123", BankAccount = "118002678999", BankName = "VietinBank - CN Bắc Giang", IsActive = true };
            var s3 = new Supplier { Code = "CASTROL", Name = "Công ty TNHH Castrol BP Petco Việt Nam", Address = "Quận 1, TP. Hồ Chí Minh", Phone = "028.3821.9153", Email = "dauthuongmai@castrol.com", ContactName = "Lê Quang Vinh", ContactPhone = "0903.888.999", TaxCode = "0300628284", BankAccount = "0071008989899", BankName = "Vietcombank - CN TP.HCM", IsActive = true };
            var s4 = new Supplier { Code = "DENSO", Name = "Công ty TNHH Denso Việt Nam", Address = "KCN Thăng Long, Đông Anh, Hà Nội", Phone = "024.3881.1601", Email = "sales@denso.com.vn", ContactName = "Phạm Tuấn Anh", ContactPhone = "0915.222.333", TaxCode = "0101183569", BankAccount = "19033456789012", BankName = "Techcombank - CN Thăng Long", IsActive = true };
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

        if (!await db.PartOOs.AnyAsync())
        {
            var pBrake = await db.Parts.FirstOrDefaultAsync(p => p.Code == "58101-C1A00");
            var pFilter = await db.Parts.FirstOrDefaultAsync(p => p.Code == "26300-35505");
            var pAir = await db.Parts.FirstOrDefaultAsync(p => p.Code == "28113-1R100");
            var ro1 = await db.ROs.Include(r => r.Car).Include(r => r.Customer).FirstOrDefaultAsync();

            var partOOs = new List<PartOO>
            {
                // 1. Xe Santa Fe (30G-889.21): Đang nợ khách 1 bộ má phanh (Chờ hàng về kho)
                new PartOO
                {
                    OONo = $"OO{DateTime.Today:yyMMdd}-001",
                    PartId = pBrake?.Id ?? 4,
                    PartCode = pBrake?.Code ?? "58101-C1A00",
                    PartName = pBrake?.Name ?? "Bộ má phanh đĩa trước Hyundai Santa Fe",
                    OOPlateNo = "30G-889.21",
                    Model = "Hyundai Santa Fe 2.2D",
                    SoLuongNo = 1,
                    SoLuongTra = 0,
                    CVDV = "CVDV Tuấn",
                    NgayDatHang = DateTime.Today.AddDays(-2),
                    NgayVeDuKien = DateTime.Today.AddDays(2),
                    NgayHenTra = DateTime.Today.AddDays(3),
                    GhiChu = "Phụ tùng đang chuyển từ kho tổng Mobis Bắc Giang. Đã liên hệ khách thông báo thời gian.",
                    Status = PartOOStatus.Owed,
                    CreatedBy = "CVDV Tuấn",
                    CreatedAt = DateTime.Now.AddDays(-2)
                },
                // 2. Xe Accent (29A-123.45): Nợ 2 lọc gió động cơ Accent, hàng đã về kho (InStock > 0), sẵn sàng hẹn khách đến lắp
                new PartOO
                {
                    OONo = $"OO{DateTime.Today:yyMMdd}-002",
                    PartId = pAir?.Id ?? 2,
                    PartCode = pAir?.Code ?? "28113-1R100",
                    PartName = pAir?.Name ?? "Lọc gió động cơ Hyundai Accent",
                    OOPlateNo = "29A-123.45",
                    Model = "Hyundai Accent 1.4 AT",
                    SoLuongNo = 2,
                    SoLuongTra = 0,
                    CVDV = "CVDV Hương",
                    NgayDatHang = DateTime.Today.AddDays(-4),
                    NgayVeDuKien = DateTime.Today.AddDays(-1),
                    NgayHenTra = DateTime.Today.AddDays(1),
                    GhiChu = "Phụ tùng đã về tới kho dịch vụ. Đã gọi điện nhắc khách mang xe đến lắp ráp miễn phí tiền công.",
                    Status = PartOOStatus.Arrived,
                    ROId = ro1?.Id,
                    CarId = ro1?.CarId,
                    CustomerId = ro1?.CustomerId,
                    CreatedBy = "CVDV Hương",
                    CreatedAt = DateTime.Now.AddDays(-4)
                },
                // 3. Xe Tucson (51F-678.90): Nợ 1 lọc dầu động cơ, đã trả và lắp hoàn tất cho khách
                new PartOO
                {
                    OONo = $"OO{DateTime.Today:yyMMdd}-003",
                    PartId = pFilter?.Id ?? 1,
                    PartCode = pFilter?.Code ?? "26300-35505",
                    PartName = pFilter?.Name ?? "Lọc dầu động cơ Hyundai chính hãng",
                    OOPlateNo = "51F-678.90",
                    Model = "Hyundai Tucson 2.0 AT",
                    SoLuongNo = 1,
                    SoLuongTra = 1,
                    CVDV = "CVDV Tuấn",
                    NgayDatHang = DateTime.Today.AddDays(-7),
                    NgayVeDuKien = DateTime.Today.AddDays(-3),
                    NgayHenTra = DateTime.Today.AddDays(-1),
                    GhiChu = "Khách đã mang xe vào xưởng và KTV Hùng đã hoàn tất lắp đặt bàn giao.",
                    Status = PartOOStatus.Completed,
                    CreatedBy = "CVDV Tuấn",
                    CreatedAt = DateTime.Now.AddDays(-7),
                    FinishedAt = DateTime.Now.AddDays(-1),
                    ReturnedBy = "KTV Hùng"
                }
            };

            db.PartOOs.AddRange(partOOs);
            await db.SaveChangesAsync();
        }

        if (!await db.CusDebits.AnyAsync())
        {
            var customers = await db.Customers.Include(c => c.Cars).ToListAsync();
            var c1 = customers.FirstOrDefault();
            var c2 = customers.Skip(1).FirstOrDefault();
            var c3 = customers.Skip(2).FirstOrDefault();
            var ros = await db.ROs.Include(r => r.Car).Include(r => r.Customer).ToListAsync();
            var ro1 = ros.FirstOrDefault();
            var ro2 = ros.Skip(1).FirstOrDefault();

            var debits = new List<CusDebit>();

            // 1. Khoản nợ còn lại của Khách hàng 1 (Trần Văn An) phát sinh từ RO bảo dưỡng định kỳ
            var deb1 = new CusDebit
            {
                DebitNo = $"CDB{DateTime.Today:yyMMdd}-001",
                CustomerId = c1?.Id ?? 1,
                CarId = c1?.Cars.FirstOrDefault()?.Id ?? ro1?.CarId,
                ROId = ro1?.Id,
                DebitType = CusDebitType.RO,
                Status = CusDebitStatus.Active,
                DebitDate = DateTime.Today.AddDays(-10),
                DueDate = DateTime.Today.AddDays(20),
                DebitAmount = 2850000,
                PaidAmount = 1000000,
                Description = "Khách nợ lại tiền công bảo dưỡng cấp 20.000km và thay dầu máy, hẹn thanh toán trước ngày 20.",
                CreatedBy = "CVDV Tuấn",
                CreatedAt = DateTime.Now.AddDays(-10)
            };
            debits.Add(deb1);

            // 2. Khoản nợ quá hạn của Khách hàng 2 (Lê Thị Bình) - Thay má phanh và phụ tùng
            var deb2 = new CusDebit
            {
                DebitNo = $"CDB{DateTime.Today:yyMMdd}-002",
                CustomerId = c2?.Id ?? 2,
                CarId = c2?.Cars.FirstOrDefault()?.Id ?? ro2?.CarId,
                ROId = ro2?.Id,
                DebitType = CusDebitType.RO,
                Status = CusDebitStatus.Active,
                DebitDate = DateTime.Today.AddDays(-35),
                DueDate = DateTime.Today.AddDays(-5), // Đã quá hạn 5 ngày
                DebitAmount = 4200000,
                PaidAmount = 0,
                Description = "Ghi nợ chi phí thay má phanh và công gò sơn cản trước xe Accent. Quá hạn thanh toán 5 ngày.",
                CreatedBy = "CVDV Hương",
                CreatedAt = DateTime.Now.AddDays(-35)
            };
            debits.Add(deb2);

            // 3. Khoản nợ mua phụ tùng bán lẻ xuất kho của Khách hàng 3 (Phạm Quốc Cường)
            var deb3 = new CusDebit
            {
                DebitNo = $"CDB{DateTime.Today:yyMMdd}-003",
                CustomerId = c3?.Id ?? 3,
                CarId = c3?.Cars.FirstOrDefault()?.Id,
                DebitType = CusDebitType.Part,
                Status = CusDebitStatus.Active,
                DebitDate = DateTime.Today.AddDays(-3),
                DueDate = DateTime.Today.AddDays(15),
                DebitAmount = 1650000,
                PaidAmount = 0,
                Description = "Nợ tiền mua 02 bình ắc quy và lọc gió động cơ mang về tự thay.",
                CreatedBy = "Thủ kho Tuấn",
                CreatedAt = DateTime.Now.AddDays(-3)
            };
            debits.Add(deb3);

            // 4. Khoản nợ đã được tất toán 100% (Cleared) của Khách hàng 1
            var deb4 = new CusDebit
            {
                DebitNo = $"CDB{DateTime.Today:yyMMdd}-004",
                CustomerId = c1?.Id ?? 1,
                CarId = c1?.Cars.FirstOrDefault()?.Id,
                DebitType = CusDebitType.Other,
                Status = CusDebitStatus.Cleared,
                DebitDate = DateTime.Today.AddDays(-20),
                DueDate = DateTime.Today.AddDays(-5),
                DebitAmount = 1500000,
                PaidAmount = 1500000,
                Description = "Gia công tiện đĩa phanh ngoài và vệ sinh buồng đốt khí hydro. Đã thanh toán đầy đủ.",
                CreatedBy = "CVDV Tuấn",
                CreatedAt = DateTime.Now.AddDays(-20),
                ClearedAt = DateTime.Now.AddDays(-8)
            };
            debits.Add(deb4);

            db.CusDebits.AddRange(debits);
            await db.SaveChangesAsync();

            // Seed Payments
            var payments = new List<CusDebitPayment>
            {
                new CusDebitPayment
                {
                    PaymentNo = $"CDP{DateTime.Today:yyMMdd}-001",
                    CustomerId = c1?.Id ?? 1,
                    CusDebitId = deb1.Id,
                    PaymentDate = DateTime.Today.AddDays(-5),
                    PaymentAmount = 1000000,
                    Method = PaymentMethod.BankTransfer,
                    PayPersonName = c1?.Name ?? "Trần Văn An",
                    PayPersonPhone = c1?.Phone ?? "0912.345.678",
                    PayPersonIdCard = "001085002199",
                    TransactionRef = "MBB-FT260422-9981",
                    Note = "Thanh toán đợt 1 tiền nợ công sửa chữa RO.",
                    Collector = "Thu ngân Trang",
                    CreatedAt = DateTime.Now.AddDays(-5)
                },
                new CusDebitPayment
                {
                    PaymentNo = $"CDP{DateTime.Today:yyMMdd}-002",
                    CustomerId = c1?.Id ?? 1,
                    CusDebitId = deb4.Id,
                    PaymentDate = DateTime.Today.AddDays(-8),
                    PaymentAmount = 1500000,
                    Method = PaymentMethod.Cash,
                    PayPersonName = c1?.Name ?? "Trần Văn An",
                    PayPersonPhone = c1?.Phone ?? "0912.345.678",
                    PayPersonIdCard = "001085002199",
                    Note = "Tất toán toàn bộ chi phí gia công ngoài.",
                    Collector = "Thu ngân Trang",
                    CreatedAt = DateTime.Now.AddDays(-8)
                }
            };

            db.CusDebitPayments.AddRange(payments);
            await db.SaveChangesAsync();
        }

        if (!await db.SupplierDebits.AnyAsync())
        {
            var supHtc = await db.Suppliers.FirstOrDefaultAsync(s => s.Code == "HTC");
            var supMobis = await db.Suppliers.FirstOrDefaultAsync(s => s.Code == "MOBIS");
            var supCastrol = await db.Suppliers.FirstOrDefaultAsync(s => s.Code == "CASTROL");
            var supDenso = await db.Suppliers.FirstOrDefaultAsync(s => s.Code == "DENSO");
            var stockIn1 = await db.StockIns.FirstOrDefaultAsync(s => s.StockInNo == "NK260427-001");

            var debits = new List<SupplierDebit>();

            // 1. Khoản nợ phụ tùng gầm & má phanh MOBIS (đã trả 1 phần)
            var deb1 = new SupplierDebit
            {
                DebitNo = $"SDB{DateTime.Today:yyMMdd}-001",
                SupplierId = supMobis?.Id ?? 2,
                StockInId = stockIn1?.Id,
                DebitType = SupplierDebitType.StockIn,
                Status = SupplierDebitStatus.Active,
                DebitDate = DateTime.Today.AddDays(-14),
                DueDate = DateTime.Today.AddDays(16),
                DebitAmount = 18500000,
                PaidAmount = 8500000,
                Description = "Công nợ phụ tùng gầm và má phanh chính hãng nhập kho theo phiếu NK260427-001.",
                CreatedBy = "Kế toán kho",
                CreatedAt = DateTime.Now.AddDays(-14)
            };
            debits.Add(deb1);

            // 2. Khoản nợ linh kiện điện tử HTC (Quá hạn 10 ngày)
            var deb2 = new SupplierDebit
            {
                DebitNo = $"SDB{DateTime.Today:yyMMdd}-002",
                SupplierId = supHtc?.Id ?? 1,
                DebitType = SupplierDebitType.StockIn,
                Status = SupplierDebitStatus.Active,
                DebitDate = DateTime.Today.AddDays(-40),
                DueDate = DateTime.Today.AddDays(-10),
                DebitAmount = 32000000,
                PaidAmount = 0,
                Description = "Lô linh kiện điện tử cảm biến và cụm điều khiển ABS động cơ xe Tucson & SantaFe. Quá hạn thanh toán 10 ngày.",
                CreatedBy = "Kế toán kho",
                CreatedAt = DateTime.Now.AddDays(-40)
            };
            debits.Add(deb2);

            // 3. Khoản nợ dầu nhớt Castrol (Còn trong hạn)
            var deb3 = new SupplierDebit
            {
                DebitNo = $"SDB{DateTime.Today:yyMMdd}-003",
                SupplierId = supCastrol?.Id ?? 3,
                DebitType = SupplierDebitType.StockIn,
                Status = SupplierDebitStatus.Active,
                DebitDate = DateTime.Today.AddDays(-5),
                DueDate = DateTime.Today.AddDays(25),
                DebitAmount = 12600000,
                PaidAmount = 0,
                Description = "Nhập 5 phuy dầu động cơ tổng hợp Castrol Magnatec 5W-30 và 20 can dầu hộp số tự động ATF.",
                CreatedBy = "Thủ kho Hùng",
                CreatedAt = DateTime.Now.AddDays(-5)
            };
            debits.Add(deb3);

            // 4. Khoản nợ bugi Denso đã tất toán 100% (Cleared)
            var deb4 = new SupplierDebit
            {
                DebitNo = $"SDB{DateTime.Today:yyMMdd}-004",
                SupplierId = supDenso?.Id ?? 4,
                DebitType = SupplierDebitType.StockIn,
                Status = SupplierDebitStatus.Cleared,
                DebitDate = DateTime.Today.AddDays(-25),
                DueDate = DateTime.Today.AddDays(5),
                DebitAmount = 7400000,
                PaidAmount = 7400000,
                ClearedAt = DateTime.Now.AddDays(-7),
                Description = "Lô bugi đánh lửa Denso Iridium Tough và lọc gió cabin xe Creta. Đã thanh toán đầy đủ qua UNC Techcombank.",
                CreatedBy = "Kế toán kho",
                CreatedAt = DateTime.Now.AddDays(-25)
            };
            debits.Add(deb4);

            db.SupplierDebits.AddRange(debits);
            await db.SaveChangesAsync();

            // Seed Supplier Payments
            var pmtList = new List<SupplierDebitPayment>
            {
                new SupplierDebitPayment
                {
                    PaymentNo = $"SDP{DateTime.Today:yyMMdd}-001",
                    SupplierId = supMobis?.Id ?? 2,
                    SupplierDebitId = deb1.Id,
                    PaymentDate = DateTime.Today.AddDays(-7),
                    PaymentAmount = 8500000,
                    Method = PaymentMethod.BankTransfer,
                    PayPersonName = supMobis?.ContactName ?? "Kim Jung Wook",
                    PayPersonPhone = supMobis?.ContactPhone ?? "0988.777.666",
                    BankAccount = supMobis?.BankAccount ?? "118002678999",
                    BankName = supMobis?.BankName ?? "VietinBank - CN Bắc Giang",
                    TransactionRef = "UNC-VIB-2026-0418",
                    Note = "Thanh toán đợt 1 tiền hàng phụ tùng gầm và má phanh theo UNC ngân hàng.",
                    Cashier = "Thủ quỹ Minh",
                    CreatedAt = DateTime.Now.AddDays(-7)
                },
                new SupplierDebitPayment
                {
                    PaymentNo = $"SDP{DateTime.Today:yyMMdd}-002",
                    SupplierId = supDenso?.Id ?? 4,
                    SupplierDebitId = deb4.Id,
                    PaymentDate = DateTime.Today.AddDays(-7),
                    PaymentAmount = 7400000,
                    Method = PaymentMethod.BankTransfer,
                    PayPersonName = supDenso?.ContactName ?? "Phạm Tuấn Anh",
                    PayPersonPhone = supDenso?.ContactPhone ?? "0915.222.333",
                    BankAccount = supDenso?.BankAccount ?? "19033456789012",
                    BankName = supDenso?.BankName ?? "Techcombank - CN Thăng Long",
                    TransactionRef = "UNC-TCB-2026-0922",
                    Note = "Tất toán toàn bộ công nợ lô bugi đánh lửa Denso theo UNC Techcombank.",
                    Cashier = "Thủ quỹ Minh",
                    CreatedAt = DateTime.Now.AddDays(-7)
                }
            };

            db.SupplierDebitPayments.AddRange(pmtList);
            await db.SaveChangesAsync();
        }

        if (!await db.DealerHistoryRecords.AnyAsync())
        {
            var pOil = await db.Parts.FirstOrDefaultAsync(p => p.Code == "05100-00441");
            var pFilter = await db.Parts.FirstOrDefaultAsync(p => p.Code == "26300-35505");
            var pAir = await db.Parts.FirstOrDefaultAsync(p => p.Code == "28113-1R100");
            var pBrake = await db.Parts.FirstOrDefaultAsync(p => p.Code == "58101-C1A00");
            var pSpark = await db.Parts.FirstOrDefaultAsync(p => p.Code == "18846-11070");
            var pCabin = await db.Parts.FirstOrDefaultAsync(p => p.Code == "97133-D3000");

            var records = new List<DealerHistoryRecord>();

            // 1. Xe 30A-123.45 (Hyundai Accent 2022) - VIN: RLHXXAC001 - Khách: Nguyễn Văn An
            // Lần 1: Bảo dưỡng 1.000km tại Hyundai Cầu Giấy (HTC-CG)
            var rec1 = new DealerHistoryRecord
            {
                RecordNo = $"DHR{DateTime.Today.AddMonths(-18):yyMMdd}-001",
                DealerCode = "HTC-CG",
                DealerName = "Hyundai Cầu Giấy",
                PlateNo = "30A-123.45",
                FrameNo = "RLHXXAC001",
                EngineNo = "G4LC-MN89123",
                TradeMarkName = "Hyundai",
                ModelName = "Hyundai Accent 1.4 AT",
                ColorCode = "Trắng Băng (Polar White)",
                ProductYear = 2022,
                CusName = "Nguyễn Văn An",
                CusPhone = "0901111111",
                CusAddress = "Số 15 Dịch Vọng Hậu, Cầu Giấy, Hà Nội",
                RONo = "RO-CG-221015",
                CheckInDate = DateTime.Today.AddMonths(-18),
                ActualDeliveryDate = DateTime.Today.AddMonths(-18).AddHours(2),
                Odometer = 1050,
                ServiceAdvisor = "CVDV Tuấn Hùng",
                Technician = "KTV Quang Vinh",
                CustomerRequest = "Bảo dưỡng 1.000km đầu tiên miễn phí tiền công",
                CarStatus = "Xe mới xuất xưởng 1 tháng, động cơ êm, không báo lỗi",
                RepairResult = "Đã kiểm tra siết gầm, thay dầu máy và lọc dầu động cơ. Xe đạt tiêu chuẩn xuất xưởng.",
                TotalLaborAmount = 0,
                TotalPartAmount = 790000,
                TotalAmount = 790000,
                FlagClaim = false,
                CreatedBy = "dms_sync",
                CreatedAt = DateTime.Now.AddMonths(-18),
                Items = [
                    new DealerHistoryItem { ItemType = LineType.Labor, Code = "BDD-010", Name = "Bảo dưỡng 1.000km (Miễn phí tiền công hãng)", Unit = "Lần", Quantity = 1, UnitPrice = 0, Amount = 0, ExpenseType = ExpenseType.Internal, Technician = "KTV Quang Vinh", Result = "Đạt" },
                    new DealerHistoryItem { ItemType = LineType.Part, Code = "05100-00441", Name = "Dầu nhờn động cơ Hyundai Premium 5W-30 (Can 4L)", Unit = "Can", Quantity = 1, UnitPrice = 610000, Amount = 610000, ExpenseType = ExpenseType.Customer },
                    new DealerHistoryItem { ItemType = LineType.Part, Code = "26300-35505", Name = "Lọc dầu động cơ chính hãng Hyundai", Unit = "Cái", Quantity = 1, UnitPrice = 180000, Amount = 180000, ExpenseType = ExpenseType.Customer }
                ]
            };
            records.Add(rec1);

            // Lần 2: Bảo dưỡng 10.000km tại Hyundai Phạm Văn Đồng (HTC-PDV)
            var rec2 = new DealerHistoryRecord
            {
                RecordNo = $"DHR{DateTime.Today.AddMonths(-10):yyMMdd}-002",
                DealerCode = "HTC-PDV",
                DealerName = "Hyundai Phạm Văn Đồng",
                PlateNo = "30A-123.45",
                FrameNo = "RLHXXAC001",
                EngineNo = "G4LC-MN89123",
                TradeMarkName = "Hyundai",
                ModelName = "Hyundai Accent 1.4 AT",
                ColorCode = "Trắng Băng (Polar White)",
                ProductYear = 2022,
                CusName = "Nguyễn Văn An",
                CusPhone = "0901111111",
                CusAddress = "Cầu Giấy, Hà Nội",
                RONo = "RO-PDV-230620",
                CheckInDate = DateTime.Today.AddMonths(-10),
                ActualDeliveryDate = DateTime.Today.AddMonths(-10).AddHours(3),
                Odometer = 10450,
                ServiceAdvisor = "CVDV Hoàng Lan",
                Technician = "KTV Văn Đức",
                CustomerRequest = "Bảo dưỡng định kỳ Cấp 2 (10.000km), kiểm tra phanh và vệ sinh lọc gió",
                CarStatus = "Xe hoạt động ổn định, má phanh mòn đều",
                RepairResult = "Đã thay dầu, lọc dầu, lọc gió điều hòa và bảo dưỡng phanh 4 bánh",
                TotalLaborAmount = 380000,
                TotalPartAmount = 1070000,
                TotalAmount = 1450000,
                FlagClaim = false,
                CreatedBy = "dms_sync",
                CreatedAt = DateTime.Now.AddMonths(-10),
                Items = [
                    new DealerHistoryItem { ItemType = LineType.Labor, Code = "BDD-020", Name = "Công bảo dưỡng định kỳ Cấp 2 (10.000 km)", Unit = "Giờ", Quantity = 1.2m, UnitPrice = 380000, Amount = 380000, ExpenseType = ExpenseType.Customer, Technician = "KTV Văn Đức", Result = "Đạt" },
                    new DealerHistoryItem { ItemType = LineType.Part, Code = "05100-00441", Name = "Dầu nhờn động cơ Hyundai Premium 5W-30 (Can 4L)", Unit = "Can", Quantity = 1, UnitPrice = 610000, Amount = 610000, ExpenseType = ExpenseType.Customer },
                    new DealerHistoryItem { ItemType = LineType.Part, Code = "26300-35505", Name = "Lọc dầu động cơ chính hãng Hyundai", Unit = "Cái", Quantity = 1, UnitPrice = 180000, Amount = 180000, ExpenseType = ExpenseType.Customer },
                    new DealerHistoryItem { ItemType = LineType.Part, Code = "97133-D3000", Name = "Lọc gió điều hòa than hoạt tính", Unit = "Cái", Quantity = 1, UnitPrice = 280000, Amount = 280000, ExpenseType = ExpenseType.Customer }
                ]
            };
            records.Add(rec2);

            // Lần 3: Bảo dưỡng 20.000km + thay má phanh tại Hyundai Đông Đô (HTC-DD)
            var rec3 = new DealerHistoryRecord
            {
                RecordNo = $"DHR{DateTime.Today.AddMonths(-3):yyMMdd}-003",
                DealerCode = "HTC-DD",
                DealerName = "Hyundai Đông Đô",
                PlateNo = "30A-123.45",
                FrameNo = "RLHXXAC001",
                EngineNo = "G4LC-MN89123",
                TradeMarkName = "Hyundai",
                ModelName = "Hyundai Accent 1.4 AT",
                ColorCode = "Trắng Băng (Polar White)",
                ProductYear = 2022,
                CusName = "Nguyễn Văn An",
                CusPhone = "0901111111",
                CusAddress = "Cầu Giấy, Hà Nội",
                RONo = "RO-DD-240118",
                CheckInDate = DateTime.Today.AddMonths(-3),
                ActualDeliveryDate = DateTime.Today.AddMonths(-3).AddHours(4),
                Odometer = 20200,
                ServiceAdvisor = "CVDV Minh Quân",
                Technician = "KTV Đình Trọng",
                CustomerRequest = "Bảo dưỡng cấp 3 định kỳ 20.000km, đảo lốp và kiểm tra tiếng kêu nhẹ bánh trước",
                CarStatus = "Má phanh trước mòn gần tới hạn, bugi đánh lửa còn tốt",
                RepairResult = "Đã thay dầu động cơ, lọc dầu, lọc gió động cơ, thay bộ má phanh đĩa trước và đảo 4 bánh",
                TotalLaborAmount = 740000,
                TotalPartAmount = 2380000,
                TotalAmount = 3120000,
                FlagClaim = false,
                CreatedBy = "dms_sync",
                CreatedAt = DateTime.Now.AddMonths(-3),
                Items = [
                    new DealerHistoryItem { ItemType = LineType.Labor, Code = "BDD-030", Name = "Công bảo dưỡng định kỳ Cấp 3 (20.000 km)", Unit = "Giờ", Quantity = 1.8m, UnitPrice = 520000, Amount = 520000, ExpenseType = ExpenseType.Customer, Technician = "KTV Đình Trọng", Result = "Đạt chuẩn" },
                    new DealerHistoryItem { ItemType = LineType.Labor, Code = "SCC-BRK-01", Name = "Công thay bộ má phanh đĩa trước & vệ sinh cùm phanh", Unit = "Giờ", Quantity = 0.8m, UnitPrice = 220000, Amount = 220000, ExpenseType = ExpenseType.Customer, Technician = "KTV Đình Trọng", Result = "Lắp ráp đúng kỹ thuật" },
                    new DealerHistoryItem { ItemType = LineType.Part, Code = "05100-00441", Name = "Dầu nhờn động cơ Hyundai Premium 5W-30 (Can 4L)", Unit = "Can", Quantity = 1, UnitPrice = 610000, Amount = 610000, ExpenseType = ExpenseType.Customer },
                    new DealerHistoryItem { ItemType = LineType.Part, Code = "26300-35505", Name = "Lọc dầu động cơ chính hãng Hyundai", Unit = "Cái", Quantity = 1, UnitPrice = 180000, Amount = 180000, ExpenseType = ExpenseType.Customer },
                    new DealerHistoryItem { ItemType = LineType.Part, Code = "28113-1R100", Name = "Lọc gió động cơ Hyundai Accent", Unit = "Cái", Quantity = 1, UnitPrice = 240000, Amount = 240000, ExpenseType = ExpenseType.Customer },
                    new DealerHistoryItem { ItemType = LineType.Part, Code = "58101-C1A00", Name = "Bộ má phanh đĩa trước chính hãng", Unit = "Bộ", Quantity = 1, UnitPrice = 1350000, Amount = 1350000, ExpenseType = ExpenseType.Customer }
                ]
            };
            records.Add(rec3);

            // 2. Xe 51G-678.90 (Hyundai Tucson 2023) - VIN: RLHXXTC002 - Khách: Trần Thị Bình
            // Lần 1: Bảo dưỡng 5.000km tại Hyundai Sài Gòn (HTC-SG)
            var rec4 = new DealerHistoryRecord
            {
                RecordNo = $"DHR{DateTime.Today.AddMonths(-8):yyMMdd}-004",
                DealerCode = "HTC-SG",
                DealerName = "Hyundai Sài Gòn 1S",
                PlateNo = "51G-678.90",
                FrameNo = "RLHXXTC002",
                EngineNo = "G4FJ-PL98342",
                TradeMarkName = "Hyundai",
                ModelName = "Hyundai Tucson 2.0 AT",
                ColorCode = "Đỏ Mận (Fiery Red)",
                ProductYear = 2023,
                CusName = "Trần Thị Bình",
                CusPhone = "0902222222",
                CusAddress = "Quận 1, TP. Hồ Chí Minh",
                RONo = "RO-SG-230810",
                CheckInDate = DateTime.Today.AddMonths(-8),
                ActualDeliveryDate = DateTime.Today.AddMonths(-8).AddHours(2),
                Odometer = 5120,
                ServiceAdvisor = "CVDV Thanh Tùng",
                Technician = "KTV Quốc Hưng",
                CustomerRequest = "Bảo dưỡng 5.000km định kỳ, thay dầu máy",
                CarStatus = "Xe hoạt động tốt, không có hiện tượng bất thường",
                RepairResult = "Đã thay dầu máy và lọc dầu chính hãng",
                TotalLaborAmount = 250000,
                TotalPartAmount = 790000,
                TotalAmount = 1040000,
                FlagClaim = false,
                CreatedBy = "dms_sync",
                CreatedAt = DateTime.Now.AddMonths(-8),
                Items = [
                    new DealerHistoryItem { ItemType = LineType.Labor, Code = "BDD-015", Name = "Công bảo dưỡng cấp 5.000km", Unit = "Giờ", Quantity = 0.8m, UnitPrice = 250000, Amount = 250000, ExpenseType = ExpenseType.Customer, Technician = "KTV Quốc Hưng", Result = "Đạt" },
                    new DealerHistoryItem { ItemType = LineType.Part, Code = "05100-00441", Name = "Dầu nhờn động cơ Hyundai Premium 5W-30 (Can 4L)", Unit = "Can", Quantity = 1, UnitPrice = 610000, Amount = 610000, ExpenseType = ExpenseType.Customer },
                    new DealerHistoryItem { ItemType = LineType.Part, Code = "26300-35505", Name = "Lọc dầu động cơ chính hãng Hyundai", Unit = "Cái", Quantity = 1, UnitPrice = 180000, Amount = 180000, ExpenseType = ExpenseType.Customer }
                ]
            };
            records.Add(rec4);

            // Lần 2: Xử lý bảo hành lỗi bỏ lửa động cơ (Claim) tại Hyundai Service Workshop (HTC-MAIN)
            var rec5 = new DealerHistoryRecord
            {
                RecordNo = $"DHR{DateTime.Today.AddDays(-2):yyMMdd}-005",
                DealerCode = "HTC-MAIN",
                DealerName = "Hyundai Service Workshop (Đại lý hiện tại)",
                PlateNo = "51G-678.90",
                FrameNo = "RLHXXTC002",
                EngineNo = "G4FJ-PL98342",
                TradeMarkName = "Hyundai",
                ModelName = "Hyundai Tucson 2.0 AT",
                ColorCode = "Đỏ Mận (Fiery Red)",
                ProductYear = 2023,
                CusName = "Trần Thị Bình",
                CusPhone = "0902222222",
                CusAddress = "Thanh Xuân, Hà Nội",
                RONo = "ROSEED-WAR-001",
                CheckInDate = DateTime.Today.AddDays(-2),
                ActualDeliveryDate = DateTime.Today.AddDays(-1),
                Odometer = 18200,
                ServiceAdvisor = "CVDV Tuấn Hùng",
                Technician = "KTV Quang Vinh",
                CustomerRequest = "Bảo hành: Động cơ rung giật khi tăng tốc, đèn Check Engine sáng",
                CarStatus = "Máy chẩn đoán GDS đọc lỗi P0302 (Bỏ lửa xy-lanh số 2), bugi nứt sứ cách điện",
                RepairResult = "Đã thay thế bộ 4 bugi đánh lửa Iridium theo diện bảo hành hãng HTC/HMC. Xóa mã lỗi, thử xe êm ái.",
                TotalLaborAmount = 350000,
                TotalPartAmount = 880000,
                TotalAmount = 1230000,
                FlagClaim = true,
                ClaimNo = "WAR260427-001",
                ClaimStatus = "ACCE",
                CreatedBy = "dms_sync",
                CreatedAt = DateTime.Now.AddDays(-2),
                Items = [
                    new DealerHistoryItem { ItemType = LineType.Labor, Code = "WAR-LAB-01", Name = "Công chẩn đoán GDS & thay thế bugi bảo hành", Unit = "Lần", Quantity = 1, UnitPrice = 350000, Amount = 350000, ExpenseType = ExpenseType.Warranty, Technician = "KTV Quang Vinh", Result = "Đạt tiêu chuẩn bảo hành HTC" },
                    new DealerHistoryItem { ItemType = LineType.Part, Code = "18846-11070", Name = "Bugi đánh lửa Iridium cao cấp (Bộ 4 chiếc)", Unit = "Cái", Quantity = 4, UnitPrice = 220000, Amount = 880000, ExpenseType = ExpenseType.Warranty, Remark = "Bảo hành hãng chi trả 100%" }
                ]
            };
            records.Add(rec5);

            // 3. Xe 30E-888.99 (Hyundai Santa Fe 2.2D 2023) - VIN: KMHFH41BPA123456 - Khách: Hoàng Văn Cường
            var rec6 = new DealerHistoryRecord
            {
                RecordNo = $"DHR{DateTime.Today.AddMonths(-6):yyMMdd}-006",
                DealerCode = "HTC-PDV",
                DealerName = "Hyundai Phạm Văn Đồng",
                PlateNo = "30E-888.99",
                FrameNo = "KMHFH41BPA123456",
                EngineNo = "D4HB-KN67234",
                TradeMarkName = "Hyundai",
                ModelName = "Hyundai Santa Fe 2.2D HTRAC",
                ColorCode = "Đen (Phantom Black)",
                ProductYear = 2023,
                CusName = "Hoàng Văn Cường",
                CusPhone = "0918888999",
                CusAddress = "Tây Hồ, Hà Nội",
                RONo = "RO-PDV-231005",
                CheckInDate = DateTime.Today.AddMonths(-6),
                ActualDeliveryDate = DateTime.Today.AddMonths(-6).AddHours(3),
                Odometer = 12500,
                ServiceAdvisor = "CVDV Minh Quân",
                Technician = "KTV Tuấn Anh",
                CustomerRequest = "Bảo dưỡng cấp 2, kiểm tra hệ dẫn động 4 bánh toàn thời gian HTRAC",
                CarStatus = "Xe vận hành tốt, dầu phanh và nước làm mát đầy đủ",
                RepairResult = "Đã thay dầu động cơ máy dầu Castrol 5W-30, lọc dầu, lọc nhiên liệu dầu Diesel",
                TotalLaborAmount = 450000,
                TotalPartAmount = 1450000,
                TotalAmount = 1900000,
                FlagClaim = false,
                CreatedBy = "dms_sync",
                CreatedAt = DateTime.Now.AddMonths(-6),
                Items = [
                    new DealerHistoryItem { ItemType = LineType.Labor, Code = "BDD-025", Name = "Công bảo dưỡng cấp 10.000km xe máy dầu", Unit = "Giờ", Quantity = 1.5m, UnitPrice = 450000, Amount = 450000, ExpenseType = ExpenseType.Customer, Technician = "KTV Tuấn Anh", Result = "Đạt" },
                    new DealerHistoryItem { ItemType = LineType.Part, Code = "05100-00441", Name = "Dầu động cơ tổng hợp Hyundai Diesel 5W-30 (6L)", Unit = "Can", Quantity = 1.5m, UnitPrice = 650000, Amount = 975000, ExpenseType = ExpenseType.Customer },
                    new DealerHistoryItem { ItemType = LineType.Part, Code = "26300-35505", Name = "Lọc dầu động cơ Santa Fe", Unit = "Cái", Quantity = 1, UnitPrice = 215000, Amount = 215000, ExpenseType = ExpenseType.Customer },
                    new DealerHistoryItem { ItemType = LineType.Part, Code = "97133-D3000", Name = "Lọc gió điều hòa than hoạt tính", Unit = "Cái", Quantity = 1, UnitPrice = 260000, Amount = 260000, ExpenseType = ExpenseType.Customer }
                ]
            };
            records.Add(rec6);

            db.DealerHistoryRecords.AddRange(records);
            await db.SaveChangesAsync();
        }

        if (!await db.InsuranceDebits.AnyAsync())
        {
            var bv = await db.InsuranceCompanies.FirstOrDefaultAsync(c => c.InsNo == "BH-BV");
            var pvi = await db.InsuranceCompanies.FirstOrDefaultAsync(c => c.InsNo == "BH-PVI");
            var pti = await db.InsuranceCompanies.FirstOrDefaultAsync(c => c.InsNo == "BH-PTI");
            var mic = await db.InsuranceCompanies.FirstOrDefaultAsync(c => c.InsNo == "BH-MIC");

            var clm1 = await db.InsuranceClaims.FirstOrDefaultAsync(c => c.ClaimNo == "BH260427-001");
            var clm2 = await db.InsuranceClaims.FirstOrDefaultAsync(c => c.ClaimNo == "BH260427-002");
            var ro1 = await db.ROs.Include(r => r.Car).Include(r => r.Customer).FirstOrDefaultAsync();
            var ro2 = await db.ROs.Include(r => r.Car).Include(r => r.Customer).Skip(1).FirstOrDefaultAsync();

            var debits = new List<InsuranceDebit>();

            // 1. Bảo Việt - Đã tất toán (Hồ sơ BH260427-001)
            var deb1 = new InsuranceDebit
            {
                DebitNo = $"IDB{DateTime.Today.AddDays(-15):yyMMdd}-001",
                InsuranceCompanyId = bv?.Id ?? 1,
                InsNo = bv?.InsNo ?? "BH-BV",
                InsName = bv?.InsName ?? "Bảo hiểm Bảo Việt",
                InsuranceClaimId = clm1?.Id,
                ClaimNo = clm1?.ClaimNo ?? "BH260427-001",
                PolicyNo = clm1?.PolicyNo ?? "BV-VC-2026-88192",
                ROId = clm1?.ROId ?? ro1?.Id,
                RONo = ro1?.Code ?? "ROSEED-001",
                PlateNo = ro1?.Car?.Plate ?? "30A-123.45",
                CarModel = ro1?.Car?.Model ?? "Hyundai Accent 1.4 AT",
                CustomerName = ro1?.Customer?.Name ?? "Nguyễn Văn An",
                DebitType = InsuranceDebitType.Claim,
                Status = InsuranceDebitStatus.Cleared,
                DebitDate = DateTime.Today.AddDays(-15),
                DueDate = DateTime.Today.AddDays(15),
                DebitAmount = 7200000,
                PaidAmount = 7200000,
                ClearedAt = DateTime.Now.AddDays(-5),
                Description = "Bảo lãnh bồi thường thay cản trước và sơn sấy hấp xe theo hồ sơ giám định BH260427-001",
                CreatedBy = "CVDV Hoàng",
                CreatedAt = DateTime.Now.AddDays(-15)
            };
            debits.Add(deb1);

            // 2. Bảo Việt - Còn nợ (Lệnh RO sửa chữa xe 30E-888.99)
            var deb2 = new InsuranceDebit
            {
                DebitNo = $"IDB{DateTime.Today.AddDays(-5):yyMMdd}-002",
                InsuranceCompanyId = bv?.Id ?? 1,
                InsNo = bv?.InsNo ?? "BH-BV",
                InsName = bv?.InsName ?? "Bảo hiểm Bảo Việt",
                ROId = ro1?.Id,
                RONo = "RO-PDV-231005",
                PlateNo = "30E-888.99",
                CarModel = "Hyundai Santa Fe 2.2D HTRAC",
                CustomerName = "Hoàng Văn Cường",
                PolicyNo = "BV-VC-2026-99231",
                DebitType = InsuranceDebitType.RO,
                Status = InsuranceDebitStatus.Active,
                DebitDate = DateTime.Today.AddDays(-5),
                DueDate = DateTime.Today.AddDays(25),
                DebitAmount = 14500000,
                PaidAmount = 0,
                Description = "Bảo lãnh chi phí phục hồi sườn xe bên lái và thay cụm gương chiếu hậu có camera 360",
                CreatedBy = "CVDV Tuấn Hùng",
                CreatedAt = DateTime.Now.AddDays(-5)
            };
            debits.Add(deb2);

            // 3. PVI - Đã thu 1 phần (Hồ sơ BH260427-002)
            var deb3 = new InsuranceDebit
            {
                DebitNo = $"IDB{DateTime.Today.AddDays(-10):yyMMdd}-003",
                InsuranceCompanyId = pvi?.Id ?? 2,
                InsNo = pvi?.InsNo ?? "BH-PVI",
                InsName = pvi?.InsName ?? "Bảo hiểm PVI",
                InsuranceClaimId = clm2?.Id,
                ClaimNo = clm2?.ClaimNo ?? "BH260427-002",
                PolicyNo = clm2?.PolicyNo ?? "PVI-VC-2026-44319",
                ROId = clm2?.ROId ?? ro2?.Id,
                RONo = ro2?.Code ?? "ROSEED-002",
                PlateNo = ro2?.Car?.Plate ?? "51G-678.90",
                CarModel = ro2?.Car?.Model ?? "Hyundai Tucson 2.0 AT",
                CustomerName = ro2?.Customer?.Name ?? "Trần Thị Bình",
                DebitType = InsuranceDebitType.Claim,
                Status = InsuranceDebitStatus.Active,
                DebitDate = DateTime.Today.AddDays(-10),
                DueDate = DateTime.Today.AddDays(20),
                DebitAmount = 6000000,
                PaidAmount = 2000000,
                Description = "Bồi thường tổn thất tai nạn lùi xe nắp cốp sau và cụm đèn hậu xe 51G-678.90",
                CreatedBy = "CVDV Thắng",
                CreatedAt = DateTime.Now.AddDays(-10)
            };
            debits.Add(deb3);

            // 4. PTI - Quá hạn thanh toán (Kính chắn gió và nắp ca-pô)
            var deb4 = new InsuranceDebit
            {
                DebitNo = $"IDB{DateTime.Today.AddDays(-40):yyMMdd}-004",
                InsuranceCompanyId = pti?.Id ?? 3,
                InsNo = pti?.InsNo ?? "BH-PTI",
                InsName = pti?.InsName ?? "Bảo hiểm PTI",
                PlateNo = "30F-999.88",
                CarModel = "Hyundai Creta 1.5 AT",
                CustomerName = "Vũ Hải Đăng",
                PolicyNo = "PTI-AUTO-2026-1189",
                DebitType = InsuranceDebitType.DirectAdjustment,
                Status = InsuranceDebitStatus.Active,
                DebitDate = DateTime.Today.AddDays(-40),
                DueDate = DateTime.Today.AddDays(-10), // Quá hạn 10 ngày!
                DebitAmount = 9800000,
                PaidAmount = 0,
                Description = "Bồi thường rạn nứt kính chắn gió chính hãng do đá văng và sơn nắp ca-pô (Quá hạn 10 ngày)",
                CreatedBy = "CVDV Tuấn",
                CreatedAt = DateTime.Now.AddDays(-40)
            };
            debits.Add(deb4);

            db.InsuranceDebits.AddRange(debits);
            await db.SaveChangesAsync();

            // Phiếu thu tiền bảo hiểm bồi thường (InsuranceDebitPayment)
            var payments = new List<InsuranceDebitPayment>
            {
                new InsuranceDebitPayment
                {
                    PaymentNo = $"IPM{DateTime.Today.AddDays(-5):yyMMdd}-001",
                    InsuranceCompanyId = bv?.Id ?? 1,
                    InsNo = bv?.InsNo ?? "BH-BV",
                    InsName = bv?.InsName ?? "Bảo hiểm Bảo Việt",
                    InsuranceDebitId = deb1.Id,
                    PaymentDate = DateTime.Today.AddDays(-5),
                    PaymentAmount = 7200000,
                    Method = PaymentMethod.BankTransfer,
                    PayPersonName = "Nguyễn Văn Tuấn (Giám định Bảo Việt)",
                    PayPersonPhone = "0988.555.666",
                    BankAccount = "118002678999",
                    BankName = "VietinBank - CN Đống Đa",
                    TransactionRef = "GBC-VCB-2026-88912",
                    Note = "Bảo Việt chuyển khoản thanh toán 100% chi phí bồi thường hồ sơ BH260427-001 theo UNC số 88912.",
                    Cashier = "Thu ngân Lan",
                    Status = InsuranceDebitPaymentStatus.Confirmed,
                    CreatedAt = DateTime.Now.AddDays(-5)
                },
                new InsuranceDebitPayment
                {
                    PaymentNo = $"IPM{DateTime.Today.AddDays(-2):yyMMdd}-002",
                    InsuranceCompanyId = pvi?.Id ?? 2,
                    InsNo = pvi?.InsNo ?? "BH-PVI",
                    InsName = pvi?.InsName ?? "Bảo hiểm PVI",
                    InsuranceDebitId = deb3.Id,
                    PaymentDate = DateTime.Today.AddDays(-2),
                    PaymentAmount = 2000000,
                    Method = PaymentMethod.BankTransfer,
                    PayPersonName = "Lê Hoàng Long (Giám định PVI)",
                    PayPersonPhone = "0988.112.233",
                    BankAccount = "118002678999",
                    BankName = "VietinBank - CN Đống Đa",
                    TransactionRef = "GBC-BIDV-2026-4421",
                    Note = "PVI tạm ứng đợt 1 chi phí sơn nắp cốp sau và mua phụ tùng đèn hậu xe 51G-678.90.",
                    Cashier = "Thu ngân Lan",
                    Status = InsuranceDebitPaymentStatus.Confirmed,
                    CreatedAt = DateTime.Now.AddDays(-2)
                }
            };

            db.InsuranceDebitPayments.AddRange(payments);
            await db.SaveChangesAsync();
        }

        if (!await db.CustomerGroups.AnyAsync())
        {
            var cars = await db.Cars.Include(c => c.Customer).ToListAsync();
            var car1 = cars.FirstOrDefault(c => c.Plate == "30A-123.45") ?? cars.FirstOrDefault();
            var car2 = cars.FirstOrDefault(c => c.Plate == "51G-678.90") ?? cars.Skip(1).FirstOrDefault();

            // 1. Tập đoàn Mai Linh - Taxi Mai Linh Hà Nội
            var gMaiLinh = new CustomerGroup
            {
                GroupNo = "KD-MAILINH",
                GroupName = "Công ty CP Tập đoàn Mai Linh - Chi nhánh Hà Nội",
                TaxCode = "0101234567",
                Address = "Số 41 Hai Bà Trưng, P. Tràng Tiền, Q. Hoàn Kiếm, Hà Nội",
                Telephone = "024-38333333",
                Fax = "024-38333334",
                Email = "fleet.hanoi@mailinh.vn",
                ContactPerson = "Nguyễn Tuấn Anh (Trưởng phòng Quản lý Phương tiện)",
                ContactPhone = "0912.345.678",
                Description = "Thỏa thuận bảo dưỡng định kỳ đội xe taxi Mai Linh: Ưu tiên khoang sửa chữa nhanh SCS, miễn phí rửa xe hút bụi sau bảo dưỡng, hỗ trợ cứu hộ 24/7.",
                IsActive = true,
                DiscountPercentLabor = 10,
                DiscountPercentPart = 5,
                CreditLimit = 200_000_000,
                PaymentTermDays = 30,
                ContractNo = "HD-ML-2026/HYUNDAI-HN",
                ContractStartDate = DateTime.Today.AddMonths(-3),
                ContractEndDate = DateTime.Today.AddMonths(9),
                CreatedBy = "Hệ thống",
                CreatedAt = DateTime.Now.AddMonths(-3)
            };

            // 2. Vinasun Taxi
            var gVinasun = new CustomerGroup
            {
                GroupNo = "KD-VINASUN",
                GroupName = "Công ty Cổ phần Ánh Dương Việt Nam (Vinasun Taxi)",
                TaxCode = "0302032309",
                Address = "Số 648 Nguyễn Trãi, Phường 11, Quận 5, TP. Hồ Chí Minh",
                Telephone = "028-38272727",
                Email = "kythuat@vinasuntaxi.com",
                ContactPerson = "Lê Minh Trí (Giám đốc Kỹ thuật & Bảo dưỡng Đội xe)",
                ContactPhone = "0988.777.666",
                Description = "Hợp đồng sửa chữa lớn đồng sơn và đại tu đội xe vận tải du lịch & taxi thương quyền Vinasun.",
                IsActive = true,
                DiscountPercentLabor = 12,
                DiscountPercentPart = 7,
                CreditLimit = 300_000_000,
                PaymentTermDays = 45,
                ContractNo = "HD-VNS-2026/HYUNDAI-SGN",
                ContractStartDate = DateTime.Today.AddMonths(-2),
                ContractEndDate = DateTime.Today.AddMonths(10),
                CreatedBy = "Hệ thống",
                CreatedAt = DateTime.Now.AddMonths(-2)
            };

            // 3. Đội xe Vietcombank Hội sở
            var gVcb = new CustomerGroup
            {
                GroupNo = "KD-VIETCOMBANK",
                GroupName = "Ngân hàng TMCP Ngoại thương Việt Nam (Đội xe VCB Hội sở)",
                TaxCode = "0100112437",
                Address = "Số 198 Trần Quang Khải, Q. Hoàn Kiếm, Hà Nội",
                Telephone = "024-39343137",
                Email = "doixe@vietcombank.com.vn",
                ContactPerson = "Hoàng Quốc Dũng (Đội trưởng Đội xe Ban Quản trị Trụ sở)",
                ContactPhone = "0903.111.222",
                Description = "Đội xe chuyên chở cán bộ lãnh đạo ngân hàng Vietcombank; yêu cầu kiểm tra kỹ thuật an toàn định kỳ nghiêm ngặt.",
                IsActive = true,
                DiscountPercentLabor = 8,
                DiscountPercentPart = 5,
                CreditLimit = 150_000_000,
                PaymentTermDays = 30,
                ContractNo = "HD-VCB-2025/HYUNDAI-FLEET",
                ContractStartDate = DateTime.Today.AddMonths(-6),
                ContractEndDate = DateTime.Today.AddMonths(6),
                CreatedBy = "Hệ thống",
                CreatedAt = DateTime.Now.AddMonths(-6)
            };

            db.CustomerGroups.AddRange(gMaiLinh, gVinasun, gVcb);
            await db.SaveChangesAsync();

            // Gán thành viên xe vào các đoàn
            if (car1 != null)
            {
                db.CustomerGroupMembers.Add(new CustomerGroupMember
                {
                    CustomerGroupId = gMaiLinh.Id,
                    CarId = car1.Id,
                    CustomerId = car1.CustomerId,
                    PlateNo = car1.Plate,
                    DriverName = "Phạm Văn Tuấn",
                    DriverPhone = "0912.888.777",
                    Note = "Xe taxi ca ngày - Đội xe Mai Linh Hoàn Kiếm",
                    JoinedDate = DateTime.Today.AddMonths(-3),
                    IsActive = true
                });
            }

            if (car2 != null)
            {
                db.CustomerGroupMembers.Add(new CustomerGroupMember
                {
                    CustomerGroupId = gVinasun.Id,
                    CarId = car2.Id,
                    CustomerId = car2.CustomerId,
                    PlateNo = car2.Plate,
                    DriverName = "Trần Đình Khang",
                    DriverPhone = "0988.666.555",
                    Note = "Xe hợp đồng đưa đón đối tác cao cấp",
                    JoinedDate = DateTime.Today.AddMonths(-2),
                    IsActive = true
                });
            }

            await db.SaveChangesAsync();

            // Cập nhật Lệnh RO mẫu liên kết khách đoàn
            var ro = await db.ROs.Include(r => r.Lines).FirstOrDefaultAsync();
            if (ro != null)
            {
                ro.CustomerGroupId = gMaiLinh.Id;
                var laborTotal = ro.Lines.Where(l => l.Type == LineType.Labor).Sum(l => l.Amount);
                var partTotal = ro.Lines.Where(l => l.Type == LineType.Part).Sum(l => l.Amount);
                ro.CustomerGroupDiscountAmount = Math.Round(laborTotal * 0.10m + partTotal * 0.05m, 0);
                await db.SaveChangesAsync();
            }
        }

        if (!await db.PartPriceRequests.AnyAsync())
        {
            var ro1 = await db.ROs.Include(r => r.Car).FirstOrDefaultAsync();
            var pBrake = await db.Parts.FirstOrDefaultAsync(p => p.Code == "58101-C1A00");

            var requests = new List<PartPriceRequest>
            {
                // 1. Đề nghị giá đã được đại lý phê duyệt & đồng bộ vào kho (Approved / Priced)
                new PartPriceRequest
                {
                    ReqPartPriceNo = $"RPP{DateTime.Today:yyMMdd}-001",
                    DealerCode = "HTC-CG",
                    DealerName = "Hyundai Cầu Giấy",
                    Description = "Đề nghị cung cấp giá khẩn cấp cụm thước lái trợ lực điện MDPS và hộp điều khiển túi khí xe Santa Fe 2024 tai nạn bảo hiểm",
                    TSTReqPartPriceID = "TST-PR-2026-9042",
                    TSTSentDate = DateTime.Today.AddDays(-2).AddHours(9),
                    DMSStatus = DMSReqPartPriceStatus.Approved,
                    TSTStatus = TSTReqPartPriceStatus.Priced,
                    FlagIsCheck = true,
                    IsUpdatePrice = true,
                    UpdatedPriceAt = DateTime.Today.AddDays(-1).AddHours(16),
                    EffectiveDate = DateTime.Today.AddDays(-1),
                    EstimatedResponseDate = DateTime.Today.AddDays(-1),
                    CreatedBy = "Thủ kho Tuấn",
                    CreatedAt = DateTime.Today.AddDays(-2).AddHours(8),
                    ApprovedBy = "GĐ Dịch vụ Hoàng",
                    ApprovedAt = DateTime.Today.AddDays(-1).AddHours(16),
                    ROId = ro1?.Id,
                    VIN = "KMHFH41BPA123456",
                    CarModel = "Hyundai Santa Fe 2.2D HTRAC",
                    Items = [
                        new PartPriceRequestLine
                        {
                            DMSPartCode = "56500-S1000",
                            VieName = "Cụm thước lái trợ lực điện MDPS chính hãng",
                            VINCode = "KMHFH41BPA123456",
                            DeliveryForm = PartPriceDeliveryForm.VOR,
                            Quantity = 1,
                            Unit = "Cụm",
                            Remark = "Thước lái bị cong vênh trục ty do xe đâm va gầm, yêu cầu VOR khẩn",
                            TSTPartCode = "56500-S1000-TST",
                            TSTPrice = 14500000,
                            DateEffect = DateTime.Today.AddDays(-1),
                            Status = ReqPartPriceLineStatus.Priced
                        },
                        new PartPriceRequestLine
                        {
                            DMSPartCode = "95910-S1100",
                            VieName = "Hộp điều khiển túi khí trung tâm ACU",
                            VINCode = "KMHFH41BPA123456",
                            DeliveryForm = PartPriceDeliveryForm.VOR,
                            Quantity = 1,
                            Unit = "Hộp",
                            Remark = "Đã kích nổ túi khí vô lăng và phụ, cần thay thế hộp mới theo quy chuẩn HMC",
                            TSTPartCode = "95910-S1100-TST",
                            TSTPrice = 8800000,
                            DateEffect = DateTime.Today.AddDays(-1),
                            Status = ReqPartPriceLineStatus.Priced
                        }
                    ]
                },

                // 2. Đề nghị giá NCC đã phản hồi đơn giá (Responded / Priced) — Chờ CVDV/Quản lý duyệt
                new PartPriceRequest
                {
                    ReqPartPriceNo = $"RPP{DateTime.Today:yyMMdd}-002",
                    DealerCode = "HTC-CG",
                    DealerName = "Hyundai Cầu Giấy",
                    Description = "Xin báo giá bơm cao áp nhiên liệu Diesel Common Rail CRDi và cụm van tuần hoàn khí xả EGR xe Tucson máy dầu",
                    TSTReqPartPriceID = "TST-PR-2026-9118",
                    TSTSentDate = DateTime.Today.AddHours(-16),
                    DMSStatus = DMSReqPartPriceStatus.Responded,
                    TSTStatus = TSTReqPartPriceStatus.Priced,
                    FlagIsCheck = false,
                    IsUpdatePrice = false,
                    EffectiveDate = DateTime.Today,
                    EstimatedResponseDate = DateTime.Today,
                    CreatedBy = "CVDV Tuấn Hùng",
                    CreatedAt = DateTime.Today.AddHours(-18),
                    VIN = "RLHXXTC002",
                    CarModel = "Hyundai Tucson 2.0 CRDi",
                    Items = [
                        new PartPriceRequestLine
                        {
                            DMSPartCode = "33100-2F000",
                            VieName = "Bơm cao áp nhiên liệu Common Rail Diesel CRDi",
                            VINCode = "RLHXXTC002",
                            DeliveryForm = PartPriceDeliveryForm.Regular,
                            Quantity = 1,
                            Unit = "Cái",
                            Remark = "Áp suất đường ống rail tụt khi đạp ga tải nặng, báo lỗi P0087",
                            TSTPartCode = "33100-2F000-BVO",
                            TSTPrice = 18200000,
                            DateEffect = DateTime.Today,
                            Status = ReqPartPriceLineStatus.Priced
                        },
                        new PartPriceRequestLine
                        {
                            DMSPartCode = "28410-2F000",
                            VieName = "Cụm van tuần hoàn khí xả điện tử EGR",
                            VINCode = "RLHXXTC002",
                            DeliveryForm = PartPriceDeliveryForm.Regular,
                            Quantity = 1,
                            Unit = "Cụm",
                            Remark = "Kẹt van bám muội carbon không đóng kín buồng đốt",
                            TSTPartCode = "28410-2F000-TST",
                            TSTPrice = 4600000,
                            DateEffect = DateTime.Today,
                            Status = ReqPartPriceLineStatus.Priced
                        }
                    ]
                },

                // 3. Đề nghị giá đã gửi sang NCC đang thẩm định (Sent / Processing)
                new PartPriceRequest
                {
                    ReqPartPriceNo = $"RPP{DateTime.Today:yyMMdd}-003",
                    DealerCode = "HTC-CG",
                    DealerName = "Hyundai Cầu Giấy",
                    Description = "Đề nghị báo giá bộ cảm biến Radar khoảng cách ADAS SmartSense và giá đỡ cản trước xe Hyundai Creta",
                    TSTReqPartPriceID = "TST-PR-2026-9204",
                    TSTSentDate = DateTime.Today.AddHours(-3),
                    DMSStatus = DMSReqPartPriceStatus.Sent,
                    TSTStatus = TSTReqPartPriceStatus.Processing,
                    FlagIsCheck = true,
                    IsUpdatePrice = false,
                    EstimatedResponseDate = DateTime.Today.AddDays(1),
                    CreatedBy = "KTV Đạt",
                    CreatedAt = DateTime.Today.AddHours(-4),
                    VIN = "KMHEC81BPA889900",
                    CarModel = "Hyundai Creta 1.5 Cao Cấp",
                    Items = [
                        new PartPriceRequestLine
                        {
                            DMSPartCode = "96720-BV000",
                            VieName = "Cụm cảm biến Radar sóng milimet cản trước (FCA/SCC)",
                            VINCode = "KMHEC81BPA889900",
                            DeliveryForm = PartPriceDeliveryForm.Air,
                            Quantity = 1,
                            Unit = "Cái",
                            Remark = "Va quệt nứt vỡ mắt radar, hệ thống FCA báo lỗi không nhận diện khoảng cách",
                            TSTPartCode = null,
                            TSTPrice = 0,
                            Status = ReqPartPriceLineStatus.Pending
                        },
                        new PartPriceRequestLine
                        {
                            DMSPartCode = "99211-BV000",
                            VieName = "Giá đỡ & giắc điện cảm biến Radar",
                            VINCode = "KMHEC81BPA889900",
                            DeliveryForm = PartPriceDeliveryForm.Air,
                            Quantity = 1,
                            Unit = "Bộ",
                            Remark = "Gãy chân ngàm cài giá đỡ cảm biến",
                            TSTPartCode = null,
                            TSTPrice = 0,
                            Status = ReqPartPriceLineStatus.Pending
                        }
                    ]
                },

                // 4. Đề nghị mới lập tại xưởng (Draft / Pending)
                new PartPriceRequest
                {
                    ReqPartPriceNo = $"RPP{DateTime.Today:yyMMdd}-004",
                    DealerCode = "HTC-CG",
                    DealerName = "Hyundai Cầu Giấy",
                    Description = "Lập đề nghị báo giá giảm xóc điện tử điều khiển biến thiên ECS và rotuyn thanh cân bằng trước xe Palisade",
                    DMSStatus = DMSReqPartPriceStatus.Draft,
                    TSTStatus = TSTReqPartPriceStatus.Pending,
                    FlagIsCheck = false,
                    IsUpdatePrice = false,
                    CreatedBy = "Thủ kho Tuấn",
                    CreatedAt = DateTime.Today.AddMinutes(-45),
                    VIN = "KMHMU81DPA654321",
                    CarModel = "Hyundai Palisade 3.8 AWD",
                    Items = [
                        new PartPriceRequestLine
                        {
                            DMSPartCode = "54651-S8000",
                            VieName = "Giảm xóc điện tử trước bên phải (ECS Strut FR-RH)",
                            VINCode = "KMHMU81DPA654321",
                            DeliveryForm = PartPriceDeliveryForm.Regular,
                            Quantity = 1,
                            Unit = "Cây",
                            Remark = "Chảy dầu phớt ty giảm xóc, qua gờ giảm tốc có tiếng lọc cọc",
                            TSTPartCode = null,
                            TSTPrice = 0,
                            Status = ReqPartPriceLineStatus.Pending
                        },
                        new PartPriceRequestLine
                        {
                            DMSPartCode = "54830-S8000",
                            VieName = "Rotuyn thanh cân bằng trước (Link Assy-Front)",
                            VINCode = "KMHMU81DPA654321",
                            DeliveryForm = PartPriceDeliveryForm.Regular,
                            Quantity = 2,
                            Unit = "Cái",
                            Remark = "Rách chụp cao su chắn bụi và rơ lỏng khớp cầu",
                            TSTPartCode = null,
                            TSTPrice = 0,
                            Status = ReqPartPriceLineStatus.Pending
                        }
                    ]
                }
            };

            db.PartPriceRequests.AddRange(requests);
            await db.SaveChangesAsync();
        }

        if (!await db.ComplaintDiagnosticErrors.AnyAsync())
        {
            var errors = new List<ComplaintDiagnosticError>
            {
                // 1. Phàn nàn của khách hàng (PN) - Động cơ & Nhiên liệu
                new()
                {
                    ErrorCode = "PN-ENG-01",
                    ErrorName = "Động cơ rung giật khi nổ cầm chừng (Idle Vibration)",
                    ErrorType = ComplaintErrorType.Complaint,
                    SystemGroup = VehicleSystemGroup.Engine,
                    ErrorDesc = "Khách hàng phàn nàn xe rung giật mạnh ở vô lăng và cần số khi dừng đèn đỏ nổ không tải (garanti), vòng tua máy không đều dao động từ 600 - 900 RPM.",
                    Remark = "Kiểm tra họng hút, van không tải ISC, bugi đánh lửa, bô-bin và cao su chân máy/chân số giảm chấn.",
                    FlagActive = true,
                    UsageCount = 14,
                    CreatedBy = "Quản đốc xưởng"
                },
                new()
                {
                    ErrorCode = "PN-ENG-02",
                    ErrorName = "Động cơ khó khởi động buổi sáng hoặc để qua đêm",
                    ErrorType = ComplaintErrorType.Complaint,
                    SystemGroup = VehicleSystemGroup.Engine,
                    ErrorDesc = "Xe đề dai, quay máy lâu mới nổ hoặc phải đề 2-3 lần mới khởi động được, máy nổ lịm rồi mới lên ga.",
                    Remark = "Đo áp suất bơm xăng trong bình (chuẩn 3.5 - 4.0 bar), kiểm tra van một chiều bơm xăng, lọc xăng bẩn và ắc quy sụt áp khi đề.",
                    FlagActive = true,
                    UsageCount = 9,
                    CreatedBy = "Quản đốc xưởng"
                },
                new()
                {
                    ErrorCode = "PN-ENG-03",
                    ErrorName = "Máy bị hụt ga, ì máy khi tăng tốc vượt dốc",
                    ErrorType = ComplaintErrorType.Complaint,
                    SystemGroup = VehicleSystemGroup.Engine,
                    ErrorDesc = "Đạp thốc chân ga để vượt xe hoặc lên dốc xe bị trễ 2-3 giây, nghe tiếng gõ rốc máy nhẹ ở khoang động cơ rồi mới tăng tốc từ từ.",
                    Remark = "Kiểm tra cảm biến lưu lượng gió MAF/MAP, kim phun nhiên liệu bị nghẹt đầu phun, bướm ga điện tử ETC và chất lượng nhiên liệu.",
                    FlagActive = true,
                    UsageCount = 11,
                    CreatedBy = "CVDV Tuấn"
                },

                // 2. Phàn nàn của khách hàng (PN) - Hộp số
                new()
                {
                    ErrorCode = "PN-TRANS-01",
                    ErrorName = "Hộp số bị giật cục khi chuyển số N sang D hoặc sang R",
                    ErrorType = ComplaintErrorType.Complaint,
                    SystemGroup = VehicleSystemGroup.Transmission,
                    ErrorDesc = "Khi dừng xe gạt cần số từ vị trí N sang D hoặc lùi R nghe tiếng cạch và cả xe giật nảy người, đặc biệt khi máy đang nguội.",
                    Remark = "Kiểm tra mức dầu và phẩm cấp dầu hộp số tự động ATF, cập nhật phần mềm hộp số TCM, kiểm tra vỉ van điện tử và cao su chân hộp số.",
                    FlagActive = true,
                    UsageCount = 8,
                    CreatedBy = "Quản đốc xưởng"
                },

                // 3. Phàn nàn của khách hàng (PN) - Khung gầm & Phanh/Lái
                new()
                {
                    ErrorCode = "PN-CHAS-01",
                    ErrorName = "Tiếng kêu lục cục ở gầm trước khi qua gờ giảm tốc",
                    ErrorType = ComplaintErrorType.Complaint,
                    SystemGroup = VehicleSystemGroup.Chassis,
                    ErrorDesc = "Xe đi qua gờ giảm tốc, ổ gà hoặc đường mấp mô phát ra tiếng kêu lục cục, cọc cọc ở khu vực bánh trước bên phụ.",
                    Remark = "Kiểm tra rotuyn thanh cân bằng trước, cao su càng A, giảm xóc trước có hiện tượng chảy dầu phớt ty và bát bèo đầu giảm xóc.",
                    FlagActive = true,
                    UsageCount = 18,
                    CreatedBy = "CVDV Hoàng"
                },
                new()
                {
                    ErrorCode = "PN-CHAS-02",
                    ErrorName = "Vô lăng bị nhao lái lệch sang phải khi buông tay",
                    ErrorType = ComplaintErrorType.Complaint,
                    SystemGroup = VehicleSystemGroup.Chassis,
                    ErrorDesc = "Chạy thẳng trên đường phẳng tốc độ 50-80 km/h, thả nhẹ tay lái thì xe tự động dạt về lề đường bên phải sau 3-5 giây.",
                    Remark = "Kiểm tra độ chụm bánh xe và góc đặt bánh xe trên máy cân chỉnh Hunter 3D, kiểm tra độ mòn lệch hoa lốp và áp suất 4 lốp.",
                    FlagActive = true,
                    UsageCount = 12,
                    CreatedBy = "Quản đốc xưởng"
                },

                // 4. Phàn nàn của khách hàng (PN) - Điện & Điều hòa & Thân vỏ
                new()
                {
                    ErrorCode = "PN-ELEC-01",
                    ErrorName = "Đèn cảnh báo kiểm tra động cơ Check Engine sáng màu vàng",
                    ErrorType = ComplaintErrorType.Complaint,
                    SystemGroup = VehicleSystemGroup.Electrical,
                    ErrorDesc = "Đèn cảnh báo hình động cơ (cá vàng) sáng liên tục trên màn hình taplo kể từ khi khởi động, xe vẫn vận hành được bình thường.",
                    Remark = "Cắm máy chẩn đoán GDS Mobile đọc mã lỗi DTC được lưu trong bộ nhớ ECU để xác định chính xác nguyên nhân.",
                    FlagActive = true,
                    UsageCount = 25,
                    CreatedBy = "CVDV Tuấn"
                },
                new()
                {
                    ErrorCode = "PN-HVAC-01",
                    ErrorName = "Điều hòa không mát sâu, phả hơi nóng khi dừng đỗ garanti",
                    ErrorType = ComplaintErrorType.Complaint,
                    SystemGroup = VehicleSystemGroup.HVAC,
                    ErrorDesc = "Xe chạy nhanh trên cao tốc gió lạnh sâu, nhưng khi dừng đỗ đèn đỏ hoặc tắc đường giữa trưa nắng thì cửa gió thổi ra hơi nóng ẩm.",
                    Remark = "Kiểm tra quạt giải nhiệt két làm mát giàn nóng, đo áp suất gas lạnh đường cao áp/hạ áp, vệ sinh bề mặt giàn nóng két nước.",
                    FlagActive = true,
                    UsageCount = 16,
                    CreatedBy = "CVDV Lan"
                },
                new()
                {
                    ErrorCode = "PN-BODY-01",
                    ErrorName = "Cửa kính bên lái bị kẹt, phát tiếng rít ken két khi lên xuống",
                    ErrorType = ComplaintErrorType.Complaint,
                    SystemGroup = VehicleSystemGroup.BodyPaint,
                    ErrorDesc = "Bấm công tắc kính cửa sổ tài xế kính di chuyển giật cục, chậm chạp và phát ra tiếng rít ken két khó chịu, thỉnh thoảng tụt kẹt giữa chừng.",
                    Remark = "Vệ sinh bôi trơn rãnh gioăng dẫn hướng kính bằng xịt silicon chuyên dụng, kiểm tra dây cáp và mô tơ nâng hạ kính cửa xe.",
                    FlagActive = true,
                    UsageCount = 7,
                    CreatedBy = "CVDV Thắng"
                },

                // 5. Chuẩn đoán kỹ thuật viên (CD) - Động cơ & Nhiên liệu
                new()
                {
                    ErrorCode = "CD-P0300",
                    ErrorName = "Mã DTC P0300 - Phát hiện bỏ lửa ngẫu nhiên nhiều xi-lanh",
                    ErrorType = ComplaintErrorType.Diagnostic,
                    SystemGroup = VehicleSystemGroup.Engine,
                    ErrorDesc = "ECU động cơ ghi nhận xung gia tốc trục khuỷu CKP không đồng đều trên nhiều xi-lanh liên tiếp. Hiện tượng kèm theo rung giật khi tăng ga.",
                    Remark = "Thay thế bộ bugi Iridium theo định kỳ, kiểm tra điện áp kích mở bô-bin đánh lửa, đo áp suất buồng đốt và súc rửa kim phun nhiên liệu.",
                    FlagActive = true,
                    UsageCount = 15,
                    CreatedBy = "KTV Đức"
                },
                new()
                {
                    ErrorCode = "CD-P0171",
                    ErrorName = "Mã DTC P0171 - Hòa khí quá nghèo dãy xi-lanh 1 (System Too Lean)",
                    ErrorType = ComplaintErrorType.Diagnostic,
                    SystemGroup = VehicleSystemGroup.Engine,
                    ErrorDesc = "Cảm biến oxy băng rộng (Air-Fuel Ratio Sensor) trước bầu xúc tác báo tỷ lệ hòa khí thiếu xăng kéo dài, chỉ số Short/Long Term Fuel Trim > +20%.",
                    Remark = "Thử khói phát hiện rò rỉ đường ống hút chân không sau bướm ga, kiểm tra áp suất bơm xăng, làm sạch cảm biến lưu lượng gió MAF.",
                    FlagActive = true,
                    UsageCount = 10,
                    CreatedBy = "KTV Quang"
                },
                new()
                {
                    ErrorCode = "CD-P0420",
                    ErrorName = "Mã DTC P0420 - Hiệu suất bộ chuyển đổi xúc tác thấp hơn ngưỡng",
                    ErrorType = ComplaintErrorType.Diagnostic,
                    SystemGroup = VehicleSystemGroup.Engine,
                    ErrorDesc = "Tín hiệu dao động điện áp cảm biến oxy số 2 (sau bộ xúc tác) bám sát theo cảm biến số 1, chứng tỏ khả năng lưu trữ oxy của tổ ong xúc tác suy giảm.",
                    Remark = "Kiểm tra tổ ong bầu xúc tác có bị vỡ nứt hoặc nhiễm bẩn dầu nhớt, kiểm tra rò rỉ khí xả cổ góp trước khi đề xuất thay cụm bầu xúc tác.",
                    FlagActive = true,
                    UsageCount = 6,
                    CreatedBy = "KTV Đức"
                },

                // 6. Chuẩn đoán kỹ thuật viên (CD) - Hộp số & Gầm phanh & Điện & Điều hòa
                new()
                {
                    ErrorCode = "CD-P0700",
                    ErrorName = "Mã DTC P0700 - Lỗi hệ thống điều khiển hộp số tự động TCM",
                    ErrorType = ComplaintErrorType.Diagnostic,
                    SystemGroup = VehicleSystemGroup.Transmission,
                    ErrorDesc = "Hộp điều khiển hộp số TCM gửi tín hiệu MIL Request yêu cầu ECM bật đèn Check Engine do phát hiện trượt ly hợp hoặc lỗi van điện từ.",
                    Remark = "Dùng máy chẩn đoán GDS truy cập module TCM đọc các mã lỗi con (P0731, P0741...), đo điện trở các cụm Solenoid van dầu và kiểm tra mạt kim loại trong đáy các-te dầu số.",
                    FlagActive = true,
                    UsageCount = 5,
                    CreatedBy = "KTV Đức"
                },
                new()
                {
                    ErrorCode = "CD-C1201",
                    ErrorName = "Mã DTC C1201 - Lỗi tín hiệu cảm biến tốc độ bánh xe trước bên lái",
                    ErrorType = ComplaintErrorType.Diagnostic,
                    SystemGroup = VehicleSystemGroup.Chassis,
                    ErrorDesc = "Hộp điều khiển phanh ABS/ESC không nhận được tín hiệu xung điện từ cảm biến Wheel Speed Sensor bánh trước trái khi xe di chuyển > 10 km/h.",
                    Remark = "Đo điện trở cuộn dây cảm biến tốc độ bánh xe (chuẩn 1.0 - 1.3 kOhm), kiểm tra khe hở từ tính và vệ sinh mạt sắt bám trên vành răng xung ABS.",
                    FlagActive = true,
                    UsageCount = 13,
                    CreatedBy = "KTV Trọng"
                },
                new()
                {
                    ErrorCode = "CD-B1346",
                    ErrorName = "Mã DTC B1346 - Điện trở ngòi nổ túi khí vô lăng cao ngoài giới hạn",
                    ErrorType = ComplaintErrorType.Diagnostic,
                    SystemGroup = VehicleSystemGroup.Electrical,
                    ErrorDesc = "Hộp điều khiển túi khí SRSCM báo điện trở mạch ngòi nổ túi khí người lái (DAB) > 4.2 Ohm (chuẩn 1.8 - 3.2 Ohm), đèn túi khí Airbag sáng đỏ.",
                    Remark = "Đo thông mạch cáp cuộn còi vô lăng (Clock Spring) khi xoay hết lái trái/phải để phát hiện đứt ngậm dây cáp bẹ, thay cuộn cáp còi mới.",
                    FlagActive = true,
                    UsageCount = 8,
                    CreatedBy = "KTV Nam"
                },
                new()
                {
                    ErrorCode = "CD-HVAC-LEAK",
                    ErrorName = "Chẩn đoán xì rò rỉ môi chất lạnh gas R134a tại dàn lạnh điều hòa",
                    ErrorType = ComplaintErrorType.Diagnostic,
                    SystemGroup = VehicleSystemGroup.HVAC,
                    ErrorDesc = "Nén khí nitơ áp suất 250 psi thử kín hệ thống lạnh phát hiện tụt áp 15 psi sau 30 phút. Dùng camera nội soi và đèn cực tím soi thấy vết dầu nhuộm huỳnh quang rò rỉ ở góc đáy dàn lạnh.",
                    Remark = "Tháo taplo hạ cụm hộp gió điều hòa để thay thế giàn lạnh (Evaporator Core) mới chính hãng, thay phin lọc ga và hút chân không nạp lại 500g gas R134a chuẩn.",
                    FlagActive = true,
                    UsageCount = 12,
                    CreatedBy = "KTV Quốc Hưng"
                }
            };

            db.ComplaintDiagnosticErrors.AddRange(errors);
            await db.SaveChangesAsync();
        }

        if (!await db.CustomerCare72hs.AnyAsync())
        {
            var finishedROs = await db.ROs.Include(r => r.Car).Include(r => r.Customer).Where(r => r.Status == ROStatus.Finished || r.Status == ROStatus.Paid || r.Status == ROStatus.Repaired || r.Status == ROStatus.InGarage).ToListAsync();
            var car1 = await db.Cars.Include(c => c.Customer).FirstOrDefaultAsync(c => c.Plate == "30A-123.45") ?? await db.Cars.Include(c => c.Customer).FirstAsync();
            var car2 = await db.Cars.Include(c => c.Customer).FirstOrDefaultAsync(c => c.Plate == "51G-678.90") ?? await db.Cars.Include(c => c.Customer).LastAsync();

            var ro1 = finishedROs.FirstOrDefault(r => r.CarId == car1.Id) ?? finishedROs.FirstOrDefault();
            var ro2 = finishedROs.FirstOrDefault(r => r.CarId == car2.Id) ?? (finishedROs.Count > 1 ? finishedROs[1] : ro1);

            if (ro1 != null)
            {
                // 1. Khảo sát 72h hài lòng (CIFB) - Đạt chuẩn FIRFT
                var care1 = new CustomerCare72h
                {
                    Care72No = "CC72-260424-001",
                    ROId = ro1.Id,
                    CarId = ro1.CarId,
                    CustomerId = ro1.CustomerId,
                    Status = CustomerCare72hStatus.ContactedSatisfied,
                    ROFinishedDate = DateTime.Today.AddDays(-4),
                    ScheduledDate = DateTime.Today.AddDays(-1),
                    ContactedDate = DateTime.Today.AddDays(-1).AddHours(10),
                    ContactedBy = "CSKH - Thanh Hằng",
                    ServiceExplained = true,
                    BasicNeedsMet = true,
                    HasTechnicalProblem = false,
                    FixedRightFirstTime = true,
                    SatisfactionRating = 5,
                    CustomerFeedback = "Xe chạy rất bốc và êm, CVDV giải thích tận tình. Rất tin tưởng xưởng dịch vụ Hyundai.",
                    ReRepairAction = "Đã gửi tin nhắn cảm ơn và tặng mã voucher rửa xe miễn phí lần tới.",
                    InternalNote = "Khách hàng thân thiết đánh giá rất tốt. Đã ghi nhận điểm CSI 5/5.",
                    CreatedBy = "system"
                };
                db.CustomerCare72hs.Add(care1);
            }

            if (ro2 != null)
            {
                // 2. Báo động phản tu (CINFB / Re-Repair Alert) - Xe phát sinh lỗi sau 72h lăn bánh
                var care2 = new CustomerCare72h
                {
                    Care72No = "CC72-260425-002",
                    ROId = ro2.Id,
                    CarId = ro2.CarId,
                    CustomerId = ro2.CustomerId,
                    Status = CustomerCare72hStatus.NeedFeedback,
                    ROFinishedDate = DateTime.Today.AddDays(-3),
                    ScheduledDate = DateTime.Today,
                    ContactedDate = DateTime.Today.AddHours(-2),
                    ContactedBy = "CSKH - Minh Thư",
                    ServiceExplained = true,
                    BasicNeedsMet = true,
                    HasTechnicalProblem = true,
                    ProblemDetails = "Khi đánh lái cua sang phải ở dải tốc độ 20-30 km/h nghe tiếng kêu 'lục cục' bất thường phía trước gầm phụ.",
                    FixedRightFirstTime = false,
                    SatisfactionRating = 2,
                    CustomerFeedback = "Hôm trước lấy xe về đi làm việc bận chưa thử hết, nay đi qua gờ giảm tốc và cua phải thấy kêu rõ. Mong xưởng kiểm tra lại giúp.",
                    IsReRepairAlert = true,
                    ReRepairAction = "Đã thông báo Quản đốc xưởng và Cố vấn dịch vụ. Hẹn đón xe vào khoang kiểm tra ưu tiên miễn phí 100%.",
                    InternalNote = "CẢNH BÁO PHẢN TU: Kiểm tra lại bạc cân bằng và rotuyn lái phụ đã thay trên RO gốc.",
                    CreatedBy = "system"
                };
                db.CustomerCare72hs.Add(care2);

                // Cập nhật cờ phản tu trên RO gốc
                ro2.IsReRepair = true;
            }

            // 3. Phiếu CSKH 72h đang chờ liên hệ (PEND)
            var otherRO = await db.ROs.Include(r => r.Car).Include(r => r.Customer).OrderByDescending(r => r.Id).FirstOrDefaultAsync(r => (ro1 == null || r.Id != ro1.Id) && (ro2 == null || r.Id != ro2.Id));
            if (otherRO != null)
            {
                var care3 = new CustomerCare72h
                {
                    Care72No = "CC72-260427-003",
                    ROId = otherRO.Id,
                    CarId = otherRO.CarId,
                    CustomerId = otherRO.CustomerId,
                    Status = CustomerCare72hStatus.Pending,
                    ROFinishedDate = DateTime.Today.AddDays(-1),
                    ScheduledDate = DateTime.Today.AddDays(2),
                    ContactedDate = null,
                    ContactedBy = null,
                    ServiceExplained = null,
                    BasicNeedsMet = null,
                    HasTechnicalProblem = false,
                    FixedRightFirstTime = null,
                    SatisfactionRating = null,
                    CustomerFeedback = null,
                    IsReRepairAlert = false,
                    InternalNote = "Xe vừa rời xưởng ngày hôm qua. Dự kiến thực hiện cuộc gọi khảo sát sau 2 ngày tới.",
                    CreatedBy = "system"
                };
                db.CustomerCare72hs.Add(care3);
            }

            await db.SaveChangesAsync();
        }

        // --- Chăm sóc sinh nhật khách hàng & Voucher tri ân (Ser_CustomerCareBth / FrmCSCCustomerCareDOB) ---
        var existingCustomers = await db.Customers.Include(c => c.Cars).ToListAsync();
        if (existingCustomers.Count > 0)
        {
            var today = DateTime.Today;
            var c0 = existingCustomers[0];
            if (!c0.DateOfBirth.HasValue) c0.DateOfBirth = new DateTime(1988, today.Month, today.Day); // Sinh nhật HÔM NAY!
            if (string.IsNullOrEmpty(c0.Address)) c0.Address = "128 Cầu Giấy, Hà Nội";
            if (string.IsNullOrEmpty(c0.Gender)) c0.Gender = "Nam";

            if (existingCustomers.Count > 1)
            {
                var c1 = existingCustomers[1];
                if (!c1.DateOfBirth.HasValue) c1.DateOfBirth = new DateTime(1992, today.Month, Math.Min(28, DateTime.DaysInMonth(today.Year, today.Month))); // Sinh nhật trong tháng này
                if (string.IsNullOrEmpty(c1.Address)) c1.Address = "45 Nguyễn Trãi, Thanh Xuân, Hà Nội";
                if (string.IsNullOrEmpty(c1.Gender)) c1.Gender = "Nữ";
            }

            // Khách hàng đặc biệt sinh nhật ngày nhuận 29/02 để kiểm chứng luật tính năm thường (28/02)
            var leapCus = existingCustomers.FirstOrDefault(c => c.Code == "KH0009" || (c.DateOfBirth.HasValue && c.DateOfBirth.Value.Month == 2 && c.DateOfBirth.Value.Day == 29));
            if (leapCus == null)
            {
                leapCus = new Customer
                {
                    Code = "KH0009",
                    Name = "Lê Hoàng Phúc",
                    Phone = "0908290290",
                    Email = "phuc.lh@gmail.com",
                    DateOfBirth = new DateTime(1996, 2, 29), // Sinh ngày 29/02 năm nhuận
                    Address = "72 Lê Văn Lương, Hà Nội",
                    Gender = "Nam",
                    Cars = [new Car { Plate = "30H-888.29", Model = "Hyundai Santa Fe 2.5 HTRAC", Year = 2024, Vin = "RLHXXSF2902" }]
                };
                db.Customers.Add(leapCus);
            }

            // Khách hàng sinh nhật tháng tiếp theo
            var nextMonthCus = existingCustomers.FirstOrDefault(c => c.Code == "KH0010");
            if (nextMonthCus == null)
            {
                var nm = today.Month == 12 ? 1 : today.Month + 1;
                nextMonthCus = new Customer
                {
                    Code = "KH0010",
                    Name = "Phạm Quỳnh Chi",
                    Phone = "0912345678",
                    Email = "chi.pq@gmail.com",
                    DateOfBirth = new DateTime(1995, nm, 15),
                    Address = "15 Hoàng Đạo Thúy, Cầu Giấy, Hà Nội",
                    Gender = "Nữ",
                    Cars = [new Car { Plate = "30F-999.55", Model = "Hyundai Creta 1.5 Cao Cấp", Year = 2023, Vin = "RLHXXCR999" }]
                };
                db.Customers.Add(nextMonthCus);
            }

            await db.SaveChangesAsync();
        }

        if (!await db.CustomerCareBirthdays.AnyAsync())
        {
            var allCus = await db.Customers.Include(c => c.Cars).ToListAsync();
            var today = DateTime.Today;
            var targetYear = today.Year;

            // 1. Khách hàng sinh nhật HÔM NAY (Pending - Chờ CSKH gọi chúc mừng & tặng voucher)
            var cToday = allCus.FirstOrDefault(c => c.DateOfBirth.HasValue && c.DateOfBirth.Value.Month == today.Month && c.DateOfBirth.Value.Day == today.Day)
                         ?? allCus.FirstOrDefault();
            if (cToday != null)
            {
                var care1 = new CustomerCareBirthday
                {
                    CareBthNo = $"BTH{today:yyMMdd}-001",
                    CustomerId = cToday.Id,
                    CarId = cToday.Cars.FirstOrDefault()?.Id,
                    DateOfBirth = cToday.DateOfBirth ?? new DateTime(1988, today.Month, today.Day),
                    DateBth = new DateTime(targetYear, today.Month, today.Day),
                    Status = CustomerCareBirthdayStatus.Pending,
                    GiftVoucherCode = $"BDAY{targetYear}-{cToday.Code}",
                    GiftVoucherValue = 500_000m,
                    DiscountPercent = 15m,
                    VoucherValidUntil = new DateTime(targetYear, today.Month, DateTime.DaysInMonth(targetYear, today.Month)).AddDays(30),
                    IsVoucherUsed = false,
                    Remark = "Khách hàng thân thiết. Hôm nay sinh nhật, đề xuất tặng thêm áo mưa cao cấp Hyundai và voucher giảm 15% tiền công bảo dưỡng.",
                    CreatedBy = "system"
                };
                db.CustomerCareBirthdays.Add(care1);
            }

            // 2. Khách hàng sinh nhật trong THÁNG NÀY (ĐÃ LIÊN HỆ - CONTACTED - Đã tặng voucher & hẹn mang xe)
            if (allCus.Count > 1)
            {
                var cContacted = allCus[1];
                var bthMonth = today.Month;
                var bthDay = Math.Max(1, today.Day - 3);
                var care2 = new CustomerCareBirthday
                {
                    CareBthNo = $"BTH{today:yyMMdd}-002",
                    CustomerId = cContacted.Id,
                    CarId = cContacted.Cars.FirstOrDefault()?.Id,
                    DateOfBirth = cContacted.DateOfBirth ?? new DateTime(1992, bthMonth, bthDay),
                    DateBth = new DateTime(targetYear, bthMonth, bthDay),
                    Status = CustomerCareBirthdayStatus.Contacted,
                    ContactDate = today.AddDays(-1).AddHours(14),
                    ContactedBy = "CSKH - Minh Thư",
                    ContactChannel = BirthdayContactChannel.Call,
                    GiftVoucherCode = $"BDAY{targetYear}-{cContacted.Code}",
                    GiftVoucherValue = 300_000m,
                    DiscountPercent = 10m,
                    VoucherValidUntil = new DateTime(targetYear, bthMonth, DateTime.DaysInMonth(targetYear, bthMonth)).AddDays(30),
                    IsVoucherUsed = false,
                    Remark = "Đã gọi điện chúc mừng sinh nhật chị. Chị rất hài lòng và hào hứng nhận voucher dịch vụ. Đã hẹn mang xe tới bảo dưỡng cuối tuần.",
                    CreatedBy = "system"
                };
                db.CustomerCareBirthdays.Add(care2);
            }

            // 3. Khách hàng sinh nhật 29/02 (Năm thường tính 28/02 - Áp dụng luật ngày nhuận)
            var cLeap = allCus.FirstOrDefault(c => c.DateOfBirth.HasValue && c.DateOfBirth.Value.Month == 2 && c.DateOfBirth.Value.Day == 29);
            if (cLeap != null)
            {
                var dateBth = CustomerCareBirthday.CalculateDateBth(cLeap.DateOfBirth!.Value, targetYear);
                var care3 = new CustomerCareBirthday
                {
                    CareBthNo = $"BTH{today:yyMMdd}-003",
                    CustomerId = cLeap.Id,
                    CarId = cLeap.Cars.FirstOrDefault()?.Id,
                    DateOfBirth = cLeap.DateOfBirth,
                    DateBth = dateBth, // 28/02 if not leap year!
                    Status = CustomerCareBirthdayStatus.Contacted,
                    ContactDate = new DateTime(targetYear, 2, 28, 10, 30, 0),
                    ContactedBy = "CSKH - Thanh Hằng",
                    ContactChannel = BirthdayContactChannel.Zalo,
                    GiftVoucherCode = $"BDAY{targetYear}-{cLeap.Code}",
                    GiftVoucherValue = 400_000m,
                    DiscountPercent = 10m,
                    VoucherValidUntil = new DateTime(targetYear, 3, 31),
                    IsVoucherUsed = true, // Demo voucher đã áp dụng
                    Remark = "Sinh nhật ngày nhuận 29/02 đặc biệt (chúc mừng ngày 28/02). Đã gửi thiệp điện tử Zalo ZNS và khách đã sử dụng voucher khi làm dịch vụ.",
                    CreatedBy = "system"
                };
                db.CustomerCareBirthdays.Add(care3);
            }

            // 4. Khách hàng gọi chưa nghe máy (NotContacted)
            var cNotContact = allCus.LastOrDefault(c => c.Id != cToday?.Id && c.Id != cLeap?.Id);
            if (cNotContact != null)
            {
                var care4 = new CustomerCareBirthday
                {
                    CareBthNo = $"BTH{today:yyMMdd}-004",
                    CustomerId = cNotContact.Id,
                    CarId = cNotContact.Cars.FirstOrDefault()?.Id,
                    DateOfBirth = cNotContact.DateOfBirth ?? new DateTime(1990, today.Month, Math.Min(25, DateTime.DaysInMonth(targetYear, today.Month))),
                    DateBth = CustomerCareBirthday.CalculateDateBth(cNotContact.DateOfBirth ?? new DateTime(1990, today.Month, Math.Min(25, DateTime.DaysInMonth(targetYear, today.Month))), targetYear),
                    Status = CustomerCareBirthdayStatus.NotContacted,
                    ContactDate = today.AddHours(-3),
                    ContactedBy = "CSKH - Minh Thư",
                    ContactChannel = BirthdayContactChannel.Call,
                    GiftVoucherCode = $"BDAY{targetYear}-{cNotContact.Code}",
                    GiftVoucherValue = 300_000m,
                    DiscountPercent = 10m,
                    VoucherValidUntil = new DateTime(targetYear, today.Month, DateTime.DaysInMonth(targetYear, today.Month)).AddDays(30),
                    IsVoucherUsed = false,
                    Remark = "Gọi 2 cuộc khách không nghe máy (máy bận). Sẽ liên hệ lại vào buổi chiều hoặc gửi tin nhắn SMS chúc mừng kèm voucher.",
                    CreatedBy = "system"
                };
                db.CustomerCareBirthdays.Add(care4);
            }

            await db.SaveChangesAsync();
        }

        // Seed Định mức giờ công bảo hành tiêu chuẩn Flat Rate (Ser_MST_ROWarrantyWork & Ser_MST_ROWarrantyType)
        if (!await db.WarrantyWorks.AnyAsync())
        {
            var warrantyWorks = new List<WarrantyWork>
            {
                new()
                {
                    Code = "WRT-ENG-SF01",
                    Name = "Thay thế phớt đuôi trục khuỷu & hạ ráp hộp số tự động",
                    Model = "SantaFe",
                    LaborGroup = WarrantyLaborGroup.Engine,
                    CoverageType = WarrantyCoverageType.NewCar,
                    AppTypeCode = "HTC-W-ENG-01",
                    EngineType = "SmartStream D2.2 CRDi",
                    RateHour = 4.5m,
                    RatePrice = 320_000m,
                    Price = 1_440_000m,
                    VatPercent = 8,
                    RequiredPhotos = "1. Ảnh chụp đồng hồ ODO; 2. Ảnh số khung VIN kính lái; 3. Ảnh rò rỉ dầu phớt đuôi trục khuỷu khi hạ hộp số; 4. Ảnh phớt mới chính hãng đã lắp ráp hoàn thiện.",
                    Remark = "Áp dụng cho dòng SantaFe máy dầu thế hệ mới TM/MX5. Cần dùng dụng cụ chuyên dụng SST ép phớt trục khuỷu.",
                    FlagActive = true,
                    CreatedBy = "Hãng HTC"
                },
                new()
                {
                    Code = "WRT-TRN-SF02",
                    Name = "Thay cụm van điều khiển thủy lực hộp số tự động 8 cấp (Valve Body)",
                    Model = "SantaFe",
                    LaborGroup = WarrantyLaborGroup.Transmission,
                    CoverageType = WarrantyCoverageType.NewCar,
                    AppTypeCode = "HTC-W-TRN-08",
                    EngineType = "SmartStream D2.2 / G2.5",
                    RateHour = 3.2m,
                    RatePrice = 320_000m,
                    Price = 1_024_000m,
                    VatPercent = 8,
                    RequiredPhotos = "1. Ảnh ODO & VIN; 2. Ảnh chụp mã lỗi quét máy chẩn đoán GDS Mobile; 3. Ảnh cụm Valve Body tháo rời; 4. Ảnh tem phụ tùng mới Mobis.",
                    Remark = "Cần xả dầu hộp số ATF SP-IV-RR và châm mới, thực hiện quy trình cài đặt lại điểm thích ứng hộp số (Adaptation Reset).",
                    FlagActive = true,
                    CreatedBy = "Hãng HTC"
                },
                new()
                {
                    Code = "WRT-ELC-SF03",
                    Name = "Cân chỉnh cụm Radar trước & Camera kính lái hệ thống an toàn Hyundai SmartSense",
                    Model = "SantaFe",
                    LaborGroup = WarrantyLaborGroup.Electrical,
                    CoverageType = WarrantyCoverageType.CampaignRecall,
                    AppTypeCode = "HTC-W-ELC-ADAS",
                    EngineType = "Tất cả phiên bản",
                    RateHour = 1.8m,
                    RatePrice = 350_000m,
                    Price = 630_000m,
                    VatPercent = 8,
                    RequiredPhotos = "1. Ảnh ODO & VIN; 2. Ảnh bố trí bia ngắm cân chỉnh ADAS Target Board; 3. Ảnh màn hình GDS báo kết quả Calibrate Passed.",
                    Remark = "Yêu cầu thực hiện tại khoang căn chỉnh có mặt phẳng tiêu chuẩn, khoảng cách bia ngắm theo Shop Manual.",
                    FlagActive = true,
                    CreatedBy = "Hãng HTC"
                },
                new()
                {
                    Code = "WRT-ENG-TU01",
                    Name = "Thay bơm nước làm mát điện tử & van hằng nhiệt điện tử tích hợp",
                    Model = "Tucson",
                    LaborGroup = WarrantyLaborGroup.Engine,
                    CoverageType = WarrantyCoverageType.NewCar,
                    AppTypeCode = "HTC-W-ENG-16T",
                    EngineType = "SmartStream 1.6 T-GDI",
                    RateHour = 2.4m,
                    RatePrice = 320_000m,
                    Price = 768_000m,
                    VatPercent = 8,
                    RequiredPhotos = "1. Ảnh ODO & VIN; 2. Ảnh rò rỉ dung dịch làm mát hoặc mã lỗi P2681; 3. Ảnh bơm nước mới và số part number.",
                    Remark = "Xả gió hệ thống làm mát bằng máy hút chân không chuyên dụng, châm nước làm mát Hyundai Long Life Coolant.",
                    FlagActive = true,
                    CreatedBy = "Hãng HTC"
                },
                new()
                {
                    Code = "WRT-CHAS-TU02",
                    Name = "Thay thước lái trợ lực điện C-MDPS & căn chỉnh góc đặt bánh xe 3D",
                    Model = "Tucson",
                    LaborGroup = WarrantyLaborGroup.ChassisSuspension,
                    CoverageType = WarrantyCoverageType.NewCar,
                    AppTypeCode = "HTC-W-CHAS-MDPS",
                    EngineType = "Tất cả phiên bản",
                    RateHour = 2.8m,
                    RatePrice = 300_000m,
                    Price = 840_000m,
                    VatPercent = 8,
                    RequiredPhotos = "1. Ảnh ODO & VIN; 2. Ảnh thước lái rơ lắc / chảy dầu chụp bụi; 3. Ảnh kết quả đo góc đặt bánh xe trước và sau căn chỉnh.",
                    Remark = "Cài đặt cảm biến góc lái SAS (Steering Angle Sensor) về 0 độ sau khi hoàn thành.",
                    FlagActive = true,
                    CreatedBy = "Hãng HTC"
                },
                new()
                {
                    Code = "WRT-ENG-CR01",
                    Name = "Thay cụm bu-gi, mô-bin đánh lửa và gioăng nắp giàn cò động cơ",
                    Model = "Creta",
                    LaborGroup = WarrantyLaborGroup.Engine,
                    CoverageType = WarrantyCoverageType.GenuinePart,
                    AppTypeCode = "HTC-W-ENG-IGN",
                    EngineType = "SmartStream 1.5 MPI",
                    RateHour = 1.2m,
                    RatePrice = 300_000m,
                    Price = 360_000m,
                    VatPercent = 8,
                    RequiredPhotos = "1. Ảnh ODO & VIN; 2. Ảnh bugi bám muội đen / mô-bin nứt vỏ; 3. Ảnh gioăng mới lắp chuẩn lực siết.",
                    Remark = "Siết bu-lông nắp giàn cò theo đúng sơ đồ chữ thập lực 9.8 - 11.8 Nm.",
                    FlagActive = true,
                    CreatedBy = "Hãng HTC"
                },
                new()
                {
                    Code = "WRT-BRK-CR02",
                    Name = "Thay mô-tơ chấp hành phanh tay điện tử EPB bánh sau trái/phải",
                    Model = "Creta",
                    LaborGroup = WarrantyLaborGroup.BrakeSteering,
                    CoverageType = WarrantyCoverageType.NewCar,
                    AppTypeCode = "HTC-W-BRK-EPB",
                    EngineType = "SmartStream 1.5 MPI",
                    RateHour = 1.6m,
                    RatePrice = 300_000m,
                    Price = 480_000m,
                    VatPercent = 8,
                    RequiredPhotos = "1. Ảnh ODO & VIN; 2. Ảnh đèn báo lỗi EPB sáng taplo; 3. Ảnh mô tơ nứt vỏ / bó kẹt cơ cấu; 4. Ảnh cụm mới.",
                    Remark = "Thực hiện lệnh nhả phanh bảo dưỡng Brake Pad Replacement Mode bằng máy chẩn đoán trước khi tháo.",
                    FlagActive = true,
                    CreatedBy = "Hãng HTC"
                },
                new()
                {
                    Code = "WRT-ENG-AC01",
                    Name = "Lập trình cập nhật ECU động cơ khắc phục rung giật ga đầu & vệ sinh họng hút",
                    Model = "Accent",
                    LaborGroup = WarrantyLaborGroup.SoftwareECU,
                    CoverageType = WarrantyCoverageType.CampaignRecall,
                    AppTypeCode = "HTC-W-ECU-ACC01",
                    EngineType = "Kappa 1.4 MPI",
                    RateHour = 0.8m,
                    RatePrice = 350_000m,
                    Price = 280_000m,
                    VatPercent = 8,
                    RequiredPhotos = "1. Ảnh ODO & VIN; 2. Ảnh màn hình GDS hiển thị phiên bản ECU ROM ID trước và sau khi nâng cấp Update thành công.",
                    Remark = "Bản tin kỹ thuật HTC-TSB-2023-08. Yêu cầu bình ắc quy nối máy sạc duy trì điện áp trên 12.4V trong suốt quá trình flash ROM.",
                    FlagActive = true,
                    CreatedBy = "Hãng HTC"
                },
                new()
                {
                    Code = "WRT-ELC-AC02",
                    Name = "Thay cụm mô-đun điều khiển thân xe BCM & đồng bộ chìa khóa Smartkey",
                    Model = "Accent",
                    LaborGroup = WarrantyLaborGroup.Electrical,
                    CoverageType = WarrantyCoverageType.NewCar,
                    AppTypeCode = "HTC-W-ELC-BCM",
                    EngineType = "Tất cả phiên bản",
                    RateHour = 1.5m,
                    RatePrice = 320_000m,
                    Price = 480_000m,
                    VatPercent = 8,
                    RequiredPhotos = "1. Ảnh ODO & VIN; 2. Ảnh mã lỗi BCM; 3. Ảnh hộp BCM mới và chìa khóa học lệnh thành công.",
                    Remark = "Cần mã PIN code bảo mật đại lý từ cổng DMS HTC để nhập chìa mới.",
                    FlagActive = true,
                    CreatedBy = "Hãng HTC"
                },
                new()
                {
                    Code = "WRT-BDY-CU01",
                    Name = "Căn chỉnh cụm mô-tơ, dây cáp & cảm biến chống kẹt cửa trượt điện thông minh",
                    Model = "Custin",
                    LaborGroup = WarrantyLaborGroup.BodyInterior,
                    CoverageType = WarrantyCoverageType.Goodwill,
                    AppTypeCode = "HTC-W-BDY-SLD",
                    EngineType = "SmartStream 1.5 / 2.0 T-GDI",
                    RateHour = 2.0m,
                    RatePrice = 300_000m,
                    Price = 600_000m,
                    VatPercent = 8,
                    RequiredPhotos = "1. Ảnh ODO & VIN; 2. Ảnh cơ cấu ngàm khóa và dây cáp cửa trượt; 3. Video/ảnh nghiệm thu cửa đóng mở mượt mà.",
                    Remark = "Kiểm tra khe hở mép cửa trượt với thân xe (3.5mm +- 0.5mm), bôi trơn rãnh trượt chuyên dụng mỡ silicon.",
                    FlagActive = true,
                    CreatedBy = "Hãng HTC"
                },
                new()
                {
                    Code = "WRT-CHAS-I10",
                    Name = "Thay thế rô-tuyn cân bằng trước và đệm cao su chân treo hộp số chống rung giật",
                    Model = "Grand i10",
                    LaborGroup = WarrantyLaborGroup.ChassisSuspension,
                    CoverageType = WarrantyCoverageType.NewCar,
                    AppTypeCode = "HTC-W-CHAS-I10",
                    EngineType = "Kappa 1.2 MPI",
                    RateHour = 1.4m,
                    RatePrice = 280_000m,
                    Price = 392_000m,
                    VatPercent = 8,
                    RequiredPhotos = "1. Ảnh ODO & VIN; 2. Ảnh chụp khớp cầu rô-tuyn rơ rách cao su; 3. Ảnh cao su chân số nứt gãy; 4. Ảnh phụ tùng mới đã lắp.",
                    Remark = "Khắc phục triệt để tiếng kêu lục cục dưới gầm khi xe qua gờ giảm tốc.",
                    FlagActive = true,
                    CreatedBy = "Hãng HTC"
                },
                new()
                {
                    Code = "WRT-TRN-EL01",
                    Name = "Thay cụm ly hợp kép khô và cài đặt điểm bắt ly hợp Touch Point hộp số 7DCT",
                    Model = "Elantra",
                    LaborGroup = WarrantyLaborGroup.Transmission,
                    CoverageType = WarrantyCoverageType.ExtendedWarranty,
                    AppTypeCode = "HTC-W-TRN-7DCT",
                    EngineType = "1.6 T-GDI Sport",
                    RateHour = 4.0m,
                    RatePrice = 350_000m,
                    Price = 1_400_000m,
                    VatPercent = 8,
                    RequiredPhotos = "1. Ảnh ODO & VIN; 2. Ảnh lá côn cháy mòn / đo độ hở khe hở ly hợp Clearance; 3. Ảnh cụm côn mới và máy căn chỉnh SST.",
                    Remark = "Thực hiện quy trình học lại điểm ly hợp Touch Point Learning khi nhiệt độ hộp số đạt 60-80 độ C.",
                    FlagActive = true,
                    CreatedBy = "Hãng HTC"
                }
            };

            db.WarrantyWorks.AddRange(warrantyWorks);
            await db.SaveChangesAsync();

            // Link warranty works to existing RO repair lines where ExpenseType == Warranty
            var warrantyLines = await db.Lines.Where(l => l.ExpenseType == ExpenseType.Warranty && l.WarrantyWorkId == null).ToListAsync();
            if (warrantyLines.Count > 0)
            {
                var sfWork = warrantyWorks.FirstOrDefault(w => w.Code == "WRT-ENG-SF01");
                var acWork = warrantyWorks.FirstOrDefault(w => w.Code == "WRT-ENG-AC01");
                for (int i = 0; i < warrantyLines.Count; i++)
                {
                    var targetWork = (i % 2 == 0) ? sfWork : acWork;
                    if (targetWork != null)
                    {
                        warrantyLines[i].WarrantyWorkId = targetWork.Id;
                        warrantyLines[i].StdManHour = targetWork.RateHour;
                    }
                }
                await db.SaveChangesAsync();
            }
        }

        // Seed Thiết lập Chu kỳ & Cấp độ Định mức Bảo dưỡng Định kỳ xe (Ser_MST_ROMaintanceSetting)
        if (!await db.MaintenanceSettings.AnyAsync())
        {
            var pkg5k = await db.ServicePackages.FirstOrDefaultAsync(p => p.PackageNo == "PKG-BD-5K");
            var pkg10k = await db.ServicePackages.FirstOrDefaultAsync(p => p.PackageNo == "PKG-BD-10K");
            var pkg20k = await db.ServicePackages.FirstOrDefaultAsync(p => p.PackageNo == "PKG-BD-20K");
            var pkg40k = await db.ServicePackages.FirstOrDefaultAsync(p => p.PackageNo == "PKG-BD-40K");

            var settings = new List<MaintenanceSetting>
            {
                new()
                {
                    ROMSID = "ROMS-01K",
                    Name = "Bảo dưỡng lần đầu 1.000 km",
                    Km = 1000,
                    Maintances = 0,
                    Level = MaintenanceLevel.Initial1K,
                    MonthsInterval = 1,
                    TakingTimeHours = 0.5m,
                    EstimatedCost = 0m,
                    ServicePackageId = null,
                    RequiredChecklist = "Kiểm tra siết ốc gầm; Kiểm tra rò rỉ dung dịch khoang máy; Kiểm tra áp suất & siết bu-lông 4 bánh; Hướng dẫn khách hàng lịch bảo dưỡng",
                    Description = "Mốc bảo dưỡng chạy rà roda đầu tiên sau khi xuất xưởng. Miễn phí 100% tiền công kiểm tra toàn diện xe.",
                    FlagWarranty = true,
                    FlagActive = true,
                    CreatedBy = "Hệ thống HTC"
                },
                new()
                {
                    ROMSID = "ROMS-05K",
                    Name = "Bảo dưỡng Cấp 1 - 5.000 km (Nhỏ)",
                    Km = 5000,
                    Maintances = 1,
                    Level = MaintenanceLevel.Level1Minor,
                    MonthsInterval = 3,
                    TakingTimeHours = 0.8m,
                    EstimatedCost = 650000m,
                    ServicePackageId = pkg5k?.Id,
                    RequiredChecklist = "Thay dầu động cơ & long đen rốn dầu; Kiểm tra mức dung dịch phanh, nước làm mát, nước rửa kính; Vệ sinh lọc gió động cơ & điều hòa; Kiểm tra hệ thống phanh",
                    Description = "Bảo dưỡng định kỳ cấp nhỏ tiêu chuẩn Hyundai. Duy trì độ êm ái của động cơ và phát hiện sớm các dấu hiệu bất thường.",
                    FlagWarranty = true,
                    FlagActive = true,
                    CreatedBy = "Hệ thống HTC"
                },
                new()
                {
                    ROMSID = "ROMS-10K",
                    Name = "Bảo dưỡng Cấp 2 - 10.000 km (Trung bình)",
                    Km = 10000,
                    Maintances = 2,
                    Level = MaintenanceLevel.Level2Medium,
                    MonthsInterval = 6,
                    TakingTimeHours = 1.2m,
                    EstimatedCost = 1250000m,
                    ServicePackageId = pkg10k?.Id,
                    RequiredChecklist = "Thay dầu động cơ & lọc dầu nhớt; Đảo lốp & cân chỉnh áp suất lốp; Vệ sinh bảo dưỡng 4 cụm phanh; Vệ sinh lọc gió động cơ & điều hòa; Kiểm tra điện áp ắc quy",
                    Description = "Bảo dưỡng định kỳ cấp trung bình. Bắt buộc thay cốc lọc dầu động cơ và đảo lốp để đảm bảo an toàn vận hành.",
                    FlagWarranty = true,
                    FlagActive = true,
                    CreatedBy = "Hệ thống HTC"
                },
                new()
                {
                    ROMSID = "ROMS-15K",
                    Name = "Bảo dưỡng Cấp 1 - 15.000 km (Nhỏ)",
                    Km = 15000,
                    Maintances = 1,
                    Level = MaintenanceLevel.Level1Minor,
                    MonthsInterval = 9,
                    TakingTimeHours = 0.8m,
                    EstimatedCost = 650000m,
                    ServicePackageId = pkg5k?.Id,
                    RequiredChecklist = "Thay dầu động cơ & long đen rốn dầu; Kiểm tra an toàn 15 điểm tiêu chuẩn xưởng; Kiểm tra bổ sung nước làm mát, nước rửa kính",
                    Description = "Bảo dưỡng cấp 1 lặp lại giữa chu kỳ 10.000 km và 20.000 km.",
                    FlagWarranty = true,
                    FlagActive = true,
                    CreatedBy = "Hệ thống HTC"
                },
                new()
                {
                    ROMSID = "ROMS-20K",
                    Name = "Bảo dưỡng Cấp 3 - 20.000 km (Trung bình lớn)",
                    Km = 20000,
                    Maintances = 3,
                    Level = MaintenanceLevel.Level3Major,
                    MonthsInterval = 12,
                    TakingTimeHours = 1.8m,
                    EstimatedCost = 2450000m,
                    ServicePackageId = pkg20k?.Id,
                    RequiredChecklist = "Thay dầu động cơ & cốc lọc dầu; Thay lọc gió động cơ; Thay lọc gió điều hòa kháng khuẩn; Cân bằng động 4 bánh xe; Bảo dưỡng toàn bộ hệ thống phanh; Kiểm tra hệ thống treo & rô-tuyn lái",
                    Description = "Mốc bảo dưỡng quan trọng tròn 1 năm hoặc 20.000 km. Bắt buộc để bảo lưu quyền lợi bảo hành hệ thống động cơ & truyền động.",
                    FlagWarranty = true,
                    FlagActive = true,
                    CreatedBy = "Hệ thống HTC"
                },
                new()
                {
                    ROMSID = "ROMS-30K",
                    Name = "Bảo dưỡng Cấp 2 - 30.000 km (Trung bình)",
                    Km = 30000,
                    Maintances = 2,
                    Level = MaintenanceLevel.Level2Medium,
                    MonthsInterval = 18,
                    TakingTimeHours = 1.2m,
                    EstimatedCost = 1250000m,
                    ServicePackageId = pkg10k?.Id,
                    RequiredChecklist = "Thay dầu máy & lọc dầu; Đảo lốp; Vệ sinh bảo dưỡng phanh đĩa; Kiểm tra hệ thống xả và ống xả khí thải; Kiểm tra dây curoa phụ",
                    Description = "Bảo dưỡng định kỳ cấp 2 mốc 30.000 km.",
                    FlagWarranty = true,
                    FlagActive = true,
                    CreatedBy = "Hệ thống HTC"
                },
                new()
                {
                    ROMSID = "ROMS-40K",
                    Name = "Bảo dưỡng Cấp 4 - 40.000 km (Lớn toàn diện)",
                    Km = 40000,
                    Maintances = 4,
                    Level = MaintenanceLevel.Level4Comprehensive,
                    MonthsInterval = 24,
                    TakingTimeHours = 3.0m,
                    EstimatedCost = 5800000m,
                    ServicePackageId = pkg40k?.Id,
                    RequiredChecklist = "Thay dầu máy & cốc lọc dầu; Thay lọc gió động cơ & điều hòa; Thay dầu phanh DOT4 toàn bộ hệ thống; Thay dầu trợ lực lái & nước làm mát két nước; Thay dầu hộp số tự động/ATF; Kiểm tra bảo dưỡng bugi đánh lửa; Siết bu-lông gầm theo lực tiêu chuẩn",
                    Description = "Đại bảo dưỡng cấp 4 mốc 40.000 km hoặc 24 tháng. Thay thế toàn bộ các loại dung dịch tuần hoàn trên xe.",
                    FlagWarranty = true,
                    FlagActive = true,
                    CreatedBy = "Hệ thống HTC"
                },
                new()
                {
                    ROMSID = "ROMS-50K",
                    Name = "Bảo dưỡng Cấp 1 - 50.000 km (Nhỏ)",
                    Km = 50000,
                    Maintances = 1,
                    Level = MaintenanceLevel.Level1Minor,
                    MonthsInterval = 30,
                    TakingTimeHours = 0.8m,
                    EstimatedCost = 650000m,
                    ServicePackageId = pkg5k?.Id,
                    RequiredChecklist = "Thay dầu động cơ; Kiểm tra an toàn hệ thống gầm máy, phanh và lốp xe; Kiểm tra bình ắc quy",
                    Description = "Bảo dưỡng cấp nhỏ mốc 50.000 km.",
                    FlagWarranty = true,
                    FlagActive = true,
                    CreatedBy = "Hệ thống HTC"
                },
                new()
                {
                    ROMSID = "ROMS-60K",
                    Name = "Bảo dưỡng Cấp 3 - 60.000 km (Trung bình lớn)",
                    Km = 60000,
                    Maintances = 3,
                    Level = MaintenanceLevel.Level3Major,
                    MonthsInterval = 36,
                    TakingTimeHours = 2.0m,
                    EstimatedCost = 2800000m,
                    ServicePackageId = pkg20k?.Id,
                    RequiredChecklist = "Thay dầu máy & lọc dầu; Thay lọc gió động cơ & điều hòa; Thay cụm lọc nhiên liệu thùng xăng/dầu; Cân bằng động bánh xe & chỉnh góc đặt bánh xe; Kiểm tra giảm xóc, cao su càng A",
                    Description = "Bảo dưỡng cấp 3 mốc 60.000 km hoặc 3 năm. Bắt buộc thay thế lọc nhiên liệu để bảo vệ kim phun và bơm cao áp.",
                    FlagWarranty = true,
                    FlagActive = true,
                    CreatedBy = "Hệ thống HTC"
                },
                new()
                {
                    ROMSID = "ROMS-80K",
                    Name = "Bảo dưỡng Cấp 4 - 80.000 km (Lớn cấp 4)",
                    Km = 80000,
                    Maintances = 4,
                    Level = MaintenanceLevel.Level4Comprehensive,
                    MonthsInterval = 48,
                    TakingTimeHours = 3.5m,
                    EstimatedCost = 6900000m,
                    ServicePackageId = pkg40k?.Id,
                    RequiredChecklist = "Thay dầu máy & toàn bộ các loại lọc; Thay bộ 4 bugi đánh lửa Iridium cao cấp; Thay dầu phanh, dầu hộp số tự động, nước làm mát máy; Kiểm tra dây curoa tổng & cụm tăng curoa tự động; Vệ sinh họng hút & buồng đốt",
                    Description = "Bảo dưỡng lớn cấp 4 mốc 80.000 km hoặc 4 năm. Thay thế bugi đánh lửa và toàn bộ dung dịch bôi trơn truyền động.",
                    FlagWarranty = true,
                    FlagActive = true,
                    CreatedBy = "Hệ thống HTC"
                },
                new()
                {
                    ROMSID = "ROMS-100K",
                    Name = "Bảo dưỡng Cấp 4 - 100.000 km (Đại tu chu kỳ)",
                    Km = 100000,
                    Maintances = 4,
                    Level = MaintenanceLevel.Level4Comprehensive,
                    MonthsInterval = 60,
                    TakingTimeHours = 4.0m,
                    EstimatedCost = 8500000m,
                    ServicePackageId = pkg40k?.Id,
                    RequiredChecklist = "Tổng kiểm tra đại tu mốc 100.000 km: Thay toàn bộ lọc & dung dịch; Thay đai cam (nếu dùng đai); Kiểm tra bơm nước làm mát; Kiểm tra hệ thống lái trợ lực điện MDPS; Cân chỉnh thước lái 3D",
                    Description = "Mốc đại tu hoàn chỉnh chu kỳ 100.000 km hoặc 5 năm bảo hành tiêu chuẩn của Hyundai.",
                    FlagWarranty = true,
                    FlagActive = true,
                    CreatedBy = "Hệ thống HTC"
                }
            };

            db.MaintenanceSettings.AddRange(settings);
            await db.SaveChangesAsync();

            // Link existing ROs to matching maintenance milestone
            var allRos = await db.ROs.ToListAsync();
            foreach (var ro in allRos)
            {
                var matched = settings
                    .Where(s => Math.Abs(s.Km - ro.Odometer) <= 3000)
                    .OrderBy(s => Math.Abs(s.Km - ro.Odometer))
                    .FirstOrDefault();

                if (matched != null)
                {
                    ro.MaintenanceSettingId = matched.Id;
                    ro.MaintenanceMilestone = $"{matched.ROMSID} ({matched.Name})";
                }
            }
            await db.SaveChangesAsync();
        }

        // Seed Danh mục Loại ảnh bảo hành (Ser_MST_ROWarrantyPhotoType)
        if (!await db.WarrantyPhotoTypes.AnyAsync())
        {
            db.WarrantyPhotoTypes.AddRange(
                new WarrantyPhotoType { ROWPTCode = "VIN", ROWPTName = "Ảnh số khung VIN" },
                new WarrantyPhotoType { ROWPTCode = "ODO", ROWPTName = "Ảnh đồng hồ ODO" },
                new WarrantyPhotoType { ROWPTCode = "LOI", ROWPTName = "Ảnh chi tiết lỗi / hư hỏng" },
                new WarrantyPhotoType { ROWPTCode = "TONGTHE", ROWPTName = "Ảnh tổng thể xe" },
                new WarrantyPhotoType { ROWPTCode = "NGHIEMTHU", ROWPTName = "Ảnh nghiệm thu sau sửa chữa" },
                new WarrantyPhotoType { ROWPTCode = "PHUTUNG", ROWPTName = "Ảnh phụ tùng thay thế" },
                new WarrantyPhotoType { ROWPTCode = "KHAC", ROWPTName = "Ảnh khác (không hiển thị tóm tắt)" },
                new WarrantyPhotoType { ROWPTCode = "PXK", ROWPTName = "Ảnh phiếu xuất kho (không hiển thị tóm tắt)" },
                new WarrantyPhotoType { ROWPTCode = "MPTC", ROWPTName = "Ảnh mã phụ tùng chính (không hiển thị tóm tắt)" },
                new WarrantyPhotoType { ROWPTCode = "MPTM", ROWPTName = "Ảnh mã phụ tùng mới (không hiển thị tóm tắt)" }
            );
            await db.SaveChangesAsync();
        }

        // Seed Danh mục Loại bảo hành RO (Ser_MST_ROWarrantyType + Ser_MST_ROWarrantyType_PhotoType)
        if (!await db.WarrantyTypes.AnyAsync())
        {
            var types = new List<WarrantyType>
            {
                new()
                {
                    ROWTID = "ROWT-XMA", TypeCode = WarrantyTypeCode.XM, TypeName = "Bảo hành xe mới",
                    DetailCode = WarrantyTypeDetailCode.A, DetailName = "Hư hỏng do lỗi sản xuất",
                    Photos =
                    new[]
                    {
                        new WarrantyPhotoType_Seed("VIN", "Ảnh số khung VIN"),
                        new WarrantyPhotoType_Seed("ODO", "Ảnh đồng hồ ODO"),
                        new WarrantyPhotoType_Seed("LOI", "Ảnh chi tiết lỗi / hư hỏng"),
                        new WarrantyPhotoType_Seed("NGHIEMTHU", "Ảnh nghiệm thu sau sửa chữa")
                    }.Select(p => new WarrantyTypePhoto { ROWPTCode = p.Code, ROWPTName = p.Name }).ToList()
                },
                new()
                {
                    ROWTID = "ROWT-SBB", TypeCode = WarrantyTypeCode.SB, TypeName = "Sửa chữa bảo hành",
                    DetailCode = WarrantyTypeDetailCode.B, DetailName = "Hư hỏng do linh kiện",
                    Photos =
                    new[]
                    {
                        new WarrantyPhotoType_Seed("VIN", "Ảnh số khung VIN"),
                        new WarrantyPhotoType_Seed("LOI", "Ảnh chi tiết lỗi / hư hỏng"),
                        new WarrantyPhotoType_Seed("PHUTUNG", "Ảnh phụ tùng thay thế")
                    }.Select(p => new WarrantyTypePhoto { ROWPTCode = p.Code, ROWPTName = p.Name }).ToList()
                },
                new()
                {
                    ROWTID = "ROWT-PTP", TypeCode = WarrantyTypeCode.PT, TypeName = "Phụ tùng bảo hành",
                    DetailCode = WarrantyTypeDetailCode.P, DetailName = "Phụ tùng",
                    Photos =
                    new[]
                    {
                        new WarrantyPhotoType_Seed("PHUTUNG", "Ảnh phụ tùng thay thế"),
                        new WarrantyPhotoType_Seed("MPTC", "Ảnh mã phụ tùng chính (không hiển thị tóm tắt)")
                    }.Select(p => new WarrantyTypePhoto { ROWPTCode = p.Code, ROWPTName = p.Name }).ToList()
                },
                new()
                {
                    ROWTID = "ROWT-TCW", TypeCode = WarrantyTypeCode.TC, TypeName = "Bảo hành thiện chí",
                    DetailCode = WarrantyTypeDetailCode.W, DetailName = "Công việc bảo hành",
                    Photos =
                    new[]
                    {
                        new WarrantyPhotoType_Seed("VIN", "Ảnh số khung VIN"),
                        new WarrantyPhotoType_Seed("TONGTHE", "Ảnh tổng thể xe")
                    }.Select(p => new WarrantyTypePhoto { ROWPTCode = p.Code, ROWPTName = p.Name }).ToList()
                },
                new()
                {
                    ROWTID = "ROWT-BTS", TypeCode = WarrantyTypeCode.BT, TypeName = "Bảo hành bổ sung",
                    DetailCode = WarrantyTypeDetailCode.S, DetailName = "Sửa chữa",
                    Photos =
                    new[]
                    {
                        new WarrantyPhotoType_Seed("VIN", "Ảnh số khung VIN"),
                        new WarrantyPhotoType_Seed("LOI", "Ảnh chi tiết lỗi / hư hỏng"),
                        new WarrantyPhotoType_Seed("NGHIEMTHU", "Ảnh nghiệm thu sau sửa chữa")
                    }.Select(p => new WarrantyTypePhoto { ROWPTCode = p.Code, ROWPTName = p.Name }).ToList()
                }
            };

            foreach (var t in types)
            {
                t.PhotoTypeDisplay = BuildPhotoDisplay(t.Photos);
                t.LogLuDateTime = DateTime.Now;
            }

            db.WarrantyTypes.AddRange(types);
            await db.SaveChangesAsync();
        }

        // Seed Loại khách hàng dịch vụ (Ser_MST_CustomerType) + Hệ số giá dịch vụ theo loại khách (Ser_Mst_CusServiceFactor)
        if (!await db.CustomerTypes.AnyAsync())
        {
            var cusTypes = new List<CustomerType>
            {
                new() { CusTypeCode = "CT01", CusTypeName = "Khách hàng cá nhân", CusFactor = 1.0m, CusPersonType = "Personal" },
                new() { CusTypeCode = "CT02", CusTypeName = "Khách hàng doanh nghiệp", CusFactor = 0.95m, CusPersonType = "Organization" },
                new() { CusTypeCode = "CT03", CusTypeName = "Khách hàng thân thiết (VIP)", CusFactor = 0.90m, CusPersonType = "Personal" },
                new() { CusTypeCode = "CT04", CusTypeName = "Khách đoàn / Đội xe", CusFactor = 0.85m, CusPersonType = "Organization" },
                new() { CusTypeCode = "CT05", CusTypeName = "Khách bảo hiểm", CusFactor = 1.0m, CusPersonType = "Organization" }
            };
            db.CustomerTypes.AddRange(cusTypes);
            await db.SaveChangesAsync();

            // Cấu hình hệ số riêng cho một số dịch vụ (các ô còn lại dùng hệ số mặc định của loại khách)
            var services = await db.ServiceItems.OrderBy(s => s.Code).Take(6).ToListAsync();
            var factors = new List<CusServiceFactor>();
            foreach (var svc in services)
            {
                foreach (var ct in cusTypes)
                {
                    // Chỉ cấu hình riêng cho khách doanh nghiệp & khách đoàn để minh hoạ ma trận
                    if (ct.CusTypeCode is "CT02" or "CT04")
                    {
                        factors.Add(new CusServiceFactor
                        {
                            ServiceItemId = svc.Id,
                            CustomerTypeId = ct.Id,
                            Factor = ct.CusTypeCode == "CT04" ? 0.80m : 0.92m,
                            DealerCode = "VS058",
                            LogLUBy = "seed",
                            LogLUDateTime = DateTime.Now
                        });
                    }
                }
            }
            if (factors.Count > 0)
            {
                db.CusServiceFactors.AddRange(factors);
                await db.SaveChangesAsync();
            }
        }

        // Nhật ký thao tác RO (Ser_ROHistory) — sinh lịch sử mẫu cho các RO đang có
        if (!await db.RoHistories.AnyAsync())
        {
            var ros = await db.ROs.OrderBy(r => r.Id).ToListAsync();
            var histories = new List<RoHistory>();
            foreach (var ro in ros)
            {
                var baseTime = ro.CreatedAt;
                histories.Add(new RoHistory { ROId = ro.Id, Status = ROStatus.Created, HistoryDate = baseTime, UserCode = "CVDV.Lan", Note = "Tạo Báo giá" });
                histories.Add(new RoHistory { ROId = ro.Id, Status = ROStatus.Printed, HistoryDate = baseTime.AddMinutes(15), UserCode = "CVDV.Lan", Note = "In báo giá, chờ khách ký xác nhận" });
                if (ro.Status is ROStatus.InGarage or ROStatus.Repaired or ROStatus.CheckEnd or ROStatus.Paid or ROStatus.Finished)
                {
                    histories.Add(new RoHistory { ROId = ro.Id, Status = ROStatus.HasRO, HistoryDate = baseTime.AddHours(1), UserCode = "CVDV.Lan", Note = "Lập lệnh sửa chữa chính thức" });
                    histories.Add(new RoHistory { ROId = ro.Id, Status = ROStatus.InGarage, HistoryDate = baseTime.AddHours(2), UserCode = "KTV.Hùng", Note = "Xe vào xưởng, bắt đầu thi công" });
                }
                if (ro.Status is ROStatus.Paid or ROStatus.Finished)
                {
                    histories.Add(new RoHistory { ROId = ro.Id, Status = ROStatus.Repaired, HistoryDate = baseTime.AddHours(5), UserCode = "KTV.Hùng", Note = "Sửa xong, chờ kiểm tra chất lượng" });
                    histories.Add(new RoHistory { ROId = ro.Id, Status = ROStatus.CheckEnd, HistoryDate = baseTime.AddHours(6), UserCode = "KCS.Tuấn", Note = "Kiểm tra chất lượng đạt yêu cầu" });
                    histories.Add(new RoHistory { ROId = ro.Id, Status = ROStatus.Paid, HistoryDate = baseTime.AddHours(7), UserCode = "ThuNgân.Mai", Note = "Khách đã thanh toán" });
                }
                if (ro.Status == ROStatus.Finished)
                {
                    histories.Add(new RoHistory { ROId = ro.Id, Status = ROStatus.Finished, HistoryDate = baseTime.AddHours(8), UserCode = "CVDV.Lan", Note = "Giao xe, hoàn tất lệnh sửa chữa" });
                }
                if (ro.Status == ROStatus.Rejected)
                {
                    histories.Add(new RoHistory { ROId = ro.Id, Status = ROStatus.Rejected, HistoryDate = baseTime.AddHours(1), UserCode = "CVDV.Lan", Note = "Khách từ chối báo giá, hủy lệnh" });
                }
            }
            if (histories.Count > 0)
            {
                db.RoHistories.AddRange(histories);
                await db.SaveChangesAsync();
            }
        }
    }

    private readonly record struct WarrantyPhotoType_Seed(string Code, string Name);

    /// <summary>Dựng chuỗi hiển thị loại ảnh: bỏ 4 mã KHAC/PXK/MPTC/MPTM (đúng luật nguồn).</summary>
    private static string? BuildPhotoDisplay(List<WarrantyTypePhoto> photos)
    {
        var excluded = new[] { "KHAC", "PXK", "MPTC", "MPTM" };
        var names = photos
            .Where(p => !excluded.Contains(p.ROWPTCode, StringComparer.OrdinalIgnoreCase))
            .Select(p => p.ROWPTName ?? p.ROWPTCode)
            .ToList();
        return names.Count > 0 ? string.Join(", ", names) : null;
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
        var tables = new[] { "Customers", "Cars", "ROs", "Lines", "Parts", "WarrantyReports", "WarrantyReportItems", "Appointments", "StockIns", "StockInDetails", "StockOuts", "StockOutDetails", "CustomerCares", "Payments", "Quotes", "QuoteItems", "ServicePackages", "ServicePackageItems", "OrderParts", "OrderPartLines", "Cavities", "ReceptionSheets", "ReceptionItems", "GroupRepairs", "Engineers", "AssignmentWorks", "AssignmentEngineers", "InsuranceCompanies", "InsuranceContracts", "InsuranceClaims", "InsuranceClaimItems", "CampaignMarketings", "CampaignMarketingItems", "CustomerCareMaces", "StockAdjs", "StockAdjDetails", "Bulletins", "BulletinDetails", "BulletinVins", "PdiRequests", "PdiRequestItems", "PdiChecklistItems", "OrderComplains", "OrderComplainAttachFiles", "TechnicalLibraries", "ServiceItems", "Suppliers", "SupplierPayments", "SupplierPaymentDetails", "StockOutOrders", "StockOutOrderDetails", "PartOOs", "CusDebits", "CusDebitPayments", "SupplierDebits", "SupplierDebitPayments", "DealerHistoryRecords", "DealerHistoryItems", "InsuranceDebits", "InsuranceDebitPayments", "CustomerGroups", "CustomerGroupMembers", "PartPriceRequests", "PartPriceRequestLines", "ComplaintDiagnosticErrors", "CustomerCare72hs", "CustomerCareBirthdays", "WarrantyWorks", "MaintenanceSettings", "CustomerTypes", "CusServiceFactors", "RoHistories", "CarModels", "Boms", "BomLines" };
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
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"ReminderMaintanceDate\" timestamp NULL",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"ReminderMaintanceKm\" integer NULL",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"WorkDoneSoon\" boolean NOT NULL DEFAULT false",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"MemberNo\" text NULL",
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
            "ALTER TABLE miniservice.\"Suppliers\" ADD COLUMN IF NOT EXISTS \"BankAccount\" text NULL",
            "ALTER TABLE miniservice.\"Suppliers\" ADD COLUMN IF NOT EXISTS \"BankName\" text NULL",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Suppliers_OrgId_Code\" ON miniservice.\"Suppliers\" (\"OrgId\", \"Code\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"SupplierPayments\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"SupplierPaymentNo\" text NOT NULL, \"SupplierId\" integer NULL, \"SupplierName\" text NOT NULL, \"Address\" text NULL, \"PaymentDate\" timestamp NOT NULL, \"PaymentType\" integer NOT NULL, \"Status\" integer NOT NULL, \"OrderPartId\" integer NULL, \"OrderPartNo\" text NULL, \"TSTRequestNo\" text NULL, \"Description\" text NULL, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"ApprovedBy\" text NULL, \"ApprovedAt\" timestamp NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_SupplierPayments_OrgId_SupplierPaymentNo\" ON miniservice.\"SupplierPayments\" (\"OrgId\", \"SupplierPaymentNo\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"SupplierPaymentDetails\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"SupplierPaymentId\" integer NOT NULL, \"PartId\" integer NOT NULL, \"StockInId\" integer NULL, \"StockInNo\" text NULL, \"QtyPay\" numeric(18,2) NOT NULL, \"Price\" numeric(18,2) NOT NULL, \"VatPercent\" numeric(5,2) NOT NULL, \"QtyInventory\" numeric(18,2) NOT NULL, \"LocationCode\" text NULL, \"Reason\" text NULL)",
            "CREATE TABLE IF NOT EXISTS miniservice.\"StockOutOrders\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"OrderNo\" text NOT NULL, \"OrderDate\" timestamp NOT NULL, \"RequestDeliveryTime\" timestamp NULL, \"Priority\" integer NOT NULL, \"Status\" integer NOT NULL, \"ROId\" integer NULL, \"CustomerId\" integer NULL, \"CarId\" integer NULL, \"CavityId\" integer NULL, \"StockOutId\" integer NULL, \"RequesterName\" text NULL, \"Description\" text NULL, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"ApprovedBy\" text NULL, \"ApprovedAt\" timestamp NULL, \"RejectReason\" text NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_StockOutOrders_OrgId_OrderNo\" ON miniservice.\"StockOutOrders\" (\"OrgId\", \"OrderNo\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"StockOutOrderDetails\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"StockOutOrderId\" integer NOT NULL, \"PartId\" integer NOT NULL, \"PartCode\" text NOT NULL, \"PartName\" text NOT NULL, \"Unit\" text NOT NULL, \"RequestQuantity\" numeric(18,2) NOT NULL, \"IssuedQuantity\" numeric(18,2) NOT NULL, \"UnitPrice\" numeric(18,2) NOT NULL, \"VatPercent\" numeric(5,2) NOT NULL, \"Note\" text NULL)",
            "CREATE INDEX IF NOT EXISTS \"IX_StockOutOrderDetails_OrgId_StockOutOrderId\" ON miniservice.\"StockOutOrderDetails\" (\"OrgId\", \"StockOutOrderId\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"PartOOs\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"OONo\" text NOT NULL, \"PartId\" integer NOT NULL, \"PartCode\" text NOT NULL, \"PartName\" text NOT NULL, \"OOPlateNo\" text NOT NULL, \"Model\" text NULL, \"SoLuongNo\" numeric(18,2) NOT NULL, \"SoLuongTra\" numeric(18,2) NOT NULL DEFAULT 0, \"CVDV\" text NULL, \"NgayDatHang\" timestamp NULL, \"NgayVeDuKien\" timestamp NULL, \"NgayHenTra\" timestamp NULL, \"GhiChu\" text NULL, \"Status\" integer NOT NULL DEFAULT 0, \"ROId\" integer NULL, \"CarId\" integer NULL, \"CustomerId\" integer NULL, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"FinishedAt\" timestamp NULL, \"ReturnedBy\" text NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_PartOOs_OrgId_OONo\" ON miniservice.\"PartOOs\" (\"OrgId\", \"OONo\")",
            "CREATE INDEX IF NOT EXISTS \"IX_PartOOs_OrgId_OOPlateNo\" ON miniservice.\"PartOOs\" (\"OrgId\", \"OOPlateNo\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"CusDebits\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"DebitNo\" text NOT NULL, \"CustomerId\" integer NOT NULL, \"CarId\" integer NULL, \"ROId\" integer NULL, \"DebitType\" integer NOT NULL, \"Status\" integer NOT NULL, \"DebitDate\" timestamp NOT NULL, \"DueDate\" timestamp NULL, \"DebitAmount\" numeric(18,2) NOT NULL, \"PaidAmount\" numeric(18,2) NOT NULL DEFAULT 0, \"Description\" text NULL, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"ClearedAt\" timestamp NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_CusDebits_OrgId_DebitNo\" ON miniservice.\"CusDebits\" (\"OrgId\", \"DebitNo\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"CusDebitPayments\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"PaymentNo\" text NOT NULL, \"CustomerId\" integer NOT NULL, \"CusDebitId\" integer NULL, \"PaymentDate\" timestamp NOT NULL, \"PaymentAmount\" numeric(18,2) NOT NULL, \"Method\" integer NOT NULL, \"PayPersonName\" text NOT NULL, \"PayPersonIdCard\" text NULL, \"PayPersonPhone\" text NULL, \"TransactionRef\" text NULL, \"Note\" text NULL, \"Collector\" text NOT NULL, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_CusDebitPayments_OrgId_PaymentNo\" ON miniservice.\"CusDebitPayments\" (\"OrgId\", \"PaymentNo\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"SupplierDebits\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"DebitNo\" text NOT NULL, \"SupplierId\" integer NOT NULL, \"StockInId\" integer NULL, \"OrderPartId\" integer NULL, \"DebitType\" integer NOT NULL, \"Status\" integer NOT NULL, \"DebitDate\" timestamp NOT NULL, \"DueDate\" timestamp NULL, \"DebitAmount\" numeric(18,2) NOT NULL, \"PaidAmount\" numeric(18,2) NOT NULL DEFAULT 0, \"Description\" text NULL, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"ClearedAt\" timestamp NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_SupplierDebits_OrgId_DebitNo\" ON miniservice.\"SupplierDebits\" (\"OrgId\", \"DebitNo\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"SupplierDebitPayments\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"PaymentNo\" text NOT NULL, \"SupplierId\" integer NOT NULL, \"SupplierDebitId\" integer NULL, \"PaymentDate\" timestamp NOT NULL, \"PaymentAmount\" numeric(18,2) NOT NULL, \"Method\" integer NOT NULL, \"PayPersonName\" text NOT NULL, \"PayPersonIdCard\" text NULL, \"PayPersonPhone\" text NULL, \"BankAccount\" text NULL, \"BankName\" text NULL, \"TransactionRef\" text NULL, \"Note\" text NULL, \"Cashier\" text NOT NULL, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_SupplierDebitPayments_OrgId_PaymentNo\" ON miniservice.\"SupplierDebitPayments\" (\"OrgId\", \"PaymentNo\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"DealerHistoryRecords\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"RecordNo\" text NOT NULL, \"DealerCode\" text NOT NULL, \"DealerName\" text NOT NULL, \"PlateNo\" text NOT NULL, \"FrameNo\" text NOT NULL, \"EngineNo\" text NULL, \"TradeMarkName\" text NOT NULL, \"ModelName\" text NOT NULL, \"ColorCode\" text NULL, \"ProductYear\" integer NOT NULL, \"CusName\" text NOT NULL, \"CusPhone\" text NULL, \"CusAddress\" text NULL, \"RONo\" text NOT NULL, \"ROId\" integer NULL, \"CheckInDate\" timestamp NOT NULL, \"ActualDeliveryDate\" timestamp NULL, \"Odometer\" integer NOT NULL, \"ServiceAdvisor\" text NOT NULL, \"Technician\" text NULL, \"CustomerRequest\" text NULL, \"CarStatus\" text NULL, \"RepairResult\" text NULL, \"TotalLaborAmount\" numeric(18,2) NOT NULL, \"TotalPartAmount\" numeric(18,2) NOT NULL, \"TotalAmount\" numeric(18,2) NOT NULL, \"FlagClaim\" boolean NOT NULL DEFAULT false, \"ClaimNo\" text NULL, \"ClaimStatus\" text NULL, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_DealerHistoryRecords_OrgId_RecordNo\" ON miniservice.\"DealerHistoryRecords\" (\"OrgId\", \"RecordNo\")",
            "CREATE INDEX IF NOT EXISTS \"IX_DealerHistoryRecords_OrgId_PlateNo\" ON miniservice.\"DealerHistoryRecords\" (\"OrgId\", \"PlateNo\")",
            "CREATE INDEX IF NOT EXISTS \"IX_DealerHistoryRecords_OrgId_FrameNo\" ON miniservice.\"DealerHistoryRecords\" (\"OrgId\", \"FrameNo\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"DealerHistoryItems\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"DealerHistoryRecordId\" integer NOT NULL, \"ItemType\" integer NOT NULL, \"Code\" text NOT NULL, \"Name\" text NOT NULL, \"Unit\" text NOT NULL, \"Quantity\" numeric(18,2) NOT NULL, \"UnitPrice\" numeric(18,2) NOT NULL, \"Amount\" numeric(18,2) NOT NULL, \"ExpenseType\" integer NOT NULL, \"Technician\" text NULL, \"Result\" text NULL, \"Remark\" text NULL)",
            "CREATE INDEX IF NOT EXISTS \"IX_DealerHistoryItems_OrgId_DealerHistoryRecordId\" ON miniservice.\"DealerHistoryItems\" (\"OrgId\", \"DealerHistoryRecordId\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"InsuranceDebits\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"DebitNo\" text NOT NULL, \"InsuranceCompanyId\" integer NOT NULL, \"InsNo\" text NOT NULL, \"InsName\" text NOT NULL, \"InsuranceContractId\" integer NULL, \"ROId\" integer NULL, \"RONo\" text NULL, \"PlateNo\" text NULL, \"CarModel\" text NULL, \"CustomerName\" text NULL, \"InsuranceClaimId\" integer NULL, \"ClaimNo\" text NULL, \"PolicyNo\" text NULL, \"DebitType\" integer NOT NULL, \"Status\" integer NOT NULL, \"DebitDate\" timestamp NOT NULL, \"DueDate\" timestamp NULL, \"DebitAmount\" numeric(18,2) NOT NULL, \"PaidAmount\" numeric(18,2) NOT NULL DEFAULT 0, \"Description\" text NULL, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"ClearedAt\" timestamp NULL, \"CancelledReason\" text NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_InsuranceDebits_OrgId_DebitNo\" ON miniservice.\"InsuranceDebits\" (\"OrgId\", \"DebitNo\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"InsuranceDebitPayments\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"PaymentNo\" text NOT NULL, \"InsuranceCompanyId\" integer NOT NULL, \"InsNo\" text NOT NULL, \"InsName\" text NOT NULL, \"InsuranceDebitId\" integer NULL, \"PaymentDate\" timestamp NOT NULL, \"PaymentAmount\" numeric(18,2) NOT NULL, \"Method\" integer NOT NULL, \"PayPersonName\" text NOT NULL, \"PayPersonIdCard\" text NULL, \"PayPersonPhone\" text NULL, \"BankAccount\" text NULL, \"BankName\" text NULL, \"TransactionRef\" text NULL, \"Note\" text NULL, \"Cashier\" text NOT NULL, \"Status\" integer NOT NULL, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_InsuranceDebitPayments_OrgId_PaymentNo\" ON miniservice.\"InsuranceDebitPayments\" (\"OrgId\", \"PaymentNo\")",
            "ALTER TABLE miniservice.\"Customers\" ADD COLUMN IF NOT EXISTS \"CustomerGroupId\" integer NULL",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"CustomerGroupId\" integer NULL",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"CustomerGroupDiscountAmount\" numeric(18,2) NOT NULL DEFAULT 0",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"ErrorCodePN\" text NULL",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"ErrorCodeCD\" text NULL",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"DiagnosticResult\" text NULL",
            "CREATE TABLE IF NOT EXISTS miniservice.\"CustomerGroups\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"GroupNo\" text NOT NULL, \"GroupName\" text NOT NULL, \"TaxCode\" text NULL, \"Address\" text NULL, \"Telephone\" text NULL, \"Fax\" text NULL, \"Email\" text NULL, \"ContactPerson\" text NULL, \"ContactPhone\" text NULL, \"Description\" text NULL, \"IsActive\" boolean NOT NULL DEFAULT true, \"DiscountPercentLabor\" numeric(5,2) NOT NULL DEFAULT 0, \"DiscountPercentPart\" numeric(5,2) NOT NULL DEFAULT 0, \"CreditLimit\" numeric(18,2) NOT NULL DEFAULT 0, \"PaymentTermDays\" integer NOT NULL DEFAULT 30, \"ContractNo\" text NULL, \"ContractStartDate\" timestamp NULL, \"ContractEndDate\" timestamp NULL, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"UpdatedAt\" timestamp NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_CustomerGroups_OrgId_GroupNo\" ON miniservice.\"CustomerGroups\" (\"OrgId\", \"GroupNo\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"CustomerGroupMembers\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"CustomerGroupId\" integer NOT NULL, \"CustomerId\" integer NULL, \"CarId\" integer NOT NULL, \"PlateNo\" text NOT NULL, \"DriverName\" text NULL, \"DriverPhone\" text NULL, \"Note\" text NULL, \"JoinedDate\" timestamp NOT NULL DEFAULT now(), \"IsActive\" boolean NOT NULL DEFAULT true)",
            "CREATE INDEX IF NOT EXISTS \"IX_CustomerGroupMembers_OrgId_CustomerGroupId\" ON miniservice.\"CustomerGroupMembers\" (\"OrgId\", \"CustomerGroupId\")",
            "CREATE INDEX IF NOT EXISTS \"IX_CustomerGroupMembers_OrgId_CarId\" ON miniservice.\"CustomerGroupMembers\" (\"OrgId\", \"CarId\")",
            "CREATE INDEX IF NOT EXISTS \"IX_CustomerGroupMembers_OrgId_PlateNo\" ON miniservice.\"CustomerGroupMembers\" (\"OrgId\", \"PlateNo\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"PartPriceRequests\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"ReqPartPriceNo\" text NOT NULL, \"DealerCode\" text NOT NULL, \"DealerName\" text NOT NULL, \"Description\" text NOT NULL, \"TSTReqPartPriceID\" text NULL, \"TSTSentDate\" timestamp NULL, \"DMSStatus\" integer NOT NULL, \"TSTStatus\" integer NOT NULL, \"FlagIsCheck\" boolean NOT NULL DEFAULT false, \"IsUpdatePrice\" boolean NOT NULL DEFAULT false, \"UpdatedPriceAt\" timestamp NULL, \"EffectiveDate\" timestamp NULL, \"EstimatedResponseDate\" timestamp NULL, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"ApprovedBy\" text NULL, \"ApprovedAt\" timestamp NULL, \"ROId\" integer NULL, \"VIN\" text NULL, \"CarModel\" text NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_PartPriceRequests_OrgId_ReqPartPriceNo\" ON miniservice.\"PartPriceRequests\" (\"OrgId\", \"ReqPartPriceNo\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"PartPriceRequestLines\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"PartPriceRequestId\" integer NOT NULL, \"PartId\" integer NULL, \"DMSPartCode\" text NOT NULL, \"VieName\" text NOT NULL, \"VINCode\" text NULL, \"DeliveryForm\" integer NOT NULL, \"Quantity\" numeric(18,2) NOT NULL, \"Unit\" text NOT NULL, \"Remark\" text NULL, \"TSTPartCode\" text NULL, \"TSTPrice\" numeric(18,2) NOT NULL DEFAULT 0, \"DateEffect\" timestamp NULL, \"Status\" integer NOT NULL DEFAULT 0)",
            "CREATE INDEX IF NOT EXISTS \"IX_PartPriceRequestLines_OrgId_PartPriceRequestId\" ON miniservice.\"PartPriceRequestLines\" (\"OrgId\", \"PartPriceRequestId\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"ComplaintDiagnosticErrors\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"ErrorCode\" text NOT NULL, \"ErrorName\" text NOT NULL, \"ErrorType\" integer NOT NULL, \"SystemGroup\" integer NOT NULL, \"ErrorDesc\" text NULL, \"Remark\" text NULL, \"FlagActive\" boolean NOT NULL DEFAULT true, \"UsageCount\" integer NOT NULL DEFAULT 0, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"UpdatedAt\" timestamp NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_ComplaintDiagnosticErrors_OrgId_ErrorCode\" ON miniservice.\"ComplaintDiagnosticErrors\" (\"OrgId\", \"ErrorCode\")",
            "CREATE INDEX IF NOT EXISTS \"IX_ComplaintDiagnosticErrors_OrgId_ErrorType\" ON miniservice.\"ComplaintDiagnosticErrors\" (\"OrgId\", \"ErrorType\")",
            "CREATE INDEX IF NOT EXISTS \"IX_ComplaintDiagnosticErrors_OrgId_SystemGroup\" ON miniservice.\"ComplaintDiagnosticErrors\" (\"OrgId\", \"SystemGroup\")",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"IsReRepair\" boolean NOT NULL DEFAULT false",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"ReRepairParentROId\" integer NULL",
            "CREATE TABLE IF NOT EXISTS miniservice.\"CustomerCare72hs\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"Care72No\" text NOT NULL, \"ROId\" integer NOT NULL, \"CarId\" integer NOT NULL, \"CustomerId\" integer NOT NULL, \"Status\" integer NOT NULL, \"ROFinishedDate\" timestamp NOT NULL, \"ScheduledDate\" timestamp NOT NULL, \"ContactedDate\" timestamp NULL, \"ContactedBy\" text NULL, \"ServiceExplained\" boolean NULL, \"BasicNeedsMet\" boolean NULL, \"HasTechnicalProblem\" boolean NOT NULL DEFAULT false, \"ProblemDetails\" text NULL, \"FixedRightFirstTime\" boolean NULL, \"SatisfactionRating\" integer NULL, \"CustomerFeedback\" text NULL, \"IsReRepairAlert\" boolean NOT NULL DEFAULT false, \"ReRepairAction\" text NULL, \"ReRepairROId\" integer NULL, \"ReRepairAppointmentId\" integer NULL, \"InternalNote\" text NULL, \"CreatedBy\" text NOT NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_CustomerCare72hs_OrgId_Care72No\" ON miniservice.\"CustomerCare72hs\" (\"OrgId\", \"Care72No\")",
            "CREATE INDEX IF NOT EXISTS \"IX_CustomerCare72hs_OrgId_ROId\" ON miniservice.\"CustomerCare72hs\" (\"OrgId\", \"ROId\")",
            "CREATE INDEX IF NOT EXISTS \"IX_CustomerCare72hs_OrgId_CarId\" ON miniservice.\"CustomerCare72hs\" (\"OrgId\", \"CarId\")",
            "CREATE INDEX IF NOT EXISTS \"IX_CustomerCare72hs_OrgId_CustomerId\" ON miniservice.\"CustomerCare72hs\" (\"OrgId\", \"CustomerId\")",
            "CREATE INDEX IF NOT EXISTS \"IX_CustomerCare72hs_OrgId_Status\" ON miniservice.\"CustomerCare72hs\" (\"OrgId\", \"Status\")",
            "ALTER TABLE miniservice.\"Customers\" ADD COLUMN IF NOT EXISTS \"DateOfBirth\" timestamp NULL",
            "ALTER TABLE miniservice.\"Customers\" ADD COLUMN IF NOT EXISTS \"Address\" text NULL",
            "ALTER TABLE miniservice.\"Customers\" ADD COLUMN IF NOT EXISTS \"Gender\" text NULL",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"BirthdayDiscountAmount\" numeric(18,2) NOT NULL DEFAULT 0",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"BirthdayCareId\" integer NULL",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"BirthdayVoucherCode\" text NULL",
            "ALTER TABLE miniservice.\"Appointments\" ADD COLUMN IF NOT EXISTS \"CustomerCareBirthdayId\" integer NULL",
            "CREATE TABLE IF NOT EXISTS miniservice.\"CustomerCareBirthdays\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"CareBthNo\" text NOT NULL, \"CustomerId\" integer NOT NULL, \"CarId\" integer NULL, \"DateOfBirth\" timestamp NULL, \"DateBth\" timestamp NOT NULL, \"Status\" integer NOT NULL DEFAULT 0, \"ContactDate\" timestamp NULL, \"ContactedBy\" text NULL, \"ContactChannel\" integer NOT NULL DEFAULT 0, \"Remark\" text NULL, \"GiftVoucherCode\" text NULL, \"GiftVoucherValue\" numeric(18,2) NOT NULL DEFAULT 300000, \"DiscountPercent\" numeric(5,2) NOT NULL DEFAULT 10, \"VoucherValidUntil\" timestamp NULL, \"IsVoucherUsed\" boolean NOT NULL DEFAULT false, \"UsedInROId\" integer NULL, \"AppointmentId\" integer NULL, \"CreatedBy\" text NOT NULL DEFAULT 'system', \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"UpdatedAt\" timestamp NULL, \"UpdatedBy\" text NULL, \"LogLuDateTime\" timestamp NULL, \"LogLUBy\" text NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_CustomerCareBirthdays_OrgId_CareBthNo\" ON miniservice.\"CustomerCareBirthdays\" (\"OrgId\", \"CareBthNo\")",
            "CREATE INDEX IF NOT EXISTS \"IX_CustomerCareBirthdays_OrgId_CustomerId\" ON miniservice.\"CustomerCareBirthdays\" (\"OrgId\", \"CustomerId\")",
            "CREATE INDEX IF NOT EXISTS \"IX_CustomerCareBirthdays_OrgId_DateBth\" ON miniservice.\"CustomerCareBirthdays\" (\"OrgId\", \"DateBth\")",
            "CREATE INDEX IF NOT EXISTS \"IX_CustomerCareBirthdays_OrgId_Status\" ON miniservice.\"CustomerCareBirthdays\" (\"OrgId\", \"Status\")",
            "ALTER TABLE miniservice.\"Lines\" ADD COLUMN IF NOT EXISTS \"WarrantyWorkId\" integer NULL",
            "CREATE TABLE IF NOT EXISTS miniservice.\"WarrantyWorks\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"Code\" text NOT NULL, \"Name\" text NOT NULL, \"Model\" text NOT NULL, \"LaborGroup\" integer NOT NULL DEFAULT 1, \"CoverageType\" integer NOT NULL DEFAULT 1, \"AppTypeCode\" text NULL, \"EngineType\" text NULL, \"RateHour\" numeric(5,2) NOT NULL DEFAULT 1.0, \"RatePrice\" numeric(18,2) NOT NULL DEFAULT 300000, \"Price\" numeric(18,2) NOT NULL DEFAULT 300000, \"VatPercent\" integer NOT NULL DEFAULT 8, \"RequiredPhotos\" text NULL, \"Remark\" text NULL, \"FlagActive\" boolean NOT NULL DEFAULT true, \"CreatedBy\" text NOT NULL DEFAULT 'Hãng HTC', \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"UpdatedAt\" timestamp NULL, \"UpdatedBy\" text NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_WarrantyWorks_OrgId_Code\" ON miniservice.\"WarrantyWorks\" (\"OrgId\", \"Code\")",
            "CREATE INDEX IF NOT EXISTS \"IX_WarrantyWorks_OrgId_Model\" ON miniservice.\"WarrantyWorks\" (\"OrgId\", \"Model\")",
            "CREATE INDEX IF NOT EXISTS \"IX_WarrantyWorks_OrgId_LaborGroup\" ON miniservice.\"WarrantyWorks\" (\"OrgId\", \"LaborGroup\")",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"MaintenanceSettingId\" integer NULL",
            "ALTER TABLE miniservice.\"ROs\" ADD COLUMN IF NOT EXISTS \"MaintenanceMilestone\" text NULL",
            "CREATE TABLE IF NOT EXISTS miniservice.\"MaintenanceSettings\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"ROMSID\" text NOT NULL, \"Name\" text NOT NULL, \"Km\" integer NOT NULL, \"Maintances\" integer NOT NULL DEFAULT 1, \"Level\" integer NOT NULL DEFAULT 1, \"MonthsInterval\" integer NOT NULL DEFAULT 6, \"TakingTimeHours\" numeric(5,2) NOT NULL DEFAULT 1.0, \"EstimatedCost\" numeric(18,2) NOT NULL DEFAULT 650000, \"ServicePackageId\" integer NULL, \"RequiredChecklist\" text NULL, \"Description\" text NULL, \"FlagWarranty\" boolean NOT NULL DEFAULT true, \"FlagActive\" boolean NOT NULL DEFAULT true, \"LogLuDateTime\" timestamp NULL, \"LogLUBy\" text NULL, \"CreatedBy\" text NOT NULL DEFAULT 'Hệ thống HTC', \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"UpdatedAt\" timestamp NULL, \"UpdatedBy\" text NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_MaintenanceSettings_OrgId_ROMSID\" ON miniservice.\"MaintenanceSettings\" (\"OrgId\", \"ROMSID\")",
            "CREATE INDEX IF NOT EXISTS \"IX_MaintenanceSettings_OrgId_Km\" ON miniservice.\"MaintenanceSettings\" (\"OrgId\", \"Km\")",
            "CREATE INDEX IF NOT EXISTS \"IX_MaintenanceSettings_OrgId_Level\" ON miniservice.\"MaintenanceSettings\" (\"OrgId\", \"Level\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"WarrantyPhotoTypes\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"ROWPTCode\" text NOT NULL, \"ROWPTName\" text NOT NULL, \"FlagActive\" boolean NOT NULL DEFAULT true, \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_WarrantyPhotoTypes_OrgId_ROWPTCode\" ON miniservice.\"WarrantyPhotoTypes\" (\"OrgId\", \"ROWPTCode\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"WarrantyTypes\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"ROWTID\" text NOT NULL, \"TypeCode\" integer NOT NULL, \"TypeName\" text NOT NULL, \"DetailCode\" integer NOT NULL, \"DetailName\" text NOT NULL, \"PhotoTypeDisplay\" text NULL, \"FlagActive\" boolean NOT NULL DEFAULT true, \"LogLuDateTime\" timestamp NULL, \"LogLUBy\" text NULL, \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"UpdatedAt\" timestamp NULL, \"UpdatedBy\" text NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_WarrantyTypes_OrgId_TypeCode_DetailCode\" ON miniservice.\"WarrantyTypes\" (\"OrgId\", \"TypeCode\", \"DetailCode\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"WarrantyTypePhotos\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"WarrantyTypeId\" integer NOT NULL, \"ROWPTCode\" text NOT NULL, \"ROWPTName\" text NULL)",
            "CREATE INDEX IF NOT EXISTS \"IX_WarrantyTypePhotos_OrgId_WarrantyTypeId\" ON miniservice.\"WarrantyTypePhotos\" (\"OrgId\", \"WarrantyTypeId\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"RoHistories\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"ROId\" integer NOT NULL, \"Status\" integer NOT NULL, \"HistoryDate\" timestamp NOT NULL DEFAULT now(), \"UserCode\" text NULL, \"Note\" text NULL)",
            "CREATE INDEX IF NOT EXISTS \"IX_RoHistories_OrgId_ROId\" ON miniservice.\"RoHistories\" (\"OrgId\", \"ROId\")",
            "CREATE INDEX IF NOT EXISTS \"IX_RoHistories_OrgId_Status\" ON miniservice.\"RoHistories\" (\"OrgId\", \"Status\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"CarModels\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"ModelCode\" text NOT NULL, \"ModelName\" text NOT NULL, \"TradeMarkCode\" text NOT NULL, \"ProductionCode\" text NULL, \"DealerCode\" text NULL, \"Segment\" integer NOT NULL DEFAULT 0, \"ProductYear\" integer NULL, \"IsActive\" boolean NOT NULL DEFAULT true, \"CreatedBy\" text NOT NULL DEFAULT 'web', \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"LogLUBy\" text NULL, \"LogLUDateTime\" timestamp NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_CarModels_OrgId_ModelCode\" ON miniservice.\"CarModels\" (\"OrgId\", \"ModelCode\")",
            "CREATE INDEX IF NOT EXISTS \"IX_CarModels_OrgId_TradeMarkCode\" ON miniservice.\"CarModels\" (\"OrgId\", \"TradeMarkCode\")",
            "CREATE INDEX IF NOT EXISTS \"IX_CarModels_OrgId_Segment\" ON miniservice.\"CarModels\" (\"OrgId\", \"Segment\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"Boms\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"BomCode\" text NOT NULL, \"BomDesc\" text NOT NULL, \"Remark\" text NULL, \"IsActive\" boolean NOT NULL DEFAULT true, \"CreatedBy\" text NOT NULL DEFAULT 'web', \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"LogLUBy\" text NULL, \"LogLUDateTime\" timestamp NULL)",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Boms_OrgId_BomCode\" ON miniservice.\"Boms\" (\"OrgId\", \"BomCode\")",
            "CREATE TABLE IF NOT EXISTS miniservice.\"BomLines\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL, \"BomId\" integer NOT NULL, \"PartCode\" text NOT NULL, \"PartName\" text NOT NULL, \"Unit\" text NOT NULL DEFAULT 'Cái', \"QtyMin\" numeric(18,4) NOT NULL DEFAULT 1, \"LogLUBy\" text NULL, \"LogLUDateTime\" timestamp NULL)",
            "CREATE INDEX IF NOT EXISTS \"IX_BomLines_OrgId_BomId\" ON miniservice.\"BomLines\" (\"OrgId\", \"BomId\")",
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
            @"ALTER TABLE ""Suppliers"" ADD COLUMN ""BankAccount"" TEXT NULL;",
            @"ALTER TABLE ""Suppliers"" ADD COLUMN ""BankName"" TEXT NULL;",
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
            @"CREATE INDEX IF NOT EXISTS ""IX_StockOutOrderDetails_OrgId_StockOutOrderId"" ON ""StockOutOrderDetails"" (""OrgId"", ""StockOutOrderId"");",
            @"CREATE TABLE IF NOT EXISTS ""PartOOs"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""OONo"" TEXT NOT NULL,
                ""PartId"" INTEGER NOT NULL,
                ""PartCode"" TEXT NOT NULL,
                ""PartName"" TEXT NOT NULL,
                ""OOPlateNo"" TEXT NOT NULL,
                ""Model"" TEXT NULL,
                ""SoLuongNo"" TEXT NOT NULL,
                ""SoLuongTra"" TEXT NOT NULL DEFAULT '0',
                ""CVDV"" TEXT NULL,
                ""NgayDatHang"" TEXT NULL,
                ""NgayVeDuKien"" TEXT NULL,
                ""NgayHenTra"" TEXT NULL,
                ""GhiChu"" TEXT NULL,
                ""Status"" INTEGER NOT NULL DEFAULT 0,
                ""ROId"" INTEGER NULL,
                ""CarId"" INTEGER NULL,
                ""CustomerId"" INTEGER NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""FinishedAt"" TEXT NULL,
                ""ReturnedBy"" TEXT NULL,
                FOREIGN KEY (""PartId"") REFERENCES ""Parts"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""CarId"") REFERENCES ""Cars"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""CustomerId"") REFERENCES ""Customers"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PartOOs_OrgId_OONo"" ON ""PartOOs"" (""OrgId"", ""OONo"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_PartOOs_OrgId_OOPlateNo"" ON ""PartOOs"" (""OrgId"", ""OOPlateNo"");",
            @"CREATE TABLE IF NOT EXISTS ""CusDebits"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""DebitNo"" TEXT NOT NULL,
                ""CustomerId"" INTEGER NOT NULL,
                ""CarId"" INTEGER NULL,
                ""ROId"" INTEGER NULL,
                ""DebitType"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""DebitDate"" TEXT NOT NULL,
                ""DueDate"" TEXT NULL,
                ""DebitAmount"" TEXT NOT NULL,
                ""PaidAmount"" TEXT NOT NULL DEFAULT '0',
                ""Description"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""ClearedAt"" TEXT NULL,
                FOREIGN KEY (""CustomerId"") REFERENCES ""Customers"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""CarId"") REFERENCES ""Cars"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CusDebits_OrgId_DebitNo"" ON ""CusDebits"" (""OrgId"", ""DebitNo"");",
            @"CREATE TABLE IF NOT EXISTS ""CusDebitPayments"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""PaymentNo"" TEXT NOT NULL,
                ""CustomerId"" INTEGER NOT NULL,
                ""CusDebitId"" INTEGER NULL,
                ""PaymentDate"" TEXT NOT NULL,
                ""PaymentAmount"" TEXT NOT NULL,
                ""Method"" INTEGER NOT NULL,
                ""PayPersonName"" TEXT NOT NULL,
                ""PayPersonIdCard"" TEXT NULL,
                ""PayPersonPhone"" TEXT NULL,
                ""TransactionRef"" TEXT NULL,
                ""Note"" TEXT NULL,
                ""Collector"" TEXT NOT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                FOREIGN KEY (""CustomerId"") REFERENCES ""Customers"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""CusDebitId"") REFERENCES ""CusDebits"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CusDebitPayments_OrgId_PaymentNo"" ON ""CusDebitPayments"" (""OrgId"", ""PaymentNo"");",
            @"CREATE TABLE IF NOT EXISTS ""SupplierDebits"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""DebitNo"" TEXT NOT NULL,
                ""SupplierId"" INTEGER NOT NULL,
                ""StockInId"" INTEGER NULL,
                ""OrderPartId"" INTEGER NULL,
                ""DebitType"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""DebitDate"" TEXT NOT NULL,
                ""DueDate"" TEXT NULL,
                ""DebitAmount"" TEXT NOT NULL,
                ""PaidAmount"" TEXT NOT NULL DEFAULT '0',
                ""Description"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""ClearedAt"" TEXT NULL,
                FOREIGN KEY (""SupplierId"") REFERENCES ""Suppliers"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""StockInId"") REFERENCES ""StockIns"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""OrderPartId"") REFERENCES ""OrderParts"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_SupplierDebits_OrgId_DebitNo"" ON ""SupplierDebits"" (""OrgId"", ""DebitNo"");",
            @"CREATE TABLE IF NOT EXISTS ""SupplierDebitPayments"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""PaymentNo"" TEXT NOT NULL,
                ""SupplierId"" INTEGER NOT NULL,
                ""SupplierDebitId"" INTEGER NULL,
                ""PaymentDate"" TEXT NOT NULL,
                ""PaymentAmount"" TEXT NOT NULL,
                ""Method"" INTEGER NOT NULL,
                ""PayPersonName"" TEXT NOT NULL,
                ""PayPersonIdCard"" TEXT NULL,
                ""PayPersonPhone"" TEXT NULL,
                ""BankAccount"" TEXT NULL,
                ""BankName"" TEXT NULL,
                ""TransactionRef"" TEXT NULL,
                ""Note"" TEXT NULL,
                ""Cashier"" TEXT NOT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                FOREIGN KEY (""SupplierId"") REFERENCES ""Suppliers"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""SupplierDebitId"") REFERENCES ""SupplierDebits"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_SupplierDebitPayments_OrgId_PaymentNo"" ON ""SupplierDebitPayments"" (""OrgId"", ""PaymentNo"");",
            @"CREATE TABLE IF NOT EXISTS ""DealerHistoryRecords"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""RecordNo"" TEXT NOT NULL,
                ""DealerCode"" TEXT NOT NULL,
                ""DealerName"" TEXT NOT NULL,
                ""PlateNo"" TEXT NOT NULL,
                ""FrameNo"" TEXT NOT NULL,
                ""EngineNo"" TEXT NULL,
                ""TradeMarkName"" TEXT NOT NULL,
                ""ModelName"" TEXT NOT NULL,
                ""ColorCode"" TEXT NULL,
                ""ProductYear"" INTEGER NOT NULL,
                ""CusName"" TEXT NOT NULL,
                ""CusPhone"" TEXT NULL,
                ""CusAddress"" TEXT NULL,
                ""RONo"" TEXT NOT NULL,
                ""ROId"" INTEGER NULL,
                ""CheckInDate"" TEXT NOT NULL,
                ""ActualDeliveryDate"" TEXT NULL,
                ""Odometer"" INTEGER NOT NULL,
                ""ServiceAdvisor"" TEXT NOT NULL,
                ""Technician"" TEXT NULL,
                ""CustomerRequest"" TEXT NULL,
                ""CarStatus"" TEXT NULL,
                ""RepairResult"" TEXT NULL,
                ""TotalLaborAmount"" TEXT NOT NULL,
                ""TotalPartAmount"" TEXT NOT NULL,
                ""TotalAmount"" TEXT NOT NULL,
                ""FlagClaim"" INTEGER NOT NULL DEFAULT 0,
                ""ClaimNo"" TEXT NULL,
                ""ClaimStatus"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_DealerHistoryRecords_OrgId_RecordNo"" ON ""DealerHistoryRecords"" (""OrgId"", ""RecordNo"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_DealerHistoryRecords_OrgId_PlateNo"" ON ""DealerHistoryRecords"" (""OrgId"", ""PlateNo"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_DealerHistoryRecords_OrgId_FrameNo"" ON ""DealerHistoryRecords"" (""OrgId"", ""FrameNo"");",
            @"CREATE TABLE IF NOT EXISTS ""DealerHistoryItems"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""DealerHistoryRecordId"" INTEGER NOT NULL,
                ""ItemType"" INTEGER NOT NULL,
                ""Code"" TEXT NOT NULL,
                ""Name"" TEXT NOT NULL,
                ""Unit"" TEXT NOT NULL,
                ""Quantity"" TEXT NOT NULL,
                ""UnitPrice"" TEXT NOT NULL,
                ""Amount"" TEXT NOT NULL,
                ""ExpenseType"" INTEGER NOT NULL,
                ""Technician"" TEXT NULL,
                ""Result"" TEXT NULL,
                ""Remark"" TEXT NULL,
                FOREIGN KEY (""DealerHistoryRecordId"") REFERENCES ""DealerHistoryRecords"" (""Id"") ON DELETE CASCADE
            );",
            @"CREATE INDEX IF NOT EXISTS ""IX_DealerHistoryItems_OrgId_DealerHistoryRecordId"" ON ""DealerHistoryItems"" (""OrgId"", ""DealerHistoryRecordId"");",
            @"CREATE TABLE IF NOT EXISTS ""InsuranceDebits"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""DebitNo"" TEXT NOT NULL,
                ""InsuranceCompanyId"" INTEGER NOT NULL,
                ""InsNo"" TEXT NOT NULL,
                ""InsName"" TEXT NOT NULL,
                ""InsuranceContractId"" INTEGER NULL,
                ""ROId"" INTEGER NULL,
                ""RONo"" TEXT NULL,
                ""PlateNo"" TEXT NULL,
                ""CarModel"" TEXT NULL,
                ""CustomerName"" TEXT NULL,
                ""InsuranceClaimId"" INTEGER NULL,
                ""ClaimNo"" TEXT NULL,
                ""PolicyNo"" TEXT NULL,
                ""DebitType"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""DebitDate"" TEXT NOT NULL,
                ""DueDate"" TEXT NULL,
                ""DebitAmount"" TEXT NOT NULL,
                ""PaidAmount"" TEXT NOT NULL DEFAULT '0',
                ""Description"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""ClearedAt"" TEXT NULL,
                ""CancelledReason"" TEXT NULL,
                FOREIGN KEY (""InsuranceCompanyId"") REFERENCES ""InsuranceCompanies"" (""Id"") ON DELETE RESTRICT
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_InsuranceDebits_OrgId_DebitNo"" ON ""InsuranceDebits"" (""OrgId"", ""DebitNo"");",
            @"CREATE TABLE IF NOT EXISTS ""InsuranceDebitPayments"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""PaymentNo"" TEXT NOT NULL,
                ""InsuranceCompanyId"" INTEGER NOT NULL,
                ""InsNo"" TEXT NOT NULL,
                ""InsName"" TEXT NOT NULL,
                ""InsuranceDebitId"" INTEGER NULL,
                ""PaymentDate"" TEXT NOT NULL,
                ""PaymentAmount"" TEXT NOT NULL,
                ""Method"" INTEGER NOT NULL,
                ""PayPersonName"" TEXT NOT NULL,
                ""PayPersonIdCard"" TEXT NULL,
                ""PayPersonPhone"" TEXT NULL,
                ""BankAccount"" TEXT NULL,
                ""BankName"" TEXT NULL,
                ""TransactionRef"" TEXT NULL,
                ""Note"" TEXT NULL,
                ""Cashier"" TEXT NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                FOREIGN KEY (""InsuranceCompanyId"") REFERENCES ""InsuranceCompanies"" (""Id"") ON DELETE RESTRICT
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_InsuranceDebitPayments_OrgId_PaymentNo"" ON ""InsuranceDebitPayments"" (""OrgId"", ""PaymentNo"");",
            @"ALTER TABLE ""Customers"" ADD COLUMN ""CustomerGroupId"" INTEGER NULL;",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""CustomerGroupId"" INTEGER NULL;",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""CustomerGroupDiscountAmount"" TEXT NULL;",
            @"UPDATE ""ROs"" SET ""CustomerGroupDiscountAmount"" = '0' WHERE ""CustomerGroupDiscountAmount"" IS NULL;",
            @"CREATE TABLE IF NOT EXISTS ""CustomerGroups"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""GroupNo"" TEXT NOT NULL,
                ""GroupName"" TEXT NOT NULL,
                ""TaxCode"" TEXT NULL,
                ""Address"" TEXT NULL,
                ""Telephone"" TEXT NULL,
                ""Fax"" TEXT NULL,
                ""Email"" TEXT NULL,
                ""ContactPerson"" TEXT NULL,
                ""ContactPhone"" TEXT NULL,
                ""Description"" TEXT NULL,
                ""IsActive"" INTEGER NOT NULL DEFAULT 1,
                ""DiscountPercentLabor"" TEXT NOT NULL DEFAULT '0',
                ""DiscountPercentPart"" TEXT NOT NULL DEFAULT '0',
                ""CreditLimit"" TEXT NOT NULL DEFAULT '0',
                ""PaymentTermDays"" INTEGER NOT NULL DEFAULT 30,
                ""ContractNo"" TEXT NULL,
                ""ContractStartDate"" TEXT NULL,
                ""ContractEndDate"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""UpdatedAt"" TEXT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CustomerGroups_OrgId_GroupNo"" ON ""CustomerGroups"" (""OrgId"", ""GroupNo"");",
            @"CREATE TABLE IF NOT EXISTS ""CustomerGroupMembers"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""CustomerGroupId"" INTEGER NOT NULL,
                ""CustomerId"" INTEGER NULL,
                ""CarId"" INTEGER NOT NULL,
                ""PlateNo"" TEXT NOT NULL,
                ""DriverName"" TEXT NULL,
                ""DriverPhone"" TEXT NULL,
                ""Note"" TEXT NULL,
                ""JoinedDate"" TEXT NOT NULL,
                ""IsActive"" INTEGER NOT NULL DEFAULT 1,
                FOREIGN KEY (""CustomerGroupId"") REFERENCES ""CustomerGroups"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""CarId"") REFERENCES ""Cars"" (""Id"") ON DELETE RESTRICT
            );",
            @"CREATE INDEX IF NOT EXISTS ""IX_CustomerGroupMembers_OrgId_CustomerGroupId"" ON ""CustomerGroupMembers"" (""OrgId"", ""CustomerGroupId"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_CustomerGroupMembers_OrgId_CarId"" ON ""CustomerGroupMembers"" (""OrgId"", ""CarId"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_CustomerGroupMembers_OrgId_PlateNo"" ON ""CustomerGroupMembers"" (""OrgId"", ""PlateNo"");",
            @"CREATE TABLE IF NOT EXISTS ""PartPriceRequests"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""ReqPartPriceNo"" TEXT NOT NULL,
                ""DealerCode"" TEXT NOT NULL,
                ""DealerName"" TEXT NOT NULL,
                ""Description"" TEXT NOT NULL,
                ""TSTReqPartPriceID"" TEXT NULL,
                ""TSTSentDate"" TEXT NULL,
                ""DMSStatus"" INTEGER NOT NULL,
                ""TSTStatus"" INTEGER NOT NULL,
                ""FlagIsCheck"" INTEGER NOT NULL DEFAULT 0,
                ""IsUpdatePrice"" INTEGER NOT NULL DEFAULT 0,
                ""UpdatedPriceAt"" TEXT NULL,
                ""EffectiveDate"" TEXT NULL,
                ""EstimatedResponseDate"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""ApprovedBy"" TEXT NULL,
                ""ApprovedAt"" TEXT NULL,
                ""ROId"" INTEGER NULL,
                ""VIN"" TEXT NULL,
                ""CarModel"" TEXT NULL,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PartPriceRequests_OrgId_ReqPartPriceNo"" ON ""PartPriceRequests"" (""OrgId"", ""ReqPartPriceNo"");",
            @"CREATE TABLE IF NOT EXISTS ""PartPriceRequestLines"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""PartPriceRequestId"" INTEGER NOT NULL,
                ""PartId"" INTEGER NULL,
                ""DMSPartCode"" TEXT NOT NULL,
                ""VieName"" TEXT NOT NULL,
                ""VINCode"" TEXT NULL,
                ""DeliveryForm"" INTEGER NOT NULL,
                ""Quantity"" TEXT NOT NULL,
                ""Unit"" TEXT NOT NULL,
                ""Remark"" TEXT NULL,
                ""TSTPartCode"" TEXT NULL,
                ""TSTPrice"" TEXT NOT NULL DEFAULT '0',
                ""DateEffect"" TEXT NULL,
                ""Status"" INTEGER NOT NULL DEFAULT 0,
                FOREIGN KEY (""PartPriceRequestId"") REFERENCES ""PartPriceRequests"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""PartId"") REFERENCES ""Parts"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE INDEX IF NOT EXISTS ""IX_PartPriceRequestLines_OrgId_PartPriceRequestId"" ON ""PartPriceRequestLines"" (""OrgId"", ""PartPriceRequestId"");",
            @"CREATE TABLE IF NOT EXISTS ""ComplaintDiagnosticErrors"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""ErrorCode"" TEXT NOT NULL,
                ""ErrorName"" TEXT NOT NULL,
                ""ErrorType"" INTEGER NOT NULL,
                ""SystemGroup"" INTEGER NOT NULL,
                ""ErrorDesc"" TEXT NULL,
                ""Remark"" TEXT NULL,
                ""FlagActive"" INTEGER NOT NULL DEFAULT 1,
                ""UsageCount"" INTEGER NOT NULL DEFAULT 0,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""UpdatedAt"" TEXT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ComplaintDiagnosticErrors_OrgId_ErrorCode"" ON ""ComplaintDiagnosticErrors"" (""OrgId"", ""ErrorCode"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_ComplaintDiagnosticErrors_OrgId_ErrorType"" ON ""ComplaintDiagnosticErrors"" (""OrgId"", ""ErrorType"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_ComplaintDiagnosticErrors_OrgId_SystemGroup"" ON ""ComplaintDiagnosticErrors"" (""OrgId"", ""SystemGroup"");",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""ErrorCodePN"" TEXT NULL;",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""ErrorCodeCD"" TEXT NULL;",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""DiagnosticResult"" TEXT NULL;",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""IsReRepair"" INTEGER NOT NULL DEFAULT 0;",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""ReRepairParentROId"" INTEGER NULL;",
            @"CREATE TABLE IF NOT EXISTS ""CustomerCare72hs"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""Care72No"" TEXT NOT NULL,
                ""ROId"" INTEGER NOT NULL,
                ""CarId"" INTEGER NOT NULL,
                ""CustomerId"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""ROFinishedDate"" TEXT NOT NULL,
                ""ScheduledDate"" TEXT NOT NULL,
                ""ContactedDate"" TEXT NULL,
                ""ContactedBy"" TEXT NULL,
                ""ServiceExplained"" INTEGER NULL,
                ""BasicNeedsMet"" INTEGER NULL,
                ""HasTechnicalProblem"" INTEGER NOT NULL DEFAULT 0,
                ""ProblemDetails"" TEXT NULL,
                ""FixedRightFirstTime"" INTEGER NULL,
                ""SatisfactionRating"" INTEGER NULL,
                ""CustomerFeedback"" TEXT NULL,
                ""IsReRepairAlert"" INTEGER NOT NULL DEFAULT 0,
                ""ReRepairAction"" TEXT NULL,
                ""ReRepairROId"" INTEGER NULL,
                ""ReRepairAppointmentId"" INTEGER NULL,
                ""InternalNote"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""CarId"") REFERENCES ""Cars"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""CustomerId"" ) REFERENCES ""Customers"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""ReRepairROId"") REFERENCES ""ROs"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""ReRepairAppointmentId"") REFERENCES ""Appointments"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CustomerCare72hs_OrgId_Care72No"" ON ""CustomerCare72hs"" (""OrgId"", ""Care72No"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_CustomerCare72hs_OrgId_ROId"" ON ""CustomerCare72hs"" (""OrgId"", ""ROId"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_CustomerCare72hs_OrgId_CarId"" ON ""CustomerCare72hs"" (""OrgId"", ""CarId"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_CustomerCare72hs_OrgId_CustomerId"" ON ""CustomerCare72hs"" (""OrgId"", ""CustomerId"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_CustomerCare72hs_OrgId_Status"" ON ""CustomerCare72hs"" (""OrgId"", ""Status"");",
            @"ALTER TABLE ""Customers"" ADD COLUMN ""DateOfBirth"" TEXT NULL;",
            @"ALTER TABLE ""Customers"" ADD COLUMN ""Address"" TEXT NULL;",
            @"ALTER TABLE ""Customers"" ADD COLUMN ""Gender"" TEXT NULL;",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""BirthdayDiscountAmount"" NUMERIC NOT NULL DEFAULT 0;",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""BirthdayCareId"" INTEGER NULL;",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""BirthdayVoucherCode"" TEXT NULL;",
            @"ALTER TABLE ""Appointments"" ADD COLUMN ""CustomerCareBirthdayId"" INTEGER NULL;",
            @"CREATE TABLE IF NOT EXISTS ""CustomerCareBirthdays"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""CareBthNo"" TEXT NOT NULL,
                ""CustomerId"" INTEGER NOT NULL,
                ""CarId"" INTEGER NULL,
                ""DateOfBirth"" TEXT NULL,
                ""DateBth"" TEXT NOT NULL,
                ""Status"" INTEGER NOT NULL DEFAULT 0,
                ""ContactDate"" TEXT NULL,
                ""ContactedBy"" TEXT NULL,
                ""ContactChannel"" INTEGER NOT NULL DEFAULT 0,
                ""Remark"" TEXT NULL,
                ""GiftVoucherCode"" TEXT NULL,
                ""GiftVoucherValue"" NUMERIC NOT NULL DEFAULT 300000,
                ""DiscountPercent"" NUMERIC NOT NULL DEFAULT 10,
                ""VoucherValidUntil"" TEXT NULL,
                ""IsVoucherUsed"" INTEGER NOT NULL DEFAULT 0,
                ""UsedInROId"" INTEGER NULL,
                ""AppointmentId"" INTEGER NULL,
                ""CreatedBy"" TEXT NOT NULL DEFAULT 'system',
                ""CreatedAt"" TEXT NOT NULL,
                ""UpdatedAt"" TEXT NULL,
                ""UpdatedBy"" TEXT NULL,
                ""LogLuDateTime"" TEXT NULL,
                ""LogLUBy"" TEXT NULL,
                FOREIGN KEY (""CustomerId"") REFERENCES ""Customers"" (""Id"") ON DELETE RESTRICT,
                FOREIGN KEY (""CarId"") REFERENCES ""Cars"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""UsedInROId"") REFERENCES ""ROs"" (""Id"") ON DELETE SET NULL,
                FOREIGN KEY (""AppointmentId"") REFERENCES ""Appointments"" (""Id"") ON DELETE SET NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CustomerCareBirthdays_OrgId_CareBthNo"" ON ""CustomerCareBirthdays"" (""OrgId"", ""CareBthNo"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_CustomerCareBirthdays_OrgId_CustomerId"" ON ""CustomerCareBirthdays"" (""OrgId"", ""CustomerId"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_CustomerCareBirthdays_OrgId_DateBth"" ON ""CustomerCareBirthdays"" (""OrgId"", ""DateBth"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_CustomerCareBirthdays_OrgId_Status"" ON ""CustomerCareBirthdays"" (""OrgId"", ""Status"");",
            @"CREATE TABLE IF NOT EXISTS ""WarrantyWorks"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""Code"" TEXT NOT NULL,
                ""Name"" TEXT NOT NULL,
                ""Model"" TEXT NOT NULL,
                ""LaborGroup"" INTEGER NOT NULL,
                ""CoverageType"" INTEGER NOT NULL,
                ""AppTypeCode"" TEXT NULL,
                ""EngineType"" TEXT NULL,
                ""RateHour"" TEXT NOT NULL,
                ""RatePrice"" TEXT NOT NULL,
                ""Price"" TEXT NOT NULL,
                ""VatPercent"" INTEGER NOT NULL,
                ""RequiredPhotos"" TEXT NULL,
                ""Remark"" TEXT NULL,
                ""FlagActive"" INTEGER NOT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""UpdatedAt"" TEXT NULL,
                ""UpdatedBy"" TEXT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_WarrantyWorks_OrgId_Code"" ON ""WarrantyWorks"" (""OrgId"", ""Code"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_WarrantyWorks_OrgId_Model"" ON ""WarrantyWorks"" (""OrgId"", ""Model"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_WarrantyWorks_OrgId_LaborGroup"" ON ""WarrantyWorks"" (""OrgId"", ""LaborGroup"");",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""MaintenanceSettingId"" INTEGER NULL;",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""MaintenanceMilestone"" TEXT NULL;",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""ReminderMaintanceDate"" TEXT NULL;",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""ReminderMaintanceKm"" INTEGER NULL;",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""WorkDoneSoon"" INTEGER NOT NULL DEFAULT 0;",
            @"ALTER TABLE ""ROs"" ADD COLUMN ""MemberNo"" TEXT NULL;",
            @"CREATE TABLE IF NOT EXISTS ""MaintenanceSettings"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""ROMSID"" TEXT NOT NULL,
                ""Name"" TEXT NOT NULL,
                ""Km"" INTEGER NOT NULL,
                ""Maintances"" INTEGER NOT NULL DEFAULT 1,
                ""Level"" INTEGER NOT NULL DEFAULT 1,
                ""MonthsInterval"" INTEGER NOT NULL DEFAULT 6,
                ""TakingTimeHours"" TEXT NOT NULL DEFAULT '1.0',
                ""EstimatedCost"" TEXT NOT NULL DEFAULT '650000',
                ""ServicePackageId"" INTEGER NULL,
                ""RequiredChecklist"" TEXT NULL,
                ""Description"" TEXT NULL,
                ""FlagWarranty"" INTEGER NOT NULL DEFAULT 1,
                ""FlagActive"" INTEGER NOT NULL DEFAULT 1,
                ""LogLuDateTime"" TEXT NULL,
                ""LogLUBy"" TEXT NULL,
                ""CreatedBy"" TEXT NOT NULL DEFAULT 'Hệ thống HTC',
                ""CreatedAt"" TEXT NOT NULL,
                ""UpdatedAt"" TEXT NULL,
                ""UpdatedBy"" TEXT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_MaintenanceSettings_OrgId_ROMSID"" ON ""MaintenanceSettings"" (""OrgId"", ""ROMSID"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_MaintenanceSettings_OrgId_Km"" ON ""MaintenanceSettings"" (""OrgId"", ""Km"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_MaintenanceSettings_OrgId_Level"" ON ""MaintenanceSettings"" (""OrgId"", ""Level"");",
            @"CREATE TABLE IF NOT EXISTS ""WarrantyPhotoTypes"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""ROWPTCode"" TEXT NOT NULL,
                ""ROWPTName"" TEXT NOT NULL,
                ""FlagActive"" INTEGER NOT NULL DEFAULT 1,
                ""CreatedAt"" TEXT NOT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_WarrantyPhotoTypes_OrgId_ROWPTCode"" ON ""WarrantyPhotoTypes"" (""OrgId"", ""ROWPTCode"");",
            @"CREATE TABLE IF NOT EXISTS ""WarrantyTypes"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""ROWTID"" TEXT NOT NULL,
                ""TypeCode"" INTEGER NOT NULL,
                ""TypeName"" TEXT NOT NULL,
                ""DetailCode"" INTEGER NOT NULL,
                ""DetailName"" TEXT NOT NULL,
                ""PhotoTypeDisplay"" TEXT NULL,
                ""FlagActive"" INTEGER NOT NULL DEFAULT 1,
                ""LogLuDateTime"" TEXT NULL,
                ""LogLUBy"" TEXT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                ""UpdatedAt"" TEXT NULL,
                ""UpdatedBy"" TEXT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_WarrantyTypes_OrgId_TypeCode_DetailCode"" ON ""WarrantyTypes"" (""OrgId"", ""TypeCode"", ""DetailCode"");",
            @"CREATE TABLE IF NOT EXISTS ""WarrantyTypePhotos"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""WarrantyTypeId"" INTEGER NOT NULL,
                ""ROWPTCode"" TEXT NOT NULL,
                ""ROWPTName"" TEXT NULL,
                FOREIGN KEY (""WarrantyTypeId"") REFERENCES ""WarrantyTypes"" (""Id"") ON DELETE CASCADE
            );",
            @"CREATE INDEX IF NOT EXISTS ""IX_WarrantyTypePhotos_OrgId_WarrantyTypeId"" ON ""WarrantyTypePhotos"" (""OrgId"", ""WarrantyTypeId"");",
            @"CREATE TABLE IF NOT EXISTS ""CustomerTypes"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""CusTypeCode"" TEXT NOT NULL,
                ""CusTypeName"" TEXT NOT NULL,
                ""CusFactor"" TEXT NOT NULL DEFAULT '1',
                ""CusPersonType"" TEXT NOT NULL DEFAULT 'Personal',
                ""IsActive"" INTEGER NOT NULL DEFAULT 1,
                ""CreatedAt"" TEXT NOT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CustomerTypes_OrgId_CusTypeCode"" ON ""CustomerTypes"" (""OrgId"", ""CusTypeCode"");",
            @"CREATE TABLE IF NOT EXISTS ""CusServiceFactors"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""ServiceItemId"" INTEGER NOT NULL,
                ""CustomerTypeId"" INTEGER NOT NULL,
                ""Factor"" TEXT NOT NULL DEFAULT '1',
                ""DealerCode"" TEXT NULL,
                ""LogLUBy"" TEXT NULL,
                ""LogLUDateTime"" TEXT NULL,
                ""CreatedAt"" TEXT NOT NULL,
                FOREIGN KEY (""ServiceItemId"") REFERENCES ""ServiceItems"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""CustomerTypeId"") REFERENCES ""CustomerTypes"" (""Id"") ON DELETE CASCADE
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CusServiceFactors_OrgId_ServiceItemId_CustomerTypeId"" ON ""CusServiceFactors"" (""OrgId"", ""ServiceItemId"", ""CustomerTypeId"");",
            @"CREATE TABLE IF NOT EXISTS ""RoHistories"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""ROId"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL DEFAULT 0,
                ""HistoryDate"" TEXT NOT NULL,
                ""UserCode"" TEXT NULL,
                ""Note"" TEXT NULL,
                FOREIGN KEY (""ROId"") REFERENCES ""ROs"" (""Id"") ON DELETE CASCADE
            );",
            @"CREATE INDEX IF NOT EXISTS ""IX_RoHistories_OrgId_ROId"" ON ""RoHistories"" (""OrgId"", ""ROId"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_RoHistories_OrgId_Status"" ON ""RoHistories"" (""OrgId"", ""Status"");",
            @"CREATE TABLE IF NOT EXISTS ""CarModels"" (
                ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ""OrgId"" TEXT NOT NULL,
                ""ModelCode"" TEXT NOT NULL,
                ""ModelName"" TEXT NOT NULL,
                ""TradeMarkCode"" TEXT NOT NULL,
                ""ProductionCode"" TEXT NULL,
                ""DealerCode"" TEXT NULL,
                ""Segment"" INTEGER NOT NULL DEFAULT 0,
                ""ProductYear"" INTEGER NULL,
                ""IsActive"" INTEGER NOT NULL DEFAULT 1,
                ""CreatedBy"" TEXT NOT NULL DEFAULT 'web',
                ""CreatedAt"" TEXT NOT NULL,
                ""LogLUBy"" TEXT NULL,
                ""LogLUDateTime"" TEXT NULL
            );",
            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CarModels_OrgId_ModelCode"" ON ""CarModels"" (""OrgId"", ""ModelCode"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_CarModels_OrgId_TradeMarkCode"" ON ""CarModels"" (""OrgId"", ""TradeMarkCode"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_CarModels_OrgId_Segment"" ON ""CarModels"" (""OrgId"", ""Segment"");"
        };

        // Định mức vật tư tối thiểu (Mst_BOM / Mst_BOMDtl) — SQLite
        var bomSqls = new[]
        {
            "CREATE TABLE IF NOT EXISTS \"Boms\" (\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"OrgId\" TEXT NOT NULL, \"BomCode\" TEXT NOT NULL, \"BomDesc\" TEXT NOT NULL, \"Remark\" TEXT NULL, \"IsActive\" INTEGER NOT NULL DEFAULT 1, \"CreatedBy\" TEXT NOT NULL DEFAULT 'web', \"CreatedAt\" TEXT NOT NULL, \"LogLUBy\" TEXT NULL, \"LogLUDateTime\" TEXT NULL);",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Boms_OrgId_BomCode\" ON \"Boms\" (\"OrgId\", \"BomCode\");",
            "CREATE TABLE IF NOT EXISTS \"BomLines\" (\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"OrgId\" TEXT NOT NULL, \"BomId\" INTEGER NOT NULL, \"PartCode\" TEXT NOT NULL, \"PartName\" TEXT NOT NULL, \"Unit\" TEXT NOT NULL DEFAULT 'Cái', \"QtyMin\" TEXT NOT NULL DEFAULT '1', \"LogLUBy\" TEXT NULL, \"LogLUDateTime\" TEXT NULL, FOREIGN KEY (\"BomId\") REFERENCES \"Boms\" (\"Id\") ON DELETE CASCADE);",
            "CREATE INDEX IF NOT EXISTS \"IX_BomLines_OrgId_BomId\" ON \"BomLines\" (\"OrgId\", \"BomId\");"
        };
        foreach (var sql in bomSqls)
        {
            try { await db.Database.ExecuteSqlRawAsync(sql); } catch { }
        }

        foreach (var sql in sqls)
        {
            try { await db.Database.ExecuteSqlRawAsync(sql); } catch { }
        }
    }
}
