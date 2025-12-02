using System.Text.RegularExpressions;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Common.Extensions;
using NetApi.Domain.Roles;

namespace NetApi.Application.Roles.Commands;

public class CreateRoleCommandHandler(IRoleRepository repo) : ICommandHandler<CreateRoleCommand, Role>
{
    private readonly IRoleRepository _repo = repo;

    public async Task<Role> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var roleExists = await _repo.ExistsByNameAsync(request.Name);
        if (roleExists) throw new BadRequestException($"Role with name '{request.Name}' already exists.");

        // Check if name contains special characters
        if (!Regex.IsMatch(request.Name, @"^[a-zA-Z0-9_]+$"))
            throw new BadRequestException("Special characters are not allowed in role names.");

        var role = new Role {
            Name = request.Name.ToSnakeCase(),
            Description = request.Description,
            CreatedAt = DateTime.Now,
            CreatedBy = "system",
            UpdatedAt = DateTime.Now,
            UpdatedBy = "system",
        };
        var roleId = await _repo.CreateAsync(role.ToEntity());

        role.Id = roleId;
        return role;
    }
}
