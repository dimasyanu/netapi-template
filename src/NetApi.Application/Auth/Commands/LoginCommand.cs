using NetApi.Application.Common.Contracts;
using NetApi.Domain.Auth.Models;

namespace NetApi.Application.Auth.Commands;

public class LoginCommand : ICommand<LoginResult>
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}
