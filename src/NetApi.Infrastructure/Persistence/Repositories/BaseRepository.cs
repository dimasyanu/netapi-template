using NetApi.Domain.Common.Abstractions;
using NetApi.Domain.Common.Models;
using NetApi.Domain.Util;

namespace NetApi.Domain.Abstractions;

public abstract class BaseRepository<T>
{
    protected readonly AppDbContext DbContext;
    public BaseRepository(AppDbContext dbContext)
    {
        DbContext = dbContext;
    }

    protected abstract IQueryable<T> Entities { get; }

    protected abstract IQueryable<T> GetFilteredEntities(IQueryable<T> entities, Filter filter);
    protected abstract IOrderedQueryable<T> GetOrderedEntities(IQueryable<T> entities, Order? order);
    protected Paginated<T> GetPaginatedResult(IOrderedQueryable<T> orderedEntities, Filter filter)
    {
        var total = orderedEntities.Count();
        var items = orderedEntities.Skip((int)filter.StartIndex).Take(filter.PageSize).ToList();
        return new Paginated<T> {
            Items = items,
            Total = total,
            StartIndex = filter.StartIndex,
            PageSize = filter.PageSize
        };
    }

    /// <summary>
    /// Get paginated result of an Entity type
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public Paginated<T> GetPaginated(Filter filter, Order? order = null)
    {
        var filteredEntities = GetFilteredEntities(Entities, filter);
        var orderedEntities = GetOrderedEntities(filteredEntities, order);
        var paginatedResult = GetPaginatedResult(orderedEntities, filter);
        return paginatedResult;
    }
}
