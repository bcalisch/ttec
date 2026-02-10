using Backend.Api.Contracts.Sensors;
using FluentValidation;

namespace Backend.Api.Validators.Sensors;

public class CreateSensorReadingRequestValidator : AbstractValidator<CreateSensorReadingRequest>
{
    public CreateSensorReadingRequestValidator()
    {
        RuleFor(x => x.Timestamp).NotEmpty();
    }
}
