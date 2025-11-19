using NetApi.Domain.Common.Abstractions;

namespace NetApi.Domain.Roles.ValueObjects;

public sealed class RoleId(short value) : ValueObject
{
    public short Value { get; } = value;

    public static implicit operator short(RoleId roleId) => roleId.Value;

    public static implicit operator RoleId(short value) => new(value);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static RoleId Create()
    {
        return new RoleId(0);
    }
}
