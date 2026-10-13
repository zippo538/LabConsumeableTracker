using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;

public class DraftConsumptionResponseDto
{
    public Guid JobId { get; set; }
    public Guid ItemId { get; set; }
    public List<DraftConsumptionRecordDto> Consumptions { get; set; } = [];
}

public class DraftConsumptionRecordDto
{
    public Guid Id { get; set; }
    public Guid LotId { get; set; }
    public decimal Quantity { get; set; }
    public ConsumptionStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}