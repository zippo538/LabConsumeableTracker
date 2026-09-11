using Microsoft.AspNetCore.Identity;

namespace LabConsumableExpiryTracker.Domain.Models;

public class User : IdentityUser<Guid>
{
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    private User() { }

    public User(string username, string email)
    {
        UserName = username;
        Email = email;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}