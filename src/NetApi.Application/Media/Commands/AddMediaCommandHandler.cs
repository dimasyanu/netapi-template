using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Domain.Media.Entities;
using NetApi.Domain.Media.ValueObjects;
using MediaDto = NetApi.Domain.Media.Media;

namespace NetApi.Application.Media.Commands;

public class AddMediaCommandHandler(IMediaRepository repo) : ICommandHandler<AddMediaCommand, MediaDto>
{
    private readonly IMediaRepository _repo = repo;

    public async Task<MediaDto> Handle(AddMediaCommand request, CancellationToken cancellationToken)
    {
        if (request.User == null || request.User.Id == null) throw new UnauthorizedException();
        if (request.FileBytes.Length < 1) throw new BadRequestException("Invalid file");

        var extension = "";
        var fileNameSegments = request.FileName.Split('.');
        if (fileNameSegments.Length > 1) extension = fileNameSegments[1];

        // Create the directory
        if (request.Path.StartsWith('/')) request.Path = request.Path.TrimStart('/');
        var physicalPath = $"Media/{request.User.Username}/{request.Path}";
        if (!Directory.Exists(physicalPath)) Directory.CreateDirectory(physicalPath);

        // Check if the file exists. If it exists, then rename the new one
        var fileName = request.FileName;
        if (File.Exists($"{physicalPath}/{fileName}")) {
            fileName = $"{fileNameSegments[0]}1" + (fileNameSegments.Length > 1 ? fileNameSegments[1] : "");
        }

        var fileSize = request.FileBytes.Length / 1024.0;

        // Store in the machine
        using (var file = File.OpenWrite($"{physicalPath}/{fileName}")) {
            using var tmpStream = new MemoryStream(request.FileBytes);
            tmpStream.CopyTo(file);
            tmpStream.Flush();
        }

        var relativePath = request.Path;
        if (!relativePath.StartsWith('/')) relativePath = $"/{relativePath}";
        var mediaEntity = new MediaEntity {
            Name = fileNameSegments[0],
            Path = relativePath,
            UserId = request.User.Id,
            Format = extension,
            SizeInKb = fileSize,
            MediaType = GetMediaTypeFromExtension(extension),
            CreatedBy = request.User.Username,
            CreatedAt = DateTime.Now,
            UpdatedBy = request.User.Username,
            UpdatedAt = DateTime.Now
        };

        await _repo.CreateAsync(mediaEntity, cancellationToken);

        return MediaDto.FromEntity(mediaEntity);
    }

    private static MediaType GetMediaTypeFromExtension(string extension = "")
    {
        extension = extension.ToLower().Trim();
        if (string.IsNullOrEmpty(extension)) return MediaType.Unknown;

        return extension switch {
            "mp3"
            or "m4a"
            or "ogg"
                => MediaType.Audio,

            "doc"
            or "docx"
            or "md"
            or "pdf"
            or "txt"
            or "xls"
            or "xlsx"
                => MediaType.Document,

            "jpg"
            or "jpeg"
            or "png"
            or "webp"
                => MediaType.Image,

            "flv"
            or "mp4"
            or "mov"
                => MediaType.Video,

            _ => MediaType.Unknown,
        };
    }
}
