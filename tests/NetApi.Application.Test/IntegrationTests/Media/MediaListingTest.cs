using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common;
using NetApi.Application.Common.PipelineBehaviors;
using NetApi.Application.Media;
using NetApi.Application.Media.Queries;
using NetApi.Domain.Common.Constants;
using NetApi.Domain.Common.Extensions;
using NetApi.Domain.Media.Entities;
using NetApi.Domain.Media.ValueObjects;
using NetApi.Domain.Roles;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Infrastructure.Persistence;
using NetApi.Infrastructure.Persistence.Repositories;
using Xunit.Abstractions;
using MediaType = NetApi.Domain.Media.ValueObjects.MediaType;

namespace NetApi.Application.Test.IntegrationTests.Media;

public class MediaListingTest(ITestOutputHelper outputHelper) : BaseIntegrationTest(outputHelper)
{
    private const string DummyFilePath = "Files/smile_cat.jpg";

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddMediatR(conf => {
            conf.RegisterServicesFromAssemblyContaining<GetAuthorizedMediaListQueryHandler>();
            conf.AddOpenBehavior(typeof(AuthorizedRequestBehavior<,>));
        });
    }

    [Fact]
    public async Task ListMedia_AsOwner_ShouldSucceed()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;

        // Prepare user with media and proper permissions
        var user = await PrepareUser("OwnerUser");

        // Prepare media for the user
        Task<IReadOnlyList<MediaEntity>> task1 = PrepareMedia(5, user, "/"),
            task2 = PrepareMedia(2, user, "/Videos"),
            task3 = PrepareMedia(4, user, "/Images");
        var mediaLists = await Task.WhenAll(task1, task2, task3);
        var mediaList = mediaLists.SelectMany(x => x).ToList();

        Assert.All(mediaList, x => Assert.NotEqual(MediaId.Empty, x.Id));

        using (var scope = Service.CreateScope()) {
            var query = new GetAuthorizedMediaListQuery { User = user, Path = "/" };
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var results = await mediator.Send(query, cancellationToken);
            Assert.Equal(7, results.Count); // 5 files + 2 directories

            var resultIds = results.Where(x => !x.IsDirectory).Select(x => x.Id!.ToGuid()).ToHashSet();
            var expectedIds = mediaList.Where(x => x.Path == "/").Select(x => x.Id!.ToGuid()).ToHashSet();
            Assert.Equal(expectedIds, resultIds);
        }

        foreach (var (path, count) in new[] { ("/Videos", 2), ("/Images", 4) }) {
            using var scope = Service.CreateScope();
            var query = new GetAuthorizedMediaListQuery { User = user, Path = path };
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var results = await mediator.Send(query, cancellationToken);
            Assert.Equal(count, results.Count);

            var resultIds = results.Where(x => !x.IsDirectory).Select(x => x.Id!.ToGuid()).ToHashSet();
            var expectedIds = mediaList.Where(x => x.Path.StartsWith(path)).Select(x => x.Id!.ToGuid()).ToHashSet();
            Assert.Equal(expectedIds, resultIds);
        }
    }

    /// <summary>
    /// Prepare a user for testing
    /// </summary>
    /// <param name="username"></param>
    /// <param name="onCreated"></param>
    /// <returns></returns>
    private async Task<User> PrepareUser(string username)
    {
        using var scope = Service.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new User {
            Username = username,
            EmailAddress = EmailAddress.FromString($"{username.ToLower()}@example.com"),
            FirstName = username,
            Roles = [
                new Role {
                }
            ],
            CreatedAt = DateTime.Now,
            CreatedBy = Admin.Username,
            UpdatedAt = DateTime.Now,
            UpdatedBy = Admin.Username,
        }.ToEntity().SetCreated(Admin.Username);
        user.Roles.Add(new RoleEntity {
            Name = "editor",
            Description = "Editor role",
            Permissions = [
                new RolePermissionEntity() {
                    Feature = Feature.Media,
                    Action = Permission.Read,
                    IsAllowed = true,
                }.SetCreated(Admin.Username)
            ],
        }.SetCreated(Admin.Username));
        user.PasswordHash = "hashed_password";

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return User.FromEntity(user);
    }

    /// <summary>
    /// Prepare media for testing
    /// </summary>
    /// <param name="count"></param>
    /// <param name="user"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private async Task<IReadOnlyList<MediaEntity>> PrepareMedia(byte count, User user, string path = "/")
    {
        var physicalPath = $"Media/{user.Username}/{path}";
        if (!Directory.Exists(physicalPath)) Directory.CreateDirectory(physicalPath);

        var fileNames = new List<string>();
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
                Path = path,
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

        return newEntities;
    }
}
