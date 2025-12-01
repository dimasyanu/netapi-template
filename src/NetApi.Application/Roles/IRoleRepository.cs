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
    List<RoleEntity> GetList();

    /// <summary>
    /// Gets the list of all roles asynchronously.
    /// </summary>
    /// <returns></returns>
    Task<List<RoleEntity>> GetListAsync(RoleFilter? filter = null, CancellationToken cancellationToken = default);

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

    /// <summary>
    /// Soft deletes a role by its ID.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    bool SoftDelete(RoleEntity entity);

    /// <summary>
    /// Soft deletes a role by its ID.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<bool> SoftDeleteAsync(RoleEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes multiple roles by their IDs.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    bool SoftDeleteMany(RoleId[] ids);

    /// <summary>
    /// Soft deletes multiple roles by their IDs.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<bool> SoftDeleteManyAsync(RoleId[] ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role by its ID.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    bool Delete(RoleEntity entity);

    /// <summary>
    /// Deletes a role by its ID.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(RoleEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple roles by their IDs.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    bool DeleteMany(RoleId[] ids);

    /// <summary>
    /// Deletes multiple roles by their IDs.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<bool> DeleteManyAsync(RoleId[] ids, CancellationToken cancellationToken = default);
}
