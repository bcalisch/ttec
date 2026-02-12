using Ticketing.Api.Models;

namespace Ticketing.Api.Contracts.Tickets;

public record CreateTicketRequest(
    string Title,
    string Description,
    TicketPriority Priority,
    TicketCategory Category,
    string? AssignedTo,
    double? Longitude,
    double? Latitude,
    Guid? EquipmentId,
    string? SourceApp,
    string? SourceEntityType,
    Guid? SourceEntityId
);
