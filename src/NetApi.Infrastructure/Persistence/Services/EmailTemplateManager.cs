using NetApi.Application.Common.Contracts;
using NetApi.Infrastructure.Persistence.Constants.EmailTemplates;

namespace NetApi.Infrastructure.Persistence.Services;

public class EmailTemplateManager : IEmailTemplateManager
{
    private readonly Dictionary<string, IEmailTemplate> _templates = new() {
        { "WelcomeEmail", new WelcomeEmailTemplate() },
        { "PasswordReset", new PasswordResetEmailTemplate() }
    };

    public IEmailTemplate GetTemplate(string templateName)
    {
        return _templates.TryGetValue(templateName, out var template)
            ? template
            : throw new ArgumentException($"Email template '{templateName}' not found.");
    }
}
