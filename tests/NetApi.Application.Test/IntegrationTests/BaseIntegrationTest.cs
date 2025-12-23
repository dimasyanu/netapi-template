using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Users;
using NetApi.Domain.Repositories;
using NetApi.Domain.Roles;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Infrastructure.Persistence;
using NetApi.Infrastructure.Persistence.Services;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests;

public abstract class BaseIntegrationTest : IDisposable
{
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

            var adminRole = new Role {
                Name = "Admin",
                CreatedAt = DateTime.Now,
                CreatedBy = "system",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "system",
            };

            Admin = new User {
                Id = UserId.Create(),
                FirstName = "Admin",
                LastName = "User",
                Username = "admin",
                Email = EmailAddress.FromString("admin@mail.com"),
                CreatedAt = DateTime.Now,
                CreatedBy = "system",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "system",
                Roles = [adminRole]
            };
            var adminEntity = Admin.ToEntity();
            adminEntity.PasswordHash = hasher.HashPassword("Admin@123");
            dbContext.Users.Add(adminEntity);
            dbContext.SaveChanges();
        }

        using (var scope = Service.CreateScope()) {
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roles = await dbContext.Roles.Include(r => r.Users).ToListAsync(cancellationToken);
            roles.Should().HaveCount(1).And.ContainSingle(r => r.Name == "Admin");
            roles[0].Users.Should().HaveCount(1).And.ContainSingle(u => u.Username == "admin");

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
}
