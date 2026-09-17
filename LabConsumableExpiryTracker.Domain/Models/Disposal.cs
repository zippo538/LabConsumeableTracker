
namespace LabConsumableExpiryTracker.Domain.Models;

public class Disposal
{
    public Guid Id { get; private set; }
    public Guid LotId { get; private set; }
    public decimal Quantity { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTimeOffset DisposedAt { get; private set; }
    public Guid DisposedBy { get; private set; }
    public Lot Lot { get; private set; } = null!;
    private Disposal()
    {
    }

    private Disposal(Guid id, Guid lotId, decimal quantity, string reason, DateTimeOffset disposedAt)
    {
        if (lotId == Guid.Empty)
            throw new ArgumentException("Lot id is required.", nameof(lotId));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Disposal quantity must be greater than zero.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Disposal reason is required.", nameof(reason));

        Id = id;
        LotId = lotId;
        Quantity = quantity;
        Reason = reason.Trim();
        DisposedAt = disposedAt;
    }
    public Disposal(
        Guid id,
        Guid lotId,
        decimal quantity,
        string reason,
        DateTimeOffset disposedAt,
        Guid disposedBy)
        : this(id, lotId, quantity, reason, disposedAt)
    {
        if (disposedBy == Guid.Empty)
            throw new ArgumentException("Disposed by is required.", nameof(disposedBy));

        DisposedBy = disposedBy;
    }
    public static Disposal Create(Guid lotId, decimal quantity, string reason, DateTimeOffset disposedAt) =>
            new(Guid.NewGuid(), lotId, quantity, reason, disposedAt);

}
