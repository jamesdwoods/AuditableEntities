namespace AuditedEntities.Domain;

/// <summary>
/// Convenience base class for entities that want built-in audit trail support:
/// it extends <see cref="Entity{TKey}"/>, implements <see cref="IAudited{TEntity, TKey}"/>
/// for itself, and exposes/implements <see cref="GetAuditTrailEntries"/> via a protected
/// <see cref="AuditTrail"/> navigation collection (populate it with
/// <c>.Include(x => x.AuditTrail)</c> when querying). The collection itself is not
/// publicly accessible - callers read it via <see cref="GetAuditTrailEntries"/> and
/// append to it via <see cref="AddAuditTrailEntry"/>.
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
    /// Deliberately not public - EF Core maps it by name via <c>HasMany("AuditTrail")</c>
    /// (see <c>AppDbContext.OnModelCreating</c>), and callers use
    /// <see cref="GetAuditTrailEntries"/>/<see cref="AddAuditTrailEntry"/> instead of
    /// mutating the collection directly.
    /// </summary>
    protected ICollection<AuditTrailEntry<TSelf, TKey>> AuditTrail { get; set; } = new List<AuditTrailEntry<TSelf, TKey>>();

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

    /// <summary>
    /// Appends an entry to this entity's audit trail, describing a single field change.
    /// This is the only supported way for external code (e.g. <c>AppDbContext.SaveChanges</c>)
    /// to record a change, since <see cref="AuditTrail"/> itself is not publicly accessible
    /// and callers never need to know the concrete <see cref="AuditTrailEntry{TEntity, TKey}"/>
    /// subtype - <see cref="CreateAuditTrailEntry"/> constructs it.
    /// </summary>
    /// <param name="timestamp">The date and time (UTC) the change was made.</param>
    /// <param name="userId">Identifier of the user who made the change.</param>
    /// <param name="fieldId">Identifier of the property/field that changed.</param>
    /// <param name="oldValue">The previous value of the field, or null if there was none.</param>
    /// <param name="newValue">The new value of the field, or null if it was cleared.</param>
    public void AddAuditTrailEntry(DateTimeOffset timestamp, string userId, string fieldId, string? oldValue, string? newValue) =>
        AuditTrail.Add(CreateAuditTrailEntry(Id, timestamp, userId, fieldId, oldValue, newValue));

    /// <summary>
    /// Constructs the concrete <see cref="AuditTrailEntry{TEntity, TKey}"/> subtype for
    /// this entity (e.g. <c>Product</c> returns a <c>ProductAuditTrailEntry</c>), so that
    /// callers of <see cref="AddAuditTrailEntry"/> never reference the concrete type.
    /// </summary>
    protected abstract AuditTrailEntry<TSelf, TKey> CreateAuditTrailEntry(
        TKey entityId, DateTimeOffset timestamp, string userId, string fieldId, string? oldValue, string? newValue);
}
