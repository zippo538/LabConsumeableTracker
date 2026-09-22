using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.JobDTOs;
using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.Validators.JobValidator
{
    public class UpdateJobValidator : AbstractValidator<UpdateJobDto>
    {
        public UpdateJobValidator()
        {
            RuleFor(x => x.JobNumber)
                .NotEmpty()
                .WithMessage("Job Number must be filled.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description must be filled.");
        }
    }
}