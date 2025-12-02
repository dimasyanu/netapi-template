using NetApi.Application.Common.Models;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Roles.Models;
using NetApi.Domain.Roles.ValueObjects;

namespace NetApi.Application.Roles;

public interface IRoleRepository
{
    /// <summary>
    /// Gets the list of all roles.
    /// </summary>
    /// <returns></returns>
    List<RoleEntity> GetList(RoleFilter? filter = null, SortingOption? sortingOption = null);

    /// <summary>
    /// Gets the list of all roles asynchronously.
    /// </summary>
    /// <returns></returns>
    Task<List<RoleEntity>> GetListAsync(RoleFilter? filter = null, SortingOption? sortingOption = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new role in the repository.
    /// </summary>
    /// <param name="role"></param>
    /// <returns>The ID of the created role.</returns>
    RoleId Create(RoleEntity role);

    /// <summary>
    /// Creates a new role in the repository asynchronously.
    /// </summary>
    /// <param name="role"></param>
    /// <returns>The ID of the created role.</returns>
    Task<RoleId> CreateAsync(RoleEntity role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    RoleEntity? GetById(RoleId id);

    /// <summary>
    /// Gets a role by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<RoleEntity?> GetByIdAsync(RoleId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by its name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    RoleEntity? GetByName(string name);

    /// <summary>
    /// Gets a role by its name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<RoleEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a role exists by its name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing role in the repository.
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    RoleEntity? Update(RoleEntity role);

    /// <summary>
    /// Updates an existing role in the repository.
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    Task<RoleEntity?> UpdateAsync(RoleEntity role, CancellationToken cancellationToken = default);

    Task<bool> UpdateManyAsync(RoleEntity[] roles, CancellationToken cancellationToken = default);
}
