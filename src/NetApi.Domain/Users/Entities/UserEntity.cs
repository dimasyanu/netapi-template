using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Domain.Users.Entities;

public class UserEntity :
    IEntity<UserId>,
    ITimestamp,
    ISoftDelete
{
    public UserId? Id { get; set; } = UserId.Create();
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Username { get; set; } = "";
    public EmailAddress EmailAddress { get; set; } = EmailAddress.Empty;
    public string PasswordHash { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public virtual List<RoleEntity> Roles { get; set; } = [];
    public virtual List<UserSettingEntity> UserSettings { get; set; } = [];
}