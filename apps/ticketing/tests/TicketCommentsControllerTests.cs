using System.Net;
using System.Net.Http.Json;
using Ticketing.Api.Contracts.Comments;
using Ticketing.Api.Contracts.Tickets;
using Ticketing.Api.Models;
using FluentAssertions;

namespace Ticketing.Api.Tests;

public class TicketCommentsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TicketCommentsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateTestTicket()
    {
        var request = new CreateTicketRequest(
            "Ticket for comments", "Test", TicketPriority.Low, TicketCategory.Software,
            null, null, null, null, null, null, null);
        var response = await _client.PostAsJsonAsync("/api/tickets", request);
        var ticket = await response.Content.ReadFromJsonAsync<TicketDto>();
        return ticket!.Id;
    }

    [Fact]
    public async Task GetComments_ReturnsEmptyList()
    {
        var ticketId = await CreateTestTicket();

        var response = await _client.GetAsync($"/api/tickets/{ticketId}/comments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var comments = await response.Content.ReadFromJsonAsync<List<CommentResponse>>();
        comments.Should().NotBeNull();
        comments!.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateComment_ReturnsCreated()
    {
        var ticketId = await CreateTestTicket();
        var request = new CreateCommentRequest("Investigating the issue", false);

        var response = await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/comments", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var comment = await response.Content.ReadFromJsonAsync<CommentResponse>();
        comment.Should().NotBeNull();
        comment!.TicketId.Should().Be(ticketId);
        comment.Author.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateComment_OnNonexistentTicket_Returns404()
    {
        var request = new CreateCommentRequest("This should fail", false);

        var response = await _client.PostAsJsonAsync($"/api/tickets/{Guid.NewGuid()}/comments", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateComment_EmptyBody_Returns400()
    {
        var ticketId = await CreateTestTicket();
        var request = new CreateCommentRequest("", false);

        var response = await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/comments", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteComment_Success()
    {
        var ticketId = await CreateTestTicket();
        var createRequest = new CreateCommentRequest("To delete", false);
        var createResponse = await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/comments", createRequest);
        var comment = await createResponse.Content.ReadFromJsonAsync<CommentResponse>();

        var deleteResponse = await _client.DeleteAsync($"/api/tickets/{ticketId}/comments/{comment!.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CreateComment_HtmlEncodesBody()
    {
        var ticketId = await CreateTestTicket();
        var request = new CreateCommentRequest("<script>alert('xss')</script>", false);

        var response = await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/comments", request);
        var comment = await response.Content.ReadFromJsonAsync<CommentResponse>();

        comment!.Body.Should().NotContain("<script>");
    }

    private record TicketDto(Guid Id, string Title);
}
