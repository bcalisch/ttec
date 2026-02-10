using Backend.Api.Contracts.Observations;
using FluentValidation;

namespace Backend.Api.Validators.Observations;

public class CreateObservationRequestValidator : AbstractValidator<CreateObservationRequest>
{
    public CreateObservationRequestValidator()
    {
        RuleFor(x => x.Note).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
    }
}
