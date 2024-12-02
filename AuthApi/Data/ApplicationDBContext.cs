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
        modelBuilder.Entity<AppUser>().
            HasKey(e => e.UserId);
        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique();
        modelBuilder.Entity<AppUser>()
           .HasIndex(u => u.UserName)
           .IsUnique();
        modelBuilder.Entity<AppUser>(
            au =>
            {
                au.Property(u => u.Email).IsRequired().HasColumnType("varchar(100)");
                au.Property(u => u.UserName).IsRequired().HasColumnType("varchar(50)");
                au.Property(u => u.PasswordHash).IsRequired().HasColumnType("text");
                au.Property(u => u.FullName).IsRequired().HasColumnType("varchar(100)");
                au.Property(u => u.Address).HasColumnType("varchar(255)");
                au.Property(u => u.DateOfBirth).HasColumnType("date");
            }
        );
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
