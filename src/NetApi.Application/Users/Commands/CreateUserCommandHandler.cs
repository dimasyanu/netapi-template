using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Roles;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Users.Commands;

public class CreateUserCommandHandler(IUserRepository repo, IRoleRepository roleRepo, IHashingService hashingService) : ICommandHandler<CreateUserCommand, UserId>
{
    private readonly IUserRepository _repo = repo;
    private readonly IRoleRepository _roleRepo = roleRepo;
    private readonly IHashingService _hashingService = hashingService;

    public async Task<UserId> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.User == null) throw new UnauthorizedException();

        var errors = new List<KeyValuePair<string, string[]>>();
        if (request.Password != request.ConfirmPassword) {
            errors.Add(KeyValuePair.Create("password", new[] { "Password and Confirm Password do not match." }));
        }
        if (await _repo.GetByUsernameAsync(request.Username, cancellationToken) != null) {
            errors.Add(KeyValuePair.Create("username", new[] { $"Username '{request.Username}' is already taken." }));
        }
        if (await _repo.GetByEmailAsync(EmailAddress.FromString(request.Email), [], cancellationToken) != null) {
            errors.Add(KeyValuePair.Create("email", new[] { $"Email '{request.Email}' is already registered." }));
        }
        var roles = await _roleRepo.GetListAsync(filter: new() {
            Ids = [.. request.Roles],
        }, cancellationToken: cancellationToken);
        if (roles.Count < 1) {
            errors.Add(KeyValuePair.Create("roles", new[] { "At least one valid role must be assigned to the user." }));
        }
        if (errors.Count > 0) throw new BadRequestException(errors);

        var newUser = new User {
            // Id = UserId.New(),
            Username = request.Username,
            EmailAddress = EmailAddress.FromString(request.Email),
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.Now,
            CreatedBy = request.User.Username,
            UpdatedAt = DateTime.Now,
            UpdatedBy = request.User.Username,
        }.ToEntity();
        newUser.Roles = roles;
        newUser.PasswordHash = _hashingService.HashPassword(request.Password);
        var result = await _repo.CreateAsync(newUser, cancellationToken);

        // var newUserRoles = roles.Select(r => new UserRoleEntity {
        //     UserId = result,
        //     RoleId = r.Id!,
        //     AssignedAt = DateTime.Now,
        // }).ToList();
        // await _roleRepo.AssignRolesToUserAsync(result, newUserRoles, cancellationToken);

        return result;
    }
}
