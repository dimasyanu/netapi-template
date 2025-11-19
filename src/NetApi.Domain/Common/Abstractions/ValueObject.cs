namespace NetApi.Domain.Common.Abstractions;

public abstract class ValueObject : IEquatable<ValueObject>
{
    public abstract IEnumerable<object> GetEqualityComponents();
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((hash, obj) => hash ^ obj);
    }

    public bool Equals(ValueObject? other)
        => Equals((object?)other);

    public static bool operator ==(ValueObject? a, ValueObject? b)
        => Equals(a, b);

    public static bool operator !=(ValueObject? a, ValueObject? b)
        => !Equals(a, b);
}
