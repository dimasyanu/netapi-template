using Microsoft.EntityFrameworkCore;
using NetApi.Application.Users;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Infrastructure.Persistence.Repositories;

public class UserSettingRepository : IUserSettingRepository
{
    private readonly AppDbContext _dbContext;

    public UserSettingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<UserSettingEntity>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserSettings
            .Where(us => us.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SaveUserSettingsAsync(UserId userId, List<UserSettingEntity> settings, CancellationToken cancellationToken = default)
    {
        var existingSettings = await _dbContext.UserSettings
            .Where(us => us.UserId == userId)
            .ToListAsync(cancellationToken);

        _dbContext.UserSettings.RemoveRange(existingSettings);
        await _dbContext.UserSettings.AddRangeAsync(settings, cancellationToken);

        var result = await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}