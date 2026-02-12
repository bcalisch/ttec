namespace Ticketing.Api.Models;

public class Equipment
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public EquipmentType Type { get; set; }
    public EquipmentManufacturer Manufacturer { get; set; }
    public string? Model { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
