using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetApi.Application.Users;
using NetApi.Domain.Abstractions;
using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Common.Models;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Infrastructure.Persistence;

namespace NetApi.Domain.Repositories;

public class UserRepository(ILogger<UserRepository> logger, AppDbContext dbContext) : BaseRepository<User, UserId, UserFilter>(logger, dbContext), IUserRepository
{
    protected override IQueryable<User> Entities => DbContext.Users.AsQueryable();

    public override string[] SortableFields() => [
        nameof(User.Id),
        nameof(User.FirstName),
        nameof(User.LastName),
        nameof(User.Username),
        nameof(User.Email),
        nameof(User.CreatedAt),
        nameof(User.UpdatedAt),
        nameof(User.DeletedAt),
    ];

    protected override IOrderedQueryable<User> DefaultSort()
        => Entities.OrderByDescending(u => u.UpdatedAt);

    public User? GetByUsername(string username)
    {
        return DbContext.Users.FirstOrDefault(u => u.Username == username);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return await DbContext.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    protected override IQueryable<User> FilterEntities(IQueryable<User> entities, UserFilter filter)
    {
        var query = entities;
        if (!string.IsNullOrEmpty(filter.SearchTerm)) {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query
                .Where(u =>
                    u.FirstName.ToLower().Contains(searchTerm)
                    || u.LastName.ToLower().Contains(searchTerm)
                    || u.Username.ToLower().Contains(searchTerm)
                    || u.Email.ToLower().Contains(searchTerm)
                );
        }

        if (!string.IsNullOrEmpty(filter.Email)) {
            var email = filter.Email.ToLower();
            query = query.Where(u => u.Email.ToLower().Contains(email));
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
