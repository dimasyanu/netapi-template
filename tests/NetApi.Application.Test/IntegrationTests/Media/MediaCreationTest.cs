using FluentAssertions;
using MediatR;
using MediaType = NetApi.Domain.Media.ValueObjects.MediaType;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Media.Commands;
using NetApi.Infrastructure.Persistence;
using Xunit.Abstractions;
using NetApi.Application.Media;
using NetApi.Infrastructure.Persistence.Repositories;

namespace NetApi.Application.Test.IntegrationTests.Media;

public class MediaCreationTest(ITestOutputHelper outputHelper) : BaseIntegrationTest(outputHelper)
{
    private const string DummyFilePath = "Files/smile_cat.jpg";
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddMediatR(conf => {
            conf.RegisterServicesFromAssemblyContaining<AddMediaCommandHandler>();
        });
    }

    [Fact]
    public async Task CreateMedia_Success()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;
        var user = await PrepareUser("User 1");
        const string fileName = "Image";
        const string extension = "jpg";
        var cmd = new AddMediaCommand {
            FileName = $"{fileName}.{extension}",
            FileBytes = await File.ReadAllBytesAsync(DummyFilePath, cancellationToken),
            Path = "/My Pictures",
            User = user,
        };

        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            Func<Task> action = async () => await mediator.Send(cmd);
            await action.Should().NotThrowAsync();
        }

        File.Exists($"Media/{user.Username}/My Pictures/Image.jpg").Should().BeTrue();
        using (var scope = Service.CreateScope()) {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var mediaEntities = await dbContext.Media.ToListAsync(cancellationToken);
            mediaEntities.Should().HaveCount(1);
            var media = mediaEntities[0];
            media.Name.Should().Be(fileName);
            media.Format.Should().Be(extension);
            media.MediaType.Should().Be(MediaType.Image);
            media.SizeInKb.Should().Be(cmd.FileBytes.Length / 1024.0);
            media.Path.Should().Be("/My Pictures");
            media.UserId.Should().Be(user.Id);
        }
    }
}
