using NetApi.Application.Common.Contracts;

namespace NetApi.Application.Test.Mocks;

public class DummyEmailTemplateManager : IEmailTemplateManager
{
    private readonly Dictionary<string, IEmailTemplate> _templates = new() {
        {
            "PasswordResetEmail",
            new DummyEmailTemplate(
                subject: "Reset your password",
                body: "Hello {{User}},\n\nTo reset your password, please click the following link: {{ResetLink}}\n\nIf you did not request a password reset, please ignore this email.\n\nBest regards,\nNetApi Team"
            )
        }
    };

    public IEmailTemplate GetTemplate(string templateName)
    {
        if (_templates.ContainsKey(templateName)) {
            return _templates[templateName];
        }
        throw new ArgumentException($"Template '{templateName}' not found.");
    }
}
