using NetApi.Domain.Abstractions;
using NetApi.Domain.Entities;
using NetApi.Domain.Util;

namespace NetApi.Domain.Repositories;

public class UserRepository : BaseRepository<User>
{
    public UserRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    protected override IQueryable<User> Entities => DbContext.Users.AsQueryable();
}
