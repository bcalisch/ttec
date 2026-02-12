using System.Net;
using System.Net.Http.Json;
using Ticketing.Api.Contracts.KnowledgeArticles;
using FluentAssertions;

namespace Ticketing.Api.Tests;

public class KnowledgeArticlesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public KnowledgeArticlesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateArticle_ReturnsCreated()
    {
        var request = new CreateKnowledgeArticleRequest(
            "How to calibrate IC roller",
            "Step 1: ...",
            "calibration,ic-roller",
            true);

        var response = await _client.PostAsJsonAsync("/api/knowledge-articles", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var article = await response.Content.ReadFromJsonAsync<KnowledgeArticleResponse>();
        article.Should().NotBeNull();
        article!.Title.Should().Be("How to calibrate IC roller");
        article.IsPublished.Should().BeTrue();
    }

    [Fact]
    public async Task GetArticles_ReturnsOnlyPublished()
    {
        // Create one published, one unpublished
        await _client.PostAsJsonAsync("/api/knowledge-articles",
            new CreateKnowledgeArticleRequest("Published article", "Content", "test", true));
        await _client.PostAsJsonAsync("/api/knowledge-articles",
            new CreateKnowledgeArticleRequest("Draft article", "Content", "test", false));

        var response = await _client.GetAsync("/api/knowledge-articles");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var articles = await response.Content.ReadFromJsonAsync<List<KnowledgeArticleResponse>>();
        articles!.Should().OnlyContain(a => a.IsPublished);
    }

    [Fact]
    public async Task GetArticleById_ReturnsEvenUnpublished()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/knowledge-articles",
            new CreateKnowledgeArticleRequest("Unpublished detail", "Secret content", "draft", false));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeArticleResponse>();

        var getResponse = await _client.GetAsync($"/api/knowledge-articles/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var article = await getResponse.Content.ReadFromJsonAsync<KnowledgeArticleResponse>();
        article!.IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateArticle_ToPublish()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/knowledge-articles",
            new CreateKnowledgeArticleRequest("Draft to publish", "Content", "draft", false));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeArticleResponse>();

        var updateRequest = new UpdateKnowledgeArticleRequest("Draft to publish", "Updated content", "published", true);
        var updateResponse = await _client.PutAsJsonAsync($"/api/knowledge-articles/{created!.Id}", updateRequest);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<KnowledgeArticleResponse>();
        updated!.IsPublished.Should().BeTrue();
    }

    [Fact]
    public async Task GetArticles_FilterByTag()
    {
        await _client.PostAsJsonAsync("/api/knowledge-articles",
            new CreateKnowledgeArticleRequest("Calibration Guide", "Content", "calibration,bomag", true));
        await _client.PostAsJsonAsync("/api/knowledge-articles",
            new CreateKnowledgeArticleRequest("Training Manual", "Content", "training,safety", true));

        var response = await _client.GetAsync("/api/knowledge-articles?tag=calibration");
        var articles = await response.Content.ReadFromJsonAsync<List<KnowledgeArticleResponse>>();
        articles!.Should().Contain(a => a.Tags.Contains("calibration"));
        articles.Should().NotContain(a => a.Title == "Training Manual");
    }

    [Fact]
    public async Task CreateArticle_EmptyTitle_Returns400()
    {
        var request = new CreateKnowledgeArticleRequest("", "Content", "tag", true);

        var response = await _client.PostAsJsonAsync("/api/knowledge-articles", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetArticle_NotFound()
    {
        var response = await _client.GetAsync($"/api/knowledge-articles/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
