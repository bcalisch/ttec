namespace Backend.Api.Contracts.Observations;

public record UpdateObservationRequest(
    DateTimeOffset Timestamp,
    double Longitude,
    double Latitude,
    string Note,
    string? Tags
);
