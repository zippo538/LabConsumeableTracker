using FluentValidation;
using LabConsumableExpiryTracker.Application.Common.CurrentUser;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.Services;

public class DashboardAlertService : IDashboardAlertService
{
    private const string ScientistRole = nameof(UserRole.Scientist);
    private const string WarehouseAdminRole = nameof(UserRole.WarehouseAdmin);

    private readonly IDashboardAlertRepository _repository;
    private readonly IValidator<DashboardAlertQuery> _validator;
    private readonly ICurrentUserContext _currentUser;
    private readonly TimeProvider _timeProvider;

    public DashboardAlertService(
        IDashboardAlertRepository repository,
        IValidator<DashboardAlertQuery> validator,
        ICurrentUserContext currentUser,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _validator = validator;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<ServiceResult<PagedResult<DashboardAlertDto>>> GetAlertsAsync(
        DashboardAlertQuery query,
        CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(query, ct);
        if (!validation.IsValid)
        {
            return ServiceResult<PagedResult<DashboardAlertDto>>.ErrorResult(
                "Validation failed.",
                validation.Errors.Select(x => x.ErrorMessage).ToList());
        }

        var isWarehouseAdmin = _currentUser.IsInRole(WarehouseAdminRole);
        var isScientist = _currentUser.IsInRole(ScientistRole);

        if (!isWarehouseAdmin && !isScientist)
        {
            return ServiceResult<PagedResult<DashboardAlertDto>>.ErrorResult(
                "You are not authorized to view dashboard alerts.");
        }

        var page = await _repository.GetAlertsAsync(query, _timeProvider.GetUtcNow(), ct);

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

        // These are navigation intents only. The target screen must still show a
        // confirmation and call its separately authorized mutation endpoint.
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
