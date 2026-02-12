using FluentValidation;

namespace Ticketing.Api.Contracts.Tickets;

public class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.SourceApp).MaximumLength(100);
        RuleFor(x => x.SourceEntityType).MaximumLength(100);
        RuleFor(x => x.AssignedTo).MaximumLength(200);
    }
}
