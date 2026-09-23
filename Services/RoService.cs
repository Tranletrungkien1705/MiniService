using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models;

namespace MiniService.Services;

public record SvcDash(int OpenRO, int InGarage, int DoneToday, decimal RevenueMonth, int Cars, int Parts, int LowStockParts,
    int PendingWarranty, decimal ApprovedWarrantyAmount,
    int TodayAppointments, int PendingAppointments,
    int PendingStockIns, decimal MonthStockInValue,
    int PendingStockOuts, decimal MonthStockOutValue,
    int PendingCustomerCares, int FeedbackCustomerCares,
    int PendingPayments, decimal MonthPaymentRevenue,
    int PendingQuotes, decimal MonthQuoteValue,
    int ServicePackages,
    int PendingOrderParts, decimal MonthOrderPartValue,
    int TotalCavities, int OccupiedCavities, int AvailableCavities,
    int PendingReceptions, int TodayReceptions,
    int ActiveAssignments, int TotalEngineers, int TotalGroups,
    int PendingInsuranceClaims, decimal ApprovedInsuranceAmount,
    int ActiveCampaigns, decimal MonthCampaignDiscount,
    int PendingCareMaces, int OverdueCareMaces, int BookedCareMaces,
    List<(ROStatus Status, int Count)> ByStatus,
    int PendingStockAdjs = 0, int DiscrepancyStockAdjs = 0,
    int ActiveBulletins = 0, int PendingBulletinVins = 0,
    int PendingPdiRequests = 0, int CompletedPdiVehicles = 0,
    int PendingOrderComplains = 0, int ApprovedOrderComplains = 0,
    int PendingPartOOs = 0, int StockAvailablePartOOs = 0,
    int ActiveCusDebits = 0, decimal TotalCusDebitBalance = 0, int OverdueCusDebits = 0,
    int TotalCustomerGroups = 0, int ActiveCustomerGroups = 0, int TotalFleetCars = 0,
    int PendingPartPriceRequests = 0, int RespondedPartPriceRequests = 0,
    int TotalComplaintDiagnosticErrors = 0, int TotalComplaintCodes = 0, int TotalDiagnosticCodes = 0,
    int TotalBirthdays = 0, int ThisMonthBirthdays = 0, int TodayBirthdays = 0, int PendingBirthdays = 0,
    int TotalWarrantyWorks = 0, int ActiveWarrantyWorks = 0,
    int TotalMaintenanceSettings = 0, int ActiveMaintenanceSettings = 0);

public interface IRoService
{
    // master
    Task<List<Customer>> CustomersAsync(string? q);
    Task<int> CreateCustomerAsync(Customer c);
    Task<List<Car>> CarsAsync(string? q);
    Task<int> CreateCarAsync(Car car);
    // parts & inventory
    Task<List<Part>> PartsAsync(string? q, bool? lowStockOnly);
    Task<Part?> GetPartAsync(int id);
    Task<int> CreatePartAsync(Part part);
    Task<(bool ok, string msg)> AdjustStockAsync(int partId, decimal qty, string mode, string? note);
    Task<List<Part>> PartsForSelectAsync();
    // RO
    Task<List<RepairOrder>> ROsAsync(ROStatus? status, string? q);
    Task<RepairOrder?> GetROAsync(int id);
    Task<int> CreateROAsync(RepairOrder ro);
    Task AddLineAsync(int roId, LineType type, string name, decimal qty, decimal price, int? partId = null, ExpenseType expenseType = ExpenseType.Customer, int? serviceItemId = null, decimal? stdManHour = null, int? warrantyWorkId = null);
    Task RemoveLineAsync(int lineId);
    Task<(bool ok, string msg)> TransitionAsync(int roId, ROStatus to);
    Task<(bool ok, string msg)> DeleteROAsync(int roId);
    Task<(bool ok, string msg)> UpdateMaintenanceReminderAsync(int roId, DateTime? reminderDate, int? reminderKm, bool workDoneSoon, string? memberNo, string updatedBy);
    Task<SvcDash> DashboardAsync();
    // warranty
    Task<List<WarrantyReport>> WarrantyReportsAsync(WarrantyStatus? status, string? q);
    Task<WarrantyReport?> GetWarrantyReportAsync(int id);
    Task<int> CreateWarrantyReportFromROAsync(int roId, string issueDesc, string diagResult, string? errCodeCD, string? errCodePN, int? partIdError, string createdBy);
    Task<(bool ok, string msg)> TransitionWarrantyAsync(int reportId, WarrantyStatus to, decimal? approvedAmount, string? note);
    Task<(bool ok, string msg)> DeleteWarrantyReportAsync(int reportId);
    Task<List<RepairOrder>> ROsEligibleForWarrantyAsync();
    // appointments (Ser_App)
    Task<List<Appointment>> AppointmentsAsync(AppointmentStatus? status, string? q, DateTime? date);
    Task<Appointment?> GetAppointmentAsync(int id);
    Task<int> CreateAppointmentAsync(Appointment app);
    Task<(bool ok, string msg)> TransitionAppointmentStatusAsync(int id, AppointmentStatus to, string? cancelReason = null);
    Task<(bool ok, string msg, int? roId)> CheckInAppointmentAsync(int id, int odometer, string? technician);
    Task<(bool ok, string msg)> DeleteAppointmentAsync(int id);
    // stock-in (Ser_Inv_StockIn)
    Task<List<StockIn>> StockInsAsync(StockInStatus? status, string? q, DateTime? fromDate, DateTime? toDate);
    Task<StockIn?> GetStockInAsync(int id);
    Task<int> CreateStockInAsync(StockIn stockIn, List<StockInDetail> items);
    Task<(bool ok, string msg)> TransitionStockInStatusAsync(int id, StockInStatus to, string? approvedBy = null, string? note = null);
    Task<(bool ok, string msg)> DeleteStockInAsync(int id);
    // stock-out (Ser_Inv_StockOut)
    Task<List<StockOut>> StockOutsAsync(StockOutStatus? status, string? q, DateTime? fromDate, DateTime? toDate, int? roId = null);
    Task<StockOut?> GetStockOutAsync(int id);
    Task<int> CreateStockOutAsync(StockOut stockOut, List<StockOutDetail> items);
    Task<(bool ok, string msg)> TransitionStockOutStatusAsync(int id, StockOutStatus to, string? approvedBy = null, string? note = null);
    Task<(bool ok, string msg)> DeleteStockOutAsync(int id);
    Task<List<RepairOrder>> ROsForStockOutAsync();
    // customer care 24h (Ser_CustomerCare24h)
    Task<List<CustomerCare>> CustomerCaresAsync(CustomerCareStatus? status, string? q);
    Task<CustomerCare?> GetCustomerCareAsync(int id);
    Task<int> CreateCustomerCareAsync(CustomerCare care);
    Task<(bool ok, string msg)> UpdateCustomerCareSurveyAsync(int id, CustomerCareStatus status, bool hasCarProblem, int? qualityRating, int? staffRating, bool? willingToReturn, int? facilityRating, string? feedback, string? internalNote, string? contactedBy);
    Task<(bool ok, string msg)> DeleteCustomerCareAsync(int id);
    Task<List<RepairOrder>> ROsEligibleForCustomerCareAsync();
    // customer care 72h & re-repair control (Ser_CustomerCare72h)
    Task<List<CustomerCare72h>> CustomerCare72hsAsync(CustomerCare72hStatus? status, string? q, bool? needFeedbackOnly = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<CustomerCare72h?> GetCustomerCare72hAsync(int id);
    Task<CustomerCare72hSummaryDto> GetCustomerCare72hSummaryAsync();
    Task<int> CreateCustomerCare72hAsync(CustomerCare72h care);
    Task<int> GenerateCustomerCare72hFromROAsync(int roId, string? createdBy = null);
    Task<List<RepairOrder>> ROsEligibleForCustomerCare72hAsync();
    Task<(bool ok, string msg)> UpdateCustomerCare72hSurveyAsync(int id, CustomerCare72hStatus status, bool? serviceExplained, bool? basicNeedsMet, bool hasTechnicalProblem, string? problemDetails, bool? fixedRightFirstTime, int? satisfactionRating, string? customerFeedback, string? reRepairAction, string? internalNote, string? contactedBy);
    Task<(bool ok, string msg, int? roId)> CreateReRepairFromCare72hAsync(int id, string? technician = null, string? note = null);
    Task<(bool ok, string msg)> DeleteCustomerCare72hAsync(int id);
    // customer care birthday & loyalty gifts (Ser_CustomerCareBth)
    Task<List<CustomerCareBirthday>> CustomerCareBirthdaysAsync(int? month, CustomerCareBirthdayStatus? status, string? q, bool? todayOnly = null);
    Task<CustomerCareBirthday?> GetCustomerCareBirthdayAsync(int id);
    Task<CustomerCareBirthdaySummaryDto> GetCustomerCareBirthdaySummaryAsync();
    Task<int> CreateCustomerCareBirthdayAsync(CustomerCareBirthday care);
    Task<(int generated, int skipped)> ScanAndGenerateBirthdayCaresAsync(int? year = null, string? createdBy = null);
    Task<(bool ok, string msg)> UpdateCustomerCareBirthdayContactAsync(int id, CustomerCareBirthdayStatus status, BirthdayContactChannel channel, string? remark, string? giftVoucherCode, decimal giftVoucherValue, decimal discountPercent, DateTime? validUntil, string? contactedBy);
    Task<(bool ok, string msg, int? appointmentId)> BookAppointmentFromBirthdayCareAsync(int id, DateTime appointmentDate, AppointmentServiceType serviceType, string? note);
    Task<(bool ok, string msg)> ApplyBirthdayVoucherToROAsync(int id, int roId);
    Task<(bool ok, string msg)> DeleteCustomerCareBirthdayAsync(int id);
    Task<List<Customer>> CustomersEligibleForBirthdayCareAsync(int year);
    // payment (Ser_Payment)
    Task<List<Payment>> PaymentsAsync(PaymentStatus? status, string? q, DateTime? fromDate, DateTime? toDate, int? roId = null);
    Task<Payment?> GetPaymentAsync(int id);
    Task<int> CreatePaymentAsync(Payment payment);
    Task<(bool ok, string msg)> TransitionPaymentStatusAsync(int id, PaymentStatus to, string? cashier = null, string? note = null);
    Task<(bool ok, string msg)> DeletePaymentAsync(int id);
    Task<List<RepairOrder>> ROsForPaymentAsync();
    // quotation (Ser_Inv_Quote)
    Task<List<Quote>> QuotesAsync(QuoteStatus? status, string? q, DateTime? fromDate, DateTime? toDate);
    Task<Quote?> GetQuoteAsync(int id);
    Task<int> CreateQuoteAsync(Quote quote, List<QuoteItem> items);
    Task<(bool ok, string msg)> TransitionQuoteStatusAsync(int id, QuoteStatus to);
    Task<(bool ok, string msg, int? stockOutId)> ConvertQuoteToStockOutAsync(int id, string? approvedBy = null);
    Task<(bool ok, string msg, int? roId)> ConvertQuoteToROAsync(int id, string? technician = null);
    Task<(bool ok, string msg)> DeleteQuoteAsync(int id);
    Task<List<Customer>> CustomersForSelectAsync();
    // service packages (Ser_ServicePackage)
    Task<List<ServicePackage>> ServicePackagesAsync(string? q, bool? isPublic, bool? isActive);
    Task<ServicePackage?> GetServicePackageAsync(int id);
    Task<int> CreateServicePackageAsync(ServicePackage package, List<ServicePackageItem> items);
    Task<(bool ok, string msg)> DeleteServicePackageAsync(int id);
    Task<List<ServicePackage>> ServicePackagesForSelectAsync();
    Task<(bool ok, string msg, int itemsAdded)> ApplyServicePackageToROAsync(int packageId, int roId);
    Task<(bool ok, string msg, int itemsAdded)> ApplyServicePackageToQuoteAsync(int packageId, int quoteId);
    // order parts (Ser_Order_Part)
    Task<List<OrderPart>> OrderPartsAsync(OrderPartStatus? status, string? q, DateTime? fromDate, DateTime? toDate);
    Task<OrderPart?> GetOrderPartAsync(int id);
    Task<int> CreateOrderPartAsync(OrderPart order, List<OrderPartLine> lines);
    Task<(bool ok, string msg)> TransitionOrderPartStatusAsync(int id, OrderPartStatus to, string? supplierOrderNo = null, string? note = null);
    Task<(bool ok, string msg, int? stockInId)> CreateStockInFromOrderPartAsync(int id, string? approvedBy = null);
    Task<(bool ok, string msg)> DeleteOrderPartAsync(int id);
    Task<List<RepairOrder>> ROsWaitingForPartAsync();
    // cavities (Ser_Cavity / Mst_Compartment)
    Task<List<Cavity>> CavitiesAsync(CavityType? type, CavityStatus? status, string? q);
    Task<Cavity?> GetCavityAsync(int id);
    Task<int> CreateCavityAsync(Cavity cavity);
    Task<(bool ok, string msg)> UpdateCavityAsync(int id, string cavityName, CavityType type, string? liftEquipment, string? areaZone, string? note);
    Task<(bool ok, string msg)> AssignCarToCavityAsync(int cavityId, int roId, string? technician, DateTime? expectedFinish = null);
    Task<(bool ok, string msg)> ReleaseCavityAsync(int cavityId, ROStatus? nextRoStatus = null);
    Task<(bool ok, string msg)> SetCavityStatusAsync(int cavityId, CavityStatus status, string? note = null);
    Task<(bool ok, string msg)> DeleteCavityAsync(int cavityId);
    Task<List<Cavity>> CavitiesForSelectAsync(CavityType? type = null);
    Task<List<RepairOrder>> ROsEligibleForCavityAsync();
    // reception & walk-around inspection (Ser_ReceptionF & Ser_ReceptionFDtl)
    Task<List<ReceptionSheet>> ReceptionsAsync(ReceptionStatus? status, string? q, DateTime? fromDate, DateTime? toDate);
    Task<ReceptionSheet?> GetReceptionAsync(int id);
    Task<int> CreateReceptionAsync(ReceptionSheet sheet, List<ReceptionItem>? items = null);
    Task<(bool ok, string msg, int? roId)> CreateROFromReceptionAsync(int id, string? technician = null);
    Task<(bool ok, string msg)> DeliverCarAsync(int id, string? deliveryBy, string? note, List<(int itemId, AuditStatus status)>? deliveryItems = null);
    Task<(bool ok, string msg)> CancelReceptionAsync(int id, string? reason = null);
    Task<(bool ok, string msg)> DeleteReceptionAsync(int id);
    Task<List<Appointment>> AppointmentsEligibleForReceptionAsync();
    List<ReceptionItem> GetDefaultChecklistItems();
    // assignment of work & technician dispatch (Ser_AssignmentWork, Ser_AssignmentWorkEngineer, Ser_Engineer, Ser_GroupRepair)
    Task<List<GroupRepair>> GroupRepairsAsync(string? q, bool? isActive);
    Task<GroupRepair?> GetGroupRepairAsync(int id);
    Task<int> CreateGroupRepairAsync(GroupRepair group);
    Task<List<GroupRepair>> GroupRepairsForSelectAsync();
    Task<(bool ok, string msg)> DeleteGroupRepairAsync(int id);
    Task<List<Engineer>> EngineersAsync(string? q, int? groupId, bool? isActive);
    Task<Engineer?> GetEngineerAsync(int id);
    Task<int> CreateEngineerAsync(Engineer eng);
    Task<List<Engineer>> EngineersForSelectAsync();
    Task<(bool ok, string msg)> DeleteEngineerAsync(int id);
    Task<List<AssignmentWork>> AssignmentWorksAsync(AssignmentWorkStatus? status, string? q, DateTime? fromDate, DateTime? toDate, int? roId = null);
    Task<AssignmentWork?> GetAssignmentWorkAsync(int id);
    Task<int> CreateAssignmentWorkAsync(AssignmentWork assignment, List<AssignmentEngineer> engineers);
    Task<(bool conflict, string? conflictMessage)> CheckCavityConflictAsync(int cavityId, DateTime start, DateTime end, int? excludeAssignmentId = null);
    Task<(bool ok, string msg)> StartAssignmentWorkAsync(int id, string? startedBy = null);
    Task<(bool ok, string msg)> CompleteAssignmentWorkAsync(int id, string? completedBy = null);
    Task<(bool ok, string msg)> CancelAssignmentWorkAsync(int id, string? reason = null);
    Task<(bool ok, string msg)> DeleteAssignmentWorkAsync(int id);
    Task<List<RepairOrder>> ROsEligibleForAssignmentAsync();
    // insurance management (Ser_Insurance, Ser_InsuranceContract, Ser_InsuranceDebit)
    Task<List<InsuranceCompany>> InsuranceCompaniesAsync(string? q, bool? isActive = null);
    Task<InsuranceCompany?> GetInsuranceCompanyAsync(int id);
    Task<int> CreateInsuranceCompanyAsync(InsuranceCompany c);
    Task<List<InsuranceCompany>> InsuranceCompaniesForSelectAsync();
    Task<List<InsuranceContract>> InsuranceContractsAsync(int? companyId = null, bool? activeOnly = null);
    Task<InsuranceContract?> GetInsuranceContractAsync(int id);
    Task<int> CreateInsuranceContractAsync(InsuranceContract c);
    Task<List<InsuranceContract>> InsuranceContractsForSelectAsync(int? companyId = null);
    Task<List<InsuranceClaim>> InsuranceClaimsAsync(InsuranceClaimStatus? status = null, string? q = null, int? roId = null, int? companyId = null);
    Task<InsuranceClaim?> GetInsuranceClaimAsync(int id);
    Task<int> CreateInsuranceClaimAsync(InsuranceClaim claim, List<InsuranceClaimItem>? items = null);
    Task<int> CreateInsuranceClaimFromROAsync(int roId, int companyId, int? contractId, string policyNo, string? claimFileNo, string? surveyorName, string? surveyorPhone, string accidentDesc, decimal deductibleAmount, decimal penaltyAmount, string createdBy);
    Task<(bool ok, string msg)> TransitionInsuranceClaimAsync(int id, InsuranceClaimStatus toStatus, decimal? approvedAmount = null, string? note = null);
    Task<(bool ok, string msg)> DeleteInsuranceClaimAsync(int id);
    Task<List<RepairOrder>> ROsEligibleForInsuranceAsync();
    // service marketing campaigns (Ser_CampaignMarketing, Ser_CampaignMarketingPart)
    Task<List<CampaignMarketing>> CampaignMarketingsAsync(CampaignMarketingStatus? status, string? q, bool? currentOnly);
    Task<CampaignMarketing?> GetCampaignMarketingAsync(int id);
    Task<int> CreateCampaignMarketingAsync(CampaignMarketing campaign, List<CampaignMarketingItem> items);
    Task<(bool ok, string msg)> TransitionCampaignStatusAsync(int id, CampaignMarketingStatus to, string? approvedBy = null);
    Task<(bool ok, string msg)> DeleteCampaignMarketingAsync(int id);
    Task<List<CampaignMarketing>> GetEligibleCampaignsForCarAsync(int carId);
    Task<(bool ok, string msg, decimal discountTotal)> ApplyCampaignToROAsync(int campaignId, int roId);
    Task<(bool ok, string msg)> RemoveCampaignFromROAsync(int roId);
    Task<List<RepairOrder>> ROsEligibleForCampaignAsync(int campaignId);
    Task<List<CampaignMarketing>> ActiveCampaignsForSelectAsync();
    // periodic maintenance reminders (Ser_CustomerCareMace / MH 83)
    Task<List<CustomerCareMace>> CustomerCareMacesAsync(CustomerCareMaceStatus? status, MaceType? maceType, string? timeFilter, string? q);
    Task<CustomerCareMace?> GetCustomerCareMaceAsync(int id);
    Task<int> CreateCustomerCareMaceAsync(CustomerCareMace mace);
    Task<(bool ok, string msg)> UpdateCustomerCareMaceCallAsync(int id, CustomerCareMaceStatus status, DateTime? contactDate, DateTime? apointDate, string? remark, string? contactBy);
    Task<(bool ok, string msg, int? appointmentId)> ConvertMaceToAppointmentAsync(int id, string? advisor = null, string? cavity = null, string? note = null);
    Task<(bool ok, string msg)> DeleteCustomerCareMaceAsync(int id);
    Task<(DateTime recommendDate, MaceType maceType, int nextKm)> CalculateNextMaintenanceAsync(int carId, DateTime? referenceDate = null, int? currentOdometer = null);
    // stock adjustments & inventory transfer (Ser_Inv_StockAdj & Ser_Inv_StockAdjDetail)
    Task<List<StockAdj>> StockAdjsAsync(StockAdjStatus? status, StockAdjType? type, string? q, DateTime? fromDate, DateTime? toDate);
    Task<StockAdj?> GetStockAdjAsync(int id);
    Task<int> CreateStockAdjAsync(StockAdj adj, List<StockAdjDetail> items);
    Task<(bool ok, string msg)> TransitionStockAdjStatusAsync(int id, StockAdjStatus to, string? approvedBy = null, string? note = null);
    Task<(bool ok, string msg)> UpdateStockAdjItemsAsync(int id, List<(int itemId, decimal actualQty, string? toLoc, string? note)> updates);
    Task<(bool ok, string msg)> DeleteStockAdjAsync(int id);
    Task<List<Part>> PartsForStockAdjAsync(string? q = null);
    // Technical Service Bulletins & Recall Campaigns (Btl_Bulletin, Btl_BulletinDtl, Btl_Bulletin_VIN)
    Task<List<Bulletin>> BulletinsAsync(BulletinStatus? status, string? q, bool? activeOnly = null);
    Task<Bulletin?> GetBulletinAsync(int id);
    Task<int> CreateBulletinAsync(Bulletin bulletin, List<BulletinDetail> details, List<BulletinVin> vins);
    Task<(bool ok, string msg)> ToggleBulletinActiveAsync(int id);
    Task<(bool ok, string msg)> TransitionBulletinStatusAsync(int id, BulletinStatus to);
    Task<(bool ok, string msg)> DeleteBulletinAsync(int id);
    Task<List<BulletinVin>> CheckVinBulletinsAsync(string vin);
    Task<(bool ok, string msg)> UpdateBulletinVinStatusAsync(int vinId, BulletinVinStatus status, string? doneBy = null, int? roId = null, string? roNo = null);
    Task<(bool ok, string msg, int itemsAdded)> ApplyBulletinToROAsync(int bulletinId, int roId);
    Task<(bool ok, string msg)> AddVinsToBulletinAsync(int bulletinId, List<string> vinList, string? model = null, string? dealerCode = null);
    // Pre-Delivery Inspection (PDI - Dlr_PDIRequest & Dlr_PDIRequestDtl)
    Task<List<PdiRequest>> PdiRequestsAsync(PdiRequestStatus? status, string? q, DateTime? fromDate, DateTime? toDate);
    Task<PdiRequest?> GetPdiRequestAsync(int id);
    Task<PdiRequestItem?> GetPdiRequestItemAsync(int itemId);
    Task<int> CreatePdiRequestAsync(PdiRequest req, List<PdiRequestItem> items);
    Task<(bool ok, string msg)> TransitionPdiRequestStatusAsync(int id, PdiRequestStatus to, string? approvedBy = null);
    Task<(bool ok, string msg, int? roId)> CreateROFromPdiItemAsync(int itemId, string? technician = null);
    Task<(bool ok, string msg)> UpdatePdiItemChecklistAsync(int itemId, List<(int checkId, AuditStatus status, string? note)> updates, string? inspector, string? notes);
    Task<(bool ok, string msg)> PassPdiItemAsync(int itemId, string? inspector = null);
    Task<(bool ok, string msg)> DeletePdiRequestAsync(int id);
    List<PdiChecklistItem> GetDefaultPdiChecklist();
    // Order Part Complaints against Supplier TST/HTC (Ser_OrderComplain & Ser_OrderComplainAttachFile)
    Task<List<OrderComplain>> OrderComplainsAsync(DMSOrderComplainStatus? dmsStatus, TSTOrderComplainStatus? tstStatus, OrderComplainType? type, string? q, DateTime? fromDate, DateTime? toDate);
    Task<OrderComplain?> GetOrderComplainAsync(int id);
    Task<int> CreateOrderComplainAsync(OrderComplain complain, List<OrderComplainAttachFile>? files = null);
    Task<(bool ok, string msg)> SendOrderComplainToTSTAsync(int id);
    Task<(bool ok, string msg)> ReviewOrderComplainAsync(int id, TSTOrderComplainStatus tstStatus, ComplainSolution solution, string? solutionNote);
    Task<(bool ok, string msg)> DeleteOrderComplainAsync(int id);
    Task<List<OrderPart>> OrderPartsForComplainSelectAsync();
    // Technical Library & Re-Repair Knowledge Base (Ser_Technical_Library / MH 63)
    Task<List<TechnicalLibrary>> TechnicalLibrariesAsync(string? model, TechnicalLibraryReRepairType? reRepairType, TechnicalLibraryType? type, bool? isActive, string? q);
    Task<TechnicalLibrary?> GetTechnicalLibraryAsync(int id);
    Task<TechnicalLibrary?> GetTechnicalLibraryByCodeAsync(string code);
    Task<int> CreateTechnicalLibraryAsync(TechnicalLibrary item);
    Task<(bool ok, string msg)> ApproveTechnicalLibraryAsync(int id, string? approvedBy = null);
    Task<(bool ok, string msg)> DeleteTechnicalLibraryAsync(int id);
    Task<List<TechnicalLibrary>> SearchSolutionsForRoAsync(int roId);
    Task<List<string>> GetDistinctModelsAsync();
    // Master Services & Flat Rate Labor Operations (Ser_MST_Service)
    Task<List<ServiceItem>> ServiceItemsAsync(ServiceROType? roType, string? model, bool? isActive, bool? flagWarranty, string? q);
    Task<ServiceItem?> GetServiceItemAsync(int id);
    Task<ServiceItem?> GetServiceItemByCodeAsync(string code);
    Task<int> CreateServiceItemAsync(ServiceItem item);
    Task<(bool ok, string msg)> UpdateServiceItemAsync(ServiceItem item);
    Task<(bool ok, string msg)> DeleteServiceItemAsync(int id);
    Task<List<ServiceItem>> ServiceItemsForSelectAsync();
    Task<(bool ok, string msg)> AddServiceItemToROAsync(int roId, int serviceItemId, ExpenseType expenseType, decimal? customHours = null, decimal? customPrice = null, string? note = null);
    Task<List<string>> GetDistinctServiceModelsAsync();
    // Car Model Master (Ser_Mst_Model / Mst_CarModelStd)
    Task<List<CarModel>> CarModelsAsync(string? tradeMarkCode, CarModelSegment? segment, bool? isActive, string? q);
    Task<CarModel?> GetCarModelAsync(int id);
    Task<CarModel?> GetCarModelByCodeAsync(string modelCode);
    Task<int> CreateCarModelAsync(CarModel model);
    Task<(bool ok, string msg)> UpdateCarModelAsync(CarModel model);
    Task<(bool ok, string msg)> DeleteCarModelAsync(int id);
    Task<CarModelSummaryDto> GetCarModelSummaryAsync();
    Task<List<string>> GetDistinctTradeMarksAsync();
    // Supplier Management & Return Parts to Supplier (Ser_Mst_Supplier, Ser_SupplierPayment, Ser_SupplierPaymentDtl / MNU_QT_DL_QUANLYPHIEUXUATTRANHACUNGCAP)
    Task<List<Supplier>> SuppliersAsync(string? q);
    Task<Supplier?> GetSupplierAsync(int id);
    Task<int> CreateSupplierAsync(Supplier supplier);
    Task<List<SupplierPayment>> SupplierPaymentsAsync(SupplierPaymentStatus? status, SupplierPaymentType? type, string? q, DateTime? fromDate, DateTime? toDate);
    Task<SupplierPayment?> GetSupplierPaymentAsync(int id);
    Task<SupplierPayment?> GetSupplierPaymentByNoAsync(string supplierPaymentNo);
    Task<int> CreateSupplierPaymentAsync(SupplierPayment payment, List<SupplierPaymentDetail> items);
    Task<(bool ok, string msg)> ApproveSupplierPaymentAsync(int id, string? approvedBy = null);
    Task<(bool ok, string msg)> CancelSupplierPaymentAsync(int id);
    Task<(bool ok, string msg)> DeleteSupplierPaymentAsync(int id);
    Task<List<Part>> PartsForSupplierPaymentAsync();
    Task<List<StockIn>> StockInsForSupplierPaymentAsync();
    Task<List<OrderPart>> OrderPartsForSupplierPaymentAsync();
    // StockOutOrder (Ser_Inv_StockOutOrder - MH 125 Quản lý yêu cầu xuất kho dịch vụ)
    Task<List<StockOutOrder>> StockOutOrdersAsync(StockOutOrderStatus? status, StockOutOrderPriority? priority, string? q, DateTime? fromDate, DateTime? toDate, int? roId);
    Task<StockOutOrder?> GetStockOutOrderAsync(int id);
    Task<StockOutOrder?> GetStockOutOrderByNoAsync(string orderNo);
    Task<int> CreateStockOutOrderAsync(StockOutOrder order, List<StockOutOrderDetail> items);
    Task<(bool ok, string msg)> ApproveStockOutOrderAsync(int id, string? approvedBy = null);
    Task<(bool ok, string msg)> RejectStockOutOrderAsync(int id, string reason);
    Task<(bool ok, string msg, int? stockOutId)> IssueStockOutFromOrderAsync(int id, string? issuedBy = null);
    Task<(bool ok, string msg)> DeleteStockOutOrderAsync(int id);
    Task<List<RepairOrder>> ROsForStockOutOrderAsync();
    Task<List<Cavity>> CavitiesForSelectAsync();
    // Part Out of Stock / Backorder (Ser_Part_OO - Quản lý Phụ tùng nợ khách)
    Task<List<PartOO>> PartOOsAsync(string? q, bool? isConNo, PartOOStatus? status);
    Task<PartOO?> GetPartOOAsync(int id);
    Task<PartOO?> GetPartOOByNoAsync(string ooNo);
    Task<int> CreatePartOOAsync(PartOO item);
    Task<(bool ok, string msg)> UpdatePartOOAsync(int id, string? model, decimal soLuongNo, decimal soLuongTra, string? cvdv, DateTime? ngayDatHang, DateTime? ngayVeDuKien, DateTime? ngayHenTra, string? ghiChu);
    Task<(bool ok, string msg)> ReturnPartOOAsync(int id, decimal returnQty, bool deductStock, string? returnedBy, string? note);
    Task<(bool ok, string msg)> CancelPartOOAsync(int id, string reason);
    Task<(bool ok, string msg)> DeletePartOOAsync(int id);
    Task<List<PartOO>> GetPartOOStockAlertsAsync();
    Task<List<PartOO>> GetPartOOsByPlateAsync(string plate);
    // Customer Debit Management (Ser_CusDebit, Ser_CusDebitPayment, Ser_InvReportCusDebitRpt / MH 54)
    Task<List<CusDebit>> CusDebitsAsync(int? customerId, CusDebitStatus? status, CusDebitType? type, string? q, bool? isOverdue, DateTime? fromDate, DateTime? toDate);
    Task<List<CustomerDebitSummaryDto>> CustomerDebitSummariesAsync(string? q, bool? onlyHasDebit);
    Task<(Customer customer, List<CusDebit> debits, List<CusDebitPayment> payments, decimal totalDebit, decimal totalPaid, decimal remainingDebit)> GetCustomerDebitProfileAsync(int customerId);
    Task<CusDebit?> GetCusDebitAsync(int id);
    Task<CusDebitPayment?> GetCusDebitPaymentAsync(int paymentId);
    Task<int> CreateCusDebitAsync(CusDebit debit);
    Task<int> CreateCusDebitPaymentAsync(CusDebitPayment payment);
    Task<(bool ok, string msg)> CancelCusDebitAsync(int id, string? reason);
    Task<(bool ok, string msg)> DeleteCusDebitPaymentAsync(int paymentId);
    Task<(bool ok, string msg, int? debitId)> CreateDebitFromROAsync(int roId, decimal? amount, DateTime? dueDate, string? note);
    Task<List<Customer>> CustomersForDebitSelectAsync();
    Task<List<RepairOrder>> ROsWithUnpaidBalanceAsync();
    // Supplier Debit Management (Ser_SupplierDebit, Ser_SupplierDebitPayment / MH 56)
    Task<List<SupplierDebit>> SupplierDebitsAsync(int? supplierId, SupplierDebitStatus? status, SupplierDebitType? type, string? q, bool? isOverdue, DateTime? fromDate, DateTime? toDate);
    Task<List<SupplierDebitSummaryDto>> SupplierDebitSummariesAsync(string? q, bool? onlyHasDebit);
    Task<(Supplier supplier, List<SupplierDebit> debits, List<SupplierDebitPayment> payments, decimal totalDebit, decimal totalPaid, decimal remainingDebit)> GetSupplierDebitProfileAsync(int supplierId);
    Task<SupplierDebit?> GetSupplierDebitAsync(int id);
    Task<SupplierDebitPayment?> GetSupplierDebitPaymentAsync(int paymentId);
    Task<int> CreateSupplierDebitAsync(SupplierDebit debit);
    Task<int> CreateSupplierDebitPaymentAsync(SupplierDebitPayment payment, bool allocateFifoIfNoDebit = true);
    Task<(bool ok, string msg)> CancelSupplierDebitAsync(int id, string? reason);
    Task<(bool ok, string msg)> DeleteSupplierDebitPaymentAsync(int paymentId);
    Task<(bool ok, string msg, int? debitId)> CreateSupplierDebitFromStockInAsync(int stockInId, int? supplierId, DateTime? dueDate, string? note);
    Task<List<Supplier>> SuppliersForDebitSelectAsync();
    Task<List<StockIn>> StockInsForDebitSelectAsync();
    // Insurance Debit Management (Ser_InsuranceDebit, Ser_InsuranceDebitPayment / MH 55)
    Task<List<InsuranceCompanyDebitSummaryDto>> InsuranceCompanyDebitSummariesAsync(string? q, bool? onlyHasDebit);
    Task<(InsuranceCompany company, InsuranceCompanyDebitSummaryDto summary, List<InsuranceDebit> debits, List<InsuranceDebitPayment> payments)> GetInsuranceCompanyDebitProfileAsync(int companyId);
    Task<List<InsuranceDebit>> InsuranceDebitsAsync(int? companyId, InsuranceDebitStatus? status, InsuranceDebitType? type, string? q, bool? isOverdue, DateTime? fromDate, DateTime? toDate);
    Task<InsuranceDebit?> GetInsuranceDebitAsync(int id);
    Task<InsuranceDebitPayment?> GetInsuranceDebitPaymentAsync(int paymentId);
    Task<List<InsuranceDebitPayment>> InsuranceDebitPaymentsAsync(int? companyId, int? debitId, DateTime? fromDate, DateTime? toDate);
    Task<int> CreateInsuranceDebitAsync(InsuranceDebit debit);
    Task<int> CreateInsuranceDebitPaymentAsync(InsuranceDebitPayment payment, bool allocateFifoIfNoDebit = true);
    Task<(bool ok, string msg)> CancelInsuranceDebitAsync(int id, string? reason);
    Task<(bool ok, string msg)> DeleteInsuranceDebitPaymentAsync(int paymentId);
    Task<(bool ok, string msg, int? debitId)> CreateInsuranceDebitFromRoAsync(int roId, int? companyId, decimal? debitAmount, DateTime? dueDate, string? note);
    Task<(bool ok, string msg, int? debitId)> CreateInsuranceDebitFromClaimAsync(int claimId, DateTime? dueDate, string? note);
    Task<List<InsuranceCompany>> InsuranceCompaniesForDebitSelectAsync();
    Task<List<RepairOrder>> ROsForInsuranceDebitSelectAsync();
    Task<List<InsuranceClaim>> ClaimsForInsuranceDebitSelectAsync();
    // Dealer Repair History Share (DealerHistoryShareMng - Quản lý tra cứu & chia sẻ lịch sử sửa chữa toàn hệ thống đại lý)
    Task<List<DealerHistoryRecord>> SearchDealerHistoryAsync(string? q, string? dealer, DateTime? fromDate, DateTime? toDate);
    Task<VehicleHistorySummaryDto?> GetVehicleServiceSummaryAsync(string plateOrVin);
    Task<DealerHistoryRecord?> GetDealerHistoryRecordAsync(int id);
    Task<int> CreateDealerHistoryRecordAsync(DealerHistoryRecord record, List<DealerHistoryItem> items);
    Task<(bool ok, string msg, int? recordId)> SyncLocalRoToHistoryAsync(int roId);
    Task<(bool ok, string msg)> DeleteDealerHistoryRecordAsync(int id);
    Task<List<string>> GetDistinctDealerCodesAsync();
    Task<List<Car>> CarsWithPlateOrVinAsync(string? q = null);
    // Customer Group & Fleet Management (Ser_CustomerGroup, Ser_CustomerGroupCustomer / MNU_QT_DL_QUANLYKHACHDOAN)
    Task<List<CustomerGroupSummaryDto>> CustomerGroupSummariesAsync(string? q, bool? isActive, bool? creditExceededOnly);
    Task<List<CustomerGroup>> CustomerGroupsAsync(string? q, bool? isActive);
    Task<CustomerGroup?> GetCustomerGroupAsync(int id);
    Task<CustomerGroup?> GetCustomerGroupByGroupNoAsync(string groupNo);
    Task<int> CreateCustomerGroupAsync(CustomerGroup group);
    Task<(bool ok, string msg)> UpdateCustomerGroupAsync(int id, CustomerGroup input);
    Task<(bool ok, string msg)> DeleteCustomerGroupAsync(int id);
    Task<(bool ok, string msg, int? memberId)> AddMemberToCustomerGroupAsync(int groupId, int carId, string? driverName, string? driverPhone, string? note);
    Task<(bool ok, string msg)> RemoveMemberFromCustomerGroupAsync(int memberId);
    Task<CustomerGroupMember?> CheckCarCustomerGroupAsync(int carId);
    Task<CustomerGroupMember?> CheckPlateCustomerGroupAsync(string plate);
    Task<(bool ok, string msg, decimal discountAmount)> ApplyCustomerGroupDiscountToRoAsync(int roId, int groupId);
    Task<List<Car>> CarsForCustomerGroupSelectAsync(int? currentGroupId = null);
    // Part Price Requests (Req_PartPrice / Req_PartPriceDtl - Đề nghị cung cấp giá phụ tùng NCC TST/HTC)
    Task<List<PartPriceRequest>> PartPriceRequestsAsync(DMSReqPartPriceStatus? dmsStatus, TSTReqPartPriceStatus? tstStatus, string? q, DateTime? fromDate, DateTime? toDate);
    Task<PartPriceRequest?> GetPartPriceRequestAsync(int id);
    Task<PartPriceRequest?> GetPartPriceRequestByNoAsync(string reqNo);
    Task<int> CreatePartPriceRequestAsync(PartPriceRequest request, List<PartPriceRequestLine> items);
    Task<(bool ok, string msg)> SendPartPriceRequestToTSTAsync(int id);
    Task<(bool ok, string msg)> SimulateTSTResponseAsync(int id, List<(int lineId, string? tstPartCode, decimal tstPrice, DateTime dateEffect)> linePrices);
    Task<(bool ok, string msg)> ApprovePartPriceRequestAsync(int id, string? approvedBy = null, bool syncToCatalog = true);
    Task<(bool ok, string msg, int? orderPartId)> ConvertToOrderPartAsync(int id, string? createdBy = null);
    Task<(bool ok, string msg)> CancelPartPriceRequestAsync(int id, string? reason = null);
    Task<(bool ok, string msg)> DeletePartPriceRequestAsync(int id);
    Task<List<RepairOrder>> ROsForPartPriceRequestSelectAsync();
    // Complaint & Diagnostic Errors (Ser_MST_ROComplaintDiagnosticError / MNU_QT_DL_QUANLYMALOIPHANNANVACHANDOAN)
    Task<List<ComplaintDiagnosticError>> ComplaintDiagnosticErrorsAsync(ComplaintErrorType? type, VehicleSystemGroup? group, string? q, bool? isActive);
    Task<ComplaintDiagnosticSummaryDto> GetComplaintDiagnosticSummaryAsync();
    Task<ComplaintDiagnosticError?> GetComplaintDiagnosticErrorAsync(int id);
    Task<ComplaintDiagnosticError?> GetComplaintDiagnosticErrorByCodeAsync(string code);
    Task<int> CreateComplaintDiagnosticErrorAsync(ComplaintDiagnosticError error);
    Task<(bool ok, string msg)> UpdateComplaintDiagnosticErrorAsync(int id, ComplaintDiagnosticError input);
    Task<(bool ok, string msg)> ToggleComplaintDiagnosticErrorActiveAsync(int id);
    Task<(bool ok, string msg)> DeleteComplaintDiagnosticErrorAsync(int id);
    Task<List<ComplaintDiagnosticError>> GetActiveComplaintsAsync(VehicleSystemGroup? group = null);
    Task<List<ComplaintDiagnosticError>> GetActiveDiagnosticsAsync(VehicleSystemGroup? group = null);
    Task<(bool ok, string msg)> ApplyErrorToROAsync(int roId, int errorId, string target);
    Task<List<RepairOrder>> ROsForErrorAssignmentAsync();
    // Warranty Standard Labor Works (Ser_MST_ROWarrantyWork & Ser_MST_ROWarrantyType)
    Task<List<WarrantyWork>> WarrantyWorksAsync(string? model, WarrantyLaborGroup? group, WarrantyCoverageType? coverage, string? q, bool? isActive);
    Task<WarrantyWork?> GetWarrantyWorkAsync(int id);
    Task<WarrantyWork?> GetWarrantyWorkByCodeAsync(string code);
    Task<WarrantyWorkSummaryDto> GetWarrantyWorkSummaryAsync();
    Task<int> CreateWarrantyWorkAsync(WarrantyWork work);
    Task<(bool ok, string msg)> UpdateWarrantyWorkAsync(int id, WarrantyWork input);
    Task<(bool ok, string msg)> ToggleWarrantyWorkActiveAsync(int id);
    Task<(bool ok, string msg)> DeleteWarrantyWorkAsync(int id);
    Task<(bool ok, string msg, int? lineId)> ApplyWarrantyWorkToRoAsync(int warrantyWorkId, int roId, decimal? customHours, string? note);
    Task<List<RepairOrder>> ROsForWarrantyWorkSelectAsync();
    Task<List<string>> DistinctWarrantyModelsAsync();
    // Maintenance Interval & Milestone Settings (Ser_MST_ROMaintanceSetting / MNU_QT_DL_THIETLAPBAODUONG)
    Task<List<MaintenanceSetting>> MaintenanceSettingsAsync(int? minKm, int? maxKm, MaintenanceLevel? level, bool? flagWarranty, bool? flagActive, string? q);
    Task<MaintenanceSetting?> GetMaintenanceSettingAsync(int id);
    Task<MaintenanceSetting?> GetMaintenanceSettingByRomsIdAsync(string romsId);
    Task<MaintenanceSettingSummaryDto> GetMaintenanceSettingSummaryAsync();
    Task<int> CreateMaintenanceSettingAsync(MaintenanceSetting setting);
    Task<(bool ok, string msg)> UpdateMaintenanceSettingAsync(int id, MaintenanceSetting input);
    Task<(bool ok, string msg)> ToggleMaintenanceSettingActiveAsync(int id);
    Task<(bool ok, string msg)> DeleteMaintenanceSettingAsync(int id);
    Task<MaintenanceSuggestionDto> SuggestMaintenanceForKmAsync(int km);
    Task<(bool ok, string msg)> ApplyMaintenanceToRoAsync(int settingId, int roId, bool addPackageCombo);
    Task<List<RepairOrder>> ROsForMaintenanceSelectAsync();
    // Warranty Type Catalog (Ser_MST_ROWarrantyType / Ser_MST_ROWarrantyType_PhotoType / Ser_MST_ROWarrantyPhotoType)
    Task<List<WarrantyType>> WarrantyTypesAsync(WarrantyTypeCode? typeCode, bool? flagActive, string? q);
    Task<WarrantyType?> GetWarrantyTypeAsync(int id);
    Task<WarrantyTypeSummaryDto> GetWarrantyTypeSummaryAsync();
    Task<int> CreateWarrantyTypeAsync(WarrantyType type, List<WarrantyTypePhoto> photos);
    Task<(bool ok, string msg)> UpdateWarrantyTypeAsync(int id, WarrantyType input, List<WarrantyTypePhoto>? photos);
    Task<(bool ok, string msg)> ToggleWarrantyTypeActiveAsync(int id);
    Task<(bool ok, string msg)> DeleteWarrantyTypeAsync(int id);
    Task<List<WarrantyPhotoType>> WarrantyPhotoTypesAsync(bool? flagActive);
    // Dealer Target / KPI (Mst_DealerTarget / Mst_DealerTargetDetail) — MH 168
    Task<List<DealerTarget>> DealerTargetsAsync(int? year, string? q);
    Task<DealerTarget?> GetDealerTargetAsync(int id);
    Task<DealerTargetSummaryDto> GetDealerTargetSummaryAsync();
    Task<(bool ok, string msg, int id)> SaveDealerTargetAsync(int? id, int year, string? remark, List<DealerTargetDetail> details, string user);
    Task<(bool ok, string msg)> DeleteDealerTargetAsync(int id);
    Task<(bool ok, string msg)> DeleteDealerTargetDetailAsync(int detailId);
    // Customer Service Factor — Hệ số giá dịch vụ theo loại khách hàng (Ser_MST_CustomerType / Ser_Mst_CusServiceFactor)
    Task<List<CustomerType>> CustomerTypesAsync(bool? isActive, string? q);
    Task<CustomerType?> GetCustomerTypeAsync(int id);
    Task<int> CreateCustomerTypeAsync(CustomerType type);
    Task<(bool ok, string msg)> UpdateCustomerTypeAsync(int id, CustomerType input);
    Task<(bool ok, string msg)> ToggleCustomerTypeActiveAsync(int id);
    Task<(bool ok, string msg)> DeleteCustomerTypeAsync(int id);
    Task<List<CusServiceFactorRowDto>> CusServiceFactorMatrixAsync(int? serviceItemId, int? customerTypeId, string? q);
    Task<CusServiceFactorSummaryDto> GetCusServiceFactorSummaryAsync();
    Task<(bool ok, string msg)> SaveCusServiceFactorAsync(int serviceItemId, int customerTypeId, decimal factor, string? dealerCode, string? user);
    Task<(bool ok, string msg)> ResetCusServiceFactorAsync(int serviceItemId, int customerTypeId);
    Task<decimal> ResolveServicePriceAsync(int serviceItemId, int? customerTypeId);
    // RO History — Nhật ký thao tác Lệnh sửa chữa (Ser_ROHistory)
    Task<List<RoHistory>> RoHistoriesAsync(int? roId, ROStatus? status, string? q);
    Task<RoHistory?> GetRoHistoryAsync(int id);
    Task<RoHistorySummaryDto> GetRoHistorySummaryAsync(int? roId);
    Task<int> AddRoHistoryAsync(int roId, ROStatus status, string? note, string? userCode);
    Task<(bool ok, string msg)> DeleteRoHistoryAsync(int id);
    // dropdown data
    Task<List<Car>> CarsForSelectAsync();
}

public class RoService(AppDbContext db) : IRoService
{
    /// <summary>Chuyển trạng thái Khiếu nại phụ tùng theo Ser_OrderComplain idn.CarService.</summary>
    public static DMSOrderComplainStatus[] AllowedNextDMSComplain(DMSOrderComplainStatus s) => s switch
    {
        DMSOrderComplainStatus.Pending => [DMSOrderComplainStatus.Sent, DMSOrderComplainStatus.Cancelled],
        DMSOrderComplainStatus.Sent => [DMSOrderComplainStatus.Finished, DMSOrderComplainStatus.Cancelled],
        _ => []
    };

    /// <summary>Chuyển trạng thái Phiếu xuất trả NCC theo Ser_SupplierPayment idn.CarService.</summary>
    public static SupplierPaymentStatus[] AllowedNextSupplierPayment(SupplierPaymentStatus s) => s switch
    {
        SupplierPaymentStatus.Pending => [SupplierPaymentStatus.Approved, SupplierPaymentStatus.Cancelled],
        _ => []
    };

    /// <summary>Chuyển trạng thái Yêu cầu xuất kho phụ tùng theo Ser_Inv_StockOutOrder idn.CarService.</summary>
    public static StockOutOrderStatus[] AllowedNextStockOutOrder(StockOutOrderStatus s) => s switch
    {
        StockOutOrderStatus.Pending => [StockOutOrderStatus.Approved, StockOutOrderStatus.Completed, StockOutOrderStatus.Rejected],
        StockOutOrderStatus.Approved => [StockOutOrderStatus.Completed, StockOutOrderStatus.Rejected],
        _ => []
    };

    /// <summary>Chuyển trạng thái Yêu cầu PDI theo Dlr_PDIRequest idn.CarService.</summary>
    public static PdiRequestStatus[] AllowedNextPdiRequest(PdiRequestStatus s) => s switch
    {
        PdiRequestStatus.Draft => [PdiRequestStatus.Pending, PdiRequestStatus.Cancelled],
        PdiRequestStatus.Pending => [PdiRequestStatus.Approved, PdiRequestStatus.Cancelled],
        PdiRequestStatus.Approved => [PdiRequestStatus.Completed, PdiRequestStatus.Cancelled],
        _ => []
    };

    /// <summary>Chuyển trạng thái Kiểm kê kho theo Ser_Inv_StockAdj idn.CarService.</summary>
    public static StockAdjStatus[] AllowedNextStockAdj(StockAdjStatus s) => s switch
    {
        StockAdjStatus.Pending => [StockAdjStatus.Executing, StockAdjStatus.Rejected],
        StockAdjStatus.Executing => [StockAdjStatus.Finished, StockAdjStatus.Rejected, StockAdjStatus.Pending],
        _ => []
    };

    /// <summary>Chuyển trạng thái Chiến dịch Marketing theo Ser_CampaignMarketing idn.CarService.</summary>
    public static CampaignMarketingStatus[] AllowedNextCampaign(CampaignMarketingStatus s) => s switch
    {
        CampaignMarketingStatus.Draft => [CampaignMarketingStatus.Active, CampaignMarketingStatus.Cancelled],
        CampaignMarketingStatus.Active => [CampaignMarketingStatus.Finished, CampaignMarketingStatus.Cancelled],
        _ => []
    };

    /// <summary>Chuyển trạng thái Bản tin kỹ thuật theo Btl_Bulletin idn.CarService.</summary>
    public static BulletinStatus[] AllowedNextBulletin(BulletinStatus s) => s switch
    {
        BulletinStatus.Draft => [BulletinStatus.Active, BulletinStatus.Cancelled],
        BulletinStatus.Active => [BulletinStatus.Finished, BulletinStatus.Cancelled],
        _ => []
    };

    /// <summary>Chuyển trạng thái hợp lệ theo state machine idn.CarService.</summary>
    public static ROStatus[] AllowedNext(ROStatus s) => s switch
    {
        ROStatus.Created => [ROStatus.Printed, ROStatus.Rejected],
        ROStatus.Printed => [ROStatus.HasRO, ROStatus.Wait4Part, ROStatus.Rejected],
        ROStatus.Wait4Part => [ROStatus.HasPart, ROStatus.NotResponding],
        ROStatus.HasPart => [ROStatus.HasRO],
        ROStatus.HasRO => [ROStatus.InGarage, ROStatus.Rejected],
        ROStatus.InGarage => [ROStatus.Repaired],
        ROStatus.Repaired => [ROStatus.CheckEnd],
        ROStatus.CheckEnd => [ROStatus.Paid],
        ROStatus.Paid => [ROStatus.Finished],
        _ => []
    };

    /// <summary>Chuyển trạng thái Báo cáo bảo hành theo Ser_WarrantyReport_Status.</summary>
    public static WarrantyStatus[] AllowedNextWarranty(WarrantyStatus s) => s switch
    {
        WarrantyStatus.Pending => [WarrantyStatus.Sent],
        WarrantyStatus.Sent => [WarrantyStatus.Confirmed, WarrantyStatus.Reverted],
        WarrantyStatus.Confirmed => [WarrantyStatus.Accepted, WarrantyStatus.Rejected, WarrantyStatus.Reverted],
        WarrantyStatus.Reverted => [WarrantyStatus.Sent],
        _ => []
    };

    /// <summary>Chuyển trạng thái cuộc hẹn dịch vụ theo SerAppStatus.</summary>
    public static AppointmentStatus[] AllowedNextAppointment(AppointmentStatus s) => s switch
    {
        AppointmentStatus.Pending => [AppointmentStatus.Contacted, AppointmentStatus.Confirmed, AppointmentStatus.Cancelled],
        AppointmentStatus.Contacted => [AppointmentStatus.Confirmed, AppointmentStatus.Cancelled],
        AppointmentStatus.Confirmed => [AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled],
        _ => []
    };

    /// <summary>Chuyển trạng thái Phiếu Nhập kho theo Ser_Inv_StockIn idn.CarService.</summary>
    public static StockInStatus[] AllowedNextStockIn(StockInStatus s) => s switch
    {
        StockInStatus.Pending => [StockInStatus.Executing, StockInStatus.Finished, StockInStatus.Rejected],
        StockInStatus.Executing => [StockInStatus.Finished, StockInStatus.Rejected],
        _ => []
    };

    /// <summary>Chuyển trạng thái Phiếu Xuất kho theo Ser_Inv_StockOut idn.CarService.</summary>
    public static StockOutStatus[] AllowedNextStockOut(StockOutStatus s) => s switch
    {
        StockOutStatus.Pending => [StockOutStatus.Executing, StockOutStatus.Finished, StockOutStatus.Rejected],
        StockOutStatus.Executing => [StockOutStatus.Finished, StockOutStatus.Rejected],
        _ => []
    };

    /// <summary>Chuyển trạng thái Phiếu thu theo Ser_Payment idn.CarService.</summary>
    public static PaymentStatus[] AllowedNextPayment(PaymentStatus s) => s switch
    {
        PaymentStatus.Draft => [PaymentStatus.Completed, PaymentStatus.Cancelled],
        PaymentStatus.Completed => [PaymentStatus.Cancelled],
        _ => []
    };

    /// <summary>Chuyển trạng thái Báo giá theo Ser_Inv_Quote idn.CarService.</summary>
    public static QuoteStatus[] AllowedNextQuote(QuoteStatus s) => s switch
    {
        QuoteStatus.Draft => [QuoteStatus.Sent, QuoteStatus.Confirmed, QuoteStatus.Rejected],
        QuoteStatus.Sent => [QuoteStatus.Confirmed, QuoteStatus.Rejected],
        QuoteStatus.Confirmed => [QuoteStatus.Converted, QuoteStatus.Rejected],
        _ => []
    };

    /// <summary>Chuyển trạng thái Đơn đặt hàng phụ tùng theo OrderPartStatus (P, A, F, R) idn.CarService.</summary>
    public static OrderPartStatus[] AllowedNextOrderPart(OrderPartStatus s) => s switch
    {
        OrderPartStatus.Pending => [OrderPartStatus.Approved, OrderPartStatus.Rejected],
        OrderPartStatus.Approved => [OrderPartStatus.Finished, OrderPartStatus.Rejected],
        _ => []
    };

    /// <summary>Chuyển trạng thái Phiếu tiếp nhận & kiểm tra xe theo Ser_ReceptionF idn.CarService.</summary>
    public static ReceptionStatus[] AllowedNextReception(ReceptionStatus s) => s switch
    {
        ReceptionStatus.Pending => [ReceptionStatus.InService, ReceptionStatus.Cancelled],
        ReceptionStatus.InService => [ReceptionStatus.Delivered, ReceptionStatus.Cancelled],
        _ => []
    };

    /// <summary>Chuyển trạng thái Phân công thợ sửa chữa theo Ser_AssignmentWork idn.CarService.</summary>
    public static AssignmentWorkStatus[] AllowedNextAssignment(AssignmentWorkStatus s) => s switch
    {
        AssignmentWorkStatus.Assigned => [AssignmentWorkStatus.InProgress, AssignmentWorkStatus.Cancelled],
        AssignmentWorkStatus.InProgress => [AssignmentWorkStatus.Completed, AssignmentWorkStatus.Cancelled],
        _ => []
    };

    public Task<List<Customer>> CustomersAsync(string? q)
    {
        var query = db.Customers.Include(c => c.Cars).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(c => c.Name.Contains(q) || c.Code.Contains(q) || (c.Phone ?? "").Contains(q));
        return query.OrderBy(c => c.Name).ToListAsync();
    }
    public async Task<int> CreateCustomerAsync(Customer c)
    {
        if (string.IsNullOrWhiteSpace(c.Code)) c.Code = $"KH{await db.Customers.CountAsync() + 1:D4}";
        db.Customers.Add(c); await db.SaveChangesAsync(); return c.Id;
    }

    public Task<List<Car>> CarsAsync(string? q)
    {
        var query = db.Cars.Include(c => c.Customer).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(c => c.Plate.Contains(q) || c.Model.Contains(q) || (c.Vin ?? "").Contains(q));
        return query.OrderBy(c => c.Plate).ToListAsync();
    }
    public Task<List<Car>> CarsForSelectAsync() => db.Cars.Include(c => c.Customer).OrderBy(c => c.Plate).ToListAsync();
    public async Task<int> CreateCarAsync(Car car) { db.Cars.Add(car); await db.SaveChangesAsync(); return car.Id; }

    public Task<List<Part>> PartsAsync(string? q, bool? lowStockOnly)
    {
        var query = db.Parts.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Code.Contains(q) || p.Name.Contains(q) || (p.Model != null && p.Model.Contains(q)) || (p.Location != null && p.Location.Contains(q)));
        if (lowStockOnly == true)
            query = query.Where(p => p.InStock <= p.MinStock);
        return query.OrderBy(p => p.Code).ToListAsync();
    }
    public Task<Part?> GetPartAsync(int id) => db.Parts.FirstOrDefaultAsync(p => p.Id == id);
    public async Task<int> CreatePartAsync(Part part)
    {
        if (string.IsNullOrWhiteSpace(part.Code))
            throw new InvalidOperationException("Mã phụ tùng không được để trống.");
        part.Code = part.Code.Trim().ToUpperInvariant();
        part.Name = part.Name.Trim();
        var exists = await db.Parts.AnyAsync(p => p.Code == part.Code);
        if (exists)
            throw new InvalidOperationException($"Mã phụ tùng '{part.Code}' đã tồn tại.");
        db.Parts.Add(part);
        await db.SaveChangesAsync();
        return part.Id;
    }
    public async Task<(bool ok, string msg)> AdjustStockAsync(int partId, decimal qty, string mode, string? note)
    {
        var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == partId);
        if (part == null) return (false, "Không tìm thấy phụ tùng.");
        if (mode == "set")
        {
            if (qty < 0) return (false, "Số lượng tồn không thể âm.");
            part.InStock = qty;
        }
        else
        {
            if (part.InStock + qty < 0) return (false, "Số lượng xuất vượt quá tồn kho hiện có.");
            part.InStock += qty;
        }
        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật tồn kho '{part.Name}': {part.InStock} {part.Unit}.");
    }
    public Task<List<Part>> PartsForSelectAsync() =>
        db.Parts.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();

    public async Task<List<RepairOrder>> ROsAsync(ROStatus? status, string? q)
    {
        var query = db.ROs.Include(r => r.Car).Include(r => r.Customer).Include(r => r.Lines).Include(r => r.Appointment).AsQueryable();
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(r => r.Code.Contains(q) || r.Car.Plate.Contains(q));
        var list = await query.ToListAsync();
        return list.OrderByDescending(r => r.CreatedAt).ToList();
    }

    public Task<RepairOrder?> GetROAsync(int id) =>
        db.ROs.Include(r => r.Car).ThenInclude(c => c.Customer).Include(r => r.Customer)
          .Include(r => r.Lines).ThenInclude(l => l.Part)
          .Include(r => r.Lines).ThenInclude(l => l.ServiceItem)
          .Include(r => r.Appointment)
          .Include(r => r.WarrantyReports).Include(r => r.StockOuts).Include(r => r.StockOutOrders).Include(r => r.CustomerCares).Include(r => r.Payments).Include(r => r.OrderParts).Include(r => r.ReceptionSheet).Include(r => r.PartOOs).Include(r => r.CusDebits).ThenInclude(d => d.Payments)
          .Include(r => r.AssignmentWorks).ThenInclude(a => a.Engineers).ThenInclude(e => e.Engineer)
          .Include(r => r.InsuranceClaims).ThenInclude(c => c.InsuranceCompany)
          .Include(r => r.InsuranceDebits).ThenInclude(d => d.Payments)
          .Include(r => r.CustomerGroup)
          .Include(r => r.Bulletin).ThenInclude(b => b!.Items)
          .Include(r => r.TechnicalLibraries)
          .Include(r => r.Histories)
          .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<int> CreateROAsync(RepairOrder ro)
    {
        var car = await db.Cars.FirstOrDefaultAsync(c => c.Id == ro.CarId) ?? throw new InvalidOperationException("Xe không tồn tại.");
        ro.CustomerId = car.CustomerId;
        ro.Code = $"RO{DateTime.Now:yyMMdd}-{await db.ROs.CountAsync() + 1:D3}";
        ro.Status = ROStatus.Created;

        // Tự động nhận diện xe thuộc Khách đoàn để liên kết CustomerGroup
        var groupMember = await db.CustomerGroupMembers
            .Include(m => m.CustomerGroup)
            .FirstOrDefaultAsync(m => m.CarId == ro.CarId && m.IsActive);
        if (groupMember?.CustomerGroup != null && groupMember.CustomerGroup.IsActive)
        {
            ro.CustomerGroupId = groupMember.CustomerGroupId;
        }

        db.ROs.Add(ro);
        await db.SaveChangesAsync();
        // Ghi nhật ký thao tác — tạo Báo giá (Ser_ROHistory, tương đương InsertToROHistory "Tạo Báo giá")
        db.RoHistories.Add(new RoHistory
        {
            ROId = ro.Id,
            Status = ROStatus.Created,
            HistoryDate = DateTime.Now,
            UserCode = string.IsNullOrWhiteSpace(ro.CreatedBy) ? "system" : ro.CreatedBy,
            Note = "Tạo Báo giá"
        });
        await db.SaveChangesAsync();
        return ro.Id;
    }

    public async Task AddLineAsync(int roId, LineType type, string name, decimal qty, decimal price, int? partId = null, ExpenseType expenseType = ExpenseType.Customer, int? serviceItemId = null, decimal? stdManHour = null, int? warrantyWorkId = null)
    {
        var ro = await db.ROs.FirstOrDefaultAsync(r => r.Id == roId) ?? throw new KeyNotFoundException();
        if (ro.Status is ROStatus.Finished or ROStatus.Paid or ROStatus.Rejected or ROStatus.NotResponding)
            throw new InvalidOperationException("RO đã kết thúc — không thêm dòng.");

        if (type == LineType.Part && partId.HasValue && partId.Value > 0)
        {
            var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == partId.Value);
            if (part != null)
            {
                if (string.IsNullOrWhiteSpace(name)) name = part.Name;
                if (price <= 0) price = part.SalePrice;
                if (part.InStock >= qty) part.InStock -= qty;
            }
        }
        else if (type == LineType.Labor && serviceItemId.HasValue && serviceItemId.Value > 0)
        {
            var svcItem = await db.ServiceItems.FirstOrDefaultAsync(s => s.Id == serviceItemId.Value);
            if (svcItem != null)
            {
                if (string.IsNullOrWhiteSpace(name)) name = svcItem.Name;
                if (price <= 0) price = svcItem.Price;
                stdManHour ??= svcItem.StdManHour;
                if (qty <= 0) qty = svcItem.StdManHour > 0 ? svcItem.StdManHour : 1;
            }
        }
        else if (type == LineType.Labor && warrantyWorkId.HasValue && warrantyWorkId.Value > 0)
        {
            var wrtWork = await db.WarrantyWorks.FirstOrDefaultAsync(w => w.Id == warrantyWorkId.Value);
            if (wrtWork != null)
            {
                if (string.IsNullOrWhiteSpace(name)) name = $"[BH {wrtWork.Code}] {wrtWork.Name}";
                if (price <= 0) price = wrtWork.RatePrice;
                stdManHour ??= wrtWork.RateHour;
                if (qty <= 0) qty = wrtWork.RateHour > 0 ? wrtWork.RateHour : 1;
                expenseType = ExpenseType.Warranty;
            }
        }

        db.Lines.Add(new RepairLine
        {
            ROId = roId,
            Type = type,
            ExpenseType = expenseType,
            PartId = (type == LineType.Part && partId > 0) ? partId : null,
            ServiceItemId = (type == LineType.Labor && serviceItemId > 0) ? serviceItemId : null,
            WarrantyWorkId = (type == LineType.Labor && warrantyWorkId > 0) ? warrantyWorkId : null,
            StdManHour = stdManHour,
            Name = name.Trim(),
            Quantity = qty <= 0 ? 1 : qty,
            UnitPrice = price
        });
        await db.SaveChangesAsync();
    }

    public async Task RemoveLineAsync(int lineId)
    {
        var l = await db.Lines.Include(x => x.Part).FirstOrDefaultAsync(x => x.Id == lineId);
        if (l != null)
        {
            if (l.Type == LineType.Part && l.PartId.HasValue && l.Part != null)
            {
                l.Part.InStock += l.Quantity;
            }
            db.Lines.Remove(l);
            await db.SaveChangesAsync();
        }
    }

    public async Task<(bool ok, string msg)> TransitionAsync(int roId, ROStatus to)
    {
        var ro = await db.ROs.FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy RO.");
        if (!AllowedNext(ro.Status).Contains(to)) return (false, $"Không thể chuyển {Ui.Status(ro.Status).text} → {Ui.Status(to).text}.");
        ro.Status = to;
        if (to == ROStatus.InGarage) ro.IntakeAt ??= DateTime.Now;
        if (to == ROStatus.Finished)
        {
            ro.FinishedAt ??= DateTime.Now;
            // Tự động kích hoạt quy trình CSKH 24h theo Ser_CustomerCare24h idn.CarService
            var hasCare = await db.CustomerCares.AnyAsync(c => c.ROId == roId);
            if (!hasCare)
            {
                var care = new CustomerCare
                {
                    CareNo = $"CC{DateTime.Now:yyMMdd}-{await db.CustomerCares.CountAsync() + 1:D3}",
                    ROId = ro.Id,
                    CarId = ro.CarId,
                    CustomerId = ro.CustomerId,
                    Status = CustomerCareStatus.Pending,
                    CreatedBy = "system"
                };
                db.CustomerCares.Add(care);
            }
            // Tự động kích hoạt quy trình Nhắc bảo dưỡng định kỳ Ser_CustomerCareMace idn.CarService
            var hasMace = await db.CustomerCareMaces.AnyAsync(m => m.ROId == roId);
            if (!hasMace)
            {
                var (recDate, mType, nextKm) = await CalculateNextMaintenanceAsync(ro.CarId, ro.FinishedAt, ro.Odometer);
                var mace = new CustomerCareMace
                {
                    MaceNo = $"MC{DateTime.Now:yyMMdd}-{await db.CustomerCareMaces.CountAsync() + 1:D3}",
                    ROId = ro.Id,
                    CarId = ro.CarId,
                    CustomerId = ro.CustomerId,
                    MaceType = mType,
                    LastKm = ro.Odometer,
                    NextKm = nextKm,
                    MaceRecomentDate = recDate,
                    Status = CustomerCareMaceStatus.Pending,
                    CreatedBy = "system"
                };
                db.CustomerCareMaces.Add(mace);
            }
            // Tự động kích hoạt quy trình CSKH 72h & Kiểm soát pan tái phát Ser_CustomerCare72h idn.CarService
            var hasCare72 = await db.CustomerCare72hs.AnyAsync(c => c.ROId == roId);
            if (!hasCare72)
            {
                var care72 = new CustomerCare72h
                {
                    Care72No = $"CC72-{DateTime.Now:yyMMdd}-{await db.CustomerCare72hs.CountAsync() + 1:D3}",
                    ROId = ro.Id,
                    CarId = ro.CarId,
                    CustomerId = ro.CustomerId,
                    ROFinishedDate = ro.FinishedAt ?? DateTime.Now,
                    ScheduledDate = (ro.FinishedAt ?? DateTime.Now).AddDays(3),
                    Status = CustomerCare72hStatus.Pending,
                    CreatedBy = "system"
                };
                db.CustomerCare72hs.Add(care72);
            }
            // Tự động hoàn tất Bản tin kỹ thuật / Triệu hồi xe nếu có liên kết số VIN
            var car = await db.Cars.FirstOrDefaultAsync(c => c.Id == ro.CarId);
            if (car != null && !string.IsNullOrWhiteSpace(car.Vin))
            {
                var cleanVin = car.Vin.Trim().ToUpperInvariant();
                var targetVins = await db.BulletinVins
                    .Where(v => v.Status == BulletinVinStatus.Pending && v.VinNo.ToUpper() == cleanVin && (ro.BulletinId == null || v.BulletinId == ro.BulletinId))
                    .ToListAsync();
                foreach (var bvin in targetVins)
                {
                    bvin.Status = BulletinVinStatus.Completed;
                    bvin.DateDone = DateTime.Now;
                    bvin.ROId = ro.Id;
                    bvin.RONo = ro.Code;
                    bvin.DoneBy = ro.Technician ?? "KTV";
                }
            }
        }
        await db.SaveChangesAsync();
        // Ghi nhật ký thao tác — chuyển trạng thái RO (Ser_ROHistory)
        db.RoHistories.Add(new RoHistory
        {
            ROId = ro.Id,
            Status = to,
            HistoryDate = DateTime.Now,
            UserCode = "system",
            Note = $"Chuyển trạng thái: {Ui.Status(ro.Status).text} → {Ui.Status(to).text}"
        });
        await db.SaveChangesAsync();
        return (true, $"Đã chuyển sang: {Ui.Status(to).text}.");
    }

    public async Task<(bool ok, string msg)> DeleteROAsync(int roId)
    {
        var ro = await db.ROs.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy RO.");
        if (ro.Status is not (ROStatus.Created or ROStatus.Printed or ROStatus.Rejected or ROStatus.NotResponding))
            return (false, "Chỉ xóa được RO ở trạng thái Lập báo giá / In / Hủy / Không liên lạc.");

        // Luật 2023.H.CarServices: Không được xóa nếu đã có Báo cáo bảo hành đính kèm
        var hasWarranty = await db.WarrantyReports.AnyAsync(w => w.ROId == roId);
        if (hasWarranty)
            return (false, "Không thể xóa RO đã lập Báo cáo bảo hành.");

        db.Lines.RemoveRange(ro.Lines);
        db.ROs.Remove(ro);
        await db.SaveChangesAsync();
        return (true, "Đã xóa RO.");
    }
    /// <summary>Cập nhật Nhắc bảo dưỡng định kỳ trên RO — Ser_RO_Update_Maintance_DL trong idn.CarService.
    /// Chỉ cho phép khi RO chưa ở trạng thái Paid/Finished. Đồng bộ ngày khuyến nghị sang các phiếu
    /// Nhắc bảo dưỡng (Ser_CustomerCareMace) đang gắn với RO này.</summary>
    public async Task<(bool ok, string msg)> UpdateMaintenanceReminderAsync(int roId, DateTime? reminderDate, int? reminderKm, bool workDoneSoon, string? memberNo, string updatedBy)
    {
        var ro = await db.ROs.FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy RO.");
        if (ro.Status is ROStatus.Paid or ROStatus.Finished)
            return (false, "RO đã ở trạng thái Đã thanh toán / Hoàn tất — không cập nhật được nhắc bảo dưỡng.");

        ro.ReminderMaintanceDate = reminderDate;
        ro.ReminderMaintanceKm = reminderKm;
        ro.WorkDoneSoon = workDoneSoon;
        ro.MemberNo = string.IsNullOrWhiteSpace(memberNo) ? null : memberNo.Trim();

        // Đồng bộ ngày khuyến nghị bảo dưỡng sang các phiếu Nhắc bảo dưỡng gắn với RO (Ser_CustomerCareMace)
        if (reminderDate.HasValue)
        {
            var maces = await db.CustomerCareMaces.Where(m => m.ROId == roId).ToListAsync();
            foreach (var m in maces) m.MaceRecomentDate = reminderDate.Value;
        }

        await db.SaveChangesAsync();
        return (true, "Đã cập nhật nhắc bảo dưỡng định kỳ cho RO.");
    }

    public async Task<SvcDash> DashboardAsync()
    {
        var ros = await db.ROs.Include(r => r.Lines).ToListAsync();
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var byStatus = ros.GroupBy(r => r.Status).Select(g => (g.Key, g.Count())).OrderBy(x => (int)x.Key).ToList();
        var openStatuses = new[] { ROStatus.Created, ROStatus.Printed, ROStatus.Wait4Part, ROStatus.HasPart, ROStatus.HasRO, ROStatus.InGarage, ROStatus.Repaired, ROStatus.CheckEnd };
        var totalParts = await db.Parts.CountAsync();
        var lowStockParts = await db.Parts.CountAsync(p => p.InStock <= p.MinStock);

        var pendingWarranty = await db.WarrantyReports.CountAsync(w =>
            w.Status == WarrantyStatus.Pending || w.Status == WarrantyStatus.Sent || w.Status == WarrantyStatus.Confirmed);
        var approvedWarrantyItems = await db.WarrantyReports.Where(w => w.Status == WarrantyStatus.Accepted)
            .Select(w => (decimal?)(w.ApprovedAmount ?? w.ClaimAmount)).ToListAsync();
        var approvedWarranty = approvedWarrantyItems.Sum() ?? 0;

        var todayAppointments = await db.Appointments.CountAsync(a => a.AppointmentDate.Date == today);
        var pendingAppointments = await db.Appointments.CountAsync(a =>
            a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Contacted || a.Status == AppointmentStatus.Confirmed);

        var pendingStockIns = await db.StockIns.CountAsync(s => s.Status == StockInStatus.Pending || s.Status == StockInStatus.Executing);
        var monthStockInItems = await db.StockIns.Include(s => s.Items)
            .Where(s => s.Status == StockInStatus.Finished && s.StockInDate >= monthStart)
            .ToListAsync();
        var monthStockInValue = monthStockInItems.Sum(s => s.Total);

        var pendingStockOuts = await db.StockOuts.CountAsync(s => s.Status == StockOutStatus.Pending || s.Status == StockOutStatus.Executing);
        var monthStockOutItems = await db.StockOuts.Include(s => s.Items)
            .Where(s => s.Status == StockOutStatus.Finished && s.StockOutDate >= monthStart)
            .ToListAsync();
        var monthStockOutValue = monthStockOutItems.Sum(s => s.Total);

        var pendingCustomerCares = await db.CustomerCares.CountAsync(c => c.Status == CustomerCareStatus.Pending);
        var feedbackCustomerCares = await db.CustomerCares.CountAsync(c => c.Status == CustomerCareStatus.NeedFeedback);

        var pendingPayments = await db.Payments.CountAsync(p => p.Status == PaymentStatus.Draft);
        var monthPayments = await db.Payments
            .Where(p => p.Status == PaymentStatus.Completed && p.PaymentDate >= monthStart)
            .ToListAsync();
        var monthPaymentRevenue = monthPayments.Sum(p => p.PaymentAmount);

        var pendingQuotes = await db.Quotes.CountAsync(q => q.Status == QuoteStatus.Draft || q.Status == QuoteStatus.Sent);
        var monthQuotes = await db.Quotes.Include(q => q.Items)
            .Where(q => (q.Status == QuoteStatus.Confirmed || q.Status == QuoteStatus.Converted) && q.QuoteDate >= monthStart)
            .ToListAsync();
        var monthQuoteValue = monthQuotes.Sum(q => q.Total);
        var totalServicePackages = await db.ServicePackages.CountAsync();

        var pendingOrderParts = await db.OrderParts.CountAsync(o => o.Status == OrderPartStatus.Pending || o.Status == OrderPartStatus.Approved);
        var monthOrderPartItems = await db.OrderParts.Include(o => o.Lines)
            .Where(o => (o.Status == OrderPartStatus.Approved || o.Status == OrderPartStatus.Finished) && o.OrderDate >= monthStart)
            .ToListAsync();
        var monthOrderPartValue = monthOrderPartItems.Sum(o => o.Total);

        var totalCavities = await db.Cavities.CountAsync();
        var occupiedCavities = await db.Cavities.CountAsync(c => c.Status == CavityStatus.Occupied);
        var availableCavities = await db.Cavities.CountAsync(c => c.Status == CavityStatus.Available && c.IsActive);

        var pendingReceptions = await db.ReceptionSheets.CountAsync(s => s.Status == ReceptionStatus.Pending);
        var todayReceptions = await db.ReceptionSheets.CountAsync(s => s.CreatedAt.Date == today);

        var activeAssignments = await db.AssignmentWorks.CountAsync(a => a.Status == AssignmentWorkStatus.Assigned || a.Status == AssignmentWorkStatus.InProgress);
        var totalEngineers = await db.Engineers.CountAsync(e => e.IsActive);
        var totalGroups = await db.GroupRepairs.CountAsync(g => g.IsActive);

        var pendingInsuranceClaims = await db.InsuranceClaims.CountAsync(c =>
            c.Status == InsuranceClaimStatus.Draft || c.Status == InsuranceClaimStatus.Submitted);
        var approvedInsuranceClaims = await db.InsuranceClaims.Where(c => c.Status == InsuranceClaimStatus.Approved || c.Status == InsuranceClaimStatus.Settled)
            .Select(c => (decimal?)c.ApprovedAmount).ToListAsync();
        var approvedInsuranceAmount = approvedInsuranceClaims.Sum() ?? 0;

        var activeCampaigns = await db.CampaignMarketings.CountAsync(c => c.Status == CampaignMarketingStatus.Active);
        var monthCampaignDiscount = await db.ROs
            .Where(r => r.CampaignMarketingId.HasValue && r.CreatedAt >= monthStart)
            .SumAsync(r => (decimal?)r.CampaignDiscountAmount) ?? 0;

        var pendingCareMaces = await db.CustomerCareMaces.CountAsync(m => m.Status == CustomerCareMaceStatus.Pending);
        var overdueCareMaces = await db.CustomerCareMaces.CountAsync(m => m.Status == CustomerCareMaceStatus.Pending && m.MaceRecomentDate.Date < today);
        var bookedCareMaces = await db.CustomerCareMaces.CountAsync(m => m.Status == CustomerCareMaceStatus.Booked && m.CreatedAt >= monthStart);

        var pendingStockAdjs = await db.StockAdjs.CountAsync(s => s.Status == StockAdjStatus.Pending || s.Status == StockAdjStatus.Executing);
        var discrepancyStockAdjs = await db.StockAdjs.Include(s => s.Items)
            .CountAsync(s => s.Status == StockAdjStatus.Executing && s.Items.Any(i => i.ActualQuantity != i.SystemQuantity));

        var activeBulletins = await db.Bulletins.CountAsync(b => b.IsActive && b.Status == BulletinStatus.Active);
        var pendingBulletinVins = await db.BulletinVins.CountAsync(v => v.Status == BulletinVinStatus.Pending && v.Bulletin.IsActive);

        var pendingPdiRequests = await db.PdiRequests.CountAsync(p => p.Status == PdiRequestStatus.Pending || p.Status == PdiRequestStatus.Approved);
        var completedPdiVehicles = await db.PdiRequestItems.CountAsync(i => i.Status == PdiItemStatus.Passed);

        var pendingOrderComplains = await db.OrderComplains.CountAsync(c => c.DMSStatus == DMSOrderComplainStatus.Pending || c.DMSStatus == DMSOrderComplainStatus.Sent);
        var approvedOrderComplains = await db.OrderComplains.CountAsync(c => c.TSTStatus == TSTOrderComplainStatus.Approved);

        var pendingPartOOs = await db.PartOOs.CountAsync(o => o.Status != PartOOStatus.Cancelled && o.Status != PartOOStatus.Completed && o.SoLuongNo > o.SoLuongTra);
        var stockAvailablePartOOs = await db.PartOOs.Include(o => o.Part).CountAsync(o => o.Status != PartOOStatus.Cancelled && o.Status != PartOOStatus.Completed && o.SoLuongNo > o.SoLuongTra && o.Part.InStock >= (o.SoLuongNo - o.SoLuongTra));

        var activeCusDebitsList = await db.CusDebits.Where(d => d.Status == CusDebitStatus.Active && d.DebitAmount > d.PaidAmount).ToListAsync();
        var activeCusDebits = activeCusDebitsList.Count;
        var totalCusDebitBalance = activeCusDebitsList.Sum(d => d.DebitAmount - d.PaidAmount);
        var overdueCusDebits = activeCusDebitsList.Count(d => d.DueDate.HasValue && d.DueDate.Value.Date < today);

        var totalCustomerGroups = await db.CustomerGroups.CountAsync();
        var activeCustomerGroups = await db.CustomerGroups.CountAsync(g => g.IsActive);
        var totalFleetCars = await db.CustomerGroupMembers.CountAsync(m => m.IsActive);

        var pendingPartPriceRequests = await db.PartPriceRequests.CountAsync(r => r.DMSStatus == DMSReqPartPriceStatus.Draft || r.DMSStatus == DMSReqPartPriceStatus.Sent);
        var respondedPartPriceRequests = await db.PartPriceRequests.CountAsync(r => r.DMSStatus == DMSReqPartPriceStatus.Responded);

        var totalComplaintDiagnosticErrors = await db.ComplaintDiagnosticErrors.CountAsync();
        var totalComplaintCodes = await db.ComplaintDiagnosticErrors.CountAsync(e => e.ErrorType == ComplaintErrorType.Complaint && e.FlagActive);
        var totalDiagnosticCodes = await db.ComplaintDiagnosticErrors.CountAsync(e => e.ErrorType == ComplaintErrorType.Diagnostic && e.FlagActive);

        var curBthMonth = today.Month;
        var curBthDay = today.Day;
        var allBirthdays = await db.CustomerCareBirthdays.ToListAsync();
        var totalBirthdays = allBirthdays.Count;
        var thisMonthBirthdays = allBirthdays.Count(b => b.DateBth.Month == curBthMonth);
        var todayBirthdays = allBirthdays.Count(b => b.DateBth.Month == curBthMonth && b.DateBth.Day == curBthDay);
        var pendingBirthdays = allBirthdays.Count(b => b.Status == CustomerCareBirthdayStatus.Pending);

        var totalWarrantyWorks = await db.WarrantyWorks.CountAsync();
        var activeWarrantyWorks = await db.WarrantyWorks.CountAsync(w => w.FlagActive);

        var totalMaintenanceSettings = await db.MaintenanceSettings.CountAsync();
        var activeMaintenanceSettings = await db.MaintenanceSettings.CountAsync(s => s.FlagActive);

        return new SvcDash(
            ros.Count(r => openStatuses.Contains(r.Status)),
            ros.Count(r => r.Status == ROStatus.InGarage),
            ros.Count(r => r.FinishedAt?.Date == today),
            ros.Where(r => r.Status is ROStatus.Paid or ROStatus.Finished && r.CreatedAt >= monthStart).Sum(r => r.Total),
            await db.Cars.CountAsync(),
            totalParts,
            lowStockParts,
            pendingWarranty,
            approvedWarranty,
            todayAppointments,
            pendingAppointments,
            pendingStockIns,
            monthStockInValue,
            pendingStockOuts,
            monthStockOutValue,
            pendingCustomerCares,
            feedbackCustomerCares,
            pendingPayments,
            monthPaymentRevenue,
            pendingQuotes,
            monthQuoteValue,
            totalServicePackages,
            pendingOrderParts,
            monthOrderPartValue,
            totalCavities,
            occupiedCavities,
            availableCavities,
            pendingReceptions,
            todayReceptions,
            activeAssignments,
            totalEngineers,
            totalGroups,
            pendingInsuranceClaims,
            approvedInsuranceAmount,
            activeCampaigns,
            monthCampaignDiscount,
            pendingCareMaces,
            overdueCareMaces,
            bookedCareMaces,
            byStatus,
            pendingStockAdjs,
            discrepancyStockAdjs,
            activeBulletins,
            pendingBulletinVins,
            pendingPdiRequests,
            completedPdiVehicles,
            pendingOrderComplains,
            approvedOrderComplains,
            pendingPartOOs,
            stockAvailablePartOOs,
            activeCusDebits,
            totalCusDebitBalance,
            overdueCusDebits,
            totalCustomerGroups,
            activeCustomerGroups,
            totalFleetCars,
            pendingPartPriceRequests,
            respondedPartPriceRequests,
            totalComplaintDiagnosticErrors,
            totalComplaintCodes,
            totalDiagnosticCodes,
            totalBirthdays,
            thisMonthBirthdays,
            todayBirthdays,
            pendingBirthdays,
            totalWarrantyWorks,
            activeWarrantyWorks,
            totalMaintenanceSettings,
            activeMaintenanceSettings);
    }

    // --- Warranty Management (Ser_ROWarrantyReport) ---
    public Task<List<WarrantyReport>> WarrantyReportsAsync(WarrantyStatus? status, string? q)
    {
        var query = db.WarrantyReports
            .Include(w => w.Car)
            .Include(w => w.Customer)
            .Include(w => w.RO)
            .Include(w => w.Items)
            .Include(w => w.PartError)
            .AsQueryable();

        if (status.HasValue) query = query.Where(w => w.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(w => w.ReportNo.Contains(term) || w.RO.Code.Contains(term) || w.Car.Plate.Contains(term) || w.Customer.Name.Contains(term));
        }
        return query.OrderByDescending(w => w.CreatedAt).ToListAsync();
    }

    public Task<WarrantyReport?> GetWarrantyReportAsync(int id) =>
        db.WarrantyReports
            .Include(w => w.Car).ThenInclude(c => c.Customer)
            .Include(w => w.Customer)
            .Include(w => w.RO).ThenInclude(r => r.Lines)
            .Include(w => w.Items).ThenInclude(i => i.Part)
            .Include(w => w.PartError)
            .FirstOrDefaultAsync(w => w.Id == id);

    public Task<List<RepairOrder>> ROsEligibleForWarrantyAsync() =>
        db.ROs.Include(r => r.Car).Include(r => r.Customer).Include(r => r.Lines)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task<int> CreateWarrantyReportFromROAsync(int roId, string issueDesc, string diagResult, string? errCodeCD, string? errCodePN, int? partIdError, string createdBy)
    {
        var ro = await db.ROs.Include(r => r.Car).Include(r => r.Customer).Include(r => r.Lines).ThenInclude(l => l.Part)
            .FirstOrDefaultAsync(r => r.Id == roId) ?? throw new InvalidOperationException("RO không tồn tại.");

        if (string.IsNullOrWhiteSpace(issueDesc))
            throw new InvalidOperationException("Triệu chứng / yêu cầu bảo hành không được để trống (CusRequest).");
        if (string.IsNullOrWhiteSpace(diagResult))
            throw new InvalidOperationException("Kết quả chẩn đoán kỹ thuật không được để trống (CarStatus).");

        var reportNo = $"WAR{DateTime.Now:yyMMdd}-{await db.WarrantyReports.CountAsync() + 1:D3}";
        var report = new WarrantyReport
        {
            ReportNo = reportNo,
            ROId = ro.Id,
            CarId = ro.CarId,
            CustomerId = ro.CustomerId,
            Odometer = ro.Odometer,
            Status = WarrantyStatus.Pending,
            IssueDescription = issueDesc.Trim(),
            DiagnosticResult = diagResult.Trim(),
            ErrorCodeCD = string.IsNullOrWhiteSpace(errCodeCD) ? "DTC-001" : errCodeCD.Trim().ToUpperInvariant(),
            ErrorCodePN = string.IsNullOrWhiteSpace(errCodePN) ? "PN-01" : errCodePN.Trim().ToUpperInvariant(),
            PartIDError = (partIdError.HasValue && partIdError.Value > 0) ? partIdError.Value : null,
            CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "web" : createdBy,
            CreatedAt = DateTime.Now
        };

        // Lấy các dòng từ RO: ưu tiên các dòng được gắn ExpenseType = Warranty; nếu chưa đánh dấu thì lấy tất cả dòng
        var warrantyLines = ro.Lines.Where(l => l.ExpenseType == ExpenseType.Warranty).ToList();
        if (warrantyLines.Count == 0) warrantyLines = ro.Lines.ToList();

        foreach (var l in warrantyLines)
        {
            report.Items.Add(new WarrantyReportItem
            {
                Type = l.Type,
                PartId = l.PartId,
                Code = l.Part?.Code ?? (l.Type == LineType.Labor ? "LAB" : "PRT"),
                Name = l.Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                IsAccepted = true
            });
        }
        report.ClaimAmount = report.Items.Sum(i => i.Amount);

        db.WarrantyReports.Add(report);
        await db.SaveChangesAsync();
        return report.Id;
    }

    public async Task<(bool ok, string msg)> TransitionWarrantyAsync(int reportId, WarrantyStatus to, decimal? approvedAmount, string? note)
    {
        var report = await db.WarrantyReports.FirstOrDefaultAsync(w => w.Id == reportId);
        if (report == null) return (false, "Không tìm thấy Báo cáo bảo hành.");

        if (!AllowedNextWarranty(report.Status).Contains(to))
            return (false, $"Không thể chuyển từ '{Ui.WarrantyStatus(report.Status).text}' sang '{Ui.WarrantyStatus(to).text}'.");

        report.Status = to;
        if (to == WarrantyStatus.Sent)
        {
            report.SubmittedAt = DateTime.Now;
        }
        else if (to == WarrantyStatus.Accepted)
        {
            report.DecidedAt = DateTime.Now;
            report.ApprovedAmount = approvedAmount ?? report.ClaimAmount;
            if (!string.IsNullOrWhiteSpace(note)) report.DecisionNote = note.Trim();
        }
        else if (to == WarrantyStatus.Rejected)
        {
            report.DecidedAt = DateTime.Now;
            report.RejectionReason = string.IsNullOrWhiteSpace(note) ? "Không đủ điều kiện bảo hành." : note.Trim();
        }
        else if (to == WarrantyStatus.Reverted)
        {
            report.RejectionReason = string.IsNullOrWhiteSpace(note) ? "Cần bổ sung hồ sơ / hình ảnh hư hỏng." : note.Trim();
        }

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật Báo cáo bảo hành sang: {Ui.WarrantyStatus(to).text}.");
    }

    public async Task<(bool ok, string msg)> DeleteWarrantyReportAsync(int reportId)
    {
        var report = await db.WarrantyReports.Include(w => w.Items).FirstOrDefaultAsync(w => w.Id == reportId);
        if (report == null) return (false, "Không tìm thấy Báo cáo bảo hành.");

        // Guard theo Ser_ROWarrantyReport_DeleteX: Chỉ xóa được khi PEND hoặc REVERT
        if (report.Status is not (WarrantyStatus.Pending or WarrantyStatus.Reverted))
            return (false, "Không thể xóa BCBH đã gửi lên HTC hoặc đã được duyệt.");

        db.WarrantyReportItems.RemoveRange(report.Items);
        db.WarrantyReports.Remove(report);
        await db.SaveChangesAsync();
        return (true, "Đã xóa Báo cáo bảo hành.");
    }

    // --- Service Appointment Management (Ser_App) ---
    public async Task<List<Appointment>> AppointmentsAsync(AppointmentStatus? status, string? q, DateTime? date)
    {
        var query = db.Appointments
            .Include(a => a.Car)
            .Include(a => a.Customer)
            .Include(a => a.RO)
            .AsQueryable();

        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        if (date.HasValue)
        {
            var targetDate = date.Value.Date;
            query = query.Where(a => a.AppointmentDate.Date == targetDate);
        }
        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(a => a.AppNo.ToLower().Contains(kw)
                || a.Car.Plate.ToLower().Contains(kw)
                || a.Car.Model.ToLower().Contains(kw)
                || a.Customer.Name.ToLower().Contains(kw)
                || (a.Customer.Phone != null && a.Customer.Phone.Contains(kw))
                || (a.Advisor != null && a.Advisor.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderBy(a => a.AppointmentDate).ToList();
    }

    public Task<Appointment?> GetAppointmentAsync(int id) =>
        db.Appointments
            .Include(a => a.Car)
            .Include(a => a.Customer)
            .Include(a => a.RO)
            .Include(a => a.ReceptionSheet)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<int> CreateAppointmentAsync(Appointment app)
    {
        var car = await db.Cars.FirstOrDefaultAsync(c => c.Id == app.CarId)
            ?? throw new InvalidOperationException("Không tìm thấy thông tin xe.");

        app.CustomerId = car.CustomerId;
        if (string.IsNullOrWhiteSpace(app.AppNo))
        {
            var countToday = await db.Appointments.CountAsync();
            app.AppNo = $"APP{DateTime.Today:yyMMdd}-{countToday + 1:D3}";
        }
        app.Status = AppointmentStatus.Pending;
        app.CreatedAt = DateTime.Now;

        db.Appointments.Add(app);
        await db.SaveChangesAsync();
        return app.Id;
    }

    public async Task<(bool ok, string msg)> TransitionAppointmentStatusAsync(int id, AppointmentStatus to, string? cancelReason = null)
    {
        var app = await db.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return (false, "Không tìm thấy lịch hẹn.");

        if (!AllowedNextAppointment(app.Status).Contains(to))
            return (false, $"Không thể chuyển từ '{Ui.AppointmentStatus(app.Status).text}' sang '{Ui.AppointmentStatus(to).text}'.");

        app.Status = to;
        if (to == AppointmentStatus.Confirmed)
        {
            app.ConfirmedAt = DateTime.Now;
        }
        else if (to == AppointmentStatus.Cancelled)
        {
            app.CancelReason = string.IsNullOrWhiteSpace(cancelReason) ? "Khách báo hủy / bận." : cancelReason.Trim();
        }

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật trạng thái lịch hẹn sang: {Ui.AppointmentStatus(to).text}.");
    }

    public async Task<(bool ok, string msg, int? roId)> CheckInAppointmentAsync(int id, int odometer, string? technician)
    {
        var app = await db.Appointments.Include(a => a.Car).Include(a => a.Customer).FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return (false, "Không tìm thấy lịch hẹn.", null);

        if (app.Status == AppointmentStatus.CheckedIn && app.ROId.HasValue)
            return (false, "Lịch hẹn này đã được tiếp nhận tạo Lệnh sửa chữa.", app.ROId);

        if (app.Status == AppointmentStatus.Cancelled)
            return (false, "Lịch hẹn đã bị hủy, không thể tiếp nhận vào xưởng.", null);

        // Sinh mã RO tự động theo quy chuẩn idn.CarService
        var roCount = await db.ROs.CountAsync();
        var roCode = $"RO{DateTime.Today:yyMMdd}-{roCount + 1:D3}";

        var intakeNote = $"[Đặt hẹn {app.AppNo} - {Ui.AppServiceType(app.ServiceType)}]";
        if (!string.IsNullOrWhiteSpace(app.Cavity)) intakeNote += $" [Khoang: {app.Cavity}]";
        if (!string.IsNullOrWhiteSpace(app.CustomerRequest)) intakeNote += $" {app.CustomerRequest}";

        var ro = new RepairOrder
        {
            Code = roCode,
            CarId = app.CarId,
            CustomerId = app.CustomerId,
            Status = ROStatus.InGarage, // Xe vào xưởng
            Odometer = odometer > 0 ? odometer : 0,
            IntakeNote = intakeNote,
            Technician = !string.IsNullOrWhiteSpace(technician) ? technician.Trim() : app.Advisor,
            AppointmentId = app.Id,
            CreatedBy = "checkin",
            CreatedAt = DateTime.Now,
            IntakeAt = DateTime.Now
        };

        db.ROs.Add(ro);
        await db.SaveChangesAsync();

        app.ROId = ro.Id;
        app.Status = AppointmentStatus.CheckedIn;
        app.CheckedInAt = DateTime.Now;
        await db.SaveChangesAsync();

        return (true, $"Tiếp nhận thành công! Đã tạo Lệnh sửa chữa {ro.Code}.", ro.Id);
    }

    public async Task<(bool ok, string msg)> DeleteAppointmentAsync(int id)
    {
        var app = await db.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return (false, "Không tìm thấy lịch hẹn.");

        if (app.Status is not (AppointmentStatus.Pending or AppointmentStatus.Cancelled))
            return (false, "Chỉ xóa được lịch hẹn ở trạng thái Mới tạo hoặc Đã hủy.");

        if (app.ROId.HasValue)
            return (false, "Không thể xóa lịch hẹn đã sinh Lệnh sửa chữa.");

        db.Appointments.Remove(app);
        await db.SaveChangesAsync();
        return (true, "Đã xóa lịch hẹn thành công.");
    }

    // --- Stock-In Management (Ser_Inv_StockIn & Ser_Inv_StockInDetail) ---
    public async Task<List<StockIn>> StockInsAsync(StockInStatus? status, string? q, DateTime? fromDate, DateTime? toDate)
    {
        var query = db.StockIns.Include(s => s.Items).ThenInclude(i => i.Part).AsQueryable();

        if (status.HasValue) query = query.Where(s => s.Status == status.Value);
        if (fromDate.HasValue) query = query.Where(s => s.StockInDate.Date >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(s => s.StockInDate.Date <= toDate.Value.Date);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(s => s.StockInNo.ToLower().Contains(kw)
                || s.SupplierName.ToLower().Contains(kw)
                || (s.BillNo != null && s.BillNo.ToLower().Contains(kw))
                || (s.Description != null && s.Description.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(s => s.StockInDate).ThenByDescending(s => s.CreatedAt).ToList();
    }

    public Task<StockIn?> GetStockInAsync(int id) =>
        db.StockIns.Include(s => s.Items).ThenInclude(i => i.Part).FirstOrDefaultAsync(s => s.Id == id);

    public async Task<int> CreateStockInAsync(StockIn stockIn, List<StockInDetail> items)
    {
        if (string.IsNullOrWhiteSpace(stockIn.SupplierName))
            throw new InvalidOperationException("Vui lòng nhập tên nhà cung cấp (SupplierName).");

        if (items.Count == 0)
            throw new InvalidOperationException("Vui lòng thêm ít nhất một phụ tùng vào phiếu nhập kho.");

        if (string.IsNullOrWhiteSpace(stockIn.StockInNo))
        {
            var countToday = await db.StockIns.CountAsync();
            stockIn.StockInNo = $"NK{DateTime.Today:yyMMdd}-{countToday + 1:D3}";
        }
        stockIn.Status = StockInStatus.Pending;
        stockIn.CreatedAt = DateTime.Now;

        foreach (var item in items)
        {
            var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId)
                ?? throw new InvalidOperationException($"Phụ tùng ID={item.PartId} không tồn tại.");

            item.PartCode = part.Code;
            item.PartName = part.Name;
            item.Unit = string.IsNullOrWhiteSpace(item.Unit) ? part.Unit : item.Unit;
            if (item.UnitPrice <= 0) item.UnitPrice = part.CostPrice > 0 ? part.CostPrice : part.SalePrice;
            if (string.IsNullOrWhiteSpace(item.Location)) item.Location = part.Location;
            stockIn.Items.Add(item);
        }

        db.StockIns.Add(stockIn);
        await db.SaveChangesAsync();
        return stockIn.Id;
    }

    public async Task<(bool ok, string msg)> TransitionStockInStatusAsync(int id, StockInStatus to, string? approvedBy = null, string? note = null)
    {
        var stockIn = await db.StockIns.Include(s => s.Items).ThenInclude(i => i.Part).FirstOrDefaultAsync(s => s.Id == id);
        if (stockIn == null) return (false, "Không tìm thấy phiếu nhập kho.");

        if (!AllowedNextStockIn(stockIn.Status).Contains(to))
            return (false, $"Không thể chuyển từ '{Ui.StockInStatus(stockIn.Status).text}' sang '{Ui.StockInStatus(to).text}'.");

        if (to == StockInStatus.Finished)
        {
            if (stockIn.Items.Count == 0)
                return (false, "Phiếu nhập kho chưa có phụ tùng nào, không thể duyệt nhập kho.");

            // Tự động tăng tồn kho và cập nhật giá vốn theo Ser_Inv_StockIn_StatusUpdateToFinished
            foreach (var item in stockIn.Items)
            {
                var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId);
                if (part != null)
                {
                    part.InStock += item.Quantity;
                    if (item.UnitPrice > 0)
                    {
                        part.CostPrice = item.UnitPrice; // Cập nhật giá vốn nhập mới nhất
                    }
                }
            }
            stockIn.Status = StockInStatus.Finished;
            stockIn.FinishedAt = DateTime.Now;
            stockIn.ApprovedBy = string.IsNullOrWhiteSpace(approvedBy) ? "Kế toán kho" : approvedBy.Trim();
            if (!string.IsNullOrWhiteSpace(note))
                stockIn.Description = string.IsNullOrWhiteSpace(stockIn.Description) ? note.Trim() : $"{stockIn.Description} | {note.Trim()}";

            // Tự động ghi nhận công nợ Nhà Cung Cấp theo Ser_SupplierDebit idn.CarService
            var existingDebit = await db.SupplierDebits.FirstOrDefaultAsync(d => d.StockInId == stockIn.Id);
            if (existingDebit == null && stockIn.Total > 0)
            {
                var supplier = await db.Suppliers.FirstOrDefaultAsync(s => s.Name == stockIn.SupplierName || s.Code == stockIn.SupplierName);
                if (supplier != null)
                {
                    var count = await db.SupplierDebits.CountAsync() + 1;
                    var newDebit = new SupplierDebit
                    {
                        DebitNo = $"SDB{DateTime.Today:yyMMdd}-{count:D3}",
                        SupplierId = supplier.Id,
                        StockInId = stockIn.Id,
                        OrderPartId = stockIn.OrderPartId,
                        DebitType = SupplierDebitType.StockIn,
                        Status = SupplierDebitStatus.Active,
                        DebitDate = stockIn.StockInDate,
                        DueDate = stockIn.StockInDate.AddDays(30),
                        DebitAmount = stockIn.Total,
                        PaidAmount = 0,
                        Description = $"Công nợ tiền hàng nhập kho {stockIn.StockInNo}" + (!string.IsNullOrWhiteSpace(stockIn.BillNo) ? $" (HĐ: {stockIn.BillNo})" : ""),
                        CreatedBy = stockIn.ApprovedBy ?? "Kế toán kho",
                        CreatedAt = DateTime.Now
                    };
                    db.SupplierDebits.Add(newDebit);
                }
            }

            await db.SaveChangesAsync();
            return (true, $"Đã duyệt nhập kho {stockIn.StockInNo}! Tồn kho và giá vốn phụ tùng đã được cập nhật thành công.");
        }
        else if (to == StockInStatus.Executing)
        {
            stockIn.Status = StockInStatus.Executing;
            if (!string.IsNullOrWhiteSpace(note))
                stockIn.Description = string.IsNullOrWhiteSpace(stockIn.Description) ? note.Trim() : $"{stockIn.Description} | {note.Trim()}";

            await db.SaveChangesAsync();
            return (true, $"Đã chuyển phiếu {stockIn.StockInNo} sang trạng thái: {Ui.StockInStatus(to).text}.");
        }
        else if (to == StockInStatus.Rejected)
        {
            stockIn.Status = StockInStatus.Rejected;
            if (!string.IsNullOrWhiteSpace(note))
                stockIn.Description = string.IsNullOrWhiteSpace(stockIn.Description) ? $"[Hủy: {note.Trim()}]" : $"{stockIn.Description} [Hủy: {note.Trim()}]";

            await db.SaveChangesAsync();
            return (true, $"Đã hủy phiếu nhập kho {stockIn.StockInNo}.");
        }

        return (false, "Trạng thái không hợp lệ.");
    }

    public async Task<(bool ok, string msg)> DeleteStockInAsync(int id)
    {
        var stockIn = await db.StockIns.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == id);
        if (stockIn == null) return (false, "Không tìm thấy phiếu nhập kho.");

        // Guard theo Ser_Inv_StockIn idn.CarService: Chỉ được xóa khi Pending hoặc Rejected, KHÔNG được xóa khi Finished
        if (stockIn.Status == StockInStatus.Finished)
            return (false, "Không thể xóa phiếu nhập kho đã hoàn tất (Finished).");

        db.StockInDetails.RemoveRange(stockIn.Items);
        db.StockIns.Remove(stockIn);
        await db.SaveChangesAsync();
        return (true, "Đã xóa phiếu nhập kho.");
    }

    // --- Stock-Out Management (Ser_Inv_StockOut & Ser_Inv_StockOutDetail) ---
    public async Task<List<StockOut>> StockOutsAsync(StockOutStatus? status, string? q, DateTime? fromDate, DateTime? toDate, int? roId = null)
    {
        var query = db.StockOuts
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .Include(s => s.RO)
            .Include(s => s.Car)
            .Include(s => s.Customer)
            .AsQueryable();

        if (status.HasValue) query = query.Where(s => s.Status == status.Value);
        if (roId.HasValue && roId.Value > 0) query = query.Where(s => s.ROId == roId.Value);
        if (fromDate.HasValue) query = query.Where(s => s.StockOutDate.Date >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(s => s.StockOutDate.Date <= toDate.Value.Date);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(s => s.StockOutNo.ToLower().Contains(kw)
                || (s.RecipientName != null && s.RecipientName.ToLower().Contains(kw))
                || (s.RO != null && s.RO.Code.ToLower().Contains(kw))
                || (s.Car != null && s.Car.Plate.ToLower().Contains(kw))
                || (s.Customer != null && s.Customer.Name.ToLower().Contains(kw))
                || (s.Description != null && s.Description.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(s => s.StockOutDate).ThenByDescending(s => s.CreatedAt).ToList();
    }

    public Task<StockOut?> GetStockOutAsync(int id) =>
        db.StockOuts
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .Include(s => s.RO).ThenInclude(r => r!.Lines)
            .Include(s => s.Car)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == id);

    public Task<List<RepairOrder>> ROsForStockOutAsync() =>
        db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Lines).ThenInclude(l => l.Part)
            .Where(r => r.Status != ROStatus.Rejected && r.Status != ROStatus.NotResponding && r.Status != ROStatus.Finished)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<int> CreateStockOutAsync(StockOut stockOut, List<StockOutDetail> items)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Vui lòng thêm ít nhất một phụ tùng vào phiếu xuất kho.");

        if (stockOut.ROId.HasValue && stockOut.ROId.Value > 0)
        {
            var ro = await db.ROs.Include(r => r.Car).Include(r => r.Customer).FirstOrDefaultAsync(r => r.Id == stockOut.ROId.Value);
            if (ro != null)
            {
                stockOut.CarId ??= ro.CarId;
                stockOut.CustomerId ??= ro.CustomerId;
                if (string.IsNullOrWhiteSpace(stockOut.RecipientName))
                    stockOut.RecipientName = ro.Technician ?? ro.Customer.Name;
            }
        }

        if (string.IsNullOrWhiteSpace(stockOut.StockOutNo))
        {
            var countToday = await db.StockOuts.CountAsync();
            stockOut.StockOutNo = $"XK{DateTime.Today:yyMMdd}-{countToday + 1:D3}";
        }
        stockOut.Status = StockOutStatus.Pending;
        stockOut.CreatedAt = DateTime.Now;

        foreach (var item in items)
        {
            var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId)
                ?? throw new InvalidOperationException($"Phụ tùng ID={item.PartId} không tồn tại.");

            item.PartCode = part.Code;
            item.PartName = part.Name;
            item.Unit = string.IsNullOrWhiteSpace(item.Unit) ? part.Unit : item.Unit;
            if (item.UnitPrice <= 0) item.UnitPrice = part.SalePrice > 0 ? part.SalePrice : part.CostPrice;
            if (string.IsNullOrWhiteSpace(item.Location)) item.Location = part.Location;
            stockOut.Items.Add(item);
        }

        db.StockOuts.Add(stockOut);
        await db.SaveChangesAsync();
        return stockOut.Id;
    }

    public async Task<(bool ok, string msg)> TransitionStockOutStatusAsync(int id, StockOutStatus to, string? approvedBy = null, string? note = null)
    {
        var stockOut = await db.StockOuts.Include(s => s.Items).ThenInclude(i => i.Part).Include(s => s.RO).FirstOrDefaultAsync(s => s.Id == id);
        if (stockOut == null) return (false, "Không tìm thấy phiếu xuất kho.");

        if (!AllowedNextStockOut(stockOut.Status).Contains(to))
            return (false, $"Không thể chuyển từ '{Ui.StockOutStatus(stockOut.Status).text}' sang '{Ui.StockOutStatus(to).text}'.");

        if (to == StockOutStatus.Finished)
        {
            if (stockOut.Items.Count == 0)
                return (false, "Phiếu xuất kho chưa có mặt hàng phụ tùng nào.");

            // Kiểm tra tồn kho thực tế trước khi duyệt xuất (CheckStockBalance theo Ser_Inv_StockOut)
            foreach (var item in stockOut.Items)
            {
                var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId);
                if (part == null)
                    return (false, $"Phụ tùng '{item.PartCode}' không còn tồn tại trong hệ thống.");

                if (part.InStock < item.Quantity)
                    return (false, $"Không đủ tồn kho cho '{part.Name}' [{part.Code}]: Hiện còn {part.InStock:0.##} {part.Unit}, yêu cầu xuất {item.Quantity:0.##} {part.Unit}.");
            }

            // Trừ tồn kho thực tế
            foreach (var item in stockOut.Items)
            {
                var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId);
                if (part != null)
                {
                    part.InStock -= item.Quantity;
                }
            }

            stockOut.Status = StockOutStatus.Finished;
            stockOut.FinishedAt = DateTime.Now;
            stockOut.ApprovedBy = string.IsNullOrWhiteSpace(approvedBy) ? "Thủ kho" : approvedBy.Trim();
            if (!string.IsNullOrWhiteSpace(note))
                stockOut.Description = string.IsNullOrWhiteSpace(stockOut.Description) ? note.Trim() : $"{stockOut.Description} | {note.Trim()}";

            await db.SaveChangesAsync();
            return (true, $"Đã hoàn tất xuất kho {stockOut.StockOutNo}! Đã trừ tồn kho {stockOut.Items.Count} mặt hàng phụ tùng.");
        }
        else if (to == StockOutStatus.Executing)
        {
            stockOut.Status = StockOutStatus.Executing;
            if (!string.IsNullOrWhiteSpace(note))
                stockOut.Description = string.IsNullOrWhiteSpace(stockOut.Description) ? note.Trim() : $"{stockOut.Description} | {note.Trim()}";

            await db.SaveChangesAsync();
            return (true, $"Đã chuyển phiếu xuất {stockOut.StockOutNo} sang trạng thái: {Ui.StockOutStatus(to).text}.");
        }
        else if (to == StockOutStatus.Rejected)
        {
            stockOut.Status = StockOutStatus.Rejected;
            if (!string.IsNullOrWhiteSpace(note))
                stockOut.Description = string.IsNullOrWhiteSpace(stockOut.Description) ? $"[Hủy: {note.Trim()}]" : $"{stockOut.Description} [Hủy: {note.Trim()}]";

            await db.SaveChangesAsync();
            return (true, $"Đã hủy phiếu xuất kho {stockOut.StockOutNo}.");
        }

        return (false, "Trạng thái không hợp lệ.");
    }

    public async Task<(bool ok, string msg)> DeleteStockOutAsync(int id)
    {
        var stockOut = await db.StockOuts.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == id);
        if (stockOut == null) return (false, "Không tìm thấy phiếu xuất kho.");

        // Guard: Chỉ được xóa khi Pending hoặc Rejected, KHÔNG được xóa khi Finished
        if (stockOut.Status == StockOutStatus.Finished)
            return (false, "Không thể xóa phiếu xuất kho đã hoàn tất (Finished).");

        db.StockOutDetails.RemoveRange(stockOut.Items);
        db.StockOuts.Remove(stockOut);
        await db.SaveChangesAsync();
        return (true, "Đã xóa phiếu xuất kho.");
    }

    // --- Customer Care 24h Management (Ser_CustomerCare24h) ---
    public async Task<List<CustomerCare>> CustomerCaresAsync(CustomerCareStatus? status, string? q)
    {
        var query = db.CustomerCares
            .Include(c => c.RO)
            .Include(c => c.Car)
            .Include(c => c.Customer)
            .AsQueryable();

        if (status.HasValue) query = query.Where(c => c.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(c => c.CareNo.ToLower().Contains(kw)
                || c.Car.Plate.ToLower().Contains(kw)
                || c.Car.Model.ToLower().Contains(kw)
                || c.Customer.Name.ToLower().Contains(kw)
                || (c.Customer.Phone != null && c.Customer.Phone.ToLower().Contains(kw))
                || (c.RO != null && c.RO.Code.ToLower().Contains(kw))
                || (c.CustomerFeedback != null && c.CustomerFeedback.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(c => c.CreatedAt).ToList();
    }

    public Task<CustomerCare?> GetCustomerCareAsync(int id) =>
        db.CustomerCares
            .Include(c => c.RO).ThenInclude(r => r.Lines).ThenInclude(l => l.Part)
            .Include(c => c.Car)
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<int> CreateCustomerCareAsync(CustomerCare care)
    {
        var ro = await db.ROs.Include(r => r.Car).Include(r => r.Customer).FirstOrDefaultAsync(r => r.Id == care.ROId)
            ?? throw new InvalidOperationException("Không tìm thấy Lệnh sửa chữa.");

        care.CarId = ro.CarId;
        care.CustomerId = ro.CustomerId;
        care.CareNo = $"CC{DateTime.Now:yyMMdd}-{await db.CustomerCares.CountAsync() + 1:D3}";
        care.CreatedAt = DateTime.Now;

        db.CustomerCares.Add(care);
        await db.SaveChangesAsync();
        return care.Id;
    }

    public async Task<(bool ok, string msg)> UpdateCustomerCareSurveyAsync(int id, CustomerCareStatus status,
        bool hasCarProblem, int? qualityRating, int? staffRating, bool? willingToReturn, int? facilityRating,
        string? feedback, string? internalNote, string? contactedBy)
    {
        var care = await db.CustomerCares.FirstOrDefaultAsync(c => c.Id == id);
        if (care == null) return (false, "Không tìm thấy phiếu CSKH.");

        care.Status = status;
        care.HasCarProblem = hasCarProblem;
        care.QualityRating = qualityRating;
        care.StaffRating = staffRating;
        care.WillingToReturn = willingToReturn;
        care.FacilityRating = facilityRating;
        care.CustomerFeedback = feedback?.Trim();
        care.InternalNote = internalNote?.Trim();
        care.ContactedBy = string.IsNullOrWhiteSpace(contactedBy) ? "CSKH" : contactedBy.Trim();
        care.ContactedDate = DateTime.Now;

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật kết quả khảo sát CSKH ({Ui.CustomerCareStatus(status).text}).");
    }

    public async Task<(bool ok, string msg)> DeleteCustomerCareAsync(int id)
    {
        var care = await db.CustomerCares.FirstOrDefaultAsync(c => c.Id == id);
        if (care == null) return (false, "Không tìm thấy phiếu CSKH.");

        db.CustomerCares.Remove(care);
        await db.SaveChangesAsync();
        return (true, "Đã xóa phiếu CSKH.");
    }

    public Task<List<RepairOrder>> ROsEligibleForCustomerCareAsync() =>
        db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.CustomerCares)
            .Where(r => (r.Status == ROStatus.Finished || r.Status == ROStatus.Paid) && !r.CustomerCares.Any())
            .OrderByDescending(r => r.FinishedAt ?? r.CreatedAt)
            .ToListAsync();

    // --- Customer Care 72h & Re-Repair Control Management (Ser_CustomerCare72h idn.CarService) ---
    public async Task<List<CustomerCare72h>> CustomerCare72hsAsync(CustomerCare72hStatus? status, string? q, bool? needFeedbackOnly = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = db.CustomerCare72hs
            .Include(c => c.RO).ThenInclude(r => r.Lines)
            .Include(c => c.Car)
            .Include(c => c.Customer)
            .Include(c => c.ReRepairRO)
            .Include(c => c.ReRepairAppointment)
            .AsQueryable();

        if (status.HasValue) query = query.Where(c => c.Status == status.Value);
        if (needFeedbackOnly == true) query = query.Where(c => c.Status == CustomerCare72hStatus.NeedFeedback || c.IsReRepairAlert || c.HasTechnicalProblem);
        if (fromDate.HasValue) query = query.Where(c => c.ScheduledDate.Date >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(c => c.ScheduledDate.Date <= toDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(c => c.Care72No.ToLower().Contains(kw)
                || c.Car.Plate.ToLower().Contains(kw)
                || c.Car.Model.ToLower().Contains(kw)
                || c.Customer.Name.ToLower().Contains(kw)
                || (c.Customer.Phone != null && c.Customer.Phone.ToLower().Contains(kw))
                || (c.RO != null && c.RO.Code.ToLower().Contains(kw))
                || (c.ProblemDetails != null && c.ProblemDetails.ToLower().Contains(kw))
                || (c.CustomerFeedback != null && c.CustomerFeedback.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(c => c.ScheduledDate).ThenByDescending(c => c.CreatedAt).ToList();
    }

    public Task<CustomerCare72h?> GetCustomerCare72hAsync(int id) =>
        db.CustomerCare72hs
            .Include(c => c.RO).ThenInclude(r => r.Lines).ThenInclude(l => l.Part)
            .Include(c => c.Car)
            .Include(c => c.Customer)
            .Include(c => c.ReRepairRO).ThenInclude(r => r!.Lines)
            .Include(c => c.ReRepairAppointment)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<CustomerCare72hSummaryDto> GetCustomerCare72hSummaryAsync()
    {
        var all = await db.CustomerCare72hs.ToListAsync();
        var contacted = all.Where(x => x.Status != CustomerCare72hStatus.Pending).ToList();
        var satisfied = all.Count(x => x.Status == CustomerCare72hStatus.ContactedSatisfied);
        var firftCount = contacted.Count(x => x.FixedRightFirstTime == true);
        var rated = contacted.Where(x => x.SatisfactionRating.HasValue && x.SatisfactionRating.Value > 0).ToList();

        return new CustomerCare72hSummaryDto
        {
            TotalCount = all.Count,
            PendingCount = all.Count(x => x.Status == CustomerCare72hStatus.Pending),
            SatisfiedCount = satisfied,
            NeedFeedbackCount = all.Count(x => x.Status == CustomerCare72hStatus.NeedFeedback),
            RejectedCount = all.Count(x => x.Status == CustomerCare72hStatus.Rejected),
            FirftRate = contacted.Count > 0 ? Math.Round((decimal)firftCount / contacted.Count * 100, 1) : 100m,
            AverageSatisfaction = rated.Count > 0 ? Math.Round((decimal)rated.Average(x => x.SatisfactionRating!.Value), 2) : 5.0m,
            ReRepairAlertCount = all.Count(x => x.IsReRepairAlert || x.HasTechnicalProblem || x.Status == CustomerCare72hStatus.NeedFeedback)
        };
    }

    public async Task<int> CreateCustomerCare72hAsync(CustomerCare72h care)
    {
        var ro = await db.ROs.Include(r => r.Car).Include(r => r.Customer).FirstOrDefaultAsync(r => r.Id == care.ROId)
            ?? throw new InvalidOperationException("Không tìm thấy Lệnh sửa chữa gốc.");

        care.CarId = ro.CarId;
        care.CustomerId = ro.CustomerId;
        care.ROFinishedDate = ro.FinishedAt ?? ro.CreatedAt;
        care.ScheduledDate = care.ROFinishedDate.AddDays(3);
        care.Care72No = $"CC72-{DateTime.Now:yyMMdd}-{await db.CustomerCare72hs.CountAsync() + 1:D3}";
        care.CreatedAt = DateTime.Now;

        db.CustomerCare72hs.Add(care);
        await db.SaveChangesAsync();
        return care.Id;
    }

    public async Task<int> GenerateCustomerCare72hFromROAsync(int roId, string? createdBy = null)
    {
        var exists = await db.CustomerCare72hs.AnyAsync(c => c.ROId == roId);
        if (exists)
        {
            var existing = await db.CustomerCare72hs.FirstAsync(c => c.ROId == roId);
            return existing.Id;
        }

        var ro = await db.ROs.FirstOrDefaultAsync(r => r.Id == roId)
            ?? throw new InvalidOperationException("Không tìm thấy Lệnh sửa chữa.");

        var care = new CustomerCare72h
        {
            ROId = ro.Id,
            CarId = ro.CarId,
            CustomerId = ro.CustomerId,
            ROFinishedDate = ro.FinishedAt ?? ro.CreatedAt,
            ScheduledDate = (ro.FinishedAt ?? ro.CreatedAt).AddDays(3),
            Status = CustomerCare72hStatus.Pending,
            CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "system" : createdBy.Trim()
        };

        return await CreateCustomerCare72hAsync(care);
    }

    public Task<List<RepairOrder>> ROsEligibleForCustomerCare72hAsync() =>
        db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.CustomerCare72hs)
            .Where(r => (r.Status == ROStatus.Finished || r.Status == ROStatus.Paid) && !r.CustomerCare72hs.Any())
            .OrderByDescending(r => r.FinishedAt ?? r.CreatedAt)
            .ToListAsync();

    public async Task<(bool ok, string msg)> UpdateCustomerCare72hSurveyAsync(int id, CustomerCare72hStatus status,
        bool? serviceExplained, bool? basicNeedsMet, bool hasTechnicalProblem, string? problemDetails,
        bool? fixedRightFirstTime, int? satisfactionRating, string? customerFeedback, string? reRepairAction,
        string? internalNote, string? contactedBy)
    {
        var care = await db.CustomerCare72hs.Include(c => c.RO).FirstOrDefaultAsync(c => c.Id == id);
        if (care == null) return (false, "Không tìm thấy phiếu CSKH 72h.");

        care.Status = status;
        care.ServiceExplained = serviceExplained;
        care.BasicNeedsMet = basicNeedsMet;
        care.HasTechnicalProblem = hasTechnicalProblem;
        care.ProblemDetails = problemDetails?.Trim();
        care.FixedRightFirstTime = fixedRightFirstTime;
        care.SatisfactionRating = satisfactionRating;
        care.CustomerFeedback = customerFeedback?.Trim();
        care.ReRepairAction = reRepairAction?.Trim();
        care.InternalNote = internalNote?.Trim();
        care.ContactedBy = string.IsNullOrWhiteSpace(contactedBy) ? "CSKH" : contactedBy.Trim();
        care.ContactedDate = DateTime.Now;

        // Tự động phân luồng Re-Repair Alert khi có lỗi kỹ thuật hoặc cần phản hồi
        if (hasTechnicalProblem || status == CustomerCare72hStatus.NeedFeedback)
        {
            care.IsReRepairAlert = true;
            care.Status = CustomerCare72hStatus.NeedFeedback;
            if (care.RO != null)
            {
                care.RO.IsReRepair = true;
            }
        }
        else if (status == CustomerCare72hStatus.ContactedSatisfied)
        {
            care.IsReRepairAlert = false;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật kết quả khảo sát CSKH 72h ({Ui.CustomerCare72hStatus(care.Status).text}).");
    }

    public async Task<(bool ok, string msg, int? roId)> CreateReRepairFromCare72hAsync(int id, string? technician = null, string? note = null)
    {
        var care = await db.CustomerCare72hs
            .Include(c => c.RO)
            .Include(c => c.Car)
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (care == null) return (false, "Không tìm thấy phiếu CSKH 72h.", null);
        if (care.ReRepairROId.HasValue) return (false, $"Phiếu CSKH này đã được lập Lệnh phản tu (mã RO #{care.ReRepairROId.Value}).", care.ReRepairROId);

        // Tạo Lệnh sửa chữa phản tu (Re-Repair RO) gắn cờ IsReRepair = true
        var count = await db.ROs.CountAsync();
        var reRepairRO = new RepairOrder
        {
            Code = $"RO-RR{DateTime.Now:yyMMdd}-{count + 1:D3}",
            CarId = care.CarId,
            CustomerId = care.CustomerId,
            Status = ROStatus.HasRO,
            Technician = string.IsNullOrWhiteSpace(technician) ? care.RO?.Technician ?? "KTV Trưởng" : technician.Trim(),
            CreatedBy = "CSKH-72h",
            CreatedAt = DateTime.Now,
            IsReRepair = true,
            ReRepairParentROId = care.ROId,
            Lines = new List<RepairLine>
            {
                new RepairLine
                {
                    Type = LineType.Labor,
                    Name = $"[TÁI KHÁM / PHẢN TU 72H] Kiểm tra & khắc phục: {(string.IsNullOrWhiteSpace(care.ProblemDetails) ? "Khách báo xe có tiếng kêu / lỗi sau 72h" : care.ProblemDetails)}",
                    Quantity = 1.0m,
                    UnitPrice = 0m, // Miễn phí chi phí cho khách vì đây là bảo hành dịch vụ phản tu
                    ExpenseType = ExpenseType.Internal
                }
            }
        };

        db.ROs.Add(reRepairRO);
        await db.SaveChangesAsync();

        care.ReRepairROId = reRepairRO.Id;
        care.ReRepairAction = $"Đã lập Lệnh phản tu {reRepairRO.Code} tiếp nhận kiểm tra miễn phí cho khách.";
        if (!string.IsNullOrWhiteSpace(note))
        {
            care.InternalNote = string.IsNullOrWhiteSpace(care.InternalNote) ? note.Trim() : $"{care.InternalNote} | {note.Trim()}";
        }

        await db.SaveChangesAsync();
        return (true, $"Đã tạo thành công Lệnh phản tu {reRepairRO.Code} tiếp nhận xử lý xe {care.Car.Plate}!", reRepairRO.Id);
    }

    public async Task<(bool ok, string msg)> DeleteCustomerCare72hAsync(int id)
    {
        var care = await db.CustomerCare72hs.FirstOrDefaultAsync(c => c.Id == id);
        if (care == null) return (false, "Không tìm thấy phiếu CSKH 72h.");

        db.CustomerCare72hs.Remove(care);
        await db.SaveChangesAsync();
        return (true, "Đã xóa phiếu CSKH 72h.");
    }

    // --- Payment & Cashier Management (Ser_Payment & Ser_PaymentDetail) ---
    public async Task<List<Payment>> PaymentsAsync(PaymentStatus? status, string? q, DateTime? fromDate, DateTime? toDate, int? roId = null)
    {
        var query = db.Payments
            .Include(p => p.RO)
            .Include(p => p.Customer)
            .Include(p => p.Car)
            .AsQueryable();

        if (status.HasValue) query = query.Where(p => p.Status == status.Value);
        if (roId.HasValue && roId.Value > 0) query = query.Where(p => p.ROId == roId.Value);
        if (fromDate.HasValue) query = query.Where(p => p.PaymentDate >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(p => p.PaymentDate <= toDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(p => p.PaymentNo.ToLower().Contains(kw)
                || p.PayPersonName.ToLower().Contains(kw)
                || (p.PayPersonPhone != null && p.PayPersonPhone.ToLower().Contains(kw))
                || (p.TransactionRef != null && p.TransactionRef.ToLower().Contains(kw))
                || p.RO.Code.ToLower().Contains(kw)
                || p.Car.Plate.ToLower().Contains(kw)
                || p.Customer.Name.ToLower().Contains(kw));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.CreatedAt).ToList();
    }

    public Task<Payment?> GetPaymentAsync(int id) =>
        db.Payments
            .Include(p => p.RO).ThenInclude(r => r.Lines)
            .Include(p => p.Customer)
            .Include(p => p.Car)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<int> CreatePaymentAsync(Payment payment)
    {
        var ro = await db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Lines)
            .Include(r => r.Payments)
            .FirstOrDefaultAsync(r => r.Id == payment.ROId)
            ?? throw new InvalidOperationException("Không tìm thấy Lệnh sửa chữa (RO).");

        payment.CustomerId = ro.CustomerId;
        payment.CarId = ro.CarId;
        payment.RoTotalAmount = ro.Total;
        payment.ThirdPartyAmount = ro.WarrantyTotal;
        payment.PayableAmount = Math.Max(0, payment.RoTotalAmount - payment.DiscountAmount - payment.ThirdPartyAmount);

        if (payment.PaymentAmount <= 0)
        {
            payment.PaymentAmount = payment.PayableAmount;
        }

        if (string.IsNullOrWhiteSpace(payment.PayPersonName))
        {
            payment.PayPersonName = ro.Customer.Name;
        }
        if (string.IsNullOrWhiteSpace(payment.PayPersonPhone))
        {
            payment.PayPersonPhone = ro.Customer.Phone;
        }

        payment.PaymentNo = $"PT{DateTime.Now:yyMMdd}-{await db.Payments.CountAsync() + 1:D3}";
        payment.CreatedAt = DateTime.Now;

        if (payment.Status == PaymentStatus.Completed)
        {
            payment.CompletedAt = DateTime.Now;
            // Nếu thu tiền đủ hoặc hoàn tất thanh toán, tự động chuyển trạng thái RO sang PAID nếu đang ở CheckEnd / Repaired / HasRO
            if (ro.Status is ROStatus.CheckEnd or ROStatus.Repaired or ROStatus.HasRO)
            {
                ro.Status = ROStatus.Paid;
            }
        }

        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        return payment.Id;
    }

    public async Task<(bool ok, string msg)> TransitionPaymentStatusAsync(int id, PaymentStatus to, string? cashier = null, string? note = null)
    {
        var payment = await db.Payments
            .Include(p => p.RO).ThenInclude(r => r.Payments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null) return (false, "Không tìm thấy phiếu thu.");

        if (!AllowedNextPayment(payment.Status).Contains(to))
            return (false, $"Không thể chuyển từ '{Ui.PaymentStatus(payment.Status).text}' sang '{Ui.PaymentStatus(to).text}'.");

        if (to == PaymentStatus.Completed)
        {
            payment.Status = PaymentStatus.Completed;
            payment.CompletedAt = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(cashier)) payment.Cashier = cashier.Trim();
            if (!string.IsNullOrWhiteSpace(note)) payment.Note = string.IsNullOrWhiteSpace(payment.Note) ? note.Trim() : $"{payment.Note} | {note.Trim()}";

            if (payment.RO.Status is ROStatus.CheckEnd or ROStatus.Repaired or ROStatus.HasRO)
            {
                payment.RO.Status = ROStatus.Paid;
            }

            await db.SaveChangesAsync();
            return (true, $"Đã hoàn tất thu tiền phiếu {payment.PaymentNo} ({payment.PaymentAmount:N0}đ)! Trạng thái RO đã chuyển sang 'Đã thanh toán'.");
        }
        else if (to == PaymentStatus.Cancelled)
        {
            payment.Status = PaymentStatus.Cancelled;
            if (!string.IsNullOrWhiteSpace(note)) payment.Note = string.IsNullOrWhiteSpace(payment.Note) ? $"[Hủy: {note.Trim()}]" : $"{payment.Note} [Hủy: {note.Trim()}]";

            // Nếu RO đang là Paid và không còn phiếu thu Completed nào khác, có thể đưa RO về CheckEnd
            var otherCompleted = payment.RO.Payments.Any(p => p.Id != id && p.Status == PaymentStatus.Completed);
            if (!otherCompleted && payment.RO.Status == ROStatus.Paid)
            {
                payment.RO.Status = ROStatus.CheckEnd;
            }

            await db.SaveChangesAsync();
            return (true, $"Đã hủy phiếu thu {payment.PaymentNo}.");
        }

        return (false, "Trạng thái không hợp lệ.");
    }

    public async Task<(bool ok, string msg)> DeletePaymentAsync(int id)
    {
        var payment = await db.Payments.FirstOrDefaultAsync(p => p.Id == id);
        if (payment == null) return (false, "Không tìm thấy phiếu thu.");

        if (payment.Status == PaymentStatus.Completed)
            return (false, "Không thể xóa phiếu thu đã hoàn tất (Completed). Vui lòng thực hiện Hủy phiếu nếu cần thu hồi.");

        db.Payments.Remove(payment);
        await db.SaveChangesAsync();
        return (true, "Đã xóa phiếu thu.");
    }

    public Task<List<RepairOrder>> ROsForPaymentAsync() =>
        db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Lines)
            .Include(r => r.Payments)
            .Where(r => r.Status != ROStatus.Created && r.Status != ROStatus.Rejected && r.Status != ROStatus.NotResponding)
            .OrderByDescending(r => r.Status == ROStatus.CheckEnd || r.Status == ROStatus.Repaired)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync();

    // --- Quotation Management (Ser_Inv_Quote & Ser_Inv_QuotePartItems) ---
    public Task<List<Customer>> CustomersForSelectAsync() =>
        db.Customers.Include(c => c.Cars).OrderBy(c => c.Name).ToListAsync();

    public async Task<List<Quote>> QuotesAsync(QuoteStatus? status, string? q, DateTime? fromDate, DateTime? toDate)
    {
        var query = db.Quotes
            .Include(x => x.Customer)
            .Include(x => x.Car)
            .Include(x => x.Items)
            .Include(x => x.StockOut)
            .Include(x => x.RO)
            .AsQueryable();

        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (fromDate.HasValue) query = query.Where(x => x.QuoteDate.Date >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(x => x.QuoteDate.Date <= toDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(x => x.QuoteNo.ToLower().Contains(kw)
                || x.CustomerName.ToLower().Contains(kw)
                || (x.CustomerPhone != null && x.CustomerPhone.Contains(kw))
                || (x.RecipientName != null && x.RecipientName.ToLower().Contains(kw))
                || (x.Car != null && x.Car.Plate.ToLower().Contains(kw))
                || (x.Note != null && x.Note.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(x => x.QuoteDate).ThenByDescending(x => x.CreatedAt).ToList();
    }

    public Task<Quote?> GetQuoteAsync(int id) =>
        db.Quotes
            .Include(q => q.Customer).ThenInclude(c => c!.Cars)
            .Include(q => q.Car)
            .Include(q => q.Items).ThenInclude(i => i.Part)
            .Include(q => q.StockOut)
            .Include(q => q.RO)
            .FirstOrDefaultAsync(q => q.Id == id);

    public async Task<int> CreateQuoteAsync(Quote quote, List<QuoteItem> items)
    {
        if (string.IsNullOrWhiteSpace(quote.CustomerName))
            throw new InvalidOperationException("Vui lòng nhập tên khách hàng.");

        if (items.Count == 0)
            throw new InvalidOperationException("Vui lòng thêm ít nhất một phụ tùng/hạng mục vào báo giá.");

        if (string.IsNullOrWhiteSpace(quote.QuoteNo))
        {
            var countToday = await db.Quotes.CountAsync();
            quote.QuoteNo = $"BG{DateTime.Today:yyMMdd}-{countToday + 1:D3}";
        }
        quote.Status = QuoteStatus.Draft;
        quote.CreatedAt = DateTime.Now;

        foreach (var item in items)
        {
            if (item.PartId.HasValue && item.PartId.Value > 0)
            {
                var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId.Value);
                if (part != null)
                {
                    item.PartCode = part.Code;
                    item.PartName = part.Name;
                    item.Unit = string.IsNullOrWhiteSpace(item.Unit) ? part.Unit : item.Unit;
                    if (item.UnitPrice <= 0) item.UnitPrice = part.SalePrice;
                }
            }
            quote.Items.Add(item);
        }

        db.Quotes.Add(quote);
        await db.SaveChangesAsync();
        return quote.Id;
    }

    public async Task<(bool ok, string msg)> TransitionQuoteStatusAsync(int id, QuoteStatus to)
    {
        var quote = await db.Quotes.FirstOrDefaultAsync(q => q.Id == id);
        if (quote == null) return (false, "Không tìm thấy báo giá.");

        if (!AllowedNextQuote(quote.Status).Contains(to))
            return (false, $"Không thể chuyển từ '{Ui.QuoteStatus(quote.Status).text}' sang '{Ui.QuoteStatus(to).text}'.");

        quote.Status = to;
        if (to == QuoteStatus.Confirmed)
        {
            quote.ConfirmedAt = DateTime.Now;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật trạng thái báo giá {quote.QuoteNo} sang: {Ui.QuoteStatus(to).text}.");
    }

    public async Task<(bool ok, string msg, int? stockOutId)> ConvertQuoteToStockOutAsync(int id, string? approvedBy = null)
    {
        var quote = await db.Quotes.Include(q => q.Items).ThenInclude(i => i.Part).FirstOrDefaultAsync(q => q.Id == id);
        if (quote == null) return (false, "Không tìm thấy báo giá.", null);

        if (quote.Status == QuoteStatus.Converted)
            return (false, "Báo giá này đã được chuyển đổi trước đó.", quote.StockOutId);

        if (quote.Items.Count == 0)
            return (false, "Báo giá không có mặt hàng phụ tùng nào.", null);

        // Tạo phiếu xuất kho loại Normal (Bán lẻ theo báo giá)
        var stockOut = new StockOut
        {
            Type = StockOutType.Normal,
            Status = StockOutStatus.Pending,
            StockOutDate = DateTime.Today,
            StockOutNo = $"XK{DateTime.Today:yyMMdd}-{await db.StockOuts.CountAsync() + 1:D3}",
            CustomerId = quote.CustomerId,
            CarId = quote.CarId,
            QuoteId = quote.Id,
            RecipientName = !string.IsNullOrWhiteSpace(quote.RecipientName) ? quote.RecipientName : quote.CustomerName,
            Description = $"Xuất kho bán lẻ phụ tùng theo Báo giá số {quote.QuoteNo}.",
            CreatedBy = quote.CreatedBy,
            CreatedAt = DateTime.Now
        };

        foreach (var item in quote.Items)
        {
            var partId = item.PartId ?? 0;
            if (partId == 0)
            {
                var p = await db.Parts.FirstOrDefaultAsync(p => p.Code == item.PartCode);
                if (p != null) partId = p.Id;
            }

            if (partId > 0)
            {
                var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == partId);
                stockOut.Items.Add(new StockOutDetail
                {
                    PartId = partId,
                    PartCode = item.PartCode,
                    PartName = item.PartName,
                    Unit = item.Unit,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    VatPercent = item.VatPercent,
                    Location = part?.Location,
                    Note = $"Xuất từ Báo giá {quote.QuoteNo}"
                });
            }
        }

        if (stockOut.Items.Count == 0)
            return (false, "Các phụ tùng trong báo giá chưa được định danh trong danh mục kho.", null);

        db.StockOuts.Add(stockOut);
        await db.SaveChangesAsync();

        quote.Status = QuoteStatus.Converted;
        quote.StockOutId = stockOut.Id;
        await db.SaveChangesAsync();

        return (true, $"Đã chuyển Báo giá {quote.QuoteNo} thành Phiếu xuất kho {stockOut.StockOutNo}.", stockOut.Id);
    }

    public async Task<(bool ok, string msg, int? roId)> ConvertQuoteToROAsync(int id, string? technician = null)
    {
        var quote = await db.Quotes.Include(q => q.Items).Include(q => q.Car).FirstOrDefaultAsync(q => q.Id == id);
        if (quote == null) return (false, "Không tìm thấy báo giá.", null);

        if (quote.Status == QuoteStatus.Converted)
            return (false, "Báo giá này đã được chuyển đổi trước đó.", quote.ROId);

        if (!quote.CarId.HasValue || quote.CarId.Value <= 0)
        {
            // Kiểm tra xem khách hàng có xe nào không
            if (quote.CustomerId.HasValue)
            {
                var firstCar = await db.Cars.FirstOrDefaultAsync(c => c.CustomerId == quote.CustomerId.Value);
                if (firstCar != null) quote.CarId = firstCar.Id;
            }
        }

        if (!quote.CarId.HasValue || quote.CarId.Value <= 0)
            return (false, "Báo giá chưa liên kết thông tin xe để lập Lệnh sửa chữa (RO). Vui lòng cập nhật thông tin xe.", null);

        var car = await db.Cars.FirstOrDefaultAsync(c => c.Id == quote.CarId.Value);
        if (car == null) return (false, "Xe không tồn tại trong hệ thống.", null);

        var ro = new RepairOrder
        {
            CarId = car.Id,
            CustomerId = car.CustomerId,
            Code = $"RO{DateTime.Now:yyMMdd}-{await db.ROs.CountAsync() + 1:D3}",
            Status = ROStatus.Created,
            IntakeNote = $"Lập Lệnh sửa chữa từ Báo giá {quote.QuoteNo}. {(string.IsNullOrWhiteSpace(quote.Note) ? "" : "Ghi chú: " + quote.Note)}",
            Technician = string.IsNullOrWhiteSpace(technician) ? "Thợ tiếp nhận" : technician.Trim(),
            CreatedBy = quote.CreatedBy,
            CreatedAt = DateTime.Now
        };

        foreach (var item in quote.Items)
        {
            ro.Lines.Add(new RepairLine
            {
                Type = LineType.Part,
                ExpenseType = ExpenseType.Customer,
                PartId = item.PartId,
                Name = item.PartName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
        }

        db.ROs.Add(ro);
        await db.SaveChangesAsync();

        quote.Status = QuoteStatus.Converted;
        quote.ROId = ro.Id;
        await db.SaveChangesAsync();

        return (true, $"Đã chuyển Báo giá {quote.QuoteNo} thành Lệnh sửa chữa {ro.Code}.", ro.Id);
    }

    public async Task<(bool ok, string msg)> DeleteQuoteAsync(int id)
    {
        var quote = await db.Quotes.Include(q => q.Items).FirstOrDefaultAsync(q => q.Id == id);
        if (quote == null) return (false, "Không tìm thấy báo giá.");

        if (quote.Status == QuoteStatus.Converted)
            return (false, "Không thể xóa báo giá đã chuyển đổi thành Phiếu xuất kho hoặc Lệnh sửa chữa.");

        db.QuoteItems.RemoveRange(quote.Items);
        db.Quotes.Remove(quote);
        await db.SaveChangesAsync();
        return (true, "Đã xóa báo giá thành công.");
    }

    // --- Service Package Management (Ser_ServicePackage) ---
    public async Task<List<ServicePackage>> ServicePackagesAsync(string? q, bool? isPublic, bool? isActive)
    {
        var query = db.ServicePackages
            .Include(p => p.Items).ThenInclude(i => i.Part)
            .AsQueryable();

        if (isPublic.HasValue) query = query.Where(p => p.IsPublic == isPublic.Value);
        if (isActive.HasValue) query = query.Where(p => p.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(p => p.PackageNo.ToLower().Contains(kw)
                || p.Name.ToLower().Contains(kw)
                || (p.Description != null && p.Description.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderBy(p => p.PackageNo).ToList();
    }

    public Task<ServicePackage?> GetServicePackageAsync(int id) =>
        db.ServicePackages
            .Include(p => p.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<int> CreateServicePackageAsync(ServicePackage package, List<ServicePackageItem> items)
    {
        if (string.IsNullOrWhiteSpace(package.PackageNo))
        {
            var count = await db.ServicePackages.CountAsync();
            package.PackageNo = $"PKG-BD-{count + 1:D2}";
        }
        else
        {
            package.PackageNo = package.PackageNo.Trim().ToUpperInvariant();
        }

        var exists = await db.ServicePackages.AnyAsync(p => p.PackageNo == package.PackageNo);
        if (exists)
            throw new InvalidOperationException($"Mã gói dịch vụ '{package.PackageNo}' đã tồn tại trong hệ thống.");

        package.CreatedAt = DateTime.Now;

        foreach (var item in items)
        {
            if (item.Type == LineType.Part && item.PartId.HasValue && item.PartId.Value > 0)
            {
                var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId.Value);
                if (part != null)
                {
                    item.Code = string.IsNullOrWhiteSpace(item.Code) ? part.Code : item.Code;
                    item.Name = string.IsNullOrWhiteSpace(item.Name) ? part.Name : item.Name;
                    item.Unit = string.IsNullOrWhiteSpace(item.Unit) ? part.Unit : item.Unit;
                    if (item.UnitPrice <= 0) item.UnitPrice = part.SalePrice > 0 ? part.SalePrice : part.CostPrice;
                }
            }
            package.Items.Add(item);
        }

        db.ServicePackages.Add(package);
        await db.SaveChangesAsync();
        return package.Id;
    }

    public async Task<(bool ok, string msg)> DeleteServicePackageAsync(int id)
    {
        var package = await db.ServicePackages.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id);
        if (package == null) return (false, "Không tìm thấy gói dịch vụ.");

        db.ServicePackageItems.RemoveRange(package.Items);
        db.ServicePackages.Remove(package);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa gói dịch vụ '{package.PackageNo} - {package.Name}'.");
    }

    public Task<List<ServicePackage>> ServicePackagesForSelectAsync() =>
        db.ServicePackages
            .Include(p => p.Items)
            .Where(p => p.IsActive)
            .OrderBy(p => p.PackageNo)
            .ToListAsync();

    public async Task<(bool ok, string msg, int itemsAdded)> ApplyServicePackageToROAsync(int packageId, int roId)
    {
        var package = await db.ServicePackages
            .Include(p => p.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(p => p.Id == packageId);
        if (package == null) return (false, "Không tìm thấy gói dịch vụ.", 0);

        var ro = await db.ROs.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy Lệnh sửa chữa (RO).", 0);

        if (ro.Status is ROStatus.Paid or ROStatus.Finished or ROStatus.Rejected or ROStatus.NotResponding)
            return (false, $"Không thể thêm hạng mục vào RO đang ở trạng thái '{Ui.Status(ro.Status).text}'.", 0);

        if (package.Items.Count == 0)
            return (false, "Gói dịch vụ này chưa có hạng mục nào để áp dụng.", 0);

        int count = 0;
        foreach (var item in package.Items)
        {
            var lineName = !string.IsNullOrWhiteSpace(item.Name) ? item.Name : (item.Part != null ? item.Part.Name : "Hạng mục bảo dưỡng");
            ro.Lines.Add(new RepairLine
            {
                ROId = roId,
                Type = item.Type,
                ExpenseType = item.ExpenseType,
                PartId = item.PartId,
                Name = $"[{package.PackageNo}] {lineName}",
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
            count++;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã áp dụng gói '{package.PackageNo} - {package.Name}' ({count} hạng mục) vào Lệnh sửa chữa {ro.Code}.", count);
    }

    public async Task<(bool ok, string msg, int itemsAdded)> ApplyServicePackageToQuoteAsync(int packageId, int quoteId)
    {
        var package = await db.ServicePackages
            .Include(p => p.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(p => p.Id == packageId);
        if (package == null) return (false, "Không tìm thấy gói dịch vụ.", 0);

        var quote = await db.Quotes.Include(q => q.Items).FirstOrDefaultAsync(q => q.Id == quoteId);
        if (quote == null) return (false, "Không tìm thấy Báo giá.", 0);

        if (quote.Status is QuoteStatus.Converted or QuoteStatus.Rejected)
            return (false, $"Không thể thêm hạng mục vào Báo giá đang ở trạng thái '{Ui.QuoteStatus(quote.Status).text}'.", 0);

        if (package.Items.Count == 0)
            return (false, "Gói dịch vụ này chưa có hạng mục nào để nạp.", 0);

        int count = 0;
        foreach (var item in package.Items)
        {
            var partCode = !string.IsNullOrWhiteSpace(item.Code) ? item.Code : (item.Part != null ? item.Part.Code : (item.Type == LineType.Labor ? "CONG-BD" : "PT-BD"));
            var partName = !string.IsNullOrWhiteSpace(item.Name) ? item.Name : (item.Part != null ? item.Part.Name : "Hạng mục bảo dưỡng");
            var unit = !string.IsNullOrWhiteSpace(item.Unit) ? item.Unit : (item.Part != null ? item.Part.Unit : (item.Type == LineType.Labor ? "Lần" : "Cái"));

            quote.Items.Add(new QuoteItem
            {
                QuoteId = quoteId,
                PartId = item.PartId,
                PartCode = partCode,
                PartName = $"[{package.PackageNo}] {partName}",
                Unit = unit,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountPercent = 0,
                VatPercent = item.VatPercent,
                Note = string.IsNullOrWhiteSpace(item.Note) ? $"Combo {package.PackageNo}" : $"[{package.PackageNo}] {item.Note}"
            });
            count++;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã nạp gói '{package.PackageNo} - {package.Name}' ({count} hạng mục) vào Báo giá {quote.QuoteNo}.", count);
    }

    // --- Order Part Management (Ser_Order_Part & Ser_Order_PartDtl) ---
    public async Task<List<OrderPart>> OrderPartsAsync(OrderPartStatus? status, string? q, DateTime? fromDate, DateTime? toDate)
    {
        var query = db.OrderParts
            .Include(o => o.Lines).ThenInclude(l => l.Part)
            .Include(o => o.RO).ThenInclude(r => r!.Car)
            .Include(o => o.StockIn)
            .AsQueryable();

        if (status.HasValue) query = query.Where(o => o.Status == status.Value);
        if (fromDate.HasValue) query = query.Where(o => o.OrderDate.Date >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(o => o.OrderDate.Date <= toDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(o => o.OrderPartNo.ToLower().Contains(kw)
                || o.SupplierName.ToLower().Contains(kw)
                || (o.OrderSuppierNo != null && o.OrderSuppierNo.ToLower().Contains(kw))
                || (o.VIN != null && o.VIN.ToLower().Contains(kw))
                || (o.RO != null && o.RO.Code.ToLower().Contains(kw))
                || (o.Remark != null && o.Remark.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.CreatedAt).ToList();
    }

    public Task<OrderPart?> GetOrderPartAsync(int id) =>
        db.OrderParts
            .Include(o => o.Lines).ThenInclude(l => l.Part)
            .Include(o => o.RO).ThenInclude(r => r!.Car)
            .Include(o => o.RO).ThenInclude(r => r!.Customer)
            .Include(o => o.StockIn)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<int> CreateOrderPartAsync(OrderPart order, List<OrderPartLine> lines)
    {
        if (string.IsNullOrWhiteSpace(order.OrderPartNo))
        {
            order.OrderPartNo = $"PO{DateTime.Now:yyMMdd}-{await db.OrderParts.CountAsync() + 1:D3}";
        }

        if (order.DeliveryForm == OrderPartDeliveryForm.Warranty && string.IsNullOrWhiteSpace(order.VIN))
        {
            throw new InvalidOperationException("Đơn đặt hàng theo chế độ Bảo hành bắt buộc phải có số khung (VIN) xe.");
        }

        order.CreatedAt = DateTime.Now;

        foreach (var line in lines)
        {
            line.OrderPartNo = order.OrderPartNo;
            var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == line.PartId)
                ?? throw new InvalidOperationException($"Phụ tùng ID={line.PartId} không tồn tại.");

            line.PartCode = part.Code;
            line.PartName = part.Name;
            line.Unit = string.IsNullOrWhiteSpace(line.Unit) ? part.Unit : line.Unit;
            if (line.UnitPrice <= 0) line.UnitPrice = part.CostPrice > 0 ? part.CostPrice : part.SalePrice;
            if (line.ApprovedQuantity <= 0) line.ApprovedQuantity = line.Quantity;
            line.StatusDtl = OrderPartStatus.Pending;

            order.Lines.Add(line);
        }

        db.OrderParts.Add(order);
        await db.SaveChangesAsync();
        return order.Id;
    }

    public async Task<(bool ok, string msg)> TransitionOrderPartStatusAsync(int id, OrderPartStatus to, string? supplierOrderNo = null, string? note = null)
    {
        var order = await db.OrderParts.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return (false, "Không tìm thấy đơn đặt hàng.");

        if (!AllowedNextOrderPart(order.Status).Contains(to))
            return (false, $"Không thể chuyển từ '{Ui.OrderPartStatus(order.Status).text}' sang '{Ui.OrderPartStatus(to).text}'.");

        order.Status = to;
        if (to == OrderPartStatus.Approved)
        {
            order.ApprovedAt = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(supplierOrderNo))
                order.OrderSuppierNo = supplierOrderNo.Trim();
            if (!string.IsNullOrWhiteSpace(note))
                order.Remark = (order.Remark != null ? order.Remark + "\n" : "") + $"Duyệt: {note.Trim()}";
            foreach (var l in order.Lines)
            {
                l.StatusDtl = OrderPartStatus.Approved;
            }
        }
        else if (to == OrderPartStatus.Rejected)
        {
            if (!string.IsNullOrWhiteSpace(note))
                order.Remark = (order.Remark != null ? order.Remark + "\n" : "") + $"Hủy: {note.Trim()}";
            foreach (var l in order.Lines)
            {
                l.StatusDtl = OrderPartStatus.Rejected;
            }
        }

        await db.SaveChangesAsync();
        return (true, $"Đã chuyển trạng thái đơn hàng sang: {Ui.OrderPartStatus(to).text}.");
    }

    public async Task<(bool ok, string msg, int? stockInId)> CreateStockInFromOrderPartAsync(int id, string? approvedBy = null)
    {
        var order = await db.OrderParts
            .Include(o => o.Lines).ThenInclude(l => l.Part)
            .Include(o => o.RO)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return (false, "Không tìm thấy đơn đặt hàng phụ tùng.", null);
        if (order.Status != OrderPartStatus.Approved)
            return (false, "Chỉ có thể tạo phiếu nhập kho cho đơn đặt hàng đã được duyệt (Approved).", null);

        if (order.StockInId.HasValue)
            return (false, "Đơn đặt hàng này đã được lập phiếu nhập kho trước đó.", order.StockInId);

        var stockInNo = $"NK{DateTime.Now:yyMMdd}-{await db.StockIns.CountAsync() + 1:D3}";
        var stockIn = new StockIn
        {
            StockInNo = stockInNo,
            StockInDate = DateTime.Today,
            SupplierName = order.SupplierName,
            BillNo = !string.IsNullOrWhiteSpace(order.OrderSuppierNo) ? order.OrderSuppierNo : order.OrderPartNo,
            Type = StockInType.Normal,
            Status = StockInStatus.Finished,
            Description = $"Nhập kho theo đơn đặt hàng NCC {order.OrderPartNo}" + (!string.IsNullOrWhiteSpace(order.Remark) ? $": {order.Remark}" : ""),
            OrderPartId = order.Id,
            OrderPartNo = order.OrderPartNo,
            CreatedBy = "system",
            ApprovedBy = string.IsNullOrWhiteSpace(approvedBy) ? "Thủ kho" : approvedBy.Trim(),
            FinishedAt = DateTime.Now
        };

        var details = new List<StockInDetail>();
        foreach (var line in order.Lines)
        {
            var qty = line.ApprovedQuantity > 0 ? line.ApprovedQuantity : line.Quantity;
            details.Add(new StockInDetail
            {
                PartId = line.PartId,
                PartCode = line.PartCode,
                PartName = line.PartName,
                Unit = line.Unit,
                Quantity = qty,
                UnitPrice = line.UnitPrice,
                VatPercent = line.VatPercent,
                Note = $"Từ PO {order.OrderPartNo}" + (!string.IsNullOrWhiteSpace(line.Note) ? $": {line.Note}" : "")
            });

            // Tăng tồn kho và cập nhật giá vốn
            if (line.Part != null)
            {
                line.Part.InStock += qty;
                if (line.UnitPrice > 0)
                {
                    line.Part.CostPrice = line.UnitPrice;
                }
            }

            line.ReceivedQuantity = qty;
            line.StatusDtl = OrderPartStatus.Finished;
        }

        stockIn.Items = details;
        db.StockIns.Add(stockIn);
        await db.SaveChangesAsync();

        order.StockInId = stockIn.Id;
        order.Status = OrderPartStatus.Finished;
        order.FinishedAt = DateTime.Now;

        // Tự động ghi nhận công nợ NCC theo Ser_SupplierDebit idn.CarService
        var sup = await db.Suppliers.FirstOrDefaultAsync(s => s.Name == order.SupplierName || s.Code == order.SupplierName);
        if (sup != null && stockIn.Total > 0)
        {
            var count = await db.SupplierDebits.CountAsync() + 1;
            db.SupplierDebits.Add(new SupplierDebit
            {
                DebitNo = $"SDB{DateTime.Today:yyMMdd}-{count:D3}",
                SupplierId = sup.Id,
                StockInId = stockIn.Id,
                OrderPartId = order.Id,
                DebitType = SupplierDebitType.StockIn,
                Status = SupplierDebitStatus.Active,
                DebitDate = stockIn.StockInDate,
                DueDate = stockIn.StockInDate.AddDays(30),
                DebitAmount = stockIn.Total,
                PaidAmount = 0,
                Description = $"Công nợ nhập kho từ đơn đặt hàng NCC {order.OrderPartNo}",
                CreatedBy = stockIn.ApprovedBy ?? "Thủ kho",
                CreatedAt = DateTime.Now
            });
        }

        // Nếu đơn hàng gắn với RO đang ở trạng thái Wait4Part (Đợi phụ tùng) -> Chuyển sang HasPart (Đã có phụ tùng)!
        if (order.ROId.HasValue)
        {
            var ro = await db.ROs.FirstOrDefaultAsync(r => r.Id == order.ROId.Value);
            if (ro != null && ro.Status == ROStatus.Wait4Part)
            {
                ro.Status = ROStatus.HasPart;
            }
        }

        await db.SaveChangesAsync();
        return (true, $"Đã tạo phiếu nhập kho {stockIn.StockInNo} và cập nhật tồn kho phụ tùng thành công.", stockIn.Id);
    }

    public async Task<(bool ok, string msg)> DeleteOrderPartAsync(int id)
    {
        var order = await db.OrderParts.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return (false, "Không tìm thấy đơn đặt hàng.");

        if (order.Status is not (OrderPartStatus.Pending or OrderPartStatus.Rejected))
            return (false, "Chỉ có thể xóa đơn hàng ở trạng thái Chờ duyệt hoặc Đã hủy.");

        if (order.StockInId.HasValue)
            return (false, "Không thể xóa đơn hàng đã có phiếu nhập kho liên kết.");

        db.OrderPartLines.RemoveRange(order.Lines);
        db.OrderParts.Remove(order);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa đơn đặt hàng {order.OrderPartNo}.");
    }

    public Task<List<RepairOrder>> ROsWaitingForPartAsync() =>
        db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Lines)
            .Where(r => r.Status == ROStatus.Wait4Part || r.Status == ROStatus.Printed || r.Status == ROStatus.HasRO)
            .OrderByDescending(r => r.Status == ROStatus.Wait4Part)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync();

    // --- Cavity & Workshop Bay Dispatch Management (Ser_Cavity / Mst_Compartment) ---
    public async Task<List<Cavity>> CavitiesAsync(CavityType? type, CavityStatus? status, string? q)
    {
        var query = db.Cavities
            .Include(c => c.CurrentRO).ThenInclude(r => r!.Car)
            .Include(c => c.CurrentRO).ThenInclude(r => r!.Customer)
            .AsQueryable();

        if (type.HasValue) query = query.Where(c => c.CavityType == type.Value);
        if (status.HasValue) query = query.Where(c => c.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(c => c.CavityNo.ToLower().Contains(kw)
                || c.CavityName.ToLower().Contains(kw)
                || (c.LiftEquipment != null && c.LiftEquipment.ToLower().Contains(kw))
                || (c.AreaZone != null && c.AreaZone.ToLower().Contains(kw))
                || (c.CurrentCarPlate != null && c.CurrentCarPlate.ToLower().Contains(kw))
                || (c.CurrentCarModel != null && c.CurrentCarModel.ToLower().Contains(kw))
                || (c.CurrentTechnician != null && c.CurrentTechnician.ToLower().Contains(kw))
                || (c.CurrentRO != null && c.CurrentRO.Code.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderBy(c => c.CavityType).ThenBy(c => c.CavityNo).ToList();
    }

    public Task<Cavity?> GetCavityAsync(int id) =>
        db.Cavities
            .Include(c => c.CurrentRO).ThenInclude(r => r!.Car)
            .Include(c => c.CurrentRO).ThenInclude(r => r!.Customer)
            .Include(c => c.CurrentRO).ThenInclude(r => r!.Lines)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<int> CreateCavityAsync(Cavity cavity)
    {
        if (string.IsNullOrWhiteSpace(cavity.CavityNo))
        {
            var prefix = cavity.CavityType switch
            {
                CavityType.EM => "KH-EM",
                CavityType.GR => "KH-GR",
                CavityType.BP => "KH-BP",
                CavityType.KCS => "KH-KCS",
                CavityType.Wash => "KH-WASH",
                _ => "KH"
            };
            var count = await db.Cavities.CountAsync(c => c.CavityType == cavity.CavityType);
            cavity.CavityNo = $"{prefix}-{count + 1:D2}";
        }
        else
        {
            cavity.CavityNo = cavity.CavityNo.Trim().ToUpperInvariant();
        }

        var exists = await db.Cavities.AnyAsync(c => c.CavityNo == cavity.CavityNo);
        if (exists)
            throw new InvalidOperationException($"Mã khoang '{cavity.CavityNo}' đã tồn tại trong xưởng.");

        cavity.CavityName = cavity.CavityName.Trim();
        cavity.CreatedAt = DateTime.Now;

        db.Cavities.Add(cavity);
        await db.SaveChangesAsync();
        return cavity.Id;
    }

    public async Task<(bool ok, string msg)> UpdateCavityAsync(int id, string cavityName, CavityType type, string? liftEquipment, string? areaZone, string? note)
    {
        var cavity = await db.Cavities.FirstOrDefaultAsync(c => c.Id == id);
        if (cavity == null) return (false, "Không tìm thấy khoang sửa chữa.");

        if (string.IsNullOrWhiteSpace(cavityName)) return (false, "Vui lòng nhập tên khoang sửa chữa.");

        cavity.CavityName = cavityName.Trim();
        cavity.CavityType = type;
        cavity.LiftEquipment = liftEquipment?.Trim();
        cavity.AreaZone = areaZone?.Trim();
        cavity.Note = note?.Trim();

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật khoang '{cavity.CavityNo} - {cavity.CavityName}'.");
    }

    public async Task<(bool ok, string msg)> AssignCarToCavityAsync(int cavityId, int roId, string? technician, DateTime? expectedFinish = null)
    {
        var cavity = await db.Cavities.FirstOrDefaultAsync(c => c.Id == cavityId);
        if (cavity == null) return (false, "Không tìm thấy khoang sửa chữa.");

        if (!cavity.IsActive)
            return (false, $"Khoang '{cavity.CavityNo}' đang ngưng hoạt động.");

        if (cavity.Status == CavityStatus.Occupied && cavity.CurrentROId.HasValue && cavity.CurrentROId.Value != roId)
            return (false, $"Khoang '{cavity.CavityNo}' đang có xe {cavity.CurrentCarPlate} sửa chữa. Vui lòng giải phóng khoang trước.");

        if (cavity.Status == CavityStatus.Maintenance)
            return (false, $"Khoang '{cavity.CavityNo}' đang trong thời gian bảo trì thiết bị/cầu nâng.");

        var ro = await db.ROs.Include(r => r.Car).Include(r => r.Customer).FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy Lệnh sửa chữa (RO).");

        if (ro.Status is ROStatus.Paid or ROStatus.Finished or ROStatus.Rejected or ROStatus.NotResponding)
            return (false, $"Lệnh RO {ro.Code} đang ở trạng thái '{Ui.Status(ro.Status).text}', không thể đưa vào khoang sửa chữa.");

        // Cập nhật thông tin trên khoang
        cavity.CurrentROId = ro.Id;
        cavity.CurrentCarPlate = ro.Car?.Plate ?? "";
        cavity.CurrentCarModel = ro.Car?.Model ?? "";
        var assignedTech = !string.IsNullOrWhiteSpace(technician) ? technician.Trim() : ro.Technician;
        cavity.CurrentTechnician = assignedTech;
        cavity.StartUseDate = DateTime.Now;
        cavity.ExpectedFinishDate = expectedFinish ?? DateTime.Now.AddHours(2);
        cavity.Status = CavityStatus.Occupied;

        // Cập nhật Lệnh RO sang InGarage nếu chưa vào
        ro.CavityId = cavity.Id;
        if (ro.Status is ROStatus.Created or ROStatus.Printed or ROStatus.HasRO)
        {
            ro.Status = ROStatus.InGarage;
            ro.IntakeAt ??= DateTime.Now;
        }
        if (!string.IsNullOrWhiteSpace(assignedTech))
        {
            ro.Technician = assignedTech;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã điều phối xe {ro.Car?.Plate} (RO: {ro.Code}) vào {cavity.CavityName}.");
    }

    public async Task<(bool ok, string msg)> ReleaseCavityAsync(int cavityId, ROStatus? nextRoStatus = null)
    {
        var cavity = await db.Cavities.FirstOrDefaultAsync(c => c.Id == cavityId);
        if (cavity == null) return (false, "Không tìm thấy khoang sửa chữa.");

        if (cavity.Status != CavityStatus.Occupied && !cavity.CurrentROId.HasValue)
            return (false, $"Khoang '{cavity.CavityNo}' hiện đang trống, không cần giải phóng.");

        string prevPlate = cavity.CurrentCarPlate ?? "xe";
        string? prevRoCode = null;

        if (cavity.CurrentROId.HasValue)
        {
            var ro = await db.ROs.FirstOrDefaultAsync(r => r.Id == cavity.CurrentROId.Value);
            if (ro != null)
            {
                prevRoCode = ro.Code;
                // Nếu người dùng chọn trạng thái tiếp theo cho RO (ví dụ Repaired hoặc CheckEnd)
                if (nextRoStatus.HasValue)
                {
                    ro.Status = nextRoStatus.Value;
                }
                else if (ro.Status == ROStatus.InGarage)
                {
                    ro.Status = ROStatus.Repaired;
                }
            }
        }

        cavity.CurrentROId = null;
        cavity.CurrentCarPlate = null;
        cavity.CurrentCarModel = null;
        cavity.CurrentTechnician = null;
        cavity.FinishUseDate = DateTime.Now;
        cavity.ExpectedFinishDate = null;
        cavity.Status = CavityStatus.Available;

        await db.SaveChangesAsync();
        return (true, $"Đã giải phóng {cavity.CavityName} (Xe {prevPlate}{(prevRoCode != null ? $" - RO: {prevRoCode}" : "")} đã rời khoang).");
    }

    public async Task<(bool ok, string msg)> SetCavityStatusAsync(int cavityId, CavityStatus status, string? note = null)
    {
        var cavity = await db.Cavities.FirstOrDefaultAsync(c => c.Id == cavityId);
        if (cavity == null) return (false, "Không tìm thấy khoang sửa chữa.");

        if (status != CavityStatus.Occupied && cavity.Status == CavityStatus.Occupied && cavity.CurrentROId.HasValue)
            return (false, $"Khoang '{cavity.CavityNo}' đang có xe làm việc. Vui lòng giải phóng xe trước khi đổi trạng thái.");

        cavity.Status = status;
        if (!string.IsNullOrWhiteSpace(note)) cavity.Note = note.Trim();

        await db.SaveChangesAsync();
        var statusDesc = Ui.CavityStatus(status).text;
        return (true, $"Đã cập nhật trạng thái khoang '{cavity.CavityNo}' thành '{statusDesc}'.");
    }

    public async Task<(bool ok, string msg)> DeleteCavityAsync(int cavityId)
    {
        var cavity = await db.Cavities.FirstOrDefaultAsync(c => c.Id == cavityId);
        if (cavity == null) return (false, "Không tìm thấy khoang sửa chữa.");

        if (cavity.Status == CavityStatus.Occupied || cavity.CurrentROId.HasValue)
            return (false, "Không thể xóa khoang đang có xe sửa chữa.");

        db.Cavities.Remove(cavity);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa khoang '{cavity.CavityNo} - {cavity.CavityName}'.");
    }

    public Task<List<Cavity>> CavitiesForSelectAsync(CavityType? type = null)
    {
        var query = db.Cavities.Where(c => c.IsActive).AsQueryable();
        if (type.HasValue) query = query.Where(c => c.CavityType == type.Value);
        return query.OrderBy(c => c.CavityType).ThenBy(c => c.CavityNo).ToListAsync();
    }

    public Task<List<RepairOrder>> ROsEligibleForCavityAsync() =>
        db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Lines)
            .Where(r => r.Status == ROStatus.HasRO || r.Status == ROStatus.InGarage || r.Status == ROStatus.Created || r.Status == ROStatus.Printed || r.Status == ROStatus.Wait4Part || r.Status == ROStatus.HasPart)
            .OrderByDescending(r => r.Status == ROStatus.InGarage)
            .ThenByDescending(r => r.Status == ROStatus.HasRO)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync();

    // --- Vehicle Reception & Walk-Around Inspection (Ser_ReceptionF & Ser_ReceptionFDtl) ---
    public List<ReceptionItem> GetDefaultChecklistItems() =>
    [
        // 1. Khoang lái (Cabin)
        new() { Group = "Khoang lái", Code = "KHOANGLAI.DTL", Name = "Bảng đồng hồ & Đèn cảnh báo taplo", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Khoang lái", Code = "KHOANGLAI.COI", Name = "Còi xe & Hệ thống tín hiệu âm thanh", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Khoang lái", Code = "KHOANGLAI.GMBP", Name = "Cần gạt mưa & Vòi xịt rửa kính", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Khoang lái", Code = "KHOANGLAI.HTAT", Name = "Dây đai an toàn & Hệ thống túi khí", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Khoang lái", Code = "KHOANGLAI.HTDH", Name = "Hệ thống điều hòa nhiệt độ & Quạt gió", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Khoang lái", Code = "KHOANGLAI.CCGD", Name = "Kính cửa sổ & Khóa cửa trung tâm", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Khoang lái", Code = "KHOANGLAI.GLD", Name = "Gương chiếu hậu trong & ngoài xe", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },

        // 2. Ngoại quan & Thân vỏ (Exterior)
        new() { Group = "Ngoại quan & Thân vỏ", Code = "TRUOCVASAUXE.DT", Name = "Cụm đèn chiếu sáng trước (Pha/Cos/Xi-nhan)", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Ngoại quan & Thân vỏ", Code = "TRUOCVASAUXE.DS", Name = "Cụm đèn sau (Đèn hậu/Phanh/Lùi/Biển số)", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Ngoại quan & Thân vỏ", Code = "THANVO.XUOC", Name = "Kiểm tra trầy xước / móp méo thân vỏ xe", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },

        // 3. Khoang động cơ (Engine Compartment)
        new() { Group = "Khoang động cơ", Code = "KHOANGDONGCO.DDC", Name = "Mức & Tình trạng dầu nhớt động cơ", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Khoang động cơ", Code = "KHOANGDONGCO.DP", Name = "Mức dầu phanh / Dầu ly hợp", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Khoang động cơ", Code = "KHOANGDONGCO.DTLL", Name = "Mức dầu trợ lực lái (nếu có)", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Khoang động cơ", Code = "KHOANGDONGCO.NLM", Name = "Mức nước làm mát động cơ & Bình phụ", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Khoang động cơ", Code = "KHOANGDONGCO.NRK", Name = "Mức nước rửa kính khoang máy", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Khoang động cơ", Code = "KHOANGDONGCO.DTD", Name = "Tình trạng dây curoa truyền động", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Khoang động cơ", Code = "KHOANGDONGCO.LG", Name = "Tình trạng lọc gió động cơ & lọc máy lạnh", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Khoang động cơ", Code = "KHOANGDONGCO.RRCL", Name = "Kiểm tra rò rỉ dung dịch đáy khoang động cơ", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },

        // 4. Lốp xe & Phanh (Tires & Brakes)
        new() { Group = "Lốp xe & Phanh", Code = "LOPXE.BXTT", Name = "Bánh xe trước trái (Áp suất & Độ mòn gai)", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Lốp xe & Phanh", Code = "LOPXE.BXTP", Name = "Bánh xe trước phải (Áp suất & Độ mòn gai)", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Lốp xe & Phanh", Code = "LOPXE.BXST", Name = "Bánh xe sau trái (Áp suất & Độ mòn gai)", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Lốp xe & Phanh", Code = "LOPXE.BXSP", Name = "Bánh xe sau phải (Áp suất & Độ mòn gai)", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },

        // 5. Cốp sau & Dụng cụ (Trunk & Tools)
        new() { Group = "Cốp sau & Dụng cụ", Code = "COPSAU.BDC", Name = "Bộ đồ nghề sửa chữa, tay quay & Kích nâng", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
        new() { Group = "Cốp sau & Dụng cụ", Code = "COPSAU.LDP", Name = "Lốp xe dự phòng & Áp suất lốp phụ", ReceptionStatus = AuditStatus.Good, DeliveryStatus = AuditStatus.Good },
    ];

    public async Task<List<ReceptionSheet>> ReceptionsAsync(ReceptionStatus? status, string? q, DateTime? fromDate, DateTime? toDate)
    {
        var query = db.ReceptionSheets
            .Include(s => s.Car)
            .Include(s => s.Customer)
            .Include(s => s.Appointment)
            .Include(s => s.RO)
            .Include(s => s.Items)
            .AsQueryable();

        if (status.HasValue) query = query.Where(s => s.Status == status.Value);
        if (fromDate.HasValue) query = query.Where(s => s.CreatedAt.Date >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(s => s.CreatedAt.Date <= toDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(s => s.ReceptionNo.ToLower().Contains(kw)
                || s.Car.Plate.ToLower().Contains(kw)
                || s.Car.Model.ToLower().Contains(kw)
                || s.Customer.Name.ToLower().Contains(kw)
                || (s.Customer.Phone != null && s.Customer.Phone.Contains(kw))
                || (s.CustomerRequest != null && s.CustomerRequest.ToLower().Contains(kw))
                || (s.CreatedBy != null && s.CreatedBy.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(s => s.CreatedAt).ToList();
    }

    public Task<ReceptionSheet?> GetReceptionAsync(int id) =>
        db.ReceptionSheets
            .Include(s => s.Car)
            .Include(s => s.Customer)
            .Include(s => s.Appointment)
            .Include(s => s.RO).ThenInclude(r => r!.Lines)
            .Include(s => s.RO).ThenInclude(r => r!.Cavity)
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<int> CreateReceptionAsync(ReceptionSheet sheet, List<ReceptionItem>? items = null)
    {
        var car = await db.Cars.Include(c => c.Customer).FirstOrDefaultAsync(c => c.Id == sheet.CarId)
            ?? throw new InvalidOperationException("Phương tiện không tồn tại.");
        sheet.CustomerId = car.CustomerId;

        if (string.IsNullOrWhiteSpace(sheet.ReceptionNo))
        {
            var countToday = await db.ReceptionSheets.CountAsync(s => s.CreatedAt.Date == DateTime.Today);
            sheet.ReceptionNo = $"TN{DateTime.Today:yyMMdd}-{countToday + 1:D3}";
        }
        else
        {
            sheet.ReceptionNo = sheet.ReceptionNo.Trim().ToUpperInvariant();
        }

        sheet.Status = ReceptionStatus.Pending;
        sheet.CreatedAt = DateTime.Now;

        // Nếu có cuộc hẹn gốc
        if (sheet.AppointmentId.HasValue)
        {
            var app = await db.Appointments.FirstOrDefaultAsync(a => a.Id == sheet.AppointmentId.Value);
            if (app != null)
            {
                if (app.Status == AppointmentStatus.Pending || app.Status == AppointmentStatus.Contacted || app.Status == AppointmentStatus.Confirmed)
                {
                    app.Status = AppointmentStatus.CheckedIn;
                    app.CheckedInAt = DateTime.Now;
                }
            }
        }

        var checklist = items != null && items.Count > 0 ? items : GetDefaultChecklistItems();
        foreach (var item in checklist)
        {
            sheet.Items.Add(new ReceptionItem
            {
                Group = item.Group,
                Code = item.Code,
                Name = item.Name,
                ReceptionStatus = item.ReceptionStatus,
                DeliveryStatus = item.DeliveryStatus,
                Note = item.Note
            });
        }

        db.ReceptionSheets.Add(sheet);
        await db.SaveChangesAsync();

        if (sheet.AppointmentId.HasValue)
        {
            var app = await db.Appointments.FirstOrDefaultAsync(a => a.Id == sheet.AppointmentId.Value);
            if (app != null)
            {
                app.ReceptionSheetId = sheet.Id;
                await db.SaveChangesAsync();
            }
        }

        return sheet.Id;
    }

    public async Task<(bool ok, string msg, int? roId)> CreateROFromReceptionAsync(int id, string? technician = null)
    {
        var sheet = await db.ReceptionSheets
            .Include(s => s.Car)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sheet == null) return (false, "Không tìm thấy phiếu tiếp nhận xe.", null);

        if (sheet.ROId.HasValue)
            return (false, $"Phiếu tiếp nhận {sheet.ReceptionNo} đã được lập Lệnh sửa chữa trước đó.", sheet.ROId);

        if (sheet.Status == ReceptionStatus.Cancelled)
            return (false, "Phiếu tiếp nhận đã bị hủy, không thể lập Lệnh sửa chữa.", null);

        if (sheet.Status == ReceptionStatus.Delivered)
            return (false, "Phiếu tiếp nhận đã hoàn tất bàn giao xe.", null);

        var ro = new RepairOrder
        {
            CarId = sheet.CarId,
            CustomerId = sheet.CustomerId,
            AppointmentId = sheet.AppointmentId,
            ReceptionSheetId = sheet.Id,
            Odometer = sheet.Odometer,
            IntakeNote = $"Tiếp nhận theo phiếu {sheet.ReceptionNo}. Cấp: {sheet.LevelOfInspection}. Yêu cầu: {sheet.CustomerRequest}"
                + (!string.IsNullOrWhiteSpace(sheet.ValuablesInCar) ? $" | Đồ đạc trên xe: {sheet.ValuablesInCar}" : "")
                + (!string.IsNullOrWhiteSpace(sheet.ExteriorCondition) ? $" | Thân vỏ: {sheet.ExteriorCondition}" : ""),
            Technician = !string.IsNullOrWhiteSpace(technician) ? technician.Trim() : (sheet.CreatedBy ?? "Kỹ thuật viên xưởng"),
            Status = ROStatus.Created,
            CreatedAt = DateTime.Now,
            CreatedBy = sheet.CreatedBy ?? "Cố vấn dịch vụ"
        };

        var countTotal = await db.ROs.CountAsync();
        ro.Code = $"RO{DateTime.Today:yyMMdd}-{countTotal + 1:D3}";

        db.ROs.Add(ro);
        await db.SaveChangesAsync();

        sheet.ROId = ro.Id;
        sheet.Status = ReceptionStatus.InService;

        // Nếu có lịch hẹn, cập nhật ROId cho lịch hẹn
        if (sheet.AppointmentId.HasValue)
        {
            var app = await db.Appointments.FirstOrDefaultAsync(a => a.Id == sheet.AppointmentId.Value);
            if (app != null)
            {
                app.ROId = ro.Id;
                app.Status = AppointmentStatus.CheckedIn;
                app.CheckedInAt ??= DateTime.Now;
            }
        }

        await db.SaveChangesAsync();
        return (true, $"Đã tạo Lệnh sửa chữa {ro.Code} thành công từ phiếu tiếp nhận {sheet.ReceptionNo}!", ro.Id);
    }

    public async Task<(bool ok, string msg)> DeliverCarAsync(int id, string? deliveryBy, string? note, List<(int itemId, AuditStatus status)>? deliveryItems = null)
    {
        var sheet = await db.ReceptionSheets
            .Include(s => s.RO)
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sheet == null) return (false, "Không tìm thấy phiếu tiếp nhận xe.");
        if (sheet.Status == ReceptionStatus.Delivered) return (false, "Xe đã được bàn giao trước đó.");
        if (sheet.Status == ReceptionStatus.Cancelled) return (false, "Phiếu tiếp nhận đã bị hủy.");

        sheet.Status = ReceptionStatus.Delivered;
        sheet.DeliveryDateTime = DateTime.Now;
        sheet.DeliveryBy = !string.IsNullOrWhiteSpace(deliveryBy) ? deliveryBy.Trim() : "Cố vấn dịch vụ";
        sheet.DeliveryNote = note?.Trim();

        if (deliveryItems != null && deliveryItems.Count > 0)
        {
            foreach (var (itemId, status) in deliveryItems)
            {
                var itm = sheet.Items.FirstOrDefault(i => i.Id == itemId);
                if (itm != null) itm.DeliveryStatus = status;
            }
        }

        // Nếu RO liên kết đang ở Paid, tự động chuyển sang Finished (FNS)
        if (sheet.RO != null && sheet.RO.Status == ROStatus.Paid)
        {
            sheet.RO.Status = ROStatus.Finished;
            sheet.RO.FinishedAt = DateTime.Now;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã hoàn tất nghiệm thu và bàn giao xe cho khách hàng theo phiếu {sheet.ReceptionNo}!");
    }

    public async Task<(bool ok, string msg)> CancelReceptionAsync(int id, string? reason = null)
    {
        var sheet = await db.ReceptionSheets.FirstOrDefaultAsync(s => s.Id == id);
        if (sheet == null) return (false, "Không tìm thấy phiếu tiếp nhận xe.");

        if (sheet.Status == ReceptionStatus.Delivered)
            return (false, "Không thể hủy phiếu tiếp nhận đã bàn giao xe.");

        if (sheet.ROId.HasValue)
        {
            var ro = await db.ROs.FirstOrDefaultAsync(r => r.Id == sheet.ROId.Value);
            if (ro != null && ro.Status != ROStatus.Created && ro.Status != ROStatus.Rejected)
                return (false, $"Lệnh sửa chữa {ro.Code} đang trong quá trình thực hiện ({Ui.Status(ro.Status).text}), không thể hủy phiếu tiếp nhận.");
        }

        sheet.Status = ReceptionStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
            sheet.CustomerRequest += $" [Đã hủy: {reason.Trim()}]";

        await db.SaveChangesAsync();
        return (true, $"Đã hủy phiếu tiếp nhận {sheet.ReceptionNo}.");
    }

    public async Task<(bool ok, string msg)> DeleteReceptionAsync(int id)
    {
        var sheet = await db.ReceptionSheets.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == id);
        if (sheet == null) return (false, "Không tìm thấy phiếu tiếp nhận xe.");

        if (sheet.Status == ReceptionStatus.InService || sheet.Status == ReceptionStatus.Delivered)
            return (false, "Không thể xóa phiếu tiếp nhận đang trong xưởng hoặc đã bàn giao xe.");

        if (sheet.ROId.HasValue)
            return (false, "Không thể xóa phiếu tiếp nhận đã sinh Lệnh sửa chữa.");

        db.ReceptionItems.RemoveRange(sheet.Items);
        db.ReceptionSheets.Remove(sheet);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa phiếu tiếp nhận {sheet.ReceptionNo}.");
    }

    public Task<List<Appointment>> AppointmentsEligibleForReceptionAsync() =>
        db.Appointments
            .Include(a => a.Car)
            .Include(a => a.Customer)
            .Where(a => a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.CheckedIn && !a.ROId.HasValue && !a.ReceptionSheetId.HasValue)
            .OrderByDescending(a => a.AppointmentDate.Date == DateTime.Today)
            .ThenBy(a => a.AppointmentDate)
            .ToListAsync();

    // --- Assignment of Work & Workshop Dispatch Management (Ser_AssignmentWork, Ser_AssignmentWorkEngineer, Ser_Engineer, Ser_GroupRepair) ---
    public async Task<List<GroupRepair>> GroupRepairsAsync(string? q, bool? isActive)
    {
        var query = db.GroupRepairs.Include(g => g.Engineers).AsQueryable();
        if (isActive.HasValue) query = query.Where(g => g.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(g => g.GroupRNo.ToLower().Contains(kw) || g.GroupRName.ToLower().Contains(kw) || g.LeaderName.ToLower().Contains(kw));
        }
        return await query.OrderBy(g => g.GroupRNo).ToListAsync();
    }

    public Task<GroupRepair?> GetGroupRepairAsync(int id) =>
        db.GroupRepairs.Include(g => g.Engineers).FirstOrDefaultAsync(g => g.Id == id);

    public async Task<int> CreateGroupRepairAsync(GroupRepair group)
    {
        if (string.IsNullOrWhiteSpace(group.GroupRNo))
            group.GroupRNo = $"TO-{await db.GroupRepairs.CountAsync() + 1:D2}";
        group.CreatedAt = DateTime.Now;
        db.GroupRepairs.Add(group);
        await db.SaveChangesAsync();
        return group.Id;
    }

    public Task<List<GroupRepair>> GroupRepairsForSelectAsync() =>
        db.GroupRepairs.Where(g => g.IsActive).OrderBy(g => g.GroupRName).ToListAsync();

    public async Task<(bool ok, string msg)> DeleteGroupRepairAsync(int id)
    {
        var g = await db.GroupRepairs.Include(x => x.Engineers).FirstOrDefaultAsync(x => x.Id == id);
        if (g == null) return (false, "Không tìm thấy tổ sửa chữa.");
        if (g.Engineers.Count > 0) return (false, "Không thể xóa tổ đang có kỹ thuật viên trực thuộc.");
        db.GroupRepairs.Remove(g);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa tổ thợ {g.GroupRName}.");
    }

    public async Task<List<Engineer>> EngineersAsync(string? q, int? groupId, bool? isActive)
    {
        var query = db.Engineers.Include(e => e.GroupRepair).AsQueryable();
        if (groupId.HasValue && groupId.Value > 0) query = query.Where(e => e.GroupRId == groupId.Value);
        if (isActive.HasValue) query = query.Where(e => e.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(e => e.EngineerNo.ToLower().Contains(kw)
                || e.EngineerName.ToLower().Contains(kw)
                || (e.Phone != null && e.Phone.Contains(kw))
                || e.SkillLevel.ToLower().Contains(kw)
                || e.Specialty.ToLower().Contains(kw));
        }
        return await query.OrderBy(e => e.EngineerNo).ToListAsync();
    }

    public Task<Engineer?> GetEngineerAsync(int id) =>
        db.Engineers.Include(e => e.GroupRepair).FirstOrDefaultAsync(e => e.Id == id);

    public async Task<int> CreateEngineerAsync(Engineer eng)
    {
        if (string.IsNullOrWhiteSpace(eng.EngineerNo))
            eng.EngineerNo = $"KTV-{await db.Engineers.CountAsync() + 1:D3}";
        eng.CreatedAt = DateTime.Now;
        db.Engineers.Add(eng);
        await db.SaveChangesAsync();
        return eng.Id;
    }

    public Task<List<Engineer>> EngineersForSelectAsync() =>
        db.Engineers.Include(e => e.GroupRepair).Where(e => e.IsActive).OrderBy(e => e.EngineerName).ToListAsync();

    public async Task<(bool ok, string msg)> DeleteEngineerAsync(int id)
    {
        var e = await db.Engineers.Include(x => x.Assignments).FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return (false, "Không tìm thấy kỹ thuật viên.");
        if (e.Assignments.Count > 0) return (false, "Không thể xóa KTV đã có lịch sử tham gia phân công sửa chữa.");
        db.Engineers.Remove(e);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa kỹ thuật viên {e.EngineerName}.");
    }

    public async Task<List<AssignmentWork>> AssignmentWorksAsync(AssignmentWorkStatus? status, string? q, DateTime? fromDate, DateTime? toDate, int? roId = null)
    {
        var query = db.AssignmentWorks
            .Include(a => a.RO).ThenInclude(r => r.Car)
            .Include(a => a.RO).ThenInclude(r => r.Customer)
            .Include(a => a.SCCCavity)
            .Include(a => a.SCDCavity)
            .Include(a => a.SCSCavity)
            .Include(a => a.Engineers).ThenInclude(e => e.Engineer)
            .AsQueryable();

        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        if (roId.HasValue && roId.Value > 0) query = query.Where(a => a.ROId == roId.Value);
        if (fromDate.HasValue) query = query.Where(a => a.CreatedAt.Date >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(a => a.CreatedAt.Date <= toDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(a => a.AssignmentNo.ToLower().Contains(kw)
                || a.RO.Code.ToLower().Contains(kw)
                || a.RO.Car.Plate.ToLower().Contains(kw)
                || a.RO.Car.Model.ToLower().Contains(kw)
                || a.RO.Customer.Name.ToLower().Contains(kw)
                || (a.Note != null && a.Note.ToLower().Contains(kw))
                || a.Engineers.Any(e => e.Engineer.EngineerName.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(a => a.CreatedAt).ToList();
    }

    public Task<AssignmentWork?> GetAssignmentWorkAsync(int id) =>
        db.AssignmentWorks
            .Include(a => a.RO).ThenInclude(r => r.Car)
            .Include(a => a.RO).ThenInclude(r => r.Customer)
            .Include(a => a.RO).ThenInclude(r => r.Lines)
            .Include(a => a.SCCCavity)
            .Include(a => a.SCDCavity)
            .Include(a => a.SCSCavity)
            .Include(a => a.Engineers).ThenInclude(e => e.Engineer).ThenInclude(eng => eng.GroupRepair)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<(bool conflict, string? conflictMessage)> CheckCavityConflictAsync(int cavityId, DateTime start, DateTime end, int? excludeAssignmentId = null)
    {
        if (cavityId <= 0 || start >= end) return (false, null);

        var query = db.AssignmentWorks
            .Include(a => a.RO).ThenInclude(r => r.Car)
            .Include(a => a.SCCCavity)
            .Include(a => a.SCDCavity)
            .Include(a => a.SCSCavity)
            .Where(a => a.Status == AssignmentWorkStatus.Assigned || a.Status == AssignmentWorkStatus.InProgress);

        if (excludeAssignmentId.HasValue)
            query = query.Where(a => a.Id != excludeAssignmentId.Value);

        var activeAssignments = await query.ToListAsync();

        foreach (var a in activeAssignments)
        {
            // Kiểm tra khoang SCC
            if (a.SCCCavityId == cavityId && a.SCCPlanStartDTime.HasValue && a.SCCPlanFinishDTime.HasValue)
            {
                if (a.SCCPlanStartDTime.Value < end && a.SCCPlanFinishDTime.Value > start)
                {
                    return (true, $"Khoang {a.SCCCavity?.CavityName ?? $"#{cavityId}"} đang có lịch SCC cho RO {a.RO.Code} ({a.RO.Car?.Plate}) từ {a.SCCPlanStartDTime:HH:mm dd/MM} đến {a.SCCPlanFinishDTime:HH:mm dd/MM}.");
                }
            }
            // Kiểm tra khoang SCD
            if (a.SCDCavityId == cavityId && a.SCDPlanStartDTime.HasValue && a.SCDPlanFinishDTime.HasValue)
            {
                if (a.SCDPlanStartDTime.Value < end && a.SCDPlanFinishDTime.Value > start)
                {
                    return (true, $"Khoang {a.SCDCavity?.CavityName ?? $"#{cavityId}"} đang có lịch SCD cho RO {a.RO.Code} ({a.RO.Car?.Plate}) từ {a.SCDPlanStartDTime:HH:mm dd/MM} đến {a.SCDPlanFinishDTime:HH:mm dd/MM}.");
                }
            }
            // Kiểm tra khoang SCS
            if (a.SCSCavityId == cavityId && a.SCSPlanStartDTime.HasValue && a.SCSPlanFinishDTime.HasValue)
            {
                if (a.SCSPlanStartDTime.Value < end && a.SCSPlanFinishDTime.Value > start)
                {
                    return (true, $"Khoang {a.SCSCavity?.CavityName ?? $"#{cavityId}"} đang có lịch SCS cho RO {a.RO.Code} ({a.RO.Car?.Plate}) từ {a.SCSPlanStartDTime:HH:mm dd/MM} đến {a.SCSPlanFinishDTime:HH:mm dd/MM}.");
                }
            }
        }

        return (false, null);
    }

    public async Task<int> CreateAssignmentWorkAsync(AssignmentWork assignment, List<AssignmentEngineer> engineers)
    {
        var ro = await db.ROs.Include(r => r.Car).FirstOrDefaultAsync(r => r.Id == assignment.ROId);
        if (ro == null) throw new InvalidOperationException("Không tìm thấy Lệnh sửa chữa (RO).");

        // Quy tắc idn.CarService: RO phải ở trạng thái HasRO (hoặc InGarage) mới được phân công thợ
        if (ro.Status != ROStatus.HasRO && ro.Status != ROStatus.InGarage)
            throw new InvalidOperationException($"Lệnh sửa chữa {ro.Code} đang ở trạng thái '{Ui.Status(ro.Status).text}'. Chỉ có thể phân công thợ khi RO ở trạng thái 'Chờ sửa (HasRO)' hoặc 'Đang sửa (InGarage)'.");

        // Kiểm tra xung đột khoang sửa chữa
        if (assignment.SCCCavityId.HasValue && assignment.SCCPlanStartDTime.HasValue && assignment.SCCPlanFinishDTime.HasValue)
        {
            var (conflict, msg) = await CheckCavityConflictAsync(assignment.SCCCavityId.Value, assignment.SCCPlanStartDTime.Value, assignment.SCCPlanFinishDTime.Value);
            if (conflict) throw new InvalidOperationException(msg);
        }
        if (assignment.SCDCavityId.HasValue && assignment.SCDPlanStartDTime.HasValue && assignment.SCDPlanFinishDTime.HasValue)
        {
            var (conflict, msg) = await CheckCavityConflictAsync(assignment.SCDCavityId.Value, assignment.SCDPlanStartDTime.Value, assignment.SCDPlanFinishDTime.Value);
            if (conflict) throw new InvalidOperationException(msg);
        }
        if (assignment.SCSCavityId.HasValue && assignment.SCSPlanStartDTime.HasValue && assignment.SCSPlanFinishDTime.HasValue)
        {
            var (conflict, msg) = await CheckCavityConflictAsync(assignment.SCSCavityId.Value, assignment.SCSPlanStartDTime.Value, assignment.SCSPlanFinishDTime.Value);
            if (conflict) throw new InvalidOperationException(msg);
        }

        if (string.IsNullOrWhiteSpace(assignment.AssignmentNo))
        {
            var countTotal = await db.AssignmentWorks.CountAsync();
            assignment.AssignmentNo = $"PC{DateTime.Today:yyMMdd}-{countTotal + 1:D3}";
        }

        assignment.CreatedAt = DateTime.Now;
        db.AssignmentWorks.Add(assignment);
        await db.SaveChangesAsync();

        if (engineers != null && engineers.Count > 0)
        {
            foreach (var eng in engineers)
            {
                eng.AssignmentWorkId = assignment.Id;
                db.AssignmentEngineers.Add(eng);
            }
            await db.SaveChangesAsync();
        }

        // Tự động gán KTV chính cho RO nếu chưa có
        var primaryEng = engineers?.FirstOrDefault(e => e.IsPrimary);
        if (primaryEng != null)
        {
            var engEntity = await db.Engineers.FirstOrDefaultAsync(e => e.Id == primaryEng.EngineerId);
            if (engEntity != null && string.IsNullOrWhiteSpace(ro.Technician))
            {
                ro.Technician = engEntity.EngineerName;
                await db.SaveChangesAsync();
            }
        }

        return assignment.Id;
    }

    public async Task<(bool ok, string msg)> StartAssignmentWorkAsync(int id, string? startedBy = null)
    {
        var assignment = await db.AssignmentWorks
            .Include(a => a.RO).ThenInclude(r => r.Car)
            .Include(a => a.SCCCavity)
            .Include(a => a.SCDCavity)
            .Include(a => a.SCSCavity)
            .Include(a => a.Engineers).ThenInclude(e => e.Engineer)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assignment == null) return (false, "Không tìm thấy phiếu phân công công việc.");
        if (assignment.Status != AssignmentWorkStatus.Assigned)
            return (false, $"Phiếu phân công đang ở trạng thái '{Ui.AssignmentWorkStatus(assignment.Status).text}', không thể bắt đầu.");

        assignment.Status = AssignmentWorkStatus.InProgress;
        assignment.StartedAt = DateTime.Now;

        if (assignment.HasSCC && !assignment.SCCActualStartDTime.HasValue)
            assignment.SCCActualStartDTime = DateTime.Now;
        if (assignment.HasSCD && !assignment.SCDActualStartDTime.HasValue)
            assignment.SCDActualStartDTime = DateTime.Now;
        if (assignment.HasSCS && !assignment.SCSActualStartDTime.HasValue)
            assignment.SCSActualStartDTime = DateTime.Now;

        var techName = assignment.PrimaryTechnician;

        // Cập nhật trạng thái RO sang InGarage nếu đang là HasRO
        if (assignment.RO.Status == ROStatus.HasRO)
        {
            assignment.RO.Status = ROStatus.InGarage;
            assignment.RO.IntakeAt ??= DateTime.Now;
            if (!string.IsNullOrWhiteSpace(techName) && techName != "Chưa chỉ định")
                assignment.RO.Technician = techName;
        }

        // Cập nhật trạng thái khoang sửa chữa sang Occupied
        var primaryCavity = assignment.SCCCavity ?? assignment.SCDCavity ?? assignment.SCSCavity;
        if (primaryCavity != null)
        {
            primaryCavity.Status = CavityStatus.Occupied;
            primaryCavity.CurrentROId = assignment.ROId;
            primaryCavity.CurrentCarPlate = assignment.RO.Car?.Plate;
            primaryCavity.CurrentCarModel = assignment.RO.Car?.Model;
            primaryCavity.CurrentTechnician = techName;
            primaryCavity.StartUseDate = DateTime.Now;
            primaryCavity.ExpectedFinishDate = assignment.SCCPlanFinishDTime ?? assignment.SCDPlanFinishDTime ?? assignment.SCSPlanFinishDTime;
            assignment.RO.CavityId = primaryCavity.Id;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã bắt đầu thi công Lệnh phân công {assignment.AssignmentNo}. Xe đã vào xưởng (RO: {assignment.RO.Code} → Đang sửa INGA).");
    }

    public async Task<(bool ok, string msg)> CompleteAssignmentWorkAsync(int id, string? completedBy = null)
    {
        var assignment = await db.AssignmentWorks
            .Include(a => a.RO).ThenInclude(r => r.Car)
            .Include(a => a.SCCCavity)
            .Include(a => a.SCDCavity)
            .Include(a => a.SCSCavity)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assignment == null) return (false, "Không tìm thấy phiếu phân công công việc.");
        if (assignment.Status != AssignmentWorkStatus.InProgress && assignment.Status != AssignmentWorkStatus.Assigned)
            return (false, $"Phiếu phân công đang ở trạng thái '{Ui.AssignmentWorkStatus(assignment.Status).text}', không thể hoàn tất.");

        assignment.Status = AssignmentWorkStatus.Completed;
        assignment.FinishedAt = DateTime.Now;

        if (assignment.HasSCC && !assignment.SCCActualFinishDTime.HasValue)
            assignment.SCCActualFinishDTime = DateTime.Now;
        if (assignment.HasSCD && !assignment.SCDActualFinishDTime.HasValue)
            assignment.SCDActualFinishDTime = DateTime.Now;
        if (assignment.HasSCS && !assignment.SCSActualFinishDTime.HasValue)
            assignment.SCSActualFinishDTime = DateTime.Now;

        // Giải phóng các khoang sửa chữa liên quan
        void ReleaseBay(Cavity? cavity)
        {
            if (cavity != null && cavity.CurrentROId == assignment.ROId)
            {
                cavity.Status = CavityStatus.Available;
                cavity.CurrentROId = null;
                cavity.CurrentCarPlate = null;
                cavity.CurrentCarModel = null;
                cavity.CurrentTechnician = null;
                cavity.FinishUseDate = DateTime.Now;
            }
        }

        ReleaseBay(assignment.SCCCavity);
        ReleaseBay(assignment.SCDCavity);
        ReleaseBay(assignment.SCSCavity);

        // Kiểm tra xem tất cả phân công của RO này đã hoàn tất chưa
        var otherActive = await db.AssignmentWorks.AnyAsync(a => a.ROId == assignment.ROId && a.Id != id && (a.Status == AssignmentWorkStatus.Assigned || a.Status == AssignmentWorkStatus.InProgress));
        if (!otherActive && assignment.RO.Status == ROStatus.InGarage)
        {
            assignment.RO.Status = ROStatus.Repaired;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã nghiệm thu hoàn thành phân công {assignment.AssignmentNo}. Lệnh sửa chữa {assignment.RO.Code} chuyển sang 'Sửa xong (RPRD)'.");
    }

    public async Task<(bool ok, string msg)> CancelAssignmentWorkAsync(int id, string? reason = null)
    {
        var assignment = await db.AssignmentWorks
            .Include(a => a.SCCCavity)
            .Include(a => a.SCDCavity)
            .Include(a => a.SCSCavity)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assignment == null) return (false, "Không tìm thấy phiếu phân công công việc.");
        if (assignment.Status == AssignmentWorkStatus.Completed)
            return (false, "Không thể hủy phân công đã hoàn tất nghiệm thu.");

        assignment.Status = AssignmentWorkStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
            assignment.Note = (string.IsNullOrWhiteSpace(assignment.Note) ? "" : assignment.Note + " | ") + $"[Đã hủy: {reason.Trim()}]";

        // Giải phóng khoang nếu đang chiếm dụng
        void ReleaseBay(Cavity? cavity)
        {
            if (cavity != null && cavity.CurrentROId == assignment.ROId)
            {
                cavity.Status = CavityStatus.Available;
                cavity.CurrentROId = null;
                cavity.CurrentCarPlate = null;
                cavity.CurrentCarModel = null;
                cavity.CurrentTechnician = null;
                cavity.FinishUseDate = DateTime.Now;
            }
        }
        ReleaseBay(assignment.SCCCavity);
        ReleaseBay(assignment.SCDCavity);
        ReleaseBay(assignment.SCSCavity);

        await db.SaveChangesAsync();
        return (true, $"Đã hủy phiếu phân công {assignment.AssignmentNo}.");
    }

    public async Task<(bool ok, string msg)> DeleteAssignmentWorkAsync(int id)
    {
        var assignment = await db.AssignmentWorks.Include(a => a.Engineers).FirstOrDefaultAsync(a => a.Id == id);
        if (assignment == null) return (false, "Không tìm thấy phiếu phân công công việc.");
        if (assignment.Status is not (AssignmentWorkStatus.Assigned or AssignmentWorkStatus.Cancelled))
            return (false, "Chỉ có thể xóa phiếu phân công ở trạng thái 'Chờ nhận việc' hoặc 'Đã hủy'.");

        db.AssignmentEngineers.RemoveRange(assignment.Engineers);
        db.AssignmentWorks.Remove(assignment);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa phiếu phân công {assignment.AssignmentNo}.");
    }

    public Task<List<RepairOrder>> ROsEligibleForAssignmentAsync() =>
        db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Lines)
            .Include(r => r.Cavity)
            .Include(r => r.AssignmentWorks)
            .Where(r => r.Status == ROStatus.HasRO || r.Status == ROStatus.InGarage)
            .OrderByDescending(r => r.Status == ROStatus.HasRO)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync();

    // =========================================================================
    // QUẢN LÝ BẢO HIỂM XE & HỒ SƠ BỒI THƯỜNG (Ser_Insurance, Ser_InsuranceContract, Ser_InsuranceDebit)
    // =========================================================================

    public static InsuranceClaimStatus[] AllowedInsuranceNext(InsuranceClaimStatus s) => s switch
    {
        InsuranceClaimStatus.Draft => [InsuranceClaimStatus.Submitted, InsuranceClaimStatus.Rejected],
        InsuranceClaimStatus.Submitted => [InsuranceClaimStatus.Approved, InsuranceClaimStatus.Rejected, InsuranceClaimStatus.Draft],
        InsuranceClaimStatus.Approved => [InsuranceClaimStatus.Settled, InsuranceClaimStatus.Submitted],
        _ => []
    };

    public async Task<List<InsuranceCompany>> InsuranceCompaniesAsync(string? q, bool? isActive = null)
    {
        var query = db.InsuranceCompanies.Include(c => c.Contracts).Include(c => c.Claims).AsQueryable();
        if (isActive.HasValue) query = query.Where(c => c.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = query.Where(c => c.InsNo.ToLower().Contains(s) || c.InsName.ToLower().Contains(s) || (c.Phone != null && c.Phone.Contains(s)));
        }
        return await query.OrderBy(c => c.InsNo).ToListAsync();
    }

    public Task<InsuranceCompany?> GetInsuranceCompanyAsync(int id) =>
        db.InsuranceCompanies.Include(c => c.Contracts).Include(c => c.Claims).ThenInclude(cl => cl.RO).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<int> CreateInsuranceCompanyAsync(InsuranceCompany c)
    {
        if (string.IsNullOrWhiteSpace(c.InsNo))
            c.InsNo = $"BH-{(await db.InsuranceCompanies.CountAsync() + 1):D2}";
        db.InsuranceCompanies.Add(c);
        await db.SaveChangesAsync();
        return c.Id;
    }

    public Task<List<InsuranceCompany>> InsuranceCompaniesForSelectAsync() =>
        db.InsuranceCompanies.Where(c => c.IsActive).OrderBy(c => c.InsName).ToListAsync();

    public async Task<List<InsuranceContract>> InsuranceContractsAsync(int? companyId = null, bool? activeOnly = null)
    {
        var query = db.InsuranceContracts.Include(c => c.InsuranceCompany).Include(c => c.Claims).AsQueryable();
        if (companyId.HasValue && companyId.Value > 0)
            query = query.Where(c => c.InsuranceCompanyId == companyId.Value);
        if (activeOnly == true)
            query = query.Where(c => c.IsActive && c.FinishDate >= DateTime.Today);
        return await query.OrderByDescending(c => c.StartDate).ToListAsync();
    }

    public Task<InsuranceContract?> GetInsuranceContractAsync(int id) =>
        db.InsuranceContracts.Include(c => c.InsuranceCompany).Include(c => c.Claims).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<int> CreateInsuranceContractAsync(InsuranceContract c)
    {
        if (string.IsNullOrWhiteSpace(c.ContractNo))
            c.ContractNo = $"HD-BH/{DateTime.Today:yyyy}/{(await db.InsuranceContracts.CountAsync() + 1):D2}";
        if (string.IsNullOrWhiteSpace(c.ContractCode))
            c.ContractCode = c.ContractNo;
        db.InsuranceContracts.Add(c);
        await db.SaveChangesAsync();
        return c.Id;
    }

    public Task<List<InsuranceContract>> InsuranceContractsForSelectAsync(int? companyId = null)
    {
        var query = db.InsuranceContracts.Include(c => c.InsuranceCompany).Where(c => c.IsActive);
        if (companyId.HasValue && companyId.Value > 0)
            query = query.Where(c => c.InsuranceCompanyId == companyId.Value);
        return query.OrderBy(c => c.ContractNo).ToListAsync();
    }

    public async Task<List<InsuranceClaim>> InsuranceClaimsAsync(InsuranceClaimStatus? status = null, string? q = null, int? roId = null, int? companyId = null)
    {
        var query = db.InsuranceClaims
            .Include(c => c.RO).ThenInclude(r => r.Car)
            .Include(c => c.RO).ThenInclude(r => r.Customer)
            .Include(c => c.InsuranceCompany)
            .Include(c => c.InsuranceContract)
            .Include(c => c.Items)
            .AsQueryable();

        if (status.HasValue) query = query.Where(c => c.Status == status.Value);
        if (roId.HasValue && roId.Value > 0) query = query.Where(c => c.ROId == roId.Value);
        if (companyId.HasValue && companyId.Value > 0) query = query.Where(c => c.InsuranceCompanyId == companyId.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = query.Where(c => c.ClaimNo.ToLower().Contains(s)
                || c.PolicyNo.ToLower().Contains(s)
                || (c.ClaimFileNo != null && c.ClaimFileNo.ToLower().Contains(s))
                || c.RO.Code.ToLower().Contains(s)
                || c.RO.Car.Plate.ToLower().Contains(s));
        }

        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public Task<InsuranceClaim?> GetInsuranceClaimAsync(int id) =>
        db.InsuranceClaims
            .Include(c => c.RO).ThenInclude(r => r.Car)
            .Include(c => c.RO).ThenInclude(r => r.Customer)
            .Include(c => c.RO).ThenInclude(r => r.Lines)
            .Include(c => c.InsuranceCompany)
            .Include(c => c.InsuranceContract)
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<int> CreateInsuranceClaimAsync(InsuranceClaim claim, List<InsuranceClaimItem>? items = null)
    {
        if (string.IsNullOrWhiteSpace(claim.ClaimNo))
            claim.ClaimNo = $"BH{DateTime.Now:yyMMdd}-{(await db.InsuranceClaims.CountAsync() + 1):D3}";

        claim.CreatedAt = DateTime.Now;
        if (items != null && items.Count > 0)
        {
            claim.Items = items;
            claim.EstimatedAmount = items.Sum(i => i.EstimatedAmount > 0 ? i.EstimatedAmount : (i.Quantity * i.UnitPrice));
            if (claim.ApprovedAmount <= 0)
                claim.ApprovedAmount = items.Where(i => i.IsApproved).Sum(i => i.ApprovedAmount > 0 ? i.ApprovedAmount : (i.Quantity * i.UnitPrice));
        }

        claim.InsuranceAmount = Math.Max(0, claim.ApprovedAmount - claim.DeductibleAmount - claim.PenaltyAmount);
        claim.CustomerAmount = claim.DeductibleAmount + claim.PenaltyAmount + Math.Max(0, claim.EstimatedAmount - claim.ApprovedAmount);

        db.InsuranceClaims.Add(claim);
        await db.SaveChangesAsync();
        return claim.Id;
    }

    public async Task<int> CreateInsuranceClaimFromROAsync(int roId, int companyId, int? contractId, string policyNo, string? claimFileNo, string? surveyorName, string? surveyorPhone, string accidentDesc, decimal deductibleAmount, decimal penaltyAmount, string createdBy)
    {
        var ro = await db.ROs.Include(r => r.Lines).Include(r => r.Car).FirstOrDefaultAsync(r => r.Id == roId)
            ?? throw new InvalidOperationException("Không tìm thấy Lệnh sửa chữa RO.");

        var count = await db.InsuranceClaims.CountAsync() + 1;
        var claimNo = $"BH{DateTime.Now:yyMMdd}-{count:D3}";

        var claim = new InsuranceClaim
        {
            ClaimNo = claimNo,
            ROId = roId,
            InsuranceCompanyId = companyId,
            InsuranceContractId = (contractId.HasValue && contractId.Value > 0) ? contractId.Value : null,
            PolicyNo = policyNo.Trim(),
            ClaimFileNo = claimFileNo?.Trim(),
            SurveyorName = surveyorName?.Trim(),
            SurveyorPhone = surveyorPhone?.Trim(),
            AccidentDate = DateTime.Today,
            AccidentDescription = string.IsNullOrWhiteSpace(accidentDesc) ? (ro.IntakeNote ?? "Tổn thất thân vỏ xe") : accidentDesc.Trim(),
            DeductibleAmount = deductibleAmount,
            PenaltyAmount = penaltyAmount,
            CreatedBy = createdBy,
            CreatedAt = DateTime.Now,
            Status = InsuranceClaimStatus.Draft
        };

        // Sao chép các hạng mục từ RO Lines (ưu tiên các dòng có ExpenseType = Insurance hoặc lấy tất cả nếu chưa phân loại)
        var linesToCopy = ro.Lines.Where(l => l.ExpenseType == ExpenseType.Insurance).ToList();
        if (linesToCopy.Count == 0) linesToCopy = ro.Lines.ToList();

        foreach (var l in linesToCopy)
        {
            claim.Items.Add(new InsuranceClaimItem
            {
                Type = l.Type,
                Code = l.PartId.HasValue ? $"PT-{l.PartId}" : "CV-BH",
                Name = l.Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                EstimatedAmount = l.Amount,
                ApprovedAmount = l.Amount,
                IsApproved = true
            });
            l.ExpenseType = ExpenseType.Insurance;
        }

        claim.EstimatedAmount = claim.Items.Sum(i => i.EstimatedAmount);
        claim.ApprovedAmount = claim.Items.Sum(i => i.ApprovedAmount);
        claim.InsuranceAmount = Math.Max(0, claim.ApprovedAmount - claim.DeductibleAmount - claim.PenaltyAmount);
        claim.CustomerAmount = claim.DeductibleAmount + claim.PenaltyAmount + Math.Max(0, claim.EstimatedAmount - claim.ApprovedAmount);

        db.InsuranceClaims.Add(claim);
        await db.SaveChangesAsync();
        return claim.Id;
    }

    public async Task<(bool ok, string msg)> TransitionInsuranceClaimAsync(int id, InsuranceClaimStatus toStatus, decimal? approvedAmount = null, string? note = null)
    {
        var claim = await db.InsuranceClaims.Include(c => c.Items).Include(c => c.RO).FirstOrDefaultAsync(c => c.Id == id);
        if (claim == null) return (false, "Không tìm thấy hồ sơ bảo hiểm.");

        var allowed = AllowedInsuranceNext(claim.Status);
        if (!allowed.Contains(toStatus))
            return (false, $"Không thể chuyển từ '{Ui.InsuranceClaimStatus(claim.Status).text}' sang '{Ui.InsuranceClaimStatus(toStatus).text}'.");

        claim.Status = toStatus;
        if (!string.IsNullOrWhiteSpace(note))
        {
            if (toStatus == InsuranceClaimStatus.Rejected)
                claim.RejectionReason = note.Trim();
            else
                claim.DecisionNote = note.Trim();
        }

        if (toStatus == InsuranceClaimStatus.Submitted)
        {
            claim.SubmittedAt = DateTime.Now;
        }
        else if (toStatus == InsuranceClaimStatus.Approved)
        {
            claim.ApprovedAt = DateTime.Now;
            if (approvedAmount.HasValue && approvedAmount.Value > 0)
            {
                claim.ApprovedAmount = approvedAmount.Value;
            }
            claim.InsuranceAmount = Math.Max(0, claim.ApprovedAmount - claim.DeductibleAmount - claim.PenaltyAmount);
            claim.CustomerAmount = claim.DeductibleAmount + claim.PenaltyAmount + Math.Max(0, claim.EstimatedAmount - claim.ApprovedAmount);
        }
        else if (toStatus == InsuranceClaimStatus.Settled)
        {
            claim.SettledAt = DateTime.Now;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật trạng thái hồ sơ bồi thường sang: '{Ui.InsuranceClaimStatus(toStatus).text}'.");
    }

    public async Task<(bool ok, string msg)> DeleteInsuranceClaimAsync(int id)
    {
        var claim = await db.InsuranceClaims.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
        if (claim == null) return (false, "Không tìm thấy hồ sơ bảo hiểm.");
        if (claim.Status != InsuranceClaimStatus.Draft && claim.Status != InsuranceClaimStatus.Rejected)
            return (false, "Chỉ có thể xóa hồ sơ ở trạng thái 'Lập hồ sơ' hoặc 'Từ chối bồi thường'.");

        db.InsuranceClaimItems.RemoveRange(claim.Items);
        db.InsuranceClaims.Remove(claim);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa hồ sơ bồi thường bảo hiểm {claim.ClaimNo}.");
    }

    public Task<List<RepairOrder>> ROsEligibleForInsuranceAsync() =>
        db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Lines)
            .Include(r => r.InsuranceClaims)
            .Where(r => r.Status != ROStatus.Rejected && r.Status != ROStatus.NotResponding)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    // --- Service Marketing Campaign Management (Ser_CampaignMarketing, Ser_CampaignMarketingPart) ---
    public async Task<List<CampaignMarketing>> CampaignMarketingsAsync(CampaignMarketingStatus? status, string? q, bool? currentOnly)
    {
        var query = db.CampaignMarketings
            .Include(c => c.Items).ThenInclude(i => i.Part)
            .Include(c => c.AppliedROs)
            .AsQueryable();

        if (status.HasValue) query = query.Where(c => c.Status == status.Value);
        if (currentOnly == true)
        {
            var today = DateTime.Today;
            query = query.Where(c => c.Status == CampaignMarketingStatus.Active && c.EffDateStart.Date <= today && c.EffDateEnd.Date >= today);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(c => c.CamMarketingNo.ToLower().Contains(kw)
                || c.CamMarketingName.ToLower().Contains(kw)
                || (c.CamMarketingDesc != null && c.CamMarketingDesc.ToLower().Contains(kw))
                || (c.ConditionModel != null && c.ConditionModel.ToLower().Contains(kw)));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(c => c.CreatedAt).ToList();
    }

    public Task<CampaignMarketing?> GetCampaignMarketingAsync(int id) =>
        db.CampaignMarketings
            .Include(c => c.Items).ThenInclude(i => i.Part)
            .Include(c => c.AppliedROs).ThenInclude(r => r.Car)
            .Include(c => c.AppliedROs).ThenInclude(r => r.Customer)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<int> CreateCampaignMarketingAsync(CampaignMarketing campaign, List<CampaignMarketingItem> items)
    {
        if (string.IsNullOrWhiteSpace(campaign.CamMarketingName))
            throw new InvalidOperationException("Vui lòng nhập tên chiến dịch khuyến mãi.");

        if (campaign.EffDateEnd < campaign.EffDateStart)
            throw new InvalidOperationException("Ngày kết thúc không được nhỏ hơn ngày bắt đầu.");

        if (string.IsNullOrWhiteSpace(campaign.CamMarketingNo))
        {
            var count = await db.CampaignMarketings.CountAsync();
            campaign.CamMarketingNo = $"KM{DateTime.Now:yyMMdd}-{count + 1:D3}";
        }

        campaign.CreatedAt = DateTime.Now;
        if (campaign.Status == CampaignMarketingStatus.Active && !campaign.ApprovedAt.HasValue)
        {
            campaign.ApprovedAt = DateTime.Now;
            campaign.ApprovedBy ??= campaign.CreatedBy;
        }

        db.CampaignMarketings.Add(campaign);
        await db.SaveChangesAsync();

        if (items != null && items.Count > 0)
        {
            foreach (var it in items)
            {
                it.CampaignMarketingId = campaign.Id;
                db.CampaignMarketingItems.Add(it);
            }
            await db.SaveChangesAsync();
        }

        return campaign.Id;
    }

    public async Task<(bool ok, string msg)> TransitionCampaignStatusAsync(int id, CampaignMarketingStatus to, string? approvedBy = null)
    {
        var campaign = await db.CampaignMarketings.FirstOrDefaultAsync(c => c.Id == id);
        if (campaign == null) return (false, "Không tìm thấy chiến dịch khuyến mãi.");

        var allowed = AllowedNextCampaign(campaign.Status);
        if (!allowed.Contains(to))
            return (false, $"Không thể chuyển từ '{Ui.CampaignStatus(campaign.Status).text}' sang '{Ui.CampaignStatus(to).text}'.");

        campaign.Status = to;
        if (to == CampaignMarketingStatus.Active)
        {
            campaign.ApprovedAt = DateTime.Now;
            campaign.ApprovedBy = string.IsNullOrWhiteSpace(approvedBy) ? "Ban Giám đốc" : approvedBy.Trim();
        }

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật trạng thái chiến dịch sang '{Ui.CampaignStatus(to).text}'.");
    }

    public async Task<(bool ok, string msg)> DeleteCampaignMarketingAsync(int id)
    {
        var campaign = await db.CampaignMarketings.Include(c => c.Items).Include(c => c.AppliedROs).FirstOrDefaultAsync(c => c.Id == id);
        if (campaign == null) return (false, "Không tìm thấy chiến dịch khuyến mãi.");

        if (campaign.AppliedROs.Count > 0)
            return (false, $"Không thể xóa chiến dịch đã được áp dụng trên {campaign.AppliedROs.Count} lệnh sửa chữa (RO).");

        if (campaign.Status == CampaignMarketingStatus.Active)
            return (false, "Không thể xóa chiến dịch đang chạy (Active). Hãy hủy hoặc kết thúc trước.");

        db.CampaignMarketingItems.RemoveRange(campaign.Items);
        db.CampaignMarketings.Remove(campaign);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa chiến dịch khuyến mãi '{campaign.CamMarketingNo} - {campaign.CamMarketingName}'.");
    }

    public async Task<List<CampaignMarketing>> GetEligibleCampaignsForCarAsync(int carId)
    {
        var car = await db.Cars.FirstOrDefaultAsync(c => c.Id == carId);
        if (car == null) return [];

        var today = DateTime.Today;
        var active = await db.CampaignMarketings
            .Include(c => c.Items).ThenInclude(i => i.Part)
            .Where(c => c.Status == CampaignMarketingStatus.Active && c.EffDateStart.Date <= today && c.EffDateEnd.Date >= today)
            .ToListAsync();

        return active.Where(c => IsCarMatchingCampaign(car, c)).ToList();
    }

    private static bool IsCarMatchingCampaign(Car car, CampaignMarketing c)
    {
        if (!string.IsNullOrWhiteSpace(c.ConditionModel))
        {
            if (string.IsNullOrWhiteSpace(car.Model) || !car.Model.Contains(c.ConditionModel.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;
        }

        if (!string.IsNullOrWhiteSpace(c.ConditionPlateNo))
        {
            var cond = c.ConditionPlateNo.Trim();
            if (string.IsNullOrWhiteSpace(car.Plate) || !car.Plate.StartsWith(cond, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        if (!string.IsNullOrWhiteSpace(c.ConditionVIN))
        {
            var condVin = c.ConditionVIN.Trim();
            if (string.IsNullOrWhiteSpace(car.Vin) || !car.Vin.Contains(condVin, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    public async Task<(bool ok, string msg, decimal discountTotal)> ApplyCampaignToROAsync(int campaignId, int roId)
    {
        var ro = await db.ROs
            .Include(r => r.Car)
            .Include(r => r.Lines).ThenInclude(l => l.Part)
            .FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy Lệnh sửa chữa (RO).", 0);

        if (ro.Status is ROStatus.Paid or ROStatus.Finished or ROStatus.Rejected)
            return (false, $"Lệnh RO đang ở trạng thái '{Ui.Status(ro.Status).text}', không thể áp dụng khuyến mãi.", 0);

        var campaign = await db.CampaignMarketings
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == campaignId);
        if (campaign == null) return (false, "Không tìm thấy chiến dịch khuyến mãi.", 0);

        if (campaign.Status != CampaignMarketingStatus.Active)
            return (false, $"Chiến dịch '{campaign.CamMarketingName}' không còn hoạt động.", 0);

        if (!campaign.IsActiveNow)
            return (false, $"Chiến dịch '{campaign.CamMarketingName}' đã hết hạn hoặc chưa đến ngày áp dụng.", 0);

        if (!IsCarMatchingCampaign(ro.Car, campaign))
            return (false, $"Xe '{ro.Car?.Plate} ({ro.Car?.Model})' không thuộc phạm vi áp dụng của chiến dịch này.", 0);

        // Tính toán chiết khấu tự động theo cấu hình Ser_CampaignMarketing / Ser_CampaignMarketingPart
        decimal discountTotal = 0;
        foreach (var line in ro.Lines)
        {
            if (line.ExpenseType != ExpenseType.Customer) continue; // Chỉ giảm phần khách trả

            if (line.Type == LineType.Labor)
            {
                if (campaign.DiscountLaborPercent > 0)
                {
                    var laborDisc = line.Amount * (campaign.DiscountLaborPercent / 100m);
                    discountTotal += laborDisc;
                }
            }
            else if (line.Type == LineType.Part)
            {
                // Ưu tiên giảm giá phụ tùng theo bảng CampaignMarketingItem nếu có
                var specificItem = campaign.Items.FirstOrDefault(i => i.PartId == line.PartId || (line.Part != null && i.PartCode == line.Part.Code));
                if (specificItem != null && specificItem.PercentDiscount > 0)
                {
                    var partDisc = line.Amount * (specificItem.PercentDiscount / 100m);
                    discountTotal += partDisc;
                }
                else if (campaign.DiscountPartPercent > 0)
                {
                    var partDisc = line.Amount * (campaign.DiscountPartPercent / 100m);
                    discountTotal += partDisc;
                }
            }
        }

        discountTotal = Math.Round(discountTotal);
        ro.CampaignMarketingId = campaign.Id;
        ro.CampaignDiscountAmount = discountTotal;

        await db.SaveChangesAsync();
        return (true, $"Đã áp dụng chiến dịch '{campaign.CamMarketingName}'. Tổng giá trị ưu đãi chiết khấu: {discountTotal:N0} đ.", discountTotal);
    }

    public async Task<(bool ok, string msg)> RemoveCampaignFromROAsync(int roId)
    {
        var ro = await db.ROs.FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy Lệnh sửa chữa.");

        ro.CampaignMarketingId = null;
        ro.CampaignDiscountAmount = 0;
        await db.SaveChangesAsync();
        return (true, "Đã gỡ bỏ chiến dịch khuyến mãi khỏi Lệnh sửa chữa.");
    }

    public async Task<List<RepairOrder>> ROsEligibleForCampaignAsync(int campaignId)
    {
        var campaign = await db.CampaignMarketings.FirstOrDefaultAsync(c => c.Id == campaignId);
        if (campaign == null) return [];

        var ros = await db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Lines)
            .Where(r => r.Status != ROStatus.Finished && r.Status != ROStatus.Paid && r.Status != ROStatus.Rejected)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return ros.Where(r => IsCarMatchingCampaign(r.Car, campaign)).ToList();
    }

    public Task<List<CampaignMarketing>> ActiveCampaignsForSelectAsync() =>
        db.CampaignMarketings
            .Where(c => c.Status == CampaignMarketingStatus.Active)
            .OrderBy(c => c.CamMarketingName)
            .ToListAsync();

    // --- Periodic Maintenance Reminders (Ser_CustomerCareMace / MH 83) ---
    public async Task<List<CustomerCareMace>> CustomerCareMacesAsync(CustomerCareMaceStatus? status, MaceType? maceType, string? timeFilter, string? q)
    {
        var query = db.CustomerCareMaces
            .Include(m => m.Car)
            .Include(m => m.Customer)
            .Include(m => m.RO)
            .Include(m => m.Appointment)
            .AsQueryable();

        if (status.HasValue) query = query.Where(m => m.Status == status.Value);
        if (maceType.HasValue) query = query.Where(m => m.MaceType == maceType.Value);

        var today = DateTime.Today;
        if (!string.IsNullOrWhiteSpace(timeFilter))
        {
            switch (timeFilter.Trim().ToLower())
            {
                case "overdue":
                    query = query.Where(m => m.MaceRecomentDate.Date < today && m.Status == CustomerCareMaceStatus.Pending);
                    break;
                case "due_7days":
                    var next7 = today.AddDays(7);
                    query = query.Where(m => m.MaceRecomentDate.Date >= today && m.MaceRecomentDate.Date <= next7);
                    break;
                case "month":
                    query = query.Where(m => m.MaceRecomentDate.Year == today.Year && m.MaceRecomentDate.Month == today.Month);
                    break;
            }
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(m => m.MaceNo.ToLower().Contains(term)
                || m.Car.Plate.ToLower().Contains(term)
                || m.Car.Model.ToLower().Contains(term)
                || m.Customer.Name.ToLower().Contains(term)
                || (m.Customer.Phone != null && m.Customer.Phone.Contains(term)));
        }

        return await query
            .OrderBy(m => m.Status == CustomerCareMaceStatus.Pending ? 0 : 1)
            .ThenBy(m => m.MaceRecomentDate)
            .ThenByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public Task<CustomerCareMace?> GetCustomerCareMaceAsync(int id) =>
        db.CustomerCareMaces
            .Include(m => m.Car)
            .Include(m => m.Customer)
            .Include(m => m.RO).ThenInclude(r => r!.Lines)
            .Include(m => m.Appointment)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<int> CreateCustomerCareMaceAsync(CustomerCareMace mace)
    {
        var car = await db.Cars.Include(c => c.Customer).FirstOrDefaultAsync(c => c.Id == mace.CarId)
            ?? throw new InvalidOperationException("Không tìm thấy xe được chọn.");

        mace.CustomerId = car.CustomerId;
        if (string.IsNullOrWhiteSpace(mace.MaceNo))
        {
            var count = await db.CustomerCareMaces.CountAsync() + 1;
            mace.MaceNo = $"MC{DateTime.Today:yyMMdd}-{count:D3}";
        }

        if (mace.NextKm <= 0)
        {
            var (_, _, km) = await CalculateNextMaintenanceAsync(mace.CarId, mace.MaceRecomentDate, mace.LastKm);
            mace.NextKm = km;
        }

        mace.CreatedAt = DateTime.Now;
        db.CustomerCareMaces.Add(mace);
        await db.SaveChangesAsync();
        return mace.Id;
    }

    public async Task<(bool ok, string msg)> UpdateCustomerCareMaceCallAsync(int id, CustomerCareMaceStatus status, DateTime? contactDate, DateTime? apointDate, string? remark, string? contactBy)
    {
        var mace = await db.CustomerCareMaces.Include(m => m.Car).FirstOrDefaultAsync(m => m.Id == id);
        if (mace == null) return (false, "Không tìm thấy phiếu nhắc bảo dưỡng.");

        mace.Status = status;
        mace.ContactDate = contactDate ?? DateTime.Now;
        mace.ContactBy = !string.IsNullOrWhiteSpace(contactBy) ? contactBy.Trim() : (mace.ContactBy ?? "Cố vấn CSKH");
        if (apointDate.HasValue) mace.ApointDate = apointDate;
        if (!string.IsNullOrWhiteSpace(remark)) mace.Remark = remark.Trim();
        mace.UpdatedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật tiến độ chăm sóc mốc {mace.NextKm:N0}km cho xe {mace.Car.Plate}.");
    }

    public async Task<(bool ok, string msg, int? appointmentId)> ConvertMaceToAppointmentAsync(int id, string? advisor = null, string? cavity = null, string? note = null)
    {
        var mace = await db.CustomerCareMaces
            .Include(m => m.Car)
            .Include(m => m.Customer)
            .Include(m => m.Appointment)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (mace == null) return (false, "Không tìm thấy phiếu nhắc bảo dưỡng.", null);
        if (mace.AppointmentId.HasValue && mace.Appointment != null)
            return (false, $"Phiếu nhắc này đã liên kết với cuộc hẹn {mace.Appointment.AppNo}.", mace.AppointmentId);

        var appDate = mace.ApointDate ?? mace.MaceRecomentDate;
        if (appDate < DateTime.Now) appDate = DateTime.Today.AddHours(9);

        var appCount = await db.Appointments.CountAsync() + 1;
        var appNo = $"APP{DateTime.Today:yyMMdd}-{appCount:D3}";

        var appointment = new Appointment
        {
            AppNo = appNo,
            CarId = mace.CarId,
            CustomerId = mace.CustomerId,
            AppointmentDate = appDate,
            ServiceType = AppointmentServiceType.Maintenance,
            Status = AppointmentStatus.Confirmed,
            Advisor = !string.IsNullOrWhiteSpace(advisor) ? advisor.Trim() : (mace.ContactBy ?? "Cố vấn dịch vụ"),
            Cavity = !string.IsNullOrWhiteSpace(cavity) ? cavity.Trim() : "Khoang bảo dưỡng nhanh (EM)",
            CustomerRequest = $"Bảo dưỡng định kỳ mốc {mace.NextKm:N0} km theo chương trình nhắc bảo dưỡng.",
            Note = !string.IsNullOrWhiteSpace(note) ? note.Trim() : (mace.Remark ?? $"Chốt hẹn từ phiếu nhắc {mace.MaceNo}"),
            Source = "Nhắc bảo dưỡng (CareMace)",
            CreatedBy = mace.ContactBy ?? "cskh",
            CreatedAt = DateTime.Now,
            ConfirmedAt = DateTime.Now
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync();

        mace.AppointmentId = appointment.Id;
        mace.Status = CustomerCareMaceStatus.Booked;
        mace.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();

        return (true, $"Đã tạo cuộc hẹn {appointment.AppNo} thành công cho xe {mace.Car.Plate}.", appointment.Id);
    }

    public async Task<(bool ok, string msg)> DeleteCustomerCareMaceAsync(int id)
    {
        var mace = await db.CustomerCareMaces.FirstOrDefaultAsync(m => m.Id == id);
        if (mace == null) return (false, "Không tìm thấy phiếu nhắc bảo dưỡng.");

        if (mace.Status == CustomerCareMaceStatus.Booked && mace.AppointmentId.HasValue)
            return (false, "Không thể xóa phiếu nhắc đã chốt thành cuộc hẹn dịch vụ.");

        db.CustomerCareMaces.Remove(mace);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa phiếu nhắc bảo dưỡng '{mace.MaceNo}'.");
    }

    public async Task<(DateTime recommendDate, MaceType maceType, int nextKm)> CalculateNextMaintenanceAsync(int carId, DateTime? referenceDate = null, int? currentOdometer = null)
    {
        var refDate = referenceDate ?? DateTime.Today;
        var ros = await db.ROs.Where(r => r.CarId == carId && (r.Status == ROStatus.Finished || r.Status == ROStatus.Paid || r.Status == ROStatus.Repaired))
            .OrderBy(r => r.IntakeAt ?? r.CreatedAt)
            .ToListAsync();

        DateTime recDate;
        MaceType mType;

        if (ros.Count > 1)
        {
            // M1: Tính dựa theo tần suất trung bình xe vào xưởng Fvx (Ser_CustomerCareMace / ProcessGetLastestMace idn.CarService)
            int totalDays = 0;
            for (int i = 1; i < ros.Count; i++)
            {
                var dt1 = ros[i - 1].IntakeAt ?? ros[i - 1].CreatedAt;
                var dt2 = ros[i].IntakeAt ?? ros[i].CreatedAt;
                totalDays += Math.Max(1, (int)(dt2 - dt1).TotalDays);
            }
            int avgMonths = totalDays / ((ros.Count - 1) * 30);
            if (avgMonths <= 0) avgMonths = 1;
            recDate = refDate.AddMonths(avgMonths);
            mType = MaceType.FrequencyFvx;
        }
        else
        {
            // M2: Chu kỳ tiêu chuẩn sau 6 tháng
            recDate = refDate.AddMonths(6);
            mType = MaceType.Standard6Months;
        }

        // Tính mốc km bảo dưỡng kế tiếp: mốc 5.000km chuẩn (5k, 10k, 15k, 20k...)
        int lastKm = currentOdometer ?? (ros.Count > 0 ? ros.Last().Odometer : 0);
        int nextKm = ((lastKm / 5000) + 1) * 5000;
        if (nextKm <= lastKm) nextKm = lastKm + 5000;

        return (recDate, mType, nextKm);
    }

    // --- Stock Adjustments & Transfer (Ser_Inv_StockAdj & Ser_Inv_StockAdjDetail) ---
    public async Task<List<StockAdj>> StockAdjsAsync(StockAdjStatus? status, StockAdjType? type, string? q, DateTime? fromDate, DateTime? toDate)
    {
        var query = db.StockAdjs
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .AsQueryable();

        if (status.HasValue) query = query.Where(s => s.Status == status.Value);
        if (type.HasValue) query = query.Where(s => s.Type == type.Value);
        if (fromDate.HasValue) query = query.Where(s => s.StockAdjDate >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(s => s.StockAdjDate <= toDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(s => s.StockAdjNo.ToLower().Contains(term)
                || (s.Remark != null && s.Remark.ToLower().Contains(term))
                || s.StorageCode.ToLower().Contains(term)
                || s.CreatedBy.ToLower().Contains(term)
                || s.Items.Any(i => i.PartCode.ToLower().Contains(term) || i.PartName.ToLower().Contains(term)));
        }

        return await query.OrderByDescending(s => s.StockAdjDate).ThenByDescending(s => s.CreatedAt).ToListAsync();
    }

    public Task<StockAdj?> GetStockAdjAsync(int id) =>
        db.StockAdjs
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<int> CreateStockAdjAsync(StockAdj adj, List<StockAdjDetail> items)
    {
        if (items == null || items.Count == 0)
            throw new InvalidOperationException("Vui lòng chọn ít nhất một phụ tùng để kiểm kê / điều chuyển.");

        if (string.IsNullOrWhiteSpace(adj.StockAdjNo))
        {
            var count = await db.StockAdjs.CountAsync() + 1;
            var prefix = adj.Type switch
            {
                StockAdjType.LocationTransfer => "DC",
                StockAdjType.DamageScrap => "HH",
                _ => "KK"
            };
            adj.StockAdjNo = $"{prefix}{DateTime.Today:yyMMdd}-{count:D3}";
        }
        else
        {
            adj.StockAdjNo = adj.StockAdjNo.Trim().ToUpperInvariant();
        }

        var partIds = items.Select(i => i.PartId).Distinct().ToList();
        var parts = await db.Parts.Where(p => partIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

        foreach (var itm in items)
        {
            if (parts.TryGetValue(itm.PartId, out var part))
            {
                itm.PartCode = part.Code;
                itm.PartName = part.Name;
                itm.Unit = part.Unit;
                itm.CostPrice = part.CostPrice;
                itm.SystemQuantity = part.InStock;
                itm.FromLocation = part.Location;
                if (string.IsNullOrWhiteSpace(itm.ToLocation)) itm.ToLocation = part.Location;
            }
            adj.Items.Add(itm);
        }

        adj.CreatedAt = DateTime.Now;
        db.StockAdjs.Add(adj);
        await db.SaveChangesAsync();
        return adj.Id;
    }

    public async Task<(bool ok, string msg)> TransitionStockAdjStatusAsync(int id, StockAdjStatus to, string? approvedBy = null, string? note = null)
    {
        var adj = await db.StockAdjs.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == id);
        if (adj == null) return (false, "Không tìm thấy phiếu kiểm kê kho.");

        var allowed = AllowedNextStockAdj(adj.Status);
        if (!allowed.Contains(to))
            return (false, $"Không thể chuyển trạng thái từ '{Ui.StockAdjStatus(adj.Status).text}' sang '{Ui.StockAdjStatus(to).text}'.");

        adj.Status = to;
        if (!string.IsNullOrWhiteSpace(note))
        {
            adj.Remark = string.IsNullOrWhiteSpace(adj.Remark) ? note.Trim() : $"{adj.Remark} | {note.Trim()}";
        }

        if (to == StockAdjStatus.Executing)
        {
            // Bắt đầu quá trình kiểm đếm
        }
        else if (to == StockAdjStatus.Finished)
        {
            adj.FinishedAt = DateTime.Now;
            adj.ApprovedBy = !string.IsNullOrWhiteSpace(approvedBy) ? approvedBy.Trim() : "Thủ kho trưởng";

            // Cập nhật tồn kho hoặc vị trí theo loại điều chỉnh (Luật Ser_Inv_StockAdj idn.CarService)
            var partIds = adj.Items.Select(i => i.PartId).Distinct().ToList();
            var parts = await db.Parts.Where(p => partIds.Contains(p.Id)).ToListAsync();

            foreach (var item in adj.Items)
            {
                var part = parts.FirstOrDefault(p => p.Id == item.PartId);
                if (part == null) continue;

                if (adj.Type == StockAdjType.CountBalance)
                {
                    // Cân đối kho: Cập nhật tồn kho hệ thống bằng số kiểm đếm thực tế
                    part.InStock = Math.Max(0, item.ActualQuantity);
                }
                else if (adj.Type == StockAdjType.LocationTransfer)
                {
                    // Điều chuyển vị trí lưu kho kệ A -> kệ B
                    if (!string.IsNullOrWhiteSpace(item.ToLocation))
                        part.Location = item.ToLocation.Trim();
                }
                else if (adj.Type == StockAdjType.DamageScrap)
                {
                    // Xuất hủy hàng hao hụt / hư hỏng: Giảm số lượng thực tế trong kho
                    part.InStock = Math.Max(0, part.InStock - item.ActualQuantity);
                }
            }
        }
        else if (to == StockAdjStatus.Rejected)
        {
            // Phiếu bị hủy, không làm thay đổi tồn kho
        }

        await db.SaveChangesAsync();
        return (true, $"Đã chuyển trạng thái phiếu '{adj.StockAdjNo}' sang '{Ui.StockAdjStatus(to).text}'.");
    }

    public async Task<(bool ok, string msg)> UpdateStockAdjItemsAsync(int id, List<(int itemId, decimal actualQty, string? toLoc, string? note)> updates)
    {
        var adj = await db.StockAdjs.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == id);
        if (adj == null) return (false, "Không tìm thấy phiếu kiểm kê kho.");
        if (adj.Status == StockAdjStatus.Finished || adj.Status == StockAdjStatus.Rejected)
            return (false, "Không thể cập nhật phiếu kiểm kê đã hoàn tất hoặc đã hủy.");

        foreach (var (itemId, actualQty, toLoc, note) in updates)
        {
            var item = adj.Items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                item.ActualQuantity = Math.Max(0, actualQty);
                if (!string.IsNullOrWhiteSpace(toLoc)) item.ToLocation = toLoc.Trim();
                if (note != null) item.Note = note.Trim();
            }
        }

        await db.SaveChangesAsync();
        return (true, "Đã lưu cập nhật kết quả kiểm đếm thực tế.");
    }

    public async Task<(bool ok, string msg)> DeleteStockAdjAsync(int id)
    {
        var adj = await db.StockAdjs.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == id);
        if (adj == null) return (false, "Không tìm thấy phiếu kiểm kê kho.");
        if (adj.Status == StockAdjStatus.Finished)
            return (false, "Không thể xóa phiếu kiểm kê đã hoàn tất và chốt tồn kho.");

        db.StockAdjDetails.RemoveRange(adj.Items);
        db.StockAdjs.Remove(adj);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa phiếu kiểm kê {adj.StockAdjNo}.");
    }

    public Task<List<Part>> PartsForStockAdjAsync(string? q = null)
    {
        var query = db.Parts.Where(p => p.IsActive).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p => p.Code.ToLower().Contains(term) || p.Name.ToLower().Contains(term));
        }
        return query.OrderBy(p => p.Code).ToListAsync();
    }

    // --- Technical Service Bulletins & Recall Campaigns (Btl_Bulletin, Btl_BulletinDtl, Btl_Bulletin_VIN) ---
    public async Task<List<Bulletin>> BulletinsAsync(BulletinStatus? status, string? q, bool? activeOnly = null)
    {
        var query = db.Bulletins
            .Include(b => b.Items).ThenInclude(i => i.Part)
            .Include(b => b.TargetVins)
            .Include(b => b.AppliedROs)
            .AsQueryable();

        if (status.HasValue) query = query.Where(b => b.Status == status.Value);
        if (activeOnly == true) query = query.Where(b => b.IsActive && b.Status == BulletinStatus.Active);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(b => b.BulletinNo.ToLower().Contains(term)
                || (b.BulletinNoHMC != null && b.BulletinNoHMC.ToLower().Contains(term))
                || b.Title.ToLower().Contains(term)
                || (b.Remark != null && b.Remark.ToLower().Contains(term))
                || (b.Solution != null && b.Solution.ToLower().Contains(term)));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(b => b.CreateDate).ThenByDescending(b => b.Id).ToList();
    }

    public Task<Bulletin?> GetBulletinAsync(int id) =>
        db.Bulletins
            .Include(b => b.Items).ThenInclude(i => i.Part)
            .Include(b => b.TargetVins).ThenInclude(v => v.RO).ThenInclude(r => r!.Car)
            .Include(b => b.AppliedROs).ThenInclude(r => r.Car)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<int> CreateBulletinAsync(Bulletin bulletin, List<BulletinDetail> details, List<BulletinVin> vins)
    {
        if (string.IsNullOrWhiteSpace(bulletin.Title))
            throw new InvalidOperationException("Vui lòng nhập tiêu đề Bản tin kỹ thuật.");

        if (string.IsNullOrWhiteSpace(bulletin.BulletinNo))
        {
            var count = await db.Bulletins.CountAsync();
            bulletin.BulletinNo = $"TSB-{DateTime.Today:yyMMdd}-{count + 1:D3}";
        }

        bulletin.CreatedAt = DateTime.Now;
        bulletin.Items = details;
        bulletin.TargetVins = vins;

        db.Bulletins.Add(bulletin);
        await db.SaveChangesAsync();
        return bulletin.Id;
    }

    public async Task<(bool ok, string msg)> ToggleBulletinActiveAsync(int id)
    {
        var b = await db.Bulletins.FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return (false, "Không tìm thấy bản tin kỹ thuật.");

        b.IsActive = !b.IsActive;
        await db.SaveChangesAsync();
        return (true, b.IsActive ? $"Đã kích hoạt bản tin {b.BulletinNo}." : $"Đã tạm dừng bản tin {b.BulletinNo}.");
    }

    public async Task<(bool ok, string msg)> TransitionBulletinStatusAsync(int id, BulletinStatus to)
    {
        var b = await db.Bulletins.FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return (false, "Không tìm thấy bản tin kỹ thuật.");

        var allowed = AllowedNextBulletin(b.Status);
        if (!allowed.Contains(to))
            return (false, $"Không thể chuyển trạng thái từ '{Ui.BulletinStatus(b.Status).text}' sang '{Ui.BulletinStatus(to).text}'.");

        b.Status = to;
        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật trạng thái bản tin {b.BulletinNo} sang '{Ui.BulletinStatus(to).text}'.");
    }

    public async Task<(bool ok, string msg)> DeleteBulletinAsync(int id)
    {
        var b = await db.Bulletins
            .Include(x => x.Items)
            .Include(x => x.TargetVins)
            .Include(x => x.AppliedROs)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (b == null) return (false, "Không tìm thấy bản tin kỹ thuật.");
        if (b.AppliedROs.Any())
            return (false, "Không thể xóa bản tin đã áp dụng vào Lệnh sửa chữa RO.");

        db.BulletinDetails.RemoveRange(b.Items);
        db.BulletinVins.RemoveRange(b.TargetVins);
        db.Bulletins.Remove(b);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa bản tin kỹ thuật {b.BulletinNo}.");
    }

    public async Task<List<BulletinVin>> CheckVinBulletinsAsync(string vin)
    {
        if (string.IsNullOrWhiteSpace(vin)) return [];
        var cleanVin = vin.Trim().ToUpperInvariant();

        return await db.BulletinVins
            .Include(v => v.Bulletin).ThenInclude(b => b.Items).ThenInclude(i => i.Part)
            .Include(v => v.RO)
            .Where(v => v.VinNo.ToUpper() == cleanVin && v.Bulletin.IsActive)
            .OrderByDescending(v => v.Bulletin.CreateDate)
            .ToListAsync();
    }

    public async Task<(bool ok, string msg)> UpdateBulletinVinStatusAsync(int vinId, BulletinVinStatus status, string? doneBy = null, int? roId = null, string? roNo = null)
    {
        var vin = await db.BulletinVins.Include(v => v.Bulletin).FirstOrDefaultAsync(v => v.Id == vinId);
        if (vin == null) return (false, "Không tìm thấy số khung xe trong bản tin.");

        vin.Status = status;
        if (status == BulletinVinStatus.Completed)
        {
            vin.DateDone = DateTime.Now;
            vin.DoneBy = !string.IsNullOrWhiteSpace(doneBy) ? doneBy.Trim() : "KTV";
            if (roId.HasValue) vin.ROId = roId;
            if (!string.IsNullOrWhiteSpace(roNo)) vin.RONo = roNo.Trim();
        }
        else
        {
            vin.DateDone = null;
            vin.DoneBy = null;
            vin.ROId = null;
            vin.RONo = null;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật trạng thái VIN {vin.VinNo} sang '{Ui.BulletinVinStatus(status).text}'.");
    }

    public async Task<(bool ok, string msg, int itemsAdded)> ApplyBulletinToROAsync(int bulletinId, int roId)
    {
        var b = await db.Bulletins.Include(x => x.Items).ThenInclude(i => i.Part).FirstOrDefaultAsync(x => x.Id == bulletinId);
        if (b == null) return (false, "Không tìm thấy bản tin kỹ thuật.", 0);
        if (!b.IsActive) return (false, "Bản tin kỹ thuật hiện đang tạm dừng, không thể áp dụng.", 0);

        var ro = await db.ROs.Include(r => r.Lines).Include(r => r.Car).FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy Lệnh sửa chữa RO.", 0);
        if (ro.Status is ROStatus.Finished or ROStatus.Rejected or ROStatus.NotResponding)
            return (false, "Không thể thêm hạng mục vào RO đã đóng hoặc đã hủy.", 0);

        ro.BulletinId = b.Id;

        int addedCount = 0;
        foreach (var item in b.Items)
        {
            var exists = ro.Lines.Any(l => l.Name == item.Name && l.Type == item.Type);
            if (!exists)
            {
                ro.Lines.Add(new RepairLine
                {
                    Type = item.Type,
                    ExpenseType = ExpenseType.Warranty, // Bảo hành hãng HTC chi trả 100%
                    Name = item.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    PartId = item.PartId
                });
                addedCount++;
            }
        }

        // Cập nhật trạng thái số VIN nếu trùng với xe của RO
        if (!string.IsNullOrWhiteSpace(ro.Car?.Vin))
        {
            var vinItem = await db.BulletinVins.FirstOrDefaultAsync(v => v.BulletinId == bulletinId && v.VinNo.ToUpper() == ro.Car.Vin.ToUpper());
            if (vinItem != null)
            {
                vinItem.ROId = ro.Id;
                vinItem.RONo = ro.Code;
            }
        }

        await db.SaveChangesAsync();
        return (true, $"Đã nạp {addedCount} hạng mục từ bản tin kỹ thuật '{b.BulletinNo}' vào Lệnh RO {ro.Code} (Loại bảo hành hãng).", addedCount);
    }

    public async Task<(bool ok, string msg)> AddVinsToBulletinAsync(int bulletinId, List<string> vinList, string? model = null, string? dealerCode = null)
    {
        var b = await db.Bulletins.Include(x => x.TargetVins).FirstOrDefaultAsync(x => x.Id == bulletinId);
        if (b == null) return (false, "Không tìm thấy bản tin kỹ thuật.");

        int count = 0;
        foreach (var rawVin in vinList)
        {
            var clean = rawVin?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(clean)) continue;
            if (b.TargetVins.Any(v => v.VinNo.Equals(clean, StringComparison.OrdinalIgnoreCase))) continue;

            // Tìm thông tin xe trong hệ thống nếu có
            var car = await db.Cars.FirstOrDefaultAsync(c => c.Vin != null && c.Vin.ToUpper() == clean);

            b.TargetVins.Add(new BulletinVin
            {
                VinNo = clean,
                PlateNo = car?.Plate,
                Model = !string.IsNullOrWhiteSpace(model) ? model : car?.Model,
                DealerCode = !string.IsNullOrWhiteSpace(dealerCode) ? dealerCode : "HYUNDAI-MAIN",
                Status = BulletinVinStatus.Pending
            });
            count++;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã bổ sung {count} số khung VIN vào bản tin kỹ thuật {b.BulletinNo}.");
    }

    // --- Pre-Delivery Inspection (PDI - Dlr_PDIRequest & Dlr_PDIRequestDtl) ---
    public async Task<List<PdiRequest>> PdiRequestsAsync(PdiRequestStatus? status, string? q, DateTime? fromDate, DateTime? toDate)
    {
        var query = db.PdiRequests
            .Include(p => p.Items).ThenInclude(i => i.ChecklistItems)
            .Include(p => p.Items).ThenInclude(i => i.RO)
            .AsQueryable();

        if (status.HasValue) query = query.Where(p => p.Status == status.Value);
        if (fromDate.HasValue) query = query.Where(p => p.CreatedDate >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(p => p.CreatedDate <= toDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p => p.PdiReqNo.ToLower().Contains(term)
                || (p.Remark != null && p.Remark.ToLower().Contains(term))
                || p.Items.Any(i => i.VIN.ToLower().Contains(term)
                    || i.Model.ToLower().Contains(term)
                    || i.ContractNo.ToLower().Contains(term)
                    || i.CustomerName.ToLower().Contains(term)));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(p => p.CreatedDate).ThenByDescending(p => p.Id).ToList();
    }

    public Task<PdiRequest?> GetPdiRequestAsync(int id) =>
        db.PdiRequests
            .Include(p => p.Items).ThenInclude(i => i.ChecklistItems)
            .Include(p => p.Items).ThenInclude(i => i.RO).ThenInclude(r => r!.Lines)
            .FirstOrDefaultAsync(p => p.Id == id);

    public Task<PdiRequestItem?> GetPdiRequestItemAsync(int itemId) =>
        db.PdiRequestItems
            .Include(i => i.PdiRequest)
            .Include(i => i.ChecklistItems)
            .Include(i => i.RO).ThenInclude(r => r!.Lines)
            .FirstOrDefaultAsync(i => i.Id == itemId);

    public async Task<int> CreatePdiRequestAsync(PdiRequest req, List<PdiRequestItem> items)
    {
        if (items == null || items.Count == 0)
            throw new InvalidOperationException("Phiếu yêu cầu PDI cần ít nhất một xe để kiểm tra xuất xưởng.");

        if (string.IsNullOrWhiteSpace(req.PdiReqNo))
        {
            var count = await db.PdiRequests.CountAsync() + 1;
            req.PdiReqNo = $"PDI{DateTime.Today:yyMMdd}-{count:D3}";
        }

        req.CreatedAt = DateTime.Now;
        req.CreatedDate = req.CreatedDate != default ? req.CreatedDate : DateTime.Today;

        foreach (var item in items)
        {
            item.VIN = item.VIN.Trim().ToUpperInvariant();
            item.Status = PdiItemStatus.Pending;
            if (item.ChecklistItems == null || item.ChecklistItems.Count == 0)
            {
                item.ChecklistItems = GetDefaultPdiChecklist();
            }
        }
        req.Items = items;

        db.PdiRequests.Add(req);
        await db.SaveChangesAsync();
        return req.Id;
    }

    public async Task<(bool ok, string msg)> TransitionPdiRequestStatusAsync(int id, PdiRequestStatus to, string? approvedBy = null)
    {
        var req = await db.PdiRequests.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id);
        if (req == null) return (false, "Không tìm thấy phiếu yêu cầu PDI.");

        var allowed = AllowedNextPdiRequest(req.Status);
        if (!allowed.Contains(to))
            return (false, $"Không thể chuyển từ '{Ui.PdiRequestStatus(req.Status).text}' sang '{Ui.PdiRequestStatus(to).text}'.");

        req.Status = to;
        if (to == PdiRequestStatus.Approved)
        {
            req.ApprovedDate = DateTime.Now;
            req.ApprovedBy = !string.IsNullOrWhiteSpace(approvedBy) ? approvedBy.Trim() : "Quản đốc xưởng";
            foreach (var item in req.Items.Where(i => i.Status == PdiItemStatus.Pending))
            {
                item.Status = PdiItemStatus.InProgress;
            }
        }
        else if (to == PdiRequestStatus.Completed)
        {
            req.FinishedAt = DateTime.Now;
            foreach (var item in req.Items)
            {
                item.Status = PdiItemStatus.Passed;
                item.PassedDate ??= DateTime.Now;
            }
        }

        await db.SaveChangesAsync();
        return (true, $"Đã chuyển trạng thái yêu cầu PDI sang '{Ui.PdiRequestStatus(to).text}'.");
    }

    public async Task<(bool ok, string msg, int? roId)> CreateROFromPdiItemAsync(int itemId, string? technician = null)
    {
        var item = await db.PdiRequestItems
            .Include(i => i.PdiRequest)
            .Include(i => i.RO)
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null) return (false, "Không tìm thấy thông tin xe trong phiếu PDI.", null);
        if (item.ROId.HasValue && item.RO != null)
            return (false, $"Xe đã có Lệnh sửa chữa/kiểm tra PDI số {item.RO.Code}.", item.ROId);

        // Quy trình đồng bộ xe SerCustomerCarSync_FromPDIDL trong 2023.H.CarServices
        var cleanVin = item.VIN.Trim().ToUpperInvariant();
        var car = await db.Cars.Include(c => c.Customer).FirstOrDefaultAsync(c => c.Vin != null && c.Vin.ToUpper() == cleanVin);

        if (car == null)
        {
            Customer? customer = null;
            if (!string.IsNullOrWhiteSpace(item.CustomerPhone))
            {
                customer = await db.Customers.FirstOrDefaultAsync(c => c.Phone == item.CustomerPhone.Trim());
            }
            if (customer == null && !string.IsNullOrWhiteSpace(item.CustomerName))
            {
                customer = await db.Customers.FirstOrDefaultAsync(c => c.Name.ToLower() == item.CustomerName.Trim().ToLower());
            }

            if (customer == null)
            {
                var custCount = await db.Customers.CountAsync() + 1;
                customer = new Customer
                {
                    Code = $"KH-PDI-{custCount:D4}",
                    Name = string.IsNullOrWhiteSpace(item.CustomerName) ? $"Khách hàng {item.Model}" : item.CustomerName.Trim(),
                    Phone = item.CustomerPhone?.Trim(),
                    Email = null
                };
                db.Customers.Add(customer);
                await db.SaveChangesAsync();
            }

            var platePlaceholder = $"MOI-{cleanVin[^6..]}";
            car = new Car
            {
                Plate = platePlaceholder,
                Vin = cleanVin,
                Model = item.Model.Trim(),
                Year = DateTime.Today.Year,
                CustomerId = customer.Id
            };
            db.Cars.Add(car);
            await db.SaveChangesAsync();
        }

        var roCount = await db.ROs.CountAsync() + 1;
        var roCode = $"RO-PDI{DateTime.Today:yyMMdd}-{roCount:D3}";

        var techName = !string.IsNullOrWhiteSpace(technician) ? technician.Trim()
            : (!string.IsNullOrWhiteSpace(item.Inspector) ? item.Inspector.Trim() : "KTV PDI");

        var ro = new RepairOrder
        {
            Code = roCode,
            CarId = car.Id,
            CustomerId = car.CustomerId,
            Status = ROStatus.InGarage,
            Odometer = 10,
            IntakeNote = $"Kiểm tra kỹ thuật xuất xưởng PDI tiêu chuẩn Hyundai + Lắp phụ kiện theo HĐ {item.ContractNo}",
            Technician = techName,
            CreatedBy = "PDI Dispatch",
            CreatedAt = DateTime.Now,
            IntakeAt = DateTime.Now,
            PdiRequestId = item.PdiRequestId,
            PdiReqNo = item.PdiRequest.PdiReqNo
        };

        ro.Lines.Add(new RepairLine
        {
            Type = LineType.Labor,
            Name = "Kiểm tra kỹ thuật xuất xưởng PDI tiêu chuẩn Hyundai (25 điểm)",
            Quantity = 1,
            UnitPrice = 350000,
            ExpenseType = ExpenseType.Internal
        });

        ro.Lines.Add(new RepairLine
        {
            Type = LineType.Labor,
            Name = "Vệ sinh làm sạch & Rửa xe hoàn thiện giao xe mới",
            Quantity = 1,
            UnitPrice = 150000,
            ExpenseType = ExpenseType.Internal
        });

        if (item.FlagAccessory)
        {
            ro.Lines.Add(new RepairLine
            {
                Type = LineType.Labor,
                Name = $"Lắp đặt gói phụ kiện giao xe: {item.AccessoryNote ?? "Dán film & Thảm lót sàn"}",
                Quantity = 1,
                UnitPrice = 450000,
                ExpenseType = ExpenseType.Internal
            });
        }

        db.ROs.Add(ro);
        await db.SaveChangesAsync();

        item.ROId = ro.Id;
        item.RONo = ro.Code;
        item.Status = PdiItemStatus.InProgress;
        item.Inspector = techName;
        item.InspectionDate ??= DateTime.Now;

        if (item.PdiRequest.Status == PdiRequestStatus.Pending)
        {
            item.PdiRequest.Status = PdiRequestStatus.Approved;
            item.PdiRequest.ApprovedDate ??= DateTime.Now;
            item.PdiRequest.ApprovedBy ??= techName;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã tạo Lệnh kiểm tra PDI {ro.Code} cho xe VIN {item.VIN}.", ro.Id);
    }

    public async Task<(bool ok, string msg)> UpdatePdiItemChecklistAsync(int itemId, List<(int checkId, AuditStatus status, string? note)> updates, string? inspector, string? notes)
    {
        var item = await db.PdiRequestItems
            .Include(i => i.ChecklistItems)
            .Include(i => i.PdiRequest)
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null) return (false, "Không tìm thấy thông tin xe trong phiếu PDI.");

        if (!string.IsNullOrWhiteSpace(inspector)) item.Inspector = inspector.Trim();
        if (notes != null) item.InspectionNotes = notes.Trim();
        item.InspectionDate ??= DateTime.Now;

        foreach (var (checkId, status, note) in updates)
        {
            var check = item.ChecklistItems.FirstOrDefault(c => c.Id == checkId);
            if (check != null)
            {
                check.Status = status;
                if (note != null) check.Note = note.Trim();
            }
        }

        if (item.Status == PdiItemStatus.Pending)
        {
            item.Status = PdiItemStatus.InProgress;
        }

        await db.SaveChangesAsync();
        return (true, "Đã lưu cập nhật kết quả checklist kiểm tra PDI.");
    }

    public async Task<(bool ok, string msg)> PassPdiItemAsync(int itemId, string? inspector = null)
    {
        var item = await db.PdiRequestItems
            .Include(i => i.ChecklistItems)
            .Include(i => i.PdiRequest).ThenInclude(p => p.Items)
            .Include(i => i.RO)
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null) return (false, "Không tìm thấy thông tin xe trong phiếu PDI.");

        var issueCount = item.ChecklistItems.Count(c => c.Status == AuditStatus.Replace);
        if (issueCount > 0)
        {
            return (false, $"Còn {issueCount} hạng mục checklist chưa đạt (Cần sửa/thay). Vui lòng khắc phục trước khi nghiệm thu xuất xưởng.");
        }

        item.Status = PdiItemStatus.Passed;
        item.PassedDate = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(inspector)) item.Inspector = inspector.Trim();

        if (item.RO != null && item.RO.Status != ROStatus.Finished)
        {
            item.RO.Status = ROStatus.Finished;
            item.RO.FinishedAt = DateTime.Now;
        }

        var allPassed = item.PdiRequest.Items.All(i => i.Id == item.Id ? true : (i.Status == PdiItemStatus.Passed));
        if (allPassed)
        {
            item.PdiRequest.Status = PdiRequestStatus.Completed;
            item.PdiRequest.FinishedAt = DateTime.Now;
        }

        await db.SaveChangesAsync();
        return (true, $"Nghiệm thu ĐẠT xuất xưởng xe VIN {item.VIN} ({item.Model}). Xe đã sẵn sàng bàn giao cho khách hàng.");
    }

    public async Task<(bool ok, string msg)> DeletePdiRequestAsync(int id)
    {
        var req = await db.PdiRequests
            .Include(p => p.Items).ThenInclude(i => i.ChecklistItems)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (req == null) return (false, "Không tìm thấy phiếu yêu cầu PDI.");
        if (req.Status == PdiRequestStatus.Completed)
            return (false, "Không thể xóa phiếu PDI đã hoàn tất nghiệm thu xuất xưởng.");

        foreach (var item in req.Items)
        {
            db.PdiChecklistItems.RemoveRange(item.ChecklistItems);
        }
        db.PdiRequestItems.RemoveRange(req.Items);
        db.PdiRequests.Remove(req);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa phiếu yêu cầu PDI {req.PdiReqNo}.");
    }

    public List<PdiChecklistItem> GetDefaultPdiChecklist() =>
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

    // --- Order Part Complaints against Supplier TST/HTC (Ser_OrderComplain & Ser_OrderComplainAttachFile) ---
    public async Task<List<OrderComplain>> OrderComplainsAsync(DMSOrderComplainStatus? dmsStatus, TSTOrderComplainStatus? tstStatus, OrderComplainType? type, string? q, DateTime? fromDate, DateTime? toDate)
    {
        var query = db.OrderComplains
            .Include(c => c.Part)
            .Include(c => c.OrderPart)
            .Include(c => c.AttachFiles)
            .AsQueryable();

        if (dmsStatus.HasValue) query = query.Where(c => c.DMSStatus == dmsStatus.Value);
        if (tstStatus.HasValue) query = query.Where(c => c.TSTStatus == tstStatus.Value);
        if (type.HasValue) query = query.Where(c => c.ComplainType == type.Value);
        if (fromDate.HasValue) query = query.Where(c => c.CreatedAt.Date >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(c => c.CreatedAt.Date <= toDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(c => c.OrderComplainNo.ToLower().Contains(term)
                || c.PartCode.ToLower().Contains(term)
                || c.PartName.ToLower().Contains(term)
                || (c.OrderPartNo != null && c.OrderPartNo.ToLower().Contains(term))
                || (c.VIN != null && c.VIN.ToLower().Contains(term))
                || (c.Description != null && c.Description.ToLower().Contains(term))
                || (c.TransportUnit != null && c.TransportUnit.ToLower().Contains(term))
                || (c.RequestOrderNo != null && c.RequestOrderNo.ToLower().Contains(term)));
        }

        var list = await query.ToListAsync();
        return list.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.Id).ToList();
    }

    public Task<OrderComplain?> GetOrderComplainAsync(int id) =>
        db.OrderComplains
            .Include(c => c.Part)
            .Include(c => c.OrderPart).ThenInclude(o => o!.Lines)
            .Include(c => c.AttachFiles)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<int> CreateOrderComplainAsync(OrderComplain complain, List<OrderComplainAttachFile>? files = null)
    {
        if (complain.Quantity <= 0)
            throw new InvalidOperationException("Số lượng khiếu nại phải lớn hơn 0.");

        var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == complain.PartId);
        if (part == null)
            throw new InvalidOperationException("Không tìm thấy phụ tùng khiếu nại.");

        complain.PartCode = part.Code;
        complain.PartName = part.Name;
        complain.Unit = string.IsNullOrWhiteSpace(complain.Unit) ? part.Unit : complain.Unit;
        if (complain.UnitPrice <= 0) complain.UnitPrice = part.CostPrice;

        if (complain.OrderPartId.HasValue && complain.OrderPartId.Value > 0)
        {
            var order = await db.OrderParts.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == complain.OrderPartId.Value);
            if (order != null)
            {
                complain.OrderPartNo = order.OrderPartNo;
                var line = order.Lines.FirstOrDefault(l => l.PartId == complain.PartId || l.PartCode == part.Code);
                if (line != null)
                {
                    if (complain.Quantity > line.Quantity)
                        throw new InvalidOperationException($"Số lượng khiếu nại ({complain.Quantity}) không được vượt quá số lượng trên đơn đặt hàng ({line.Quantity} {line.Unit}).");
                    if (complain.UnitPrice <= 0) complain.UnitPrice = line.UnitPrice;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(complain.OrderComplainNo))
        {
            var count = await db.OrderComplains.CountAsync() + 1;
            complain.OrderComplainNo = $"KN{DateTime.Today:yyMMdd}-{count:D3}";
        }

        complain.CreatedAt = DateTime.Now;
        complain.DMSStatus = DMSOrderComplainStatus.Pending;
        complain.TSTStatus = TSTOrderComplainStatus.Processing;

        if (files != null && files.Count > 0)
        {
            complain.AttachFiles = files;
        }

        db.OrderComplains.Add(complain);
        await db.SaveChangesAsync();
        return complain.Id;
    }

    public async Task<(bool ok, string msg)> SendOrderComplainToTSTAsync(int id)
    {
        var complain = await db.OrderComplains.FirstOrDefaultAsync(c => c.Id == id);
        if (complain == null) return (false, "Không tìm thấy hồ sơ khiếu nại.");

        if (complain.DMSStatus != DMSOrderComplainStatus.Pending)
            return (false, $"Chỉ có thể gửi khiếu nại ở trạng thái Chờ gửi (hiện tại: {Ui.DMSOrderComplainStatus(complain.DMSStatus).text}).");

        complain.DMSStatus = DMSOrderComplainStatus.Sent;
        complain.TSTStatus = TSTOrderComplainStatus.Processing;
        complain.SentAt = DateTime.Now;

        await db.SaveChangesAsync();
        return (true, $"Đã gửi hồ sơ khiếu nại {complain.OrderComplainNo} sang Nhà cung cấp TST/HTC thành công.");
    }

    public async Task<(bool ok, string msg)> ReviewOrderComplainAsync(int id, TSTOrderComplainStatus tstStatus, ComplainSolution solution, string? solutionNote)
    {
        var complain = await db.OrderComplains.Include(c => c.Part).FirstOrDefaultAsync(c => c.Id == id);
        if (complain == null) return (false, "Không tìm thấy hồ sơ khiếu nại.");

        if (complain.DMSStatus != DMSOrderComplainStatus.Sent)
            return (false, $"Chỉ có thể phê duyệt hồ sơ khiếu nại đã gửi sang TST (hiện tại: {Ui.DMSOrderComplainStatus(complain.DMSStatus).text}).");

        complain.TSTStatus = tstStatus;
        complain.TSTSolution = solution;
        complain.SolutionNote = solutionNote?.Trim();
        complain.DecidedAt = DateTime.Now;

        if (tstStatus == TSTOrderComplainStatus.Approved)
        {
            complain.DMSStatus = DMSOrderComplainStatus.Finished;
            complain.FinishedAt = DateTime.Now;

            // Nếu phương án là đổi mới phụ tùng 1:1, tự động tăng tồn kho bù
            if (solution == ComplainSolution.ReplaceNew && complain.Part != null)
            {
                complain.Part.InStock += complain.Quantity;
            }
        }
        else if (tstStatus == TSTOrderComplainStatus.Rejected)
        {
            complain.DMSStatus = DMSOrderComplainStatus.Cancelled;
            complain.FinishedAt = DateTime.Now;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật kết quả thẩm định NCC: {Ui.TSTOrderComplainStatus(tstStatus).text} ({Ui.ComplainSolution(solution).text}).");
    }

    public async Task<(bool ok, string msg)> DeleteOrderComplainAsync(int id)
    {
        var complain = await db.OrderComplains.Include(c => c.AttachFiles).FirstOrDefaultAsync(c => c.Id == id);
        if (complain == null) return (false, "Không tìm thấy hồ sơ khiếu nại.");

        if (complain.DMSStatus != DMSOrderComplainStatus.Pending && complain.DMSStatus != DMSOrderComplainStatus.Cancelled)
            return (false, "Chỉ có thể xóa hồ sơ khiếu nại ở trạng thái Mới tạo (Chờ gửi) hoặc Đã hủy.");

        db.OrderComplainAttachFiles.RemoveRange(complain.AttachFiles);
        db.OrderComplains.Remove(complain);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa hồ sơ khiếu nại {complain.OrderComplainNo}.");
    }

    public Task<List<OrderPart>> OrderPartsForComplainSelectAsync() =>
        db.OrderParts
            .Include(o => o.Lines).ThenInclude(l => l.Part)
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.Id)
            .ToListAsync();

    // --- Technical Library & Re-Repair Knowledge Base (Ser_Technical_Library / MH 63) ---
    public async Task<List<TechnicalLibrary>> TechnicalLibrariesAsync(string? model, TechnicalLibraryReRepairType? reRepairType, TechnicalLibraryType? type, bool? isActive, string? q)
    {
        var query = db.TechnicalLibraries
            .Include(t => t.RO).ThenInclude(r => r!.Car)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(model))
            query = query.Where(t => t.Model.ToLower().Contains(model.Trim().ToLower()));

        if (reRepairType.HasValue)
            query = query.Where(t => t.ReRepairType == reRepairType.Value);

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        if (isActive.HasValue)
            query = query.Where(t => t.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = query.Where(t => t.TechnicalLibraryCode.ToLower().Contains(s)
                || (t.PlateNo != null && t.PlateNo.ToLower().Contains(s))
                || t.Model.ToLower().Contains(s)
                || t.ReRepairRemark.ToLower().Contains(s)
                || t.ReRepairReason.ToLower().Contains(s)
                || t.ReRepairSolution.ToLower().Contains(s));
        }

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public Task<TechnicalLibrary?> GetTechnicalLibraryAsync(int id) =>
        db.TechnicalLibraries
            .Include(t => t.RO).ThenInclude(r => r!.Car)
            .Include(t => t.RO).ThenInclude(r => r!.Customer)
            .FirstOrDefaultAsync(t => t.Id == id);

    public Task<TechnicalLibrary?> GetTechnicalLibraryByCodeAsync(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        return db.TechnicalLibraries
            .Include(t => t.RO).ThenInclude(r => r!.Car)
            .FirstOrDefaultAsync(t => t.TechnicalLibraryCode == clean);
    }

    public async Task<int> CreateTechnicalLibraryAsync(TechnicalLibrary item)
    {
        if (string.IsNullOrWhiteSpace(item.TechnicalLibraryCode))
        {
            var count = await db.TechnicalLibraries.CountAsync() + 1;
            item.TechnicalLibraryCode = $"TLIB{DateTime.Today:yyMMdd}-{count:D3}";
        }
        else
        {
            item.TechnicalLibraryCode = item.TechnicalLibraryCode.Trim().ToUpperInvariant();
        }

        if (string.IsNullOrWhiteSpace(item.DealerCode))
            item.DealerCode = "HYUNDAI-MAIN";
        if (string.IsNullOrWhiteSpace(item.DealerName))
            item.DealerName = "Hyundai Giải Phóng";

        item.Model = item.Model.Trim();
        item.ReRepairRemark = item.ReRepairRemark.Trim();
        item.ReRepairReason = item.ReRepairReason.Trim();
        item.ReRepairSolution = item.ReRepairSolution.Trim();
        item.CreatedAt = DateTime.Now;

        db.TechnicalLibraries.Add(item);
        await db.SaveChangesAsync();
        return item.Id;
    }

    public async Task<(bool ok, string msg)> ApproveTechnicalLibraryAsync(int id, string? approvedBy = null)
    {
        var item = await db.TechnicalLibraries.FirstOrDefaultAsync(t => t.Id == id);
        if (item == null) return (false, "Không tìm thấy hồ sơ kỹ thuật.");

        if (item.IsActive)
            return (false, "Hồ sơ kỹ thuật đã được phê duyệt áp dụng trước đó.");

        item.IsActive = true;
        item.ApprovedAt = DateTime.Now;
        item.ApprovedBy = !string.IsNullOrWhiteSpace(approvedBy) ? approvedBy.Trim() : "Phòng Dịch vụ Kỹ thuật HTC";

        await db.SaveChangesAsync();
        return (true, $"Đã phê duyệt ban hành cẩm nang kỹ thuật {item.TechnicalLibraryCode} ({item.Model}).");
    }

    public async Task<(bool ok, string msg)> DeleteTechnicalLibraryAsync(int id)
    {
        var item = await db.TechnicalLibraries.FirstOrDefaultAsync(t => t.Id == id);
        if (item == null) return (false, "Không tìm thấy hồ sơ kỹ thuật.");

        db.TechnicalLibraries.Remove(item);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa hồ sơ kỹ thuật {item.TechnicalLibraryCode}.");
    }

    public async Task<List<TechnicalLibrary>> SearchSolutionsForRoAsync(int roId)
    {
        var ro = await db.ROs.Include(r => r.Car).FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null || ro.Car == null) return [];

        var modelName = ro.Car.Model.ToLower();
        var intakeWords = (ro.IntakeNote ?? "").ToLower().Split([' ', ',', '.', ';', '-', '/'], StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length >= 3).ToList();

        var candidates = await db.TechnicalLibraries
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.Type)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

        return candidates.Where(t =>
            modelName.Contains(t.Model.ToLower()) || t.Model.ToLower().Contains(modelName) ||
            intakeWords.Any(w => t.ReRepairRemark.ToLower().Contains(w) || t.ReRepairReason.ToLower().Contains(w))
        ).Take(5).ToList();
    }

    public async Task<List<string>> GetDistinctModelsAsync() =>
        await db.TechnicalLibraries
            .Select(t => t.Model)
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync();

    // --- Master Services & Flat Rate Labor Operations (Ser_MST_Service) ---
    public async Task<List<ServiceItem>> ServiceItemsAsync(ServiceROType? roType, string? model, bool? isActive, bool? flagWarranty, string? q)
    {
        var query = db.ServiceItems.Include(s => s.RepairLines).AsQueryable();
        if (roType.HasValue) query = query.Where(s => s.ROType == roType.Value);
        if (!string.IsNullOrWhiteSpace(model))
        {
            var m = model.Trim().ToLower();
            query = query.Where(s => s.Model != null && s.Model.ToLower().Contains(m));
        }
        if (isActive.HasValue) query = query.Where(s => s.IsActive == isActive.Value);
        if (flagWarranty.HasValue) query = query.Where(s => s.FlagWarranty == flagWarranty.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = query.Where(x => x.Code.ToLower().Contains(s) || x.Name.ToLower().Contains(s) || (x.Note != null && x.Note.ToLower().Contains(s)));
        }
        return await query.OrderBy(s => s.ROType).ThenBy(s => s.Code).ToListAsync();
    }

    public Task<ServiceItem?> GetServiceItemAsync(int id) =>
        db.ServiceItems.Include(s => s.RepairLines).ThenInclude(l => l.RO).ThenInclude(r => r.Car)
            .FirstOrDefaultAsync(s => s.Id == id);

    public Task<ServiceItem?> GetServiceItemByCodeAsync(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        return db.ServiceItems.FirstOrDefaultAsync(s => s.Code == clean);
    }

    public async Task<int> CreateServiceItemAsync(ServiceItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Code))
            throw new InvalidOperationException("Vui lòng nhập mã công việc dịch vụ (SerCode).");
        if (string.IsNullOrWhiteSpace(item.Name))
            throw new InvalidOperationException("Vui lòng nhập tên công việc dịch vụ (SerName).");

        item.Code = item.Code.Trim().ToUpperInvariant();
        var exists = await db.ServiceItems.AnyAsync(s => s.Code == item.Code);
        if (exists)
            throw new InvalidOperationException($"Mã công việc {item.Code} đã tồn tại trong danh mục.");

        if (item.StdManHour <= 0) item.StdManHour = 1.0m;
        if (item.Price < 0) item.Price = 0;
        if (item.Cost < 0) item.Cost = 0;
        if (item.VatPercent < 0) item.VatPercent = 8;

        db.ServiceItems.Add(item);
        await db.SaveChangesAsync();
        return item.Id;
    }

    public async Task<(bool ok, string msg)> UpdateServiceItemAsync(ServiceItem item)
    {
        var existing = await db.ServiceItems.FirstOrDefaultAsync(s => s.Id == item.Id);
        if (existing == null) return (false, "Không tìm thấy công việc dịch vụ.");

        existing.Name = item.Name.Trim();
        existing.ROType = item.ROType;
        existing.StdManHour = item.StdManHour > 0 ? item.StdManHour : 1.0m;
        existing.Price = item.Price >= 0 ? item.Price : 0;
        existing.Cost = item.Cost >= 0 ? item.Cost : 0;
        existing.VatPercent = item.VatPercent >= 0 ? item.VatPercent : 8;
        existing.Model = item.Model?.Trim();
        existing.FlagWarranty = item.FlagWarranty;
        existing.Note = item.Note?.Trim();
        existing.IsActive = item.IsActive;

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật công việc {existing.Code} - {existing.Name}.");
    }

    public async Task<(bool ok, string msg)> DeleteServiceItemAsync(int id)
    {
        var existing = await db.ServiceItems.Include(s => s.RepairLines).FirstOrDefaultAsync(s => s.Id == id);
        if (existing == null) return (false, "Không tìm thấy công việc dịch vụ.");
        if (existing.RepairLines.Count > 0)
        {
            existing.IsActive = false;
            await db.SaveChangesAsync();
            return (true, $"Công việc {existing.Code} đã phát sinh {existing.RepairLines.Count} dòng sửa chữa trên RO nên đã chuyển sang trạng thái Tạm dừng.");
        }

        db.ServiceItems.Remove(existing);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa công việc {existing.Code}.");
    }

    public Task<List<ServiceItem>> ServiceItemsForSelectAsync() =>
        db.ServiceItems.Where(s => s.IsActive).OrderBy(s => s.ROType).ThenBy(s => s.Code).ToListAsync();

    public async Task<(bool ok, string msg)> AddServiceItemToROAsync(int roId, int serviceItemId, ExpenseType expenseType, decimal? customHours = null, decimal? customPrice = null, string? note = null)
    {
        var ro = await db.ROs.FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy lệnh sửa chữa RO.");
        if (ro.Status is ROStatus.Finished or ROStatus.Paid or ROStatus.Rejected or ROStatus.NotResponding)
            return (false, "Lệnh sửa chữa RO đã kết thúc — không thể thêm công việc.");

        var svcItem = await db.ServiceItems.FirstOrDefaultAsync(s => s.Id == serviceItemId);
        if (svcItem == null) return (false, "Không tìm thấy công việc dịch vụ chuẩn.");

        var hours = (customHours.HasValue && customHours.Value > 0) ? customHours.Value : (svcItem.StdManHour > 0 ? svcItem.StdManHour : 1.0m);
        var unitPrice = (customPrice.HasValue && customPrice.Value >= 0) ? customPrice.Value : svcItem.Price;

        var line = new RepairLine
        {
            ROId = roId,
            Type = LineType.Labor,
            ExpenseType = expenseType,
            ServiceItemId = svcItem.Id,
            StdManHour = hours,
            Name = !string.IsNullOrWhiteSpace(note) ? $"{svcItem.Name} ({note.Trim()})" : svcItem.Name,
            Quantity = hours,
            UnitPrice = unitPrice
        };

        db.Lines.Add(line);
        await db.SaveChangesAsync();
        return (true, $"Đã nạp công việc [{svcItem.Code}] {svcItem.Name} ({hours} giờ định mức) vào Lệnh sửa chữa {ro.Code}.");
    }

    public Task<List<string>> GetDistinctServiceModelsAsync() =>
        db.ServiceItems.Where(s => !string.IsNullOrEmpty(s.Model))
            .Select(s => s.Model!)
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync();

    // --- Car Model Master (Ser_Mst_Model / Mst_CarModelStd) ---
    public async Task<List<CarModel>> CarModelsAsync(string? tradeMarkCode, CarModelSegment? segment, bool? isActive, string? q)
    {
        var query = db.CarModels.AsQueryable();
        if (!string.IsNullOrWhiteSpace(tradeMarkCode))
        {
            var tm = tradeMarkCode.Trim().ToUpperInvariant();
            query = query.Where(m => m.TradeMarkCode == tm);
        }
        if (segment.HasValue) query = query.Where(m => m.Segment == segment.Value);
        if (isActive.HasValue) query = query.Where(m => m.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = query.Where(m => m.ModelCode.ToLower().Contains(s)
                || m.ModelName.ToLower().Contains(s)
                || m.TradeMarkCode.ToLower().Contains(s)
                || (m.ProductionCode != null && m.ProductionCode.ToLower().Contains(s)));
        }
        return await query.OrderBy(m => m.TradeMarkCode).ThenBy(m => m.ModelCode).ToListAsync();
    }

    public Task<CarModel?> GetCarModelAsync(int id) =>
        db.CarModels.FirstOrDefaultAsync(m => m.Id == id);

    public Task<CarModel?> GetCarModelByCodeAsync(string modelCode)
    {
        var clean = modelCode.Trim().ToUpperInvariant();
        return db.CarModels.FirstOrDefaultAsync(m => m.ModelCode == clean);
    }

    public async Task<int> CreateCarModelAsync(CarModel model)
    {
        if (string.IsNullOrWhiteSpace(model.ModelCode))
            throw new InvalidOperationException("Vui lòng nhập mã dòng xe (ModelCode).");
        if (string.IsNullOrWhiteSpace(model.ModelName))
            throw new InvalidOperationException("Vui lòng nhập tên dòng xe (ModelName).");
        if (string.IsNullOrWhiteSpace(model.TradeMarkCode))
            throw new InvalidOperationException("Vui lòng nhập mã thương hiệu (TradeMarkCode).");

        model.ModelCode = model.ModelCode.Trim().ToUpperInvariant();
        model.TradeMarkCode = model.TradeMarkCode.Trim().ToUpperInvariant();
        model.ModelName = model.ModelName.Trim();

        var exists = await db.CarModels.AnyAsync(m => m.ModelCode == model.ModelCode);
        if (exists)
            throw new InvalidOperationException($"Mã dòng xe {model.ModelCode} đã tồn tại trong danh mục.");

        model.CreatedAt = DateTime.Now;
        model.LogLUDateTime = DateTime.Now;
        model.LogLUBy = model.CreatedBy;
        db.CarModels.Add(model);
        await db.SaveChangesAsync();
        return model.Id;
    }

    public async Task<(bool ok, string msg)> UpdateCarModelAsync(CarModel model)
    {
        var existing = await db.CarModels.FirstOrDefaultAsync(m => m.Id == model.Id);
        if (existing == null) return (false, "Không tìm thấy dòng xe.");

        if (string.IsNullOrWhiteSpace(model.ModelName))
            return (false, "Tên dòng xe không được để trống.");

        existing.ModelName = model.ModelName.Trim();
        existing.TradeMarkCode = string.IsNullOrWhiteSpace(model.TradeMarkCode) ? existing.TradeMarkCode : model.TradeMarkCode.Trim().ToUpperInvariant();
        existing.ProductionCode = model.ProductionCode?.Trim();
        existing.DealerCode = model.DealerCode?.Trim();
        existing.Segment = model.Segment;
        existing.ProductYear = model.ProductYear;
        existing.IsActive = model.IsActive;
        existing.LogLUBy = model.LogLUBy ?? "web";
        existing.LogLUDateTime = DateTime.Now;

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật dòng xe {existing.ModelCode} - {existing.ModelName}.");
    }

    public async Task<(bool ok, string msg)> DeleteCarModelAsync(int id)
    {
        var existing = await db.CarModels.FirstOrDefaultAsync(m => m.Id == id);
        if (existing == null) return (false, "Không tìm thấy dòng xe.");

        // Kiểm tra dòng xe đã được dùng bởi xe trong hệ thống (Ser_Car.Model) chưa.
        var modelName = existing.ModelName;
        var inUse = await db.Cars.AnyAsync(c => c.Model == modelName);
        if (inUse)
        {
            existing.IsActive = false;
            existing.LogLUDateTime = DateTime.Now;
            await db.SaveChangesAsync();
            return (true, $"Dòng xe {existing.ModelCode} đã được gán cho xe trong hệ thống nên đã chuyển sang trạng thái Tạm dừng.");
        }

        db.CarModels.Remove(existing);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa dòng xe {existing.ModelCode}.");
    }

    public async Task<CarModelSummaryDto> GetCarModelSummaryAsync()
    {
        var all = await db.CarModels.ToListAsync();
        return new CarModelSummaryDto
        {
            TotalModels = all.Count,
            ActiveModels = all.Count(m => m.IsActive),
            InactiveModels = all.Count(m => !m.IsActive),
            TradeMarkCount = all.Select(m => m.TradeMarkCode).Distinct().Count(),
            SegmentCount = all.Select(m => m.Segment).Distinct().Count()
        };
    }

    public Task<List<string>> GetDistinctTradeMarksAsync() =>
        db.CarModels.Select(m => m.TradeMarkCode)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();

    // --- Supplier Management & Return Parts to Supplier (Ser_Mst_Supplier, Ser_SupplierPayment) ---
    public async Task<List<Supplier>> SuppliersAsync(string? q)
    {
        var query = db.Suppliers.Include(s => s.SupplierPayments).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = query.Where(x => x.Code.ToLower().Contains(s)
                || x.Name.ToLower().Contains(s)
                || (x.Phone != null && x.Phone.ToLower().Contains(s))
                || (x.ContactName != null && x.ContactName.ToLower().Contains(s)));
        }
        return await query.OrderBy(s => s.Name).ToListAsync();
    }

    public Task<Supplier?> GetSupplierAsync(int id) =>
        db.Suppliers.Include(s => s.SupplierPayments).FirstOrDefaultAsync(s => s.Id == id);

    public async Task<int> CreateSupplierAsync(Supplier supplier)
    {
        if (string.IsNullOrWhiteSpace(supplier.Code))
            throw new InvalidOperationException("Vui lòng nhập mã nhà cung cấp (SupplierCode).");
        if (string.IsNullOrWhiteSpace(supplier.Name))
            throw new InvalidOperationException("Vui lòng nhập tên nhà cung cấp (SupplierName).");

        supplier.Code = supplier.Code.Trim().ToUpperInvariant();
        supplier.Name = supplier.Name.Trim();
        var exists = await db.Suppliers.AnyAsync(s => s.Code == supplier.Code);
        if (exists)
            throw new InvalidOperationException($"Mã nhà cung cấp {supplier.Code} đã tồn tại trong danh mục.");

        supplier.CreatedAt = DateTime.Now;
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync();
        return supplier.Id;
    }

    public async Task<List<SupplierPayment>> SupplierPaymentsAsync(SupplierPaymentStatus? status, SupplierPaymentType? type, string? q, DateTime? fromDate, DateTime? toDate)
    {
        var query = db.SupplierPayments
            .Include(p => p.Supplier)
            .Include(p => p.OrderPart)
            .Include(p => p.Items).ThenInclude(i => i.Part)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (type.HasValue)
            query = query.Where(p => p.PaymentType == type.Value);

        if (fromDate.HasValue)
            query = query.Where(p => p.PaymentDate >= fromDate.Value.Date);

        if (toDate.HasValue)
            query = query.Where(p => p.PaymentDate <= toDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = query.Where(p => p.SupplierPaymentNo.ToLower().Contains(s)
                || p.SupplierName.ToLower().Contains(s)
                || (p.OrderPartNo != null && p.OrderPartNo.ToLower().Contains(s))
                || (p.TSTRequestNo != null && p.TSTRequestNo.ToLower().Contains(s))
                || (p.Description != null && p.Description.ToLower().Contains(s))
                || p.Items.Any(i => i.Part.Code.ToLower().Contains(s) || i.Part.Name.ToLower().Contains(s)));
        }

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public Task<SupplierPayment?> GetSupplierPaymentAsync(int id) =>
        db.SupplierPayments
            .Include(p => p.Supplier)
            .Include(p => p.OrderPart)
            .Include(p => p.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(p => p.Id == id);

    public Task<SupplierPayment?> GetSupplierPaymentByNoAsync(string supplierPaymentNo)
    {
        var clean = supplierPaymentNo.Trim().ToUpperInvariant();
        return db.SupplierPayments
            .Include(p => p.Supplier)
            .Include(p => p.OrderPart)
            .Include(p => p.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(p => p.SupplierPaymentNo == clean);
    }

    public async Task<int> CreateSupplierPaymentAsync(SupplierPayment payment, List<SupplierPaymentDetail> items)
    {
        if (items == null || items.Count == 0)
            throw new InvalidOperationException("Phiếu xuất trả NCC cần ít nhất một phụ tùng (Ser_SupplierPaymentDtl).");

        if (string.IsNullOrWhiteSpace(payment.SupplierPaymentNo))
        {
            var today = DateTime.Today;
            var prefix = $"PXNCC-{today:yyMMdd}-";
            var count = await db.SupplierPayments.CountAsync(p => p.SupplierPaymentNo.StartsWith(prefix)) + 1;
            payment.SupplierPaymentNo = $"{prefix}{count:D3}";
        }
        else
        {
            payment.SupplierPaymentNo = payment.SupplierPaymentNo.Trim().ToUpperInvariant();
            var exists = await db.SupplierPayments.AnyAsync(p => p.SupplierPaymentNo == payment.SupplierPaymentNo);
            if (exists)
                throw new InvalidOperationException($"Số phiếu xuất trả {payment.SupplierPaymentNo} đã tồn tại.");
        }

        // Link Supplier metadata
        if (payment.SupplierId.HasValue && payment.SupplierId.Value > 0)
        {
            var sup = await db.Suppliers.FirstOrDefaultAsync(s => s.Id == payment.SupplierId.Value);
            if (sup != null)
            {
                payment.SupplierName = sup.Name;
                payment.Address = sup.Address;
            }
        }
        if (string.IsNullOrWhiteSpace(payment.SupplierName))
            throw new InvalidOperationException("Vui lòng chỉ định Nhà cung cấp tiếp nhận phụ tùng xuất trả.");

        // Link OrderPart if provided
        if (payment.OrderPartId.HasValue && payment.OrderPartId.Value > 0)
        {
            var op = await db.OrderParts.FirstOrDefaultAsync(o => o.Id == payment.OrderPartId.Value);
            if (op != null)
            {
                payment.OrderPartNo = op.OrderPartNo;
            }
        }

        payment.PaymentDate = payment.PaymentDate == default ? DateTime.Today : payment.PaymentDate;
        payment.Status = SupplierPaymentStatus.Pending;
        payment.CreatedAt = DateTime.Now;

        // Validate items and check stock availability
        var partIds = items.Select(i => i.PartId).Distinct().ToList();
        var parts = await db.Parts.Where(p => partIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

        foreach (var item in items)
        {
            if (item.QtyPay <= 0)
                throw new InvalidOperationException("Số lượng phụ tùng xuất trả phải lớn hơn 0.");

            if (!parts.TryGetValue(item.PartId, out var part))
                throw new InvalidOperationException($"Không tìm thấy phụ tùng với ID {item.PartId}.");

            item.QtyInventory = part.InStock;
            if (item.QtyPay > part.InStock)
            {
                throw new InvalidOperationException($"Số lượng trả phụ tùng [{part.Code}] {part.Name} ({item.QtyPay:N0}) vượt quá tồn kho thực tế ({part.InStock:N0}).");
            }

            if (item.Price <= 0)
                item.Price = part.CostPrice > 0 ? part.CostPrice : part.SalePrice;

            if (item.VatPercent < 0)
                item.VatPercent = 10;

            if (string.IsNullOrWhiteSpace(item.LocationCode))
                item.LocationCode = part.Location;

            payment.Items.Add(item);
        }

        db.SupplierPayments.Add(payment);
        await db.SaveChangesAsync();
        return payment.Id;
    }

    public async Task<(bool ok, string msg)> ApproveSupplierPaymentAsync(int id, string? approvedBy = null)
    {
        var payment = await db.SupplierPayments
            .Include(p => p.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null) return (false, "Không tìm thấy phiếu xuất trả nhà cung cấp.");
        if (payment.Status != SupplierPaymentStatus.Pending)
            return (false, "Chỉ có thể phê duyệt phiếu xuất trả ở trạng thái Chờ duyệt.");

        if (payment.Items.Count == 0)
            return (false, "Phiếu xuất trả không có phụ tùng để xuất kho.");

        // Check stock balance before deduction (Ser_SupplierPayment_Appr_Input_QtyInventoryNotEnough)
        foreach (var line in payment.Items)
        {
            if (line.Part.InStock < line.QtyPay)
            {
                return (false, $"Tồn kho phụ tùng [{line.Part.Code}] {line.Part.Name} không đủ để xuất trả (Tồn: {line.Part.InStock:N0}, Cần xuất: {line.QtyPay:N0}).");
            }
        }

        // Deduct inventory
        foreach (var line in payment.Items)
        {
            line.Part.InStock -= line.QtyPay;
        }

        payment.Status = SupplierPaymentStatus.Approved;
        payment.ApprovedAt = DateTime.Now;
        payment.ApprovedBy = !string.IsNullOrWhiteSpace(approvedBy) ? approvedBy.Trim() : "Thủ kho trưởng";

        await db.SaveChangesAsync();
        return (true, $"Đã phê duyệt xuất kho trả hàng phiếu {payment.SupplierPaymentNo}. Đã trừ tồn kho {payment.TotalQuantity:N0} phụ tùng với tổng giá trị {payment.TotalAmount:N0} đ.");
    }

    public async Task<(bool ok, string msg)> CancelSupplierPaymentAsync(int id)
    {
        var payment = await db.SupplierPayments.FirstOrDefaultAsync(p => p.Id == id);
        if (payment == null) return (false, "Không tìm thấy phiếu xuất trả nhà cung cấp.");
        if (payment.Status != SupplierPaymentStatus.Pending)
            return (false, "Chỉ có thể hủy phiếu xuất trả ở trạng thái Chờ duyệt.");

        payment.Status = SupplierPaymentStatus.Cancelled;
        await db.SaveChangesAsync();
        return (true, $"Đã hủy phiếu xuất trả {payment.SupplierPaymentNo}.");
    }

    public async Task<(bool ok, string msg)> DeleteSupplierPaymentAsync(int id)
    {
        var payment = await db.SupplierPayments.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id);
        if (payment == null) return (false, "Không tìm thấy phiếu xuất trả nhà cung cấp.");
        if (payment.Status == SupplierPaymentStatus.Approved)
            return (false, "Phiếu xuất trả đã duyệt xuất kho không thể xóa (vui lòng kiểm tra sổ kho).");

        db.SupplierPayments.Remove(payment);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa phiếu xuất trả {payment.SupplierPaymentNo}.");
    }

    public Task<List<Part>> PartsForSupplierPaymentAsync() =>
        db.Parts.Where(p => p.InStock > 0).OrderBy(p => p.Code).ToListAsync();

    public Task<List<StockIn>> StockInsForSupplierPaymentAsync() =>
        db.StockIns.Where(s => s.Status == StockInStatus.Finished).OrderByDescending(s => s.StockInDate).Take(25).ToListAsync();

    public Task<List<OrderPart>> OrderPartsForSupplierPaymentAsync() =>
        db.OrderParts.OrderByDescending(o => o.OrderDate).Take(25).ToListAsync();

    // --- StockOutOrder: Quản lý Yêu cầu xuất kho dịch vụ (Ser_Inv_StockOutOrder - MH 125) ---
    public async Task<List<StockOutOrder>> StockOutOrdersAsync(StockOutOrderStatus? status, StockOutOrderPriority? priority, string? q, DateTime? fromDate, DateTime? toDate, int? roId)
    {
        var query = db.StockOutOrders
            .Include(o => o.RO)
            .Include(o => o.Car)
            .Include(o => o.Customer)
            .Include(o => o.Cavity)
            .Include(o => o.StockOut)
            .Include(o => o.Items).ThenInclude(i => i.Part)
            .AsQueryable();

        if (status.HasValue) query = query.Where(o => o.Status == status.Value);
        if (priority.HasValue) query = query.Where(o => o.Priority == priority.Value);
        if (roId.HasValue && roId.Value > 0) query = query.Where(o => o.ROId == roId.Value);
        if (fromDate.HasValue) query = query.Where(o => o.OrderDate >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(o => o.OrderDate <= toDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = query.Where(o => o.OrderNo.ToLower().Contains(s)
                || (o.RO != null && o.RO.Code.ToLower().Contains(s))
                || (o.Car != null && o.Car.Plate.ToLower().Contains(s))
                || (o.Customer != null && o.Customer.Name.ToLower().Contains(s))
                || (o.RequesterName != null && o.RequesterName.ToLower().Contains(s))
                || (o.Description != null && o.Description.ToLower().Contains(s))
                || o.Items.Any(i => i.PartCode.ToLower().Contains(s) || i.PartName.ToLower().Contains(s)));
        }

        return await query.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.CreatedAt).ToListAsync();
    }

    public Task<StockOutOrder?> GetStockOutOrderAsync(int id) =>
        db.StockOutOrders
            .Include(o => o.RO)
            .Include(o => o.Car)
            .Include(o => o.Customer)
            .Include(o => o.Cavity)
            .Include(o => o.StockOut)
            .Include(o => o.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(o => o.Id == id);

    public Task<StockOutOrder?> GetStockOutOrderByNoAsync(string orderNo)
    {
        var clean = orderNo.Trim().ToUpperInvariant();
        return db.StockOutOrders
            .Include(o => o.RO)
            .Include(o => o.Car)
            .Include(o => o.Customer)
            .Include(o => o.Cavity)
            .Include(o => o.StockOut)
            .Include(o => o.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(o => o.OrderNo == clean);
    }

    public async Task<int> CreateStockOutOrderAsync(StockOutOrder order, List<StockOutOrderDetail> items)
    {
        if (items == null || items.Count == 0)
            throw new InvalidOperationException("Phiếu yêu cầu xuất kho cần ít nhất một phụ tùng.");

        if (string.IsNullOrWhiteSpace(order.OrderNo))
        {
            var today = DateTime.Today;
            var prefix = $"SOO{today:yyMMdd}-";
            var count = await db.StockOutOrders.CountAsync(o => o.OrderNo.StartsWith(prefix)) + 1;
            order.OrderNo = $"{prefix}{count:D3}";
        }
        else
        {
            order.OrderNo = order.OrderNo.Trim().ToUpperInvariant();
            var exists = await db.StockOutOrders.AnyAsync(o => o.OrderNo == order.OrderNo);
            if (exists) throw new InvalidOperationException($"Số phiếu yêu cầu {order.OrderNo} đã tồn tại.");
        }

        if (order.ROId.HasValue && order.ROId.Value > 0)
        {
            var ro = await db.ROs.Include(r => r.Car).Include(r => r.Customer).FirstOrDefaultAsync(r => r.Id == order.ROId.Value);
            if (ro != null)
            {
                order.CarId ??= ro.CarId;
                order.CustomerId ??= ro.CustomerId;
                order.CavityId ??= ro.CavityId;
                if (string.IsNullOrWhiteSpace(order.RequesterName))
                    order.RequesterName = ro.Technician ?? "Kỹ thuật viên";
            }
        }

        order.OrderDate = order.OrderDate == default ? DateTime.Today : order.OrderDate;
        order.Status = StockOutOrderStatus.Pending;
        order.CreatedAt = DateTime.Now;

        foreach (var item in items)
        {
            var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId)
                ?? throw new InvalidOperationException($"Phụ tùng ID={item.PartId} không tồn tại.");

            item.PartCode = part.Code;
            item.PartName = part.Name;
            item.Unit = string.IsNullOrWhiteSpace(item.Unit) ? part.Unit : item.Unit;
            if (item.UnitPrice <= 0) item.UnitPrice = part.SalePrice > 0 ? part.SalePrice : part.CostPrice;
            item.RequestQuantity = item.RequestQuantity <= 0 ? 1 : item.RequestQuantity;
            item.IssuedQuantity = 0;
            order.Items.Add(item);
        }

        db.StockOutOrders.Add(order);
        await db.SaveChangesAsync();
        return order.Id;
    }

    public async Task<(bool ok, string msg)> ApproveStockOutOrderAsync(int id, string? approvedBy = null)
    {
        var order = await db.StockOutOrders.FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return (false, "Không tìm thấy phiếu yêu cầu xuất kho phụ tùng.");
        if (order.Status != StockOutOrderStatus.Pending)
            return (false, "Chỉ có thể phê duyệt phiếu yêu cầu ở trạng thái Chờ xuất kho.");

        order.Status = StockOutOrderStatus.Approved;
        order.ApprovedBy = approvedBy ?? "Quản đốc xưởng";
        order.ApprovedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return (true, $"Đã duyệt tiếp nhận yêu cầu xuất kho {order.OrderNo}. Sẵn sàng xuất kho.");
    }

    public async Task<(bool ok, string msg)> RejectStockOutOrderAsync(int id, string reason)
    {
        var order = await db.StockOutOrders.FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return (false, "Không tìm thấy phiếu yêu cầu xuất kho phụ tùng.");
        if (order.Status == StockOutOrderStatus.Completed)
            return (false, "Phiếu yêu cầu đã xuất kho hoàn tất, không thể từ chối.");

        order.Status = StockOutOrderStatus.Rejected;
        order.RejectReason = string.IsNullOrWhiteSpace(reason) ? "Từ chối bởi Quản đốc / Thủ kho" : reason.Trim();
        await db.SaveChangesAsync();
        return (true, $"Đã từ chối phiếu yêu cầu xuất kho {order.OrderNo}. Lý do: {order.RejectReason}");
    }

    public async Task<(bool ok, string msg, int? stockOutId)> IssueStockOutFromOrderAsync(int id, string? issuedBy = null)
    {
        var order = await db.StockOutOrders
            .Include(o => o.Items).ThenInclude(i => i.Part)
            .Include(o => o.RO)
            .Include(o => o.Car)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return (false, "Không tìm thấy phiếu yêu cầu xuất kho phụ tùng.", null);
        if (order.Status == StockOutOrderStatus.Completed)
            return (false, "Phiếu yêu cầu xuất kho đã hoàn tất xuất kho trước đó.", order.StockOutId);
        if (order.Status == StockOutOrderStatus.Rejected)
            return (false, "Phiếu yêu cầu xuất kho đã bị từ chối / hủy, không thể xuất kho.", null);

        // Kiểm tra tồn kho từng mặt hàng (CheckStockBalance theo idn.CarService)
        foreach (var item in order.Items)
        {
            var part = item.Part ?? await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId);
            if (part == null) return (false, $"Không tìm thấy phụ tùng ID={item.PartId}.", null);
            if (part.InStock < item.RequestQuantity)
            {
                return (false, $"Phụ tùng [{part.Code}] {part.Name} không đủ tồn kho (Tồn: {part.InStock:N0}, Yêu cầu: {item.RequestQuantity:N0}).", null);
            }
        }

        // Tự động sinh phiếu xuất kho Ser_Inv_StockOut
        var countToday = await db.StockOuts.CountAsync();
        var stockOut = new StockOut
        {
            StockOutNo = $"XK{DateTime.Today:yyMMdd}-{countToday + 1:D3}",
            StockOutDate = DateTime.Today,
            Type = StockOutType.Service,
            Status = StockOutStatus.Finished,
            ROId = order.ROId,
            CustomerId = order.CustomerId,
            CarId = order.CarId,
            StockOutOrderId = order.Id,
            RecipientName = order.RequesterName ?? order.RO?.Technician ?? "Kỹ thuật viên",
            Description = $"Xuất vật tư theo yêu cầu {order.OrderNo}" + (string.IsNullOrWhiteSpace(order.Description) ? "" : $" - {order.Description}"),
            CreatedBy = order.CreatedBy,
            CreatedAt = DateTime.Now,
            ApprovedBy = issuedBy ?? "Thủ kho",
            FinishedAt = DateTime.Now
        };

        foreach (var item in order.Items)
        {
            var part = item.Part!;
            part.InStock -= item.RequestQuantity;
            item.IssuedQuantity = item.RequestQuantity;

            stockOut.Items.Add(new StockOutDetail
            {
                PartId = item.PartId,
                PartCode = item.PartCode,
                PartName = item.PartName,
                Unit = item.Unit,
                Quantity = item.RequestQuantity,
                UnitPrice = item.UnitPrice,
                VatPercent = item.VatPercent,
                Location = part.Location,
                Note = item.Note
            });
        }

        db.StockOuts.Add(stockOut);
        await db.SaveChangesAsync();

        order.Status = StockOutOrderStatus.Completed;
        order.StockOutId = stockOut.Id;
        order.ApprovedBy ??= issuedBy ?? "Thủ kho";
        order.ApprovedAt ??= DateTime.Now;

        await db.SaveChangesAsync();
        return (true, $"Đã xuất kho thành công phiếu {stockOut.StockOutNo} từ yêu cầu {order.OrderNo}. Đã trừ tồn kho {order.TotalRequestQuantity:N0} phụ tùng.", stockOut.Id);
    }

    public async Task<(bool ok, string msg)> DeleteStockOutOrderAsync(int id)
    {
        var order = await db.StockOutOrders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return (false, "Không tìm thấy phiếu yêu cầu xuất kho.");
        if (order.Status == StockOutOrderStatus.Completed)
            return (false, "Không thể xóa phiếu yêu cầu đã xuất kho hoàn tất.");

        db.StockOutOrders.Remove(order);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa phiếu yêu cầu xuất kho {order.OrderNo}.");
    }

    public Task<List<RepairOrder>> ROsForStockOutOrderAsync() =>
        db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Cavity)
            .Include(r => r.Lines).ThenInclude(l => l.Part)
            .Where(r => r.Status != ROStatus.Rejected && r.Status != ROStatus.NotResponding && r.Status != ROStatus.Finished && r.Status != ROStatus.Paid)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public Task<List<Cavity>> CavitiesForSelectAsync() =>
        db.Cavities.Where(c => c.IsActive).OrderBy(c => c.CavityNo).ToListAsync();

    // --- Part Out of Stock / Backorder (Ser_Part_OO - Quản lý Phụ tùng nợ khách) ---
    public async Task<List<PartOO>> PartOOsAsync(string? q, bool? isConNo, PartOOStatus? status)
    {
        var query = db.PartOOs
            .Include(o => o.Part)
            .Include(o => o.RO).ThenInclude(r => r!.Car)
            .Include(o => o.Car)
            .Include(o => o.Customer)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (isConNo.HasValue)
        {
            if (isConNo.Value)
                query = query.Where(o => o.SoLuongNo > o.SoLuongTra && o.Status != PartOOStatus.Cancelled);
            else
                query = query.Where(o => o.SoLuongNo <= o.SoLuongTra || o.Status == PartOOStatus.Cancelled);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(o =>
                o.OONo.ToLower().Contains(kw) ||
                o.OOPlateNo.ToLower().Contains(kw) ||
                o.PartCode.ToLower().Contains(kw) ||
                o.PartName.ToLower().Contains(kw) ||
                (o.Model != null && o.Model.ToLower().Contains(kw)) ||
                (o.CVDV != null && o.CVDV.ToLower().Contains(kw)) ||
                (o.GhiChu != null && o.GhiChu.ToLower().Contains(kw)));
        }

        return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
    }

    public Task<PartOO?> GetPartOOAsync(int id) =>
        db.PartOOs
            .Include(o => o.Part)
            .Include(o => o.RO).ThenInclude(r => r!.Car)
            .Include(o => o.RO).ThenInclude(r => r!.Customer)
            .Include(o => o.Car)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id);

    public Task<PartOO?> GetPartOOByNoAsync(string ooNo) =>
        db.PartOOs
            .Include(o => o.Part)
            .Include(o => o.RO).ThenInclude(r => r!.Car)
            .Include(o => o.Car)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.OONo == ooNo);

    public async Task<int> CreatePartOOAsync(PartOO item)
    {
        if (string.IsNullOrWhiteSpace(item.OOPlateNo))
            throw new ArgumentException("Biển số xe nợ phụ tùng là bắt buộc.");

        item.OOPlateNo = item.OOPlateNo.Trim().ToUpperInvariant();
        if (item.OOPlateNo.Length < 7 || item.OOPlateNo.Length > 12)
            throw new ArgumentException("Biển số xe phải từ 7 đến 12 ký tự hợp lệ.");

        if (item.PartId <= 0)
            throw new ArgumentException("Vui lòng chọn phụ tùng nợ.");

        var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId);
        if (part == null)
            throw new ArgumentException("Không tìm thấy phụ tùng trong danh mục.");

        if (item.SoLuongNo <= 0)
            throw new ArgumentException("Số lượng nợ phải lớn hơn 0.");

        if (item.SoLuongTra < 0 || item.SoLuongTra > item.SoLuongNo)
            throw new ArgumentException("Số lượng đã trả không hợp lệ.");

        // Kiểm tra trùng: Không được tạo thêm phiếu nợ cùng phụ tùng cho cùng 1 xe nếu phiếu trước vẫn chưa trả xong
        var existsUnfinished = await db.PartOOs.AnyAsync(o =>
            o.PartId == item.PartId &&
            o.OOPlateNo == item.OOPlateNo &&
            o.Status != PartOOStatus.Cancelled &&
            o.Status != PartOOStatus.Completed &&
            o.SoLuongNo > o.SoLuongTra);

        if (existsUnfinished)
            throw new InvalidOperationException($"Xe {item.OOPlateNo} hiện đã có phiếu nợ phụ tùng {part.Code} chưa giải quyết xong.");

        item.PartCode = part.Code;
        item.PartName = part.Name;

        // Sinh mã phiếu OONo
        if (string.IsNullOrWhiteSpace(item.OONo))
        {
            var prefix = $"OO{DateTime.Today:yyMMdd}-";
            var countToday = await db.PartOOs.CountAsync(o => o.OONo.StartsWith(prefix));
            item.OONo = $"{prefix}{countToday + 1:D3}";
        }

        // Tự động liên kết Xe và Khách hàng nếu có sẵn trong hệ thống
        if (!item.CarId.HasValue || item.CarId.Value <= 0)
        {
            var car = await db.Cars.Include(c => c.Customer).FirstOrDefaultAsync(c => c.Plate == item.OOPlateNo);
            if (car != null)
            {
                item.CarId = car.Id;
                item.CustomerId = car.CustomerId;
                if (string.IsNullOrWhiteSpace(item.Model)) item.Model = car.Model;
            }
        }

        // Nếu có ROId liên kết
        if (item.ROId.HasValue && item.ROId.Value > 0)
        {
            var ro = await db.ROs.Include(r => r.Car).FirstOrDefaultAsync(r => r.Id == item.ROId.Value);
            if (ro != null)
            {
                if (!item.CarId.HasValue) item.CarId = ro.CarId;
                if (!item.CustomerId.HasValue) item.CustomerId = ro.CustomerId;
                if (string.IsNullOrWhiteSpace(item.Model) && ro.Car != null) item.Model = ro.Car.Model;
            }
        }

        // Xác định trạng thái ban đầu
        if (item.SoLuongTra >= item.SoLuongNo)
        {
            item.Status = PartOOStatus.Completed;
            item.FinishedAt = DateTime.Now;
        }
        else if (part.InStock >= item.SoLuongConNo)
        {
            item.Status = PartOOStatus.Arrived;
        }
        else
        {
            item.Status = PartOOStatus.Owed;
        }

        item.CreatedAt = DateTime.Now;
        db.PartOOs.Add(item);
        await db.SaveChangesAsync();
        return item.Id;
    }

    public async Task<(bool ok, string msg)> UpdatePartOOAsync(int id, string? model, decimal soLuongNo, decimal soLuongTra, string? cvdv, DateTime? ngayDatHang, DateTime? ngayVeDuKien, DateTime? ngayHenTra, string? ghiChu)
    {
        var item = await db.PartOOs.Include(o => o.Part).FirstOrDefaultAsync(o => o.Id == id);
        if (item == null) return (false, "Không tìm thấy phiếu nợ phụ tùng.");

        if (item.Status == PartOOStatus.Cancelled)
            return (false, "Không thể cập nhật phiếu đã hủy.");

        if (soLuongNo <= 0)
            return (false, "Số lượng nợ phải lớn hơn 0.");

        if (soLuongTra < 0 || soLuongTra > soLuongNo)
            return (false, "Số lượng đã trả không thể nhỏ hơn 0 hoặc lớn hơn số lượng nợ.");

        item.Model = model?.Trim() ?? item.Model;
        item.SoLuongNo = soLuongNo;
        item.SoLuongTra = soLuongTra;
        item.CVDV = cvdv?.Trim();
        item.NgayDatHang = ngayDatHang;
        item.NgayVeDuKien = ngayVeDuKien;
        item.NgayHenTra = ngayHenTra;
        item.GhiChu = ghiChu?.Trim();

        // Cập nhật trạng thái
        if (item.SoLuongTra >= item.SoLuongNo)
        {
            item.Status = PartOOStatus.Completed;
            if (!item.FinishedAt.HasValue) item.FinishedAt = DateTime.Now;
        }
        else
        {
            item.FinishedAt = null;
            if (item.Part != null && item.Part.InStock >= item.SoLuongConNo)
                item.Status = PartOOStatus.Arrived;
            else
                item.Status = PartOOStatus.Owed;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật phiếu nợ {item.OONo}.");
    }

    public async Task<(bool ok, string msg)> ReturnPartOOAsync(int id, decimal returnQty, bool deductStock, string? returnedBy, string? note)
    {
        var item = await db.PartOOs.Include(o => o.Part).FirstOrDefaultAsync(o => o.Id == id);
        if (item == null) return (false, "Không tìm thấy phiếu nợ phụ tùng.");

        if (item.Status == PartOOStatus.Cancelled)
            return (false, "Không thể trả phụ tùng cho phiếu đã hủy.");

        if (item.Status == PartOOStatus.Completed)
            return (false, "Phiếu nợ này đã hoàn tất trả đủ phụ tùng.");

        if (returnQty <= 0)
            return (false, "Số lượng trả phải lớn hơn 0.");

        if (item.SoLuongTra + returnQty > item.SoLuongNo)
            return (false, $"Số lượng trả vượt quá số lượng còn nợ (còn nợ: {item.SoLuongConNo}).");

        if (deductStock)
        {
            if (item.Part == null)
                return (false, "Không tìm thấy dữ liệu phụ tùng để trừ kho.");

            if (item.Part.InStock < returnQty)
                return (false, $"Tồn kho hiện tại ({item.Part.InStock}) không đủ để xuất {returnQty} {item.Part.Unit}.");

            item.Part.InStock -= returnQty;
        }

        item.SoLuongTra += returnQty;
        item.ReturnedBy = string.IsNullOrWhiteSpace(returnedBy) ? "Kỹ thuật viên xưởng" : returnedBy.Trim();

        if (!string.IsNullOrWhiteSpace(note))
        {
            var timeStamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            item.GhiChu = string.IsNullOrWhiteSpace(item.GhiChu)
                ? $"[{timeStamp}] Đã trả {returnQty} {item.Part?.Unit ?? "cái"}: {note.Trim()}"
                : $"{item.GhiChu}\n[{timeStamp}] Đã trả {returnQty} {item.Part?.Unit ?? "cái"}: {note.Trim()}";
        }

        if (item.SoLuongTra >= item.SoLuongNo)
        {
            item.Status = PartOOStatus.Completed;
            item.FinishedAt = DateTime.Now;
        }
        else
        {
            if (item.Part != null && item.Part.InStock >= item.SoLuongConNo)
                item.Status = PartOOStatus.Arrived;
            else
                item.Status = PartOOStatus.Owed;
        }

        await db.SaveChangesAsync();
        var remainText = item.SoLuongConNo > 0 ? $" (Còn nợ: {item.SoLuongConNo})" : " (Đã trả đủ 100%)";
        return (true, $"Đã ghi nhận trả {returnQty} {item.Part?.Unit ?? "cái"} cho xe {item.OOPlateNo}{remainText}.");
    }

    public async Task<(bool ok, string msg)> CancelPartOOAsync(int id, string reason)
    {
        var item = await db.PartOOs.FirstOrDefaultAsync(o => o.Id == id);
        if (item == null) return (false, "Không tìm thấy phiếu nợ phụ tùng.");

        if (item.Status == PartOOStatus.Completed)
            return (false, "Không thể hủy phiếu nợ đã hoàn tất trả đủ.");

        if (item.SoLuongTra > 0)
            return (false, $"Phiếu nợ đã trả {item.SoLuongTra} sản phẩm, không thể hủy hoàn toàn. Hãy cập nhật lại số lượng nợ.");

        item.Status = PartOOStatus.Cancelled;
        var timeStamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        item.GhiChu = string.IsNullOrWhiteSpace(item.GhiChu)
            ? $"[Hủy {timeStamp}] {reason.Trim()}"
            : $"{item.GhiChu}\n[Hủy {timeStamp}] {reason.Trim()}";

        await db.SaveChangesAsync();
        return (true, $"Đã hủy phiếu nợ {item.OONo}.");
    }

    public async Task<(bool ok, string msg)> DeletePartOOAsync(int id)
    {
        var item = await db.PartOOs.FirstOrDefaultAsync(o => o.Id == id);
        if (item == null) return (false, "Không tìm thấy phiếu nợ phụ tùng.");

        if (item.SoLuongTra > 0)
            return (false, "Không thể xóa phiếu đã có phát sinh trả phụ tùng.");

        db.PartOOs.Remove(item);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa phiếu nợ phụ tùng {item.OONo}.");
    }

    public Task<List<PartOO>> GetPartOOStockAlertsAsync() =>
        db.PartOOs
            .Include(o => o.Part)
            .Include(o => o.Car)
            .Include(o => o.Customer)
            .Where(o => o.Status != PartOOStatus.Cancelled && o.Status != PartOOStatus.Completed && o.SoLuongNo > o.SoLuongTra && o.Part.InStock >= (o.SoLuongNo - o.SoLuongTra))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public Task<List<PartOO>> GetPartOOsByPlateAsync(string plate)
    {
        var p = plate.Trim().ToUpperInvariant();
        return db.PartOOs
            .Include(o => o.Part)
            .Where(o => o.OOPlateNo == p)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    // --- Customer Debit Management (Ser_CusDebit, Ser_CusDebitPayment, Ser_InvReportCusDebitRpt / MH 54) ---
    public async Task<List<CusDebit>> CusDebitsAsync(int? customerId, CusDebitStatus? status, CusDebitType? type, string? q, bool? isOverdue, DateTime? fromDate, DateTime? toDate)
    {
        var query = db.CusDebits
            .Include(d => d.Customer)
            .Include(d => d.Car)
            .Include(d => d.RO)
            .Include(d => d.Payments)
            .AsQueryable();

        if (customerId.HasValue && customerId.Value > 0)
            query = query.Where(d => d.CustomerId == customerId.Value);

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        if (type.HasValue)
            query = query.Where(d => d.DebitType == type.Value);

        if (isOverdue.HasValue)
        {
            var today = DateTime.Today;
            if (isOverdue.Value)
                query = query.Where(d => d.Status == CusDebitStatus.Active && d.DueDate.HasValue && d.DueDate.Value.Date < today);
            else
                query = query.Where(d => d.Status != CusDebitStatus.Active || !d.DueDate.HasValue || d.DueDate.Value.Date >= today);
        }

        if (fromDate.HasValue)
            query = query.Where(d => d.DebitDate >= fromDate.Value.Date);

        if (toDate.HasValue)
            query = query.Where(d => d.DebitDate <= toDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(d =>
                d.DebitNo.ToLower().Contains(kw) ||
                d.Customer.Name.ToLower().Contains(kw) ||
                (d.Customer.Phone != null && d.Customer.Phone.Contains(kw)) ||
                (d.Car != null && d.Car.Plate.ToLower().Contains(kw)) ||
                (d.RO != null && d.RO.Code.ToLower().Contains(kw)) ||
                (d.Description != null && d.Description.ToLower().Contains(kw)));
        }

        return await query.OrderByDescending(d => d.DebitDate).ThenByDescending(d => d.CreatedAt).ToListAsync();
    }

    public async Task<List<CustomerDebitSummaryDto>> CustomerDebitSummariesAsync(string? q, bool? onlyHasDebit)
    {
        var customers = await db.Customers
            .Include(c => c.Cars)
            .Include(c => c.CusDebits)
            .Include(c => c.CusDebitPayments)
            .ToListAsync();

        var list = new List<CustomerDebitSummaryDto>();
        var today = DateTime.Today;

        foreach (var c in customers)
        {
            var validDebits = c.CusDebits.Where(d => d.Status != CusDebitStatus.Cancelled).ToList();
            var totalDebit = validDebits.Sum(d => d.DebitAmount);
            var totalPaid = validDebits.Sum(d => d.PaidAmount);
            var rem = Math.Max(0, totalDebit - totalPaid);
            var activeCount = validDebits.Count(d => d.Status == CusDebitStatus.Active && d.DebitAmount > d.PaidAmount);
            var overdueCount = validDebits.Count(d => d.Status == CusDebitStatus.Active && d.DebitAmount > d.PaidAmount && d.DueDate.HasValue && d.DueDate.Value.Date < today);

            var firstCar = c.Cars.FirstOrDefault();

            list.Add(new CustomerDebitSummaryDto
            {
                CustomerId = c.Id,
                CustomerCode = c.Code,
                CustomerName = c.Name,
                Phone = c.Phone,
                PlateNo = firstCar?.Plate,
                CarModel = firstCar?.Model,
                TotalDebitAmount = totalDebit,
                TotalPaidAmount = totalPaid,
                ActiveDebitCount = activeCount,
                OverdueDebitCount = overdueCount,
                LastDebitDate = validDebits.OrderByDescending(d => d.DebitDate).FirstOrDefault()?.DebitDate,
                LastPaymentDate = c.CusDebitPayments.OrderByDescending(p => p.PaymentDate).FirstOrDefault()?.PaymentDate
            });
        }

        if (onlyHasDebit.HasValue && onlyHasDebit.Value)
        {
            list = list.Where(x => x.HasDebit).ToList();
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            list = list.Where(x =>
                x.CustomerName.ToLower().Contains(kw) ||
                x.CustomerCode.ToLower().Contains(kw) ||
                (x.Phone != null && x.Phone.Contains(kw)) ||
                (x.PlateNo != null && x.PlateNo.ToLower().Contains(kw)) ||
                (x.CarModel != null && x.CarModel.ToLower().Contains(kw))).ToList();
        }

        return list.OrderByDescending(x => x.RemainingDebit).ThenBy(x => x.CustomerName).ToList();
    }

    public async Task<(Customer customer, List<CusDebit> debits, List<CusDebitPayment> payments, decimal totalDebit, decimal totalPaid, decimal remainingDebit)> GetCustomerDebitProfileAsync(int customerId)
    {
        var customer = await db.Customers
            .Include(c => c.Cars)
            .Include(c => c.CusDebits).ThenInclude(d => d.RO)
            .Include(c => c.CusDebits).ThenInclude(d => d.Car)
            .Include(c => c.CusDebits).ThenInclude(d => d.Payments)
            .Include(c => c.CusDebitPayments).ThenInclude(p => p.CusDebit)
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null) throw new InvalidOperationException("Không tìm thấy khách hàng.");

        var debits = customer.CusDebits.OrderByDescending(d => d.DebitDate).ThenByDescending(d => d.CreatedAt).ToList();
        var payments = customer.CusDebitPayments.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.CreatedAt).ToList();

        var validDebits = debits.Where(d => d.Status != CusDebitStatus.Cancelled).ToList();
        var totalDebit = validDebits.Sum(d => d.DebitAmount);
        var totalPaid = validDebits.Sum(d => d.PaidAmount);
        var remainingDebit = Math.Max(0, totalDebit - totalPaid);

        return (customer, debits, payments, totalDebit, totalPaid, remainingDebit);
    }

    public Task<CusDebit?> GetCusDebitAsync(int id) =>
        db.CusDebits
            .Include(d => d.Customer)
            .Include(d => d.Car)
            .Include(d => d.RO)
            .Include(d => d.Payments)
            .FirstOrDefaultAsync(d => d.Id == id);

    public Task<CusDebitPayment?> GetCusDebitPaymentAsync(int paymentId) =>
        db.CusDebitPayments
            .Include(p => p.Customer)
            .Include(p => p.CusDebit).ThenInclude(d => d!.Car)
            .Include(p => p.CusDebit).ThenInclude(d => d!.RO)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

    public async Task<int> CreateCusDebitAsync(CusDebit debit)
    {
        if (debit.DebitAmount <= 0)
            throw new InvalidOperationException("Số tiền công nợ phải lớn hơn 0.");

        if (debit.CustomerId <= 0)
            throw new InvalidOperationException("Vui lòng chọn khách hàng.");

        if (string.IsNullOrWhiteSpace(debit.DebitNo))
        {
            var today = DateTime.Today;
            var prefix = $"CDB{today:yyMMdd}-";
            var count = await db.CusDebits.CountAsync(d => d.DebitNo.StartsWith(prefix)) + 1;
            debit.DebitNo = $"{prefix}{count:D3}";
        }

        // Fill Car and Customer from RO if RO provided
        if (debit.ROId.HasValue && debit.ROId.Value > 0)
        {
            var ro = await db.ROs.FirstOrDefaultAsync(r => r.Id == debit.ROId.Value);
            if (ro != null)
            {
                if (debit.CustomerId <= 0) debit.CustomerId = ro.CustomerId;
                if (!debit.CarId.HasValue) debit.CarId = ro.CarId;
            }
        }

        debit.DebitDate = debit.DebitDate == default ? DateTime.Today : debit.DebitDate;
        debit.Status = CusDebitStatus.Active;
        debit.PaidAmount = 0;
        debit.CreatedAt = DateTime.Now;

        db.CusDebits.Add(debit);
        await db.SaveChangesAsync();
        return debit.Id;
    }

    public async Task<int> CreateCusDebitPaymentAsync(CusDebitPayment payment)
    {
        if (payment.PaymentAmount <= 0)
            throw new InvalidOperationException("Số tiền thu nợ phải lớn hơn 0.");

        if (payment.CustomerId <= 0)
            throw new InvalidOperationException("Vui lòng chọn khách hàng.");

        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == payment.CustomerId);
        if (customer == null)
            throw new InvalidOperationException("Không tìm thấy khách hàng.");

        if (string.IsNullOrWhiteSpace(payment.PayPersonName))
            payment.PayPersonName = customer.Name;

        if (string.IsNullOrWhiteSpace(payment.PaymentNo))
        {
            var today = DateTime.Today;
            var prefix = $"CDP{today:yyMMdd}-";
            var count = await db.CusDebitPayments.CountAsync(p => p.PaymentNo.StartsWith(prefix)) + 1;
            payment.PaymentNo = $"{prefix}{count:D3}";
        }

        payment.PaymentDate = payment.PaymentDate == default ? DateTime.Today : payment.PaymentDate;
        payment.CreatedAt = DateTime.Now;

        // Allocation logic
        if (payment.CusDebitId.HasValue && payment.CusDebitId.Value > 0)
        {
            var debit = await db.CusDebits.Include(d => d.RO).FirstOrDefaultAsync(d => d.Id == payment.CusDebitId.Value);
            if (debit == null)
                throw new InvalidOperationException("Không tìm thấy khoản nợ được chỉ định.");

            if (debit.Status == CusDebitStatus.Cancelled)
                throw new InvalidOperationException("Khoản nợ này đã bị hủy.");

            debit.PaidAmount += payment.PaymentAmount;
            if (debit.PaidAmount >= debit.DebitAmount)
            {
                debit.Status = CusDebitStatus.Cleared;
                debit.ClearedAt = DateTime.Now;
            }

            // Đồng bộ trạng thái RO nếu Lệnh sửa chữa đã thu đủ tiền
            if (debit.ROId.HasValue && debit.RO != null)
            {
                var otherDebits = await db.CusDebits.Where(d => d.ROId == debit.ROId.Value && d.Status == CusDebitStatus.Active).ToListAsync();
                if (otherDebits.All(d => d.Id == debit.Id || d.PaidAmount >= d.DebitAmount))
                {
                    if (debit.RO.Status is ROStatus.CheckEnd or ROStatus.Repaired or ROStatus.HasRO)
                    {
                        debit.RO.Status = ROStatus.Paid;
                    }
                }
            }
        }
        else
        {
            // Tự động phân bổ vào các khoản nợ của khách theo thứ tự thời gian phát sinh (FIFO)
            var activeDebits = await db.CusDebits
                .Include(d => d.RO)
                .Where(d => d.CustomerId == payment.CustomerId && d.Status == CusDebitStatus.Active && d.DebitAmount > d.PaidAmount)
                .OrderBy(d => d.DebitDate)
                .ThenBy(d => d.Id)
                .ToListAsync();

            var moneyLeft = payment.PaymentAmount;
            foreach (var d in activeDebits)
            {
                if (moneyLeft <= 0) break;
                var needed = d.DebitAmount - d.PaidAmount;
                var alloc = Math.Min(moneyLeft, needed);
                d.PaidAmount += alloc;
                moneyLeft -= alloc;

                if (d.PaidAmount >= d.DebitAmount)
                {
                    d.Status = CusDebitStatus.Cleared;
                    d.ClearedAt = DateTime.Now;

                    if (d.ROId.HasValue && d.RO != null)
                    {
                        if (d.RO.Status is ROStatus.CheckEnd or ROStatus.Repaired or ROStatus.HasRO)
                        {
                            d.RO.Status = ROStatus.Paid;
                        }
                    }
                }
            }
        }

        db.CusDebitPayments.Add(payment);
        await db.SaveChangesAsync();
        return payment.Id;
    }

    public async Task<(bool ok, string msg)> CancelCusDebitAsync(int id, string? reason)
    {
        var debit = await db.CusDebits.FirstOrDefaultAsync(d => d.Id == id);
        if (debit == null) return (false, "Không tìm thấy khoản nợ.");

        if (debit.PaidAmount > 0)
            return (false, $"Khoản nợ đã phát sinh thanh toán ({debit.PaidAmount:N0} đ), không thể hủy trực tiếp.");

        debit.Status = CusDebitStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            debit.Description = string.IsNullOrWhiteSpace(debit.Description)
                ? $"[Hủy: {reason.Trim()}]"
                : $"{debit.Description}\n[Hủy: {reason.Trim()}]";
        }

        await db.SaveChangesAsync();
        return (true, $"Đã hủy khoản nợ {debit.DebitNo}.");
    }

    public async Task<(bool ok, string msg)> DeleteCusDebitPaymentAsync(int paymentId)
    {
        var payment = await db.CusDebitPayments
            .Include(p => p.CusDebit)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null) return (false, "Không tìm thấy phiếu thu nợ.");

        if (payment.CusDebit != null)
        {
            payment.CusDebit.PaidAmount = Math.Max(0, payment.CusDebit.PaidAmount - payment.PaymentAmount);
            if (payment.CusDebit.Status == CusDebitStatus.Cleared && payment.CusDebit.PaidAmount < payment.CusDebit.DebitAmount)
            {
                payment.CusDebit.Status = CusDebitStatus.Active;
                payment.CusDebit.ClearedAt = null;
            }
        }

        db.CusDebitPayments.Remove(payment);
        await db.SaveChangesAsync();
        return (true, $"Đã hủy phiếu thu nợ {payment.PaymentNo} và hoàn trả dư nợ.");
    }

    public async Task<(bool ok, string msg, int? debitId)> CreateDebitFromROAsync(int roId, decimal? amount, DateTime? dueDate, string? note)
    {
        var ro = await db.ROs
            .Include(r => r.Lines)
            .Include(r => r.Payments)
            .Include(r => r.Customer)
            .Include(r => r.Car)
            .FirstOrDefaultAsync(r => r.Id == roId);

        if (ro == null) return (false, "Không tìm thấy Lệnh sửa chữa (RO).", null);

        var debtAmount = amount ?? ro.RemainingBalance;
        if (debtAmount <= 0)
            return (false, "Lệnh sửa chữa đã được thanh toán đủ, không có dư nợ phát sinh.", null);

        var today = DateTime.Today;
        var prefix = $"CDB{today:yyMMdd}-";
        var count = await db.CusDebits.CountAsync(d => d.DebitNo.StartsWith(prefix)) + 1;

        var debit = new CusDebit
        {
            DebitNo = $"{prefix}{count:D3}",
            CustomerId = ro.CustomerId,
            CarId = ro.CarId,
            ROId = ro.Id,
            DebitType = CusDebitType.RO,
            Status = CusDebitStatus.Active,
            DebitDate = DateTime.Today,
            DueDate = dueDate ?? DateTime.Today.AddDays(30),
            DebitAmount = debtAmount,
            PaidAmount = 0,
            Description = !string.IsNullOrWhiteSpace(note) ? note.Trim() : $"Ghi nhận công nợ từ Lệnh sửa chữa {ro.Code} (Xe {ro.Car?.Plate})",
            CreatedBy = "CVDV",
            CreatedAt = DateTime.Now
        };

        db.CusDebits.Add(debit);
        await db.SaveChangesAsync();
        return (true, $"Đã ghi nhận công nợ {debit.DebitNo} số tiền {debtAmount:N0} đ cho Lệnh sửa chữa {ro.Code}.", debit.Id);
    }

    public Task<List<Customer>> CustomersForDebitSelectAsync() =>
        db.Customers
            .Include(c => c.Cars)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public Task<List<RepairOrder>> ROsWithUnpaidBalanceAsync() =>
        db.ROs
            .Include(r => r.Customer)
            .Include(r => r.Car)
            .Include(r => r.Lines)
            .Include(r => r.Payments)
            .Where(r => r.Status != ROStatus.Rejected && r.Status != ROStatus.NotResponding)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    // =========================================================================
    // QUẢN LÝ CÔNG NỢ NHÀ CUNG CẤP & THANH TOÁN NỢ NCC (Ser_SupplierDebit, Ser_SupplierDebitPayment / MH 56)
    // =========================================================================

    public async Task<List<SupplierDebit>> SupplierDebitsAsync(int? supplierId, SupplierDebitStatus? status, SupplierDebitType? type, string? q, bool? isOverdue, DateTime? fromDate, DateTime? toDate)
    {
        var query = db.SupplierDebits
            .Include(d => d.Supplier)
            .Include(d => d.StockIn)
            .Include(d => d.OrderPart)
            .Include(d => d.Payments)
            .AsQueryable();

        if (supplierId.HasValue && supplierId.Value > 0)
            query = query.Where(d => d.SupplierId == supplierId.Value);

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        if (type.HasValue)
            query = query.Where(d => d.DebitType == type.Value);

        if (isOverdue == true)
        {
            var today = DateTime.Today;
            query = query.Where(d => d.Status == SupplierDebitStatus.Active && d.DueDate.HasValue && d.DueDate.Value.Date < today);
        }

        if (fromDate.HasValue)
            query = query.Where(d => d.DebitDate >= fromDate.Value.Date);

        if (toDate.HasValue)
            query = query.Where(d => d.DebitDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(d => d.DebitNo.ToLower().Contains(term)
                || d.Supplier.Name.ToLower().Contains(term)
                || d.Supplier.Code.ToLower().Contains(term)
                || (d.StockIn != null && d.StockIn.StockInNo.ToLower().Contains(term))
                || (d.StockIn != null && d.StockIn.BillNo != null && d.StockIn.BillNo.ToLower().Contains(term))
                || (d.OrderPart != null && d.OrderPart.OrderPartNo.ToLower().Contains(term))
                || (d.Description != null && d.Description.ToLower().Contains(term)));
        }

        return await query.OrderByDescending(d => d.DebitDate).ThenByDescending(d => d.CreatedAt).ToListAsync();
    }

    public async Task<List<SupplierDebitSummaryDto>> SupplierDebitSummariesAsync(string? q, bool? onlyHasDebit)
    {
        var suppliers = await db.Suppliers
            .Include(s => s.SupplierDebits)
            .Include(s => s.SupplierDebitPayments)
            .Where(s => s.IsActive)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            suppliers = suppliers.Where(s => s.Name.ToLower().Contains(term)
                || s.Code.ToLower().Contains(term)
                || (s.Phone != null && s.Phone.Contains(term))
                || (s.ContactName != null && s.ContactName.ToLower().Contains(term))).ToList();
        }

        var summaries = suppliers.Select(s =>
        {
            var validDebits = s.SupplierDebits.Where(d => d.Status != SupplierDebitStatus.Cancelled).ToList();
            var totalDebit = validDebits.Sum(d => d.DebitAmount);
            var totalPaid = validDebits.Sum(d => d.PaidAmount);
            var activeCount = validDebits.Count(d => d.Status == SupplierDebitStatus.Active && d.RemainAmount > 0);
            var overdueCount = validDebits.Count(d => d.IsOverdue);
            var lastDebit = validDebits.OrderByDescending(d => d.DebitDate).FirstOrDefault()?.DebitDate;
            var lastPayment = s.SupplierDebitPayments.OrderByDescending(p => p.PaymentDate).FirstOrDefault()?.PaymentDate;

            return new SupplierDebitSummaryDto
            {
                SupplierId = s.Id,
                SupplierCode = s.Code,
                SupplierName = s.Name,
                Phone = s.Phone,
                Address = s.Address,
                ContactName = s.ContactName,
                BankAccount = s.BankAccount,
                BankName = s.BankName,
                TotalDebitAmount = totalDebit,
                TotalPaidAmount = totalPaid,
                ActiveDebitCount = activeCount,
                OverdueDebitCount = overdueCount,
                LastDebitDate = lastDebit,
                LastPaymentDate = lastPayment
            };
        });

        if (onlyHasDebit == true)
            summaries = summaries.Where(s => s.HasDebit);

        return summaries.OrderByDescending(s => s.RemainingDebit).ThenBy(s => s.SupplierName).ToList();
    }

    public async Task<(Supplier supplier, List<SupplierDebit> debits, List<SupplierDebitPayment> payments, decimal totalDebit, decimal totalPaid, decimal remainingDebit)> GetSupplierDebitProfileAsync(int supplierId)
    {
        var supplier = await db.Suppliers
            .Include(s => s.SupplierDebits).ThenInclude(d => d.StockIn)
            .Include(s => s.SupplierDebits).ThenInclude(d => d.OrderPart)
            .Include(s => s.SupplierDebits).ThenInclude(d => d.Payments)
            .Include(s => s.SupplierDebitPayments).ThenInclude(p => p.SupplierDebit)
            .FirstOrDefaultAsync(s => s.Id == supplierId);

        if (supplier == null)
            throw new InvalidOperationException("Không tìm thấy Nhà cung cấp.");

        var debits = supplier.SupplierDebits.OrderByDescending(d => d.DebitDate).ThenByDescending(d => d.CreatedAt).ToList();
        var payments = supplier.SupplierDebitPayments.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.CreatedAt).ToList();

        var validDebits = debits.Where(d => d.Status != SupplierDebitStatus.Cancelled).ToList();
        var totalDebit = validDebits.Sum(d => d.DebitAmount);
        var totalPaid = validDebits.Sum(d => d.PaidAmount);
        var remainingDebit = Math.Max(0, totalDebit - totalPaid);

        return (supplier, debits, payments, totalDebit, totalPaid, remainingDebit);
    }

    public Task<SupplierDebit?> GetSupplierDebitAsync(int id) =>
        db.SupplierDebits
            .Include(d => d.Supplier)
            .Include(d => d.StockIn).ThenInclude(s => s!.Items)
            .Include(d => d.OrderPart)
            .Include(d => d.Payments)
            .FirstOrDefaultAsync(d => d.Id == id);

    public Task<SupplierDebitPayment?> GetSupplierDebitPaymentAsync(int paymentId) =>
        db.SupplierDebitPayments
            .Include(p => p.Supplier)
            .Include(p => p.SupplierDebit).ThenInclude(d => d!.StockIn)
            .Include(p => p.SupplierDebit).ThenInclude(d => d!.OrderPart)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

    public async Task<int> CreateSupplierDebitAsync(SupplierDebit debit)
    {
        if (debit.DebitAmount <= 0)
            throw new InvalidOperationException("Số tiền công nợ phải lớn hơn 0.");

        if (debit.SupplierId <= 0)
            throw new InvalidOperationException("Vui lòng chọn Nhà cung cấp.");

        if (string.IsNullOrWhiteSpace(debit.DebitNo))
        {
            var today = DateTime.Today;
            var prefix = $"SDB{today:yyMMdd}-";
            var count = await db.SupplierDebits.CountAsync(d => d.DebitNo.StartsWith(prefix)) + 1;
            debit.DebitNo = $"{prefix}{count:D3}";
        }

        // Link with StockIn if supplied
        if (debit.StockInId.HasValue && debit.StockInId.Value > 0)
        {
            var si = await db.StockIns.FirstOrDefaultAsync(s => s.Id == debit.StockInId.Value);
            if (si != null)
            {
                if (!debit.OrderPartId.HasValue) debit.OrderPartId = si.OrderPartId;
            }
        }

        debit.DebitDate = debit.DebitDate == default ? DateTime.Today : debit.DebitDate;
        debit.DueDate = debit.DueDate ?? debit.DebitDate.AddDays(30);
        debit.Status = SupplierDebitStatus.Active;
        debit.PaidAmount = 0;
        debit.CreatedAt = DateTime.Now;

        db.SupplierDebits.Add(debit);
        await db.SaveChangesAsync();
        return debit.Id;
    }

    public async Task<int> CreateSupplierDebitPaymentAsync(SupplierDebitPayment payment, bool allocateFifoIfNoDebit = true)
    {
        if (payment.PaymentAmount <= 0)
            throw new InvalidOperationException("Số tiền thanh toán phải lớn hơn 0.");

        if (payment.SupplierId <= 0)
            throw new InvalidOperationException("Vui lòng chọn Nhà cung cấp.");

        var supplier = await db.Suppliers.FirstOrDefaultAsync(s => s.Id == payment.SupplierId);
        if (supplier == null)
            throw new InvalidOperationException("Không tìm thấy Nhà cung cấp.");

        if (string.IsNullOrWhiteSpace(payment.PayPersonName))
            payment.PayPersonName = !string.IsNullOrWhiteSpace(supplier.ContactName) ? supplier.ContactName : supplier.Name;

        if (string.IsNullOrWhiteSpace(payment.BankAccount))
            payment.BankAccount = supplier.BankAccount;

        if (string.IsNullOrWhiteSpace(payment.BankName))
            payment.BankName = supplier.BankName;

        if (string.IsNullOrWhiteSpace(payment.PaymentNo))
        {
            var today = DateTime.Today;
            var prefix = $"SDP{today:yyMMdd}-";
            var count = await db.SupplierDebitPayments.CountAsync(p => p.PaymentNo.StartsWith(prefix)) + 1;
            payment.PaymentNo = $"{prefix}{count:D3}";
        }

        payment.PaymentDate = payment.PaymentDate == default ? DateTime.Today : payment.PaymentDate;
        payment.CreatedAt = DateTime.Now;

        // Allocation logic
        if (payment.SupplierDebitId.HasValue && payment.SupplierDebitId.Value > 0)
        {
            var debit = await db.SupplierDebits.FirstOrDefaultAsync(d => d.Id == payment.SupplierDebitId.Value);
            if (debit == null)
                throw new InvalidOperationException("Không tìm thấy khoản nợ được chỉ định.");

            if (debit.Status == SupplierDebitStatus.Cancelled)
                throw new InvalidOperationException("Khoản nợ này đã bị hủy.");

            debit.PaidAmount += payment.PaymentAmount;
            if (debit.PaidAmount >= debit.DebitAmount)
            {
                debit.Status = SupplierDebitStatus.Cleared;
                debit.ClearedAt = DateTime.Now;
            }
        }
        else if (allocateFifoIfNoDebit)
        {
            // Tự động phân bổ vào các khoản nợ của NCC theo thứ tự hạn thanh toán (FIFO)
            var activeDebits = await db.SupplierDebits
                .Where(d => d.SupplierId == payment.SupplierId && d.Status == SupplierDebitStatus.Active && d.DebitAmount > d.PaidAmount)
                .OrderBy(d => d.DueDate ?? d.DebitDate)
                .ThenBy(d => d.Id)
                .ToListAsync();

            var moneyLeft = payment.PaymentAmount;
            foreach (var d in activeDebits)
            {
                if (moneyLeft <= 0) break;
                var needed = d.DebitAmount - d.PaidAmount;
                var alloc = Math.Min(moneyLeft, needed);
                d.PaidAmount += alloc;
                moneyLeft -= alloc;

                if (d.PaidAmount >= d.DebitAmount)
                {
                    d.Status = SupplierDebitStatus.Cleared;
                    d.ClearedAt = DateTime.Now;
                }
            }
        }

        db.SupplierDebitPayments.Add(payment);
        await db.SaveChangesAsync();
        return payment.Id;
    }

    public async Task<(bool ok, string msg)> CancelSupplierDebitAsync(int id, string? reason)
    {
        var debit = await db.SupplierDebits.FirstOrDefaultAsync(d => d.Id == id);
        if (debit == null) return (false, "Không tìm thấy khoản nợ.");

        if (debit.PaidAmount > 0)
            return (false, $"Khoản nợ đã phát sinh thanh toán ({debit.PaidAmount:N0} đ), không thể hủy trực tiếp.");

        debit.Status = SupplierDebitStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            debit.Description = string.IsNullOrWhiteSpace(debit.Description)
                ? $"[Hủy: {reason.Trim()}]"
                : $"{debit.Description}\n[Hủy: {reason.Trim()}]";
        }

        await db.SaveChangesAsync();
        return (true, $"Đã hủy khoản nợ NCC {debit.DebitNo}.");
    }

    public async Task<(bool ok, string msg)> DeleteSupplierDebitPaymentAsync(int paymentId)
    {
        var payment = await db.SupplierDebitPayments
            .Include(p => p.SupplierDebit)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null) return (false, "Không tìm thấy phiếu chi thanh toán nợ.");

        if (payment.SupplierDebit != null)
        {
            payment.SupplierDebit.PaidAmount = Math.Max(0, payment.SupplierDebit.PaidAmount - payment.PaymentAmount);
            if (payment.SupplierDebit.Status == SupplierDebitStatus.Cleared && payment.SupplierDebit.PaidAmount < payment.SupplierDebit.DebitAmount)
            {
                payment.SupplierDebit.Status = SupplierDebitStatus.Active;
                payment.SupplierDebit.ClearedAt = null;
            }
        }

        db.SupplierDebitPayments.Remove(payment);
        await db.SaveChangesAsync();
        return (true, $"Đã hủy phiếu chi {payment.PaymentNo} và hoàn trả dư nợ NCC.");
    }

    public async Task<(bool ok, string msg, int? debitId)> CreateSupplierDebitFromStockInAsync(int stockInId, int? supplierId, DateTime? dueDate, string? note)
    {
        var stockIn = await db.StockIns.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == stockInId);
        if (stockIn == null) return (false, "Không tìm thấy Phiếu nhập kho.", null);

        var existing = await db.SupplierDebits.FirstOrDefaultAsync(d => d.StockInId == stockInId && d.Status != SupplierDebitStatus.Cancelled);
        if (existing != null)
            return (false, $"Phiếu nhập kho này đã được ghi nợ trước đó ({existing.DebitNo}).", existing.Id);

        var supId = supplierId;
        if (!supId.HasValue || supId.Value <= 0)
        {
            var matchSup = await db.Suppliers.FirstOrDefaultAsync(s => s.Name == stockIn.SupplierName || s.Code == stockIn.SupplierName);
            if (matchSup != null) supId = matchSup.Id;
            else
            {
                var firstSup = await db.Suppliers.FirstOrDefaultAsync(s => s.IsActive);
                supId = firstSup?.Id;
            }
        }

        if (!supId.HasValue || supId.Value <= 0)
            return (false, "Vui lòng chỉ định Nhà cung cấp cho phiếu nợ.", null);

        var debtAmount = stockIn.Total;
        if (debtAmount <= 0)
            return (false, "Phiếu nhập kho có giá trị bằng 0 đ, không thể tạo nợ.", null);

        var today = DateTime.Today;
        var prefix = $"SDB{today:yyMMdd}-";
        var count = await db.SupplierDebits.CountAsync(d => d.DebitNo.StartsWith(prefix)) + 1;

        var debit = new SupplierDebit
        {
            DebitNo = $"{prefix}{count:D3}",
            SupplierId = supId.Value,
            StockInId = stockIn.Id,
            OrderPartId = stockIn.OrderPartId,
            DebitType = SupplierDebitType.StockIn,
            Status = SupplierDebitStatus.Active,
            DebitDate = stockIn.StockInDate,
            DueDate = dueDate ?? stockIn.StockInDate.AddDays(30),
            DebitAmount = debtAmount,
            PaidAmount = 0,
            Description = !string.IsNullOrWhiteSpace(note) ? note.Trim() : $"Ghi nhận công nợ nhập kho phụ tùng {stockIn.StockInNo}" + (!string.IsNullOrWhiteSpace(stockIn.BillNo) ? $" (HĐ: {stockIn.BillNo})" : ""),
            CreatedBy = "Kế toán kho",
            CreatedAt = DateTime.Now
        };

        db.SupplierDebits.Add(debit);
        await db.SaveChangesAsync();
        return (true, $"Đã ghi nhận công nợ NCC {debit.DebitNo} số tiền {debtAmount:N0} đ cho phiếu nhập kho {stockIn.StockInNo}.", debit.Id);
    }

    public Task<List<Supplier>> SuppliersForDebitSelectAsync() =>
        db.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();

    public async Task<List<StockIn>> StockInsForDebitSelectAsync()
    {
        var existingDebitStockInIds = await db.SupplierDebits
            .Where(d => d.StockInId.HasValue && d.Status != SupplierDebitStatus.Cancelled)
            .Select(d => d.StockInId!.Value)
            .ToListAsync();

        return await db.StockIns
            .Include(s => s.Items)
            .Where(s => s.Status == StockInStatus.Finished && !existingDebitStockInIds.Contains(s.Id))
            .OrderByDescending(s => s.StockInDate)
            .Take(30)
            .ToListAsync();
    }

    // =========================================================================
    // QUẢN LÝ TRA CỨU & CHIA SẺ LỊCH SỬ SỬA CHỮA TOÀN HỆ THỐNG ĐẠI LÝ (DealerHistoryShareMng)
    // =========================================================================

    public async Task<List<DealerHistoryRecord>> SearchDealerHistoryAsync(string? q, string? dealer, DateTime? fromDate, DateTime? toDate)
    {
        var query = db.DealerHistoryRecords
            .Include(r => r.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(dealer))
        {
            var d = dealer.Trim().ToLower();
            query = query.Where(r => r.DealerCode.ToLower() == d || r.DealerName.ToLower().Contains(d));
        }

        if (fromDate.HasValue)
            query = query.Where(r => r.CheckInDate >= fromDate.Value.Date);

        if (toDate.HasValue)
            query = query.Where(r => r.CheckInDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(r =>
                r.PlateNo.ToLower().Contains(term) ||
                r.FrameNo.ToLower().Contains(term) ||
                (r.EngineNo != null && r.EngineNo.ToLower().Contains(term)) ||
                r.CusName.ToLower().Contains(term) ||
                (r.CusPhone != null && r.CusPhone.Contains(term)) ||
                r.RONo.ToLower().Contains(term) ||
                r.ModelName.ToLower().Contains(term) ||
                (r.CustomerRequest != null && r.CustomerRequest.ToLower().Contains(term)));
        }

        return await query.OrderByDescending(r => r.CheckInDate).ThenByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<VehicleHistorySummaryDto?> GetVehicleServiceSummaryAsync(string plateOrVin)
    {
        if (string.IsNullOrWhiteSpace(plateOrVin)) return null;
        var term = plateOrVin.Trim().ToUpperInvariant();

        // 1. Tìm trong DealerHistoryRecords
        var records = await db.DealerHistoryRecords
            .Include(r => r.Items)
            .Where(r => r.PlateNo.ToUpper() == term || r.FrameNo.ToUpper() == term || r.PlateNo.ToUpper().Contains(term) || r.FrameNo.ToUpper().Contains(term))
            .OrderByDescending(r => r.CheckInDate)
            .ToListAsync();

        // 2. Tìm xe trong local database để kiểm tra bổ sung lịch sử từ RO nội bộ
        var localCar = await db.Cars
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Plate.ToUpper() == term || (c.Vin != null && c.Vin.ToUpper() == term) || c.Plate.ToUpper().Contains(term) || (c.Vin != null && c.Vin.ToUpper().Contains(term)));

        if (localCar != null)
        {
            // Kiểm tra các RO local của xe này đã được sync sang DealerHistory chưa
            var existingRoIds = records.Where(r => r.ROId.HasValue).Select(r => r.ROId!.Value).ToHashSet();
            var localRos = await db.ROs
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .Include(r => r.Lines).ThenInclude(l => l.Part)
                .Include(r => r.Lines).ThenInclude(l => l.ServiceItem)
                .Include(r => r.WarrantyReports)
                .Where(r => r.CarId == localCar.Id && !existingRoIds.Contains(r.Id))
                .ToListAsync();

            foreach (var ro in localRos)
            {
                var (_, _, recId) = await SyncLocalRoToHistoryAsync(ro.Id);
                if (recId.HasValue)
                {
                    var newRec = await db.DealerHistoryRecords.Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == recId.Value);
                    if (newRec != null) records.Add(newRec);
                }
            }
            records = records.OrderByDescending(r => r.CheckInDate).ToList();
        }

        if (records.Count == 0 && localCar == null) return null;

        var firstRec = records.FirstOrDefault();
        var plate = firstRec?.PlateNo ?? localCar?.Plate ?? term;
        var vin = firstRec?.FrameNo ?? localCar?.Vin ?? "";
        var model = firstRec?.ModelName ?? localCar?.Model ?? "";
        var year = firstRec?.ProductYear ?? localCar?.Year ?? DateTime.Now.Year;
        var engine = firstRec?.EngineNo;
        var color = firstRec?.ColorCode;
        var cusName = firstRec?.CusName ?? localCar?.Customer.Name ?? "";
        var cusPhone = firstRec?.CusPhone ?? localCar?.Customer.Phone ?? "";
        var cusAddress = firstRec?.CusAddress;

        var totalSpent = records.Sum(r => r.TotalAmount);
        var totalClaims = records.Count(r => r.FlagClaim);
        var maxKm = records.Count > 0 ? records.Max(r => r.Odometer) : 0;
        var firstDate = records.Count > 0 ? records.Min(r => r.CheckInDate) : (DateTime?)null;
        var lastDate = records.Count > 0 ? records.Max(r => r.CheckInDate) : (DateTime?)null;
        var lastDealer = firstRec?.DealerName;
        var lastAdvisor = firstRec?.ServiceAdvisor;

        return new VehicleHistorySummaryDto
        {
            PlateNo = plate,
            FrameNo = vin,
            EngineNo = engine,
            TradeMarkName = "Hyundai",
            ModelName = model,
            ColorCode = color,
            ProductYear = year,
            CusName = cusName,
            CusPhone = cusPhone,
            CusAddress = cusAddress,
            CurrentKm = maxKm,
            TotalVisits = records.Count,
            TotalSpent = totalSpent,
            TotalClaims = totalClaims,
            FirstVisitDate = firstDate,
            LastVisitDate = lastDate,
            LastDealerName = lastDealer,
            LastServiceAdvisor = lastAdvisor,
            Records = records
        };
    }

    public Task<DealerHistoryRecord?> GetDealerHistoryRecordAsync(int id) =>
        db.DealerHistoryRecords
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<int> CreateDealerHistoryRecordAsync(DealerHistoryRecord record, List<DealerHistoryItem> items)
    {
        if (string.IsNullOrWhiteSpace(record.PlateNo))
            throw new InvalidOperationException("Vui lòng nhập Biển số xe.");
        if (string.IsNullOrWhiteSpace(record.DealerName))
            throw new InvalidOperationException("Vui lòng nhập Tên đại lý thực hiện.");
        if (string.IsNullOrWhiteSpace(record.RONo))
            throw new InvalidOperationException("Vui lòng nhập Số lệnh sửa chữa (RONo).");

        record.PlateNo = record.PlateNo.Trim().ToUpperInvariant();
        record.FrameNo = !string.IsNullOrWhiteSpace(record.FrameNo) ? record.FrameNo.Trim().ToUpperInvariant() : "";
        record.ModelName = record.ModelName.Trim();
        record.CusName = record.CusName.Trim();
        record.CheckInDate = record.CheckInDate == default ? DateTime.Now : record.CheckInDate;
        record.CreatedAt = DateTime.Now;

        if (string.IsNullOrWhiteSpace(record.RecordNo))
        {
            var count = await db.DealerHistoryRecords.CountAsync() + 1;
            record.RecordNo = $"DHR{DateTime.Today:yyMMdd}-{count:D3}";
        }

        foreach (var item in items)
        {
            item.Amount = item.Quantity * item.UnitPrice;
            record.Items.Add(item);
        }

        record.TotalLaborAmount = record.Items.Where(i => i.ItemType == LineType.Labor).Sum(i => i.Amount);
        record.TotalPartAmount = record.Items.Where(i => i.ItemType == LineType.Part).Sum(i => i.Amount);
        record.TotalAmount = record.TotalLaborAmount + record.TotalPartAmount;

        db.DealerHistoryRecords.Add(record);
        await db.SaveChangesAsync();
        return record.Id;
    }

    public async Task<(bool ok, string msg, int? recordId)> SyncLocalRoToHistoryAsync(int roId)
    {
        var ro = await db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Lines).ThenInclude(l => l.Part)
            .Include(r => r.Lines).ThenInclude(l => l.ServiceItem)
            .Include(r => r.WarrantyReports)
            .FirstOrDefaultAsync(r => r.Id == roId);

        if (ro == null) return (false, "Không tìm thấy Lệnh sửa chữa.", null);

        var existing = await db.DealerHistoryRecords.Include(r => r.Items).FirstOrDefaultAsync(r => r.ROId == roId);
        if (existing != null)
        {
            existing.ActualDeliveryDate = ro.FinishedAt;
            existing.Odometer = ro.Odometer;
            existing.ServiceAdvisor = string.IsNullOrWhiteSpace(ro.CreatedBy) ? "CVDV" : ro.CreatedBy;
            existing.Technician = ro.Technician;
            existing.CustomerRequest = ro.IntakeNote;
            existing.TotalLaborAmount = ro.LaborTotal;
            existing.TotalPartAmount = ro.PartTotal;
            existing.TotalAmount = ro.Total;
            existing.FlagClaim = ro.WarrantyReports.Any();
            existing.ClaimNo = ro.WarrantyReports.FirstOrDefault()?.ReportNo;
            existing.ClaimStatus = ro.WarrantyReports.FirstOrDefault() != null ? Ui.WarrantyStatus(ro.WarrantyReports.First().Status).code : null;

            existing.Items.Clear();
            foreach (var line in ro.Lines)
            {
                existing.Items.Add(new DealerHistoryItem
                {
                    ItemType = line.Type,
                    Code = line.Type == LineType.Part ? (line.Part?.Code ?? "PRT") : (line.ServiceItem?.Code ?? "LABOR"),
                    Name = line.Name,
                    Unit = line.Type == LineType.Part ? (line.Part?.Unit ?? "Cái") : "Giờ",
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    Amount = line.Amount,
                    ExpenseType = line.ExpenseType,
                    Technician = ro.Technician,
                    Result = "Đạt tiêu chuẩn kỹ thuật xuất xưởng",
                    Remark = line.Type == LineType.Part ? (line.Part?.Location) : null
                });
            }

            await db.SaveChangesAsync();
            return (true, $"Đã cập nhật lịch sử sửa chữa {existing.RecordNo} cho RO {ro.Code}.", existing.Id);
        }

        var today = DateTime.Today;
        var count = await db.DealerHistoryRecords.CountAsync(r => r.RecordNo.StartsWith($"DHR{today:yyMMdd}-")) + 1;
        var record = new DealerHistoryRecord
        {
            RecordNo = $"DHR{today:yyMMdd}-{count:D3}",
            DealerCode = "HTC-MAIN",
            DealerName = "Hyundai Service Workshop (Đại lý hiện tại)",
            PlateNo = ro.Car.Plate,
            FrameNo = !string.IsNullOrWhiteSpace(ro.Car.Vin) ? ro.Car.Vin : "RLHXX" + ro.Car.Plate.Replace("-", "").Replace(".", ""),
            ModelName = ro.Car.Model,
            ProductYear = ro.Car.Year > 0 ? ro.Car.Year : DateTime.Today.Year,
            TradeMarkName = "Hyundai",
            CusName = ro.Customer.Name,
            CusPhone = ro.Customer.Phone,
            RONo = ro.Code,
            ROId = ro.Id,
            CheckInDate = ro.IntakeAt ?? ro.CreatedAt,
            ActualDeliveryDate = ro.FinishedAt,
            Odometer = ro.Odometer,
            ServiceAdvisor = string.IsNullOrWhiteSpace(ro.CreatedBy) ? "CVDV Tiếp nhận" : ro.CreatedBy,
            Technician = ro.Technician,
            CustomerRequest = !string.IsNullOrWhiteSpace(ro.IntakeNote) ? ro.IntakeNote : "Bảo dưỡng & sửa chữa định kỳ",
            CarStatus = "Tiếp nhận xe vào xưởng theo quy trình chuẩn",
            RepairResult = ro.Status == ROStatus.Finished ? "Đã sửa xong và nghiệm thu bàn giao xe hoàn hảo" : "Đang thực hiện dịch vụ",
            TotalLaborAmount = ro.LaborTotal,
            TotalPartAmount = ro.PartTotal,
            TotalAmount = ro.Total,
            FlagClaim = ro.WarrantyReports.Any(),
            ClaimNo = ro.WarrantyReports.FirstOrDefault()?.ReportNo,
            ClaimStatus = ro.WarrantyReports.FirstOrDefault() != null ? Ui.WarrantyStatus(ro.WarrantyReports.First().Status).code : null,
            CreatedBy = "sync_ro",
            CreatedAt = DateTime.Now
        };

        foreach (var line in ro.Lines)
        {
            record.Items.Add(new DealerHistoryItem
            {
                ItemType = line.Type,
                Code = line.Type == LineType.Part ? (line.Part?.Code ?? "PRT") : (line.ServiceItem?.Code ?? "LABOR"),
                Name = line.Name,
                Unit = line.Type == LineType.Part ? (line.Part?.Unit ?? "Cái") : "Giờ",
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                Amount = line.Amount,
                ExpenseType = line.ExpenseType,
                Technician = ro.Technician,
                Result = "Đạt tiêu chuẩn kỹ thuật",
                Remark = line.Type == LineType.Part ? (line.Part?.Location) : null
            });
        }

        db.DealerHistoryRecords.Add(record);
        await db.SaveChangesAsync();
        return (true, $"Đã đồng bộ Lệnh sửa chữa {ro.Code} vào Hệ thống tra cứu lịch sử sửa chữa toàn quốc.", record.Id);
    }

    public async Task<(bool ok, string msg)> DeleteDealerHistoryRecordAsync(int id)
    {
        var record = await db.DealerHistoryRecords.FirstOrDefaultAsync(r => r.Id == id);
        if (record == null) return (false, "Không tìm thấy hồ sơ lịch sử.");

        db.DealerHistoryRecords.Remove(record);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa hồ sơ lịch sử sửa chữa {record.RecordNo}.");
    }

    public Task<List<string>> GetDistinctDealerCodesAsync() =>
        db.DealerHistoryRecords
            .Select(r => r.DealerName)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

    public async Task<List<Car>> CarsWithPlateOrVinAsync(string? q = null)
    {
        var query = db.Cars.Include(c => c.Customer).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = query.Where(c => c.Plate.ToLower().Contains(s) || (c.Vin != null && c.Vin.ToLower().Contains(s)) || c.Customer.Name.ToLower().Contains(s));
        }
        return await query.OrderBy(c => c.Plate).Take(20).ToListAsync();
    }

    // =========================================================================
    // QUẢN LÝ CÔNG NỢ BẢO HIỂM XE & BỒI THƯỜNG (Ser_InsuranceDebit, Ser_InsuranceDebitPayment / MH 55)
    // =========================================================================

    public async Task<List<InsuranceCompanyDebitSummaryDto>> InsuranceCompanyDebitSummariesAsync(string? q, bool? onlyHasDebit)
    {
        var companies = await db.InsuranceCompanies
            .Include(c => c.Debits)
            .Include(c => c.Payments)
            .Where(c => c.IsActive)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            companies = companies.Where(c => c.InsName.ToLower().Contains(term)
                || c.InsNo.ToLower().Contains(term)
                || (c.Phone != null && c.Phone.Contains(term))
                || (c.Hotline != null && c.Hotline.Contains(term))
                || (c.Email != null && c.Email.ToLower().Contains(term))).ToList();
        }

        var summaries = companies.Select(c =>
        {
            var validDebits = c.Debits.Where(d => d.Status != InsuranceDebitStatus.Cancelled).ToList();
            var totalDebit = validDebits.Sum(d => d.DebitAmount);
            var totalPaid = validDebits.Sum(d => d.PaidAmount);
            var activeCount = validDebits.Count(d => d.Status == InsuranceDebitStatus.Active && d.RemainAmount > 0);
            var overdueCount = validDebits.Count(d => d.IsOverdue);
            var lastDebit = validDebits.OrderByDescending(d => d.DebitDate).FirstOrDefault()?.DebitDate;
            var lastPayment = c.Payments.Where(p => p.Status == InsuranceDebitPaymentStatus.Confirmed).OrderByDescending(p => p.PaymentDate).FirstOrDefault()?.PaymentDate;

            return new InsuranceCompanyDebitSummaryDto
            {
                InsuranceCompanyId = c.Id,
                InsNo = c.InsNo,
                InsName = c.InsName,
                Address = c.Address,
                Phone = c.Phone,
                Email = c.Email,
                TaxCode = c.TaxCode,
                Hotline = c.Hotline,
                TotalDebitAmount = totalDebit,
                TotalPaidAmount = totalPaid,
                ActiveDebitCount = activeCount,
                OverdueDebitCount = overdueCount,
                LastDebitDate = lastDebit,
                LastPaymentDate = lastPayment
            };
        });

        if (onlyHasDebit == true)
            summaries = summaries.Where(s => s.HasDebit);

        return summaries.OrderByDescending(s => s.RemainingDebit).ThenBy(s => s.InsName).ToList();
    }

    public async Task<(InsuranceCompany company, InsuranceCompanyDebitSummaryDto summary, List<InsuranceDebit> debits, List<InsuranceDebitPayment> payments)> GetInsuranceCompanyDebitProfileAsync(int companyId)
    {
        var company = await db.InsuranceCompanies
            .Include(c => c.Debits).ThenInclude(d => d.RO)
            .Include(c => c.Debits).ThenInclude(d => d.InsuranceClaim)
            .Include(c => c.Debits).ThenInclude(d => d.Payments)
            .Include(c => c.Payments).ThenInclude(p => p.InsuranceDebit)
            .FirstOrDefaultAsync(c => c.Id == companyId);

        if (company == null)
            throw new InvalidOperationException("Không tìm thấy Hãng bảo hiểm.");

        var debits = company.Debits.OrderByDescending(d => d.DebitDate).ThenByDescending(d => d.CreatedAt).ToList();
        var payments = company.Payments.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.CreatedAt).ToList();

        var validDebits = debits.Where(d => d.Status != InsuranceDebitStatus.Cancelled).ToList();
        var totalDebit = validDebits.Sum(d => d.DebitAmount);
        var totalPaid = validDebits.Sum(d => d.PaidAmount);

        var summary = new InsuranceCompanyDebitSummaryDto
        {
            InsuranceCompanyId = company.Id,
            InsNo = company.InsNo,
            InsName = company.InsName,
            Address = company.Address,
            Phone = company.Phone,
            Email = company.Email,
            TaxCode = company.TaxCode,
            Hotline = company.Hotline,
            TotalDebitAmount = totalDebit,
            TotalPaidAmount = totalPaid,
            ActiveDebitCount = validDebits.Count(d => d.Status == InsuranceDebitStatus.Active && d.RemainAmount > 0),
            OverdueDebitCount = validDebits.Count(d => d.IsOverdue),
            LastDebitDate = validDebits.OrderByDescending(d => d.DebitDate).FirstOrDefault()?.DebitDate,
            LastPaymentDate = payments.Where(p => p.Status == InsuranceDebitPaymentStatus.Confirmed).OrderByDescending(p => p.PaymentDate).FirstOrDefault()?.PaymentDate
        };

        return (company, summary, debits, payments);
    }

    public async Task<List<InsuranceDebit>> InsuranceDebitsAsync(int? companyId, InsuranceDebitStatus? status, InsuranceDebitType? type, string? q, bool? isOverdue, DateTime? fromDate, DateTime? toDate)
    {
        var query = db.InsuranceDebits
            .Include(d => d.InsuranceCompany)
            .Include(d => d.RO)
            .Include(d => d.InsuranceClaim)
            .Include(d => d.Payments)
            .AsQueryable();

        if (companyId.HasValue && companyId.Value > 0)
            query = query.Where(d => d.InsuranceCompanyId == companyId.Value);

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        if (type.HasValue)
            query = query.Where(d => d.DebitType == type.Value);

        if (fromDate.HasValue)
            query = query.Where(d => d.DebitDate >= fromDate.Value.Date);

        if (toDate.HasValue)
            query = query.Where(d => d.DebitDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(d => d.DebitNo.ToLower().Contains(term)
                || (d.RONo != null && d.RONo.ToLower().Contains(term))
                || (d.PlateNo != null && d.PlateNo.ToLower().Contains(term))
                || (d.CustomerName != null && d.CustomerName.ToLower().Contains(term))
                || (d.ClaimNo != null && d.ClaimNo.ToLower().Contains(term))
                || (d.PolicyNo != null && d.PolicyNo.ToLower().Contains(term))
                || d.InsName.ToLower().Contains(term)
                || (d.Description != null && d.Description.ToLower().Contains(term)));
        }

        var list = await query.OrderByDescending(d => d.DebitDate).ThenByDescending(d => d.Id).ToListAsync();

        if (isOverdue == true)
            list = list.Where(d => d.IsOverdue).ToList();

        return list;
    }

    public Task<InsuranceDebit?> GetInsuranceDebitAsync(int id) =>
        db.InsuranceDebits
            .Include(d => d.InsuranceCompany)
            .Include(d => d.InsuranceContract)
            .Include(d => d.RO).ThenInclude(r => r!.Car)
            .Include(d => d.RO).ThenInclude(r => r!.Customer)
            .Include(d => d.InsuranceClaim)
            .Include(d => d.Payments)
            .FirstOrDefaultAsync(d => d.Id == id);

    public Task<InsuranceDebitPayment?> GetInsuranceDebitPaymentAsync(int paymentId) =>
        db.InsuranceDebitPayments
            .Include(p => p.InsuranceCompany)
            .Include(p => p.InsuranceDebit).ThenInclude(d => d!.RO)
            .Include(p => p.InsuranceDebit).ThenInclude(d => d!.InsuranceClaim)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

    public async Task<List<InsuranceDebitPayment>> InsuranceDebitPaymentsAsync(int? companyId, int? debitId, DateTime? fromDate, DateTime? toDate)
    {
        var query = db.InsuranceDebitPayments
            .Include(p => p.InsuranceCompany)
            .Include(p => p.InsuranceDebit)
            .AsQueryable();

        if (companyId.HasValue && companyId.Value > 0)
            query = query.Where(p => p.InsuranceCompanyId == companyId.Value);

        if (debitId.HasValue && debitId.Value > 0)
            query = query.Where(p => p.InsuranceDebitId == debitId.Value);

        if (fromDate.HasValue)
            query = query.Where(p => p.PaymentDate >= fromDate.Value.Date);

        if (toDate.HasValue)
            query = query.Where(p => p.PaymentDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

        return await query.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.Id).ToListAsync();
    }

    public async Task<int> CreateInsuranceDebitAsync(InsuranceDebit debit)
    {
        if (debit.DebitAmount <= 0)
            throw new InvalidOperationException("Số tiền công nợ bồi thường bảo hiểm phải lớn hơn 0.");

        if (debit.InsuranceCompanyId <= 0)
            throw new InvalidOperationException("Vui lòng chọn Hãng bảo hiểm.");

        var company = await db.InsuranceCompanies.FirstOrDefaultAsync(c => c.Id == debit.InsuranceCompanyId);
        if (company == null)
            throw new InvalidOperationException("Không tìm thấy Hãng bảo hiểm được chọn.");

        debit.InsNo = company.InsNo;
        debit.InsName = company.InsName;

        if (string.IsNullOrWhiteSpace(debit.DebitNo))
        {
            var today = DateTime.Today;
            var prefix = $"IDB{today:yyMMdd}-";
            var count = await db.InsuranceDebits.CountAsync(d => d.DebitNo.StartsWith(prefix)) + 1;
            debit.DebitNo = $"{prefix}{count:D3}";
        }

        // Auto populate from RO if linked
        if (debit.ROId.HasValue && debit.ROId.Value > 0)
        {
            var ro = await db.ROs.Include(r => r.Car).Include(r => r.Customer).FirstOrDefaultAsync(r => r.Id == debit.ROId.Value);
            if (ro != null)
            {
                if (string.IsNullOrWhiteSpace(debit.RONo)) debit.RONo = ro.Code;
                if (string.IsNullOrWhiteSpace(debit.PlateNo)) debit.PlateNo = ro.Car?.Plate;
                if (string.IsNullOrWhiteSpace(debit.CarModel)) debit.CarModel = ro.Car?.Model;
                if (string.IsNullOrWhiteSpace(debit.CustomerName)) debit.CustomerName = ro.Customer?.Name;
            }
        }

        // Auto populate from Claim if linked
        if (debit.InsuranceClaimId.HasValue && debit.InsuranceClaimId.Value > 0)
        {
            var claim = await db.InsuranceClaims.Include(c => c.RO).ThenInclude(r => r.Car).FirstOrDefaultAsync(c => c.Id == debit.InsuranceClaimId.Value);
            if (claim != null)
            {
                if (string.IsNullOrWhiteSpace(debit.ClaimNo)) debit.ClaimNo = claim.ClaimNo;
                if (string.IsNullOrWhiteSpace(debit.PolicyNo)) debit.PolicyNo = claim.PolicyNo;
                if (string.IsNullOrWhiteSpace(debit.PlateNo)) debit.PlateNo = claim.RO?.Car?.Plate;
            }
        }

        debit.DebitDate = debit.DebitDate == default ? DateTime.Today : debit.DebitDate;
        debit.DueDate = debit.DueDate ?? debit.DebitDate.AddDays(30);
        debit.Status = InsuranceDebitStatus.Active;
        debit.PaidAmount = 0;
        debit.CreatedAt = DateTime.Now;

        db.InsuranceDebits.Add(debit);
        await db.SaveChangesAsync();
        return debit.Id;
    }

    public async Task<int> CreateInsuranceDebitPaymentAsync(InsuranceDebitPayment payment, bool allocateFifoIfNoDebit = true)
    {
        if (payment.PaymentAmount <= 0)
            throw new InvalidOperationException("Số tiền thu bồi thường bảo hiểm phải lớn hơn 0.");

        if (payment.InsuranceCompanyId <= 0)
            throw new InvalidOperationException("Vui lòng chọn Hãng bảo hiểm.");

        var company = await db.InsuranceCompanies.FirstOrDefaultAsync(c => c.Id == payment.InsuranceCompanyId);
        if (company == null)
            throw new InvalidOperationException("Không tìm thấy Hãng bảo hiểm.");

        payment.InsNo = company.InsNo;
        payment.InsName = company.InsName;

        if (string.IsNullOrWhiteSpace(payment.PayPersonName))
            payment.PayPersonName = "Đại diện giám định viên / Kế toán " + company.InsName;

        if (string.IsNullOrWhiteSpace(payment.PaymentNo))
        {
            var today = DateTime.Today;
            var prefix = $"IPM{today:yyMMdd}-";
            var count = await db.InsuranceDebitPayments.CountAsync(p => p.PaymentNo.StartsWith(prefix)) + 1;
            payment.PaymentNo = $"{prefix}{count:D3}";
        }

        payment.PaymentDate = payment.PaymentDate == default ? DateTime.Today : payment.PaymentDate;
        payment.Status = InsuranceDebitPaymentStatus.Confirmed;
        payment.CreatedAt = DateTime.Now;

        // Allocation logic
        if (payment.InsuranceDebitId.HasValue && payment.InsuranceDebitId.Value > 0)
        {
            var debit = await db.InsuranceDebits.FirstOrDefaultAsync(d => d.Id == payment.InsuranceDebitId.Value);
            if (debit == null)
                throw new InvalidOperationException("Không tìm thấy khoản nợ bảo hiểm được chỉ định.");

            if (debit.Status == InsuranceDebitStatus.Cancelled)
                throw new InvalidOperationException("Khoản nợ bảo hiểm này đã bị hủy.");

            debit.PaidAmount += payment.PaymentAmount;
            if (debit.PaidAmount >= debit.DebitAmount)
            {
                debit.Status = InsuranceDebitStatus.Cleared;
                debit.ClearedAt = DateTime.Now;
            }
        }
        else if (allocateFifoIfNoDebit)
        {
            // Tự động phân bổ vào các khoản nợ của Hãng bảo hiểm theo thứ tự hạn thanh toán (FIFO)
            var activeDebits = await db.InsuranceDebits
                .Where(d => d.InsuranceCompanyId == payment.InsuranceCompanyId && d.Status == InsuranceDebitStatus.Active && d.DebitAmount > d.PaidAmount)
                .OrderBy(d => d.DueDate ?? d.DebitDate)
                .ThenBy(d => d.Id)
                .ToListAsync();

            var moneyLeft = payment.PaymentAmount;
            foreach (var d in activeDebits)
            {
                if (moneyLeft <= 0) break;
                var needed = d.DebitAmount - d.PaidAmount;
                var alloc = Math.Min(moneyLeft, needed);
                d.PaidAmount += alloc;
                moneyLeft -= alloc;

                if (d.PaidAmount >= d.DebitAmount)
                {
                    d.Status = InsuranceDebitStatus.Cleared;
                    d.ClearedAt = DateTime.Now;
                }
            }
        }

        db.InsuranceDebitPayments.Add(payment);
        await db.SaveChangesAsync();
        return payment.Id;
    }

    public async Task<(bool ok, string msg)> CancelInsuranceDebitAsync(int id, string? reason)
    {
        var debit = await db.InsuranceDebits.FirstOrDefaultAsync(d => d.Id == id);
        if (debit == null) return (false, "Không tìm thấy khoản nợ bảo hiểm.");

        if (debit.PaidAmount > 0)
            return (false, $"Khoản nợ bảo hiểm đã thu tiền ({debit.PaidAmount:N0} đ), không thể hủy trực tiếp. Vui lòng hủy phiếu thu trước.");

        debit.Status = InsuranceDebitStatus.Cancelled;
        debit.CancelledReason = reason?.Trim() ?? "Hãng bảo hiểm từ chối bồi thường hoặc điều chỉnh lệnh";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            debit.Description = string.IsNullOrWhiteSpace(debit.Description)
                ? $"[Hủy: {reason.Trim()}]"
                : $"{debit.Description}\n[Hủy: {reason.Trim()}]";
        }

        await db.SaveChangesAsync();
        return (true, $"Đã hủy khoản nợ bồi thường bảo hiểm {debit.DebitNo}.");
    }

    public async Task<(bool ok, string msg)> DeleteInsuranceDebitPaymentAsync(int paymentId)
    {
        var payment = await db.InsuranceDebitPayments
            .Include(p => p.InsuranceDebit)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null) return (false, "Không tìm thấy phiếu thu tiền bảo hiểm.");

        if (payment.InsuranceDebit != null)
        {
            payment.InsuranceDebit.PaidAmount = Math.Max(0, payment.InsuranceDebit.PaidAmount - payment.PaymentAmount);
            if (payment.InsuranceDebit.Status == InsuranceDebitStatus.Cleared && payment.InsuranceDebit.PaidAmount < payment.InsuranceDebit.DebitAmount)
            {
                payment.InsuranceDebit.Status = InsuranceDebitStatus.Active;
                payment.InsuranceDebit.ClearedAt = null;
            }
        }

        payment.Status = InsuranceDebitPaymentStatus.Cancelled;
        db.InsuranceDebitPayments.Remove(payment);
        await db.SaveChangesAsync();
        return (true, $"Đã hủy phiếu thu {payment.PaymentNo} và hoàn trả dư nợ bồi thường của Hãng bảo hiểm.");
    }

    public async Task<(bool ok, string msg, int? debitId)> CreateInsuranceDebitFromRoAsync(int roId, int? companyId, decimal? debitAmount, DateTime? dueDate, string? note)
    {
        var ro = await db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Lines)
            .Include(r => r.InsuranceClaims).ThenInclude(c => c.InsuranceCompany)
            .FirstOrDefaultAsync(r => r.Id == roId);

        if (ro == null) return (false, "Không tìm thấy Lệnh sửa chữa.", null);

        var existing = await db.InsuranceDebits.FirstOrDefaultAsync(d => d.ROId == roId && d.Status != InsuranceDebitStatus.Cancelled);
        if (existing != null)
            return (false, $"Lệnh sửa chữa này đã được ghi nhận nợ bảo hiểm ({existing.DebitNo}).", existing.Id);

        var compId = companyId;
        if (!compId.HasValue || compId.Value <= 0)
        {
            var firstClaimComp = ro.InsuranceClaims.FirstOrDefault()?.InsuranceCompanyId;
            if (firstClaimComp.HasValue && firstClaimComp.Value > 0) compId = firstClaimComp;
            else
            {
                var firstComp = await db.InsuranceCompanies.FirstOrDefaultAsync(c => c.IsActive);
                compId = firstComp?.Id;
            }
        }

        if (!compId.HasValue || compId.Value <= 0)
            return (false, "Vui lòng chọn Hãng bảo hiểm bảo lãnh bồi thường.", null);

        var amount = debitAmount ?? ro.InsuranceTotal;
        if (amount <= 0) amount = ro.Total;
        if (amount <= 0)
            return (false, "Lệnh sửa chữa không có chi phí bảo lãnh bảo hiểm phát sinh (> 0 đ).", null);

        var today = DateTime.Today;
        var prefix = $"IDB{today:yyMMdd}-";
        var count = await db.InsuranceDebits.CountAsync(d => d.DebitNo.StartsWith(prefix)) + 1;

        var comp = await db.InsuranceCompanies.FirstOrDefaultAsync(c => c.Id == compId.Value);

        var debit = new InsuranceDebit
        {
            DebitNo = $"{prefix}{count:D3}",
            InsuranceCompanyId = compId.Value,
            InsNo = comp?.InsNo ?? "INS",
            InsName = comp?.InsName ?? "Hãng bảo hiểm",
            ROId = ro.Id,
            RONo = ro.Code,
            PlateNo = ro.Car?.Plate,
            CarModel = ro.Car?.Model,
            CustomerName = ro.Customer?.Name,
            ClaimNo = ro.InsuranceClaims.FirstOrDefault()?.ClaimNo,
            PolicyNo = ro.InsuranceClaims.FirstOrDefault()?.PolicyNo,
            DebitType = InsuranceDebitType.RO,
            Status = InsuranceDebitStatus.Active,
            DebitDate = today,
            DueDate = dueDate ?? today.AddDays(30),
            DebitAmount = amount,
            PaidAmount = 0,
            Description = string.IsNullOrWhiteSpace(note) ? $"Công nợ bồi thường thân vỏ & phụ tùng theo Lệnh sửa chữa {ro.Code} (Biển số: {ro.Car?.Plate})" : note.Trim(),
            CreatedBy = "CVDV",
            CreatedAt = DateTime.Now
        };

        db.InsuranceDebits.Add(debit);
        await db.SaveChangesAsync();
        return (true, $"Đã tạo khoản nợ bảo hiểm {debit.DebitNo} cho RO {ro.Code} thành công.", debit.Id);
    }

    public async Task<(bool ok, string msg, int? debitId)> CreateInsuranceDebitFromClaimAsync(int claimId, DateTime? dueDate, string? note)
    {
        var claim = await db.InsuranceClaims
            .Include(c => c.InsuranceCompany)
            .Include(c => c.RO).ThenInclude(r => r.Car)
            .Include(c => c.RO).ThenInclude(r => r.Customer)
            .FirstOrDefaultAsync(c => c.Id == claimId);

        if (claim == null) return (false, "Không tìm thấy Hồ sơ bồi thường bảo hiểm.", null);

        var existing = await db.InsuranceDebits.FirstOrDefaultAsync(d => d.InsuranceClaimId == claimId && d.Status != InsuranceDebitStatus.Cancelled);
        if (existing != null)
            return (false, $"Hồ sơ bồi thường này đã được ghi nợ trước đó ({existing.DebitNo}).", existing.Id);

        var amount = claim.InsuranceAmount > 0 ? claim.InsuranceAmount : claim.ApprovedAmount;
        if (amount <= 0)
            return (false, "Số tiền bảo hiểm chi trả chưa được phê duyệt (> 0 đ).", null);

        var today = DateTime.Today;
        var prefix = $"IDB{today:yyMMdd}-";
        var count = await db.InsuranceDebits.CountAsync(d => d.DebitNo.StartsWith(prefix)) + 1;

        var debit = new InsuranceDebit
        {
            DebitNo = $"{prefix}{count:D3}",
            InsuranceCompanyId = claim.InsuranceCompanyId,
            InsNo = claim.InsuranceCompany.InsNo,
            InsName = claim.InsuranceCompany.InsName,
            ROId = claim.ROId,
            RONo = claim.RO?.Code,
            PlateNo = claim.RO?.Car?.Plate,
            CarModel = claim.RO?.Car?.Model,
            CustomerName = claim.RO?.Customer?.Name,
            InsuranceClaimId = claim.Id,
            ClaimNo = claim.ClaimNo,
            PolicyNo = claim.PolicyNo,
            DebitType = InsuranceDebitType.Claim,
            Status = InsuranceDebitStatus.Active,
            DebitDate = today,
            DueDate = dueDate ?? today.AddDays(30),
            DebitAmount = amount,
            PaidAmount = 0,
            Description = string.IsNullOrWhiteSpace(note) ? $"Công nợ bồi thường theo Hồ sơ duyệt bảo hiểm {claim.ClaimNo} (Đơn BH: {claim.PolicyNo})" : note.Trim(),
            CreatedBy = "CVDV",
            CreatedAt = DateTime.Now
        };

        db.InsuranceDebits.Add(debit);
        await db.SaveChangesAsync();
        return (true, $"Đã tạo khoản nợ bảo hiểm {debit.DebitNo} theo Hồ sơ {claim.ClaimNo} thành công.", debit.Id);
    }

    public Task<List<InsuranceCompany>> InsuranceCompaniesForDebitSelectAsync() =>
        db.InsuranceCompanies.Where(c => c.IsActive).OrderBy(c => c.InsName).ToListAsync();

    public Task<List<RepairOrder>> ROsForInsuranceDebitSelectAsync() =>
        db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Lines)
            .OrderByDescending(r => r.CreatedAt)
            .Take(30)
            .ToListAsync();

    public Task<List<InsuranceClaim>> ClaimsForInsuranceDebitSelectAsync() =>
        db.InsuranceClaims
            .Include(c => c.InsuranceCompany)
            .Include(c => c.RO).ThenInclude(r => r.Car)
            .Where(c => c.Status == InsuranceClaimStatus.Approved || c.Status == InsuranceClaimStatus.Settled || c.Status == InsuranceClaimStatus.Submitted)
            .OrderByDescending(c => c.CreatedAt)
            .Take(30)
            .ToListAsync();

    // =========================================================================
    // QUẢN LÝ KHÁCH ĐOÀN & HỢP ĐỒNG ĐỘI XE (Ser_CustomerGroup, Ser_CustomerGroupCustomer / MNU_QT_DL_QUANLYKHACHDOAN)
    // =========================================================================

    public async Task<List<CustomerGroupSummaryDto>> CustomerGroupSummariesAsync(string? q, bool? isActive, bool? creditExceededOnly)
    {
        var query = db.CustomerGroups
            .Include(g => g.Members).ThenInclude(m => m.Car)
            .Include(g => g.RepairOrders).ThenInclude(r => r.Lines)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = query.Where(g => g.GroupNo.ToLower().Contains(s)
                || g.GroupName.ToLower().Contains(s)
                || (g.TaxCode != null && g.TaxCode.ToLower().Contains(s))
                || (g.ContactPerson != null && g.ContactPerson.ToLower().Contains(s))
                || (g.ContactPhone != null && g.ContactPhone.ToLower().Contains(s))
                || g.Members.Any(m => m.PlateNo.ToLower().Contains(s)));
        }

        if (isActive.HasValue)
            query = query.Where(g => g.IsActive == isActive.Value);

        var groups = await query.OrderByDescending(g => g.CreatedAt).ToListAsync();

        var result = new List<CustomerGroupSummaryDto>();
        foreach (var g in groups)
        {
            var carIds = g.Members.Where(m => m.IsActive).Select(m => m.CarId).ToList();
            var customerIds = g.Members.Where(m => m.IsActive && m.CustomerId.HasValue).Select(m => m.CustomerId!.Value).ToList();

            var currentDebt = await db.CusDebits
                .Where(d => d.Status == CusDebitStatus.Active && (customerIds.Contains(d.CustomerId) || (d.CarId.HasValue && carIds.Contains(d.CarId.Value))))
                .SumAsync(d => d.DebitAmount - d.PaidAmount);

            var activeRos = g.RepairOrders.Where(r => r.Status != ROStatus.Rejected).ToList();
            var totalRev = activeRos.Sum(r => r.Total);
            var totalDisc = g.RepairOrders.Sum(r => r.CustomerGroupDiscountAmount);

            var summary = new CustomerGroupSummaryDto
            {
                Id = g.Id,
                GroupNo = g.GroupNo,
                GroupName = g.GroupName,
                TaxCode = g.TaxCode,
                Address = g.Address,
                Telephone = g.Telephone,
                ContactPerson = g.ContactPerson,
                ContactPhone = g.ContactPhone,
                DiscountPercentLabor = g.DiscountPercentLabor,
                DiscountPercentPart = g.DiscountPercentPart,
                CreditLimit = g.CreditLimit,
                PaymentTermDays = g.PaymentTermDays,
                ContractNo = g.ContractNo,
                ContractStartDate = g.ContractStartDate,
                ContractEndDate = g.ContractEndDate,
                IsActive = g.IsActive,
                MemberCount = g.Members.Count(m => m.IsActive),
                ROCount = g.RepairOrders.Count,
                TotalRevenue = totalRev,
                TotalDiscountGiven = totalDisc,
                CurrentDebt = currentDebt
            };

            if (creditExceededOnly == true && !summary.IsCreditExceeded)
                continue;

            result.Add(summary);
        }

        return result;
    }

    public Task<List<CustomerGroup>> CustomerGroupsAsync(string? q, bool? isActive)
    {
        var query = db.CustomerGroups.Include(g => g.Members).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = query.Where(g => g.GroupNo.ToLower().Contains(s) || g.GroupName.ToLower().Contains(s));
        }
        if (isActive.HasValue) query = query.Where(g => g.IsActive == isActive.Value);
        return query.OrderBy(g => g.GroupName).ToListAsync();
    }

    public Task<CustomerGroup?> GetCustomerGroupAsync(int id) =>
        db.CustomerGroups
            .Include(g => g.Members.OrderBy(m => m.PlateNo)).ThenInclude(m => m.Car).ThenInclude(c => c.Customer)
            .Include(g => g.Members).ThenInclude(m => m.Customer)
            .Include(g => g.RepairOrders.OrderByDescending(r => r.CreatedAt).Take(30)).ThenInclude(r => r.Car)
            .Include(g => g.RepairOrders).ThenInclude(r => r.Lines)
            .FirstOrDefaultAsync(g => g.Id == id);

    public Task<CustomerGroup?> GetCustomerGroupByGroupNoAsync(string groupNo) =>
        db.CustomerGroups
            .Include(g => g.Members).ThenInclude(m => m.Car)
            .FirstOrDefaultAsync(g => g.GroupNo == groupNo);

    public async Task<int> CreateCustomerGroupAsync(CustomerGroup group)
    {
        if (string.IsNullOrWhiteSpace(group.GroupName))
            throw new InvalidOperationException("Tên khách đoàn không được để trống.");

        if (string.IsNullOrWhiteSpace(group.GroupNo))
        {
            var count = await db.CustomerGroups.CountAsync() + 1;
            group.GroupNo = $"KD{DateTime.Now:yyMM}-{count:D3}";
        }
        else
        {
            group.GroupNo = group.GroupNo.Trim().ToUpperInvariant();
        }

        var exists = await db.CustomerGroups.AnyAsync(g => g.GroupNo == group.GroupNo);
        if (exists) throw new InvalidOperationException($"Mã khách đoàn '{group.GroupNo}' đã tồn tại trong hệ thống.");

        group.CreatedAt = DateTime.Now;
        db.CustomerGroups.Add(group);
        await db.SaveChangesAsync();
        return group.Id;
    }

    public async Task<(bool ok, string msg)> UpdateCustomerGroupAsync(int id, CustomerGroup input)
    {
        var group = await db.CustomerGroups.FirstOrDefaultAsync(g => g.Id == id);
        if (group == null) return (false, "Không tìm thấy khách đoàn.");

        group.GroupName = input.GroupName.Trim();
        group.TaxCode = input.TaxCode?.Trim();
        group.Address = input.Address?.Trim();
        group.Telephone = input.Telephone?.Trim();
        group.Fax = input.Fax?.Trim();
        group.Email = input.Email?.Trim();
        group.ContactPerson = input.ContactPerson?.Trim();
        group.ContactPhone = input.ContactPhone?.Trim();
        group.Description = input.Description?.Trim();
        group.IsActive = input.IsActive;
        group.DiscountPercentLabor = input.DiscountPercentLabor;
        group.DiscountPercentPart = input.DiscountPercentPart;
        group.CreditLimit = input.CreditLimit;
        group.PaymentTermDays = input.PaymentTermDays > 0 ? input.PaymentTermDays : 30;
        group.ContractNo = input.ContractNo?.Trim();
        group.ContractStartDate = input.ContractStartDate;
        group.ContractEndDate = input.ContractEndDate;
        group.UpdatedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return (true, "Đã cập nhật thông tin khách đoàn thành công.");
    }

    public async Task<(bool ok, string msg)> DeleteCustomerGroupAsync(int id)
    {
        var group = await db.CustomerGroups
            .Include(g => g.RepairOrders)
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null) return (false, "Không tìm thấy khách đoàn.");
        if (group.RepairOrders.Count > 0)
        {
            group.IsActive = false;
            group.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
            return (true, "Khách đoàn đã có Lệnh sửa chữa nên đã được chuyển sang trạng thái Tạm dừng (Inactive).");
        }

        db.CustomerGroupMembers.RemoveRange(group.Members);
        db.CustomerGroups.Remove(group);
        await db.SaveChangesAsync();
        return (true, "Đã xóa khách đoàn thành công.");
    }

    public async Task<(bool ok, string msg, int? memberId)> AddMemberToCustomerGroupAsync(int groupId, int carId, string? driverName, string? driverPhone, string? note)
    {
        var group = await db.CustomerGroups.FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null) return (false, "Không tìm thấy khách đoàn.", null);

        var car = await db.Cars.Include(c => c.Customer).FirstOrDefaultAsync(c => c.Id == carId);
        if (car == null) return (false, "Không tìm thấy thông tin xe.", null);

        var existingMember = await db.CustomerGroupMembers.FirstOrDefaultAsync(m => m.CustomerGroupId == groupId && m.CarId == carId);
        if (existingMember != null)
        {
            if (existingMember.IsActive)
                return (false, $"Xe biển số {car.Plate} đã có trong danh sách đoàn.", existingMember.Id);

            existingMember.IsActive = true;
            existingMember.DriverName = driverName ?? existingMember.DriverName;
            existingMember.DriverPhone = driverPhone ?? existingMember.DriverPhone;
            existingMember.Note = note ?? existingMember.Note;
            await db.SaveChangesAsync();
            return (true, $"Đã kích hoạt lại xe {car.Plate} trong đoàn {group.GroupName}.", existingMember.Id);
        }

        var member = new CustomerGroupMember
        {
            CustomerGroupId = groupId,
            CarId = carId,
            CustomerId = car.CustomerId,
            PlateNo = car.Plate,
            DriverName = driverName?.Trim() ?? car.Customer?.Name,
            DriverPhone = driverPhone?.Trim() ?? car.Customer?.Phone,
            Note = note?.Trim(),
            JoinedDate = DateTime.Now,
            IsActive = true
        };

        db.CustomerGroupMembers.Add(member);

        if (car.Customer != null && !car.Customer.CustomerGroupId.HasValue)
        {
            car.Customer.CustomerGroupId = groupId;
        }

        await db.SaveChangesAsync();
        return (true, $"Đã thêm xe {car.Plate} vào khách đoàn {group.GroupName}.", member.Id);
    }

    public async Task<(bool ok, string msg)> RemoveMemberFromCustomerGroupAsync(int memberId)
    {
        var member = await db.CustomerGroupMembers.Include(m => m.Car).FirstOrDefaultAsync(m => m.Id == memberId);
        if (member == null) return (false, "Không tìm thấy xe trong đoàn.");

        db.CustomerGroupMembers.Remove(member);
        await db.SaveChangesAsync();
        return (true, $"Đã đưa xe {member.PlateNo} ra khỏi khách đoàn.");
    }

    public Task<CustomerGroupMember?> CheckCarCustomerGroupAsync(int carId) =>
        db.CustomerGroupMembers
            .Include(m => m.CustomerGroup)
            .Include(m => m.Car).ThenInclude(c => c.Customer)
            .FirstOrDefaultAsync(m => m.CarId == carId && m.IsActive && m.CustomerGroup.IsActive);

    public Task<CustomerGroupMember?> CheckPlateCustomerGroupAsync(string plate)
    {
        var clean = plate.Trim().ToUpperInvariant();
        return db.CustomerGroupMembers
            .Include(m => m.CustomerGroup)
            .Include(m => m.Car).ThenInclude(c => c.Customer)
            .FirstOrDefaultAsync(m => m.PlateNo.ToUpper() == clean && m.IsActive && m.CustomerGroup.IsActive);
    }

    public async Task<(bool ok, string msg, decimal discountAmount)> ApplyCustomerGroupDiscountToRoAsync(int roId, int groupId)
    {
        var ro = await db.ROs
            .Include(r => r.Lines)
            .Include(r => r.CustomerGroup)
            .FirstOrDefaultAsync(r => r.Id == roId);

        if (ro == null) return (false, "Không tìm thấy Lệnh sửa chữa (RO).", 0);
        if (ro.Status is ROStatus.Finished or ROStatus.Paid or ROStatus.Rejected)
            return (false, "Lệnh sửa chữa đã hoàn tất hoặc bị hủy — không thể thay đổi chiết khấu.", 0);

        var group = await db.CustomerGroups.FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null) return (false, "Không tìm thấy khách đoàn.", 0);
        if (!group.IsActive) return (false, "Khách đoàn đang ở trạng thái Tạm dừng.", 0);

        var customerLaborLines = ro.Lines.Where(l => l.Type == LineType.Labor && l.ExpenseType == ExpenseType.Customer).ToList();
        var customerPartLines = ro.Lines.Where(l => l.Type == LineType.Part && l.ExpenseType == ExpenseType.Customer).ToList();

        var laborTotal = customerLaborLines.Sum(l => l.Amount);
        var partTotal = customerPartLines.Sum(l => l.Amount);

        var laborDiscount = laborTotal * (group.DiscountPercentLabor / 100m);
        var partDiscount = partTotal * (group.DiscountPercentPart / 100m);
        var discountTotal = Math.Round(laborDiscount + partDiscount, 0);

        ro.CustomerGroupId = group.Id;
        ro.CustomerGroupDiscountAmount = discountTotal;

        await db.SaveChangesAsync();
        return (true, $"Đã áp dụng chiết khấu đoàn '{group.GroupName}': Tiền công -{group.DiscountPercentLabor}% ({laborDiscount:N0}đ), Phụ tùng -{group.DiscountPercentPart}% ({partDiscount:N0}đ). Tổng giảm: {discountTotal:N0}đ.", discountTotal);
    }

    public async Task<List<Car>> CarsForCustomerGroupSelectAsync(int? currentGroupId = null)
    {
        var query = db.Cars.Include(c => c.Customer).AsQueryable();
        if (currentGroupId.HasValue)
        {
            var existingCarIds = await db.CustomerGroupMembers
                .Where(m => m.CustomerGroupId == currentGroupId.Value && m.IsActive)
                .Select(m => m.CarId)
                .ToListAsync();
            query = query.Where(c => !existingCarIds.Contains(c.Id));
        }
        return await query.OrderBy(c => c.Plate).ToListAsync();
    }

    // =========================================================================
    // QUẢN LÝ ĐỀ NGHỊ CUNG CẤP GIÁ PHỤ TÙNG NCC TST / HTC (Req_PartPrice / Req_PartPriceDtl)
    // =========================================================================

    public async Task<List<PartPriceRequest>> PartPriceRequestsAsync(DMSReqPartPriceStatus? dmsStatus, TSTReqPartPriceStatus? tstStatus, string? q, DateTime? fromDate, DateTime? toDate)
    {
        var query = db.PartPriceRequests
            .Include(r => r.RO).ThenInclude(ro => ro!.Car)
            .Include(r => r.Items).ThenInclude(i => i.Part)
            .AsQueryable();

        if (dmsStatus.HasValue)
            query = query.Where(r => r.DMSStatus == dmsStatus.Value);

        if (tstStatus.HasValue)
            query = query.Where(r => r.TSTStatus == tstStatus.Value);

        if (fromDate.HasValue)
            query = query.Where(r => r.CreatedAt.Date >= fromDate.Value.Date);

        if (toDate.HasValue)
            query = query.Where(r => r.CreatedAt.Date <= toDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = query.Where(r =>
                r.ReqPartPriceNo.ToLower().Contains(s) ||
                r.Description.ToLower().Contains(s) ||
                (r.VIN != null && r.VIN.ToLower().Contains(s)) ||
                (r.CarModel != null && r.CarModel.ToLower().Contains(s)) ||
                (r.TSTReqPartPriceID != null && r.TSTReqPartPriceID.ToLower().Contains(s)) ||
                (r.RO != null && r.RO.Code.ToLower().Contains(s)) ||
                r.Items.Any(i => i.DMSPartCode.ToLower().Contains(s) || i.VieName.ToLower().Contains(s) || (i.TSTPartCode != null && i.TSTPartCode.ToLower().Contains(s))));
        }

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public Task<PartPriceRequest?> GetPartPriceRequestAsync(int id) =>
        db.PartPriceRequests
            .Include(r => r.RO).ThenInclude(ro => ro!.Car)
            .Include(r => r.RO).ThenInclude(ro => ro!.Customer)
            .Include(r => r.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(r => r.Id == id);

    public Task<PartPriceRequest?> GetPartPriceRequestByNoAsync(string reqNo)
    {
        var clean = reqNo.Trim().ToUpperInvariant();
        return db.PartPriceRequests
            .Include(r => r.RO).ThenInclude(ro => ro!.Car)
            .Include(r => r.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(r => r.ReqPartPriceNo.ToUpper() == clean);
    }

    public async Task<int> CreatePartPriceRequestAsync(PartPriceRequest request, List<PartPriceRequestLine> items)
    {
        if (items == null || items.Count == 0)
            throw new InvalidOperationException("Phiếu đề nghị giá phải có ít nhất 01 dòng phụ tùng.");

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new InvalidOperationException("Vui lòng nhập lý do / mô tả đề nghị giá.");

        var today = DateTime.Today;
        if (string.IsNullOrWhiteSpace(request.ReqPartPriceNo))
        {
            var prefix = $"RPP{today:yyMMdd}-";
            var count = await db.PartPriceRequests.CountAsync(r => r.ReqPartPriceNo.StartsWith(prefix)) + 1;
            request.ReqPartPriceNo = $"{prefix}{count:D3}";
        }

        request.DealerCode = string.IsNullOrWhiteSpace(request.DealerCode) ? "HTC-CG" : request.DealerCode.Trim();
        request.DealerName = string.IsNullOrWhiteSpace(request.DealerName) ? "Hyundai Cầu Giấy" : request.DealerName.Trim();
        request.DMSStatus = DMSReqPartPriceStatus.Draft;
        request.TSTStatus = TSTReqPartPriceStatus.Pending;
        request.CreatedAt = DateTime.Now;

        // Auto link VIN & Model from RO if provided
        if (request.ROId.HasValue && request.ROId.Value > 0)
        {
            var ro = await db.ROs.Include(r => r.Car).FirstOrDefaultAsync(r => r.Id == request.ROId.Value);
            if (ro != null)
            {
                if (string.IsNullOrWhiteSpace(request.VIN)) request.VIN = ro.Car?.Vin;
                if (string.IsNullOrWhiteSpace(request.CarModel)) request.CarModel = ro.Car?.Model;
            }
        }

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.DMSPartCode))
                throw new InvalidOperationException("Vui lòng nhập Mã phụ tùng yêu cầu (DMSPartCode).");
            if (string.IsNullOrWhiteSpace(item.VieName))
                throw new InvalidOperationException($"Vui lòng nhập Tên phụ tùng tiếng Việt cho mã {item.DMSPartCode}.");
            if (item.Quantity <= 0) item.Quantity = 1;
            if (string.IsNullOrWhiteSpace(item.Unit)) item.Unit = "Cái";
            if (string.IsNullOrWhiteSpace(item.VINCode) && !string.IsNullOrWhiteSpace(request.VIN))
                item.VINCode = request.VIN;

            // Link existing part if found
            if (!item.PartId.HasValue || item.PartId.Value <= 0)
            {
                var cleanCode = item.DMSPartCode.Trim().ToUpperInvariant();
                var p = await db.Parts.FirstOrDefaultAsync(x => x.Code.ToUpper() == cleanCode);
                if (p != null) item.PartId = p.Id;
            }

            item.Status = ReqPartPriceLineStatus.Pending;
            request.Items.Add(item);
        }

        db.PartPriceRequests.Add(request);
        await db.SaveChangesAsync();
        return request.Id;
    }

    public async Task<(bool ok, string msg)> SendPartPriceRequestToTSTAsync(int id)
    {
        var request = await db.PartPriceRequests.Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == id);
        if (request == null) return (false, "Không tìm thấy phiếu đề nghị cung cấp giá.");

        if (request.DMSStatus != DMSReqPartPriceStatus.Draft)
            return (false, "Chỉ gửi được phiếu đang ở trạng thái Mới tạo (Draft).");

        if (request.Items.Count == 0)
            return (false, "Phiếu đề nghị chưa có danh sách phụ tùng.");

        request.DMSStatus = DMSReqPartPriceStatus.Sent;
        request.TSTStatus = TSTReqPartPriceStatus.Processing;
        request.TSTSentDate = DateTime.Now;
        if (string.IsNullOrWhiteSpace(request.TSTReqPartPriceID))
        {
            request.TSTReqPartPriceID = $"TST-PR-{DateTime.Today:yyyyMM}-{new Random().Next(1000, 9999)}";
        }
        request.EstimatedResponseDate = DateTime.Today.AddDays(request.FlagIsCheck ? 1 : 2);

        await db.SaveChangesAsync();
        return (true, $"Đã gửi đề nghị cung cấp giá sang NCC TST/HTC thành công (Mã số tiếp nhận: {request.TSTReqPartPriceID}).");
    }

    public async Task<(bool ok, string msg)> SimulateTSTResponseAsync(int id, List<(int lineId, string? tstPartCode, decimal tstPrice, DateTime dateEffect)> linePrices)
    {
        var request = await db.PartPriceRequests.Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == id);
        if (request == null) return (false, "Không tìm thấy phiếu đề nghị giá.");

        if (request.DMSStatus != DMSReqPartPriceStatus.Sent && request.DMSStatus != DMSReqPartPriceStatus.Draft)
            return (false, "Phiếu đề nghị phải ở trạng thái Đã gửi hoặc Mới tạo để tiếp nhận báo giá từ NCC.");

        if (linePrices == null || linePrices.Count == 0)
            return (false, "Vui lòng nhập đơn giá do NCC phản hồi.");

        foreach (var lp in linePrices)
        {
            var line = request.Items.FirstOrDefault(i => i.Id == lp.lineId);
            if (line != null)
            {
                line.TSTPrice = lp.tstPrice;
                line.TSTPartCode = !string.IsNullOrWhiteSpace(lp.tstPartCode) ? lp.tstPartCode.Trim() : line.DMSPartCode;
                line.DateEffect = lp.dateEffect == default ? DateTime.Today : lp.dateEffect;
                line.Status = lp.tstPrice > 0 ? ReqPartPriceLineStatus.Priced : ReqPartPriceLineStatus.Rejected;
            }
        }

        request.DMSStatus = DMSReqPartPriceStatus.Responded;
        request.TSTStatus = TSTReqPartPriceStatus.Priced;
        request.EffectiveDate = linePrices.FirstOrDefault().dateEffect == default ? DateTime.Today : linePrices.FirstOrDefault().dateEffect;

        await db.SaveChangesAsync();
        return (true, $"NCC TST/HTC đã phản hồi báo giá cho {linePrices.Count} hạng mục phụ tùng của phiếu {request.ReqPartPriceNo}.");
    }

    public async Task<(bool ok, string msg)> ApprovePartPriceRequestAsync(int id, string? approvedBy = null, bool syncToCatalog = true)
    {
        var request = await db.PartPriceRequests.Include(r => r.Items).ThenInclude(i => i.Part).FirstOrDefaultAsync(r => r.Id == id);
        if (request == null) return (false, "Không tìm thấy phiếu đề nghị giá.");

        if (request.DMSStatus != DMSReqPartPriceStatus.Responded)
            return (false, "Chỉ có thể phê duyệt khi Nhà cung cấp đã phản hồi đơn giá (Responded).");

        request.DMSStatus = DMSReqPartPriceStatus.Approved;
        request.ApprovedBy = string.IsNullOrWhiteSpace(approvedBy) ? "Quản lý dịch vụ" : approvedBy.Trim();
        request.ApprovedAt = DateTime.Now;
        request.IsUpdatePrice = syncToCatalog;
        request.UpdatedPriceAt = DateTime.Now;

        // Tự động đồng bộ / cập nhật giá vốn và giá bán niêm yết vào danh mục phụ tùng Part master
        if (syncToCatalog)
        {
            foreach (var line in request.Items.Where(i => i.TSTPrice > 0))
            {
                if (line.Part != null)
                {
                    line.Part.CostPrice = line.TSTPrice;
                    if (line.Part.SalePrice < line.TSTPrice * 1.15m)
                    {
                        line.Part.SalePrice = Math.Round(line.TSTPrice * 1.35m, 0);
                    }
                }
                else
                {
                    var cleanCode = line.DMSPartCode.Trim().ToUpperInvariant();
                    var existing = await db.Parts.FirstOrDefaultAsync(p => p.Code.ToUpper() == cleanCode);
                    if (existing != null)
                    {
                        existing.CostPrice = line.TSTPrice;
                        if (existing.SalePrice < line.TSTPrice * 1.15m)
                        {
                            existing.SalePrice = Math.Round(line.TSTPrice * 1.35m, 0);
                        }
                        line.PartId = existing.Id;
                    }
                    else
                    {
                        var newPart = new Part
                        {
                            Code = line.DMSPartCode.Trim(),
                            Name = line.VieName.Trim(),
                            Unit = line.Unit,
                            CostPrice = line.TSTPrice,
                            SalePrice = Math.Round(line.TSTPrice * 1.35m, 0),
                            InStock = 0,
                            MinStock = 1,
                            Model = request.CarModel ?? "Chung",
                            Location = "K-VOR-CHUYEN",
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        };
                        db.Parts.Add(newPart);
                        await db.SaveChangesAsync();
                        line.PartId = newPart.Id;
                    }
                }
            }
        }

        await db.SaveChangesAsync();
        return (true, $"Đã phê duyệt áp dụng đơn giá NCC cho phiếu {request.ReqPartPriceNo}. {(syncToCatalog ? "Đơn giá đã được đồng bộ vào danh mục phụ tùng kho." : "")}");
    }

    public async Task<(bool ok, string msg, int? orderPartId)> ConvertToOrderPartAsync(int id, string? createdBy = null)
    {
        var request = await db.PartPriceRequests.Include(r => r.Items).ThenInclude(i => i.Part).FirstOrDefaultAsync(r => r.Id == id);
        if (request == null) return (false, "Không tìm thấy phiếu đề nghị giá.", null);

        if (request.DMSStatus != DMSReqPartPriceStatus.Approved)
            return (false, "Chỉ có thể lập Đơn đặt hàng từ phiếu đề nghị giá đã được phê duyệt.", null);

        var pricedLines = request.Items.Where(i => i.TSTPrice > 0).ToList();
        if (pricedLines.Count == 0)
            return (false, "Phiếu đề nghị giá không có hạng mục nào có đơn giá hợp lệ.", null);

        var today = DateTime.Today;
        var prefix = $"PO{today:yyMMdd}-";
        var count = await db.OrderParts.CountAsync(o => o.OrderPartNo.StartsWith(prefix)) + 1;
        var orderNo = $"{prefix}{count:D3}";

        var isVor = pricedLines.Any(i => i.DeliveryForm == PartPriceDeliveryForm.VOR);

        var order = new OrderPart
        {
            OrderPartNo = orderNo,
            SupplierName = "Nhà cung cấp phụ tùng TST Bravo / HTC",
            DeliveryForm = isVor ? OrderPartDeliveryForm.UrgentVOR : OrderPartDeliveryForm.Normal,
            DeliveryLocation = "Kho phụ tùng chính - Hyundai Workshop",
            OrderDate = today,
            EstimatedDeliverDate = today.AddDays(isVor ? 1 : 3),
            VIN = request.VIN,
            ROId = request.ROId,
            Remark = $"Tạo tự động từ Đề nghị báo giá {request.ReqPartPriceNo} (Mã TST: {request.TSTReqPartPriceID}). {request.Description}",
            CreatedBy = createdBy ?? "Thủ kho",
            Status = OrderPartStatus.Pending,
            CreatedAt = DateTime.Now
        };

        foreach (var line in pricedLines)
        {
            var partId = line.PartId;
            if (!partId.HasValue || partId.Value <= 0)
            {
                var cleanCode = line.DMSPartCode.Trim().ToUpperInvariant();
                var p = await db.Parts.FirstOrDefaultAsync(x => x.Code.ToUpper() == cleanCode);
                partId = p?.Id ?? (await db.Parts.Select(x => x.Id).FirstOrDefaultAsync());
            }

            order.Lines.Add(new OrderPartLine
            {
                PartId = partId.GetValueOrDefault(1),
                Quantity = line.Quantity,
                UnitPrice = line.TSTPrice,
                DiscountRate = 0,
                VatPercent = 8,
                ApprovedQuantity = line.Quantity,
                Note = $"{line.DMSPartCode} - {line.VieName} ({line.DeliveryForm})"
            });
        }

        db.OrderParts.Add(order);
        await db.SaveChangesAsync();

        return (true, $"Đã tạo thành công Đơn đặt hàng phụ tùng {order.OrderPartNo} gửi NCC TST/HTC.", order.Id);
    }

    public async Task<(bool ok, string msg)> CancelPartPriceRequestAsync(int id, string? reason = null)
    {
        var request = await db.PartPriceRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (request == null) return (false, "Không tìm thấy phiếu đề nghị giá.");

        if (request.DMSStatus == DMSReqPartPriceStatus.Approved)
            return (false, "Không thể hủy phiếu đã được phê duyệt áp dụng giá.");

        request.DMSStatus = DMSReqPartPriceStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            request.Description = string.IsNullOrWhiteSpace(request.Description)
                ? $"[Đã hủy: {reason.Trim()}]"
                : $"{request.Description}\n[Đã hủy: {reason.Trim()}]";
        }

        await db.SaveChangesAsync();
        return (true, $"Đã hủy phiếu đề nghị giá {request.ReqPartPriceNo}.");
    }

    public async Task<(bool ok, string msg)> DeletePartPriceRequestAsync(int id)
    {
        var request = await db.PartPriceRequests.Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == id);
        if (request == null) return (false, "Không tìm thấy phiếu đề nghị giá.");

        if (request.DMSStatus == DMSReqPartPriceStatus.Approved)
            return (false, "Không thể xóa phiếu đã được phê duyệt giá. Vui lòng giữ lại để đối soát báo giá.");

        db.PartPriceRequestLines.RemoveRange(request.Items);
        db.PartPriceRequests.Remove(request);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa phiếu đề nghị giá {request.ReqPartPriceNo} thành công.");
    }

    public Task<List<RepairOrder>> ROsForPartPriceRequestSelectAsync() =>
        db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Where(r => r.Status != ROStatus.Rejected && r.Status != ROStatus.Finished)
            .OrderByDescending(r => r.CreatedAt)
            .Take(40)
            .ToListAsync();

    // --- Complaint & Diagnostic Errors (Ser_MST_ROComplaintDiagnosticError / MNU_QT_DL_QUANLYMALOIPHANNANVACHANDOAN) ---
    public async Task<List<ComplaintDiagnosticError>> ComplaintDiagnosticErrorsAsync(ComplaintErrorType? type, VehicleSystemGroup? group, string? q, bool? isActive)
    {
        var query = db.ComplaintDiagnosticErrors.AsQueryable();

        if (type.HasValue) query = query.Where(e => e.ErrorType == type.Value);
        if (group.HasValue) query = query.Where(e => e.SystemGroup == group.Value);
        if (isActive.HasValue) query = query.Where(e => e.FlagActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(e => e.ErrorCode.ToLower().Contains(term)
                || e.ErrorName.ToLower().Contains(term)
                || (e.ErrorDesc != null && e.ErrorDesc.ToLower().Contains(term))
                || (e.Remark != null && e.Remark.ToLower().Contains(term)));
        }

        return await query
            .OrderByDescending(e => e.UsageCount)
            .ThenBy(e => e.ErrorCode)
            .ToListAsync();
    }

    public async Task<ComplaintDiagnosticSummaryDto> GetComplaintDiagnosticSummaryAsync()
    {
        var all = await db.ComplaintDiagnosticErrors.ToListAsync();
        return new ComplaintDiagnosticSummaryDto
        {
            TotalErrors = all.Count,
            ComplaintCount = all.Count(e => e.ErrorType == ComplaintErrorType.Complaint),
            DiagnosticCount = all.Count(e => e.ErrorType == ComplaintErrorType.Diagnostic),
            ActiveCount = all.Count(e => e.FlagActive),
            InactiveCount = all.Count(e => !e.FlagActive),
            EngineCount = all.Count(e => e.SystemGroup == VehicleSystemGroup.Engine),
            TransmissionCount = all.Count(e => e.SystemGroup == VehicleSystemGroup.Transmission),
            ChassisCount = all.Count(e => e.SystemGroup == VehicleSystemGroup.Chassis),
            ElectricalCount = all.Count(e => e.SystemGroup == VehicleSystemGroup.Electrical),
            HvacCount = all.Count(e => e.SystemGroup == VehicleSystemGroup.HVAC),
            BodyPaintCount = all.Count(e => e.SystemGroup == VehicleSystemGroup.BodyPaint),
            TotalUsageCount = all.Sum(e => e.UsageCount),
            TopUsedErrors = all.OrderByDescending(e => e.UsageCount).Take(5).ToList()
        };
    }

    public Task<ComplaintDiagnosticError?> GetComplaintDiagnosticErrorAsync(int id) =>
        db.ComplaintDiagnosticErrors.FirstOrDefaultAsync(e => e.Id == id);

    public Task<ComplaintDiagnosticError?> GetComplaintDiagnosticErrorByCodeAsync(string code)
    {
        var cleanCode = code.Trim().ToUpperInvariant();
        return db.ComplaintDiagnosticErrors.FirstOrDefaultAsync(e => e.ErrorCode.ToUpper() == cleanCode);
    }

    public async Task<int> CreateComplaintDiagnosticErrorAsync(ComplaintDiagnosticError error)
    {
        if (string.IsNullOrWhiteSpace(error.ErrorCode))
            throw new ArgumentException("Mã lỗi (ErrorCode) không được để trống.");

        if (string.IsNullOrWhiteSpace(error.ErrorName))
            throw new ArgumentException("Tên mã lỗi (ErrorName) không được để trống.");

        error.ErrorCode = error.ErrorCode.Trim().ToUpperInvariant();
        error.ErrorName = error.ErrorName.Trim();
        if (error.ErrorDesc != null) error.ErrorDesc = error.ErrorDesc.Trim();
        if (error.Remark != null) error.Remark = error.Remark.Trim();

        var exists = await db.ComplaintDiagnosticErrors.AnyAsync(e => e.ErrorCode == error.ErrorCode);
        if (exists)
            throw new InvalidOperationException($"Mã lỗi '{error.ErrorCode}' đã tồn tại trong danh mục.");

        error.CreatedAt = DateTime.Now;
        db.ComplaintDiagnosticErrors.Add(error);
        await db.SaveChangesAsync();
        return error.Id;
    }

    public async Task<(bool ok, string msg)> UpdateComplaintDiagnosticErrorAsync(int id, ComplaintDiagnosticError input)
    {
        var error = await db.ComplaintDiagnosticErrors.FirstOrDefaultAsync(e => e.Id == id);
        if (error == null) return (false, "Không tìm thấy mã lỗi cần cập nhật.");

        var cleanCode = input.ErrorCode.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(cleanCode)) return (false, "Mã lỗi không được để trống.");
        if (string.IsNullOrWhiteSpace(input.ErrorName)) return (false, "Tên mã lỗi không được để trống.");

        if (error.ErrorCode != cleanCode)
        {
            var exists = await db.ComplaintDiagnosticErrors.AnyAsync(e => e.ErrorCode == cleanCode && e.Id != id);
            if (exists) return (false, $"Mã lỗi '{cleanCode}' đã được sử dụng bởi bản ghi khác.");
            error.ErrorCode = cleanCode;
        }

        error.ErrorName = input.ErrorName.Trim();
        error.ErrorType = input.ErrorType;
        error.SystemGroup = input.SystemGroup;
        error.ErrorDesc = input.ErrorDesc?.Trim();
        error.Remark = input.Remark?.Trim();
        error.FlagActive = input.FlagActive;
        error.UpdatedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật thông tin mã lỗi '{error.ErrorCode}' thành công.");
    }

    public async Task<(bool ok, string msg)> ToggleComplaintDiagnosticErrorActiveAsync(int id)
    {
        var error = await db.ComplaintDiagnosticErrors.FirstOrDefaultAsync(e => e.Id == id);
        if (error == null) return (false, "Không tìm thấy mã lỗi.");

        error.FlagActive = !error.FlagActive;
        error.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();

        var statusText = error.FlagActive ? "Đang hiệu lực" : "Ngừng sử dụng";
        return (true, $"Đã chuyển trạng thái mã lỗi '{error.ErrorCode}' sang '{statusText}'.");
    }

    public async Task<(bool ok, string msg)> DeleteComplaintDiagnosticErrorAsync(int id)
    {
        var error = await db.ComplaintDiagnosticErrors.FirstOrDefaultAsync(e => e.Id == id);
        if (error == null) return (false, "Không tìm thấy mã lỗi.");

        if (error.UsageCount > 0)
        {
            error.FlagActive = false;
            error.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
            return (true, $"Mã lỗi '{error.ErrorCode}' đã được sử dụng {error.UsageCount} lần trong RO/Warranty, đã tự động chuyển sang trạng thái Ngừng sử dụng thay vì xóa.");
        }

        db.ComplaintDiagnosticErrors.Remove(error);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa mã lỗi '{error.ErrorCode}' khỏi hệ thống.");
    }

    public Task<List<ComplaintDiagnosticError>> GetActiveComplaintsAsync(VehicleSystemGroup? group = null)
    {
        var q = db.ComplaintDiagnosticErrors
            .Where(e => e.ErrorType == ComplaintErrorType.Complaint && e.FlagActive);

        if (group.HasValue) q = q.Where(e => e.SystemGroup == group.Value);

        return q.OrderBy(e => e.ErrorCode).ToListAsync();
    }

    public Task<List<ComplaintDiagnosticError>> GetActiveDiagnosticsAsync(VehicleSystemGroup? group = null)
    {
        var q = db.ComplaintDiagnosticErrors
            .Where(e => e.ErrorType == ComplaintErrorType.Diagnostic && e.FlagActive);

        if (group.HasValue) q = q.Where(e => e.SystemGroup == group.Value);

        return q.OrderBy(e => e.ErrorCode).ToListAsync();
    }

    public async Task<(bool ok, string msg)> ApplyErrorToROAsync(int roId, int errorId, string target)
    {
        var ro = await db.ROs.Include(r => r.Car).FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy Lệnh sửa chữa RO.");

        var err = await db.ComplaintDiagnosticErrors.FirstOrDefaultAsync(e => e.Id == errorId);
        if (err == null) return (false, "Không tìm thấy mã lỗi tương ứng.");

        var cleanTarget = (target ?? "PN").Trim().ToUpperInvariant();
        if (cleanTarget == "PN" || (cleanTarget == "AUTO" && err.ErrorType == ComplaintErrorType.Complaint))
        {
            ro.ErrorCodePN = err.ErrorCode;
            if (string.IsNullOrWhiteSpace(ro.IntakeNote))
            {
                ro.IntakeNote = $"{err.ErrorName}. {err.ErrorDesc}".Trim();
            }
            else
            {
                ro.IntakeNote += $" | [{err.ErrorCode}] {err.ErrorName}";
            }
        }
        else if (cleanTarget == "CD" || (cleanTarget == "AUTO" && err.ErrorType == ComplaintErrorType.Diagnostic))
        {
            ro.ErrorCodeCD = err.ErrorCode;
            if (string.IsNullOrWhiteSpace(ro.DiagnosticResult))
            {
                ro.DiagnosticResult = $"{err.ErrorName}. {err.Remark}".Trim();
            }
            else
            {
                ro.DiagnosticResult += $" | [{err.ErrorCode}] {err.ErrorName}";
            }
        }

        err.UsageCount++;
        await db.SaveChangesAsync();
        return (true, $"Đã áp dụng mã lỗi {err.ErrorCode} ({err.ErrorName}) vào Lệnh sửa chữa {ro.Code}.");
    }

    public Task<List<RepairOrder>> ROsForErrorAssignmentAsync() =>
        db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Where(r => r.Status != ROStatus.Rejected && r.Status != ROStatus.Finished && r.Status != ROStatus.Paid)
            .OrderByDescending(r => r.CreatedAt)
            .Take(30)
            .ToListAsync();

    // --- Customer Birthday Care & Loyalty Gifts (Ser_CustomerCareBth) ---
    public async Task<List<CustomerCareBirthday>> CustomerCareBirthdaysAsync(int? month, CustomerCareBirthdayStatus? status, string? q, bool? todayOnly = null)
    {
        var query = db.CustomerCareBirthdays
            .Include(c => c.Customer)
            .Include(c => c.Car)
            .Include(c => c.UsedInRO)
            .Include(c => c.Appointment)
            .AsQueryable();

        if (status.HasValue) query = query.Where(c => c.Status == status.Value);
        if (month.HasValue && month.Value >= 1 && month.Value <= 12)
        {
            query = query.Where(c => c.DateBth.Month == month.Value);
        }
        if (todayOnly == true)
        {
            var tMonth = DateTime.Today.Month;
            var tDay = DateTime.Today.Day;
            query = query.Where(c => c.DateBth.Month == tMonth && c.DateBth.Day == tDay);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(c => c.CareBthNo.ToLower().Contains(kw)
                || c.Customer.Name.ToLower().Contains(kw)
                || (c.Customer.Phone != null && c.Customer.Phone.ToLower().Contains(kw))
                || (c.Car != null && c.Car.Plate.ToLower().Contains(kw))
                || (c.GiftVoucherCode != null && c.GiftVoucherCode.ToLower().Contains(kw)));
        }

        return await query.OrderBy(c => c.DateBth.Month).ThenBy(c => c.DateBth.Day).ToListAsync();
    }

    public Task<CustomerCareBirthday?> GetCustomerCareBirthdayAsync(int id) =>
        db.CustomerCareBirthdays
            .Include(c => c.Customer).ThenInclude(cus => cus.Cars)
            .Include(c => c.Car)
            .Include(c => c.UsedInRO)
            .Include(c => c.Appointment)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<CustomerCareBirthdaySummaryDto> GetCustomerCareBirthdaySummaryAsync()
    {
        var all = await db.CustomerCareBirthdays.ToListAsync();
        var today = DateTime.Today;
        return new CustomerCareBirthdaySummaryDto
        {
            TotalCount = all.Count,
            ThisMonthCount = all.Count(x => x.DateBth.Month == today.Month),
            TodayCount = all.Count(x => x.DateBth.Month == today.Month && x.DateBth.Day == today.Day),
            PendingCount = all.Count(x => x.Status == CustomerCareBirthdayStatus.Pending),
            ContactedCount = all.Count(x => x.Status == CustomerCareBirthdayStatus.Contacted),
            NotContactedCount = all.Count(x => x.Status == CustomerCareBirthdayStatus.NotContacted),
            VouchersIssuedCount = all.Count(x => !string.IsNullOrEmpty(x.GiftVoucherCode)),
            VouchersUsedCount = all.Count(x => x.IsVoucherUsed),
            TotalVoucherValue = all.Where(x => !string.IsNullOrEmpty(x.GiftVoucherCode)).Sum(x => x.GiftVoucherValue)
        };
    }

    public async Task<int> CreateCustomerCareBirthdayAsync(CustomerCareBirthday care)
    {
        var cus = await db.Customers.Include(c => c.Cars).FirstOrDefaultAsync(c => c.Id == care.CustomerId);
        if (cus == null) throw new InvalidOperationException("Khách hàng không tồn tại.");

        if (care.CarId == null || care.CarId <= 0)
        {
            care.CarId = cus.Cars.FirstOrDefault()?.Id;
        }

        if (care.DateOfBirth.HasValue)
        {
            cus.DateOfBirth = care.DateOfBirth.Value;
            var targetYear = care.DateBth != default ? care.DateBth.Year : DateTime.Today.Year;
            care.DateBth = CustomerCareBirthday.CalculateDateBth(care.DateOfBirth.Value, targetYear);
        }

        var count = await db.CustomerCareBirthdays.CountAsync();
        care.CareBthNo = $"BTH{DateTime.Today:yyMMdd}-{(count + 1):D3}";
        if (string.IsNullOrWhiteSpace(care.GiftVoucherCode))
        {
            care.GiftVoucherCode = $"BDAY{care.DateBth.Year}-{cus.Code}";
        }
        if (care.VoucherValidUntil == null)
        {
            care.VoucherValidUntil = new DateTime(care.DateBth.Year, care.DateBth.Month, DateTime.DaysInMonth(care.DateBth.Year, care.DateBth.Month)).AddDays(30);
        }
        care.CreatedAt = DateTime.Now;

        db.CustomerCareBirthdays.Add(care);
        await db.SaveChangesAsync();
        return care.Id;
    }

    public async Task<(int generated, int skipped)> ScanAndGenerateBirthdayCaresAsync(int? year = null, string? createdBy = null)
    {
        var targetYear = year ?? DateTime.Today.Year;
        var customers = await db.Customers.Include(c => c.Cars).Where(c => c.DateOfBirth.HasValue).ToListAsync();
        int generated = 0;
        int skipped = 0;

        foreach (var cus in customers)
        {
            var dateBth = CustomerCareBirthday.CalculateDateBth(cus.DateOfBirth!.Value, targetYear);
            var exists = await db.CustomerCareBirthdays.AnyAsync(b => b.CustomerId == cus.Id && b.DateBth.Year == targetYear);
            if (exists)
            {
                skipped++;
                continue;
            }

            var count = await db.CustomerCareBirthdays.CountAsync() + generated;
            var care = new CustomerCareBirthday
            {
                CareBthNo = $"BTH{DateTime.Today:yyMMdd}-{(count + 1):D3}",
                CustomerId = cus.Id,
                CarId = cus.Cars.FirstOrDefault()?.Id,
                DateOfBirth = cus.DateOfBirth,
                DateBth = dateBth,
                Status = CustomerCareBirthdayStatus.Pending,
                GiftVoucherCode = $"BDAY{targetYear}-{cus.Code}",
                GiftVoucherValue = 300_000m,
                DiscountPercent = 10m,
                VoucherValidUntil = new DateTime(targetYear, dateBth.Month, DateTime.DaysInMonth(targetYear, dateBth.Month)).AddDays(30),
                CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "auto-scanner" : createdBy.Trim(),
                CreatedAt = DateTime.Now
            };

            db.CustomerCareBirthdays.Add(care);
            generated++;
        }

        if (generated > 0)
        {
            await db.SaveChangesAsync();
        }

        return (generated, skipped);
    }

    public async Task<(bool ok, string msg)> UpdateCustomerCareBirthdayContactAsync(int id, CustomerCareBirthdayStatus status, BirthdayContactChannel channel, string? remark, string? giftVoucherCode, decimal giftVoucherValue, decimal discountPercent, DateTime? validUntil, string? contactedBy)
    {
        var care = await db.CustomerCareBirthdays.Include(c => c.Customer).FirstOrDefaultAsync(c => c.Id == id);
        if (care == null) return (false, "Không tìm thấy phiếu CSKH sinh nhật.");

        care.Status = status;
        care.ContactChannel = channel;
        care.ContactDate = DateTime.Now;
        care.ContactedBy = string.IsNullOrWhiteSpace(contactedBy) ? "CSKH" : contactedBy.Trim();
        if (!string.IsNullOrWhiteSpace(remark)) care.Remark = remark.Trim();
        if (!string.IsNullOrWhiteSpace(giftVoucherCode)) care.GiftVoucherCode = giftVoucherCode.Trim();
        if (giftVoucherValue >= 0) care.GiftVoucherValue = giftVoucherValue;
        if (discountPercent >= 0) care.DiscountPercent = discountPercent;
        if (validUntil.HasValue) care.VoucherValidUntil = validUntil.Value;

        care.UpdatedAt = DateTime.Now;
        care.UpdatedBy = care.ContactedBy;
        care.LogLuDateTime = DateTime.Now;
        care.LogLUBy = care.ContactedBy;

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật trạng thái liên hệ: {Ui.CustomerCareBirthdayStatus(care.Status).text} qua {Ui.BirthdayContactChannel(channel).text}.");
    }

    public async Task<(bool ok, string msg, int? appointmentId)> BookAppointmentFromBirthdayCareAsync(int id, DateTime appointmentDate, AppointmentServiceType serviceType, string? note)
    {
        var care = await db.CustomerCareBirthdays.Include(c => c.Customer).Include(c => c.Car).FirstOrDefaultAsync(c => c.Id == id);
        if (care == null) return (false, "Không tìm thấy phiếu CSKH sinh nhật.", null);

        var carId = care.CarId ?? care.Customer.Cars.FirstOrDefault()?.Id;
        if (carId == null) return (false, "Khách hàng chưa có thông tin xe trong hệ thống.", null);

        var appCount = await db.Appointments.CountAsync();
        var app = new Appointment
        {
            AppNo = $"APP{DateTime.Today:yyMMdd}-{(appCount + 1):D3}",
            CarId = carId.Value,
            CustomerId = care.CustomerId,
            AppointmentDate = appointmentDate,
            ServiceType = serviceType,
            Status = AppointmentStatus.Confirmed,
            Advisor = "CVDV Tiếp nhận SN",
            CustomerRequest = $"Bảo dưỡng/Dịch vụ tri ân dịp sinh nhật. Áp dụng Voucher: {care.GiftVoucherCode} (Giảm {care.DiscountPercent}% hoặc {care.GiftVoucherValue:N0}đ)",
            Note = note?.Trim(),
            Source = "CSKH Sinh nhật",
            CreatedBy = care.ContactedBy ?? "CSKH",
            CreatedAt = DateTime.Now,
            CustomerCareBirthdayId = care.Id
        };

        db.Appointments.Add(app);
        await db.SaveChangesAsync();

        care.AppointmentId = app.Id;
        care.Status = CustomerCareBirthdayStatus.Contacted;
        care.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();

        return (true, $"Đã tạo lịch hẹn dịch vụ {app.AppNo} thành công cho xe ngày {appointmentDate:dd/MM/yyyy HH:mm}.", app.Id);
    }

    public async Task<(bool ok, string msg)> ApplyBirthdayVoucherToROAsync(int id, int roId)
    {
        var care = await db.CustomerCareBirthdays.Include(c => c.Customer).FirstOrDefaultAsync(c => c.Id == id);
        if (care == null) return (false, "Không tìm thấy phiếu CSKH sinh nhật.");

        var ro = await db.ROs.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy Lệnh sửa chữa chỉ định.");

        if (ro.Status == ROStatus.Finished || ro.Status == ROStatus.Paid || ro.Status == ROStatus.Rejected)
            return (false, "Không thể áp dụng voucher cho RO đã thanh toán/hoàn tất hoặc bị hủy.");

        decimal discount = 0;
        if (care.DiscountPercent > 0)
        {
            var laborTotal = ro.Lines.Where(l => l.Type == LineType.Labor && l.ExpenseType == ExpenseType.Customer).Sum(l => l.Amount);
            discount = Math.Round(laborTotal * (care.DiscountPercent / 100m), 0);
        }
        if (discount < care.GiftVoucherValue && care.GiftVoucherValue > 0)
        {
            discount = care.GiftVoucherValue;
        }

        ro.BirthdayDiscountAmount = discount;
        ro.BirthdayCareId = care.Id;
        ro.BirthdayVoucherCode = care.GiftVoucherCode;

        care.IsVoucherUsed = true;
        care.UsedInROId = ro.Id;
        care.UpdatedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return (true, $"Đã áp dụng Voucher sinh nhật {care.GiftVoucherCode} vào RO {ro.Code}, giảm trừ {discount:N0}đ.");
    }

    public async Task<(bool ok, string msg)> DeleteCustomerCareBirthdayAsync(int id)
    {
        var care = await db.CustomerCareBirthdays.FirstOrDefaultAsync(c => c.Id == id);
        if (care == null) return (false, "Không tìm thấy phiếu CSKH sinh nhật.");
        if (care.IsVoucherUsed) return (false, "Không thể xóa phiếu sinh nhật có voucher đã sử dụng trong Lệnh sửa chữa.");

        db.CustomerCareBirthdays.Remove(care);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa phiếu CSKH sinh nhật {care.CareBthNo}.");
    }

    public async Task<List<Customer>> CustomersEligibleForBirthdayCareAsync(int year)
    {
        var existingCustomerIds = await db.CustomerCareBirthdays
            .Where(b => b.DateBth.Year == year)
            .Select(b => b.CustomerId)
            .ToListAsync();

        return await db.Customers
            .Include(c => c.Cars)
            .Where(c => !existingCustomerIds.Contains(c.Id))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    // =========================================================================
    // ĐỊNH MỨC GIỜ CÔNG BẢO HÀNH HÃNG FLAT RATE (Ser_MST_ROWarrantyWork & Ser_MST_ROWarrantyType)
    // =========================================================================

    public async Task<List<WarrantyWork>> WarrantyWorksAsync(string? model, WarrantyLaborGroup? group, WarrantyCoverageType? coverage, string? q, bool? isActive)
    {
        var query = db.WarrantyWorks
            .Include(w => w.RepairLines)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(model))
        {
            var m = model.Trim().ToLower();
            query = query.Where(w => w.Model.ToLower().Contains(m));
        }

        if (group.HasValue)
            query = query.Where(w => w.LaborGroup == group.Value);

        if (coverage.HasValue)
            query = query.Where(w => w.CoverageType == coverage.Value);

        if (isActive.HasValue)
            query = query.Where(w => w.FlagActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(w =>
                w.Code.ToLower().Contains(term) ||
                w.Name.ToLower().Contains(term) ||
                w.Model.ToLower().Contains(term) ||
                (w.AppTypeCode != null && w.AppTypeCode.ToLower().Contains(term)) ||
                (w.EngineType != null && w.EngineType.ToLower().Contains(term)) ||
                (w.Remark != null && w.Remark.ToLower().Contains(term)));
        }

        return await query
            .OrderBy(w => w.Model)
            .ThenBy(w => w.LaborGroup)
            .ThenBy(w => w.Code)
            .ToListAsync();
    }

    public async Task<WarrantyWork?> GetWarrantyWorkAsync(int id)
    {
        return await db.WarrantyWorks
            .Include(w => w.RepairLines)
                .ThenInclude(l => l.RO)
                    .ThenInclude(r => r.Car)
            .Include(w => w.RepairLines)
                .ThenInclude(l => l.RO)
                    .ThenInclude(r => r.Customer)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<WarrantyWork?> GetWarrantyWorkByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var c = code.Trim().ToUpper();
        return await db.WarrantyWorks.FirstOrDefaultAsync(w => w.Code.ToUpper() == c);
    }

    public async Task<WarrantyWorkSummaryDto> GetWarrantyWorkSummaryAsync()
    {
        var works = await db.WarrantyWorks
            .Include(w => w.RepairLines)
            .ToListAsync();

        var total = works.Count;
        var active = works.Count(w => w.FlagActive);
        var inactive = total - active;
        var totalModels = works.Select(w => w.Model.Trim().ToLower()).Where(m => !string.IsNullOrEmpty(m)).Distinct().Count();
        var avgHours = total > 0 ? Math.Round(works.Average(w => w.RateHour), 2) : 0m;
        var avgPrice = total > 0 ? Math.Round(works.Average(w => w.Price), 0) : 0m;
        var claims = works.Sum(w => w.RepairLines.Count);

        return new WarrantyWorkSummaryDto
        {
            TotalWorks = total,
            ActiveWorks = active,
            InactiveWorks = inactive,
            TotalModels = totalModels,
            AvgRateHour = avgHours,
            AvgPrice = avgPrice,
            TotalClaimsApplied = claims
        };
    }

    public async Task<int> CreateWarrantyWorkAsync(WarrantyWork work)
    {
        work.Code = work.Code.Trim().ToUpper();
        if (string.IsNullOrWhiteSpace(work.Code))
            throw new ArgumentException("Mã công việc bảo hành không được để trống.");

        var exists = await db.WarrantyWorks.AnyAsync(w => w.Code == work.Code);
        if (exists)
            throw new InvalidOperationException($"Mã công việc bảo hành '{work.Code}' đã tồn tại.");

        if (string.IsNullOrWhiteSpace(work.Name))
            throw new ArgumentException("Tên công việc bảo hành không được để trống.");

        if (string.IsNullOrWhiteSpace(work.Model))
            work.Model = "Tất cả dòng xe";

        if (work.RateHour <= 0) work.RateHour = 1.0m;
        if (work.RatePrice <= 0) work.RatePrice = 300_000m;
        work.Price = Math.Round(work.RateHour * work.RatePrice, 0);

        if (work.VatPercent < 0) work.VatPercent = 8;
        if (work.CreatedAt == default) work.CreatedAt = DateTime.Now;

        db.WarrantyWorks.Add(work);
        await db.SaveChangesAsync();
        return work.Id;
    }

    public async Task<(bool ok, string msg)> UpdateWarrantyWorkAsync(int id, WarrantyWork input)
    {
        var work = await db.WarrantyWorks.FirstOrDefaultAsync(w => w.Id == id);
        if (work == null) return (false, "Không tìm thấy công việc bảo hành.");

        var code = input.Code.Trim().ToUpper();
        if (string.IsNullOrWhiteSpace(code)) return (false, "Mã công việc bảo hành không được để trống.");

        var duplicate = await db.WarrantyWorks.AnyAsync(w => w.Id != id && w.Code == code);
        if (duplicate) return (false, $"Mã công việc '{code}' đã được dùng cho bản ghi khác.");

        work.Code = code;
        work.Name = input.Name.Trim();
        work.Model = string.IsNullOrWhiteSpace(input.Model) ? "Tất cả dòng xe" : input.Model.Trim();
        work.LaborGroup = input.LaborGroup;
        work.CoverageType = input.CoverageType;
        work.AppTypeCode = input.AppTypeCode?.Trim();
        work.EngineType = input.EngineType?.Trim();
        work.RateHour = input.RateHour > 0 ? input.RateHour : 1.0m;
        work.RatePrice = input.RatePrice > 0 ? input.RatePrice : 300_000m;
        work.Price = Math.Round(work.RateHour * work.RatePrice, 0);
        work.VatPercent = input.VatPercent >= 0 ? input.VatPercent : 8;
        work.RequiredPhotos = input.RequiredPhotos?.Trim();
        work.Remark = input.Remark?.Trim();
        work.FlagActive = input.FlagActive;
        work.UpdatedAt = DateTime.Now;
        work.UpdatedBy = input.UpdatedBy ?? "user";

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật công việc bảo hành {work.Code}.");
    }

    public async Task<(bool ok, string msg)> ToggleWarrantyWorkActiveAsync(int id)
    {
        var work = await db.WarrantyWorks.FirstOrDefaultAsync(w => w.Id == id);
        if (work == null) return (false, "Không tìm thấy công việc bảo hành.");

        work.FlagActive = !work.FlagActive;
        work.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return (true, $"Đã {(work.FlagActive ? "kích hoạt" : "ngừng áp dụng")} công việc {work.Code}.");
    }

    public async Task<(bool ok, string msg)> DeleteWarrantyWorkAsync(int id)
    {
        var work = await db.WarrantyWorks
            .Include(w => w.RepairLines)
            .FirstOrDefaultAsync(w => w.Id == id);
        if (work == null) return (false, "Không tìm thấy công việc bảo hành.");

        if (work.RepairLines.Count > 0)
        {
            work.FlagActive = false;
            work.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
            return (true, $"Công việc {work.Code} đã phát sinh {work.RepairLines.Count} dòng sửa chữa trên Lệnh RO — đã chuyển trạng thái ngừng áp dụng (ngưng hiệu lực).");
        }

        db.WarrantyWorks.Remove(work);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa công việc bảo hành {work.Code}.");
    }

    public async Task<(bool ok, string msg, int? lineId)> ApplyWarrantyWorkToRoAsync(int warrantyWorkId, int roId, decimal? customHours, string? note)
    {
        var work = await db.WarrantyWorks.FirstOrDefaultAsync(w => w.Id == warrantyWorkId);
        if (work == null) return (false, "Không tìm thấy công việc bảo hành định mức.", null);

        var ro = await db.ROs
            .Include(r => r.Lines)
            .Include(r => r.Car)
            .FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy Lệnh sửa chữa RO.", null);

        if (ro.Status is ROStatus.Finished or ROStatus.Paid or ROStatus.Rejected or ROStatus.NotResponding)
            return (false, $"RO {ro.Code} ở trạng thái {ro.Status} — không thể thêm công việc mới.", null);

        var hours = customHours.HasValue && customHours.Value > 0 ? customHours.Value : work.RateHour;
        var unitPrice = work.RatePrice;

        var line = new RepairLine
        {
            ROId = roId,
            Type = LineType.Labor,
            ExpenseType = ExpenseType.Warranty,
            WarrantyWorkId = work.Id,
            StdManHour = hours,
            Quantity = hours,
            UnitPrice = unitPrice,
            Name = string.IsNullOrWhiteSpace(note)
                ? $"[BH {work.Code}] {work.Name} ({work.Model})"
                : $"[BH {work.Code}] {work.Name} - {note.Trim()}"
        };

        db.Lines.Add(line);
        await db.SaveChangesAsync();

        return (true, $"Đã áp dụng công việc bảo hành {work.Code} ({hours:N1}h) vào RO {ro.Code} thành công.", line.Id);
    }

    public async Task<List<RepairOrder>> ROsForWarrantyWorkSelectAsync()
    {
        var openStatuses = new[] { ROStatus.Created, ROStatus.Printed, ROStatus.Wait4Part, ROStatus.HasPart, ROStatus.HasRO, ROStatus.InGarage, ROStatus.Repaired, ROStatus.CheckEnd };
        return await db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Where(r => openStatuses.Contains(r.Status))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<string>> DistinctWarrantyModelsAsync()
    {
        return await db.WarrantyWorks
            .Select(w => w.Model.Trim())
            .Where(m => !string.IsNullOrEmpty(m))
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync();
    }

    // =========================================================================
    // THIẾT LẬP CHU KỲ & ĐỊNH MỨC BẢO DƯỠNG ĐỊNH KỲ XE (Ser_MST_ROMaintanceSetting)
    // =========================================================================

    public async Task<List<MaintenanceSetting>> MaintenanceSettingsAsync(int? minKm, int? maxKm, MaintenanceLevel? level, bool? flagWarranty, bool? flagActive, string? q)
    {
        var query = db.MaintenanceSettings
            .Include(m => m.ServicePackage)
            .Include(m => m.RepairOrders)
            .AsQueryable();

        if (minKm.HasValue) query = query.Where(m => m.Km >= minKm.Value);
        if (maxKm.HasValue) query = query.Where(m => m.Km <= maxKm.Value);
        if (level.HasValue) query = query.Where(m => m.Level == level.Value);
        if (flagWarranty.HasValue) query = query.Where(m => m.FlagWarranty == flagWarranty.Value);
        if (flagActive.HasValue) query = query.Where(m => m.FlagActive == flagActive.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(m =>
                m.ROMSID.ToLower().Contains(term) ||
                m.Name.ToLower().Contains(term) ||
                (m.Description != null && m.Description.ToLower().Contains(term)) ||
                (m.RequiredChecklist != null && m.RequiredChecklist.ToLower().Contains(term)));
        }

        return await query.OrderBy(m => m.Km).ToListAsync();
    }

    public async Task<MaintenanceSetting?> GetMaintenanceSettingAsync(int id)
    {
        return await db.MaintenanceSettings
            .Include(m => m.ServicePackage)
                .ThenInclude(p => p!.Items)
            .Include(m => m.RepairOrders)
                .ThenInclude(r => r.Car)
            .Include(m => m.RepairOrders)
                .ThenInclude(r => r.Customer)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<MaintenanceSetting?> GetMaintenanceSettingByRomsIdAsync(string romsId)
    {
        if (string.IsNullOrWhiteSpace(romsId)) return null;
        var r = romsId.Trim().ToUpper();
        return await db.MaintenanceSettings
            .Include(m => m.ServicePackage)
            .FirstOrDefaultAsync(m => m.ROMSID.ToUpper() == r);
    }

    public async Task<MaintenanceSettingSummaryDto> GetMaintenanceSettingSummaryAsync()
    {
        var list = await db.MaintenanceSettings
            .Include(m => m.RepairOrders)
            .ToListAsync();

        var total = list.Count;
        var active = list.Count(m => m.FlagActive);
        var inactive = total - active;
        var warrantyReq = list.Count(m => m.FlagWarranty);
        var linkedPkg = list.Count(m => m.ServicePackageId.HasValue);
        var avgHours = total > 0 ? Math.Round(list.Average(m => m.TakingTimeHours), 1) : 0m;
        var avgCost = total > 0 ? Math.Round(list.Average(m => m.EstimatedCost), 0) : 0m;
        var maxKm = list.Count > 0 ? list.Max(m => m.Km) : 0;
        var totalROs = list.Sum(m => m.RepairOrders.Count);

        return new MaintenanceSettingSummaryDto
        {
            TotalSettings = total,
            ActiveSettings = active,
            InactiveSettings = inactive,
            WarrantyRequiredCount = warrantyReq,
            LinkedPackageCount = linkedPkg,
            AvgLaborHours = avgHours,
            AvgEstimatedCost = avgCost,
            MaxKm = maxKm,
            TotalROsApplied = totalROs
        };
    }

    public async Task<int> CreateMaintenanceSettingAsync(MaintenanceSetting setting)
    {
        setting.ROMSID = setting.ROMSID.Trim().ToUpper();
        if (string.IsNullOrWhiteSpace(setting.ROMSID))
            throw new ArgumentException("Mã thiết lập bảo dưỡng (ROMSID) không được để trống.");

        var existsRomsId = await db.MaintenanceSettings.AnyAsync(m => m.ROMSID == setting.ROMSID);
        if (existsRomsId)
            throw new InvalidOperationException($"Mã thiết lập bảo dưỡng '{setting.ROMSID}' đã tồn tại.");

        if (setting.Km <= 0)
            throw new ArgumentException("Mốc số Kilomet phải lớn hơn 0.");

        var existsKm = await db.MaintenanceSettings.AnyAsync(m => m.Km == setting.Km);
        if (existsKm)
            throw new InvalidOperationException($"Mốc số Kilomet {setting.Km:N0} km đã được thiết lập trước đó.");

        if (string.IsNullOrWhiteSpace(setting.Name))
            setting.Name = $"Bảo dưỡng {setting.LevelName} - {setting.Km:N0} km";

        if (setting.TakingTimeHours <= 0) setting.TakingTimeHours = 1.0m;
        if (setting.EstimatedCost < 0) setting.EstimatedCost = 0m;
        if (setting.CreatedAt == default) setting.CreatedAt = DateTime.Now;
        setting.LogLuDateTime = DateTime.Now;

        db.MaintenanceSettings.Add(setting);
        await db.SaveChangesAsync();
        return setting.Id;
    }

    public async Task<(bool ok, string msg)> UpdateMaintenanceSettingAsync(int id, MaintenanceSetting input)
    {
        var setting = await db.MaintenanceSettings.FirstOrDefaultAsync(m => m.Id == id);
        if (setting == null) return (false, "Không tìm thấy thiết lập bảo dưỡng.");

        var romsId = input.ROMSID.Trim().ToUpper();
        if (string.IsNullOrWhiteSpace(romsId)) return (false, "Mã thiết lập bảo dưỡng (ROMSID) không được để trống.");

        var dupRomsId = await db.MaintenanceSettings.AnyAsync(m => m.Id != id && m.ROMSID == romsId);
        if (dupRomsId) return (false, $"Mã thiết lập '{romsId}' đã được sử dụng cho bản ghi khác.");

        if (input.Km <= 0) return (false, "Mốc số Kilomet phải lớn hơn 0.");

        var dupKm = await db.MaintenanceSettings.AnyAsync(m => m.Id != id && m.Km == input.Km);
        if (dupKm) return (false, $"Mốc {input.Km:N0} km đã tồn tại trong hệ thống.");

        setting.ROMSID = romsId;
        setting.Name = input.Name.Trim();
        setting.Km = input.Km;
        setting.Maintances = input.Maintances;
        setting.Level = input.Level;
        setting.MonthsInterval = input.MonthsInterval > 0 ? input.MonthsInterval : 6;
        setting.TakingTimeHours = input.TakingTimeHours > 0 ? input.TakingTimeHours : 1.0m;
        setting.EstimatedCost = input.EstimatedCost >= 0 ? input.EstimatedCost : 0m;
        setting.ServicePackageId = input.ServicePackageId;
        setting.RequiredChecklist = input.RequiredChecklist?.Trim();
        setting.Description = input.Description?.Trim();
        setting.FlagWarranty = input.FlagWarranty;
        setting.FlagActive = input.FlagActive;
        setting.UpdatedAt = DateTime.Now;
        setting.UpdatedBy = input.UpdatedBy ?? "web";
        setting.LogLuDateTime = DateTime.Now;
        setting.LogLUBy = input.UpdatedBy ?? "web";

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật mốc bảo dưỡng {setting.ROMSID} ({setting.Km:N0} km).");
    }

    public async Task<(bool ok, string msg)> ToggleMaintenanceSettingActiveAsync(int id)
    {
        var setting = await db.MaintenanceSettings.FirstOrDefaultAsync(m => m.Id == id);
        if (setting == null) return (false, "Không tìm thấy thiết lập bảo dưỡng.");

        setting.FlagActive = !setting.FlagActive;
        setting.UpdatedAt = DateTime.Now;
        setting.LogLuDateTime = DateTime.Now;
        await db.SaveChangesAsync();

        return (true, $"Đã {(setting.FlagActive ? "kích hoạt" : "ngừng áp dụng")} mốc bảo dưỡng {setting.ROMSID}.");
    }

    public async Task<(bool ok, string msg)> DeleteMaintenanceSettingAsync(int id)
    {
        var setting = await db.MaintenanceSettings
            .Include(m => m.RepairOrders)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (setting == null) return (false, "Không tìm thấy thiết lập bảo dưỡng.");

        if (setting.RepairOrders.Count > 0)
        {
            setting.FlagActive = false;
            setting.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
            return (true, $"Mốc {setting.ROMSID} đã được áp dụng cho {setting.RepairOrders.Count} Lệnh RO — đã chuyển trạng thái ngừng áp dụng (ngưng hiệu lực).");
        }

        db.MaintenanceSettings.Remove(setting);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa mốc bảo dưỡng {setting.ROMSID}.");
    }

    public async Task<MaintenanceSuggestionDto> SuggestMaintenanceForKmAsync(int km)
    {
        if (km < 0) km = 0;
        var activeSettings = await db.MaintenanceSettings
            .Include(m => m.ServicePackage)
                .ThenInclude(p => p!.Items)
            .Where(m => m.FlagActive)
            .OrderBy(m => m.Km)
            .ToListAsync();

        if (activeSettings.Count == 0)
        {
            return new MaintenanceSuggestionDto
            {
                CurrentKm = km,
                StatusAdvice = "Chưa có cấu hình định mức bảo dưỡng trong hệ thống",
                AdviceNote = "Vui lòng thiết lập danh mục mốc bảo dưỡng trong hệ thống."
            };
        }

        // Tìm mốc gần nhất theo khoảng cách số Km
        var matched = activeSettings
            .OrderBy(s => Math.Abs(s.Km - km))
            .First();

        var next = activeSettings.FirstOrDefault(s => s.Km > km);
        var diff = km - matched.Km;

        string advice;
        string badgeClass;
        bool isCompliant = true;
        string adviceNote;

        if (Math.Abs(diff) <= 500)
        {
            advice = "Đúng hạn bảo dưỡng";
            badgeClass = "bg-success";
            adviceNote = $"Xe hiện đạt {km:N0} km, rất chuẩn với mốc {matched.Km:N0} km ({matched.Name}). Khuyến nghị thực hiện đầy đủ các hạng mục theo tiêu chuẩn hãng.";
        }
        else if (diff > 500)
        {
            advice = $"Quá hạn {diff:N0} km";
            badgeClass = "bg-danger";
            if (diff > 1500 && matched.FlagWarranty)
            {
                isCompliant = false;
                adviceNote = $"CẢNH BÁO: Xe đã vượt mốc {matched.Km:N0} km hơn {diff:N0} km! Theo Chính sách Bảo hành của hãng HTC, việc quá hạn bảo dưỡng vượt quá 1.500 km có thể làm mất quyền lợi bảo hành các cụm chi tiết liên quan nếu xảy ra hư hỏng do dầu mỡ bôi trơn.";
            }
            else
            {
                adviceNote = $"Xe đã chạy vượt mốc {matched.Km:N0} km. Cần thực hiện bảo dưỡng ngay để tránh mài mòn các chi tiết động cơ.";
            }
        }
        else
        {
            advice = $"Sắp đến hạn (còn {Math.Abs(diff):N0} km)";
            badgeClass = "bg-info text-dark";
            adviceNote = $"Xe còn cách mốc {matched.Km:N0} km khoảng {Math.Abs(diff):N0} km. Quý khách có thể thực hiện bảo dưỡng sớm hoặc tiếp tục vận hành thêm.";
        }

        return new MaintenanceSuggestionDto
        {
            CurrentKm = km,
            MatchedSetting = matched,
            NextSetting = next,
            KmDifference = diff,
            StatusAdvice = advice,
            LevelBadgeClass = badgeClass,
            SuggestedPackage = matched.ServicePackage,
            IsWarrantyCompliant = isCompliant,
            AdviceNote = adviceNote
        };
    }

    public async Task<(bool ok, string msg)> ApplyMaintenanceToRoAsync(int settingId, int roId, bool addPackageCombo)
    {
        var setting = await db.MaintenanceSettings
            .Include(m => m.ServicePackage)
                .ThenInclude(p => p!.Items)
            .FirstOrDefaultAsync(m => m.Id == settingId);
        if (setting == null) return (false, "Không tìm thấy thiết lập bảo dưỡng.");

        var ro = await db.ROs
            .Include(r => r.Lines)
            .Include(r => r.Car)
            .FirstOrDefaultAsync(r => r.Id == roId);
        if (ro == null) return (false, "Không tìm thấy Lệnh sửa chữa RO.");

        if (ro.Status is ROStatus.Finished or ROStatus.Paid or ROStatus.Rejected or ROStatus.NotResponding)
            return (false, $"RO {ro.Code} ở trạng thái {ro.Status} — không thể cập nhật mốc bảo dưỡng.");

        ro.MaintenanceSettingId = setting.Id;
        ro.MaintenanceMilestone = $"{setting.ROMSID} ({setting.Name})";

        int itemsAdded = 0;
        if (addPackageCombo && setting.ServicePackage != null && setting.ServicePackage.Items.Count > 0)
        {
            foreach (var item in setting.ServicePackage.Items)
            {
                var line = new RepairLine
                {
                    ROId = roId,
                    Type = item.Type,
                    PartId = item.PartId,
                    Name = $"[{setting.ROMSID}] {item.Name}",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    ExpenseType = item.ExpenseType,
                    StdManHour = item.Type == LineType.Labor ? setting.TakingTimeHours : null
                };
                db.Lines.Add(line);
                itemsAdded++;
            }
        }

        await db.SaveChangesAsync();

        var comboNote = itemsAdded > 0 ? $" và tự động nạp {itemsAdded} hạng mục gói {setting.ServicePackage?.Name}" : "";
        return (true, $"Đã gán mốc bảo dưỡng {setting.ROMSID} vào RO {ro.Code}{comboNote} thành công.");
    }

    public async Task<List<RepairOrder>> ROsForMaintenanceSelectAsync()
    {
        var openStatuses = new[] { ROStatus.Created, ROStatus.Printed, ROStatus.Wait4Part, ROStatus.HasPart, ROStatus.HasRO, ROStatus.InGarage, ROStatus.Repaired, ROStatus.CheckEnd };
        return await db.ROs
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Where(r => openStatuses.Contains(r.Status))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    // ===== Danh mục Loại bảo hành RO (Ser_MST_ROWarrantyType) =====
    public async Task<List<WarrantyType>> WarrantyTypesAsync(WarrantyTypeCode? typeCode, bool? flagActive, string? q)
    {
        var query = db.WarrantyTypes
            .Include(t => t.Photos)
            .AsQueryable();

        if (typeCode.HasValue) query = query.Where(t => t.TypeCode == typeCode.Value);
        if (flagActive.HasValue) query = query.Where(t => t.FlagActive == flagActive.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(t =>
                t.TypeName.ToLower().Contains(term) ||
                t.DetailName.ToLower().Contains(term) ||
                (t.PhotoTypeDisplay != null && t.PhotoTypeDisplay.ToLower().Contains(term)));
        }

        return await query
            .OrderBy(t => t.TypeCode).ThenBy(t => t.DetailCode)
            .ToListAsync();
    }

    public async Task<WarrantyType?> GetWarrantyTypeAsync(int id)
    {
        return await db.WarrantyTypes
            .Include(t => t.Photos)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<WarrantyTypeSummaryDto> GetWarrantyTypeSummaryAsync()
    {
        var list = await db.WarrantyTypes.Include(t => t.Photos).ToListAsync();
        var total = list.Count;
        var active = list.Count(t => t.FlagActive);
        return new WarrantyTypeSummaryDto
        {
            TotalTypes = total,
            ActiveTypes = active,
            InactiveTypes = total - active,
            TotalPhotoTypes = list.Sum(t => t.Photos.Count),
            TypesWithPhotos = list.Count(t => t.Photos.Count > 0),
            DistinctMainCodes = list.Select(t => t.TypeCode).Distinct().Count(),
            DistinctDetailCodes = list.Select(t => t.DetailCode).Distinct().Count()
        };
    }

    public async Task<int> CreateWarrantyTypeAsync(WarrantyType type, List<WarrantyTypePhoto> photos)
    {
        if (string.IsNullOrWhiteSpace(type.TypeName))
            throw new ArgumentException("Tên loại bảo hành chính không được để trống.");
        if (string.IsNullOrWhiteSpace(type.DetailName))
            throw new ArgumentException("Tên loại bảo hành chi tiết không được để trống.");

        var exists = await db.WarrantyTypes.AnyAsync(t => t.TypeCode == type.TypeCode && t.DetailCode == type.DetailCode);
        if (exists)
            throw new InvalidOperationException($"Cặp loại bảo hành {type.TypeCode}/{type.DetailCode} đã tồn tại.");

        type.TypeName = type.TypeName.Trim();
        type.DetailName = type.DetailName.Trim();
        type.ROWTID = string.IsNullOrWhiteSpace(type.ROWTID) ? $"ROWT-{type.TypeCode}{type.DetailCode}" : type.ROWTID.Trim();
        type.Photos = NormalizePhotos(photos);
        type.PhotoTypeDisplay = BuildPhotoDisplay(type.Photos);
        type.LogLuDateTime = DateTime.Now;
        if (type.CreatedAt == default) type.CreatedAt = DateTime.Now;

        db.WarrantyTypes.Add(type);
        await db.SaveChangesAsync();
        return type.Id;
    }

    public async Task<(bool ok, string msg)> UpdateWarrantyTypeAsync(int id, WarrantyType input, List<WarrantyTypePhoto>? photos)
    {
        var type = await db.WarrantyTypes.Include(t => t.Photos).FirstOrDefaultAsync(t => t.Id == id);
        if (type == null) return (false, "Không tìm thấy loại bảo hành.");

        if (string.IsNullOrWhiteSpace(input.TypeName)) return (false, "Tên loại bảo hành chính không được để trống.");
        if (string.IsNullOrWhiteSpace(input.DetailName)) return (false, "Tên loại bảo hành chi tiết không được để trống.");

        var dup = await db.WarrantyTypes.AnyAsync(t => t.Id != id && t.TypeCode == input.TypeCode && t.DetailCode == input.DetailCode);
        if (dup) return (false, $"Cặp loại bảo hành {input.TypeCode}/{input.DetailCode} đã được dùng cho bản ghi khác.");

        type.TypeCode = input.TypeCode;
        type.TypeName = input.TypeName.Trim();
        type.DetailCode = input.DetailCode;
        type.DetailName = input.DetailName.Trim();
        type.FlagActive = input.FlagActive;
        type.UpdatedAt = DateTime.Now;
        type.UpdatedBy = input.UpdatedBy ?? "web";
        type.LogLuDateTime = DateTime.Now;
        type.LogLUBy = input.UpdatedBy ?? "web";

        // Gửi danh sách ảnh = thay trọn; không gửi = giữ nguyên (đúng hành vi nguồn).
        if (photos != null)
        {
            db.WarrantyTypePhotos.RemoveRange(type.Photos);
            type.Photos = NormalizePhotos(photos);
        }
        type.PhotoTypeDisplay = BuildPhotoDisplay(type.Photos);

        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật loại bảo hành {type.TypeCode}/{type.DetailCode}.");
    }

    public async Task<(bool ok, string msg)> ToggleWarrantyTypeActiveAsync(int id)
    {
        var type = await db.WarrantyTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (type == null) return (false, "Không tìm thấy loại bảo hành.");

        type.FlagActive = !type.FlagActive;
        type.UpdatedAt = DateTime.Now;
        type.LogLuDateTime = DateTime.Now;
        await db.SaveChangesAsync();
        return (true, $"Đã {(type.FlagActive ? "kích hoạt" : "ngừng áp dụng")} loại bảo hành {type.TypeCode}/{type.DetailCode}.");
    }

    public async Task<(bool ok, string msg)> DeleteWarrantyTypeAsync(int id)
    {
        var type = await db.WarrantyTypes.Include(t => t.Photos).FirstOrDefaultAsync(t => t.Id == id);
        if (type == null) return (false, "Không tìm thấy loại bảo hành.");

        db.WarrantyTypePhotos.RemoveRange(type.Photos);
        db.WarrantyTypes.Remove(type);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa loại bảo hành {type.TypeCode}/{type.DetailCode}.");
    }

    public async Task<List<WarrantyPhotoType>> WarrantyPhotoTypesAsync(bool? flagActive)
    {
        var query = db.WarrantyPhotoTypes.AsQueryable();
        if (flagActive.HasValue) query = query.Where(p => p.FlagActive == flagActive.Value);
        return await query.OrderBy(p => p.ROWPTCode).ToListAsync();
    }

    /// <summary>Chuẩn hoá danh sách loại ảnh: bỏ dòng trống, chống trùng mã ảnh, gán tên từ master nếu thiếu.</summary>
    private static List<WarrantyTypePhoto> NormalizePhotos(List<WarrantyTypePhoto> photos)
    {
        var result = new List<WarrantyTypePhoto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in photos)
        {
            var code = (p.ROWPTCode ?? "").Trim();
            if (string.IsNullOrWhiteSpace(code)) continue;
            if (!seen.Add(code)) continue;
            result.Add(new WarrantyTypePhoto { ROWPTCode = code, ROWPTName = p.ROWPTName?.Trim() });
        }
        return result;
    }

    /// <summary>Dựng chuỗi hiển thị loại ảnh theo đúng 2 luật nguồn: bỏ 4 mã KHAC/PXK/MPTC/MPTM và bỏ cặp TC+R.</summary>
    private static string? BuildPhotoDisplay(List<WarrantyTypePhoto> photos)
    {
        var excluded = new[] { "KHAC", "PXK", "MPTC", "MPTM" };
        var names = photos
            .Where(p => !excluded.Contains(p.ROWPTCode, StringComparer.OrdinalIgnoreCase))
            .Select(p => p.ROWPTName ?? p.ROWPTCode)
            .ToList();
        return names.Count > 0 ? string.Join(", ", names) : null;
    }

    // =========================================================================
    // CHỈ TIÊU KINH DOANH ĐẠI LÝ (Mst_DealerTarget / Mst_DealerTargetDetail) — MH 168
    // =========================================================================
    public async Task<List<DealerTarget>> DealerTargetsAsync(int? year, string? q)
    {
        var query = db.DealerTargets.Include(t => t.Details).AsQueryable();
        if (year.HasValue) query = query.Where(t => t.TargetYear == year.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim();
            query = query.Where(t => t.Details.Any(d => d.DealerCode.Contains(kw) || (d.DealerName != null && d.DealerName.Contains(kw))));
        }
        return await query.OrderByDescending(t => t.TargetYear).ToListAsync();
    }

    public async Task<DealerTarget?> GetDealerTargetAsync(int id)
    {
        return await db.DealerTargets
            .Include(t => t.Details)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<DealerTargetSummaryDto> GetDealerTargetSummaryAsync()
    {
        var periods = await db.DealerTargets.Include(t => t.Details).ToListAsync();
        var allDetails = periods.SelectMany(p => p.Details).ToList();
        var currentYear = DateTime.Today.Year;
        var currentDetails = allDetails.Where(d => d.TargetYear == currentYear).ToList();
        return new DealerTargetSummaryDto
        {
            TotalPeriods = periods.Count,
            TotalDetails = allDetails.Count,
            TotalDealers = allDetails.Select(d => d.DealerCode).Distinct().Count(),
            TotalTypes = allDetails.Select(d => d.TargetType).Distinct().Count(),
            TotalTargetValue = allDetails.Sum(d => d.TargetValue),
            CurrentYear = currentYear,
            CurrentYearDetails = currentDetails.Count,
            CurrentYearTargetValue = currentDetails.Sum(d => d.TargetValue)
        };
    }

    /// <summary>Lưu kỳ chỉ tiêu (thêm mới hoặc cập nhật) kèm danh sách chi tiết — theo Mst_DealerTarget_Save.</summary>
    public async Task<(bool ok, string msg, int id)> SaveDealerTargetAsync(int? id, int year, string? remark, List<DealerTargetDetail> details, string user)
    {
        // Validate kỳ chỉ tiêu (TargetYear 1900..2100)
        if (year < 1900 || year > 2100)
            return (false, "Năm chỉ tiêu không hợp lệ (phải trong khoảng 1900..2100).", 0);

        if (details == null || details.Count == 0)
            return (false, "Cần ít nhất một dòng chi tiết chỉ tiêu (DealerTargetDetail).", 0);

        // Validate từng dòng chi tiết theo đúng luật nguồn
        for (var i = 0; i < details.Count; i++)
        {
            var d = details[i];
            var row = i + 1;
            if (string.IsNullOrWhiteSpace(d.DealerCode))
                return (false, $"Dòng {row}: Mã đại lý (DealerCode) không được để trống.", 0);
            if (d.TargetMonth == default)
                return (false, $"Dòng {row}: Tháng chỉ tiêu (TargetMonth) không được để trống.", 0);
            if (d.TargetMonth.Year != year)
                return (false, $"Dòng {row}: Tháng chỉ tiêu {d.TargetMonth:MM/yyyy} không khớp với năm chỉ tiêu {year}.", 0);
            if (d.TargetValue < 0)
                return (false, $"Dòng {row}: Giá trị chỉ tiêu (TargetValue) phải >= 0.", 0);
        }

        DealerTarget target;
        if (id.HasValue && id.Value > 0)
        {
            var existing = await db.DealerTargets.Include(t => t.Details).FirstOrDefaultAsync(t => t.Id == id.Value);
            if (existing == null) return (false, "Không tìm thấy kỳ chỉ tiêu.", 0);

            var dupYear = await db.DealerTargets.AnyAsync(t => t.Id != id.Value && t.TargetYear == year);
            if (dupYear) return (false, $"Kỳ chỉ tiêu năm {year} đã tồn tại.", 0);

            target = existing;
            target.TargetYear = year;
            target.Remark = remark?.Trim();
            target.UpdatedBy = user;
            target.UpdatedAt = DateTime.Now;
            db.DealerTargetDetails.RemoveRange(target.Details);
            target.Details = [];
        }
        else
        {
            var dupYear = await db.DealerTargets.AnyAsync(t => t.TargetYear == year);
            if (dupYear) return (false, $"Kỳ chỉ tiêu năm {year} đã tồn tại.", 0);

            target = new DealerTarget
            {
                TargetYear = year,
                Remark = remark?.Trim(),
                CreatedBy = user,
                CreatedAt = DateTime.Now
            };
            db.DealerTargets.Add(target);
        }

        foreach (var d in details)
        {
            target.Details.Add(new DealerTargetDetail
            {
                TargetYear = year,
                DealerCode = d.DealerCode.Trim(),
                DealerName = d.DealerName?.Trim(),
                TargetMonth = new DateTime(d.TargetMonth.Year, d.TargetMonth.Month, 1),
                TargetType = d.TargetType,
                TargetValue = d.TargetValue,
                CreatedBy = user,
                CreatedAt = DateTime.Now
            });
        }

        await db.SaveChangesAsync();
        return (true, $"Đã lưu kỳ chỉ tiêu kinh doanh năm {year} ({target.Details.Count} dòng).", target.Id);
    }

    public async Task<(bool ok, string msg)> DeleteDealerTargetAsync(int id)
    {
        var target = await db.DealerTargets.Include(t => t.Details).FirstOrDefaultAsync(t => t.Id == id);
        if (target == null) return (false, "Không tìm thấy kỳ chỉ tiêu.");
        db.DealerTargetDetails.RemoveRange(target.Details);
        db.DealerTargets.Remove(target);
        await db.SaveChangesAsync();
        return (true, $"Đã xoá kỳ chỉ tiêu năm {target.TargetYear}.");
    }

    public async Task<(bool ok, string msg)> DeleteDealerTargetDetailAsync(int detailId)
    {
        var detail = await db.DealerTargetDetails.FirstOrDefaultAsync(d => d.Id == detailId);
        if (detail == null) return (false, "Không tìm thấy dòng chi tiết chỉ tiêu.");
        db.DealerTargetDetails.Remove(detail);
        await db.SaveChangesAsync();
        return (true, "Đã xoá dòng chi tiết chỉ tiêu.");
    }

    // --- Customer Service Factor — Hệ số giá dịch vụ theo loại khách hàng (Ser_MST_CustomerType / Ser_Mst_CusServiceFactor) ---
    public async Task<List<CustomerType>> CustomerTypesAsync(bool? isActive, string? q)
    {
        var query = db.CustomerTypes.AsQueryable();
        if (isActive.HasValue) query = query.Where(t => t.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = query.Where(t => t.CusTypeCode.ToLower().Contains(s) || t.CusTypeName.ToLower().Contains(s));
        }
        return await query.OrderBy(t => t.CusTypeCode).ToListAsync();
    }

    public Task<CustomerType?> GetCustomerTypeAsync(int id) =>
        db.CustomerTypes.Include(t => t.ServiceFactors).FirstOrDefaultAsync(t => t.Id == id);

    public async Task<int> CreateCustomerTypeAsync(CustomerType type)
    {
        if (string.IsNullOrWhiteSpace(type.CusTypeCode))
            throw new InvalidOperationException("Vui lòng nhập mã loại khách hàng (CusTypeCode).");
        if (string.IsNullOrWhiteSpace(type.CusTypeName))
            throw new InvalidOperationException("Vui lòng nhập tên loại khách hàng (CusTypeName).");

        type.CusTypeCode = type.CusTypeCode.Trim().ToUpperInvariant();
        var exists = await db.CustomerTypes.AnyAsync(t => t.CusTypeCode == type.CusTypeCode);
        if (exists) throw new InvalidOperationException($"Mã loại khách hàng {type.CusTypeCode} đã tồn tại.");
        if (type.CusFactor <= 0) type.CusFactor = 1.0m;

        db.CustomerTypes.Add(type);
        await db.SaveChangesAsync();
        return type.Id;
    }

    public async Task<(bool ok, string msg)> UpdateCustomerTypeAsync(int id, CustomerType input)
    {
        var existing = await db.CustomerTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (existing == null) return (false, "Không tìm thấy loại khách hàng.");
        if (string.IsNullOrWhiteSpace(input.CusTypeName)) return (false, "Tên loại khách hàng không được để trống.");

        existing.CusTypeName = input.CusTypeName.Trim();
        existing.CusFactor = input.CusFactor > 0 ? input.CusFactor : 1.0m;
        existing.CusPersonType = string.IsNullOrWhiteSpace(input.CusPersonType) ? "Personal" : input.CusPersonType.Trim();
        existing.IsActive = input.IsActive;
        await db.SaveChangesAsync();
        return (true, $"Đã cập nhật loại khách hàng {existing.CusTypeCode} - {existing.CusTypeName}.");
    }

    public async Task<(bool ok, string msg)> ToggleCustomerTypeActiveAsync(int id)
    {
        var existing = await db.CustomerTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (existing == null) return (false, "Không tìm thấy loại khách hàng.");
        existing.IsActive = !existing.IsActive;
        await db.SaveChangesAsync();
        return (true, $"Loại khách hàng {existing.CusTypeCode} đã {(existing.IsActive ? "kích hoạt" : "tạm dừng")}.");
    }

    public async Task<(bool ok, string msg)> DeleteCustomerTypeAsync(int id)
    {
        var existing = await db.CustomerTypes.Include(t => t.ServiceFactors).FirstOrDefaultAsync(t => t.Id == id);
        if (existing == null) return (false, "Không tìm thấy loại khách hàng.");
        if (existing.ServiceFactors.Count > 0)
        {
            existing.IsActive = false;
            await db.SaveChangesAsync();
            return (true, $"Loại khách hàng {existing.CusTypeCode} đang có {existing.ServiceFactors.Count} cấu hình hệ số giá nên đã chuyển sang trạng thái Tạm dừng.");
        }
        db.CustomerTypes.Remove(existing);
        await db.SaveChangesAsync();
        return (true, $"Đã xóa loại khách hàng {existing.CusTypeCode}.");
    }

    /// <summary>Dựng ma trận hệ số giá dịch vụ × loại khách hàng với giá hiệu lực (3 tầng dự phòng: Factor → CusFactor → 1).</summary>
    public async Task<List<CusServiceFactorRowDto>> CusServiceFactorMatrixAsync(int? serviceItemId, int? customerTypeId, string? q)
    {
        var servicesQuery = db.ServiceItems.Where(s => s.IsActive).AsQueryable();
        if (serviceItemId.HasValue && serviceItemId.Value > 0)
            servicesQuery = servicesQuery.Where(s => s.Id == serviceItemId.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            servicesQuery = servicesQuery.Where(x => x.Code.ToLower().Contains(s) || x.Name.ToLower().Contains(s));
        }
        var services = await servicesQuery.OrderBy(s => s.Code).ToListAsync();

        var typesQuery = db.CustomerTypes.Where(t => t.IsActive).AsQueryable();
        if (customerTypeId.HasValue && customerTypeId.Value > 0)
            typesQuery = typesQuery.Where(t => t.Id == customerTypeId.Value);
        var types = await typesQuery.OrderBy(t => t.CusTypeCode).ToListAsync();

        var factors = await db.CusServiceFactors.ToListAsync();
        var lookup = factors.ToDictionary(f => (f.ServiceItemId, f.CustomerTypeId), f => f);

        var rows = new List<CusServiceFactorRowDto>();
        foreach (var svc in services)
        {
            foreach (var ct in types)
            {
                var hasCustom = lookup.TryGetValue((svc.Id, ct.Id), out var f);
                var factor = hasCustom ? f!.Factor : (ct.CusFactor > 0 ? ct.CusFactor : 1.0m);
                rows.Add(new CusServiceFactorRowDto
                {
                    ServiceItemId = svc.Id,
                    ServiceCode = svc.Code,
                    ServiceName = svc.Name,
                    BasePrice = svc.Price,
                    CustomerTypeId = ct.Id,
                    CusTypeCode = ct.CusTypeCode,
                    CusTypeName = ct.CusTypeName,
                    Factor = factor,
                    EffectivePrice = Math.Round(svc.Price * factor, 0),
                    IsCustomized = hasCustom
                });
            }
        }
        return rows;
    }

    public async Task<CusServiceFactorSummaryDto> GetCusServiceFactorSummaryAsync()
    {
        var serviceCount = await db.ServiceItems.CountAsync(s => s.IsActive);
        var typeCount = await db.CustomerTypes.CountAsync(t => t.IsActive);
        var factors = await db.CusServiceFactors.ToListAsync();
        var customized = factors.Count;
        var totalCells = serviceCount * typeCount;
        return new CusServiceFactorSummaryDto
        {
            TotalServices = serviceCount,
            TotalCustomerTypes = typeCount,
            TotalCells = totalCells,
            CustomizedCells = customized,
            AvgFactor = factors.Count > 0 ? Math.Round(factors.Average(f => f.Factor), 4) : 0m,
            MinFactor = factors.Count > 0 ? factors.Min(f => f.Factor) : 0m,
            MaxFactor = factors.Count > 0 ? factors.Max(f => f.Factor) : 0m
        };
    }

    /// <summary>Lưu (upsert) hệ số giá cho một ô dịch vụ × loại khách — theo Ser_Mst_CusServiceFactor_Update.</summary>
    public async Task<(bool ok, string msg)> SaveCusServiceFactorAsync(int serviceItemId, int customerTypeId, decimal factor, string? dealerCode, string? user)
    {
        if (factor <= 0) return (false, "Hệ số giá phải lớn hơn 0.");
        var svc = await db.ServiceItems.FirstOrDefaultAsync(s => s.Id == serviceItemId);
        if (svc == null) return (false, "Không tìm thấy dịch vụ.");
        var ct = await db.CustomerTypes.FirstOrDefaultAsync(t => t.Id == customerTypeId);
        if (ct == null) return (false, "Không tìm thấy loại khách hàng.");

        var existing = await db.CusServiceFactors.FirstOrDefaultAsync(f => f.ServiceItemId == serviceItemId && f.CustomerTypeId == customerTypeId);
        if (existing == null)
        {
            db.CusServiceFactors.Add(new CusServiceFactor
            {
                ServiceItemId = serviceItemId,
                CustomerTypeId = customerTypeId,
                Factor = factor,
                DealerCode = dealerCode?.Trim(),
                LogLUBy = user,
                LogLUDateTime = DateTime.Now
            });
        }
        else
        {
            existing.Factor = factor;
            existing.DealerCode = dealerCode?.Trim();
            existing.LogLUBy = user;
            existing.LogLUDateTime = DateTime.Now;
        }
        await db.SaveChangesAsync();
        return (true, $"Đã lưu hệ số giá {factor} cho dịch vụ [{svc.Code}] × loại khách [{ct.CusTypeCode}].");
    }

    /// <summary>Xoá cấu hình hệ số riêng của một ô — quay về dùng hệ số mặc định của loại khách.</summary>
    public async Task<(bool ok, string msg)> ResetCusServiceFactorAsync(int serviceItemId, int customerTypeId)
    {
        var existing = await db.CusServiceFactors.FirstOrDefaultAsync(f => f.ServiceItemId == serviceItemId && f.CustomerTypeId == customerTypeId);
        if (existing == null) return (false, "Ô này chưa có cấu hình hệ số riêng.");
        db.CusServiceFactors.Remove(existing);
        await db.SaveChangesAsync();
        return (true, "Đã xoá cấu hình hệ số riêng — ô này dùng hệ số mặc định của loại khách.");
    }

    /// <summary>Tra giá hiệu lực của một dịch vụ theo loại khách (Factor → CusFactor → 1).</summary>
    public async Task<decimal> ResolveServicePriceAsync(int serviceItemId, int? customerTypeId)
    {
        var svc = await db.ServiceItems.FirstOrDefaultAsync(s => s.Id == serviceItemId);
        if (svc == null) return 0m;
        decimal factor = 1.0m;
        if (customerTypeId.HasValue && customerTypeId.Value > 0)
        {
            var custom = await db.CusServiceFactors.FirstOrDefaultAsync(f => f.ServiceItemId == serviceItemId && f.CustomerTypeId == customerTypeId.Value);
            if (custom != null) factor = custom.Factor;
            else
            {
                var ct = await db.CustomerTypes.FirstOrDefaultAsync(t => t.Id == customerTypeId.Value);
                if (ct != null && ct.CusFactor > 0) factor = ct.CusFactor;
            }
        }
        return Math.Round(svc.Price * factor, 0);
    }

    // --- RO History — Nhật ký thao tác Lệnh sửa chữa (Ser_ROHistory) ---
    public async Task<List<RoHistory>> RoHistoriesAsync(int? roId, ROStatus? status, string? q)
    {
        var query = db.RoHistories.Include(h => h.RO).ThenInclude(r => r!.Car).AsQueryable();
        if (roId.HasValue && roId.Value > 0) query = query.Where(h => h.ROId == roId.Value);
        if (status.HasValue) query = query.Where(h => h.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(h =>
                (h.Note != null && h.Note.Contains(term)) ||
                (h.UserCode != null && h.UserCode.Contains(term)) ||
                (h.RO != null && h.RO.Code.Contains(term)));
        }
        return await query.OrderByDescending(h => h.HistoryDate).ThenByDescending(h => h.Id).ToListAsync();
    }

    public Task<RoHistory?> GetRoHistoryAsync(int id) =>
        db.RoHistories.Include(h => h.RO).ThenInclude(r => r!.Car).FirstOrDefaultAsync(h => h.Id == id);

    public async Task<RoHistorySummaryDto> GetRoHistorySummaryAsync(int? roId)
    {
        var query = db.RoHistories.AsQueryable();
        if (roId.HasValue && roId.Value > 0) query = query.Where(h => h.ROId == roId.Value);
        var list = await query.ToListAsync();
        return new RoHistorySummaryDto
        {
            TotalEntries = list.Count,
            RejectCount = list.Count(h => h.Status == ROStatus.Rejected),
            DistinctStatusCount = list.Select(h => h.Status).Distinct().Count(),
            FirstEntryAt = list.Count > 0 ? list.Min(h => h.HistoryDate) : null,
            LastEntryAt = list.Count > 0 ? list.Max(h => h.HistoryDate) : null
        };
    }

    /// <summary>Ghi 1 dòng nhật ký thao tác RO — tương đương InsertToROHistory trong idn.CarService.</summary>
    public async Task<int> AddRoHistoryAsync(int roId, ROStatus status, string? note, string? userCode)
    {
        var entry = new RoHistory
        {
            ROId = roId,
            Status = status,
            HistoryDate = DateTime.Now,
            Note = note,
            UserCode = userCode ?? "system"
        };
        db.RoHistories.Add(entry);
        await db.SaveChangesAsync();
        return entry.Id;
    }

    public async Task<(bool ok, string msg)> DeleteRoHistoryAsync(int id)
    {
        var entry = await db.RoHistories.FirstOrDefaultAsync(h => h.Id == id);
        if (entry == null) return (false, "Không tìm thấy dòng nhật ký.");
        db.RoHistories.Remove(entry);
        await db.SaveChangesAsync();
        return (true, "Đã xóa dòng nhật ký thao tác.");
    }
}

