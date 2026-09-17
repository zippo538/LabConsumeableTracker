using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Infrastructure.Persistence.Seeders
{
    public class UserSeeder
    {
        public const string ScientistRoleName = nameof(UserRole.Scientist);
        public const string WarehouseAdminRoleName = nameof(UserRole.Scientist);

        private const int DefaultSeed = 20260908;
        private const string EmailDomain = "lab-consumable.local";
        private const string PasswordConfigurationKey = "Seed:Users:Password";
        private const int ScientistCount = 5;
        private const int WarehouseAdminCount = 5;

        private readonly IConfiguration _configuration;
        private readonly ILogger<UserSeeder> _logger;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserSeeder(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IConfiguration configuration,
            ILogger<UserSeeder> logger)
        {
            ArgumentNullException.ThrowIfNull(userManager);
            ArgumentNullException.ThrowIfNull(roleManager);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(logger);

            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await EnsureRolesAsync();

            var accounts = GenerateAccounts();
            var password = _configuration[PasswordConfigurationKey];

            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning(
                    "User seed dilewati; '{Key}' belum dikonfigurasi. " +
                    "NFR-08 melarang password seed hardcoded di source.",
                    PasswordConfigurationKey);

                return;
            }

            var createdCount = 0;
            var failedUserNames = new List<string>();

            foreach (var account in accounts)
            {
                var existing = await _userManager
                    .FindByNameAsync(account.UserName);

                if (existing is not null)
                {
                    continue;
                }

                var user = new ApplicationUser
                {
                    Id = account.Id,
                    UserName = account.UserName,
                    Email = account.Email,
                    EmailConfirmed = true,
                    FullName = account.FullName,
                    IsActive = true
                };

                var creation = await _userManager.CreateAsync(user, password);

                if (!creation.Succeeded)
                {
                    failedUserNames.Add(account.UserName);

                    _logger.LogError(
                        "Failed Create User Seeder '{UserName}': {Errors}",
                        account.UserName,
                        Describe(creation));

                    continue;
                }

                var assignment = await _userManager
                    .AddToRoleAsync(user, account.RoleName);

                if (!assignment.Succeeded)
                {
                    // FR-12-03: kegagalan assignment role tidak boleh
                    // meninggalkan akun parsial tanpa membership.
                    failedUserNames.Add(account.UserName);

                    _logger.LogError(
                        "Gagal menetapkan role '{Role}' pada '{UserName}': {Errors}",
                        account.RoleName,
                        account.UserName,
                        Describe(assignment));

                    await _userManager.DeleteAsync(user);

                    continue;
                }

                createdCount++;
            }

            _logger.LogInformation(
                "Complete User Seed; {Created} total user, {Skipped} already exist.",
                createdCount,
                accounts.Count - createdCount - failedUserNames.Count);

            if (failedUserNames.Count > 0)
            {
                throw new InvalidOperationException(
                    $"User seeding failed: {string.Join(", ", failedUserNames)}.");
            }
        }

        private async Task EnsureRolesAsync()
        {
            string[] roleNames =
            [
                ScientistRoleName,
                WarehouseAdminRoleName
            ];

            foreach (var roleName in roleNames)
            {
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    continue;
                }

                var creation = await _roleManager
                    .CreateAsync(new IdentityRole<Guid>(roleName));

                if (!creation.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Gagal membuat role '{roleName}': {Describe(creation)}.");
                }
            }
        }

        private static IReadOnlyList<SeedAccount> GenerateAccounts()
        {
            var faker = new Faker("en");
            faker.Random = new Randomizer(DefaultSeed);

            var usedUserNames = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            var accounts = new List<SeedAccount>(
                ScientistCount + WarehouseAdminCount);

            AddAccounts(
                accounts,
                usedUserNames,
                faker,
                ScientistRoleName,
                ScientistCount);

            AddAccounts(
                accounts,
                usedUserNames,
                faker,
                WarehouseAdminRoleName,
                WarehouseAdminCount);

            return accounts;
        }

        private static void AddAccounts(
            ICollection<SeedAccount> accounts,
            ISet<string> usedUserNames,
            Faker faker,
            string roleName,
            int count)
        {
            for (var index = 1; index <= count; index++)
            {
                var person = faker.Person;

                var userName = ResolveUniqueUserName(
                    $"{NormalizeToken(person.FirstName)}.{NormalizeToken(person.LastName)}",
                    usedUserNames);

                accounts.Add(new SeedAccount(
                    faker.Random.Guid(),
                    userName,
                    $"{userName}@{EmailDomain}",
                    $"{person.FirstName} {person.LastName}",
                    roleName));
            }
        }

        private static string ResolveUniqueUserName(
            string baseName,
            ISet<string> usedUserNames)
        {
            var candidate = baseName;
            var suffix = 2;

            while (!usedUserNames.Add(candidate))
            {
                candidate = $"{baseName}{suffix}";
                suffix++;
            }

            return candidate;
        }

        private static string NormalizeToken(string value)
        {
            var builder = new StringBuilder(value.Length);

            foreach (var character in value.ToLowerInvariant())
            {
                if (char.IsAsciiLetter(character))
                {
                    builder.Append(character);
                }
            }

            return builder.Length == 0 ? "user" : builder.ToString();
        }

        private static string Describe(IdentityResult result) =>
            string.Join("; ", result.Errors.Select(error => error.Description));

        private sealed record SeedAccount(
            Guid Id,
            string UserName,
            string Email,
            string FullName,
            string RoleName);
    }
}
