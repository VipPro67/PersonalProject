using AuthApi.Repositories;
using AuthApi.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AuthApiTest.Repositories;
public class RefreshTokenRepositoryTest
{
    private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private async Task<ApplicationDbContext> CreateContextAndSeedDatabase()
    {
        var options = CreateNewContextOptions();

        var context = new ApplicationDbContext(options);
        var listRefreshToken = new List<RefreshToken> {
            new RefreshToken { UserId = 1, Token = "test1", ExpiresAt = DateTime.Now.AddMinutes(10) },
            new RefreshToken { UserId = 2, Token = "test2", ExpiresAt = DateTime.Now.AddMinutes(10) },
            new RefreshToken { UserId = 3, Token = "test3", ExpiresAt = DateTime.Now.AddMinutes(10) }
            };
        await context.RefreshTokens.AddRangeAsync(listRefreshToken);
        await context.SaveChangesAsync();
        return context;
    }

    private RefreshTokenRepository CreateRefreshTokenRepositoryWithSeededData(ApplicationDbContext context)
    {
        return new RefreshTokenRepository(context);
    }

    [Fact]
    public async Task AddRefreshTokenAsync_ShouldAddRefreshToken()
    {
        var context = await CreateContextAndSeedDatabase();
        var repository = CreateRefreshTokenRepositoryWithSeededData(context);
        var refreshToken = new RefreshToken { UserId = 4, Token = "test4", ExpiresAt = DateTime.Now.AddMinutes(10) };

        var result = await repository.AddRefreshTokenAsync(refreshToken);

        result.Should().BeTrue();
        context.RefreshTokens.Count().Should().Be(4);
    }

    [Fact]
    public async Task GetRefreshTokenAsync_ShouldReturnRefreshToken()
    {
        var context = await CreateContextAndSeedDatabase();
        var repository = CreateRefreshTokenRepositoryWithSeededData(context);

        var refreshToken = await repository.GetRefreshTokenAsync("test2");

        refreshToken.Should().NotBeNull();
        refreshToken.UserId.Should().Be(2);
        refreshToken.Token.Should().Be("test2");

    }

    [Fact]
    public async Task RemoveRefreshTokenAsync_ValidToken_True()
    {
        var context = await CreateContextAndSeedDatabase();
        var repository = CreateRefreshTokenRepositoryWithSeededData(context);
        var result = await repository.RemoveRefreshTokenAsync("test1");

        result.Should().BeTrue();
    }

    

    [Fact]
    public async Task RemoveAllRefreshTokensByUserIdAsync_ValidUserId_True()
    {
        var context = await CreateContextAndSeedDatabase();
        var repository = CreateRefreshTokenRepositoryWithSeededData(context);
        var result = await repository.RemoveAllRefreshTokensByUserIdAsync(1);
        result.Should().BeTrue();
    }

}

