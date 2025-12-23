using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Users;
using NetApi.Application.Users.Commands;
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
                    UserId = Admin.Id!,
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
            var userSettings = await mediator.Send(new GetUserSettingsQuery(Admin.Id!));

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
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var command = new SaveUserSettingsCommand { UserId = Admin.Id!, UserSettings = newSettings, User = Admin };
            Func<Task> act = async () => await mediator.Send(command);
            await act.Should().NotThrowAsync();
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

    [Fact]
    public async Task UpdateUserSettings_ShouldModifyExistingSettings()
    {
        // Arrange initial settings
        var newSettings = new Domain.Users.UserSetting {
            Theme = "dark",
            DefaultLanguage = "fr",
            ReceiveNewsletter = false
        };

        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var command = new SaveUserSettingsCommand { UserId = Admin.Id!, UserSettings = newSettings, User = Admin };
            Func<Task> act = async () => await mediator.Send(command);
            await act.Should().NotThrowAsync();
        }


        // Act: Update the "theme" setting
        newSettings = new Domain.Users.UserSetting {
            Theme = "light",
            DefaultLanguage = "en",
            ReceiveNewsletter = true
        };
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var command = new SaveUserSettingsCommand { UserId = Admin.Id!, UserSettings = newSettings, User = Admin };
            Func<Task> act = async () => await mediator.Send(command);
            await act.Should().NotThrowAsync();
        }

        // Assert: Verify the "theme" setting is updated
        using (var scope = Service.CreateScope()) {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updatedSetting = await dbContext.UserSettings.ToListAsync(TestCancelToken);

            updatedSetting.Should().HaveCount(3);
            updatedSetting.FirstOrDefault(us => us.Key == "theme").Should().NotBeNull()
                .And.Subject.As<UserSettingEntity>().Value.Should().Be(JsonSerializer.Serialize(newSettings.Theme));
            updatedSetting.FirstOrDefault(us => us.Key == "language").Should().NotBeNull()
                .And.Subject.As<UserSettingEntity>().Value.Should().Be(JsonSerializer.Serialize(newSettings.DefaultLanguage));
            updatedSetting.FirstOrDefault(us => us.Key == "receive-newsletter").Should().NotBeNull()
                .And.Subject.As<UserSettingEntity>().Value.Should().Be(newSettings.ReceiveNewsletter.ToString().ToLower());
        }
    }
}
