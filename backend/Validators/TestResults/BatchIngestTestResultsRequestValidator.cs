using Backend.Api.Contracts.TestResults;
using FluentValidation;

namespace Backend.Api.Validators.TestResults;

public class BatchIngestTestResultsRequestValidator : AbstractValidator<BatchIngestTestResultsRequest>
{
    public BatchIngestTestResultsRequestValidator()
    {
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Items).NotEmpty();
    }
}
