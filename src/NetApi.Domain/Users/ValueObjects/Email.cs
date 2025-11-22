using NetApi.Domain.Common.Abstractions;

namespace NetApi.Domain.Users.ValueObjects;

public class EmailAddress : ValueObject
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

    public static EmailAddress Create(string emailAddress)
        => new(emailAddress);

    public static EmailAddress NewEmpty()
        => new("");
}
