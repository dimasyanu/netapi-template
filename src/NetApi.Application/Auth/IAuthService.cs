using NetApi.Domain.Auth.Models;
using NetApi.Domain.Users;

namespace NetApi.Application.Auth;

public interface IAuthService
{
    Task<User> AttemptLoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<LoginResult> GenerateAuthTokenAsync(User user, CancellationToken cancellationToken = default);
}
