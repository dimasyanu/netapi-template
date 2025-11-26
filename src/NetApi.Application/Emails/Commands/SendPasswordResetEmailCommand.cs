using NetApi.Application.Common.Contracts;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Emails.Commands;

public class SendPasswordResetEmailCommand : ICommand
{
    public EmailAddress Email { get; set; } = EmailAddress.Empty;
    public User? User { get; set; }
}
