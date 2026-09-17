using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;

namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface IDashboardAlertRepository
{
    Task<PagedResult<DashboardAlertDto>> GetAlertsAsync(
        DashboardAlertQuery query,
        DateTimeOffset evaluatedAt,
        CancellationToken ct = default);
}
