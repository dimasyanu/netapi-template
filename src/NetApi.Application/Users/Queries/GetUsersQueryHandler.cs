using NetApi.Application.Common.Contracts;
using NetApi.Domain.Common.Models;
using NetApi.Domain.Users;

namespace NetApi.Application.Users.Queries;

public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, Paginated<User>>
{
    public async Task<Paginated<User>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return new();
    }
}
