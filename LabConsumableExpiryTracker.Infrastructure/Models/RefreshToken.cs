namespace LabConsumableExpiryTracker.Infrastructure.Models;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresOnUtc { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedOnUtc { get; set; }
    
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresOnUtc;
    public bool IsActive => !IsRevoked && !IsExpired;
}
