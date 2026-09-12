namespace LabConsumableExpiryTracker.Application.Configurations;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 15; // Berubah jadi menit (pendek)
    public int RefreshTokenExpiryDays { get; set; } = 7; 
}