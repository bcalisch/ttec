using Ticketing.Api.Contracts.Billing;
using Ticketing.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ticketing.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/billing")]
public class BillingController : ControllerBase
{
    private const decimal BaseCharge = 200m;
    private readonly AppDbContext _dbContext;

    public BillingController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("tickets/{id:guid}")]
    public async Task<ActionResult<TicketBillingResponse>> GetTicketBilling(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ticket is null)
            return NotFound();

        var timeEntries = await _dbContext.TimeEntries
            .AsNoTracking()
            .Where(x => x.TicketId == id)
            .ToListAsync(cancellationToken);

        var lineItems = timeEntries.Select(e => new TimeEntryLineItem(
            e.Id, e.Hours, e.HourlyRate, e.Hours * e.HourlyRate, e.Description
        )).ToList();

        var hourlyTotal = lineItems.Sum(x => x.LineTotal);

        return Ok(new TicketBillingResponse(
            ticket.Id, ticket.Title, BaseCharge, hourlyTotal, BaseCharge + hourlyTotal, lineItems
        ));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<BillingSummaryResponse>> GetBillingSummary(CancellationToken cancellationToken)
    {
        var tickets = await _dbContext.Tickets
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var allTimeEntries = await _dbContext.TimeEntries
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var ticketBillings = tickets.Select(ticket =>
        {
            var entries = allTimeEntries.Where(e => e.TicketId == ticket.Id).ToList();
            var lineItems = entries.Select(e => new TimeEntryLineItem(
                e.Id, e.Hours, e.HourlyRate, e.Hours * e.HourlyRate, e.Description
            )).ToList();
            var hourlyTotal = lineItems.Sum(x => x.LineTotal);
            return new TicketBillingResponse(
                ticket.Id, ticket.Title, BaseCharge, hourlyTotal, BaseCharge + hourlyTotal, lineItems
            );
        }).ToList();

        var totalBase = ticketBillings.Sum(x => x.BaseCharge);
        var totalHourly = ticketBillings.Sum(x => x.HourlyTotal);

        return Ok(new BillingSummaryResponse(totalBase, totalHourly, totalBase + totalHourly, ticketBillings.Count, ticketBillings));
    }
}
