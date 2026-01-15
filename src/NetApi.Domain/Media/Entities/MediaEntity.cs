using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Media.ValueObjects;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Domain.Media.Entities;

public class MediaEntity : IEntity<MediaId>, ITimestamp
{
    public MediaId? Id { get; set; }
    public UserId? UserId { get; set; }
    public string Name { get; set; } = "";
    public string? Format { get; set; }
    public MediaType MediaType { get; set; } = MediaType.Empty;
    public double SizeInKb { get; set; }
    public string Path { get; set; } = "";

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";

    public virtual UserEntity? User { get; set; }
}
