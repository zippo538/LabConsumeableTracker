using LabConsumableExpiryTracker.Application.DTOs.AuditDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LabConsumableExpiryTracker.Infrastructure.Repositories;

/// <summary>
/// Read-only projections. Filtering takes place in SQL before Count/Skip/Take.
/// It assumes Consumption has Id, LotId, JobId, Quantity, ConsumedAt and ConsumedBy,
/// and Disposal has DisposedBy. These actor IDs are required by SRS FR-08-02.
/// </summary>
public sealed class AuditRepository : IAuditRepository
{
    private readonly AppDbContext _context;

    public AuditRepository(AppDbContext context) => _context = context;

    public async Task<PagedResult<AuditEventDto>> GetAuditEventsAsync(AuditQuery query, CancellationToken ct = default)
    {
        var consumptions =
            from consumption in _context.Consumptions.AsNoTracking()
            join lot in _context.Lots.AsNoTracking() on consumption.LotId equals lot.Id
            join item in _context.Items.AsNoTracking() on lot.ItemId equals item.Id
            select new AuditEventDto
            {
                EventType = AuditEventType.Consumption,
                EventId = consumption.Id,
                ItemId = lot.ItemId,
                LotId = lot.Id,
                LotNumber = lot.LotNumber,
                Quantity = consumption.Quantity,
                BaseUnit = item.BaseUnit,
                OccurredAt = consumption.ConsumedAt,
                ActorId = consumption.ConsumedBy,
                JobId = consumption.JobId,
                Reason = null
            };

        var disposals =
            from disposal in _context.Disposals.AsNoTracking()
            join lot in _context.Lots.AsNoTracking() on disposal.LotId equals lot.Id
            join item in _context.Items.AsNoTracking() on lot.ItemId equals item.Id
            select new AuditEventDto
            {
                EventType = AuditEventType.Disposal,
                EventId = disposal.Id,
                ItemId = lot.ItemId,
                LotId = lot.Id,
                LotNumber = lot.LotNumber,
                Quantity = disposal.Quantity,
                BaseUnit = item.BaseUnit,
                OccurredAt = disposal.DisposedAt,
                ActorId = disposal.DisposedBy,
                JobId = null,
                Reason = disposal.Reason
            };

        IQueryable<AuditEventDto> events = query.EventType switch
        {
            AuditEventType.Consumption => consumptions,
            AuditEventType.Disposal => disposals,
            _ => consumptions.Concat(disposals)
        };

        events = ApplyEventFilters(events, query);
        var totalCount = await events.CountAsync(ct);
        var items = await events
            .OrderByDescending(x => x.OccurredAt)
            .ThenBy(x => x.EventType)
            .ThenBy(x => x.EventId)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<AuditEventDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<ConsumptionAuditDto>> GetConsumptionsAsync(AuditQuery query, CancellationToken ct = default)
    {
        var source =
            from consumption in _context.Consumptions.AsNoTracking()
            join lot in _context.Lots.AsNoTracking() on consumption.LotId equals lot.Id
            join item in _context.Items.AsNoTracking() on lot.ItemId equals item.Id
            select new ConsumptionAuditDto
            {
                Id = consumption.Id,
                JobId = consumption.JobId,
                LotId = lot.Id,
                ItemId = lot.ItemId,
                LotNumber = lot.LotNumber,
                Quantity = consumption.Quantity,
                BaseUnit = item.BaseUnit,
                ConsumedAt = consumption.ConsumedAt,
                ConsumedBy = consumption.ConsumedBy
            };

        source = ApplyConsumptionFilters(source, query);
        var totalCount = await source.CountAsync(ct);
        var items = await source.OrderByDescending(x => x.ConsumedAt).ThenBy(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return new PagedResult<ConsumptionAuditDto> { Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = totalCount };
    }

    public Task<ConsumptionAuditDto?> GetConsumptionByIdAsync(Guid id, CancellationToken ct = default) =>
        (from consumption in _context.Consumptions.AsNoTracking()
         join lot in _context.Lots.AsNoTracking() on consumption.LotId equals lot.Id
         join item in _context.Items.AsNoTracking() on lot.ItemId equals item.Id
         where consumption.Id == id
         select new ConsumptionAuditDto
         {
             Id = consumption.Id, JobId = consumption.JobId, LotId = lot.Id, ItemId = lot.ItemId,
             LotNumber = lot.LotNumber, Quantity = consumption.Quantity, BaseUnit = item.BaseUnit,
             ConsumedAt = consumption.ConsumedAt, ConsumedBy = consumption.ConsumedBy
         }).FirstOrDefaultAsync(ct);

    public async Task<PagedResult<DisposalAuditDto>> GetDisposalsAsync(AuditQuery query, CancellationToken ct = default)
    {
        var source = DisposalDetails();
        source = ApplyDisposalFilters(source, query);
        var totalCount = await source.CountAsync(ct);
        var items = await source.OrderByDescending(x => x.DisposedAt).ThenBy(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return new PagedResult<DisposalAuditDto> { Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = totalCount };
    }

    public Task<DisposalAuditDto?> GetDisposalByIdAsync(Guid id, CancellationToken ct = default) =>
        DisposalDetails().FirstOrDefaultAsync(x => x.Id == id, ct);

    private IQueryable<DisposalAuditDto> DisposalDetails() =>
        from disposal in _context.Disposals.AsNoTracking()
        join lot in _context.Lots.AsNoTracking() on disposal.LotId equals lot.Id
        join item in _context.Items.AsNoTracking() on lot.ItemId equals item.Id
        select new DisposalAuditDto
        {
            Id = disposal.Id, LotId = lot.Id, ItemId = lot.ItemId, LotNumber = lot.LotNumber,
            Quantity = disposal.Quantity, BaseUnit = item.BaseUnit, Reason = disposal.Reason,
            DisposedAt = disposal.DisposedAt, DisposedBy = disposal.DisposedBy
        };

    private static IQueryable<AuditEventDto> ApplyEventFilters(IQueryable<AuditEventDto> source, AuditQuery query)
    {
        if (query.ItemId.HasValue) source = source.Where(x => x.ItemId == query.ItemId.Value);
        if (query.LotId.HasValue) source = source.Where(x => x.LotId == query.LotId.Value);
        if (query.JobId.HasValue) source = source.Where(x => x.JobId == query.JobId.Value);
        if (query.From.HasValue) source = source.Where(x => x.OccurredAt >= query.From.Value);
        if (query.To.HasValue) source = source.Where(x => x.OccurredAt < query.To.Value);
        return source;
    }

    private static IQueryable<ConsumptionAuditDto> ApplyConsumptionFilters(IQueryable<ConsumptionAuditDto> source, AuditQuery query)
    {
        if (query.ItemId.HasValue) source = source.Where(x => x.ItemId == query.ItemId.Value);
        if (query.LotId.HasValue) source = source.Where(x => x.LotId == query.LotId.Value);
        if (query.JobId.HasValue) source = source.Where(x => x.JobId == query.JobId.Value);
        if (query.From.HasValue) source = source.Where(x => x.ConsumedAt >= query.From.Value);
        if (query.To.HasValue) source = source.Where(x => x.ConsumedAt < query.To.Value);
        return source;
    }

    private static IQueryable<DisposalAuditDto> ApplyDisposalFilters(IQueryable<DisposalAuditDto> source, AuditQuery query)
    {
        if (query.ItemId.HasValue) source = source.Where(x => x.ItemId == query.ItemId.Value);
        if (query.LotId.HasValue) source = source.Where(x => x.LotId == query.LotId.Value);
        if (query.From.HasValue) source = source.Where(x => x.DisposedAt >= query.From.Value);
        if (query.To.HasValue) source = source.Where(x => x.DisposedAt < query.To.Value);
        return source;
    }
}
