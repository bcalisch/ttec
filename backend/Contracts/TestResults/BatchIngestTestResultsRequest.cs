namespace Backend.Api.Contracts.TestResults;

public record BatchIngestTestResultsRequest(
    string IdempotencyKey,
    IReadOnlyList<CreateTestResultRequest> Items
);
