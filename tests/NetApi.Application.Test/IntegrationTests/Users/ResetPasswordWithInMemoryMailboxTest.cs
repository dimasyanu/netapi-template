using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;
using NetApi.Application.Test.Mocks;
using NetApi.Application.Users;
using NetApi.Application.Users.Commands;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Infrastructure.Persistence;
using NetApi.Infrastructure.Persistence.Repositories;
using NetApi.Infrastructure.Persistence.Services;
using Quartz;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Users;

public class ResetPasswordWithInMemoryMailboxTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    private const string _schedulerName = "QuartzScheduler_InMemoryMailbox";

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddQuartz(opt => {
            opt.SchedulerId = _schedulerName;
            opt.SchedulerName = _schedulerName;
            opt.UseInMemoryStore();
            opt.UseDefaultThreadPool(tp => tp.MaxConcurrency = 5);
        });
        services.AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);

        services.AddSingleton<IJobService, QuartzJobService>();
        services.AddSingleton<IMailService, DummyMailService>();
        services.AddSingleton<IEmailTemplateManager, DummyEmailTemplateManager>();
        services.AddSingleton<DummyMailInboxClient>(); // Register the dummy mail inbox client for simulate retrieving emails
        services.AddMediatR(conf => {
            conf.RegisterServicesFromAssemblyContaining<ResetPasswordCommandHandler>();
        });
        services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();

    }

    [Fact]
    public async Task ResetUserPassword_UsingInMemoryMailbox_Succeeds()
    {
        // Start job service
        using (var scope = Service.CreateScope()) {
            var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
            await jobService.StartAsync();
        }

        var initialPasswordHash = "";

        // Request reset admin password
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var resetPasswordRequest = new ResetPasswordCommand { Email = Admin.EmailAddress, User = Admin };
            var email = await mediator.Send(resetPasswordRequest);
            email.Should().NotBeNull().And.Be(Admin.EmailAddress);
            await Task.Delay(100); // Wait for the "email" to be "sent"

            // Verify email sent
            var mailService = scope.ServiceProvider.GetRequiredService<DummyMailInboxClient>();
            (await mailService.GetInboxAsync(Admin.EmailAddress)).Should().HaveCount(1);

            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordResetEntities = dbContext.PasswordResets.ToList();
            passwordResetEntities.Should().HaveCount(1);
            passwordResetEntities[0].Id.Should().NotBe(PasswordResetId.Empty);
            passwordResetEntities[0].UsedAt.Should().BeNull();

            var admin = await dbContext.Users.FindAsync(Admin.Id);
            admin.Should().NotBeNull();
            initialPasswordHash = admin!.PasswordHash;
        }

        const string newPassword = "NewP@ssw0rd!";
        // Proceed reset password with wrong token
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var resetPasswordRequest = new ProceedPasswordResetCommand {
                Token = "invalid-token",
                ConfirmPassword = newPassword,
                NewPassword = newPassword
            };
            Func<Task> act = async () => { await mediator.Send(resetPasswordRequest); };
            await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Invalid or expired password reset token.");
        }

        // Proceed reset password with valid token
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordResetEntity = dbContext.PasswordResets.First();

            var result = false;
            var resetPasswordRequest = new ProceedPasswordResetCommand {
                Token = passwordResetEntity.Token,
                ConfirmPassword = "123",
                NewPassword = "123"
            };

            // Short password
            Func<Task> action = async () => result = await mediator.Send(resetPasswordRequest);
            await action.Should().ThrowAsync<BadRequestException>();

            // All-letter characters
            resetPasswordRequest.ConfirmPassword = "Testing";
            resetPasswordRequest.NewPassword = "Testing";
            action = async () => result = await mediator.Send(resetPasswordRequest);
            await action.Should().ThrowAsync<BadRequestException>();

            // All lowercase characters
            resetPasswordRequest.ConfirmPassword = "testing";
            resetPasswordRequest.NewPassword = "testing";
            action = async () => result = await mediator.Send(resetPasswordRequest);
            await action.Should().ThrowAsync<BadRequestException>();

            // All lowercase characters
            resetPasswordRequest.ConfirmPassword = newPassword;
            resetPasswordRequest.NewPassword = newPassword + "_";
            action = async () => result = await mediator.Send(resetPasswordRequest);
            await action.Should().ThrowAsync<BadRequestException>();

            // Valid password
            resetPasswordRequest.ConfirmPassword = newPassword;
            resetPasswordRequest.NewPassword = newPassword;
            action = async () => result = await mediator.Send(resetPasswordRequest);
            await action.Should().NotThrowAsync();
            result.Should().BeTrue();

            // Verify password updated and reset marked as used
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var updatedAdmin = await userRepo.GetByIdAsync(Admin.Id!);
            updatedAdmin!.PasswordHash.Should().NotBe(initialPasswordHash);

            var updatedResetEntry = await dbContext.PasswordResets.FindAsync(passwordResetEntity.Id);
            updatedResetEntry!.UsedAt.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ValidatePasswordResetAttempt_InvalidToken()
    {
        using var scope = Service.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var cmd = new ValidatePasswordResetAttemptCommand { Token = "an_invalid_token" };
        Func<Task> action = async () => await mediator.Send(cmd);
        await action.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ValidatePasswordResetAttempt_ExpiredToken()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;

        // Start job service
        using (var scope = Service.CreateScope()) {
            var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
            await jobService.StartAsync();
        }
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var resetPasswordRequest = new ResetPasswordCommand { Email = Admin.EmailAddress, User = Admin };
            await mediator.Send(resetPasswordRequest);
        }

        var token = "";
        using (var scope = Service.CreateScope()) {
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordReset = dbContext.PasswordResets.Single();
            passwordReset.ExpiresAt = DateTime.Now.AddMinutes(-1);
            await dbContext.SaveChangesAsync(cancellationToken);
            token = passwordReset.Token;
        }

        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var cmd = new ValidatePasswordResetAttemptCommand { Token = token };
            Func<Task> action = async () => await mediator.Send(cmd);
            await action.Should().ThrowAsync<BadRequestException>();
        }
    }

    [Fact]
    public async Task ValidatePasswordResetAttempt_Success()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;

        // Start job service
        using (var scope = Service.CreateScope()) {
            var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
            await jobService.StartAsync();
        }
        using (var scope = Service.CreateScope()) {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var resetPasswordRequest = new ResetPasswordCommand { Email = Admin.EmailAddress, User = Admin };
            await mediator.Send(resetPasswordRequest, cancellationToken);
        }

        using (var scope = Service.CreateScope()) {
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordReset = dbContext.PasswordResets.Single();

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var cmd = new ValidatePasswordResetAttemptCommand { Token = passwordReset.Token };
            Func<Task> action = async () => await mediator.Send(cmd);
            await action.Should().NotThrowAsync();
        }
    }
}
