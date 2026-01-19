using NetApi.Application.Common;
using NetApi.Application.Common.Abstractions;
using NetApi.Application.Common.Attributes;
using NetApi.Application.Common.Models;
using NetApi.Domain.Common.Constants;
using NetApi.Domain.Roles;
using NetApi.Domain.Roles.Models;

namespace NetApi.Application.Roles.Queries;

[Authorize(Feature.Role, Permission.Read)]
public class GetRolesQuery : AuthorizedQuery<List<Role>>
{
    public RoleFilter? Filter { get; set; }
    public SortingOption? SortingOption { get; set; }
}

