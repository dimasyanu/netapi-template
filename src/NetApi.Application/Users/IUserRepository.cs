using NetApi.Domain.Users;

namespace NetApi.Application.Users;

public interface IUserRepository
{
    /// <summary>
    /// Creates a new user in the repository.
    /// </summary>
    /// <param name="user"></param>
    /// <returns>The ID of the created user.</returns>
    long Create(User user);

    /// <summary>
    /// Creates a new user in the repository.
    /// </summary>
    /// <param name="user"></param>
    /// <returns>The ID of the created user.</returns>
    Task<long> CreateAsync(User user);

    /// <summary>
    /// Gets a user by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    User? GetById(long id);

    /// <summary>
    /// Gets a user by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<User?> GetByIdAsync(long id);

    /// <summary>
    /// Gets a user by its username.
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    User? GetByUsername(string username);

    /// <summary>
    /// Gets a user by its username.
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Updates an existing user in the repository.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    User? Update(User user);

    /// <summary>
    /// Updates an existing user in the repository.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<User?> UpdateAsync(User user);

    /// <summary>
    /// Deletes a user from the repository.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    bool Delete(User user);

    /// <summary>
    /// Deletes a user from the repository.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(User user);

    /// <summary>
    /// Deletes many users.
    /// </summary>
    /// <param name="userIds"></param>
    /// <returns></returns>
    bool DeleteMany(Span<long> userIds);

    /// <summary>
    /// Deletes many users.
    /// </summary>
    /// <param name="userIds"></param>
    /// <returns></returns>
    Task<bool> DeleteManyAsync(Span<long> userIds);
}
