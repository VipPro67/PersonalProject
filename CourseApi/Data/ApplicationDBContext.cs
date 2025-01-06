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
    public DbSet<Enrollment> Enrollments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Course>(cs =>
        {
            cs.HasKey(c => c.CourseId);
            cs.Property(c => c.CourseId).IsRequired().HasMaxLength(10);
            cs.Property(c => c.CourseName).IsRequired().HasColumnType("varchar(100)");
            cs.Property(c => c.Description).IsRequired().HasColumnType("varchar(500)");
            cs.Property(c => c.Credit).IsRequired();
            cs.Property(c => c.Instructor).IsRequired().HasColumnType("varchar(100)");
            cs.Property(c => c.Department).IsRequired().HasColumnType("varchar(100)");
            cs.Property(c => c.StartDate).IsRequired().HasColumnType("date");
            cs.Property(c => c.EndDate).IsRequired().HasColumnType("date");
            cs.Property(c => c.Schedule).IsRequired().HasMaxLength(100);
            cs.HasMany(c => c.Enrollments).WithOne(e => e.Course).HasForeignKey(e => e.CourseId).IsRequired();
        });

        modelBuilder.Entity<Enrollment>(e =>
        {
            e.HasKey(e => e.EnrollmentId);
            e.Property(e => e.StudentId).IsRequired();
            e.HasOne(e => e.Course).WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId).HasConstraintName("FK_Enrollment_Course");
            e.Ignore(e => e.Student);
            e.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
        });

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
                    Schedule = "9:00 AM - 12:00 PM Mon, 2:00 PM - 5:00 PM Sat"
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
                    Schedule = "2:00 PM - 5:00 PM Thu"
                }
            };
        modelBuilder.Entity<Course>().HasData(courses);
        var enrollments = new List<Enrollment>()
            {
                new Enrollment { EnrollmentId = 1, CourseId = "C001", StudentId = 1 },
                new Enrollment { EnrollmentId = 2, CourseId = "OOP", StudentId = 1 },
                new Enrollment { EnrollmentId = 3, CourseId = "IT007", StudentId = 1 },
                new Enrollment { EnrollmentId = 4, CourseId = "C001", StudentId = 2 },
                new Enrollment { EnrollmentId = 5, CourseId = "OOP", StudentId = 2 },
                new Enrollment { EnrollmentId = 6, CourseId = "IT007", StudentId = 2 }
            };
        modelBuilder.Entity<Enrollment>().HasData(enrollments);
    }
}

