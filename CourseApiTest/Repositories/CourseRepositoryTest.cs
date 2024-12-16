using CourseApi.Data;
using CourseApi.Helpers;
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
public class CourseRepositoryTest
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

        var listCourses = new List<Course>
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

        context.Courses.AddRange(listCourses);
        await context.SaveChangesAsync();
        return context;
    }

    private CourseRepository CreateCourseRepository(ApplicationDbContext context)
    {
        return new CourseRepository(context);
    }

    [Fact]
    public async Task CreateCourseAsync_ValidCourse_CourseCreated()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = CreateCourseRepository(context);
        var newCourse = new Course
        {
            CourseId = "CS102",
            CourseName = "Data Structures",
            Instructor = "Dr. Brown",
            Department = "Computer Science",
            Credit = 3,
            Schedule = "TTh 2:00-3:30",
            Description = "This course covers the basics of data structures in C#.",
            StartDate = new DateOnly(2024, 9, 29),
            EndDate = new DateOnly(2025, 2, 15)
        };

        // Act
        var result = await repository.CreateCourseAsync(newCourse);

        // Assert
        result.Should().NotBeNull();
        result.CourseId.Should().Be("CS102");
        context.Courses.Count().Should().Be(4);
    }

    [Fact]
    public async Task DeleteCourseAsync_ValidCourse_CourseDeleted()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = CreateCourseRepository(context);
        var courseToDelete = await context.Courses.FirstOrDefaultAsync(c => c.CourseId == "C001");

        // Act
        var result = await repository.DeleteCourseAsync(courseToDelete);

        // Assert
        result.Should().BeTrue();
        context.Courses.Count().Should().Be(2);
    }

    [Fact]
    public async Task EditCourseAsync_ValidCourse_CourseUpdated()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = CreateCourseRepository(context);
        var courseToEdit = await context.Courses.FirstOrDefaultAsync(c => c.CourseId == "C001");
        courseToEdit.CourseName = "Intro to CS - Updated";

        // Act
        var result = await repository.EditCourseAsync(courseToEdit);

        // Assert
        result.Should().NotBeNull();
        result.CourseName.Should().Be("Intro to CS - Updated");
    }

    [Fact]
    public async Task GetAllCoursesAsync_QueryMatches_CoursesReturned()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = CreateCourseRepository(context);
        var query = new CourseQuery { Instructor = "Jane Smith" };

        // Act
        var result = await repository.GetAllCoursesAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        result[0].CourseId.Should().Be("OOP");
    }

    [Fact]
    public async Task GetAllCoursesAsync_QueryNoMatches_EmptyList()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = CreateCourseRepository(context);
        var query = new CourseQuery { Instructor = "Dr. Unknown" };

        // Act
        var result = await repository.GetAllCoursesAsync(query);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCoursesByIdsAsync_ValidIds_CoursesReturned()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = CreateCourseRepository(context);
        var courseIds = new List<string> { "C001", "OOP" };

        // Act
        var result = await repository.GetCoursesByIdsAsync(courseIds);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result.Select(c => c.CourseId).Should().Contain("C001", "OOP");
    }

    [Fact]
    public async Task GetCourseByCourseIdAsync_CourseExists_CourseReturned()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = CreateCourseRepository(context);
        var courseId = "C001";

        // Act
        var result = await repository.GetCourseByCourseIdAsync(courseId);

        // Assert
        result.Should().NotBeNull();
        result.CourseId.Should().Be(courseId);
        result.CourseName.Should().Be("Introduction to C#");
    }

    [Fact]
    public async Task GetCourseByCourseIdAsync_CourseDoesNotExist_NullReturned()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = CreateCourseRepository(context);
        var courseId = "UNKNOWN";

        // Act
        var result = await repository.GetCourseByCourseIdAsync(courseId);

        // Assert
        result.Should().BeNull();
    }
}

