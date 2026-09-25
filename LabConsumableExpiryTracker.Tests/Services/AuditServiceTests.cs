using FluentValidation;
using FluentValidation.Results;
using LabConsumableExpiryTracker.Application.DTOs.AuditDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace LabConsumableExpiryTracker.Tests.Services;

[TestFixture]
public class AuditServiceTests
{
    private Mock<IAuditRepository> _auditRepository = null!;
    private Mock<IValidator<AuditQuery>> _validator = null!;
    private Mock<ILogger<AuditService>> _logger = null!;
    private AuditService _service = null!;

    [SetUp]
    public void Setup()
    {
        _auditRepository =
            new Mock<IAuditRepository>(MockBehavior.Strict);

        _validator =
            new Mock<IValidator<AuditQuery>>(MockBehavior.Strict);

        _logger =
            new Mock<ILogger<AuditService>>();

        _service = new AuditService(
            _auditRepository.Object,
            _validator.Object,
            _logger.Object);
    }

    [Test]
    public async Task GetAuditEventsAsync_ShouldReturnError_WhenValidationFails()
    {
        var query = CreateQuery();

        var validationResult = new ValidationResult(
        [
            new ValidationFailure(
                nameof(AuditQuery.Page),
                "Page must be greater than zero.")
        ]);

        _validator
            .Setup(x => x.ValidateAsync(
                query,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var result =
            await _service.GetAuditEventsAsync(query);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("Validation failed."));

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(
            result.Errors,
            Has.One.EqualTo("Page must be greater than zero."));

        _auditRepository.Verify(
            x => x.GetAuditEventsAsync(
                It.IsAny<AuditQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetAuditEventsAsync_ShouldReturnSuccess_WhenQueryIsValid()
    {
        var query = CreateQuery();

        var pagedResult = CreateAuditEventPagedResult();

        SetupValidValidation(query);

        _auditRepository
            .Setup(x => x.GetAuditEventsAsync(
                query,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result =
            await _service.GetAuditEventsAsync(query);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.SameAs(pagedResult));

        _auditRepository.Verify(
            x => x.GetAuditEventsAsync(
                query,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetAuditEventsAsync_ShouldNormalizeEventTypeToConsumption_WhenJobIdProvidedWithoutEventType()
    {
        var jobId =
            Guid.Parse("11111111-1111-1111-1111-111111111111");

        var itemId =
            Guid.Parse("22222222-2222-2222-2222-222222222222");

        var lotId =
            Guid.Parse("33333333-3333-3333-3333-333333333333");

        var query = new AuditQuery
        {
            EventType = null,
            ItemId = itemId,
            LotId = lotId,
            JobId = jobId,
            Search = "test",
            Page = 2,
            PageSize = 20
        };

        var pagedResult = CreateAuditEventPagedResult();

        SetupValidValidation(query);

        _auditRepository
            .Setup(x => x.GetAuditEventsAsync(
                It.Is<AuditQuery>(normalized =>
                    normalized.EventType == AuditEventType.Consumption &&
                    normalized.ItemId == itemId &&
                    normalized.LotId == lotId &&
                    normalized.JobId == jobId &&
                    normalized.Search == "test" &&
                    normalized.Page == 2 &&
                    normalized.PageSize == 20),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result =
            await _service.GetAuditEventsAsync(query);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.SameAs(pagedResult));

        _auditRepository.Verify(
            x => x.GetAuditEventsAsync(
                It.Is<AuditQuery>(normalized =>
                    normalized.EventType == AuditEventType.Consumption &&
                    normalized.ItemId == itemId &&
                    normalized.LotId == lotId &&
                    normalized.JobId == jobId &&
                    normalized.Search == "test" &&
                    normalized.Page == 2 &&
                    normalized.PageSize == 20),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetAuditEventsAsync_ShouldNotNormalize_WhenEventTypeAlreadyProvided()
    {
        var query = new AuditQuery
        {
            EventType = AuditEventType.Disposal,
            JobId = Guid.NewGuid(),
            Page = 1,
            PageSize = 20
        };

        var pagedResult = CreateAuditEventPagedResult();

        SetupValidValidation(query);

        _auditRepository
            .Setup(x => x.GetAuditEventsAsync(
                query,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result =
            await _service.GetAuditEventsAsync(query);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.SameAs(pagedResult));

        _auditRepository.Verify(
            x => x.GetAuditEventsAsync(
                query,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetConsumptionsAsync_ShouldReturnError_WhenValidationFails()
    {
        var query = CreateQuery();

        var validationResult = new ValidationResult(
        [
            new ValidationFailure(
                nameof(AuditQuery.PageSize),
                "Page size is invalid.")
        ]);

        _validator
            .Setup(x => x.ValidateAsync(
                It.Is<AuditQuery>(q =>
                    q.EventType == AuditEventType.Consumption),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var result =
            await _service.GetConsumptionsAsync(query);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("Validation failed."));

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(
            result.Errors,
            Has.One.EqualTo("Page size is invalid."));

        _auditRepository.Verify(
            x => x.GetConsumptionsAsync(
                It.IsAny<AuditQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetConsumptionsAsync_ShouldReturnSuccess_WhenQueryIsValid()
    {
        var query = CreateQuery();

        var pagedResult =
            CreateConsumptionPagedResult();

        _validator
            .Setup(x => x.ValidateAsync(
                It.Is<AuditQuery>(q =>
                    q.EventType == AuditEventType.Consumption &&
                    q.ItemId == query.ItemId &&
                    q.LotId == query.LotId &&
                    q.JobId == query.JobId &&
                    q.Search == query.Search &&
                    q.Page == query.Page &&
                    q.PageSize == query.PageSize),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _auditRepository
            .Setup(x => x.GetConsumptionsAsync(
                query,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result =
            await _service.GetConsumptionsAsync(query);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.SameAs(pagedResult));

        _auditRepository.Verify(
            x => x.GetConsumptionsAsync(
                query,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetConsumptionByIdAsync_ShouldReturnError_WhenNotFound()
    {
        var id =
            Guid.Parse("11111111-1111-1111-1111-111111111111");

        _auditRepository
            .Setup(x => x.GetConsumptionByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsumptionAuditDto?)null);

        var result =
            await _service.GetConsumptionByIdAsync(id);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("Consumption not found."));
        Assert.That(result.Data, Is.Null);
    }

    [Test]
    public async Task GetConsumptionByIdAsync_ShouldReturnSuccess_WhenFound()
    {
        var id =
            Guid.Parse("11111111-1111-1111-1111-111111111111");

        var dto = new ConsumptionAuditDto();

        _auditRepository
            .Setup(x => x.GetConsumptionByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result =
            await _service.GetConsumptionByIdAsync(id);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.SameAs(dto));
    }

    [Test]
    public async Task GetDisposalsAsync_ShouldReturnError_WhenValidationFails()
    {
        var query = CreateQuery();

        var validationResult = new ValidationResult(
        [
            new ValidationFailure(
                nameof(AuditQuery.ItemId),
                "Item is invalid.")
        ]);

        _validator
            .Setup(x => x.ValidateAsync(
                It.Is<AuditQuery>(q =>
                    q.EventType == AuditEventType.Disposal),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var result =
            await _service.GetDisposalsAsync(query);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("Validation failed."));

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(
            result.Errors,
            Has.One.EqualTo("Item is invalid."));

        _auditRepository.Verify(
            x => x.GetDisposalsAsync(
                It.IsAny<AuditQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetDisposalsAsync_ShouldReturnSuccess_WhenQueryIsValid()
    {
        var query = CreateQuery();

        var pagedResult =
            CreateDisposalPagedResult();

        _validator
            .Setup(x => x.ValidateAsync(
                It.Is<AuditQuery>(q =>
                    q.EventType == AuditEventType.Disposal &&
                    q.ItemId == query.ItemId &&
                    q.LotId == query.LotId &&
                    q.Search == query.Search &&
                    q.Page == query.Page &&
                    q.PageSize == query.PageSize),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _auditRepository
            .Setup(x => x.GetDisposalsAsync(
                query,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result =
            await _service.GetDisposalsAsync(query);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.SameAs(pagedResult));

        _auditRepository.Verify(
            x => x.GetDisposalsAsync(
                query,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetDisposalByIdAsync_ShouldReturnError_WhenNotFound()
    {
        var id =
            Guid.Parse("11111111-1111-1111-1111-111111111111");

        _auditRepository
            .Setup(x => x.GetDisposalByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((DisposalAuditDto?)null);

        var result =
            await _service.GetDisposalByIdAsync(id);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Message,
            Is.EqualTo("Disposal not found."));
        Assert.That(result.Data, Is.Null);
    }

    [Test]
    public async Task GetDisposalByIdAsync_ShouldReturnSuccess_WhenFound()
    {
        var id =
            Guid.Parse("11111111-1111-1111-1111-111111111111");

        var dto = new DisposalAuditDto();

        _auditRepository
            .Setup(x => x.GetDisposalByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result =
            await _service.GetDisposalByIdAsync(id);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.SameAs(dto));
    }

    private static AuditQuery CreateQuery()
    {
        return new AuditQuery
        {
            EventType = AuditEventType.Consumption,
            ItemId = Guid.NewGuid(),
            LotId = Guid.NewGuid(),
            JobId = Guid.NewGuid(),
            Search = "test",
            Page = 1,
            PageSize = 20
        };
    }

    private static PagedResult<AuditEventDto> CreateAuditEventPagedResult()
    {
        return new PagedResult<AuditEventDto>
        {
            Items = [],
            Page = 1,
            PageSize = 20,
            TotalCount = 0
        };
    }

    private static PagedResult<ConsumptionAuditDto> CreateConsumptionPagedResult()
    {
        return new PagedResult<ConsumptionAuditDto>
        {
            Items = [],
            Page = 1,
            PageSize = 20,
            TotalCount = 0
        };
    }

    private static PagedResult<DisposalAuditDto> CreateDisposalPagedResult()
    {
        return new PagedResult<DisposalAuditDto>
        {
            Items = [],
            Page = 1,
            PageSize = 20,
            TotalCount = 0
        };
    }

    private void SetupValidValidation(AuditQuery query)
    {
        _validator
            .Setup(x => x.ValidateAsync(
                query,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }
}

