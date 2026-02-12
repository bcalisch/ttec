namespace GeoOps.Api.Contracts.Observations;

public record ObservationResponse(
    Guid Id,
    Guid ProjectId,
    DateTimeOffset Timestamp,
    double Longitude,
    double Latitude,
    string Note,
    string? Tags
);
