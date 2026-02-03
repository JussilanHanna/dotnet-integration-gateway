using IntegrationGateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntegrationGateway.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Invoice>()
            .HasIndex(x => x.ExternalInvoiceId)
            .IsUnique();

        b.Entity<IdempotencyRecord>()
            .HasIndex(x => new { x.Key, x.Route })
            .IsUnique();

        b.Entity<Invoice>()
            .HasMany(x => x.Lines)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(b);
    }
}
