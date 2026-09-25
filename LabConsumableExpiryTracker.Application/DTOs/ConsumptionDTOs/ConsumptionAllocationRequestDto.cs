namespace LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;

public class ConsumptionAllocationRequestDto
{
    public Guid LotId { get; init; }
    public decimal Quantity { get; init; }
    public byte[] ExpectedRowVersion { get; init; } = [];
}
