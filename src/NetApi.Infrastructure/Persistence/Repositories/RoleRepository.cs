using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetApi.Application.Roles;
using NetApi.Domain.Abstractions;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Roles.Models;
using NetApi.Domain.Roles.ValueObjects;

namespace NetApi.Infrastructure.Persistence.Repositories;

public class RoleRepository(ILogger<RoleRepository> logger, AppDbContext dbContext) : BaseRepository<RoleEntity, RoleId, RoleFilter>(logger, dbContext), IRoleRepository
{
    public override string[] SortableFields() => ["Name", "CreatedAt", "UpdatedAt"];

    protected override IQueryable<RoleEntity> Entities => DbContext.Roles.AsQueryable();

    public RoleEntity? GetByName(string name)
        => Entities.FirstOrDefault(r => r.Name == name);

    public async Task<RoleEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => await Entities.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);

    public List<RoleEntity> GetList(RoleFilter? filter = null)
    {
        var query = FilterEntities(Entities, filter ?? new());
        query = DefaultSort(query);
        return [.. query];
    }

    public Task<List<RoleEntity>> GetListAsync(RoleFilter? filter = null, CancellationToken cancellationToken = default)
    {
        var query = FilterEntities(Entities, filter ?? new());
        query = DefaultSort(query);
        return query.ToListAsync(cancellationToken);
    }

    public bool SoftDelete(RoleEntity entity)
    {
        entity.DeletedAt = DateTime.Now;
        DbContext.Roles.Update(entity);
        return DbContext.SaveChanges() > 0;
    }

    public async Task<bool> SoftDeleteAsync(RoleEntity entity, CancellationToken cancellationToken = default)
    {
        entity.DeletedAt = DateTime.Now;
        DbContext.Roles.Update(entity);
        return await DbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    public bool SoftDeleteMany(RoleId[] ids)
    {
        var roles = Entities.Where(r => ids.Contains(r.Id)).ToList();
        roles.ForEach(role => role.DeletedAt = DateTime.Now);
        DbContext.Roles.UpdateRange(roles);
        return DbContext.SaveChanges() > 0;
    }

    public async Task<bool> SoftDeleteManyAsync(RoleId[] ids, CancellationToken cancellationToken = default)
    {
        var roles = await Entities.Where(r => ids.Contains(r.Id)).ToListAsync(cancellationToken);
        roles.ForEach(role => role.DeletedAt = DateTime.Now);
        DbContext.Roles.UpdateRange(roles);
        return await DbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    protected override IQueryable<RoleEntity> FilterEntities(IQueryable<RoleEntity> entities, RoleFilter filter)
    {
        if (filter is not null && filter.SearchTerm is not null) {
            var searchTerm = filter.SearchTerm.ToLower();
            entities = entities.Where(r => r.Name.ToLower().Contains(searchTerm));
        }

        return entities;
    }

    protected override IOrderedQueryable<RoleEntity> DefaultSort(IQueryable<RoleEntity> entities)
    {
        return entities.OrderBy(r => r.Name);
    }
}