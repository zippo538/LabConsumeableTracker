using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.JobDTOs
{
    public class JobDto
    {
        public Guid Id { get; init; }
        public string JobNumber { get; init; } = string.Empty;
        public JobStatus Status { get; init; }
        public DateTimeOffset? StartedAt { get; init; }
        public DateTimeOffset? CompletedAt { get; init; }
    }
}