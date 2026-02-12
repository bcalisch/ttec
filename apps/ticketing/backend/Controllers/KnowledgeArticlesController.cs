using Ticketing.Api.Contracts.KnowledgeArticles;
using Ticketing.Api.Data;
using Ticketing.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ticketing.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/knowledge-articles")]
public class KnowledgeArticlesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public KnowledgeArticlesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<KnowledgeArticleResponse>>> GetArticles(
        [FromQuery] string? tag,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.KnowledgeArticles
            .AsNoTracking()
            .Where(x => x.IsPublished);

        if (!string.IsNullOrWhiteSpace(tag))
            query = query.Where(x => x.Tags.Contains(tag));

        var articles = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new KnowledgeArticleResponse(x.Id, x.Title, x.Content, x.Tags, x.IsPublished, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Ok(articles);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<KnowledgeArticleResponse>> GetArticle(Guid id, CancellationToken cancellationToken)
    {
        var article = await _dbContext.KnowledgeArticles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (article is null)
            return NotFound();

        return Ok(new KnowledgeArticleResponse(article.Id, article.Title, article.Content, article.Tags, article.IsPublished, article.CreatedAt, article.UpdatedAt));
    }

    [HttpPost]
    public async Task<ActionResult<KnowledgeArticleResponse>> CreateArticle(
        [FromBody] CreateKnowledgeArticleRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var article = new KnowledgeArticle
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            Tags = request.Tags,
            IsPublished = request.IsPublished,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.KnowledgeArticles.Add(article);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new KnowledgeArticleResponse(article.Id, article.Title, article.Content, article.Tags, article.IsPublished, article.CreatedAt, article.UpdatedAt);
        return CreatedAtAction(nameof(GetArticle), new { id = article.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<KnowledgeArticleResponse>> UpdateArticle(
        Guid id,
        [FromBody] UpdateKnowledgeArticleRequest request,
        CancellationToken cancellationToken)
    {
        var article = await _dbContext.KnowledgeArticles
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (article is null)
            return NotFound();

        article.Title = request.Title;
        article.Content = request.Content;
        article.Tags = request.Tags;
        article.IsPublished = request.IsPublished;
        article.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new KnowledgeArticleResponse(article.Id, article.Title, article.Content, article.Tags, article.IsPublished, article.CreatedAt, article.UpdatedAt));
    }
}
