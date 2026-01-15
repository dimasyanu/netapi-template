using NetApi.Application.Common.Abstractions;
using MediaDto = NetApi.Domain.Media.Media;

namespace NetApi.Application.Media.Commands;

public class AddMediaCommand : AuthorizedCommand<MediaDto>
{
    public Guid? ParentId { get; set; }
    public byte[] FileBytes { get; set; } = [];
    public string FileName { get; set; } = "";
    public string Path { get; set; } = "/";
}
