using NetApi.Domain.Common.Abstractions;
using NetApi.Domain.Roles.ValueObjects;

namespace NetApi.Domain.Roles.Models;

public class RolePermissionFilter : Filter
{
    public IEnumerable<RolePermissionId>? Ids { get; set; }
}
