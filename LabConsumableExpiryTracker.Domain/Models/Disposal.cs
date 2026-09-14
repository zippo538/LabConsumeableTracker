
namespace LabConsumableExpiryTracker.Domain.Models;

public class Disposal
{
    private Disposal()
    {
    }

    internal Disposal(
        Guid id,
        Guid subLotId,
        decimal quantity,
        string reason,
        DateTimeOffset disposedAt,
        Guid disposedBy)
    {
        Id = id;
        SubLotId = subLotId;
        Quantity = quantity;
        Reason = reason.Trim();
        DisposedAt = disposedAt;
        DisposedBy = disposedBy;
    }

    public Guid Id { get; private set; }
    public Guid SubLotId { get; private set; }
    public decimal Quantity { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTimeOffset DisposedAt { get; private set; }
    public Guid DisposedBy { get; private set; }
}
