namespace GeoOps.Api.Contracts.Sensors;

public record CreateSensorReadingRequest(
    DateTimeOffset Timestamp,
    decimal Value
);
