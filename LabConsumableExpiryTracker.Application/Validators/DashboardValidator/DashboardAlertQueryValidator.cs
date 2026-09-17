using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;

namespace LabConsumableExpiryTracker.Application.Validators.DashboardValidator;

public class DashboardAlertQueryValidator : AbstractValidator<DashboardAlertQuery>
{
    public DashboardAlertQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0)
            .WithMessage("Page must be greater than zero.");

        RuleFor(x => x.PageSize).InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.Type).IsInEnum()
            .When(x => x.Type.HasValue)
            .WithMessage("Type is invalid.");
    }
}
