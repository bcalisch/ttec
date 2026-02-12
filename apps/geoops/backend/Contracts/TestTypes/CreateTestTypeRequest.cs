namespace GeoOps.Api.Contracts.TestTypes;

public record CreateTestTypeRequest(
    string Name,
    string Unit,
    decimal? MinThreshold,
    decimal? MaxThreshold,
    string? MetadataJson
);
