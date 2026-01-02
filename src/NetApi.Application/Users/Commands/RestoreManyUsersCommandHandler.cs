using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Users.Commands;

public class RestoreManyUsersCommandHandler(IUserRepository repo) : ICommandHandler<RestoreManyUsersCommand, bool>
{
    private readonly IUserRepository _repo = repo;

    public async Task<bool> Handle(RestoreManyUsersCommand request, CancellationToken cancellationToken)
    {
        if (request.User == null || request.User.Id == null) throw new UnauthorizedException();

        var ids = request.Ids.Select(x => UserId.FromGuid(x)).ToList();
        var users = await _repo.GetByIdsAsync(ids, cancellationToken);
        foreach (var user in users) {
            user.DeletedAt = null;
            user.DeletedBy = null;
        }
        await _repo.UpdateManyAsync([.. users], cancellationToken);

        return true;
    }
}
