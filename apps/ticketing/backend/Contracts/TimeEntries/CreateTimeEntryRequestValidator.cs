using FluentValidation;

namespace Ticketing.Api.Contracts.TimeEntries;

public class CreateTimeEntryRequestValidator : AbstractValidator<CreateTimeEntryRequest>
{
    public CreateTimeEntryRequestValidator()
    {
        RuleFor(x => x.Hours).GreaterThan(0).LessThanOrEqualTo(24);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
