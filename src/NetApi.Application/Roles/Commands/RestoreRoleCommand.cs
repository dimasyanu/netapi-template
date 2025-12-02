using NetApi.Application.Common.Abstractions;
using NetApi.Domain.Roles.ValueObjects;

namespace NetApi.Application.Roles.Commands;

public class RestoreRoleCommand : AuthorizedCommand<bool>
{
    public required RoleId[] RoleIds { get; set; }
}
