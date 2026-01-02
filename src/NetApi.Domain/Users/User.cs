using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Roles;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Domain.Users;

public class User : IHasEntity<UserEntity>
{
    public UserId? Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Username { get; set; } = "";
    public EmailAddress EmailAddress { get; set; } = EmailAddress.Empty;
    public string RefreshToken { get; set; } = "";
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";
    public bool IsDeleted { get; set; } = false;

    public IReadOnlyList<Role>? Roles { get; set; }
    public UserSetting? UserSettings { get; set; }

    public static User FromEntity(UserEntity userEntity)
    {
        return new User {
            Id = userEntity.Id,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
            Username = userEntity.Username,
            EmailAddress = userEntity.EmailAddress,
            RefreshToken = userEntity.RefreshToken,
            RefreshTokenExpiryTime = userEntity.RefreshTokenExpiryTime,
            CreatedAt = userEntity.CreatedAt,
            CreatedBy = userEntity.CreatedBy,
            UpdatedAt = userEntity.UpdatedAt,
            UpdatedBy = userEntity.UpdatedBy,
            IsDeleted = userEntity.DeletedAt.HasValue,
            Roles = [.. userEntity.Roles?.Select(x => Role.FromEntity(x, false)) ?? []],
            UserSettings = UserSetting.FromEntities(userEntity.UserSettings)
        };
    }

    public UserEntity ToEntity()
    {
        return new UserEntity {
            Id = Id,
            FirstName = FirstName,
            LastName = LastName,
            Username = Username,
            EmailAddress = EmailAddress,
            RefreshToken = RefreshToken,
            RefreshTokenExpiryTime = RefreshTokenExpiryTime,
            CreatedAt = CreatedAt,
            CreatedBy = CreatedBy,
            UpdatedAt = UpdatedAt,
            UpdatedBy = UpdatedBy,
            DeletedAt = IsDeleted ? DateTime.Now : null,
            DeletedBy = IsDeleted ? UpdatedBy : null,
            Roles = [.. Roles?.Select(r => r.ToEntity()) ?? []],
        };
    }
}
