using NetApi.Domain.Emails;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Common.Contracts;

public interface IMailService
{
    Task<bool> SendAsync(EmailAddress[] to, string subject, string body, EmailAddress[]? cc = null, EmailAddress[]? bcc = null);
    Task<IReadOnlyList<EmailMessage>> GetInboxAsync(EmailAddress address);
}
