using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace LabConsumableExpiryTracker.Infrastructure.Persistence.Seeders;

public class IdentitySeeder(
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<ApplicationUser> userManager)
{
    public async Task SeedAsync()
    {
        foreach (var role in Enum.GetValues<UserRole>())
        {
            var roleName = role.ToString();

            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(
                    new IdentityRole<Guid>(roleName));
            }
        }

        var admin = await userManager.FindByNameAsync("warehouseadmin");

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

            var result = await userManager.CreateAsync(
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

        if (!await userManager.IsInRoleAsync(
                admin,
                UserRole.WarehouseAdmin.ToString()))
        {
            await userManager.AddToRoleAsync(
                admin,
                UserRole.WarehouseAdmin.ToString());
        }
    }
}