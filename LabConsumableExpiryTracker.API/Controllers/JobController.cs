
using Bogus.DataSets;
using FluentValidation;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.JobDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    [Authorize(Roles = nameof(UserRole.Scientist))]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;
        public JobController(
            IJobService jobService,
            IValidator<CreateJobDto> createJobValidator,
            IValidator<UpdateJobDto> updateJobValidator)
        {
            _jobService = jobService;
        }
        [HttpGet]
        [Authorize(Roles = nameof(UserRole.Scientist))]
        public async Task<ActionResult<ServiceResult<IReadOnlyList<JobDto>>>> GetAll(CancellationToken ct)
        {
            var response = await _jobService.GetAllAsync(ct);
            return response.Success
            ? Ok(response)
            : NotFound(response);
        }


        [HttpGet("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.Scientist))]
        public async Task<ActionResult<ServiceResult<JobDto>>> GetById(Guid id, CancellationToken ct)
        {
            var response = await _jobService.GetByIdAsync(id, ct);
            return response.Success
            ? Ok(response)
            : NotFound(response);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Scientist))]
        public async Task<ActionResult<ServiceResult<JobDto>>> Create(CreateJobDto dto, CancellationToken ct)
        {

            var response = await _jobService.CreateAsync(dto, ct);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            // return CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response);
            return response.Success
            ? CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response)
            : Conflict(response);


        }
        [HttpPut]
        [Authorize(Roles = nameof(UserRole.Scientist))]
        public async Task<ActionResult<ServiceResult<JobDto>>> Update(Guid id, UpdateJobDto dto, CancellationToken ct)
        {
            var response = await _jobService.UpdateStatusAsync(id, dto, ct);
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
            var response = await _jobService.StartAsync(id, ct);
            return response.Success
            ? Ok(response)
            : BadRequest(response);

        }

        [HttpPost("{id:guid}/complete")]
        [Authorize(Roles = nameof(UserRole.Scientist))]
        public async Task<ActionResult<ServiceResult<JobDto>>> Complete(Guid id, CancellationToken ct)
        {
            var response = await _jobService.CompleteAsync(id, ct);
            return response.Success
            ? Ok(response)
            : BadRequest(response);
        }
    }
}