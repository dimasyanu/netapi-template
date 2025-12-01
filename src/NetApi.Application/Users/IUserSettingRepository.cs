using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Users;

public interface IUserSettingRepository
{
    Task<List<UserSettingEntity>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserSettingsAsync(UserId userId, List<UserSettingEntity> settings, CancellationToken cancellationToken = default);
}
