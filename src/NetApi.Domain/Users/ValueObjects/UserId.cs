using NetApi.Domain.Common.Abstractions;
using NetApi.Domain.Common.Contracts;

namespace NetApi.Domain.Users.ValueObjects;

public sealed class UserId : ValueObject, IGuidObject
{
    private readonly Guid _id;

    private UserId(Guid id)
    {
        _id = id;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return _id;
    }

    public static UserId Create()
        => new(Guid.NewGuid());

    public static UserId Create(Guid id)
        => new(id);

    public Guid ToGuid() => _id;
}
