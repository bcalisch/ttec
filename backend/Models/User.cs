namespace Backend.Api.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public ICollection<ProjectMembership> Memberships { get; set; } = new List<ProjectMembership>();
}
