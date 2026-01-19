using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Domain.Media.ValueObjects;
using MediaDto = NetApi.Domain.Media.Media;

namespace NetApi.Application.Media.Queries;

public class GetAuthorizedMediaByIdQueryHandler(IMediaRepository repo) : IQueryHandler<GetAuthorizedMediaByIdQuery, MediaDto>
{
    private readonly IMediaRepository _repo = repo;

    public async Task<MediaDto> Handle(GetAuthorizedMediaByIdQuery request, CancellationToken cancellationToken = default)
    {
        var mediaEntity = await _repo.GetByIdAsync(MediaId.FromGuid(request.MediaId), null, cancellationToken)
            ?? throw new NotFoundException("Media not found");
        var mediaDto = MediaDto.FromEntity(mediaEntity);
        return mediaDto;
    }
}
