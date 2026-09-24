using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Domain.Models;

public class Item
{
    private readonly List<Lot> _lots = [];
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid? ImageId { get; private set; }
    public UnitOfMeasure BaseUnit { get; private set; }
    public decimal MinimumStock { get; private set; }
    public int ExpiringSoonDays { get; private set; }
    public IReadOnlyCollection<Lot> Lots => _lots.AsReadOnly();


    private Item()
    {
    }
    public Item(
        Guid id,
        string code,
        string name,
        UnitOfMeasure baseUnit,
        decimal minimumStock,
        int expiringSoonDays)
    {
        Id = id;
        Code = code.Trim();
        Name = name.Trim();
        BaseUnit = baseUnit;
        MinimumStock = minimumStock;
        ExpiringSoonDays = expiringSoonDays;
    }
    
  
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
                "Item name is required.",
                nameof(name));
        }

        if (minimumStock < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumStock));
        }

        if (expiringSoonDays < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expiringSoonDays));
        }

        Name = name.Trim();
        BaseUnit = baseUnit;
        MinimumStock = minimumStock;
        ExpiringSoonDays = expiringSoonDays;
    }
    public StockLevelStatus GetStockStatus(
        decimal totalUsableQuantity)
    {
        if (totalUsableQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalUsableQuantity));
        }

        if (totalUsableQuantity == 0)
        {
            return StockLevelStatus.OutOfStock;
        }

        if (totalUsableQuantity < MinimumStock)
        {
            return StockLevelStatus.LowStock;
        }

        if (totalUsableQuantity == MinimumStock)
        {
            return StockLevelStatus.AtMinimum;
        }

        return StockLevelStatus.Sufficient;
    }

    public bool IsLowStock(decimal totalUsableQuantity)
    {
        if (totalUsableQuantity < MinimumStock)
        {
            return true;
        }

        return false;
    }

}
