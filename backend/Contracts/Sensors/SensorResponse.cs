namespace Backend.Api.Contracts.Sensors;

public record SensorResponse(
    Guid Id,
    Guid ProjectId,
    string Type,
    double Longitude,
    double Latitude,
    string? MetadataJson
);
