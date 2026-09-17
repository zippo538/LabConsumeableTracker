using CsvHelper;
using System.Globalization;
using System.Text;
using CsvHelper.Configuration;
using LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api/exports")]
[Authorize(Roles = $"{nameof(UserRole.WarehouseAdmin)},{nameof(UserRole.Scientist)}")] // "Kedua role"
public class ExportsController : ControllerBase
{
    private readonly IExportService _exportService;

    public ExportsController(IExportService exportService) => _exportService = exportService;

    [HttpGet("stock")]
    public async Task<IActionResult> ExportStock(
        [FromQuery] string format,
        [FromQuery] Guid? itemId,
        [FromQuery] bool? isLowStock,
        CancellationToken ct)
    {
        var result = await _exportService.GetStockExport(
            new StockExportFilterDto { Format = format, ItemId = itemId, IsLowStock = isLowStock }, ct);

        return result.Success
            ? ToCsvFile(result.Data!, "stock-export")
            : BadRequest(result);
    }

    [HttpGet("consumptions")]
    public async Task<IActionResult> ExportConsumptions(
        [FromQuery] string format,
        [FromQuery] Guid? jobId,
        [FromQuery] Guid? itemId,
        [FromQuery] Guid? lotId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken ct)
    {
        var result = await _exportService.GetConsumptionExport(
            new ConsumptionExportFilterDto
            {
                Format = format, JobId = jobId, ItemId = itemId, LotId = lotId, From = from, To = to
            }, ct);

        return result.Success
            ? ToCsvFile(result.Data!, "consumptions-export")
            : BadRequest(result);
    }

    [HttpGet("disposals")]
    public async Task<IActionResult> ExportDisposals(
        [FromQuery] string format,
        [FromQuery] Guid? itemId,
        [FromQuery] Guid? lotId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken ct)
    {
        var result = await _exportService.GetDisposalExport(
            new DisposalExportFilterDto { Format = format, ItemId = itemId, LotId = lotId, From = from, To = to }, ct);

        return result.Success
            ? ToCsvFile(result.Data!, "disposals-export")
            : BadRequest(result);
    }

    // SRS §7.10: UTF-8, Content-Disposition: attachment, header-only kalau kosong.
    private FileContentResult ToCsvFile<T>(IEnumerable<T> rows, string fileNamePrefix)
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), leaveOpen: true))
        {
            writer.WriteLine("sep=,");   // ← baris baru, WAJIB paling atas, sebelum header

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ","
            };

            using var csv = new CsvWriter(writer, config);
            csv.WriteHeader<T>();
            csv.NextRecord();
            foreach (var row in rows)
            {
                csv.WriteRecord(row);
                csv.NextRecord();
            }
        }

        var fileName = $"{fileNamePrefix}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(memoryStream.ToArray(), "text/csv", fileName);
    }
}