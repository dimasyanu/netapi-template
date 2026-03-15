using NetApi.Domain.Approvals.Entities;
using NetApi.Domain.Approvals.ValueObjects;
using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Roles.ValueObjects;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Domain.Approvals;

public class Approval : IHasEntity<ApprovalEntity>
{
    public ApprovalId? Id { get; set; }
    public string Description { get; set; } = "";
    public UserId? RequesterId { get; set; }
    public RoleId? ApproverRoleId { get; set; }
    public byte Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime? DeletedAt { get; set; }
    public string DeletedBy { get; set; } = "";

    public ApprovalEntity ToEntity() => new() {
        Id = Id,
        RequesterId = RequesterId,
        ApproverRoleId = ApproverRoleId,
        Description = Description,
        Status = Status,
        CreatedAt = CreatedAt,
        CreatedBy = CreatedBy,
        DeletedAt = DeletedAt,
        DeletedBy = DeletedBy,
    };
}
