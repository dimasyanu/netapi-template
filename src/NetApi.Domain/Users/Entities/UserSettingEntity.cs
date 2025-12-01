using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Domain.Users.Entities;

public class UserSettingEntity : IEntity
{
    public UserId UserId { get; set; } = UserId.Create();
    public string Key { get; set; } = "";
    public string? Value { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";

    public UserEntity? User { get; set; }
}
