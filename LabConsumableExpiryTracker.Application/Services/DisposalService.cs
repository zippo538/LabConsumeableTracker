using AutoMapper;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Application.Services;

public class DisposalService(
    ILotRepository lotRepository,
    IDisposalRepository disposalRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    TimeProvider timeProvider,
    ILogger<DisposalService> logger)
    : IDisposalService
{
    public async Task<ServiceResult<CreateDisposalResponseDto>> CreateDisposalAsync(
        Guid lotId,
        CreateDisposalDto request,
        CancellationToken ct = default)
    {
        logger.LogDebug("Creating disposal for lot {LotId}", lotId);

        if (request.Quantity <= 0)
            return ServiceResult<CreateDisposalResponseDto>.ErrorResult("Quantity harus lebih dari nol.");
            
        if (string.IsNullOrWhiteSpace(request.Reason))
            return ServiceResult<CreateDisposalResponseDto>.ErrorResult("Reason wajib diisi.");

        var lot = await lotRepository.GetByIdAsync(lotId, ct);
        if (lot is null)
            return ServiceResult<CreateDisposalResponseDto>.ErrorResult("Lot not found.");

        if (lot.AdministrativeStatus == LotAdministrativeStatus.Disposed)
            return ServiceResult<CreateDisposalResponseDto>.ErrorResult("Lot is already disposed.");

        if (request.Quantity > lot.RemainingQuantity)
            return ServiceResult<CreateDisposalResponseDto>.ErrorResult("Disposal quantity exceeds remaining quantity.");

        try
        {
             var expectedBytes = Convert.FromBase64String(request.ExpectedRowVersion);
            lot.SetConcurrencyToken(expectedBytes);

            var now = timeProvider.GetUtcNow();

            lot.UpdateRemainingQuantity(request.Quantity);
            
            if (lot.RemainingQuantity == 0)
            {
                lot.Dispose();
            }

            var disposal = Disposal.Create(lotId, request.Quantity, request.Reason, now,request.DisposedBy);
            
            await lotRepository.UpdateAsync(lot);
            await disposalRepository.AddAsync(disposal, ct);
            await unitOfWork.SaveChangesAsync(ct);

            logger.LogInformation("Disposal {DisposalId} created for lot {LotId}", disposal.Id, lotId);

            var response = new CreateDisposalResponseDto
            {
                Disposal = mapper.Map<DisposalDto>(disposal),
                Lot = mapper.Map<LotSnapshotDto>(lot) 
            };

            return ServiceResult<CreateDisposalResponseDto>.SuccessResult(response, "Lot disposed successfully.");
        }
        catch (FormatException)
        {
            return ServiceResult<CreateDisposalResponseDto>.ErrorResult("Invalid RowVersion format.");
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogWarning("Concurrency conflict during disposal for lot {LotId}", lotId);
            return ServiceResult<CreateDisposalResponseDto>.ErrorResult(
                "Concurrency conflict. The lot was modified by another user.",
                ["CONCURRENCY_CONFLICT"]
            );
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult<CreateDisposalResponseDto>.ErrorResult(ex.Message);
        }
    }
}