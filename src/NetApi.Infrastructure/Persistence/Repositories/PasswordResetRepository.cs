using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Users;
using NetApi.Domain.Abstractions;
using NetApi.Domain.Users;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.Models;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Infrastructure.Persistence.Repositories;

public class PasswordResetRepository(ILogger<PasswordResetRepository> logger, AppDbContext dbContext) : BaseRepository<PasswordResetEntity, PasswordResetId, PasswordResetFilter>(logger, dbContext), IPasswordResetRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    protected override IQueryable<PasswordResetEntity> Entities => _dbContext.PasswordResets.AsQueryable();

    public async Task<PasswordResetEntity?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await Entities.FirstOrDefaultAsync(pr => pr.Token == token, cancellationToken);
    }

    public async Task<User> MarkAsUsedAsync(PasswordResetId id, CancellationToken cancellationToken = default)
    {
        var resetEntry = await _dbContext.PasswordResets.FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken)
            ?? throw new NotFoundException("Password reset entry not found.");

        if (resetEntry.IsUsed)
            throw new BadRequestException("Password reset token has already been used.");

        resetEntry.MarkAsUsed();
        await _dbContext.SaveChangesAsync(cancellationToken);

        var entity = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == resetEntry.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return User.FromEntity(entity);
    }

    public override string[] SortableFields() => ["CreatedAt", "ExpiresAt"];

    protected override IOrderedQueryable<PasswordResetEntity> DefaultSort(IQueryable<PasswordResetEntity> passwordResets)
    {
        return passwordResets.OrderByDescending(pr => pr.CreatedAt);
    }

    protected override IQueryable<PasswordResetEntity> FilterEntities(IQueryable<PasswordResetEntity> passwordResets, PasswordResetFilter filter)
    {
        // Filter by UserId
        if (filter != null && filter.UserId != null) {
            passwordResets = passwordResets.Where(pr => pr.UserId == filter.UserId);
        }

        // Filter by used status
        if (filter != null && filter.IsUsed != null && filter.IsUsed.Value) {
            passwordResets = passwordResets.Where(pr => pr.UsedAt != null);
        }
        if (filter != null && filter.IsUsed != null && !filter.IsUsed.Value) {
            passwordResets = passwordResets.Where(pr => pr.UsedAt == null);
        }

        return passwordResets;
    }
}
