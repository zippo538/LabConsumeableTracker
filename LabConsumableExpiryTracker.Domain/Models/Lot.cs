using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Domain.Models;

public class Lot
{

    public Guid Id { get; private set; }
    public Guid ItemId { get; private set; }
    public string LotNumber { get; private set; }
    public string? SupplierLotNumber { get; private set; }
    public DateTimeOffset ReceivedAt { get; private set; }
    public string? SupplierName { get; private set; }
    public decimal InitialQuantity { get; private set; }
    public decimal RemainingQuantity { get; private set; }
    public DateOnly ExpiryDate { get; private set; }
    public string StorageLocation { get; private set; }
    public LotStatus Status { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public Lot(
    Guid id,
    Guid itemId,
    string lotNumber,
    string? supplierLotNumber,
    DateTimeOffset receivedAt,
    string? supplierName,
    decimal initialQuantity,
    decimal remainingQuantity,
    DateOnly expiryDate,
    string storageLocation,
    LotStatus status)
    {
        Id = id;
        ItemId = itemId;
        LotNumber = lotNumber.Trim();
        SupplierLotNumber = supplierLotNumber?.Trim();
        ReceivedAt = receivedAt;
        SupplierName = supplierName?.Trim();
        InitialQuantity = initialQuantity;
        RemainingQuantity = remainingQuantity;
        ExpiryDate = expiryDate;
        StorageLocation = storageLocation.Trim();
        Status = status;
    }
    public void UpdateRemainingQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        if (quantity > RemainingQuantity)
        {
            throw new InvalidOperationException(
                "Consumption quantity exceeds remaining quantity.");
        }

        RemainingQuantity -= quantity;
    }
    public void UpdateStorageLocation(string storageLocation)
    {
        if (string.IsNullOrWhiteSpace(storageLocation))
        {
            throw new ArgumentException(
                "Storage location is required.",
                nameof(storageLocation));
        }

        StorageLocation = storageLocation.Trim();
    }
    public bool IsEligible(DateTimeOffset now)
    {
        var today = DateOnly.FromDateTime(
            now.UtcDateTime);

        return Status == LotStatus.Active
            && RemainingQuantity > 0
            && ExpiryDate >= today;
    }
}

