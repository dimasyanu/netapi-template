using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Users;
using NetApi.Application.Users.Queries;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Infrastructure.Persistence;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Users;

public class UserFetchTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddMediatR(conf => {
            conf.RegisterServicesFromAssemblyContaining<GetUserByIdQueryHandler>();
            conf.RegisterServicesFromAssemblyContaining<GetUserByEmailAddressQueryHandler>();
        });
    }

    [Fact]
    public async Task GetUserById_ShouldSucceed()
    {
        var initialUser = await PrepareUser();
        if (initialUser == null || initialUser.Id == null) throw new OperationCanceledException("Failed to generate initial user.");

        // Act
        using var scope = Service.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var request = new GetUserByIdQuery(initialUser.Id.ToGuid());
        var user = await mediator.Send(request);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().Be(initialUser.Id);
        user.Username.Should().Be(initialUser.Username);
        user.EmailAddress.Should().Be(initialUser.EmailAddress);
        user.FirstName.Should().Be(initialUser.FirstName);
        user.LastName.Should().Be(initialUser.LastName);
        user.Roles.Should().ContainSingle()
            .Which.Name.Should().Be(initialUser.Roles![0].Name);
    }

    [Fact]
    public async Task GetByEmailAddress_Success()
    {
        var initialUser = await PrepareUser();
        if (initialUser == null || initialUser.Id == null) throw new OperationCanceledException("Failed to generate initial user.");

        // Act
        using var scope = Service.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var request = new GetUserByEmailAddressQuery(initialUser.EmailAddress.ToString());
        var user = await mediator.Send(request);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().Be(initialUser.Id);
        user.Username.Should().Be(initialUser.Username);
        user.EmailAddress.Should().Be(initialUser.EmailAddress);
        user.FirstName.Should().Be(initialUser.FirstName);
        user.LastName.Should().Be(initialUser.LastName);
        user.Roles.Should().ContainSingle()
            .Which.Name.Should().Be(initialUser.Roles![0].Name);
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
