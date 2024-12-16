using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudentApi.Data;
using StudentApi.Models;
using StudentApi.Repositories;
using StudentApi.Helpers;

namespace StudentApiTest.Repositories;
public class StudentRepositoryTest
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

        var listStudents = new List<Student>
            {
                new Student { StudentId = 1, FullName = "John Doe", Grade = 1 , Address = "123 Main St", DateOfBirth = new DateOnly(2003, 2, 2), Email = "john.doe@example.com", PhoneNumber = "5551234567" },
                new Student { StudentId = 2, FullName = "Jane Smith", Grade = 1, Address = "456 Elm St", DateOfBirth = new DateOnly(2004, 5, 9), Email = "jane.smith@example.com", PhoneNumber = "1234567890" },
                new Student { StudentId = 3, FullName = "Tom Johnson", Grade = 2, Address = "789 Oak St", DateOfBirth = new DateOnly(2005, 8, 15), Email = "tom.johnson@example.com", PhoneNumber = "9876543210" }
            };

        context.Students.AddRange(listStudents);
        await context.SaveChangesAsync();
        return context;
    }

    private async Task<StudentRepository> CreateStudentRepository(ApplicationDbContext context)
    {
        return new StudentRepository(context);
    }
    [Fact]
    public async Task GetAllStudentsAsync_QueryMatches_StudentsReturned()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateStudentRepository(context);
        var query = new StudentQuery { StudentName = "John Doe", Address = "123 Main St", Email = "john.doe@example.com" };

        // Act
        var result = await repository.GetAllStudentsAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        result[0].FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetAllStudentsAsync_QueryNotMatches_EmptyList()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateStudentRepository(context);
        var query = new StudentQuery { GradeMax = 1, GradeMin = 2, PhoneNumber = "1234567890" };
        // Act
        var result = await repository.GetAllStudentsAsync(query);
        // Assert
        result.Should().BeEmpty();
        result.Count.Should().Be(0);
    }
    [Fact]
    public async Task GetStudentById_StudentExists_Student()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateStudentRepository(context);
        var studentId = 1;

        // Act
        var result = await repository.GetStudentByIdAsync(studentId);

        // Assert
        result.Should().NotBeNull();
        result.StudentId.Should().Be(studentId);
        result.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetStudentById_StudentDoesNotExist_Null()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateStudentRepository(context);
        var studentId = 4;

        // Act
        var result = await repository.GetStudentByIdAsync(studentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateStudentAsync_ValidStudent_StudentCreated()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateStudentRepository(context);
        var newStudent = new Student { FullName = "New Student", Email = "new.student@example.com", Address = "123 New St", Grade = 1, DateOfBirth = new DateOnly(2005, 1, 1), PhoneNumber = "5550000000" };

        // Act
        var result = await repository.CreateStudentAsync(newStudent);

        // Assert
        result.Should().NotBeNull();
        result.FullName.Should().Be("New Student");
    }
    [Fact]
    public async Task DeleteStudentAsync_ValidStudent_StudentDeleted()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateStudentRepository(context);
        var student = await repository.GetStudentByIdAsync(1);

        // Act
        var result = await repository.DeleteStudentAsync(student);
        
        // Assert
        result.Should().BeTrue();
        context.Students.Count().Should().Be(2);
    }

    [Fact]
    public async Task DeleteStudentAsync_StudentDoesNotExist_ThrowException()
    {
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateStudentRepository(context);
        var student = new Student
        {
            StudentId = 4,
            FullName = "Delete Student",
            Grade = 1,
            Address = "000 Main St",
            DateOfBirth = new DateOnly(2003, 2, 2),
            Email = "john.doe@example.com",
            PhoneNumber = "5550004567"
        };

        var result = async () =>
        {
            await repository.DeleteStudentAsync(student);
        };

        await result.Should().ThrowExactlyAsync<DbUpdateConcurrencyException>();
        context.Students.Count().Should().Be(3);
    }

    [Fact]
    public async Task UpdateStudentAsync_ValidStudent_StudentUpdated()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateStudentRepository(context);
        var studentId = 1;
        var student = await repository.GetStudentByIdAsync(studentId);
        student.FullName = "Updated Student";

        // Act
        var result = await repository.UpdateStudentAsync(student);

        // Assert
        result.Should().NotBeNull();
        result.FullName.Should().Be("Updated Student");
    }

    [Fact]
    public async Task GetStudentsByIdsAsync_ValidIds_StudentsReturned()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateStudentRepository(context);
        var ids = new List<int> { 1, 2 };

        // Act
        var result = await repository.GetStudentsByIdsAsync(ids);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetStudentByEmailAsync_StudentExists_StudentReturned()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateStudentRepository(context);
        var email = "john.doe@example.com";

        // Act
        var result = await repository.GetStudentByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetStudentByEmailAsync_StudentDoesNotExist_NullReturned()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var repository = await CreateStudentRepository(context);
        var email = "non.existent@example.com";

        // Act
        var result = await repository.GetStudentByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }


}
