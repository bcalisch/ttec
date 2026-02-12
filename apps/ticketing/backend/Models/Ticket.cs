using NetTopologySuite.Geometries;

namespace Ticketing.Api.Models;

public class Ticket
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketCategory Category { get; set; } = TicketCategory.Other;
    public string ReportedBy { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public Point? Location { get; set; }
    public Guid? EquipmentId { get; set; }

    // Generic external reference — opaque metadata
    public string? SourceApp { get; set; }
    public string? SourceEntityType { get; set; }
    public Guid? SourceEntityId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public DateTimeOffset? SlaDeadline { get; set; }

    public Equipment? Equipment { get; set; }
    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}
