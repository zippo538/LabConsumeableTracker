using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;
using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.Validators.ItemValidator
{
    public class CreateItemValidator : AbstractValidator<CreateItemDto>
    {
        public CreateItemValidator()
        {
            RuleFor(item => item.Code)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Item code is required.")
                .MaximumLength(50).WithMessage("Item code cannot exceed 50 characters.");

            RuleFor(item => item.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Item name is required.")
                .MaximumLength(200).WithMessage("Item name cannot exceed 200 characters.");

            RuleFor(item => item.BaseUnit)
                .NotEmpty()
                .WithMessage("BaseUnit wajib diisi.")
                .Must(value => Enum.TryParse<UnitOfMeasure>(value,ignoreCase: true,out _))
                .WithMessage("BaseUnit tidak valid.");;

            RuleFor(item => item.MinimumStock)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum stock cannot be negative.");

            RuleFor(item => item.ExpiringSoonDays)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Expiring soon days cannot be negative.");
        }
    }
}