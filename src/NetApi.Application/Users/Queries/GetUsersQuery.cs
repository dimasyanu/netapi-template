using NetApi.Application.Common.Contracts;
using NetApi.Domain.Common.Models;
using NetApi.Domain.Users;

namespace NetApi.Application.Users.Queries;

public class GetUsersQuery : IQuery<Paginated<User>>
{
    public string? SearchTerm { get; init; }
    public string? Username { get; init; }
    public string? Email { get; init; }
    public bool? IsDeleted { get; init; }
    public int StartIndex { get; init; } = 0;
    public int PageSize { get; init; } = 25;
}
