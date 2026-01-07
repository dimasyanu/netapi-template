using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Roles.ValueObjects;

namespace NetApi.Domain.Roles;

public class RolePermission : IHasEntity<RolePermissionEntity>
{
    public RolePermissionId? Id { get; set; }
    public RoleId RoleId { get; set; } = RoleId.Empty;

    public string Feature { get; set; } = "";
    public byte Action { get; set; }
    public bool IsAllowed { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";

    public RolePermissionEntity ToEntity()
        => new() {
            Id = Id,
            RoleId = RoleId,
            Feature = Feature,
            Action = Action,
            IsAllowed = IsAllowed,
            CreatedAt = CreatedAt,
            CreatedBy = CreatedBy,
            UpdatedAt = UpdatedAt,
            UpdatedBy = UpdatedBy,
        };

    public static RolePermission FromEntity(RolePermissionEntity entity)
        => new() {
            Id = entity.Id,
            RoleId = entity.RoleId,
            Feature = entity.Feature,
            Action = entity.Action,
            IsAllowed = entity.IsAllowed,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
        };
}

