using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;

public sealed record DashboardAlertDto
{
    public DashboardAlertType AlertType { get; init; }
    public AlertResourceType ResourceType { get; init; }
    public Guid ResourceId { get; init; }
    public Guid ItemId { get; init; }
    public Guid? LotId { get; init; }
    public string? ItemName { get; init; }
    public string? ItemCode { get; init; }
    public string? LotNumber { get; init; }
    public string? LotName => LotNumber;
    public decimal? RemainingQuantity { get; init; }
    public UnitOfMeasure? BaseUnit { get; init; }
    public DateOnly? ExpiryDate { get; init; }
    public DateOnly? Expiry => ExpiryDate;
    public string Message { get; init; } = string.Empty;
    public DateTimeOffset EvaluatedAt { get; init; }
    public IReadOnlyList<QuickActionDto> AllowedActions { get; init; } = Array.Empty<QuickActionDto>();
}
