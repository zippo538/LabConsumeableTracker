using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;
using LabConsumableExpiryTracker.Application.DTOs.FefoDtos;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;

namespace LabConsumableExpiryTracker.Application.Services.Interfaces;

public interface ILotService
{
    Task<ServiceResult<IEnumerable<LotDto>>> GetAllLot(CancellationToken ct);
    Task<ServiceResult<PagedResult<LotDto>>> GetAllLotPage(
        CancellationToken ct,
        int page,
        int pageSize,
        string? search,
        Guid? itemId,
        string? administrativeStatus,
        string? expiryCondition
    );
    Task<ServiceResult<IEnumerable<LotSummaryDto>>> GetSummaryByItemId(Guid itemId, CancellationToken ct);
    Task<ServiceResult<IEnumerable<LotSummaryDto>>> GetAllSummary(CancellationToken ct);
    Task<ServiceResult<LotDto>> GetByIdLot(Guid id, CancellationToken ct);
    Task<ServiceResult<LotDto>> CreateLot(CreateLotDto lot, CancellationToken ct);
    Task<ServiceResult<LotDto>> UpdateLot(Guid id, UpdateLotDto lot, CancellationToken ct);
    Task<ServiceResult<bool>> DeleteLot(Guid id, CancellationToken ct);
    Task<ServiceResult<FefoAllocationPreviewDto>> RequestFefoAllocation(
        Guid jobId, Guid itemId, decimal requestedQuantity, CancellationToken ct);

    Task<ServiceResult<DraftConsumptionResponseDto>> CreateDraftConsumption(Guid jobId,
        CreateDraftConsumptionRequestDto request,
        Guid createdBy,
        CancellationToken ct);

    Task<ServiceResult<VerifyScanResponseDto>> VerifyScan(Guid jobId,
        VerifyScanRequestDto request,
        CancellationToken ct);

}