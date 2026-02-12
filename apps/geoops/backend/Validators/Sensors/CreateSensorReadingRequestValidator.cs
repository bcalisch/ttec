using GeoOps.Api.Contracts.Sensors;
using FluentValidation;

namespace GeoOps.Api.Validators.Sensors;

public class CreateSensorReadingRequestValidator : AbstractValidator<CreateSensorReadingRequest>
{
    public CreateSensorReadingRequestValidator()
    {
        RuleFor(x => x.Timestamp).NotEmpty();
    }
}
