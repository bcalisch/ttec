using System.Net;
using System.Net.Http.Json;
using GeoOps.Api.Contracts.Observations;
using GeoOps.Api.Contracts.Projects;
using GeoOps.Api.Models;
using FluentAssertions;

namespace GeoOps.Api.Tests;

public class ObservationsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ObservationsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateProjectAsync()
    {
        var request = new CreateProjectRequest("Obs Test Project", "Client", ProjectStatus.Active, null, null);
        var response = await _client.PostAsJsonAsync("/api/projects", request);
        response.EnsureSuccessStatusCode();
        var project = await response.Content.ReadFromJsonAsync<ProjectIdDto>();
        return project!.Id;
    }

    [Fact]
    public async Task CreateObservation_ReturnsDto()
    {
        var projectId = await CreateProjectAsync();
        var request = new CreateObservationRequest(
            DateTimeOffset.UtcNow,
            -104.93, 39.73,
            "Test observation",
            "tag1,tag2");

        var response = await _client.PostAsJsonAsync($"/api/projects/{projectId}/observations", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var obs = await response.Content.ReadFromJsonAsync<ObservationResponse>();
        obs.Should().NotBeNull();
        obs!.Note.Should().Be("Test observation");
        obs.Longitude.Should().BeApproximately(-104.93, 0.01);
        obs.Latitude.Should().BeApproximately(39.73, 0.01);
        obs.Tags.Should().Be("tag1,tag2");
        obs.ProjectId.Should().Be(projectId);
    }

    [Fact]
    public async Task GetObservations_ReturnsCreated()
    {
        var projectId = await CreateProjectAsync();
        var request = new CreateObservationRequest(
            DateTimeOffset.UtcNow, -104.93, 39.73, "Listed observation", null);
        await _client.PostAsJsonAsync($"/api/projects/{projectId}/observations", request);

        var response = await _client.GetAsync($"/api/projects/{projectId}/observations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var observations = await response.Content.ReadFromJsonAsync<List<ObservationResponse>>();
        observations.Should().NotBeNull();
        observations!.Count.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task UpdateObservation_Success()
    {
        var projectId = await CreateProjectAsync();
        var createRequest = new CreateObservationRequest(
            DateTimeOffset.UtcNow, -104.93, 39.73, "Original", null);
        var createResponse = await _client.PostAsJsonAsync($"/api/projects/{projectId}/observations", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ObservationResponse>();

        var updateRequest = new UpdateObservationRequest(
            DateTimeOffset.UtcNow, -104.95, 39.75, "Updated", "updated-tag");
        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/projects/{projectId}/observations/{created!.Id}", updateRequest);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ObservationResponse>();
        updated!.Note.Should().Be("Updated");
        updated.Tags.Should().Be("updated-tag");
    }

    [Fact]
    public async Task DeleteObservation_Success()
    {
        var projectId = await CreateProjectAsync();
        var createRequest = new CreateObservationRequest(
            DateTimeOffset.UtcNow, -104.93, 39.73, "To delete", null);
        var createResponse = await _client.PostAsJsonAsync($"/api/projects/{projectId}/observations", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ObservationResponse>();

        var deleteResponse = await _client.DeleteAsync(
            $"/api/projects/{projectId}/observations/{created!.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetObservation_ReturnsNotFound_WhenProjectNotExists()
    {
        var response = await _client.GetAsync($"/api/projects/{Guid.NewGuid()}/observations");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private record ProjectIdDto(Guid Id);
}
