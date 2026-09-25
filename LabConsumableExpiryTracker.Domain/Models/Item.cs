using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Domain.Models;

public class Item
{
    private readonly List<Lot> _lots = [];
    public Guid Id { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public Guid? ImageId { get; private set; }
    public UnitOfMeasure BaseUnit { get; private set; }
    public decimal MinimumStock { get; private set; }
    public int ExpiringSoonDays { get; private set; }
    public IReadOnlyCollection<Lot> Lots => _lots.AsReadOnly();



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

        // Terapkan perubahan jika semua valid
        Name = name;
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
