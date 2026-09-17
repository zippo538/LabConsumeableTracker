using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;
using LabConsumableExpiryTracker.Application.DTOs.FefoDtos;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Services.Interfaces
{
    public interface ILotService
    {
        Task<ServiceResult<IEnumerable<LotDto>>> GetAllLot(CancellationToken ct);
        Task<ServiceResult<OffsetPagedResult<LotDto>>> GetAllLotOffsetPage(int offset, int limit,CancellationToken ct);
        Task<ServiceResult<IEnumerable<LotSummaryDto>>> GetSummaryByItemId(Guid itemId, CancellationToken ct);
        Task<ServiceResult<IEnumerable<LotSummaryDto>>> GetAllSummary(CancellationToken ct);
        Task<ServiceResult<LotDto>> GetByIdLot(Guid id, CancellationToken ct);
        Task<ServiceResult<LotDto>> CreateLot(CreateLotDto lot, CancellationToken ct);
        Task<ServiceResult<LotDto>> UpdateLot(Guid id, UpdateLotDto lot, CancellationToken ct);
        Task<ServiceResult<bool>> DeleteLot(Guid id, CancellationToken ct);
        Task<ServiceResult<FefoAllocationPreviewDto>> RequestFefoAllocation(
            Guid jobId, Guid itemId, decimal requestedQuantity, CancellationToken ct);

        Task<ServiceResult<ConsumptionResponseDto>> ConfirmConsumption(Guid jobId,
            ConfirmConsumptionRequestDto request,
            Guid consumedBy,
            CancellationToken ct);


    }
}