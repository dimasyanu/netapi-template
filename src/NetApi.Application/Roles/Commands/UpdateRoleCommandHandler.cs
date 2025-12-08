using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Common.Extensions;
using NetApi.Domain.Roles;

namespace NetApi.Application.Roles.Commands;

public class UpdateRoleCommandHandler(IRoleRepository repo) : ICommandHandler<UpdateRoleCommand, Role>
{
    private readonly IRoleRepository _repo = repo;

    public async Task<Role> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.User == null)
            throw new UnauthorizedException("Unauthorized access");

        var role = await _repo.GetByIdAsync(request.Id, null, cancellationToken)
            ?? throw new NotFoundException($"Role with ID {request.Id} not found.");

        role.Name = request.Name.ToSnakeCase();
        role.Description = request.Description;
        role.UpdatedAt = DateTime.Now;
        role.UpdatedBy = request.User.Username;

        role = await _repo.UpdateAsync(role, cancellationToken);
        return Role.FromEntity(role!);
    }
}
