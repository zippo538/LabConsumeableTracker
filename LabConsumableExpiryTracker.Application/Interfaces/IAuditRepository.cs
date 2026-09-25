using LabConsumableExpiryTracker.Application.DTOs.AuditDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;

namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface IAuditRepository 
{
    Task<PagedResult<AuditEventDto>> GetAuditEventsAsync(AuditQuery query, CancellationToken ct = default);
    Task<PagedResult<ConsumptionAuditDto>> GetConsumptionsAsync(AuditQuery query, CancellationToken ct = default);
    Task<ConsumptionAuditDto?> GetConsumptionByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<DisposalAuditDto>> GetDisposalsAsync(AuditQuery query, CancellationToken ct = default);
    Task<DisposalAuditDto?> GetDisposalByIdAsync(Guid id, CancellationToken ct = default);
}
