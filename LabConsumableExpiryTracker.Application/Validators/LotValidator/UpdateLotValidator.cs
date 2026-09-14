using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.Validators.Helpers;

namespace LabConsumableExpiryTracker.Application.Validators.LotValidator;

public class UpdateLotValidator : AbstractValidator<UpdateLotDto>
{
      public UpdateLotValidator()
      {
            RuleFor(x => x.LotNumber)
                  .Cascade(CascadeMode.Stop)
                  .Must(LotHelperValidator.NotBeBlank)
                  .WithMessage("LotNumber is required.")
                  .MaximumLength(100)
                  .WithMessage("LotNumber maximum 100 characters.");

            RuleFor(x => x.SupplierLotNumber)
                  .MaximumLength(100)
                  .WithMessage("SupplierLotNumber maximum 100 characters.");

            RuleFor(x => x.SupplierName)
                  .Cascade(CascadeMode.Stop)
                  .Must(LotHelperValidator.NotBeBlank)
                  .WithMessage("SupplierName wajib diisi.")
                  .MaximumLength(200)
                  .WithMessage("SupplierName maximum 200 characters.");

            RuleFor(x => x.ExpiryDate)
                  .NotEqual(default(DateOnly))
                  .WithMessage("ExpiryDate is required.");

            RuleFor(x => x.StorageLocation)
                  .Cascade(CascadeMode.Stop)
                  .Must(LotHelperValidator.NotBeBlank)
                  .WithMessage("StorageLocation is required.")
                  .MaximumLength(200)
                  .WithMessage("StorageLocation maximum 200 character.");

            RuleFor(x => x.RowVersion)
                  .NotNull()
                  .WithMessage("RowVersion is reuqired.")
                  .Must(value => value is { Length: > 0 })
                  .WithMessage("RowVersion cannot be empty.");
      }

}