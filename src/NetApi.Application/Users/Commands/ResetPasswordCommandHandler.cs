using MediatR;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Common.Models;
using NetApi.Application.Emails.Commands;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Users.Commands;

public class ResetPasswordCommandHandler(IJobService jobService, IPasswordResetRepository repo) : ICommandHandler<ResetPasswordCommand, EmailAddress>
{
    private readonly IPasswordResetRepository _repo = repo;

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

        var resetEntry = new PasswordReset {
            Id = PasswordResetId.Create(),
            UserId = request.User.Id,
            CreatedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddHours(1),
            Token = "some-generated-token"
        };

        // Save the password reset entry to the repository
        await _repo.CreateAsync(resetEntry, cancellationToken);

        await jobService.EnqueueAsync(
            new Job<SendPasswordResetEmailCommand> {
                Command = new() {
                    Email = request.Email,
                    User = request.User
                },
                Key = request.Email.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmssss"),
            },
            cancellationToken
        );

        return request.Email;
    }
}
