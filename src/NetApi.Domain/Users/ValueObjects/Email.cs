using NetApi.Domain.Common.Abstractions;
using NetApi.Domain.Common.Contracts;

namespace NetApi.Domain.Users.ValueObjects;

public class EmailAddress : ValueObject, IStringObject
{
    private readonly string _emailAddress;

    private EmailAddress(string emailAddress)
    {
        _emailAddress = emailAddress;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return _emailAddress;
    }

    public static EmailAddress FromString(string emailAddress)
        => new(emailAddress);

    public static EmailAddress NewEmpty()
        => new("");

    public override string ToString()
        => _emailAddress;
}
