using Backend.Api.Contracts.Projects;
using FluentValidation;

namespace Backend.Api.Validators.Projects;

public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Client).NotEmpty().MaximumLength(200);
    }
}
