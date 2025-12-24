using NetApi.Application.Common.Contracts;

namespace NetApi.Infrastructure.Persistence.Constants.EmailTemplates;

public class PasswordResetEmailTemplate : IEmailTemplate
{
    private readonly Dictionary<string, string> _properties = new();

    public string Subject => "Password Reset Request";

    public string Render()
    {
        var html = @"
            <html>
                <body>
                    <h1>Password Reset Request</h1>
                    <p>Hello [Name],</p>
                    <p>You have requested a password reset. Please click the link below to reset your password:</p>
                    <a href='[ResetUrl]'>Reset Password</a>
                    <p>If you did not request this, please ignore this email.</p>
                    <p>Best regards,<br/>The NetApi Team</p>
                </body>
            </html>
        ";
        foreach (var prop in _properties) {
            html = html.Replace($"[{prop.Key}]", prop.Value, StringComparison.InvariantCultureIgnoreCase);
        }
        return html;
    }

    public void SetProperty(string key, string value)
    {
        _properties[key] = value;
    }
}
