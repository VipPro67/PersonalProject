using System.Net;
using AutoMapper;
using CourseApi.DTOs;
using CourseApi.Helpers;
using CourseApi.Models;
using CourseApi.Repositories;
using CourseApi.Services;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
namespace CourseApiTest.Services;
public class EnrollmentServiceTests
{
    private readonly Mock<ICourseRepository> _mockCourseRepository;
    private readonly Mock<IEnrollmentRepository> _mockEnrollmentRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly EnrollmentService _enrollmentService;
    private readonly Mock<EnrollmentService> _mockEnrollmentService;

    public EnrollmentServiceTests()
    {
        _mockCourseRepository = new Mock<ICourseRepository>();
        _mockEnrollmentRepository = new Mock<IEnrollmentRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://fakeapi.com")
        };
        _enrollmentService = new EnrollmentService(
            _mockCourseRepository.Object,
            _mockEnrollmentRepository.Object,
            _mockMapper.Object
        );
        _mockEnrollmentService = new Mock<EnrollmentService>(_mockCourseRepository.Object, _mockEnrollmentRepository.Object, _mockMapper.Object)
        {
            CallBase = true
        };
        _mockEnrollmentService.Setup(s => s.CreateHttpClient()).Returns(httpClient);

    }
    [Fact]
    public async Task GetAllEnrollmentsAsync_NoEnrollments_NotFound()
    {
        // Arrange
        _mockEnrollmentRepository.Setup(r => r.GetAllEnrollmentsAsync())
                                 .ReturnsAsync(new List<Enrollment>());

        // Act
        var result = await _enrollmentService.GetAllEnrollmentsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.NotFound);
        result.Message.Should().Be("No enrollments found");
    }

    [Fact]
    public async Task GetAllEnrollmentsAsync_Success()
    {
        // Arrange
        var enrollments = new List<Enrollment>
    {
        new Enrollment { EnrollmentId = 1, StudentId = 1, CourseId = "C001" },
        new Enrollment { EnrollmentId = 2, StudentId = 2, CourseId = "C002" }
    };
        _mockEnrollmentRepository.Setup(r => r.GetAllEnrollmentsAsync())
                                 .ReturnsAsync(enrollments);

        var studentsApiResponse = new ApiResponse<List<Student>>()
        {
            Data = new List<Student>
        {
            new Student { StudentId = 1, FullName = "John Doe" },
            new Student { StudentId = 2, FullName = "Jane Smith" }
        }
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(studentsApiResponse))
            });
        Environment.SetEnvironmentVariable("StudentApiUrl", "https://localhost:5002");
        // Act
        var result = await _mockEnrollmentService.Object.GetAllEnrollmentsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.Ok);
        result.Message.Should().Be("Get all enrollments successfully");
    }
    [Fact]
    public async Task GetEnrollmentByIdAsync_EnrollmentNotFound_NotFound()
    {
        // Arrange
        var enrollmentId = 1;
        _mockEnrollmentRepository.Setup(r => r.GetEnrollmentByIdAsync(enrollmentId))
            .ReturnsAsync((Enrollment)null);

        // Act
        var result = await _enrollmentService.GetEnrollmentByIdAsync(enrollmentId);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.NotFound);
        result.Message.Should().Be("No enrollments found");
    }

    [Fact]
    public async Task GetEnrollmentByIdAsync_Success()
    {
        // Arrange
        var enrollmentId = 1;
        var enrollment = new Enrollment { EnrollmentId = enrollmentId, StudentId = 1, CourseId = "C001" };
        _mockEnrollmentRepository.Setup(r => r.GetEnrollmentByIdAsync(enrollmentId))
                                 .ReturnsAsync(enrollment);

        var studentApiResponse = new ApiResponse<Student>()
        {
            Data = new Student { StudentId = 1, FullName = "John Doe" }
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(studentApiResponse))
            });

        // Act
        var result = await _mockEnrollmentService.Object.GetEnrollmentByIdAsync(enrollmentId);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.Ok);
        result.Message.Should().Be("Get enrollment by id successfully");
    }
    [Fact]
    public async Task EnrollStudentInCourseAsync_CourseNotFound_NotFound()
    {
        // Arrange
        var createEnrollmentDto = new CreateEnrollmentDto { StudentId = 1, CourseId = "C001" };
        _mockCourseRepository.Setup(r => r.GetCourseByCourseIdAsync(createEnrollmentDto.CourseId))
                             .ReturnsAsync((Course)null);

        // Act
        var result = await _enrollmentService.EnrollStudentInCourseAsync(createEnrollmentDto);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.NotFound);
        result.Message.Should().Be("Course not found");
    }

    [Fact]
    public async Task EnrollStudentInCourseAsync_StudentAlreadyEnrolled_BadRequest()
    {
        // Arrange
        var createEnrollmentDto = new CreateEnrollmentDto { StudentId = 1, CourseId = "C001" };
        _mockCourseRepository.Setup(r => r.GetCourseByCourseIdAsync(createEnrollmentDto.CourseId))
                             .ReturnsAsync(new Course { CourseId = "C001", CourseName = "Test Course" });
        _mockEnrollmentRepository.Setup(r => r.IsStudentEnrolledInCourseAsync(createEnrollmentDto.StudentId, createEnrollmentDto.CourseId))
                                 .ReturnsAsync(true);

        // Act
        var result = await _enrollmentService.EnrollStudentInCourseAsync(createEnrollmentDto);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.BadRequest);
        result.Message.Should().Be("Student is already enrolled in the course");
    }

    [Fact]
    public async Task EnrollStudentInCourseAsync_Success()
    {
        // Arrange
        var createEnrollmentDto = new CreateEnrollmentDto { StudentId = 1, CourseId = "C001" };
        _mockCourseRepository.Setup(r => r.GetCourseByCourseIdAsync(createEnrollmentDto.CourseId))
                             .ReturnsAsync(new Course { CourseId = "C001", CourseName = "Test Course" });
        _mockEnrollmentRepository.Setup(r => r.IsStudentEnrolledInCourseAsync(createEnrollmentDto.StudentId, createEnrollmentDto.CourseId))
                                 .ReturnsAsync(false);

        var studentApiResponse = new ApiResponse<Student>()
        {
            Data = new Student { StudentId = 1, FullName = "John Doe" }
        };

        _mockHttpMessageHandler.Protected()
                               .Setup<Task<HttpResponseMessage>>(
                                   "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                               .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                               {
                                   Content = new StringContent(JsonConvert.SerializeObject(studentApiResponse))
                               });

        _mockMapper.Setup(m => m.Map<Enrollment>(It.IsAny<CreateEnrollmentDto>()))
                   .Returns(new Enrollment { StudentId = 1, CourseId = "C001" });

        _mockEnrollmentRepository.Setup(r => r.CreateEnrollmentAsync(It.IsAny<Enrollment>()))
                                 .ReturnsAsync(new Enrollment { EnrollmentId = 1, StudentId = 1, CourseId = "C001" });

        // Act
        var result = await _mockEnrollmentService.Object.EnrollStudentInCourseAsync(createEnrollmentDto);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.Ok);
        result.Message.Should().Be("Enroll student in course successfully");
    }
    [Fact]
    public async Task DeleteEnrollmentAsync_EnrollmentNotFound_NotFound()
    {
        // Arrange
        var enrollmentId = 1;
        _mockEnrollmentRepository.Setup(r => r.GetEnrollmentByIdAsync(enrollmentId))
            .ReturnsAsync((Enrollment)null);

        // Act
        var result = await _enrollmentService.DeleteEnrollmentAsync(enrollmentId);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.NotFound);
        result.Message.Should().Be("Enrollment not found");
    }

    [Fact]
    public async Task DeleteEnrollmentAsync_Success()
    {
        // Arrange
        var enrollmentId = 1;
        var enrollment = new Enrollment { EnrollmentId = enrollmentId, StudentId = 1, CourseId = "C001" };
        _mockEnrollmentRepository.Setup(r => r.GetEnrollmentByIdAsync(enrollmentId))
                                 .ReturnsAsync(enrollment);
        _mockEnrollmentRepository.Setup(r => r.DeleteEnrollmentAsync(enrollment))
                                 .ReturnsAsync(true);

        // Act
        var result = await _enrollmentService.DeleteEnrollmentAsync(enrollmentId);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.Ok);
        result.Message.Should().Be("Delete enrollment successfully");
    }

}
