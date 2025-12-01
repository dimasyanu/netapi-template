using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Users.Commands;

public class CreateUserCommandHandler(IUserRepository repo, IHashingService hashingService) : ICommandHandler<CreateUserCommand, UserId>
{
    private readonly IUserRepository _repo = repo;
    private readonly IHashingService _hashingService = hashingService;

    public async Task<UserId> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.User == null) throw new UnauthorizedException();

        var errors = new List<KeyValuePair<string, string[]>>();
        if (request.Password != request.ConfirmPassword) {
            errors.Add(KeyValuePair.Create("password", new[] { "Password and Confirm Password do not match." }));
        }
        if ((await _repo.GetByUsernameAsync(request.Username, cancellationToken)) != null) {
            errors.Add(KeyValuePair.Create("username", new[] { $"Username '{request.Username}' is already taken." }));
        }
        if ((await _repo.GetByEmailAsync(request.Email, cancellationToken)) != null) {
            errors.Add(KeyValuePair.Create("email", new[] { $"Email '{request.Email}' is already registered." }));
        }
        if (errors.Count > 0) throw new BadRequestException(errors);

        var newUser = new User {
            Username = request.Username,
            Email = EmailAddress.FromString(request.Email),
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.Now,
            CreatedBy = request.User.Username,
            UpdatedAt = DateTime.Now,
            UpdatedBy = request.User.Username,
        }.ToEntity();
        newUser.PasswordHash = _hashingService.HashPassword(request.Password);

        var result = await _repo.CreateAsync(newUser, cancellationToken);
        return result;
    }
}
