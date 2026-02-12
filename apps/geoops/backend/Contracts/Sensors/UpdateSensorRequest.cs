namespace GeoOps.Api.Contracts.Sensors;

public record UpdateSensorRequest(
    string Type,
    double Longitude,
    double Latitude,
    string? MetadataJson
);
