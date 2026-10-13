using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;
using LabConsumableExpiryTracker.Application.Validators.Helpers;

namespace LabConsumableExpiryTracker.Application.Validators.ConsumptionValidator;

public class VerifyScanRequestValidator : AbstractValidator<VerifyScanRequestDto>
{
    public VerifyScanRequestValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty()
            .WithMessage("ItemId is required.");

        RuleFor(x => x.ScannedLotNumber)
            .Cascade(CascadeMode.Stop)
            .Must(LotHelperValidator.NotBeBlank)
            .WithMessage("ScannedLotNumber is required.")
            .MaximumLength(100)
            .WithMessage("ScannedLotNumber maximum 100 characters.");
    }
}
