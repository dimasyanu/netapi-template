using NetApi.Domain.Common.Abstractions;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Domain.Users.Models;

public class PasswordResetFilter : Filter
{
    public UserId? UserId { get; set; }
    public bool? IsUsed { get; set; }
}
