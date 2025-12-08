using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Users;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Application.Users.Queries;
using MediatR;
using NetApi.Domain.Users;
using Xunit.Abstractions;
using NetApi.Application.Users.Commands;
using NetApi.Infrastructure.Persistence;
using NetApi.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Users.Entities;
using NetApi.Application.Roles;
using NetApi.Infrastructure.Persistence.Repositories;

namespace NetApi.Application.Test.IntegrationTests.Users;

public class UserCreationTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddScoped<IRoleRepository, RoleRepository>();

        services.AddMediatR(conf => {
            conf.RegisterServicesFromAssemblyContaining<GetUserByIdQueryHandler>();
            conf.RegisterServicesFromAssemblyContaining<CreateUserCommandHandler>();
        });
    }

    [Fact]
    public async Task GetUserById_ShouldSucceed()
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
        var newUser = new User {
            Username = "testuser",
            Email = EmailAddress.FromString("testuser@example.com"),
            FirstName = "Test",
            LastName = "User",
        }.ToEntity();
        using (var scope = Service.CreateScope()) {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            newUser.PasswordHash = "hashedpassword";
            userId = await userRepository.CreateAsync(newUser);

            var userRole = new UserRoleEntity {
                UserId = userId,
                RoleId = newRole.Id ?? throw new Exception("Role ID should not be null"),
                AssignedAt = DateTime.Now,
            };
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.UserRoles.Add(userRole);
            await dbContext.SaveChangesAsync();
        }

        using (var scope = Service.CreateScope()) {
            // Act
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var request = new GetUserByIdQuery(userId.ToGuid());
            var user = await mediator.Send(request);

            // Assert
            user.Should().NotBeNull();
            user.Id.Should().Be(userId);
            user.Username.Should().Be(newUser.Username);
            user.Email.Should().Be(newUser.Email);
            user.FirstName.Should().Be(newUser.FirstName);
            user.LastName.Should().Be(newUser.LastName);
            user.Roles.Should().ContainSingle()
                .Which.Name.Should().Be(newRole.Name);
        }
    }

    [Fact]
    public async Task CreateUser_ShouldSucceed()
    {
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

        var command = new CreateUserCommand {
            Username = "newuser",
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User",
            Password = "password123",
            ConfirmPassword = "password123",
            User = Admin,
            Roles = [newRole.Id!]
        };

        UserId? userId = null;
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            userId = await mediator.Send(command);
            userId.ToGuid().Should().NotBe(Guid.Empty);
        }
        var ts = DateTime.Now;

        using (var scope = Service.CreateScope()) {
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var users = await dbContext.Users.ToListAsync();
            var createdUser = await dbContext.Users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Id == userId);

            createdUser.Should().NotBeNull();
            createdUser.Username.Should().Be(command.Username);
            createdUser.Email.ToString().Should().Be(command.Email);
            createdUser.FirstName.Should().Be(command.FirstName);
            createdUser.LastName.Should().Be(command.LastName);
            createdUser.PasswordHash.Should().NotBe(command.Password); // Assuming password is hashed
            createdUser.CreatedAt.Should().BeCloseTo(ts, TimeSpan.FromSeconds(5));
            createdUser.CreatedBy.Should().Be(Admin.Username);
            createdUser.UpdatedAt.Should().BeCloseTo(ts, TimeSpan.FromSeconds(5));
            createdUser.UpdatedBy.Should().Be(Admin.Username);
            createdUser.Roles.Should().ContainSingle()
                .Which.Name.Should().Be(newRole.Name);
        }

        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var request = new GetUserRolesQuery { UserId = userId };
            var roles = await mediator.Send(request);
            roles.Should().ContainSingle()
                .Which.Name.Should().Be(newRole.Name);
        }
    }

    [Fact]
    public async Task CreateUser_WithMismatchedPasswords_ShouldThrowBadRequestException()
    {
        using var scope = Service.CreateScope();
        var command = new CreateUserCommand {
            Username = "newuser",
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User",
            Password = "password123!",
            ConfirmPassword = "password123",
            User = Admin
        };
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        UserId? userId = null;
        Func<Task> action = async () => userId = await mediator.Send(command);
        await action.Should().ThrowAsync<BadRequestException>();

        userId.Should().BeNull();
    }

    [Fact]
    public async Task CreateUser_WithExistingEmail_ShouldThrowConflictException()
    {
        using var scope = Service.CreateScope();
        var email = "newuser@example.com";
        var command = new CreateUserCommand {
            Username = "newuser1",
            Email = email,
            FirstName = "New1",
            LastName = "User1",
            Password = "password123",
            ConfirmPassword = "password123",
            User = Admin
        };

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        Func<Task> action = async () => await mediator.Send(command);
        await action.Should().NotThrowAsync();

        // Second attempt with same email
        action = async () => await mediator.Send(command);
        (await action.Should().ThrowAsync<BadRequestException>())
            .Which.Errors.Should().HaveCountGreaterThan(0);

        using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var users = await dbContext.Users.Where(x => x.Email == EmailAddress.FromString(email)).ToListAsync();
        Assert.Single(users);
    }
}
