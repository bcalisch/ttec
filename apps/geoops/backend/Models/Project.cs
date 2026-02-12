namespace GeoOps.Api.Models;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Client { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public ICollection<ProjectBoundary> Boundaries { get; set; } = new List<ProjectBoundary>();
    public ICollection<ProjectMembership> Memberships { get; set; } = new List<ProjectMembership>();
}
