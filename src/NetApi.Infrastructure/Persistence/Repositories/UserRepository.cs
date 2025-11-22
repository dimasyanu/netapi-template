using NetApi.Domain.Common.Abstractions;
using NetApi.Domain.Common.Models;
using NetApi.Domain.Users;
using NetApi.Domain.Util;

namespace NetApi.Domain.Repositories;

public class UserRepository(AppDbContext dbContext)
{
    protected IQueryable<User> GetFilteredEntities(IQueryable<User> entities, Filter filter)
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

        return query;
    }

    protected IOrderedQueryable<User> GetOrderedEntities(IQueryable<User> entities, Order? order)
    {
        if (order is null) return entities.OrderBy(u => u.Id);
        if (order.PropertyName != null) {
            if (order.IsDescending ?? true) {

            }
        }
    }
}
