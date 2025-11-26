using NetApi.Domain.Users;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Users;

public interface IPasswordResetRepository
{
    Task<PasswordResetId> CreateAsync(UserId userId, CancellationToken cancellationToken);
    Task<PasswordReset> GetByTokenAsync(string token, CancellationToken cancellationToken);
    Task<User> MarkAsUsedAsync(PasswordResetId id, CancellationToken cancellationToken);
}
