namespace LabConsumableExpiryTracker.Domain.Models;

public class Consumption
{
    private Consumption()
    {
    }

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
    }

    public Guid Id { get; private set; }
    public Guid JobId { get; private set; }
    public Guid LotId { get; private set; }
    public decimal Quantity { get; private set; }
    public DateTimeOffset ConsumedAt { get; private set; }
    public Guid ConsumedBy { get; private set; }
}
