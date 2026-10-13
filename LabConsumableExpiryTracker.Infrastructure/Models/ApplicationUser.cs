using Microsoft.AspNetCore.Identity;

namespace LabConsumableExpiryTracker.Infrastructure.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
