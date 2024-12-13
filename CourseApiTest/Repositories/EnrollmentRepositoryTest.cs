using CourseApi.Data;
using CourseApi.Models;
using CourseApi.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CourseApiTest.Repositories;
public class EnrollmentRepositoryTest
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

        var listEnrollments = new List<Enrollment>
            {
                new Enrollment { EnrollmentId = 1, CourseId = "C001", StudentId = 1, Course =   new Course
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
                } },
                new Enrollment { EnrollmentId = 2, CourseId = "OOP", StudentId = 1, Course = new Course
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
                }},
                new Enrollment { EnrollmentId = 3, CourseId = "IT007", StudentId = 2, Course = new Course
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
                }}
            };

        context.Enrollments.AddRange(listEnrollments);
        await context.SaveChangesAsync();
        return context;
    }

    private async Task<EnrollmentRepository> CreateEnrollmentRepository(ApplicationDbContext context)
    {
        return new EnrollmentRepository(context);
    }

    [Fact]
    public async Task CreateEnrollmentAsync_ValidEnrollment_EnrollmentCreated()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateEnrollmentRepository(context);
        var newEnrollment = new Enrollment { StudentId = 3, CourseId = "CS102" };

        // Act
        var result = await repository.CreateEnrollmentAsync(newEnrollment);

        // Assert
        result.Should().NotBeNull();
        result.EnrollmentId.Should().BeGreaterThan(0); 
        context.Enrollments.Count().Should().Be(4); 
    }

    [Fact]
    public async Task DeleteEnrollmentAsync_ValidEnrollment_EnrollmentDeleted()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateEnrollmentRepository(context);
        var enrollmentToDelete = await context.Enrollments.FirstOrDefaultAsync(e => e.EnrollmentId == 1);

        // Act
        var result = await repository.DeleteEnrollmentAsync(enrollmentToDelete);

        // Assert
        result.Should().BeTrue();
        context.Enrollments.Count().Should().Be(2); 
    }

    [Fact]
    public async Task GetAllEnrollmentsAsync_EnrollmentsExist_EnrollmentsReturned()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateEnrollmentRepository(context);

        // Act
        var result = await repository.GetAllEnrollmentsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(3);
    }

    [Fact]
    public async Task GetEnrollmentsByCourseIdAsync_ValidCourseId_EnrollmentsReturned()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateEnrollmentRepository(context);
        var courseId = "C001";

        // Act
        var result = await repository.GetEnrollmentsByCourseIdAsync(courseId);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        result[0].CourseId.Should().Be(courseId);
    }

    [Fact]
    public async Task GetEnrollmentsByCourseIdAsync_InvalidCourseId_EmptyList()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateEnrollmentRepository(context);
        var courseId = "UNKNOWN";

        // Act
        var result = await repository.GetEnrollmentsByCourseIdAsync(courseId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEnrollmentsByStudentIdAsync_ValidStudentId_EnrollmentsReturned()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateEnrollmentRepository(context);
        var studentId = 1;

        // Act
        var result = await repository.GetEnrollmentsByStudentIdAsync(studentId);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetEnrollmentByIdAsync_ValidEnrollmentId_EnrollmentReturned()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateEnrollmentRepository(context);
        var enrollmentId = 1;

        // Act
        var result = await repository.GetEnrollmentByIdAsync(enrollmentId);

        // Assert
        result.Should().NotBeNull();
        result.EnrollmentId.Should().Be(enrollmentId);
        result.StudentId.Should().Be(1); 
    }

    [Fact]
    public async Task GetEnrollmentByIdAsync_InvalidEnrollmentId_NullReturned()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateEnrollmentRepository(context);
        var enrollmentId = 999; 
        // Act
        var result = await repository.GetEnrollmentByIdAsync(enrollmentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IsStudentEnrolledInCourseAsync_StudentIsEnrolled_ReturnTrue()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateEnrollmentRepository(context);
        var studentId = 1;
        var courseId = "C001";

        // Act
        var result = await repository.IsStudentEnrolledInCourseAsync(studentId, courseId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsStudentEnrolledInCourseAsync_StudentIsNotEnrolled_ReturnFalse()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateEnrollmentRepository(context);
        var studentId = 2;
        var courseId = "CS101";

        // Act
        var result = await repository.IsStudentEnrolledInCourseAsync(studentId, courseId);

        // Assert
        result.Should().BeFalse();
    }
}

