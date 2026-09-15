using FluentValidation;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.AuditDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.Services;

public sealed class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;
    private readonly IValidator<AuditQuery> _validator;

    public AuditService(IAuditRepository auditRepository, IValidator<AuditQuery> validator)
    {
        _auditRepository = auditRepository;
        _validator = validator;
    }

    public async Task<ServiceResult<PagedResult<AuditEventDto>>> GetAuditEventsAsync(AuditQuery query, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(query, ct);
        if (!validation.IsValid)
            return ServiceResult<PagedResult<AuditEventDto>>.ErrorResult("Validation failed.", validation.Errors.Select(x => x.ErrorMessage).ToList());

        // SRS FR-08-04: JobId without EventType means Consumption only.
        var normalized = query.JobId.HasValue && !query.EventType.HasValue
            ? new AuditQuery
            {
                EventType = AuditEventType.Consumption,
                ItemId = query.ItemId, LotId = query.LotId, JobId = query.JobId,
                From = query.From, To = query.To, Page = query.Page, PageSize = query.PageSize
            }
            : query;

        return ServiceResult<PagedResult<AuditEventDto>>.SuccessResult(
            await _auditRepository.GetAuditEventsAsync(normalized, ct));
    }

    public async Task<ServiceResult<PagedResult<ConsumptionAuditDto>>> GetConsumptionsAsync(AuditQuery query, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(query with { EventType = AuditEventType.Consumption }, ct);
        if (!validation.IsValid)
            return ServiceResult<PagedResult<ConsumptionAuditDto>>.ErrorResult("Validation failed.", validation.Errors.Select(x => x.ErrorMessage).ToList());

        return ServiceResult<PagedResult<ConsumptionAuditDto>>.SuccessResult(
            await _auditRepository.GetConsumptionsAsync(query, ct));
    }

    public async Task<ServiceResult<ConsumptionAuditDto>> GetConsumptionByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _auditRepository.GetConsumptionByIdAsync(id, ct);
        return item is null
            ? ServiceResult<ConsumptionAuditDto>.ErrorResult("Consumption not found.")
            : ServiceResult<ConsumptionAuditDto>.SuccessResult(item);
    }

    public async Task<ServiceResult<PagedResult<DisposalAuditDto>>> GetDisposalsAsync(AuditQuery query, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(query with { EventType = AuditEventType.Disposal }, ct);
        if (!validation.IsValid)
            return ServiceResult<PagedResult<DisposalAuditDto>>.ErrorResult("Validation failed.", validation.Errors.Select(x => x.ErrorMessage).ToList());

        return ServiceResult<PagedResult<DisposalAuditDto>>.SuccessResult(
            await _auditRepository.GetDisposalsAsync(query, ct));
    }

    public async Task<ServiceResult<DisposalAuditDto>> GetDisposalByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _auditRepository.GetDisposalByIdAsync(id, ct);
        return item is null
            ? ServiceResult<DisposalAuditDto>.ErrorResult("Disposal not found.")
            : ServiceResult<DisposalAuditDto>.SuccessResult(item);
    }
}
