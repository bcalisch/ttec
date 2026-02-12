using GeoOps.Api.Contracts.Observations;
using FluentValidation;

namespace GeoOps.Api.Validators.Observations;

public class UpdateObservationRequestValidator : AbstractValidator<UpdateObservationRequest>
{
    public UpdateObservationRequestValidator()
    {
        RuleFor(x => x.Note).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
    }
}
