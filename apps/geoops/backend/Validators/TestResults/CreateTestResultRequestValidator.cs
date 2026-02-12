using GeoOps.Api.Contracts.TestResults;
using FluentValidation;

namespace GeoOps.Api.Validators.TestResults;

public class CreateTestResultRequestValidator : AbstractValidator<CreateTestResultRequest>
{
    public CreateTestResultRequestValidator()
    {
        RuleFor(x => x.TestTypeId).NotEmpty();
        RuleFor(x => x.Timestamp).NotEmpty();
        RuleFor(x => x.Value).NotNull();
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Source).MaximumLength(100);
        RuleFor(x => x.Technician).MaximumLength(200);
        RuleFor(x => x.Status).MaximumLength(20);
    }
}
