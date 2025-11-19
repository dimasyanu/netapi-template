using NetApi.Domain.Common.Abstractions;

namespace NetApi.Domain.Users.ValueObjects;

public sealed class UserId(Guid id) : ValueObject
{
    private Guid _id = id;

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return _id;
    }

    public static UserId Create()
        => new(Guid.NewGuid());
}
