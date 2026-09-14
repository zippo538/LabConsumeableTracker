using AutoMapper;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Application.Services
{
    public class LotService : ILotService
    {
        private readonly ILotRepository _lotRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LotService> _logger;

        public LotService(
            ILotRepository lotRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<LotService> logger)
        {
            _lotRepository = lotRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ServiceResult<IEnumerable<LotDto>>> GetAllLot(CancellationToken ct)
        {
            _logger.LogDebug("Retrieving all lots");

            var lots = await _lotRepository.GetAllAsync(ct);
            var response = _mapper.Map<IEnumerable<LotDto>>(lots);

            _logger.LogInformation("All lots retrieved successfully");

            return ServiceResult<IEnumerable<LotDto>>.SuccessResult(
                response,
                "Lots retrieved successfully.");
        }

        public async Task<ServiceResult<LotDto>> GetByIdLot(Guid id, CancellationToken ct)
        {
            _logger.LogDebug("Retrieving lot {LotId}", id);

            var lot = await _lotRepository.GetByIdAsync(id, ct);
            if (lot is null)
            {
                _logger.LogWarning("Lot {LotId} was not found", id);
                return ServiceResult<LotDto>.ErrorResult("NotFound");
            }

            var response = _mapper.Map<LotDto>(lot);

            _logger.LogInformation("Lot {LotId} retrieved successfully", id);

            return ServiceResult<LotDto>.SuccessResult(response);
        }

        public async Task<ServiceResult<LotDto>> CreateLot(CreateLotDto createLotDto, CancellationToken ct)
        {
            _logger.LogDebug("Validating lot creation request for item {ItemId}", createLotDto.ItemId);

            if (createLotDto.ItemId == Guid.Empty)
            {
                _logger.LogWarning("Lot creation rejected because ItemId is empty");
                return ServiceResult<LotDto>.ErrorResult("ItemId is required.");
            }

            if (string.IsNullOrWhiteSpace(createLotDto.LotNumber))
            {
                _logger.LogWarning(
                    "Lot creation rejected because LotNumber is empty for item {ItemId}",
                    createLotDto.ItemId);
                return ServiceResult<LotDto>.ErrorResult("Lot number is required.");
            }

            if (createLotDto.InitialQuantity <= 0)
            {
                _logger.LogWarning(
                    "Lot creation rejected because InitialQuantity is not positive for item {ItemId}",
                    createLotDto.ItemId);
                return ServiceResult<LotDto>.ErrorResult("Initial quantity must be greater than zero.");
            }

            var duplicate = await _lotRepository.ExistsAsync(
                createLotDto.ItemId,
                createLotDto.LotNumber.Trim(),
                createLotDto.ExpiryDate,
                ct);

            if (duplicate)
            {
                _logger.LogWarning(
                    "Duplicate lot creation rejected for item {ItemId} with expiry date {ExpiryDate}",
                    createLotDto.ItemId,
                    createLotDto.ExpiryDate);
                return ServiceResult<LotDto>.ErrorResult("A lot with the same item, lot number, and expiry date already exists.");
            }

            var lot = _mapper.Map<Lot>(createLotDto);
            var created = await _lotRepository.AddAsync(lot, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            var response = _mapper.Map<LotDto>(created);

            _logger.LogInformation(
                "Lot {LotId} created successfully for item {ItemId}",
                created.Id,
                createLotDto.ItemId);

            return ServiceResult<LotDto>.SuccessResult(
                response,
                "Lot created successfully.");
        }

        public async Task<ServiceResult<LotDto>> UpdateLot(Guid id, UpdateLotDto updateLotDto, CancellationToken ct)
        {
            _logger.LogDebug("Updating lot {LotId}", id);

            var existing = await _lotRepository.GetByIdAsync(id, ct);
            if (existing is null)
            {
                _logger.LogWarning("Lot update rejected because lot {LotId} was not found", id);
                return ServiceResult<LotDto>.ErrorResult("Lot not found.");
            }

            existing.UpdateStorageLocation(updateLotDto.StorageLocation);

            var updated = await _lotRepository.UpdateAsync(existing, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            var response = _mapper.Map<LotDto>(updated);

            _logger.LogInformation("Lot {LotId} updated successfully", id);

            return ServiceResult<LotDto>.SuccessResult(
                response,
                "Lot updated successfully.");
        }

        public async Task<ServiceResult<bool>> DeleteLot(Guid id, CancellationToken ct)
        {
            _logger.LogDebug("Deleting lot {LotId}", id);

            var deleted = await _lotRepository.DeleteAsync(id, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            if (!deleted)
            {
                _logger.LogWarning("Lot deletion rejected because lot {LotId} was not found", id);
                return ServiceResult<bool>.ErrorResult("Lot not found.");
            }

            _logger.LogInformation("Lot {LotId} deleted successfully", id);

            return ServiceResult<bool>.SuccessResult(
                true,
                "Lot deleted successfully.");
        }

        // Lot Summary 
        public async Task<ServiceResult<IEnumerable<LotSummaryDto>>> GetSummaryByItemId(Guid itemId, CancellationToken ct)
        {
            _logger.LogDebug("Retrieving lot summaries for item {ItemId}", itemId);

            var lots = await _lotRepository.GetByItemIdAsync(itemId, ct);
            var summaries = _mapper.Map<IEnumerable<LotSummaryDto>>(lots);

            _logger.LogInformation("Lot summaries for item {ItemId} retrieved successfully", itemId);

            return ServiceResult<IEnumerable<LotSummaryDto>>.SuccessResult(
                summaries,
                "Lots retrieved successfully.");
        }

        public async Task<ServiceResult<IEnumerable<LotSummaryDto>>> GetAllSummary(CancellationToken ct)
        {
            _logger.LogDebug("Retrieving all lot summaries");

            var lots = await _lotRepository.GetAllAsync(ct);

            var summaries = _mapper.Map<IEnumerable<LotSummaryDto>>(lots);

            _logger.LogInformation("All lot summaries retrieved successfully");

            return ServiceResult<IEnumerable<LotSummaryDto>>.SuccessResult(
                summaries,
                "Lot summaries retrieved successfully.");
        }
    }
}
