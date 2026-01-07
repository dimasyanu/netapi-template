using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Common.PipelineBehaviors;
using NetApi.Application.Roles.Commands;
using NetApi.Application.Roles.Queries;
using NetApi.Application.Users;
using NetApi.Domain.Common.Constants;
using NetApi.Domain.Common.Extensions;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Users;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Infrastructure.Persistence;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Roles;

public class RolePermissionTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddMediatR(conf => {
            conf.RegisterServicesFromAssemblyContaining<CreateRoleCommandHandler>();
            conf.AddOpenBehavior(typeof(AuthorizedRequestBehavior<,>));
        });
    }

    [Fact]
    public async Task CreateRole_WithSpecificPermission_Success()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;
        var userEntity = await PrepareUser(cancellationToken);
        var user = User.FromEntity(userEntity);

        // Without permission
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var cmd = new CreateRoleCommand {
                Name = "another_role",
                Description = "Another role",
                User = user
            };
            Func<Task> action = async () => await mediator.Send(cmd, cancellationToken);
            await action.Should().ThrowAsync<UnauthorizedException>();
        }

        // Update create permission
        using (var scope = Service.CreateScope()) {
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var permissions = await dbContext.RolePermissions.ToListAsync(cancellationToken);
            var roles = await dbContext.Roles.ToListAsync(cancellationToken);
            var permission = await dbContext.RolePermissions.SingleAsync(x =>
                x.RoleId == user.Roles![0].Id
                && x.Feature == RoleConstant.FeatureName
                && x.Action == RoleConstant.Permission.Create
            );
            permission.IsAllowed = true;
            dbContext.RolePermissions.Update(permission);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // With permission
        using (var scope = Service.CreateScope()) {
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            userEntity = await userRepo.GetByIdAsync(user.Id!, [u => u.Roles]);
            userEntity.Should().NotBeNull();
            userEntity.Roles.Should().NotBeNull().And.HaveCount(1);
            userEntity.Roles[0].Permissions.Should().NotBeNull().And.HaveCount(4);
            userEntity.Roles[0].Permissions!.Any(x => x.IsAllowed).Should().BeTrue();

            var cmd = new CreateRoleCommand {
                Name = "another_role",
                Description = "Another role",
                User = User.FromEntity(userEntity)
            };
            Func<Task> action = async () => await mediator.Send(cmd, cancellationToken);
            await action.Should().NotThrowAsync();
        }
    }

    [Fact]
    public async Task ReadRole_WithSpecificPermission_Success()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;
        var userEntity = await PrepareUser(cancellationToken);
        var user = User.FromEntity(userEntity);

        // Without permission
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var cmd = new GetRolesQuery { User = user };
            Func<Task> action = async () => await mediator.Send(cmd, cancellationToken);
            await action.Should().ThrowAsync<UnauthorizedException>();
        }

        // Update create permission
        using (var scope = Service.CreateScope()) {
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var permissions = await dbContext.RolePermissions.ToListAsync(cancellationToken);
            var roles = await dbContext.Roles.ToListAsync(cancellationToken);
            var permission = await dbContext.RolePermissions.SingleAsync(x =>
                x.RoleId == user.Roles![0].Id
                && x.Feature == RoleConstant.FeatureName
                && x.Action == RoleConstant.Permission.Read
            );
            permission.IsAllowed = true;
            dbContext.RolePermissions.Update(permission);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // With permission
        using (var scope = Service.CreateScope()) {
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            userEntity = await userRepo.GetByIdAsync(user.Id!, [u => u.Roles]);
            userEntity.Should().NotBeNull();

            var cmd = new GetRolesQuery { User = User.FromEntity(userEntity) };
            Func<Task> action = async () => await mediator.Send(cmd, cancellationToken);
            await action.Should().NotThrowAsync();
        }
    }

    private async Task<UserEntity> PrepareUser(CancellationToken cancellationToken = default)
    {
        var adminEmailAddress = Admin.EmailAddress.ToString();
        var newUser = new UserEntity {
            FirstName = "New Customer",
            EmailAddress = EmailAddress.FromString("new_customer@mail.com"),
            Username = "new_customer",
            PasswordHash = Guid.NewGuid().ToString(),
            Roles = [
                new RoleEntity {
                    Name = "maintainer",
                    Description = "Testing role",
                    IsSuperAdmin = false,
                    Permissions = [
                        new RolePermissionEntity() {
                            Feature = RoleConstant.FeatureName,
                            Action = RoleConstant.Permission.Read,
                            IsAllowed = false,
                        }.SetCreated(adminEmailAddress),
                        new RolePermissionEntity() {
                            Feature = RoleConstant.FeatureName,
                            Action = RoleConstant.Permission.Create,
                            IsAllowed = false,
                        }.SetCreated(adminEmailAddress),
                        new RolePermissionEntity() {
                            Feature = RoleConstant.FeatureName,
                            Action = RoleConstant.Permission.Update,
                            IsAllowed = false,
                        }.SetCreated(adminEmailAddress),
                        new RolePermissionEntity() {
                            Feature = RoleConstant.FeatureName,
                            Action = RoleConstant.Permission.Delete,
                            IsAllowed = false,
                        }.SetCreated(adminEmailAddress),
                    ]
                }.SetCreated(adminEmailAddress),
            ]
        };

        using var scope = Service.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Users.AddAsync(newUser, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return newUser;
    }
}

