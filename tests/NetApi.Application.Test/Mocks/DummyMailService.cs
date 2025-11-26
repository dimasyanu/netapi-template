using NetApi.Application.Common.Contracts;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Test.Mocks;

public class DummyMailService : IMailService
{
    private readonly List<EmailMessage> _inbox = [];
    public async Task<bool> SendAsync(EmailAddress[] to, string subject, string body, EmailAddress[]? cc = null, EmailAddress[]? bcc = null)
    {
        _inbox.Add(new EmailMessage(to, subject, body, cc, bcc));
        return true;
    }
}

public record EmailMessage(EmailAddress[] To, string Subject, string Body, EmailAddress[]? Cc = null, EmailAddress[]? Bcc = null);
