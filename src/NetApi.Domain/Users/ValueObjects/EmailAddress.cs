using NetApi.Domain.Common.Contracts;

namespace NetApi.Domain.Users.ValueObjects;

public sealed record EmailAddress(string Value) : IValueObject
{
    public static EmailAddress Empty => new("");
    public static EmailAddress FromString(string value) => new(value);

    public override string ToString() => Value;
    public bool IsEmpty() => string.IsNullOrEmpty(Value);
}

public static class EmailAddressExtensions
{
    public static string ToLower(this EmailAddress emailAddress)
        => emailAddress.Value.ToLower();

    public static string ToUpper(this EmailAddress emailAddress)
        => emailAddress.Value.ToUpper();
}