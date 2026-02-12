namespace GeoOps.Api.Models;

public class ProjectMembership
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;

    public Project Project { get; set; } = default!;
    public User User { get; set; } = default!;
}
