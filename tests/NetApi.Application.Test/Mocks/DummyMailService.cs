using NetApi.Application.Common.Contracts;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Test.Mocks;

public class DummyMailService : IMailService
{
    private readonly List<EmailMessage> _inbox = [];

    public async Task<bool> SendAsync(EmailAddress[] to, string subject, string body, EmailAddress[]? cc = null, EmailAddress[]? bcc = null)
    {
        foreach (var mailAddress in to) {
            _inbox.Add(new EmailMessage(mailAddress, subject, body, cc, bcc));
        }
        return true;
    }

    public async Task<IReadOnlyList<EmailMessage>> GetInboxAsync(EmailAddress address)
    {
        return _inbox
            .Where(email => email.To == address)
            .ToList()
            .AsReadOnly();
    }
}

public record EmailMessage(EmailAddress To, string Subject, string Body, EmailAddress[]? Cc = null, EmailAddress[]? Bcc = null);
