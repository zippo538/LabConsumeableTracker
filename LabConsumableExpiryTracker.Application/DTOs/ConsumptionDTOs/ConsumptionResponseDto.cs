using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;

public class ConsumptionResponseDto
{
    public Guid JobId { get; init; }
    public Guid ItemId { get; init; }    
    public UnitOfMeasure BaseUnit { get; init; }
    public decimal TotalConsumedQuantity { get; init; }
    public List<ConsumptionRecordDto> Consumptions { get; init; } = [];
    public List<UpdatedLotDto> UpdatedLots { get; init; } = [];
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
