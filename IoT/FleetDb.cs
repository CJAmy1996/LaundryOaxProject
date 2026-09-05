using Microsoft.EntityFrameworkCore;
namespace LaundryOaxWebAPI.IoT;
public sealed class FleetDb(DbContextOptions<FleetDb> options) : DbContext(options)
{
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<InboxEvent> Events => Set<InboxEvent>();
    public DbSet<CycleNotification> Notifications => Set<CycleNotification>();
    public DbSet<AuditEntry> Audit => Set<AuditEntry>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Machine>().HasKey(x => new { x.TenantId, x.DeviceId });
        b.Entity<Machine>().ToTable(t => {
            t.HasCheckConstraint("CK_Machine_Progress", "[Progress] >= 0 AND [Progress] <= 100");
            t.HasCheckConstraint("CK_Machine_Sequence", "[LastSequence] >= 0");
        });
        b.Entity<InboxEvent>().ToTable(t => t.HasCheckConstraint("CK_Event_Sequence", "[Sequence] > 0"));
        b.Entity<Machine>().Property(x => x.LastSequence).IsConcurrencyToken();
        b.Entity<InboxEvent>().HasKey(x => new { x.TenantId, x.EventId });
        b.Entity<InboxEvent>().HasIndex(x => new { x.TenantId, x.DeviceId, x.Sequence }).IsUnique();
        b.Entity<InboxEvent>().HasIndex(x => x.ProcessedUtc);
        b.Entity<InboxEvent>().HasOne<Machine>().WithMany().HasForeignKey(x => new { x.TenantId, x.DeviceId });
        b.Entity<CycleNotification>().HasKey(x => new { x.TenantId, x.Id });
        b.Entity<CycleNotification>().HasIndex(x => new { x.TenantId, x.CreatedUtc });
        b.Entity<AuditEntry>().HasIndex(x => new { x.TenantId, x.CreatedUtc });
    }
}
