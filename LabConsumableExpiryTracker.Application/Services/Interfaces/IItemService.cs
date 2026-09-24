using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;

namespace LabConsumableExpiryTracker.Application.Services.Interfaces
{
    public interface IItemService
    {
        Task<ServiceResult<IEnumerable<ItemDto>>> GetAllItem(CancellationToken ct);
        Task<ServiceResult<PagedResult<ItemDto>>> GetAllItemPage(
            int page,
            int pageSize,
            CancellationToken ct
            );

        Task<ServiceResult<ItemDto>> GetByIdItem(Guid id, CancellationToken ct);

        Task<ServiceResult<ItemDto>> CreateItem(CreateItemDto lot, CancellationToken ct);

        Task<ServiceResult<ItemDto>> UpdateItem(
            Guid id,
            UpdateItemDto lot,
            CancellationToken ct);

        Task<ServiceResult<bool>> DeleteItem(Guid id, CancellationToken ct);
    }
}
