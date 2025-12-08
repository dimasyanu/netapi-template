using NetApi.Application.Common.Contracts;
using NetApi.Domain.Roles;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Users.Queries;

public class GetUserRolesQuery : IQuery<List<Role>>
{
    public required UserId UserId { get; set; }
}