namespace Ticketing.Api.Contracts.Comments;

public record CreateCommentRequest(
    string Body,
    bool IsInternal
);
