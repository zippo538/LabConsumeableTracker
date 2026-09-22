using FluentValidation;
using LabConsumableExpiryTracker.Application.Common.CurrentUser;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.Extensions.Logging; 

namespace LabConsumableExpiryTracker.Application.Services;

public class DashboardAlertService : IDashboardAlertService
{
    private const string ScientistRole = nameof(UserRole.Scientist);
    private const string WarehouseAdminRole = nameof(UserRole.WarehouseAdmin);

    private readonly IDashboardAlertRepository _repository;
    private readonly IValidator<DashboardAlertQuery> _validator;
    private readonly ICurrentUserContext _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DashboardAlertService> _logger; 

    public DashboardAlertService(
        IDashboardAlertRepository repository,
        IValidator<DashboardAlertQuery> validator,
        ICurrentUserContext currentUser,
        TimeProvider timeProvider,
        ILogger<DashboardAlertService> logger) 
    {
        _repository = repository;
        _validator = validator;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<DashboardAlertDto>>> GetAlertsAsync(
        DashboardAlertQuery query,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Processing dashboard alert request. Type: {AlertType}, Page: {Page}, PageSize: {PageSize}",
            query.Type, query.Page, query.PageSize);

        var validation = await _validator.ValidateAsync(query, ct);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Dashboard alert query validation failed: {ValidationErrors}",
                string.Join("; ", validation.Errors.Select(x => x.ErrorMessage)));

            return ServiceResult<PagedResult<DashboardAlertDto>>.ErrorResult(
                "Validation failed.",
                validation.Errors.Select(x => x.ErrorMessage).ToList());
        }

        var isWarehouseAdmin = _currentUser.IsInRole(WarehouseAdminRole);
        var isScientist = _currentUser.IsInRole(ScientistRole);

        if (!isWarehouseAdmin && !isScientist)
        {
            _logger.LogWarning("Unauthorized access attempt to dashboard alerts. User lacks 'Scientist' or 'WarehouseAdmin' role.");

            return ServiceResult<PagedResult<DashboardAlertDto>>.ErrorResult(
                "You are not authorized to view dashboard alerts.");
        }

        var now = _timeProvider.GetUtcNow();
        _logger.LogDebug("Fetching alerts from repository evaluated at {EvaluatedAtUtc}", now);

        var page = await _repository.GetAlertsAsync(query, now, ct);

        var response = new PagedResult<DashboardAlertDto>
        {
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
            Items = page.Items.Select(alert => alert with
            {
                AllowedActions = GetAllowedActions(alert, isWarehouseAdmin)
            }).ToList()
        };

        _logger.LogInformation("Successfully retrieved {ItemCount} dashboard alerts (Page {Page} of {TotalCount} total items)",
            response.Items.Count, response.Page, response.TotalCount);

        return ServiceResult<PagedResult<DashboardAlertDto>>.SuccessResult(response);
    }

    private static IReadOnlyList<QuickActionDto> GetAllowedActions(
        DashboardAlertDto alert,
        bool isWarehouseAdmin)
    {
        var resourceHref = alert.ResourceType == AlertResourceType.Item
            ? $"/app/items/{alert.ItemId}"
            : $"/app/lots/{alert.LotId}";

        var actions = new List<QuickActionDto>
        {
            new()
            {
                Code = alert.ResourceType == AlertResourceType.Item ? "ViewItem" : "ViewLot",
                Label = alert.ResourceType == AlertResourceType.Item ? "View item" : "View lot",
                Href = resourceHref
            }
        };

        if (!isWarehouseAdmin)
            return actions;

        switch (alert.AlertType)
        {
            case DashboardAlertType.LowStock:
                actions.Add(new QuickActionDto
                {
                    Code = "RegisterLot",
                    Label = "Register lot",
                    Href = $"/app/lots?itemId={alert.ItemId}&action=create"
                });
                break;
            case DashboardAlertType.ExpiringSoon:
            case DashboardAlertType.Expired:
                actions.Add(new QuickActionDto
                {
                    Code = "OpenDisposal",
                    Label = "Review disposal",
                    Href = $"/app/lots/{alert.LotId}?action=dispose"
                });
                break;
            case DashboardAlertType.Blocked:
                actions.Add(new QuickActionDto
                {
                    Code = "ManageLotStatus",
                    Label = "Manage status",
                    Href = $"/app/lots/{alert.LotId}?action=status"
                });
                break;
        }

        return actions;
    }
}