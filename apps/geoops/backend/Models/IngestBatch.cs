namespace GeoOps.Api.Models;

public class IngestBatch
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; set; }
    public int ItemCount { get; set; }
}
