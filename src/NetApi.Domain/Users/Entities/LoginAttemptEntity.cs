using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Domain.Users.Entities;

public class LoginAttemptEntity : IEntity<LoginAttemptId>
{
    public LoginAttemptId? Id { get; set; }
    public UserId UserId { get; set; } = UserId.Empty;
    public bool Success { get; set; }
    public string? Location { get; set; }
    public string? IpAddress { get; set; }
    public DateTime AttemptDateTime { get; set; }

    public virtual UserEntity? User { get; set; }
}