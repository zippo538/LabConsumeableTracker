using AutoMapper;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;
using LabConsumableExpiryTracker.Application.DTOs.FefoDtos;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ILotSelectionService = LabConsumableExpiryTracker.Application.Services.Interfaces.ILotSelectionService;

namespace LabConsumableExpiryTracker.Application.Services;

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
                ["ItemId must be a valid GUID.."]);
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

        var lot = Lot.Create(
            itemId,
            createLotDto.LotNumber,
            createLotDto.SupplierLotNumber,
            createLotDto.ReceivedAt.ToUniversalTime(),
            createLotDto.SupplierName,
            createLotDto.InitialQuantity,
            createLotDto.ExpiryDate,
            createLotDto.StorageLocation);
        _logger.LogInformation(
            "ReceivedAt DTO: {DtoReceivedAt} (Offset: {DtoOffset}); Lot: {LotReceivedAt} (Offset: {LotOffset})",
            createLotDto.ReceivedAt,
            createLotDto.ReceivedAt.Offset,
            lot.ReceivedAt,
            lot.ReceivedAt.Offset);
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

        if (updateLotDto.RowVersion is not { Length: > 0 })
        {
            return ServiceResult<LotDto>.ErrorResult("RowVersion is required for update.");
        }

        var hasChanges = false;

        if (updateLotDto.LotNumber != null)
        {
            hasChanges |= existing.UpdateLotNumber(updateLotDto.LotNumber);
        }

        if (updateLotDto.SupplierLotNumber != null)
        {
            hasChanges |= existing.UpdateSupplierLotNumber(updateLotDto.SupplierLotNumber);
        }

        if (updateLotDto.SupplierName != null)
        {
            hasChanges |= existing.UpdateSupplierName(updateLotDto.SupplierName);
        }

        if (updateLotDto.StorageLocation != null)
        {
            hasChanges |= existing.UpdateStorageLocation(updateLotDto.StorageLocation);
        }

        if (updateLotDto.ExpiryDate.HasValue)
        {
            hasChanges |= existing.UpdateExpiryDate(updateLotDto.ExpiryDate.Value);
        }

        if (updateLotDto.AdministrativeStatus.HasValue)
        {
            hasChanges |= existing.UpdateAdministrativeStatus(updateLotDto.AdministrativeStatus.Value);
        }

        if (!hasChanges)
        {
            _logger.LogInformation("Lot {LotId} update skipped because no changes detected", id);
            var hasNow = _timeProvider.GetUtcNow();
            var hasNoResponse = MapToDtoWithExpiryEngine(existing, existing.Item.ExpiringSoonDays, hasNow);
            return ServiceResult<LotDto>.SuccessResult(hasNoResponse, "No changes detected.");
        }

        existing.SetConcurrencyToken(updateLotDto.RowVersion);

        var updated = await _lotRepository.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync(ct);

        var now = _timeProvider.GetUtcNow();
        var response = MapToDtoWithExpiryEngine(updated, updated.Item.ExpiringSoonDays, now);

        _logger.LogInformation("Lot {LotId} updated successfully", id);
        return ServiceResult<LotDto>.SuccessResult(response, "Lot updated successfully.");

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
    public async Task<ServiceResult<PagedResult<LotDto>>> GetAllLotPage(
        CancellationToken ct,
        int page,
        int pageSize,
        string? search = null,
        Guid? itemId = null,
        string? administrativeStatus = null,
        string? expiryCondition = null)
    {
        var pagedLots = await _lotRepository.GetAllWithItemAsyncPage(
            page, pageSize, search, itemId, administrativeStatus, expiryCondition, ct);

        var now = _timeProvider.GetUtcNow();

        var lotDtos = pagedLots.Items.Select(lot =>
        {
            var warningDays = lot.Item?.ExpiringSoonDays ?? 0;
            return MapToDtoWithExpiryEngine(lot, warningDays, now);
        }).ToList();

        var pagedResult = new PagedResult<LotDto>
        {
            Items = lotDtos,
            Page = pagedLots.Page,
            PageSize = pagedLots.PageSize,
            TotalCount = pagedLots.TotalCount 
        };

        return ServiceResult<PagedResult<LotDto>>.SuccessResult(
            pagedResult,
            "Paged lots retrieved successfully.");
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
                    StorageLocation = lot.StorageLocation,
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


    public async Task<ServiceResult<DraftConsumptionResponseDto>> CreateDraftConsumption(
        Guid jobId,
        CreateDraftConsumptionRequestDto request,
        Guid createdBy,
        CancellationToken ct)
    {
        _logger.LogDebug(
            "Creating consumption draft for job {JobId}, item {ItemId}",
            jobId,
            request.ItemId);

        if (request.Allocations.Count == 0)
        {
            return ServiceResult<DraftConsumptionResponseDto>.ErrorResult(
                "At least one allocation is required.", code: ErrorCodes.ValidationError);
        }

        if (request.Allocations.Any(a => a.Quantity <= 0))
        {
            return ServiceResult<DraftConsumptionResponseDto>.ErrorResult(
                "Every allocation quantity must be greater than zero.", code: ErrorCodes.ValidationError);
        }

        if (request.Allocations.GroupBy(a => a.LotId).Any(g => g.Count() > 1))
        {
            return ServiceResult<DraftConsumptionResponseDto>.ErrorResult(
                "Duplicate LotId is not allowed within a single consumption request.",
                code: ErrorCodes.ValidationError);
        }

        var job = await _jobRepository.GetByIdAsync(jobId, ct);
        if (job is null)
        {
            return ServiceResult<DraftConsumptionResponseDto>.ErrorResult(
                "Job not found.", code: ErrorCodes.JobNotFound);
        }

        if (job.Status != JobStatus.InProgress)
        {
            return ServiceResult<DraftConsumptionResponseDto>.ErrorResult(
                $"Job must be InProgress to create a consumption draft. Current status: {job.Status}.",
                code: ErrorCodes.InvalidJobState);
        }

        var now = _timeProvider.GetUtcNow();
        var drafts = new List<Consumption>();

        foreach (var allocation in request.Allocations)
        {
            var lot = await _lotRepository.GetByIdAsync(allocation.LotId, ct);
            if (lot is null)
            {
                return ServiceResult<DraftConsumptionResponseDto>.ErrorResult(
                    $"Lot {allocation.LotId} not found.", code: ErrorCodes.LotNotFound);
            }

            if (lot.ItemId != request.ItemId)
            {
                return ServiceResult<DraftConsumptionResponseDto>.ErrorResult(
                    $"Lot {allocation.LotId} does not belong to item {request.ItemId}.",
                    code: ErrorCodes.ValidationError);
            }

            if (!lot.RowVersion.SequenceEqual(allocation.ExpectedRowVersion))
            {
                _logger.LogWarning(
                    "RowVersion mismatch for lot {LotId}. DB: {DbValue}, Expected: {ExpectedValue}",
                    allocation.LotId,
                    Convert.ToBase64String(lot.RowVersion),
                    Convert.ToBase64String(allocation.ExpectedRowVersion));
                return ServiceResult<DraftConsumptionResponseDto>.ErrorResult(
                    $"Lot {allocation.LotId} has changed since it was previewed (stale RowVersion).",
                    code: ErrorCodes.ConcurrencyConflict);
            }

            if (!lot.IsEligible(now))
            {
                return ServiceResult<DraftConsumptionResponseDto>.ErrorResult(
                    $"Lot {allocation.LotId} is not eligible for consumption.",
                    code: ErrorCodes.LotNotEligible);
            }

            // BR-19: draft hanya merekam niat. Tidak memanggil DeductStock, tidak
            // me-reserve lot, sehingga draft orang lain tidak terblokir preview FEFO.
            drafts.Add(Consumption.CreateDraft(
                Guid.NewGuid(), jobId, lot.Id, allocation.Quantity, now, createdBy));
        }

        foreach (var draft in drafts)
        {
            await _consumptionRepository.AddAsync(draft, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var response = new DraftConsumptionResponseDto
        {
            JobId = jobId,
            ItemId = request.ItemId,
            Consumptions = [.. drafts
                .Select(c => new DraftConsumptionRecordDto
                {
                    Id = c.Id,
                    LotId = c.LotId,
                    Quantity = c.Quantity,
                    Status = c.Status,
                    CreatedAt = c.ConsumedAt
                })]
        };

        _logger.LogInformation(
            "Consumption draft created for job {JobId}, item {ItemId}, {LotCount} lot(s). Stock unchanged.",
            jobId,
            request.ItemId,
            drafts.Count);

        return ServiceResult<DraftConsumptionResponseDto>.SuccessResult(
            response,
            "Consumption draft created. Stock is deducted after scan verification.");
    }

    public async Task<ServiceResult<VerifyScanResponseDto>> VerifyScan(
        Guid jobId,
        VerifyScanRequestDto request,
        CancellationToken ct)
    {
        _logger.LogDebug(
            "Verifying scan for job {JobId}, item {ItemId}, lot {LotNumber}",
            jobId,
            request.ItemId,
            request.ScannedLotNumber);

        var job = await _jobRepository.GetByIdAsync(jobId, ct);
        if (job is null)
        {
            return ServiceResult<VerifyScanResponseDto>.ErrorResult(
                "Job not found.", code: ErrorCodes.JobNotFound);
        }

        if (job.Status != JobStatus.InProgress)
        {
            return ServiceResult<VerifyScanResponseDto>.ErrorResult(
                $"Job must be InProgress to verify a scan. Current status: {job.Status}.",
                code: ErrorCodes.InvalidJobState);
        }

        var lot = await _lotRepository.GetByItemIdAndLotNumberAsync(
            request.ItemId, request.ScannedLotNumber, ct);
        if (lot is null)
        {
            return ServiceResult<VerifyScanResponseDto>.ErrorResult(
                $"Lot '{request.ScannedLotNumber}' not found for item {request.ItemId}.",
                code: ErrorCodes.LotNotFound);
        }

        var draft = await _consumptionRepository.GetPendingByJobAndLotAsync(jobId, lot.Id, ct);
        if (draft is null)
        {
            return ServiceResult<VerifyScanResponseDto>.ErrorResult(
                $"No pending consumption waiting for scan of lot '{lot.LotNumber}' in this job.",
                code: ErrorCodes.NoPendingConsumption);
        }

        var now = _timeProvider.GetUtcNow();

        // BR-21: eligibility dan stok dievaluasi ulang pada waktu scan aktual.
        // First-to-Scan Wins (D-15): draft Scientist lain tidak me-reserve stok, jadi
        // bila stok habis duluan, request ini yang gagal.
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        if (today >= lot.ExpiryDate)
        {
            return ServiceResult<VerifyScanResponseDto>.ErrorResult(
                $"Lot '{lot.LotNumber}' expired on {lot.ExpiryDate:yyyy-MM-dd}.",
                code: ErrorCodes.LotExpired);
        }

        if (!lot.IsEligible(now))
        {
            return ServiceResult<VerifyScanResponseDto>.ErrorResult(
                $"Lot '{lot.LotNumber}' is no longer eligible for consumption.",
                code: ErrorCodes.LotNotEligible);
        }

        if (draft.Quantity > lot.RemainingQuantity)
        {
            return ServiceResult<VerifyScanResponseDto>.ErrorResult(
                $"Lot '{lot.LotNumber}' has only {lot.RemainingQuantity} left but the draft needs {draft.Quantity}. " +
                "Refresh the FEFO preview and create a new draft.",
                code: ErrorCodes.InsufficientStock);
        }

        lot.DeductStock(draft.Quantity, now);
        draft.VerifyScan(now);

        await _lotRepository.UpdateAsync(lot);
        await _consumptionRepository.UpdateAsync(draft);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // RowVersion lot berubah sejak dibaca: Scientist lain menang race-nya.
            _logger.LogWarning(
                "Concurrency conflict verifying scan for lot {LotId} in job {JobId}",
                lot.Id,
                jobId);
            return ServiceResult<VerifyScanResponseDto>.ErrorResult(
                $"Lot '{lot.LotNumber}' was updated by another user. Refresh the FEFO preview and try again.",
                code: ErrorCodes.ConcurrencyConflict);
        }

        var response = new VerifyScanResponseDto
        {
            ConsumptionId = draft.Id,
            LotId = lot.Id,
            LotNumber = lot.LotNumber,
            Quantity = draft.Quantity,
            Status = draft.Status,
            VerifiedAt = draft.VerifiedAt,
            UpdatedLot = new UpdatedLotDto
            {
                LotId = lot.Id,
                RemainingQuantity = lot.RemainingQuantity,
                RowVersion = lot.RowVersion
            }
        };

        _logger.LogInformation(
            "Scan verified for job {JobId}, lot {LotNumber}, quantity {Quantity}, remaining {Remaining}",
            jobId,
            lot.LotNumber,
            draft.Quantity,
            lot.RemainingQuantity);

        return ServiceResult<VerifyScanResponseDto>.SuccessResult(
            response,
            "Scan verified successfully.");
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