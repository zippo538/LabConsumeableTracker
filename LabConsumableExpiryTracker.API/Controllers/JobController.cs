using System.Security.Claims;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;
using LabConsumableExpiryTracker.Application.DTOs.FefoDtos;
using LabConsumableExpiryTracker.Application.DTOs.JobDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers ;
    [ApiController]
    [Route("api/jobs")]
    [Authorize(Roles = nameof(UserRole.Scientist))]
    public class JobController(
        IJobService jobService, ILotService lotService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = nameof(UserRole.Scientist))]
        public async Task<ActionResult<ServiceResult<IReadOnlyList<JobDto>>>> GetAll(CancellationToken ct)
        {
            var response = await jobService.GetAllAsync(ct);
            return response.Success
                ? Ok(response)
                : NotFound(response);
        }


        [HttpGet("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.Scientist))]
        public async Task<ActionResult<ServiceResult<JobDto>>> GetById(Guid id, CancellationToken ct)
        {
            var response = await jobService.GetByIdAsync(id, ct);
            return response.Success
                ? Ok(response)
                : NotFound(response);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Scientist))]
        public async Task<ActionResult<ServiceResult<JobDto>>> Create(CreateJobDto? dto, CancellationToken ct)
        {
            var response = await jobService.CreateAsync(dto, ct);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return response.Success
                ? CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response)
                : Conflict(response);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.Scientist))]
        public async Task<ActionResult<ServiceResult<JobDto>>> Update(Guid id, UpdateJobDto dto, CancellationToken ct)
        {
            var response = await jobService.UpdateAsync(id, dto, ct);
            if (!response.Success)
            {
                return NotFound(response);
            }

            return response.Success
                ? Ok(response)
                : Conflict(response);
        }

        [HttpPost("{id:guid}/start")]
        [Authorize(Roles = nameof(UserRole.Scientist))]
        public async Task<ActionResult<ServiceResult<JobDto>>> Start(Guid id, CancellationToken ct)
        {
            var response = await jobService.StartAsync(id, ct);
            return response.Success
                ? Ok(response)
                : BadRequest(response);
        }
        
        [HttpPost("{id:guid}/cancel")]
        [Authorize(Roles = nameof(UserRole.Scientist))]
        public async Task<ActionResult<ServiceResult<JobDto>>> Cancel(Guid id, CancellationToken ct)
        {
            var response = await jobService.CancelAsync(id, ct);
            return response.Success
                ? Ok(response)
                : BadRequest(response);
        }
        
        [HttpPost("{id:guid}/complete")]
        [Authorize(Roles = nameof(UserRole.Scientist))]
        public async Task<ActionResult<ServiceResult<JobDto>>> Complete(Guid id, CancellationToken ct)
        {
            var response = await jobService.CompleteAsync(id, ct);
            return response.Success
                ? Ok(response)
                : BadRequest(response);
        }

        [HttpPost("{jobId:guid}/allocations/preview")]
        [Authorize(Roles = nameof(UserRole.Scientist))]
        public async Task<ActionResult<ServiceResult<FefoAllocationPreviewDto>>> PreviewAllocation(
            Guid jobId,
            [FromBody] RequestFefoAllocationDto request,
            CancellationToken ct)
        {
            var response = await lotService.RequestFefoAllocation(
                jobId, request.ItemId, request.RequestedQuantity, ct);

            return response.Success ? Ok(response) : BadRequest(response);
        }
        
        [HttpPost("{jobId:guid}/consumptions/draft")]
        [Authorize(Roles = nameof(UserRole.Scientist))]
        public async Task<ActionResult<ServiceResult<DraftConsumptionResponseDto>>> CreateDraftConsumption(
            Guid jobId,
            [FromBody] CreateDraftConsumptionRequestDto request,
            CancellationToken ct)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var createdBy))
            {
                return Unauthorized(ServiceResult<DraftConsumptionResponseDto>.ErrorResult("Invalid user token."));
            }

            var response = await lotService.CreateDraftConsumption(jobId, request, createdBy, ct);

            if (response.Success)
            {
                return Created($"/api/jobs/{jobId}/consumptions/draft", response);
            }

            return ToActionResult(
                response,
                ErrorCodes.ValidationError,
                ErrorCodes.LotNotFound,
                ErrorCodes.JobNotFound);
        }

        [HttpPost("{jobId:guid}/consumptions/verify-scan")]
        [Authorize(Roles = nameof(UserRole.Scientist))]
        public async Task<ActionResult<ServiceResult<VerifyScanResponseDto>>> VerifyScan(
            Guid jobId,
            [FromBody] VerifyScanRequestDto request,
            CancellationToken ct)
        {
            var response = await lotService.VerifyScan(jobId, request, ct);

            if (response.Success)
            {
                return Ok(response);
            }

            return ToActionResult(
                response,
                ErrorCodes.ValidationError,
                ErrorCodes.LotNotFound,
                ErrorCodes.JobNotFound);
        }

        /// <summary>
        /// Memetakan <see cref="ServiceResult{T}.Code"/> ke status HTTP: 400 untuk
        /// validasi, 404 untuk entity hilang, 409 untuk konflik state/stok.
        /// </summary>
        private ActionResult<ServiceResult<T>> ToActionResult<T>(
            ServiceResult<T> response,
            string validationCode,
            params string[] notFoundCodes)
        {
            if (response.Code == validationCode)
            {
                return BadRequest(response);
            }

            if (Array.IndexOf(notFoundCodes, response.Code) >= 0)
            {
                return NotFound(response);
            }

            return Conflict(response);
        }

    }
