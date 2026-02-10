using System.Net;
using System.Net.Http.Json;
using Backend.Api.Contracts.Auth;
using FluentAssertions;

namespace Backend.Api.Tests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var request = new LoginRequest("admin@test.com", "password");

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        login.Should().NotBeNull();
        login!.Token.Should().NotBeNullOrEmpty();
        login.User.Should().NotBeNull();
        login.User.Email.Should().Be("admin@test.com");
        login.User.DisplayName.Should().Be("Admin User");
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var request = new LoginRequest("admin@test.com", "wrongpassword");

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_InvalidEmail_ReturnsUnauthorized()
    {
        var request = new LoginRequest("nonexistent@test.com", "password");

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_AllDevUsers_Work()
    {
        var users = new[]
        {
            ("admin@test.com", "Admin User"),
            ("tech1@test.com", "Tech One"),
            ("tech2@test.com", "Tech Two"),
        };

        foreach (var (email, displayName) in users)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "password"));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
            login!.User.Email.Should().Be(email);
            login.User.DisplayName.Should().Be(displayName);
        }
    }
}
