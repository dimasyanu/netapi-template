using NetApi.Domain.Common.Contracts;

namespace NetApi.Domain.Users.ValueObjects;

public sealed record LoginAttemptId(Guid Value) : IValueObject
{
    public static LoginAttemptId Empty => new(Guid.Empty);
    public static LoginAttemptId FromGuid(Guid id) => new(id);

    public Guid ToGuid() => Value;
    public override string ToString() => Value.ToString();
    public bool IsEmpty() => Value == Guid.Empty;
}
