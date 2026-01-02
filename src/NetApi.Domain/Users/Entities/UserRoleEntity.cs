using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Roles.ValueObjects;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Domain.Users.Entities;

public class UserRoleEntity
{
    public UserId UserId { get; set; } = UserId.Empty;
    public RoleId RoleId { get; set; } = RoleId.Empty;
    public DateTime AssignedAt { get; set; } = DateTime.Now;

    public virtual RoleEntity? Role { get; set; }
    public virtual UserEntity? User { get; set; }
}
