using System.Net;
using System.Net.Http.Json;
using Backend.Api.Contracts.Projects;
using Backend.Api.Contracts.TestResults;
using Backend.Api.Contracts.TestTypes;
using Backend.Api.Models;
using FluentAssertions;

namespace Backend.Api.Tests;

public class TestResultsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TestResultsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateProjectAsync()
    {
        var request = new CreateProjectRequest("TR Test Project", "Client", ProjectStatus.Active, null, null);
        var response = await _client.PostAsJsonAsync("/api/projects", request);
        response.EnsureSuccessStatusCode();
        var project = await response.Content.ReadFromJsonAsync<ProjectIdDto>();
        return project!.Id;
    }

    private async Task<Guid> CreateTestTypeAsync()
    {
        var request = new CreateTestTypeRequest("Density", "pcf", 95m, 105m, null);
        var response = await _client.PostAsJsonAsync("/api/test-types", request);
        response.EnsureSuccessStatusCode();
        var testType = await response.Content.ReadFromJsonAsync<TestTypeIdDto>();
        return testType!.Id;
    }

    [Fact]
    public async Task CreateTestResult_ReturnsDto_NotRawEntity()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync();

        var request = new CreateTestResultRequest(
            testTypeId,
            DateTimeOffset.UtcNow,
            100.5m,
            -104.93, 39.73,
            "Pass",
            "Field Test",
            "Tech 1");

        var response = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/test-results", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TestResultResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.ProjectId.Should().Be(projectId);
        result.TestTypeId.Should().Be(testTypeId);
        result.Value.Should().Be(100.5m);
        result.Status.Should().Be("Pass");
        result.Longitude.Should().BeApproximately(-104.93, 0.01);
        result.Latitude.Should().BeApproximately(39.73, 0.01);
        result.Source.Should().Be("Field Test");
        result.Technician.Should().Be("Tech 1");
    }

    [Fact]
    public async Task CreateTestResult_WithAutoStatus_Pass()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync(); // min=95, max=105

        var request = new CreateTestResultRequest(
            testTypeId,
            DateTimeOffset.UtcNow,
            100m, // within range
            -104.93, 39.73,
            null, // auto-resolve status
            null, null);

        var response = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/test-results", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TestResultResponse>();
        result!.Status.Should().Be("Pass");
    }

    [Fact]
    public async Task CreateTestResult_WithAutoStatus_Fail_BelowMin()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync(); // min=95, max=105

        var request = new CreateTestResultRequest(
            testTypeId,
            DateTimeOffset.UtcNow,
            80m, // below min
            -104.93, 39.73,
            null, null, null);

        var response = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/test-results", request);

        var result = await response.Content.ReadFromJsonAsync<TestResultResponse>();
        result!.Status.Should().Be("Fail");
    }

    [Fact]
    public async Task CreateTestResult_WithAutoStatus_Fail_AboveMax()
    {
        var projectId = await CreateProjectAsync();
        var testTypeId = await CreateTestTypeAsync(); // min=95, max=105

        var request = new CreateTestResultRequest(
            testTypeId,
            DateTimeOffset.UtcNow,
            120m, // above max
            -104.93, 39.73,
            null, null, null);

        var response = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/test-results", request);

        var result = await response.Content.ReadFromJsonAsync<TestResultResponse>();
        result!.Status.Should().Be("Fail");
    }

    [Fact]
    public async Task CreateTestResult_WithInvalidTestType_ReturnsBadRequest()
    {
        var projectId = await CreateProjectAsync();

        var request = new CreateTestResultRequest(
            Guid.NewGuid(), // nonexistent
            DateTimeOffset.UtcNow,
            100m,
            -104.93, 39.73,
            null, null, null);

        var response = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/test-results", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTestResult_WithInvalidProject_ReturnsNotFound()
    {
        var testTypeId = await CreateTestTypeAsync();

        var request = new CreateTestResultRequest(
            testTypeId,
            DateTimeOffset.UtcNow,
            100m,
            -104.93, 39.73,
            null, null, null);

        var response = await _client.PostAsJsonAsync(
            $"/api/projects/{Guid.NewGuid()}/test-results", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private record ProjectIdDto(Guid Id);
    private record TestTypeIdDto(Guid Id);
}
