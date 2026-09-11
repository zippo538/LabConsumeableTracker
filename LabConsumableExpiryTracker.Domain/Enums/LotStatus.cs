namespace LabConsumableExpiryTracker.Domain.Enums;

public enum LotStatus
{
    Active = 1,
    Quarantined = 2,
    ManuallyBlocked = 3,
    Depleted = 4,
    Disposed = 5
}
