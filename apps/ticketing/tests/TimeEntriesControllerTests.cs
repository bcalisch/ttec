using System.Net;
using System.Net.Http.Json;
using Ticketing.Api.Contracts.Tickets;
using Ticketing.Api.Contracts.TimeEntries;
using Ticketing.Api.Models;
using FluentAssertions;

namespace Ticketing.Api.Tests;

public class TimeEntriesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TimeEntriesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateTestTicket()
    {
        var request = new CreateTicketRequest(
            "Ticket for time entries", "Test", TicketPriority.Low, TicketCategory.Software,
            null, null, null, null, null, null, null);
        var response = await _client.PostAsJsonAsync("/api/tickets", request);
        var ticket = await response.Content.ReadFromJsonAsync<TicketDto>();
        return ticket!.Id;
    }

    [Fact]
    public async Task CreateTimeEntry_WithDefaultRate()
    {
        var ticketId = await CreateTestTicket();
        var request = new CreateTimeEntryRequest(2.5m, "On-site calibration");

        var response = await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/time-entries", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var entry = await response.Content.ReadFromJsonAsync<TimeEntryResponse>();
        entry.Should().NotBeNull();
        entry!.Hours.Should().Be(2.5m);
        entry.HourlyRate.Should().Be(250m);
        entry.Technician.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetTimeEntries_ReturnsList()
    {
        var ticketId = await CreateTestTicket();
        await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/time-entries",
            new CreateTimeEntryRequest(1m, "First entry"));
        await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/time-entries",
            new CreateTimeEntryRequest(2m, "Second entry"));

        var response = await _client.GetAsync($"/api/tickets/{ticketId}/time-entries");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var entries = await response.Content.ReadFromJsonAsync<List<TimeEntryResponse>>();
        entries.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task CreateTimeEntry_ZeroHours_Returns400()
    {
        var ticketId = await CreateTestTicket();
        var request = new CreateTimeEntryRequest(0m, "No hours");

        var response = await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/time-entries", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTimeEntry_NegativeHours_Returns400()
    {
        var ticketId = await CreateTestTicket();
        var request = new CreateTimeEntryRequest(-1m, "Negative hours");

        var response = await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/time-entries", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTimeEntry_ExceedsMaxHours_Returns400()
    {
        var ticketId = await CreateTestTicket();
        var request = new CreateTimeEntryRequest(25m, "Over 24 hours");

        var response = await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/time-entries", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private record TicketDto(Guid Id, string Title);
}
