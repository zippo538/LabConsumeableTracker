using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.FefoDTOs;


public class FefoAllocationPreviewDto
{
    public Guid JobId { get; set; }
    public Guid ItemId { get; set; }
    public UnitOfMeasure BaseUnit { get; set; }
    public decimal RequestedQuantity { get; set; }
    public decimal AllocatedQuantity { get; set; }
    public bool IsReservation { get; set; } = false;
    public IReadOnlyList<LotAllocationDto> Allocations { get; set; } = [];
    public DateTimeOffset PreviewedAt { get; set; }
}