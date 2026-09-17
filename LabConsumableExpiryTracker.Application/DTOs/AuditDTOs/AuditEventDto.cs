using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;

/// <summary>Unified, immutable read model for Consumption and Disposal.</summary>
public sealed class AuditEventDto
{
    public AuditEventType EventType { get; init; }
    public Guid EventId { get; init; }
    public Guid ItemId { get; init; }
    public Guid LotId { get; init; }
    public string LotNumber { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public UnitOfMeasure BaseUnit { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public Guid ActorId { get; init; }
    public Guid? JobId { get; init; }
    public string? Reason { get; init; }
}
