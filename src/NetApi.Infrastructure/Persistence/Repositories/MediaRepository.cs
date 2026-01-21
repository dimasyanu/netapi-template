using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Media;
using NetApi.Domain.Abstractions;
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

    public async Task<IReadOnlyList<MediaEntity>> GetFileListAsync(MediaFilter filter, CancellationToken cancellationToken = default)
    {
        var query = FilterEntities(Entities, filter);
        var items = await query.ToListAsync(cancellationToken);
        return items;
    }

    public async Task<IReadOnlyList<string>> GetDirectoryListAsync(MediaFilter filter, CancellationToken cancellationToken = default)
    {
        if (filter.UserId == null || filter.UserId == UserId.Empty) throw new UnauthorizedException();

        var path = filter.Path ?? "/";
        if (!path.StartsWith('/')) path = "/" + path;
        if (!path.EndsWith('/')) path += "/";

        var pathPattern1 = @$"^{Regex.Escape(path)}[^/]+$"; // Directories one level below the current path
        var query = Entities
            .Where(x => x.UserId == filter.UserId)
            .Where(x => Regex.IsMatch(x.Path, pathPattern1));
        var test = await query.ToListAsync(cancellationToken);
        var directoryQuery = query
            .Select(x => x.Path)
            .Distinct();

        if (!string.IsNullOrEmpty(filter.Name)) {
            var fName = filter.Name.ToLower().Trim();
            directoryQuery = directoryQuery.Where(x => x.ToLower().Contains(fName));
        }

        return [.. directoryQuery];
    }

    public async Task<bool> CheckOwnershipAsync(IEnumerable<object> ids, UserId userId)
    {
        if (ids.Any(x => x is not MediaId && x is not Guid))
            throw new BadRequestException("Invalid id(s)");

        var mediaIds = ids.Select(x => x is Guid guid ? MediaId.FromGuid(guid) : (MediaId)x);

        return await Entities.Where(x => mediaIds.Contains(x.Id)).AllAsync(x => x.UserId == userId);
    }
}
