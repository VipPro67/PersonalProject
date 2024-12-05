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
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AppUser>(
        au =>
        {
            au.HasKey(e => e.UserId);
            au.HasIndex(u => u.Email).IsUnique();
            au.HasIndex(u => u.UserName).IsUnique();
            au.Property(u => u.Email).IsRequired().HasColumnType("varchar(100)");
            au.Property(u => u.UserName).IsRequired().HasColumnType("varchar(50)");
            au.Property(u => u.PasswordHash).IsRequired().HasColumnType("text");
            au.Property(u => u.FullName).IsRequired().HasColumnType("varchar(100)");
            au.Property(u => u.Address).HasColumnType("varchar(255)");
            au.Property(u => u.DateOfBirth).HasColumnType("date");
        });

        modelBuilder.Entity<RefreshToken>(
        rt =>
        {
            rt.HasKey(rt => rt.Token);
            rt.Property(rt => rt.UserId).IsRequired();
            rt.Property(rt => rt.Token).IsRequired().HasColumnType("text");
            rt.Property(rt => rt.ExpiresAt).IsRequired();
            rt.HasOne(rt => rt.User)
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(rt => rt.UserId);
        });
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
