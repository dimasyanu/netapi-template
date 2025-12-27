using NetApi.Application.Common.Contracts;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Users.Queries;

public class GetUserByEmailAddressQueryHandler(IUserRepository repo) : IQueryHandler<GetUserByEmailAddressQuery, User>
{
    private readonly IUserRepository _repo = repo;

    public async Task<User> Handle(GetUserByEmailAddressQuery request, CancellationToken cancellationToken)
    {
        var userEntity = await _repo.GetByEmailAsync(EmailAddress.FromString(request.EmailAddress), [u => u.Roles], cancellationToken)
            ?? throw new KeyNotFoundException($"User with email address {request.EmailAddress} not found.");
        return User.FromEntity(userEntity);
    }
}
