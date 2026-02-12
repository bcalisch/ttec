namespace GeoOps.Api.Contracts.Projects;

public record ProjectBoundaryResponse(
    Guid Id,
    Guid ProjectId,
    string GeoJson
);
