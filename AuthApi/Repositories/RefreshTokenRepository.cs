using AuthApi.Data;
using AuthApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Repositories;
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetRefreshTokenAsync(string token);

    Task<bool> AddRefreshTokenAsync(RefreshToken refreshToken);

    Task<bool> RemoveRefreshTokenAsync(string token);
    Task<bool> RemoveAllRefreshTokensByUserIdAsync(int userId);
}
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;
    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<bool> RemoveRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        if (refreshToken != null)
        {
            _context.RefreshTokens.Remove(refreshToken);
            return await _context.SaveChangesAsync() > 0;
        }
        return false;
    }

    public async Task<bool> RemoveAllRefreshTokensByUserIdAsync(int userId)
    {
        _context.RefreshTokens.RemoveRange(_context.RefreshTokens.Where(rt => rt.UserId == userId));
        return await _context.SaveChangesAsync() > 0;
    }
}
