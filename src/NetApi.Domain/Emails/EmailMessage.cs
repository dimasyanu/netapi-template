using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Domain.Emails;

public class EmailMessage
{
    public EmailMessage(EmailAddress[] to, string subject, string body, EmailAddress[]? cc = null, EmailAddress[]? bcc = null)
    {
        To = to;
        Subject = subject;
        Body = body;
        Cc = cc;
        Bcc = bcc;
    }

    public EmailAddress[] To { get; }
    public string Subject { get; }
    public string Body { get; }
    public EmailAddress[]? Cc { get; }
    public EmailAddress[]? Bcc { get; }
}
