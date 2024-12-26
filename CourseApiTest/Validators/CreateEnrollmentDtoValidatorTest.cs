using System;
using FluentAssertions;
using FluentValidation.TestHelper;
using CourseApi.DTOs;
using CourseApi.Validators;
namespace CourseApiTest.Validators;

public class CreateEnrollmentDtoApiValidatorTest
{
    public readonly CreateEnrollmentDtoValidator _validator;
    public CreateEnrollmentDtoApiValidatorTest()
    {
        _validator = new CreateEnrollmentDtoValidator();
    }

    [Fact]
    public void CreateCourseDtoValidator_ValidInput_PassValidation()
    {
        // Arrange
        var createEnrollmentDto = new CreateEnrollmentDto
        {
            CourseId = "CO001",
            StudentId = 1,
        };
        // Act
        var result = _validator.TestValidate(createEnrollmentDto);
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    [Fact]
    public void CreateEnrollmentDtoValidator_EmptyFields_FailValidation()
    {
        // Arrange
        var createEnrollmentDto = new CreateEnrollmentDto();
        // Act
        var result = _validator.TestValidate(createEnrollmentDto);
        // Assert
        result.ShouldHaveValidationErrorFor(e => e.CourseId).WithErrorMessage("Course ID is required");
        result.ShouldHaveValidationErrorFor(e => e.StudentId).WithErrorMessage("StudentId is required");
    }

    [Theory]
    [InlineData("C O")]
    [InlineData("MMO$")]

    public void CreateEnrollmentDtoValidator_InvalidCourseId_FailValidation(string courseId)
    {
        // Arrange
        var createEnrollmentDto = new CreateEnrollmentDto
        {
            CourseId = courseId,
            StudentId = 1,
        };
        // Act
        var result = _validator.TestValidate(createEnrollmentDto);
        // Assert
        result.ShouldHaveValidationErrorFor(e => e.CourseId);
    }


}

