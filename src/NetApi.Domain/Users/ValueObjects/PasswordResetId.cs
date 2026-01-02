using NetApi.Domain.Common.Contracts;

namespace NetApi.Domain.Users.ValueObjects;

public sealed record PasswordResetId(Guid Value) : IValueObject
{
    public static PasswordResetId Empty => new(Guid.Empty);
    public static PasswordResetId FromGuid(Guid id) => new(id);
    public static PasswordResetId New() => new(Guid.NewGuid());

    public Guid ToGuid() => Value;
    public override string ToString() => Value.ToString();
    public bool IsEmpty() => Value == Guid.Empty;
}
