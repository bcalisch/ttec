using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ticketing.Api.Contracts.Tickets;
using Ticketing.Api.Models;
using FluentAssertions;

namespace Ticketing.Api.Tests;

public class TicketsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public TicketsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTickets_ReturnsEmptyList_WhenNoTickets()
    {
        var response = await _client.GetAsync("/api/tickets");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tickets = await response.Content.ReadFromJsonAsync<List<TicketDto>>(JsonOpts);
        tickets.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTicket_ReturnsCreated()
    {
        var request = new CreateTicketRequest(
            "IC roller sensor malfunction",
            "BOMAG BW 226 CMV sensor returning zero values",
            TicketPriority.High,
            TicketCategory.Hardware,
            null, null, null, null, null, null, null);

        var response = await _client.PostAsJsonAsync("/api/tickets", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var ticket = await response.Content.ReadFromJsonAsync<TicketDto>(JsonOpts);
        ticket.Should().NotBeNull();
        ticket!.Title.Should().Be("IC roller sensor malfunction");
        ticket.Status.Should().Be(TicketStatus.Open);
        ticket.Priority.Should().Be(TicketPriority.High);
        ticket.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateTicket_WithSourceReference_RoundTrips()
    {
        var sourceId = Guid.NewGuid();
        var request = new CreateTicketRequest(
            "Issue from GeoOps project",
            "Test results failing consistently",
            TicketPriority.Medium,
            TicketCategory.Software,
            null, null, null, null,
            "geoops", "project", sourceId);

        var createResponse = await _client.PostAsJsonAsync("/api/tickets", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<TicketDto>(JsonOpts);

        created!.SourceApp.Should().Be("geoops");
        created.SourceEntityType.Should().Be("project");
        created.SourceEntityId.Should().Be(sourceId);
    }

    [Fact]
    public async Task GetTicket_ReturnsNotFound_WhenNotExists()
    {
        var response = await _client.GetAsync($"/api/tickets/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndGetTicket_Roundtrip()
    {
        var request = new CreateTicketRequest(
            "Roundtrip test ticket",
            "Testing create and get",
            TicketPriority.Low,
            TicketCategory.Training,
            null, null, null, null, null, null, null);

        var createResponse = await _client.PostAsJsonAsync("/api/tickets", request);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<TicketDto>(JsonOpts);

        var getResponse = await _client.GetAsync($"/api/tickets/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<TicketDto>(JsonOpts);
        fetched!.Title.Should().Be("Roundtrip test ticket");
    }

    [Fact]
    public async Task UpdateTicket_Success()
    {
        var createRequest = new CreateTicketRequest(
            "Before update",
            "Original description",
            TicketPriority.Low,
            TicketCategory.Software,
            null, null, null, null, null, null, null);
        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<TicketDto>(JsonOpts);

        var updateRequest = new UpdateTicketRequest(
            "After update",
            "Updated description",
            TicketStatus.InProgress,
            TicketPriority.High,
            TicketCategory.Hardware,
            "Tech A",
            null, null, null);
        var updateResponse = await _client.PutAsJsonAsync($"/api/tickets/{created!.Id}", updateRequest);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TicketDto>(JsonOpts);
        updated!.Title.Should().Be("After update");
        updated.Status.Should().Be(TicketStatus.InProgress);
        updated.Priority.Should().Be(TicketPriority.High);
        updated.AssignedTo.Should().Be("Tech A");
    }

    [Fact]
    public async Task DeleteTicket_Success()
    {
        var request = new CreateTicketRequest(
            "To delete",
            "Will be deleted",
            TicketPriority.Low,
            TicketCategory.Other,
            null, null, null, null, null, null, null);
        var createResponse = await _client.PostAsJsonAsync("/api/tickets", request);
        var created = await createResponse.Content.ReadFromJsonAsync<TicketDto>(JsonOpts);

        var deleteResponse = await _client.DeleteAsync($"/api/tickets/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/tickets/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTicket_WithLocation()
    {
        var request = new CreateTicketRequest(
            "Field issue with location",
            "GPS-tagged ticket",
            TicketPriority.Medium,
            TicketCategory.FieldSupport,
            null, -97.495, 35.500, null, null, null, null);

        var response = await _client.PostAsJsonAsync("/api/tickets", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var ticket = await response.Content.ReadFromJsonAsync<TicketDto>(JsonOpts);
        ticket!.Longitude.Should().BeApproximately(-97.495, 0.001);
        ticket.Latitude.Should().BeApproximately(35.500, 0.001);
    }

    [Fact]
    public async Task CreateTicket_EmptyTitle_Returns400()
    {
        var request = new CreateTicketRequest(
            "",
            "Description",
            TicketPriority.Low,
            TicketCategory.Software,
            null, null, null, null, null, null, null);

        var response = await _client.PostAsJsonAsync("/api/tickets", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTickets_FilteredBySourceApp()
    {
        var sourceId = Guid.NewGuid();
        var request = new CreateTicketRequest(
            "Filtered ticket",
            "Has source ref",
            TicketPriority.Low,
            TicketCategory.Software,
            null, null, null, null,
            "test-app", "entity", sourceId);
        await _client.PostAsJsonAsync("/api/tickets", request);

        var response = await _client.GetAsync($"/api/tickets?sourceApp=test-app&sourceEntityId={sourceId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tickets = await response.Content.ReadFromJsonAsync<List<TicketDto>>(JsonOpts);
        tickets.Should().NotBeNull();
        tickets!.Should().Contain(t => t.Title == "Filtered ticket");
    }

    [Fact]
    public async Task CreateTicket_SetsSlaDeadline()
    {
        var request = new CreateTicketRequest(
            "Critical issue",
            "Needs immediate attention",
            TicketPriority.Critical,
            TicketCategory.Hardware,
            null, null, null, null, null, null, null);

        var before = DateTimeOffset.UtcNow;
        var response = await _client.PostAsJsonAsync("/api/tickets", request);
        var ticket = await response.Content.ReadFromJsonAsync<TicketDto>(JsonOpts);

        ticket!.SlaDeadline.Should().NotBeNull();
        // Critical = 4 hours SLA
        ticket.SlaDeadline!.Value.Should().BeCloseTo(before.AddHours(4), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdateTicket_ToResolved_SetsResolvedAt()
    {
        var createRequest = new CreateTicketRequest(
            "To resolve",
            "Test resolution",
            TicketPriority.Low,
            TicketCategory.Software,
            null, null, null, null, null, null, null);
        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<TicketDto>(JsonOpts);

        var updateRequest = new UpdateTicketRequest(
            "To resolve",
            "Test resolution",
            TicketStatus.Resolved,
            TicketPriority.Low,
            TicketCategory.Software,
            null, null, null, null);
        var updateResponse = await _client.PutAsJsonAsync($"/api/tickets/{created!.Id}", updateRequest);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TicketDto>(JsonOpts);

        updated!.ResolvedAt.Should().NotBeNull();
        updated.Status.Should().Be(TicketStatus.Resolved);
    }

    private record TicketDto(
        Guid Id,
        string Title,
        string Description,
        TicketStatus Status,
        TicketPriority Priority,
        TicketCategory Category,
        string ReportedBy,
        string? AssignedTo,
        double? Longitude,
        double? Latitude,
        Guid? EquipmentId,
        string? SourceApp,
        string? SourceEntityType,
        Guid? SourceEntityId,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? ResolvedAt,
        DateTimeOffset? SlaDeadline,
        bool IsOverdue);
}
