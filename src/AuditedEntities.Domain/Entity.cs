namespace AuditedEntities.Domain;

/// <summary>
/// Base class for all entities persisted via Entity Framework.
/// </summary>
/// <typeparam name="TKey">The type of the entity's primary key. Must be comparable so
/// entities can be ordered/sorted by key (e.g. <c>int</c>, <c>Guid</c>, <c>string</c>).</typeparam>
public abstract class Entity<TKey> : IComparable<Entity<TKey>>
    where TKey : IComparable<TKey>
{
    /// <summary>
    /// The primary key of the entity.
    /// </summary>
    public required TKey Id { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TKey> other || other.GetType() != GetType())
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return EqualityComparer<TKey>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode() => EqualityComparer<TKey>.Default.GetHashCode(Id!);

    public int CompareTo(Entity<TKey>? other) => other is null ? 1 : Id.CompareTo(other.Id);

    public static bool operator ==(Entity<TKey>? left, Entity<TKey>? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity<TKey>? left, Entity<TKey>? right) => !(left == right);
}
