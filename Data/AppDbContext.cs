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
