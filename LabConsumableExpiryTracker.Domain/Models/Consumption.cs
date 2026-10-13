using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Domain.Models;

public class Consumption
{
    public Consumption(
        Guid id,
        Guid jobId,
        Guid lotId,
        decimal quantity,
        DateTimeOffset consumedAt,
        Guid consumedBy)
    {
        Id = id;
        JobId = jobId;
        LotId = lotId;
        Quantity = quantity;
        ConsumedAt = consumedAt;
        ConsumedBy = consumedBy;
        Status = ConsumptionStatus.Verified;
    }

    public Guid Id { get; private set; }
    public Guid JobId { get; private set; }
    public Guid LotId { get; private set; }
    public decimal Quantity { get; private set; }
    public DateTimeOffset ConsumedAt { get; private set; }
    public Guid ConsumedBy { get; private set; }
    public ConsumptionStatus Status { get; private set; }
    public DateTimeOffset? VerifiedAt { get; private set; }

    public static Consumption CreateDraft(
        Guid id,
        Guid jobId,
        Guid lotId,
        decimal quantity,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        return new Consumption(
            id,
            jobId,
            lotId,
            quantity,
            createdAt,
            createdBy)
        {
            Status = ConsumptionStatus.PendingScan
        };
    }

    public void VerifyScan(DateTimeOffset now)
    {
        if (Status == ConsumptionStatus.Verified)
        {
            throw new InvalidOperationException("Consumption has already been verified.");
        }

        Status = ConsumptionStatus.Verified;
        VerifiedAt = now;
    }
}
