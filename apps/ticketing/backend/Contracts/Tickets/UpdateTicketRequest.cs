using Ticketing.Api.Models;

namespace Ticketing.Api.Contracts.Tickets;

public record UpdateTicketRequest(
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    TicketCategory Category,
    string? AssignedTo,
    double? Longitude,
    double? Latitude,
    Guid? EquipmentId
);
