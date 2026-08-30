using AuditedEntities.Data.Sample;
using AuditedEntities.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuditedEntities.Data;

/// <summary>
/// EF Core database context. Managed via EF Core migrations against SQL Server
/// (see the "dotnet ef migrations" commands in the README).
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Identifier of the user attributed to audit trail entries created by the next
    /// call to <see cref="SaveChanges()"/>/<see cref="SaveChangesAsync"/>. Set this
    /// (e.g. from the current HTTP/auth context) before saving.
    /// </summary>
    public string CurrentUserId { get; set; } = "system";

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductAuditTrailEntry> ProductAuditTrailEntries => Set<ProductAuditTrailEntry>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        CreateAuditTrailEntries();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        CreateAuditTrailEntries();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Inspects tracked <see cref="Product"/> entries for added/modified scalar
    /// property values and appends a <see cref="ProductAuditTrailEntry"/> per
    /// changed field to that product's <see cref="Product.AuditTrail"/> navigation
    /// collection. EF Core fixes up the FK (<see cref="ProductAuditTrailEntry.EntityId"/>)
    /// automatically once the product's key is generated, even for new products.
    /// </summary>
    private void CreateAuditTrailEntries()
    {
        var timestamp = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Product>())
        {
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
            {
                continue;
            }

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.Name == nameof(Product.Id))
                {
                    continue;
                }

                bool isNew = entry.State == EntityState.Added;
                if (!isNew && !property.IsModified)
                {
                    continue;
                }

                string? oldValue = isNew ? null : property.OriginalValue?.ToString();
                string? newValue = property.CurrentValue?.ToString();

                if (!isNew && oldValue == newValue)
                {
                    continue;
                }

                entry.Entity.AuditTrail.Add(new ProductAuditTrailEntry
                {
                    EntityId = entry.Entity.Id,
                    Timestamp = timestamp,
                    UserId = CurrentUserId,
                    FieldId = property.Metadata.Name,
                    OldValue = oldValue,
                    NewValue = newValue,
                });
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(builder =>
        {
            builder.ToTable("Products");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedOnAdd();
            builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
            builder.Property(p => p.Price).HasColumnType("decimal(18,2)");

            builder.HasMany(p => p.AuditTrail)
                .WithOne()
                .HasForeignKey(e => e.EntityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AuditTrail is typed as the abstract AuditTrailEntry<Product, int> (declared on
        // AuditedEntity<TSelf, TKey>), so the key/columns/table must be configured on that
        // base type; ProductAuditTrailEntry then maps onto the same table via table-per-
        // hierarchy (it's the only concrete derived type, so no discriminator column is
        // needed - EF omits it automatically for a single-type hierarchy).
        modelBuilder.Entity<AuditTrailEntry<Product, int>>(builder =>
        {
            builder.ToTable("ProductAuditTrailEntries");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
            builder.Property(e => e.EntityId).IsRequired();
            builder.Property(e => e.Timestamp).IsRequired();
            builder.Property(e => e.UserId).IsRequired().HasMaxLength(256);
            builder.Property(e => e.FieldId).IsRequired().HasMaxLength(256);
            builder.Property(e => e.OldValue);
            builder.Property(e => e.NewValue);

            builder.HasIndex(e => e.EntityId);
        });
    }
}
