using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Roles.ValueObjects;

namespace NetApi.Application.Roles;

public interface IRolePermissionRepository
{
    Task<List<RolePermissionEntity>> GetListAsync(RoleId roleId, CancellationToken cancellationToken = default);
    Task<bool> CheckAccessAsync(string feature, byte action, IEnumerable<RoleId> roleIds, CancellationToken cancellationToken = default);
}

