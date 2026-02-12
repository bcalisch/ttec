using GeoOps.Api.Contracts.TestTypes;
using FluentValidation;

namespace GeoOps.Api.Validators.TestTypes;

public class UpdateTestTypeRequestValidator : AbstractValidator<UpdateTestTypeRequest>
{
    public UpdateTestTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(50);
    }
}
