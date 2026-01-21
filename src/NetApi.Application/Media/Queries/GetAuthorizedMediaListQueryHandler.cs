using NetApi.Application.Common.Contracts;
using NetApi.Domain.Media.Models;
using MediaDto = NetApi.Domain.Media.Media;

namespace NetApi.Application.Media.Queries;

public class GetAuthorizedMediaListQueryHandler(IMediaRepository repo) : IQueryHandler<GetAuthorizedMediaListQuery, IReadOnlyList<MediaDto>>
{
    private readonly IMediaRepository _repo = repo;

    public async Task<IReadOnlyList<MediaDto>> Handle(GetAuthorizedMediaListQuery request, CancellationToken cancellationToken)
    {
        var username = request.User!.Username;
        var filter = new MediaFilter {
            UserId = request.User.Id!,
            Name = request.Name,
            Path = request.Path
        };
        var files = await _repo.GetFileListAsync(filter, cancellationToken);
        var directories = await _repo.GetDirectoryListAsync(filter, cancellationToken);
        return [.. files.Select(x => MediaDto.FromEntity(x)), .. directories.Select(MediaDto.FromDirectory)];
    }
}
