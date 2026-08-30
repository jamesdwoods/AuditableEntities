using AuditableEntities.Domain;

namespace AuditableEntities.Data.Sample;

// Product extends AuditedEntity, so it gets its AuditTrail navigation collection and
// IAudited.GetAuditTrail implementation for free - eagerly load audit history with
// `.Include(p => p.AuditTrail)` and call GetAuditTrail()/GetAuditTrailEntries().

/// <summary>
/// Example concrete entity used to demonstrate the Entity/AuditTrailEntry/IAudited
/// abstractions end-to-end with Entity Framework Core against SQL Server.
/// </summary>
public class Product : AuditedEntity<Product, int>
{
    public required string Name { get; set; }

    public decimal Price { get; set; }
}
