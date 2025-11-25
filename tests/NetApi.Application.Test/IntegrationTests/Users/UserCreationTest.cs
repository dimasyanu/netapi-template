using Microsoft.Extensions.DependencyInjection;
using NetApi.Domain.Repositories;
using NetApi.Application.Users;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Application.Users.Queries;
using MediatR;
using NetApi.Domain.Users;
using Xunit.Abstractions;
using NetApi.Application.Users.Commands;
using NetApi.Infrastructure.Persistence;
using NetApi.Application.Common.Contracts;
using NetApi.Infrastructure.Persistence.Services;
using NetApi.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace NetApi.Application.Test.IntegrationTests.Users;

public class UserCreationTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IHashingService, HashingService>();

        services.AddMediatR(conf => conf.RegisterServicesFromAssemblyContaining<GetUserByIdQueryHandler>());
        services.AddMediatR(conf => conf.RegisterServicesFromAssemblyContaining<CreateUserCommandHandler>());
        services.AddMediatR(conf => conf.RegisterServicesFromAssemblyContaining<UpdateUserCommandHandler>());
    }

    [Fact]
    public async Task GetUserById_ShouldSucceed()
    {
        // Arrange
        using var scope = Service.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var newUser = new User {
            Username = "testuser",
            Email = EmailAddress.FromString("testuser@example.com"),
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hashedpassword",
        };
        var userId = await userRepository.CreateAsync(newUser);

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
    }

    [Fact]
    public async Task CreateUser_ShouldSucceed()
    {
        var command = new CreateUserCommand {
            Username = "newuser",
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User",
            Password = "password123",
            ConfirmPassword = "password123",
            User = Admin
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
            var createdUser = await dbContext.Users.FindAsync(userId);

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

    [Fact]
    public async Task UpdateUser_ShouldSucceed()
    {
        UserId? userId = null;
        User? user = null;
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

        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Arrange
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            userId = await mediator.Send(command);

            user = await mediator.Send(new GetUserByIdQuery(userId.ToGuid()));

            await Task.Delay(250); // Ensure timestamp difference

            // Act
            var updateCommand = new UpdateUserCommand {
                UserId = userId,
                FirstName = "newuser1_1",
                LastName = "User1_1",
                User = user,
            };
            Func<Task> action = async () => user = await mediator.Send(updateCommand);
            await action.Should().NotThrowAsync();
        }

        using (var scope = Service.CreateScope()) {
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var users = await dbContext.Users.ToListAsync();
            var updatedUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            updatedUser.Should().NotBeNull();
            updatedUser.FirstName.Should().Be("newuser1_1");
            updatedUser.LastName.Should().Be("User1_1");
            updatedUser.UpdatedAt.Should().NotBe(updatedUser.CreatedAt).And.BeAfter(updatedUser.CreatedAt);
            updatedUser.CreatedBy.Should().Be(Admin.Username);
            updatedUser.UpdatedBy.Should().Be(user.Username);
        }
    }
}
