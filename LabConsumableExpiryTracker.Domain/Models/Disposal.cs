namespace LabConsumableExpiryTracker.Domain.Models;

public class Disposal(
    Guid id,
    Guid lotId,
    decimal quantity,
    string reason,
    DateTimeOffset disposedAt,
    Guid disposedBy)
{
    public Guid Id { get; } = id;
    public Guid LotId { get; } = ValidateGuid(lotId, nameof(lotId));
    public decimal Quantity { get; } = ValidateQuantity(quantity);
    public string Reason { get; } = ValidateReason(reason);
    public DateTimeOffset DisposedAt { get; } = disposedAt;
    public Guid DisposedBy { get; } = ValidateGuid(disposedBy, nameof(disposedBy));

    public Lot Lot { get; private set; } = null!;

    public static Disposal Create(
        Guid lotId, 
        decimal quantity, 
        string reason, 
        DateTimeOffset disposedAt, 
        Guid disposedBy) =>
        new(Guid.NewGuid(), lotId, quantity, reason, disposedAt, disposedBy);

    private static Guid ValidateGuid(Guid value, string paramName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"{paramName} is required.", paramName);
        return value;
    }

    private static decimal ValidateQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Disposal quantity must be greater than zero.");
        return quantity;
    }

    private static string ValidateReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Disposal reason is required.", nameof(reason));
        return reason.Trim();
    }
}