using NetApi.Domain.Approvals.ValueObjects;
using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Roles.ValueObjects;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Domain.Approvals.Entities;

public class ApprovalEntity : IEntity<ApprovalId>, ISoftDelete
{
    public ApprovalId? Id { get; set; }

    public string Description { get; set; } = "";
    public UserId? RequesterId { get; set; }
    public RoleId? ApproverRoleId { get; set; }
    public byte Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
