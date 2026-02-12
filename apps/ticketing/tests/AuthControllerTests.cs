using System.Net;
using System.Net.Http.Json;
using Ticketing.Api.Contracts.Auth;
using FluentAssertions;

namespace Ticketing.Api.Tests;

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
        var request = new LoginRequest("Benjamin", "isHired!");

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        login.Should().NotBeNull();
        login!.Token.Should().NotBeNullOrEmpty();
        login.User.Should().NotBeNull();
        login.User.Email.Should().Be("Benjamin");
        login.User.DisplayName.Should().Be("Benjamin Calisch");
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var request = new LoginRequest("Benjamin", "wrongpassword");

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_InvalidUsername_ReturnsUnauthorized()
    {
        var request = new LoginRequest("nonexistent", "isHired!");

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
