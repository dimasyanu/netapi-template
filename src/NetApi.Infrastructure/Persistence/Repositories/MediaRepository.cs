using Microsoft.Extensions.Logging;
using NetApi.Application.Media;
using NetApi.Domain.Abstractions;
using NetApi.Domain.Media.Entities;
using NetApi.Domain.Media.Models;
using NetApi.Domain.Media.ValueObjects;

namespace NetApi.Infrastructure.Persistence.Repositories;

public class MediaRepository(ILogger<MediaRepository> logger, AppDbContext dbContext) : BaseRepository<MediaEntity, MediaId, MediaFilter>(logger, dbContext), IMediaRepository
{
    protected override IQueryable<MediaEntity> Entities => DbContext.Media;

    public override string[] SortableFields()
        => [
            "Name",
            "SizeInKb",
            "Format",
            "CreatedAt",
            "UpdatedAt",
        ];

    protected override IOrderedQueryable<MediaEntity> DefaultSort(IQueryable<MediaEntity> entities)
        => Entities.OrderBy(x => x.Name);

    protected override IQueryable<MediaEntity> FilterEntities(IQueryable<MediaEntity> entities, MediaFilter filter)
    {
        if (filter.Name != null && !string.IsNullOrEmpty(filter.Name)) {
            var fName = filter.Name.ToLower().Trim();
            entities = entities.Where(x => x.Name.ToLower().Contains(fName));
        }

        if (filter.Path != null && !string.IsNullOrEmpty(filter.Path)) {
            entities = entities.Where(x => x.Path == filter.Path);
        }

        return entities;
    }
}
