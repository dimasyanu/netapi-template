using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetApi.Application.Common.Models;
using NetApi.Domain.Common.Abstractions;
using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Common.Models;
using NetApi.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace NetApi.Domain.Abstractions;

public abstract class BaseRepository<TEntity, TKey, TFilter>(ILogger logger, AppDbContext dbContext)
    where TEntity : class, IEntity<TKey>
    where TKey : notnull
    where TFilter : Filter
{
    protected readonly AppDbContext DbContext = dbContext;
    protected readonly ILogger _logger = logger;

    public abstract string[] SortableFields();
    protected abstract IOrderedQueryable<TEntity> DefaultSort(IQueryable<TEntity> entities);
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

    protected virtual IQueryable<TEntity> GetEagerLoadedQuery(List<Expression<Func<TEntity, object>>>? includes = null)
    {
        var query = DbContext.Set<TEntity>().AsQueryable();
        if (includes != null && includes.Count > 0) {
            foreach (var include in includes) {
                query = query.Include(include);
            }
        }
        return query;
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

    public virtual TKey Create(TEntity entity)
    {
        DbContext.Set<TEntity>().Add(entity);
        DbContext.SaveChanges();
        return entity.Id!;
    }

    public virtual async Task<TKey> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbContext.Set<TEntity>().Add(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
        return entity.Id!;
    }

    public TEntity? GetById(TKey id, List<Expression<Func<TEntity, object>>>? includes)
    {
        return GetEagerLoadedQuery(includes)
            .FirstOrDefault(e => e.Id!.Equals(id));
    }

    public async Task<TEntity?> GetByIdAsync(TKey id, List<Expression<Func<TEntity, object>>>? includes, CancellationToken cancellationToken = default)
    {
        return await GetEagerLoadedQuery(includes)
            .FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);
    }

    public virtual TEntity? Update(TEntity entity)
    {
        DbContext.Set<TEntity>().Update(entity);
        DbContext.SaveChanges();
        return entity;
    }

    public virtual async Task<TEntity?> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbContext.Set<TEntity>().Attach(entity);
        DbContext.Set<TEntity>().Update(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task<TEntity?> UpdateAsync(TKey id, Action<TEntity> updateAction, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, null, cancellationToken);
        if (entity == null) {
            return null;
        }
        updateAction(entity);
        DbContext.Set<TEntity>().Update(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> UpdateManyAsync(TEntity[] entities, CancellationToken cancellationToken = default)
    {
        try {
            DbContext.Set<TEntity>().UpdateRange(entities);
            await DbContext.SaveChangesAsync(cancellationToken);
        } catch (Exception e) {
            // Log the exception or handle it as needed
            _logger.LogError(e, "Error updating multiple entities asynchronously");
            return false;
        }
        return true;
    }

    public virtual bool DeleteMany(TEntity[] entities)
    {
        try {
            DbContext.Set<TEntity>().RemoveRange(entities);
            DbContext.SaveChanges();
        } catch (Exception e) {
            // Log the exception or handle it as needed
            _logger.LogError(e, "Error deleting multiple entities");
            return false;
        }
        return true;
    }

    public virtual async Task<bool> DeleteManyAsync(TEntity[] entities, CancellationToken cancellationToken = default)
    {
        try {
            DbContext.Set<TEntity>().RemoveRange(entities);
            await DbContext.SaveChangesAsync(cancellationToken);
        } catch (Exception e) {
            // Log the exception or handle it as needed
            _logger.LogError(e, "Error deleting multiple entities asynchronously");
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
        if (order is null) return DefaultSort(entities);
        if (order.SortBy != null) {
            if (!SortableFields().Any(prop => prop.Equals(order.SortBy, StringComparison.OrdinalIgnoreCase))) {
                throw new ArgumentException($"Invalid SortBy value: {order.SortBy}");
            }

            if (order.SortDirection != null && order.SortDirection == SortingOption.DIRECTION_DESCENDING) {
                return entities.OrderByDescending(u => EF.Property<object>(u, order.SortBy));
            }

            return entities.OrderBy(u => EF.Property<object>(u, order.SortBy));
        }
        return DefaultSort(entities);
    }
}
