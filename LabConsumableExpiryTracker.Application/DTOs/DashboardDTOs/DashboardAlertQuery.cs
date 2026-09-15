using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;

public sealed record DashboardAlertQuery
{
    public DashboardAlertType? Type { get; init; }
    public Guid? ItemId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
