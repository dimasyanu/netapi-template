using NetApi.Application.Common.Abstractions;
using NetApi.Domain.Roles.ValueObjects;

namespace NetApi.Application.Roles.Commands;

public class SoftDeleteRoleCommand : AuthorizedCommand<bool>
{
    public required RoleId[] Ids { get; set; }
}
