using Ticketing.Api.Contracts.Tickets;
using Ticketing.Api.Data;
using Ticketing.Api.Models;
using Ticketing.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Ticketing.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    private static readonly Dictionary<TicketPriority, TimeSpan> SlaWindows = new()
    {
        [TicketPriority.Critical] = TimeSpan.FromHours(4),
        [TicketPriority.High] = TimeSpan.FromHours(8),
        [TicketPriority.Medium] = TimeSpan.FromHours(24),
        [TicketPriority.Low] = TimeSpan.FromHours(72),
    };

    public TicketsController(AppDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketResponse>>> GetTickets(
        [FromQuery] string? sourceApp,
        [FromQuery] Guid? sourceEntityId,
        [FromQuery] TicketStatus? status,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Tickets.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(sourceApp))
            query = query.Where(x => x.SourceApp == sourceApp);

        if (sourceEntityId.HasValue)
            query = query.Where(x => x.SourceEntityId == sourceEntityId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var tickets = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(tickets.Select(MapToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketResponse>> GetTicket(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ticket is null)
            return NotFound();

        return Ok(MapToResponse(ticket));
    }

    [HttpPost]
    public async Task<ActionResult<TicketResponse>> CreateTicket(
        [FromBody] CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = TicketStatus.Open,
            Priority = request.Priority,
            Category = request.Category,
            ReportedBy = _currentUser.DisplayName,
            AssignedTo = request.AssignedTo,
            EquipmentId = request.EquipmentId,
            SourceApp = request.SourceApp,
            SourceEntityType = request.SourceEntityType,
            SourceEntityId = request.SourceEntityId,
            CreatedAt = now,
            UpdatedAt = now,
            SlaDeadline = now.Add(SlaWindows[request.Priority]),
        };

        if (request.Longitude.HasValue && request.Latitude.HasValue)
        {
            var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            ticket.Location = factory.CreatePoint(new Coordinate(request.Longitude.Value, request.Latitude.Value));
        }

        _dbContext.Tickets.Add(ticket);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, MapToResponse(ticket));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TicketResponse>> UpdateTicket(
        Guid id,
        [FromBody] UpdateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ticket is null)
            return NotFound();

        var oldStatus = ticket.Status;

        ticket.Title = request.Title;
        ticket.Description = request.Description;
        ticket.Status = request.Status;
        ticket.Priority = request.Priority;
        ticket.Category = request.Category;
        ticket.AssignedTo = request.AssignedTo;
        ticket.EquipmentId = request.EquipmentId;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;

        if (request.Longitude.HasValue && request.Latitude.HasValue)
        {
            var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            ticket.Location = factory.CreatePoint(new Coordinate(request.Longitude.Value, request.Latitude.Value));
        }
        else
        {
            ticket.Location = null;
        }

        // Auto-set ResolvedAt on status transitions
        if (request.Status == TicketStatus.Resolved && oldStatus != TicketStatus.Resolved)
        {
            ticket.ResolvedAt = DateTimeOffset.UtcNow;
        }
        else if (request.Status != TicketStatus.Resolved && oldStatus == TicketStatus.Resolved)
        {
            ticket.ResolvedAt = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapToResponse(ticket));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteTicket(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ticket is null)
            return NotFound();

        // Remove related entities
        _dbContext.TicketComments.RemoveRange(
            await _dbContext.TicketComments.Where(x => x.TicketId == id).ToListAsync(cancellationToken));
        _dbContext.TimeEntries.RemoveRange(
            await _dbContext.TimeEntries.Where(x => x.TicketId == id).ToListAsync(cancellationToken));

        _dbContext.Tickets.Remove(ticket);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("sla-summary")]
    public async Task<ActionResult> GetSlaSummary(CancellationToken cancellationToken)
    {
        var openTickets = await _dbContext.Tickets
            .AsNoTracking()
            .Where(x => x.Status != TicketStatus.Closed && x.Status != TicketStatus.Resolved)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var overdue = openTickets.Count(t => t.SlaDeadline.HasValue && t.SlaDeadline.Value < now);
        var atRisk = openTickets.Count(t => t.SlaDeadline.HasValue && t.SlaDeadline.Value >= now && t.SlaDeadline.Value < now.AddHours(2));
        var onTrack = openTickets.Count - overdue - atRisk;

        return Ok(new { overdue, atRisk, onTrack, total = openTickets.Count });
    }

    private static TicketResponse MapToResponse(Ticket t)
    {
        var isOverdue = t.SlaDeadline.HasValue
            && t.Status != TicketStatus.Resolved
            && t.Status != TicketStatus.Closed
            && t.SlaDeadline.Value < DateTimeOffset.UtcNow;

        return new TicketResponse(
            t.Id,
            t.Title,
            t.Description,
            t.Status,
            t.Priority,
            t.Category,
            t.ReportedBy,
            t.AssignedTo,
            t.Location?.X,
            t.Location?.Y,
            t.EquipmentId,
            t.SourceApp,
            t.SourceEntityType,
            t.SourceEntityId,
            t.CreatedAt,
            t.UpdatedAt,
            t.ResolvedAt,
            t.SlaDeadline,
            isOverdue
        );
    }
}
