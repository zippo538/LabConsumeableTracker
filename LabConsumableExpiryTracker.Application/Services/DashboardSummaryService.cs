using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Application.Services
{
    public class DashboardSummaryService : IDashboardSummaryService
    {
        private readonly IItemRepository _itemRepository;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<DashboardAlertDto> _logger;
        public DashboardSummaryService(
            IItemRepository itemRepository,
            TimeProvider timeProvider,
            ILogger<DashboardAlertDto> logger)
        {
            _itemRepository = itemRepository;
            _timeProvider = timeProvider;
            _logger = logger;
        }
        public async Task<ServiceResult<DashboardSummaryDto>> GetSummaryAsync(Guid? itemId, CancellationToken ct = default)
        {
            _logger.LogDebug("Calculating dashboard summary. ItemId filter: {ItemId}", itemId ?? (object)"Global");

            var now = _timeProvider.GetUtcNow();
            int lowStockItemCount = 0;
            int expiringSoonLotCount = 0;
            int expiredLotCount = 0;
            int blockedLotCount = 0;

            if (itemId.HasValue)
            {
                var item = await _itemRepository.GetByIdWithLotsAsync(itemId.Value, ct);
                if (item is null)
                {
                    _logger.LogWarning("Dashboard summary rejected because item {ItemId} was not found", itemId.Value);
                    return ServiceResult<DashboardSummaryDto>.ErrorResult("Item tidak ditemukan.");
                }

                EvaluateItemMetrics(item, now, ref lowStockItemCount, ref expiringSoonLotCount, ref expiredLotCount, ref blockedLotCount);
            }
            else
            {
                // Menggunakan method yang ada: memuat semua item beserta lots-nya
                var items = await _itemRepository.GetAllWithLotsAsync(ct);

                foreach (var item in items)
                {
                    EvaluateItemMetrics(item, now, ref lowStockItemCount, ref expiringSoonLotCount, ref expiredLotCount, ref blockedLotCount);
                }
            }

            var response = new DashboardSummaryDto
            {
                LowStockItemCount = lowStockItemCount,
                ExpiringSoonLotCount = expiringSoonLotCount,
                ExpiredLotCount = expiredLotCount,
                BlockedLotCount = blockedLotCount,
                EvaluatedAt = now
            };

            _logger.LogInformation("Dashboard summary calculated successfully at {EvaluatedAt}", now);
            return ServiceResult<DashboardSummaryDto>.SuccessResult(response, "Dashboard summary retrieved successfully.");
        }
        private void EvaluateItemMetrics(
       Item item,
       DateTimeOffset now,
       ref int lowStock,
       ref int expiringSoon,
       ref int expired,
       ref int blocked)
        {
            decimal usableStock = 0;

            foreach (var lot in item.Lots)
            {
                // FR-03-05: Blocked mencakup Quarantined atau ManuallyBlocked
                if (lot.AdministrativeStatus == LotAdministrativeStatus.Quarantined ||
                    lot.AdministrativeStatus == LotAdministrativeStatus.ManuallyBlocked)
                {
                    blocked++;
                    continue; // Kategori tumpang tindih tidak dijumlahkan ke expired/expiring
                }

                // Hanya Lot Active dengan sisa stok yang dievaluasi expiry-nya
                if (lot.AdministrativeStatus == LotAdministrativeStatus.Active && lot.RemainingQuantity > 0)
                {
                    // Menggunakan domain method persis seperti di LotService
                    var condition = lot.GetExpiryCondition(now, item.ExpiringSoonDays);

                    if (condition == ExpiryCondition.ExpiringSoon)
                    {
                        expiringSoon++;
                        usableStock += lot.RemainingQuantity; // ExpiringSoon masih dianggap eligible
                    }
                    else if (condition == ExpiryCondition.Expired)
                    {
                        expired++;
                    }
                    else // Valid
                    {
                        usableStock += lot.RemainingQuantity;
                    }
                }
            }

            // D-06: Low stock threshold (mengusulkan operator <)
            if (usableStock < item.MinimumStock)
            {
                lowStock++;
            }
        }
    }

}
