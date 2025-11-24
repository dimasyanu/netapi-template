using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using NetApi.Application.Common.Contracts;

namespace NetApi.Application.Users.Commands;

public class CreateUserCommand : ICommand<Guid>
{
    [Required]
    public string Username { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    [Required]
    public string ConfirmPassword { get; set; } = "";
}
