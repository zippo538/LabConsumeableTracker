using LabConsumableExpiryTracker.Application.DTOs.ConsumptionHistoryDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LabConsumableExpiryTracker.Infrastructure.Repositories;

public class ConsumptionHistoryRepository(AppDbContext context) : IConsumptionHistoryRepository
{
    public async Task<(IReadOnlyList<ConsumptionListItemDto> Items, int TotalCount)> GetPagedAsync(
        Guid consumedBy,
        Guid? jobId,
        Guid? itemId,
        Guid? lotId,
        string? lotNumber,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query =
            from c in context.Consumptions.AsNoTracking()
            join l in context.Lots.AsNoTracking() on c.LotId equals l.Id
            join i in context.Items.AsNoTracking() on l.ItemId equals i.Id
            join j in context.Jobs.AsNoTracking() on c.JobId equals j.Id
            where c.ConsumedBy == consumedBy
            select new { c, l, i, j };

        if (jobId.HasValue)
        {
            query = query.Where(x => x.c.JobId == jobId.Value);
        }

        if (itemId.HasValue)
        {
            query = query.Where(x => x.l.ItemId == itemId.Value);
        }

        if (lotId.HasValue)
        {
            query = query.Where(x => x.c.LotId == lotId.Value);
        }

        if (!string.IsNullOrWhiteSpace(lotNumber))
        {
            query = query.Where(x =>
                EF.Functions.ILike(x.l.LotNumber, $"%{lotNumber}%"));
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.c.ConsumedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.c.ConsumedAt <= to.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.c.ConsumedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ConsumptionListItemDto
            {
                Id = x.c.Id,
                JobId = x.c.JobId,
                JobNumber = x.j.JobNumber,
                LotId = x.c.LotId,
                LotNumber = x.l.LotNumber,
                ItemId = x.l.ItemId,
                ItemName = x.i.Name,
                ItemCode = x.i.Code,
                Quantity = x.c.Quantity,
                BaseUnit = x.i.BaseUnit,
                ConsumedAt = x.c.ConsumedAt,
                ConsumedBy = x.c.ConsumedBy
            })
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<ConsumptionHistoryDetailDto?> GetDetailAsync(
        Guid consumptionId,
        Guid consumedBy,
        CancellationToken ct = default)
    {
        return await (
            from c in context.Consumptions.AsNoTracking()
            join l in context.Lots.AsNoTracking()
                on c.LotId equals l.Id
            join i in context.Items.AsNoTracking()
                on l.ItemId equals i.Id
            join j in context.Jobs.AsNoTracking()
                on c.JobId equals j.Id
            where c.Id == consumptionId
                  && c.ConsumedBy == consumedBy
            select new ConsumptionHistoryDetailDto
            {
                Id = c.Id,

                JobId = c.JobId,
                JobNumber = j.JobNumber,
                JobDescription = j.Description!,

                ItemId = l.ItemId,
                ItemCode = i.Code,
                ItemName = i.Name,
                BaseUnit = i.BaseUnit,

                LotId = c.LotId,
                LotNumber = l.LotNumber,
                ExpiryDate = l.ExpiryDate,
                StorageLocation = l.StorageLocation,

                Quantity = c.Quantity,

                ConsumedBy = c.ConsumedBy,
                ConsumedAt = c.ConsumedAt
            }
        ).FirstOrDefaultAsync(ct);
    }
}