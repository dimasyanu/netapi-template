using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Domain.Users;

namespace NetApi.Application.Users.Commands;

public class UpdateUserCommandHandler(IUserRepository repo) : ICommandHandler<UpdateUserCommand, User>
{
    public async Task<User> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.User == null) throw new UnauthorizedException();
        if (request.UserId == null || request.UserId.IsEmpty())
            throw new NotFoundException($"User not found.");

        var errors = new List<KeyValuePair<string, string[]>>();
        if (string.IsNullOrEmpty(request.FirstName)) {
            errors.Add(new KeyValuePair<string, string[]>("FirstName", new[] { "First name is required." }));
        }

        var user = await repo.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User with ID '{request.UserId}' not found.");

        // Update fields
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.UpdatedAt = DateTime.Now;
        user.UpdatedBy = request.User.Username;

        user = await repo.UpdateAsync(user, cancellationToken)
            ?? throw new InternalErrorException($"Failed to update user with ID '${request.UserId}'");
        return User.FromEntity(user);
    }
}
