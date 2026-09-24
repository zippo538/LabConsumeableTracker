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
        
        [HttpPost("{jobId:guid}/consumptions")]
        [Authorize(Roles = nameof(UserRole.Scientist))]
        public async Task<ActionResult<ServiceResult<ConsumptionResponseDto>>> ConfirmConsumption(
            Guid jobId,
            [FromBody] ConfirmConsumptionRequestDto request,
            CancellationToken ct)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var consumedBy))
            {
                return Unauthorized(ServiceResult<ConsumptionResponseDto>.ErrorResult("Invalid user token."));
            }
 
            var response = await lotService.ConfirmConsumption(jobId, request, consumedBy, ct);
 
            return response.Success
                ? Created("/api/consumptions", response)
                : Conflict(response);
        }


    }
