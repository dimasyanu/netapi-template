using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetApi.Application.Common.Models;
using NetApi.Application.Roles;
using NetApi.Domain.Abstractions;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Roles.Models;
using NetApi.Domain.Roles.ValueObjects;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Infrastructure.Persistence.Repositories;

public class RoleRepository(ILogger<RoleRepository> logger, AppDbContext dbContext) : BaseRepository<RoleEntity, RoleId, RoleFilter>(logger, dbContext), IRoleRepository
{
    public override string[] SortableFields() => ["Name", "CreatedAt", "UpdatedAt"];

    protected override IQueryable<RoleEntity> Entities => DbContext.Roles.AsQueryable();

    public RoleEntity? GetByName(string name)
        => Entities.FirstOrDefault(r => r.Name == name);

    public async Task<RoleEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => await Entities.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await Entities.CountAsync(r => r.Name == name, cancellationToken) > 0;
    }

    public List<RoleEntity> GetList(RoleFilter? filter = null, SortingOption? sortingOption = null)
    {
        var query = FilterEntities(Entities, filter ?? new());
        query = GetOrderedEntities(query, sortingOption);
        return [.. query];
    }

    public Task<List<RoleEntity>> GetListAsync(RoleFilter? filter = null, SortingOption? sortingOption = null, CancellationToken cancellationToken = default)
    {
        var query = FilterEntities(Entities, filter ?? new());
        query = GetOrderedEntities(query, sortingOption);
        return query.ToListAsync(cancellationToken);
    }

    protected override IQueryable<RoleEntity> FilterEntities(IQueryable<RoleEntity> entities, RoleFilter filter)
    {
        if (filter is not null && filter.SearchTerm is not null) {
            var searchTerm = filter.SearchTerm.ToLower();
            entities = entities.Where(r => r.Name.ToLower().Contains(searchTerm));
        }

        if (filter is not null && filter.Ids is not null && filter.Ids.Length > 0) {
            entities = entities.Where(r => filter.Ids!.Contains(r.Id!));
        }

        return entities;
    }

    protected override IOrderedQueryable<RoleEntity> DefaultSort(IQueryable<RoleEntity> entities)
    {
        return entities.OrderBy(r => r.Name);
    }

    // Disable bulk delete operations for RoleEntity
    public override bool DeleteMany(RoleEntity[] entities)
        => throw new NotImplementedException();
    public override Task<bool> DeleteManyAsync(RoleEntity[] entities, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public async Task<bool> AssignRolesToUserAsync(UserId userId, List<UserRoleEntity> userRoles, CancellationToken cancellationToken = default)
    {
        // await DbContext.UserRoles.AddRangeAsync(userRoles, cancellationToken);
        foreach (var userRole in userRoles) {
            await DbContext.UserRoles.AddAsync(userRole, cancellationToken);
        }
        await DbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
