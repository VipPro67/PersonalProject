using AutoMapper;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using StudentApi.Repositories;
using StudentApi.Services;
using StudentApi.DTOs;
using StudentApi.Models;
using System.Net;
using StudentApi.Helpers;
using Moq.Protected;
namespace StudentApiTest.Services;
public class StudentServiceTests
{
    private readonly Mock<IStudentRepository> _mockStudentRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<StudentService> _mockStudentService;
    private readonly StudentService _studentService;
    public StudentServiceTests()
    {
        _mockStudentRepository = new Mock<IStudentRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        { BaseAddress = new Uri("https://localhost:5001") };
        _mockStudentService = new Mock<StudentService>(_mockStudentRepository.Object, _mockMapper.Object) { CallBase = true };
        _mockStudentService.Setup(s => s.CreateHttpClient()).Returns(_httpClient);
        _studentService = new StudentService(_mockStudentRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task CreateStudentAsync_EmailExists_BadRequest()
    {
        // Arrange 
        var createStudentDto = new CreateStudentDto
        {
            Email = "existing_email@example.com",
            FullName = "New Student",
            Address = "123 New St",
            DateOfBirth = new DateOnly(2000, 1, 1),
            PhoneNumber = "1234567890"
        };
        var existingStudent = new Student
        {
            Email = "existing_email@example.com"
        };
        _mockStudentRepository.Setup(repo => repo.GetStudentByEmailAsync(createStudentDto.Email)).ReturnsAsync(existingStudent);
        // Act
        var result = await _studentService.CreateStudentAsync(createStudentDto);
        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.BadRequest);
        result.Message.Should().Be("Student with email already exists");
        _mockStudentRepository.Verify(repo => repo.GetStudentByEmailAsync(createStudentDto.Email), Times.Once);
        _mockStudentRepository.Verify(repo => repo.CreateStudentAsync(It.IsAny<Student>()), Times.Never);
    }
    [Fact]
    public async Task CreateStudentAsync_ValidEmail_CreatesStudent()
    {
        // Arrange 
        var createStudentDto = new CreateStudentDto
        {
            Email = "new_email@example.com",
            FullName = "New Student",
            Address = "123 New St",
            DateOfBirth = new DateOnly(2000, 1, 1),
            PhoneNumber = "1234567890"
        };
        var newStudent = new Student
        {
            StudentId = 1,
            Email = "new_email@example.com",
            FullName = "New Student",
            Address = "123 New St",
            DateOfBirth = new DateOnly(2000, 1, 1),
            PhoneNumber = "1234567890"
        }; var studentDto = new StudentDto
        {
            StudentId = 1,
            Email = "new_email@example.com",
            FullName = "New Student",
            Address = "123 New St",
            DateOfBirth = new DateOnly(2000, 1, 1),
            PhoneNumber = "1234567890"
        };
        _mockStudentRepository.Setup(repo => repo.GetStudentByEmailAsync(createStudentDto.Email)).ReturnsAsync((Student)null);
        _mockMapper.Setup(m => m.Map<Student>(createStudentDto)).Returns(newStudent);
        _mockStudentRepository.Setup(repo => repo.CreateStudentAsync(newStudent)).ReturnsAsync(newStudent);
        _mockMapper.Setup(m => m.Map<StudentDto>(newStudent)).Returns(studentDto);
        // Act 
        var result = await _studentService.CreateStudentAsync(createStudentDto);
        // Assert 
        result.Should().NotBeNull(); result.Type.Should().Be(ResultType.Ok);
        result.Message.Should().Be("Create student successfully");
        result.Data.Should().BeEquivalentTo(studentDto); _mockStudentRepository.Verify(repo => repo.GetStudentByEmailAsync(createStudentDto.Email), Times.Once);
        _mockStudentRepository.Verify(repo => repo.CreateStudentAsync(newStudent), Times.Once);
    }
    [Fact]
    public async Task DeleteStudentAsync_SuccessfulDeletion_Ok()
    {
        // Arrange
        int studentId = 1;
        var student = new Student { StudentId = studentId };
        Environment.SetEnvironmentVariable("CourseApiUrl", "https://localhost:5001");
        _mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(studentId)).ReturnsAsync(student);
        _mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
         ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
         .ReturnsAsync(new HttpResponseMessage
         {
             StatusCode = HttpStatusCode.NotFound,
         });
        _mockStudentRepository.Setup(repo => repo.DeleteStudentAsync(student)).ReturnsAsync(true);

        // Act
        var result = await _mockStudentService.Object.DeleteStudentAsync(studentId);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.Ok);
        result.Message.Should().Be("Delete student successfully");
    }
    [Fact]
    public async Task DeleteStudentAsync_StudentNotFound_NotFound()
    {
        // Arrange
        int studentId = 1;
        _mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(studentId)).ReturnsAsync((Student)null);

        // Act
        var result = await _studentService.DeleteStudentAsync(studentId);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.NotFound);
        result.Message.Should().Be("Student not found");
    }

    [Fact]
    public async Task DeleteStudentAsync_StudentInCourse_BadRequest()
    {
        // Arrange 
        int studentId = 1;
        var student = new Student { StudentId = studentId };
        var enrollments = new ApiResponse<List<EnrollmentDto>>
        {
            Data = new List<EnrollmentDto>
             { new EnrollmentDto { StudentId = studentId } }
        };
        Environment.SetEnvironmentVariable("CourseApiUrl", "https://localhost:5001");
        _mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(studentId)).ReturnsAsync(student);
        _mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
         ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
         .ReturnsAsync(new HttpResponseMessage
         {
             StatusCode = HttpStatusCode.OK,
             Content = new StringContent(JsonConvert.SerializeObject(enrollments))
         });
        // Act 
        var result = await _mockStudentService.Object.DeleteStudentAsync(studentId);
        // Assert 
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.BadRequest);
        result.Message.Should().Be("Student has enrollments");
    }


    [Fact]
    public async Task DeleteStudentAsync_ExceptionThrown_InternalServerError()
    {
        // Arrange
        int studentId = 1;
        var student = new Student { StudentId = studentId };

        _mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(studentId)).ReturnsAsync(student);
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new Exception());

        // Act
        var result = await _studentService.DeleteStudentAsync(studentId);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.InternalServerError);
        result.Message.Should().Be("Error retrieving students from CourseApi");
    }
    [Fact]
    public async Task GetStudentByEmailAsync_EmailExists_Ok()
    {
        // Arrange
        var email = "existing_email@example.com";
        var student = new Student
        {
            StudentId = 1,
            Email = email,
            FullName = "Existing Student",
            Address = "123 Existing St",
            DateOfBirth = new DateOnly(2000, 1, 1),
            PhoneNumber = "1234567890"
        };
        var studentDto = new StudentDto
        {
            StudentId = 1,
            Email = email,
            FullName = "Existing Student",
            Address = "123 Existing St",
            DateOfBirth = new DateOnly(2000, 1, 1),
            PhoneNumber = "1234567890"
        };
        _mockStudentRepository.Setup(repo => repo.GetStudentByEmailAsync(email)).ReturnsAsync(student);
        _mockMapper.Setup(m => m.Map<StudentDto>(student)).Returns(studentDto);

        // Act
        var result = await _studentService.GetStudentByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.Ok);
        result.Message.Should().Be("Get student by email successfully");
        result.Data.Should().BeEquivalentTo(studentDto);
        _mockStudentRepository.Verify(repo => repo.GetStudentByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task GetStudentByEmailAsync_EmailDoesNotExist_NotFound()
    {
        // Arrange
        string email = "nonexistent_email@example.com";
        _mockStudentRepository.Setup(repo => repo.GetStudentByEmailAsync(email)).ReturnsAsync((Student)null);

        // Act
        var result = await _studentService.GetStudentByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.NotFound);
        result.Message.Should().Be("Student not found");
        _mockStudentRepository.Verify(repo => repo.GetStudentByEmailAsync(email), Times.Once);
    }
    [Fact]
    public async Task GetStudentByIdAsync_ExistingId_Ok()
    {
        // Arrange
        int studentId = 1;
        var student = new Student
        {
            StudentId = studentId,
            FullName = "John Doe",
            Address = "123 Main St",
            Email = "john.doe@example.com",
            DateOfBirth = new DateOnly(2003, 2, 2),
            PhoneNumber = "5551234567",
            Grade = 2
        };
        var studentDto = new StudentDto
        {
            StudentId = studentId,
            FullName = "John Doe",
            Address = "123 Main St",
            Email = "john.doe@example.com",
            DateOfBirth = new DateOnly(2003, 2, 2),
            PhoneNumber = "5551234567",
            Grade = 2
        };

        _mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(studentId)).ReturnsAsync(student);
        _mockMapper.Setup(m => m.Map<StudentDto>(student)).Returns(studentDto);

        // Act
        var result = await _studentService.GetStudentByIdAsync(studentId);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.Ok);
        result.Message.Should().Be("Get student by id successfully");
        result.Data.Should().BeEquivalentTo(studentDto);
        _mockStudentRepository.Verify(repo => repo.GetStudentByIdAsync(studentId), Times.Once);
    }
    [Fact]
    public async Task GetStudentByIdAsync_StudentNotFound_NotFound()
    {
        // Arrange
        int studentId = 999;
        _mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(studentId)).ReturnsAsync((Student)null);

        // Act
        var result = await _studentService.GetStudentByIdAsync(studentId);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.NotFound);
        result.Message.Should().Be("Student not found");
        _mockStudentRepository.Verify(repo => repo.GetStudentByIdAsync(studentId), Times.Once);
    }
    [Fact]
    public async Task GetStudentsAsync_NoStudents_NotFound()
    {
        // Arrange
        var query = new StudentQuery();
        _mockStudentRepository.Setup(repo => repo.GetAllStudentsAsync(query)).ReturnsAsync(new List<Student>());

        // Act
        var result = await _studentService.GetStudentsAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.NotFound);
        result.Message.Should().Be("Students not found");
        _mockStudentRepository.Verify(repo => repo.GetAllStudentsAsync(query), Times.Once);
    }

    [Fact]
    public async Task GetStudentsByIdsAsync_ValidIds_Ok()
    {
        // Arrange
        var ids = new List<int> { 1, 2 };
        var students = new List<Student>
        {
            new Student { StudentId = 1, FullName = "John Doe" },
            new Student { StudentId = 2, FullName = "Jane Smith" }
        };
        var studentDtos = new List<StudentDto>
        {
            new StudentDto { StudentId = 1, FullName = "John Doe" },
            new StudentDto { StudentId = 2, FullName = "Jane Smith" }
        };
        _mockStudentRepository.Setup(repo => repo.GetStudentsByIdsAsync(ids)).ReturnsAsync(students);
        _mockMapper.Setup(m => m.Map<List<StudentDto>>(students)).Returns(studentDtos);

        // Act
        var result = await _studentService.GetStudentsByIdsAsync(ids);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.Ok);
        result.Message.Should().Be("Get list students by ids successfully");
        result.Data.Should().BeEquivalentTo(studentDtos);
        _mockStudentRepository.Verify(repo => repo.GetStudentsByIdsAsync(ids), Times.Once);
    }
    [Fact]
    public async Task GetStudentsByIdsAsync_UserNotExist_NotFound()
    {
        // Arrange
        var ids = new List<int> { 1, 2 };
        var students = new List<Student>{};
        _mockStudentRepository.Setup(repo => repo.GetStudentsByIdsAsync(ids)).ReturnsAsync(students);

        // Act
        var result = await _studentService.GetStudentsByIdsAsync(ids);

        // Assert
        result.Type.Should().Be(ResultType.NotFound);
        result.Message.Should().Be("Students not found");
    }

    [Fact]
    public async Task GetStudentsAsync_ValidQuery_Ok()
    {
        // Arrange
        var query = new StudentQuery();
        var listStudents = new List<Student>
        {
            new Student
            {
                StudentId = 1,
                FullName = "John Doe",
                Address = "123 Main St",
                Email = "john.doe@example.com",
                DateOfBirth = new DateOnly(2003, 2, 2),
                PhoneNumber = "5551234567",
                Grade = 2
            },
            new Student
            {
                StudentId = 2,
                FullName = "Jane Smith",
                Address = "456 Elm St",
                Email = "jane.smith@example.com",
                DateOfBirth = new DateOnly(2004, 5, 9),
                PhoneNumber = "1234567890",
                Grade = 1
            }
        };
        var listStudentDtos = new List<StudentDto>
        {
            new StudentDto
            {
                StudentId = 1,
                FullName = "John Doe",
                Address = "123 Main St",
                Email = "john.doe@example.com",
                DateOfBirth = new DateOnly(2003, 2, 2),
                PhoneNumber = "5551234567",
                Grade = 2
            },
            new StudentDto
            {
                StudentId = 2,
                FullName = "Jane Smith",
                Address = "456 Elm St",
                Email = "jane.smith@example.com",
                DateOfBirth = new DateOnly(2004, 5, 9),
                PhoneNumber = "1234567890",
                Grade = 1
            }
        };
        _mockStudentRepository.Setup(repo => repo.GetAllStudentsAsync(query)).ReturnsAsync(listStudents);
        _mockMapper.Setup(m => m.Map<List<StudentDto>>(listStudents)).Returns(listStudentDtos);

        // Act
        var result = await _studentService.GetStudentsAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.Ok);
        result.Message.Should().Be("Get list students successfully");
        result.Data.Should().BeEquivalentTo(listStudentDtos);
        _mockStudentRepository.Verify(repo => repo.GetAllStudentsAsync(query), Times.Once);
    }

    [Fact]
    public async Task UpdateStudentAsync_ValidStudent_Ok()
    {
        // Arrange
        int studentId = 1;
        var updateStudentDto = new UpdateStudentDto
        {
            StudentId = studentId,
            FullName = "Updated Name",
            Address = "Updated Address",
            Email = "updated.email@example.com",
            DateOfBirth = new DateOnly(2000, 1, 1),
            PhoneNumber = "0987654321"
        };
        var student = new Student
        {
            StudentId = studentId,
            FullName = "John Doe",
            Address = "123 Main St",
            Email = "john.doe@example.com",
            DateOfBirth = new DateOnly(2003, 2, 2),
            PhoneNumber = "5551234567",
            Grade = 2
        };
        var updatedStudent = new Student
        {
            StudentId = studentId,
            FullName = "Updated Name",
            Address = "Updated Address",
            Email = "updated.email@example.com",
            DateOfBirth = new DateOnly(2000, 1, 1),
            PhoneNumber = "0987654321"
        };
        _mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(studentId)).ReturnsAsync(student);
        _mockStudentRepository.Setup(repo => repo.UpdateStudentAsync(updatedStudent)).ReturnsAsync(updatedStudent);

        var result = await _studentService.UpdateStudentAsync(studentId, updateStudentDto);

        result.Should().NotBeNull();
        result.Message.Should().Be("Update student successfully");

    }

    [Fact]
    public async Task UpdateStudentAsync_MismatchedId_BadRequest()
    {
        // Arrange
        int studentId = 1;
        var updateStudentDto = new UpdateStudentDto
        {
            StudentId = 2, // Mismatched ID
            FullName = "Updated Student",
            Address = "456 Updated St",
            Email = "updated_email@example.com",
            DateOfBirth = new DateOnly(2000, 1, 1),
            PhoneNumber = "0987654321"
        };

        // Act
        var result = await _studentService.UpdateStudentAsync(studentId, updateStudentDto);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.BadRequest);
        result.Message.Should().Be("Id in UpdateStudentDto does not match the id in the URL");
    }
    [Fact]
    public async Task UpdateStudentAsync_StudentNotFound_NotFound()
    {
        // Arrange
        int nonExistentStudentId = 999;
        var updateStudentDto = new UpdateStudentDto
        {
            StudentId = nonExistentStudentId,
            FullName = "Updated Name",
            Address = "Updated Address",
            Email = "updated.email@example.com",
            DateOfBirth = new DateOnly(2000, 1, 1),
            PhoneNumber = "0987654321"
        };

        _mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(nonExistentStudentId)).ReturnsAsync((Student)null);

        // Act
        var result = await _studentService.UpdateStudentAsync(nonExistentStudentId, updateStudentDto);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.NotFound);
        result.Message.Should().Be("Student not found");
        _mockStudentRepository.Verify(repo => repo.GetStudentByIdAsync(nonExistentStudentId), Times.Once);
        _mockStudentRepository.Verify(repo => repo.UpdateStudentAsync(It.IsAny<Student>()), Times.Never);
    }

}
