using AutoMapper;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Services
{
    public class LotService : ILotService
    {
        private readonly ILotRepository _lotRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public LotService(ILotRepository lotRepository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _lotRepository = lotRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult<IEnumerable<LotDto>>> GetAllLot(CancellationToken ct)
        {
            var lots = await _lotRepository.GetAllAsync(ct);
            var response = _mapper.Map<IEnumerable<LotDto>>(lots);
            return ServiceResult<LotDto>.SuccessResult(
                response,
                "Lots retrieved successfully.");
        }
        public async Task<ServiceResult<LotDto>> GetByIdLot(Guid id, CancellationToken ct)
        {

            var lot = await _lotRepository.GetByIdAsync(id, ct);
            if (lot is null)
            {
                return ServiceResult<LotDto>.ErrorResult("NotFound");
            }
            var response = _mapper.Map<LotDto>(lot);
            return ServiceResult<LotDto>.SuccessResult(response);
        }
        public async Task<ServiceResult<LotDto>> CreateLot(CreateLotDto createLotDto, CancellationToken ct)
        {
            if (createLotDto.ItemId == Guid.Empty)
            {
                return ServiceResult<LotDto>.ErrorResult("ItemId is required.");
            }
            if (string.IsNullOrWhiteSpace(createLotDto.LotNumber))
            {
                return ServiceResult<LotDto>.ErrorResult("Lot number is required.");
            }
            if (createLotDto.InitialQuantity <= 0)
            {
                return ServiceResult<LotDto>.ErrorResult("Initial quantity must be greater than zero.");
            }
            var duplicate = await _lotRepository.ExistsAsync(
                createLotDto.ItemId,
                createLotDto.LotNumber.Trim(),
                createLotDto.ExpiryDate,
                ct);

            if (duplicate)
            {
                return ServiceResult<LotDto>.ErrorResult("A lot with the same item, lot number, and expiry date already exists.");
            }
            var lot = _mapper.Map<Lot>(createLotDto);
            lot.UpdateRemainingQuantity(createLotDto.InitialQuantity);

            var created = await _lotRepository.AddAsync(lot, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            var response = _mapper.Map<LotDto>(created);

            return ServiceResult<LotDto>.SuccessResult(
                response,
                "Lot created successfully.");
        }

        public async Task<ServiceResult<LotDto>> UpdateLot(Guid id, UpdateLotDto updateLotDto, CancellationToken ct)
        {
            var existing = await _lotRepository.GetByIdAsync(id, ct);
            if (existing is null)
            {
                return ServiceResult<LotDto>.ErrorResult("Lot not found.");
            }
            existing.UpdateStorageLocation(updateLotDto.StorageLocation);

            var updated = await _lotRepository.UpdateAsync(existing, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            var response = _mapper.Map<LotDto>(updated);
            return ServiceResult<LotDto>.SuccessResult(
                response,
                "Lot updated successfully.");
        }

        public async Task<ServiceResult<bool>> DeleteLot(Guid id, CancellationToken ct)
        {
            var deleted = await _lotRepository.DeleteAsync(id, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return deleted
            ? ServiceResult<bool>.SuccessResult(
                true,
                "Lot deleted successfully.")
            : ServiceResult<bool>.ErrorResult(
                "Lot not found.");
        }
        // Lot Summary 
        public async Task<ServiceResult<IEnumerable<LotSummaryDto>>> GetSummaryByItemId(Guid itemId, CancellationToken ct)
        {
            var lots = await _lotRepository.GetByItemIdAsync(itemId, ct);
            var summaries = _mapper.Map<IEnumerable<LotSummaryDto>>(lots);
            return ServiceResult<LotSummaryDto>.SuccessResult(
                summaries,
                "Lots retrieved successfully.");
        }

        public async Task<ServiceResult<IEnumerable<LotSummaryDto>>> GetAllSummary(CancellationToken ct)
        {
            var lots = await _lotRepository.GetAllAsync(ct);

            var summaries = _mapper.Map<IEnumerable<LotSummaryDto>>(lots);

            return ServiceResult<IEnumerable<LotSummaryDto>>.SuccessResult(
            summaries,
            "Lot summaries retrieved successfully.");
        }
    }
}