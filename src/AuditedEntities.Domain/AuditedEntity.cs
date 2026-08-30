namespace AuditedEntities.Domain;

/// <summary>
/// Convenience base class for entities that want built-in audit trail support:
/// it extends <see cref="Entity{TKey}"/>, implements <see cref="IAudited{TEntity, TKey}"/>
/// for itself, and exposes/implements <see cref="GetAuditTrailEntries"/> via an
/// <see cref="AuditTrail"/> navigation collection (populate it with
/// <c>.Include(x => x.AuditTrail)</c> when querying).
/// </summary>
/// <typeparam name="TSelf">
/// The concrete, most-derived entity type (curiously recurring template pattern),
/// e.g. <c>class Product : AuditedEntity&lt;Product, int&gt;</c>. This lets
/// <see cref="AuditTrail"/> and <see cref="GetAuditTrailEntries"/> be strongly typed
/// to the concrete entity without every derived class re-implementing them.
/// </typeparam>
/// <typeparam name="TKey">The type of the primary key of <typeparamref name="TSelf"/>.</typeparam>
public abstract class AuditedEntity<TSelf, TKey> : Entity<TKey>, IAudited<TSelf, TKey>
    where TSelf : AuditedEntity<TSelf, TKey>
    where TKey : IComparable<TKey>
{
    /// <summary>
    /// EF Core navigation collection of audit trail entries for this entity, keyed by
    /// <see cref="AuditTrailEntry{TEntity, TKey}.EntityId"/> == <see cref="Entity{TKey}.Id"/>.
    /// </summary>
    public ICollection<AuditTrailEntry<TSelf, TKey>> AuditTrail { get; set; } = new List<AuditTrailEntry<TSelf, TKey>>();

    /// <inheritdoc />
    public IReadOnlyCollection<AuditTrailEntry<TSelf, TKey>> GetAuditTrail(TSelf entity) => GetAuditTrailEntries(entity);

    /// <summary>
    /// Returns the set of audit trail entries recorded for the given entity instance,
    /// ordered chronologically. This is the concrete implementation backing
    /// <see cref="IAudited{TEntity, TKey}.GetAuditTrail"/>.
    /// </summary>
    /// <param name="entity">The entity to retrieve audit trail entries for.</param>
    public IReadOnlyCollection<AuditTrailEntry<TSelf, TKey>> GetAuditTrailEntries(TSelf entity) =>
        entity.AuditTrail.OrderBy(e => e.Timestamp).ToList();
}
