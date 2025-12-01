using NetApi.Application.Common.Contracts;
using NetApi.Domain.Roles;

namespace NetApi.Application.Roles.Commands;

public class CreateRoleCommand : ICommand<Role>
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}
