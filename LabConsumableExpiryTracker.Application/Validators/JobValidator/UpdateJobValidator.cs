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
            RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status Job tidak valid.")
            .Must(status => status is JobStatus.InProgress or JobStatus.Completed)
            .WithMessage("Status hanya boleh InProgress atau Completed.");
        }
    }
}