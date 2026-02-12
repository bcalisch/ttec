using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GeoOps.Api.Contracts.Projects;
using GeoOps.Api.Contracts.TestResults;
using GeoOps.Api.Contracts.TestTypes;
using GeoOps.Api.Models;
using FluentAssertions;

namespace GeoOps.Api.Tests;

public class ExportControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExportControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateProjectAsync()
    {
        var request = new CreateProjectRequest("Export Test", "Client", ProjectStatus.Active, null, null);
        var response = await _client.PostAsJsonAsync("/api/projects", request);
        response.EnsureSuccessStatusCode();
        var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    private async Task<Guid> CreateTestTypeAsync(string name = "ExportDensity")
    {
        var request = new CreateTestTypeRequest(name, "pcf", 95m, 105m, null);
        var response = await _client.PostAsJsonAsync("/api/test-types", request);
        response.EnsureSuccessStatusCode();
        var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    private async Task CreateTestResultAsync(Guid projectId, Guid testTypeId, decimal value, string? status = null, double lon = -104.93, double lat = 39.73)
    {
        var request = new CreateTestResultRequest(testTypeId, DateTimeOffset.UtcNow, value, lon, lat, status, "Field", "Tech");
        var response = await _client.PostAsJsonAsync($"/api/projects/{projectId}/test-results", request);
        response.EnsureSuccessStatusCode();
    }

    // --- CSV Export tests ---

    [Fact]
    public async Task ExportCsv_ReturnsFile_WithResults()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync("CsvDensity");
        await CreateTestResultAsync(projectId, testTypeId, 100m);

        var response = await _client.GetAsync($"/api/projects/{projectId}/export/csv");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Id,ProjectId,TestTypeId,TestTypeName");
        content.Should().Contain("CsvDensity");
    }

    [Fact]
    public async Task ExportCsv_ReturnsEmpty_WhenNoResults()
    {
        var projectId = await CreateProjectAsync();

        var response = await _client.GetAsync($"/api/projects/{projectId}/export/csv");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(1, "only the header line should be present");
    }

    [Fact]
    public async Task ExportCsv_FiltersBy_TestTypeId()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId1 = await CreateTestTypeAsync("TypeA");
        var testTypeId2 = await CreateTestTypeAsync("TypeB");
        await CreateTestResultAsync(projectId, testTypeId1, 100m);
        await CreateTestResultAsync(projectId, testTypeId2, 200m);

        var response = await _client.GetAsync($"/api/projects/{projectId}/export/csv?testTypeId={testTypeId1}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("TypeA");
        content.Should().NotContain("TypeB");
    }

    [Fact]
    public async Task ExportCsv_FiltersBy_CommaSeparatedStatus()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync("StatusFilter");
        await CreateTestResultAsync(projectId, testTypeId, 100m, "Pass");
        await CreateTestResultAsync(projectId, testTypeId, 80m, "Fail");
        await CreateTestResultAsync(projectId, testTypeId, 94m, "Warn");

        var response = await _client.GetAsync($"/api/projects/{projectId}/export/csv?status=Pass,Fail");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Pass");
        content.Should().Contain("Fail");
        content.Should().NotContain(",Warn,");
    }

    [Fact]
    public async Task ExportCsv_InvalidBbox_ReturnsBadRequest()
    {
        var projectId = await CreateProjectAsync();

        var response = await _client.GetAsync($"/api/projects/{projectId}/export/csv?bbox=invalid");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- GeoJSON Export tests ---

    [Fact]
    public async Task ExportGeoJson_ReturnsFeatureCollection()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync("GeoDensity");
        await CreateTestResultAsync(projectId, testTypeId, 100m, null, -104.5, 39.5);

        var response = await _client.GetAsync($"/api/projects/{projectId}/export/geojson");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/geo+json");
        var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = doc.RootElement;
        root.GetProperty("type").GetString().Should().Be("FeatureCollection");
        root.GetProperty("features").GetArrayLength().Should().BeGreaterOrEqualTo(1);

        var feature = root.GetProperty("features")[0];
        feature.GetProperty("type").GetString().Should().Be("Feature");
        feature.GetProperty("geometry").GetProperty("type").GetString().Should().Be("Point");
        feature.GetProperty("properties").GetProperty("testTypeName").GetString().Should().Be("GeoDensity");
    }

    [Fact]
    public async Task ExportGeoJson_ReturnsEmpty_WhenNoResults()
    {
        var projectId = await CreateProjectAsync();

        var response = await _client.GetAsync($"/api/projects/{projectId}/export/geojson");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        doc.RootElement.GetProperty("features").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task ExportGeoJson_FiltersBy_CommaSeparatedStatus()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync("GeoStatusFilter");
        await CreateTestResultAsync(projectId, testTypeId, 100m, "Pass");
        await CreateTestResultAsync(projectId, testTypeId, 80m, "Fail");

        var response = await _client.GetAsync($"/api/projects/{projectId}/export/geojson?status=Fail");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var features = doc.RootElement.GetProperty("features");
        features.GetArrayLength().Should().Be(1);
        features[0].GetProperty("properties").GetProperty("status").GetString().Should().Be("Fail");
    }

    [Fact]
    public async Task ExportGeoJson_FiltersBy_DateRange()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync("DateFilter");
        await CreateTestResultAsync(projectId, testTypeId, 100m);

        var from = Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(-1).ToString("o"));
        var to = Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(1).ToString("o"));

        var response = await _client.GetAsync($"/api/projects/{projectId}/export/geojson?from={from}&to={to}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        doc.RootElement.GetProperty("features").GetArrayLength().Should().BeGreaterOrEqualTo(1);
    }
}
