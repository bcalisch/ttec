namespace GeoOps.Api.Contracts.TestResults;

public record CreateTestResultRequest(
    Guid TestTypeId,
    DateTimeOffset Timestamp,
    decimal Value,
    double Longitude,
    double Latitude,
    string? Status,
    string? Source,
    string? Technician
);
