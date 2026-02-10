using NetTopologySuite.Geometries;

namespace Backend.Api.Models;

public class Observation
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Point Location { get; set; } = default!;
    public DateTimeOffset Timestamp { get; set; }
    public string Note { get; set; } = string.Empty;
    public string? Tags { get; set; }

    public Project Project { get; set; } = default!;
}
