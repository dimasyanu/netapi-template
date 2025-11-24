using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetApi.Application.Common.Models;
using NetApi.Domain.Common.Abstractions;
using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Common.Models;
using NetApi.Infrastructure.Persistence;

namespace NetApi.Domain.Abstractions;

public abstract class BaseRepository<TEntity, TKey, TFilter>(ILogger logger, AppDbContext dbContext)
    where TEntity : class, IEntity<TKey>
    where TKey : notnull
    where TFilter : Filter
{
    protected readonly AppDbContext DbContext = dbContext;
    protected readonly ILogger _logger = logger;

    public abstract string[] SortableFields();
    protected abstract IOrderedQueryable<TEntity> DefaultSort();
    protected abstract IQueryable<TEntity> Entities { get; }
    protected abstract IQueryable<TEntity> FilterEntities(IQueryable<TEntity> entities, TFilter filter);
    protected Paginated<TEntity> GetPaginatedResult(IOrderedQueryable<TEntity> orderedEntities, TFilter filter)
    {
        var total = orderedEntities.Count();
        var items = BaseRepository<TEntity, TKey, TFilter>.Paginate(orderedEntities, filter.StartIndex, filter.PageSize);
        return new Paginated<TEntity> {
            Items = items,
            Total = total,
            StartIndex = filter.StartIndex ?? 0,
            PageSize = filter.PageSize ?? total
        };
    }

    /// <summary>
    /// Get paginated result of an Entity type
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public Paginated<TEntity> GetPaginatedList(TFilter filter, SortingOption? order = null)
    {
        var filteredEntities = FilterEntities(Entities, filter);
        var orderedEntities = GetOrderedEntities(filteredEntities, order);
        var paginatedResult = GetPaginatedResult(orderedEntities, filter);
        return paginatedResult;
    }

    /// <summary>
    /// Get paginated result of an Entity type asynchronously
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="order"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Paginated<TEntity>> GetPaginatedListAsync(TFilter filter, SortingOption? order = null, CancellationToken cancellationToken = default)
    {
        var filteredEntities = FilterEntities(Entities, filter);
        var orderedEntities = GetOrderedEntities(filteredEntities, order);
        var total = await orderedEntities.CountAsync(cancellationToken);
        var items = await BaseRepository<TEntity, TKey, TFilter>.PaginateAsync(orderedEntities, filter.StartIndex, filter.PageSize, cancellationToken);
        return new Paginated<TEntity> {
            Items = items,
            Total = total,
            StartIndex = filter.StartIndex ?? 0,
            PageSize = filter.PageSize ?? total
        };
    }

    public TKey Create(TEntity entity)
    {
        DbContext.Set<TEntity>().Add(entity);
        DbContext.SaveChanges();
        return entity.Id;
    }

    public async Task<TKey> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbContext.Set<TEntity>().Add(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public TEntity? GetById(TKey id)
    {
        return DbContext.Set<TEntity>().Find(id);
    }

    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    public virtual TEntity? Update(TEntity entity)
    {
        DbContext.Set<TEntity>().Update(entity);
        DbContext.SaveChanges();
        return entity;
    }

    public virtual async Task<TEntity?> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbContext.Set<TEntity>().Update(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual bool Delete(TEntity entity)
    {
        try {
            DbContext.Set<TEntity>().Remove(entity);
            DbContext.SaveChanges();
        } catch (Exception e) {
            // Log the exception or handle it as needed
            _logger.LogError(e, "Error deleting entity with ID {EntityId}", entity.Id);
            return false;
        }
        return true;
    }

    public virtual async Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try {
            DbContext.Set<TEntity>().Remove(entity);
            await DbContext.SaveChangesAsync(cancellationToken);
        } catch (Exception e) {
            // Log the exception or handle it as needed
            _logger.LogError(e, "Error deleting entity with ID {EntityId}", entity.Id);
            return false;
        }
        return true;
    }

    public virtual bool DeleteMany(TKey[] ids)
    {
        try {
            var entitiesToDelete = DbContext.Set<TEntity>().Where(e => ids.Contains(e.Id)).ToList();
            DbContext.Set<TEntity>().RemoveRange(entitiesToDelete);
            DbContext.SaveChanges();
        } catch (Exception e) {
            // Log the exception or handle it as needed
            _logger.LogError(e, "Error deleting multiple entities");
            return false;
        }
        return true;
    }

    public virtual async Task<bool> DeleteManyAsync(TKey[] ids, CancellationToken cancellationToken = default)
    {
        try {
            var entitiesToDelete = await DbContext.Set<TEntity>().Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
            DbContext.Set<TEntity>().RemoveRange(entitiesToDelete);
            await DbContext.SaveChangesAsync(cancellationToken);
        } catch (Exception e) {
            // Log the exception or handle it as needed
            _logger.LogError(e, "Error deleting multiple entities asynchronously with IDs {EntityIds}", ids);
            return false;
        }
        return true;
    }

    private static IReadOnlyList<TEntity> Paginate(IQueryable<TEntity> query, long? startIndex, long? pageSize)
    {
        if (startIndex != null) {
            query = query.Skip((int)startIndex);
        }
        if (pageSize != null) {
            query = query.Take((int)pageSize);
        }
        return [.. query];
    }

    private static async Task<IReadOnlyList<TEntity>> PaginateAsync(IQueryable<TEntity> query, long? startIndex, long? pageSize, CancellationToken cancellationToken = default)
    {
        if (startIndex != null) {
            query = query.Skip((int)startIndex);
        }
        if (pageSize != null) {
            query = query.Take((int)pageSize);
        }

        return await query.ToListAsync(cancellationToken);
    }

    protected virtual IOrderedQueryable<TEntity> GetOrderedEntities(IQueryable<TEntity> entities, SortingOption? order)
    {
        if (order is null) return DefaultSort();
        if (order.SortBy != null) {
            if (!SortableFields().Any(prop => prop.Equals(order.SortBy, StringComparison.OrdinalIgnoreCase))) {
                throw new ArgumentException($"Invalid SortBy value: {order.SortBy}");
            }

            if (order.SortDirection != null && order.SortDirection == SortingOption.DIRECTION_DESCENDING) {
                return entities.OrderByDescending(u => EF.Property<object>(u, order.SortBy));
            }

            return entities.OrderBy(u => EF.Property<object>(u, order.SortBy));
        }
        return DefaultSort();
    }
}
