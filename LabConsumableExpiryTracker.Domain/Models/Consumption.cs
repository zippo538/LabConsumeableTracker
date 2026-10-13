namespace LabConsumableExpiryTracker.Domain.Models;

public class Consumption(
    Guid id,
    Guid jobId,
    Guid lotId,
    decimal quantity,
    DateTimeOffset consumedAt,
    Guid consumedBy)
{
    public Guid Id { get; private set; } = id;
    public Guid JobId { get; private set; } = jobId;
    public Guid LotId { get; private set; } = lotId;
    public decimal Quantity { get; private set; } = quantity;
    public DateTimeOffset ConsumedAt { get; private set; } = consumedAt;
    public Guid ConsumedBy { get; private set; } = consumedBy;
}
