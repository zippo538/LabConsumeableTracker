using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.AuditDTOs;

public sealed class AuditEventDto
{
    public AuditEventType EventType { get; init; }
    public Guid EventId { get; init; }
    public Guid ItemId { get; init; }
    public string ItemName {get;init;} = string.Empty;
    public string ItemCode {get;init;} = string.Empty;
    public Guid LotId { get; init; }
    public string LotNumber { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public UnitOfMeasure BaseUnit { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public Guid ActorId { get; init; }
    public string FullName {get;init;}= string.Empty;
    public Guid? JobId { get; init; }
    public string JobNumber {get;init;} = string.Empty;
    
    public string? Reason { get; init; }
}
