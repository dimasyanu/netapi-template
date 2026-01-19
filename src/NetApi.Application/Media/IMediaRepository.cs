using System.Linq.Expressions;
using NetApi.Application.Common.Contracts;
using NetApi.Domain.Media.Entities;
using NetApi.Domain.Media.Models;
using NetApi.Domain.Media.ValueObjects;

namespace NetApi.Application.Media;

public interface IMediaRepository : IHasOwnershipRepository
{
    Task<MediaEntity?> GetByIdAsync(MediaId mediaId, List<Expression<Func<MediaEntity, object>>>? includes, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MediaEntity>> GetListAsync(MediaFilter filter, CancellationToken cancellationToken = default);
    Task<MediaId> CreateAsync(MediaEntity entity, CancellationToken cancellationToken = default);
    Task<MediaEntity?> UpdateAsync(MediaEntity entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteManyAsync(MediaEntity[] entities, CancellationToken cancellationToken = default);
}
