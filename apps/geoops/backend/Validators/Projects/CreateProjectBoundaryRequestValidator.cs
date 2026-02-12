using GeoOps.Api.Contracts.Projects;
using FluentValidation;

namespace GeoOps.Api.Validators.Projects;

public class CreateProjectBoundaryRequestValidator : AbstractValidator<CreateProjectBoundaryRequest>
{
    public CreateProjectBoundaryRequestValidator()
    {
        RuleFor(x => x.GeoJson).NotEmpty();
    }
}
