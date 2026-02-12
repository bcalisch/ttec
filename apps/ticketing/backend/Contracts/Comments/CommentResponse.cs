namespace Ticketing.Api.Contracts.Comments;

public record CommentResponse(
    Guid Id,
    Guid TicketId,
    string Author,
    string Body,
    bool IsInternal,
    DateTimeOffset CreatedAt
);
