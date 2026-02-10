using Backend.Api.Models;

namespace Backend.Api.Contracts.Projects;

public record CreateProjectRequest(
    string Name,
    string Client,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? EndDate
);
