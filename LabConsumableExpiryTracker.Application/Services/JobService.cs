using AutoMapper;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.JobDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly TimeProvider _timeProvider;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public JobService(
            IJobRepository jobRepository,
            TimeProvider timeProvider,
            IMapper mapper,
            IUnitOfWork unitOfWork
            )
        {
            _jobRepository = jobRepository;
            _timeProvider = timeProvider;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }



        public async Task<ServiceResult<JobDto>> CreateAsync(CreateJobDto dto, CancellationToken ct = default)
        {
            if (dto == null)
            {
                return ServiceResult<JobDto>.ErrorResult(
               "Request cannot be null."
           );
            }

            if (string.IsNullOrWhiteSpace(dto.JobNumber))
            {
                return ServiceResult<JobDto>.ErrorResult("Job number is required.");
            }
            var jobNumber = dto.JobNumber.Trim();

            var existing = await _jobRepository.ExistsByJobNumberAsync(jobNumber, ct);
            if (existing)
            {
                return ServiceResult<JobDto>.ErrorResult($"Job number '{jobNumber}' already exists.");
            }

            var job = new Job(Guid.NewGuid(), jobNumber);
            var created = await _jobRepository.AddAsync(job, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            var response = _mapper.Map<JobDto>(created);
            return ServiceResult<JobDto>.SuccessResult(
                response,
                "Job Create successfully");
        }

        public async Task<ServiceResult<IReadOnlyList<JobDto>>> GetAllAsync(CancellationToken ct = default)
        {
            var jobs = await _jobRepository.GetAllAsync(ct);
            var response = _mapper.Map<IReadOnlyList<JobDto>>(jobs);
            return ServiceResult<IReadOnlyList<JobDto>>.SuccessResult(
                response,
                "Job retrieved successfully.");
        }

        public async Task<ServiceResult<JobDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty)
            {
                return ServiceResult<JobDto>.ErrorResult("Id is null");
            }
            var create = await _jobRepository.GetByIdAsync(id, ct);
            if (create is null)
            {
                return ServiceResult<JobDto>.ErrorResult(
                    $"Job with id '{id}' not found.");
            }
            var response = _mapper.Map<JobDto>(create);
            return ServiceResult<JobDto>.SuccessResult(
                response);

        }
        public async Task<ServiceResult<JobDto>> UpdateStatusAsync(Guid id, UpdateJobDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            return dto.Status switch
            {
                JobStatus.InProgress => await StartAsync(id, ct),
                JobStatus.Completed => await CompleteAsync(id, ct),
                _ => throw new InvalidOperationException("Only InProgress and Completed are valid lifecycle updates.")
            };

        }

        public async Task<ServiceResult<JobDto>> StartAsync(Guid id, CancellationToken ct = default)
        {
            return await TransitionAsync(
               id,
               job => job.Start(_timeProvider.GetUtcNow()),
               ct);
        }
        public async Task<ServiceResult<JobDto>> CompleteAsync(Guid id, CancellationToken ct = default)
        {
            return await TransitionAsync(
                id,
                job => job.Complete(_timeProvider.GetUtcNow()),
                ct);
        }
        private async Task<ServiceResult<JobDto>> TransitionAsync(Guid id, Action<Job> transition, CancellationToken ct)
        {
            if (id == Guid.Empty)
            {
                return ServiceResult<JobDto>.ErrorResult("Job id is null");

            }
            var job = await _jobRepository.GetByIdAsync(id, ct);
            if (job is null)
            {
                return ServiceResult<JobDto>.ErrorResult($"Job with id '{id}' not found.");
            }
            transition(job);
            var create = await _jobRepository.UpdateAsync(job, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            var response = _mapper.Map<JobDto>(create);
            return ServiceResult<JobDto>.SuccessResult(response);
        }


    }
}