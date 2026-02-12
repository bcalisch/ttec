namespace Ticketing.Api.Contracts.KnowledgeArticles;

public record CreateKnowledgeArticleRequest(
    string Title,
    string Content,
    string Tags,
    bool IsPublished
);
