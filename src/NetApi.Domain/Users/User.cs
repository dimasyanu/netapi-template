using System.ComponentModel.DataAnnotations;
using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Roles;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Domain.Users;

public class User :
    ITimestamp,
    ISoftDelete
{
    public UserId Id { get; set; } = UserId.Create();
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Username { get; set; } = "";
    public EmailAddress Email { get; set; } = EmailAddress.NewEmpty();
    public string PasswordHash { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public DateTime CreatedAt { get; set; }

    [MaxLength(100)]
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedAt { get; set; }

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = "";
    public DateTime? DeletedAt { get; set; }

    [MaxLength(100)]
    public string? DeletedBy { get; set; }

    public List<Role>? Roles { get; set; }
}
