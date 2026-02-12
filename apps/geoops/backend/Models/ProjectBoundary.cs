using NetTopologySuite.Geometries;

namespace GeoOps.Api.Models;

public class ProjectBoundary
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Geometry Polygon { get; set; } = default!;

    public Project Project { get; set; } = default!;
}
