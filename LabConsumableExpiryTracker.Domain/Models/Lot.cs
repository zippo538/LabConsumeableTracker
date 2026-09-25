using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Domain.Models;

public class Lot
{
    public Guid Id { get; }
    public Guid ItemId { get; private set; }
    public Item Item { get;}
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
        ReceivedAt = receivedAt.ToUniversalTime();
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

    public Consumption Consume(Guid jobId, decimal quantity, DateTimeOffset now, Guid consumedBy)
    {

        if (!IsEligible(now))
        {
            throw new InvalidOperationException(
                $"Lot '{LotNumber}' is no longer eligible for consumption.");
        }
        UpdateRemainingQuantity(quantity);

        if (RemainingQuantity <= 0)
        {
            AdministrativeStatus = LotAdministrativeStatus.Depleted;
        }

        return new Consumption(Guid.NewGuid(), jobId, Id, quantity, now, consumedBy);
    }
    public void UpdateLotNumber(string lotNumber)
    {
        if (string.IsNullOrWhiteSpace(lotNumber))
            throw new ArgumentException("Lot number is required.", nameof(lotNumber));

        var trimmed = lotNumber.Trim();
        if (LotNumber == trimmed) return;

        LotNumber = trimmed;
    }

    public void UpdateSupplierLotNumber(string? supplierLotNumber)
    {
        var trimmed = supplierLotNumber?.Trim();
        if (SupplierLotNumber == trimmed) return; 

        SupplierLotNumber = trimmed;
    }

    public void UpdateSupplierName(string? supplierName)
    {
        var trimmed = supplierName?.Trim();
        if (SupplierName == trimmed) return; 

        SupplierName = trimmed;
    }

    public void UpdateStorageLocation(string storageLocation)
    {
        if (string.IsNullOrWhiteSpace(storageLocation))
        {
            throw new ArgumentException("Storage location is required.", nameof(storageLocation));
        }
        var trimmed = storageLocation.Trim();
        if (StorageLocation == trimmed) return; 

        StorageLocation = trimmed;
    }
    public void UpdateExpiryDate(DateOnly expiryDate)
    {
        if (ExpiryDate == expiryDate) return; 
        ExpiryDate = expiryDate;
    }
    public void UpdateAdministrativeStatus(LotAdministrativeStatus status)
    {
        if (AdministrativeStatus == status)
            return;

        var isValidTransition =
            (AdministrativeStatus == LotAdministrativeStatus.Active &&
             (status == LotAdministrativeStatus.ManuallyBlocked ||
              status == LotAdministrativeStatus.Quarantined))
            ||
            (AdministrativeStatus == LotAdministrativeStatus.ManuallyBlocked &&
             (status == LotAdministrativeStatus.Active ||
              status == LotAdministrativeStatus.Quarantined))
            ||
            (AdministrativeStatus == LotAdministrativeStatus.Quarantined &&
             (status == LotAdministrativeStatus.Active ||
              status == LotAdministrativeStatus.ManuallyBlocked));

        if (!isValidTransition)
        {
            throw new InvalidOperationException(
                $"Cannot change lot status from {AdministrativeStatus} to {status}.");
        }

        AdministrativeStatus = status;
    }
    public bool IsEligible(DateTimeOffset now)
    {
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        return AdministrativeStatus == LotAdministrativeStatus.Active && RemainingQuantity > 0 && today < ExpiryDate;
    }

    public ExpiryCondition GetExpiryCondition(DateTimeOffset now, int warningDays)
    {
        if (warningDays < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(warningDays), "Warning days cannot be negative.");
        }
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        if (today >= ExpiryDate)
        {
            return ExpiryCondition.Expired;
        }

        var warningStart = ExpiryDate.AddDays(-warningDays);
        return today >= warningStart ? ExpiryCondition.ExpiringSoon : ExpiryCondition.Valid;
    }
    public void Dispose()
    {
        if (RemainingQuantity == 0)
        {
            AdministrativeStatus = LotAdministrativeStatus.Disposed;
        }
    }
    internal void SetConcurrencyToken(byte[] expectedRowVersion)
    {
        if (expectedRowVersion == null || expectedRowVersion.Length == 0)
            throw new ArgumentException("ExpectedRowVersion tidak boleh kosong.");

        RowVersion = expectedRowVersion;
    }


}

