namespace LabConsumableExpiryTracker.Application.DTOs.FefoDTOs;

public class RequestFefoAllocationDto
{
    public Guid ItemId { get; set; }
    public decimal RequestedQuantity { get; set; }
}