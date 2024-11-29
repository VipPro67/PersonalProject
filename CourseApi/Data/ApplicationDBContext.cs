using CourseApi.Models;
using Microsoft.EntityFrameworkCore;
namespace CourseApi.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //create default list course when database is created
        var courses = new List<Course>()
        {
            new Course
            {
                CourseId = "C001",
                CourseName = "Introduction to C#",
                Description = "This course introduces you to the world of C# programming language and its fundamentals.",
                Credit = 3,
                Instructor = "John Doe",
                Department = "Computer Science",
                StartDate = new DateOnly(2024, 09, 15),
                EndDate = new DateOnly(2025, 02, 22),
                Schedule = "9:00 AM - 12:00 PM Mon, 2:00 PM - 5:00 PM Sat",
            },
            new Course
            {
                CourseId = "OOP",
                CourseName = "Object-Oriented Programming",
                Description = "This course teaches you the fundamentals of object-oriented programming in C#.",
                Credit = 4,
                Instructor = "Jane Smith",
                Department = "Computer Science",
                StartDate = new DateOnly(2024, 02, 10),
                EndDate = new DateOnly(2024, 05, 18),
                Schedule = "9:00 AM - 12:00 PM Tue, 2:00 PM - 5:00 PM Wen"
            },
            new Course
            {
                CourseId = "IT007",
                CourseName = "Introduction to IT Security",
                Description = "This course covers the basics of IT security and how to protect your digital assets.",
                Credit = 2,
                Instructor = "Bob Johnson",
                Department = "Computer Science",
                StartDate = new DateOnly(2024, 09, 15),
                EndDate = new DateOnly(2025, 02, 22),
                Schedule = " 2:00 PM - 5:00 PM Thu"
            }

        };
        modelBuilder.Entity<Course>().HasData(courses);
    }
}
