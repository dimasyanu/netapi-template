using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Test.Mocks;
using NetApi.Application.Users;
using NetApi.Application.Users.Commands;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Infrastructure.Persistence;
using NetApi.Infrastructure.Persistence.Models;
using NetApi.Infrastructure.Persistence.Repositories;
using NetApi.Infrastructure.Persistence.Services;
using Quartz;
using Quartz.Impl.AdoJobStore;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Users;

public class ResetPasswordWithSmtpTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    private const string _schedulerName = "NetApiQuartzScheduler_TestResetPasswordWithSmtp";

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSingleton<IJobService, QuartzJobService>();
        services.AddSingleton<IMailService, SmtpMailService>();
        services.AddScoped<DummyMailtrapClient>(); // Added DummyMailTrapClient for testing purposes
        services.AddSingleton<IEmailTemplateManager, DummyEmailTemplateManager>();
        services.AddMediatR(conf => {
            conf.RegisterServicesFromAssemblyContaining<ResetPasswordCommandHandler>();
        });
        services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();

        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: false)
            .Build();

        // Todo: Add serialization option

        services.AddSingleton(config.Get<AppSettings>() ?? throw new InvalidConfigurationException("AppSetting is not configured"));
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


    [Fact(Skip = "Requires Mailtrap account credentials to run.")]
    public async Task ResetPassword_ShouldSendEmailUsingSmtp()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token; // Timeout after 30 seconds

        // Start job service
        using (var scope = Service.CreateScope()) {
            var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
            await jobService.StartAsync(cancellationToken);

            // Clean Mailtrap inbox
            var mailService = scope.ServiceProvider.GetRequiredService<DummyMailtrapClient>();
            await mailService.CleanInboxAsync(cancellationToken);
        }

        // Send reset password request
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var resetPasswordRequest = new ResetPasswordCommand { Email = Admin.EmailAddress, User = Admin };
            var email = await mediator.Send(resetPasswordRequest, cancellationToken);
            email.Should().NotBeNull().And.Be(Admin.EmailAddress);

            // Wait for job to be processed
            var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
            var jobs = await jobService.GetQueuedJobsAsync(cancellationToken);
            jobs.Should().HaveCount(1);

            await jobs[0].WaitForCompletionAsync(cancellationToken); // Wait for the "email" to be "sent"

            // Verify email sent
            var mailService = scope.ServiceProvider.GetRequiredService<DummyMailtrapClient>();
            (await mailService.GetMessagesAsync(cancellationToken)).Should().HaveCount(1);

            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordResetEntities = dbContext.PasswordResets.ToList();
            passwordResetEntities.Should().HaveCount(1);
            passwordResetEntities[0].Id.Should().NotBe(PasswordResetId.Empty);
            passwordResetEntities[0].UsedAt.Should().BeNull();
        }
    }
}
