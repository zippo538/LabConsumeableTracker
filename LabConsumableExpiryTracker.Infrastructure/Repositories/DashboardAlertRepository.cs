using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LabConsumableExpiryTracker.Infrastructure.Repositories;

/// <summary>
/// Produces dashboard read models only. No mutation is performed here or by the
/// quick-action links returned to the client.
/// </summary>
public class DashboardAlertRepository : IDashboardAlertRepository
{
    private readonly AppDbContext _context;

    public DashboardAlertRepository(AppDbContext context) => _context = context;

    public async Task<PagedResult<DashboardAlertDto>> GetAlertsAsync(
        DashboardAlertQuery query,
        DateTimeOffset evaluatedAt,
        CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(evaluatedAt.UtcDateTime);

        // BR-07: low stock is based on usable (active, unexpired) stock, not
        // on all RemainingQuantity values.
        var lowStock = _context.Items.AsNoTracking()
            .Select(item => new
            {
                Item = item,
                UsableStock = _context.Lots
                    .Where(lot => lot.ItemId == item.Id
                        && lot.AdministrativeStatus == LotAdministrativeStatus.Active
                        && lot.RemainingQuantity > 0
                        && lot.ExpiryDate >= today)
                    .Sum(lot => (decimal?)lot.RemainingQuantity) ?? 0m
            })
            .Where(x => x.UsableStock < x.Item.MinimumStock)
            .Select(x => new DashboardAlertDto
            {
                AlertType = DashboardAlertType.LowStock,
                ResourceType = AlertResourceType.Item,
                ResourceId = x.Item.Id,
                ItemId = x.Item.Id,
                LotId = null,
                Message = "Usable stock is below the minimum stock level.",
                EvaluatedAt = evaluatedAt
            });

        var expiringSoon =
            from lot in _context.Lots.AsNoTracking()
            join item in _context.Items.AsNoTracking() on lot.ItemId equals item.Id
            where lot.RemainingQuantity > 0
                && lot.AdministrativeStatus == LotAdministrativeStatus.Active
                && lot.ExpiryDate >= today
                && lot.ExpiryDate <= today.AddDays(item.ExpiringSoonDays)
            select new DashboardAlertDto
            {
                AlertType = DashboardAlertType.ExpiringSoon,
                ResourceType = AlertResourceType.Lot,
                ResourceId = lot.Id,
                ItemId = lot.ItemId,
                LotId = lot.Id,
                Message = "Lot is expiring soon.",
                EvaluatedAt = evaluatedAt
            };

        var expired = _context.Lots.AsNoTracking()
            .Where(lot => lot.RemainingQuantity > 0 && lot.ExpiryDate < today)
            .Select(lot => new DashboardAlertDto
            {
                AlertType = DashboardAlertType.Expired,
                ResourceType = AlertResourceType.Lot,
                ResourceId = lot.Id,
                ItemId = lot.ItemId,
                LotId = lot.Id,
                Message = "Lot has expired.",
                EvaluatedAt = evaluatedAt
            });

        var blocked = _context.Lots.AsNoTracking()
            .Where(lot => lot.AdministrativeStatus == LotAdministrativeStatus.ManuallyBlocked
                || lot.AdministrativeStatus == LotAdministrativeStatus.Quarantined)
            .Select(lot => new DashboardAlertDto
            {
                AlertType = DashboardAlertType.Blocked,
                ResourceType = AlertResourceType.Lot,
                ResourceId = lot.Id,
                ItemId = lot.ItemId,
                LotId = lot.Id,
                Message = "Lot is blocked or quarantined.",
                EvaluatedAt = evaluatedAt
            });

        IQueryable<DashboardAlertDto> alerts = query.Type switch
        {
            DashboardAlertType.LowStock => lowStock,
            DashboardAlertType.ExpiringSoon => expiringSoon,
            DashboardAlertType.Expired => expired,
            DashboardAlertType.Blocked => blocked,
            _ => lowStock.Concat(expiringSoon).Concat(expired).Concat(blocked)
        };

        if (query.ItemId.HasValue)
            alerts = alerts.Where(x => x.ItemId == query.ItemId.Value);

        var totalCount = await alerts.CountAsync(ct);
        var items = await alerts
            .OrderBy(x => x.AlertType)
            .ThenBy(x => x.ResourceType)
            .ThenBy(x => x.ResourceId)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<DashboardAlertDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }
}
