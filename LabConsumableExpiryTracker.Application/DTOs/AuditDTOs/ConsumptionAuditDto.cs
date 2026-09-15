using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.AuditDTOs;

public sealed class ConsumptionAuditDto
{
    public Guid Id { get; init; }
    public Guid JobId { get; init; }
    public Guid LotId { get; init; }
    public Guid ItemId { get; init; }
    public string LotNumber { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public UnitOfMeasure BaseUnit { get; init; }
    public DateTimeOffset ConsumedAt { get; init; }
    public Guid ConsumedBy { get; init; }
}
