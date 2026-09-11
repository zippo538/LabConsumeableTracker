using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.Validators.Helpers;

namespace LabConsumableExpiryTracker.Application.Validators.LotValidator;

public class UpdateLotValidator : AbstractValidator<UpdateLotDto>
{
      public UpdateLotValidator()
      {
            RuleFor(x => x.LotId)
                  .NotEmpty()
                  .WithMessage("LotId wajib diisi.");

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

            RuleFor(x => x.ExpiryDate)
                  .NotEqual(default(DateOnly))
                  .WithMessage("ExpiryDate wajib diisi.");

            RuleFor(x => x.StorageLocation)
                  .Cascade(CascadeMode.Stop)
                  .Must(LotHelperValidator.NotBeBlank)
                  .WithMessage("StorageLocation wajib diisi.")
                  .MaximumLength(200)
                  .WithMessage("StorageLocation maksimal 200 karakter.");

            RuleFor(x => x.RowVersion)
                  .NotNull()
                  .WithMessage("RowVersion wajib diisi.")
                  .Must(value => value is { Length: > 0 })
                  .WithMessage("RowVersion tidak boleh kosong.");
      }

}