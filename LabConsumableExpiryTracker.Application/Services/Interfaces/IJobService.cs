using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.JobDTOs;

namespace LabConsumableExpiryTracker.Application.Services.Interfaces
{
    public interface IJobService
    {
        Task<ServiceResult<JobDto>> CreateAsync(CreateJobDto dto, CancellationToken ct = default);
        Task<ServiceResult<JobDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<ServiceResult<IReadOnlyList<JobDto>>> GetAllAsync(CancellationToken ct = default);
        Task<ServiceResult<JobDto>> StartAsync(Guid id, CancellationToken ct = default);
        Task<ServiceResult<JobDto>> CancelAsync(Guid id, CancellationToken ct = default);
        Task<ServiceResult<JobDto>> CompleteAsync(Guid id, CancellationToken ct = default);
        Task<ServiceResult<JobDto>> UpdateStatusAsync(Guid id, UpdateJobDto dto, CancellationToken ct = default);

    }
}