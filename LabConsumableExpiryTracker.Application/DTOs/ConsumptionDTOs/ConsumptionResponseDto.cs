using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;

public class ConsumptionResponseDto
{
    public Guid JobId { get; set; }
    public Guid ItemId { get; set; }
    public UnitOfMeasure BaseUnit { get; set; }
    public decimal TotalConsumedQuantity { get; set; }
    public List<ConsumptionRecordDto> Consumptions { get; set; } = [];
    public List<UpdatedLotDto> UpdatedLots { get; set; } = [];
}
 
public class ConsumptionRecordDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid LotId { get; set; }
    public decimal Quantity { get; set; }
    public DateTimeOffset ConsumedAt { get; set; }
    public Guid ConsumedBy { get; set; }
}
 
public class UpdatedLotDto
{
    public Guid LotId { get; set; }
    public decimal RemainingQuantity { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
