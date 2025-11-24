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

namespace NetApi.Application.Test.IntegrationTests.Users;

public class UserCreationTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IHashingService, HashingService>();
    }

    [Fact]
    public async Task GetUserById_ShouldSucceed()
    {
        // Arrange
        var userRepository = GetService<IUserRepository>();
        var newUser = new User {
            Username = "testuser",
            Email = EmailAddress.Create("testuser@example.com"),
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hashedpassword",
        };
        var userId = await userRepository.CreateAsync(newUser);

        // Act
        var mediator = GetService<IMediator>();
        var request = new GetUserByIdQuery(userId.ToGuid());
        var user = await mediator.Send(request);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
        Assert.Equal(newUser.Username, user.Username);
        Assert.Equal(newUser.Email, user.Email);
        Assert.Equal(newUser.FirstName, user.FirstName);
        Assert.Equal(newUser.LastName, user.LastName);
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
            ConfirmPassword = "password123"
        };
        var mediator = GetService<IMediator>();
        var userId = await mediator.Send(command);
        Assert.NotEqual(Guid.Empty, userId);

        using var dbContext = GetService<AppDbContext>();
        var createdUser = await dbContext.Users.FindAsync(UserId.Create(userId));

        Assert.NotNull(createdUser);
        Assert.Equal(command.Username, createdUser!.Username);
        Assert.Equal(command.Email, createdUser.Email.ToString());
        Assert.Equal(command.FirstName, createdUser.FirstName);
        Assert.Equal(command.LastName, createdUser.LastName);
        Assert.NotEqual(command.Password, createdUser.PasswordHash); // Assuming password is hashed
    }

    [Fact]
    public async Task CreateUser_WithMismatchedPasswords_ShouldThrowBadRequestException()
    {
        var command = new CreateUserCommand {
            Username = "newuser",
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User",
            Password = "password123!",
            ConfirmPassword = "password123"
        };
        var mediator = GetService<IMediator>();
        var userId = Guid.Empty;
        async Task action() => userId = await mediator.Send(command);
        await Assert.ThrowsAsync<BadRequestException>(action);

        Assert.Equal(Guid.Empty, userId);
    }
}
