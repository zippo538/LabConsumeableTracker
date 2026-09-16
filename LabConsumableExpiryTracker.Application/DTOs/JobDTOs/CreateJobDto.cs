using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabConsumableExpiryTracker.Application.DTOs.JobDTOs
{
    public class CreateJobDto
    {
        public string JobNumber { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
    }
}