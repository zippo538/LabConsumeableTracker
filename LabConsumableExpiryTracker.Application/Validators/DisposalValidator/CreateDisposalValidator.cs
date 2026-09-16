using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;
using LabConsumableExpiryTracker.Application.Validators.Helpers;

namespace LabConsumableExpiryTracker.Application.Validators.DisposalValidator;

public class CreateDisposalValidator : AbstractValidator<CreateDisposalDto>
{
    public CreateDisposalValidator()
    {
        RuleFor(x => x.LotId)
            .NotEmpty()
            .WithMessage("LotId wajib diisi.")
            .Must(DisposalHelperValidator.BeValidGuid)
            .WithMessage("LotId harus berupa GUID yang valid.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity harus lebih dari nol.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason wajib diisi.")
            .MaximumLength(500)
            .WithMessage("Reason maksimal 500 karakter.");
    }
}
