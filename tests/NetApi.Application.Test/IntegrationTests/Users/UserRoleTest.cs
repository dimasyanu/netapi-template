using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Users;

public class UserRoleTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    override protected void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddMediatR(conf => {
            // Todo: conf.RegisterServicesFromAssemblyContaining<AssignRolesToUserCommandHandler>();
            // Todo: conf.RegisterServicesFromAssemblyContaining<GetUserRolesQueryHandler>();
            // Todo: conf.RegisterServicesFromAssemblyContaining<RemoveRolesFromUserCommandHandler>();
        });
    }
}
