using NetApi.Domain.Common.Abstractions;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Domain.Media.Models;

public class MediaFilter : Filter
{
    public required UserId UserId { get; set; }
    public string? Name { get; set; }
    public string Path { get; set; } = "/";
}
