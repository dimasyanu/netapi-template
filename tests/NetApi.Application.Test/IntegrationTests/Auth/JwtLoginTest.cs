using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Auth;
using NetApi.Application.Auth.Commands;
using NetApi.Application.Common.Exceptions;
using NetApi.Domain.Auth.Models;
using NetApi.Infrastructure.Persistence.Models;
using NetApi.Infrastructure.Persistence.Services;
using Xunit.Abstractions;

namespace NetApi.Application.Test.IntegrationTests.Auth;

public class JwtLoginTest(ITestOutputHelper output) : BaseIntegrationTest(output)
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddScoped<IAuthService, JwtAuthService>();
        services.AddSingleton(new AppSettings {
            Jwt = new JwtSettings {
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                SecretKey = "TestSuperSecretKey1234567890qwertyuiopasdfghjkl", // Minimum 16 characters for HMACSHA256
                AccessTokenExpirationMinutes = 60,
                AuthKeyLengthInBytes = 32,
                RefreshTokenValidityInDays = 7
            }
        });

        services.AddMediatR(conf => {
            conf.RegisterServicesFromAssemblyContaining<LoginCommandHandler>();
        });
    }

    [Fact]
    public async Task Should_Fail_Login_With_Invalid_Credentials()
    {
        // Arrange
        var email = "invalid@example.com";
        var password = "wrongpassword";

        // Act
        var command = new LoginCommand {
            EmailAddress = email,
            Password = password
        };

        // Assert
        using var scope = Service.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        LoginResult? result = null;
        Func<Task<LoginResult>> action = async () => result = await mediator.Send(command);
        await action.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Should_Login_With_Valid_Credentials()
    {
        // Arrange
        var email = Admin.EmailAddress;
        var password = AdminPassword;

        // Act
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token; // 10 seconds timeout
        var command = new LoginCommand {
            EmailAddress = email.ToString(),
            Password = password
        };

        using var scope = Service.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        LoginResult? result = null;
        Func<Task<LoginResult>> action = async () => result = await mediator.Send(command);
        await action.Should().NotThrowAsync();

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty()
            .And.Subject.Length.Should().BeGreaterThan(10);
        result.RefreshToken.Should().NotBeNullOrEmpty()
            .And.Subject.Length.Should().BeGreaterThan(5);
    }
}
