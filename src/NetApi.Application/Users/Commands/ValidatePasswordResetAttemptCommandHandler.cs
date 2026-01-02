using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;

namespace NetApi.Application.Users.Commands;

public class ValidatePasswordResetAttemptCommandHandler(IPasswordResetRepository repo) : ICommandHandler<ValidatePasswordResetAttemptCommand, string>
{
    private readonly IPasswordResetRepository _repo = repo;

    public async Task<string> Handle(ValidatePasswordResetAttemptCommand request, CancellationToken cancellationToken)
    {
        var token = await _repo.GetByTokenAsync(request.Token, cancellationToken)
            ?? throw new BadRequestException("Invalid or expired token.");
        if (token.ExpiresAt < DateTime.Now || token.IsUsed)
            throw new BadRequestException("Invalid or expired token.");

        return token.Token;
    }
}
