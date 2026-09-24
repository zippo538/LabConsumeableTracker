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
[Authorize(Roles = $"{nameof(UserRole.WarehouseAdmin)}")]
public class ExportsController(IExportService exportService) : ControllerBase
{
    [HttpGet("stock")]
    public async Task<IActionResult> ExportStock(
        [FromQuery] StockExportFilterDto filter,
        CancellationToken ct)
    {
        var result = await exportService.GetStockExport(
         filter, ct);

        return result.Success
            ? ToCsvFile(result.Data!, "stock-export")
            : BadRequest(result);
    }

    [HttpGet("consumptions")]
    public async Task<IActionResult> ExportConsumptions(
        [FromQuery] ConsumptionExportFilterDto filter,
        CancellationToken ct)
    {
        var result = await exportService.GetConsumptionExport(filter, ct);

        return result.Success
            ? ToCsvFile(result.Data!, "consumptions-export")
            : BadRequest(result);
    }

    [HttpGet("disposals")]
    public async Task<IActionResult> ExportDisposals(
       [FromQuery] DisposalExportFilterDto filter,
        CancellationToken ct)
    {
        var result = await exportService.GetDisposalExport(
           filter, ct);

        return result.Success
            ? ToCsvFile(result.Data!, "disposals-export")
            : BadRequest(result);
    }

    private FileContentResult ToCsvFile<T>(IEnumerable<T> rows, string fileNamePrefix)
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), leaveOpen: true))
        {
            writer.WriteLine("sep=,");

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