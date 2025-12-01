using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Roles.ValueObjects;
using NetApi.Domain.Users.Entities;

namespace NetApi.Domain.Roles.Entities;

public class RoleEntity : ITimestamp, ISoftDelete
{
    public RoleId Id { get; set; } = new(0);
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public List<UserEntity>? Users { get; set; }
}