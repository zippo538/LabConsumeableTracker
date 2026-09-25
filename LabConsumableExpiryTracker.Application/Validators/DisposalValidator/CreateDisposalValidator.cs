using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;
namespace LabConsumableExpiryTracker.Application.Validators.DisposalValidator;
public class CreateDisposalValidator : AbstractValidator<CreateDisposalDto>
{
    public CreateDisposalValidator()
    {
        RuleFor(x => x.ExpectedRowVersion)
            .NotNull()
            .WithMessage("Expected Row Empty.")
            .Must(value => value is { Length: > 0 })
            .WithMessage("RowVersion cannot be empty.");
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
