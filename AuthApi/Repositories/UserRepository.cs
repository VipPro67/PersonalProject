using AuthApi.Data;
using AuthApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Repositories
{
    public interface IUserRepository
    {

        Task<AppUser?> GetAppUserByIdAsync(int id);

        Task<AppUser?> GetAppUserByEmailAsync(string email);
        Task<AppUser?> GetAppUserByUserNameAsync(string userName);
        Task<bool> IsUserExistAsync(string? email, string? userName);
        Task<AppUser?> CreateAppUserAsync(AppUser appUser);
        Task<AppUser?> UpdateAppUserAsync(AppUser appUser);
        Task DeleteAppUserAsync(AppUser appUser);
    }
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AppUser?> GetAppUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<AppUser?> GetAppUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<AppUser?> GetAppUserByUserNameAsync(string userName)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<bool> IsUserExistAsync(string? email, string? userName)
        {
            return await _context.Users.AnyAsync(u => u.Email.Equals(email,StringComparison.OrdinalIgnoreCase)  || u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)  );
        }
        public async Task<AppUser?> CreateAppUserAsync(AppUser appUser)
        {
            await _context.Users.AddAsync(appUser);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return appUser;
            }
            return null;
        }
        public async Task<AppUser?> UpdateAppUserAsync(AppUser appUser)
        {
            _context.Users.Update(appUser);
            await _context.SaveChangesAsync();
            return appUser;
        }
        public async Task DeleteAppUserAsync(AppUser appUser)
        {
            _context.Users.Remove(appUser);
            await _context.SaveChangesAsync();
        }
    }
}