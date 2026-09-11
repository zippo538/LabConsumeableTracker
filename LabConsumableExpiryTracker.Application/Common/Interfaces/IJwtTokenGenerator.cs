using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user, string role);
}