namespace LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;

public class ConfirmConsumptionRequestDto
{
    public Guid ItemId { get; init; }
    public decimal RequestedQuantity { get; init; }
    public List<ConsumptionAllocationRequestDto> Allocations { get; init; } = [];
}
 
