namespace GeoOps.Api.Contracts.Sensors;

public record CreateSensorRequest(
    string Type,
    double Longitude,
    double Latitude,
    string? MetadataJson
);
