using NetApi.Domain.Common.Abstractions;

namespace NetApi.Domain.Media.Models;

public class MediaFilter : Filter
{
    public string? Name { get; set; }
    public string? Path { get; set; }
}
