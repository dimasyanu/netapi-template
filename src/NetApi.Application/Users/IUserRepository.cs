using NetApi.Application.Common.Models;
using NetApi.Domain.Common.Models;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Users;

public interface IUserRepository
{
    /// <summary>
    /// Gets the list of fields that can be used for sorting.
    /// </summary>
    string[] SortableFields();

    /// <summary>
    /// Gets users based on the provided filter and sorting options.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="sortingOption"></param>
    /// <returns></returns>
    Paginated<User> GetPaginatedList(UserFilter filter, SortingOption sortingOption);

    /// <summary>
    /// Gets users based on the provided filter and sorting options.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="sortingOption"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Paginated<User>> GetPaginatedListAsync(UserFilter filter, SortingOption sortingOption, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user in the repository.
    /// </summary>
    /// <param name="user"></param>
    /// <returns>The ID of the created user.</returns>
    UserId Create(User user);

    /// <summary>
    /// Creates a new user in the repository.
    /// </summary>
    /// <param name="user"></param>
    /// <returns>The ID of the created user.</returns>
    Task<UserId> CreateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    User? GetById(UserId id);

    /// <summary>
    /// Gets a user by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);

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
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by its email.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    User? GetByEmail(string email);

    /// <summary>
    /// Gets a user by its email.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

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
    Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default);

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
    Task<bool> DeleteAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes many users.
    /// </summary>
    /// <param name="userIds"></param>
    /// <returns></returns>
    bool DeleteMany(UserId[] userIds);

    /// <summary>
    /// Deletes many users.
    /// </summary>
    /// <param name="userIds"></param>
    /// <returns></returns>
    Task<bool> DeleteManyAsync(UserId[] userIds, CancellationToken cancellationToken = default);
}
