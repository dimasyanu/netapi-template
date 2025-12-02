using NetApi.Domain.Common.Abstractions;
using NetApi.Domain.Roles.ValueObjects;

namespace NetApi.Domain.Roles.Models;

public class RoleFilter : Filter
{
    public RoleId[]? Ids { get; set; }
}
