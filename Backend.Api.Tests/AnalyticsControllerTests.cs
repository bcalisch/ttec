using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Api.Contracts.Projects;
using Backend.Api.Contracts.TestResults;
using Backend.Api.Contracts.TestTypes;
using Backend.Api.Models;
using FluentAssertions;

namespace Backend.Api.Tests;

public class AnalyticsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AnalyticsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateProjectAsync()
    {
        var request = new CreateProjectRequest("Analytics Test", "Client", ProjectStatus.Active, null, null);
        var response = await _client.PostAsJsonAsync("/api/projects", request);
        response.EnsureSuccessStatusCode();
        var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    private async Task<Guid> CreateTestTypeAsync(string name = "Density", decimal? min = 95m, decimal? max = 105m)
    {
        var request = new CreateTestTypeRequest(name, "pcf", min, max, null);
        var response = await _client.PostAsJsonAsync("/api/test-types", request);
        response.EnsureSuccessStatusCode();
        var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    private async Task CreateTestResultAsync(Guid projectId, Guid testTypeId, decimal value, double lon = -104.93, double lat = 39.73)
    {
        var request = new CreateTestResultRequest(testTypeId, DateTimeOffset.UtcNow, value, lon, lat, null, null, null);
        var response = await _client.PostAsJsonAsync($"/api/projects/{projectId}/test-results", request);
        response.EnsureSuccessStatusCode();
    }

    private async Task CreateBoundaryAsync(Guid projectId, double minLon, double minLat, double maxLon, double maxLat)
    {
        var geoJson = $$"""
        {
          "type": "Polygon",
          "coordinates": [[
            [{{minLon}}, {{minLat}}],
            [{{maxLon}}, {{minLat}}],
            [{{maxLon}}, {{maxLat}}],
            [{{minLon}}, {{maxLat}}],
            [{{minLon}}, {{minLat}}]
          ]]
        }
        """;
        var request = new CreateProjectBoundaryRequest(geoJson);
        var response = await _client.PostAsJsonAsync($"/api/projects/{projectId}/boundaries", request);
        response.EnsureSuccessStatusCode();
    }

    // --- Out-of-spec tests ---

    [Fact]
    public async Task GetOutOfSpec_ReturnsEmpty_WhenAllResultsInSpec()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync(); // min=95, max=105
        await CreateTestResultAsync(projectId, testTypeId, 100m); // within range

        var response = await _client.GetAsync($"/api/projects/{projectId}/analytics/out-of-spec");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<OutOfSpecDto>>(JsonOptions);
        items.Should().NotBeNull();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOutOfSpec_ReturnsSeverity3_WhenFarBelowMin()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync("Compaction", 95m, 105m);
        // 70 is 26.3% below 95 → severity 3 (High)
        await CreateTestResultAsync(projectId, testTypeId, 70m);

        var response = await _client.GetAsync($"/api/projects/{projectId}/analytics/out-of-spec");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<OutOfSpecDto>>(JsonOptions);
        items.Should().HaveCount(1);
        items![0].Severity.Should().Be(3);
        items[0].Threshold.Should().Be(95m);
        items[0].TestTypeName.Should().Be("Compaction");
    }

    [Fact]
    public async Task GetOutOfSpec_ReturnsSeverity2_WhenModeratelyAboveMax()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync("Moisture", 10m, 20m);
        // 23 is 15% above 20 → severity 2 (Medium)
        await CreateTestResultAsync(projectId, testTypeId, 23m);

        var response = await _client.GetAsync($"/api/projects/{projectId}/analytics/out-of-spec");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<OutOfSpecDto>>(JsonOptions);
        items.Should().HaveCount(1);
        items![0].Severity.Should().Be(2);
        items[0].Threshold.Should().Be(20m);
    }

    [Fact]
    public async Task GetOutOfSpec_ReturnsSeverity1_WhenSlightlyOutOfSpec()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync("pH", 6m, 8m);
        // 8.5 is 6.25% above 8 → severity 1 (Low)
        await CreateTestResultAsync(projectId, testTypeId, 8.5m);

        var response = await _client.GetAsync($"/api/projects/{projectId}/analytics/out-of-spec");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<OutOfSpecDto>>(JsonOptions);
        items.Should().HaveCount(1);
        items![0].Severity.Should().Be(1);
        items[0].Threshold.Should().Be(8m);
    }

    [Fact]
    public async Task GetOutOfSpec_ReturnsCorrectCoordinates()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync("Temp", 0m, 50m);
        await CreateTestResultAsync(projectId, testTypeId, 60m, -105.5, 40.2);

        var response = await _client.GetAsync($"/api/projects/{projectId}/analytics/out-of-spec");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<OutOfSpecDto>>(JsonOptions);
        items.Should().HaveCount(1);
        items![0].Longitude.Should().BeApproximately(-105.5, 0.01);
        items[0].Latitude.Should().BeApproximately(40.2, 0.01);
    }

    // --- Coverage tests ---

    [Fact]
    public async Task GetCoverage_ReturnsMessage_WhenNoBoundaries()
    {
        var projectId = await CreateProjectAsync();

        var response = await _client.GetAsync($"/api/projects/{projectId}/analytics/coverage");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        doc.RootElement.GetProperty("cells").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task GetCoverage_ReturnsGridCells_WithBoundary()
    {
        var projectId = await CreateProjectAsync();
        await CreateBoundaryAsync(projectId, -105.0, 39.0, -104.0, 40.0);
        var testTypeId = await CreateTestTypeAsync("CoverageDensity", 90m, 110m);
        await CreateTestResultAsync(projectId, testTypeId, 100m, -104.5, 39.5);

        var response = await _client.GetAsync($"/api/projects/{projectId}/analytics/coverage");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = doc.RootElement;
        root.GetProperty("gridSize").GetInt32().Should().Be(10);
        root.GetProperty("cells").GetArrayLength().Should().Be(100); // 10x10 grid
        root.GetProperty("gaps").GetInt32().Should().BeGreaterOrEqualTo(0);

        // At least one cell should have count > 0
        var cells = root.GetProperty("cells");
        var hasNonEmpty = false;
        foreach (var cell in cells.EnumerateArray())
        {
            if (cell.GetProperty("count").GetInt32() > 0)
            {
                hasNonEmpty = true;
                break;
            }
        }
        hasNonEmpty.Should().BeTrue("at least one cell should contain a test result");
    }

    // --- Trends tests ---

    [Fact]
    public async Task GetTrends_ReturnsEmpty_WhenNoResults()
    {
        var projectId = await CreateProjectAsync();

        var response = await _client.GetAsync($"/api/projects/{projectId}/analytics/trends");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<TrendDto>>(JsonOptions);
        items.Should().NotBeNull();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTrends_ReturnsAggregatedData()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync("TrendDensity", 90m, 110m);
        await CreateTestResultAsync(projectId, testTypeId, 100m);
        await CreateTestResultAsync(projectId, testTypeId, 102m);

        var response = await _client.GetAsync($"/api/projects/{projectId}/analytics/trends?interval=day");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<TrendDto>>(JsonOptions);
        items.Should().NotBeEmpty();
        items![0].Avg.Should().BeGreaterThan(0);
        items[0].Count.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task GetTrends_SupportsWeekInterval()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync("WeekTrend", 90m, 110m);
        await CreateTestResultAsync(projectId, testTypeId, 100m);

        var response = await _client.GetAsync($"/api/projects/{projectId}/analytics/trends?interval=week");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<TrendDto>>(JsonOptions);
        items.Should().NotBeEmpty();
    }

    private record OutOfSpecDto(
        Guid Id,
        string TestTypeName,
        decimal Value,
        decimal Threshold,
        int Severity,
        double Longitude,
        double Latitude,
        DateTimeOffset Timestamp);

    private record TrendDto(
        string Period,
        Guid TestTypeId,
        string TestTypeName,
        decimal Avg,
        decimal Min,
        decimal Max,
        int Count);
}
