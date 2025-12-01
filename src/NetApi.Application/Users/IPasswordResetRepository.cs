using NetApi.Domain.Users;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Users;

public interface IPasswordResetRepository
{
    Task<PasswordResetId> CreateAsync(PasswordResetEntity passwordReset, CancellationToken cancellationToken = default);
    Task<PasswordResetEntity?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<User> MarkAsUsedAsync(PasswordResetId id, CancellationToken cancellationToken = default);
}
