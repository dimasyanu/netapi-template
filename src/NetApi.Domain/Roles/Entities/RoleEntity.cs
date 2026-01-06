using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Roles.ValueObjects;
using NetApi.Domain.Users.Entities;

namespace NetApi.Domain.Roles.Entities;

public class RoleEntity : IEntity<RoleId>, ITimestamp, ISoftDelete
{
    public RoleId? Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }

    public bool IsSuperAdmin { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public virtual List<UserEntity>? Users { get; set; }
    public virtual List<RolePermissionEntity>? Permissions { get; set; }
}
