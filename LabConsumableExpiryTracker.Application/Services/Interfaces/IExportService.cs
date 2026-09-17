using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;

namespace LabConsumableExpiryTracker.Application.Services.Interfaces;

public interface IExportService
{
    Task<ServiceResult<IEnumerable<StockExportRowDto>>> GetStockExport(
        StockExportFilterDto filter, CancellationToken ct);

    Task<ServiceResult<IEnumerable<ConsumptionExportRowDto>>> GetConsumptionExport(
        ConsumptionExportFilterDto filter, CancellationToken ct);

    Task<ServiceResult<IEnumerable<DisposalExportRowDto>>> GetDisposalExport(
        DisposalExportFilterDto filter, CancellationToken ct);
}