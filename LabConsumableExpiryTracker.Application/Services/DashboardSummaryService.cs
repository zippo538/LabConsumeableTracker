using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Application.Services;

public class DashboardSummaryService(
    IItemRepository itemRepository,
    TimeProvider timeProvider,
    ILogger<DashboardAlertDto> logger)
    : IDashboardSummaryService
{
    public async Task<ServiceResult<DashboardSummaryDto>> GetSummaryAsync(Guid? itemId, CancellationToken ct = default)
    {
        logger.LogDebug("Calculating dashboard summary. ItemId filter: {ItemId}", itemId ?? (object)"Global");

        var now = timeProvider.GetUtcNow();
        var lowStockItemCount = 0;
        var validLotCount = 0;
        var expiringSoonLotCount = 0;
        var expiredLotCount = 0;
        var blockedLotCount = 0;

        if (itemId.HasValue)
        {
            var item = await itemRepository.GetByIdWithLotsAsync(itemId.Value, ct);
            if (item is null)
            {
                logger.LogWarning("Dashboard summary rejected because item {ItemId} was not found", itemId.Value);
                return ServiceResult<DashboardSummaryDto>.ErrorResult("Item tidak ditemukan.");
            }

            EvaluateItemMetrics(item, now, ref lowStockItemCount, ref validLotCount, ref expiringSoonLotCount, ref expiredLotCount, ref blockedLotCount);
        }
        else
        {
            var items = await itemRepository.GetAllWithLotsAsync(ct);

            foreach (var item in items)
            {
                EvaluateItemMetrics(item, now, ref lowStockItemCount, ref validLotCount, ref expiringSoonLotCount, ref expiredLotCount, ref blockedLotCount);
            }
        }

        var response = new DashboardSummaryDto
        {
            LowStockItemCount = lowStockItemCount,
            ValidLotCount = validLotCount,
            ExpiringSoonLotCount = expiringSoonLotCount,
            ExpiredLotCount = expiredLotCount,
            BlockedLotCount = blockedLotCount,
            EvaluatedAt = now
        };

        logger.LogInformation("Dashboard summary calculated successfully at {EvaluatedAt}", now);
        return ServiceResult<DashboardSummaryDto>.SuccessResult(response, "Dashboard summary retrieved successfully.");
    }
    private static void EvaluateItemMetrics(
        Item item,
        DateTimeOffset now,
        ref int lowStock,
        ref int validLot,
        ref int expiringSoon,
        ref int expired,
        ref int blocked)
    {
        decimal usableStock = 0;

        foreach (var lot in item.Lots)
        {
            if (lot.AdministrativeStatus is LotAdministrativeStatus.Quarantined or LotAdministrativeStatus.ManuallyBlocked)
            {
                blocked++;
                continue; 
            }

            if (lot.AdministrativeStatus != LotAdministrativeStatus.Active || lot.RemainingQuantity <= 0) continue;
            var condition = lot.GetExpiryCondition(now, item.ExpiringSoonDays);

            switch (condition)
            {
                case ExpiryCondition.ExpiringSoon:
                    expiringSoon++;
                    usableStock += lot.RemainingQuantity;
                    break;
                case ExpiryCondition.Expired:
                    expired++;
                    break;
                default:
                    validLot++;
                    usableStock += lot.RemainingQuantity;
                    break;
            }
        }
        if (usableStock < item.MinimumStock)
        {
            lowStock++;
        }
    }
}