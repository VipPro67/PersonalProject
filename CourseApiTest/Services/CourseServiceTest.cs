using System;
using System.Net;
using System.Text;
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
public class CourseServiceTest
{
    private readonly CourseService _courseService;
    private readonly Mock<ICourseRepository> _mockCourseRepository;
    private readonly Mock<IEnrollmentRepository> _mockEnrollmentRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;


    private readonly Mock<CourseService> _mockCourseService;
    public CourseServiceTest()
    {
        _mockCourseRepository = new Mock<ICourseRepository>();
        _mockEnrollmentRepository = new Mock<IEnrollmentRepository>();
        _mockMapper = new Mock<IMapper>();
        _courseService = new CourseService(_mockCourseRepository.Object, _mockEnrollmentRepository.Object, _mockMapper.Object);
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        { BaseAddress = new Uri("https://localhost:5002") };
        _mockCourseService = new Mock<CourseService>(_mockCourseRepository.Object, _mockEnrollmentRepository.Object, _mockMapper.Object)
        { CallBase = true };
        _mockCourseService.Setup(s => s.CreateHttpClient()).Returns(httpClient);
    }

    [Fact]
    public async Task GetCoursesAsync_CoursesFound_ServiceResultOk()
    {
        // Arrange
        var listsCourses = new List<Course> {
        new Course
            {
                CourseId = "C001",
                CourseName = "Advanced Programming",
                Description = "An advanced course on programming concepts and techniques.",
                Credit = 4,
                Instructor = "Dr. John Smith",
                Department = "Computer Science",
                StartDate = new DateOnly(2024, 1, 10),
                EndDate = new DateOnly(2024, 5, 10),
                Schedule = "Mon, Wed, Fri 10:00-11:30"
            }};
        var listCoursesDto = new List<CourseDto> {
            new CourseDto
            {
                CourseId = "C001",
                CourseName = "Advanced Programming",
                Description = "An advanced course on programming concepts and techniques.",
                Credit = 4,
                Instructor = "Dr. John Smith",
                Department = "Computer Science",
                StartDate = new DateOnly(2024, 1, 10),
                EndDate = new DateOnly(2024, 5, 10),
                Schedule = "Mon, Wed, Fri 10:00-11:30"
            }};
        _mockMapper.Setup(m => m.Map<List<CourseDto>>(listsCourses)).Returns(listCoursesDto);
        var query = new CourseQuery();
        _mockCourseRepository.Setup(r => r.GetAllCoursesAsync(query)).ReturnsAsync(listsCourses);
        _mockMapper.Setup(m => m.Map<List<CourseDto>>(listsCourses)).Returns(listCoursesDto);

        // Act
        var result = await _courseService.GetCoursesAsync(query);
        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.Ok);
        result.Data.Should().BeEquivalentTo(listCoursesDto);
    }
    [Fact]
    public async Task GetCoursesAsync_NoCoursesFound_ServiceResultNotFound()
    {
        // Arrange
        var query = new CourseQuery();
        _mockCourseRepository.Setup(r => r.GetAllCoursesAsync(query)).ReturnsAsync(new List<Course>());
        // Act
        var result = await _courseService.GetCoursesAsync(query);
        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.NotFound);
        result.Message.Should().Be("No courses found");
    }
    [Fact]
    public async Task GetCourseByIdAsync_CourseFound_ServiceResultOk()
    {
        // Arrange
        var course = new Course
        {
            CourseId = "C001",
            CourseName = "Advanced Programming",
            Description = "An advanced course on programming concepts and techniques.",
            Credit = 4,
            Instructor = "Dr. John Smith",
            Department = "Computer Science",
            StartDate = new DateOnly(2024, 1, 10),
            EndDate = new DateOnly(2024, 5, 10),
            Schedule = "Mon, Wed, Fri 10:00-11:30"
        };
        var courseDto = new CourseDto
        {
            CourseId = "C001",
            CourseName = "Advanced Programming",
            Description = "An advanced course on programming concepts and techniques.",
            Credit = 4,
            Instructor = "Dr. John Smith",
            Department = "Computer Science",
            StartDate = new DateOnly(2024, 1, 10),
            EndDate = new DateOnly(2024, 5, 10),
            Schedule = "Mon, Wed, Fri 10:00-11:30"
        };
        _mockCourseRepository.Setup(r => r.GetCourseByCourseIdAsync(course.CourseId)).ReturnsAsync(course);
        _mockMapper.Setup(m => m.Map<CourseDto>(course)).Returns(courseDto);
        // Act
        var result = await _courseService.GetCourseByCourseIdAsync(course.CourseId);
        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.Ok);
        result.Data.Should().BeEquivalentTo(courseDto);
    }
    [Fact]
    public async Task GetCourseByIdAsync_NoCourseFound_ServiceResultNotFound()
    {
        // Arrange
        var courseId = "C001";
        _mockCourseRepository.Setup(r => r.GetCourseByCourseIdAsync(courseId)).ReturnsAsync((Course)null);
        // Act
        var result = await _courseService.GetCourseByCourseIdAsync(courseId);
        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.NotFound);
        result.Message.Should().Be("Course not found");
    }

    [Fact]
    public async Task CreateCourseAsync_ValidCourse_ServiceResultOk()
    {
        // Arrange
        var createCourseDto = new CreateCourseDto
        {
            CourseId = "C001",
            CourseName = "Advanced Programming",
            Description = "An advanced course on programming concepts and techniques.",
            Credit = 4,
            Instructor = "Dr. John Smith",
            Department = "Computer Science",
            StartDate = new DateOnly(2024, 1, 10),
            EndDate = new DateOnly(2024, 5, 10),
            Schedule = "Mon, Wed, Fri 10:00-11:30"
        };
        var newCourse = new Course
        {
            CourseId = "C001",
            CourseName = "Advanced Programming",
            Description = "An advanced course on programming concepts and techniques.",
            Credit = 4,
            Instructor = "Dr. John Smith",
            Department = "Computer Science",
            StartDate = new DateOnly(2024, 1, 10),
            EndDate = new DateOnly(2024, 5, 10),
            Schedule = "Mon, Wed, Fri 10:00-11:30"
        };
        var courseDto = new CourseDto
        {
            CourseId = "C001",
            CourseName = "Advanced Programming",
            Description = "An advanced course on programming concepts and techniques.",
            Credit = 4,
            Instructor = "Dr. John Smith",
            Department = "Computer Science",
            StartDate = new DateOnly(2024, 1, 10),
            EndDate = new DateOnly(2024, 5, 10),
            Schedule = "Mon, Wed, Fri 10:00-11:30"
        };
        _mockCourseRepository.Setup(r => r.GetCourseByCourseIdAsync(createCourseDto.CourseId)).ReturnsAsync((Course)null);
        _mockCourseRepository.Setup(r => r.CreateCourseAsync(newCourse)).ReturnsAsync(newCourse);
        _mockMapper.Setup(m => m.Map<Course>(createCourseDto)).Returns(newCourse);
        _mockMapper.Setup(m => m.Map<CourseDto>(newCourse)).Returns(courseDto);
        // Act
        var result = await _courseService.CreateCourseAsync(createCourseDto);
        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.Ok);
        result.Data.Should().BeEquivalentTo(courseDto);

    }
    [Fact]
    public async Task CreateCourseAsync_CourseIdExist_ServiceResultBadRequest()
    {
        // Arrange
        var createCourseDto = new CreateCourseDto
        {
            CourseId = "C001",
            CourseName = "Advanced Programming",
            Description = "An advanced course on programming concepts and techniques.",
            Credit = 4,
            Instructor = "Dr. John Smith",
            Department = "Computer Science",
            StartDate = new DateOnly(2024, 1, 10),
            EndDate = new DateOnly(2024, 5, 10),
            Schedule = "Mon, Wed, Fri 10:00-11:30"
        };
        var course = new Course
        {
            CourseId = "C001",
            CourseName = "Advanced Programming",
            Description = "An advanced course on programming concepts and techniques.",
            Credit = 4,
            Instructor = "Dr. John Smith",
            Department = "Computer Science",
            StartDate = new DateOnly(2024, 1, 10),
            EndDate = new DateOnly(2024, 5, 10),
            Schedule = "Mon, Wed, Fri 10:00-11:30"
        };
        _mockCourseRepository.Setup(r => r.GetCourseByCourseIdAsync(createCourseDto.CourseId)).ReturnsAsync(new Course());
        _mockMapper.Setup(m => m.Map<Course>(createCourseDto)).Returns(course);
        // Act
        var result = await _courseService.CreateCourseAsync(createCourseDto);
        // Assert
        result.Should().NotBeNull();

    }

    [Fact]
    public async Task UpdateCourseAsync_CourseFound_ServiceResultOk()
    {
        // Arrange
        var course = new Course
        {
            CourseId = "C001",
            CourseName = "Advanced Programming",
            Description = "An advanced course on programming concepts and techniques.",
            Credit = 4,
            Instructor = "Dr. John Smith",
            Department = "Computer Science",
            StartDate = new DateOnly(2024, 1, 10),
            EndDate = new DateOnly(2024, 5, 10),
            Schedule = "Mon, Wed, Fri 10:00-11:30"
        };
        var updatedCourseDto = new UpdateCourseDto
        {
            CourseName = "Updated Advanced Programming",
            Description = "Updated advanced course on programming concepts and techniques.",
            Credit = 4,
            Instructor = "Dr. John Smith",
            Department = "Computer Science",
            StartDate = new DateOnly(2024, 2, 10),
            EndDate = new DateOnly(2024, 6, 10),
            Schedule = "Mon, Wed, Fri 10:00-11:30"
        };
        var updatedCourse = new Course
        {
            CourseId = "C001",
            CourseName = "Updated Advanced Programming",
            Description = "Updated advanced course on programming concepts and techniques.",
            Credit = 4,
            Instructor = "Dr. John Smith",
            Department = "Computer Science",
            StartDate = new DateOnly(2024, 2, 10),
            EndDate = new DateOnly(2024, 6, 10),
            Schedule = "Mon, Wed, Fri 10:00-11:30"
        };
        var courseDto = new CourseDto
        {
            CourseId = "C001",
            CourseName = "Updated Advanced Programming",
            Description = "Updated advanced course on programming concepts and techniques.",
            Credit = 4,
            Instructor = "Dr. John Smith",
            Department = "Computer Science",
            StartDate = new DateOnly(2024, 2, 10),
            EndDate = new DateOnly(2024, 6, 10),
            Schedule = "Mon, Wed, Fri 10:00-11:30"
        };
        _mockCourseRepository.Setup(r => r.GetCourseByCourseIdAsync(course.CourseId)).ReturnsAsync(course);
        _mockMapper.Setup(m => m.Map(updatedCourse, course)).Returns(updatedCourse);
        _mockCourseRepository.Setup(r => r.EditCourseAsync(It.IsAny<Course>())).ReturnsAsync(updatedCourse);
        _mockMapper.Setup(m => m.Map<CourseDto>(updatedCourse)).Returns(courseDto);

        // Act
        var result = await _courseService.UpdateCourseAsync(course.CourseId, updatedCourseDto);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.Ok);
        result.Data.Should().BeEquivalentTo(courseDto);
    }
    [Fact]
    public async Task UpdateCourseAsync_NoCourseFound_ServiceResultNotFound()
    {
        // Arrange
        var courseId = "C001";

        _mockCourseRepository.Setup(r => r.GetCourseByCourseIdAsync(courseId)).ReturnsAsync((Course)null);
        // Act
        var result = await _courseService.UpdateCourseAsync(courseId, new UpdateCourseDto());
        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.NotFound);
        result.Message.Should().Be("Course not found");
    }

    [Fact]
    public async Task DeleteCourseAsync_CourseFound_ServiceResultOk()
    {
        // Arrange
        var course = new Course
        {
            CourseId = "C001",
            CourseName = "Advanced Programming",
            Description = "An advanced course on programming concepts and techniques.",
            Credit = 4,
            Instructor = "Dr. John Smith",
            Department = "Computer Science",
            StartDate = new DateOnly(2024, 1, 10),
            EndDate = new DateOnly(2024, 5, 10),
            Schedule = "Mon, Wed, Fri 10:00-11:30"
        };
        _mockCourseRepository.Setup(r => r.GetCourseByCourseIdAsync(course.CourseId)).ReturnsAsync(course);
        _mockEnrollmentRepository.Setup(r => r.GetEnrollmentsByCourseIdAsync(course.CourseId)).ReturnsAsync(new List<Enrollment>());
        _mockCourseRepository.Setup(r => r.DeleteCourseAsync(course)).ReturnsAsync(true);

        // Act
        var result = await _courseService.DeleteCourseAsync(course.CourseId);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.Ok);
        result.Message.Should().Be("Delete course successfully");
    }
    [Fact]
    public async Task DeleteCourseAsync_NoCourseFound_ServiceResultNotFound()
    {
        // Arrange
        var courseId = "C001";
        _mockCourseRepository.Setup(r => r.GetCourseByCourseIdAsync(courseId)).ReturnsAsync((Course)null);
        // Act
        var result = await _courseService.DeleteCourseAsync(courseId);
        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.NotFound);
        result.Message.Should().Be("Course not found");
    }
    [Fact]
    public async Task DeleteCourseAsync_CourseInEnrollment_BadRequest()
    {

        // Arrange
        var course = new Course
        {
            CourseId = "C001",
            CourseName = "Advanced Programming",
            Description = "An advanced course on programming concepts and techniques.",
            Credit = 4,
            Instructor = "Dr. John Smith",
            Department = "Computer Science",
            StartDate = new DateOnly(2024, 1, 10),
            EndDate = new DateOnly(2024, 5, 10),
            Schedule = "Mon, Wed, Fri 10:00-11:30"
        };
        var listErollments = new List<Enrollment> { new Enrollment { CourseId = "C001", StudentId = 1 } };

        _mockCourseRepository.Setup(r => r.GetCourseByCourseIdAsync(course.CourseId)).ReturnsAsync(course);
        _mockEnrollmentRepository.Setup(r => r.GetEnrollmentsByCourseIdAsync(course.CourseId)).ReturnsAsync(listErollments);
        _mockCourseRepository.Setup(r => r.DeleteCourseAsync(course)).ReturnsAsync(true);

        // Act
        var result = await _courseService.DeleteCourseAsync(course.CourseId);
        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.BadRequest);
        result.Message.Should().Be("Exist enrollment in this course");
    }
    [Fact]
    public async Task GetStudentsByCourseIdAsync_NoEnrollments_NotFound()
    {
        // Arrange
        var courseId = "C001";
        _mockEnrollmentRepository.Setup(repo => repo.GetEnrollmentsByCourseIdAsync(courseId))
            .ReturnsAsync(new List<Enrollment>());

        // Act
        var result = await _courseService.GetStudentsByCourseIdAsync(courseId);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.NotFound);
        result.Message.Should().Be("No enrollment found");
        _mockEnrollmentRepository.Verify(repo => repo.GetEnrollmentsByCourseIdAsync(courseId), Times.Once);
    }

    [Fact]
    public async Task GetStudentsByCourseIdAsync_FailedApiCall_InternalServerError()
    {
        // Arrange
        var courseId = "C001";
        var enrollments = new List<Enrollment>
    {
        new Enrollment { StudentId = 1 },
        new Enrollment { StudentId = 2 }
    };
        _mockEnrollmentRepository.Setup(repo => repo.GetEnrollmentsByCourseIdAsync(courseId))
                                 .ReturnsAsync(enrollments);

        var studentIds = enrollments.Select(e => e.StudentId).ToList();

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        // Act
        var result = await _courseService.GetStudentsByCourseIdAsync(courseId);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.InternalServerError);
        result.Message.Should().Be("Error retrieving students from StudentApi");
    }

    [Fact]
    public async Task GetStudentsByCourseIdAsync_Successfully_FetchedStudents_Ok()
    {
        // Arrange
        var courseId = "C001";
        var enrollments = new List<Enrollment>
    {
        new Enrollment { StudentId = 1},
        new Enrollment { StudentId = 2 }
    };
        var studentApiResponse = new ApiResponse<List<Student>>()
        {
            Data = new List<Student>
        {
            new Student { StudentId = 1, FullName = "John Doe" },
            new Student { StudentId = 2, FullName = "Jane Smith" }
        }
        };

        _mockEnrollmentRepository.Setup(repo => repo.GetEnrollmentsByCourseIdAsync(courseId))
            .ReturnsAsync(enrollments);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(studentApiResponse))
            });

        // Act
        var result = await _mockCourseService.Object.GetStudentsByCourseIdAsync(courseId);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.Ok);
        result.Message.Should().Be("Get list student in course successfully");
    }

    [Fact]
    public async Task GetStudentsByCourseIdAsync_ExceptionThrown_InternalServerError()
    {
        // Arrange
        var courseId = "C001";
        var enrollments = new List<Enrollment>
    {
        new Enrollment { StudentId = 1 }
    };

        _mockEnrollmentRepository.Setup(repo => repo.GetEnrollmentsByCourseIdAsync(courseId))
            .ReturnsAsync(enrollments);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("External API error"));

        // Act
        var result = await _courseService.GetStudentsByCourseIdAsync(courseId);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ResultType.InternalServerError);
        result.Message.Should().Be("Error retrieving students from StudentApi");
    }

}
