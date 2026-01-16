using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Roles;
using NetApi.Application.Users;
using NetApi.Domain.Common.Extensions;
using NetApi.Domain.Roles;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Roles.ValueObjects;
using NetApi.Domain.Users;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Infrastructure.Persistence;
using NetApi.Infrastructure.Persistence.Repositories;
using NetApi.Infrastructure.Persistence.Services;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests;

public abstract class BaseIntegrationTest : IDisposable
{
    protected const string AdminPassword = "Admin@123";

    protected readonly IServiceProvider Service;
    protected readonly ITestOutputHelper Output;

    protected User Admin { get; private set; } = new();
    protected readonly CancellationToken TestCancelToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token; // 30 seconds timeout

    public BaseIntegrationTest(ITestOutputHelper output)
    {
        Output = output;

        var dbName = "TestDb_" + new Random().Next().ToString();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDbContext<AppDbContext>(options => {
            options.UseInMemoryDatabase(dbName);
        });
        serviceCollection.AddLogging();

        ConfigureServices(serviceCollection);
        Service = serviceCollection.BuildServiceProvider();

        ConfigureAdminUser().GetAwaiter().GetResult();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IHashingService, HashingService>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
    }

    private async Task ConfigureAdminUser()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token; // 20 seconds timeout

        var t = DateTime.Now;
        using (var scope = Service.CreateScope()) {
            var hasher = scope.ServiceProvider.GetRequiredService<IHashingService>();
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureCreated();

            var adminRole = new RoleEntity {
                Name = "Admin",
                IsSuperAdmin = true
            }.SetCreated("system");

            var adminEntity = new UserEntity {
                Id = UserId.New(),
                FirstName = "Admin",
                LastName = "User",
                Username = "admin",
                EmailAddress = EmailAddress.FromString("admin@mail.com"),
                Roles = [adminRole]
            }.SetCreated("system");
            adminEntity.PasswordHash = hasher.HashPassword(AdminPassword);
            await dbContext.Users.AddAsync(adminEntity);
            await dbContext.SaveChangesAsync();

            Admin = User.FromEntity(adminEntity);
        }

        using (var scope = Service.CreateScope()) {
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roles = await dbContext.Roles.Include(r => r.Users).ToListAsync(cancellationToken);
            roles.Should().HaveCount(1).And.ContainSingle(r => r.Name == "Admin");
            roles[0].Users.Should().HaveCount(1).And.ContainSingle(u => u.Username == "admin");
            roles[0].IsSuperAdmin.Should().BeTrue();

            var userRoles = await dbContext.UserRoles.ToListAsync(cancellationToken);
            userRoles.Should().HaveCount(1);
            userRoles[0].UserId.Should().Be(Admin.Id);
            userRoles[0].RoleId.Should().Be(roles[0].Id);
            userRoles[0].AssignedAt.Should().BeCloseTo(t, TimeSpan.FromSeconds(5));
        }
    }

    public virtual void Dispose()
    {
        using var scope = Service.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureDeleted();

        // Dispose all IDisposables in the service provider
        var disposable = scope.ServiceProvider.GetServices<IDisposable>();
        if (disposable != null && disposable.Any()) {
            foreach (var d in disposable) d.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    protected virtual void DeleteFolder(string path = "")
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;
        var dir = new DirectoryInfo(path);
        foreach (var subDir in dir.GetDirectories()) DeleteFolder(subDir.FullName);
        foreach (var file in dir.GetFiles()) file.Delete();
        Directory.Delete(path);
    }

    protected async Task<User> PrepareUser(string userName = "User1")
    {
        // Arrange
        var newRole = new RoleEntity {
            Name = "customer",
            Description = "Editor role",
            CreatedAt = DateTime.Now,
            CreatedBy = Admin.Username,
            UpdatedAt = DateTime.Now,
            UpdatedBy = Admin.Username,
        };
        using (var scope = Service.CreateScope()) {
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Roles.Add(newRole);
            await dbContext.SaveChangesAsync();
        }

        UserId userId;
        var lowerUsername = userName.ToLower();
        var newUserEntity = new User {
            Username = lowerUsername,
            EmailAddress = EmailAddress.FromString($"{lowerUsername}@example.com"),
            FirstName = userName,
            LastName = "Ipsum",
        }.ToEntity();
        newUserEntity.Roles = [newRole];

        using (var scope = Service.CreateScope()) {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            newUserEntity.PasswordHash = "hashedpassword";
            userId = await userRepository.CreateAsync(newUserEntity);
        }

        using (var scope = Service.CreateScope()) {
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userRoles = dbContext.UserRoles.ToList();
            userRoles.Should().HaveCount(2);
            userRoles.FirstOrDefault(x => x.UserId == userId && x.RoleId == newRole.Id)
                .Should().NotBeNull();
        }

        return User.FromEntity(newUserEntity);
    }

    protected string GetTestName()
    {
        var type = Output.GetType();
        var testMember = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
        var test = (ITest)testMember!.GetValue(Output)!;
        return test.DisplayName.Split('.').Last();
    }
}
