using NetApi.Domain.Roles;

namespace NetApi.Application.Roles;

public interface IRoleRepository
{
    /// <summary>
    /// Creates a new role in the repository.
    /// </summary>
    /// <param name="role"></param>
    /// <returns>The ID of the created role.</returns>
    short Create(Role role);

    /// <summary>
    /// Creates a new role in the repository asynchronously.
    /// </summary>
    /// <param name="role"></param>
    /// <returns>The ID of the created role.</returns>
    Task<short> CreateAsync(Role role);

    /// <summary>
    /// Gets a role by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Role? GetById(short id);

    /// <summary>
    /// Gets a role by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Role?> GetByIdAsync(short id);

    /// <summary>
    /// Gets a role by its name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Role? GetByName(string name);

    /// <summary>
    /// Gets a role by its name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<Role?> GetByNameAsync(string name);

    /// <summary>
    /// Updates an existing role in the repository.
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    Role? Update(Role role);

    /// <summary>
    /// Updates an existing role in the repository.
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    Task<Role?> UpdateAsync(Role role);

    /// <summary>
    /// Deletes a role by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    bool Delete(short id);

    /// <summary>
    /// Deletes a role by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(short id);

    /// <summary>
    /// Deletes multiple roles by their IDs.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    bool DeleteMany(IEnumerable<short> ids);

    /// <summary>
    /// Deletes multiple roles by their IDs.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<bool> DeleteManyAsync(IEnumerable<short> ids);
}
