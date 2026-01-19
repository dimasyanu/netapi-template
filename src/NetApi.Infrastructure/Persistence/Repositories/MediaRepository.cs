using System.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Media;
using NetApi.Domain.Abstractions;
using NetApi.Domain.Media;
using NetApi.Domain.Media.Entities;
using NetApi.Domain.Media.Models;
using NetApi.Domain.Media.ValueObjects;
using NetApi.Domain.Users.ValueObjects;

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

    public async Task<IReadOnlyList<MediaEntity>> GetListAsync(MediaFilter filter, CancellationToken cancellationToken = default)
    {
        var query = FilterEntities(Entities, filter);
        var items = await query.ToListAsync(cancellationToken);
        return items;
    }

    public async Task<bool> CheckOwnershipAsync(IEnumerable<object> ids, UserId userId)
    {
        if (ids.Any(x => x is not MediaId && x is not Guid))
            throw new BadRequestException("Invalid id(s)");

        var mediaIds = ids.Select(x => x is Guid guid ? MediaId.FromGuid(guid) : (MediaId)x);

        return await Entities.Where(x => mediaIds.Contains(x.Id)).AllAsync(x => x.UserId == userId);
    }
}
