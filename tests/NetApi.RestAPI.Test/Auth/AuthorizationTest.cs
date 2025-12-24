using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetApi.Application.Auth.Commands;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Roles;
using NetApi.Application.Users;
using NetApi.Domain.Auth.Models;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;
using NetApi.Models;

namespace NetApi.RestAPI.Test.Auth;

public class AuthorizationTest : IClassFixture<TestingApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly IServiceProvider _serviceProvider;

    public AuthorizationTest(TestingApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _serviceProvider = factory.Services;
    }

    [Fact]
    public async Task CheckEndpoint_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/v1/Auth/Check");

        // Act
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CheckEndpoint_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/v1/Auth/Check");
        request.Headers.Add("Authorization", "Bearer InvalidToken");

        // Act
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CheckEndpoint_WithValidToken_ReturnsOk()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var hashingSvc = scope.ServiceProvider.GetRequiredService<IHashingService>();
        var roleRepo = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var role = new RoleEntity {
            Name = "admin",
            Description = "Administrator role",
            CreatedAt = DateTime.Now
        };
        var roleId = await roleRepo.CreateAsync(role);
        role.Id = roleId;

        var password = "Admin@123";
        var user = new UserEntity {
            Username = "admin",
            Email = EmailAddress.FromString("user@mail.com"),
            PasswordHash = hashingSvc.HashPassword(password),
            FirstName = "Admin User",
            LastName = "Admin",
            Roles = [role]
        };
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        await userRepo.CreateAsync(user);

        // Login
        var loginReq = await _client.PostAsJsonAsync("/v1/Auth/Login", new LoginCommand {
            EmailAddress = user.Email.ToString(),
            Password = password
        });
        var t = await loginReq.Content.ReadAsStringAsync();
        var loginResp = await loginReq.Content.ReadFromJsonAsync<Res<LoginResult>>();
        loginResp.Should().NotBeNull();
        loginResp.Success.Should().BeTrue();
        loginResp.Data.Should().NotBeNull();

        var request = new HttpRequestMessage(HttpMethod.Get, "/v1/Auth/Check");
        request.Headers.Add("Authorization", "Bearer " + loginResp.Data.AccessToken);

        // Act
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        request = new HttpRequestMessage(HttpMethod.Get, "/v1/Auth/Check");
        request.Headers.Add("Authorization", "Bearer " + loginResp.Data.AccessToken + "_");
        response = await _client.SendAsync(request);
        content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
