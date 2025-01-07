using System.Text.Json;
using CourseApi.Controllers;
using CourseApi.DTOs;
using CourseApi.Helpers;
using CourseApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;

namespace CourseApiTest.Controllers;

public class EnrollmentControllerTests
{
    private readonly Mock<IEnrollmentService> _mockEnrollmentService;
    private readonly EnrollmentController _enrollmentController;
    private readonly Mock<IHybridCacheWrapper> _mockCache;

    public EnrollmentControllerTests()
    {
        _mockEnrollmentService = new Mock<IEnrollmentService>();
        _mockCache = new Mock<IHybridCacheWrapper>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Cache-Control"] = "no-cache";
        _enrollmentController = new EnrollmentController(_mockEnrollmentService.Object, _mockCache.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }
    [Fact]
    public async Task GetAllEnrollmentsAsync_ServiceResultTypeSuccess_OkResult()
    {
        // Arrange

        var query = new EnrollmentQuery { };
        var expectedEnrollments = new List<EnrollmentDto> { };
        var serviceResult = new ServiceResult<List<EnrollmentDto>> { Type = ResultType.Ok, Data = expectedEnrollments, Message = "Get all enrollments successfully" };
        var serializedResult = JsonSerializer.Serialize(serviceResult);
        _mockCache.Setup(c => c.GetOrCreateAsync(
             It.IsAny<string>(),
             It.IsAny<Func<CancellationToken, ValueTask<string>>>(),
             It.IsAny<HybridCacheEntryOptions>(),
             It.IsAny<IEnumerable<string>>(),
             It.IsAny<CancellationToken>()))
         .ReturnsAsync(serializedResult);
        // Act
        var result = await _enrollmentController.GetAllEnrollmentsAsync(query);

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result);
        var actualResult = Assert.IsType<SuccessResponse>(actionResult.Value);
        actualResult.Message.Should().Be("Get all enrollments successfully");
        Assert.Equal(expectedEnrollments, actualResult.Data);
    }

    [Fact]
    public async Task GetEnrollmentByIdAsync_ValidId_OkResult()
    {
        // Arrange
        int enrollmentId = 1;
        var enrollmentDto = new EnrollmentDto
        {
            EnrollmentId = enrollmentId,
            StudentId = 1,
            CourseId = "C001",
        };
        var serviceResult = new ServiceResult<EnrollmentDto> { Type = ResultType.Ok, Data = enrollmentDto, Message = "Enrollment retrieved successfully" };
        _mockEnrollmentService.Setup(s => s.GetEnrollmentByIdAsync(enrollmentId)).ReturnsAsync(serviceResult);
        var serializedResult = JsonSerializer.Serialize(serviceResult);
        _mockCache.Setup(c => c.GetOrCreateAsync(
             It.IsAny<string>(),
             It.IsAny<Func<CancellationToken, ValueTask<string>>>(),
             It.IsAny<HybridCacheEntryOptions>(),
             It.IsAny<IEnumerable<string>>(),
             It.IsAny<CancellationToken>()))
         .ReturnsAsync(serializedResult);
        // Act
        var result = await _enrollmentController.GetEnrollmentByIdAsync(enrollmentId);
        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeOfType<SuccessResponse>();
        var successResponse = okResult.Value as SuccessResponse;
        successResponse.Data.Should().BeEquivalentTo(enrollmentDto);
        successResponse.Message.Should().Be("Enrollment retrieved successfully");
    }
    [Fact]
    public async Task GetEnrollmentByIdAsync_InvalidId_NotFound()
    {
        // Arrange
        int invalidId = 999;
        var serviceResult = new ServiceResult<EnrollmentDto>(ResultType.NotFound, "Enrollment not found");
        _mockEnrollmentService.Setup(s => s.GetEnrollmentByIdAsync(invalidId)).ReturnsAsync(serviceResult);
        var serializedResult = JsonSerializer.Serialize(serviceResult);
        _mockCache.Setup(c => c.GetOrCreateAsync(
             It.IsAny<string>(),
             It.IsAny<Func<CancellationToken, ValueTask<string>>>(),
             It.IsAny<HybridCacheEntryOptions>(),
             It.IsAny<IEnumerable<string>>(),
             It.IsAny<CancellationToken>()))
         .ReturnsAsync(serializedResult);

        // Act
        var result = await _enrollmentController.GetEnrollmentByIdAsync(invalidId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Value.Should().BeOfType<ErrorResponse>();
        var errorResponse = notFoundResult.Value as ErrorResponse;
        errorResponse.Message.Should().Be("Enrollment not found");
    }
    [Fact]
    public async Task GetEnrollmentsByCourseIdAsync_ValidCourseId_OkResultWithEnrollments()
    {
        // Arrange
        var courseId = "C001";
        var enrollments = new List<EnrollmentDto>
        {
            new EnrollmentDto { EnrollmentId = 1, CourseId = courseId, StudentId = 101 , CourseName = "Advanced Programming" , StudentName = "John Doe" },
            new EnrollmentDto { EnrollmentId = 2, CourseId = courseId, StudentId = 102 , CourseName = "Advanced Programming" , StudentName = "Jane Smith" }
        };
        var serviceResult = new ServiceResult<List<EnrollmentDto>>(enrollments, "Enrollments retrieved successfully");
        _mockEnrollmentService.Setup(s => s.GetEnrollmentsByCourseIdAsync(courseId)).ReturnsAsync(serviceResult);
        var serializedResult = JsonSerializer.Serialize(serviceResult);
        _mockCache.Setup(c => c.GetOrCreateAsync(
             It.IsAny<string>(),
             It.IsAny<Func<CancellationToken, ValueTask<string>>>(),
             It.IsAny<HybridCacheEntryOptions>(),
             It.IsAny<IEnumerable<string>>(),
             It.IsAny<CancellationToken>()))
         .ReturnsAsync(serializedResult);

        // Act
        var result = await _enrollmentController.GetEnrollmentsByCourseIdAsync(courseId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeOfType<SuccessResponse>();
        var successResponse = okResult.Value as SuccessResponse;
        successResponse.Data.Should().BeEquivalentTo(enrollments);
        successResponse.Message.Should().Be("Enrollments retrieved successfully");
    }
    [Fact]
    public async Task GetEnrollmentsByStudentIdAsync_ValidStudentId_OkResultWithEnrollments()
    {
        // Arrange
        int studentId = 1;
        var enrollments = new List<EnrollmentDto>
        {
            new EnrollmentDto { EnrollmentId = 1, StudentId = studentId, CourseId = "C001" },
            new EnrollmentDto { EnrollmentId = 2, StudentId = studentId, CourseId = "C002" }
        };
        var serviceResult = new ServiceResult<List<EnrollmentDto>>(enrollments, "Enrollments retrieved successfully");
        _mockEnrollmentService.Setup(s => s.GetEnrollmentsByStudentIdAsync(studentId)).ReturnsAsync(serviceResult);
        var serializedResult = JsonSerializer.Serialize(serviceResult);
        _mockCache.Setup(c => c.GetOrCreateAsync(
             It.IsAny<string>(),
             It.IsAny<Func<CancellationToken, ValueTask<string>>>(),
             It.IsAny<HybridCacheEntryOptions>(),
             It.IsAny<IEnumerable<string>>(),
             It.IsAny<CancellationToken>()))
         .ReturnsAsync(serializedResult);

        // Act
        var result = await _enrollmentController.GetEnrollmentsByStudentIdAsync(studentId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeOfType<SuccessResponse>();
        var successResponse = okResult.Value as SuccessResponse;
        successResponse.Data.Should().BeEquivalentTo(enrollments);
        successResponse.Message.Should().Be("Enrollments retrieved successfully");
    }
    [Fact]
    public async Task CreateEnrollmentAsync_ValidData_OkResult()
    {
        // Arrange
        var createEnrollmentDto = new CreateEnrollmentDto
        {
            StudentId = 1,
            CourseId = "C001"
        };
        var enrollmentDto = new EnrollmentDto
        {
            EnrollmentId = 1,
            StudentId = 1,
            CourseId = "C001",
            CourseName = "Course ",
            StudentName = "John Doe"
        };
        var serviceResult = new ServiceResult<EnrollmentDto>(enrollmentDto, "Enrollment created successfully");
        _mockEnrollmentService.Setup(s => s.EnrollStudentInCourseAsync(createEnrollmentDto)).ReturnsAsync(serviceResult);

        // Act
        var result = await _enrollmentController.CreateEnrollmentAsync(createEnrollmentDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeOfType<SuccessResponse>();
        var successResponse = okResult.Value as SuccessResponse;
        successResponse.Data.Should().BeEquivalentTo(enrollmentDto);
        successResponse.Message.Should().Be("Enrollment created successfully");
    }

    [Fact]
    public async Task CreateEnrollmentAsync_InvalidData_BadRequest()
    {
        // Arrange
        var invalidEnrollmentDto = new CreateEnrollmentDto();
        var serviceResult = new ServiceResult<EnrollmentDto>(ResultType.BadRequest, "Invalid enrollment data");
        _mockEnrollmentService.Setup(s => s.EnrollStudentInCourseAsync(invalidEnrollmentDto)).ReturnsAsync(serviceResult);

        // Act
        var result = await _enrollmentController.CreateEnrollmentAsync(invalidEnrollmentDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Value.Should().BeOfType<ErrorResponse>();
        var errorResponse = badRequestResult.Value as ErrorResponse;
        errorResponse.Message.Should().Be("Invalid enrollment data");
    }
    [Fact]
    public async Task CreateEnrollmentAsync_ConnectionError_InternalServerError()
    {
        // Arrange
        var serviceResult = new ServiceResult<EnrollmentDto>(ResultType.InternalServerError, "Some error");
        _mockEnrollmentService.Setup(s => s.EnrollStudentInCourseAsync(It.IsAny<CreateEnrollmentDto>())).ReturnsAsync(serviceResult);

        // Act
        var result = await _enrollmentController.CreateEnrollmentAsync(new CreateEnrollmentDto());

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult.StatusCode.Should().Be(500);
        objectResult.Value.Should().BeOfType<ErrorResponse>();
        var errorResponse = objectResult.Value as ErrorResponse;
        errorResponse.Message.Should().Be("Some error");
    }

    [Fact]
    public async Task DeleteEnrollmentAsync_ValidId_OkResult()
    {
        // Arrange
        int enrollmentId = 1;
        var serviceResult = new ServiceResult<bool>(true, "Enrollment deleted successfully");
        _mockEnrollmentService.Setup(s => s.DeleteEnrollmentAsync(enrollmentId)).ReturnsAsync(serviceResult);
        // Act
        var result = await _enrollmentController.DeleteEnrollmentAsync(enrollmentId);
        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeOfType<SuccessResponse>();
        var successResponse = okResult.Value as SuccessResponse;
        successResponse.Message.Should().Be("Enrollment deleted successfully");
    }
    [Fact]
    public async Task DeleteEnrollmentAsync_InvalidId_NotFound()
    {
        // Arrange
        int invalidId = 999;
        var serviceResult = new ServiceResult<bool>(ResultType.NotFound, "Enrollment not found");
        _mockEnrollmentService.Setup(s => s.DeleteEnrollmentAsync(invalidId)).ReturnsAsync(serviceResult);

        // Act
        var result = await _enrollmentController.DeleteEnrollmentAsync(invalidId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Value.Should().BeOfType<ErrorResponse>();
        var errorResponse = notFoundResult.Value as ErrorResponse;
        errorResponse.Message.Should().Be("Enrollment not found");
    }
}

internal class HttpRequestBase
{
}