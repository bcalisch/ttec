namespace Backend.Api.Contracts.Observations;

public record CreateObservationRequest(
    DateTimeOffset Timestamp,
    double Longitude,
    double Latitude,
    string Note,
    string? Tags
);
