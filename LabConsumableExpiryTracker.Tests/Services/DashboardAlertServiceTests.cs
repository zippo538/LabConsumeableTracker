using FluentValidation;
using FluentValidation.Results;
using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Services;

[TestFixture]
public class DashboardAlertServiceTests
{
    private static readonly DateTimeOffset FixedNow =
        new(
            2026,
            9,
            25,
            10,
            0,
            0,
            TimeSpan.Zero);

    private Mock<IDashboardAlertRepository> _repository = null!;
    private Mock<IValidator<DashboardAlertQuery>> _validator = null!;
    private Mock<ICurrentUserContext> _currentUser = null!;
    private Mock<TimeProvider> _timeProvider = null!;
    private Mock<ILogger<DashboardAlertService>> _logger = null!;
    private DashboardAlertService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repository =
            new Mock<IDashboardAlertRepository>(MockBehavior.Strict);

        _validator =
            new Mock<IValidator<DashboardAlertQuery>>(MockBehavior.Strict);

        _currentUser =
            new Mock<ICurrentUserContext>(MockBehavior.Strict);

        _timeProvider =
            new Mock<TimeProvider>(MockBehavior.Strict);

        _logger =
            new Mock<ILogger<DashboardAlertService>>();

        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(FixedNow);

        _service = new DashboardAlertService(
            _repository.Object,
            _validator.Object,
            _currentUser.Object,
            _timeProvider.Object,
            _logger.Object);
    }

    [Test]
    public async Task GetAlertsAsync_ShouldReturnError_WhenValidationFails()
    {
        var query = CreateQuery();

        var validationResult = new ValidationResult(
        [
            new ValidationFailure(
                nameof(DashboardAlertQuery.Page),
                "Page must be greater than zero.")
        ]);

        _validator
            .Setup(x => x.ValidateAsync(
                query,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var result = await _service.GetAlertsAsync(query);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Validation failed."));
        Assert.That(result.Errors, Has.Count.EqualTo(1));
        Assert.That(
            result.Errors[0],
            Is.EqualTo("Page must be greater than zero."));

        _currentUser.Verify(
            x => x.IsInRole(It.IsAny<string>()),
            Times.Never);

        _repository.Verify(
            x => x.GetAlertsAsync(
                It.IsAny<DashboardAlertQuery>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetAlertsAsync_ShouldReturnError_WhenUserHasNoAllowedRole()
    {
        var query = CreateQuery();

        SetupValidValidation();

        _currentUser
            .Setup(x => x.IsInRole("WarehouseAdmin"))
            .Returns(false);

        _currentUser
            .Setup(x => x.IsInRole("Scientist"))
            .Returns(false);

        var result = await _service.GetAlertsAsync(query);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo(
                "You are not authorized to view dashboard alerts."));

        _repository.Verify(
            x => x.GetAlertsAsync(
                It.IsAny<DashboardAlertQuery>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetAlertsAsync_ShouldReturnAlerts_ForScientist()
    {
        var query = CreateQuery();

        var alert = CreateAlert(
            DashboardAlertType.LowStock,
            AlertResourceType.Item);

        var page = CreatePagedResult(alert);

        SetupValidValidation();

        _currentUser
            .Setup(x => x.IsInRole("WarehouseAdmin"))
            .Returns(false);

        _currentUser
            .Setup(x => x.IsInRole("Scientist"))
            .Returns(true);

        _repository
            .Setup(x => x.GetAlertsAsync(
                query,
                FixedNow,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _service.GetAlertsAsync(query);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Items, Has.Count.EqualTo(1));

        var actions = result.Data.Items[0].AllowedActions;

        Assert.That(actions, Has.Count.EqualTo(1));
        Assert.That(actions[0].Code, Is.EqualTo("ViewItem"));
        Assert.That(actions[0].Label, Is.EqualTo("View item"));
        Assert.That(
            actions[0].Href,
            Is.EqualTo($"/app/items/{alert.ItemId}"));
    }

    [Test]
    public async Task GetAlertsAsync_ShouldReturnLotAction_ForScientist()
    {
        var query = CreateQuery();

        var alert = CreateAlert(
            DashboardAlertType.Expired,
            AlertResourceType.Lot);

        var page = CreatePagedResult(alert);

        SetupValidValidation();

        _currentUser
            .Setup(x => x.IsInRole("WarehouseAdmin"))
            .Returns(false);

        _currentUser
            .Setup(x => x.IsInRole("Scientist"))
            .Returns(true);

        _repository
            .Setup(x => x.GetAlertsAsync(
                query,
                FixedNow,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _service.GetAlertsAsync(query);

        Assert.That(result.Success, Is.True);

        var actions = result.Data!.Items[0].AllowedActions;

        Assert.That(actions, Has.Count.EqualTo(1));
        Assert.That(actions[0].Code, Is.EqualTo("ViewLot"));
        Assert.That(actions[0].Label, Is.EqualTo("View lot"));
        Assert.That(
            actions[0].Href,
            Is.EqualTo($"/app/lots/{alert.LotId}"));
    }

    [Test]
    public async Task GetAlertsAsync_ShouldAddRegisterLotAction_ForWarehouseAdminLowStock()
    {
        var query = CreateQuery();

        var alert = CreateAlert(
            DashboardAlertType.LowStock,
            AlertResourceType.Item);

        var page = CreatePagedResult(alert);

        SetupValidValidation();

        SetupWarehouseAdmin();

        _repository
            .Setup(x => x.GetAlertsAsync(
                query,
                FixedNow,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _service.GetAlertsAsync(query);

        Assert.That(result.Success, Is.True);

        var actions = result.Data!.Items[0].AllowedActions;

        Assert.That(actions, Has.Count.EqualTo(2));

        Assert.That(actions[0].Code, Is.EqualTo("ViewItem"));
        Assert.That(actions[1].Code, Is.EqualTo("RegisterLot"));
        Assert.That(actions[1].Label, Is.EqualTo("Register lot"));
        Assert.That(
            actions[1].Href,
            Is.EqualTo(
                $"/app/lots?itemId={alert.ItemId}&action=create"));
    }

    [Test]
    public async Task GetAlertsAsync_ShouldAddDisposalAction_ForWarehouseAdminExpiringSoon()
    {
        var query = CreateQuery();

        var alert = CreateAlert(
            DashboardAlertType.ExpiringSoon,
            AlertResourceType.Lot);

        var page = CreatePagedResult(alert);

        SetupValidValidation();

        SetupWarehouseAdmin();

        _repository
            .Setup(x => x.GetAlertsAsync(
                query,
                FixedNow,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _service.GetAlertsAsync(query);

        Assert.That(result.Success, Is.True);

        var actions = result.Data!.Items[0].AllowedActions;

        Assert.That(actions, Has.Count.EqualTo(2));
        Assert.That(actions[0].Code, Is.EqualTo("ViewLot"));
        Assert.That(actions[1].Code, Is.EqualTo("OpenDisposal"));
        Assert.That(actions[1].Label, Is.EqualTo("Review disposal"));
        Assert.That(
            actions[1].Href,
            Is.EqualTo(
                $"/app/lots/{alert.LotId}?action=dispose"));
    }

    [Test]
    public async Task GetAlertsAsync_ShouldAddDisposalAction_ForWarehouseAdminExpired()
    {
        var query = CreateQuery();

        var alert = CreateAlert(
            DashboardAlertType.Expired,
            AlertResourceType.Lot);

        var page = CreatePagedResult(alert);

        SetupValidValidation();

        SetupWarehouseAdmin();

        _repository
            .Setup(x => x.GetAlertsAsync(
                query,
                FixedNow,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _service.GetAlertsAsync(query);

        Assert.That(result.Success, Is.True);

        var actions = result.Data!.Items[0].AllowedActions;

        Assert.That(actions, Has.Count.EqualTo(2));
        Assert.That(actions[1].Code, Is.EqualTo("OpenDisposal"));
        Assert.That(actions[1].Label, Is.EqualTo("Review disposal"));
    }

    [Test]
    public async Task GetAlertsAsync_ShouldAddManageStatusAction_ForWarehouseAdminBlocked()
    {
        var query = CreateQuery();

        var alert = CreateAlert(
            DashboardAlertType.Blocked,
            AlertResourceType.Lot);

        var page = CreatePagedResult(alert);

        SetupValidValidation();

        SetupWarehouseAdmin();

        _repository
            .Setup(x => x.GetAlertsAsync(
                query,
                FixedNow,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _service.GetAlertsAsync(query);

        Assert.That(result.Success, Is.True);

        var actions = result.Data!.Items[0].AllowedActions;

        Assert.That(actions, Has.Count.EqualTo(2));
        Assert.That(actions[0].Code, Is.EqualTo("ViewLot"));
        Assert.That(actions[1].Code, Is.EqualTo("ManageLotStatus"));
        Assert.That(actions[1].Label, Is.EqualTo("Manage status"));
        Assert.That(
            actions[1].Href,
            Is.EqualTo(
                $"/app/lots/{alert.LotId}?action=status"));
    }

    [Test]
    public async Task GetAlertsAsync_ShouldReturnOnlyViewAction_ForWarehouseAdminUnknownAlertType()
    {
        var query = CreateQuery();

        var alert = CreateAlert(
            (DashboardAlertType)999,
            AlertResourceType.Lot);

        var page = CreatePagedResult(alert);

        SetupValidValidation();

        SetupWarehouseAdmin();

        _repository
            .Setup(x => x.GetAlertsAsync(
                query,
                FixedNow,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _service.GetAlertsAsync(query);

        Assert.That(result.Success, Is.True);

        var actions = result.Data!.Items[0].AllowedActions;

        Assert.That(actions, Has.Count.EqualTo(1));
        Assert.That(actions[0].Code, Is.EqualTo("ViewLot"));
    }

    [Test]
    public async Task GetAlertsAsync_ShouldReturnEmptyItems_WhenRepositoryReturnsEmptyPage()
    {
        var query = CreateQuery();

        var page = new PagedResult<DashboardAlertDto>
        {
            Page = 2,
            PageSize = 20,
            TotalCount = 0,
            Items = []
        };

        SetupValidValidation();

        _currentUser
            .Setup(x => x.IsInRole("WarehouseAdmin"))
            .Returns(true);

        _currentUser
            .Setup(x => x.IsInRole("Scientist"))
            .Returns(false);

        _repository
            .Setup(x => x.GetAlertsAsync(
                query,
                FixedNow,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await _service.GetAlertsAsync(query);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Page, Is.EqualTo(2));
        Assert.That(result.Data.PageSize, Is.EqualTo(20));
        Assert.That(result.Data.TotalCount, Is.EqualTo(0));
        Assert.That(result.Data.Items, Is.Empty);
    }

    private void SetupValidValidation()
    {
        _validator
            .Setup(x => x.ValidateAsync(
                It.IsAny<DashboardAlertQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private void SetupWarehouseAdmin()
    {
        _currentUser
            .Setup(x => x.IsInRole("WarehouseAdmin"))
            .Returns(true);

        _currentUser
            .Setup(x => x.IsInRole("Scientist"))
            .Returns(false);
    }

    private static DashboardAlertQuery CreateQuery()
    {
        return new DashboardAlertQuery
        {
            Page = 1,
            PageSize = 20
        };
    }

    private static DashboardAlertDto CreateAlert(
        DashboardAlertType alertType,
        AlertResourceType resourceType)
    {
        return new DashboardAlertDto
        {
            AlertType = alertType,
            ResourceType = resourceType,
            ItemId = Guid.NewGuid(),
            LotId = Guid.NewGuid()
        };
    }

    private static PagedResult<DashboardAlertDto> CreatePagedResult(
        DashboardAlertDto alert)
    {
        return new PagedResult<DashboardAlertDto>
        {
            Page = 1,
            PageSize = 20,
            TotalCount = 1,
            Items = [alert]
        };
    }
}