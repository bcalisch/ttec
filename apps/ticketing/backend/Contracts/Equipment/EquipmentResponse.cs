using Ticketing.Api.Models;

namespace Ticketing.Api.Contracts.Equipment;

public record EquipmentResponse(
    Guid Id,
    string Name,
    string SerialNumber,
    EquipmentType Type,
    EquipmentManufacturer Manufacturer,
    string? Model,
    DateTimeOffset CreatedAt
);
