namespace Ticketing.Api.Models;

public class TimeEntry
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Technician { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public decimal HourlyRate { get; set; } = 250m;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public Ticket Ticket { get; set; } = null!;
}
