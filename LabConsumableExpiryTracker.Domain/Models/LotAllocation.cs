namespace LabConsumableExpiryTracker.Domain.Models;

public class LotAllocation
{
    public Guid LotId { get; }
    public decimal Quantity { get; }

    public LotAllocation(Guid lotId, decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Allocation quantity must be greater than zero.");
        }

        LotId = lotId;
        Quantity = quantity;
    }
}