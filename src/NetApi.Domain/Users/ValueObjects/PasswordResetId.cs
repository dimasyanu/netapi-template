using NetApi.Domain.Common.Abstractions;
using NetApi.Domain.Common.Contracts;

namespace NetApi.Domain.Users.ValueObjects;

public class PasswordResetId : ValueObject, IGuidObject
{
    private readonly Guid _id;

    private PasswordResetId(Guid id)
    {
        _id = id;
    }

    public static PasswordResetId Empty { get; } = new(Guid.Empty);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return _id;
    }

    public static PasswordResetId Create()
        => new(Guid.NewGuid());

    public static PasswordResetId FromGuid(Guid id)
        => new(id);

    public Guid ToGuid() => _id;
    public override string ToString() => _id.ToString();
    public bool IsEmpty() => _id == Guid.Empty;
}
