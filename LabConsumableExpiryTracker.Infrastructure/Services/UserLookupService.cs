using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace LabConsumableExpiryTracker.Infrastructure.Services;

public class UserLookupService : IUserLookupService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserLookupService(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(
        IEnumerable<Guid> userIds, CancellationToken ct)
    {
        var ids = userIds.Distinct().ToList();

        var users = await _userManager.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(ct);

        return users.ToDictionary(u => u.Id, u => u.FullName);
    }
}