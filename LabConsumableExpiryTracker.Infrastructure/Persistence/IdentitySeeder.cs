using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace LabConsumableExpiryTracker.Infrastructure.Persistence;

public class IdentitySeeder
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentitySeeder(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        Console.WriteLine("=== IDENTITY SEEDER RUNNING ===");
        foreach (var role in Enum.GetValues<UserRole>())
        {
            var roleName = role.ToString();

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(
                    new IdentityRole<Guid>(roleName));
            }
        }

        var admin = await _userManager.FindByNameAsync("warehouseadmin");

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = "warehouseadmin",
                Email = "admin@lab.com",
                FullName = "Warehouse Admin",
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(
                admin,
                "Admin@123");

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description)));
            }
        }

        if (!await _userManager.IsInRoleAsync(
                admin,
                UserRole.WarehouseAdmin.ToString()))
        {
            await _userManager.AddToRoleAsync(
                admin,
                UserRole.WarehouseAdmin.ToString());
        }
    }
}