using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Users;
using NetApi.Application.Users.Commands;
using NetApi.Application.Users.Queries;
using NetApi.Domain.Repositories;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Infrastructure.Persistence;
using NetApi.Infrastructure.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Users;

public class UserModificationTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddMediatR(conf => conf.RegisterServicesFromAssemblyContaining<UpdateUserCommandHandler>());
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

    [Fact]
    public async Task ChangeEmail_ShouldSucceed()
    {
    }
}
