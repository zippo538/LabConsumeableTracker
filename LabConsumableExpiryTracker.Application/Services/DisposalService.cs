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

    public async Task<ServiceResult<CreateDisposalResponseDto>> CreateDisposalAsync(
        Guid lotId,
        CreateDisposalDto request,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Creating disposal for lot {LotId}", lotId);

        if (request.Quantity <= 0)
            return ServiceResult<CreateDisposalResponseDto>.ErrorResult("Quantity harus lebih dari nol.");
            
        if (string.IsNullOrWhiteSpace(request.Reason))
            return ServiceResult<CreateDisposalResponseDto>.ErrorResult("Reason wajib diisi.");

        var lot = await _lotRepository.GetByIdAsync(lotId, ct);
        if (lot is null)
            return ServiceResult<CreateDisposalResponseDto>.ErrorResult("Lot not found.");

        if (lot.AdministrativeStatus == LotAdministrativeStatus.Disposed)
            return ServiceResult<CreateDisposalResponseDto>.ErrorResult("Lot is already disposed.");

        if (request.Quantity > lot.RemainingQuantity)
            return ServiceResult<CreateDisposalResponseDto>.ErrorResult("Disposal quantity exceeds remaining quantity.");

        try
        {
            // 1. CRITICAL: Set RowVersion dari client agar EF Core bisa melakukan concurrency check
            // EF Core akan menggunakan nilai ini sebagai "OriginalValue" untuk klausa WHERE
             var expectedBytes = Convert.FromBase64String(request.ExpectedRowVersion);
            lot.SetConcurrencyToken(expectedBytes);

            var now = _timeProvider.GetUtcNow();

            // 2. Kurangi stok menggunakan method domain
            lot.UpdateRemainingQuantity(request.Quantity);
            
            // 3. Jika stok habis, ubah status menjadi Disposed (sesuai Lot.cs)
            if (lot.RemainingQuantity == 0)
            {
                lot.Dispose();
            }

            // 4. Buat entitas Disposal
            var disposal = Disposal.Create(lotId, request.Quantity, request.Reason, now);
            
            // 5. Simpan perubahan (AppDbContext akan auto-generate RowVersion baru di sini)
            await _lotRepository.UpdateAsync(lot, ct);
            await _disposalRepository.AddAsync(disposal, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Disposal {DisposalId} created for lot {LotId}", disposal.Id, lotId);

            // 6. Map response menggunakan AutoMapper
            var response = new CreateDisposalResponseDto
            {
                Disposal = _mapper.Map<DisposalDto>(disposal),
                Lot = _mapper.Map<LotSnapshotDto>(lot) // Lot ini sekarang punya RowVersion baru dari DB
            };

            return ServiceResult<CreateDisposalResponseDto>.SuccessResult(response, "Lot disposed successfully.");
        }
        catch (FormatException)
        {
            return ServiceResult<CreateDisposalResponseDto>.ErrorResult("Invalid RowVersion format.");
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Concurrency conflict during disposal for lot {LotId}", lotId);
            return ServiceResult<CreateDisposalResponseDto>.ErrorResult(
                "Concurrency conflict. The lot was modified by another user.", 
                new List<string> { "CONCURRENCY_CONFLICT" }
            );
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult<CreateDisposalResponseDto>.ErrorResult(ex.Message);
        }
    }
}