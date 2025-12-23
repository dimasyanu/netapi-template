using NetApi.Application.Common.Contracts;
using NetApi.Domain.Auth.Models;

namespace NetApi.Application.Auth.Commands;

public class LoginCommandHandler(IAuthService authService) : ICommandHandler<LoginCommand, LoginResult>
{
    private readonly IAuthService _authService = authService;

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _authService.AttemptLoginAsync(request.Email, request.Password, cancellationToken);
        var result = await _authService.GenerateAuthTokenAsync(user, cancellationToken);

        return result;
    }
}
