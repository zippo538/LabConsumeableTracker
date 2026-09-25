using Microsoft.AspNetCore.Identity;

namespace LabConsumableExpiryTracker.Domain.Models;

public sealed class User : IdentityUser<Guid>
{
    public bool IsActive { get; private set; } = true;
    
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