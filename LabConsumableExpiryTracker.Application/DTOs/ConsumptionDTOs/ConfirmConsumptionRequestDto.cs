namespace LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;

public class ConfirmConsumptionRequestDto
{
    public Guid ItemId { get; set; }
    public decimal RequestedQuantity { get; set; }
    public List<ConsumptionAllocationRequestDto> Allocations { get; set; } = [];
}
 
