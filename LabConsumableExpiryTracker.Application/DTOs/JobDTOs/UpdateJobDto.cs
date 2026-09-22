using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.JobDTOs
{
    public class UpdateJobDto
    {
        public string JobNumber { get; init; } = string.Empty;
        public string Description { get; init; }
    }
}