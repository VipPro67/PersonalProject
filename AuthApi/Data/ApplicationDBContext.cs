using AuthApi.Models;
using Microsoft.EntityFrameworkCore;
namespace AuthApi.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //create default user when database is created
        var user = new AppUser
        {
            UserId = 1,
            UserName = "admin",
            Email = "admin@localhost.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123123@Aa"),
            FullName = "Admin User",
            Address = "123 Main St",
            DateOfBirth = new DateOnly(1990, 1, 1)
        };
        modelBuilder.Entity<AppUser>().HasData(user);

    }
}
