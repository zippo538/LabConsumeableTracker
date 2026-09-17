using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace LabConsumableExpiryTracker.Application.DTOs.ItemDTOs
{
    public class CreateItemDto
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
        public string BaseUnit { get; set; } = string.Empty;

        public decimal MinimumStock { get; set; }

        public int ExpiringSoonDays { get; set; }

    }
}