using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.OffsetPaginationDTOs;

namespace LabConsumableExpiryTracker.Application.Validators.OffsetPageValidator
{
    public class OffsetPaginationValidator : AbstractValidator<OffsetPaginationRequest>
    {
        public OffsetPaginationValidator()
        {
            RuleFor(x => x.Offset)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Offset tidak boleh kurang dari 0.");

            RuleFor(x => x.Limit)
                .InclusiveBetween(1, 100)
                .WithMessage("Limit harus berada antara 1 sampai 100.");
        }
    }
}