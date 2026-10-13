using LabConsumableExpiryTracker.Infrastructure.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using LabConsumableExpiryTracker.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LabConsumableExpiryTracker.Tests.Repository;

[TestFixture]
public class RefreshTokenRepositoryTests
{
    private static string _connectionString = null!;
    private AppDbContext _context = null!;
    private RefreshTokenRepository _repository = null!;
    private readonly List<string> _seededTokens = [];
    private readonly List<Guid> _seededUserIds = [];

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _connectionString = Connection.GetConnection("UnitTestConnection");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString)
            .EnableSensitiveDataLogging()
            .Options;
        _context = new AppDbContext(options);
        _repository = new RefreshTokenRepository(_context);
        await _context.Database.CanConnectAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_seededTokens.Count > 0)
        {
            var tokens = await _context.RefreshTokens.Where(t => _seededTokens.Contains(t.Token)).ToListAsync();
            _context.RefreshTokens.RemoveRange(tokens);
            await _context.SaveChangesAsync();
            _seededTokens.Clear();
        }
        if (_seededUserIds.Count > 0)
        {
            var users = await _context.Users.Where(u => _seededUserIds.Contains(u.Id)).ToListAsync();
            _context.Users.RemoveRange(users);
            await _context.SaveChangesAsync();
            _seededUserIds.Clear();
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown() => await _context.DisposeAsync();

    private async Task<ApplicationUser> SeedUserAsync()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"user_{Guid.NewGuid():N}",
            Email = $"user_{Guid.NewGuid():N}@test.com",
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _seededUserIds.Add(user.Id);
        return user;
    }

    private async Task<RefreshToken> SeedTokenAsync(Guid userId, string tokenValue, bool isRevoked = false)
    {
        var token = new RefreshToken
        {
            Token = tokenValue,
            UserId = userId,
            IsRevoked = isRevoked,
            RevokedOnUtc = isRevoked ? DateTime.UtcNow : null
        };
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
        _seededTokens.Add(tokenValue);
        return token;
    }

    [Test]
    public async Task GetByTokenWithUserAsync_ReturnsTokenWithUser_WhenExists()
    {
        var user = await SeedUserAsync();
        var tokenVal = $"RT-{Guid.NewGuid():N}";
        await SeedTokenAsync(user.Id, tokenVal);
        var result = await _repository.GetByTokenWithUserAsync(tokenVal);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Token, Is.EqualTo(tokenVal));
        Assert.That(result.User, Is.Not.Null);
        Assert.That(result.User.Id, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task GetByTokenWithUserAsync_ReturnsNull_WhenNotExists()
    {
        var result = await _repository.GetByTokenWithUserAsync("NON-EXISTENT-TOKEN");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task RevokeAllByUserIdAsync_RevokesActiveTokensForUser()
    {
        var user = await SeedUserAsync();
        var t1 = await SeedTokenAsync(user.Id, $"RT-ACTIVE-1-{Guid.NewGuid():N}");
        var t2 = await SeedTokenAsync(user.Id, $"RT-ACTIVE-2-{Guid.NewGuid():N}");
        await _repository.RevokeAllByUserIdAsync(user.Id);
        var fetched1 = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == t1.Token);
        var fetched2 = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == t2.Token);
        Assert.That(fetched1, Is.Not.Null);
        Assert.That(fetched1!.IsRevoked, Is.True);
        Assert.That(fetched1.RevokedOnUtc, Is.Not.Null);
        Assert.That(fetched2, Is.Not.Null);
        Assert.That(fetched2!.IsRevoked, Is.True);
    }

    [Test]
    public async Task RevokeAllByUserIdAsync_DoesNotAffectOtherUsersTokens()
    {
        var user1 = await SeedUserAsync();
        var user2 = await SeedUserAsync();
        var t1 = await SeedTokenAsync(user1.Id, $"RT-U1-{Guid.NewGuid():N}");
        var t2 = await SeedTokenAsync(user2.Id, $"RT-U2-{Guid.NewGuid():N}");
        await _repository.RevokeAllByUserIdAsync(user1.Id);
        var fetched1 = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == t1.Token);
        var fetched2 = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == t2.Token);
        Assert.That(fetched1, Is.Not.Null);
        Assert.That(fetched1!.IsRevoked, Is.True);
        Assert.That(fetched2, Is.Not.Null);
        Assert.That(fetched2!.IsRevoked, Is.False);
    }

    [Test]
    public async Task AddAsync_ShouldAddRefreshToken()
    {
        var user = await SeedUserAsync();
        var tokenValue = $"test-token-{Guid.NewGuid():N}";

        var refreshToken = new RefreshToken
        {
            Token = tokenValue,
            UserId = user.Id,
            IsRevoked = false,
            RevokedOnUtc = null
        };

        await _repository.AddAsync(refreshToken);
        _seededTokens.Add(tokenValue);

        var result = await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Token == tokenValue);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Token, Is.EqualTo(tokenValue));
        Assert.That(result.UserId, Is.EqualTo(user.Id));
        Assert.That(result.IsRevoked, Is.False);
        Assert.That(result.RevokedOnUtc, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateRefreshToken()
    {
        var user = await SeedUserAsync();
        var tokenValue = $"test-token-{Guid.NewGuid():N}";

        var refreshToken = await SeedTokenAsync(
            user.Id,
            tokenValue
        );

        refreshToken.IsRevoked = true;
        refreshToken.RevokedOnUtc = DateTime.UtcNow;

        await _repository.UpdateAsync(refreshToken);

        var result = await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Token == tokenValue);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Token, Is.EqualTo(tokenValue));
        Assert.That(result.UserId, Is.EqualTo(user.Id));
        Assert.That(result.IsRevoked, Is.True);
        Assert.That(result.RevokedOnUtc, Is.Not.Null);
    }
}