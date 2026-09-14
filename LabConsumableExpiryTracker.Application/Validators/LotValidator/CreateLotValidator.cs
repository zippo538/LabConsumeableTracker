using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.Validators.Helpers;

namespace LabConsumableExpiryTracker.Application.Validators.LotValidator;

public class CreateLotValidator : AbstractValidator<CreateLotDto>
{
    private readonly TimeProvider _timeProvider;
    public CreateLotValidator(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

        RuleFor(x => x.ItemId)
            .NotEmpty()
            .WithMessage("ItemId is required.");

        RuleFor(x => x.LotNumber)
            .Cascade(CascadeMode.Stop)
            .Must(LotHelperValidator.NotBeBlank)
            .WithMessage("LotNumber is required.")
            .MaximumLength(100)
            .WithMessage("LotNumber maximum 100 characters.");

        RuleFor(x => x.SupplierLotNumber)
            .Must(LotHelperValidator.NotBeBlank)
            .MaximumLength(100)
            .WithMessage("SupplierLotNumber maximum 100 characters");

        RuleFor(x => x.SupplierName)
            .Cascade(CascadeMode.Stop)
            .Must(LotHelperValidator.NotBeBlank)
            .WithMessage("SupplierName is required")
            .MaximumLength(200)
            .WithMessage("SupplierName maximum 200 characters");

        RuleFor(x => x.InitialQuantity)
            .GreaterThan(0)
            .WithMessage("InitialQuantity must greater than 0.")
            .LessThanOrEqualTo(999_999_999_999.9999m)
            .WithMessage("InitialQuantity exceeds the allowed limit.");

        RuleFor(x => x.ReceivedAt)
            .Cascade(CascadeMode.Stop)
            .NotEqual(default(DateTimeOffset))
            .WithMessage("ReceivedAt is required.")
            .Must(BeInThePastOrPresent)
            .WithMessage("ReceiveAt cannot be later than the expiry date.");

        RuleFor(x => x.ExpiryDate)
            .NotEqual(default(DateOnly))
            .WithMessage("ExpiryDate is required.")
            .GreaterThanOrEqualTo(x => DateOnly.FromDateTime(x.ReceivedAt.UtcDateTime))
            .WithMessage("ExpiryDate cannot be earlier than ReceiveAt.");

        RuleFor(x => x.StorageLocation)
            .Cascade(CascadeMode.Stop)
            .Must(LotHelperValidator.NotBeBlank)
            .WithMessage("StorageLocation is required")
            .MaximumLength(200)
            .WithMessage("StorageLocation maximum 200 character.");
    }
    private bool BeInThePastOrPresent(DateTimeOffset value)
    {
        if (value == default)
        {
            return false;
        }

        return value <= _timeProvider.GetUtcNow();
    }
}
