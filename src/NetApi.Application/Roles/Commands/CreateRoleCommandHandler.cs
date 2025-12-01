using NetApi.Application.Common.Contracts;
using NetApi.Domain.Roles;

namespace NetApi.Application.Roles.Commands;

public class CreateRoleCommandHandler(IRoleRepository repo) : ICommandHandler<CreateRoleCommand, Role>
{
    private readonly IRoleRepository _repo = repo;

    public async Task<Role> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = new Role {
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.Now,
            CreatedBy = "system",
            UpdatedAt = DateTime.Now,
            UpdatedBy = "system",
        };
        await _repo.CreateAsync(role.ToEntity());
        return role;
    }
}
