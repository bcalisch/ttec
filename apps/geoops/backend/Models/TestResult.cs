using NetTopologySuite.Geometries;

namespace GeoOps.Api.Models;

public class TestResult
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid TestTypeId { get; set; }
    public Point Location { get; set; } = default!;
    public DateTimeOffset Timestamp { get; set; }
    public decimal Value { get; set; }
    public TestStatus Status { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Technician { get; set; } = string.Empty;

    public Project Project { get; set; } = default!;
    public TestType TestType { get; set; } = default!;
}
