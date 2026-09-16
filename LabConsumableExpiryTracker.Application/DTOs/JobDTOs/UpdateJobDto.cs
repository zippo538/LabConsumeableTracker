using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.JobDTOs
{
    public class UpdateJobDto
    {
        public JobStatus Status { get; init; }
        public string Description { get; init; }
    }
}