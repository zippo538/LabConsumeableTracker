using Microsoft.AspNetCore.Http;

namespace LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;

public class CreateItemDto
{
    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
    public IFormFile? Image { get; set; }
    public string BaseUnit { get; init; } = string.Empty;

    public decimal MinimumStock { get; init; }

    public int ExpiringSoonDays { get; init; }

}