using NetApi.Domain.Common.Contracts;

namespace NetApi.Domain.Products.ValueObjects;

public sealed record ProductTagId(uint Value) : IValueObject
{
    public static ProductTagId Empty => new(0);
    public static ProductTagId FromInt(uint value) => new(value);
    public uint ToInt() => Value;
    public bool IsEmpty() => Value == 0;
}
