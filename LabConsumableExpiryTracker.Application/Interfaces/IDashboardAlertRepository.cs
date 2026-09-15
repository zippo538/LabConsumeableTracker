using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;

namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface IDashboardAlertRepository
{
    Task<PagedResult<DashboardAlertDto>> GetAlertsAsync(
        DashboardAlertQuery query,
        DateTimeOffset evaluatedAt,
        CancellationToken ct = default);
}
