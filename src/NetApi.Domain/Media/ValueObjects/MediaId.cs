using NetApi.Domain.Common.Contracts;

namespace NetApi.Domain.Media.ValueObjects;

public sealed record MediaId(Guid Value) : IValueObject
{
    public static MediaId Empty => new(Guid.Empty);
    public static MediaId FromGuid(Guid id) => new(id);
    public static MediaId New() => new(Guid.NewGuid());

    public Guid ToGuid() => Value;
    public override string ToString() => Value.ToString();
    public bool IsEmpty() => Value == Guid.Empty;
}
