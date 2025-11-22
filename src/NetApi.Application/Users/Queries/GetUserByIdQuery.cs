using NetApi.Application.Common.Contracts;
using NetApi.Domain.Users;

namespace NetApi.Application.Users.Queries;

public class GetUserByIdQuery : IQuery<User>
{
    public required Guid UserId { get; init; }
}
