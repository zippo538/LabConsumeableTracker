using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;
using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.Validators.ItemValidator
{
    public class UpdateItemValidator : AbstractValidator<UpdateItemDto>
    {
        public UpdateItemValidator()
        {
            RuleFor(item => item.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Item name is required.")
            .MaximumLength(200).WithMessage("Item name cannot exceed 200 characters.");

            RuleFor(item => item.BaseUnit)
                .Must(unit => Enum.IsDefined(typeof(UnitOfMeasure), unit))
                .WithMessage("Base unit is invalid.");

            RuleFor(item => item.MinimumStock)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum stock cannot be negative.");

            RuleFor(item => item.ExpiringSoonDays)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Expiring soon days cannot be negative.");
        }
    }
}