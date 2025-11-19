using NetApi.Domain.Roles.ValueObjects;
using NetApi.Domain.Users;

namespace NetApi.Domain.Roles;

public class Role
{
    public RoleId Id { get; set; } = new(0);
    public string Name { get; set; } = "";

    public List<User>? Users { get; set; }
}
