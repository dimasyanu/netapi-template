using NetApi.Application.Common.Abstractions;
using NetApi.Domain.Users.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace NetApi.Application.Users.Commands;

public class CreateUserCommand : AuthorizedCommand<UserId>
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
