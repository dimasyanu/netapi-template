using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Roles;
using NetApi.Application.Roles.Commands;
using NetApi.Application.Roles.Queries;
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
            // Todo: conf.RegisterServicesFromAssemblyContaining<CreateRoleCommandHandler>();
            conf.RegisterServicesFromAssemblyContaining<GetRolesQueryHandler>();
            // Todo: conf.RegisterServicesFromAssemblyContaining<UpdateRoleCommandHandler>();
            // Todo: conf.RegisterServicesFromAssemblyContaining<SoftDeleteRoleCommandHandler>();
            // Todo: conf.RegisterServicesFromAssemblyContaining<DeleteRoleCommandHandler>();
        });
    }

    [Fact]
    public async Task ListRoles_ShouldReturnCreatedRole()
    {
        var roleName = string.Concat("TestRole_", Guid.NewGuid().ToString("N").AsSpan(0, 8));

        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Create Role
            var createCommand = new CreateRoleCommand {
                Name = roleName,
                Description = "A role created during integration testing."
            };
            var createdRole = await mediator.Send(createCommand);

            // Verify Role Exists in DB
            using (var innerScope = Service.CreateScope()) {
                var dbContext = innerScope.ServiceProvider.GetRequiredService<AppDbContext>();
                var roleInDb = await dbContext.Roles.FindAsync(createdRole.Id);
                roleInDb.Should().NotBeNull();
                roleInDb.Name.Should().Be(roleName);
            }

            // List Roles
            var getRolesQuery = new GetRolesQuery();
            var roles = await mediator.Send(getRolesQuery);
            roles.Should().NotBeNull()
                .And.HaveCount(2)
                .And.Contain(x => x.Name == roleName);
        }
    }
}
