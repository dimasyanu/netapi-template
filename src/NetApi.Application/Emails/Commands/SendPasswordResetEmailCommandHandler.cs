using NetApi.Application.Common.Contracts;

namespace NetApi.Application.Emails.Commands;

public class SendPasswordResetEmailCommandHandler(IEmailTemplateManager templateManager, IMailService mailService) : ICommandHandler<SendPasswordResetEmailCommand>
{
    public async Task Handle(SendPasswordResetEmailCommand request, CancellationToken cancellationToken)
    {
        if (request.User == null)
            throw new ArgumentNullException(nameof(request.User), "User information is required to send password reset email.");

        var template = templateManager.GetTemplate("PasswordResetEmail");
        template.SetProperty("User", request.User.FirstName);
        template.SetProperty("Email", request.Email.ToString());

        await mailService.SendAsync(
            to: [request.Email],
            subject: template.Subject,
            body: template.Render()
        );
    }
}
