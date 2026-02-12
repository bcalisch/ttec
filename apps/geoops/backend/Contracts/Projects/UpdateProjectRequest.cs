using GeoOps.Api.Models;

namespace GeoOps.Api.Contracts.Projects;

public record UpdateProjectRequest(
    string Name,
    string Client,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? EndDate
);
