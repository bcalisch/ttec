using System.Net;
using System.Net.Http.Json;
using GeoOps.Api.Contracts.Projects;
using GeoOps.Api.Models;
using FluentAssertions;

namespace GeoOps.Api.Tests;

public class ProjectsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProjectsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProjects_ReturnsEmptyList_WhenNoProjects()
    {
        var response = await _client.GetAsync("/api/projects");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var projects = await response.Content.ReadFromJsonAsync<List<ProjectDto>>();
        projects.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateProject_ReturnsCreated()
    {
        var request = new CreateProjectRequest("Test Project", "Test Client", ProjectStatus.Active, null, null);

        var response = await _client.PostAsJsonAsync("/api/projects", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        project.Should().NotBeNull();
        project!.Name.Should().Be("Test Project");
        project.Client.Should().Be("Test Client");
        project.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProject_ReturnsNotFound_WhenNotExists()
    {
        var response = await _client.GetAsync($"/api/projects/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndGetProject_Roundtrip()
    {
        var request = new CreateProjectRequest("Roundtrip Project", "Roundtrip Client", ProjectStatus.Draft, null, null);
        var createResponse = await _client.PostAsJsonAsync("/api/projects", request);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        var getResponse = await _client.GetAsync($"/api/projects/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<ProjectDto>();
        fetched!.Name.Should().Be("Roundtrip Project");
    }

    [Fact]
    public async Task UpdateProject_Success()
    {
        var request = new CreateProjectRequest("Before Update", "Client", ProjectStatus.Active, null, null);
        var createResponse = await _client.PostAsJsonAsync("/api/projects", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        var updateRequest = new UpdateProjectRequest("After Update", "New Client", ProjectStatus.Closed, null, null);
        var updateResponse = await _client.PutAsJsonAsync($"/api/projects/{created!.Id}", updateRequest);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ProjectDto>();
        updated!.Name.Should().Be("After Update");
        updated.Client.Should().Be("New Client");
    }

    [Fact]
    public async Task DeleteProject_Success()
    {
        var request = new CreateProjectRequest("To Delete", "Client", ProjectStatus.Active, null, null);
        var createResponse = await _client.PostAsJsonAsync("/api/projects", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        var deleteResponse = await _client.DeleteAsync($"/api/projects/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/projects/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProject_ReturnsNotFound_WhenNotExists()
    {
        var response = await _client.DeleteAsync($"/api/projects/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Simple DTO for deserialization (avoids using EF entity with Point geometry)
    private record ProjectDto(Guid Id, string Name, string Client, string Status);
}
