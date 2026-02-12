using NetTopologySuite.Geometries;

namespace GeoOps.Api.Models;

public class Sensor
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Type { get; set; } = string.Empty;
    public Point Location { get; set; } = default!;
    public string? MetadataJson { get; set; }

    public Project Project { get; set; } = default!;
    public ICollection<SensorReading> Readings { get; set; } = new List<SensorReading>();
}
