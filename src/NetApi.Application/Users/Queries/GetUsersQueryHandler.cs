using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Models;
using NetApi.Domain.Common.Models;
using NetApi.Domain.Users;
using NetApi.Domain.Users.Models;

namespace NetApi.Application.Users.Queries;

public class GetUsersQueryHandler(IUserRepository repo) : IQueryHandler<GetUsersQuery, Paginated<User>>
{
    private readonly IUserRepository _repo = repo;

    public async Task<Paginated<User>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var filter = MapUsersQueryToFilter(request);
        var sortingOption = MapUsersQueryToSortingOption(request);

        // Retrieve users from repository
        var entities = await _repo.GetPaginatedListAsync(filter, sortingOption, cancellationToken);

        var results = entities.Items.Select(x => User.FromEntity(x)).ToList();

        return new Paginated<User> {
            Items = results,
            PageSize = entities.PageSize,
            StartIndex = entities.StartIndex,
            Total = entities.Total
        };
    }

    /// <summary>
    /// Maps GetUsersQuery to UserFilter
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private static UserFilter MapUsersQueryToFilter(GetUsersQuery request)
    {
        return new UserFilter {
            Username = request.Username,
            Email = request.Email,
            IsDeleted = request.IsDeleted,
            StartIndex = request.StartIndex,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm
        };
    }

    /// <summary>
    /// Maps GetUsersQuery to SortingOption
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private static SortingOption MapUsersQueryToSortingOption(GetUsersQuery request)
    {
        return new SortingOption {
            SortBy = request.SortBy,
            SortDirection = request.SortDirection
        };
    }
}
