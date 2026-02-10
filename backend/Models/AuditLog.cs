namespace Backend.Api.Models;

public class AuditLog
{
    public Guid Id { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? MetadataJson { get; set; }
}
