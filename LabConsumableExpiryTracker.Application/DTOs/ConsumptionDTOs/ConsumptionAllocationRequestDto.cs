namespace LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;

public class ConsumptionAllocationRequestDto
{
    public Guid LotId { get; set; }
    public decimal Quantity { get; set; }
    public byte[] ExpectedRowVersion { get; set; } = [];
}
