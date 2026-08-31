using AuditedEntities.Domain;

namespace AuditedEntities.Data.Sample;

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

    protected override AuditTrailEntry<Product, int> CreateAuditTrailEntry(
        int entityId, DateTimeOffset timestamp, string userId, string fieldId, string? oldValue, string? newValue) =>
        new ProductAuditTrailEntry
        {
            EntityId = entityId,
            Timestamp = timestamp,
            UserId = userId,
            FieldId = fieldId,
            OldValue = oldValue,
            NewValue = newValue,
        };
}
