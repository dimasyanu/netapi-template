using System.ComponentModel.DataAnnotations;
using NetApi.Application.Common.Contracts;

namespace NetApi.Application.Users.Commands;

public class ProceedPasswordResetCommand : ICommand<bool>
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;
}
