using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Test.Mocks;
using NetApi.Application.Users;
using NetApi.Application.Users.Commands;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Infrastructure.Persistence;
using NetApi.Infrastructure.Persistence.Repositories;
using NetApi.Infrastructure.Persistence.Services;
using Quartz;
using Quartz.Impl;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Users;

public class ResetPasswordTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSingleton<IHashingService, HashingService>();
        services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
        services.AddSingleton<IJobService, DummyJobService>();
        services.AddSingleton<IMailService, DummyMailService>();
        services.AddSingleton<IEmailTemplateManager, DummyEmailTemplateManager>();
        services.AddSingleton<DummyMailInboxClient>(); // Register the dummy mail inbox client for simulate retrieving emails
        services.AddMediatR(conf => {
            conf.RegisterServicesFromAssemblyContaining<ResetPasswordCommandHandler>();
        });
        services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();

        services.AddQuartz(opt => {
            opt.UseInMemoryStore();
            opt.UseDefaultThreadPool(tp => tp.MaxConcurrency = 5);
        });
        services.AddQuartzHostedService(opt => {
            opt.AwaitApplicationStarted = false;
            opt.WaitForJobsToComplete = true;
            opt.StartDelay = null;
        });
    }

    [Fact]
    public async Task ResetUserPassword_Succeeds()
    {
        // Start job service
        using (var scope = Service.CreateScope()) {
            var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
            await jobService.StartAsync();
        }

        // Request reset admin password
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var resetPasswordRequest = new ResetPasswordCommand { Email = Admin.Email, User = Admin };
            var email = await mediator.Send(resetPasswordRequest);
            email.Should().NotBeNull().And.Be(Admin.Email);
            await Task.Delay(100); // Wait for the "email" to be "sent"

            var mailService = scope.ServiceProvider.GetRequiredService<DummyMailInboxClient>();
            (await mailService.GetInboxAsync(Admin.Email)).Should().HaveCount(1);

            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordResetEntities = dbContext.PasswordResets.ToList();
            passwordResetEntities.Should().HaveCount(1);
            passwordResetEntities[0].Id.Should().NotBe(PasswordResetId.Empty);
        }
    }
}
