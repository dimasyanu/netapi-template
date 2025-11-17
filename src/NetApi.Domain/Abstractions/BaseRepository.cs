using Microsoft.EntityFrameworkCore;
using NetApi.Domain.Dtos;
using NetApi.Domain.Util;

namespace NetApi.Domain.Abstractions;

public abstract class BaseRepository<T>(AppDbContext dbContext)
{
    protected readonly AppDbContext DbContext = dbContext;

    protected abstract IQueryable<T> Entities { get; }

    /// <summary>
    /// Get paginated result of an Entity type
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public Paginated<T> GetPaginated(Filter filter)
    {
        var users = Entities.ToList();
        return new Paginated<T>
        {
            Items = users,
            Total = 0,
            StartIndex = filter.StartIndex,
            PageSize = filter.PageSize
        };
    }

    /// <summary>
    /// Get entity by Id
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<T?> GetById<TKey>(TKey id)
    {
        return await Entities.FirstOrDefaultAsync(e => e.Id.Equals(id));
    }
}
