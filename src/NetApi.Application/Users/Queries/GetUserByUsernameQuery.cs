using NetApi.Application.Common.Contracts;
using NetApi.Domain.Users;

namespace NetApi.Application.Users.Queries;

public class GetUserByUsernameQuery : IQuery<User>
{
    public required string Username { get; init; }
}
