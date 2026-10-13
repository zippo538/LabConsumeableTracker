using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Domain.Models;

public class Lot(
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
    // Inisialisasi properti langsung dari parameter primary constructor
    public Guid Id { get; } = id;
    public Guid ItemId { get; private set; } = itemId;
    
    // Fix Warning CS8618: EF Core akan mengisi navigasi properti ini nanti.
    public Item Item { get; private set; } = null!; 

    public string LotNumber { get; private set; } = lotNumber.Trim();
    public string? SupplierLotNumber { get; private set; } = supplierLotNumber?.Trim();
    public DateTimeOffset ReceivedAt { get; private set; } = receivedAt.ToUniversalTime();
    public string? SupplierName { get; private set; } = supplierName?.Trim();
    
    public decimal InitialQuantity { get; private set; } = initialQuantity;
    
    // Validasi dipindahkan ke helper statis agar tetap berjalan saat inisialisasi
    public decimal RemainingQuantity { get; private set; } = ValidateRemainingQuantity(remainingQuantity, initialQuantity);
    
    public DateOnly ExpiryDate { get; private set; } = expiryDate;
    public string StorageLocation { get; private set; } = storageLocation.Trim();
    public LotAdministrativeStatus AdministrativeStatus { get; private set; } = administrativeStatus;
    
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; private set; } = [];

    private static decimal ValidateRemainingQuantity(decimal remaining, decimal initial)
    {
        if (remaining < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(remaining), "Remaining quantity cannot be negative.");
        }
        if (remaining > initial)
        {
            throw new ArgumentException("Remaining quantity cannot exceed initial quantity.", nameof(remaining));
        }
        return remaining;
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
            throw new InvalidOperationException("Consumption quantity exceeds remaining quantity.");
        }
        RemainingQuantity -= quantity;
    }

    public Consumption Consume(Guid jobId, decimal quantity, DateTimeOffset now, Guid consumedBy)
    {
        DeductStock(quantity, now);

        return new Consumption(Guid.NewGuid(), jobId, Id, quantity, now, consumedBy);
    }

    public void DeductStock(decimal quantity, DateTimeOffset now)
    {
        if (!IsEligible(now))
        {
            throw new InvalidOperationException($"Lot '{LotNumber}' is no longer eligible for consumption.");
        }

        UpdateRemainingQuantity(quantity);

        if (RemainingQuantity <= 0)
        {
            AdministrativeStatus = LotAdministrativeStatus.Depleted;
        }
    }

    public bool UpdateLotNumber(string lotNumber)
    {
        if (string.IsNullOrWhiteSpace(lotNumber))
            throw new ArgumentException("Lot number is required.", nameof(lotNumber));
            
        var trimmed = lotNumber.Trim();
        if (LotNumber == trimmed) return false;
        
        LotNumber = trimmed;
        return true;
    }

    public bool UpdateSupplierLotNumber(string? supplierLotNumber)
    {
        var trimmed = supplierLotNumber?.Trim();
        if (SupplierLotNumber == trimmed) return false;
        
        SupplierLotNumber = trimmed;
        return true;
    }

    public bool UpdateSupplierName(string? supplierName)
    {
        var trimmed = supplierName?.Trim();
        if (SupplierName == trimmed) return false;
        
        SupplierName = trimmed;
        return true;
    }

    public bool UpdateStorageLocation(string storageLocation)
    {
        if (string.IsNullOrWhiteSpace(storageLocation))
        {
            throw new ArgumentException("Storage location is required.", nameof(storageLocation));
        }
        
        var trimmed = storageLocation.Trim();
        if (StorageLocation == trimmed) return false;
        
        StorageLocation = trimmed;
        return true;
    }

    public bool UpdateExpiryDate(DateOnly expiryDate)
    {
        if (ExpiryDate == expiryDate) return false;
        
        ExpiryDate = expiryDate;
        return true;
    }

    public bool UpdateAdministrativeStatus(LotAdministrativeStatus status)
    {
        if (AdministrativeStatus == status)
            return false;
            
        var isValidTransition =
            (AdministrativeStatus == LotAdministrativeStatus.Active &&
             status is LotAdministrativeStatus.ManuallyBlocked or LotAdministrativeStatus.Quarantined)
            ||
            (AdministrativeStatus == LotAdministrativeStatus.ManuallyBlocked &&
             status is LotAdministrativeStatus.Active or LotAdministrativeStatus.Quarantined)
            ||
            (AdministrativeStatus == LotAdministrativeStatus.Quarantined &&
             status is LotAdministrativeStatus.Active or LotAdministrativeStatus.ManuallyBlocked);
             
        if (!isValidTransition)
        {
            throw new InvalidOperationException($"Cannot change lot status from {AdministrativeStatus} to {status}.");
        }
        
        AdministrativeStatus = status;
        return true;
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