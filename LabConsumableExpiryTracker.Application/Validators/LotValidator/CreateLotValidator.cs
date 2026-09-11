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
            .WithMessage("ItemId wajib diisi.");

        RuleFor(x => x.LotNumber)
            .Cascade(CascadeMode.Stop)
            .Must(LotHelperValidator.NotBeBlank)
            .WithMessage("LotNumber wajib diisi.")
            .MaximumLength(100)
            .WithMessage("LotNumber maksimal 100 karakter.");

        RuleFor(x => x.SupplierLotNumber)
            .MaximumLength(100)
            .WithMessage("SupplierLotNumber maksimal 100 karakter.");

        RuleFor(x => x.SupplierName)
            .Cascade(CascadeMode.Stop)
            .Must(LotHelperValidator.NotBeBlank)
            .WithMessage("SupplierName wajib diisi.")
            .MaximumLength(200)
            .WithMessage("SupplierName maksimal 200 karakter.");

        RuleFor(x => x.InitialQuantity)
            .GreaterThan(0)
            .WithMessage("InitialQuantity harus lebih besar dari 0.")
            .LessThanOrEqualTo(999_999_999_999.9999m)
            .WithMessage("InitialQuantity melebihi batas yang diperbolehkan.");

        RuleFor(x => x.RemainingQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("RemainingQuantity tidak boleh negatif.")
            .LessThanOrEqualTo(x => x.InitialQuantity)
            .WithMessage("RemainingQuantity tidak boleh melebihi InitialQuantity.");

        RuleFor(x => x.ReceivedAt)
            .NotEqual(default(DateTime))
            .WithMessage("ReceivedAt wajib diisi.")
            .Must(BeInThePastOrPresent)
            .WithMessage("ReceivedAt tidak boleh berada di masa depan.");

        RuleFor(x => x.ExpiryDate)
            .NotEqual(default(DateOnly))
            .WithMessage("ExpiryDate wajib diisi.")
            .GreaterThanOrEqualTo(x => DateOnly.FromDateTime(x.ReceivedAt.ToUniversalTime()))
            .WithMessage("ExpiryDate tidak boleh mendahului tanggal penerimaan.");

        RuleFor(x => x.StorageLocation)
            .Cascade(CascadeMode.Stop)
            .Must(LotHelperValidator.NotBeBlank)
            .WithMessage("StorageLocation wajib diisi.")
            .MaximumLength(200)
            .WithMessage("StorageLocation maksimal 200 karakter.");
      }
    private bool BeInThePastOrPresent(DateTime value)
    {
        if (value == default)
        {
            return false;
        }

        return value.ToUniversalTime() <= _timeProvider.GetUtcNow().UtcDateTime;
    }
    }
