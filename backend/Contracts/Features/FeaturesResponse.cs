namespace Backend.Api.Contracts.Features;

public record TestResultFeature(
    Guid Id,
    Guid TestTypeId,
    string TestTypeName,
    string Unit,
    DateTimeOffset Timestamp,
    decimal Value,
    string Status,
    double Longitude,
    double Latitude
);

public record ObservationFeature(
    Guid Id,
    DateTimeOffset Timestamp,
    string Note,
    string? Tags,
    double Longitude,
    double Latitude
);

public record SensorFeature(
    Guid Id,
    string Type,
    double Longitude,
    double Latitude
);

public record FeaturesResponse(
    IReadOnlyList<TestResultFeature> Tests,
    IReadOnlyList<ObservationFeature> Observations,
    IReadOnlyList<SensorFeature> Sensors
);
