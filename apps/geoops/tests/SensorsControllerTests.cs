using System.Net;
using System.Net.Http.Json;
using GeoOps.Api.Contracts.Projects;
using GeoOps.Api.Contracts.Sensors;
using GeoOps.Api.Models;
using FluentAssertions;

namespace GeoOps.Api.Tests;

public class SensorsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SensorsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateProjectAsync()
    {
        var request = new CreateProjectRequest("Sensor Test Project", "Client", ProjectStatus.Active, null, null);
        var response = await _client.PostAsJsonAsync("/api/projects", request);
        response.EnsureSuccessStatusCode();
        var project = await response.Content.ReadFromJsonAsync<ProjectIdDto>();
        return project!.Id;
    }

    [Fact]
    public async Task CreateSensor_ReturnsDto()
    {
        var projectId = await CreateProjectAsync();
        var request = new CreateSensorRequest("Temperature", -104.93, 39.73, "{\"model\":\"T100\"}");

        var response = await _client.PostAsJsonAsync($"/api/projects/{projectId}/sensors", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sensor = await response.Content.ReadFromJsonAsync<SensorResponse>();
        sensor.Should().NotBeNull();
        sensor!.Type.Should().Be("Temperature");
        sensor.ProjectId.Should().Be(projectId);
        sensor.MetadataJson.Should().Contain("T100");
    }

    [Fact]
    public async Task GetSensors_ReturnsCreated()
    {
        var projectId = await CreateProjectAsync();
        await _client.PostAsJsonAsync($"/api/projects/{projectId}/sensors",
            new CreateSensorRequest("Strain", -104.93, 39.73, null));

        var response = await _client.GetAsync($"/api/projects/{projectId}/sensors");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sensors = await response.Content.ReadFromJsonAsync<List<SensorResponse>>();
        sensors.Should().NotBeNull();
        sensors!.Count.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task UpdateSensor_Success()
    {
        var projectId = await CreateProjectAsync();
        var createResponse = await _client.PostAsJsonAsync($"/api/projects/{projectId}/sensors",
            new CreateSensorRequest("Old Type", -104.93, 39.73, null));
        var created = await createResponse.Content.ReadFromJsonAsync<SensorResponse>();

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/projects/{projectId}/sensors/{created!.Id}",
            new UpdateSensorRequest("New Type", -104.95, 39.75, "{\"updated\":true}"));

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<SensorResponse>();
        updated!.Type.Should().Be("New Type");
    }

    [Fact]
    public async Task DeleteSensor_Success()
    {
        var projectId = await CreateProjectAsync();
        var createResponse = await _client.PostAsJsonAsync($"/api/projects/{projectId}/sensors",
            new CreateSensorRequest("To Delete", -104.93, 39.73, null));
        var created = await createResponse.Content.ReadFromJsonAsync<SensorResponse>();

        var deleteResponse = await _client.DeleteAsync(
            $"/api/projects/{projectId}/sensors/{created!.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CreateSensorReading_ReturnsDto()
    {
        var projectId = await CreateProjectAsync();
        var sensorResponse = await _client.PostAsJsonAsync($"/api/projects/{projectId}/sensors",
            new CreateSensorRequest("Pressure", -104.93, 39.73, null));
        var sensor = await sensorResponse.Content.ReadFromJsonAsync<SensorResponse>();

        var readingRequest = new CreateSensorReadingRequest(DateTimeOffset.UtcNow, 42.5m);
        var response = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/sensors/{sensor!.Id}/readings", readingRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reading = await response.Content.ReadFromJsonAsync<SensorReadingResponse>();
        reading.Should().NotBeNull();
        reading!.Value.Should().Be(42.5m);
        reading.SensorId.Should().Be(sensor.Id);
    }

    [Fact]
    public async Task GetSensorReadings_ReturnsCreated()
    {
        var projectId = await CreateProjectAsync();
        var sensorResponse = await _client.PostAsJsonAsync($"/api/projects/{projectId}/sensors",
            new CreateSensorRequest("Vibration", -104.93, 39.73, null));
        var sensor = await sensorResponse.Content.ReadFromJsonAsync<SensorResponse>();

        await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/sensors/{sensor!.Id}/readings",
            new CreateSensorReadingRequest(DateTimeOffset.UtcNow, 10m));

        var response = await _client.GetAsync(
            $"/api/projects/{projectId}/sensors/{sensor.Id}/readings");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var readings = await response.Content.ReadFromJsonAsync<List<SensorReadingResponse>>();
        readings.Should().NotBeNull();
        readings!.Count.Should().Be(1);
    }

    private record ProjectIdDto(Guid Id);
}
