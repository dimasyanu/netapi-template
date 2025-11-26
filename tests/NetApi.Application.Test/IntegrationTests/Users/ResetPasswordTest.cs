using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Test.Mocks;
using NetApi.Application.Users.Commands;
using NetApi.Infrastructure.Persistence.Services;
using Quartz;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Users;

public class ResetPasswordTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSingleton<IHashingService, HashingService>();
        services.AddSingleton<IJobService, DummyJobService>();
        services.AddSingleton<IMailService, DummyMailService>();
        services.AddSingleton<DummyMailInboxClient>(); // Register the dummy mail inbox client for simulate retrieving emails
        services.AddMediatR(conf => {
            conf.RegisterServicesFromAssemblyContaining<ResetPasswordCommandHandler>();
        });

        services.AddQuartzHostedService(options => {
            options.WaitForJobsToComplete = true;
        });
    }

    [Fact]
    public async Task ResetUserPassword_Succeeds()
    {
        // Request reset admin password
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var resetPasswordRequest = new ResetPasswordCommand { Email = Admin.Email, User = Admin };
            var email = await mediator.Send(resetPasswordRequest);
            email.Should().NotBeNull().And.Be(Admin.Email);
            await Task.Delay(250); // Wait for the "email" to be "sent"

            var mailService = scope.ServiceProvider.GetRequiredService<DummyMailInboxClient>();
            (await mailService.GetInboxAsync(Admin.Email)).Should().HaveCount(1);
        }

    }
}