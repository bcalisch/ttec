namespace GeoOps.Api.Contracts.TestResults;

public record CsvImportRequest(
    string FileName,
    string BlobUri,
    string? Notes
);
