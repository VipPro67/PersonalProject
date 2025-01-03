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

    private readonly Mock<CourseApi.Protos.StudentService.StudentServiceClient> _mockStudentServiceClient;
    private readonly EnrollmentService _enrollmentService;
    public EnrollmentServiceTests()
    {
        _mockCourseRepository = new Mock<ICourseRepository>();
        _mockEnrollmentRepository = new Mock<IEnrollmentRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockStudentServiceClient = new Mock<CourseApi.Protos.StudentService.StudentServiceClient>();
        _enrollmentService = new EnrollmentService(
            _mockCourseRepository.Object,
            _mockEnrollmentRepository.Object,
            _mockMapper.Object,
            _mockStudentServiceClient.Object
        );
    }
    [Fact]
    public async Task GetAllEnrollmentsAsync_NoEnrollments_NotFound()
    {
        // Arrange
        var query = new EnrollmentQuery { };
        _mockEnrollmentRepository.Setup(r => r.GetAllEnrollmentsAsync(query))
                                 .ReturnsAsync(new List<Enrollment>());

        // Act
        var result = await _enrollmentService.GetAllEnrollmentsAsync(query);

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
        var query = new EnrollmentQuery { };
        _mockEnrollmentRepository.Setup(r => r.GetAllEnrollmentsAsync(query))
                                 .ReturnsAsync(enrollments);

        var studentResponse = new CourseApi.Protos.StudentsResponse
        {
            Students =
            {
                new CourseApi.Protos.StudentResponse { StudentId = 1, Name = "John Doe" },
                new CourseApi.Protos.StudentResponse { StudentId = 2, Name = "Jane Doe" }
            }
        };
        _mockStudentServiceClient.Setup(c => c.GetStudentsByIdsAsync(It.IsAny<CourseApi.Protos.GetStudentsByIdsRequest>(), null, null, default))
                                 .Returns(new Grpc.Core.AsyncUnaryCall<CourseApi.Protos.StudentsResponse>(
                                     Task.FromResult(studentResponse),
                                     Task.FromResult(new Grpc.Core.Metadata()),
                                     () => new Grpc.Core.Status(),
                                     () => new Grpc.Core.Metadata(),
                                     () => { }
                                 ));

        _mockEnrollmentRepository.Setup(r => r.GetTotalEnrollmentsAsync(query))
                                 .ReturnsAsync(2);
        _mockMapper.Setup(m => m.Map<List<EnrollmentDto>>(It.IsAny<List<Enrollment>>()))
                   .Returns(new List<EnrollmentDto> {
                   new EnrollmentDto { EnrollmentId = 1, StudentId = 1, CourseId = "C001" },
                   new EnrollmentDto { EnrollmentId = 2, StudentId = 2, CourseId = "C002" }
                   });

        // Act
        var result = await _enrollmentService.GetAllEnrollmentsAsync(query);

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
        var response = new CourseApi.Protos.StudentResponse { StudentId = 1, Name = "John Doe" };
        _mockStudentServiceClient.Setup(c => c.GetStudentByIdAsync(It.IsAny<CourseApi.Protos.GetStudentByIdRequest>(), null, null, default))
                                 .Returns(new Grpc.Core.AsyncUnaryCall<CourseApi.Protos.StudentResponse>(
                                     Task.FromResult(response),
                                     Task.FromResult(new Grpc.Core.Metadata()),
                                     () => new Grpc.Core.Status(),
                                     () => new Grpc.Core.Metadata(),
                                     () => { }
                                 ));

        // Act
        var result = await _enrollmentService.GetEnrollmentByIdAsync(enrollmentId);

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
        var query = new EnrollmentQuery { CourseId = "C001", StudentId = 1 };
        _mockEnrollmentRepository.Setup(r => r.IsStudentEnrolledInCourseAsync(query.StudentId.Value, query.CourseId))
                                 .ReturnsAsync(true);
        // Act
        var result = await _enrollmentService.EnrollStudentInCourseAsync(createEnrollmentDto);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.BadRequest);
        result.Message.Should().Be("Student already enrolled in course");
    }


    [Fact]
    public async Task EnrollStudentInCourseAsync_ConnectStudentServiceFailed_InternalServerError()
    {
        // Arrange
        var createEnrollmentDto = new CreateEnrollmentDto { StudentId = 1, CourseId = "C001" };
        _mockCourseRepository.Setup(r => r.GetCourseByCourseIdAsync(createEnrollmentDto.CourseId))
        .ReturnsAsync(new Course { CourseId = "C001", CourseName = "Test Course" });
        _mockEnrollmentRepository.Setup(r => r.IsStudentEnrolledInCourseAsync(createEnrollmentDto.StudentId, createEnrollmentDto.CourseId))
            .ReturnsAsync(false);
        _mockMapper.Setup(m => m.Map<Enrollment>(It.IsAny<CreateEnrollmentDto>()))
            .Returns(new Enrollment { StudentId = 1, CourseId = "C001" });
        _mockStudentServiceClient.Setup(c => c.GetStudentByIdAsync(It.IsAny<CourseApi.Protos.GetStudentByIdRequest>(), null, null, default))
            .Throws(new Exception("Error retrieving student from StudentApi"));

        // Act
        var result = await _enrollmentService.EnrollStudentInCourseAsync(createEnrollmentDto);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.InternalServerError);
        result.Message.Should().Be("Error retrieving students from StudentApi");
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

        _mockMapper.Setup(m => m.Map<Enrollment>(It.IsAny<CreateEnrollmentDto>()))
            .Returns(new Enrollment { StudentId = 1, CourseId = "C001" });

        _mockEnrollmentRepository.Setup(r => r.CreateEnrollmentAsync(It.IsAny<Enrollment>()))
            .ReturnsAsync(new Enrollment { EnrollmentId = 1, StudentId = 1, CourseId = "C001" });
        _mockStudentServiceClient.Setup(c => c.GetStudentByIdAsync(It.IsAny<CourseApi.Protos.GetStudentByIdRequest>(), null, null, default))
            .Returns(new Grpc.Core.AsyncUnaryCall<CourseApi.Protos.StudentResponse>(
                Task.FromResult(new CourseApi.Protos.StudentResponse { StudentId = 1, Name = "John Doe" }),
                Task.FromResult(new Grpc.Core.Metadata()),
                () => new Grpc.Core.Status(),
                () => new Grpc.Core.Metadata(),
                () => { }
            ));

        // Act
        var result = await _enrollmentService.EnrollStudentInCourseAsync(createEnrollmentDto);

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
