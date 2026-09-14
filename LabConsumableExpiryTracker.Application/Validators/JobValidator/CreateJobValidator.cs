using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.JobDTOs;
using LabConsumableExpiryTracker.Application.Validators.Helpers;

namespace LabConsumableExpiryTracker.Application.Validators.JobValidator
{
    public class CreateJobValidator : AbstractValidator<CreateJobDto>
    {
        public CreateJobValidator()
        {
            RuleFor(x => x.JobNumber)
                .Cascade(CascadeMode.Stop)
                .Must(LotHelperValidator.NotBeBlank)
                .WithMessage("JobNumber wajib diisi.")
                .MaximumLength(100)
                .WithMessage("JobNumber maksimal 100 karakter.");
        }
    }
}