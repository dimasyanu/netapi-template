using System.ComponentModel.DataAnnotations;
using NetApi.Application.Common.Abstractions;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Users.Commands;

public class ResetPasswordCommand : AuthorizedCommand<EmailAddress>
{
    [Required]
    public EmailAddress Email { get; set; } = EmailAddress.Empty;
}
