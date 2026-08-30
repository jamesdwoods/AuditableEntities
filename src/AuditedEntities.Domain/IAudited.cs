namespace AuditedEntities.Domain;

/// <summary>
/// Implemented by services/repositories that can produce the audit trail history
/// for a given entity. The constraint <c>TEntity : Entity&lt;TKey&gt;</c> means this is
/// effectively <c>IAudited&lt;Entity&lt;TKey&gt;&gt;</c>, with the key type named explicitly
/// so it can be used to strongly type <see cref="AuditTrailEntry{TEntity, TKey}"/>.
/// </summary>
/// <typeparam name="TEntity">The entity type being audited.</typeparam>
/// <typeparam name="TKey">The type of the primary key of <typeparamref name="TEntity"/>.</typeparam>
public interface IAudited<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    /// <summary>
    /// Returns the set of audit trail entries recorded for the given entity instance,
    /// ordered chronologically.
    /// </summary>
    /// <param name="entity">The entity to retrieve audit trail entries for.</param>
    IReadOnlyCollection<AuditTrailEntry<TEntity, TKey>> GetAuditTrail(TEntity entity);
}
