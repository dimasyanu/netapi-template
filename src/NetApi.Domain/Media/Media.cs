using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Media.Entities;
using NetApi.Domain.Media.ValueObjects;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Domain.Media;

public class Media : IHasEntity<MediaEntity>
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

    public User? User { get; set; }

    public MediaEntity ToEntity()
        => new() {
            Id = Id,
            UserId = UserId,
            Name = Name,
            Format = Format,
            MediaType = MediaType,
            SizeInKb = SizeInKb,
            Path = Path,
            CreatedAt = CreatedAt,
            CreatedBy = CreatedBy,
            UpdatedAt = UpdatedAt,
            UpdatedBy = UpdatedBy,
            User = User!.ToEntity(),
        };

    public static Media FromEntity(MediaEntity entity)
        => new() {
            Id = entity.Id,
            UserId = entity.UserId,
            Name = entity.Name,
            Format = entity.Format,
            MediaType = entity.MediaType,
            SizeInKb = entity.SizeInKb,
            Path = entity.Path,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
            User = entity.User != null ? User.FromEntity(entity.User) : null,
        };
}
