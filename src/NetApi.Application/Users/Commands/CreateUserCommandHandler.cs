using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;

namespace NetApi.Application.Users.Commands;

public class CreateUserCommandHandler(IUserRepository repo, IHashingService hashingService) : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _repo = repo;
    private readonly IHashingService _hashingService = hashingService;

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.Password != request.ConfirmPassword) {
            throw new BadRequestException("Password and Confirm Password do not match.");
        }

        var newUser = new Domain.Users.User {
            Username = request.Username,
            Email = Domain.Users.ValueObjects.EmailAddress.Create(request.Email),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = _hashingService.HashPassword(request.Password),
        };

        var result = await _repo.CreateAsync(newUser, cancellationToken);
        return result.ToGuid();
    }
}
