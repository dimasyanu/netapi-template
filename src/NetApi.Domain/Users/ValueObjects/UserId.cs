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

    public static UserId Empty { get; } = new(Guid.Empty);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return _id;
    }

    public static UserId Create()
        => new(Guid.NewGuid());

    public static UserId FromGuid(Guid id)
        => new(id);

    public Guid ToGuid() => _id;
    public override string ToString() => _id.ToString();
    public bool IsEmpty() => _id == Guid.Empty;
}
