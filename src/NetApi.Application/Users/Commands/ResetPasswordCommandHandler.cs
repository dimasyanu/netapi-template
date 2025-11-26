using MediatR;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Emails.Commands;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Users.Commands;

public class ResetPasswordCommandHandler(IJobService jobService) : ICommandHandler<ResetPasswordCommand, EmailAddress>
{
    public async Task<EmailAddress> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (request.User == null || request.User.Id == null) {
            throw new UnauthorizedAccessException("User must be authenticated to reset password.");
        }
        if (request.User.Email != request.Email) {
            throw new UnauthorizedAccessException("Users can only reset their own password.");
        }
        if (request.Email == EmailAddress.Empty) {
            throw new BadRequestException([KeyValuePair.Create("Email", new[] { "Email is required." })]);
        }

        jobService.EnqueueAsync(
            new SendPasswordResetEmailCommand {
                Email = request.Email,
                User = request.User
            }
        );

        return request.Email;
    }
}
