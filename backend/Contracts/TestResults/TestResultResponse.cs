namespace Backend.Api.Contracts.TestResults;

public record TestResultResponse(
    Guid Id,
    Guid ProjectId,
    Guid TestTypeId,
    DateTimeOffset Timestamp,
    decimal Value,
    string Status,
    double Longitude,
    double Latitude,
    string Source,
    string Technician
);
