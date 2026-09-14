using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Domain.Models;

public class Job
{
    private readonly List<Consumption> _consumptions = [];
    public Guid Id { get; private set; }
    public string JobNumber { get; private set; } = string.Empty;
    public JobStatus Status { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public IReadOnlyCollection<Consumption> Consumptions => _consumptions.AsReadOnly();

    public Job(Guid id, string jobNumber)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Job id is required.", nameof(id));
        }
        if (string.IsNullOrWhiteSpace(jobNumber))
        {
            throw new ArgumentException("Job number is required.", nameof(jobNumber));
        }
        Id = id;
        JobNumber = jobNumber.Trim();
        Status = JobStatus.Draft;
    }

    public void Start(DateTimeOffset now)
    {
        if (Status != JobStatus.Draft)
        {
            throw new InvalidOperationException($"Job '{JobNumber}' can only be started from Draft. Current status: {Status}.");
        }

        Status = JobStatus.InProgress;
        StartedAt = now;
    }

    public void Complete(DateTimeOffset now)
    {
        if (Status != JobStatus.InProgress)
        {
            throw new InvalidOperationException($"Job '{JobNumber}' can only be completed from InProgress. Current status: {Status}.");
        }
        Status = JobStatus.Completed; 
        CompletedAt = now;
    }
    public void StartNow(DateTimeOffset now) => Start(now);
}
