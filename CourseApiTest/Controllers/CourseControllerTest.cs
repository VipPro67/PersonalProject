using AutoMapper;
using CourseApi.Controllers;
using CourseApi.DTOs;
using CourseApi.Helpers;
using CourseApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CourseApiTest.Controllers;
public class CourseControllerTests
{
    private readonly Mock<ICourseService> _mockCourseService;
    private readonly CourseController _courseController;

    public CourseControllerTests()
    {
        _mockCourseService = new Mock<ICourseService>();
        _courseController = new CourseController(_mockCourseService.Object);
    }

    [Fact]
    public async Task GetAllCoursesAsync_ServiceResultTypeSuccess_OkResult()
    {
        // Arrange
        var query = new CourseQuery();
        var listCourses = new List<CourseDto>
        {
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
           }
       };
        var serviceResult = new ServiceResult(listCourses, "Get list course successfully");
        _mockCourseService.Setup(s => s.GetCoursesAsync(query)).ReturnsAsync(serviceResult);

        // Act
        var result = await _courseController.GetAllCoursesAsync(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeOfType<SuccessResponse>();
        var successResponse = okResult.Value as SuccessResponse;
        successResponse.Data.Should().BeEquivalentTo(listCourses);
    }
    [Fact]
    public async Task GetAllCoursesAsync_ServiceResultNotFound_NotFoundResult()
    {
        // Arrange
        var query = new CourseQuery();
        var serviceResult = new ServiceResult(ResultType.NotFound, "Courses not found");
        _mockCourseService.Setup(s => s.GetCoursesAsync(query)).ReturnsAsync(serviceResult);
        // Act
        var result = await _courseController.GetAllCoursesAsync(query);
        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
    [Fact]
    public async Task GetCourseByIdAsync_ServiceResultTypeSuccess_OkResult()
    {
        // Arrange
        var courseId = "C001";
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
        var serviceResult = new ServiceResult(courseDto, "Course found successfully");
        _mockCourseService.Setup(s => s.GetCourseByCourseIdAsync(courseId)).ReturnsAsync(serviceResult);

        // Act
        var result = await _courseController.GetCourseByIdAsync(courseId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeOfType<SuccessResponse>();
        var successResponse = okResult.Value as SuccessResponse;
        successResponse.Data.Should().BeEquivalentTo(courseDto);
    }

    [Fact]
    public async Task GetCourseByIdAsync_ServiceResultTypeNotFound_NotFoundResult()
    {
        // Arrange
        var courseId = "C001";
        var serviceResult = new ServiceResult(ResultType.NotFound, "Course not found");
        _mockCourseService.Setup(s => s.GetCourseByCourseIdAsync(courseId)).ReturnsAsync(serviceResult);

        // Act
        var result = await _courseController.GetCourseByIdAsync(courseId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetStudentsInCourseAsync_ServiceResultTypeSuccess_OkResult()
    {
        // Arrange
        var courseId = "C001";
        var listStudents = new List<StudentDto> {
            new StudentDto {
                StudentId = 1,
                FullName = "John Doe",
            }
        };
        var serviceResult = new ServiceResult(listStudents, "Get list of students successfully");
        _mockCourseService.Setup(s => s.GetStudentsByCourseIdAsync(courseId)).ReturnsAsync(serviceResult);

        // Act
        var result = await _courseController.GetStudentsInCourseAsync(courseId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeOfType<SuccessResponse>();
        var successResponse = okResult.Value as SuccessResponse;
        successResponse.Data.Should().BeEquivalentTo(listStudents);
    }
    [Fact]
    public async Task GetStudentsInCourseAsync_ServiceResultTypeNotFound_NotFoundResult()
    {
        // Arrange
        var courseId = "C001";
        var serviceResult = new ServiceResult(ResultType.NotFound, "Course not found");
        _mockCourseService.Setup(s => s.GetStudentsByCourseIdAsync(courseId)).ReturnsAsync(serviceResult);

        // Act
        var result = await _courseController.GetStudentsInCourseAsync(courseId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }


    [Fact]
    public async Task AddCourseAsync_ServiceResultTypeSuccess_CreatedResult()
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
        var serviceResult = new ServiceResult(courseDto, "Create course successfully");
        _mockCourseService.Setup(s => s.CreateCourseAsync(createCourseDto)).ReturnsAsync(serviceResult);
        // Act
        var result = await _courseController.AddCourseAsync(createCourseDto);
        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var createdResult = result as OkObjectResult;
    }
    [Fact]
    public async Task AddCourseAsync_ServiceResultTypeBadRequest_BadRequestResult()
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

        var serviceResult = new ServiceResult(ResultType.BadRequest, "Course with id already exists");
        _mockCourseService.Setup(s => s.CreateCourseAsync(createCourseDto)).ReturnsAsync(serviceResult);

        // Act
        var result = await _courseController.AddCourseAsync(createCourseDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();

    }

    [Fact]
    public async Task UpdateCourseAsync_ServiceResultTypeSuccess_OkResult()
    {
        // Arrange
        var courseId = "C001";
        var updateCourseDto = new UpdateCourseDto
        {
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
        var serviceResult = new ServiceResult(courseDto, "Update course successfully");
        _mockCourseService.Setup(s => s.UpdateCourseAsync(courseId, updateCourseDto)).ReturnsAsync(serviceResult);

        // Act
        var result = await _courseController.UpdateCourseAsync(courseId, updateCourseDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeOfType<SuccessResponse>();
        var successResponse = okResult.Value as SuccessResponse;
        successResponse.Data.Should().BeEquivalentTo(courseDto);
    }
    [Fact]
    public async Task UpdateCourseAsync_ServiceResultTypeNotFound_NotFoundResult()
    {
        // Arrange
        var courseId = "C001";
        var serviceResult = new ServiceResult(ResultType.NotFound, "Course not found");
        _mockCourseService.Setup(s => s.UpdateCourseAsync(courseId, It.IsAny<UpdateCourseDto>())).ReturnsAsync(serviceResult);
    
        // Act
        var result = await _courseController.UpdateCourseAsync(courseId, It.IsAny<UpdateCourseDto>());
        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
    [Fact]
    public async Task DeleteCourseAsync_ServiceResultTypeSuccess_OkResult()
    {
        // Arrange
        var courseId = "C001";
        var serviceResult = new ServiceResult(ResultType.Ok, "Delete course successfully");
        _mockCourseService.Setup(s => s.DeleteCourseAsync(courseId)).ReturnsAsync(serviceResult);

        // Act
        var result = await _courseController.DeleteCourseAsync(courseId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeOfType<SuccessResponse>();
        var successResponse = okResult.Value as SuccessResponse;
        successResponse.Message.Should().Be("Delete course successfully");
    }

}

