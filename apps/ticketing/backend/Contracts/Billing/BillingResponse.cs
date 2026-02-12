namespace Ticketing.Api.Contracts.Billing;

public record TicketBillingResponse(
    Guid TicketId,
    string Title,
    decimal BaseCharge,
    decimal HourlyTotal,
    decimal TotalCharge,
    IReadOnlyList<TimeEntryLineItem> TimeEntries
);

public record TimeEntryLineItem(
    Guid Id,
    decimal Hours,
    decimal HourlyRate,
    decimal LineTotal,
    string Description
);

public record BillingSummaryResponse(
    decimal TotalBaseCharges,
    decimal TotalHourlyCharges,
    decimal GrandTotal,
    int TicketCount,
    IReadOnlyList<TicketBillingResponse> Tickets
);
