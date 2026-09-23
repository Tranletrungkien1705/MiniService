using Microsoft.EntityFrameworkCore;
using MiniService.Models;

namespace MiniService.Data;

public class AppDbContext : DbContext
{
    private readonly Guid _orgId;
    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenant) : base(options) => _orgId = tenant.OrgId;

    public DbSet<Org> Orgs => Set<Org>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<RepairOrder> ROs => Set<RepairOrder>();
    public DbSet<RepairLine> Lines => Set<RepairLine>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<WarrantyReport> WarrantyReports => Set<WarrantyReport>();
    public DbSet<WarrantyReportItem> WarrantyReportItems => Set<WarrantyReportItem>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<StockIn> StockIns => Set<StockIn>();
    public DbSet<StockInDetail> StockInDetails => Set<StockInDetail>();
    public DbSet<StockOut> StockOuts => Set<StockOut>();
    public DbSet<StockOutDetail> StockOutDetails => Set<StockOutDetail>();
    public DbSet<CustomerCare> CustomerCares => Set<CustomerCare>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteItem> QuoteItems => Set<QuoteItem>();
    public DbSet<ServicePackage> ServicePackages => Set<ServicePackage>();
    public DbSet<ServicePackageItem> ServicePackageItems => Set<ServicePackageItem>();
    public DbSet<OrderPart> OrderParts => Set<OrderPart>();
    public DbSet<OrderPartLine> OrderPartLines => Set<OrderPartLine>();
    public DbSet<Cavity> Cavities => Set<Cavity>();
    public DbSet<ReceptionSheet> ReceptionSheets => Set<ReceptionSheet>();
    public DbSet<ReceptionItem> ReceptionItems => Set<ReceptionItem>();
    public DbSet<GroupRepair> GroupRepairs => Set<GroupRepair>();
    public DbSet<Engineer> Engineers => Set<Engineer>();
    public DbSet<AssignmentWork> AssignmentWorks => Set<AssignmentWork>();
    public DbSet<AssignmentEngineer> AssignmentEngineers => Set<AssignmentEngineer>();
    public DbSet<InsuranceCompany> InsuranceCompanies => Set<InsuranceCompany>();
    public DbSet<InsuranceContract> InsuranceContracts => Set<InsuranceContract>();
    public DbSet<InsuranceClaim> InsuranceClaims => Set<InsuranceClaim>();
    public DbSet<InsuranceClaimItem> InsuranceClaimItems => Set<InsuranceClaimItem>();
    public DbSet<CampaignMarketing> CampaignMarketings => Set<CampaignMarketing>();
    public DbSet<CampaignMarketingItem> CampaignMarketingItems => Set<CampaignMarketingItem>();
    public DbSet<CustomerCareMace> CustomerCareMaces => Set<CustomerCareMace>();
    public DbSet<StockAdj> StockAdjs => Set<StockAdj>();
    public DbSet<StockAdjDetail> StockAdjDetails => Set<StockAdjDetail>();
    public DbSet<Bulletin> Bulletins => Set<Bulletin>();
    public DbSet<BulletinDetail> BulletinDetails => Set<BulletinDetail>();
    public DbSet<BulletinVin> BulletinVins => Set<BulletinVin>();
    public DbSet<PdiRequest> PdiRequests => Set<PdiRequest>();
    public DbSet<PdiRequestItem> PdiRequestItems => Set<PdiRequestItem>();
    public DbSet<PdiChecklistItem> PdiChecklistItems => Set<PdiChecklistItem>();
    public DbSet<OrderComplain> OrderComplains => Set<OrderComplain>();
    public DbSet<OrderComplainAttachFile> OrderComplainAttachFiles => Set<OrderComplainAttachFile>();
    public DbSet<TechnicalLibrary> TechnicalLibraries => Set<TechnicalLibrary>();
    public DbSet<ServiceItem> ServiceItems => Set<ServiceItem>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierPayment> SupplierPayments => Set<SupplierPayment>();
    public DbSet<SupplierPaymentDetail> SupplierPaymentDetails => Set<SupplierPaymentDetail>();
    public DbSet<StockOutOrder> StockOutOrders => Set<StockOutOrder>();
    public DbSet<StockOutOrderDetail> StockOutOrderDetails => Set<StockOutOrderDetail>();
    public DbSet<PartOO> PartOOs => Set<PartOO>();
    public DbSet<CusDebit> CusDebits => Set<CusDebit>();
    public DbSet<CusDebitPayment> CusDebitPayments => Set<CusDebitPayment>();
    public DbSet<SupplierDebit> SupplierDebits => Set<SupplierDebit>();
    public DbSet<SupplierDebitPayment> SupplierDebitPayments => Set<SupplierDebitPayment>();
    public DbSet<DealerHistoryRecord> DealerHistoryRecords => Set<DealerHistoryRecord>();
    public DbSet<DealerHistoryItem> DealerHistoryItems => Set<DealerHistoryItem>();
    public DbSet<InsuranceDebit> InsuranceDebits => Set<InsuranceDebit>();
    public DbSet<InsuranceDebitPayment> InsuranceDebitPayments => Set<InsuranceDebitPayment>();
    public DbSet<CustomerGroup> CustomerGroups => Set<CustomerGroup>();
    public DbSet<CustomerGroupMember> CustomerGroupMembers => Set<CustomerGroupMember>();
    public DbSet<PartPriceRequest> PartPriceRequests => Set<PartPriceRequest>();
    public DbSet<PartPriceRequestLine> PartPriceRequestLines => Set<PartPriceRequestLine>();
    public DbSet<ComplaintDiagnosticError> ComplaintDiagnosticErrors => Set<ComplaintDiagnosticError>();
    public DbSet<CustomerCare72h> CustomerCare72hs => Set<CustomerCare72h>();
    public DbSet<CustomerCareBirthday> CustomerCareBirthdays => Set<CustomerCareBirthday>();
    public DbSet<WarrantyWork> WarrantyWorks => Set<WarrantyWork>();
    public DbSet<MaintenanceSetting> MaintenanceSettings => Set<MaintenanceSetting>();
    public DbSet<WarrantyType> WarrantyTypes => Set<WarrantyType>();
    public DbSet<WarrantyTypePhoto> WarrantyTypePhotos => Set<WarrantyTypePhoto>();
    public DbSet<WarrantyPhotoType> WarrantyPhotoTypes => Set<WarrantyPhotoType>();
    public DbSet<DealerTarget> DealerTargets => Set<DealerTarget>();
    public DbSet<DealerTargetDetail> DealerTargetDetails => Set<DealerTargetDetail>();
    public DbSet<RoDeliveryDateHistory> RoDeliveryDateHistories => Set<RoDeliveryDateHistory>();
    public DbSet<CustomerType> CustomerTypes => Set<CustomerType>();
    public DbSet<CusServiceFactor> CusServiceFactors => Set<CusServiceFactor>();
    public DbSet<RoHistory> RoHistories => Set<RoHistory>();
    public DbSet<CarModel> CarModels => Set<CarModel>();
    public DbSet<Bom> Boms => Set<Bom>();
    public DbSet<BomLine> BomLines => Set<BomLine>();
    public DbSet<DealerBankAccount> DealerBankAccounts => Set<DealerBankAccount>();
    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();
    public DbSet<WorkingCalendar> WorkingCalendars => Set<WorkingCalendar>();
    public DbSet<RoAttachment> RoAttachments => Set<RoAttachment>();
    public DbSet<SharePart> ShareParts => Set<SharePart>();
    public DbSet<SharePartLine> SharePartLines => Set<SharePartLine>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        if (Database.IsNpgsql()) b.HasDefaultSchema("miniservice");
        b.Entity<Org>().HasIndex(x => x.ApiKey).IsUnique();
        b.Entity<Customer>(e => {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.HasOne(x => x.CustomerGroup).WithMany(g => g.Customers).HasForeignKey(x => x.CustomerGroupId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Car>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Plate }).IsUnique();
            e.HasOne(x => x.Customer).WithMany(x => x.Cars).HasForeignKey(x => x.CustomerId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Part>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.Property(x => x.CostPrice).HasPrecision(18, 2);
            e.Property(x => x.SalePrice).HasPrecision(18, 2);
            e.Property(x => x.InStock).HasPrecision(18, 2);
            e.Property(x => x.MinStock).HasPrecision(18, 2);
            e.Ignore(x => x.IsLowStock);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<RepairOrder>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.Ignore(x => x.Total); e.Ignore(x => x.GrossTotal); e.Ignore(x => x.LaborTotal); e.Ignore(x => x.PartTotal);
            e.Ignore(x => x.CustomerTotal); e.Ignore(x => x.WarrantyTotal); e.Ignore(x => x.InsuranceTotal);
            e.Ignore(x => x.PaidAmount); e.Ignore(x => x.RemainingBalance);
            e.Property(x => x.CampaignDiscountAmount).HasPrecision(18, 2);
            e.Property(x => x.CustomerGroupDiscountAmount).HasPrecision(18, 2);
            e.Property(x => x.BirthdayDiscountAmount).HasPrecision(18, 2);
            e.HasOne(x => x.CustomerGroup).WithMany(g => g.RepairOrders).HasForeignKey(x => x.CustomerGroupId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Car).WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Appointment).WithMany().HasForeignKey(x => x.AppointmentId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.CampaignMarketing).WithMany(x => x.AppliedROs).HasForeignKey(x => x.CampaignMarketingId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Bulletin).WithMany(x => x.AppliedROs).HasForeignKey(x => x.BulletinId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.MaintenanceSetting).WithMany(x => x.RepairOrders).HasForeignKey(x => x.MaintenanceSettingId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.DeliveryDateHistories).WithOne(x => x.RO).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Histories).WithOne(x => x.RO).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.ReminderMaintanceKm).HasPrecision(18, 0);
            e.HasMany(x => x.WarrantyReports).WithOne(x => x.RO).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.StockOuts).WithOne(x => x.RO).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.CustomerCares).WithOne(x => x.RO).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Payments).WithOne(x => x.RO).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.OrderParts).WithOne(x => x.RO).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Cavity).WithMany().HasForeignKey(x => x.CavityId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.ReceptionSheet).WithMany().HasForeignKey(x => x.ReceptionSheetId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.AssignmentWorks).WithOne(x => x.RO).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.InsuranceClaims).WithOne(x => x.RO).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.PdiRequest).WithMany(x => x.RepairOrders).HasForeignKey(x => x.PdiRequestId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.CusDebits).WithOne(x => x.RO).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.PartPriceRequests).WithOne(x => x.RO).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<RepairLine>(e =>
        {
            e.Ignore(x => x.Amount);
            e.Property(x => x.Quantity).HasPrecision(18, 2);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.StdManHour).HasPrecision(5, 2);
            e.HasOne(x => x.RO).WithMany(x => x.Lines).HasForeignKey(x => x.ROId);
            e.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.ServiceItem).WithMany(x => x.RepairLines).HasForeignKey(x => x.ServiceItemId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<WarrantyReport>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.ReportNo }).IsUnique();
            e.Property(x => x.ClaimAmount).HasPrecision(18, 2);
            e.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
            e.Ignore(x => x.LaborClaimTotal); e.Ignore(x => x.PartClaimTotal);
            e.HasOne(x => x.Car).WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.PartError).WithMany().HasForeignKey(x => x.PartIDError).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Items).WithOne(x => x.WarrantyReport).HasForeignKey(x => x.WarrantyReportId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<WarrantyReportItem>(e =>
        {
            e.Ignore(x => x.Amount);
            e.Property(x => x.Quantity).HasPrecision(18, 2);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Appointment>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.AppNo }).IsUnique();
            e.HasOne(x => x.Car).WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.RO).WithMany().HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.ReceptionSheet).WithMany().HasForeignKey(x => x.ReceptionSheetId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<StockIn>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.StockInNo }).IsUnique();
            e.Ignore(x => x.SubTotal); e.Ignore(x => x.TotalVat); e.Ignore(x => x.Total); e.Ignore(x => x.ItemCount);
            e.HasOne(x => x.OrderPart).WithMany().HasForeignKey(x => x.OrderPartId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Items).WithOne(x => x.StockIn).HasForeignKey(x => x.StockInId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<StockInDetail>(e =>
        {
            e.Ignore(x => x.SubTotal); e.Ignore(x => x.VatAmount); e.Ignore(x => x.Amount);
            e.Property(x => x.Quantity).HasPrecision(18, 2);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.VatPercent).HasPrecision(5, 2);
            e.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<StockOut>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.StockOutNo }).IsUnique();
            e.Ignore(x => x.SubTotal); e.Ignore(x => x.TotalVat); e.Ignore(x => x.Total); e.Ignore(x => x.ItemCount);
            e.HasOne(x => x.RO).WithMany(x => x.StockOuts).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Car).WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Quote).WithMany().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Items).WithOne(x => x.StockOut).HasForeignKey(x => x.StockOutId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<StockOutDetail>(e =>
        {
            e.Ignore(x => x.SubTotal); e.Ignore(x => x.VatAmount); e.Ignore(x => x.Amount);
            e.Property(x => x.Quantity).HasPrecision(18, 2);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.VatPercent).HasPrecision(5, 2);
            e.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CustomerCare>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.CareNo }).IsUnique();
            e.HasOne(x => x.RO).WithMany(x => x.CustomerCares).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Car).WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CustomerCare72h>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Care72No }).IsUnique();
            e.HasOne(x => x.RO).WithMany(x => x.CustomerCare72hs).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Car).WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ReRepairRO).WithMany().HasForeignKey(x => x.ReRepairROId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.ReRepairAppointment).WithMany().HasForeignKey(x => x.ReRepairAppointmentId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Payment>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.PaymentNo }).IsUnique();
            e.Property(x => x.RoTotalAmount).HasPrecision(18, 2);
            e.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            e.Property(x => x.ThirdPartyAmount).HasPrecision(18, 2);
            e.Property(x => x.PayableAmount).HasPrecision(18, 2);
            e.Property(x => x.PaymentAmount).HasPrecision(18, 2);
            e.HasOne(x => x.RO).WithMany(x => x.Payments).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Car).WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Quote>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.QuoteNo }).IsUnique();
            e.Ignore(x => x.SubTotal); e.Ignore(x => x.TotalDiscount); e.Ignore(x => x.TotalVat); e.Ignore(x => x.Total); e.Ignore(x => x.ItemCount);
            e.HasOne(x => x.Customer).WithMany(x => x.Quotes).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Car).WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.RO).WithMany(x => x.Quotes).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.StockOut).WithMany().HasForeignKey(x => x.StockOutId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Items).WithOne(x => x.Quote).HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<QuoteItem>(e =>
        {
            e.Ignore(x => x.LineTotal); e.Ignore(x => x.DiscountAmount); e.Ignore(x => x.TaxableAmount); e.Ignore(x => x.VatAmount); e.Ignore(x => x.Amount);
            e.Property(x => x.Quantity).HasPrecision(18, 2);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.DiscountPercent).HasPrecision(5, 2);
            e.Property(x => x.VatPercent).HasPrecision(5, 2);
            e.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ServicePackage>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.PackageNo }).IsUnique();
            e.Property(x => x.TakingTimeHours).HasPrecision(5, 2);
            e.Ignore(x => x.LaborSubTotal); e.Ignore(x => x.PartSubTotal);
            e.Ignore(x => x.SubTotal); e.Ignore(x => x.TotalVat); e.Ignore(x => x.Total);
            e.Ignore(x => x.ItemCount); e.Ignore(x => x.LaborCount); e.Ignore(x => x.PartCount);
            e.HasMany(x => x.Items).WithOne(x => x.ServicePackage).HasForeignKey(x => x.ServicePackageId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ServicePackageItem>(e =>
        {
            e.Ignore(x => x.SubTotal); e.Ignore(x => x.VatAmount); e.Ignore(x => x.Amount);
            e.Property(x => x.Quantity).HasPrecision(18, 2);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.VatPercent).HasPrecision(5, 2);
            e.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<OrderPart>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.OrderPartNo }).IsUnique();
            e.Ignore(x => x.SubTotalBeforeDiscount); e.Ignore(x => x.TotalDiscount); e.Ignore(x => x.TaxableAmount);
            e.Ignore(x => x.TotalVat); e.Ignore(x => x.Total);
            e.Ignore(x => x.TotalQuantityOrdered); e.Ignore(x => x.TotalQuantityApproved); e.Ignore(x => x.TotalQuantityReceived);
            e.Ignore(x => x.ItemCount);
            e.HasOne(x => x.RO).WithMany(x => x.OrderParts).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.StockIn).WithMany().HasForeignKey(x => x.StockInId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Lines).WithOne(x => x.OrderPart).HasForeignKey(x => x.OrderPartId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<OrderPartLine>(e =>
        {
            e.Ignore(x => x.SubTotalBeforeDiscount); e.Ignore(x => x.DiscountAmount); e.Ignore(x => x.TaxableAmount); e.Ignore(x => x.VatAmount); e.Ignore(x => x.Amount);
            e.Property(x => x.Quantity).HasPrecision(18, 2);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.DiscountRate).HasPrecision(5, 2);
            e.Property(x => x.VatPercent).HasPrecision(5, 2);
            e.Property(x => x.ApprovedQuantity).HasPrecision(18, 2);
            e.Property(x => x.ReceivedQuantity).HasPrecision(18, 2);
            e.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Cavity>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.CavityNo }).IsUnique();
            e.Ignore(x => x.IsInUse);
            e.Ignore(x => x.ElapsedTime);
            e.HasOne(x => x.CurrentRO).WithMany().HasForeignKey(x => x.CurrentROId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ReceptionSheet>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.ReceptionNo }).IsUnique();
            e.Ignore(x => x.ItemCount);
            e.Ignore(x => x.IssuesCount);
            e.HasOne(x => x.Car).WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Appointment).WithMany().HasForeignKey(x => x.AppointmentId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.RO).WithMany(x => x.ReceptionSheets).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Items).WithOne(x => x.ReceptionSheet).HasForeignKey(x => x.ReceptionSheetId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ReceptionItem>(e =>
        {
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<GroupRepair>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.GroupRNo }).IsUnique();
            e.HasMany(x => x.Engineers).WithOne(x => x.GroupRepair).HasForeignKey(x => x.GroupRId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Engineer>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.EngineerNo }).IsUnique();
            e.HasOne(x => x.GroupRepair).WithMany(x => x.Engineers).HasForeignKey(x => x.GroupRId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<AssignmentWork>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.AssignmentNo }).IsUnique();
            e.Ignore(x => x.PrimaryTechnician);
            e.Ignore(x => x.HasSCC);
            e.Ignore(x => x.HasSCD);
            e.Ignore(x => x.HasSCS);
            e.Ignore(x => x.EngineerCount);
            e.Ignore(x => x.TotalAssignedHours);
            e.HasOne(x => x.RO).WithMany(x => x.AssignmentWorks).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SCCCavity).WithMany().HasForeignKey(x => x.SCCCavityId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.SCDCavity).WithMany().HasForeignKey(x => x.SCDCavityId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.SCSCavity).WithMany().HasForeignKey(x => x.SCSCavityId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Engineers).WithOne(x => x.AssignmentWork).HasForeignKey(x => x.AssignmentWorkId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<AssignmentEngineer>(e =>
        {
            e.Property(x => x.AssignedHours).HasPrecision(5, 2);
            e.HasOne(x => x.Engineer).WithMany(x => x.Assignments).HasForeignKey(x => x.EngineerId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<InsuranceCompany>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.InsNo }).IsUnique();
            e.HasMany(x => x.Contracts).WithOne(x => x.InsuranceCompany).HasForeignKey(x => x.InsuranceCompanyId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Claims).WithOne(x => x.InsuranceCompany).HasForeignKey(x => x.InsuranceCompanyId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<InsuranceContract>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.ContractNo }).IsUnique();
            e.Property(x => x.PaymentLimit).HasPrecision(18, 2);
            e.Property(x => x.DiscountLaborRate).HasPrecision(5, 2);
            e.Property(x => x.DiscountPartRate).HasPrecision(5, 2);
            e.HasMany(x => x.Claims).WithOne(x => x.InsuranceContract).HasForeignKey(x => x.InsuranceContractId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<InsuranceClaim>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.ClaimNo }).IsUnique();
            e.Property(x => x.EstimatedAmount).HasPrecision(18, 2);
            e.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
            e.Property(x => x.DeductibleAmount).HasPrecision(18, 2);
            e.Property(x => x.PenaltyAmount).HasPrecision(18, 2);
            e.Property(x => x.InsuranceAmount).HasPrecision(18, 2);
            e.Property(x => x.CustomerAmount).HasPrecision(18, 2);
            e.Ignore(x => x.ItemCount);
            e.HasOne(x => x.RO).WithMany(x => x.InsuranceClaims).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Items).WithOne(x => x.InsuranceClaim).HasForeignKey(x => x.InsuranceClaimId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<InsuranceClaimItem>(e =>
        {
            e.Property(x => x.Quantity).HasPrecision(18, 2);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.EstimatedAmount).HasPrecision(18, 2);
            e.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CampaignMarketing>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.CamMarketingNo }).IsUnique();
            e.Property(x => x.DiscountLaborPercent).HasPrecision(5, 2);
            e.Property(x => x.DiscountPartPercent).HasPrecision(5, 2);
            e.Ignore(x => x.ItemCount);
            e.Ignore(x => x.ROAppliedCount);
            e.Ignore(x => x.IsActiveNow);
            e.Ignore(x => x.TotalDiscountGranted);
            e.Ignore(x => x.TotalRevenueGenerated);
            e.HasMany(x => x.Items).WithOne(x => x.CampaignMarketing).HasForeignKey(x => x.CampaignMarketingId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.AppliedROs).WithOne(x => x.CampaignMarketing).HasForeignKey(x => x.CampaignMarketingId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CampaignMarketingItem>(e =>
        {
            e.Property(x => x.PercentDiscount).HasPrecision(5, 2);
            e.Property(x => x.MaxQuantity).HasPrecision(18, 2);
            e.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CustomerCareMace>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.MaceNo }).IsUnique();
            e.Ignore(x => x.IsOverdue);
            e.Ignore(x => x.IsDueSoon);
            e.HasOne(x => x.Car).WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.RO).WithMany(x => x.CustomerCareMaces).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Appointment).WithOne(x => x.CustomerCareMace).HasForeignKey<CustomerCareMace>(x => x.AppointmentId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<StockAdj>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.StockAdjNo }).IsUnique();
            e.Ignore(x => x.TotalItems);
            e.Ignore(x => x.TotalSystemQty);
            e.Ignore(x => x.TotalActualQty);
            e.Ignore(x => x.TotalDiffQty);
            e.Ignore(x => x.TotalDiffAmount);
            e.Ignore(x => x.HasDiscrepancy);
            e.Ignore(x => x.DiscrepancyCount);
            e.HasMany(x => x.Items).WithOne(x => x.StockAdj).HasForeignKey(x => x.StockAdjId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<StockAdjDetail>(e =>
        {
            e.Ignore(x => x.DiffQuantity);
            e.Ignore(x => x.DiffAmount);
            e.Property(x => x.CostPrice).HasPrecision(18, 2);
            e.Property(x => x.SystemQuantity).HasPrecision(18, 2);
            e.Property(x => x.ActualQuantity).HasPrecision(18, 2);
            e.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Bulletin>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.BulletinNo }).IsUnique();
            e.Ignore(x => x.TotalVinCount);
            e.Ignore(x => x.CompletedVinCount);
            e.Ignore(x => x.PendingVinCount);
            e.Ignore(x => x.CompletionRate);
            e.Ignore(x => x.IsExpired);
            e.HasMany(x => x.Items).WithOne(x => x.Bulletin).HasForeignKey(x => x.BulletinId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.TargetVins).WithOne(x => x.Bulletin).HasForeignKey(x => x.BulletinId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.AppliedROs).WithOne(x => x.Bulletin).HasForeignKey(x => x.BulletinId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<BulletinDetail>(e =>
        {
            e.Ignore(x => x.Amount);
            e.Property(x => x.Quantity).HasPrecision(18, 2);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<BulletinVin>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.BulletinId, x.VinNo });
            e.HasOne(x => x.RO).WithMany().HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<PdiRequest>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.PdiReqNo }).IsUnique();
            e.Ignore(x => x.VinTotal);
            e.Ignore(x => x.VinFTotal);
            e.Ignore(x => x.VinPendingTotal);
            e.Ignore(x => x.CompletionRate);
            e.Ignore(x => x.IsAllPassed);
            e.HasMany(x => x.Items).WithOne(x => x.PdiRequest).HasForeignKey(x => x.PdiRequestId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.RepairOrders).WithOne(x => x.PdiRequest).HasForeignKey(x => x.PdiRequestId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<PdiRequestItem>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.PdiRequestId, x.VIN });
            e.Ignore(x => x.TotalChecklistCount);
            e.Ignore(x => x.PassedChecklistCount);
            e.Ignore(x => x.IssueChecklistCount);
            e.Ignore(x => x.IsReadyForDelivery);
            e.HasOne(x => x.RO).WithMany(x => x.PdiRequestItems).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.ChecklistItems).WithOne(x => x.PdiRequestItem).HasForeignKey(x => x.PdiRequestItemId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<PdiChecklistItem>(e =>
        {
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<OrderComplain>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.OrderComplainNo }).IsUnique();
            e.Ignore(x => x.Amount);
            e.Ignore(x => x.AttachCount);
            e.Ignore(x => x.CanSend);
            e.Ignore(x => x.CanReview);
            e.Ignore(x => x.IsApproved);
            e.Ignore(x => x.IsRejected);
            e.Property(x => x.Quantity).HasPrecision(18, 2);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.HasOne(x => x.OrderPart).WithMany(x => x.Complains).HasForeignKey(x => x.OrderPartId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.AttachFiles).WithOne(x => x.OrderComplain).HasForeignKey(x => x.OrderComplainId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<OrderComplainAttachFile>(e =>
        {
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<TechnicalLibrary>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.TechnicalLibraryCode }).IsUnique();
            e.Ignore(x => x.IsApproved);
            e.Ignore(x => x.StatusText);
            e.HasOne(x => x.RO).WithMany(x => x.TechnicalLibraries).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ServiceItem>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.Ignore(x => x.TotalWithVat);
            e.Ignore(x => x.GrossProfit);
            e.Ignore(x => x.GrossMargin);
            e.Property(x => x.StdManHour).HasPrecision(5, 2);
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Property(x => x.Cost).HasPrecision(18, 2);
            e.Property(x => x.VatPercent).HasPrecision(5, 2);
            e.HasMany(x => x.RepairLines).WithOne(x => x.ServiceItem).HasForeignKey(x => x.ServiceItemId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Supplier>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.HasMany(x => x.SupplierPayments).WithOne(x => x.Supplier).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SupplierPayment>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.SupplierPaymentNo }).IsUnique();
            e.Ignore(x => x.ItemCount);
            e.Ignore(x => x.TotalQuantity);
            e.Ignore(x => x.SubTotal);
            e.Ignore(x => x.TotalVat);
            e.Ignore(x => x.TotalAmount);
            e.Ignore(x => x.CanApprove);
            e.Ignore(x => x.CanCancel);
            e.HasOne(x => x.Supplier).WithMany(x => x.SupplierPayments).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.OrderPart).WithMany(x => x.SupplierPayments).HasForeignKey(x => x.OrderPartId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Items).WithOne(x => x.SupplierPayment).HasForeignKey(x => x.SupplierPaymentId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SupplierPaymentDetail>(e =>
        {
            e.Ignore(x => x.SubTotal);
            e.Ignore(x => x.VatAmount);
            e.Ignore(x => x.Amount);
            e.Property(x => x.QtyPay).HasPrecision(18, 2);
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Property(x => x.VatPercent).HasPrecision(5, 2);
            e.Property(x => x.QtyInventory).HasPrecision(18, 2);
            e.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<StockOutOrder>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.OrderNo }).IsUnique();
            e.Ignore(x => x.ItemCount);
            e.Ignore(x => x.TotalRequestQuantity);
            e.Ignore(x => x.TotalIssuedQuantity);
            e.Ignore(x => x.SubTotal);
            e.Ignore(x => x.TotalVat);
            e.Ignore(x => x.TotalAmount);
            e.Ignore(x => x.CanApprove);
            e.Ignore(x => x.CanIssue);
            e.Ignore(x => x.CanReject);
            e.Ignore(x => x.CanDelete);
            e.HasOne(x => x.RO).WithMany(x => x.StockOutOrders).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Car).WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Cavity).WithMany().HasForeignKey(x => x.CavityId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.StockOut).WithOne(x => x.StockOutOrder).HasForeignKey<StockOutOrder>(x => x.StockOutId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Items).WithOne(x => x.StockOutOrder).HasForeignKey(x => x.StockOutOrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<StockOutOrderDetail>(e =>
        {
            e.Ignore(x => x.SubTotal);
            e.Ignore(x => x.VatAmount);
            e.Ignore(x => x.Amount);
            e.Property(x => x.RequestQuantity).HasPrecision(18, 2);
            e.Property(x => x.IssuedQuantity).HasPrecision(18, 2);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.VatPercent).HasPrecision(5, 2);
            e.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<PartOO>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.OONo }).IsUnique();
            e.HasIndex(x => new { x.OrgId, x.OOPlateNo });
            e.Property(x => x.SoLuongNo).HasPrecision(18, 2);
            e.Property(x => x.SoLuongTra).HasPrecision(18, 2);
            e.Ignore(x => x.SoLuongConNo);
            e.Ignore(x => x.IsConNoKhach);
            e.Ignore(x => x.IsStockAvailable);
            e.Ignore(x => x.TotalOwedAmount);
            e.Ignore(x => x.RemainingAmount);
            e.HasOne(x => x.Part).WithMany(x => x.PartOOs).HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.RO).WithMany(x => x.PartOOs).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Car).WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CusDebit>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.DebitNo }).IsUnique();
            e.Property(x => x.DebitAmount).HasPrecision(18, 2);
            e.Property(x => x.PaidAmount).HasPrecision(18, 2);
            e.Ignore(x => x.RemainAmount);
            e.Ignore(x => x.IsOverdue);
            e.Ignore(x => x.CanPay);
            e.HasOne(x => x.Customer).WithMany(x => x.CusDebits).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Car).WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.RO).WithMany(x => x.CusDebits).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Payments).WithOne(x => x.CusDebit).HasForeignKey(x => x.CusDebitId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CusDebitPayment>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.PaymentNo }).IsUnique();
            e.Property(x => x.PaymentAmount).HasPrecision(18, 2);
            e.HasOne(x => x.Customer).WithMany(x => x.CusDebitPayments).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CusDebit).WithMany(x => x.Payments).HasForeignKey(x => x.CusDebitId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SupplierDebit>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.DebitNo }).IsUnique();
            e.Property(x => x.DebitAmount).HasPrecision(18, 2);
            e.Property(x => x.PaidAmount).HasPrecision(18, 2);
            e.Ignore(x => x.RemainAmount);
            e.Ignore(x => x.IsOverdue);
            e.Ignore(x => x.CanPay);
            e.HasOne(x => x.Supplier).WithMany(x => x.SupplierDebits).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.StockIn).WithMany(x => x.SupplierDebits).HasForeignKey(x => x.StockInId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.OrderPart).WithMany().HasForeignKey(x => x.OrderPartId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Payments).WithOne(x => x.SupplierDebit).HasForeignKey(x => x.SupplierDebitId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SupplierDebitPayment>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.PaymentNo }).IsUnique();
            e.Property(x => x.PaymentAmount).HasPrecision(18, 2);
            e.HasOne(x => x.Supplier).WithMany(x => x.SupplierDebitPayments).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SupplierDebit).WithMany(x => x.Payments).HasForeignKey(x => x.SupplierDebitId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<DealerHistoryRecord>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.RecordNo }).IsUnique();
            e.HasIndex(x => new { x.OrgId, x.PlateNo });
            e.HasIndex(x => new { x.OrgId, x.FrameNo });
            e.Property(x => x.TotalLaborAmount).HasPrecision(18, 2);
            e.Property(x => x.TotalPartAmount).HasPrecision(18, 2);
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
            e.Ignore(x => x.LaborCount);
            e.Ignore(x => x.PartCount);
            e.HasMany(x => x.Items).WithOne(x => x.DealerHistoryRecord).HasForeignKey(x => x.DealerHistoryRecordId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<DealerHistoryItem>(e =>
        {
            e.Property(x => x.Quantity).HasPrecision(18, 2);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<InsuranceDebit>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.DebitNo }).IsUnique();
            e.Property(x => x.DebitAmount).HasPrecision(18, 2);
            e.Property(x => x.PaidAmount).HasPrecision(18, 2);
            e.Ignore(x => x.RemainAmount);
            e.Ignore(x => x.IsOverdue);
            e.Ignore(x => x.CanPay);
            e.HasOne(x => x.InsuranceCompany).WithMany(x => x.Debits).HasForeignKey(x => x.InsuranceCompanyId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.InsuranceContract).WithMany().HasForeignKey(x => x.InsuranceContractId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.RO).WithMany(x => x.InsuranceDebits).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.InsuranceClaim).WithMany(x => x.Debits).HasForeignKey(x => x.InsuranceClaimId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Payments).WithOne(x => x.InsuranceDebit).HasForeignKey(x => x.InsuranceDebitId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<InsuranceDebitPayment>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.PaymentNo }).IsUnique();
            e.Property(x => x.PaymentAmount).HasPrecision(18, 2);
            e.HasOne(x => x.InsuranceCompany).WithMany(x => x.Payments).HasForeignKey(x => x.InsuranceCompanyId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.InsuranceDebit).WithMany(x => x.Payments).HasForeignKey(x => x.InsuranceDebitId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CustomerGroup>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.GroupNo }).IsUnique();
            e.Property(x => x.DiscountPercentLabor).HasPrecision(5, 2);
            e.Property(x => x.DiscountPercentPart).HasPrecision(5, 2);
            e.Property(x => x.CreditLimit).HasPrecision(18, 2);
            e.HasMany(x => x.Members).WithOne(m => m.CustomerGroup).HasForeignKey(m => m.CustomerGroupId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.RepairOrders).WithOne(r => r.CustomerGroup).HasForeignKey(r => r.CustomerGroupId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CustomerGroupMember>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.CustomerGroupId, x.CarId });
            e.HasIndex(x => new { x.OrgId, x.PlateNo });
            e.HasOne(x => x.CustomerGroup).WithMany(g => g.Members).HasForeignKey(x => x.CustomerGroupId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Car).WithMany(c => c.GroupMemberships).HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Customer).WithMany(c => c.GroupMemberships).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<PartPriceRequest>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.ReqPartPriceNo }).IsUnique();
            e.Ignore(x => x.TotalItems);
            e.Ignore(x => x.TotalPricedAmount);
            e.Ignore(x => x.CanSend);
            e.Ignore(x => x.CanSimulateResponse);
            e.Ignore(x => x.CanApprove);
            e.Ignore(x => x.CanCancel);
            e.Ignore(x => x.CanCreateOrderPart);
            e.Ignore(x => x.IsApproved);
            e.HasOne(x => x.RO).WithMany(r => r.PartPriceRequests).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Items).WithOne(i => i.PartPriceRequest).HasForeignKey(i => i.PartPriceRequestId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<PartPriceRequestLine>(e =>
        {
            e.Property(x => x.Quantity).HasPrecision(18, 2);
            e.Property(x => x.TSTPrice).HasPrecision(18, 2);
            e.Ignore(x => x.Amount);
            e.HasOne(x => x.Part).WithMany(p => p.PartPriceRequestLines).HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ComplaintDiagnosticError>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.ErrorCode }).IsUnique();
            e.HasIndex(x => new { x.OrgId, x.ErrorType });
            e.HasIndex(x => new { x.OrgId, x.SystemGroup });
            e.Ignore(x => x.ErrorTypeCode);
            e.Ignore(x => x.ErrorTypeName);
            e.Ignore(x => x.SystemGroupName);
            e.Ignore(x => x.BadgeTypeClass);
            e.Ignore(x => x.BadgeGroupClass);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CustomerCareBirthday>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.CareBthNo }).IsUnique();
            e.HasIndex(x => new { x.OrgId, x.CustomerId });
            e.HasIndex(x => new { x.OrgId, x.DateBth });
            e.HasIndex(x => new { x.OrgId, x.Status });
            e.Property(x => x.GiftVoucherValue).HasPrecision(18, 2);
            e.Property(x => x.DiscountPercent).HasPrecision(5, 2);
            e.Ignore(x => x.BirthMonth);
            e.Ignore(x => x.BirthDay);
            e.Ignore(x => x.CurrentAge);
            e.Ignore(x => x.IsTodayBirthday);
            e.Ignore(x => x.IsThisMonthBirthday);
            e.Ignore(x => x.DaysUntilBirthday);
            e.HasOne(x => x.Customer).WithMany(c => c.BirthdayCares).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Car).WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.UsedInRO).WithMany(r => r.CustomerCareBirthdays).HasForeignKey(x => x.UsedInROId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Appointment).WithMany().HasForeignKey(x => x.AppointmentId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<WarrantyWork>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.Ignore(x => x.TotalWithVat);
            e.Ignore(x => x.UsageCount);
            e.Property(x => x.RateHour).HasPrecision(5, 2);
            e.Property(x => x.RatePrice).HasPrecision(18, 2);
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.HasMany(x => x.RepairLines).WithOne(x => x.WarrantyWork).HasForeignKey(x => x.WarrantyWorkId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<MaintenanceSetting>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.ROMSID }).IsUnique();
            e.HasIndex(x => new { x.OrgId, x.Km });
            e.HasIndex(x => new { x.OrgId, x.Level });
            e.Ignore(x => x.UsageCount);
            e.Ignore(x => x.LevelName);
            e.Ignore(x => x.LevelBadgeClass);
            e.Property(x => x.TakingTimeHours).HasPrecision(5, 2);
            e.Property(x => x.EstimatedCost).HasPrecision(18, 2);
            e.HasOne(x => x.ServicePackage).WithMany().HasForeignKey(x => x.ServicePackageId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.RepairOrders).WithOne(r => r.MaintenanceSetting).HasForeignKey(r => r.MaintenanceSettingId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<WarrantyType>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.TypeCode, x.DetailCode }).IsUnique();
            e.Ignore(x => x.TypeCodeText);
            e.Ignore(x => x.DetailCodeText);
            e.Ignore(x => x.PhotoCount);
            e.HasMany(x => x.Photos).WithOne(p => p.WarrantyType).HasForeignKey(p => p.WarrantyTypeId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<WarrantyTypePhoto>(e =>
        {
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<WarrantyPhotoType>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.ROWPTCode }).IsUnique();
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<DealerTarget>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.TargetYear }).IsUnique();
            e.Ignore(x => x.DetailCount);
            e.Ignore(x => x.DealerCount);
            e.Ignore(x => x.MonthCount);
            e.Ignore(x => x.TotalTargetValue);
            e.HasMany(x => x.Details).WithOne(x => x.DealerTarget).HasForeignKey(x => x.DealerTargetId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<DealerTargetDetail>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.TargetYear, x.DealerCode, x.TargetMonth, x.TargetType }).IsUnique();
            e.Ignore(x => x.TargetMonthNumber);
            e.Ignore(x => x.TargetMonthText);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CustomerType>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.CusTypeCode }).IsUnique();
            e.Property(x => x.CusFactor).HasPrecision(9, 4);
            e.HasMany(x => x.ServiceFactors).WithOne(f => f.CustomerType).HasForeignKey(f => f.CustomerTypeId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CusServiceFactor>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.ServiceItemId, x.CustomerTypeId }).IsUnique();
            e.Property(x => x.Factor).HasPrecision(9, 4);
            e.HasOne(x => x.ServiceItem).WithMany().HasForeignKey(x => x.ServiceItemId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CustomerType).WithMany(t => t.ServiceFactors).HasForeignKey(x => x.CustomerTypeId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<RoHistory>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.ROId });
            e.HasIndex(x => new { x.OrgId, x.Status });
            e.HasOne(x => x.RO).WithMany(r => r.Histories).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CarModel>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.ModelCode }).IsUnique();
            e.HasIndex(x => new { x.OrgId, x.TradeMarkCode });
            e.HasIndex(x => new { x.OrgId, x.Segment });
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Bom>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.BomCode }).IsUnique();
            e.HasMany(x => x.Lines).WithOne(l => l.BomHeader).HasForeignKey(l => l.BomId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<BomLine>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.BomId });
            e.Property(x => x.QtyMin).HasPrecision(18, 4);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<DealerBankAccount>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.DealerCode, x.AccountNo }).IsUnique();
            e.HasIndex(x => new { x.OrgId, x.DealerCode });
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<WarehouseLocation>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.DealerCode, x.LocationCode }).IsUnique();
            e.HasIndex(x => new { x.OrgId, x.DealerCode });
            e.HasIndex(x => new { x.OrgId, x.StockNo });
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<WorkingCalendar>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.CalendarType, x.Date, x.DealerCode }).IsUnique();
            e.HasIndex(x => new { x.OrgId, x.CalendarType, x.Date });
            e.Ignore(x => x.IsWorkingDay);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<RoAttachment>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.ROId, x.ImageName }).IsUnique();
            e.HasIndex(x => new { x.OrgId, x.ROId });
            e.Ignore(x => x.FileExtension);
            e.HasOne(x => x.RO).WithMany(x => x.Attachments).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SharePart>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.SharePartNo }).IsUnique();
            e.HasIndex(x => new { x.OrgId, x.DealerCode });
            e.Ignore(x => x.ItemCount);
            e.Ignore(x => x.TotalQuantityShare);
            e.HasMany(x => x.Lines).WithOne(l => l.SharePart).HasForeignKey(l => l.SharePartId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SharePartLine>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.SharePartId });
            e.Property(x => x.QuantityShare).HasPrecision(18, 2);
            e.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
    }

    public override int SaveChanges() { StampOrg(); return base.SaveChanges(); }
    public override Task<int> SaveChangesAsync(CancellationToken ct = default) { StampOrg(); return base.SaveChangesAsync(ct); }
    private void StampOrg()
    {
        foreach (var e in ChangeTracker.Entries<IOrgOwned>())
            if (e.State == EntityState.Added && e.Entity.OrgId == Guid.Empty) e.Entity.OrgId = _orgId;
    }
}
