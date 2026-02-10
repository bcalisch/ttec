using Backend.Api.Models;

namespace Backend.Api.Contracts.Projects;

public record UpdateProjectRequest(
    string Name,
    string Client,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? EndDate
);
