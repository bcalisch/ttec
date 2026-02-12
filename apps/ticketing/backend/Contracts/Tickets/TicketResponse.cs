using Ticketing.Api.Models;

namespace Ticketing.Api.Contracts.Tickets;

public record TicketResponse(
    Guid Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    TicketCategory Category,
    string ReportedBy,
    string? AssignedTo,
    double? Longitude,
    double? Latitude,
    Guid? EquipmentId,
    string? SourceApp,
    string? SourceEntityType,
    Guid? SourceEntityId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset? SlaDeadline,
    bool IsOverdue
);
