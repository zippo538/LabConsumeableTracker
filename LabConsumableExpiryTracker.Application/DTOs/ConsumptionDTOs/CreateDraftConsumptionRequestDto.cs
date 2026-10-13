namespace LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;

public class CreateDraftConsumptionRequestDto
{
    public Guid ItemId { get; init; }
    public List<ConsumptionAllocationRequestDto> Allocations { get; init; } = [];
}