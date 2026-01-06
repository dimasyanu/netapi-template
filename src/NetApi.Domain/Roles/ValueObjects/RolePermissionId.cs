using NetApi.Domain.Common.Contracts;

namespace NetApi.Domain.Roles.ValueObjects;

public sealed record RolePermissionId(Guid Value) : IValueObject
{
    public static RolePermissionId Empty => new(Guid.Empty);
    public static RolePermissionId FromGuid(Guid id) => new(id);
    public static RolePermissionId New() => new(Guid.NewGuid());

    public Guid ToGuid() => Value;
    public override string ToString() => Value.ToString();
    public bool IsEmpty() => Value == Guid.Empty;
}

