namespace LabConsumableExpiryTracker.Application.DTOs.FefoDtos;

public class RequestFefoAllocationDto
{
    public Guid ItemId { get; set; }
    public decimal RequestedQuantity { get; set; }
}