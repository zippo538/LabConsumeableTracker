namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string userName, string email, string role);
    string GenerateRefreshToken();
}