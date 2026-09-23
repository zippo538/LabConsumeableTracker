using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;

/// <summary>Read-only filters shared by the audit endpoints.</summary>
public sealed record AuditQuery
{
    public AuditEventType? EventType { get; init; }
    public Guid? ItemId { get; init; }
    public Guid? LotId { get; init; }
    public Guid? JobId { get; init; }
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
