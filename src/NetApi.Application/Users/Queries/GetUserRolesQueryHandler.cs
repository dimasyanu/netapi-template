using NetApi.Application.Common.Contracts;
using NetApi.Domain.Roles;

namespace NetApi.Application.Users.Queries;

public class GetUserRolesQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUserRolesQuery, List<Role>>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<List<Role>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _userRepository.GetUserRolesAsync(request.UserId, cancellationToken);
        return [.. roles.Select(r => Role.FromEntity(r))];
    }
}
