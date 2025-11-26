using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common.Contracts;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Infrastructure.Persistence;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests;

public abstract class BaseIntegrationTest : IDisposable
{
    protected readonly IServiceProvider Service;
    protected readonly ITestOutputHelper Output;

    protected User Admin { get; private set; } = new();

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

        ConfigureAdminUser();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    private void ConfigureAdminUser()
    {
        using var scope = Service.CreateScope();
        var hasher = scope.ServiceProvider.GetRequiredService<IHashingService>();
        using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureCreated();
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
            PasswordHash = hasher.HashPassword("Admin@123"),
        };
        dbContext.Users.Add(Admin);
        dbContext.SaveChanges();
    }

    public void Dispose()
    {
        using var scope = Service.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureDeleted();
        GC.SuppressFinalize(this);
    }
}
