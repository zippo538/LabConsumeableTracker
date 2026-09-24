using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Application.Services;

public class ExportService : IExportService
{
    private readonly IItemRepository _itemRepository;
    private readonly ILotRepository _lotRepository;
    private readonly IConsumptionRepository _consumptionRepository; 
    private readonly IDisposalRepository _disposalRepository;
    private readonly IUserLookupService _userLookupService;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ExportService> _logger;

    public ExportService(IItemRepository itemRepository, ILotRepository lotRepository,
        IConsumptionRepository consumptionRepository, IDisposalRepository disposalRepository,IUserLookupService userLookupService, TimeProvider timeProvider,
        ILogger<ExportService> logger)
    {
        _itemRepository = itemRepository;
        _lotRepository = lotRepository;
        _consumptionRepository = consumptionRepository;
        _disposalRepository = disposalRepository;
        _userLookupService = userLookupService;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<ServiceResult<IEnumerable<StockExportRowDto>>> GetStockExport(
        StockExportFilterDto filter, CancellationToken ct)
    {
        if (!IsValidCsvFormat(filter.Format))
        {
            return ServiceResult<IEnumerable<StockExportRowDto>>.ErrorResult(
                "Invalid format. Only 'csv' is currently supported.");
        }

        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        var now = _timeProvider.GetUtcNow();

        var items = filter.ItemId is { } itemId
            ? await _itemRepository.GetByIdWithLotsAsync(itemId, ct) is { } single
                ? [single]
                : []
            : (await _itemRepository.GetAllWithLotsAsync(ct)).ToArray();

        var itemIds = items.Select(i => i.Id).ToArray();
        var usableQuantities = await _lotRepository.GetUsableQuantityByItemIdsAsync(itemIds, today, ct);

        var rows = items
            .Select(item =>
            {
                var usable = usableQuantities.GetValueOrDefault(item.Id, 0m);
                return new StockExportRowDto
                {
                    ItemId = item.Id,
                    Code = item.Code,
                    Name = item.Name,
                    BaseUnit = item.BaseUnit,
                    UsableStock = usable,
                    MinimumStock = item.MinimumStock,
                    IsLowStock = item.IsLowStock(usable),
                    EvaluatedAt = now
                };
            })
            .Where(row => filter.IsLowStock is null || row.IsLowStock == filter.IsLowStock)
            .ToList();

        _logger.LogInformation("Stock export produced {RowCount} row(s)", rows.Count);

        return ServiceResult<IEnumerable<StockExportRowDto>>.SuccessResult(rows, "Stock export retrieved.");
    }

    public async Task<ServiceResult<IEnumerable<ConsumptionExportRowDto>>> GetConsumptionExport(
        ConsumptionExportFilterDto filter, CancellationToken ct)
    {
        if (!IsValidCsvFormat(filter.Format))
            return ServiceResult<IEnumerable<ConsumptionExportRowDto>>.ErrorResult(
                "Invalid format. Only 'csv' is currently supported.");

        if (filter.From is not null && filter.To is not null && filter.From > filter.To)
            return ServiceResult<IEnumerable<ConsumptionExportRowDto>>.ErrorResult(
                "Invalid date range: From must be <= To.");

        var consumptions = await _consumptionRepository.SearchAsync(
            filter.JobId, filter.ItemId, filter.LotId, filter.From, filter.To, ct);

        if (consumptions.Count == 0)
            return ServiceResult<IEnumerable<ConsumptionExportRowDto>>.SuccessResult(
                Array.Empty<ConsumptionExportRowDto>(), "Consumption export retrieved.");

        var lotIds = consumptions.Select(c => c.LotId).Distinct().ToList();
        var lots = await _lotRepository.GetByIdsAsync(lotIds, ct);
        var lotsById = lots.ToDictionary(l => l.Id);
        
        var actorIds = consumptions.Select(c => c.ConsumedBy).Distinct().ToList();
        var names = await _userLookupService.GetDisplayNamesAsync(actorIds, ct);

        var rows = consumptions.Select(c =>
        {
            var lot = lotsById[c.LotId];
            return new ConsumptionExportRowDto
            {
                Id = c.Id,
                JobId = c.JobId,
                LotId = c.LotId,
                ItemId = lot.ItemId,
                ItemName = lot.Item.Name,
                Quantity = c.Quantity,
                BaseUnit = lot.Item.BaseUnit,
                ConsumedAt = c.ConsumedAt,
                ConsumedBy = names.GetValueOrDefault(c.ConsumedBy, c.ConsumedBy.ToString())
            };
        }).ToList();

        return ServiceResult<IEnumerable<ConsumptionExportRowDto>>.SuccessResult(rows, "Consumption export retrieved.");
    }

    public async Task<ServiceResult<IEnumerable<DisposalExportRowDto>>> GetDisposalExport(
        DisposalExportFilterDto filter, CancellationToken ct)
    {
        if (!IsValidCsvFormat(filter.Format))
            return ServiceResult<IEnumerable<DisposalExportRowDto>>.ErrorResult(
                "Invalid format. Only 'csv' is currently supported.");

        if (filter.From is not null && filter.To is not null && filter.From > filter.To)
            return ServiceResult<IEnumerable<DisposalExportRowDto>>.ErrorResult(
                "Invalid date range: From must be <= To.");

        var disposals = await _disposalRepository.SearchAsync(
            filter.ItemId, filter.LotId, filter.From, filter.To, ct);

        var actorIds = disposals.Select(d => d.DisposedBy).Distinct().ToList();
        var names = await _userLookupService.GetDisplayNamesAsync(actorIds, ct);
        
        var rows = disposals.Select(d => new DisposalExportRowDto
        {
            Id = d.Id,
            LotId = d.LotId,
            LotNumber = d.Lot.LotNumber,
            ItemId = d.Lot.ItemId,   
            ItemName = d.Lot.Item.Name,
            Quantity = d.Quantity,
            BaseUnit = d.Lot.Item.BaseUnit,
            Reason = d.Reason,
            DisposedAt = d.DisposedAt,
            DisposedBy = names.GetValueOrDefault(d.DisposedBy, d.DisposedBy.ToString())
        }).ToList();

        return ServiceResult<IEnumerable<DisposalExportRowDto>>.SuccessResult(rows, "Disposal export retrieved.");
    }

    private static bool IsValidCsvFormat(string format) =>
        string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase);
}