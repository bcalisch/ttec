namespace Ticketing.Api.Models;

public class TicketComment
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Ticket Ticket { get; set; } = null!;
}
