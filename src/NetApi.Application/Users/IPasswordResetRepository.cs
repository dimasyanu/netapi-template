using NetApi.Domain.Users;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Users;

public interface IPasswordResetRepository
{
    Task<PasswordResetId> CreateAsync(PasswordReset passwordReset, CancellationToken cancellationToken = default);
    Task<PasswordReset?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<User> MarkAsUsedAsync(PasswordResetId id, CancellationToken cancellationToken = default);
}
