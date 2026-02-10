using Backend.Api.Contracts.TestResults;
using FluentValidation;

namespace Backend.Api.Validators.TestResults;

public class CsvImportRequestValidator : AbstractValidator<CsvImportRequest>
{
    public CsvImportRequestValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BlobUri).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
