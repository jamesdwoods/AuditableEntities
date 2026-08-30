using AuditableEntities.Domain;

namespace AuditableEntities.Data.Sample;

/// <summary>
/// Audit trail entry for changes made to <see cref="Product"/> instances.
/// </summary>
public class ProductAuditTrailEntry : AuditTrailEntry<Product, int>
{
}
