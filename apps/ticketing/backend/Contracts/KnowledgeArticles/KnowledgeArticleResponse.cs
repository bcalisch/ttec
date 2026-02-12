namespace Ticketing.Api.Contracts.KnowledgeArticles;

public record KnowledgeArticleResponse(
    Guid Id,
    string Title,
    string Content,
    string Tags,
    bool IsPublished,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
