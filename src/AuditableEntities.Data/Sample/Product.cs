using AuditableEntities.Domain;

namespace AuditableEntities.Data.Sample;

// Product owns its audit trail as an EF Core navigation collection, so no separate
// repository/reader class is needed just to surface audit history - eagerly load it
// with `.Include(p => p.AuditTrail)` and call GetAuditTrail().

/// <summary>
/// Example concrete entity used to demonstrate the Entity/AuditTrailEntry/IAuditable
/// abstractions end-to-end with Entity Framework Core against SQL Server.
/// </summary>
public class Product : Entity<int>, IAuditable<Product, int>
{
    public required string Name { get; set; }

    public decimal Price { get; set; }

    /// <summary>
    /// EF Core navigation collection of audit trail entries for this product,
    /// keyed by <see cref="ProductAuditTrailEntry.EntityId"/> == <see cref="Entity{TKey}.Id"/>.
    /// Populate it via <c>.Include(p => p.AuditTrail)</c> before calling <see cref="GetAuditTrail"/>.
    /// </summary>
    public ICollection<ProductAuditTrailEntry> AuditTrail { get; set; } = new List<ProductAuditTrailEntry>();

    public IReadOnlyCollection<AuditTrailEntry<Product, int>> GetAuditTrail(Product entity) =>
        entity.AuditTrail.OrderBy(e => e.Timestamp).ToList();
}
