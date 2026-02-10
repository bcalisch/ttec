namespace Backend.Api.Contracts.Sensors;

public record CreateSensorReadingRequest(
    DateTimeOffset Timestamp,
    decimal Value
);
