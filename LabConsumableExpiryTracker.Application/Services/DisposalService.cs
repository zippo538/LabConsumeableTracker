using AutoMapper;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Application.Services;

public class DisposalService : IDisposalService
{
    private readonly ILotRepository _lotRepository;
    private readonly IDisposalRepository _disposalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DisposalService> _logger;

    public DisposalService(
        ILotRepository lotRepository,
        IDisposalRepository disposalRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        TimeProvider timeProvider,
        ILogger<DisposalService> logger)
    {
        _lotRepository = lotRepository;
        _disposalRepository = disposalRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<ServiceResult<DisposalDto>> CreateDisposalAsync(
        CreateDisposalDto request,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Creating disposal for lot {LotId}", request.LotId);

        if (!Guid.TryParse(request.LotId, out var lotId))
        {
            return ServiceResult<DisposalDto>.ErrorResult(
                "Validation failed.",
                new List<string> { "LotId harus berupa GUID yang valid." });
        }

        if (request.Quantity <= 0)
            return ServiceResult<DisposalDto>.ErrorResult("Quantity harus lebih dari nol.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            return ServiceResult<DisposalDto>.ErrorResult("Reason wajib diisi.");

        var lot = await _lotRepository.GetByIdAsync(lotId, ct);
        if (lot is null)
        {
            _logger.LogWarning("Disposal rejected because lot {LotId} was not found", request.LotId);
            return ServiceResult<DisposalDto>.ErrorResult("Lot not found.");
        }

        if (lot.AdministrativeStatus != LotAdministrativeStatus.Active)
        {
            _logger.LogWarning(
                "Disposal rejected because lot {LotId} has status {AdministrativeStatus}",
                lot.Id,
                lot.AdministrativeStatus);
            return ServiceResult<DisposalDto>.ErrorResult("Only active lots are eligible for disposal.");
        }

        if (request.Quantity > lot.RemainingQuantity)
        {
            _logger.LogWarning(
                "Disposal rejected because quantity {Quantity} exceeds remaining stock {RemainingQuantity} for lot {LotId}",
                request.Quantity,
                lot.RemainingQuantity,
                lot.Id);
            return ServiceResult<DisposalDto>.ErrorResult("Disposal quantity exceeds remaining quantity.");
        }

        try
        {
            var now = _timeProvider.GetUtcNow();
            var disposal = Disposal.Create(lotId, request.Quantity, request.Reason, now);

            lot.UpdateRemainingQuantity(request.Quantity);
            await _lotRepository.UpdateAsync(lot, ct);
            lot.Dispose();
            var created = await _disposalRepository.AddAsync(disposal, ct);
            
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Disposal {DisposalId} created for lot {LotId} with quantity {Quantity}",
                created.Id,
                created.LotId,
                created.Quantity);

            return ServiceResult<DisposalDto>.SuccessResult(
                _mapper.Map<DisposalDto>(created),
                "Lot disposed successfully.");
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(exception, "Disposal request for lot {LotId} was invalid", request.LotId);
            return ServiceResult<DisposalDto>.ErrorResult(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(exception, "Disposal request for lot {LotId} was rejected", request.LotId);
            return ServiceResult<DisposalDto>.ErrorResult(exception.Message);
        }
    }
}
