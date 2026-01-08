using NetApi.Domain.Common.Contracts;

namespace NetApi.Domain.Products.ValueObjects;

public sealed record ProductId(uint Value) : IValueObject
{
    public static ProductId Empty => new(0);
    public static ProductId FromInt(uint value) => new(value);
    public uint ToInt() => Value;
    public bool IsEmpty() => Value == 0;
}
