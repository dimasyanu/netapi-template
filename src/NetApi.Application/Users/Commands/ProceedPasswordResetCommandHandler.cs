using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;

namespace NetApi.Application.Users.Commands;

public class ProceedPasswordResetCommandHandler(IPasswordResetRepository repo, IUserRepository userRepo, IHashingService hashingService) : ICommandHandler<ProceedPasswordResetCommand, bool>
{
    private readonly IPasswordResetRepository _repo = repo;
    private readonly IUserRepository _userRepo = userRepo;
    private readonly IHashingService _hashingService = hashingService;

    public async Task<bool> Handle(ProceedPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<KeyValuePair<string, string[]>>();
        if (request.NewPassword.Length < 6) {
            errors.Add(new KeyValuePair<string, string[]>("NewPassword", ["Password must be at least 6 characters long."]));
        }
        if (request.NewPassword.All(char.IsLetter)) {
            errors.Add(new KeyValuePair<string, string[]>("NewPassword", ["Password must contain at least one non-letter character."]));
        }
        if (request.NewPassword.All(char.IsLower)) {
            errors.Add(new KeyValuePair<string, string[]>("NewPassword", ["Password must contain at least one uppercase letter."]));
        }

        if (request.NewPassword != request.ConfirmPassword) {
            errors.Add(new KeyValuePair<string, string[]>("ConfirmPassword", ["New password and confirmation do not match."]));
        }
        if (errors.Count > 0) {
            throw new BadRequestException(errors);
        }

        var passwordReset = await _repo.GetByTokenAsync(request.Token, cancellationToken)
            ?? throw new UnauthorizedException("Invalid or expired password reset token.");
        if (passwordReset.ExpiresAt < DateTime.Now || passwordReset.IsUsed)
            throw new UnauthorizedException("Invalid or expired password reset token.");

        var user = await _userRepo.GetByIdAsync(passwordReset.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        // Hash new password
        user.PasswordHash = _hashingService.HashPassword(request.NewPassword);

        // Update user password
        await _userRepo.UpdateAsync(user, cancellationToken);

        // Mark password reset as used
        await _repo.MarkAsUsedAsync(passwordReset.Id, cancellationToken);

        return true;
    }
}
