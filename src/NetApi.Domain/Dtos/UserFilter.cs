using NetApi.Domain.Abstractions;

namespace NetApi.Domain.Dtos;

public class UserFilter : Filter
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool? IsDeleted { get; set; }
}
