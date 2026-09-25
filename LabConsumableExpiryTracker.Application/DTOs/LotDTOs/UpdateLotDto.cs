using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.LotDTOs;

public class UpdateLotDto
{
    public string? LotNumber { get; init; }
    public LotAdministrativeStatus? AdministrativeStatus { get; init; }
    public string? SupplierLotNumber { get; init; }
    public string? SupplierName { get; init; }
    public DateOnly? ExpiryDate { get; init; }
    public string? StorageLocation { get; init; }
    public byte[] RowVersion { get; init; } = [];
}