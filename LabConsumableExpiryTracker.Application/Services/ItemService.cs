using AutoMapper;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Application.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;
        private readonly ILotRepository _lotRepository;
        private readonly IMapper _mapper;
        private readonly TimeProvider _timeProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ItemService> _logger;

        public ItemService(
            IItemRepository itemRepository,
            IMapper mapper,
            TimeProvider timeProvider,
            ILotRepository lotRepository,
            IUnitOfWork unitOfWork,
            ILogger<ItemService> logger)
        {
            _itemRepository = itemRepository;
            _mapper = mapper;
            _timeProvider = timeProvider;
            _lotRepository = lotRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ServiceResult<IEnumerable<ItemDto>>> GetAllItem(CancellationToken ct)
        {
            _logger.LogDebug("Retrieving all items with usable stock information");

            var items = await _itemRepository.GetAllWithLotsAsync(ct);
            var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

            var itemIds = items.Select(item => item.Id).ToArray();
            var usableQuantities = await _lotRepository.GetUsableQuantityByItemIdsAsync(itemIds, today, ct);

            IEnumerable<ItemDto> data = items.Select(item =>
            {
                var totalUsableQuantity = usableQuantities.GetValueOrDefault(item.Id, 0m);

                var dto = _mapper.Map<ItemDto>(item);
                dto.TotalRemainingQuantity = totalUsableQuantity;
                dto.StockStatus = item.GetStockStatus(totalUsableQuantity);
                return dto;
            }).ToList();

            _logger.LogInformation(
                "Retrieved {ItemCount} items with stock evaluated on {EvaluationDate}",
                itemIds.Length,
                today);

            return ServiceResult<IEnumerable<ItemDto>>.SuccessResult(
                data,
                "Items retrieved successfully.");
        }

        public async Task<ServiceResult<ItemDto>> GetByIdItem(Guid id, CancellationToken ct)
        {
            _logger.LogDebug("Retrieving item {ItemId} with usable stock information", id);

            var item = await _itemRepository.GetByIdWithLotsAsync(id, ct);
            if (item is null)
            {
                _logger.LogWarning("Item {ItemId} was not found", id);
                return ServiceResult<ItemDto>.ErrorResult(
                    "Item not found.");
            }

            var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
            var totalUsableQuantity = await _lotRepository.GetTotalUsableQuantityAsync(
                item.Id,
                today,
                ct);
            var response = _mapper.Map<ItemDto>(item);
            response.TotalRemainingQuantity = totalUsableQuantity;
            response.StockStatus = item.GetStockStatus(totalUsableQuantity);

            _logger.LogInformation(
                "Item {ItemId} retrieved with usable quantity {UsableQuantity} and stock status {StockStatus}",
                id,
                totalUsableQuantity,
                response.StockStatus);

            return ServiceResult<ItemDto>.SuccessResult(
                response,
                "Item retrieved successfully.");
        }

        public async Task<ServiceResult<ItemDto>> CreateItem(CreateItemDto dto, CancellationToken ct)
        {
            _logger.LogDebug("Validating item creation request for code {ItemCode}", dto.Code);
            var existing = await _itemRepository.GetByCodeAsync(dto.Code, ct);
            if (existing is not null)
            {
                _logger.LogWarning(
                    "Item creation rejected because code {ItemCode} already exists",
                    dto.Code);
                return ServiceResult<ItemDto>.ErrorResult("Item code already exists.");
            }
            
            if (!Enum.TryParse<UnitOfMeasure>(dto.BaseUnit, true, out var baseUnit))
            {
                return ServiceResult<ItemDto>.ErrorResult(
                    "Validation failed.",
                    new List<string> { "BaseUnit tidak valid." });
            }

            var item = new Item(
                Guid.NewGuid(),
                dto.Code,
                dto.Name,
                baseUnit,
                dto.MinimumStock,
                dto.ExpiringSoonDays);
            var created = await _itemRepository.AddAsync(item, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            var response = _mapper.Map<ItemDto>(created);

            _logger.LogInformation(
                "Item {ItemId} with code {ItemCode} created successfully",
                created.Id,
                dto.Code);

            return ServiceResult<ItemDto>.SuccessResult(
                response,
                "Item created successfully.");
        }

        public async Task<ServiceResult<ItemDto>> UpdateItem(
            Guid id,
            UpdateItemDto request,
            CancellationToken ct)
        {
            _logger.LogDebug("Updating item {ItemId}", id);

            var item = await _itemRepository.GetByIdAsync(id, ct);

            if (item is null)
            {
                _logger.LogWarning("Item update rejected because item {ItemId} was not found", id);
                return ServiceResult<ItemDto>.ErrorResult(
                    "Item not found.");
            }

            var now = _timeProvider.GetUtcNow();

            var totalUsableQuantity = item.Lots
                .Where(lot => lot.IsEligible(now))
                .Sum(lot => lot.RemainingQuantity);

            var stockStatus = item.GetStockStatus(
                totalUsableQuantity);

            _logger.LogDebug(
                "Item {ItemId} currently has usable quantity {UsableQuantity} and stock status {StockStatus}",
                id,
                totalUsableQuantity,
                stockStatus);

            item.UpdateDetails(
                request.Name,
                request.BaseUnit,
                request.MinimumStock,
                request.ExpiringSoonDays);

            var updated = await _itemRepository.UpdateAsync(item, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            var response = _mapper.Map<ItemDto>(updated);

            _logger.LogInformation("Item {ItemId} updated successfully", id);

            return ServiceResult<ItemDto>.SuccessResult(
                response,
                "Item updated successfully.");
        }

        public async Task<ServiceResult<bool>> DeleteItem(
            Guid id,
            CancellationToken ct)
        {
            _logger.LogDebug("Deleting item {ItemId}", id);

            var deleted = await _itemRepository.DeleteAsync(id, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            if (!deleted)
            {
                _logger.LogWarning("Item deletion rejected because item {ItemId} was not found", id);
                return ServiceResult<bool>.ErrorResult("Item not found.");
            }

            _logger.LogInformation("Item {ItemId} deleted successfully", id);

            return ServiceResult<bool>.SuccessResult(
                true,
                "Item deleted successfully.");
        }
    }
}
