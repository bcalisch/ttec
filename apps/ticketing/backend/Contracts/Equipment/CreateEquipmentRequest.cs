using Ticketing.Api.Models;

namespace Ticketing.Api.Contracts.Equipment;

public record CreateEquipmentRequest(
    string Name,
    string SerialNumber,
    EquipmentType Type,
    EquipmentManufacturer Manufacturer,
    string? Model
);
