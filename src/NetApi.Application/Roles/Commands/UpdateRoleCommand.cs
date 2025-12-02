using NetApi.Application.Common.Abstractions;
using NetApi.Domain.Roles;
using NetApi.Domain.Roles.ValueObjects;

namespace NetApi.Application.Roles.Commands;

public class UpdateRoleCommand : AuthorizedCommand<Role>
{
    public required RoleId Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
