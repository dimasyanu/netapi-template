using NetApi.Domain.Roles.ValueObjects;
using NetApi.Domain.Users;

namespace NetApi.Domain.Roles;

public class Role
{
    public RoleId Id { get; set; } = new(0);
    public string Name { get; set; } = "";
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";
    public bool IsDeleted { get; set; } = false;

    public List<User>? Users { get; set; }

    public static Role FromRoleEntity(Entities.RoleEntity roleEntity)
    {
        return new Role {
            Id = roleEntity.Id,
            Name = roleEntity.Name,
            Description = roleEntity.Description,
            CreatedAt = roleEntity.CreatedAt,
            CreatedBy = roleEntity.CreatedBy,
            UpdatedAt = roleEntity.UpdatedAt,
            UpdatedBy = roleEntity.UpdatedBy,
            IsDeleted = roleEntity.DeletedAt != null,
            Users = roleEntity.Users?.Select(User.FromEntity).ToList()
        };
    }
}
