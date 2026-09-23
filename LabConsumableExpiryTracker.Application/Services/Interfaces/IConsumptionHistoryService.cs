using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ConsumptionHistoryDTOs;

namespace LabConsumableExpiryTracker.Application.Services.Interfaces;

public interface IConsumptionHistoryService
{
    Task<ServiceResult<MyConsumptionsResponseDto>> GetMyConsumptions(
        Guid consumedBy,
        ConsumptionListQueryDto query,
        CancellationToken ct);
       
    Task<ServiceResult<ConsumptionHistoryDetailDto>> GetConsumptionDetail(
        Guid consumptionId,
        Guid consumedBy,
        CancellationToken ct);
}