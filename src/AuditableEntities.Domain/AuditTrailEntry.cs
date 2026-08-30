namespace AuditableEntities.Domain;

/// <summary>
/// Represents a single audited change to a field on an instance of <typeparamref name="TEntity"/>.
/// One row is written per changed field, per save, so that the full history of a field's
/// value over time can be reconstructed.
/// </summary>
/// <typeparam name="TEntity">The entity type this audit trail entry describes changes for.</typeparam>
/// <typeparam name="TKey">The type of the primary key of <typeparamref name="TEntity"/>.</typeparam>
public abstract class AuditTrailEntry<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    /// <summary>
    /// Surrogate primary key of the audit trail entry itself.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The primary key of the <typeparamref name="TEntity"/> this entry is associated with.
    /// </summary>
    public required TKey EntityId { get; set; }

    /// <summary>
    /// The date and time (UTC) the entry was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Identifier (e.g. username, email, or subject id) of the user who performed the action.
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// Identifier of the property/field on <typeparamref name="TEntity"/> that was changed.
    /// </summary>
    public required string FieldId { get; set; }

    /// <summary>
    /// The previous value of the field, serialized as a string. Null when the field
    /// previously had no value (e.g. on entity creation).
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// The new value of the field, serialized as a string. Null when the field
    /// was cleared.
    /// </summary>
    public string? NewValue { get; set; }
}
