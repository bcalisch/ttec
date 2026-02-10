using Backend.Api.Contracts.Projects;
using FluentValidation;

namespace Backend.Api.Validators.Projects;

public class CreateProjectBoundaryRequestValidator : AbstractValidator<CreateProjectBoundaryRequest>
{
    public CreateProjectBoundaryRequestValidator()
    {
        RuleFor(x => x.GeoJson).NotEmpty();
    }
}
