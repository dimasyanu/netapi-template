using NetApi.Application.Common.Abstractions;
using MediaDto = NetApi.Domain.Media.Media;

namespace NetApi.Application.Media.Queries;

public class GetAuthorizedMediaQuery : AuthorizedQuery<MediaDto>
{
    public Guid MediaId { get; set; }
}
