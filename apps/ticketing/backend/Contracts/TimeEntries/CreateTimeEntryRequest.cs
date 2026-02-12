namespace Ticketing.Api.Contracts.TimeEntries;

public record CreateTimeEntryRequest(
    decimal Hours,
    string Description
);
