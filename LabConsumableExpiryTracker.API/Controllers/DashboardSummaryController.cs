using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(Roles = AllowedRoles)] // SRS: Akses S/W (scientis & WarehouseAdmin)
    public class DashboardSummaryController : ControllerBase
    {
        public const string AllowedRoles = nameof(UserRole.Scientist) + "," + nameof(UserRole.WarehouseAdmin);
        private readonly IDashboardSummaryService _dashboardSummaryService;
        public DashboardSummaryController(IDashboardSummaryService dashboardSummaryService)
        {
            _dashboardSummaryService = dashboardSummaryService;
        }
        [HttpGet("summary")]
        [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSummary([FromQuery] Guid? itemId, CancellationToken ct)
        {
            var response = await _dashboardSummaryService.GetSummaryAsync(itemId, ct);

            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}