using NetApi.Application.Common.Contracts;

namespace NetApi.Infrastructure.Persistence.Constants.EmailTemplates;

public class WelcomeEmailTemplate : IEmailTemplate
{
    private readonly Dictionary<string, string> _properties = new();
    public string Subject => "Welcome to NetApi!";

    public string Render()
    {
        var html = @"
            <html>
                <body>
                    <h1>Welcome to NetApi, [Name]!</h1>
                    <p>Thank you for joining our platform. We're excited to have you on board.</p>
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
