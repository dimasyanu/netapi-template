using NetApi.Domain.Common.Abstractions;

namespace NetApi.Domain.Users.Models;

public class UserFilter : Filter
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool? IsDeleted { get; set; }
    public string[]? Roles { get; set; }
}
