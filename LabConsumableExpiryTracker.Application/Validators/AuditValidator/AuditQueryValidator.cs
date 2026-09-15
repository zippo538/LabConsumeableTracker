using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.AuditDTOs;
using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.Validators.AuditValidator;

public sealed class AuditQueryValidator : AbstractValidator<AuditQuery>
{
    public AuditQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0)
            .WithMessage("Page must be greater than zero.");

        RuleFor(x => x.PageSize).InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.EventType)
            .IsInEnum()
            .When(x => x.EventType.HasValue)
            .WithMessage("EventType is invalid.");

        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From.Value < x.To.Value)
            .WithMessage("From must be earlier than To.");

        RuleFor(x => x)
            .Must(x => x.EventType != AuditEventType.Disposal || !x.JobId.HasValue)
            .WithMessage("JobId can only be used for Consumption audit events.");
    }
}
