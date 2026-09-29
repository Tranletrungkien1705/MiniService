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
    public DbSet<StockOut> StockOuts => Set<StockOut>();
    public DbSet<Payment> Payments => Set<Payment>();
    // ---- Port 1:1 từ DMSCarSv — xem Models/EntitiesCarSv.cs ----
    public DbSet<RoStockRequisition> StockReqs => Set<RoStockRequisition>();
    public DbSet<PartQuotation> PartQuotations => Set<PartQuotation>();
    public DbSet<PartQuotationItem> PartQuotationItems => Set<PartQuotationItem>();
    public DbSet<PartOrder> PartOrders => Set<PartOrder>();
    public DbSet<PartOrderDetail> PartOrderDetails => Set<PartOrderDetail>();

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
        b.Entity<RepairOrder>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.Ignore(x => x.Total); e.Ignore(x => x.LaborTotal); e.Ignore(x => x.PartTotal);
            e.HasOne(x => x.Car).WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<RepairLine>(e =>
        {
            e.Ignore(x => x.Amount);
            e.Property(x => x.Quantity).HasPrecision(18, 2);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.Vat).HasPrecision(18, 2);   // Đợt 2 — Ser_ROPartItems.VAT (DbDefine.cs:1004)
            e.HasOne(x => x.RO).WithMany(x => x.Lines).HasForeignKey(x => x.ROId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Part>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Ignore(x => x.LowStock); e.Ignore(x => x.StockValue);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<StockOut>(e =>
        {
            e.Ignore(x => x.Amount);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.HasIndex(x => new { x.OrgId, x.PartId });
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Payment>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasIndex(x => new { x.OrgId, x.ROId });
            // Đợt 2 — bảng kê phiếu thu lọc theo PayDate + PaymentType (BizCarSv.Debit.cs:3705-3708).
            e.HasIndex(x => new { x.OrgId, x.PaidAt });
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });

        // ===== Port 1:1 DMSCarSv =====
        // Ser_ROStockRequisition — FrmROStockRequisitionList / FrmROStockRequisition
        b.Entity<RoStockRequisition>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.StkReqId }).IsUnique();
            e.HasIndex(x => new { x.OrgId, x.RONo });
            // 3 trường dẫn xuất — nguồn tính ở tầng collection, không lưu DB.
            e.Ignore(x => x.ROReStockRequisition); e.Ignore(x => x.StatusName); e.Ignore(x => x.LSNo);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        // Ser_Inv_Quote — FrmPartQuotationSearch
        b.Entity<PartQuotation>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.QuoteNo }).IsUnique();
            e.Ignore(x => x.SumAmount); e.Ignore(x => x.StatusValue); e.Ignore(x => x.StatusText);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<PartQuotationItem>(e =>
        {
            e.Ignore(x => x.Amount);
            e.Property(x => x.Quantity).HasPrecision(18, 2);
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Property(x => x.VAT).HasPrecision(18, 2);
            e.Property(x => x.Factor).HasPrecision(18, 4);
            e.HasOne(x => x.Quote).WithMany(x => x.Items).HasForeignKey(x => x.QuoteId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        // Ser_Part_Order — FrmOrderPartSearch
        b.Entity<PartOrder>(e =>
        {
            e.HasIndex(x => new { x.OrgId, x.OrderNo }).IsUnique();
            e.Ignore(x => x.Amount); e.Ignore(x => x.SumQuantity);
            e.Ignore(x => x.SumDeliveryQuantity); e.Ignore(x => x.StatusText);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<PartOrderDetail>(e =>
        {
            e.Ignore(x => x.BeforeTax); e.Ignore(x => x.AfterTax); e.Ignore(x => x.PendingDeliveryQty);
            e.Property(x => x.Quantity).HasPrecision(18, 2);
            e.Property(x => x.OrderQuantity).HasPrecision(18, 2);
            e.Property(x => x.DeliveryQuantity).HasPrecision(18, 2);
            e.Property(x => x.Cost).HasPrecision(18, 2);
            e.Property(x => x.VAT).HasPrecision(18, 2);
            e.Property(x => x.Discount).HasPrecision(18, 2);
            e.Property(x => x.Factor).HasPrecision(18, 4);
            e.HasOne(x => x.Order).WithMany(x => x.Details).HasForeignKey(x => x.OrderPartId);
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
