using FluentValidation;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Application.Services;

public sealed class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;
    private readonly IValidator<AuditQuery> _validator;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IAuditRepository auditRepository,
        IValidator<AuditQuery> validator,
        ILogger<AuditService> logger)
    {
        _auditRepository = auditRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<AuditEventDto>>> GetAuditEventsAsync(AuditQuery query, CancellationToken ct = default)
    {
        _logger.LogDebug(
            "Processing unified audit events request. EventType: {EventType}, ItemId: {ItemId}, LotId: {LotId}, JobId: {JobId}, Search: {Search}, Page: {Page}, PageSize: {PageSize}",
            query.EventType, query.ItemId, query.LotId, query.JobId, query.Search, query.Page, query.PageSize);

        var validation = await _validator.ValidateAsync(query, ct);
        if (!validation.IsValid)
        {
            _logger.LogWarning(
                "Unified audit query validation failed: {ValidationErrors}",
                string.Join("; ", validation.Errors.Select(x => x.ErrorMessage)));

            return ServiceResult<PagedResult<AuditEventDto>>.ErrorResult(
                "Validation failed.",
                validation.Errors.Select(x => x.ErrorMessage).ToList());
        }
        var normalized = query is { JobId: not null, EventType: null }
            ? query with { EventType = AuditEventType.Consumption }
            : query;

        if (normalized != query)
        {
            _logger.LogDebug(
                "FR-08-04: JobId provided without EventType. Auto-normalized EventType to Consumption for JobId {JobId}",
                query.JobId);
        }
        var result = await _auditRepository.GetAuditEventsAsync(normalized, ct);
        _logger.LogInformation(
            "Successfully retrieved {ItemCount} unified audit events (Page {Page}, TotalCount {TotalCount})",
            result.Items.Count, result.Page, result.TotalCount);

        return ServiceResult<PagedResult<AuditEventDto>>.SuccessResult(result);
    }

    public async Task<ServiceResult<PagedResult<ConsumptionAuditDto>>> GetConsumptionsAsync(AuditQuery query, CancellationToken ct = default)
    {
        _logger.LogDebug(
           "Processing consumption audit request. ItemId: {ItemId}, LotId: {LotId}, JobId: {JobId}, Search: {Search}, Page: {Page}, PageSize: {PageSize}",
           query.ItemId, query.LotId, query.JobId, query.Search, query.Page, query.PageSize);

        var validation = await _validator.ValidateAsync(
            query with { EventType = AuditEventType.Consumption }, ct);
        if (!validation.IsValid)
        {
            _logger.LogWarning(
                "Consumption audit query validation failed: {ValidationErrors}",
                string.Join("; ", validation.Errors.Select(x => x.ErrorMessage)));

            return ServiceResult<PagedResult<ConsumptionAuditDto>>.ErrorResult(
                "Validation failed.",
                validation.Errors.Select(x => x.ErrorMessage).ToList());
        }
        var result = await _auditRepository.GetConsumptionsAsync(query, ct);

        _logger.LogInformation(
            "Successfully retrieved {ItemCount} consumption audit records (Page {Page}, TotalCount {TotalCount})",
            result.Items.Count, result.Page, result.TotalCount);
        return ServiceResult<PagedResult<ConsumptionAuditDto>>.SuccessResult(result);
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
        _logger.LogDebug(
            "Processing disposal audit request. ItemId: {ItemId},Search: {Search} ,LotId: {LotId}, Page: {Page}, PageSize: {PageSize}",
            query.ItemId, query.LotId, query.Search, query.Page, query.PageSize);

        var validation = await _validator.ValidateAsync(
            query with { EventType = AuditEventType.Disposal }, ct);

        if (!validation.IsValid)
        {
            _logger.LogWarning(
                "Disposal audit query validation failed: {ValidationErrors}",
                string.Join("; ", validation.Errors.Select(x => x.ErrorMessage)));

            return ServiceResult<PagedResult<DisposalAuditDto>>.ErrorResult(
                "Validation failed.",
                validation.Errors.Select(x => x.ErrorMessage).ToList());
        }
        var result = await _auditRepository.GetDisposalsAsync(query, ct);

        _logger.LogInformation(
            "Successfully retrieved {ItemCount} disposal audit records (Page {Page}, TotalCount {TotalCount})",
            result.Items.Count, result.Page, result.TotalCount);

        return ServiceResult<PagedResult<DisposalAuditDto>>.SuccessResult(result);
    }

    public async Task<ServiceResult<DisposalAuditDto>> GetDisposalByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _auditRepository.GetDisposalByIdAsync(id, ct);
        return item is null
            ? ServiceResult<DisposalAuditDto>.ErrorResult("Disposal not found.")
            : ServiceResult<DisposalAuditDto>.SuccessResult(item);
    }
}
