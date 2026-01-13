using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Media.Entities;
using NetApi.Domain.Media.ValueObjects;

namespace NetApi.Domain.Media;

public class Media : IHasEntity<MediaEntity>
{
    public MediaId? Id { get; set; }

    public MediaEntity ToEntity()
        => new() {
            Id = Id,
        };
}
