using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;

public sealed class DisposalAuditDto
{
    public Guid Id { get; init; }
    public Guid LotId { get; init; }
    public Guid ItemId { get; init; }
    public string LotNumber { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public UnitOfMeasure BaseUnit { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset DisposedAt { get; init; }
    public Guid DisposedBy { get; init; }
}
