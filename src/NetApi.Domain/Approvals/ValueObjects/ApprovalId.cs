using NetApi.Domain.Common.Contracts;

namespace NetApi.Domain.Approvals.ValueObjects;

public sealed record ApprovalId(Guid Value) : IValueObject
{
    public static ApprovalId Empty => new(Guid.Empty);
    public static ApprovalId FromGuid(Guid guid) => new(guid);
    public static ApprovalId New() => new(Guid.NewGuid());

    public Guid ToGuid() => Value;
    public override string ToString() => Value.ToString();
    public bool IsEmpty() => Value == Guid.Empty;
}
