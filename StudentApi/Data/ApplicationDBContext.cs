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
        var students = new List<Student>()
        {
            new Student(){
                StudentId = 1,
                FullName = "Joh Doe",
                Address = "123 Main St",
                Email = "john.doe@example.com",
                DateOfBirth = new DateOnly(2003, 2, 2),
                PhoneNumber = "555-123-4567",
                Grade = 2
            },
            new Student(){
                StudentId = 2,
                FullName = "Jane Smith",
                Address = "456 Elm St",
                Email = "jane.smith@example.com",
                DateOfBirth = new DateOnly(2004, 5, 9),
                PhoneNumber = "123-456-7890",
                Grade = 1
            },

        };
        modelBuilder.Entity<Student>().HasData(students);
    }
}
