using Ticketing.Api.Models;

namespace Ticketing.Api.Contracts.Equipment;

public record UpdateEquipmentRequest(
    string Name,
    string SerialNumber,
    EquipmentType Type,
    EquipmentManufacturer Manufacturer,
    string? Model
);
