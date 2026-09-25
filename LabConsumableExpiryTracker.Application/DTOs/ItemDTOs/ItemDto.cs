using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;

public class ItemDto
{
    public Guid Id { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
    public Guid? ImageId {get;init;}

    public UnitOfMeasure BaseUnit { get; init; }

    public decimal MinimumStock { get; init; }

    public StockLevelStatus StockStatus {get;set;}

    public decimal TotalRemainingQuantity { get; set; }

    public bool IsLowStock => StockStatus != StockLevelStatus.Sufficient;

    public IReadOnlyCollection<LotSummaryDto> Lots { get; init; } = Array.Empty<LotSummaryDto>();
}