using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Domain.Users.Entities;

public class PasswordResetEntity : IEntity<PasswordResetId>
{
    public PasswordResetId? Id { get; set; }
    public UserId UserId { get; set; } = UserId.Empty;
    public string Token { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public bool IsUsed => UsedAt != null;

    public void MarkAsUsed() => UsedAt = DateTime.Now;
}
