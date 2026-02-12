using FluentValidation;

namespace Ticketing.Api.Contracts.Equipment;

public class CreateEquipmentRequestValidator : AbstractValidator<CreateEquipmentRequest>
{
    public CreateEquipmentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Manufacturer).IsInEnum();
        RuleFor(x => x.Model).MaximumLength(200);
    }
}
