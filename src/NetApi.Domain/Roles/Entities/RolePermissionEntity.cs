using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Roles.ValueObjects;

namespace NetApi.Domain.Roles.Entities;

public class RolePermissionEntity : IEntity<RolePermissionId>, ITimestamp
{
    public RolePermissionId? Id { get; set; }
    public RoleId RoleId { get; set; } = RoleId.Empty;

    public string Feature { get; set; } = "";
    public byte Action { get; set; }
    public bool IsAllowed { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";

    public virtual RoleEntity? Role { get; set; }
}

