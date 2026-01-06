using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetApi.Application.Roles;
using NetApi.Domain.Abstractions;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Roles.Models;
using NetApi.Domain.Roles.ValueObjects;
using System.Collections.Immutable;

namespace NetApi.Infrastructure.Persistence.Repositories;

public class RolePermissionRepository(ILogger<RolePermissionRepository> logger, AppDbContext dbContext)
    : BaseRepository<RolePermissionEntity, RolePermissionId, RolePermissionFilter>(logger, dbContext), IRolePermissionRepository
{
    protected override IQueryable<RolePermissionEntity> Entities => DbContext.RolePermissions.AsQueryable();

    public override string[] SortableFields()
        => ["feature", "action"];

    protected override IOrderedQueryable<RolePermissionEntity> DefaultSort(IQueryable<RolePermissionEntity> entities)
        => entities.OrderBy(x => x.Feature).ThenBy(x => x.Action);

    public async Task<bool> CheckAccessAsync(string feature, byte action, IEnumerable<RoleId> roleIds, CancellationToken cancellationToken = default)
        => await Entities.AnyAsync(x =>
            roleIds.Contains(x.RoleId)
            && x.Feature == feature
            && x.Action == action
            && x.IsAllowed
        , cancellationToken);

    public async Task<List<RolePermissionEntity>> GetListAsync(RoleId roleId, CancellationToken cancellationToken = default)
        => await Entities.Where(x => x.RoleId == roleId).ToListAsync(cancellationToken);


    protected override IQueryable<RolePermissionEntity> FilterEntities(IQueryable<RolePermissionEntity> entities, RolePermissionFilter filter)
    {
        if (filter.Ids != null && filter.Ids.Any()) {
            entities = entities.Where(x => filter.Ids.Contains(x.Id));
        }

        if (filter is not null && filter.SearchTerm is not null) {
            var searchTerm = filter.SearchTerm.ToLower();
            entities = entities.Where(r => r.Feature.ToLower().Contains(searchTerm));
        }

        return entities;
    }
}

