using System.Net;
using System.Net.Http.Json;
using Ticketing.Api.Contracts.Billing;
using Ticketing.Api.Contracts.Tickets;
using Ticketing.Api.Contracts.TimeEntries;
using Ticketing.Api.Models;
using FluentAssertions;

namespace Ticketing.Api.Tests;

public class BillingControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BillingControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateTestTicket(string title = "Billing test ticket")
    {
        var request = new CreateTicketRequest(
            title, "Test", TicketPriority.Low, TicketCategory.Software,
            null, null, null, null, null, null, null);
        var response = await _client.PostAsJsonAsync("/api/tickets", request);
        var ticket = await response.Content.ReadFromJsonAsync<TicketDto>();
        return ticket!.Id;
    }

    [Fact]
    public async Task GetTicketBilling_BaseChargeOnly()
    {
        var ticketId = await CreateTestTicket();

        var response = await _client.GetAsync($"/api/billing/tickets/{ticketId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var billing = await response.Content.ReadFromJsonAsync<TicketBillingResponse>();
        billing.Should().NotBeNull();
        billing!.BaseCharge.Should().Be(200m);
        billing.HourlyTotal.Should().Be(0m);
        billing.TotalCharge.Should().Be(200m);
        billing.TimeEntries.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTicketBilling_BaseChargeAndHours()
    {
        var ticketId = await CreateTestTicket();
        await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/time-entries",
            new CreateTimeEntryRequest(2m, "On-site work"));
        await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/time-entries",
            new CreateTimeEntryRequest(1.5m, "Remote diagnostics"));

        var response = await _client.GetAsync($"/api/billing/tickets/{ticketId}");
        var billing = await response.Content.ReadFromJsonAsync<TicketBillingResponse>();

        billing!.BaseCharge.Should().Be(200m);
        billing.HourlyTotal.Should().Be(3.5m * 250m); // 875
        billing.TotalCharge.Should().Be(200m + 875m);
        billing.TimeEntries.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTicketBilling_NotFound()
    {
        var response = await _client.GetAsync($"/api/billing/tickets/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBillingSummary_Aggregates()
    {
        var ticketId1 = await CreateTestTicket("Summary ticket 1");
        var ticketId2 = await CreateTestTicket("Summary ticket 2");
        await _client.PostAsJsonAsync($"/api/tickets/{ticketId1}/time-entries",
            new CreateTimeEntryRequest(1m, "Work"));

        var response = await _client.GetAsync("/api/billing/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<BillingSummaryResponse>();
        summary.Should().NotBeNull();
        summary!.TicketCount.Should().BeGreaterOrEqualTo(2);
        summary.GrandTotal.Should().Be(summary.TotalBaseCharges + summary.TotalHourlyCharges);
    }

    private record TicketDto(Guid Id, string Title);
}
