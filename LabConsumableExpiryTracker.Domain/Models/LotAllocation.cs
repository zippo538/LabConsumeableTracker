namespace LabConsumableExpiryTracker.Domain.Models;
public class LotAllocation(Guid lotId, decimal quantity)
{
    public Guid LotId { get; } = lotId;
    public decimal Quantity { get; } = ValidateQuantity(quantity);
    private static decimal ValidateQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Allocation quantity must be greater than zero.");
        }
        return quantity;
    }
}