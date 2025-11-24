using NetApi.Application.Common.Contracts;
using NetApi.Domain.Users;

namespace NetApi.Application.Users.Queries;

public class GetUserByIdQuery : IQuery<User>
{
    public Guid UserId { get; init; }

    public GetUserByIdQuery()
    {
    }

    public GetUserByIdQuery(Guid userId)
    {
        UserId = userId;
    }
}
