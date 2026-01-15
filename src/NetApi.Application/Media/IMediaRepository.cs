using NetApi.Domain.Media.Entities;
using NetApi.Domain.Media.ValueObjects;

namespace NetApi.Application.Media;

public interface IMediaRepository
{
    Task<MediaId> CreateAsync(MediaEntity entity, CancellationToken cancellationToken = default);
    Task<MediaEntity?> UpdateAsync(MediaEntity entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteManyAsync(MediaEntity[] entities, CancellationToken cancellationToken = default);
}
