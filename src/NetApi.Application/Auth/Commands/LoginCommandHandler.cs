using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Users;
using NetApi.Domain.Auth.Models;
using NetApi.Domain.Users;

namespace NetApi.Application.Auth.Commands;

public class LoginCommandHandler(IUserRepository userRepo, IHashingService hashingService) : ICommandHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepo = userRepo;
    private readonly IHashingService _hashingService = hashingService;

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userEntity = await _userRepo.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new BadRequestException("Invalid email or password.");

        if (!_hashingService.VerifyPassword(request.Password, userEntity.PasswordHash))
            throw new BadRequestException("Invalid email or password.");

        var user = User.FromEntity(userEntity);
        var token = user.GenerateAuthToken();

        return LoginResult.Success(token);
    }
}
