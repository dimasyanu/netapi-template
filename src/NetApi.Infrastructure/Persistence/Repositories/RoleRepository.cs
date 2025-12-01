using Microsoft.Extensions.Logging;
using NetApi.Application.Roles;
using NetApi.Domain.Abstractions;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Roles.Models;
using NetApi.Domain.Roles.ValueObjects;

namespace NetApi.Infrastructure.Persistence.Repositories;

public class RoleRepository(Logger<RoleRepository> logger, AppDbContext dbContext) : BaseRepository<RoleEntity, RoleId, RoleFilter>(logger, dbContext), IRoleRepository
{
    protected override IQueryable<RoleEntity> Entities => throw new NotImplementedException();

    public RoleEntity? GetByName(string name)
    {
        throw new NotImplementedException();
    }

    public Task<RoleEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public List<RoleEntity> GetList()
    {
        throw new NotImplementedException();
    }

    public Task<List<RoleEntity>> GetListAsync(RoleFilter? filter = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public bool SoftDelete(RoleEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteAsync(RoleEntity entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public bool SoftDeleteMany(RoleId[] ids)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteManyAsync(RoleId[] ids, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    protected override IQueryable<RoleEntity> FilterEntities(IQueryable<RoleEntity> entities, RoleFilter filter)
    {
        throw new NotImplementedException();
    }

    public override string[] SortableFields()
    {
        throw new NotImplementedException();
    }

    protected override IOrderedQueryable<RoleEntity> DefaultSort(IQueryable<RoleEntity> entities)
    {
        throw new NotImplementedException();
    }
}