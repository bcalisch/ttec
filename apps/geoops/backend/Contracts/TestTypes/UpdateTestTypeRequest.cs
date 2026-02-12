namespace GeoOps.Api.Contracts.TestTypes;

public record UpdateTestTypeRequest(
    string Name,
    string Unit,
    decimal? MinThreshold,
    decimal? MaxThreshold,
    string? MetadataJson
);
