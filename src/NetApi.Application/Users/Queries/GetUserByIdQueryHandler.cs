using NetApi.Application.Common.Contracts;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Users.Queries;

public class GetUserByIdQueryHandler(IUserRepository repo) : IQueryHandler<GetUserByIdQuery, User>
{
    private readonly IUserRepository _repo = repo;

    public async Task<User> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var userEntity = await _repo.GetByIdAsync(UserId.FromGuid(request.UserId), [u => u.Roles], cancellationToken)
            ?? throw new KeyNotFoundException($"User with ID {request.UserId} not found.");
        return User.FromEntity(userEntity);
    }
}
