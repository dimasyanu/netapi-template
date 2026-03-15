using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Approvals.Commands;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Approval;

public class CreateApprovalItemTest(ITestOutputHelper outputHelper) : BaseIntegrationTest(outputHelper)
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssemblyContaining<CreateApprovalCommand>();
        });
    }

}
