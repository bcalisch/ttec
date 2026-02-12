using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Ticketing.Api.Tests;

public class HealthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOk_WithServiceName()
    {
        var response = await _client.GetAsync("/api/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<HealthDto>();
        body.Should().NotBeNull();
        body!.Status.Should().Be("healthy");
        body.Service.Should().Be("ticketing-api");
    }

    private record HealthDto(string Status, string Service);
}
