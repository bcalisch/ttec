namespace GeoOps.Api.Models;

public class SensorReading
{
    public Guid Id { get; set; }
    public Guid SensorId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public decimal Value { get; set; }

    public Sensor Sensor { get; set; } = default!;
}
