namespace Ticketing.Api.Contracts.KnowledgeArticles;

public record UpdateKnowledgeArticleRequest(
    string Title,
    string Content,
    string Tags,
    bool IsPublished
);
