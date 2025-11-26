using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Test.Mocks;
using NetApi.Application.Users.Commands;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Users;

public class ResetPasswordTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSingleton<IMailService, DummyMailService>();
    }

    [Fact]
    public async Task ResetUserPassword_Succeeds()
    {
        // Request reset admin password
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var resetPasswordRequest = new ResetPasswordCommand { Email = Admin.Email };
            var email = await mediator.Send(resetPasswordRequest);
            email.Should().NotBeNull().And.Be(Admin.Email);

            var mailService = scope.ServiceProvider.GetRequiredService<IMailService>();
            mailService.GetInboxAsync().Should().HaveCount(1);
        }

    }
}