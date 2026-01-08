using NetApi.Domain.Common.Contracts;

namespace NetApi.Domain.Products.ValueObjects;

public sealed record ProductCategoryId(uint Value) : IValueObject
{
    public static ProductCategoryId Empty => new(0);
    public static ProductCategoryId FromInt(uint value) => new(value);
    public uint ToInt() => Value;
    public bool IsEmpty() => Value == 0;
}
