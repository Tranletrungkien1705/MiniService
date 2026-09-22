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

    protected override void OnModelCreating(ModelBuilder b)
    {
        if (Database.IsNpgsql()) b.HasDefaultSchema("miniservice");
        b.Entity<Org>().HasIndex(x => x.ApiKey).IsUnique();
        b.Entity<Customer>(e => { e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique(); e.HasQueryFilter(x => x.OrgId == _orgId); });
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
            e.Ignore(x => x.Total); e.Ignore(x => x.LaborTotal); e.Ignore(x => x.PartTotal);
            e.Ignore(x => x.CustomerTotal); e.Ignore(x => x.WarrantyTotal);
            e.Ignore(x => x.PaidAmount); e.Ignore(x => x.RemainingBalance);
            e.HasOne(x => x.Car).WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Appointment).WithMany().HasForeignKey(x => x.AppointmentId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.WarrantyReports).WithOne(x => x.RO).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.StockOuts).WithOne(x => x.RO).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.CustomerCares).WithOne(x => x.RO).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Payments).WithOne(x => x.RO).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.OrderParts).WithOne(x => x.RO).HasForeignKey(x => x.ROId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<RepairLine>(e =>
        {
            e.Ignore(x => x.Amount);
            e.Property(x => x.Quantity).HasPrecision(18, 2);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.HasOne(x => x.RO).WithMany(x => x.Lines).HasForeignKey(x => x.ROId);
            e.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.SetNull);
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
    }

    public override int SaveChanges() { StampOrg(); return base.SaveChanges(); }
    public override Task<int> SaveChangesAsync(CancellationToken ct = default) { StampOrg(); return base.SaveChangesAsync(ct); }
    private void StampOrg()
    {
        foreach (var e in ChangeTracker.Entries<IOrgOwned>())
            if (e.State == EntityState.Added && e.Entity.OrgId == Guid.Empty) e.Entity.OrgId = _orgId;
    }
}
