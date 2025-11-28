using NetApi.Application.Common.Contracts;
using NetApi.Application.Users;
using NetApi.Domain.Users.Entities;

namespace NetApi.Application.Emails.Commands;

public class SendPasswordResetEmailCommandHandler(IEmailTemplateManager templateManager, IMailService mailService, IPasswordResetRepository repo) : ICommandHandler<SendPasswordResetEmailCommand>
{
    const string TemplateName = "PasswordResetEmail";

    private readonly IPasswordResetRepository _repo = repo;

    public async Task Handle(SendPasswordResetEmailCommand request, CancellationToken cancellationToken)
    {
        if (request.User == null)
            throw new ArgumentNullException(nameof(request.User), "User information is required to send password reset email.");

        var template = templateManager.GetTemplate(TemplateName);
        template.SetProperty("User", request.User.FirstName);
        template.SetProperty("Email", request.Email.ToString());
        template.SetProperty("ResetLink", $"https://example.com/reset-password?email={request.Email}&token=some-generated-token");

        var resetEntry = new PasswordReset {
            UserId = request.User.Id,
            CreatedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddHours(1),
            Token = "some-generated-token"
        };

        // Save the password reset entry to the repository
        await _repo.CreateAsync(resetEntry, cancellationToken);

        await mailService.SendAsync(
            to: [request.Email],
            subject: template.Subject,
            body: template.Render()
        );
    }
}
