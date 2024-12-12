using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentApi.Controllers;
using StudentApi.DTOs;
using StudentApi.Helpers;
using StudentApi.Services;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace StudentApiTest.Controllers;
public class StudentControllerTests
{
    private readonly Mock<IStudentService> _mockStudentService;
    private readonly StudentController _studentController;

    public StudentControllerTests()
    {
        _mockStudentService = new Mock<IStudentService>();
        _studentController = new StudentController(_mockStudentService.Object);
    }

    [Fact]
    public async Task GetAllStudentsAsync_ServiceResultTypeSuccess_OkResult()
    {
        // Arrange
        var query = new StudentQuery();
        var listStudents = new List<StudentDto> {
            new StudentDto {
                 StudentId = 1,
                 FullName = "John Doe" ,
                 Address = "123 Main St",
                 Email = "john.doe@example.com",
                 DateOfBirth = new DateOnly(2003, 2, 2),
                 PhoneNumber = "5551234567",
                 Grade = 2},
            new StudentDto {
                StudentId = 2,
                FullName = "Jane Smith",
                Address = "456 Elm St",
                Email = "jane.smith@example.com",
                DateOfBirth = new DateOnly(2004, 5, 9),
                PhoneNumber = "1234567890",
                Grade = 1}
        };
        var serviceResult = new ServiceResult(listStudents, "Get list students successfully");
        _mockStudentService.Setup(s => s.GetStudentsAsync(query)).ReturnsAsync(serviceResult);

        // Act
        var result = await _studentController.GetAllStudentsAsync(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        serviceResult.Data.Should().BeEquivalentTo(listStudents);
        _mockStudentService.Verify(s => s.GetStudentsAsync(query), Times.Once);
    }

    [Fact]
    public async Task GGetAllStudentsAsync_ServiceResultTypeNotFound_NotFoundResult()
    {
        var query = new StudentQuery();
        var serviceResult = new ServiceResult(ResultType.NotFound, "Students not found");
        _mockStudentService.Setup(s => s.GetStudentsAsync(query)).ReturnsAsync(serviceResult);

        // Act
        var result = await _studentController.GetAllStudentsAsync(query);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockStudentService.Verify(s => s.GetStudentsAsync(query), Times.Once);
    }
    [Fact]
    public async Task GetStudentByIdAsync_ServiceResultTypeSuccess_OkResult()
    {
        // Arrange
        int studentId = 1;
        var student = new StudentDto
        {
            StudentId = 1,
            FullName = "John Doe",
            Address = "123 Main St",
            Email = "john.doe@example.com",
            DateOfBirth = new DateOnly(2003, 2, 2),
            PhoneNumber = "5551234567",
            Grade = 2
        };
        var serviceResult = new ServiceResult(student, "Get student by id successfully");
        _mockStudentService.Setup(s => s.GetStudentByIdAsync(studentId)).ReturnsAsync(serviceResult);

        // Act
        var result = await _studentController.GetStudentByIdAsync(studentId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetStudentByIdAsync_ServiceResultTypeNotFound_NotFoundResult()
    {
        var query = new List<StudentDto>();
        var serviceResult = new ServiceResult(ResultType.NotFound, "Students not found");

        _mockStudentService.Setup(s => s.GetStudentByIdAsync(It.IsAny<int>())).ReturnsAsync(serviceResult);

        // Act
        var result = await _studentController.GetStudentByIdAsync(1);

        result.Should().NotBeNull();
        result.Should().BeOfType<NotFoundObjectResult>();
    }


    [Fact]
    public async Task GetStudentsByIdsAsync_ServiceResultTypeSuccess_OkResult()
    {
        // Arrange
        var ids = new List<int> { 1, 2 };
        var listStudents = new List<StudentDto> {
            new StudentDto {
                 StudentId = 1,
                 FullName = "John Doe" ,
                 Address = "123 Main St",
                 Email = "john.doe@example.com",
                 DateOfBirth = new DateOnly(2003, 2, 2),
                 PhoneNumber = "5551234567",
                 Grade = 2},
            new StudentDto {
                StudentId = 2,
                FullName = "Jane Smith",
                Address = "456 Elm St",
                Email = "jane.smith@example.com",
                DateOfBirth = new DateOnly(2004, 5, 9),
                PhoneNumber = "1234567890",
                Grade = 1}
        };

        var serviceResult = new ServiceResult(listStudents, "Get list students by ids successfully");
        _mockStudentService.Setup(s => s.GetStudentsByIdsAsync(ids)).ReturnsAsync(serviceResult);

        // Act
        var result = await _studentController.GetStudentsByIdsAsync(ids);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        List<StudentDto> resultStudents = (result as OkObjectResult).Value as List<StudentDto>;

    }

    [Fact]
    public async Task AddStudentAsync_CreatedResult()
    {
        // Arrange
        var newStudent = new CreateStudentDto()
        {
            FullName = "John Doe Updated",
            Address = "123 Main St Updated",
            Email = "john.doe@example.com",
            DateOfBirth = new DateOnly(2003, 2, 2),
            PhoneNumber = "5551234567"
        };
        var serviceResult = new ServiceResult(newStudent, "Create student successfully");

        _mockStudentService.Setup(s => s.CreateStudentAsync(newStudent)).ReturnsAsync(serviceResult);

        // Act
        var result = await _studentController.AddStudentAsync(newStudent);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        (result as OkObjectResult).Value.Should().BeOfType<SuccessResponse>();
        _mockStudentService.Verify(s => s.CreateStudentAsync(newStudent), Times.Once);
    }

    [Fact]
    public async Task UpdateStudentAsync_ServiceResultTypeSuccess_OkResult()
    {
        // Arrange
        int studentId = 1;
        var updateStudent = new UpdateStudentDto()
        {
            StudentId = 1,
            FullName = "John Doe Updated",
            Address = "123 Main St Updated",
            Email = "john.doe@example.com",
            DateOfBirth = new DateOnly(2003, 2, 2),
            PhoneNumber = "5551234567"
        };
        var serviceResult = new ServiceResult(updateStudent, "Update student successfully");
        _mockStudentService.Setup(s => s.UpdateStudentAsync(studentId, updateStudent)).ReturnsAsync(serviceResult);

        // Act
        var result = await _studentController.UpdateStudentAsync(studentId, updateStudent);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockStudentService.Verify(s => s.UpdateStudentAsync(studentId, updateStudent), Times.Once);
    }

    [Fact]
    public async Task DeleteStudentAsync_ServiceResultTypeSuccess_OkResult()
    {
        // Arrange
        var studentId = 1;
        var serviceResult = new ServiceResult(true, "Delete student successfully");
        _mockStudentService.Setup(s => s.DeleteStudentAsync(studentId)).ReturnsAsync(serviceResult);

        var result = await _studentController.DeleteStudentAsync(studentId);

        result.Should().BeOfType<OkObjectResult>();
        _mockStudentService.Verify(s => s.DeleteStudentAsync(studentId), Times.Once);

    }
}
