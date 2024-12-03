using StudentApi.Models;
using Microsoft.EntityFrameworkCore;
namespace StudentApi.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Student>().HasKey(s => s.StudentId);

        modelBuilder.Entity<Student>().HasIndex(s => s.Email).IsUnique();

        modelBuilder.Entity<Student>().HasIndex(s => s.PhoneNumber).IsUnique();

        modelBuilder.Entity<Student>(st =>
        {
            st.Property(s => s.Email).IsRequired().HasColumnType("varchar(100)");
            st.Property(s => s.PhoneNumber).IsRequired().HasColumnType("varchar(20)");
            st.Property(s => s.FullName).IsRequired().HasColumnType("varchar(100)");
            st.Property(s => s.Address).HasColumnType("varchar(100)");
            st.Property(s => s.Grade).IsRequired();
            st.Property(s => s.DateOfBirth).IsRequired().HasColumnType("date");
        });

        var students = new List<Student>()
        {
            new Student(){
                StudentId = 1,
                FullName = "Joh Doe",
                Address = "123 Main St",
                Email = "john.doe@example.com",
                DateOfBirth = new DateOnly(2003, 2, 2),
                PhoneNumber = "5551234567",
                Grade = 2
            },
            new Student(){
                StudentId = 2,
                FullName = "Jane Smith",
                Address = "456 Elm St",
                Email = "jane.smith@example.com",
                DateOfBirth = new DateOnly(2004, 5, 9),
                PhoneNumber = "1234567890",
                Grade = 1
            },
        };
        modelBuilder.Entity<Student>().HasData(students);
    }
}
