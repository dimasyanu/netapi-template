using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Users.Queries;
using NetApi.Domain.Common.Models;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Users;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Infrastructure.Persistence;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Users;

public class UserListTest(ITestOutputHelper outputHelper) : BaseIntegrationTest(outputHelper)
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddMediatR(conf => {
            conf.RegisterServicesFromAssemblyContaining<GetUserByIdQueryHandler>();
        });
    }
    [Fact]
    public async Task GetUserList_Success()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;
        var count = 50;
        await PrepareUsersAsync(count, cancellationToken);

        var query = new GetUsersQuery();
        using var scope = Service.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        Paginated<User> result = new();
        Func<Task> action = async () => result = await mediator.Send(query, cancellationToken);
        await action.Should().NotThrowAsync();
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(25);
        result.Total.Should().Be(count + 1); // +1 admin
    }

    private async Task PrepareUsersAsync(int count, CancellationToken cancellationToken = default)
    {

        using var scope = Service.CreateScope();
        var role = new RoleEntity {
            Name = "member",
            CreatedAt = DateTime.Now,
            CreatedBy = Admin.EmailAddress.ToString(),
            UpdatedAt = DateTime.Now,
            UpdatedBy = Admin.EmailAddress.ToString(),
        };
        using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Roles.AddAsync(role, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var newUsers = new List<UserEntity>();
        for (var i = 0; i < count; i++) {
            newUsers.Add(new() {
                FirstName = "User" + (i + 1),
                EmailAddress = EmailAddress.FromString($"user{i + 1}@mail.com"),
                Username = $"user{i + 1}",
                PasswordHash = Guid.NewGuid().ToString(),
                Roles = [role],
                CreatedAt = DateTime.Now,
                CreatedBy = Admin.EmailAddress.ToString(),
                UpdatedAt = DateTime.Now,
                UpdatedBy = Admin.EmailAddress.ToString(),
            });
        }
        await dbContext.Users.AddRangeAsync(newUsers, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
