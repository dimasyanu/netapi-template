using NetApi.Application.Common.Contracts;

namespace NetApi.Application.Users.Commands;

public class ValidatePasswordResetAttemptCommand : ICommand<string>
{
    public string Token { get; set; } = string.Empty;
}
