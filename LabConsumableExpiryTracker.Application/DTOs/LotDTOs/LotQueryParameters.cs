namespace LabConsumableExpiryTracker.Application.DTOs.LotDTOs;

public class LotQueryParameters
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public Guid? ItemId { get; init; }
    public string? Status { get; init; }
    public string? ExpiryCondition { get; init; }
}