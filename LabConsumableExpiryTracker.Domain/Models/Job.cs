using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Domain.Models;

public class Job(
    Guid id,
    string jobNumber,
    string? description = null)
{
    private readonly List<Consumption> _consumptions = [];

    public Guid Id { get; } = ValidateId(id);
    public string? JobNumber { get; private set; } = ValidateJobNumber(jobNumber);
    public string? Description { get; private set; } = NormalizeDescription(description);
    public JobStatus Status { get; private set; } = JobStatus.Draft;
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    
    public IReadOnlyCollection<Consumption> Consumptions => _consumptions.AsReadOnly();

    private static Guid ValidateId(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Job id is required.", nameof(id));
        return id;
    }

    private static string ValidateJobNumber(string jobNumber)
    {
        if (string.IsNullOrWhiteSpace(jobNumber))
            throw new ArgumentException("Job number is required.", nameof(jobNumber));
        return jobNumber.Trim();
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void UpdateJobNumber(string? jobNumber)
    {
        JobNumber = string.IsNullOrWhiteSpace(jobNumber) ? null : jobNumber.Trim();
    }

    public void UpdateDescription(string? description)
    {
        Description = NormalizeDescription(description);
    }

    public void Start(DateTimeOffset now)
    {
        if (Status != JobStatus.Draft)
        {
            throw new InvalidOperationException(
                $"Job '{JobNumber}' can only be started from Draft. Current status: {Status}.");
        }
        Status = JobStatus.InProgress;
        StartedAt = now;
    }

    public void Cancel(DateTimeOffset now)
    {
        if (Status != JobStatus.Draft && Status != JobStatus.InProgress)
        {
            throw new InvalidOperationException(
                $"Job '{JobNumber}' can only be canceled from Draft or InProgress. Current status: {Status}.");
        }
        Status = JobStatus.Cancelled;
        CompletedAt = now;
    }

    public void Complete(DateTimeOffset now)
    {
        if (Status != JobStatus.InProgress)
        {
            throw new InvalidOperationException(
                $"Job '{JobNumber}' can only be completed from InProgress. Current status: {Status}.");
        }
        Status = JobStatus.Completed;
        CompletedAt = now;
    }
}