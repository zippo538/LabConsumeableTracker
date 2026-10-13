namespace LabConsumableExpiryTracker.Application.Common.Result;

/// <summary>
/// Kode error mesin-baca yang dikembalikan lewat <see cref="ServiceResult{T}.Code"/>.
/// Controller memetakan kode ini ke status HTTP (400/404/409).
/// </summary>
public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string JobNotFound = "JOB_NOT_FOUND";
    public const string LotNotFound = "LOT_NOT_FOUND";
    public const string NoPendingConsumption = "NO_PENDING_CONSUMPTION";
    public const string InsufficientStock = "INSUFFICIENT_STOCK";
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
    public const string InvalidJobState = "INVALID_JOB_STATE";
    public const string LotNotEligible = "LOT_NOT_ELIGIBLE";
    public const string LotExpired = "LOT_EXPIRED";
}
