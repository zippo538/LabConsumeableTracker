using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.AuditDTOs;

namespace LabConsumableExpiryTracker.Application.Services.Interfaces;

public interface IAuditService
{
    Task<ServiceResult<PagedResult<AuditEventDto>>> GetAuditEventsAsync(AuditQuery query, CancellationToken ct = default);
    Task<ServiceResult<PagedResult<ConsumptionAuditDto>>> GetConsumptionsAsync(AuditQuery query, CancellationToken ct = default);
    Task<ServiceResult<ConsumptionAuditDto>> GetConsumptionByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<PagedResult<DisposalAuditDto>>> GetDisposalsAsync(AuditQuery query, CancellationToken ct = default);
    Task<ServiceResult<DisposalAuditDto>> GetDisposalByIdAsync(Guid id, CancellationToken ct = default);
}
