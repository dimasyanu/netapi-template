namespace NetApi.Domain.Common.Abstractions;

public abstract class Entity<TId>(TId id) : IEquatable<Entity<TId>> where TId : notnull
{
    public TId Id { get; private set; } = id;

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (Entity<TId>)obj;

        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
        => Id.GetHashCode();

    public bool Equals(Entity<TId>? other)
        => Equals((object?)other);

    public static bool operator ==(Entity<TId>? a, Entity<TId>? b)
        => Equals(a, b);

    public static bool operator !=(Entity<TId>? a, Entity<TId>? b)
        => !Equals(a, b);
}
