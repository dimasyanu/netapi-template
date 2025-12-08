using System.Linq.Expressions;
using NetApi.Application.Common.Models;
using NetApi.Domain.Common.Models;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.Models;
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
    Paginated<UserEntity> GetPaginatedList(UserFilter filter, SortingOption sortingOption);

    /// <summary>
    /// Gets users based on the provided filter and sorting options.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="sortingOption"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Paginated<UserEntity>> GetPaginatedListAsync(UserFilter filter, SortingOption sortingOption, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user in the repository.
    /// </summary>
    /// <param name="user"></param>
    /// <returns>The ID of the created user.</returns>
    UserId Create(UserEntity user);

    /// <summary>
    /// Creates a new user in the repository.
    /// </summary>
    /// <param name="user"></param>
    /// <returns>The ID of the created user.</returns>
    Task<UserId> CreateAsync(UserEntity user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    UserEntity? GetById(UserId id, List<Expression<Func<UserEntity, object>>> includes);

    /// <summary>
    /// Gets a user by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<UserEntity?> GetByIdAsync(UserId id, List<Expression<Func<UserEntity, object>>>? includes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by its username.
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    UserEntity? GetByUsername(string username);

    /// <summary>
    /// Gets a user by its username.
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by its email.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    UserEntity? GetByEmail(string email);

    /// <summary>
    /// Gets a user by its email.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<RoleEntity[]> GetUserRolesAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user in the repository.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    UserEntity? Update(UserEntity user);

    /// <summary>
    /// Updates an existing user in the repository.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<UserEntity?> UpdateAsync(UserEntity user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes many users.
    /// </summary>
    /// <param name="userIds"></param>
    /// <returns></returns>
    bool DeleteMany(UserEntity[] userIds);

    /// <summary>
    /// Deletes many users.
    /// </summary>
    /// <param name="userIds"></param>
    /// <returns></returns>
    Task<bool> DeleteManyAsync(UserEntity[] userIds, CancellationToken cancellationToken = default);
}
