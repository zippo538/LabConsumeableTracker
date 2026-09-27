using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;

public class UpdateItemDto
{
    public string Name { get; init; } = string.Empty;
    public UnitOfMeasure BaseUnit { get; init; }
    public decimal MinimumStock { get; init; }
    public int ExpiringSoonDays { get; init; }

    public IFormFile? Image { get; init; }
}