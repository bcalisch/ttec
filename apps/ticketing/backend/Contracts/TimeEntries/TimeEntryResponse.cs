namespace Ticketing.Api.Contracts.TimeEntries;

public record TimeEntryResponse(
    Guid Id,
    Guid TicketId,
    string Technician,
    decimal Hours,
    decimal HourlyRate,
    string Description,
    DateTimeOffset CreatedAt
);
