using NetApi.Application.Common.Contracts;
using NetApi.Domain.Roles;
using NetApi.Domain.Roles.Models;

namespace NetApi.Application.Roles.Queries;

public class GetRolesQuery : IQuery<List<Role>>
{
    public RoleFilter? Filter { get; set; }
}
