using NetApi.Application.Common.Abstractions;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace NetApi.Application.Users.Commands;

public class UpdateUserCommand : AuthorizedCommand<User>
{
    [Required]
    public UserId UserId { get; set; } = UserId.Empty;

    [Required]
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}
