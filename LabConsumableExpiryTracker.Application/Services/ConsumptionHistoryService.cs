using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ConsumptionHistoryDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;

namespace LabConsumableExpiryTracker.Application.Services;

public class ConsumptionHistoryService : IConsumptionHistoryService
{
    private readonly IConsumptionHistoryRepository _consumptionHistoryRepository;

    public ConsumptionHistoryService(IConsumptionHistoryRepository consumptionHistoryRepository)
    {
        _consumptionHistoryRepository = consumptionHistoryRepository;
    }

    public async Task<ServiceResult<MyConsumptionsResponseDto>> GetMyConsumptions(
        Guid consumedBy,
        ConsumptionListQueryDto query,
        CancellationToken ct)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _consumptionHistoryRepository.GetPagedAsync(
            consumedBy,
            query.JobId,
            query.ItemId,
            query.LotId,
            query.LotNumber,
            query.From,
            query.To,
            page,
            pageSize,
            ct);

        var response = new MyConsumptionsResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return ServiceResult<MyConsumptionsResponseDto>.SuccessResult(
            response,
            "Consumptions retrieved successfully.");
    }

    public async Task<ServiceResult<ConsumptionHistoryDetailDto>> GetConsumptionDetail(
        Guid consumptionId,
        Guid consumedBy,
        CancellationToken ct)
    {
        var consumption = await _consumptionHistoryRepository.GetDetailAsync(
            consumptionId,
            consumedBy,
            ct);

        if (consumption is null)
        {
            return ServiceResult<ConsumptionHistoryDetailDto>.ErrorResult(
                "Consumption not found.");
        }

        return ServiceResult<ConsumptionHistoryDetailDto>.SuccessResult(
            consumption,
            "Consumption retrieved successfully.");
    }
}