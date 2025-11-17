using System.ComponentModel.DataAnnotations;
using NetApi.Domain.Abstractions;
using NetApi.Domain.Contracts;

namespace NetApi.Domain.Entities;

public class User : Int64BaseEntity,
    IBaseEntity<long>,
    ITimestamp,
    ISoftDelete
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
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
