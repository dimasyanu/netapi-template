using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Common.PipelineBehaviors;
using NetApi.Application.Media;
using NetApi.Application.Media.Queries;
using NetApi.Application.Roles;
using NetApi.Domain.Common.Constants;
using NetApi.Domain.Common.Extensions;
using NetApi.Domain.Media.Entities;
using NetApi.Domain.Media.ValueObjects;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Users;
using NetApi.Infrastructure.Persistence;
using NetApi.Infrastructure.Persistence.Repositories;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Media;

public class MediaFetchTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    private const string DummyFilePath = "Files/smile_cat.jpg";
    private readonly List<User> _users = [];

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddScoped<IMediaRepository, MediaRepository>();

        services.AddMediatR(conf => {
            conf.RegisterServicesFromAssemblyContaining<GetAuthorizedMediaByIdQueryHandler>();
            conf.AddOpenBehavior(typeof(AuthorizedRequestBehavior<,>));
        });
    }

    [Fact]
    public async Task FetchSingleMedia_AsAnotherUser_ShouldFailed()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;
        var mediaList = await PrepareMedia(10);
        var anotherUser = await PrepareUser("User2");
        mediaList.Should().AllSatisfy(x => x.Id.Should().NotBe(MediaId.Empty));

        // Get a single media by random index
        var randomIndex = new Random().Next(mediaList.Count);
        var targetMedia = mediaList[randomIndex];

        var query = new GetAuthorizedMediaByIdQuery {
            MediaId = targetMedia.Id!.ToGuid(),
            User = anotherUser,
        };

        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            Func<Task> action = async () => await mediator.Send(query, cancellationToken);
            await action.Should().ThrowAsync<UnauthorizedException>();
        }
    }

    [Fact]
    public async Task FetchSingleMedia_AsTheOwner_ShouldSuccess()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;
        var mediaList = await PrepareMedia(10);
        mediaList.Should().AllSatisfy(x => x.Id.Should().NotBe(MediaId.Empty));

        // Get a single media by random index
        var randomIndex = new Random().Next(mediaList.Count);
        var targetMedia = mediaList[randomIndex];

        var query = new GetAuthorizedMediaByIdQuery {
            MediaId = targetMedia.Id!.ToGuid(),
            User = _users[0],
        };

        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            Func<Task> action = async () => await mediator.Send(query, cancellationToken);
            await action.Should().NotThrowAsync();
        }
    }

    private async Task<IReadOnlyList<MediaEntity>> PrepareMedia(byte count)
    {
        const string physicalPath = "Media/Images";
        if (!Directory.Exists(physicalPath)) Directory.CreateDirectory(physicalPath);

        var fileNames = new List<string>();
        var user = await PrepareUser("User1", async userEntity => {
            using var scope = Service.CreateScope();
            var roleRepo = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
            var role = userEntity.Roles[0];
            role.Permissions = [
                new RolePermissionEntity() {
                    Feature = Feature.Media,
                    Action = Permission.Read,
                    IsAllowed = true,
                }.SetCreated(Admin.Username)
            ];
            await roleRepo.UpdateAsync(role);
        });

        var tasks = new List<Task>();
        var srcFileBytes = await File.ReadAllBytesAsync(DummyFilePath);
        for (byte i = 0; i < count; i++) {
            var c = i + 1;
            fileNames.Add($"pic{c}.jpg");
            var fileName = $"pic{c}.jpg";
            tasks.Add(File.AppendAllBytesAsync($"{physicalPath}/{fileName}", srcFileBytes));
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
                SizeInKb = srcFileBytes.Length / 1024.0,
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

        _users.Add(user);

        return newEntities;
    }
}
