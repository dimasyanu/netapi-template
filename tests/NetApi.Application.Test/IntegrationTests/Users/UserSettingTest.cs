using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Users;
using NetApi.Application.Users.Queries;
using NetApi.Domain.Users.Entities;
using NetApi.Infrastructure.Persistence;
using NetApi.Infrastructure.Persistence.Repositories;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Users;

public class UserSettingTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddScoped<IUserSettingRepository, UserSettingRepository>();
        services.AddMediatR(conf => conf.RegisterServicesFromAssemblyContaining<GetUserSettingsQueryHandler>());
    }

    [Fact]
    public async Task GetUserSettings_ShouldReturnSettings_ForExistingUser()
    {
        var initialSettings = new Dictionary<string, object> {
            { "theme", "dark" },
            { "receive-newsletter", false },
            { "language", "en-US" }
        };

        using (var scope = Service.CreateScope()) {
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            foreach (var (key, value) in initialSettings) {
                await dbContext.UserSettings.AddAsync(new UserSettingEntity {
                    UserId = Admin.Id,
                    Key = key,
                    Value = JsonSerializer.Serialize(value),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = Admin.Username,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = Admin.Username
                });
            }
            await dbContext.SaveChangesAsync();
        }

        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var userSettings = await mediator.Send(new GetUserSettingsQuery(Admin.Id));

            userSettings.Should().NotBeNull();
            userSettings.Theme.Should().Be("dark");
            userSettings.DefaultLanguage.Should().Be("en-US");
            userSettings.ReceiveNewsletter.Should().BeFalse();
        }
    }

    [Fact]
    public async Task CreateUserSettings_ShouldSavedIntoDatabase()
    {
        var newSettings = new Domain.Users.UserSetting {
            Theme = "dark",
            DefaultLanguage = "fr",
            ReceiveNewsletter = false
        };

        using (var scope = Service.CreateScope()) {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userSettingEntities = newSettings.ToEntities();
            foreach (var entity in userSettingEntities) {
                entity.UserId = Admin.Id;
                entity.CreatedAt = DateTime.UtcNow;
                entity.CreatedBy = Admin.Username;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = Admin.Username;
                await dbContext.UserSettings.AddAsync(entity);
            }
            await dbContext.SaveChangesAsync();
        }

        using (var scope = Service.CreateScope()) {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var savedSettings = await dbContext.UserSettings
                .Where(us => us.UserId == Admin.Id)
                .ToListAsync();

            savedSettings.Should().HaveCount(3);
            savedSettings.FirstOrDefault(us => us.Key == "theme")!.Value.Should().Be("\"dark\"");
            savedSettings.FirstOrDefault(us => us.Key == "language")!.Value.Should().Be("\"fr\"");
            savedSettings.FirstOrDefault(us => us.Key == "receive-newsletter")!.Value.Should().Be("false");
        }
    }
}
