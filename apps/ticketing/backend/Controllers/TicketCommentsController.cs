using Ticketing.Api.Contracts.Comments;
using Ticketing.Api.Data;
using Ticketing.Api.Models;
using Ticketing.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Ticketing.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets/{ticketId:guid}/comments")]
public class TicketCommentsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public TicketCommentsController(AppDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CommentResponse>>> GetComments(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        var ticketExists = await _dbContext.Tickets
            .AsNoTracking()
            .AnyAsync(x => x.Id == ticketId, cancellationToken);

        if (!ticketExists)
            return NotFound();

        var comments = await _dbContext.TicketComments
            .AsNoTracking()
            .Where(x => x.TicketId == ticketId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new CommentResponse(x.Id, x.TicketId, x.Author, x.Body, x.IsInternal, x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(comments);
    }

    [HttpPost]
    public async Task<ActionResult<CommentResponse>> CreateComment(
        Guid ticketId,
        [FromBody] CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var ticketExists = await _dbContext.Tickets
            .AsNoTracking()
            .AnyAsync(x => x.Id == ticketId, cancellationToken);

        if (!ticketExists)
            return NotFound();

        var comment = new TicketComment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            Author = _currentUser.DisplayName,
            Body = WebUtility.HtmlEncode(request.Body),
            IsInternal = request.IsInternal,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.TicketComments.Add(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new CommentResponse(
            comment.Id, comment.TicketId, comment.Author,
            comment.Body, comment.IsInternal, comment.CreatedAt);

        return Created($"api/tickets/{ticketId}/comments/{comment.Id}", response);
    }

    [HttpDelete("{commentId:guid}")]
    public async Task<ActionResult> DeleteComment(
        Guid ticketId,
        Guid commentId,
        CancellationToken cancellationToken)
    {
        var comment = await _dbContext.TicketComments
            .FirstOrDefaultAsync(x => x.TicketId == ticketId && x.Id == commentId, cancellationToken);

        if (comment is null)
            return NotFound();

        _dbContext.TicketComments.Remove(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
