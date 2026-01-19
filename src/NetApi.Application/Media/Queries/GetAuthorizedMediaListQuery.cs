using NetApi.Application.Common;
using NetApi.Application.Common.Abstractions;
using NetApi.Application.Common.Attributes;
using NetApi.Domain.Common.Constants;
using MediaDto = NetApi.Domain.Media.Media;

namespace NetApi.Application.Media.Queries;

[Authorize(Feature.Media, Permission.Read)]
public class GetAuthorizedMediaListQuery : AuthorizedQuery<MediaDto>
{
}
