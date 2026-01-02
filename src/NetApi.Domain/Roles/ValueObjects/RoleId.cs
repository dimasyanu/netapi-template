using NetApi.Domain.Common.Contracts;

namespace NetApi.Domain.Roles.ValueObjects;

public sealed record RoleId(ushort Value) : IValueObject
{
    public static RoleId Empty => new(0);
    public static RoleId FromShort(ushort value) => new(value);

    public ushort ToShort() => Value;
    public bool IsEmpty() => Value == 0;
}
