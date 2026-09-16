using AutoMapper;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;
using LabConsumableExpiryTracker.Application.DTOs.FefoDtos;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.Extensions.Logging;
using ILotSelectionService = LabConsumableExpiryTracker.Application.Services.Interfaces.ILotSelectionService;

namespace LabConsumableExpiryTracker.Application.Services
{
    public class LotService : ILotService
    {
        private readonly ILotRepository _lotRepository;
        private readonly IItemRepository _itemRepository;
        private readonly ILotSelectionService _lotSelectionService;
        private readonly IJobRepository _jobRepository;
        private readonly IConsumptionRepository _consumptionRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LotService> _logger;
        private readonly TimeProvider _timeProvider;

        public LotService(
            ILotRepository lotRepository,
            IItemRepository itemRepository,
            IJobRepository jobRepository,
            IConsumptionRepository consumptionRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<LotService> logger,
            ILotSelectionService lotSelectionService,
            TimeProvider timeProvider)
        {
            _lotRepository = lotRepository;
            _itemRepository = itemRepository;
            _lotSelectionService = lotSelectionService;
            _jobRepository = jobRepository;
            _consumptionRepository = consumptionRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _lotSelectionService = lotSelectionService;
            _timeProvider = timeProvider;
        }

        private LotDto MapToDtoWithExpiryEngine(
            Lot lot,
            int warningDays,
            DateTimeOffset now)
        {
            var response = _mapper.Map<LotDto>(lot);

            response.ExpiryCondition = lot.GetExpiryCondition(now, warningDays);
            response.IsEligible = lot.IsEligible(now);
            response.EvaluatedAt = now;

            return response;
        }

        public async Task<ServiceResult<IEnumerable<LotDto>>> GetAllLot(CancellationToken ct)
        {
            _logger.LogDebug("Retrieving all lots");

            var lots = await _lotRepository.GetAllWithItemAsync(ct);
            var now = _timeProvider.GetUtcNow();

            var response = lots
                .Select(lot => MapToDtoWithExpiryEngine(lot, lot.Item.ExpiringSoonDays, now))
                .ToList();

            _logger.LogInformation("All lots retrieved successfully");

            return ServiceResult<IEnumerable<LotDto>>.SuccessResult(
                response,
                "Lots retrieved successfully.");
        }

        public async Task<ServiceResult<LotDto>> GetByIdLot(Guid id, CancellationToken ct)
        {
            _logger.LogDebug("Retrieving lot {LotId}", id);

            var lot = await _lotRepository.GetByIdWithItemAsync(id, ct);
            if (lot is null)
            {
                _logger.LogWarning("Lot {LotId} was not found", id);
                return ServiceResult<LotDto>.ErrorResult("NotFound");
            }

            var now = _timeProvider.GetUtcNow();
            var response = MapToDtoWithExpiryEngine(lot, lot.Item.ExpiringSoonDays, now);

            _logger.LogInformation("Lot {LotId} retrieved successfully", id);

            return ServiceResult<LotDto>.SuccessResult(response);
        }

        public async Task<ServiceResult<LotDto>> CreateLot(CreateLotDto createLotDto, CancellationToken ct)
        {
            _logger.LogDebug("Validating lot creation request for item {ItemId}", createLotDto.ItemId);

            if (!Guid.TryParse(createLotDto.ItemId, out var itemId))
            {
                _logger.LogWarning("Lot creation rejected because ItemId is empty");
                return ServiceResult<LotDto>.ErrorResult(
                    "Validation failed.",
                    new List<string> { "ItemId must be a valid GUID.." });
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
                itemId,
                createLotDto.LotNumber.Trim(),
                createLotDto.ExpiryDate,
                ct);

            if (duplicate)
            {
                _logger.LogWarning(
                    "Duplicate lot creation rejected for item {ItemId} with expiry date {ExpiryDate}",
                    createLotDto.ItemId,
                    createLotDto.ExpiryDate);
                return ServiceResult<LotDto>.ErrorResult(
                    "A lot with the same item, lot number, and expiry date already exists.");
            }

            var lot = _mapper.Map<Lot>(createLotDto);
            var created = await _lotRepository.AddAsync(lot, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            var item = await _itemRepository.GetByIdAsync(itemId, ct);
            var now = _timeProvider.GetUtcNow();
            var response = MapToDto(created, item!.ExpiringSoonDays, now);

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

            var existing = await _lotRepository.GetByIdWithItemAsync(id, ct);
            if (existing is null)
            {
                _logger.LogWarning("Lot update rejected because lot {LotId} was not found", id);
                return ServiceResult<LotDto>.ErrorResult("Lot not found.");
            }

            existing.UpdateStorageLocation(updateLotDto.StorageLocation);

            var updated = await _lotRepository.UpdateAsync(existing, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var now = _timeProvider.GetUtcNow();
            var response = MapToDtoWithExpiryEngine(updated, updated.Item.ExpiringSoonDays, now);

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
        public async Task<ServiceResult<IEnumerable<LotSummaryDto>>> GetSummaryByItemId(Guid itemId,
            CancellationToken ct)
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
        public async Task<ServiceResult<FefoAllocationPreviewDto>> RequestFefoAllocation(
            Guid jobId,
            Guid itemId,
            decimal requestedQuantity,
            CancellationToken ct)
        {
            _logger.LogDebug(
                "Requesting FEFO allocation for job {JobId}, item {ItemId}, quantity {RequestedQuantity}",
                jobId,
                itemId,
                requestedQuantity);

            var job = await _jobRepository.GetByIdAsync(jobId, ct);
            if (job is null)
            {
                _logger.LogWarning("FEFO allocation rejected because job {JobId} was not found", jobId);
                return ServiceResult<FefoAllocationPreviewDto>.ErrorResult("Job not found.");
            }

            if (job.Status != JobStatus.InProgress)
            {
                _logger.LogWarning(
                    "FEFO allocation rejected because job {JobId} is not InProgress (status: {Status})",
                    jobId,
                    job.Status);
                return ServiceResult<FefoAllocationPreviewDto>.ErrorResult(
                    $"Job must be InProgress to request FEFO allocation. Current status: {job.Status}.");
            }

            if (requestedQuantity <= 0)
            {
                _logger.LogWarning(
                    "FEFO allocation rejected because requestedQuantity is not positive for item {ItemId}",
                    itemId);
                return ServiceResult<FefoAllocationPreviewDto>.ErrorResult(
                    "Requested quantity must be greater than zero.");
            }

            var candidates = (await _lotRepository.GetByItemIdAsync(itemId, ct)).ToList();
            var now = _timeProvider.GetUtcNow();

            var allocationResult = await _lotSelectionService.AllocateAsync(itemId, requestedQuantity, now, ct);

            if (!allocationResult.Success)
            {
                _logger.LogWarning(
                    "FEFO allocation rejected for item {ItemId}: {Reason}",
                    itemId,
                    allocationResult.Message);
                return ServiceResult<FefoAllocationPreviewDto>.ErrorResult(allocationResult.Message);
            }

            var allocations = allocationResult.Data!;
            var candidatesById = candidates.ToDictionary(lot => lot.Id);

            var lines = allocations
                .Select(allocation =>
                {
                    var lot = candidatesById[allocation.LotId];
                    return new LotAllocationDto
                    {
                        LotId = lot.Id,
                        LotNumber = lot.LotNumber,
                        ExpiryDate = lot.ExpiryDate,
                        RemainingQuantity = lot.RemainingQuantity,
                        Quantity = allocation.Quantity,
                        RowVersion = lot.RowVersion
                    };
                })
                .ToList();

            var baseUnit = candidates[0].Item.BaseUnit;

            var response = new FefoAllocationPreviewDto
            {
                JobId = jobId,
                ItemId = itemId,
                BaseUnit = baseUnit,
                RequestedQuantity = requestedQuantity,
                AllocatedQuantity = lines.Sum(line => line.Quantity),
                IsReservation = false,
                Allocations = lines,
                PreviewedAt = now
            };

            _logger.LogInformation(
                "FEFO allocation for job {JobId}, item {ItemId} recommended across {LotCount} lot(s)",
                jobId,
                itemId,
                lines.Count);

            return ServiceResult<FefoAllocationPreviewDto>.SuccessResult(
                response,
                "Lots allocated successfully.");
        }
        
        public async Task<ServiceResult<ConsumptionResponseDto>> ConfirmConsumption(
            Guid jobId,
            ConfirmConsumptionRequestDto request,
            Guid consumedBy,
            CancellationToken ct)
        {
            _logger.LogDebug(
                "Confirming consumption for job {JobId}, item {ItemId}",
                jobId,
                request.ItemId);
 
            if (request.Allocations.Count == 0)
            {
                return ServiceResult<ConsumptionResponseDto>.ErrorResult(
                    "At least one allocation is required.");
            }
 
            if (request.Allocations.Any(a => a.Quantity <= 0))
            {
                return ServiceResult<ConsumptionResponseDto>.ErrorResult(
                    "Every allocation quantity must be greater than zero.");
            }
 
            var duplicateLotIds = request.Allocations
                .GroupBy(a => a.LotId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
 
            if (duplicateLotIds.Count > 0)
            {
                return ServiceResult<ConsumptionResponseDto>.ErrorResult(
                    "Duplicate LotId is not allowed within a single consumption request.");
            }
 
            var allocationTotal = request.Allocations.Sum(a => a.Quantity);
            if (allocationTotal != request.RequestedQuantity)
            {
                return ServiceResult<ConsumptionResponseDto>.ErrorResult(
                    $"Sum of allocation quantities ({allocationTotal}) does not match RequestedQuantity ({request.RequestedQuantity}).");
            }
            
 
            var job = await _jobRepository.GetByIdAsync(jobId, ct);
            if (job is null)
            {
                return ServiceResult<ConsumptionResponseDto>.ErrorResult("Job not found.");
            }
 
            if (job.Status != JobStatus.InProgress)
            {
                return ServiceResult<ConsumptionResponseDto>.ErrorResult(
                    $"Job must be InProgress to confirm consumption. Current status: {job.Status}.");
            }
 
            var now = _timeProvider.GetUtcNow();
            
 
            var consumptions = new List<Consumption>();
            var updatedLots = new List<Lot>();
 
            foreach (var allocation in request.Allocations)
            {
                
                var lot = await _lotRepository.GetByIdAsync(allocation.LotId, ct);
                if (lot is null)
                {
                    return ServiceResult<ConsumptionResponseDto>.ErrorResult(
                        $"Lot {allocation.LotId} not found.");
                }
 
                if (lot.ItemId != request.ItemId)
                {
                    return ServiceResult<ConsumptionResponseDto>.ErrorResult(
                        $"Lot {allocation.LotId} does not belong to item {request.ItemId}.");
                }
 
                if (!lot.RowVersion.SequenceEqual(allocation.ExpectedRowVersion))
                {
                    _logger.LogWarning(
                        "RowVersion mismatch for lot {LotId}. DB: {DbValue}, Expected: {ExpectedValue}",
                        allocation.LotId,
                        Convert.ToBase64String(lot.RowVersion),
                        Convert.ToBase64String(allocation.ExpectedRowVersion));
                    return ServiceResult<ConsumptionResponseDto>.ErrorResult(
                        $"Lot {allocation.LotId} has changed since it was previewed (stale RowVersion).");
                }
 
                try
                {
                    var consumption = lot.Consume(jobId, allocation.Quantity, now, consumedBy);
                    consumptions.Add(consumption);
                    updatedLots.Add(lot);
                }
                catch (InvalidOperationException ex)
                {
                    return ServiceResult<ConsumptionResponseDto>.ErrorResult(
                        $"Lot {allocation.LotId}: {ex.Message}");
                }
            }
 
            foreach (var consumption in consumptions)
            {
                await _consumptionRepository.AddAsync(consumption, ct);
            }
 
            foreach (var lot in updatedLots)
            {
                await _lotRepository.UpdateAsync(lot, ct);
            }
 
            await _unitOfWork.SaveChangesAsync(ct);
            
            var item = await _itemRepository.GetByIdAsync(request.ItemId, ct);
 
            var response = new ConsumptionResponseDto
            {
                JobId = jobId,
                ItemId = request.ItemId,
                BaseUnit = item!.BaseUnit,
                TotalConsumedQuantity = consumptions.Sum(c => c.Quantity),
                Consumptions = consumptions
                    .Select(c => new ConsumptionRecordDto
                    {
                        Id = c.Id,
                        JobId = c.JobId,
                        LotId = c.LotId,
                        Quantity = c.Quantity,
                        ConsumedAt = c.ConsumedAt,
                        ConsumedBy = c.ConsumedBy
                    })
                    .ToList(),
                UpdatedLots = updatedLots
                    .Select(lot => new UpdatedLotDto
                    {
                        LotId = lot.Id,
                        RemainingQuantity = lot.RemainingQuantity,
                        RowVersion = lot.RowVersion
                    })
                    .ToList()
            };
 
            _logger.LogInformation(
                "Consumption confirmed for job {JobId}, item {ItemId}, {LotCount} lot(s), total {Total}",
                jobId,
                request.ItemId,
                updatedLots.Count,
                response.TotalConsumedQuantity);
 
            return ServiceResult<ConsumptionResponseDto>.SuccessResult(
                response,
                "Consumption confirmed successfully.");
        }

        private LotDto MapToDto(
           Lot lot,
           int warningDays,
           DateTimeOffset now)
        {
            var response = _mapper.Map<LotDto>(lot);

            response.ExpiryCondition =
                lot.GetExpiryCondition(now, warningDays);

            response.IsEligible =
                lot.IsEligible(now);

            response.EvaluatedAt = now;

            return response;
        }
    }
}