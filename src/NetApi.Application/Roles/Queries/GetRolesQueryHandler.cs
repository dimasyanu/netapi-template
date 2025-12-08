using NetApi.Application.Common.Contracts;
using NetApi.Domain.Roles;

namespace NetApi.Application.Roles.Queries;

public class GetRolesQueryHandler(IRoleRepository repo) : IQueryHandler<GetRolesQuery, List<Role>>
{
    private readonly IRoleRepository _repo = repo;

    public async Task<List<Role>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var entities = await _repo.GetListAsync(request.Filter, request.SortingOption, cancellationToken);
        return [.. entities.Select(x => Role.FromEntity(x, false))];
    }
}
