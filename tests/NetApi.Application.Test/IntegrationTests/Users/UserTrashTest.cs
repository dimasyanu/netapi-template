using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Users;
using NetApi.Application.Users.Commands;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Infrastructure.Persistence;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Users;

public class UserTrashTest(ITestOutputHelper outputHelper) : BaseIntegrationTest(outputHelper)
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddMediatR(conf => {
            conf.RegisterServicesFromAssemblyContaining<TrashManyUsersCommand>();
        });
    }

    [Fact]
    public async Task TrashAndRestoreUser_Success()
    {
        var user = await PrepareUser();

        var trashCmd = new TrashManyUsersCommand { Ids = [user.Id!.ToGuid()], User = Admin };
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            Func<Task> action = async () => await mediator.Send(trashCmd);
            await action.Should().NotThrowAsync();
        }

        using (var scope = Service.CreateScope()) {
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userEntity = dbContext.Users.Single(x => x.Id == user.Id);
            userEntity.DeletedAt.Should().NotBeNull();
            userEntity.DeletedBy.Should().NotBeNull();
        }

        var restoreCmd = new RestoreManyUsersCommand { Ids = [user.Id!.ToGuid()], User = Admin };
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            Func<Task> action = async () => await mediator.Send(restoreCmd);
            await action.Should().NotThrowAsync();
        }

        using (var scope = Service.CreateScope()) {
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userEntity = dbContext.Users.Single(x => x.Id == user.Id);
            userEntity.DeletedAt.Should().BeNull();
            userEntity.DeletedBy.Should().BeNull();
        }
    }

    private async Task<User> PrepareUser()
    {
        // Arrange
        var newRole = new RoleEntity {
            Name = "customer",
            Description = "Administrator role",
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
        var newUserEntity = new User {
            Username = "testuser",
            EmailAddress = EmailAddress.FromString("testuser@example.com"),
            FirstName = "Test",
            LastName = "User",
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
}
