using Microsoft.Extensions.DependencyInjection;
using NetApi.Domain.Media.Entities;
using NetApi.Domain.Media.ValueObjects;
using NetApi.Infrastructure.Persistence;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Media;

public class MediaFetchTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    private const string DummyFilePath = "Files/smile_cat.jpg";

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
    }

    [Fact]
    public async Task FetchSingleMedia_AsAnotherUser_ShouldFailed()
    {
        await PrepareMedia(10);
    }

    private async Task PrepareMedia(byte count)
    {
        const string physicalPath = "Media/Images";
        if (!Directory.Exists(physicalPath)) Directory.CreateDirectory(physicalPath);

        var fileNames = new List<string>();
        var user = await PrepareUser("User1");
        var tasks = new List<Task>();

        using var srcFileStream = File.OpenRead(DummyFilePath);
        for (var i = 0; i < count; i++) {
            var c = i + 1;
            tasks.Add(Task.Run(async () => {
                var fileName = $"pic{c}.jpg";
                using var fileStream = File.Create($"{physicalPath}/{fileName}");
                tasks.Add(srcFileStream.CopyToAsync(fileStream));
                fileNames.Add(fileName);
            }));
        }

        using var scope = Service.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var newEntities = new List<MediaEntity>();

        foreach (var fileName in fileNames) {
            newEntities.Add(new() {
                Name = fileName,
                Path = "Media/Images",
                Format = "jpg",
                MediaType = MediaType.Image,
                SizeInKb = srcFileStream.Length / 1024.0,
                UserId = user.Id,
                CreatedAt = DateTime.Now,
                CreatedBy = "user1",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "user1",
            });
        }

        tasks.Add(Task.Run(async () => {
            await dbContext.AddRangeAsync(newEntities);
            await dbContext.SaveChangesAsync();
        }));
        await Task.WhenAll(tasks);
    }

    public override void Dispose()
    {
        // Remove all dummy files
        DeleteFolder("Files");
        DeleteFolder("Media");
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
