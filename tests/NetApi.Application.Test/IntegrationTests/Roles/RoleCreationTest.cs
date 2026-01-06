using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Common.Extensions;
using NetApi.Application.Common.Models;
using NetApi.Application.Common.PipelineBehaviors;
using NetApi.Application.Roles;
using NetApi.Application.Roles.Commands;
using NetApi.Application.Roles.Queries;
using NetApi.Domain.Common.Extensions;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Roles.ValueObjects;
using NetApi.Domain.Users;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Infrastructure.Persistence;
using NetApi.Infrastructure.Persistence.Repositories;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Roles;

public class RoleCreationTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    override protected void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddScoped<IRoleRepository, RoleRepository>();

        services.AddMediatR(conf => {
            conf.RegisterServicesFromAssemblyContaining<CreateRoleCommandHandler>();
            conf.RegisterServicesFromAssemblyContaining<GetRolesQueryHandler>();
            conf.RegisterServicesFromAssemblyContaining<UpdateRoleCommandHandler>();
            conf.RegisterServicesFromAssemblyContaining<SoftDeleteRoleCommandHandler>();
            conf.RegisterServicesFromAssemblyContaining<RestoreRoleCommandHandler>();

            conf.AddOpenBehavior(typeof(AuthorizedRequestBehavior<,>));
        });
    }

    [Fact]
    public async Task ListRoles_ShouldReturnCreatedRole()
    {
        var roleName = string.Concat("TestRole_", Guid.NewGuid().ToString("N").AsSpan(0, 8));

        using (var scope = Service.CreateScope()) {
            // Create Role
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var newRole = new RoleEntity {
                Name = roleName.ToSnakeCase(),
                Description = "A role created during integration testing.",
            }.SetCreated(Admin.EmailAddress.ToString());
            dbContext.Roles.Add(newRole);
            await dbContext.SaveChangesAsync();
        }

        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // List Roles
            var getRolesQuery = new GetRolesQuery {
                SortingOption = new SortingOption {
                    SortBy = "Name",
                    SortDirection = SortingOption.DIRECTION_DESCENDING
                }
            };
            var roles = await mediator.Send(getRolesQuery);
            roles.Should().NotBeNull()
                .And.HaveCount(2)
                .And.Contain(x => x.Name == roleName.ToSnakeCase())
                .And.BeInDescendingOrder(r => r.Name);
        }
    }

    [Fact]
    public async Task CreateRole_WithUnauthorizedPermission_ShouldFail()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;
        var adminUser = Admin.EmailAddress.ToString();
        var customerRole = new RoleEntity {
            Name = "customer",
            Description = "A role for testing"
        }.SetCreated(adminUser);
        var newUser = new UserEntity {
            FirstName = "New Customer",
            EmailAddress = EmailAddress.FromString("new_customer@mail.com"),
            Roles = [customerRole]
        }.SetCreated(adminUser);
        var customerPassword = "abcde";

        using (var scope = Service.CreateScope()) {
            var hashingService = scope.ServiceProvider.GetRequiredService<IHashingService>();
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            newUser.PasswordHash = hashingService.HashPassword(customerPassword);
            await dbContext.Users.AddAsync(newUser, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var customerUser = User.FromEntity(newUser);
        var createRoleCmd = new CreateRoleCommand {
            Name = "editor",
            Description = "Another role"
        };
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            Func<Task> action = async () => await mediator.Send(createRoleCmd, cancellationToken);
            await action.Should().ThrowAsync<UnauthorizedException>();

            // Create a role with customer role permissions - should fail
            createRoleCmd = new() {
                Name = createRoleCmd.Name,
                Description = createRoleCmd.Description,
                User = customerUser,
            };
            action = async () => await mediator.Send(createRoleCmd, cancellationToken);
            await action.Should().ThrowAsync<UnauthorizedException>();

            // Create a role with admin role permission - should success
            createRoleCmd = new() {
                Name = createRoleCmd.Name,
                Description = createRoleCmd.Description,
                User = Admin,
            };
            action = async () => await mediator.Send(createRoleCmd, cancellationToken);
            await action.Should().NotThrowAsync();
        }
    }

    [Fact]
    public async Task CreateRole_ShouldSucceed()
    {
        var roleName = string.Concat("TestRole_", Guid.NewGuid().ToString("N").AsSpan(0, 8));
        RoleId roleId;

        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Create Role
            var createCommand = new CreateRoleCommand {
                Name = roleName,
                Description = "A role created during integration testing.",
                User = Admin
            };
            var createdRole = await mediator.Send(createCommand);
            createdRole.Should().NotBeNull();
            createdRole.Id.Should().NotBeNull();
            createdRole.Name.Should().Be(roleName.ToSnakeCase());
            createdRole.Description.Should().Be(createCommand.Description);

            roleId = createdRole.Id;
        }

        // Verify Role Exists in DB
        using (var scope = Service.CreateScope()) {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var rolesInDb = await dbContext.Roles.ToListAsync();
            rolesInDb.Should().NotBeNull()
                .And.HaveCountGreaterThan(1); // Including seeded roles
            var roleInDb = rolesInDb.FirstOrDefault(r => r.Id == roleId);
            roleInDb.Should().NotBeNull();
            roleInDb.Name.Should().Be(roleName.ToSnakeCase());
        }
    }

    [Fact]
    public async Task CreateRole_WithDuplicateName_ShouldFail()
    {
        var roleName = "Admin"; // Assuming "Admin" role already exists from seeding
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Attempt to Create Role with Duplicate Name
            var createCommand = new CreateRoleCommand {
                Name = roleName,
                Description = "Attempting to create a duplicate role.",
                User = Admin
            };

            Func<Task> act = async () => { await mediator.Send(createCommand); };

            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage($"Role with name '{roleName}' already exists.");
        }
    }

    [Fact]
    public async Task CreateRole_WithInvalidName_ShouldFail()
    {
        var invalidRoleName = "Invalid Role Name!@#"; // Invalid characters
        using var scope = Service.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Attempt to Create Role with Invalid Name
        var createCommand = new CreateRoleCommand {
            Name = invalidRoleName,
            Description = "Attempting to create a role with invalid name.",
            User = Admin
        };
        Func<Task> act = async () => { await mediator.Send(createCommand); };
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Special characters are not allowed in role names.");
    }

    [Fact]
    public async Task UpdateRole_ShouldSucceed()
    {
        var originalRoleName = string.Concat("TestRole_", Guid.NewGuid().ToString("N").AsSpan(0, 8));
        var updatedRoleName = string.Concat("UpdatedRole_", Guid.NewGuid().ToString("N").AsSpan(0, 8));
        RoleId roleId;
        using (var scope = Service.CreateScope()) {

            // Create Role
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var newRole = new RoleEntity {
                Name = originalRoleName.ToSnakeCase(),
                Description = "A role created during integration testing.",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "IntegrationTest",
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "IntegrationTest"
            };
            dbContext.Roles.Add(newRole);
            await dbContext.SaveChangesAsync();
            roleId = newRole.Id!;
        }

        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Update Role
            var updateCommand = new UpdateRoleCommand {
                Id = roleId,
                Name = updatedRoleName,
                Description = "Updated description.",
                User = Admin
            };
            var updatedRole = await mediator.Send(updateCommand);
            updatedRole.Should().NotBeNull();
            updatedRole.Id.Should().Be(roleId);
            updatedRole.Name.Should().Be(updatedRoleName.ToSnakeCase());
            updatedRole.Description.Should().Be(updateCommand.Description);
            updatedRole.UpdatedAt.Should().BeAfter(updatedRole.CreatedAt);
            updatedRole.UpdatedBy.Should().Be(Admin.Username);
        }
    }

    [Fact]
    public async Task UpdateRole_NonExistentRole_ShouldFail()
    {
        var nonExistentRoleId = RoleId.FromShort(99);
        using var scope = Service.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Attempt to Update Non-Existent Role
        var updateCommand = new UpdateRoleCommand {
            Id = nonExistentRoleId,
            Name = "SomeRole",
            Description = "Attempting to update a non-existent role.",
            User = Admin
        };

        Func<Task> act = async () => { await mediator.Send(updateCommand); };
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Role with ID {nonExistentRoleId} not found.");
    }

    [Fact]
    public async Task SoftDeleteRole_ShouldSucceed()
    {
        var roleName = string.Concat("TestRole_", Guid.NewGuid().ToString("N").AsSpan(0, 8));
        RoleId roleId;

        using (var scope = Service.CreateScope()) {
            // Create Role
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var newRole = new RoleEntity {
                Name = roleName.ToSnakeCase(),
                Description = "A role created during integration testing.",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "IntegrationTest",
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "IntegrationTest"
            };
            dbContext.Roles.Add(newRole);
            await dbContext.SaveChangesAsync();
            roleId = newRole.Id!;
        }

        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Soft Delete Role
            var softDeleteCommand = new SoftDeleteRoleCommand {
                Ids = new[] { roleId },
                User = Admin
            };
            var result = await mediator.Send(softDeleteCommand);
            result.Should().BeTrue();
        }

        // Verify Role is Soft Deleted in DB
        using (var scope = Service.CreateScope()) {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleInDb = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            roleInDb.Should().NotBeNull();
            roleInDb.DeletedAt.Should().NotBeNull();
            roleInDb.DeletedBy.Should().Be(Admin.Username);
        }
    }

    [Fact]
    public async Task RestoreRole_ShouldSucceed()
    {
        var roleName = string.Concat("TestRole_", Guid.NewGuid().ToString("N").AsSpan(0, 8));
        RoleId roleId;

        using (var scope = Service.CreateScope()) {
            // Create Role
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var newRole = new RoleEntity {
                Name = roleName.ToSnakeCase(),
                Description = "A role created during integration testing.",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Admin.Username,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = Admin.Username,
                DeletedAt = DateTime.UtcNow,
                DeletedBy = Admin.Username
            };
            dbContext.Roles.Add(newRole);
            await dbContext.SaveChangesAsync();
            roleId = newRole.Id!;
        }

        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Restore Role
            var restoreCommand = new RestoreRoleCommand {
                RoleIds = new[] { roleId },
                User = Admin
            };
            var result = await mediator.Send(restoreCommand);
            result.Should().BeTrue();
        }

        // Verify Role is Restored in DB
        using (var scope = Service.CreateScope()) {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleInDb = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            roleInDb.Should().NotBeNull();
            roleInDb.DeletedAt.Should().BeNull();
            roleInDb.DeletedBy.Should().BeNull();
        }
    }
}