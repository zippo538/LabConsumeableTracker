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
    public LotAdministrativeStatus AdministrativeStatus { get; private set; }
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
        LotAdministrativeStatus administrativeStatus)
    {
        if (remainingQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(remainingQuantity), "Remaining quantity cannot be negative.");
        }

        if (remainingQuantity > initialQuantity)
        {
            throw new ArgumentException("Remaining quantity cannot exceed initial quantity.", nameof(remainingQuantity));
        }

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
        AdministrativeStatus = administrativeStatus;
    }

    public static Lot Create(
        Guid itemId,
        string lotNumber,
        string? supplierLotNumber,
        DateTimeOffset receivedAt,
        string? supplierName,
        decimal initialQuantity,
        DateOnly expiryDate,
        string storageLocation)
    {
        if (initialQuantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialQuantity), "Initial quantity must be greater than zero.");
        }

        return new Lot(
            Guid.NewGuid(),
            itemId,
            lotNumber,
            supplierLotNumber,
            receivedAt,
            supplierName,
            initialQuantity,
            initialQuantity,
            expiryDate,
            storageLocation,
            LotAdministrativeStatus.Active);
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

        return AdministrativeStatus == LotAdministrativeStatus.Active
            && RemainingQuantity > 0
            && ExpiryDate >= today;
    }
}

