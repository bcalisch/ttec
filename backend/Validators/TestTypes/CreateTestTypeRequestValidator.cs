using Backend.Api.Contracts.TestTypes;
using FluentValidation;

namespace Backend.Api.Validators.TestTypes;

public class CreateTestTypeRequestValidator : AbstractValidator<CreateTestTypeRequest>
{
    public CreateTestTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(50);
    }
}
