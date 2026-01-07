using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Users;
using NetApi.Domain.Abstractions;
using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.Models;
using NetApi.Domain.Users.ValueObjects;
using System.Linq.Expressions;

namespace NetApi.Infrastructure.Persistence.Repositories;

public class UserRepository(ILogger<UserRepository> logger, AppDbContext dbContext) : BaseRepository<UserEntity, UserId, UserFilter>(logger, dbContext), IUserRepository
{
    protected override IQueryable<UserEntity> Entities => DbContext.Users.AsQueryable();

    public override string[] SortableFields() => [
        nameof(UserEntity.Id),
        nameof(UserEntity.FirstName),
        nameof(UserEntity.LastName),
        nameof(UserEntity.Username),
        nameof(UserEntity.EmailAddress),
        nameof(UserEntity.CreatedAt),
        nameof(UserEntity.UpdatedAt),
        nameof(UserEntity.DeletedAt),
    ];

    protected override IOrderedQueryable<UserEntity> DefaultSort(IQueryable<UserEntity> users)
        => users.OrderByDescending(u => u.UpdatedAt);

    public UserEntity? GetByUsername(string username)
        => DbContext.Users.FirstOrDefault(u => u.Username == username);

    public async Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
        => await DbContext.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public UserEntity? GetByEmail(EmailAddress emailAddress, List<Expression<Func<UserEntity, object>>>? includes = null)
        => GetEagerLoadedQuery(includes).FirstOrDefault(u => u.EmailAddress == emailAddress);

    public async Task<UserEntity?> GetByEmailAsync(EmailAddress emailAddress, List<Expression<Func<UserEntity, object>>>? includes = null, CancellationToken cancellationToken = default)
        => await GetEagerLoadedQuery(includes).FirstOrDefaultAsync(u => u.EmailAddress == emailAddress, cancellationToken);

    protected override IQueryable<UserEntity> GetEagerLoadedQuery(List<Expression<Func<UserEntity, object>>>? includes = null)
    {
        var query = DbContext.Users.AsQueryable();
        if (includes != null && includes.Count > 0) {
            foreach (var include in includes) {
                var includeQuery = query.Include(include);
                if (include.Body.Type == typeof(List<RoleEntity>))
                    includeQuery = includeQuery.ThenInclude(x => ((RoleEntity)x).Permissions!);
                query = includeQuery;
            }
        }
        return query;
    }

    public override UserId Create(UserEntity entity)
    {
        DbContext.Users.Add(entity);
        DbContext.SaveChanges();
        return entity.Id!;
    }

    public override async Task<UserId> CreateAsync(UserEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity.Roles.Count > 0) {
            foreach (var role in entity.Roles) {
                if (role.Id == null) continue;
                DbContext.Attach(role);
            }
        }
        await DbContext.Users.AddAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
        return entity.Id!;
    }

    public async Task<IEnumerable<UserEntity>> GetByIdsAsync(IReadOnlyList<UserId> ids, CancellationToken cancellationToken = default)
    {
        return await Entities.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserEntity>> UpdateManyAsync(IReadOnlyList<UserEntity> users, CancellationToken cancellationToken = default)
    {
        DbContext.AttachRange(users);
        DbContext.UpdateRange(users);
        await DbContext.SaveChangesAsync(cancellationToken);
        return users;
    }

    public async Task<RoleEntity[]> GetUserRolesAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var user = await Entities.Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException($"User with ID {userId} not found.");
        return [.. user.Roles ?? []];
    }

    public bool TrashMany(IReadOnlyList<UserId> ids, string currentUser)
    {
        DbContext.Users
            .Where(x => ids.Contains(x.Id))
            .ExecuteUpdate(builder => builder
                .SetProperty(x => x.DeletedAt, DateTime.Now)
                .SetProperty(x => x.DeletedBy, currentUser)
            );
        return true;
    }

    public async Task<bool> TrashManyAsync(IReadOnlyList<UserId> ids, string currentUser, CancellationToken cancellationToken = default)
    {
        await DbContext.Users
            .Where(x => ids.Contains(x.Id))
            .ExecuteUpdateAsync(builder => builder
                .SetProperty(x => x.DeletedAt, DateTime.Now)
                .SetProperty(x => x.DeletedBy, currentUser)
            , cancellationToken);
        return true;
    }

    public bool RestoreMany(IReadOnlyList<UserId> ids, string currentUser)
    {
        DbContext.Users
            .Where(x => ids.Contains(x.Id))
            .ExecuteUpdate(builder => builder
                .SetProperty(x => x.UpdatedAt, DateTime.Now)
                .SetProperty(x => x.UpdatedBy, currentUser)
                .SetProperty(x => x.DeletedAt, _ => null)
                .SetProperty(x => x.DeletedBy, _ => null)
            );
        return true;
    }

    public async Task<bool> RestoreManyAsync(IReadOnlyList<UserId> ids, string currentUser, CancellationToken cancellationToken = default)
    {
        await DbContext.Users
            .Where(x => ids.Contains(x.Id))
            .ExecuteUpdateAsync(builder => builder
                .SetProperty(x => x.UpdatedAt, DateTime.Now)
                .SetProperty(x => x.UpdatedBy, currentUser)
                .SetProperty(x => x.DeletedAt, _ => null)
                .SetProperty(x => x.DeletedBy, _ => null)
            , cancellationToken);
        return true;
    }

    protected override IQueryable<UserEntity> FilterEntities(IQueryable<UserEntity> entities, UserFilter filter)
    {
        var query = entities;
        if (!string.IsNullOrEmpty(filter.SearchTerm)) {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query
                .Where(u =>
                    u.FirstName.ToLower().Contains(searchTerm)
                    || u.LastName.ToLower().Contains(searchTerm)
                    || u.Username.ToLower().Contains(searchTerm)
                    || u.EmailAddress.ToLower().Contains(searchTerm)
                );
        }

        if (!string.IsNullOrEmpty(filter.Email)) {
            var email = filter.Email.ToLower();
            query = query.Where(u => u.EmailAddress.ToLower().Contains(email));
        }

        if (!string.IsNullOrEmpty(filter.Username)) {
            var username = filter.Username.ToLower();
            query = query.Where(u => u.Username.ToLower().Contains(username));
        }

        if (filter.IsDeleted != null && filter.IsDeleted.Value) {
            query = query.Where(u => u.DeletedAt != null);
        }

        if (filter.IsDeleted != null && !filter.IsDeleted.Value) {
            query = query.Where(u => u.DeletedAt == null);
        }

        if (filter.Roles != null) {
            var roleNames = filter.Roles.ToList();
            query = query.Where(u => u.Roles != null ? u.Roles.Any(role => roleNames.Contains(role.Name.ToLower())) : false);
        }

        return query;
    }
}
