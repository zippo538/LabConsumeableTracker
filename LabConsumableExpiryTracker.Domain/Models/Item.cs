using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Domain.Models;

public class Item(
    Guid id,
    string code,
    string name,
    UnitOfMeasure baseUnit,
    decimal minimumStock,
    int expiringSoonDays)
{
    private readonly List<Lot> _lots = [];
    public Guid Id { get; private set; } = id;
    public string Code { get; private set; } = code.Trim();
    public string Name { get; private set; } = name.Trim();
    public Guid? ImageId { get; private set; }
    public UnitOfMeasure BaseUnit { get; private set; } = baseUnit;
    public decimal MinimumStock { get; private set; } = minimumStock;
    public int ExpiringSoonDays { get; private set; } = expiringSoonDays;
    public IReadOnlyCollection<Lot> Lots => _lots.AsReadOnly();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    

    public void SetImageId(Guid? imageId)
    {
        ImageId = imageId;
    }


    public void UpdateDetails(
        string name,
        UnitOfMeasure baseUnit,
        decimal minimumStock,
        int expiringSoonDays)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Item name cannot be empty or whitespace.",
                nameof(name));
        }

        if (expiringSoonDays < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expiringSoonDays));
        }


        if (minimumStock < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumStock),
                "Minimum stock cannot be negative.");
        }

        Name = name;
        BaseUnit = baseUnit;
        MinimumStock = minimumStock;
        ExpiringSoonDays = expiringSoonDays;
    }

    public StockLevelStatus GetStockStatus(
        decimal totalUsableQuantity)
    {
        switch (totalUsableQuantity)
        {
            case < 0:
                throw new ArgumentOutOfRangeException(
                    nameof(totalUsableQuantity));
            case 0:
                return StockLevelStatus.OutOfStock;
        }

        if (totalUsableQuantity < MinimumStock)
        {
            return StockLevelStatus.LowStock;
        }

        return totalUsableQuantity == MinimumStock ? StockLevelStatus.AtMinimum : StockLevelStatus.Sufficient;
    }

    public bool IsLowStock(decimal totalUsableQuantity)
    {
        return totalUsableQuantity < MinimumStock;
    }
}