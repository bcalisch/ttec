using Ticketing.Api.Contracts.TimeEntries;
using Ticketing.Api.Data;
using Ticketing.Api.Models;
using Ticketing.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ticketing.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets/{ticketId:guid}/time-entries")]
public class TimeEntriesController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public TimeEntriesController(AppDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TimeEntryResponse>>> GetTimeEntries(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        var ticketExists = await _dbContext.Tickets
            .AsNoTracking()
            .AnyAsync(x => x.Id == ticketId, cancellationToken);

        if (!ticketExists)
            return NotFound();

        var entries = await _dbContext.TimeEntries
            .AsNoTracking()
            .Where(x => x.TicketId == ticketId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new TimeEntryResponse(x.Id, x.TicketId, x.Technician, x.Hours, x.HourlyRate, x.Description, x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(entries);
    }

    [HttpPost]
    public async Task<ActionResult<TimeEntryResponse>> CreateTimeEntry(
        Guid ticketId,
        [FromBody] CreateTimeEntryRequest request,
        CancellationToken cancellationToken)
    {
        var ticketExists = await _dbContext.Tickets
            .AsNoTracking()
            .AnyAsync(x => x.Id == ticketId, cancellationToken);

        if (!ticketExists)
            return NotFound();

        var entry = new TimeEntry
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            Technician = _currentUser.DisplayName,
            Hours = request.Hours,
            HourlyRate = 250m,
            Description = request.Description,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.TimeEntries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new TimeEntryResponse(
            entry.Id, entry.TicketId, entry.Technician,
            entry.Hours, entry.HourlyRate, entry.Description, entry.CreatedAt);

        return Created($"api/tickets/{ticketId}/time-entries/{entry.Id}", response);
    }
}
