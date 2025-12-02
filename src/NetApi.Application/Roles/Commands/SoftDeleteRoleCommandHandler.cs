using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;

namespace NetApi.Application.Roles.Commands;

public class SoftDeleteRoleCommandHandler(IRoleRepository repo) : ICommandHandler<SoftDeleteRoleCommand, bool>
{
    private readonly IRoleRepository _repo = repo;

    public async Task<bool> Handle(SoftDeleteRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.User == null)
            throw new UnauthorizedException("User information is required to perform this action.");

        var roles = await _repo.GetListAsync(filter: new() { Ids = request.Ids }, cancellationToken: cancellationToken);
        if (roles == null || roles.Count == 0) return false;

        roles.ForEach(role => {
            role.DeletedAt = DateTime.Now;
            role.DeletedBy = request.User.Username;
        });

        await _repo.UpdateManyAsync([.. roles], cancellationToken);

        return true;
    }
}
