using System.Net;
using System.Net.Http.Json;
using Backend.Api.Contracts.Features;
using Backend.Api.Contracts.Projects;
using Backend.Api.Contracts.TestResults;
using Backend.Api.Contracts.TestTypes;
using Backend.Api.Models;
using FluentAssertions;

namespace Backend.Api.Tests;

public class FeaturesEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public FeaturesEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateProjectAsync()
    {
        var request = new CreateProjectRequest("Features Test", "Client", ProjectStatus.Active, null, null);
        var response = await _client.PostAsJsonAsync("/api/projects", request);
        response.EnsureSuccessStatusCode();
        var project = await response.Content.ReadFromJsonAsync<ProjectIdDto>();
        return project!.Id;
    }

    private async Task<Guid> CreateTestTypeAsync(string name = "Density", decimal? min = 95, decimal? max = 105)
    {
        var request = new CreateTestTypeRequest(name, "pcf", min, max, null);
        var response = await _client.PostAsJsonAsync("/api/test-types", request);
        response.EnsureSuccessStatusCode();
        var testType = await response.Content.ReadFromJsonAsync<TestTypeIdDto>();
        return testType!.Id;
    }

    private async Task CreateTestResultAsync(Guid projectId, Guid testTypeId, decimal value, string? status = null, double lon = -104.93, double lat = 39.73)
    {
        var request = new CreateTestResultRequest(
            testTypeId, DateTimeOffset.UtcNow, value, lon, lat, status, null, null);
        var response = await _client.PostAsJsonAsync($"/api/projects/{projectId}/test-results", request);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetFeatures_ReturnsTests()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync();
        await CreateTestResultAsync(projectId, testTypeId, 100m);

        var response = await _client.GetAsync($"/api/projects/{projectId}/features");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var features = await response.Content.ReadFromJsonAsync<FeaturesResponse>();
        features.Should().NotBeNull();
        features!.Tests.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetFeatures_Pagination_DefaultPageSize()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync();

        // Create more results than default page size
        for (int i = 0; i < 3; i++)
        {
            await CreateTestResultAsync(projectId, testTypeId, 100m + i);
        }

        var response = await _client.GetAsync($"/api/projects/{projectId}/features?pageSize=2&page=1");
        var features = await response.Content.ReadFromJsonAsync<FeaturesResponse>();

        features.Should().NotBeNull();
        features!.Tests.Count.Should().Be(2);
        features.TotalTests.Should().Be(3);
        features.Page.Should().Be(1);
        features.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetFeatures_Pagination_Page2()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync();

        for (int i = 0; i < 3; i++)
        {
            await CreateTestResultAsync(projectId, testTypeId, 100m + i);
        }

        var response = await _client.GetAsync($"/api/projects/{projectId}/features?pageSize=2&page=2");
        var features = await response.Content.ReadFromJsonAsync<FeaturesResponse>();

        features.Should().NotBeNull();
        features!.Tests.Count.Should().Be(1); // remaining item on page 2
        features.TotalTests.Should().Be(3);
        features.Page.Should().Be(2);
    }

    [Fact]
    public async Task GetFeatures_TypeFilter_TestsOnly()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync();
        await CreateTestResultAsync(projectId, testTypeId, 100m);

        var response = await _client.GetAsync($"/api/projects/{projectId}/features?types=tests");
        var features = await response.Content.ReadFromJsonAsync<FeaturesResponse>();

        features!.Tests.Count.Should().Be(1);
        features.Observations.Count.Should().Be(0);
        features.Sensors.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetFeatures_InvalidBbox_ReturnsBadRequest()
    {
        var projectId = await CreateProjectAsync();

        var response = await _client.GetAsync($"/api/projects/{projectId}/features?bbox=invalid");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetFeatures_NonExistentProject_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/projects/{Guid.NewGuid()}/features");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetFeatures_PageSizeExceedsMax_ClampedTo200()
    {
        var projectId = await CreateProjectAsync();

        var response = await _client.GetAsync($"/api/projects/{projectId}/features?pageSize=500");
        var features = await response.Content.ReadFromJsonAsync<FeaturesResponse>();

        features!.PageSize.Should().Be(200);
    }

    private record ProjectIdDto(Guid Id);
    private record TestTypeIdDto(Guid Id);
}
