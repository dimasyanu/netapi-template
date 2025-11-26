using System.Net.Mail;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Common.Contracts;

public interface IMailService
{
    Task<bool> SendAsync(EmailAddress[] to, string subject, string body, EmailAddress[]? cc = null, EmailAddress[]? bcc = null);
}
