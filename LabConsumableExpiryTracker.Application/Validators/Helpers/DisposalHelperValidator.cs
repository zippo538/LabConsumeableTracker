using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabConsumableExpiryTracker.Application.Validators.Helpers
{
    public class DisposalHelperValidator
    {
        public static bool BeValidGuid(string value)
        {
            return Guid.TryParse(value, out _);
        }
    }
}