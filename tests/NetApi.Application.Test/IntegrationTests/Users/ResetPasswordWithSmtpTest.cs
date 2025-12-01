using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Test.Mocks;
using NetApi.Application.Users;
using NetApi.Application.Users.Commands;
using NetApi.Domain.Repositories;
using NetApi.Domain.Settings;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Infrastructure.Persistence;
using NetApi.Infrastructure.Persistence.Repositories;
using NetApi.Infrastructure.Persistence.Services;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.AdoJobStore;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Users;

public class ResetPasswordWithSmtpTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    private const string _schedulerName = "NetApiQuartzScheduler_TestResetPasswordWithSmtp";

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSingleton<IHashingService, HashingService>();
        services.AddSingleton<IJobService, QuartzJobService>();
        services.AddSingleton<IMailService, SmtpMailService>();
        services.AddScoped<DummyMailtrapClient>(); // Added DummyMailTrapClient for testing purposes
        services.AddSingleton<IEmailTemplateManager, DummyEmailTemplateManager>();
        services.AddMediatR(conf => {
            conf.RegisterServicesFromAssemblyContaining<ResetPasswordCommandHandler>();
        });
        services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: false)
            .Build();

        // Todo: Add serialization option

        services.AddSingleton(config.Get<AppSetting>() ?? throw new InvalidConfigurationException("AppSetting is not configured"));
        services.AddSingleton(config);

        services.AddQuartz(opt => {
            opt.SchedulerId = _schedulerName;
            opt.SchedulerName = _schedulerName;
            opt.UseInMemoryStore();
            opt.UseDefaultThreadPool(tp => tp.MaxConcurrency = 5);
        });
        services.AddQuartzHostedService(opt => {
            opt.WaitForJobsToComplete = true;
        });
    }

    public override void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
    }


    [Fact]
    public async Task ResetPassword_ShouldSendEmailUsingSmtp()
    {
        // Start job service
        using (var scope = Service.CreateScope()) {
            var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
            await jobService.StartAsync();

            // Clean Mailtrap inbox
            var mailService = scope.ServiceProvider.GetRequiredService<DummyMailtrapClient>();
            await mailService.CleanInboxAsync();
        }

        // Send reset password request
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var resetPasswordRequest = new ResetPasswordCommand { Email = Admin.Email, User = Admin };
            var email = await mediator.Send(resetPasswordRequest);
            email.Should().NotBeNull().And.Be(Admin.Email);

            // Wait for job to be processed
            var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
            var jobs = await jobService.GetQueuedJobsAsync();
            jobs.Should().HaveCount(1);
            await jobs[0].WaitForCompletionAsync(); // Wait for the "email" to be "sent"
            // await Task.Delay(10000); 

            // Verify email sent
            var mailService = scope.ServiceProvider.GetRequiredService<DummyMailtrapClient>();
            (await mailService.GetMessagesAsync()).Should().HaveCount(1);

            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordResetEntities = dbContext.PasswordResets.ToList();
            passwordResetEntities.Should().HaveCount(1);
            passwordResetEntities[0].Id.Should().NotBe(PasswordResetId.Empty);
            passwordResetEntities[0].UsedAt.Should().BeNull();
        }
    }
}
