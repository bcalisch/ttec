namespace GeoOps.Api.Contracts.Sensors;

public record SensorReadingResponse(
    Guid Id,
    Guid SensorId,
    DateTimeOffset Timestamp,
    decimal Value
);
