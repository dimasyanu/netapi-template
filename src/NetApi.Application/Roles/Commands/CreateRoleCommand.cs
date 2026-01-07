using NetApi.Application.Common.Abstractions;
using NetApi.Application.Common.Attributes;
using NetApi.Domain.Common.Constants;
using NetApi.Domain.Roles;

namespace NetApi.Application.Roles.Commands;

[Permission(RoleConstant.FeatureName, RoleConstant.Permission.Create)]
public class CreateRoleCommand : AuthorizedCommand<Role>
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

