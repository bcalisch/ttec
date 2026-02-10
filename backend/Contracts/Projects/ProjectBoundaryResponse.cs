namespace Backend.Api.Contracts.Projects;

public record ProjectBoundaryResponse(
    Guid Id,
    Guid ProjectId,
    string GeoJson
);
